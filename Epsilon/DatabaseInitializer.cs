using System.Runtime.InteropServices.JavaScript;
using Epsilon.Data;
using Epsilon.Models;
using Microsoft.EntityFrameworkCore;

namespace Epsilon
{
    public class DatabaseInitializer
    {
        public static void Seed(WebApplication app)
        {
            using (var serviceScope = app.Services.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<EpsilonDbContext>();

                if (context != null)
                {
                    if (!context.Users.Any())
                        InitUsers(context);
                    
                    if (!context.Messages.Any())
                        InitMessages(context);
                }
            }
            
        }

        public static void Migrate(WebApplication app)
        {
            // Migrate db
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<EpsilonDbContext>();
                db.Database.Migrate();
            }
        }
        
        private static byte[] GenerateRandomBytes(int length)
        {
            byte[] randomBytes = new byte[length];
            Random random = new Random();

            random.NextBytes(randomBytes);

            return randomBytes;
        }
        
        private static void InitUsers(EpsilonDbContext context)
        {
            var usersToAdd = new List<User>
            {
                new User { Id = GenerateRandomBytes(32), PublicKey = GenerateRandomBytes(256), Username = "user_1"},
                new User { Id = GenerateRandomBytes(32), PublicKey = GenerateRandomBytes(256), Username = "user_2"},
                new User { Id = GenerateRandomBytes(32), PublicKey = GenerateRandomBytes(256), Username = "user_3"},
                new User { Id = GenerateRandomBytes(32), PublicKey = GenerateRandomBytes(256), Username = "user_4"},
            };
            foreach (var user in usersToAdd)
                context.Users.Add(user);

            context.SaveChanges();
        }
        
        private static void InitMessages(EpsilonDbContext context)
        {
            var messagesToAdd = new List<Message>
            {
                new Message { EncryptedMessage  = GenerateRandomBytes(256), SenderId = GenerateRandomBytes(32), RecipientId = GenerateRandomBytes(32), CreatedAt = new DateTime() },
                new Message { EncryptedMessage  = GenerateRandomBytes(256), SenderId = GenerateRandomBytes(32), RecipientId = GenerateRandomBytes(32), CreatedAt = new DateTime() },
                new Message { EncryptedMessage  = GenerateRandomBytes(256), SenderId = GenerateRandomBytes(32), RecipientId = GenerateRandomBytes(32), CreatedAt = new DateTime() },
                new Message { EncryptedMessage  = GenerateRandomBytes(256), SenderId = GenerateRandomBytes(32), RecipientId = GenerateRandomBytes(32), CreatedAt = new DateTime() }
            };
            
            foreach (var message in messagesToAdd)
                context.Messages.Add(message);

            context.SaveChanges();
        }
    }
}