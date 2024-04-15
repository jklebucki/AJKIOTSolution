import 'package:ajk_iot_mobile/widgets/round_button_widget.dart';
import 'package:flutter/material.dart';
import '../models/device_feature.dart';

class DeviceFeatureWidget extends StatefulWidget {
  final DeviceFeature feature;

  const DeviceFeatureWidget({super.key, required this.feature});

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
          value: value == 1 ? true : false,
          onChanged: (newValue) {
            setState(() {
              value = newValue ? 1 : 0;
              // Tu trzeba dodać logikę aktualizacji stanu za pomocą providera
            });
          },
        );
      case 'OpenTimer':
        return Button3D(feature: widget.feature);
      default:
        return const Text('Unknown feature type');
    }
  }
}
