using Epsilon.Data;
using Epsilon.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Epsilon.Controllers;

[ApiController]
[Route("api")]
public class TestController
{

    private readonly EpsilonDbContext _epsilonDbContext;

    public TestController(EpsilonDbContext epsilonDbContext)
    {
        _epsilonDbContext = epsilonDbContext;
    }

    [HttpGet("users")]
    public async Task<ActionResult<IList<User>>> GetUsers()
    {
        return await _epsilonDbContext.Users.ToListAsync();
    }
}