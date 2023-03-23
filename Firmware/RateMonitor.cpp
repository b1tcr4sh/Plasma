#include <Wire.h>
#include "MAX30105.h"
#include "heartRate.h"
#include "RateMonitor.h"

RateMonitor::RateMonitor() {
  Sensor.begin(Wire, I2C_SPEED_FAST);

  Sensor.setup(); 
  Sensor.setPulseAmplitudeRed(0x0A); 
  Sensor.setPulseAmplitudeGreen(0);
}
float RateMonitor::sample(int size) {
  float sum = 0;
  long lastBeat = 0;

  for (int i = 0; i < size;) {
    long irValue = Sensor.getIR();

    if (checkForBeat(irValue)) {
      long delta = millis() - lastBeat;
      lastBeat = millis();

      float bpm = 60 / (delta / 1000.0);

      if (bpm < 255 && bpm > 20) {
        sum += bpm;
        i++;
      }
    }
  }

  return sum / size;
}