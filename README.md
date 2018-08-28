# ColorTurbine ![build status](https://gitlab.com/ColorTurbine/ColorTurbine/badges/master/build.svg)

ColorTurbine is built to stream animations to DIY wifi-connected addressible LED strips.

# Build a wifi-connected LED strip

Firmware is currently available for ESP8266 + WS2812b based LED strips. See [https://github.com/ColorTurbine/esp8266-udp-pixel-renderer](ColorTurbine/esp8266-udp-pixel-renderer).

# Setup

Copy `config.json.example` to `config.json` and customize it to fit your needs. Define at least one strip and choose a plugin to be displayed on it.

# Running

ColorTurbine can be run directly in dotnet (`dotnet run`) or with docker.

## Docker

`docker-compose.yml`:
```
  colorturbine:
    image: registry.gitlab.com/colorturbine/colorturbine:latest
    links:
      - mosquitto
      - influxdb
    volumes:
      - colorturbine/config.json:/app/config.json
      - colorturbine/plugins/:/plugins
    restart: unless-stopped
    
  mosquitto:
    image: eclipse-mosquitto
    ports:
      - 1883:1883
      - 9001:9001
    volumes:
      - mosquitto/config:/mosquitto/config
      - mosquitto/data:/mosquitto/data
    restart: unless-stopped
    
  influxdb:
    image: influxdb
    ports:
      - 8083:8083
      - 8086:8086
    volumes:
      - influxdb:/var/lib/influxdb
    environment:
      - "INFLUXDB_HTTP_AUTH_ENABLED=false"
      - "INFLUXDB_REPORTING_DISABLED=true"
      - "INFLUXDB_MONITOR_STORE_INTERVAL=180s"
    restart: unless-stopped
```