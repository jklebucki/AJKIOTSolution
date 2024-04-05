#include "Serializer.h"
#include "WiFiUtils.h"
#include <WebSockets2_Generic.h>
#include <WiFiClient.h>
#include <NTPClient.h>
#include <WiFiUdp.h>
#include <ArduinoJson.h>

using namespace websockets2_generic;

WebsocketsClient client;
const char* ssid = "---";
const char* password = "---";
const String api_address = "wss://iot.ajk-software.pl/ws";
WiFiUDP ntpUDP;
NTPClient timeClient(ntpUDP, "pool.ntp.org", 3600);
JsonDocument device = []{
  DynamicJsonDocument doc(1024);
  doc["name"] = "Door";
  doc["id"] = "100002";
  doc["features"][0] = "Switch";
  doc["features"][1] = "Knob";
  return doc;
}();
const int PIN_P = 0;

void connectToWiFi() {
  WiFi.mode(WIFI_STA);
  WiFi.begin(ssid, password);
  Serial.printf("Connecting to %s", ssid);
  while (WiFi.status() != WL_CONNECTED) {
    Serial.print(".");
    delay(300);
  }
  Serial.println("\nConnected");
  Serial.print("IP Address: ");
  Serial.println(WiFi.localIP());
}

void setup() {
  Serial.begin(115200);
  pinMode(PIN_P, OUTPUT);
  digitalWrite(PIN_P, HIGH);
  connectToWiFi();
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
    digitalWrite(PIN_P, HIGH);
    String device_status = String(device["name"].as<String>() + ":" + device["id"].as<String>() + ":2:");
    client.send(device_status);
  }
}

void loop() {
  static auto lastConnectTry = millis();
  if (!client.available()) {
    if (millis() - lastConnectTry > 5000) { // Reconnect every 5 seconds
      lastConnectTry = millis();
      if (!client.connect(api_address)) {
        Serial.println("Connection to WS failed");
        return;
      }
    }
  } else {
    String device_status = String(device["name"].as<String>() + ":" + device["id"].as<String>() + ":" + digitalRead(PIN_P) + ":");
    client.send(device_status);
    client.poll();
  }
}
