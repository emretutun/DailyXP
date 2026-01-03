using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DailyXP.Repository.Identity;

namespace DailyXP.Services.Auth;

public interface IJwtTokenService
{
    Task<string> CreateTokenAsync(AppUser user);
}