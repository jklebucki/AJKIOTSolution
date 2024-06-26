#define DEBUG_ESP_PORT Serial
#define OUT_PIN 0
#define EEPROM_SIZE 512
#include <EEPROM.h>
#include <ESP8266WiFi.h>
#include <ESP8266MQTTClient.h>
#include <ArduinoJson.h>
#include <ScheduleEntry.h>
#include <LinkedList.h>
#include <TimeLib.h>
#include <NTPClient.h>
#include <WiFiClientSecureBearSSL.h>
#include <WiFiUdp.h>

#define OUT_PIN 2

const char *ntpServer = "pool.ntp.org";
WiFiUDP ntpUDP;
NTPClient timeClient(ntpUDP, "pool.ntp.org", 3600, 60000);

LinkedList<ScheduleEntry *> scheduleEntries = LinkedList<ScheduleEntry *>();

// WiFi credentials
const int ssidAddress = 0;
const int passAddress = 64;
const int maxLength = 64;
char ssid[maxLength];
char password[maxLength];

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
const char *mqtt_server = "ajkdesktop"; // Adres serwera MQTT (bez ws:// i portu)

// Certyfikaty przechowywane w pamięci PROGMEM
static const char cacert[] PROGMEM = R"EOF(
)EOF";

static const char client_cert[] PROGMEM = R"EOF(
)EOF";

static const char client_key[] PROGMEM = R"EOF(
)EOF";

BearSSL::WiFiClientSecure net;
MQTTClient mqtt;

void readEEPROM(char *strToRead, int addrOffset)
{
  for (int i = 0; i < maxLength; i++)
  {
    char c = EEPROM.read(addrOffset + i);
    if (c == '\0')
    {
      strToRead[i] = '\0';
      break;
    }
    strToRead[i] = c;
  }
}

void writeEEPROM(const char *strToWrite, int addrOffset)
{
  int len = strlen(strToWrite);
  for (int i = 0; i < len; i++)
  {
    EEPROM.write(addrOffset + i, strToWrite[i]);
  }
  EEPROM.write(addrOffset + len, '\0');
  EEPROM.commit();
}

void setWiFiCredentials(const char *newSsid, const char *newPassword)
{
  writeEEPROM(newSsid, ssidAddress);
  writeEEPROM(newPassword, passAddress);
}

void setup_wifi()
{
  delay(10);
  WiFi.begin(ssid, password);
  while (WiFi.status() != WL_CONNECTED)
  {
    delay(500);
    Serial.print(".");
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
  struct tm timeinfo;
  int currentDayOfWeek = -1;
  if (getLocalTime(&timeinfo))
  {
    currentDayOfWeek = timeinfo.tm_wday;
  }
  else
  {
    Serial.println("Failed to obtain time");
    return;
  }

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
  Serial.println("Subscribing to MQTT topics...");
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
  EEPROM.begin(EEPROM_SIZE);
  // setWiFiCredentials("YourSSID", "YourPassword");
  Serial.begin(115200);
  // Read EEPROM
  readEEPROM(ssid, ssidAddress);
  readEEPROM(password, passAddress);
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

  // Konfiguracja połączenia SSL/TLS
  BearSSL::X509List *cert = nullptr;
  BearSSL::X509List *client_crt = nullptr;
  BearSSL::PrivateKey *key = nullptr;

  Serial.println("Initializing certificates...");
  cert = new BearSSL::X509List(cacert);
  client_crt = new BearSSL::X509List(client_cert);
  key = new BearSSL::PrivateKey(client_key);

  if (!cert || !client_crt || !key)
  {
    Serial.println("Failed to allocate memory for certificates or key.");
    return;
  }

  net.setTrustAnchors(cert);
  net.setClientRSACert(client_crt, key);

  Serial.println("Certificates initialized.");

  mqtt.begin(mqtt_server, 8883, net); // Używamy portu 8883 dla połączeń SSL/TLS

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

  mqtt.onDisconnect([]()
                    {
    Serial.printf("MQTT: Disconnected\r\n");
    mqqtUnsubscribeTopics(); });

  Serial.println("Subscribing to MQTT topics...");
  mqttSubscribeTopics();

  Serial.println("Connecting to MQTT...");
  mqtt.connect();
  Serial.println("MQTT connection attempt finished.");

  timeClient.begin();
}

void debugSystemStatus()
{
  int pinStatus = digitalRead(OUT_PIN);
  Serial.printf("Pin is %s\n", pinStatus == HIGH ? "HIGH" : "LOW");
  Serial.printf("Free RAM: %d bytes\n", ESP.getFreeHeap());
  struct tm timeinfo;
  getLocalTime(&timeinfo);
  Serial.printf("Current time: %s - Current weekday: %d\n", timeClient.getFormattedTime().c_str(), timeinfo.tm_wday);
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

void printLocalTime()
{
  struct tm timeinfo;
  if (getLocalTime(&timeinfo))
  {
    char buffer[64]; // Bufor na sformatowany czas
    strftime(buffer, sizeof(buffer), "%A, %B %d %Y %H:%M:%S", &timeinfo);
    Serial.println(buffer);
  }
  else
  {
    Serial.println("Failed to obtain time");
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
    printLocalTime();
    debugSystemStatus();
    mqtt.publish(controlDeviceTopic, controlOnline, 0, 0);
  }
}