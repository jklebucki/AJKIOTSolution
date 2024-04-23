class DeviceFeature {
  int id;
  String type;
  String name;
  int value;
  int minValue;
  int maxValue;
  int numberOfSteps;

  DeviceFeature({
    required this.id,
    required this.type,
    required this.name,
    required this.value,
    required this.minValue,
    required this.maxValue,
    required this.numberOfSteps,
  });

  Map<String, dynamic> toJson() => {
    'Id': id,
    'Type': type,
    'Name': name,
    'Value': value,
    'MinValue': minValue,
    'MaxValue': maxValue,
    'NumberOfSteps': numberOfSteps,
  };

  static DeviceFeature fromJson(Map<String, dynamic> json) => DeviceFeature(
    id: json['Id'],
    type: json['Type'],
    name: json['Name'],
    value: json['Value'],
    minValue: json['MinValue'],
    maxValue: json['MaxValue'],
    numberOfSteps: json['NumberOfSteps'],
  );
}