#include <WiFi.h>
#include <WiFiClientSecure.h>
#include <PubSubClient.h>
#include "FS.h"
#include "SPIFFS.h"

static const char cacert[] PROGMEM = R"EOF(
)EOF";

static const char client_cert[] PROGMEM = R"EOF(
)EOF";

static const char client_key[] PROGMEM = R"EOF(

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
  espClient.setCertificate(client_cert);
  espClient.setPrivateKey(client_key);
  espClient.setCACert(cacert);
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
