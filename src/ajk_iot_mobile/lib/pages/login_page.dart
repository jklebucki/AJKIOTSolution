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
      body: Center(
        child: SingleChildScrollView(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: <Widget>[
              SizedBox(
                width: MediaQuery.of(context).size.width * 0.8,
                child: TextField(
                  controller: emailController,
                  decoration: InputDecoration(
                    labelText: 'Email',
                    filled: true,
                    fillColor: Colors.grey[200],
                    border: OutlineInputBorder( // Zmienia typ obramowania
                      borderRadius: BorderRadius.circular(8), // Dodaje zaokrąglenie
                      borderSide: BorderSide.none, // Usuwa standardową ramkę
                    ),
                    contentPadding: const EdgeInsets.all(10),
                  ),
                ),
              ),
              const SizedBox(height: 20),
              SizedBox(
                width: MediaQuery.of(context).size.width * 0.8,
                child: TextField(
                  controller: passwordController,
                  obscureText: true,
                  decoration: InputDecoration(
                    labelText: 'Password',
                    filled: true,
                    fillColor: Colors.grey[200],
                    border: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(8), // Dodaje zaokrąglenie
                      borderSide: BorderSide.none, // Usuwa standardową ramkę
                    ),
                    contentPadding: const EdgeInsets.all(10),
                  ),
                ),
              ),
              const SizedBox(height: 20),
              ElevatedButton(
                onPressed: () async {
                  final success = await authProvider.login(emailController.text, passwordController.text);
                  if (success) {
                    Navigator.of(context).pushReplacementNamed('/home');
                  } else {
                    ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Authentication Failed')));
                  }
                },
                style: ElevatedButton.styleFrom(
                    backgroundColor: Colors.black, // Tło przycisku
                    foregroundColor: Colors.white, // Kolor tekstu
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(30), // Zaokrąglenie krawędzi
                    ),
                    padding: const EdgeInsets.symmetric(vertical: 15), // Padding dla większej wysokości
                  ),
                  child: const Text('Login', style: TextStyle(fontSize: 18)),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
