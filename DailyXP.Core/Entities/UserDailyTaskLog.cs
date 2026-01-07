using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyXP.Core.Entities;

public class UserDailyTaskLog
{
    public long Id { get; set; }
    public string UserId { get; set; } = default!;
    public int TaskDefinitionId { get; set; }
    public DateOnly Date { get; set; }
    public int EarnedXp { get; set; }
    public int? Value { get; set; }

    public TaskCheckinSource Source { get; set; } = TaskCheckinSource.Manual;
    public DateTime ClientRecordedAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsSuspicious { get; set; }
}
