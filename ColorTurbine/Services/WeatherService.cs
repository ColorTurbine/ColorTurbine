using System;
using System.Threading.Tasks;
using CreativeGurus.Weather.Wunderground;
using CreativeGurus.Weather.Wunderground.Models;

namespace ColorTurbine
{
    // Backup: https://github.com/swiftyspiffy/OpenWeatherMap-API-CSharp

    public class WeatherService
    {
        WeatherClient client;
        string lat, lon;
        public WeatherService()
        {
            var config = Services.Configuration.GetServiceConfiguration("weather");
            var apikey = (string)config["apikey"];
            if(apikey == null)
                throw new ArgumentNullException("Weather service requires an API key");
            client = new WeatherClient(apikey);

            lat = config["latitude"];
            lon = config["longitude"];
        }

        // [Light/Heavy] Drizzle
        // [Light/Heavy] Rain
        // [Light/Heavy] Snow
        // [Light/Heavy] Snow Grains
        // [Light/Heavy] Ice Crystals
        // [Light/Heavy] Ice Pellets
        // [Light/Heavy] Hail
        // [Light/Heavy] Mist
        // [Light/Heavy] Fog
        // [Light/Heavy] Fog Patches
        // [Light/Heavy] Smoke
        // [Light/Heavy] Volcanic Ash
        // [Light/Heavy] Widespread Dust
        // [Light/Heavy] Sand
        // [Light/Heavy] Haze
        // [Light/Heavy] Spray
        // [Light/Heavy] Dust Whirls
        // [Light/Heavy] Sandstorm
        // [Light/Heavy] Low Drifting Snow
        // [Light/Heavy] Low Drifting Widespread Dust
        // [Light/Heavy] Low Drifting Sand
        // [Light/Heavy] Blowing Snow
        // [Light/Heavy] Blowing Widespread Dust
        // [Light/Heavy] Blowing Sand
        // [Light/Heavy] Rain Mist
        // [Light/Heavy] Rain Showers
        // [Light/Heavy] Snow Showers
        // [Light/Heavy] Snow Blowing Snow Mist
        // [Light/Heavy] Ice Pellet Showers
        // [Light/Heavy] Hail Showers
        // [Light/Heavy] Small Hail Showers
        // [Light/Heavy] Thunderstorm
        // [Light/Heavy] Thunderstorms and Rain
        // [Light/Heavy] Thunderstorms and Snow
        // [Light/Heavy] Thunderstorms and Ice Pellets
        // [Light/Heavy] Thunderstorms with Hail
        // [Light/Heavy] Thunderstorms with Small Hail
        // [Light/Heavy] Freezing Drizzle
        // [Light/Heavy] Freezing Rain
        // [Light/Heavy] Freezing Fog
        // Patches of Fog
        // Shallow Fog
        // Partial Fog
        // Overcast
        // Clear
        // Partly Cloudy
        // Mostly Cloudy
        // Scattered Clouds
        // Small Hail
        // Squalls
        // Funnel Cloud
        // Unknown Precipitation
        // Unknown

        private CurrentObservation _conditions;
        private DateTime lastUpdate;

        public async Task<CurrentObservation> GetConditions()
        {
            if (DateTime.UtcNow - lastUpdate > TimeSpan.FromMinutes(5))
            {
                lastUpdate = DateTime.UtcNow;

                var cond = await client.GetConditionsAsync(QueryType.GPS,
                    new QueryOptions() { 
                        Latitude = lat,
                        Longitude = lon,
                        });
                _conditions = cond.CurrentObservation;
            }
            return _conditions;
        }
    }
}
