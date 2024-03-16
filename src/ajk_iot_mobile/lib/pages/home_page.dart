import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../providers/auth_provider.dart';

class HomePage extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    // Dostęp do AuthProvider
    final authProvider = Provider.of<AuthProvider>(context);

    return Scaffold(
      appBar: AppBar(
        title: Text('Strona Główna'),
      ),
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: <Widget>[
            Text(
              'Witaj, ${authProvider.userInfo['username'] ?? 'Gościu'}!',
              style: TextStyle(fontSize: 20),
            ),
            SizedBox(height: 20), // Dodaj trochę przestrzeni
            ElevatedButton(
              onPressed: () {
                authProvider.logout(); // Wywołanie metody logout z AuthProvider
                Navigator.of(context).pushReplacementNamed('/'); // Przekierowanie do strony logowania
              },
              child: Text('Wyloguj'),
            ),
          ],
        ),
      ),
    );
  }
}