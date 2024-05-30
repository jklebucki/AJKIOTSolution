#include "ScheduleEntry.h"
#include <ArduinoJson.h>
#include <TimeLib.h>

ScheduleEntry::ScheduleEntry()
{
    StartTime = 0;
    EndTime = 0;
}

bool ScheduleEntry::parseJson(const char *jsonString)
{
    Serial.print("Parsing JSON: ");
    Serial.println(jsonString);
    DynamicJsonDocument doc(1024);
    DeserializationError error = deserializeJson(doc, jsonString);

    if (error)
    {
        Serial.print("deserializeJson() failed: ");
        Serial.println(error.f_str());
        return false;
    }

    if (!doc.containsKey("Id") || !doc.containsKey("FeatureId") || !doc.containsKey("DayNumber") ||
        !doc.containsKey("EntryNumber") || !doc.containsKey("StartTime") || !doc.containsKey("EndTime"))
    {
        Serial.println("JSON structure error: Missing required fields");
        return false;
    }

    for (JsonPair kv : doc.as<JsonObject>())
    {
        Serial.print(kv.key().c_str());
        Serial.print(": ");
        Serial.println(kv.value().as<String>());
        if (String(kv.key().c_str()).equals("Id"))
        {
            Id = kv.value().as<int>();
        }
        if (String(kv.key().c_str()).equals("FeatureId"))
        {
            FeatureId = kv.value().as<int>();
        }
        if (String(kv.key().c_str()).equals("DayNumber"))
        {
            DayNumber = kv.value().as<int>();
        }
        if (String(kv.key().c_str()).equals("EntryNumber"))
        {
            EntryNumber = kv.value().as<int>();
        }
        if (String(kv.key().c_str()).equals("StartTime"))
        {
            parseTime(kv.value().as<String>(), StartTime);
        }
        if (String(kv.key().c_str()).equals("EndTime"))
        {
            parseTime(kv.value().as<String>(), EndTime);
        }
    }

    // Print values after parsing
    Serial.println("JSON parsed successfully. Displaying values:");
    Serial.print("Id: ");
    Serial.println(Id);
    Serial.print("FeatureId: ");
    Serial.println(FeatureId);
    Serial.print("DayNumber: ");
    Serial.println(DayNumber);
    Serial.print("EntryNumber: ");
    Serial.println(EntryNumber);
    Serial.print("StartTime: ");
    Serial.println(StartTime);
    Serial.print("EndTime: ");
    Serial.println(EndTime);
    Serial.println("JSON parsed successfully");
    return true;
}

bool ScheduleEntry::parseTime(const String &timeStr, time_t &timeEl)
{
    int firstColon = timeStr.indexOf(':');    // Find the first colon
    int lastColon = timeStr.lastIndexOf(':'); // Find the last colon

    if (firstColon == -1 || lastColon == -1 || firstColon == lastColon)
    {
        Serial.println("Failed to parse time string: " + timeStr);
        return false;
    }

    // Extract hours, minutes, and seconds
    String hourStr = timeStr.substring(0, firstColon);
    String minuteStr = timeStr.substring(firstColon + 1, lastColon);
    String secondStr = timeStr.substring(lastColon + 1);

    // Convert strings to integers
    int hour = hourStr.toInt();
    int minute = minuteStr.toInt();
    int second = secondStr.toInt();

    // Additional check to ensure time values are within valid ranges
    if (hour < 0 || hour > 23 ||
        minute < 0 || minute > 59 ||
        second < 0 || second > 59)
    {
        Serial.println("Time values out of range: " + timeStr);
        return false;
    }

    // Convert to time_t (seconds since midnight)
    timeEl = hour * 3600 + minute * 60 + second;

    return true;
}

bool ScheduleEntry::isCurrentTimeInRange(time_t currentTime) const
{
    return (currentTime >= StartTime && currentTime <= EndTime);
}

String ScheduleEntry::timeToString(time_t time) const
{
    // Convert time_t (seconds since midnight) to HH:MM:SS format
    int hours = (time % 86400L) / 3600;
    int minutes = (time % 3600) / 60;
    int seconds = time % 60;

    char buffer[9];
    sprintf(buffer, "%02d:%02d:%02d", hours, minutes, seconds);
    return String(buffer);
}
