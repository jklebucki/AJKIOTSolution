import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

class AuthProvider with ChangeNotifier {
  final String _baseUrl = 'https://localhost:7253/api/Users';
  String _token = "";
  String _refreshToken = "";
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
      _token = responseData['token'][0];
      _refreshToken = responseData['token'][1];
      await _storage.write(key: 'jwt', value: _token);
      await _storage.write(key: 'jwtRefresh', value: _refreshToken);
      setUserInfo(responseData);
      notifyListeners();
      return true;
    }
    return false;
  }

  Future<bool> refreshToken() async {
    final url = Uri.parse('$_baseUrl/refresh');
    final response = await http.post(
      url,
      headers: {'Content-Type': 'application/json'},
      body: json.encode({
        'refreshToken': _refreshToken,
      }),
    );

    if (response.statusCode == 200) {
      final responseData = json.decode(response.body);
      _token = responseData['token'][0];
      _refreshToken = responseData['token'][1];
      await _storage.write(key: 'jwt', value: _token);
      await _storage.write(key: 'jwtRefresh', value: _refreshToken);
      notifyListeners();
      return true;
    }
    return false;
  }

  void setUserInfo(Map<String, dynamic> userInfo) {
    _userInfo = userInfo;
    notifyListeners();
  }

  Future<bool> tryAutoLogin() async {
    final storedToken = await _storage.read(key: 'jwtRefresh');
    if (storedToken != null) {
      await refreshToken();
      notifyListeners();
      return true;
    }
    return false;
  }

  Future<void> logout() async {
    _token = "";
    _userInfo = {};
    await _storage.delete(key: 'jwt');
    await _storage.delete(key: 'jwtRefresh');
    notifyListeners();
  }
}
