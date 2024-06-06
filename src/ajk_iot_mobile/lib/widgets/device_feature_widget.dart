import 'package:ajk_iot_mobile/pages/schedule_page.dart';
import 'package:ajk_iot_mobile/providers/device_provider.dart';
import 'package:ajk_iot_mobile/widgets/round_button_widget.dart';
import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:provider/provider.dart';
import '../models/device_feature.dart';

class DeviceFeatureWidget extends StatefulWidget {
  final DeviceFeature feature;
  final int deviceId;

  const DeviceFeatureWidget(
      {super.key, required this.feature, required this.deviceId});

  @override
  DeviceFeatureWidgetState createState() => DeviceFeatureWidgetState();
}

class DeviceFeatureWidgetState extends State<DeviceFeatureWidget> {
  Future<void> updateFeature() async {
    var feature = widget.feature;
    var device = Provider.of<DeviceProvider>(context, listen: false)
        .devices
        .firstWhere((element) => element.id == widget.deviceId);
    var features = device.getFeatures();
    var featureIndex =
        features.indexWhere((element) => element.id == widget.feature.id);
    if (featureIndex != -1) {
      features[featureIndex] = feature;
    }
    device.setFeatures(features);
    await Provider.of<DeviceProvider>(context, listen: false)
        .updateDevice(device);
  }

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        buildFeature(),
      ],
    );
  }

  Widget buildFeature() {
    switch (widget.feature.type) {
      case 'Switch':
        return Column(
          children: [
            Switch(
              value: widget.feature.value == 1 ? true : false,
              onChanged: (newValue) {
                setState(() {
                  widget.feature.value = newValue ? 1 : 0;
                  updateFeature();
                });
              },
            ),
            Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                ElevatedButton(
                    child: const Icon(Icons.schedule),
                    onPressed: () {
                      Navigator.push(
                        context,
                        MaterialPageRoute(
                          builder: (context) => SchedulePage(
                              featureId: widget.feature.id,
                              deviceId: widget.deviceId),
                        ),
                      );
                    }),
                SizedBox(height: 5.spMax),
                widget.feature.value == 1
                    ? Text(
                        "Not scheduled",
                        style: TextStyle(
                          fontSize: 12.sp,
                          color: Colors.red,
                          fontWeight: FontWeight.bold,
                        ),
                      )
                    : Text(
                        "Scheduled",
                        style: TextStyle(
                          fontSize: 12.sp,
                          color: Colors.green,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
              ],
            )
          ],
        );
      case 'OpenTimer':
        return Column(
          children: [
            Button3D(
                feature: widget.feature,
                onChange: (val) =>
                    {widget.feature.value = val, updateFeature()}),
            SizedBox(height: 10.spMax),
          ],
        );
      default:
        return const Text('Unknown feature type');
    }
  }
}
