#include "ScheduleEntry.h"
#include <ArduinoJson.h>
#include <TimeLib.h>

ScheduleEntry::ScheduleEntry()
{
    memset(&StartTime, 0, sizeof(StartTime));
    memset(&EndTime, 0, sizeof(EndTime));
}

bool ScheduleEntry::parseJson(const char *jsonString)
{
    Serial.print("Parsing JSON: ");
    Serial.println(jsonString);
    JsonDocument doc;
    DeserializationError error = deserializeJson(doc, jsonString);

    if (error)
    {
        Serial.print("deserializeJson() failed: ");
        Serial.println(error.f_str());
        return false;
    }

    for (JsonPair kv : doc.as<JsonObject>()) {
        Serial.print(kv.key().c_str());
        Serial.print(": ");
        Serial.println(kv.value().as<String>());
    }

    // Sprawdź, czy każdy klucz istnieje i czy typ danych jest odpowiedni
    if (!doc.containsKey("Id") || !doc.containsKey("FeatureId") || !doc.containsKey("DayNumber") || 
        !doc.containsKey("EntryNumber") || !doc.containsKey("StartTime") || !doc.containsKey("EndTime")) {
        Serial.println("JSON structure error: Missing required fields");
        return false;
    }

    Id = doc["Id"].as<int>();
    FeatureId = doc["FeatureId"].as<int>();
    DayNumber = doc["DayNumber"].as<int>();
    EntryNumber = doc["EntryNumber"].as<int>();

    if (!parseTime(doc["StartTime"].as<String>(), StartTime) || !parseTime(doc["EndTime"].as<String>(), EndTime))
    {
        Serial.println("Time parsing error");
        return false;
    }

    return true;
}

bool ScheduleEntry::parseTime(const String &timeStr, tmElements_t &timeEl)
{
    int readItems = sscanf(timeStr.c_str(), "%d:%d:%d", &timeEl.Hour, &timeEl.Minute, &timeEl.Second);
    if (readItems != 3)
    { // Jeśli nie uda się przeczytać trzech wartości, zwróć false
        Serial.println("Failed to parse time string: " + timeStr);
        return false;
    }
    return true;
}
