import 'package:ajk_iot_mobile/providers/device_provider.dart';
import 'package:ajk_iot_mobile/widgets/device_feature_widget.dart';
import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
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
    return Center(
      child: SizedBox(
        width: 0.9.sw,
        child: Card(
            elevation: 6,
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(10.spMax),
            ),
            child: Consumer<DeviceProvider>(
              builder: (context, deviceProvider, child) => Padding(
                padding: EdgeInsets.all(6.spMax),
                child: Column(
                  children: [
                    Text(widget.device.deviceName,
                        style: TextStyle(
                          fontSize: 22.spMax,
                          fontWeight: FontWeight.bold,
                        )),
                    ...widget.device.getFeatures().map((f) =>
                        DeviceFeatureWidget(
                            feature: f, deviceId: widget.device.id))
                  ],
                ),
              ),
            )),
      ),
    );
  }
}
