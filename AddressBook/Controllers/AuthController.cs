using System.Security.Cryptography;
using AddressBook.HelperService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModelLayer.DTO;
using RepositoryLayer.Entity;

namespace AddressBook.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly TokenService tokenService;
        private readonly EmailService emailService;
        public AuthController(ApplicationDbContext dbContext,TokenService tokenService, EmailService emailService)
        {
            this.dbContext = dbContext;
            this.tokenService = tokenService;
            this.emailService = emailService;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO user)
        {
            var newUser = new User
            {
                Username = user.Username,
                Password = user.Password,
                Email = user.Email,
                Role = "User" // use Admin to create a admin 
            };

            dbContext.Users.Add(newUser);
            await dbContext.SaveChangesAsync();


            await emailService.SendEmailAsync(user.Email, "Registeration verification","Welcome to our application" );


            return Ok(new { message = "User created", newUser });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {
            if (login == null)
            {
                return BadRequest("Invalid request");
            }

            var user = await dbContext.Users
                .FirstOrDefaultAsync(x => x.Email == login.Email && x.Password == login.Password);

            if (user == null)
            {
                return Unauthorized("User does not exist or incorrect credentials");
            }

            var token = tokenService.GenerateToken(user);

            return Ok(new
            {
                success = true,
                message = "Login successful",
                Token=token
            });
        }
    }
}
