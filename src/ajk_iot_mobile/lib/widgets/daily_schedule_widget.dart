import 'package:flutter/material.dart';
import 'package:ajk_iot_mobile/models/daily_schedule_entry.dart';
import 'package:ajk_iot_mobile/models/time_only.dart';
import 'package:flutter_time_picker_spinner/flutter_time_picker_spinner.dart';

class DailyScheduleWidget extends StatefulWidget {
  final List<DailyScheduleEntry> entries;

  const DailyScheduleWidget({super.key, required this.entries});

  @override
  DailyScheduleWidgetState createState() => DailyScheduleWidgetState();
}

class DailyScheduleWidgetState extends State<DailyScheduleWidget> {
  late List<DailyScheduleEntry> _entries;
  late TimeOfDay _startTime;
  late TimeOfDay _endTime;

  @override
  void initState() {
    super.initState();
    _entries = widget.entries;
    _startTime = TimeOfDay.now();
    _endTime = TimeOfDay.now();
  }

  void _addEntry(DailyScheduleEntry entry) {
    setState(() {
      _entries.add(entry);
    });
  }

  void _showAddEntryDialog() {
    final formKey = GlobalKey<FormState>();

    showDialog(
      context: context,
      builder: (context) {
        return AlertDialog(
          title: const Text('Add New Entry'),
          content: Form(
            key: formKey,
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                const Text('Start Time'),
                TimePickerSpinner(
                  is24HourMode: true,
                  normalTextStyle: const TextStyle(fontSize: 18),
                  highlightedTextStyle:
                      const TextStyle(fontSize: 24, color: Colors.blue),
                  spacing: 50,
                  itemHeight: 40,
                  isForce2Digits: true,
                  onTimeChange: (time) {
                    setState(() {
                      _startTime =
                          TimeOfDay(hour: time.hour, minute: time.minute);
                    });
                  },
                ),
                const SizedBox(height: 20),
                const Text('End Time'),
                TimePickerSpinner(
                  is24HourMode: true,
                  normalTextStyle: const TextStyle(fontSize: 18),
                  highlightedTextStyle:
                      const TextStyle(fontSize: 24, color: Colors.blue),
                  spacing: 50,
                  itemHeight: 40,
                  isForce2Digits: true,
                  onTimeChange: (time) {
                    setState(() {
                      _endTime =
                          TimeOfDay(hour: time.hour, minute: time.minute);
                    });
                  },
                ),
              ],
            ),
          ),
          actions: [
            TextButton(
              onPressed: () {
                Navigator.of(context).pop();
              },
              child: const Text('Cancel'),
            ),
            TextButton(
              onPressed: () {
                if (formKey.currentState?.validate() ?? false) {
                  final newEntry = DailyScheduleEntry(
                    id: _entries.length + 1,
                    featureId: 1, // Replace with actual feature ID if necessary
                    dayNumber: 1, // Replace with actual day number if necessary
                    entryNumber: _entries.length + 1,
                    startTime: TimeOnly.fromHourAndMinute(
                        _startTime.hour, _startTime.minute),
                    endTime: TimeOnly.fromHourAndMinute(
                        _endTime.hour, _endTime.minute),
                  );
                  _addEntry(newEntry);
                  Navigator.of(context).pop();
                }
              },
              child: const Text('Add'),
            ),
          ],
        );
      },
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: ListView.builder(
        itemCount: _entries.length,
        itemBuilder: (context, index) {
          final entry = _entries[index];
          return Card(
            child: ListTile(
              title: Text('Entry ${entry.entryNumber}'),
              subtitle: Text(
                'Start: ${entry.startTime}\nEnd: ${entry.endTime}',
                style: const TextStyle(color: Colors.grey),
              ),
              leading: const Icon(Icons.schedule),
              trailing: IconButton(
                icon: const Icon(Icons.delete),
                onPressed: () {
                  setState(() {
                    _entries.removeAt(index);
                  });
                },
              ),
            ),
          );
        },
      ),
      floatingActionButton: FloatingActionButton(
        onPressed: _showAddEntryDialog,
        child: const Icon(Icons.add),
      ),
    );
  }
}
