using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DailyXP.Core.Entities;

namespace DailyXP.Repository.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (!db.TaskDefinitions.Any())
        {
            db.TaskDefinitions.AddRange(
                new TaskDefinition
                {
                    Title = "10 dakika yürüyüş",
                    Description = "Gün içinde en az 10 dakika yürüyüş yap",
                    Type = TaskType.Manual,
                    BaseXp = 10,
                    IsActive = true
                },
                new TaskDefinition
                {
                    Title = "Su iç (2L)",
                    Description = "Günlük 2 litre su hedefi",
                    Type = TaskType.Checklist,
                    BaseXp = 8,
                    IsActive = true
                },
                new TaskDefinition
                {
                    Title = "Adım hedefi (8000)",
                    Description = "Günlük 8000 adım hedefi",
                    Type = TaskType.StepCount,
                    BaseXp = 20,
                    IsActive = true,
                    TargetValue = 8000
                },
                new TaskDefinition
                {
                    Title = "Kitap oku (15 dk)",
                    Description = "15 dakika kitap oku",
                    Type = TaskType.Manual,
                    BaseXp = 12,
                    IsActive = true
                }
            );

            db.SaveChanges();
        }
    }
}
