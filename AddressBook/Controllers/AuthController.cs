using System.Security.Claims;
using AddressBook.HelperService;
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
        private readonly RabbitMQService rabbitMQService;

        public AuthController(ApplicationDbContext dbContext, TokenService tokenService, EmailService emailService, RabbitMQService rabbitMQService)
        {
            this.dbContext = dbContext;
            this.tokenService = tokenService;
            this.emailService = emailService;
            this.rabbitMQService = rabbitMQService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Password) || string.IsNullOrWhiteSpace(user.Email))
            {
                return BadRequest(new { message = "Invalid registration details." });
            }

            // Trim inputs to prevent unnecessary spaces
            user.Email = user.Email.Trim().ToLower();
            user.Username = user.Username.Trim();

            // Check if email already exists
            var existingUser = await dbContext.Users.AnyAsync(x => x.Email == user.Email);
            if (existingUser)
            {
                return BadRequest(new { message = "Email is already registered." });
            }

            // Hash the password securely
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);

            var newUser = new User
            {
                Username = user.Username,
                Password = hashedPassword,
                Email = user.Email,
                Role = "Admin" // Use "Admin" to create an admin
            };

            dbContext.Users.Add(newUser);
            await dbContext.SaveChangesAsync();

            // Send a confirmation email
            await emailService.SendEmailAsync(user.Email, "Registration Verification", "Welcome to our application");

            return Ok(new { message = "User created successfully!", user = newUser });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {
            if (login == null || string.IsNullOrWhiteSpace(login.Email) || string.IsNullOrWhiteSpace(login.Password))
            {
                return BadRequest(new { message = "Invalid login details." });
            }

            // Trim input to avoid trailing spaces and ensure case-insensitive matching
            login.Email = login.Email.Trim().ToLower();

            var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == login.Email);

            if (user == null)
            {
                return Unauthorized(new { message = "User does not exist." });
            }

            // Verify the hashed password correctly
            if (!BCrypt.Net.BCrypt.Verify(login.Password, user.Password))
            {
                return Unauthorized(new { message = "Incorrect password." });
            }

            // Generate JWT token
            var token = tokenService.GenerateToken(user);

            return Ok(new
            {
                success = true,
                message = "Login successful",
                Token = token
            });
        }


        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
                return BadRequest(new { message = "Email not found." });

            var resetToken = tokenService.GenerateResetToken(user);

            //await emailService.SendEmailAsync(user.Email, "Password Reset Request", resetToken);
            var message = new
            {
                Email = user.Email,
                Subject = "Password Reset Request",
                Body = resetToken
            };

            await rabbitMQService.PublishMessageAsync(message);


            return Ok(new { message = "Reset password link has been sent to your email." });
        }




        [HttpPut("reset-password")]
        public async Task<IActionResult> ResetPassword([FromQuery] string token, [FromBody] ResetPasswordModel model)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { message = "Token is required." });
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                return BadRequest(new { message = "New password and confirm password do not match." });
            }

            var principal = tokenService.ValidateToken(token);
            if (principal == null)
            {
                return Unauthorized(new { message = "Invalid or expired token." });
            }

            var email = principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized(new { message = "Invalid token format." });
            }

            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return BadRequest(new { message = "Invalid user." });
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword); // Hash new password before saving
            await dbContext.SaveChangesAsync();

            return Ok(new { message = "Password reset successful." });
        }
    }
}
