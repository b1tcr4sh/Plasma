#include "MAX30105.h"

class RateMonitor {
  private: 
    MAX30105 Sensor;
    int sampleSize;
    int beatAvg = 0;
    byte rates[4]; 
    byte rateSpot = 0;
    long lastBeat = 0;
    long delta;

  public:
    RateMonitor(int sampleSize);
    int RateMonitor::average();
    float bpm();
};