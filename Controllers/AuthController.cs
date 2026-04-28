using Microsoft.AspNetCore.Mvc;
using AudioClassification.Data;
using AudioClassification.Models;
using AudioClassification.Services;

namespace AudioClassification.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwt;

        public AuthController(AppDbContext context, JwtService jwt)
        {
            _context = context;
            _jwt = jwt;
        }

        // 🔹 Register
        [HttpPost("register")]
        public IActionResult Register(RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Username and password are required.");

            var username = request.Username.Trim();

            var exists = _context.Users.Any(u => u.Username == username && !u.IsDeleted);
            if (exists)
                return Conflict("Username already exists.");

            var user = new User
            {
                Username = username,
                Password = request.Password,

                CreatedBy = username,
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok("User registered");
        }

        // 🔹 Login
        [HttpPost("login")]
        public IActionResult Login(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Username and password are required.");

            var username = request.Username.Trim();

            var existing = _context.Users
                .FirstOrDefault(u =>
                    u.Username == username &&
                    u.Password == request.Password &&
                    !u.IsDeleted);

            if (existing == null)
                return Unauthorized("Invalid credentials");

            var token = _jwt.GenerateToken(existing.Username);

            return Ok(new { token });
        }
    }
}
