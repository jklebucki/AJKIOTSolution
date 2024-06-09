import 'package:ajk_iot_mobile/services/device_config_service.dart';
import 'package:flutter/gestures.dart';
import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

class DeviceSettingsPage extends StatefulWidget {
  final String deviceId;

  const DeviceSettingsPage({
    super.key,
    required this.deviceId,
  });

  @override
  DeviceSettingsPageState createState() => DeviceSettingsPageState();
}

class DeviceSettingsPageState extends State<DeviceSettingsPage> {
  final TextEditingController _apiAddressController = TextEditingController();
  final TextEditingController _deviceIdController = TextEditingController();
  final TextEditingController _wifiSsidController = TextEditingController();
  final TextEditingController _wifiPasswordController = TextEditingController();
  final _storage = const FlutterSecureStorage();
  final _deviceConfigService = DeviceConfigService();

  bool _isEditing = false;
  bool _isSending = false; // State to track if data is being sent

  @override
  void initState() {
    super.initState();
    _deviceIdController.text = widget.deviceId;
    _fetchApiAddress();
  }

  Future<void> _fetchApiAddress() async {
    // Simulacja zapytania sieciowego w celu pobrania adresu API
    var apiAddress = (await _storage.read(key: 'apiUrl')) ?? '';
    var wifiSsid = await _storage.read(key: 'wifiSsid');
    var wifiPassword = await _storage.read(key: 'wifiPassword');
    setState(() {
      _apiAddressController.text = apiAddress.isNotEmpty
          ? apiAddress
              .replaceFirst('http://', '')
              .replaceFirst('https://', '')
              .split(':')[0]
          : 'set your api address';
      _wifiSsidController.text = wifiSsid ?? '';
      _wifiPasswordController.text = wifiPassword ?? '';
    });
  }

  void _toggleEditing() {
    setState(() {
      _isEditing = !_isEditing;
    });
  }

  Future<void> _sendDataToDevice() async {
    setState(() {
      _isSending = true; // Set loading state to true
    });

    _storage.write(key: 'wifiSsid', value: _wifiSsidController.text);
    _storage.write(key: 'wifiPassword', value: _wifiPasswordController.text);

    var result = await _deviceConfigService.setDevice(
        deviceid: _deviceIdController.text,
        ssid: _wifiSsidController.text,
        password: _wifiPasswordController.text,
        mqttserver: _apiAddressController.text);

    if (mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        result
            ? const SnackBar(content: Text('Data sent to device'))
            : const SnackBar(content: Text('Error sending data to device')),
      );
    }

    setState(() {
      _isSending = false; // Set loading state to false
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Center(child: Text('Device Settings')),
        actions: [
          IconButton(
            icon: Icon(_isEditing ? Icons.check : Icons.edit),
            onPressed: _toggleEditing,
          ),
        ],
      ),
      body: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Card(
          elevation: 4.0,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(10.spMax),
          ),
          child: SingleChildScrollView(
            child: Padding(
              padding: EdgeInsets.all(12.spMax),
              child: Column(
                children: [
                  SizedBox(height: 6.spMax),
                  Text(
                    "Reset the device using the reset button. Connect to the device's WiFi and fill out the form below with the SSID and password of the WiFi network you want the device to connect to. The API Address and Device ID fields are filled in automatically. Then, click the 'Send to device' button.",
                    style: TextStyle(
                      fontSize: 12.spMax,
                      fontWeight: FontWeight.normal,
                    ),
                    textAlign: TextAlign.justify,
                  ),
                  SizedBox(height: 20.spMax),
                  TextField(
                    controller: _apiAddressController,
                    readOnly: !_isEditing,
                    decoration: InputDecoration(
                      labelText: 'API Address',
                      border: OutlineInputBorder(
                        borderRadius: BorderRadius.circular(8.spMax),
                      ),
                      focusedBorder: const OutlineInputBorder(
                        borderSide: BorderSide(
                          color: Colors.green,
                        ),
                      ),
                      enabledBorder: const OutlineInputBorder(
                        borderSide: BorderSide(
                          color: Colors.blue,
                        ),
                      ),
                    ),
                    autocorrect: false,
                  ),
                  const SizedBox(height: 16.0),
                  TextField(
                    controller: _deviceIdController,
                    readOnly: !_isEditing,
                    decoration: InputDecoration(
                      labelText: 'Device ID',
                      border: OutlineInputBorder(
                        borderRadius: BorderRadius.circular(8.spMax),
                      ),
                      focusedBorder: const OutlineInputBorder(
                        borderSide: BorderSide(
                          color: Colors.green,
                        ),
                      ),
                      enabledBorder: const OutlineInputBorder(
                        borderSide: BorderSide(
                          color: Colors.blue,
                        ),
                      ),
                    ),
                    autocorrect: false,
                  ),
                  const SizedBox(height: 16.0),
                  TextField(
                    controller: _wifiSsidController,
                    decoration: InputDecoration(
                      labelText: 'WiFi SSID',
                      border: OutlineInputBorder(
                        borderRadius: BorderRadius.circular(8.spMax),
                      ),
                      focusedBorder: const OutlineInputBorder(
                        borderSide: BorderSide(
                          color: Colors.green,
                        ),
                      ),
                      enabledBorder: const OutlineInputBorder(
                        borderSide: BorderSide(
                          color: Colors.blue,
                        ),
                      ),
                    ),
                    autocorrect: false,
                  ),
                  const SizedBox(height: 16.0),
                  TextField(
                    controller: _wifiPasswordController,
                    decoration: InputDecoration(
                      labelText: 'WiFi Password',
                      border: OutlineInputBorder(
                        borderRadius: BorderRadius.circular(8.spMax),
                      ),
                      focusedBorder: const OutlineInputBorder(
                        borderSide: BorderSide(
                          color: Colors.green,
                        ),
                      ),
                      enabledBorder: const OutlineInputBorder(
                        borderSide: BorderSide(
                          color: Colors.blue,
                        ),
                      ),
                    ),
                    autocorrect: false,
                    obscureText: true,
                  ),
                  const SizedBox(height: 20),
                  ElevatedButton(
                    onPressed: _isSending
                        ? null
                        : _sendDataToDevice, // Disable button when sending
                    child: _isSending
                        ? const CircularProgressIndicator(
                            color: Colors.white,
                          )
                        : const Text('Send to device'),
                  ),
                  SizedBox(height: 20.spMax),
                  Text.rich(
                    TextSpan(
                      text: 'The device creates an access point named ',
                      style: TextStyle(fontSize: 12.spMax),
                      children: <TextSpan>[
                        const TextSpan(
                          text: 'AJK_IoT_ESP32_Config',
                          style: TextStyle(fontWeight: FontWeight.bold),
                        ),
                        const TextSpan(
                          text:
                              ' which you need to connect to. You can also go to the device\'s webpage in your browser at ',
                        ),
                        TextSpan(
                          text: 'http://192.168.4.1',
                          style: const TextStyle(
                            color: Colors.blue,
                            decoration: TextDecoration.underline,
                          ),
                          recognizer: TapGestureRecognizer()
                            ..onTap = () {
                              //launch('http://192.168.4.1');
                            },
                        ),
                        const TextSpan(
                          text:
                              ' and fill out the configuration form on the page.',
                        ),
                      ],
                    ),
                    textAlign: TextAlign.justify,
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }

  @override
  void dispose() {
    _apiAddressController.dispose();
    _deviceIdController.dispose();
    _wifiSsidController.dispose();
    _wifiPasswordController.dispose();
    super.dispose();
  }
}
