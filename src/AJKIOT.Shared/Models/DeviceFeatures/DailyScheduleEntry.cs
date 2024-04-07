namespace AJKIOT.Shared.Models.DeviceFeatures
{
    public class DailyScheduleEntry
    {
        public int Id { get; set; }
        public int DeviceId { get; set; }
        public int DayNumber { get; set; }
        public int EntryNumber { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}
