import 'package:ajk_iot_mobile/providers/device_provider.dart';
import 'package:ajk_iot_mobile/services/signalr_service.dart';
import 'package:ajk_iot_mobile/widgets/iot_device_widget.dart';
import 'package:flutter/material.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:provider/provider.dart';
import '../providers/auth_provider.dart';

class HomePage extends StatefulWidget {
  const HomePage({super.key});

  @override
  State<HomePage> createState() => _HomePageState();
}

class _HomePageState extends State<HomePage> {
  late SignalRService _signalRService;
  final FlutterSecureStorage _storage = const FlutterSecureStorage();
  String _apiUrl = '';
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      var deviceProvider = Provider.of<DeviceProvider>(context, listen: false);
      deviceProvider.getDevices();
      _initializeSignalR();
    });
  }

  Future<void> _initializeSignalR() async {
    _apiUrl = await _storage.read(key: 'apiUrl') ?? '';
    _signalRService = SignalRService(
        apiUrl: _apiUrl,
        onUpdateDevice: (device) {
          Provider.of<DeviceProvider>(context, listen: false)
              .updateDeviceFromMessage(device);
        });
    _signalRService.startConnection();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Strona Główna'),
      ),
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: <Widget>[
            Consumer<AuthProvider>(
              builder: (context, authProvider, child) => Text(
                'Witaj, ${authProvider.userInfo['username'] ?? 'Gościu'}!',
                style: const TextStyle(fontSize: 20),
              ),
            ),
            Expanded(
              child: Consumer<DeviceProvider>(
                builder: (context, deviceProvider, child) => ListView.builder(
                  itemCount: deviceProvider.devices.length,
                  itemBuilder: (context, index) {
                    var device = deviceProvider.devices[index];
                    return IotDeviceWidget(device: device);
                  },
                ),
              ),
            ),
            const SizedBox(height: 20),
            Consumer<AuthProvider>(
              builder: (context, authProvider, child) => ElevatedButton(
                onPressed: () {
                  authProvider.logout();
                  Navigator.of(context).pushReplacementNamed('/login');
                },
                child: const Text('Wyloguj'),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
