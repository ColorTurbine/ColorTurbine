# ColorTurbine ![build status](https://gitlab.com/ColorTurbine/ColorTurbine/badges/master/build.svg)

ColorTurbine is built to stream animations to DIY wifi-connected addressible LED strips.

# Build a wifi-connected LED strip

Firmware is currently available for ESP8266 + WS2812b based LED strips. See [https://github.com/ColorTurbine/esp8266-udp-pixel-renderer](ColorTurbine/esp8266-udp-pixel-renderer).

# Setup

Copy `config.json.example` to `config.json` and customize it to fit your needs. Define at least one strip and choose a plugin to be displayed on it.

# Running

ColorTurbine can be run directly in dotnet (`dotnet run`) or with docker. A public docker image is planned.