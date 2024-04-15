import 'package:ajk_iot_mobile/providers/device_provider.dart';
import 'package:ajk_iot_mobile/widgets/iot_device_widget.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../providers/auth_provider.dart';

class HomePage extends StatefulWidget {
  const HomePage({super.key});

  @override
  State<HomePage> createState() => _HomePageState();
}

class _HomePageState extends State<HomePage> {
  @override
  void initState() {
    super.initState();
    Future.microtask(
        () => Provider.of<DeviceProvider>(context, listen: false).getDevices());
    // Future.microtask(
    //     () => Provider.of<AuthProvider>(context, listen: false).loadUserInfo());
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
