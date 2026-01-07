using DailyXP.Repository.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DailyXP.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeaderboardController : ControllerBase
{
    private readonly AppDbContext _db;

    public LeaderboardController(AppDbContext db)
    {
        _db = db;
    }

    // Son 7 gün (UTC) toplam XP
    [Authorize]
    [HttpGet("weekly")]
    public async Task<IActionResult> Weekly([FromQuery] int top = 20)
    {
        top = Math.Clamp(top, 1, 100);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var start = today.AddDays(-6); // bugün dahil 7 gün

        var rows = await _db.UserDailyTaskLogs
            .Where(x => x.Date >= start && x.Date <= today)
            .GroupBy(x => x.UserId)
            .Select(g => new
            {
                userId = g.Key,
                totalXp = g.Sum(x => x.EarnedXp)
            })
            .OrderByDescending(x => x.totalXp)
            .Take(top)
            .ToListAsync();

        // kullanıcı isimlerini çekelim
        var userIds = rows.Select(r => r.userId).ToList();

        var users = await _db.Users
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.DisplayName, u.UserName, u.AvatarUrl })
            .ToListAsync();

        var result = rows.Select((r, index) =>
        {
            var u = users.FirstOrDefault(x => x.Id == r.userId);
            return new
            {
                rank = index + 1,
                userId = r.userId,
                displayName = u?.DisplayName ?? u?.UserName ?? "User",
                avatarUrl = u?.AvatarUrl,
                totalXp = r.totalXp
            };
        });

        return Ok(result);
    }
}
