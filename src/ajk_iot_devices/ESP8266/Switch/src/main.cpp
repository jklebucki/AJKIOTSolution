
#include <ESP8266WiFi.h>
#include <ESP8266Ping.h>
#include <ESP8266MQTTClient.h>
#include <ArduinoJson.h>

// WiFi credentials
const char *ssid = "Orange_Swiatlowod_B5AC";
const char *password = "cdHqhMotvgMSJ9L4tD";

// MQTT Server settings
const char *mqtt_server = "ws://ajkdesktop.local:5217/mqtt";
const char *mqtt_client_id = "device-0001";
String address = "ajkdesktop.local";

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

void deserializeIotDevice(String json)
{
  StaticJsonDocument<1024> doc;
  deserializeJson(doc, json);

  int id = doc["Id"];
  const char *ownerId = doc["OwnerId"];
  const char *deviceType = doc["DeviceType"];
  const char *deviceName = doc["DeviceName"];
  const char *deviceFeaturesJson = doc["DeviceFeaturesJson"];
  const char *deviceScheduleJson = doc["DeviceScheduleJson"];

  // You can now use these values in your application
}

String getAddress(String addressData)
{
  int colonIndex = addressData.indexOf(':'); // Znajduje indeks pierwszego wystąpienia dwukropka

  if (colonIndex != -1)
  {                                               // Sprawdza, czy znaleziono dwukropek
    return addressData.substring(colonIndex + 1); // Pobiera substring zaczynający się po dwukropku // Wypisuje wynik
  }
  else
  {
    return "";
  }
}

void callback(char *topic, byte *payload, unsigned int length)
{
  payload[length] = '\0'; // Ensure null-terminated string
  String message = String((char *)payload);
  deserializeIotDevice(message);
}

void setup()
{
  Serial.begin(115200);
  setup_wifi();
  configTime(3 * 3600, 0, "pool.ntp.org", "time.nist.gov");

  // mqtt.onSecure([](WiFiClientSecure *client, String host)               {
  //     Serial.printf("Secure: %s\r\n", host.c_str());
  //     return client->setFingerprint(fingerprint.c_str()); });

  // topic, data, data is continuing
  mqtt.onData([](String topic, String data, bool cont)
              {
    Serial.printf("Data received, topic: %s, data: %s\r\n", topic.c_str(), data.c_str());
    if (data.indexOf("Stop pinging") != -1)
    pinging = false;
    else if (data.indexOf("Start pinging") != -1)
    pinging = true;

    if (data.indexOf("Address:") != -1)
    address = getAddress(data);

    mqtt.unSubscribe("/qos0"); });

  mqtt.onSubscribe([](int sub_id)
                   { 
    Serial.printf("Subscribe topic id: %d ok\r\n", sub_id); 
    mqtt.publish("test/topic", "Test message", 0, 0); });
  mqtt.onConnect([]()
                 {
    Serial.printf("MQTT: Connected\r\n");
    Serial.printf("Subscribe id: %d\r\n", mqtt.subscribe("test1/topic", 0));
    mqtt.subscribe("test/topic", 1);
    mqtt.subscribe("test2/topic", 2); });

  mqtt.begin("ws://172.16.90.151:5217/mqtt", mqtt_client_id, {.lwtTopic = "test/topic", .lwtMsg = "Offline", .lwtQos = 0, .lwtRetain = 0});
  // mqtt.begin("ws://test.mosquitto.org:8443", {.lwtTopic = "hello", .lwtMsg = "offline", .lwtQos = 0, .lwtRetain = 0});
  // mqtt.begin("ws://mosquito.org:8443", "user", "pass");
  // mqtt.begin("ws://mosquito.org:8443", "clientId", "user", "pass");
}

void loop()
{
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
}