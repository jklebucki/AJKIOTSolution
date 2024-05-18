#include <WiFi.h>
#include <WiFiClientSecure.h>
#include <PubSubClient.h>
#include "FS.h"
#include "SPIFFS.h"

static const char cacert[] PROGMEM = R"EOF(
-----BEGIN CERTIFICATE-----
MIIDuzCCAqOgAwIBAgIUUpgos+QK4YkAuxL178EeFH5huLEwDQYJKoZIhvcNAQEL
BQAwbTEWMBQGA1UEAwwNYWprZGVza3RvcCBDQTEPMA0GA1UECwwGQUpLIElUMQww
CgYDVQQKDANBSksxEDAOBgNVBAcMB0xlZ25pY2ExFTATBgNVBAgMDGRvbG5vc2xh
c2tpZTELMAkGA1UEBhMCUEwwHhcNMjQwNTE4MTc0MTE5WhcNMjUwNTE4MTc0MTE5
WjBtMRYwFAYDVQQDDA1hamtkZXNrdG9wIENBMQ8wDQYDVQQLDAZBSksgSVQxDDAK
BgNVBAoMA0FKSzEQMA4GA1UEBwwHTGVnbmljYTEVMBMGA1UECAwMZG9sbm9zbGFz
a2llMQswCQYDVQQGEwJQTDCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEB
AK5A8FxKiLvColj6iWWa1yCV290PGpwRD+Z+tWZhm2YURXDoxipZdSMjqTV0Io2a
Vy53i3JHcuLx3t/IwBzHerCNMzU9wQiuoMQ20mEY+hitMVfBo1kGJF4+g/HQ37i7
M/aqOf3fF6Qs4l8Yu7qRfJYev/vnwhIiY8rjmHFZwxVUI+MD1rBR75zhRT8UBCmN
aoupxj4y+advkB2UxF7MJPb6dbJ7opkmGVJH97JN9Wm26KTWNXRvAZePGiqlZDfW
W2GjPiyEh3mfFI5eHHloeEWYyV6ftaAj4kZQtUd64HGCavvmnsE7LStwLvs4mHhY
+prKDQvoA3ywyq7boCiBFtMCAwEAAaNTMFEwHQYDVR0OBBYEFBn+qZPwxqxk49FG
H0kiHBoMM+WnMB8GA1UdIwQYMBaAFBn+qZPwxqxk49FGH0kiHBoMM+WnMA8GA1Ud
EwEB/wQFMAMBAf8wDQYJKoZIhvcNAQELBQADggEBABRxZ7HRORFXmQVM2LIcn6uh
LrTA5UjMA3L5uwJN0ekvCr6hTGto6SXnw2HU+d7UfoWK6mmuLyozrc5z0kprjbcF
/M7Uf5a/YRCmQMwKTli7OivdmUDhJkZnfvQ5sbYpZgfXUaHS0DsQ/r+wKzVtD6H4
jBHHGn/V7mNZt70NmuomgARAioRcefxQOtrT5O46iHV7ok+sTUGo7qYleOcQZNgJ
ypgSYMneZI3M0PgOdFslLFlbhkW2rcXB2PLlwNst7y9nH3okTx5JxMn8LNNnGK3V
r0O3yNY40sBkw2O+uSk86+noGsn9FuH5aKZYsU3Jm3TR71rDcyjXZ+g4+ew6zSw=
-----END CERTIFICATE-----
)EOF";

static const char client_cert[] PROGMEM = R"EOF(
-----BEGIN CERTIFICATE-----
MIIDXTCCAkUCFHfSFCXN6H6gsTlARCDOyJYGWyPQMA0GCSqGSIb3DQEBCwUAMG0x
FjAUBgNVBAMMDWFqa2Rlc2t0b3AgQ0ExDzANBgNVBAsMBkFKSyBJVDEMMAoGA1UE
CgwDQUpLMRAwDgYDVQQHDAdMZWduaWNhMRUwEwYDVQQIDAxkb2xub3NsYXNraWUx
CzAJBgNVBAYTAlBMMB4XDTI0MDUxODE3NDEyMFoXDTI1MDUxODE3NDEyMFowaTES
MBAGA1UEAwwJSW9URGV2aWNlMQ8wDQYDVQQLDAZBSksgSVQxDDAKBgNVBAoMA0FK
SzEQMA4GA1UEBwwHTGVnbmljYTEVMBMGA1UECAwMZG9sbm9zbGFza2llMQswCQYD
VQQGEwJQTDCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBALUJzcZnvZz2
Lj0G6axTzDoOR4/8Ntf38iv9rLX5iEsEm6W+rUA2JaP/AwBIJf/QUZ5ABIp/PxPm
uU6jyIc4H++85NUaavV78cL9JeeeqtDasACELGabPLMBkXYWWjnAsswVAtCIDyIi
dJV9i/AzibD37hb3BdMWzeHhP46qqtVSQ5vI5bWuMWLYbA7R3S246v3arjxbdRST
RQF4VtVPyzgImwNzfpvDzBSe9ogjIBkaPNpUnYmyq3FzlJfVFMC98lhOrIy6gZPJ
/leRI/6xENz7uEao1I9+/DHeNHdfiM8v1mrRuS9ZwN+cjEg1y/L9XkYY1r1gnBta
0sQLQCJdUs0CAwEAATANBgkqhkiG9w0BAQsFAAOCAQEAUKRZfk8cHJ02bZWueSGr
8sSlOK+w4zpPPYebyVojpXbFh9czutl30xvguiQrjIEci7zd0sxgx9fQBRMzutHR
puiJpbk009H2d0UBDGsF4h6yj7rM02CbuP9S5G+cdTDDklKRZVr/s2r5VE1yWBhm
XKDVnMKbT5wbc31Tf/dAt9rTCbGzTzsRSv4KEeyF1hHOzsyhJ1tb98VDhw3NZ/JH
TZHpkJl0xyx+CGhPtAfMu3NMBCMmKAcEXRU80WZSxE1WRz2kO012R7flV0ry30O2
/pRalBVg8msIJZk/riVC+9xYxNFlihamXiZlBH6PLQVfHXewF3GHug35Sw2AVmzX
YQ==
-----END CERTIFICATE-----
)EOF";

static const char client_key[] PROGMEM = R"EOF(
-----BEGIN PRIVATE KEY-----
MIIEvAIBADANBgkqhkiG9w0BAQEFAASCBKYwggSiAgEAAoIBAQC1Cc3GZ72c9i49
BumsU8w6DkeP/DbX9/Ir/ay1+YhLBJulvq1ANiWj/wMASCX/0FGeQASKfz8T5rlO
o8iHOB/vvOTVGmr1e/HC/SXnnqrQ2rAAhCxmmzyzAZF2Flo5wLLMFQLQiA8iInSV
fYvwM4mw9+4W9wXTFs3h4T+OqqrVUkObyOW1rjFi2GwO0d0tuOr92q48W3UUk0UB
eFbVT8s4CJsDc36bw8wUnvaIIyAZGjzaVJ2Jsqtxc5SX1RTAvfJYTqyMuoGTyf5X
kSP+sRDc+7hGqNSPfvwx3jR3X4jPL9Zq0bkvWcDfnIxINcvy/V5GGNa9YJwbWtLE
C0AiXVLNAgMBAAECggEAQjpm6+kxpYUt3ydzPpadRLPKnDLxQUq0bkFr+Fpj8MWr
xxOdP7tPQ9Jbn7PFKvNjmFflEWGBit9B9THXfxgaHtLkh80VSd7gz6taPYR/Cs6l
ylqP/61UpEuzkhaVRUFoZ15cXDsaBivCqJl4IxRHj9TzZbVSjSlvge2sGZ33xvem
uZ7v37IOVbwlcj9cdMk0FF3rdKJRMXuufCH54x1MXdW80LtVbccdqyec9TA0YPN9
tVwmI8yZX1EELpLMrQ8zw5XPCcolsLtMrG/3UWXZTGEMuKNafYK1wprirvQWoaSx
sQep1t2L1RZWxcR7G+gIxSQ8gRbLzY4QaamSVndhpQKBgQDTzvnCUv4znnrrRuOF
xdNIDRWGbmRSH8ljOCQo4McvHw63gBMi0YKIw+wLDj6CSnjoyhS5dB+nCl0FEt6s
d1mnnE9VnfeyAATyVpj43g79odCH5ly6/kMEHMofNPx/vaglcoX30F1BR2DMkwtt
vPEy1Gq7MaI6pybgUCqe4q/85wKBgQDaz1fnnXrpYqiIsGN4IbctydQIEy5lXQ79
LUzPxrEgd/xplBiiVkcT3Dj6cBbOgcMd1SOL+MH57b3YHz8AxFvF33aB+qdwRMES
B2P+0knTkVWyOK6O8yBHVxeiuye0EMH9CprC/8SzE4GzV3cW2OoRfAbde6ue9wSX
Z/NWXXBoKwKBgGOSRHWnAFuR4CUk4SbtFeMkS38z/DNjQBBFvzH8YYb0ab24Fsbi
iSP0Ps3/t0EW83o0LcP1JEAprgsJkOaxANO7tswABAaI3cpzDVzJP3DaliadE/DQ
QP747cf358/Bf/+CtBoIuR5MCOSDJ/dBwH3tv/MaZTJ/i9YdubuRw7v3AoGAc1YR
3uuaq0Su04YumFclSER3uF3r+dAoo3lqYKc6HIRCj6BZr9BMnQJbIl9NFkM+Bw6f
MxvHm6ceh7pIqm3WdiHJRNBLzBjhsFAm/F36PkQAaPYJxR4QqKoWsld2oSqoJmqd
kyXgmAgzOMZk5q0mDFtU/xA+MYfBatGHacHNC4sCgYAPgl2/IT860poye+6kr7oi
1D+8aBML7HDgYbzJs5llPIm9HO+6X5ekcfIgnTnAt6AZnBTN3aeuIf7j6xM/WQK2
EypWWh0VYSzeynsO+DezLUMaWAlC5IRnCd1wdlC62I0Zg4GDPO9JimaLyii1zgXU
mS8BVDuJsq6/AriqbXjBHg==
-----END PRIVATE KEY-----
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
  File root = SPIFFS.open("/ca.crt", "r");
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
