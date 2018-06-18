using System;
using System.Threading.Tasks;

namespace ColorTurbine
{

    public class TwoColorClock : IPlugin
    {
        private RGBWColor _dayColor;
        public RGBWColor DayColor
        {
            get { return _dayColor; }
            set
            {
                _dayColor = value;
                smudged = true;
            }
        }

        private RGBWColor _nightColor;
        public RGBWColor NightColor
        {
            get { return _nightColor; }
            set
            {
                _nightColor = value;
                smudged = true;
            }
        }

        private bool prevNightMode = false;
        private bool smudged = false;

        public override void Initialize(IStrip s, PluginConfig config)
        {
            base.Initialize(s, config);
            
            DayColor = RGBWColor.Parse((string)config["day"]);
            NightColor = RGBWColor.Parse((string)config["night"]);
        }

        public override void OnEnable()
        {
            smudged = true;
        }

        public override bool NeedsRender()
        {
            if (smudged)
                return true;

            // Force a refresh when nightmode changes
            if (prevNightMode != Services.Sun.NightMode)
            {
                prevNightMode = Services.Sun.NightMode;
                return true;
            }
            
            var alt = Services.Sun.CalculateSunPosition();
            if (alt.altitude > 5 || alt.altitude < -1)
            {
                return false;
            }
            return true;
        }

        public override Task Render()
        {
            smudged = false;
            return Task.CompletedTask;
        }

        public override void Paint()
        {
            var color = DayColor;
            if (Services.Sun.NightMode)
            {
                color = NightColor;
            }

            for (int i = 0; i < strip.led_count; i++)
            {
                strip[i] = color * Services.Sun.Brightness;
            }
        }
    }
}
