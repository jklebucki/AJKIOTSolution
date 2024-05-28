import 'package:http/http.dart' as http;

class DeviceConfigService {
  final String url = 'http://192.168.4.1/setWiFi';

  /// Fuction to set device config
  /// [ssid] - WiFi SSID
  /// [password] - WiFi Password
  /// [deviceid] - Device ID
  /// [mqttserver] - MQTT Server Domain Name or IP Address
  Future<void> setDevice({
    required String ssid,
    required String password,
    required String deviceid,
    required String mqttserver,
  }) async {
    final Map<String, String> body = {
      'ssid': ssid,
      'password': password,
      'deviceid': deviceid,
      'mqttserver': mqttserver,
    };

    //POST x-www-form-urlencoded
    final response = await http.post(
      Uri.parse(url),
      body: body,
    );

    // Response handling
    if (response.statusCode == 200) {
      // Sukces
      print('WiFi ustawione pomyślnie.');
    } else {
      // Błąd
      print('Błąd podczas ustawiania WiFi: ${response.statusCode}');
    }
  }
}
