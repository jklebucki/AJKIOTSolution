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
    tmElements_t StartTime; // Using tmElements_t to store time
    tmElements_t EndTime;   // Using tmElements_t to store time

    ScheduleEntry();
    bool parseJson(const char *jsonString);

private:
    bool parseTime(const String &timeStr, tmElements_t &timeEl);
};

#endif // SCHEDULE_ENTRY_H
