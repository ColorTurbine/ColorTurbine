using System;
using System.Threading.Tasks;

namespace ColorTurbine
{

    public class TwoModeClock : IPlugin
    {
        public IPlugin DayMode { get; set; }
        public IPlugin NightMode { get; set; }

        private bool prevNightMode = false;

        private IStrip currentStrip()
        {
            if (Services.Sun.NightMode)
                return NightMode.strip;
            return DayMode.strip;
        }
        public override IStrip strip { get => currentStrip(); }

        public override void Initialize(IStrip s, PluginConfig config)
        {
            base.Initialize(s, config);
            // This initialization is usually handled by the StripManager
            DayMode = Services.Configuration.CreatePlugin(s, config["day"].ToObject<PluginConfig>());
            NightMode = Services.Configuration.CreatePlugin(s, config["night"].ToObject<PluginConfig>());
        }

        public override bool NeedsRender()
        {
            // Force a few refreshes when nightmode changes
            if (prevNightMode != Services.Sun.NightMode)
            {
                prevNightMode = Services.Sun.NightMode;
                return true;
            }

            if (Services.Sun.NightMode)
            {
                return NightMode.NeedsRender();
            }
            return DayMode.NeedsRender();
        }

        public override async Task Render()
        {
            if (Services.Sun.NightMode)
            {
                await NightMode.Render();
            }
            else
            {
                await DayMode.Render();
            }
        }

        public override void Paint()
        {
            if (Services.Sun.NightMode)
            {
                NightMode.Paint();
            }
            else
            {
                DayMode.Paint();
            }
        }
    }
}
