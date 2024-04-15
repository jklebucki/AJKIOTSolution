import 'dart:async'; // Import for Timer
import 'package:flutter/material.dart';
import 'package:ajk_iot_mobile/models/device_feature.dart'; // Assuming this is the correct path for your DeviceFeature model

class Button3D extends StatefulWidget {
  final DeviceFeature feature;

  const Button3D({super.key, required this.feature});

  @override
  Button3DState createState() => Button3DState();
}

class Button3DState extends State<Button3D> {
  bool _isPressed = false;
  Timer? _pressTimer;

  void _onPressed() {
    setState(() {
      _isPressed = true;
    });
    // Cancel any existing timer
    _pressTimer?.cancel();
    // Set the pressed state to revert after 3 seconds
    _pressTimer = Timer(const Duration(seconds: 3), () {
      setState(() {
        _isPressed = false;
      });
    });
  }

  void _onReleased() {
    // The release functionality is handled by the timer
  }

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.all(8.0),
      child: GestureDetector(
        onTapDown: (_) => _onPressed(),
        onTapUp: (_) => _onReleased(),
        onTapCancel: _onReleased,
        child: AnimatedContainer(
          duration: const Duration(milliseconds: 100), // Quick response time for visual feedback
          width: 80,
          height: 80,
          decoration: BoxDecoration(
            color: _isPressed ? Colors.green : Colors.grey[300], // Green when pressed, grey otherwise
            borderRadius: BorderRadius.circular(40),
            boxShadow: [
              BoxShadow(
                color: const Color(0xFF666666),
                offset: _isPressed ? const Offset(0, 2) : const Offset(0, 4),
                blurRadius: 0,
              ),
            ],
          ),
          alignment: Alignment.center,
          child: Text(
            _isPressed ? '' : 'OPEN',  // Display "OPEN" only when not pressed
            style: const TextStyle(
              color: Colors.black,
              fontSize: 16,
              fontWeight: FontWeight.bold,
            ),
          ),
        ),
      ),
    );
  }

  @override
  void dispose() {
    _pressTimer?.cancel(); // Make sure to dispose of the timer to avoid memory leaks
    super.dispose();
  }
}
