#include <WiFi.h>
#include <WiFiClientSecure.h>
#include <PubSubClient.h>
#include "FS.h"
#include "SPIFFS.h"

const char *ssl_cert[] = {
    "-----BEGIN CERTIFICATE-----",
    "MIIEBjCCAu6gAwIBAgIUHKqm0okjo/5hgxUTTYwaLqv2ea8wDQYJKoZIhvcNAQEL",
    "BQAwfTELMAkGA1UEBhMCUEwxFDASBgNVBAgMC01hem93aWVja2llMQ8wDQYDVQQH",
    "DAZXYXJzYXcxFDASBgNVBAoMC0FKS1NvZnR3YXJlMQswCQYDVQQLDAJDQTEkMCIG",
    "A1UEAwwbQUpLU29mdHdhcmUgSW50ZXJtZWRpYXRlIENBMB4XDTI0MDUxMTIyNTA0",
    "MVoXDTM0MDUwOTIyNTA0MVowcjELMAkGA1UEBhMCUEwxFDASBgNVBAgMC01hem93",
    "aWVja2llMQ8wDQYDVQQHDAZXYXJzYXcxFDASBgNVBAoMC0FKS1NvZnR3YXJlMQsw",
    "CQYDVQQLDAJJVDEZMBcGA1UEAwwQKi5hamtzb2Z0d2FyZS5wbDCCASIwDQYJKoZI",
    "hvcNAQEBBQADggEPADCCAQoCggEBAKHqo5EH5zL8yUEbZ8R9EVI5fQrlDfmz6j7E",
    "8kgEl2lTy6/qYh1EWW5WAGEke4Q6RQpGTeSK6l8nSMdp2vWnG1+kxvvdxbrq71Ra",
    "BbtoufCBtuHdhPkrHeqvQRr/lYkjsPtejS3ZH2o7AHkjJu82U6jLynED++z6uTXh",
    "e9qjs58BKxs9RB0WkcqJ9Jj1ln9prY+nEnBoIUtRS22eqsBjd/o5JKsYjdL7HTHe",
    "jHch0nVdqv4FaIzJGAohd39ryQD9oDU/B7vLs9hYs1S80P3UZAJxOsCQQ7JzhM/p",
    "xlftSZLleFMTWQbIF4bfhaxBr3poszmKjkuizSEoCM08A0jWa+8CAwEAAaOBiDCB",
    "hTAfBgNVHSMEGDAWgBQ/b1Wcu6rAlcbcyvtntZuj7QoKqzAJBgNVHRMEAjAAMAsG",
    "A1UdDwQEAwIF4DArBgNVHREEJDAighAqLmFqa3NvZnR3YXJlLnBsgg5hamtzb2Z0",
    "d2FyZS5wbDAdBgNVHQ4EFgQU05v8GKQo19MT2s3QoWp4g5ioLCIwDQYJKoZIhvcN",
    "AQELBQADggEBABVWo94l9JeSBm/IvjP4nGghOWvXBtE0mDf5L0mefWrTfeekmOM0",
    "xt1Tbt8zJWM3CzSDgomduC+gmFOQakiQ7fAibjnmfRkPAeUCwBIDD5Wxt14nPzP4",
    "vh0JwoJxiCtK7LOStSujNyp+QqIYHpPHae3FkAuX85JHOZJNISnYLh6c+Yt4tpvF",
    "iiHBnpC8q04BX482/ThbOKmlJHDTJSJVgZd0JKQKY83CZOARpJEzdqO/FiQjBHOq",
    "yVH+gYQE9HlrvnUu9v1k2nT3JYzFllf+uN+/0sqd45D8pkVaoxas1Vk/C0BTBWJH",
    "hvyy7NmO/YDkoiujPZAcXPt2uVQ/DZV+aMo=",
    "-----END CERTIFICATE-----",
    "-----BEGIN CERTIFICATE-----",
    "MIIEXTCCA0WgAwIBAgIUepIla6WFVvv9APQCTpGVvzky3dEwDQYJKoZIhvcNAQEL",
    "BQAwdTELMAkGA1UEBhMCUEwxFDASBgNVBAgMC01hem93aWVja2llMQ8wDQYDVQQH",
    "DAZXYXJzYXcxFDASBgNVBAoMC0FKS1NvZnR3YXJlMQswCQYDVQQLDAJDQTEcMBoG",
    "A1UEAwwTQUpLU29mdHdhcmUgUm9vdCBDQTAeFw0yNDA1MTEyMjUwNDFaFw0zNDA1",
    "MDkyMjUwNDFaMH0xCzAJBgNVBAYTAlBMMRQwEgYDVQQIDAtNYXpvd2llY2tpZTEP",
    "MA0GA1UEBwwGV2Fyc2F3MRQwEgYDVQQKDAtBSktTb2Z0d2FyZTELMAkGA1UECwwC",
    "Q0ExJDAiBgNVBAMMG0FKS1NvZnR3YXJlIEludGVybWVkaWF0ZSBDQTCCASIwDQYJ",
    "KoZIhvcNAQEBBQADggEPADCCAQoCggEBAJg/TJWIL4EbH0TbRHW3c5Ln3R0qavPZ",
    "d/MUIPvfc5fYWJRAadx58eASS/1caJfINlVkPoqbEP1iM+zvyCacq90usLSHUPQ9",
    "d9lHSejwdVjrhOPh7n+MP6SlCW8FqK6f3xVq9joTXjtqFRUGAdbWwFyTX3PRz+d4",
    "Xe9vh8DzPvI2IC8YoY4OJPdHX9iaZDjTfHUfAnQkR1+Sj/TfNayaL6Icqqqh7JQe",
    "FkD7AROjwuT3cUd1kh3vKp6zNJVL6h6uK2GD5C5ZFxv9lM1jEKapuHftCY09eEqr",
    "N2mOkRDb+oTZF9AqSS9C0rU+VQ6qGnAnO3oOHrmruSwm0LOze3d/INUCAwEAAaOB",
    "3DCB2TCBnAYDVR0jBIGUMIGRoXmkdzB1MQswCQYDVQQGEwJQTDEUMBIGA1UECAwL",
    "TWF6b3dpZWNraWUxDzANBgNVBAcMBldhcnNhdzEUMBIGA1UECgwLQUpLU29mdHdh",
    "cmUxCzAJBgNVBAsMAkNBMRwwGgYDVQQDDBNBSktTb2Z0d2FyZSBSb290IENBghRZ",
    "K6JrBsJuZ3oXJWhOiomszDb3AzAMBgNVHRMEBTADAQH/MAsGA1UdDwQEAwIB5jAd",
    "BgNVHQ4EFgQUP29VnLuqwJXG3Mr7Z7Wbo+0KCqswDQYJKoZIhvcNAQELBQADggEB",
    "AJQd8REiYKAassyv2A+bBTUqefwVDsU07mYkmQqm+yWuVRO/VEJyHEP20yZ8fMBm",
    "xuXovks+12zLzmgeSx3f8l63sWjH+0oaZXndc0A1E7y9oBVYDNC/ZBskErIZhtIX",
    "0P6SdWqFqumAgxOta9ZmasKJYNz+LwKNsipI4F1SCKetmnvypp1jBqPR37GNKhiw",
    "GWjviliPgO2qRcRdnlG5DeIi3eu0UEd98+KEI9WYx2KNdNaQ9STElxYSG5E77DB1",
    "mQqhd+dfpdPCnVUbDKb7H3ikrcjrD2zA9dfmn6HyjJ/UPTM41prkEq6f/8tUTifj",
    "rjbk9UKvy8OfF3GCFj0LrcA=",
    "-----END CERTIFICATE-----",
    "-----BEGIN CERTIFICATE-----",
    "MIIDcTCCAlkCFFkromsGwm5nehclaE6KiazMNvcDMA0GCSqGSIb3DQEBCwUAMHUx",
    "CzAJBgNVBAYTAlBMMRQwEgYDVQQIDAtNYXpvd2llY2tpZTEPMA0GA1UEBwwGV2Fy",
    "c2F3MRQwEgYDVQQKDAtBSktTb2Z0d2FyZTELMAkGA1UECwwCQ0ExHDAaBgNVBAMM",
    "E0FKS1NvZnR3YXJlIFJvb3QgQ0EwHhcNMjQwNTExMjI1MDQwWhcNMzQwNTA5MjI1",
    "MDQwWjB1MQswCQYDVQQGEwJQTDEUMBIGA1UECAwLTWF6b3dpZWNraWUxDzANBgNV",
    "BAcMBldhcnNhdzEUMBIGA1UECgwLQUpLU29mdHdhcmUxCzAJBgNVBAsMAkNBMRww",
    "GgYDVQQDDBNBSktTb2Z0d2FyZSBSb290IENBMIIBIjANBgkqhkiG9w0BAQEFAAOC",
    "AQ8AMIIBCgKCAQEAol17Gddmlmk17Qirj6K28K5n+NfLbHC7vsud87V2eW42lhId",
    "n4g01KqbNWQMQDAjvoQK0DRf/aArNiDrjq2KRJI2pAACY+UKQm9WSiQ7tHSavw6H",
    "RZSWRmelpNjaXOKxioJiOfNDATtFkxePVfjJWFyOi/jkreh6/7rURScW7cAizGod",
    "FUek4aC7IIFJhHB3cnAQ6YXM28kB8xs18LIEvVYoO+oUbCpQ3GoWVT2ymR9i9aac",
    "ErK8JABFa5RBJ1RI7jhlXSMtmZOtNyMPpmAWOF2nBvbJ5DjhFZiKICtbhKvuF/2U",
    "zNzyMS/zDSdZC3UlFpbCeXyX6vbm/Ya+wxuD7wIDAQABMA0GCSqGSIb3DQEBCwUA",
    "A4IBAQA8WVsH6mUafnvv/uqU/VoSzd2959HEhY9BoHdrkEkuJjW1AlNlVJDaLEUQ",
    "1DRPRA8HmEEH4P1dgMhqh/GueCIR/OSEmUNYqpk0/cylo0woO2R5cCV51vk4mWi+",
    "HRvlFZjExiDe2m2Pc4UUULvzy0Svy8RSbw0Ai9d5cxdwBEuvOJgPWZa2cmOlKf6Y",
    "Dc7TV58GpuClrtuZTrm5+vAdsp0+Xgl40yoBITWzHqCZEVnsvV4JNahBzQPCswgA",
    "CkXWA8zqbyEi6YKs9sLjfWFa/SKHmgCpoHcnnNmMIKfakjCoMsUJoG2Oj59+eDV2",
    "JbyTm0cHNRqOQT65Tu4g4c0gJVgL",
    "-----END CERTIFICATE-----",
    NULL};

static const char *root_ca PROGMEM = R"EOF(
-----BEGIN CERTIFICATE-----
MIIDZTCCAk2gAwIBAgIUaMQE5lFlVtAmtpvof98tAOJcJaswDQYJKoZIhvcNAQEL
BQAwQjELMAkGA1UEBhMCUEwxFTATBgNVBAgMDGRvbG5vc2xhc2tpZTEOMAwGA1UE
BwwFTHViYW4xDDAKBgNVBAoMA0FKSzAeFw0yNDA1MTIyMTMxMDFaFw0yNzAzMDIy
MTMxMDFaMEIxCzAJBgNVBAYTAlBMMRUwEwYDVQQIDAxkb2xub3NsYXNraWUxDjAM
BgNVBAcMBUx1YmFuMQwwCgYDVQQKDANBSkswggEiMA0GCSqGSIb3DQEBAQUAA4IB
DwAwggEKAoIBAQCdEz/v1Ivrwnx38BGz77pm2rdcZkdHGXaqyYAUb2WRfz45hyAo
Bflfa+pDymzhsTGG9dciDDrDRZc+cd7LTHny3FTqP9uxsbmvXBSMJ2cJdV3bH1F/
QJQEAnt9Nowd7/6G/eqrUV5DzzsbvXVqQDeq041lpItA5RCPuxWNFoFYrZJtuJs+
UxymZL0Ofbtoi+iLtBUiu9uD7uS5TohOAXOX/1YI+hEyzRexOhk7U8heIDxwyY2+
aSlx6a8cawi38WqdGsbQ2Z9t8zil7FOG/CGmZmadRRfZed4R2dztwszfUFFSdttZ
YsyOj6+dAi5JMHJvuvl5YgPhIw1C7z1mI2GZAgMBAAGjUzBRMB0GA1UdDgQWBBRR
/rH4V5KjdLGwU2+nn26PAun56zAfBgNVHSMEGDAWgBRR/rH4V5KjdLGwU2+nn26P
Aun56zAPBgNVHRMBAf8EBTADAQH/MA0GCSqGSIb3DQEBCwUAA4IBAQB5DewAU5Hi
cz54WDHFAMkRuvEOhrnvo2XxFeOq9hEuAo4iTMNSH736oRfdQ8+avHTHphmuA8rJ
piPoBaJbSMCeUeVTdI9AAZrBcD/C/kKUpzszEbmhbYT1D60awWl5bKmcz4ZLuXLT
b2DiMWhvIRIig0BaYdxlL+Rl21kiH1kc2VLe9G5G0+nCKs5wANgte81T6dFjzi9T
qt502tNWCF3p16D566tpi2lS6cPFXzSWmaEoB9UPiLllIjZSm+2j533EbDCmrIVz
kD53jCOtKnvNpln/mpwrBEnb+Fi31mLvl75WZRaMCJn7UNF9kWx/Rl/2fFqCV4V2
jkyvJiz4H2gP
-----END CERTIFICATE-----)EOF";
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
  // espClient.setCACert(root_ca);
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
