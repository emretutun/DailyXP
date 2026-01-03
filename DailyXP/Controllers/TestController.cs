using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DailyXP.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [Authorize]
    [HttpGet("secure")]
    public IActionResult Secure()
    {
        var userId = User.FindFirstValue("uid");
        var email = User.FindFirstValue(ClaimTypes.Email);

        return Ok(new
        {
            message = "JWT works",
            userId,
            email
        });
    }
}
