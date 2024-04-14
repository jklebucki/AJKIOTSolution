class IotDevice {
  final int id;
  final String ownerId;
  final String deviceType;
  final String deviceName;
  final String deviceFeaturesJson;
  final String deviceScheduleJson;

  IotDevice({
    required this.id,
    required this.ownerId,
    required this.deviceType,
    required this.deviceName,
    required this.deviceFeaturesJson,
    required this.deviceScheduleJson,
  });

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'ownerId': ownerId,
      'deviceType': deviceType,
      'deviceName': deviceName,
      'deviceFeaturesJson': deviceFeaturesJson,
      'deviceScheduleJson': deviceScheduleJson,
    };
  }

  factory IotDevice.fromJson(Map<String, dynamic> json) {
    return IotDevice(
      id: json['id'],
      ownerId: json['ownerId'],
      deviceType: json['deviceType'],
      deviceName: json['deviceName'],
      deviceFeaturesJson: json['deviceFeaturesJson'],
      deviceScheduleJson: json['deviceScheduleJson'],
    );
  }
}