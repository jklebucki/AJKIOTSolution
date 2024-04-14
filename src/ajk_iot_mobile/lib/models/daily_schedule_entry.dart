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

  static DailyScheduleEntry fromJson(Map<String, dynamic> json) => DailyScheduleEntry(
    id: json['id'],
    featureId: json['featureId'],
    dayNumber: json['dayNumber'],
    entryNumber: json['entryNumber'],
    startTime: TimeOnly.fromJson(json['startTime']),
    endTime: TimeOnly.fromJson(json['endTime']),
  );
}