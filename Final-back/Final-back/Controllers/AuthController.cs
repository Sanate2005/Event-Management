using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Final_back.Requests;              
using Final_back.Services.Abstraction;    
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Final_back.Models;
using Final_back.Data;
using Final_back.Mails;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Final_back.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    // ──────── injected services ────────
    private readonly DataContext _db;
    private readonly IUserService _userSvc;
    private readonly IJWTService _jwt;
    private readonly EmailSender _mailer;
    private readonly IConfiguration _cfg;

    public AuthController(
        DataContext db,
        IUserService userSvc,
        IJWTService jwt,
        EmailSender mailer,
        IConfiguration cfg)
    {
        _db = db;
        _userSvc = userSvc;
        _jwt = jwt;
        _mailer = mailer;
        _cfg = cfg;
    }

    // ─────────────────── REGISTER ───────────────────
    // POST /api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register(AddUser dto)
    {
        if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
            return BadRequest("User already exists");

        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = "User",
            HasConfirmed = false          // NOTE: your entity uses HasConfirmed
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var tokenObj = _jwt.GenerateToken(user);          // ← ① returns UserToken
        var verify = $"{_cfg["Frontend:BaseUrl"]}/verify-email?token=" +
                       Uri.EscapeDataString(tokenObj.Token);  // ← ② use .Token

        var body = $"""
        <h3>Welcome!</h3>
        <p>Your verification code is:</p>
        <h2 style="letter-spacing:1px;">{tokenObj.Token}</h2>   <!-- ③ show the code -->
        <p>Copy this code into the app or simply click the link below:</p>
        <p><a href="{verify}">Verify account</a></p>
        <p>This link and code expire in 12&nbsp;hours.</p>
    """;

        _mailer.sendMail(user.Email, "Confirm your account", body);
        return Ok("Check your inbox to confirm your e-mail.");
    }

    // ─────────────────── LOGIN ───────────────────────
    public record LoginBody([Required] string Email, [Required] string Password);

    // POST /api/auth/login
    [HttpPost("login")]
    public ActionResult<UserToken> LogIn([FromBody] LoginBody body)
    {
        var user = _userSvc.Login(body.Email, body.Password);
        if (user is null) return Unauthorized(new { Message = "Invalid credentials" });

        var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new(ClaimTypes.Email, user.Email),
        new(ClaimTypes.Role, user.Role) 
    };

        var tokenObj = _jwt.GenerateToken(user);  
        return Ok(new                              
        {
            token = tokenObj.Token,
            role = user.Role,
            userId = user.Id,
            fullName = user.FullName

        });
    }

    // ─────────────── VERIFY-EMAIL ───────────────
    // GET /api/auth/verify-email?token=...
    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var parameters = new TokenValidationParameters
            {
                ValidIssuer = JwtConstants.Issuer,
                ValidAudience = JwtConstants.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Convert.FromHexString(JwtConstants.Key)),  // ← no more null
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true
            };

            var principal = handler.ValidateToken(token, parameters, out _);
            var userId = int.Parse(
                principal.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var user = await _db.Users.FindAsync(userId);
            if (user is null) return BadRequest("User not found");

            user.HasConfirmed = true;
            await _db.SaveChangesAsync();

            return Ok("E-mail verified! You can now log in.");
        }
        catch (SecurityTokenException)
        {
            return BadRequest("Invalid or expired code");
        }
    }

    // ─────────────── ADMIN: LIST USERS ───────────────
    // GET /api/auth/users
    [HttpGet("users")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public ActionResult<IEnumerable<User>> GetAllUsers()
        => Ok(_userSvc.GetUsers());

    // ─────────────── PROFILE ───────────────
    // GET /api/auth/profile
    [HttpGet("profile")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public ActionResult<User> GetProfile()
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
            return Unauthorized();

        var profile = _userSvc.GetProfile(id);
        return profile is null ? NotFound() : Ok(profile);
    }
}
