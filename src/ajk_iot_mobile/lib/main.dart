import 'dart:io';

import 'package:ajk_iot_mobile/pages/home_page.dart';
import 'package:ajk_iot_mobile/pages/try_login_page.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'providers/auth_provider.dart';
import 'package:ajk_iot_mobile/pages/login_page.dart';


class MyHttpOverrides extends HttpOverrides {
  @override
  HttpClient createHttpClient(SecurityContext? context) {
    return super.createHttpClient(context)
      ..badCertificateCallback =
          (X509Certificate cert, String host, int port) => true;
  }
}
void main() {
  HttpOverrides.global = MyHttpOverrides();
  runApp(const AjkIotApp());
}

class AjkIotApp extends StatelessWidget {
  const AjkIotApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MultiProvider(
        providers: [
          ChangeNotifierProvider(create: (context) => AuthProvider()),
        ],
        child: MaterialApp(
          title: 'AJK IoT',
          initialRoute: '/trylogin',
          routes: {
            '/trylogin': (context) => const TryLoginPage(),
            '/login': (context) => const LoginPage(),
            '/home': (context) => HomePage(),
          },
        ));
  }
}
