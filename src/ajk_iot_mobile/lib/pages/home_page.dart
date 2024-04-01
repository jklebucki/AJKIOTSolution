import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../providers/auth_provider.dart';

class HomePage extends StatelessWidget {
  const HomePage({super.key});

  @override
  Widget build(BuildContext context) {
    // Dostęp do AuthProvider
    final authProvider = Provider.of<AuthProvider>(context);

    return Scaffold(
      appBar: AppBar(
        title: const Text('Strona Główna'),
      ),
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: <Widget>[
            Text(
              'Witaj, ${authProvider.userInfo['username'] ?? 'Gościu'}!',
              style: const TextStyle(fontSize: 20),
            ),
            const SizedBox(height: 20),
            ElevatedButton(
              onPressed: () {
                authProvider.logout(); 
                Navigator.of(context).pushReplacementNamed('/login');
              },
              child: const Text('Wyloguj'),
            ),
          ],
        ),
      ),
    );
  }
}