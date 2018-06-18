using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using static ColorTurbine.StripManager;

namespace ColorTurbine
{

    public class TimeGate2D : IPlugin2D
    {
        public IPlugin2D plugin { get; private set; }
        public TimeSpan timeStart { get; set; }
        public TimeSpan timeEnd { get; set; }
        public DayOfWeek[] days { get; set; }

        public override void Initialize(IStrip s, PluginConfig config)
        {
            base.Initialize(s, config);
            var plu = Services.Configuration.CreatePlugin(s, config["timegate_plugin"].ToObject<PluginConfig>());
            plugin = (IPlugin2D)plu; // TODO: Error checking (verify it's an IPlugin2D)

            string ts = config["timeStart"];
            string te = config["timeEnd"];

            timeStart = DateTime.ParseExact(ts, "h:mm tt", CultureInfo.InvariantCulture).TimeOfDay;
            timeEnd = DateTime.ParseExact(te, "h:mm tt", CultureInfo.InvariantCulture).TimeOfDay;

            var strdays = (string)config["days"];
            days = strdays.ToUpper().ToList().Select(x =>
            {
                if (x == 'M') return DayOfWeek.Monday;
                if (x == 'T') return DayOfWeek.Tuesday;
                if (x == 'W') return DayOfWeek.Wednesday;
                if (x == 'R') return DayOfWeek.Thursday;
                if (x == 'F') return DayOfWeek.Friday;
                if (x == 'S') return DayOfWeek.Saturday;
                if (x == 'U') return DayOfWeek.Sunday;
                throw new ArgumentException("Invalid day of week");
            }).ToArray();
        }

        internal bool shouldDisplay()
        {
            var now = DateTime.Now;
            if (!days.Contains(now.DayOfWeek))
                return false;
            if (now.TimeOfDay < timeStart || now.TimeOfDay > timeEnd)
                return false;
            return true;
        }

        public override bool NeedsRender() => shouldDisplay() ? plugin.NeedsRender() : false;

        public override async Task Render()
        {
            await plugin.Render();
        }

        public override void Paint()
        {
            if(!shouldDisplay())
                return;
            plugin.Paint();
        }
    }
}
