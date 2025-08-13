using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TodoApi.Data;
using TodoApi.DTOs.Auth;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // ------------------- REGISTER -------------------
        [HttpPost("register")]
        public async Task<ActionResult<RegisterUserResponseDto>> Register(RegisterUserRequestDto request)
        {
            // Check if email is already registered
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest("Email is already taken.");

            // Hash password before saving
            var hashedPassword = HashPassword(request.Password);

            var user = new User
            {
                Email = request.Email,
                Password = hashedPassword
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            

            // Generate token for the new user
            var token = GenerateJwtToken(user);

            return Ok(new RegisterUserResponseDto
            {
                Token = token
            });
        }

        // ------------------- LOGIN -------------------
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !VerifyPassword(request.Password, user.Password))
                return Unauthorized("Invalid email or password.");


            var token = GenerateJwtToken(user);

            return Ok(new LoginResponseDto
            {
                Token = token
            });
        }

        // ------------------- PASSWORD SECURITY -------------------
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        private bool VerifyPassword(string enteredPassword, string storedHashedPassword)
        {
            var enteredHash = HashPassword(enteredPassword);
            return enteredHash == storedHashedPassword;
        }

        // ------------------- JWT GENERATION -------------------
        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, "User") // Replace with dynamic role if available
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
