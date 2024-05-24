import 'package:ajk_iot_mobile/models/days_of_week.dart';
import 'package:ajk_iot_mobile/pages/schedule_entry_page.dart';
import 'package:flutter/material.dart';

class SchedulePage extends StatelessWidget {
  final int featureId;
  final int deviceId;
  const SchedulePage(
      {super.key, required this.featureId, required this.deviceId});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Weekly Schedule'),
      ),
      body: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Center(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: DaysOfWeek.values.map((day) {
              return Padding(
                padding: const EdgeInsets.symmetric(vertical: 8.0),
                child: ElevatedButton(
                  style: ElevatedButton.styleFrom(
                    padding: const EdgeInsets.symmetric(vertical: 16.0),
                    textStyle: const TextStyle(fontSize: 18),
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(12),
                    ),
                  ),
                  onPressed: () {
                    Navigator.push(
                      context,
                      MaterialPageRoute(
                        builder: (context) => ScheduleEntryPage(
                          day: day,
                          featureId: featureId,
                          deviceId: deviceId,
                        ),
                      ),
                    );
                  },
                  child: Text(day.toString().split('.').last),
                ),
              );
            }).toList(),
          ),
        ),
      ),
    );
  }
}
