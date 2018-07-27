using System;
using System.Threading.Tasks;
using static ColorTurbine.SunService;

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
                _smudged = true;
            }
        }

        private RGBWColor _nightColor;
        public RGBWColor NightColor
        {
            get { return _nightColor; }
            set
            {
                _nightColor = value;
                _smudged = true;
            }
        }

        private bool _smudged = false;

        public override void Initialize(IStrip s, PluginConfig config)
        {
            base.Initialize(s, config);
            
            DayColor = RGBWColor.Parse((string)config["day"]);
            NightColor = RGBWColor.Parse((string)config["night"]);

            Services.Sun.OnSunEvent += (ev) =>
            {
                _smudged = true;
            };
        }

        public override void OnEnable()
        {
            _smudged = true;
        }

        public override bool NeedsRender() => _smudged;

        public override Task Render()
        {
            _smudged = false;
            return Task.CompletedTask;
        }

        public override void Paint()
        {
            strip.Fill(0, strip.led_count - 1, Services.Sun.NightMode ? NightColor : DayColor);
        }
    }
}
