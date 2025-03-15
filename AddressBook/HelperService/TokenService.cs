using Microsoft.IdentityModel.Tokens;
using RepositoryLayer.Entity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AddressBook.HelperService
{
    public class TokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration configuration)
        {
            _config = configuration;
        }

        public string GenerateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Issuer"], // 🔹 Ensure audience is provided
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public string GenerateResetToken(User user)
        {
            string resetKey = _config["ResetToken:Key"];
            if (string.IsNullOrEmpty(resetKey))
            {
                throw new Exception("Reset token secret key is missing in appsettings.json");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(resetKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(JwtRegisteredClaimNames.Exp, DateTime.UtcNow.AddMinutes(60).ToString())
    };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        public ClaimsPrincipal ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            string secretKey = _config["ResetToken:Key"];
            if (string.IsNullOrEmpty(secretKey))
            {
                Console.WriteLine(" ResetToken:Key is NULL or EMPTY in configuration!");
                return null;
            }

            var key = Encoding.UTF8.GetBytes(secretKey);
            //Console.WriteLine($" Using ResetToken Key: {Convert.ToBase64String(key)}"); // Debug key properly

            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,  // Ensure issuer is validated
                    ValidateAudience = false, // No audience validation for reset token
                    ValidateLifetime = true,  // Token expiration check
                    ValidateIssuerSigningKey = true,  // Ensures the token signature is correct
                    ValidIssuer = _config["Jwt:Issuer"], // Fetching issuer from appsettings.json
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero  // Disable default 5-minute tolerance
                };

                return tokenHandler.ValidateToken(token, validationParameters, out _);
            }
            catch (SecurityTokenExpiredException)
            {
                Console.WriteLine(" Token expired.");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Token validation error: {ex.Message}");
                return null;
            }
        }




    }

}
