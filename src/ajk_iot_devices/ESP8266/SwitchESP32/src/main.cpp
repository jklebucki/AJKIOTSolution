#include <WiFi.h>
#include <WiFiClientSecure.h>
#include <PubSubClient.h>
#include "FS.h"
#include "SPIFFS.h"

static const char cacert[] PROGMEM = R"EOF(
-----BEGIN CERTIFICATE-----
MIID3DCCAsSgAwIBAgIUfHt0NwnhDvLDpKhFr8kaFQd0YsQwDQYJKoZIhvcNAQEL
BQAwajELMAkGA1UEBhMCUEwxFTATBgNVBAgMDGRvbG5vc2xhc2tpZTEQMA4GA1UE
BwwHTGVnbmljYTEMMAoGA1UECgwDQUpLMQ8wDQYDVQQLDAZBSksgSVQxEzARBgNV
BAMMCmFqa2Rlc2t0b3AwHhcNMjQwNTE1MTgyMTU4WhcNMzQwNTEzMTgyMTU4WjBq
MQswCQYDVQQGEwJQTDEVMBMGA1UECAwMZG9sbm9zbGFza2llMRAwDgYDVQQHDAdM
ZWduaWNhMQwwCgYDVQQKDANBSksxDzANBgNVBAsMBkFKSyBJVDETMBEGA1UEAwwK
YWprZGVza3RvcDCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAKjeE0xH
vAMdRZL/kGbZ0Curg/Upq0YyzTljGlibaufcSzh1ys0vGOxL4+lolFSNWPZcsC9i
wNqhMQPOH3515HN53cKFTVT5AmQb1kUnBUe36YxYInt6vujfz72K78WhKw8I1G8C
IXHoqBnheFd98PtZAI7do32WQMHVkn0KcD1JGd97T2SSmlwv74q3q8aAwv3/NXA8
DzS80mdMHrKjwmSCH1+aAKUPMdv4pM9sIYt2KkP0v7BfFy40hXVDAdCzwOn57ObL
Bienbd2v6avGGHzuSu5uBm7UEDpF6OGayJNzYoS81xWvda0dzbPX4hN5BDDqzjJU
VzxC/gbsEh0txUsCAwEAAaN6MHgwHQYDVR0OBBYEFOSPyl45gyLJJzw0epi6HuBN
7wMwMB8GA1UdIwQYMBaAFOSPyl45gyLJJzw0epi6HuBN7wMwMA8GA1UdEwEB/wQF
MAMBAf8wDgYDVR0PAQH/BAQDAgEGMBUGA1UdEQQOMAyCCmFqa2Rlc2t0b3AwDQYJ
KoZIhvcNAQELBQADggEBAA69uaqOiw7ahb7fbZKpbB0MJUYWNR+XywbaB3ubhChl
5J2gZ7XzxlxqmLd1JFFgWBxZ5ilTiGX820GAeAm3n5y0hyzclBoHu/TfRyVqBump
fzQTdD5jgWFMzRRUCyxMrNdE+zKcLOD4kJITddptRQwJ85aT3DJbteJo2N9nd9qT
Kr/gQOd1niBGc155RA0VMP6N8vmYNszwCXIZ+i30iSrSWudRht10xofUJa60di8M
9cbO0vFnlitqh2SJOtaBi9KjODfaSOjMkJurgUTSmyWHrOobSb+fZrMUNTiNPmwj
fBY4K3jpCDLURXHBYIGQi/JJlRChU37gXyBeMfT/IkE=
-----END CERTIFICATE-----
)EOF";

static const char client_cert[] PROGMEM = R"EOF(
-----BEGIN CERTIFICATE-----
MIID9DCCAtygAwIBAgIUY5YdyDRD2MoPctIHH+3oNlGNU0gwDQYJKoZIhvcNAQEL
BQAwajELMAkGA1UEBhMCUEwxFTATBgNVBAgMDGRvbG5vc2xhc2tpZTEQMA4GA1UE
BwwHTGVnbmljYTEMMAoGA1UECgwDQUpLMQ8wDQYDVQQLDAZBSksgSVQxEzARBgNV
BAMMCmFqa2Rlc2t0b3AwHhcNMjQwNTE1MTgyMTU4WhcNMjUwNTE1MTgyMTU4WjBq
MQswCQYDVQQGEwJQTDEVMBMGA1UECAwMZG9sbm9zbGFza2llMRAwDgYDVQQHDAdM
ZWduaWNhMQwwCgYDVQQKDANBSksxDzANBgNVBAsMBkFKSyBJVDETMBEGA1UEAwwK
YWprZGVza3RvcDCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBANsUD31Z
hV2fNFDVn4kEGZCspb1J5NZUhHaGBjhzR1J98aKkGfyvyiM+eQhv/M/BhW+RTc4C
RCKwJhsPnIjLvkqW3Pi+E/zguEfKyIC3QZPSslmuovjk1318etuhqg7KBGWJQprL
CNur9ZLLW+/5OfuaNCvnhEvuW6nbDkpc/MAOJ2ih71qAzoYEoEbtlNO5RkqSnnom
soSNtxVMfPtqMtlpBsMCDUwLRhku3n8Wr6839EwIbbaaFEnE64Ri9pn8hSiNEvG4
ArWp4obwiRcYlIf42oO3O76VHPEF0trZqxwscT8H/FZVq7UxAZRdeSHTxwBQARIv
QOITBma36gOXcjUCAwEAAaOBkTCBjjAdBgNVHQ4EFgQULzx2g41lgzCxyL8966u8
xFF15egwHwYDVR0jBBgwFoAU5I/KXjmDIsknPDR6mLoe4E3vAzAwCQYDVR0TBAIw
ADALBgNVHQ8EBAMCBeAwHQYDVR0lBBYwFAYIKwYBBQUHAwEGCCsGAQUFBwMCMBUG
A1UdEQQOMAyCCmFqa2Rlc2t0b3AwDQYJKoZIhvcNAQELBQADggEBAKC64oCCN7Js
2ovqHQGxw3T3/HADdJC7wI1SLm67HBqKjNvRtU5AfY50uaik7avSmwdPftT4J3wi
1ZrPEKqj7fLAhoEiNX51YxiTFX8BAwYkxaJzCWSTO5uamTPcLfVC5OjKq6EtHAR6
nlJ9tH6mBEUJUcR0xcKrF+Hmxf0GQu1WF0jaq1FbSX4lSrP7pm7mZvJblq+mYzBS
NkePqZvO+V5AZUXTtNeYYTdxgwpuwHS4RXHwBtYLu4rMQsIzS7EHP/4kMIczVYt/
r9ize7FUf7OQzRFt/p8vTDr5D9NLFjvBC8B0wSy7NeOb8fQcLUJJHy3/FqjbMPEd
aM0sHODcxAI=
-----END CERTIFICATE-----
)EOF";

static const char client_key[] PROGMEM = R"EOF(
-----BEGIN PRIVATE KEY-----
MIIEvAIBADANBgkqhkiG9w0BAQEFAASCBKYwggSiAgEAAoIBAQDbFA99WYVdnzRQ
1Z+JBBmQrKW9SeTWVIR2hgY4c0dSffGipBn8r8ojPnkIb/zPwYVvkU3OAkQisCYb
D5yIy75Kltz4vhP84LhHysiAt0GT0rJZrqL45Nd9fHrboaoOygRliUKaywjbq/WS
y1vv+Tn7mjQr54RL7lup2w5KXPzADidooe9agM6GBKBG7ZTTuUZKkp56JrKEjbcV
THz7ajLZaQbDAg1MC0YZLt5/Fq+vN/RMCG22mhRJxOuEYvaZ/IUojRLxuAK1qeKG
8IkXGJSH+NqDtzu+lRzxBdLa2ascLHE/B/xWVau1MQGUXXkh08cAUAESL0DiEwZm
t+oDl3I1AgMBAAECggEAAmpMlV1qsxNcLwqkFNC9X8gWbniTzf9duL/lAmzCMOq+
7mxKYaHR7ETIpDRRBJRV0pDHwHr5GYLajsxDpOgsitld9f7b1V51waNI54TLKXUv
rpHTCLAHYwwoGm3zYLrcnFzZ6R128HGR/5TeICX1XceQchM0CT7YLIRUxHzsxHgK
4N3znl4t9RM8kFxgNAJZicUirafx/xDC4jDL2kxzytlSIRQMfP9DqxS0JPni3Tju
C3qQdzgmw1f8ob1xz/CQDGIcr0wnZGmrwoZqy/4FIcQ3Hba6ujl76Dh03Q2RlNxM
Z5C9EL3YxuulKlo5r8CouFzTLC0ehxg57j3uIHtQmQKBgQD7fqtAS5s1HpR2CawL
88Th1kRFc9+Xq7KBtl1yd5Hai5lLR1YBcSRa/RkopR9W9zL9O19gwmXbMIq9wa3M
2w9vosKYwBV7sfWntS70/WvbMgO8Wur56DBy9qAriSro6uWbKrPIdzkLT5org3xL
UjoBE3pSoZdIwsWGwSKzUpPZKQKBgQDfALudlohappLJpfIol5bwInsVtTiHScPF
hWNh1I3pD/TqZWmr/t5++eHoyBqtMKdtm5bYno6lVdkztVGtygnIkOZYtJzjlHet
kC1V/Jn9Wh+j8JmQVKuVam73djEz6qa65qtDRKQrkp6y29JfUPCWFGsj5DU84drf
SD2/f8LWLQKBgGxJ930g0PzHZQ+KTJ184kqH0y/AnAcXtC4sEwKlv5TFxUTnu321
dP0EvB+HMf5lRHxLY18rdWYy+ZqJWce6j2P3Rik1lEqFrwv+dPCiah2g5IDm+QSQ
WMQ3s3HrhyFxe1obwfvQciyPoweXfx6DklxCQcpwr85xp1HFuaZTKTw5AoGAWvpg
kHUmYd1NS2khN0BI+uUGVB/f1QnaDc5SckoWLzwsTihbiSjsut7VNHPWtkuAMu5k
BzIAviEdAMR7cyxW+3VqFExzKUGb5bpJVKg+ZqcK+4YgEEsKyWVnqhuVDToxFVUg
D8yEMaaaihu2Yt6RmJjx8O2cbp8x/R/q2SKVaW0CgYB4EYA1cvhY9czZEHeUrEmw
2MccPGJdCJTRtz/UuCIk4C3mBzmtnhLns/9ixthkSFilT6ss4s5U+uXF76+IFi/j
Y7alxzZPiz7QNgV6YMS5N5ZyLy+Idh8BAx13MOAYEHWmbHSY6ACJ3XMEduJgiXHr
TaacdIgJ8LADsLa2wK83AQ==
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
