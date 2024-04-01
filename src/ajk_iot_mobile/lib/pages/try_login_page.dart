import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../providers/auth_provider.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

class TryLoginPage extends StatefulWidget {
  const TryLoginPage({super.key});

  @override
  _TryLoginPageState createState() => _TryLoginPageState();
}

class _TryLoginPageState extends State<TryLoginPage> {
  final FlutterSecureStorage _storage = const FlutterSecureStorage();

  @override
  void initState() {
    super.initState();
    _tryAutoLogin();
  }

  Future<void> _tryAutoLogin() async {
    final email = await _storage.read(key: 'email');
    final password = await _storage.read(key: 'password');
    if (email != null && password != null) {
      final authProvider = Provider.of<AuthProvider>(context, listen: false);
      bool success = await authProvider.login(email, password);
      if (!success) {
        setState(() {
          Navigator.pushReplacementNamed(context, '/login');
        });
      }
      if (success) {
        Navigator.pushReplacementNamed(context, '/home');
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    double screenWidth = MediaQuery.of(context).size.width;

    return Scaffold(
      appBar: AppBar(
        title: const Text('Auto Login'),
      ),
      body: Center(
        child: SizedBox(
          width: screenWidth * 0.8,
          height: screenWidth * 0.8,
          child: Center(
            child: CircularProgressIndicator(
              valueColor: AlwaysStoppedAnimation<Color>(
                  Theme.of(context).colorScheme.secondary),
              strokeWidth: 8.0,
            ),
          ),
        ),
      ),
    );
  }
}
