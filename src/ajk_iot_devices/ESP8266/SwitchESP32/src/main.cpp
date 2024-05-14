#include <WiFi.h>
#include <WiFiClientSecure.h>
#include <PubSubClient.h>
#include "FS.h"
#include "SPIFFS.h"

const char *ssl_cert PROGMEM = R"EOF(
  -----BEGIN CERTIFICATE-----
MIICsTCCAZkCFHYzZ2INQ6RXFnzugeOtBL2mxggwMA0GCSqGSIb3DQEBCwUAMBQx
EjAQBgNVBAMMCUFKS1Jvb3RDQTAeFw0yNDA1MTMxODU2MjlaFw0zNDA1MTExODU2
MjlaMBYxFDASBgNVBAMMC0VTUDMyQ2xpZW50MIIBIjANBgkqhkiG9w0BAQEFAAOC
AQ8AMIIBCgKCAQEAuhXr46chV5+EtMwPyEaKvmbrTG1gbLr6EBe/WpxFn6SFPz9M
Wr68Y3Adag+i3Hih2eWtMSawLw7kD+SlHw4Rsoer1EvRVSshzop+Wc17aRlThqKW
4VRs55QQIiAKpcJ4zLsQcTH4MqPvWuCfts1Sa3Fm/zUwH33zbz5LydGv8SBv2kub
G+T7hVVd+AM+y1qjx4cFlJ4OHx8FH2t5pzN6+Py3qw8J9i1hvl0L+8eV6U8njnVP
1YpMc4xwsYmme5plEcpHLTUAtq/zVgRzncgxdVe6ofw9syvM2kmRCwGdLwGPjII7
4T1NKITaicRN4ugpntwF2iGYWb+y+IpS+m2scQIDAQABMA0GCSqGSIb3DQEBCwUA
A4IBAQCPa6H78JZ5n1s12BepIkviWk/GfFuTqJf7QuYvrtsY7JHZrIsz6N0vSTtc
Fj4Mhg4BVGmXAJJLN1uFXqZeTzuAMSSiwxtHGa62/7xQ36KgJhdpRbdU3hyEWRE7
9mj3R8rURSnJCfcnHUue9I2DjzdOp398xXPytqllXBLCthSNveKYDSnni+M9U4yd
YkbvtiA59JKTmWmr+KuBlzTCZPDhYPi9/ohKgg7JuRooKEf28I/h+tDNK/0ocvXk
68w1XyPU6QmiStqHmcu20Ob7tgR9vZw0LekQ26kbtFAxRL5scORDAw9QZ5gVJ3Xv
x0NTgoWFotWodvsVU8SyOKlbfe6E
-----END CERTIFICATE-----
    )EOF";

static const char *root_ca PROGMEM = R"EOF(
-----BEGIN CERTIFICATE-----
MIIDCTCCAfGgAwIBAgIUJz+BNUN+t0DTgbEZOySp7/d2Sh0wDQYJKoZIhvcNAQEL
BQAwFDESMBAGA1UEAwwJQUpLUm9vdENBMB4XDTI0MDUxMzE4NTYyOFoXDTM0MDUx
MTE4NTYyOFowFDESMBAGA1UEAwwJQUpLUm9vdENBMIIBIjANBgkqhkiG9w0BAQEF
AAOCAQ8AMIIBCgKCAQEAyn3J5OrOBvx7y7oyv5sms1lc2j+WauO5iONqykMbE75L
Kogvn1DrwF0ZvKHZ/hgh6E2ZHtXo9uu4TQhxsNbwHc1ZBfLioOPmxxbhs/6mJPmh
+tS6tigHFT8yJr9YmP2NLGaQh22godHYU8soxd8JcvNOo4o6mu8Z/8uKJGH88iun
47jQLkHBsecVLFb4N42b5g9QrT0RYBZnvI0dYnnrmavG2JErR7W1pIixQDubZ4Gn
drROYB4sE3EHo19QLnsdIDJKkARSg+iPVm/BtT7VRF5sy9nxPuHDh/Qzjhhexb3b
CTgB+FgukuX1V3wTi4DQIUhskK5WdX22ugzPN1iG3wIDAQABo1MwUTAdBgNVHQ4E
FgQUIcIsRzIGuupQjNs4z77p/q/kg78wHwYDVR0jBBgwFoAUIcIsRzIGuupQjNs4
z77p/q/kg78wDwYDVR0TAQH/BAUwAwEB/zANBgkqhkiG9w0BAQsFAAOCAQEAqJRm
yUcZNdLRZ2orZZbTPgl39qilNCyMRL3Ao14tYqc85ANvhrlEuXdTLo0MT8/gkhWO
5CE87X0YDayk94MtkWLLsw4A5yzHRUBXdtC3At38JNLwnJYOxf3XY2HsZ/8+uIo8
o5q6ndldmmn/9P7N/J8LHd9EzdIlrK7X8aIJEloXFAcU9xJDK1RNnctImEqICLFC
B61cnmoJYQlsnM2mdjOiC8Ro/PK+CiQyWeW6fWu7/STZd30TXSE0UC8JIBVm3qcd
O6C1hhKjzXUYjX1z8EI56JLRBdIPyFRdBW4sm52N887UgMu4Ahp+UF5GTR8RH9+m
wlh+xQN0H1cmrXNRUQ==
-----END CERTIFICATE-----
    )EOF";
const char *ssid = "Orange_Swiatlowod_B5AC";
const char *password = "cdHqhMotvgMSJ9L4tD";

// MQTT Server details
const char *mqtt_server = "ajkdesktop";
const int mqtt_port = 8883; // Updated to use SSL port
const char *deviceId = "3";
const char *updateFeatureTopic = "updateFeature/3";
const char *updateScheduleTopic = "configSchedule/3";
const char *signalScheduleTopic = "signalSchedule/3";
const char *configDeviceTopic = "configDevice/3";
const char *controlDeviceTopic = "controlDevice/3";
const char *controlOnline = "online:3";

WiFiClientSecure espClient; // Using secure client
// X509List ssl_cert_list(ssl_cert);
PubSubClient client(espClient);

char *prepare_certificate(const char *certs[])
{
  if (!certs)
    return NULL;
  int total_len = 0;
  for (int i = 0; certs[i] != NULL; i++)
  {
    total_len += strlen(certs[i]) + 1; // +1 for '\n'
  }

  char *full_cert = (char *)malloc(total_len + 1); // +1 for final null character
  if (!full_cert)
    return NULL;

  full_cert[0] = '\0'; // Initialize the string
  for (int i = 0; certs[i] != NULL; i++)
  {
    strcat(full_cert, certs[i]);
    if (certs[i][strlen(certs[i]) - 1] != '\n')
    {
      strcat(full_cert, "\n"); // Ensure each part ends with a newline
    }
  }

  return full_cert;
}

char *create_cert_string(const char **cert_array)
{
  if (cert_array == NULL)
    return NULL;

  // Obliczanie całkowitej długości potrzebnej na certyfikat
  size_t total_length = 0;
  for (int i = 0; cert_array[i] != NULL; i++)
  {
    total_length += strlen(cert_array[i]) + 1; // +1 dla '\0'
  }

  // Alokuje pamięć dla całego certyfikatu
  char *cert = (char *)malloc(total_length);
  if (cert == NULL)
    return NULL; // W przypadku, gdy alokacja nie powiedzie się

  // Kopiowanie pierwszego fragmentu
  strcpy(cert, cert_array[0]);

  // Konkatenacja pozostałych fragmentów
  for (int i = 1; cert_array[i] != NULL; i++)
  {
    strcat(cert, cert_array[i]);
  }

  return cert;
}

void setup_wifi()
{
  delay(10);
  Serial.println("Connecting to WiFi...");
  WiFi.begin(ssid, password);

  while (WiFi.status() != WL_CONNECTED)
  {
    delay(500);
    Serial.print(".");
  }

  Serial.println("");
  Serial.println("WiFi connected");
  Serial.println("IP Address: ");
  Serial.println(WiFi.localIP());
}

void reconnect()
{
  while (!client.connected())
  {
    Serial.print("Connecting to MQTT server...");
    if (client.connect(deviceId))
    {
      Serial.println("connected");
      client.subscribe(updateFeatureTopic);
      client.subscribe(updateScheduleTopic);
      client.subscribe(signalScheduleTopic);
      client.subscribe(configDeviceTopic);
      client.subscribe(controlDeviceTopic);
    }
    else
    {
      Serial.print("failed, rc=");
      Serial.print(client.state());
      Serial.println(" try again in 5 seconds");
      delay(5000);
    }
  }
}

void callback(char *topic, byte *payload, unsigned int length)
{
  Serial.print("Message arrived [");
  Serial.print(topic);
  Serial.print("] ");
  for (int i = 0; i < length; i++)
  {
    Serial.print((char)payload[i]);
  }
  Serial.println();
}

void readCertFiles()
{
  if (!SPIFFS.begin(true))
  {
    Serial.println("Wystąpił błąd podczas montowania SPIFFS");
    return;
  }

  // Ładowanie certyfikatu
  File cert = SPIFFS.open("/cert.crt", "r");
  if (!cert)
  {
    Serial.println("Nie udało się otworzyć pliku certyfikatu");
    return;
  }

  size_t certSize = cert.size();
  char *certBuf = new char[certSize];
  if (cert.readBytes(certBuf, certSize) != certSize)
  {
    Serial.println("Błąd odczytu certyfikatu");
    delete[] certBuf;
    cert.close();
    return;
  }
  cert.close();
  espClient.setCertificate(certBuf);
  Serial.println("Certyfikat załadowany");
  // Serial.println(certBuf);
  delete[] certBuf; // Zwolnienie pamięci

  // Ładowanie klucza prywatnego
  File key = SPIFFS.open("/key.key", "r");
  if (!key)
  {
    Serial.println("Nie udało się otworzyć pliku klucza");
    return;
  }

  size_t keySize = key.size();
  char *keyBuf = new char[keySize];
  if (key.readBytes(keyBuf, keySize) != keySize)
  {
    Serial.println("Błąd odczytu klucza prywatnego");
    delete[] keyBuf;
    key.close();
    return;
  }
  key.close();
  espClient.setPrivateKey(keyBuf);
  Serial.println("Klucz prywatny załadowany");
  // Serial.println(keyBuf);
  delete[] keyBuf; // Zwolnienie pamięci

  // Ładowanie rootCA
  File root = SPIFFS.open("/rootCA.crt", "r");
  if (!root)
  {
    Serial.println("Nie udało się otworzyć pliku rootCA");
    return;
  }

  size_t rootSize = root.size();
  char *rootBuf = new char[rootSize];
  if (root.readBytes(rootBuf, rootSize) != rootSize)
  {
    Serial.println("Błąd odczytu rootCA");
    delete[] rootBuf;
    root.close();
    return;
  }
  root.close();
  espClient.setCACert(rootBuf);
  Serial.println("Klucz rootCA załadowany");
  Serial.println(rootBuf);
  delete[] rootBuf; // Zwolnienie pamięci
}

void setup()
{
  Serial.begin(115200);
  setup_wifi();
  // readCertFiles();
  //  Serial.println(prepare_certificate(ssl_cert));
  espClient.setInsecure();
  espClient.setCertificate(ssl_cert);
  espClient.setCACert(root_ca);
  client.setServer(mqtt_server, mqtt_port);
  client.setCallback(callback);
}

void loop()
{
  if (!client.connected())
  {
    reconnect();
  }
  client.loop();
}
