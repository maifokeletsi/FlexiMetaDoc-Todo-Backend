using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TodoApi.Data;
using TodoApi.DTOs;
using TodoApi.DTOs.Auth;
using TodoApi.DTOs.Todo;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public UserController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // GET: api/user
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUsers()
        {
            var users = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Include(u => u.Todos)
                .ToListAsync();

            return Ok(users.Select(MapToUserResponseDto));
        }

        // GET: api/user/{email}
        [HttpGet("{email}")]
        public async Task<ActionResult<UserResponseDto>> GetUser(string email)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Include(u => u.Todos)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return NotFound();

            return Ok(MapToUserResponseDto(user));
        }

        // ------------------- CREATE USER -------------------
        [HttpPost]
        public async Task<ActionResult<RegisterUserResponseDto>> CreateUser(UserRequestDto request)
        {
            // 1. Check if email is already in use
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest("Email is already taken.");

            // 2. Hash password
            var hashedPassword = HashPassword(request.Password);

            // 3. Create new user entity
            var user = new User
            {
                Email = request.Email,
                Password = hashedPassword
            };

            // 4. Assign roles from DB if any were provided
            if (request.RoleIds != null && request.RoleIds.Any())
            {
                var rolesFromDb = await _context.Roles
                    .Where(r => request.RoleIds.Contains(r.Id))
                    .ToListAsync();

                user.UserRoles = rolesFromDb.Select(role => new UserRole
                {
                    UserEmail = user.Email,
                    RoleId = role.Id,
                    Role = role
                }).ToList();
            }

            // 5. Save user to database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // 6. Reload user with roles from DB to ensure we have Role.Name
            await _context.Entry(user)
                .Collection(u => u.UserRoles)
                .Query()
                .Include(ur => ur.Role)
                .LoadAsync();

            // 7. Generate JWT token with proper role
            var token = GenerateJwtToken(user);

            // 8. Return response DTO with token
            return Ok(new RegisterUserResponseDto
            {
                Token = token
            });
        }

        private UserResponseDto MapToUserResponseDto(User user)
        {
            return new UserResponseDto
            {
                Email = user.Email,
                Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
                Todos = user.Todos.Select(t => new TodoResponseDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Completed = t.Completed
                }).ToList()
            };
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
            var roleName = user.UserRoles.FirstOrDefault()?.Role?.Name ?? "User";

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, roleName)
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
