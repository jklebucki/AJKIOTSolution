import 'package:ajk_iot_mobile/models/iot_device.dart';

class UpdateDeviceRequest {
  final IotDevice device;

  UpdateDeviceRequest({required this.device});

  Map<String, dynamic> toJson() {
    return {
      'device': device.toJson(),
    };
  }

  static UpdateDeviceRequest fromJson(Map<String, dynamic> json) {
    return UpdateDeviceRequest(
      device: IotDevice.fromJson(json['device']),
    );
  }
}
