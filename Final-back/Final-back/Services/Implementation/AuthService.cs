using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Final_back.Data;
using Final_back.Models;
using Microsoft.EntityFrameworkCore;
using Final_back.Services.Abstraction;

namespace Final_back.Services.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly DataContext _db;
        private readonly IConfiguration _cfg;
        public AuthService(DataContext db, IConfiguration cfg) { _db = db; _cfg = cfg; }

        public User Register(User req)
        {
            if (_db.Users.Any(u => u.Email == req.Email)) throw new InvalidOperationException("Email taken");

            req.Password = BCrypt.Net.BCrypt.HashPassword(req.Password);
            req.Role = "User";
            req.CreatedAt = DateTime.UtcNow;
            req.HasConfirmed = true;

            _db.Users.Add(req);
            _db.SaveChanges();
            return req;
        }

        public string? Login(string email, string password)
        {
            var user = _db.Users.SingleOrDefault(u => u.Email == email);
            if (user is null || !BCrypt.Net.BCrypt.Verify(password, user.Password)) return null;

            // build JWT (same code as before, just without async)
            var key = Encoding.ASCII.GetBytes(_cfg["Jwt:Key"]!);
            var creds = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _cfg["Jwt:Issuer"],
                audience: _cfg["Jwt:Audience"],
                claims: new[]
                {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email,          user.Email),
                new Claim(ClaimTypes.Role,           user.Role)
                },
                expires: DateTime.UtcNow.AddHours(12),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
