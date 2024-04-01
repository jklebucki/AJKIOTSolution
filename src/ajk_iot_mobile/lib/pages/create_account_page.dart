import 'package:flutter/material.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:provider/provider.dart';
import '../providers/auth_provider.dart'; // Adjust the import path according to your project structure

class CreateAccountPage extends StatefulWidget {
  const CreateAccountPage({super.key});

  @override
  CreateAccountPageState createState() => CreateAccountPageState();
}

class CreateAccountPageState extends State<CreateAccountPage> {
  final _formKey = GlobalKey<FormState>();
  final _storage = const FlutterSecureStorage();
  final _emailController = TextEditingController();
  final _usernameController = TextEditingController();
  final _passwordController = TextEditingController();
  final _confirmPasswordController = TextEditingController();
  final _apiAddressController = TextEditingController();
  String _loginError = '';

  @override
  void dispose() {
    // Dispose of the controllers when the widget is removed from the widget tree
    _emailController.dispose();
    _usernameController.dispose();
    _passwordController.dispose();
    _confirmPasswordController.dispose();
    super.dispose();
  }

  Future<void> _saveCredentials() async {
    await _storage.write(key: 'email', value: _emailController.text);
    await _storage.write(key: 'password', value: _passwordController.text);
    await _storage.write(key: 'apiUrl', value: _apiAddressController.text);
  }

  bool _isPasswordCompliant(String password, [int minLength = 8]) {
    if (password.isEmpty) {
      return false;
    }
    bool hasUppercase = password.contains(RegExp(r'[A-Z]'));
    bool hasDigits = password.contains(RegExp(r'[0-9]'));
    bool hasLowercase = password.contains(RegExp(r'[a-z]'));
    bool hasMinLength = password.length >= minLength;

    return hasDigits & hasUppercase & hasLowercase & hasMinLength;
  }

  Future<void> _submit() async {
    final isValid = _formKey.currentState?.validate();
    if (isValid ?? false) {
      await _saveCredentials();
      if (mounted) {
        final provider = Provider.of<AuthProvider>(context, listen: false);
        var success = await provider.register(
          _emailController.text,
          _usernameController.text,
          _passwordController.text,
          'YourApplicationAddress', // Define or get your application address
          'User',
        );
        if (success && mounted) {
          Navigator.pushReplacementNamed(context, '/trylogin');
        } else {
          setState(() {
            _loginError = provider.errorMessage;
          });
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
      appBar: AppBar(
        title: const Text('Create Account'),
      ),
      body: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Form(
          key: _formKey,
          child: SingleChildScrollView(
            child: Column(
              children: <Widget>[
                TextFormField(
                  controller: _apiAddressController,
                  decoration: const InputDecoration(labelText: 'API Address'),
                  validator: (value) => Uri.tryParse(value!)?.isAbsolute == true
                      ? null
                      : 'Please enter a valid URL',
                ),
                TextFormField(
                  controller: _emailController,
                  decoration: const InputDecoration(labelText: 'Email'),
                  validator: (value) {
                    if (value == null ||
                        value.isEmpty ||
                        !value.contains('@')) {
                      return 'Please enter a valid email';
                    }
                    return null;
                  },
                ),
                TextFormField(
                  controller: _usernameController,
                  decoration: const InputDecoration(labelText: 'Username'),
                  validator: (value) {
                    if (value == null || value.isEmpty) {
                      return 'Please enter a username';
                    }
                    return null;
                  },
                ),
                TextFormField(
                  controller: _passwordController,
                  decoration: const InputDecoration(labelText: 'Password'),
                  obscureText: true,
                  validator: (value) {
                    if (value == null || !_isPasswordCompliant(value)) {
                      return 'Password must be at least 8 characters, include an uppercase letter, a number, and a lowercase letter.';
                    }
                    return null;
                  },
                ),
                TextFormField(
                  controller: _confirmPasswordController,
                  decoration:
                      const InputDecoration(labelText: 'Confirm Password'),
                  obscureText: true,
                  validator: (value) {
                    if (value != _passwordController.text) {
                      return 'Passwords do not match';
                    }
                    return null;
                  },
                ),
                const SizedBox(height: 20),
                ElevatedButton(
                  onPressed: _submit,
                  child: const Text('Create Account'),
                ),
                if (_loginError
                    .isNotEmpty) // Display login error message if any
                  Padding(
                    padding: const EdgeInsets.only(top: 20),
                    child: Text(_loginError,
                        style: const TextStyle(color: Colors.red)),
                  ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
