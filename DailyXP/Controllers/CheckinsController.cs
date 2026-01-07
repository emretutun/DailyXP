using DailyXP.Core.Entities;
using DailyXP.Repository.Data;
using DailyXP.Repository.Identity;
using DailyXP.Web.Models.Checkins;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DailyXP.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CheckinsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _userManager;

    public CheckinsController(AppDbContext db, UserManager<AppUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCheckinRequest request)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var task = await _db.TaskDefinitions.FirstOrDefaultAsync(t => t.Id == request.TaskId && t.IsActive);
        if (task is null) return NotFound(new { message = "Task not found" });

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Aynı görev aynı gün 1 kez
        var exists = await _db.UserDailyTaskLogs.AnyAsync(x =>
            x.UserId == user.Id &&
            x.TaskDefinitionId == task.Id &&
            x.Date == today);

        if (exists)
            return Conflict(new { message = "Already checked in today for this task" });

        // Basit XP: BaseXp (stepcount için Value ile sonra geliştireceğiz)
        var earnedXp = task.BaseXp;

        if (task.Type == TaskType.StepCount)
        {
            if (request.Value is null || request.Value <= 0)
                return BadRequest(new { message = "Value (steps) is required for StepCount tasks" });

            if (task.TargetValue is null || task.TargetValue <= 0)
                return BadRequest(new { message = "Task target is not configured" });

            var ratio = (double)request.Value.Value / task.TargetValue.Value;

            if (ratio >= 1.0) earnedXp = task.BaseXp;
            else if (ratio >= 0.5) earnedXp = (int)Math.Round(task.BaseXp * 0.5);
            else earnedXp = 0;
        }

        var log = new UserDailyTaskLog
        {
            UserId = user.Id,
            TaskDefinitionId = task.Id,
            Date = today,
            EarnedXp = earnedXp,
            Value = request.Value
        };

        _db.UserDailyTaskLogs.Add(log);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Check-in created",
            earnedXp
        });
    }

    [Authorize]
    [HttpPut("steps/today")]
    public async Task<IActionResult> UpsertStepsToday([FromBody] UpdateStepsTodayRequest request)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        // Manual kabul etmiyoruz (hile azaltma)
        if (request.Source == TaskCheckinSource.Manual)
            return BadRequest(new { message = "Manual source is not allowed for step check-ins" });

        if (request.Steps <= 0)
            return BadRequest(new { message = "Steps must be > 0" });

        // Günlük max step (örnek: 30000)
        const int maxDailySteps = 30000;
        var clampedSteps = Math.Min(request.Steps, maxDailySteps);

        var task = await _db.TaskDefinitions.FirstOrDefaultAsync(t => t.Id == request.TaskId && t.IsActive);
        if (task is null) return NotFound(new { message = "Task not found" });

        if (task.Type != TaskType.StepCount)
            return BadRequest(new { message = "Task is not StepCount type" });

        if (task.TargetValue is null || task.TargetValue <= 0)
            return BadRequest(new { message = "Task target is not configured" });

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Bugünün kaydı var mı?
        var log = await _db.UserDailyTaskLogs.FirstOrDefaultAsync(x =>
            x.UserId == user.Id &&
            x.TaskDefinitionId == task.Id &&
            x.Date == today);

        // XP hesap fonksiyonu (kademeli)
        int CalculateXp(int steps)
        {
            var ratio = (double)steps / task.TargetValue!.Value;

            if (ratio >= 1.0) return task.BaseXp;
            if (ratio >= 0.5) return (int)Math.Round(task.BaseXp * 0.5);
            return 0;
        }

        // Anomali kontrol (step/dk) - sadece önceki kayıt varsa hesaplanır
        bool suspicious = false;

        if (log is null)
        {
            var earnedXp = CalculateXp(clampedSteps);

            log = new UserDailyTaskLog
            {
                UserId = user.Id,
                TaskDefinitionId = task.Id,
                Date = today,
                Value = clampedSteps,
                EarnedXp = earnedXp,
                Source = request.Source,
                ClientRecordedAtUtc = request.ClientRecordedAtUtc == default ? DateTime.UtcNow : request.ClientRecordedAtUtc,
                IsSuspicious = false
            };

            _db.UserDailyTaskLogs.Add(log);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Step check-in created",
                steps = log.Value,
                earnedXp = log.EarnedXp,
                suspicious = log.IsSuspicious
            });
        }
        else
        {
            // XP sadece artsın: adım da sadece artsın
            var previousSteps = log.Value ?? 0;
            var newSteps = Math.Max(previousSteps, clampedSteps);

            // step/dk anomali
            var prevTime = log.ClientRecordedAtUtc;
            var newTime = request.ClientRecordedAtUtc == default ? DateTime.UtcNow : request.ClientRecordedAtUtc;

            // zaman ters ise düzelt
            if (newTime < prevTime) newTime = DateTime.UtcNow;

            var deltaSteps = newSteps - previousSteps;
            var deltaMinutes = Math.Max((newTime - prevTime).TotalMinutes, 1);

            var stepsPerMinute = deltaSteps / deltaMinutes;

            // örnek limit: 400 step/dk üzeri şüpheli
            if (stepsPerMinute > 400)
                suspicious = true;

            var recalculatedXp = CalculateXp(newSteps);

            // XP sadece artsın
            var finalXp = Math.Max(log.EarnedXp, recalculatedXp);

            // Şüpheliyse XP vermeyi kapatmak istersen:
            // if (suspicious) finalXp = log.EarnedXp;  // (mevcut XP'yi koru)
            // veya:
            // if (suspicious) finalXp = 0;             // (sert)
            // Ben MVP için: XP'yi arttırma, mevcut kalsın.
            if (suspicious)
                finalXp = log.EarnedXp;

            log.Value = newSteps;
            log.EarnedXp = finalXp;
            log.Source = request.Source; // son kaynağı sakla
            log.ClientRecordedAtUtc = newTime;
            log.IsSuspicious = log.IsSuspicious || suspicious;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Step check-in updated",
                steps = log.Value,
                earnedXp = log.EarnedXp,
                suspicious = log.IsSuspicious
            });
        }
    }


    [Authorize]
    [HttpGet("today")]
    public async Task<IActionResult> Today()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var items = await _db.UserDailyTaskLogs
            .Where(x => x.UserId == user.Id && x.Date == today)
            .Join(_db.TaskDefinitions,
                log => log.TaskDefinitionId,
                task => task.Id,
                (log, task) => new
                {
                    taskId = task.Id,
                    taskTitle = task.Title,
                    taskType = task.Type,
                    baseXp = task.BaseXp,
                    targetValue = task.TargetValue,

                    earnedXp = log.EarnedXp,
                    value = log.Value,
                    source = log.Source,
                    suspicious = log.IsSuspicious,
                    clientRecordedAtUtc = log.ClientRecordedAtUtc
                })
            .OrderBy(x => x.taskId)
            .ToListAsync();

        var totalXpToday = items.Sum(x => x.earnedXp);

        return Ok(new
        {
            dateUtc = today.ToString(),
            totalXpToday,
            items
        });
    }

}
