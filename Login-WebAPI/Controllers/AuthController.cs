using Login_WebAPI.Data;
using Login_WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;

namespace Login_WebAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwtService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(ApplicationDbContext context, JwtService jwtService, ILogger<AuthController> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _logger = logger;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto model)
        {
            _logger.LogInformation("Attempting login for user: {Email}", model.Email);

            try
            {
                var user = _context.Users.SingleOrDefault(u => u.Email == model.Email);

                if (user == null)
                {
                    _logger.LogWarning("Invalid credentials: User not found for email {Email}.", model.Email);
                    return Unauthorized(new { message = "Invalid credentials" });
                }

                // Validate the password
                if (!BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Invalid credentials: Incorrect password for email {Email}.", model.Email);
                    return Unauthorized(new { message = "Invalid credentials" });
                }

                // Generate JWT token and refresh token for the user
                var token = _jwtService.GenerateToken(user);
                var refreshToken = _jwtService.GenerateRefreshToken();

                // Save refresh token in the database (optional, for additional security)
                user.RefreshToken = refreshToken;
                _context.SaveChanges();

                _logger.LogInformation("Login successful for user: {Email}", model.Email);

                return Ok(new { Token = token, RefreshToken = refreshToken, Role = user.Role });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while logging in for user: {Email}.", model.Email);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("refresh-token")]
        public IActionResult RefreshToken([FromBody] TokenRequest model)
        {
            var refreshToken = model.RefreshToken;

            try
            {
                // Validate the refresh token and generate a new access token
                var user = _context.Users.SingleOrDefault(u => u.RefreshToken == refreshToken);
                if (user == null)
                {
                    _logger.LogWarning("Invalid refresh token for user.");
                    return Unauthorized(new { message = "Invalid refresh token" });
                }

                // Generate new access token and refresh token
                var newAccessToken = _jwtService.GenerateToken(user);
                var newRefreshToken = _jwtService.GenerateRefreshToken();

                // Update refresh token in the database
                user.RefreshToken = newRefreshToken;
                _context.SaveChanges();

                _logger.LogInformation("New access token generated for user: {Email}", user.Email);

                return Ok(new { Token = newAccessToken, RefreshToken = newRefreshToken });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while refreshing token for user.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
