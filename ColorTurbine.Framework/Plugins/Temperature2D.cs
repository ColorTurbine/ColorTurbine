using System;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Threading.Tasks;

namespace ColorTurbine
{
    public class Temperature2D : IPlugin2D
    {
        CreativeGurus.Weather.Wunderground.CurrentObservation conditions;
        DateTime lastRender;
        RGBWStrip2D s2d;

        public override void Initialize(IStrip s, PluginConfig config)
        {
            base.Initialize(s, config);
            s2d = s as RGBWStrip2D;
            if (s2d == null)
            {
                throw new ArgumentException("Strip must be of type RGBWStrip2D");
            }

            // TODO: Use magic to properly initialize 2d plugins
            var loc = config.location.Replace("(", "").Replace(")", "").Split(",").Select(x=>int.Parse(x.Trim()));
            this.location = new Point(loc.ElementAt(0), loc.ElementAt(1));
            //this.Font = this.FontCollection.Install("fonts/ostrich-sans-regular.ttf").CreateFont(22);
        }

        public override bool NeedsRender()
        {
            // Refresh every five seconds
            return DateTime.UtcNow - lastRender > TimeSpan.FromSeconds(5) ||
                   // Or when the weather hasn't been fetched yet
                   conditions == null;
        }

        internal RGBWColor CurrentColor(bool nightmode, string weather)
        {
            if (nightmode)
            {
                return new RGBWColor(25, 0, 0, 0);
            }

            switch (weather)
            {
                case "Clear":
                    return new RGBWColor(0, 0, 127, 255);
                case "Partly Cloudy":
                    return new RGBWColor(0, 0, 195, 195);
                case "Mostly Cloudy":
                    return new RGBWColor(0, 0, 175, 175);
                case "Clouds":
                    return new RGBWColor(0, 0, 255, 127);
                case "Rain":
                    return new RGBWColor(0, 0, 255, 0);
                case "Snow":
                    return new RGBWColor(0, 0, 255, 255);
            }

            // TODO: Other weather conditions
            return new RGBWColor(0, 0, 0, 255);
        }

        double text;
        RGBWColor color;

        public override async Task Render()
        {
            lastRender = DateTime.UtcNow;
            conditions = await Services.Weather.GetConditions();
            text = Math.Round(conditions.TempF);
            color = CurrentColor(Services.Sun.NightMode, conditions.Weather);
        }

        public override void Paint()
        {
            // var img = new Image<Rgba32>((int)Math.Round(s2d.height * 1.0), s2d.height);
            // img.Mutate(x => {
            //         x.DrawText(text.ToString(), this.Font, Brushes.Solid(Rgba32.Red), null, Vector2.Zero, new TextGraphicsOptions(true));
            //         x.Resize(s2d.width, s2d.height);
            //     });
            // s2d.CopyImage(img);

            s2d.RenderString(this.location, text.ToString(), color * Services.Sun.Brightness);
        }
    }
}
