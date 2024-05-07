#define DEBUG_ESP_PORT Serial
#define DEFAULT_MQTT_BUFFER_SIZE_BYTES 4096
#define DEFAULT_MQTT_MAX_QUEUE (4096 * 8)
#include <ESP8266WiFi.h>
#include <ESP8266Ping.h>
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
// Millis
unsigned long onlineWatchdog = 0;
bool watchdogEnabled = false;

// MQTT Server settings
const char *mqtt_server = "ws://ajkdesktop:5217/mqtt";
String address = "ajkdesktop";

bool pinging = false;
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
  time_t startTime = makeTime(start);
  time_t endTime = makeTime(end);
  return currentTime >= startTime && currentTime <= endTime;
}

void maintainPinState()
{
  int currentDayOfWeek = weekday(); // Pobranie aktualnego dnia tygodnia (1 = niedziela, 2 = poniedziałek, ..., 7 = sobota)
  if (currentDayOfWeek == 1)
  {
    currentDayOfWeek = 7; // Niedziela jako 7
  }
  else
  {
    currentDayOfWeek -= 1; // Poniedziałek jako 1, ..., Sobota jako 6
  }
  bool pinShouldBeOn = false;

  for (int i = 0; i < scheduleEntries.size(); i++)
  {
    ScheduleEntry *entry = scheduleEntries.get(i);
    if (entry->DayNumber == currentDayOfWeek)
    {
      if (isTimeWithinInterval(now(), entry->StartTime, entry->EndTime))
      {
        pinShouldBeOn = true;
        break; // Jeśli znaleziono pasujący zakres, nie ma potrzeby dalszego przeszukiwania
      }
    }
    Serial.print((entry->StartTime).Hour);
    Serial.print(":");
    Serial.println((entry->StartTime).Minute);
  }
  digitalWrite(OUT_PIN, pinShouldBeOn ? HIGH : LOW); // Ustawienie stanu pinu
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
    Serial.println("JSON parsed successfully!");
    scheduleEntries.add(entry);
  }
  else
  {
    Serial.println("Failed to parse JSON.");
    delete entry; // Important to prevent memory leak
  }
}

void setup()
{
  Serial.begin(115200);
  setup_wifi();
  configTime(3 * 3600, 0, "pool.ntp.org", "time.nist.gov");
  Serial.printf("Device id: %s \r\n", deviceId);
  Serial.printf("Update feature topic: %s \r\n", updateFeatureTopic);
  Serial.printf("Config schedule topic: %s \r\n", updateScheduleTopic);
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
    if(topic == updateScheduleTopic){
    parseSchedule(data.c_str());
    }

    if (data.indexOf("Stop pinging") != -1)
    pinging = false;
    else if (data.indexOf("Start pinging") != -1)
    pinging = true;
    if (data.indexOf("Address:") != -1)
    address = getAddress(data);
    if (data.indexOf("Watchdog:") != -1)
    watchdogEnabled = !watchdogEnabled;
    mqtt.unSubscribe("/qos0"); });

  mqtt.onSubscribe([](int sub_id)
                   { 
    Serial.printf("Subscribe topic id: %d ok\r\n", sub_id); 
    mqtt.publish(updateFeatureTopic, "Device online", 0, 0); });

  mqtt.onConnect([]()
                 {
    Serial.printf("MQTT: Connected\r\n");
    Serial.printf("Subscribe id: %d\r\n", mqtt.subscribe(updateFeatureTopic, 0));
    mqtt.subscribe(updateScheduleTopic, 1);
    mqtt.subscribe(signalScheduleTopic, 1);
    mqtt.subscribe(configDeviceTopic, 1);
    mqtt.subscribe(controlDeviceTopic, 1); });

  mqtt.begin("ws://172.16.90.151:5217/mqtt", deviceId, {.lwtTopic = "test/topic", .lwtMsg = "Offline", .lwtQos = 0, .lwtRetain = 0});
  // mqtt.begin("ws://test.mosquitto.org:8443", {.lwtTopic = "hello", .lwtMsg = "offline", .lwtQos = 0, .lwtRetain = 0});
  // mqtt.begin("ws://mosquito.org:8443", "user", "pass");
  // mqtt.begin("ws://mosquito.org:8443", "clientId", "user", "pass");

  timeClient.begin();
}

void loop()
{
  timeClient.update();
  maintainPinState();
  unsigned long currentMillis = millis();
  if (pinging)
    if (Ping.ping(address.c_str(), 1))
    {
      Serial.printf("%s Success. Time taken: ", address.c_str());
      Serial.print(Ping.averageTime());
      Serial.print(" ms DNS: ");
      Serial.println(WiFi.dnsIP());
    }
    else
    {
      Serial.printf("Ping %s failed, DNS: ", address.c_str(), WiFi.dnsIP());
    }
  mqtt.handle();
  if (watchdogEnabled && currentMillis - onlineWatchdog >= 5000)
  {
    onlineWatchdog = currentMillis;
    Serial.print("Free RAM: ");
    Serial.println(ESP.getFreeHeap());
    mqtt.publish(controlDeviceTopic, "Device online", 0, 0);
  }
}