# Plasma
A simple (kind of jank) solution for capturing and streaming heartrate to VRChat or other osc applications without lizard middle-men

**This is a DIY hardware project. It requires soldering, 3D printing, and at least a basic knowledge of working with VRChat systems**

## Using For Yourself
You'll have to get your paws a little dirty building some rudimentary hardware to get this up and running.

### Hardware
I'd recommend using an ESP32 WROOM dev board ([like this one](https://www.amazon.com/HiLetgo-ESP-WROOM-32-Development-Microcontroller-Integrated/dp/B0718T232Z/ref=sr_1_5?keywords=esp32+wroom&qid=1680793412&sr=8-5)) and a MAX30102 pulse oximeter ([like this one](https://www.amazon.com/HiLetgo-MAX30102-Breakout-Oximetry-Solution/dp/B07QC67KMQ/ref=sr_1_3?crid=3LUMMSUWOBMA7&keywords=MAX30102&qid=1680793460&sprefix=max30102%2Caps%2C1123&sr=8-3))
> Please note: whatever board you use must have onboard Wifi and support for Arduino Core.

To connect the pulse oximeter to the board, connect/solder these pins:
| MAX30102 | ESP32 GPIO |
| -------- | ---------- |
| VIN/VCC | 3.3V |
| GND | GND |
| SDA | 21 |
| SDL | 22 |

Then you'll need to install Arduino IDE 2 or Arduino Cli and the ESP32 board extension, then configure your wifi SSID, password, and the ip address of the server in `Firmware.ino`, then flash to the board.  Once you power the board on, it should connect to wifi, then search for the server (Read the serial output).

I'm also planning to design a 3d-printable case which mounts a battery pack and uses elastic bands to attach the device to your upper arm or wrist. I'm lazy and CAD is hard, so maybe I'll get to it.. ?
### Server
This is really simple.  Just download a build of the server from [Releases](https://github.com/ChronicallyKyra/Plasma/releases) and run it from a cmd before starting VRChat.
>Make sure to include the --port= and --address= parameters


### Configuring Your Avatar
The server communicates with your avatar over OSC by reading and updating three required parameters:

| Parameter | Type | Notes |
| ----- | --- | --- |
| `Plasma/enable` | bool | Writeable; toggled to enable/disable updating of bpm |
| `Plasma/connected` | bool | Readonly; shows connection status of server |
| `Plasma/bpm` | int | Readonly; heartrate in bpm |
