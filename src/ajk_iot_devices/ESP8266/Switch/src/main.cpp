#include <Arduino.h>
#include <ESP8266WiFi.h>
#include <WebSocketsClient.h>
#include <Hash.h>
#include <PubSubClient.h>

// Dane WiFi
const char* ssid = "YOUR_SSID";
const char* password = "YOUR_PASSWORD";

// Konfiguracja WebSocket
WebSocketsClient webSocket;
WiFiClientSecure wifiClient;
PubSubClient mqttClient(wifiClient);

// Dane broker MQTT
const char* mqtt_server = "YOUR_BROKER_ADDRESS"; // np. broker.hivemq.com
int port = 443; // Port dla WSS
const char* mqtt_topic = "your/topic";

void handleMqttMessage(char* topic, byte* payload, unsigned int length) {
    Serial.print("Wiadomość w topicu [");
    Serial.print(topic);
    Serial.print("]: ");
    for (int i = 0; i < length; i++) {
        Serial.print((char)payload[i]);
    }
    Serial.println();
}

void mqttSubscribe() {
    if (mqttClient.connected()) {
        mqttClient.subscribe(mqtt_topic);
    }
}

void mqttReconnect() {
    while (!mqttClient.connected()) {
        Serial.print("Łączenie z MQTT broker...");
        if (mqttClient.connect("ESP8266Client")) {
            Serial.println("połączono");
            mqttSubscribe(); // Subskrypcja po połączeniu
        } else {
            Serial.print("nie udało się, rc=");
            Serial.print(mqttClient.state());
            Serial.println(" próbuj ponownie za 5 sekund");
            delay(5000);
        }
    }
}

void webSocketEvent(WStype_t type, uint8_t * payload, size_t length) {
    switch(type) {
        case WStype_DISCONNECTED:
            Serial.println("[WSc] Rozłączono!");
            break;
        case WStype_CONNECTED: {
            Serial.println("[WSc] Połączono z URL");
            mqttReconnect(); // Łączenie z MQTT po nawiązaniu połączenia WebSocket
            break;
        }
        case WStype_TEXT:
            Serial.printf("[WSc] Tekst: %s\n", payload);
            mqttClient.loop(); // Zapewnia, że klient MQTT nadal działa i obsługuje wiadomości
            break;
        case WStype_BIN:
            Serial.printf("[WSc] Binarnie: %u bajtów\n", length);
            break;
    }
}

void setup() {
    Serial.begin(115200);
    WiFi.begin(ssid, password);
    while (WiFi.status() != WL_CONNECTED) {
        delay(100);
        Serial.print(".");
    }
    Serial.println("\nWiFi połączone");
    Serial.println("IP: ");
    Serial.println(WiFi.localIP());

    // Konfiguracja WebSocket
    webSocket.beginSSL(mqtt_server, port, "/", "mqtt"); // Ustawienia dla WSS
    webSocket.onEvent(webSocketEvent);
    webSocket.setReconnectInterval(5000);

    // Konfiguracja MQTT
    mqttClient.setServer(mqtt_server, port);
    mqttClient.setCallback(handleMqttMessage);
}

void loop() {
    if (!mqttClient.connected()) {
        mqttReconnect();
    }
    webSocket.loop();
    mqttClient.loop();
}
