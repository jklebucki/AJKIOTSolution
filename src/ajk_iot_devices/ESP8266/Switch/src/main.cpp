
#include <ESP8266WiFi.h>
#include <PubSubClient.h>
#include <WiFiClientSecure.h>
#include <ArduinoJson.h>

// WiFi credentials
const char *ssid = "Orange_Swiatlowod_B5AC";
const char *password = "cdHqhMotvgMSJ9L4tD";

// MQTT Server settings
const char *mqtt_server = "wss://localhost:7253/mqtt";
const int mqtt_port = 7253;

WiFiClientSecure espClient;
PubSubClient client(espClient);

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

void callback(char *topic, byte *payload, unsigned int length)
{
  payload[length] = '\0'; // Ensure null-terminated string
  String message = String((char *)payload);
  deserializeIotDevice(message);
}

void reconnect()
{
  while (!client.connected())
  {
    Serial.print("Attempting MQTT connection...");
    if (client.connect("ESP8266Client"))
    {
      Serial.println("connected");
      client.subscribe("iotdevice/DeviceId");
    }
    else
    {
      Serial.print("failed, rc=");
      Serial.print(client.state());
      Serial.println(" try again in 5 seconds");
      // Wait 5 seconds before retrying
      delay(5000);
    }
  }
}

void setup()
{
  Serial.begin(57600);
  setup_wifi();
  client.setServer(mqtt_server, mqtt_port);
  client.setCallback(callback);
}

void loop()
{
  if (!client.connected())
  {
    reconnect();
  }
  client.loop();
}