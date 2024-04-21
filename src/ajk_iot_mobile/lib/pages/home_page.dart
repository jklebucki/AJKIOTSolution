import 'package:ajk_iot_mobile/providers/device_provider.dart';
import 'package:ajk_iot_mobile/widgets/iot_device_widget.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../providers/auth_provider.dart';
import 'package:signalr_flutter/signalr_api.dart';
import 'package:signalr_flutter/signalr_flutter.dart';

class HomePage extends StatefulWidget {
  const HomePage({super.key});

  @override
  State<HomePage> createState() => _HomePageState();
}

class _HomePageState extends State<HomePage> {
  String signalRStatus = "disconnected";
  late SignalR signalR;
  @override
  void initState() {
    super.initState();
    initPlatformState();
    Future.microtask(
        () => Provider.of<DeviceProvider>(context, listen: false).getDevices());
    // Future.microtask(
    //     () => Provider.of<AuthProvider>(context, listen: false).loadUserInfo());
  }

Future<void> initPlatformState() async {
    signalR = SignalR(
      "<Your server url here>",
      "<Your hub name here>",
      hubMethods: ["<Your Hub Method Names>"],
      statusChangeCallback: _onStatusChange,
      hubCallback: _onNewMessage,
    );
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
void _onStatusChange(ConnectionStatus? status) {
    if (mounted) {
      setState(() {
        signalRStatus = status?.name ?? ConnectionStatus.disconnected.name;
      });
    }
  }

  void _onNewMessage(String methodName, String message) {
    print("MethodName = $methodName, Message = $message");
  }

  void _buttonTapped() async {
    try {
      final result = await signalR.invokeMethod("<Your Method Name>",
          arguments: ["<Your Method Arguments>"]);
      print(result);
    } catch (e) {
      print(e);
    }
  }

}
