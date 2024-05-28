#define OUT_PIN 2
#define RESET_PIN 21
#define EEPROM_SIZE 512
#define RESET_HOLD_TIME 10000 // 10 seconds
#define WIFI_RETRY_INTERVAL 5000
#include <AsyncTCP.h>
#include <ESPAsyncWebServer.h>
#include <EEPROM.h>
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

const int ssidAddress = 0;
const int passAddress = 64;
const int deviceIdAddress = 128;
const int mqttServerAddress = 192;
const int maxLength = 64;
char ssid[maxLength];
char password[maxLength];
char deviceId[maxLength];
char mqtt_server[maxLength];
char updateFeatureTopic[64];
char updateScheduleTopic[64];
char signalScheduleTopic[64];
char configDeviceTopic[64];
char controlDeviceTopic[64];
char controlOnline[64];
// MQTT Server details
const int mqtt_port = 8883;

WiFiClientSecure espClient;
PubSubClient client(espClient);

const char *ntpServer = "pool.ntp.org";
WiFiUDP ntpUDP;
NTPClient timeClient(ntpUDP, "pool.ntp.org", 3600, 60000);
AjkLinkedList<ScheduleEntry *> scheduleEntries = AjkLinkedList<ScheduleEntry *>();
String signalSchedule = "";
unsigned long localMillis = 0;
unsigned long buttonPressStartTime = 0;
bool buttonPressed = false;
unsigned long lastWiFiAttemptTime = 0;
bool cofigMode = false;

AsyncWebServer server(80);

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

void setDeviceId(const char *newDeviceId)
{
  writeEEPROM(newDeviceId, deviceIdAddress);
}

void setMQTTServer(const char *newMQTTServer)
{
  writeEEPROM(newMQTTServer, mqttServerAddress);
}

void handleNewWiFiCredentials(const String &newSSID, const String &newPassword, const String &newDeviceId, const String &newMQTTServer)
{
  newSSID.toCharArray(ssid, maxLength);
  newPassword.toCharArray(password, maxLength);
  newDeviceId.toCharArray(deviceId, maxLength);
  newMQTTServer.toCharArray(mqtt_server, maxLength);

  setWiFiCredentials(ssid, password);
  setDeviceId(deviceId);
  setMQTTServer(mqtt_server);

  snprintf(updateFeatureTopic, sizeof(updateFeatureTopic), "updateFeature/%s", deviceId);
  snprintf(updateScheduleTopic, sizeof(updateScheduleTopic), "configSchedule/%s", deviceId);
  snprintf(signalScheduleTopic, sizeof(signalScheduleTopic), "signalSchedule/%s", deviceId);
  snprintf(configDeviceTopic, sizeof(configDeviceTopic), "configDevice/%s", deviceId);
  snprintf(controlDeviceTopic, sizeof(controlDeviceTopic), "controlDevice/%s", deviceId);
  snprintf(controlOnline, sizeof(controlOnline), "online:%s", deviceId);

  Serial.println("New WiFi credentials, Device ID, and MQTT Server saved. Restarting...");
  ESP.restart();
}

bool isDaylightSavingTime(const tm &timeinfo)
{
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
  if (month == 10 && (day - wday) >= 25 && wday == 0)
  {
    return false;
  }
  if (month == 10 && (day - wday) >= 25 && wday != 0)
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

void enterWiFiConfigMode()
{
  Serial.println("Entering WiFi Config Mode...");
  WiFi.disconnect();

  IPAddress local_IP(192, 168, 4, 1);
  IPAddress gateway(192, 168, 4, 1);
  IPAddress subnet(255, 255, 255, 0);

  if (!WiFi.softAPConfig(local_IP, gateway, subnet))
  {
    Serial.println("AP Config Failed");
  }

  if (!WiFi.softAP("AJK_IoT_ESP32_Config"))
  {
    Serial.println("AP Start Failed");
  }

  Serial.print("AP IP address: ");
  Serial.println(WiFi.softAPIP());

  server.on("/", HTTP_GET, [](AsyncWebServerRequest *request)
            { request->send(200, "text/html",
                            "<!DOCTYPE html>"
                            "<html>"
                            "<head>"
                            "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">"
                            "<style>"
                            "body { font-family: Arial, sans-serif; margin: 0; padding: 0; display: flex; justify-content: center; align-items: center; height: 100vh; background-color: #f2f2f2; }"
                            ".container { max-width: 400px; width: 100%; padding: 20px; background-color: white; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1); border-radius: 10px; }"
                            "h2 { text-align: center; }"
                            "input[type=text], input[type=password], input[type=submit] { width: 90%; padding: 10px; margin: 10px 10px; border: 1px solid #ccc; border-radius: 5px; }"
                            "input[type=submit] { background-color: #4CAF50; color: white; border: none; cursor: pointer; }"
                            "input[type=submit]:hover { background-color: #45a049; }"
                            "</style>"
                            "</head>"
                            "<body>"
                            "<div class=\"container\">"
                            "<h2>WiFi Configuration</h2>"
                            "<form action=\"/setWiFi\" method=\"POST\">"
                            "<label for=\"ssid\">SSID:</label>"
                            "<input type=\"text\" id=\"ssid\" name=\"ssid\">"
                            "<label for=\"password\">Password:</label>"
                            "<input type=\"text\" id=\"password\" name=\"password\">"
                            "<label for=\"deviceid\">Device ID:</label>"
                            "<input type=\"text\" id=\"deviceid\" name=\"deviceid\">"
                            "<label for=\"mqttserver\">MQTT Server:</label>"
                            "<input type=\"text\" id=\"mqttserver\" name=\"mqttserver\">"
                            "<input type=\"submit\" value=\"Submit\">"
                            "</form>"
                            "</div>"
                            "</body>"
                            "</html>"); });

  server.on("/setWiFi", HTTP_POST, [](AsyncWebServerRequest *request)
            {
        String newSSID;
        String newPassword;
        String newDeviceId;
        String newMQTTServer;

        if (request->hasParam("ssid", true) && request->hasParam("password", true) && request->hasParam("deviceid", true) && request->hasParam("mqttserver", true)) {
            newSSID = request->getParam("ssid", true)->value();
            newPassword = request->getParam("password", true)->value();
            newDeviceId = request->getParam("deviceid", true)->value();
            newMQTTServer = request->getParam("mqttserver", true)->value();
            request->send(200, "text/plain", "WiFi credentials, Device ID, and MQTT Server received. Restarting...");
            handleNewWiFiCredentials(newSSID, newPassword, newDeviceId, newMQTTServer);
        } else {
            request->send(400, "text/plain", "Missing SSID, Password, Device ID, or MQTT Server");
        } });

  server.begin();
}

void resetEEPROM()
{
  // Resetujemy wszystkie zapisane zmienne w EEPROM
  for (int i = 0; i < EEPROM_SIZE; i++)
  {
    EEPROM.write(i, 0);
  }
  EEPROM.commit();
  Serial.println("EEPROM reset. Restarting...");
  ESP.restart();
}

bool checkIfEEPROMEmpty()
{
  if (strlen(ssid) == 0 || strlen(password) == 0 || strlen(deviceId) == 0 || strlen(mqtt_server) == 0)
  {
    Serial.println("No WiFi credentials, Device ID, or MQTT Server found. Entering config mode.");
    return true;
  }
  return false;
}

void setMqttTopics()
{
  snprintf(updateFeatureTopic, sizeof(updateFeatureTopic), "updateFeature/%s", deviceId);
  snprintf(updateScheduleTopic, sizeof(updateScheduleTopic), "configSchedule/%s", deviceId);
  snprintf(signalScheduleTopic, sizeof(signalScheduleTopic), "signalSchedule/%s", deviceId);
  snprintf(configDeviceTopic, sizeof(configDeviceTopic), "configDevice/%s", deviceId);
  snprintf(controlDeviceTopic, sizeof(controlDeviceTopic), "controlDevice/%s", deviceId);
  snprintf(controlOnline, sizeof(controlOnline), "online:%s", deviceId);
}

bool connectToWiFi()
{
  Serial.println("Connecting to WiFi...");
  WiFi.begin(ssid, password);

  unsigned long startTime = millis();
  while (WiFi.status() != WL_CONNECTED)
  {
    if (millis() - startTime >= WIFI_RETRY_INTERVAL)
    {
      Serial.println("Failed to connect to WiFi");
      return false;
    }
    delay(500);
    Serial.print(".");
  }

  Serial.println("");
  Serial.println("WiFi connected");
  Serial.println("IP Address: ");
  Serial.println(WiFi.localIP());
  setMqttTopics();
  return true;
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

void configDemand()
{
  client.publish(configDeviceTopic, deviceId);
}

void setup()
{
  pinMode(OUT_PIN, OUTPUT);
  pinMode(RESET_PIN, INPUT_PULLUP);
  Serial.begin(115200);

  // Initialize EEPROM
  if (!EEPROM.begin(EEPROM_SIZE))
  {
    Serial.println("Błąd inicjalizacji EEPROM");
    return;
  }

  // resetEEPROM();
  // setWiFiCredentials("YourSSID", "YourPassword");
  // setDeviceId("YourDeviceId");
  // setMQTTServer("YourMQTTServer domain name or IP address");

  // Read EEPROM
  readEEPROM(ssid, ssidAddress);
  readEEPROM(password, passAddress);
  readEEPROM(deviceId, deviceIdAddress);
  readEEPROM(mqtt_server, mqttServerAddress);
  if (checkIfEEPROMEmpty()) // Check if WiFi credentials are readed from EEPROM if not enter WiFi config mode
  {
    Serial.println("No WiFi credentials, Device ID, or MQTT Server found. Entering config mode...");
    cofigMode = true;
    enterWiFiConfigMode();
  }
  else
  {
    if (!connectToWiFi())
    {
      Serial.println("Failed to connect to WiFi at startup. Continuing to loop...");
    }

    setTime();
    timeClient.update();
    digitalWrite(OUT_PIN, LOW);
    loadCertificates(espClient);
    client.setServer(mqtt_server, mqtt_port);
    client.setCallback(callback);
    configDemand();
  }
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

void checkResetButton()
{
  if (digitalRead(RESET_PIN) == LOW)
  { // Check if reset button is pressed
    if (!buttonPressed)
    {
      buttonPressed = true;
      buttonPressStartTime = millis(); // Start measuring time when button is pressed
    }
    if (millis() - buttonPressStartTime >= RESET_HOLD_TIME)
    { // Check if button is held for RESET_HOLD_TIME
      resetEEPROM();
    }
  }
  else
  {
    buttonPressed = false; // Reset flag when button is released
  }
}

void loop()
{
  checkResetButton();
  if (!cofigMode)
  {
    if (WiFi.status() != WL_CONNECTED)
    { // Check if WiFi is connected
      unsigned long currentMillis = millis();
      if (currentMillis - lastWiFiAttemptTime >= WIFI_RETRY_INTERVAL)
      {
        lastWiFiAttemptTime = currentMillis;
        if (connectToWiFi())
        {
          configDemand();
          Serial.println("WiFi reconnected");
        }
      }
    }

    if (WiFi.status() == WL_CONNECTED)
    {
      if (!client.connected())
      {
        reconnect();
        configDemand();
      }
    }

    maintainPinState();
    unsigned long currentMillis = millis();
    if (currentMillis - localMillis >= 5000)
    {
      timeClient.update();
      localMillis = currentMillis;
      printLocalTime();
      debugSystemStatus();
      client.publish(controlDeviceTopic, controlOnline);
    }
    client.loop();
  }
}
