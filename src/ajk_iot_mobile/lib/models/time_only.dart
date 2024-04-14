class TimeOnly {
  final DateTime dateTime;

  TimeOnly(this.dateTime);

  TimeOnly.fromHourAndMinute(int hour, int minute)
      : dateTime = DateTime(1, 1, 1, hour, minute);

  String toJson() => '${dateTime.hour.toString().padLeft(2, '0')}:${dateTime.minute.toString().padLeft(2, '0')}';

  static TimeOnly fromJson(String json) {
    final parts = json.split(':');
    return TimeOnly.fromHourAndMinute(int.parse(parts[0]), int.parse(parts[1]));
  }

  @override
  String toString() => toJson();
}