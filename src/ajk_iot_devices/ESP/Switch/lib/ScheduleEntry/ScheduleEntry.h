#ifndef SCHEDULE_ENTRY_H
#define SCHEDULE_ENTRY_H

#include <Arduino.h>
#include <ArduinoJson.h>
#include <TimeLib.h> // Include the Time library to handle time elements

class ScheduleEntry
{
public:
    int Id;
    int FeatureId;
    int DayNumber;
    int EntryNumber;
    time_t StartTime; // Using time_t to store time
    time_t EndTime;   // Using time_t to store time

    ScheduleEntry();
    bool parseJson(const char *jsonString);
    bool isCurrentTimeInRange(time_t currentTime) const; // Function now takes currentTime as parameter
    String timeToString(time_t time) const;              // Function to convert time_t to HH:MM:SS string

private:
    bool parseTime(const String &timeStr, time_t &timeEl);
};

#endif // SCHEDULE_ENTRY_H
