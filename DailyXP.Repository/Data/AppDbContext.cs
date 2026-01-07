using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DailyXP.Repository.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DailyXP.Core.Entities; 


namespace DailyXP.Repository.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }
    public DbSet<TaskDefinition> TaskDefinitions => Set<TaskDefinition>();
    public DbSet<UserDailyTaskLog> UserDailyTaskLogs => Set<UserDailyTaskLog>();


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // İleride config sınıflarını buradan topluca uygularız:
        // builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}