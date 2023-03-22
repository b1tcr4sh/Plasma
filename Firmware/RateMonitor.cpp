#include <Wire.h>
#include "MAX30105.h"
#include "heartRate.h"
#include "RateMonitor.h"

RateMonitor::RateMonitor() {
  if (!Sensor.begin(Wire, I2C_SPEED_FAST)) {
    return null; 
  }

  Sensor.setup(); 
  Sensor.setPulseAmplitudeRed(0x0A); 
  Sensor.setPulseAmplitudeGreen(0);
}
int RateMonitor::average_heartrate() {

}
int RateMonitor::bpm() {

}