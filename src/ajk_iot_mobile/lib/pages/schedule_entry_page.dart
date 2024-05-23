import 'package:ajk_iot_mobile/models/days_of_week.dart';
import 'package:ajk_iot_mobile/models/daily_schedule_entry.dart';
import 'package:ajk_iot_mobile/providers/device_provider.dart';
import 'package:ajk_iot_mobile/widgets/daily_schedule_widget.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

class ScheduleEntryPage extends StatefulWidget {
  final DaysOfWeek day;
  final int featureId;
  final int deviceId;
  const ScheduleEntryPage(
      {super.key,
      required this.day,
      required this.featureId,
      required this.deviceId});

  @override
  ScheduleEntryPageState createState() => ScheduleEntryPageState();
}

class ScheduleEntryPageState extends State<ScheduleEntryPage> {
  List<DailyScheduleEntry> entries = [];
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      setState(() {
        entries = Provider.of<DeviceProvider>(context, listen: false)
            .devices
            .firstWhere((element) => element.id == widget.deviceId)
            .getSchedule()
            .where((element) => element.dayNumber == (widget.day.index + 1))
            .toList();
      });
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text('Schedule for ${widget.day.toString().split('.').last}'),
      ),
      body: entries.isEmpty
          ? const Center(child: CircularProgressIndicator())
          : DailyScheduleWidget(entries: entries),
    );
  }
}
