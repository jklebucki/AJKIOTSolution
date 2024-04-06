using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AJKIOT.Shared.Models.DeviceFeatures
{
    public class DailyScheduleEntry
    {
        public int DayNumber { get; set; }
        public int EntryNumber { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}
