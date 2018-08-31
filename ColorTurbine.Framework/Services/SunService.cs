using System;
using System.Linq;
using System.Threading;
using Hangfire;
using Innovative.SolarCalculator;

namespace ColorTurbine
{
    public static class TimeHelpers
    {
        public static TimeSpan TimeToNextSunChange(DateTime time, double lat, double lon)
        {
            SolarTimes solarTimes = new SolarTimes(time, lat, lon);
            DateTime sunrise = solarTimes.Sunrise;
            DateTime sunset = solarTimes.Sunset;

            var now = time;
            if (now < sunrise)
                return sunrise - now;

            if (now < sunset)
                return sunset - now;

            SolarTimes tomorrowSolarTimes = new SolarTimes(time + TimeSpan.FromDays(1), lat, lon);
            DateTime tomorrowSunrise = tomorrowSolarTimes.Sunrise;
            return tomorrowSunrise - now;
        }
    }

    public class SunService
    {
        double lat, lon;
        public SunService()
        {
            var config = Services.Configuration.GetServiceConfiguration("sun");
            lat = double.Parse((string)config["latitude"]);
            lon = double.Parse((string)config["longitude"]);

            // TODO: How to handle adding sunset/sunrise offset?
            sun_changed();
        }

        private void set_sun(SunEvent sunevent)
        {
            if (OnSunEvent != null)
                OnSunEvent(sunevent);

            switch (sunevent)
            {
                case SunEvent.Sunrise:
                    if (OnSunrise != null)
                        OnSunrise(sunevent);
                    break;
                case SunEvent.Sunset:
                    if (OnSunset != null)
                        OnSunset(sunevent);
                    break;
            }
        }

        public void sun_changed()
        {
            var sunevent = CalculateSunPosition().SolarElevation < 0.0 ? SunEvent.Sunset : SunEvent.Sunrise;
            
            // Clear night mode override once it becomes the world-state
            nightModeOverride = false;

            var prevNightMode = actualNightModeValue;

            if (sunevent == SunEvent.Sunrise)
                actualNightModeValue = false;
            if (sunevent == SunEvent.Sunset)
                actualNightModeValue = true;

            // Make sure the sun state actually changed (we may be called multiple times)
            if(prevNightMode == actualNightModeValue)
                return;

            set_sun(sunevent);

            var nextSunChange = TimeHelpers.TimeToNextSunChange(DateTime.Now, lat, lon);
            BackgroundJob.Schedule(() => sun_changed(), nextSunChange);
            Console.WriteLine($"{DateTime.Now}: Sun is {(actualNightModeValue?"down":"up")}, next event scheduled in {nextSunChange}, at {DateTime.Now + nextSunChange}");
        }

        public enum SunEvent
        {
            Sunrise,
            Sunset
        }

        public delegate void SunEventHandler(SunEvent e);
        public event SunEventHandler OnSunEvent;
        public event SunEventHandler OnSunset;
        public event SunEventHandler OnSunrise;

        DateTime lastCalculationTime;
        SolarTimes lastCalculation;

        private SolarTimes CalculateSunPosition()
        {
            if (DateTime.UtcNow - lastCalculationTime < TimeSpan.FromSeconds(10)) // TODO: Remove after all usages are gone
            {
                return lastCalculation;
            }
            lastCalculationTime = DateTime.UtcNow;
            lastCalculation = CalculateSunPosition(DateTime.Now);
            return lastCalculation;
        }
        
        private SolarTimes CalculateSunPosition(DateTime time)
        {
            return new SolarTimes(time, lat, lon);
        }

        bool nightModeValue = false;
        bool nightModeOverride = false;
        bool actualNightModeValue;

        public bool NightMode
        {
            get
            {
                if (nightModeOverride)
                {
                    return nightModeValue;
                }
                return actualNightModeValue;
            }
            set
            {
                if(NightMode != value)
                {
                    nightModeValue = value;
                    nightModeOverride = true;
                    set_sun(nightModeValue ? SunEvent.Sunset : SunEvent.Sunrise);
                }
            }
        }

        public double Altitude
        {
            get
            {
                return CalculateSunPosition().SolarElevation;
            }
        }

        public double Azimuth
        {
            get
            {
                return CalculateSunPosition().SolarAzimuth;
            }
        }

        public double Brightness
        {
            get
            {
                var pos = CalculateSunPosition();

                var brtscl = 1.0;
                if(!NightMode && Altitude < 5)
                {
                    brtscl = Math.Max(1 - ((Altitude - 5) / (-8 - 5)), 0.1);
                }
                return brtscl;
            }
        }
    }
}
