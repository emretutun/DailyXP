namespace DailyXP.Web.Models.Checkins;

public class CreateCheckinRequest
{
    public int TaskId { get; set; }
    public int? Value { get; set; } // step sayısı gibi
}
