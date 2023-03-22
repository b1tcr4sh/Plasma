#include "WiFi.h"
#include "WiFiClient.h"
#include "RateMonitor.h"

#define SSID "YOUR_SSID"
#define PASS "YOUR_PASS"
#define SERVER_IP "SERVER_ADDRESS"

WiFiClient client;

void setup() {
  Serial.begin(115200);

  RateMonitor sensor = RateMonitor();
  if (!sensor) {
    Serial.println("Couldn't connect to MAX30105");
  }

  wifi_connect();

  IPAddress server_address = IPAddress(192, 168, 1, 118);

  client = WiFiClient();
  int socket_connection = 0;

  while (!socket_connection) {
    Serial.println("Trying to connect to tcp socket");
    socket_connection = client.connect(server_address, 3012);
  }
  
  Serial.println("Connected!");
}

void loop() {
  if (WiFi.status() != WL_CONNECTED) {
    wifi_connect();
  }
  
  client.write("uwu\0");
}

void wifi_connect() {
  Serial.print("Waiting for wifi");

    WiFi.mode(WIFI_STA);
    WiFi.disconnect();
    delay(100);

    
    WiFi.begin(SSID, PASS);
    
    int time = 0;
    while (WiFi.status() != WL_CONNECTED && time < 60) {
      Serial.print('.');
      delay(1000);
      time++;
    }
}