import 'package:ajk_iot_mobile/providers/device_provider.dart';
import 'package:ajk_iot_mobile/widgets/device_feature_widget.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../models/iot_device.dart';

// The StatefulWidget
class IotDeviceWidget extends StatefulWidget {
  final IotDevice device;

  const IotDeviceWidget({super.key, required this.device});

  @override
  IotDeviceWidgetState createState() => IotDeviceWidgetState();
}

// The State class for IotDeviceWidget
class IotDeviceWidgetState extends State<IotDeviceWidget> {
  @override
  Widget build(BuildContext context) {
    return Card(
        child: Consumer<DeviceProvider>(
      builder: (context, deviceProvider, child) => ExpansionTile(
        title: Text(widget
            .device.deviceName), // Accessing the device using widget.device
        initiallyExpanded: true,
        children: widget.device
            .getFeatures()
            .map((f) =>
                DeviceFeatureWidget(feature: f, deviceId: widget.device.id))
            .toList(),
      ),
    ));
  }
}
