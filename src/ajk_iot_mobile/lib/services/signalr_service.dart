import 'package:ajk_iot_mobile/models/iot_device.dart';
import 'package:flutter/foundation.dart';
import 'package:signalr_netcore/signalr_client.dart';

class SignalRService {
  Function(IotDevice)? onUpdateDevice;
  late HubConnection _hubConnection;
  String apiUrl;
  String userEmail;


  SignalRService({required this.apiUrl, required this.userEmail, this.onUpdateDevice}) {
    _hubConnection = HubConnectionBuilder().withUrl('$apiUrl/notificationHub?clientId=$userEmail').build();
    _hubConnection.onclose(_connectionClosed);
    _hubConnection.on("DeviceUpdated", _handleAClientProvidedFunction);
  }

  void _handleAClientProvidedFunction(List<Object?>? arguments) {
    var device = IotDevice.fromJson(arguments![0] as Map<String, dynamic>);
    onUpdateDevice?.call(device);
    if (kDebugMode) {
      print('Received message: ${arguments[0]}');
    }
  }

  void _connectionClosed({Exception? error}) {
    if (kDebugMode) {
      print('Connection closed: $error');
    }
  }

  Future<void> startConnection() async {
    try {
      await _hubConnection.start();
      if (kDebugMode) {
        print('SignalR Connection started');
      }
    } catch (e) {
      if (kDebugMode) {
        print('Could not connect: $e');
      }
    }
  }

  Future<void> stopConnection() async {
    await _hubConnection.stop();
    if (kDebugMode) {
      print('SignalR Connection stopped');
    }
  }
}
