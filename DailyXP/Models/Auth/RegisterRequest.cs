namespace DailyXP.Web.Models.Auth;

public class RegisterRequest
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string? DisplayName { get; set; }
}