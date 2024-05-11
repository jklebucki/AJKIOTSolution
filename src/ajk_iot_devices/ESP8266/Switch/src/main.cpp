#define DEBUG_ESP_PORT Serial
#define DEFAULT_MQTT_BUFFER_SIZE_BYTES 4096
#define DEFAULT_MQTT_MAX_QUEUE (4096 * 8)
#include <ESP8266WiFi.h>
#include <ESP8266MQTTClient.h>
#include <ArduinoJson.h>
#include <ScheduleEntry.h>
#include <LinkedList.h>
#include <TimeLib.h>
#include <NTPClient.h>
#include <WiFiUdp.h>
#define OUT_PIN 0
WiFiUDP ntpUDP; // Obiekt UDP dla komunikacji z serwerem NTP
NTPClient timeClient(ntpUDP, "pool.ntp.org", 3600, 60000);

LinkedList<ScheduleEntry *> scheduleEntries = LinkedList<ScheduleEntry *>();
// WiFi credentials
const char *ssid = "Orange_Swiatlowod_B5AC";
const char *password = "cdHqhMotvgMSJ9L4tD";
// Device settings
const char *deviceId = "3";
const char *updateFeatureTopic = "updateFeature/3";
const char *updateScheduleTopic = "configSchedule/3";
const char *signalScheduleTopic = "signalSchedule/3";
const char *configDeviceTopic = "configDevice/3";
const char *controlDeviceTopic = "controlDevice/3";
const char *controlOnline = "online:3";
String signal = "";
int restartCounter = 0;

// Millis
unsigned long onlineWatchdog = 0;
bool watchdogEnabled = false;

// MQTT Server settings
const char *mqtt_server = "ws://ajkdesktop:5217/mqtt";
String fingerprint = "7E 36 22 01 F9 7E 99 2F C5 DB 3D BE AC 48 67 5B 5D 47 94 D2";

MQTTClient mqtt;

void setup_wifi()
{
  delay(10);
  WiFi.begin(ssid, password);
  while (WiFi.status() != WL_CONNECTED)
  {
    delay(500);
  }
  Serial.println("");
  Serial.println("WiFi connected");
  Serial.print("IP address: ");
  Serial.println(WiFi.localIP());
  Serial.print("DNS: ");
  Serial.println(WiFi.dnsIP());
}

bool isTimeWithinInterval(time_t currentTime, tmElements_t start, tmElements_t end)
{
  // Set a fixed date for all time comparisons (e.g., January 1, 2000)
  int fixedYear = 2000;
  int fixedMonth = 1;
  int fixedDay = 1;

  // Configure the fixed start and end dates
  tmElements_t fixedStart, fixedEnd;
  fixedStart.Year = fixedYear - 1970;
  fixedStart.Month = fixedMonth;
  fixedStart.Day = fixedDay;
  fixedStart.Hour = start.Hour;
  fixedStart.Minute = start.Minute;
  fixedStart.Second = 0;

  fixedEnd.Year = fixedYear - 1970;
  fixedEnd.Month = fixedMonth;
  fixedEnd.Day = fixedDay;
  fixedEnd.Hour = end.Hour;
  fixedEnd.Minute = end.Minute;
  fixedEnd.Second = 0;

  // Convert to time_t for easier comparison
  time_t fixedStartTime = makeTime(fixedStart);
  time_t fixedEndTime = makeTime(fixedEnd);

  // Extract current time components
  tmElements_t tempCurrent;
  breakTime(currentTime, tempCurrent);
  tempCurrent.Year = fixedYear - 1970;
  tempCurrent.Month = fixedMonth;
  tempCurrent.Day = fixedDay;
  time_t adjustedCurrentTime = makeTime(tempCurrent);
  // Adjust for midnight crossing
  if (fixedEndTime <= fixedStartTime)
  {
    fixedEndTime += SECS_PER_DAY; // Add one day to the end time
    if (adjustedCurrentTime < fixedStartTime)
    {
      adjustedCurrentTime += SECS_PER_DAY; // Adjust current time to the next day for comparison
    }
  }

  // Perform the comparison
  return (adjustedCurrentTime >= fixedStartTime && adjustedCurrentTime <= fixedEndTime);
}

void maintainPinState()
{
  int currentDayOfWeek = weekday();
  bool pinShouldBeOn = false;
  time_t currentTime = timeClient.getEpochTime();

  for (int i = 0; i < scheduleEntries.size(); i++)
  {
    ScheduleEntry *entry = scheduleEntries.get(i);
    if (entry->DayNumber == currentDayOfWeek)
    {
      if (isTimeWithinInterval(currentTime, entry->StartTime, entry->EndTime))
      {
        pinShouldBeOn = true;
        break;
      }
    }
  }
  digitalWrite(OUT_PIN, pinShouldBeOn ? HIGH : LOW); // Ustawienie stanu pinu
}

void showEntries()
{
  for (int i = 0; i < scheduleEntries.size(); i++)
  {
    ScheduleEntry *entry = scheduleEntries.get(i);
    Serial.printf("Entry %d: Day: %d, StartTime: %02d:%02d, EndTime: %02d:%02d\n", i, entry->DayNumber, entry->StartTime.Hour, entry->StartTime.Minute, entry->EndTime.Hour, entry->EndTime.Minute);
  }
}

String getAddress(String addressData)
{
  int colonIndex = addressData.indexOf(':');

  if (colonIndex != -1)
  {
    return addressData.substring(colonIndex + 1);
  }
  else
  {
    return "";
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
  configTime(3600, 3600, "pool.ntp.org", "time.nist.gov");
  while (time(nullptr) < SECS_YR_2000)
  { // Wait until the time is set
    delay(100);
    Serial.print(".");
  }
}

// Centralized subscription setup to reuse upon reconnect
void mqttSubscribeTopics()
{
  Serial.printf("Subscribe id: %d\r\n", mqtt.subscribe(updateFeatureTopic, 0));
  mqtt.subscribe(updateScheduleTopic, 1);
  mqtt.subscribe(signalScheduleTopic, 1);
  mqtt.subscribe(configDeviceTopic, 1);
  mqtt.subscribe(controlDeviceTopic, 1);
  restartCounter = 0;
}

void mqqtUnsubscribeTopics()
{
  mqtt.unSubscribe(updateFeatureTopic);
  mqtt.unSubscribe(updateScheduleTopic);
  mqtt.unSubscribe(signalScheduleTopic);
  mqtt.unSubscribe(configDeviceTopic);
  mqtt.unSubscribe(controlDeviceTopic);
  restartCounter++;
}

void clearScheduleEntries()
{
  while (scheduleEntries.size() > 0)
  {
    ScheduleEntry *entry = scheduleEntries.shift(); // Retrieve and remove the first element
    delete entry;                                   // Delete the dynamically allocated ScheduleEntry
  }
}

void setup()
{
  pinMode(OUT_PIN, OUTPUT);
  Serial.begin(115200);
  setup_wifi();
  setTime();
  Serial.println("\nTime set.");
  digitalWrite(OUT_PIN, LOW);
  Serial.printf("Device id: %s \r\n", deviceId);
  Serial.printf("Update feature topic: %s \r\n", updateFeatureTopic);
  Serial.printf("Config schedule topic: %s \r\n", updateScheduleTopic);
  Serial.printf("Control signal topic: %s \r\n", signalScheduleTopic);
  Serial.printf("Config device topic: %s \r\n", configDeviceTopic);
  Serial.printf("Control device topic: %s \r\n", controlDeviceTopic);
  mqtt.onSecure([](WiFiClientSecure *client, String host)
                {
      Serial.printf("Secure: %s\r\n", host.c_str());
      return client->setFingerprint(fingerprint.c_str()); });

  // topic, data, data is continuing
  mqtt.onData([](String topic, String data, bool cont)
              {
    Serial.printf("Data received, topic: %s, data: ", topic.c_str());
    Serial.println(data);
    if (topic == signalScheduleTopic) {
        if (data == "start") {
            signal = "start";
            clearScheduleEntries();  // Clear existing schedule entries if starting anew
        } else if (data == "stop") {
            signal = "stop";
        }
    }
    // Process schedule updates only if the signal is "start"
    if (signal == "start" && topic == updateScheduleTopic) {
        parseSchedule(data.c_str());
    } });

  mqtt.onSubscribe([](int sub_id)
                   { 
    Serial.printf("Subscribe topic id: %d ok\r\n", sub_id); 
    mqtt.publish(controlDeviceTopic, controlOnline, 0, 0); });

  mqtt.onConnect([]()
                 { Serial.printf("MQTT: Connected\r\n"); });

  mqtt.begin(mqtt_server, deviceId);
  // mqtt.begin("ws://test.mosquitto.org:8443", {.lwtTopic = "hello", .lwtMsg = "offline", .lwtQos = 0, .lwtRetain = 0});
  // mqtt.begin("ws://mosquito.org:8443", "user", "pass");
  // mqtt.begin("ws://mosquito.org:8443", "clientId", "user", "pass");

  timeClient.begin();
}

void debugSystemStatus()
{
  int pinStatus = digitalRead(OUT_PIN);
  Serial.printf("Pin is %s\n", pinStatus == HIGH ? "HIGH" : "LOW");
  Serial.printf("Free RAM: %d bytes\n", ESP.getFreeHeap());
  Serial.printf("Current time: %s - Current weekday: %d\n", timeClient.getFormattedTime().c_str(), weekday());
  showEntries();
}

// Check if MQTT is connected, if not attempt to reconnect
void ensureMQTTConnected()
{
  if (!mqtt.connected())
  {
    mqqtUnsubscribeTopics();
    Serial.println("MQTT disconnected. Attempting to reconnect...");
    // Attempt to reconnect
    if (mqtt.connect())
    {
      // Re-subscribe to topics after successful reconnection
      mqttSubscribeTopics();
      Serial.println("Reconnected and subscribed.");
    }
    else
    {
      Serial.println("Failed to reconnect.");
    }
  }
}

void loop()
{
  if (restartCounter >= 10)
  {
    Serial.println("Not enough free RAM. Rebooting...");
    ESP.reset();
  }
  ensureMQTTConnected();
  timeClient.update();
  maintainPinState();
  unsigned long currentMillis = millis();
  mqtt.handle();
  if (currentMillis - onlineWatchdog >= 5000)
  {
    onlineWatchdog = currentMillis;
    debugSystemStatus();
    mqtt.publish(controlDeviceTopic, controlOnline, 0, 0);
  }
}