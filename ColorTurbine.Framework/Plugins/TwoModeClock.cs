using System;
using System.Threading.Tasks;

namespace ColorTurbine
{

    public class TwoModeClock : IPlugin
    {
        private IPlugin _currentPlugin;
        private IPlugin _dayMode;
        private IPlugin _nightMode;

        public IPlugin DayMode {
            get {
                return _dayMode;
            }
            set {
                _dayMode = value;
                if (!Services.Sun.NightMode)
                {
                    _currentPlugin = value;
                    forceRefreshCounter = 3;
                }
            }
        }

        public IPlugin NightMode
        {
            get
            {
                return _nightMode;
            }
            set
            {
                _nightMode = value;
                if (Services.Sun.NightMode)
                {
                    _currentPlugin = value;
                    forceRefreshCounter = 3;
                }
            }
        }

        private int forceRefreshCounter = 3;

        public override IStrip strip { get => _currentPlugin.strip; }

        public override void Initialize(IStrip s, PluginConfig config)
        {
            base.Initialize(s, config);
            // This initialization is usually handled by the StripManager
            DayMode = Services.Configuration.CreatePlugin(s, config["day"].ToObject<PluginConfig>());
            NightMode = Services.Configuration.CreatePlugin(s, config["night"].ToObject<PluginConfig>());

            Services.Sun.OnSunrise += (_) =>
            {
                forceRefreshCounter = 3;
                _currentPlugin = DayMode;
            };
            Services.Sun.OnSunset += (_) =>
            {
                forceRefreshCounter = 3;
                _currentPlugin = NightMode;
            };
        }

        public override bool NeedsRender()
        {
            // Force a few refreshes when anything changes
            if (forceRefreshCounter > 0)
            {
                forceRefreshCounter--;
                return true;
            }

            return _currentPlugin.NeedsRender();
        }

        public override async Task Render()
        {
            await _currentPlugin.Render();
        }

        public override void Paint()
        {
            _currentPlugin.Paint();
        }
    }
}
