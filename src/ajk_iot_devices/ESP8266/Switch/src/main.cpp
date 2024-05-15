#define DEBUG_ESP_PORT Serial
// define DEFAULT_MQTT_BUFFER_SIZE_BYTES 4096
// define DEFAULT_MQTT_MAX_QUEUE (4096 * 8)
#include <ESP8266WiFi.h>
#include <ESP8266MQTTClient.h>
#include <ArduinoJson.h>
#include <ScheduleEntry.h>
#include <LinkedList.h>
#include <TimeLib.h>
#include <NTPClient.h>
#include <WiFiClientSecureBearSSL.h> // Zaktualizowana biblioteka dla BearSSL
#include <WiFiUdp.h>                 // Dodajemy bibliotekę do obsługi UDP

#define OUT_PIN 2

const char *ntpServer = "pool.ntp.org";
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
const char *mqtt_server = "ajkdesktop"; // Adres serwera MQTT (bez ws:// i portu)

// Certyfikaty przechowywane w pamięci PROGMEM
static const char cacert[] PROGMEM = R"EOF(
-----BEGIN CERTIFICATE-----
MIID3DCCAsSgAwIBAgIUfHt0NwnhDvLDpKhFr8kaFQd0YsQwDQYJKoZIhvcNAQEL
BQAwajELMAkGA1UEBhMCUEwxFTATBgNVBAgMDGRvbG5vc2xhc2tpZTEQMA4GA1UE
BwwHTGVnbmljYTEMMAoGA1UECgwDQUpLMQ8wDQYDVQQLDAZBSksgSVQxEzARBgNV
BAMMCmFqa2Rlc2t0b3AwHhcNMjQwNTE1MTgyMTU4WhcNMzQwNTEzMTgyMTU4WjBq
MQswCQYDVQQGEwJQTDEVMBMGA1UECAwMZG9sbm9zbGFza2llMRAwDgYDVQQHDAdM
ZWduaWNhMQwwCgYDVQQKDANBSksxDzANBgNVBAsMBkFKSyBJVDETMBEGA1UEAwwK
YWprZGVza3RvcDCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAKjeE0xH
vAMdRZL/kGbZ0Curg/Upq0YyzTljGlibaufcSzh1ys0vGOxL4+lolFSNWPZcsC9i
wNqhMQPOH3515HN53cKFTVT5AmQb1kUnBUe36YxYInt6vujfz72K78WhKw8I1G8C
IXHoqBnheFd98PtZAI7do32WQMHVkn0KcD1JGd97T2SSmlwv74q3q8aAwv3/NXA8
DzS80mdMHrKjwmSCH1+aAKUPMdv4pM9sIYt2KkP0v7BfFy40hXVDAdCzwOn57ObL
Bienbd2v6avGGHzuSu5uBm7UEDpF6OGayJNzYoS81xWvda0dzbPX4hN5BDDqzjJU
VzxC/gbsEh0txUsCAwEAAaN6MHgwHQYDVR0OBBYEFOSPyl45gyLJJzw0epi6HuBN
7wMwMB8GA1UdIwQYMBaAFOSPyl45gyLJJzw0epi6HuBN7wMwMA8GA1UdEwEB/wQF
MAMBAf8wDgYDVR0PAQH/BAQDAgEGMBUGA1UdEQQOMAyCCmFqa2Rlc2t0b3AwDQYJ
KoZIhvcNAQELBQADggEBAA69uaqOiw7ahb7fbZKpbB0MJUYWNR+XywbaB3ubhChl
5J2gZ7XzxlxqmLd1JFFgWBxZ5ilTiGX820GAeAm3n5y0hyzclBoHu/TfRyVqBump
fzQTdD5jgWFMzRRUCyxMrNdE+zKcLOD4kJITddptRQwJ85aT3DJbteJo2N9nd9qT
Kr/gQOd1niBGc155RA0VMP6N8vmYNszwCXIZ+i30iSrSWudRht10xofUJa60di8M
9cbO0vFnlitqh2SJOtaBi9KjODfaSOjMkJurgUTSmyWHrOobSb+fZrMUNTiNPmwj
fBY4K3jpCDLURXHBYIGQi/JJlRChU37gXyBeMfT/IkE=
-----END CERTIFICATE-----
)EOF";

static const char client_cert[] PROGMEM = R"EOF(
-----BEGIN CERTIFICATE-----
MIID8jCCAtqgAwIBAgIUe2DZS3p6YgrMzikY/JMMX43GRb4wDQYJKoZIhvcNAQEL
BQAwajELMAkGA1UEBhMCUEwxFTATBgNVBAgMDGRvbG5vc2xhc2tpZTEQMA4GA1UE
BwwHTGVnbmljYTEMMAoGA1UECgwDQUpLMQ8wDQYDVQQLDAZBSksgSVQxEzARBgNV
BAMMCmFqa2Rlc2t0b3AwHhcNMjQwNTE1MTgyMTU4WhcNMjUwNTE1MTgyMTU4WjBp
MQswCQYDVQQGEwJQTDEVMBMGA1UECAwMZG9sbm9zbGFza2llMRAwDgYDVQQHDAdM
ZWduaWNhMQwwCgYDVQQKDANBSksxDzANBgNVBAsMBkFKSyBJVDESMBAGA1UEAwwJ
SW9URGV2aWNlMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAyU4liqrG
2Qn1NWGxdQoGNrKBLM64AnIfC3K8n8nFkUepyoKbaAVJRXwiEI9uAh0ikLDxRByH
yzYsYJF3z3dRc7jfAlSrW8VqU2UJgiEjrk5nj/CD/nonZN2ETijG9eLGVsfP/RZy
h3qoNxE0ejcXt4+8oixnbc+m+GoXX9fAFx8kNVavY9jsllXYVMef9Yt4TTXhhxdk
eL2jlR2hH1eFKU6UkUvKWUSDuqPovcNv+fGFCUlQRhXd/m+CtP4mcGYEsMwnACZO
TU4Rr0qmVc7zg7Z+MYCkVjPcdpZTH5+iNubPNwEVKYR7hRI+zvPdvnZT+C4r4VXM
5Wirxlp3WWrJwQIDAQABo4GQMIGNMB0GA1UdDgQWBBRY/Zvbp98aqgoXrCK0npVA
UzRP+DAfBgNVHSMEGDAWgBTkj8peOYMiySc8NHqYuh7gTe8DMDAJBgNVHRMEAjAA
MAsGA1UdDwQEAwIF4DAdBgNVHSUEFjAUBggrBgEFBQcDAQYIKwYBBQUHAwIwFAYD
VR0RBA0wC4IJSW9URGV2aWNlMA0GCSqGSIb3DQEBCwUAA4IBAQCFsl2xWVoJxkNf
GlUTZlGTHkbpfupLRXnv7Jr0DdPByKk26YNDW7qPvsuIDPtf/ERXwTkuzyeUHMg+
le22qy/tKkOrvKjLlTvOEkH/eyDAE4UQUyG63MeOa3ZCkE/+4JEiqUuqhiIkR8Au
vgQzFcsJlTX+oKv0NBjIagIylduWkAAPlOwj9SMTbMXmkXnQZle/JNluI6aux6Xp
bTCQ4CNxTtp4wqFD1rb5/h1rLQBoH8CUVqtCZVQvVSKc/nT5QfwqOZEQe2snB3cL
EM7O/FZhC/LtjszOys7NvCWPB2CD/i50RGt9fGFRpvQGELS/gxMXa/FnJJnkEK3Z
knSFiewL
-----END CERTIFICATE-----
)EOF";

static const char client_key[] PROGMEM = R"EOF(
-----BEGIN PRIVATE KEY-----
MIIEvgIBADANBgkqhkiG9w0BAQEFAASCBKgwggSkAgEAAoIBAQDJTiWKqsbZCfU1
YbF1CgY2soEszrgCch8LcryfycWRR6nKgptoBUlFfCIQj24CHSKQsPFEHIfLNixg
kXfPd1FzuN8CVKtbxWpTZQmCISOuTmeP8IP+eidk3YROKMb14sZWx8/9FnKHeqg3
ETR6Nxe3j7yiLGdtz6b4ahdf18AXHyQ1Vq9j2OyWVdhUx5/1i3hNNeGHF2R4vaOV
HaEfV4UpTpSRS8pZRIO6o+i9w2/58YUJSVBGFd3+b4K0/iZwZgSwzCcAJk5NThGv
SqZVzvODtn4xgKRWM9x2llMfn6I25s83ARUphHuFEj7O892+dlP4LivhVczlaKvG
WndZasnBAgMBAAECggEAAY29Kc/hvSH32tWuQMHPHJfV+gReLJQoh85G1Qmh0QpL
Xd8ST5wnVa5EpcmymtjyV+kqsoixFl0cnHHw+ZihV/4duAMS58BbPsmnSDIk6Cru
HxD3VtTm5g8U3uTPDM2nU9sC0KBuSbwFTXgANn+DewjkpwCGW5VMQ8tvUt+7mX4S
NpoMXr7zP5LLSJguUfVfOeOR3NuVjSmpxJbc3zACFu5r4pq6rbFTLHWUz1dmbkQI
F55ipxi1tQvsOUcKsJXjgCouW1y0ytA+JTBFWU6b0IwbCVET1f9AffLparccYGGy
5nTXZ1UlU76mtdAi3wpKNy9IsePuxOy4xhzQ1X9wAQKBgQDyny+yhhGUNyyJmQ8J
x0FWMFALYjgAN82a21M9M59A11YZpy6D98YKXPru2Z1/VyOMD5BTkWQdshMXq4oW
n3MGefkNlmFIS7DJJrUDBFk0DDEk5nvgYq6iylWxCCvCZ8WHQBvfahGpdMbJV26w
1dDcJuDSCh6aOnETOYQakVUUAQKBgQDUZ73raiCNFz1fHKkI/6lI2103QpoptDEj
AsuEYbSlptWejQ69MxZ4t0WT6o7iecPTw1upPMhfZ8ifMSz4Dylrkk7diwPMdQ3y
Xs8gYmecJkxGAUoW4o18obRRLNqJrZ/7355ELdHz7xeChHjnWuJDKaa9zirgHdQZ
UGft+SK1wQKBgQCY+uyNSWHOLdfkTuEfjkbRPJdMBjs/T/BRY1eh8GnK7bw5YqO+
zD1QDX6fmhFn2J7uUmXze2pAGydUtnHUf+d8Pu5dteNTPX6T5tcdtuE0Izfdh4K2
YiKxPJk5jehhpSL0e+TXQz8ttRcenwWJdNhD6DO73GudXZoswAhe5CJcAQKBgAm0
G5cPqCEJ+MHJm2dP1rn/W4jUwBo7oysIS6fmlswijQvLQese1F37cXBKfPCXw3x/
JkbbAgRpx16ObpF+j9PcQUIZwbireFGkRy4hkL4vebiRAoN9Ih2ZfZVfyTQU/IQT
OTXAKiFMwPQzfRqEqmWLF5UAQDEw66mkZLsYcJ+BAoGBAOU+/JqkWyF8UTLkeNxC
ZM4DUmu0nFT6O3kp9dlQkFnkbGJakEBfWsTjBDUg/Vkax/Yu+jSk3KNzNEu/v1F6
pMPOJAozmAzBYSlFSC/vQCI91QKdjlWbHA1JHcj9Vf5MatvAutvVhmqDYscfAQYd
HutPW3gIyI9amPAzNq0X3qI0
-----END PRIVATE KEY-----
)EOF";

BearSSL::WiFiClientSecure net;
MQTTClient mqtt;

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