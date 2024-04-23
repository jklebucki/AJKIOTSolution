import 'package:ajk_iot_mobile/providers/device_provider.dart';
import 'package:ajk_iot_mobile/widgets/round_button_widget.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../models/device_feature.dart';

class DeviceFeatureWidget extends StatefulWidget {
  final DeviceFeature feature;
  final int deviceId;

  const DeviceFeatureWidget({super.key, required this.feature, required this.deviceId});

  @override
  DeviceFeatureWidgetState createState() => DeviceFeatureWidgetState();
}

class DeviceFeatureWidgetState extends State<DeviceFeatureWidget> {
  late dynamic value;
  late dynamic featureType;

  @override
  void initState() {
    super.initState();
    value = widget.feature.value;
    featureType = widget.feature.type;
  }

  Future<void> updateFeature() async {
    var feature = widget.feature;
    var device = Provider.of<DeviceProvider>(context, listen: false).devices.firstWhere((element) => element.id == widget.deviceId);
    var features = device.getFeatures();
    var featureIndex = features.indexWhere((element) => element.id == widget.feature.id);
    if (featureIndex != -1) {
      features[featureIndex] = feature;
    }
    device.setFeatures(features);
    await Provider.of<DeviceProvider>(context, listen: false).updateDevice(device);
  }

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        buildValue(),
      ],
    );
  }

  Widget buildValue() {
    switch (featureType) {
      case 'Switch':
        return Switch(
          value: widget.feature.value == 1 ? true : false,
          onChanged: (newValue) {
            setState(() {
              widget.feature.value = newValue ? 1 : 0;
              updateFeature();
            });
          },
        );
      case 'OpenTimer':
        return Button3D(feature: widget.feature, onChange: (val) => {
          widget.feature.value = val,
          updateFeature()
        });
      default:
        return const Text('Unknown feature type');
    }
  }
}
