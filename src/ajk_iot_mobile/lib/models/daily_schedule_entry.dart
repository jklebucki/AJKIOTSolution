import 'package:ajk_iot_mobile/models/time_only.dart';

class DailyScheduleEntry {
  int id;
  int featureId;
  int dayNumber;
  int entryNumber;
  TimeOnly startTime;
  TimeOnly endTime;

  DailyScheduleEntry({
    required this.id,
    required this.featureId,
    required this.dayNumber,
    required this.entryNumber,
    required this.startTime,
    required this.endTime,
  });

  Map<String, dynamic> toJson() => {
        'id': id,
        'featureId': featureId,
        'dayNumber': dayNumber,
        'entryNumber': entryNumber,
        'startTime': startTime.toJson(),
        'endTime': endTime.toJson(),
      };

  static DailyScheduleEntry fromJson(Map<String, dynamic> json) =>
      DailyScheduleEntry(
        id: json['Id'],
        featureId: json['FeatureId'],
        dayNumber: json['DayNumber'],
        entryNumber: json['EntryNumber'],
        startTime: TimeOnly.fromJson(json['StartTime']),
        endTime: TimeOnly.fromJson(json['EndTime']),
      );
}
