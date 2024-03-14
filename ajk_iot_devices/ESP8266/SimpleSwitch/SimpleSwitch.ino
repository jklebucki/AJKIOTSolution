#include <WebSockets2_Generic.h>
#include <ESP8266WiFi.h>
#include <WiFiClient.h>
#include <NTPClient.h>
#include <WiFiUdp.h>
#include <ArduinoJson.h>
using namespace websockets2_generic;

WebsocketsClient client;
const char* ssid = "---";
const char* password = "---";
const String api_address = "wss://iot.ajk-software.pl/ws";
const long utcOffsetInSeconds = 3600;
JsonDocument device;


WiFiUDP ntpUDP;
NTPClient timeClient(ntpUDP, "pool.ntp.org", utcOffsetInSeconds);
char daysOfTheWeek[7][12] = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
int PIN_P = 0;

void setup() {
  device["name"] = "Door";
  device["id"] = "100002";
  device["features"][0]="Switch";
  device["features"][1]="Knob";
  Serial.begin(115200);
  pinMode(PIN_P, OUTPUT);
  digitalWrite(PIN_P, HIGH);
  WiFi.mode(WIFI_STA);
  WiFi.begin(ssid, password);
  Serial.printf("Connecting to %s", ssid);
  while (WiFi.status() != WL_CONNECTED) {
    Serial.print(".");
    delay(300);
  }
  Serial.println();
  Serial.println("Connected");
  Serial.print("IP Address: ");
  Serial.println(WiFi.localIP());
  client.onMessage(onClientMessage);
  timeClient.begin();
}

void onClientMessage(WebsocketsMessage message) {
  if (message.data() == String(device["name"].as<String>() + ":" + device["id"].as<String>() + ":1:")) {
    Serial.println("Switched");
    timeClient.update();
    Serial.println(timeClient.getFormattedTime());
    digitalWrite(PIN_P, LOW);
    delay(3000);
    String device_status = String(device["name"].as<String>() + ":" + device["id"].as<String>() + ":2:");
    client.send(device_status);
  }
  digitalWrite(PIN_P, HIGH);
  delay(300);
}

void loop() {
  if (!client.available()) {
    if (!client.connect(api_address)) {
      return;
    }
  }
  String device_status = String(device["name"].as<String>() + ":" + device["id"].as<String>() + ":" + digitalRead(PIN_P) + ":");
  client.send(device_status);
  client.poll();
}
