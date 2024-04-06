namespace AJKIOT.Shared.Models.DeviceFeatures
{
    public class DailySchedule
    {
        public int DayNumber { get; set; }
        public List<DailyScheduleEntry> Entries { get; set; } = new List<DailyScheduleEntry>();
    }
}
