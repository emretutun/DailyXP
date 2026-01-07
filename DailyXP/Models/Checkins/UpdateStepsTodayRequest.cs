using DailyXP.Core.Entities;

namespace DailyXP.Web.Models.Checkins;

public class UpdateStepsTodayRequest
{
    public int TaskId { get; set; }          // StepCount task id (örn 3)
    public int Steps { get; set; }           // günlük toplam adım
    public TaskCheckinSource Source { get; set; } // GoogleFit / HealthKit
    public DateTime ClientRecordedAtUtc { get; set; } // mobilde ölçüm zamanı (UTC gönder)
}
