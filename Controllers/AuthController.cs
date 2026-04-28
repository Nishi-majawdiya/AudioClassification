using Microsoft.AspNetCore.Mvc;
using AudioClassification.Data;
using AudioClassification.Models;
using AudioClassification.Services;
using BCrypt.Net;

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
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (_context.Users.Any(u => u.Username == request.Username))
                return BadRequest("Username already exists");

            if (_context.Users.Any(u => u.Email == request.Email))
                return BadRequest("Email already exists");

            // 🔐 HASH PASSWORD
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                Password = hashedPassword,

                CreatedBy = request.Username,
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok("User registered successfully");
        }

        // 🔹 Login
        [HttpPost("login")]
        public IActionResult Login(LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = _context.Users.FirstOrDefault(u =>
                u.Username == request.Username && !u.IsDeleted);

            if (user == null)
                return Unauthorized("Invalid username or password");

            // 🔐 VERIFY HASH
            bool isValid = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);

            if (!isValid)
                return Unauthorized("Invalid username or password");

            var token = _jwt.GenerateToken(user.Username);

            return Ok(new { token });
        }
    }
}