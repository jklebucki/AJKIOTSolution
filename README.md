# AJKIOTSolution

IoT Solution (WEB, API, Mobile, IoT)

![100commitow](https://img.shields.io/badge/c%23,flutter,c-100commitow-blue)![Version](https://img.shields.io/badge/bundle_version-0.0.1-green)

This solution delivers four projects:

- **AJKIOT.Web** - An application that manages accounts and accesses
- **ajk_iot_mobile** - A mobile application
- **AJKIOT.Api** - Endpoints for the WebApp and MobileApp
- **ajk_iot_devices** - Software for the ESP8266 module

100 commits are aimed at providing a solution to support at least one IoT device.

---

## Project: Independent IoT Platform

### Project Goal

The goal of this project is to create a platform for IoT devices that can operate independently of the manufacturer's cloud services. The software is designed to be installed on your own resources, eliminating dependency on external servers. This architecture ensures full control over the devices even in the event of a global network outage. The project is simplified to ensure easy installation and maintenance.

The primary scenario is to install the API and WEB software on internal resources (e.g., servers within a facility containing IoT devices) and make them accessible externally. This configuration allows device control both from within the local network and from outside. Future releases will include the ability to define an arbitrary number of endpoints and support for new types of devices that will be managed through mobile and web applications.

### System Operation

The API application provides endpoints for all other components: WEB, Mobile, and IoT. Communication between WEB and Mobile occurs via REST requests and websockets using SignalR. IoT devices communicate through the MQTT protocol. All interactions occur in real-time, with interfaces updating to reflect the current status of devices.

### Typical Operation

1. Create an account in the WEB application.
2. Go to the "Devices" section and add the devices you have (currently, only two types of devices are supported).
3. Download the firmware available under the button on the device card.
4. Install the firmware on the device using PlatformIO (a separate guide with ready commands will be available).
5. Upon startup, the IoT device will expose an AP to which you need to connect. Then, using the mobile app (recommended) or a browser (address 192.168.4.1), configure the device to connect to the WiFi network visible to the API server.
6. Control devices through the mobile or WEB application.

### Technologies and Tools

- **API**: REST, SignalR
- **IoT Communication Protocol**: MQTT
- **Firmware Installation**: PlatformIO

The project is designed with simplicity and reliability in mind, emphasizing independence from external servers and flexibility in managing IoT devices.

---

By dedicating efforts to design this platform, we gain full control over IoT devices, independence from manufacturers' clouds, and ease of installation and management. We invite you to participate in the project and submit improvement proposals on GitHub.


---

# It's alive!

Visit the live site: [https://iot.ajksoftware.pl/](https://iot.ajksoftware.pl/)

The current release version is ready for public testing. Considering the current stage and stability, it is recommended to label this release as a **Release Candidate (RC)** version.

## Android mobile app available
Consent to test: [https://play.google.com/apps/testing/pl.ajksoftware.ajkiot](https://play.google.com/apps/testing/pl.ajksoftware.ajkiot)

Link to the application on Google Store: [https://play.google.com/store/apps/details?id=pl.ajksoftware.ajkiot](https://play.google.com/store/apps/details?id=pl.ajksoftware.ajkiot)

## iOS mobile  app available
In the first step, install the TestFlight app from the AppStore.

In the second step, click on the link to join the application testing. [https://testflight.apple.com/join/piRa838S]()

### Set API Address: [https://iot.ajksoftware.pl:7253](https://iot.ajksoftware.pl:7253)

### Create an account and feel free to test!

----
