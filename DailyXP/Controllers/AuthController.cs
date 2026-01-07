using DailyXP.Repository.Identity;
using DailyXP.Services.Auth;
using DailyXP.Web.Models.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DailyXP.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthController(UserManager<AppUser> userManager, IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var user = new AppUser
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                errors = result.Errors.Select(e => new { e.Code, e.Description })
            });
        }

        return Ok(new { message = "User created" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Unauthorized(new { message = "Invalid credentials" });

        var ok = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!ok)
            return Unauthorized(new { message = "Invalid credentials" });

        // refresh token oluştur
        user.RefreshToken = GenerateRefreshToken();
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        var accessToken = await _jwtTokenService.CreateTokenAsync(user);

        return Ok(new
        {
            accessToken,
            refreshToken = user.RefreshToken
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var user = _userManager.Users
            .FirstOrDefault(u =>
                u.RefreshToken == request.RefreshToken &&
                u.RefreshTokenExpiresAt > DateTime.UtcNow);

        if (user is null)
            return Unauthorized(new { message = "Invalid refresh token" });

        // refresh token rotation (güvenlik)
        user.RefreshToken = GenerateRefreshToken();
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        var newAccessToken = await _jwtTokenService.CreateTokenAsync(user);

        return Ok(new
        {
            accessToken = newAccessToken,
            refreshToken = user.RefreshToken
        });
    }



    // Basit bir refresh token oluşturucu = Yardımcı Fonksiyon
    private static string GenerateRefreshToken()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }

}


    