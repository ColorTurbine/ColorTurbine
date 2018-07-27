using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ColorTurbine
{
    public class DuolingoPixel : PixelPlugin
    {
        public override void Initialize(IStrip s, PluginConfig config)
        {
            base.Initialize(s, config);
            UserURL = "https://www.duolingo.com/users/" + (string)config["username"];
        }

        string languageCode;
        DateTime lastGet;
        dynamic cachedResult;
        string UserURL;

        internal async Task<dynamic> GetUser()
        {
            if (cachedResult != null && DateTime.UtcNow - lastGet < TimeSpan.FromMinutes(1))
            {
                return cachedResult;
            }
            lastGet = DateTime.UtcNow;

            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();

            var msg = await client.GetStringAsync(UserURL);
            cachedResult = JObject.Parse(msg);
            return cachedResult;
        }

        internal RGBWColor mapGoodnessToColor(int goodness)
        {
            if (goodness >= 100)
            {
                return Services.Theme.GetColorStyle().BestColor;
            }
            else if (goodness >= 50)
            {
                return Services.Theme.GetColorStyle().GoodColor;
            }
            else if (goodness > 0)
            {
                return Services.Theme.GetColorStyle().MidColor;
            }
            else
            {
                return Services.Theme.GetColorStyle().BadColor;
            }
        }

        DateTime epochToTime(long epoch)
        {
            return (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds(epoch).ToLocalTime();
        }

        internal override async Task<RGBWColor[]> GetValueForDates(DateTime start, DateTime end)
        {
            var days = (end - start).Days;
            int[] goodness = new int[days];

            var user = await GetUser();
            var language = user.language_data[languageCode];
            if (language == null) {
                return Enumerable.Repeat(Services.Theme.GetColorStyle().BadColor, days).ToArray();
            }

            var calendar = language.calendar;
            foreach (var cal in calendar)
            {
                var improvement = (int)cal.improvement;

                // Convert epoch to day
                var time = epochToTime((long)cal.datetime);
                if (time > end)
                {
                    goodness[(start - time).Days] += improvement;
                }
            }

            return goodness.Select(x => mapGoodnessToColor(x)).ToArray();
        }
    }
}
