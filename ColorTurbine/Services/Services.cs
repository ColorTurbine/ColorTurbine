namespace ColorTurbine
{
    public static class Services
    {
        private static ConfigurationService _cs;
        public static ConfigurationService Configuration
        {
            get
            {
                if(_cs == null)
                    _cs = new ConfigurationService();
                return _cs;
            }
        }
        public static ThemeService Theme { get; } = new ThemeService();
        public static WeatherService Weather { get; } = new WeatherService();
        public static SunService Sun { get; } = new SunService();
        public static HueService Hue { get; } = new HueService();
        public static HabiticaService Habitica { get; } = new HabiticaService();
        public static InfluxDBService InfluxDB { get; } = new InfluxDBService();
        public static ColorMindService ColorMind { get; } = new ColorMindService();
    }
}
