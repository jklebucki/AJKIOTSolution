import 'package:ajk_iot_mobile/widgets/device_feature_widget.dart';
import 'package:flutter/material.dart';
import '../models/iot_device.dart';

class IotDeviceWidget extends StatelessWidget {
  final IotDevice device;

  const IotDeviceWidget({super.key, required this.device});
  

  @override
  Widget build(BuildContext context) {
    return Card(
      child: ExpansionTile(
        title: Text(device.deviceName),
        initiallyExpanded: true,
        children: device.getFeatures().map((f) => DeviceFeatureWidget(feature: f)).toList(),
      ),
    );
  }
}
