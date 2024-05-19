#define OUT_PIN 2
#include <WiFi.h>
#include <WiFiClientSecure.h>
#include <PubSubClient.h>
#include "FS.h"
#include "SPIFFS.h"
#include <ScheduleEntry.h>
#include <vector>
#include <LinkedList.h>
#include <TimeLib.h>
#include <NTPClient.h>

const char *ssid = "Orange_Swiatlowod_B5AC";
const char *password = "cdHqhMotvgMSJ9L4tD";

// MQTT Server details
const char *mqtt_server = "ajkdesktop";
const int mqtt_port = 8883;
const char *deviceId = "3";
const char *updateFeatureTopic = "updateFeature/3";
const char *updateScheduleTopic = "configSchedule/3";
const char *signalScheduleTopic = "signalSchedule/3";
const char *configDeviceTopic = "configDevice/3";
const char *controlDeviceTopic = "controlDevice/3";
const char *controlOnline = "online:3";

WiFiClientSecure espClient;
PubSubClient client(espClient);

const char *ntpServer = "pool.ntp.org";
WiFiUDP ntpUDP;
NTPClient timeClient(ntpUDP, "pool.ntp.org", 3600, 60000);
LinkedList<ScheduleEntry *> scheduleEntries = LinkedList<ScheduleEntry *>();
String signalSchedule = "";
unsigned long localMillis = 0;

bool isDaylightSavingTime(const tm &timeinfo)
{
  // Sprawdzanie czy jest czas letni (ostatnia niedziela marca do ostatniej niedzieli października)
  int month = timeinfo.tm_mon + 1;
  int day = timeinfo.tm_mday;
  int wday = timeinfo.tm_wday;

  // Marzec: sprawdzamy ostatnią niedzielę
  if (month == 3 && (day - wday) >= 25)
  {
    return true;
  }

  // Kwiecień do września: zawsze czas letni
  if (month > 3 && month < 10)
  {
    return true;
  }

  // Październik: sprawdzamy ostatnią niedzielę
  if (month == 10 && (day - wday) < 25)
  {
    return true;
  }

  return false;
}

int mondayAsFirstDayOfWeek(int weekDay)
{
  if (weekDay == 0)
  {
    return 7;
  }
  return weekDay;
}
void maintainPinState()
{
  timeClient.update();
  struct tm timeinfo;
  if (getLocalTime(&timeinfo))
  {
    int timeOffsetSeconds = 0;
    if (isDaylightSavingTime(timeinfo))
    {
      timeOffsetSeconds = 3600; // Przesunięcie czasowe dla CEST (UTC+2)
    }

    time_t currentTime = (timeClient.getEpochTime() + timeOffsetSeconds) % 86400; // Użycie czasu NTP z przesunięciem czasowym

    int currentDayOfWeek = mondayAsFirstDayOfWeek(timeinfo.tm_wday);
    bool pinShouldBeOn = false;

    for (int i = 0; i < scheduleEntries.size(); i++)
    {
      ScheduleEntry *entry = scheduleEntries.get(i);
      if (entry->DayNumber == currentDayOfWeek)
      {
        if (entry->isCurrentTimeInRange(currentTime))
        {
          pinShouldBeOn = true;
          break;
        }
      }
    }
    digitalWrite(OUT_PIN, pinShouldBeOn ? HIGH : LOW); // Ustawienie stanu pinu
  }
  else
  {
    Serial.println("Failed to obtain time");
  }
}

void showEntries()
{
  for (int i = 0; i < scheduleEntries.size(); i++)
  {
    ScheduleEntry *entry = scheduleEntries.get(i);
    Serial.printf("Entry %d: Day: %d, StartTime: %s, EndTime: %s", i, entry->DayNumber, entry->timeToString(entry->StartTime), entry->timeToString(entry->EndTime));
    Serial.printf(" - StartTime: %d, EndTime: %d", entry->StartTime, entry->EndTime);
    Serial.println();
  }
}

void parseSchedule(const char *json)
{
  ScheduleEntry *entry = new ScheduleEntry();
  if (entry->parseJson(json))
  {
    scheduleEntries.add(entry);
    Serial.println("Entry added!");
  }
  else
  {
    Serial.println("Failed to parse JSON.");
    delete entry; // Important to prevent memory leak
  }
}

void setTime()
{
  configTime(0, 0, "pool.ntp.org", "time.nist.gov");
  while (time(nullptr) < SECS_YR_2000)
  { // Wait until the time is set
    delay(100);
    Serial.print(".");
  }
  Serial.println("Time set!");
}

void printLocalTime()
{
  struct tm timeinfo;
  if (getLocalTime(&timeinfo))
  {
    int timeOffsetSeconds = 0;
    if (isDaylightSavingTime(timeinfo))
    {
      timeOffsetSeconds = 3600; // Przesunięcie czasowe dla CEST (UTC+2)
    }

    time_t currentTime = timeClient.getEpochTime() + timeOffsetSeconds; // Dodanie przesunięcia czasowego
    struct tm *localTimeInfo = localtime(&currentTime);

    char buffer[64]; // Bufor na sformatowany czas
    strftime(buffer, sizeof(buffer), "%A, %B %d %Y %H:%M:%S", localTimeInfo);
    Serial.println(buffer);
    Serial.print("Epoch time: \t");
    Serial.println(currentTime % 86400); // Czas z klienta NTP
    Serial.print("Current time: \t");
    Serial.println(mktime(localTimeInfo) % 86400); // Czas z `localtime`
  }
  else
  {
    Serial.println("Failed to obtain time");
  }
}

void setup_wifi()
{
  delay(10);
  Serial.println("Connecting to WiFi...");
  WiFi.begin(ssid, password);

  while (WiFi.status() != WL_CONNECTED)
  {
    delay(500);
    Serial.print(".");
  }

  Serial.println("");
  Serial.println("WiFi connected");
  Serial.println("IP Address: ");
  Serial.println(WiFi.localIP());
}

void reconnect()
{
  while (!client.connected())
  {
    Serial.print("Connecting to MQTT server...");
    if (client.connect(deviceId))
    {
      Serial.println("connected");
      client.subscribe(updateFeatureTopic, 1);
      client.subscribe(updateScheduleTopic, 1);
      client.subscribe(signalScheduleTopic, 1);
      client.subscribe(configDeviceTopic, 1);
      client.subscribe(controlDeviceTopic, 1);
    }
    else
    {
      Serial.print("failed, rc=");
      Serial.print(client.state());
      Serial.println(" try again in 5 seconds");
      delay(5000);
    }
  }
}
void clearScheduleEntries()
{
  while (scheduleEntries.size() > 0)
  {
    ScheduleEntry *entry = scheduleEntries.shift(); // Retrieve and remove the first element
    delete entry;                                   // Delete the dynamically allocated ScheduleEntry
  }
}

void callback(char *topic, byte *payload, unsigned int length)
{
  Serial.print("[");
  Serial.print(topic);
  Serial.print("] ");
  String data = String((char *)payload).substring(0, length);
  Serial.println(data);
  String topicStr(topic);
  if (topicStr == signalScheduleTopic)
  {
    if (data == String("start"))
    {
      signalSchedule = String("start");
      clearScheduleEntries(); // Clear existing schedule entries if starting anew
    }
    else if (data == String("stop"))
    {
      signalSchedule = String("stop");
    }
  }
  // Process schedule updates only if the signal is "start"
  if (signalSchedule == String("start") && topicStr == updateScheduleTopic)
  {
    parseSchedule(data.c_str());
  }
}

bool loadCertFile(const char *path, std::function<bool(Stream &, size_t)> loadFunction)
{
  if (!SPIFFS.exists(path))
  {
    Serial.printf("File %s does not exist\n", path);
    return false;
  }
  File file = SPIFFS.open(path, "r");
  if (!file)
  {
    Serial.printf("Failed to open file %s\n", path);
    return false;
  }
  size_t size = file.size();
  bool result = loadFunction(file, size);
  file.close();
  return result;
}

void loadCertificates(WiFiClientSecure &client)
{
  if (!SPIFFS.begin(true))
  {
    Serial.println("An error occurred while mounting SPIFFS");
    return;
  }

  if (loadCertFile("/client-cert.pem", [&client](Stream &stream, size_t size)
                   { return client.loadCertificate(stream, size); }))
  {
    Serial.println("Certificate loaded");
  }
  else
  {
    Serial.println("Failed to load certificate");
  }

  if (loadCertFile("/client-key.pem", [&client](Stream &stream, size_t size)
                   { return client.loadPrivateKey(stream, size); }))
  {
    Serial.println("Private key loaded");
  }
  else
  {
    Serial.println("Failed to load private key");
  }

  if (loadCertFile("/ca-cert.pem", [&client](Stream &stream, size_t size)
                   { return client.loadCACert(stream, size); }))
  {
    Serial.println("Root CA loaded");
  }
  else
  {
    Serial.println("Failed to load root CA");
  }
}

void setup()
{
  pinMode(OUT_PIN, OUTPUT);
  Serial.begin(115200);
  setup_wifi();
  setTime();
  timeClient.update();
  digitalWrite(OUT_PIN, LOW);
  loadCertificates(espClient);
  client.setServer(mqtt_server, mqtt_port);
  client.setCallback(callback);
}

void debugSystemStatus()
{
  int pinStatus = digitalRead(OUT_PIN);
  Serial.printf("Pin is %s\n", pinStatus == HIGH ? "HIGH" : "LOW");
  Serial.printf("Free RAM: %d bytes\n", ESP.getFreeHeap());
  struct tm timeinfo;
  getLocalTime(&timeinfo);
  Serial.printf("Current time: %02d:%02d:%02d - Current weekday: %d\n", timeinfo.tm_hour, timeinfo.tm_min, timeinfo.tm_sec, mondayAsFirstDayOfWeek(timeinfo.tm_wday));
  showEntries();
}

void loop()
{
  if (!client.connected())
  {
    reconnect();
  }
  maintainPinState();
  unsigned long currentMillis = millis();
  if (currentMillis - localMillis >= 5000)
  {
    timeClient.update();
    localMillis = currentMillis;
    printLocalTime();
    debugSystemStatus();
  }
  client.loop();
}
