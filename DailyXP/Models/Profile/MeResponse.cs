namespace DailyXP.Web.Models.Profile;

public class MeResponse
{
    public string Id { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
}
