import 'dart:async';
import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

class AuthProvider with ChangeNotifier {
  // Initialization of necessary variables
  String _baseUrl = ''; 
  String _token = "";
  String _refreshToken = "";
  final FlutterSecureStorage _storage = const FlutterSecureStorage();
  Map<String, dynamic> _userInfo = {};
  String _errorMessage = "";

  AuthProvider() {
    loadBaseUrl();
  }

  // Getters to expose necessary data
  bool get isAuthenticated => _token.isNotEmpty;
  Map<String, dynamic> get userInfo => _userInfo;
  String get errorMessage => _errorMessage;

  // Method to load base URL from secure storage
  Future<void> loadBaseUrl() async {
    String? storedUrl = await _storage.read(key: 'apiUrl');
    _baseUrl = storedUrl ?? ''; 
    notifyListeners();
  }

  // Private method to set error message and notify listeners
  void _setError(String message) {
    _errorMessage = message;
    notifyListeners();
  }

  // Login method
  Future<bool> login(String email, String password) async {
    await loadBaseUrl(); // Ensure the latest base URL is loaded
    final Uri url = Uri.parse('$_baseUrl/api/Users/login');
    try {
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
        final data = responseData['data'];
        _token = data['tokens'][0];
        _refreshToken = data['tokens'][1];
        await _storage.write(key: 'jwt', value: _token);
        await _storage.write(key: 'jwtRefresh', value: _refreshToken);
        _userInfo = responseData['data'];
        notifyListeners();
        return true;
      } else {
        _setError("Failed to login. Please check your credentials.");
        return false;
      }
    } catch (e) {
      _setError("Failed to connect to the server.\n$e");
      return false;
    }
  }

  // Refresh token method
  Future<bool> refreshToken() async {
    await loadBaseUrl(); // Ensure the latest base URL is loaded
    final Uri url = Uri.parse('$_baseUrl/Users/refreshToken');
    try {
      final response = await http.post(
        url,
        headers: {'Content-Type': 'application/json'},
        body: json.encode({
          'refreshToken': _refreshToken,
        }),
      );

      if (response.statusCode == 200) {
        final responseData = json.decode(response.body);
        _token = responseData['token'];
        _refreshToken = responseData['refreshToken'];
        await _storage.write(key: 'jwt', value: _token);
        await _storage.write(key: 'jwtRefresh', value: _refreshToken);
        notifyListeners();
        return true;
      } else {
        _setError("Failed to refresh token.");
        return false;
      }
    } catch (e) {
      _setError("Failed to connect to the server.");
      return false;
    }
  }

Future<bool> register(String email, String username, String password, String applicationAddress, String role) async {
  final url = Uri.parse('$_baseUrl/api/Users/register');
  try {
    final response = await http.post(
      url,
      headers: {'Content-Type': 'application/json'},
      body: json.encode({
        'email': email,
        'username': username,
        'password': password,
        'applicationAddress': applicationAddress,
        'role': role,
      }),
    );

    if (response.statusCode == 200) {
      // Handle successful registration
      final responseData = json.decode(response.body);
      _token = responseData['token'];
      _refreshToken = responseData['refreshToken'];
      _userInfo = responseData['user'];
      await _storage.write(key: 'token', value: _token);
      await _storage.write(key: 'refreshToken', value: _refreshToken);
      notifyListeners();
      return true;
    } else {
      _setError("Failed to register");
      return false;
    }
  } catch (error) {
    _setError("Failed to connect to the server.");
    return false;
  }
}

  // Logout method
  Future<void> logout() async {
    _token = "";
    _refreshToken = "";
    _userInfo = {};
    _errorMessage = "";
    await _storage.delete(key: 'jwt');
    await _storage.delete(key: 'jwtRefresh');
    notifyListeners();
  }

}
