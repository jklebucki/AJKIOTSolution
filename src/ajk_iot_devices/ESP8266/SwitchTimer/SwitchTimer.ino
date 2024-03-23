#include <ESP8266WiFi.h>
#include <WiFiClient.h>
#include <EEPROM.h>
#include <ESP8266WebServer.h>


ESP8266WebServer server(80);
const int configFlagAddr = 96; 

void setup() {
  Serial.begin(115200);
  EEPROM.begin(512);

  if (checkConfigFlag()) {
    String ssid = readEEPROM(0, 32);
    String password = readEEPROM(32, 64);
    if (!connectToWiFi(ssid, password)) {
      setupAccessPoint();
    }
  } else {
    setupAccessPoint();
  }

  // Konfiguracja WebSocket, NTP, itd.
}

void loop() {
  server.handleClient();
  // Reszta kodu loop
}

void setupAccessPoint() {
  WiFi.softAP("ESP_Config", NULL);
  Serial.println("Access Point Started");
  server.on("/", HTTP_GET, []() {
    server.send(200, "text/html", /* HTML form */);
  });
  server.on("/save", HTTP_POST, []() {
    String ssid = server.arg("ssid");
    String pass = server.arg("pass");
    saveEEPROM(0, ssid);
    saveEEPROM(32, pass);
    // Ustawienie flagi konfiguracji na 1
    EEPROM.write(configFlagAddr, 1);
    EEPROM.commit();
    server.send(200, "text/html", "<h1>Restarting...</h1>");
    delay(1000);
    ESP.restart();
  });
  server.begin();
}

bool connectToWiFi(const String &ssid, const String &password) {
  WiFi.begin(ssid.c_str(), password.c_str());
  for (int i = 0; i < 20; ++i) { // Czekaj maksymalnie 10 sekund
    if (WiFi.status() == WL_CONNECTED) {
      Serial.println("WiFi connected");
      return true;
    }
    delay(500);
  }
  Serial.println("Failed to connect to WiFi. Entering AP mode.");
  return false;
}

bool checkConfigFlag() {
