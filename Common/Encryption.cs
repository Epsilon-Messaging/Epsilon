using System.Text;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Security;

namespace Common;

public static class Encryption
{
    public static (string publicKey, string privateKey) GenerateKeys(string password, string identity)
    {
        var kpg = new RsaKeyPairGenerator();
        kpg.Init(new KeyGenerationParameters(new SecureRandom(), 2048));
        var keyPair = kpg.GenerateKeyPair();

        var pgpKeyPair = new PgpKeyPair(PublicKeyAlgorithmTag.RsaGeneral, keyPair, DateTime.UtcNow);

        var keyRingGen = new PgpKeyRingGenerator(
            PgpSignature.DefaultCertification,
            pgpKeyPair,
            identity,
            SymmetricKeyAlgorithmTag.Cast5,
            password.ToCharArray(),
            true,
            null,
            null,
            new SecureRandom()
        );

        string publicKey;
        using (var memoryStream = new MemoryStream())
        {
            using (var armoredStream = new ArmoredOutputStream(memoryStream))
            {
                keyRingGen.GeneratePublicKeyRing().Encode(armoredStream);
            }

            publicKey = Convert.ToBase64String(memoryStream.ToArray());
        }

        string privateKey;
        using (var memoryStream = new MemoryStream())
        {
            using (var armoredStream = new ArmoredOutputStream(memoryStream))
            {
                keyRingGen.GenerateSecretKeyRing().Encode(armoredStream);
            }

            privateKey = Convert.ToBase64String(memoryStream.ToArray());
        }

        return (publicKey.Trim(), privateKey.Trim());
    }

    public static string EncryptMessage(string message, PgpPrivateKey signingKey, PgpPublicKey publicKey)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message);
        var outputBytes = new MemoryStream();

        var encryptedDataGenerator = new PgpEncryptedDataGenerator(
            SymmetricKeyAlgorithmTag.Cast5,
            true,
            new SecureRandom()
        );
        encryptedDataGenerator.AddMethod(publicKey);

        using (var encryptedOut = encryptedDataGenerator.Open(outputBytes, new byte[1 << 16]))
        {
            var compressedDataGenerator = new PgpCompressedDataGenerator(CompressionAlgorithmTag.Zip);
            using var compressedOut = compressedDataGenerator.Open(encryptedOut);
            var signatureGenerator = new PgpSignatureGenerator(
                publicKey.Algorithm,
                HashAlgorithmTag.Sha1
            );
            signatureGenerator.InitSign(PgpSignature.BinaryDocument, signingKey);
            var onePassSignature = signatureGenerator.GenerateOnePassVersion(false);
            onePassSignature.Encode(compressedOut);
            using var literalOut = new PgpLiteralDataGenerator().Open(
                compressedOut,
                PgpLiteralData.Binary,
                "message",
                messageBytes.Length,
                DateTime.UtcNow
            );

            literalOut.Write(messageBytes, 0, messageBytes.Length);
            signatureGenerator.Update(messageBytes);
            var signature = signatureGenerator.Generate();
            signature.Encode(compressedOut);
        }

        return Convert.ToBase64String(outputBytes.ToArray());
    }

    public static string DecryptMessage(string encryptedMessage, PgpPrivateKey privateKey, PgpPublicKey verificationKey)
    {
        var encryptedBytes = Convert.FromBase64String(encryptedMessage);
        using var inputStream = new MemoryStream(encryptedBytes);
        using var decoderStream = PgpUtilities.GetDecoderStream(inputStream);
        var pgpObjectFactory = new PgpObjectFactory(decoderStream);

        PgpEncryptedDataList? encryptedDataList = null;
        PgpObject pgpObject;
        while ((pgpObject = pgpObjectFactory.NextPgpObject()) != null)
        {
            if (pgpObject is PgpEncryptedDataList list)
            {
                encryptedDataList = list;
                break;
            }
        }

        if (encryptedDataList == null)
            throw new ArgumentException("Invalid PGP encrypted message");

        PgpPublicKeyEncryptedData? encryptedData = null;

        foreach (PgpPublicKeyEncryptedData pked in encryptedDataList.GetEncryptedDataObjects())
        {
            if (pked.KeyId == privateKey.KeyId)
            {
                encryptedData = pked;
                break;
            }
        }

        if (encryptedData == null)
            throw new ArgumentException("Matching private key not found for decryption");

        using var decryptedStream = encryptedData.GetDataStream(privateKey);
        var decryptedFactory = new PgpObjectFactory(decryptedStream);

        var compressedData = (PgpCompressedData)decryptedFactory.NextPgpObject();
        var compressedStream = compressedData.GetDataStream();
        var compressedFactory = new PgpObjectFactory(compressedStream);

        PgpOnePassSignature? onePassSignature = null;
        PgpLiteralData? literalData = null;
        PgpSignature? signature = null;

        while ((pgpObject = compressedFactory.NextPgpObject()) != null)
        {
            if (pgpObject is PgpOnePassSignatureList onePassSignatureList)
            {
                onePassSignature = onePassSignatureList[0];
            }
            else if (pgpObject is PgpLiteralData ld)
            {
                literalData = ld;
            }
            else if (pgpObject is PgpSignatureList signatureList)
            {
                signature = signatureList[0];
            }
        }

        if (literalData == null || onePassSignature == null || signature == null)
            throw new ArgumentException("Invalid PGP message structure");

        using var outputStream = new MemoryStream();
        using var literalDataStream = literalData.GetInputStream();
        var buffer = new byte[1024];
        int length;

        onePassSignature.InitVerify(verificationKey);
        while ((length = literalDataStream.Read(buffer, 0, buffer.Length)) > 0)
        {
            onePassSignature.Update(buffer, 0, length);
            outputStream.Write(buffer, 0, length);
        }

        if (!onePassSignature.Verify(signature))
            throw new ArgumentException("Signature verification failed");

        return Encoding.UTF8.GetString(outputStream.ToArray());
    }

    public static PgpPublicKey ReadPublicKey(string publicKeyContent)
    {
        var keyBytes = Convert.FromBase64String(publicKeyContent);
        using var memoryStream = new MemoryStream(keyBytes);
        var input = PgpUtilities.GetDecoderStream(memoryStream);
        var pgpPub = new PgpPublicKeyRingBundle(input);

        foreach (PgpPublicKeyRing keyRing in pgpPub.GetKeyRings())
        {
            foreach (PgpPublicKey key in keyRing.GetPublicKeys())
            {
                if (key.IsEncryptionKey)
                    return key;
            }
        }

        throw new ArgumentException("Can't find encryption key in key ring.");
    }

    public static PgpPrivateKey ReadPrivateKey(string privateKeyContent, string password)
    {
        return ReadSecretKey(privateKeyContent).ExtractPrivateKey(password.ToCharArray());
    }

    public static PgpSecretKey ReadSecretKey(string privateKeyContent)
    {
        var keyBytes = Convert.FromBase64String(privateKeyContent);
        using var memoryStream = new MemoryStream(keyBytes);
        var input = PgpUtilities.GetDecoderStream(memoryStream);
        var pgpSec = new PgpSecretKeyRingBundle(input);
        foreach (PgpSecretKeyRing keyRing in pgpSec.GetKeyRings())
        {
            foreach (PgpSecretKey key in keyRing.GetSecretKeys())
            {
                if (key.IsSigningKey)
                    return key;
            }
        }

        throw new ArgumentException("Can't find signing key in key ring.");
    }
}