# Plasma
*Feel the degeneracy in your veins*

Plasma consists of two components: The server and hardware client.

## Server
A CLI that takes a `--port=` parameter and `--address=` parameter for configuring which address and port the server should listen on. (This needs to be the same as is set in the firmware) 

## Hardware/Firmware
The initial design is using an ESP32s dev board with a MAX30102 pulse oximeter module. You could use any microcontroller board with support for Arduino Core and any pulse oximeter in the MAX30105 family. 
> Please note: whatever board you use must have Wifi support.

I'm also planning to design a 3d-printable case which mounts a battery pack and uses elastic bands to attach the device to your upper arm or wrist.

## Configuring Your Avatar
The server communicates with your avatar over OSC by reading and updating three required parameters:

| Parameter | Type | Notes |
| ----- | --- | --- |
| `Plasma/enable` | bool | Writeable; toggled to enable/disable updating of bpm |
| `Plasma/connected` | bool | Readonly; shows connection status of server |
| `Plasma/bpm` | int | Readonly; heartrate in bpm |
