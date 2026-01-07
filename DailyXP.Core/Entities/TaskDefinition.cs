using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyXP.Core.Entities;

public enum TaskType
{
    Checklist = 1,
    StepCount = 2,
    Manual = 3
}

public class TaskDefinition
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string? Description { get; set; }

    public TaskType Type { get; set; }
    public int BaseXp { get; set; }
    public bool IsActive { get; set; } = true;
    public int? TargetValue { get; set; } // StepCount için hedef adım (örn 8000)
}



