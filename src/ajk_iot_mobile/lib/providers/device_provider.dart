import 'dart:convert';
import 'package:ajk_iot_mobile/models/iot_device.dart';
import 'package:ajk_iot_mobile/models/api_response.dart';
import 'package:flutter/foundation.dart';
import 'package:http/http.dart' as http;
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

class DeviceProvider with ChangeNotifier {
  final FlutterSecureStorage _storage = const FlutterSecureStorage();
  List<IotDevice> _devices = []; // Ensuring _devices is never null.

  List<IotDevice> get devices => _devices;

  Future<void> getDevices() async {
    final baseUrl = await _storage.read(key: 'apiUrl') ?? ''; // Providing a default empty string if null
    final username = await _storage.read(key: 'email') ?? ''; // Providing a default empty string if null
    final token = await _storage.read(key: 'jwt') ?? ''; // Providing a default empty string if null

    final response = await http.get(
      Uri.parse('$baseUrl/api/Devices/$username'),
      headers: {'Authorization': 'Bearer $token'},
    );

    if (response.statusCode == 200) {
      ApiResponse<List<IotDevice>> apiResponse = ApiResponse.fromJson(
        jsonDecode(response.body), 
        (data) => List<IotDevice>.from(data.map((item) => IotDevice.fromJson(item)))
      );
      if (apiResponse.isSuccess && apiResponse.data != null) {
        _devices = apiResponse.data!;
        notifyListeners(); // Notify listeners about data change
      }
    } else {
      throw Exception('Failed to load devices: ${response.body}');
    }
  }

  Future<void> updateDevice(IotDevice updatedDevice) async {
    final baseUrl = await _storage.read(key: 'apiUrl') ?? ''; // Providing a default empty string if null
    final token = await _storage.read(key: 'jwt') ?? ''; // Providing a default empty string if null

    final response = await http.patch(
      Uri.parse('$baseUrl/api/Devices/${updatedDevice.id}'), // Assuming each device has an ID
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer $token'
      },
      body: jsonEncode(updatedDevice.toJson()),
    );

    if (response.statusCode == 200) {
      ApiResponse<IotDevice> apiResponse = ApiResponse.fromJson(
        jsonDecode(response.body), 
        (data) => IotDevice.fromJson(data)
      );
      if (apiResponse.isSuccess && apiResponse.data != null) {
        int index = _devices.indexWhere((device) => device.id == updatedDevice.id);
        if (index != -1) {
          _devices[index] = apiResponse.data!;  // Update the specific device in the list
          notifyListeners();  // Notify listeners about the update
          if (kDebugMode) {
            print('Update successful');
          }
        }
      } else {
        if (kDebugMode) {
          print('Failed to update device: ${apiResponse.errors.join(", ")}');
        }
      }
    } else {
      if (kDebugMode) {
        print('Failed to update device: ${response.body}');
      }
    }
  }
}
