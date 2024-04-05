// Serializer.cpp
#include "Serializer.h"
#include <ESP8266WiFi.h>

void Serializer::AddBlock(int int1, int int2, double double1, double double2) {
    blocks.push_back(DataBlock(int1, int2, double1, double2));
}

void Serializer::RemoveBlock(size_t index) {
    if (index < blocks.size()) {
        blocks.erase(blocks.begin() + index);
    }
}

std::string Serializer::Serialize() const {
    std::ostringstream oss;
    for (const auto& block : blocks) {
        oss << block.int1 << ":" << block.int2 << ":" << block.double1 << ":" << block.double2 << "|";
    }
    std::string result = oss.str();
    if (!result.empty()) result.pop_back(); // Usuń ostatni znak '|'
    return result;
}

void Serializer::Deserialize(const std::string& data) {
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

void Serializer::Print() const {
    for (const auto& block : blocks) {
        Serial.print("Block: ");
        Serial.print(block.int1);
        Serial.print(", ");
        Serial.print(block.int2);
        Serial.print(", ");
        Serial.print(block.double1);
        Serial.print(", ");
        Serial.println(block.double2);
    }
}
