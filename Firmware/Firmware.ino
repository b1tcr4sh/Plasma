#include "WiFi.h"
#include "WiFiClient.h"

#define SSID "NOLA"
#define PASS "RyanMelody14"
#define SERVER_IP "192.158.1.118"

WiFiClient client;

void setup() {
    Serial.begin(115200);
    Serial.print("Waiting for wifi");

    WiFi.mode(WIFI_STA);
    WiFi.disconnect();
    delay(100);

    
    WiFi.begin(SSID, PASS);
    while (WiFi.status() != WL_CONNECTED) {
    Serial.print('.');
    delay(1000);
  }
  Serial.println("Connected!");

  IPAddress server_address = IPAddress(192, 168, 1, 118);

  client = WiFiClient();
  int connection_status = 0;

  while (!connection_status) {
    Serial.println("Trying to connect to tcp socket");
    connection_status = client.connect(server_address, 3012);
  }
  
  Serial.println("Connected!");
}

void loop() {
  while (WiFi.status() != WL_CONNECTED) {
    delay(1000);
  }  

  client.write("uwu\0");
}
