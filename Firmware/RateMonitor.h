#include "MAX30105.h"

class RateMonitor {
  private: 
    MAX30105 Sensor;

  public:
    RateMonitor();
    float sample(int);
};