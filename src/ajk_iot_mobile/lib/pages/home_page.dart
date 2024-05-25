import 'package:ajk_iot_mobile/providers/device_provider.dart';
import 'package:ajk_iot_mobile/services/signalr_service.dart';
import 'package:ajk_iot_mobile/widgets/iot_device_widget.dart';
import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
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
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      var deviceProvider = Provider.of<DeviceProvider>(context, listen: false);
      deviceProvider.getDevices();
      _initializeSignalR();
    });
  }

  void _refreshWidgets() {
    setState(() {
      Provider.of<DeviceProvider>(context, listen: false).getDevices();
      _initializeSignalR();
    });
  }

  Future<void> _initializeSignalR() async {
    var apiUrl = await _storage.read(key: 'apiUrl') ?? '';
    var userEmail = await _storage.read(key: 'email') ?? '';
    _signalRService = SignalRService(
        apiUrl: apiUrl,
        userEmail: userEmail,
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
        title: const Text('AJK IoT Mobile'),
      ),
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: <Widget>[
            SizedBox(
              width: 0.9.sw,
              child: Card(
                color: Colors.blue,
                elevation: 6,
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(5.spMax),
                ),
                child: Text(
                  'Your devices',
                  textAlign: TextAlign.center,
                  style: TextStyle(
                      fontSize: 20.spMax, fontWeight: FontWeight.bold),
                ),
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
            SizedBox(height: 10.spMax),
            Row(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Consumer<AuthProvider>(
                  builder: (context, authProvider, child) => ElevatedButton(
                    onPressed: _refreshWidgets,
                    child: const Text('Refresh'),
                  ),
                ),
                SizedBox(width: 10.spMax),
                Consumer<AuthProvider>(
                  builder: (context, authProvider, child) => ElevatedButton(
                    onPressed: () {
                      authProvider.logout();
                      Navigator.of(context).pushReplacementNamed('/login');
                    },
                    child: const Text('Logout'),
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}
