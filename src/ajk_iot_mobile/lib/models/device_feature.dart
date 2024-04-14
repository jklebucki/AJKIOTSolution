class DeviceFeature {
  final int id;
  final String type;
  final String name;
  final int value;
  final int minValue;
  final int maxValue;
  final int numberOfSteps;

  DeviceFeature({
    required this.id,
    required this.type,
    required this.name,
    required this.value,
    required this.minValue,
    required this.maxValue,
    required this.numberOfSteps,
  });

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'type': type,
      'name': name,
      'value': value,
      'minValue': minValue,
      'maxValue': maxValue,
      'numberOfSteps': numberOfSteps,
    };
  }

  factory DeviceFeature.fromJson(Map<String, dynamic> json) {
    return DeviceFeature(
      id: json['id'],
      type: json['type'],
      name: json['name'],
      value: json['value'],
      minValue: json['minValue'],
      maxValue: json['maxValue'],
      numberOfSteps: json['numberOfSteps'],
    );
  }
}