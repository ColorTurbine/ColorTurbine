using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Numerics;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using QuickType;
using System.Threading.Tasks;

namespace ColorTurbine
{
    public class NexTrip : IPlugin2D
    {
        DateTime lastRender;
        RGBWStrip2D s2d;

        string baseURL;
        StopCollection stops;
        List<BusDeparture> allDepartures;

        public override void Initialize(IStrip s, PluginConfig config)
        {
            base.Initialize(s, config);
            s2d = s as RGBWStrip2D;
            if (s2d == null)
            {
                throw new ArgumentException("Strip must be of type RGBWStrip2D");
            }

            baseURL = (string)config["baseURL"];
            var tok = (JToken)config["busStops"];
            var busstops = tok.ToObject<BusStop[]>();

            stops = new StopCollection(baseURL, busstops);
        }

        class StopCollection
        {
            public StopCollection(string baseURL, BusStop[] busStop)
            {
                _baseURL = baseURL;
                this.stops = busStop;
            }

            private string _baseURL { get; }
            public BusStop[] stops {get; private set;}

            private async Task<BusDeparture[]> GetStopDepartures(BusStop stop)
            {
                var client = new HttpClient();
                var jsonString = await client.GetStringAsync(_baseURL + stop.StopId.ToString() + "?format=json");

                var departures = BusDeparture.FromJson(jsonString);
                for (int i = 0; i < departures.Length; i++)
                {
                    departures[i].BusStop = stop;
                }
                return departures;
            }

            public async Task<List<BusDeparture>> GetAllDepartures()
            {
                List<BusDeparture> depts = new List<BusDeparture>(stops.Count());
                foreach(var stop in stops)
                {
                    var dept = await GetStopDepartures(stop);
                    depts.AddRange(dept);
                }
                return depts;
            }

            public RGBWColor GetColorForDeparture(BusDeparture departure)
            {
                var stop = departure.BusStop;
                var timeleft = departure.DepartureTime - DateTime.Now;
                var s = Services.Theme.GetColorStyle(true);

                // TODO: Display something for 'not actual'
                // TODO: Display countdown thing
                // TODO: Goodness as floating point value

                var extraTime = timeleft - stop.WalkTime;

                if(extraTime < TimeSpan.Zero)
                {
                    return new RGBWColor(0,0,0,0);
                } else if(extraTime < TimeSpan.FromMinutes(1))
                {
                    return s.BadColor;
                } else if(extraTime < TimeSpan.FromMinutes(2))
                {
                    return s.MidColor;
                } else if(extraTime < TimeSpan.FromMinutes(4))
                {
                    return s.BestColor;
                } else if(extraTime < TimeSpan.FromMinutes(7))
                {
                    return s.GoodColor;
                } else if(extraTime < TimeSpan.FromMinutes(15))
                {
                    return s.GoodColor * 0.5;
                }

                return s.BadColor;
            }

            public async Task<BusDeparture> GetBestDeparture()
            {
                var allDepartures = await GetAllDepartures();
                return GetBestDeparture(allDepartures);
            }

            public BusDeparture GetBestDeparture(List<BusDeparture> allDepartures)
            {
                if (allDepartures == null || allDepartures.Count() == 0)
                {
                    return null;
                }

                // Choose departures on a desired route that can be walked to in time
                var departures = from departure in allDepartures
                    where (DateTime.Now + departure.BusStop.WalkTime) < departure.DepartureTime
                    where departure.BusStop.Routes.Any(x => x.RouteCode == departure.Route &&
                          (x.Terminal == null || x.Terminal == departure.Terminal))
                    select departure;

                if (departures.Count() == 0)
                {
                    return null;
                }

                var depts = departures.ToList();
                depts.Sort();

                Console.WriteLine("Departures: ");
                for (var i = 0; i < Math.Min(departures.Count(), 10); i++)
                {
                    Console.WriteLine(depts[i]);
                }
                Console.WriteLine("--\n");

                // TODO: This would be interesting data to log to grafana
                // TODO: Log time to best bus arrival - that would be really cool data to have!
                return depts.Min();
            }
        }

        public override bool NeedsRender()
        {
            if (allDepartures == null)
            {
                return true;
            }
            if (DateTime.UtcNow - lastRender < TimeSpan.FromMinutes(1))
            {
                return false;
            }
            return true;
        }

        public override async Task Render()
        {
            lastRender = DateTime.UtcNow;
            allDepartures = await stops.GetAllDepartures();
        }

        public override void Paint()
        {
            if (allDepartures == null)
            {
                return;
            }

            var bestDeparture = stops.GetBestDeparture(allDepartures);
            s2d.RenderString(this.location, bestDeparture.Route.ToString() + bestDeparture.Terminal, stops.GetColorForDeparture(bestDeparture));

            // TODO: render wait time indicator
        }
    }
}
