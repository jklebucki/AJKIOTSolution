#include "IotDevice.h"

bool IotDevice::decodeFeatureJson(const char *json, DeviceFeature &feature)
{
    StaticJsonDocument<256> doc;

    DeserializationError error = deserializeJson(doc, json);
    if (error)
    {
        // Błąd deserializacji JSON
        return false;
    }

    JsonObject jsonObject = doc.as<JsonObject>();
    parseJson(jsonObject, feature);

    return true;
}

String IotDevice::encodeFeatureJson(const DeviceFeature &feature)
{
    StaticJsonDocument<256> doc;
    JsonObject jsonObject = doc.to<JsonObject>();

    fillJsonObject(jsonObject, feature);

    String jsonString;
    serializeJson(doc, jsonString); // Serializacja dokumentu JSON do obiektu String

    return jsonString;
}

void IotDevice::parseJson(JsonObject &jsonObject, DeviceFeature &feature)
{
    feature.Id = jsonObject["Id"];
    feature.Type = String(jsonObject["Type"].as<const char *>()); // Convert to String
    feature.Name = String(jsonObject["Name"].as<const char *>()); // Convert to String
    feature.Value = jsonObject["Value"];
    feature.MinValue = jsonObject["MinValue"];
    feature.MaxValue = jsonObject["MaxValue"];
    feature.NumberOfSteps = jsonObject["NumberOfSteps"];
}

void IotDevice::fillJsonObject(JsonObject &jsonObject, const DeviceFeature &feature)
{
    jsonObject["Id"] = feature.Id;
    jsonObject["Type"] = feature.Type.c_str(); // Convert to const char*
    jsonObject["Name"] = feature.Name.c_str(); // Convert to const char*
    jsonObject["Value"] = feature.Value;
    jsonObject["MinValue"] = feature.MinValue;
    jsonObject["MaxValue"] = feature.MaxValue;
    jsonObject["NumberOfSteps"] = feature.NumberOfSteps;
}
