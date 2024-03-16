import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '/providers/auth_provider.dart';

class LoginPage extends StatelessWidget {
  const LoginPage({super.key});

  @override
  Widget build(BuildContext context) {
    final emailController = TextEditingController();
    final passwordController = TextEditingController();
    final authProvider = Provider.of<AuthProvider>(context, listen: false);

    return Scaffold(
      appBar: AppBar(title: const Text('Login')),
      body: Column(
        children: <Widget>[
          TextField(controller: emailController, decoration: const InputDecoration(labelText: 'Email')),
          TextField(controller: passwordController, decoration: const InputDecoration(labelText: 'Password'), obscureText: true),
          ElevatedButton(
            onPressed: () async {
              final success = await authProvider.login(emailController.text, passwordController.text);
              if (success) {
                Navigator.of(context).pushReplacementNamed('/home');
              } else {
                ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Authentication Failed')));
              }
            },
            child: const Text('Login'),
          ),
        ],
      ),
    );
  }
}