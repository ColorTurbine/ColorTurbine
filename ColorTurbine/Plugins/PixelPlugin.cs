using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ColorTurbine
{
    public abstract class PixelPlugin : IPlugin
    {
        RGBWStrip2D s2d;
        public int Days { get; private set; }

        public override void Initialize(IStrip s, PluginConfig config)
        {
            base.Initialize(s, config);
            this.height = config["height"];
            this.Days = config["days"];

            s2d = s as RGBWStrip2D;
            if (s2d == null)
            {
                throw new ArgumentException("Strip must be of type Strip2D");
            }
        }

        internal int RefreshSeconds = 120;
        internal int height;

        public override bool NeedsRender()
        {
            return DateTime.UtcNow - lastRender > TimeSpan.FromSeconds(RefreshSeconds) ||
                     user == null; // no data yet
        }

        abstract internal Task<RGBWColor[]> GetValueForDates(DateTime start, DateTime end);

        RGBWColor[] user;
        DateTime lastRender;

        public override async Task Render()
        {
            if (!NeedsRender())
                return;
            lastRender = DateTime.UtcNow;
            var now = DateTime.Now;
            user = await GetValueForDates(now - TimeSpan.FromDays(Days), now);
        }

        public override void Paint()
        {
            for (int i = 0; i < user.Length; i++)
            {
                s2d[(s2d.width - 1) - i, s2d.height - 1 - height] = user[i];
            }
        }
    }
}
