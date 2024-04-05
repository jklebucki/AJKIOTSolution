// WiFiUtils.h
#ifndef WIFIUTILS_H
#define WIFIUTILS_H

#include <ESP8266WiFi.h>

void connectToWiFi(const char* ssid, const char* password) {
    WiFi.mode(WIFI_STA);
    WiFi.begin(ssid, password);
    Serial.printf("Connecting to %s ", ssid);
    while (WiFi.status() != WL_CONNECTED) {
        delay(500);
        Serial.print(".");
    }
    Serial.println(" connected");
    Serial.print("IP Address: ");
    Serial.println(WiFi.localIP());
}

#endif
