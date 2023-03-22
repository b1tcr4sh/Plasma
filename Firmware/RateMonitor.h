#include "MAX30105.h"

class RateMonitor {
  private: 
    MAX30105 Sensor;

  public:
    RateMonitor();
    int average_heartrate();
    int bpm();
};