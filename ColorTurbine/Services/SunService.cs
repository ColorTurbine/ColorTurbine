using System;

namespace ColorTurbine
{
    public class SunService
    {
        public SunService()
        {
            var config = Services.Configuration.GetServiceConfiguration("sun");
            double lat = double.Parse((string)config["latitude"]);
            double lon = double.Parse((string)config["longitude"]);
            sun = new SunPosition(lat, lon);
        }

        SunPosition sun;

        DateTime lastCalculationTime;
        SunPosition.SunPos lastCalculation;

        // TODO Make this private
        public SunPosition.SunPos CalculateSunPosition()
        {
            if (DateTime.UtcNow - lastCalculationTime < TimeSpan.FromSeconds(10))
            {
                return lastCalculation;
            }
            lastCalculationTime = DateTime.UtcNow;
            lastCalculation = CalculateSunPosition(DateTime.Now);
            return lastCalculation;
        }
        private SunPosition.SunPos CalculateSunPosition(DateTime time)
        {
            return sun.CalculateSunPosition(time);
        }

        bool nightModeValue = false;
        bool nightModeOverride = false;

        // TODO: Add event for night mode on/off
        public bool NightMode
        {
            get
            {
                // Reset night mode override every day
                if (CalculateSunPosition().isNightMode == nightModeValue)
                {
                    nightModeOverride = false;
                }

                if (nightModeOverride)
                {
                    return nightModeValue;
                }

                return CalculateSunPosition().isNightMode;
            }
            set
            {
                nightModeValue = value;
                nightModeOverride = true;
            }
        }

        public double Altitude
        {
            get
            {
                return CalculateSunPosition().altitude;
            }
        }

        public double Azimuth
        {
            get
            {
                return CalculateSunPosition().azimuth;
            }
        }

        public double Brightness
        {
            get
            {
                return CalculateSunPosition().brtscl;
            }
        }
    }
}
