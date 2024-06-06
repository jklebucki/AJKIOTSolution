import 'dart:io';
import 'package:ajk_iot_mobile/pages/create_account_page.dart';
import 'package:ajk_iot_mobile/pages/home_page.dart';
import 'package:ajk_iot_mobile/pages/try_login_page.dart';
import 'package:ajk_iot_mobile/providers/device_provider.dart';
import 'package:flutter/material.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
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

void main() async {
  HttpOverrides.global = MyHttpOverrides();
  await ScreenUtil.ensureScreenSize();
  runApp(const AjkIotApp());
}

class AjkIotApp extends StatelessWidget {
  const AjkIotApp({super.key});

  @override
  Widget build(BuildContext context) {
    ScreenUtil.init(context, designSize: const Size(360, 690));
    return MultiProvider(
      providers: [
        ChangeNotifierProvider(create: (context) => AuthProvider()),
        ChangeNotifierProvider(create: (context) => DeviceProvider()),
      ],
      child: MaterialApp(
        debugShowCheckedModeBanner: false,
        title: 'AJK IoT',
        theme: ThemeData(
          appBarTheme: AppBarTheme(
            backgroundColor: Colors.black,
            foregroundColor: Colors.white,
            titleTextStyle: TextStyle(
                color: Colors.white,
                fontSize: 28.spMax,
                fontWeight: FontWeight.bold),
          ),
          primarySwatch: Colors.red,
          textTheme: TextTheme(
            displayLarge:
                TextStyle(fontSize: 24.sp, fontWeight: FontWeight.bold),
            bodyLarge: TextStyle(fontSize: 14.sp),
          ),
          switchTheme: SwitchThemeData(
            trackColor: WidgetStateProperty.resolveWith((states) {
              if (states.contains(WidgetState.selected)) {
                return Colors.green; // Active state color
              }
              return Colors.red; // Inactive state color
            }),
            thumbColor: WidgetStateProperty.all(Colors.white),
          ),
          elevatedButtonTheme: ElevatedButtonThemeData(
            style: ButtonStyle(
              minimumSize:
                  WidgetStateProperty.all<Size>(Size(120.spMax, 40.spMax)),
              backgroundColor: WidgetStateProperty.all<Color>(Colors.blue),
              foregroundColor: WidgetStateProperty.all<Color>(Colors.white),
              textStyle: WidgetStateProperty.all<TextStyle>(
                TextStyle(fontSize: 16.sp),
              ),
              shape: WidgetStateProperty.all<RoundedRectangleBorder>(
                RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(6.spMax),
                ),
              ),
              overlayColor: WidgetStateProperty.resolveWith<Color?>((states) {
                if (states.contains(WidgetState.pressed)) {
                  return Colors.blue.withOpacity(0.5); // Effect color on press
                }
                return null;
              }),
            ),
          ),
          colorScheme: ColorScheme.fromSwatch(
            backgroundColor: Colors.grey[300],
          ),
        ),
        initialRoute: '/trylogin',
        routes: {
          '/trylogin': (context) => const TryLoginPage(),
          '/login': (context) => const LoginPage(),
          '/home': (context) => const HomePage(),
          '/register': (context) => const CreateAccountPage(),
        },
      ),
    );
  }
}
