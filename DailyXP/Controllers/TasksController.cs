using DailyXP.Repository.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DailyXP.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _db;

    public TasksController(AppDbContext db)
    {
        _db = db;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetActive()
    {
        var tasks = await _db.TaskDefinitions
            .Where(t => t.IsActive)
            .OrderBy(t => t.Id)
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.Description,
                t.Type,
                t.BaseXp
            })
            .ToListAsync();

        return Ok(tasks);
    }
}
