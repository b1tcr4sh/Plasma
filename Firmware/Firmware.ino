#include "WiFi.h"
#include "WiFiClient.h"
#include <Wire.h>
#include "MAX30105.h"
#include "heartRate.h"

#define SSID "NOLA"
#define PASS "RyanMelody14"
// #define SERVER_IP "SERVER_ADDRESS"

WiFiClient client;
MAX30105 Sensor;

void setup() {
  Serial.begin(115200);

  wifi_connect();
  IPAddress server_address = IPAddress(192, 168, 1, 118);
  client = WiFiClient();
  int socket_connection = 0;

  while (!socket_connection) {
    Serial.println("Trying to connect to tcp socket");
    socket_connection = client.connect(server_address, 3012);
  }
  
  Serial.println("Connected!");

  Sensor.begin(Wire, I2C_SPEED_FAST);
  Sensor.setup(); 
  Sensor.setPulseAmplitudeRed(0x0A); 
  Sensor.setPulseAmplitudeGreen(0);
}

void loop() {
  if (WiFi.status() != WL_CONNECTED) {
    delay(100);
  }

  char buffer[3];

  int bpm = sample(4);
  itoa(bpm, buffer, 10);
  client.write(buffer);
}

void wifi_connect() {
  Serial.print("Waiting for wifi");

    WiFi.mode(WIFI_STA);
    WiFi.disconnect();
    delay(100);

    
    WiFi.begin(SSID, PASS);
    while (WiFi.status() != WL_CONNECTED) {
      Serial.print('.');
      delay(1000);
    }
}

int sample(int size) {
  float sum = 0;
  long lastBeat = 0;

  for (int i = 0; i < size;) {
    long irValue = Sensor.getIR();

    if (checkForBeat(irValue)) {
      long delta = millis() - lastBeat;
      lastBeat = millis();

      float bpm = 60 / (delta / 1000.0);

      if (bpm < 255 && bpm > 20) {
        Serial.println(bpm);
        sum += bpm;
        i++;
      }
    }
  }

  return (int) sum / size;
}