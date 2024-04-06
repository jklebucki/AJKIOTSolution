using System;
using System.Collections.Generic;

namespace AJKIOT.Web.Translations
{
    public class Weekday
    {
        public int DayNumber { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;

        private static readonly Lazy<Dictionary<(int, string), Weekday>> DaysOfWeekByLanguage = new Lazy<Dictionary<(int, string), Weekday>>(() =>
            new Dictionary<(int, string), Weekday>
            {
                { (1, "EN"), new Weekday { DayNumber = 1, Name = "Monday", ShortName = "Mon" } },
                { (2, "EN"), new Weekday { DayNumber = 2, Name = "Tuesday", ShortName = "Tue" } },
                { (3, "EN"), new Weekday { DayNumber = 3, Name = "Wednesday", ShortName = "Wed" } },
                { (4, "EN"), new Weekday { DayNumber = 4, Name = "Thursday", ShortName = "Thu" } },
                { (5, "EN"), new Weekday { DayNumber = 5, Name = "Friday", ShortName = "Fri" } },
                { (6, "EN"), new Weekday { DayNumber = 6, Name = "Saturday", ShortName = "Sat" } },
                { (7, "EN"), new Weekday { DayNumber = 7, Name = "Sunday", ShortName = "Sun" } },
                { (1, "PL"), new Weekday { DayNumber = 1, Name = "Poniedziałek", ShortName = "Pon" } },
                { (2, "PL"), new Weekday { DayNumber = 2, Name = "Wtorek", ShortName = "Wt" } },
                { (3, "PL"), new Weekday { DayNumber = 3, Name = "Środa", ShortName = "Śr" } },
                { (4, "PL"), new Weekday { DayNumber = 4, Name = "Czwartek", ShortName = "Czw" } },
                { (5, "PL"), new Weekday { DayNumber = 5, Name = "Piątek", ShortName = "Pt" } },
                { (6, "PL"), new Weekday { DayNumber = 6, Name = "Sobota", ShortName = "Sob" } },
                { (7, "PL"), new Weekday { DayNumber = 7, Name = "Niedziela", ShortName = "Ndz" } }
            });

        public static Weekday? GetDayOfWeek(int dayNumber, string language)
        {
            var key = (dayNumber, language.ToUpper());
            if (DaysOfWeekByLanguage.Value.TryGetValue(key, out Weekday? day))
            {
                return day;
            }

            return null;
        }
    }
}
