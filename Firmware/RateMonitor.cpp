#include <Wire.h>
#include "MAX30105.h"
#include "heartRate.h"
#include "RateMonitor.h"

RateMonitor::RateMonitor(int samples) {
  Sensor.begin(Wire, I2C_SPEED_FAST);

  Sensor.setup(); 
  Sensor.setPulseAmplitudeRed(0x0A); 
  Sensor.setPulseAmplitudeGreen(0);

  sampleSize = samples;
  rates = new byte[sampleSize];
}
int RateMonitor::average() {
  beatAvg = 0;
  for (byte x = 0 ; x < sampleSize ; x++) {
    beatAvg += rates[x];
  }
  beatAvg /= sampleSize;

  return beatAvg;
}
float RateMonitor::bpm() {
  long irValue = particleSensor.getIR();

  if (checkForBeat(irValue) == true) {
    //We sensed a beat!
    delta = millis() - lastBeat;
    lastBeat = millis();

    float bpm = 60 / (delta / 1000.0);

    if (bpm < 255 && bpm > 20) {
      rates[rateSpot++] = (byte)bpm; 
      rateSpot %= sampleSize;
    }
  }

  return bpm;
}