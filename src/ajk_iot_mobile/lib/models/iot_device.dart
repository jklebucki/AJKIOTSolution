import 'dart:convert';

import 'package:ajk_iot_mobile/models/daily_schedule_entry.dart';
import 'package:ajk_iot_mobile/models/device_feature.dart';

class IotDevice {
  int id;
  String ownerId;
  String deviceType;
  String deviceName;
  String deviceFeaturesJson;
  String deviceScheduleJson;

  IotDevice({
    required this.id,
    required this.ownerId,
    required this.deviceType,
    required this.deviceName,
    required this.deviceFeaturesJson,
    required this.deviceScheduleJson,
  });

  bool isScheduleAvailable() {
    switch (deviceType) {
      case 'Switch':
        return true;
      case 'OpenTimer':
        return false;
      default:
        return false;
    }
  }

  void setFeatures(List<DeviceFeature> deviceFeatures) {
    deviceFeaturesJson = jsonEncode(deviceFeatures.map((f) => f.toJson()).toList());
  }

  List<DeviceFeature> getFeatures() {
    return jsonDecode(deviceFeaturesJson).map<DeviceFeature>((f) => DeviceFeature.fromJson(f)).toList();
  }

  void setSchedule(List<DailyScheduleEntry> dailyScheduleEntries) {
    if (isScheduleAvailable()) {
      deviceScheduleJson = jsonEncode(dailyScheduleEntries.map((e) => e.toJson()).toList());
    }
  }

  List<DailyScheduleEntry> getSchedule() {
    if (isScheduleAvailable() && deviceScheduleJson.isNotEmpty) {
      return jsonDecode(deviceScheduleJson).map<DailyScheduleEntry>((e) => DailyScheduleEntry.fromJson(e)).toList();
    } else {
      return [];
    }
  }

  Map<String, dynamic> toJson() => {
    'id': id,
    'ownerId': ownerId,
    'deviceType': deviceType,
    'deviceName': deviceName,
    'deviceFeaturesJson': deviceFeaturesJson,
    'deviceScheduleJson': deviceScheduleJson,
  };

  static IotDevice fromJson(Map<String, dynamic> json) => IotDevice(
    id: json['id'],
    ownerId: json['ownerId'],
    deviceType: json['deviceType'],
    deviceName: json['deviceName'],
    deviceFeaturesJson: json['deviceFeaturesJson'],
    deviceScheduleJson: json['deviceScheduleJson'],
  );
}
