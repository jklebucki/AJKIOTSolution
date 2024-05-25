import 'package:ajk_iot_mobile/pages/create_account_page.dart';
import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:provider/provider.dart';
import '../providers/auth_provider.dart';

class LoginPage extends StatefulWidget {
  const LoginPage({super.key});

  @override
  LoginPageState createState() => LoginPageState();
}

class LoginPageState extends State<LoginPage> {
  final _formKey = GlobalKey<FormState>();
  final _storage = const FlutterSecureStorage();
  final TextEditingController _emailController = TextEditingController();
  final TextEditingController _passwordController = TextEditingController();
  final TextEditingController _apiAddressController = TextEditingController();
  String _loginError = '';

  @override
  void initState() {
    super.initState();
    _loadCredentials();
  }

  Future<void> _loadCredentials() async {
    final storedEmail = await _storage.read(key: 'email');
    final storedPassword = await _storage.read(key: 'password');
    final storedApiAddress = await _storage.read(key: 'apiUrl');

    _emailController.text = storedEmail ?? '';
    _passwordController.text = storedPassword ?? '';
    _apiAddressController.text = storedApiAddress ?? '';
  }

  Future<void> _saveCredentials() async {
    await _storage.write(key: 'email', value: _emailController.text);
    await _storage.write(key: 'password', value: _passwordController.text);
    await _storage.write(key: 'apiUrl', value: _apiAddressController.text);
  }

  Future<void> _attemptLogin() async {
    if (_formKey.currentState!.validate()) {
      await _saveCredentials();

      if (mounted) {
        final provider = Provider.of<AuthProvider>(context, listen: false);
        final loginSuccessful = await provider.login(
            _emailController.text, _passwordController.text);
        if (mounted) {
          // Check if the widget is still in the tree
          if (loginSuccessful) {
            Navigator.of(context).pushReplacementNamed('/home');
          } else {
            setState(() {
              _loginError = provider.errorMessage;
            });
          }
        }
      } else {
        setState(() {
          _loginError = "Application error";
        });
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('AJK IoT Mobile')),
      body: Form(
        key: _formKey,
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(20.0),
          child: Column(
            children: [
              SizedBox(height: 0.05.sh),
              Text(
                "Welcome!",
                style: TextStyle(fontSize: 36.sp),
                textAlign: TextAlign.center,
                softWrap: true,
              ),
              Text(
                "AJK IoT Mobile",
                style: TextStyle(fontSize: 36.sp),
                textAlign: TextAlign.center,
                softWrap: true,
              ),
              SizedBox(height: 0.03.sh),
              TextFormField(
                controller: _apiAddressController,
                autocorrect: false,
                decoration: InputDecoration(
                    labelText: 'API Address',
                    labelStyle:
                        TextStyle(color: Colors.black, fontSize: 16.spMax)),
                validator: (value) => Uri.tryParse(value!)?.isAbsolute == true
                    ? null
                    : 'Please enter a valid URL',
              ),
              TextFormField(
                controller: _emailController,
                autocorrect: false,
                keyboardType: TextInputType.emailAddress,
                decoration: InputDecoration(
                    labelText: 'Email',
                    labelStyle:
                        TextStyle(color: Colors.black, fontSize: 16.spMax)),
                validator: (value) =>
                    value!.contains('@') ? null : 'Please enter a valid email',
              ),
              TextFormField(
                controller: _passwordController,
                decoration: InputDecoration(
                    labelText: 'Password',
                    labelStyle:
                        TextStyle(color: Colors.black, fontSize: 16.spMax)),
                autocorrect: false,
                obscureText: true,
                validator: (value) => value!.length >= 6
                    ? null
                    : 'Password must be at least 6 characters',
              ),
              const SizedBox(height: 20),
              ElevatedButton(
                onPressed: _attemptLogin,
                child: const Text('Login'),
              ),
              const SizedBox(
                  height:
                      20), // Provides spacing between the login form and the create account button.
              TextButton(
                onPressed: () {
                  // Navigate to the CreateAccountPage
                  Navigator.of(context).push(
                    MaterialPageRoute(
                        builder: (context) => const CreateAccountPage()),
                  );
                },
                child: const Text('Don\'t have an account? Create one'),
              ),
              if (_loginError.isNotEmpty) // Display login error message if any
                Padding(
                  padding: const EdgeInsets.only(top: 20),
                  child: Text(_loginError,
                      style: const TextStyle(color: Colors.red)),
                ),
            ],
          ),
        ),
      ),
    );
  }

  @override
  void dispose() {
    // Dispose controllers when the widget is disposed
    _emailController.dispose();
    _passwordController.dispose();
    _apiAddressController.dispose();
    super.dispose();
  }
}
