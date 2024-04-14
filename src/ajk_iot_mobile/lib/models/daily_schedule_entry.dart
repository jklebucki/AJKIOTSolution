class DailyScheduleEntry {
  final int id;
  final int featureId;
  final int dayNumber;
  final int entryNumber;
  final String startTime;
  final String endTime;

  DailyScheduleEntry({
    required this.id,
    required this.featureId,
    required this.dayNumber,
    required this.entryNumber,
    required this.startTime,
    required this.endTime,
  });

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'featureId': featureId,
      'dayNumber': dayNumber,
      'entryNumber': entryNumber,
      'startTime': startTime,
      'endTime': endTime,
    };
  }

  factory DailyScheduleEntry.fromJson(Map<String, dynamic> json) {
    return DailyScheduleEntry(
      id: json['id'],
      featureId: json['featureId'],
      dayNumber: json['dayNumber'],
      entryNumber: json['entryNumber'],
      startTime: json['startTime'],
      endTime: json['endTime'],
    );
  }
}