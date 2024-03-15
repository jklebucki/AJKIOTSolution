import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

class AuthProvider with ChangeNotifier {
  final String _baseUrl = 'https://localhost:7253/api/Users';
  String _token = "";
  final _storage = const FlutterSecureStorage();
  Map<String, dynamic> _userInfo = {};


  bool get isAuthenticated => _token != null;
  Map<String, dynamic> get userInfo => _userInfo;

  Future<bool> login(String email, String password) async {
    final url = Uri.parse('$_baseUrl/login');
    final response = await http.post(
      url,
      headers: {'Content-Type': 'application/json'},
      body: json.encode({
        'email': email,
        'password': password,
      }),
    );

    if (response.statusCode == 200) {
      final responseData = json.decode(response.body);
      _token = responseData['token'];
      await _storage.write(key: 'jwt', value: _token);
      setUserInfo(responseData); // Załóżmy, że API zwraca informacje o użytkowniku w 'user'
      notifyListeners();
      return true;
    }
    return false;
  }

  void setUserInfo(Map<String, dynamic> userInfo) {
    _userInfo = userInfo;
    notifyListeners();
  }

  Future<void> tryAutoLogin() async {
    final storedToken = await _storage.read(key: 'jwt');
    if (storedToken != null) {
      _token = storedToken;
      // Tutaj możesz chcieć ponownie pobrać informacje o użytkowniku z API,
      // używając storedToken do uwierzytelnienia żądania
      notifyListeners();
    }
  }

  Future<void> logout() async {
    _token = "";
    _userInfo = {};
    await _storage.delete(key: 'jwt');
    notifyListeners();
  }
}