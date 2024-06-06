import 'package:flutter/foundation.dart';
import 'package:http/http.dart' as http;

class DeviceConfigService {
  final String url = 'http://192.168.4.1/setWiFi';

  /// Function to set device config
  /// [ssid] - WiFi SSID
  /// [password] - WiFi Password
  /// [deviceid] - Device ID
  /// [mqttserver] - MQTT Server Domain Name or IP Address
  Future<bool> setDevice({
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

    try {
      //POST x-www-form-urlencoded
      final response = await http.post(
        Uri.parse(url),
        body: body,
      );

      // Response handling
      if (response.statusCode == 200) {
        if (kDebugMode) {
          print('WiFi ustawione pomyślnie.');
        }
        return true;
      } else {
        if (kDebugMode) {
          print('Błąd podczas ustawiania WiFi: ${response.statusCode}');
        }
        return false;
      }
    } catch (e) {
      if (kDebugMode) {
        print('Wystąpił wyjątek: $e');
      }
      return false;
    }
  }
}
