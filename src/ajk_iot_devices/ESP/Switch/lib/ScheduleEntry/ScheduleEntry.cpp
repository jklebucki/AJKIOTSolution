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

    // Dodaj wydruk wartości po zakończeniu parsowania
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
    Serial.printf("%02d:%02d:%02d\n", StartTime.Hour, StartTime.Minute, StartTime.Second);
    Serial.print("EndTime: ");
    Serial.printf("%02d:%02d:%02d\n", EndTime.Hour, EndTime.Minute, EndTime.Second);
    Serial.println("JSON parsed successfully");
    return true;
}

bool ScheduleEntry::parseTime(const String &timeStr, tmElements_t &timeEl)
{
    int firstColon = timeStr.indexOf(':');    // Znajdź pierwszy dwukropek
    int lastColon = timeStr.lastIndexOf(':'); // Znajdź ostatni dwukropek

    if (firstColon == -1 || lastColon == -1 || firstColon == lastColon)
    {
        Serial.println("Failed to parse time string: " + timeStr);
        return false;
    }

    // Ekstrakcja godzin, minut i sekund
    String hourStr = timeStr.substring(0, firstColon);
    String minuteStr = timeStr.substring(firstColon + 1, lastColon);
    String secondStr = timeStr.substring(lastColon + 1);

    // Konwersja stringów na liczby całkowite
    timeEl.Hour = hourStr.toInt();
    timeEl.Minute = minuteStr.toInt();
    timeEl.Second = secondStr.toInt();

    // Dodatkowe sprawdzenie, czy wartości czasu są w prawidłowym zakresie
    if (timeEl.Hour < 0 || timeEl.Hour > 23 ||
        timeEl.Minute < 0 || timeEl.Minute > 59 ||
        timeEl.Second < 0 || timeEl.Second > 59)
    {
        Serial.println("Time values out of range: " + timeStr);
        return false;
    }

    return true;
}
