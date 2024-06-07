#ifndef IOT_DEVICE_H
#define IOT_DEVICE_H

#include <ArduinoJson.h>

class IotDevice
{
public:
    struct DeviceFeature
    {
        int Id;
        String Type;
        String Name;
        int Value;
        int MinValue;
        int MaxValue;
        int NumberOfSteps;
    };

    bool decodeFeatureJson(const char *json, DeviceFeature &feature);
    String encodeFeatureJson(const DeviceFeature &feature);

private:
    void parseJson(JsonObject &jsonObject, DeviceFeature &feature);
    void fillJsonObject(JsonObject &jsonObject, const DeviceFeature &feature);
};

#endif // IOT_DEVICE_H
