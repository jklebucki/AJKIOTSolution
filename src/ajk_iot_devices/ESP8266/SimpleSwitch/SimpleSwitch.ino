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

class DataBlock {
public:
    int int1, int2;
    double double1, double2;

    DataBlock(int i1, int i2, double d1, double d2) : int1(i1), int2(i2), double1(d1), double2(d2) {}
};

class Serializer {
private:
    std::vector<DataBlock> blocks;

public:
    void AddBlock(int int1, int int2, double double1, double double2) {
        blocks.push_back(DataBlock(int1, int2, double1, double2));
    }

    void RemoveBlock(size_t index) {
        if (index < blocks.size()) {
            blocks.erase(blocks.begin() + index);
        }
    }

    std::string Serialize() const {
        std::ostringstream oss;
        for (const auto& block : blocks) {
            oss << block.int1 << ":" << block.int2 << ":" << block.double1 << ":" << block.double2 << "|";
        }
        std::string result = oss.str();
        if (!result.empty()) result.pop_back(); // Usuń ostatni znak '|'
        return result;
    }

    void Deserialize(const std::string& data) {
        std::istringstream iss(data);
        std::string segment;
        blocks.clear(); // Usuń bieżące bloki przed deserializacją

        while (std::getline(iss, segment, '|')) {
            std::istringstream segmentStream(segment);
            std::string element;
            std::vector<std::string> elements;

            while (std::getline(segmentStream, element, ':')) {
                elements.push_back(element);
            }

            if (elements.size() == 4) { // Upewnij się, że segment zawiera 4 elementy
                int int1 = std::stoi(elements[0]);
                int int2 = std::stoi(elements[1]);
                double double1 = std::stod(elements[2]);
                double double2 = std::stod(elements[3]);
                AddBlock(int1, int2, double1, double2);
            }
        }
    }

    void Print() const {
        for (const auto& block : blocks) {
            std::cout << "Block: " << block.int1 << ", " << block.int2 << ", " << block.double1 << ", " << block.double2 << std::endl;
        }
    }
};


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
