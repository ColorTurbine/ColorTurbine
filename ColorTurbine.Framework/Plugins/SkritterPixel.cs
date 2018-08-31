using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ColorTurbine
{
    public class SkritterPixel : PixelPlugin
    {
        DateTime lastRefresh;
        DateTime lastGet;
        dynamic cachedResult;
        string cached_access_token;
        bool logThis;

        string token;
        DateTime loginGoodUntil;
        string oauth_client_name;
        string oauth_client_secret;
        string user_name;
        string user_password;
        bool enable_logging;
        ServiceConfig influxConfig;

        public override void Initialize(IStrip s, PluginConfig config)
        {
            base.Initialize(s, config);

            oauth_client_name = config["OAUTH_CLIENT_NAME"];
            oauth_client_secret = config["OAUTH_CLIENT_SECRET"];
            user_name = config["username"];
            user_password = config["password"];
            influxConfig = Services.Configuration.GetServiceConfiguration("influxDB");
            enable_logging = influxConfig != null;
        }

        private async Task<string> login()
        {
            // TODO Refresh tokens as they expire: http://beta.skritter.com/api/v0/docs/authentication
            if (cached_access_token != null && DateTime.UtcNow < loginGoodUntil)
            {
                return cached_access_token;
            }

            string query;
            using (var content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]{
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("client_id", oauth_client_name),
                new KeyValuePair<string, string>("username", user_name),
                new KeyValuePair<string, string>("password", user_password),
            }))
            {
                query = content.ReadAsStringAsync().Result;
            }

            var credentials = $"{oauth_client_name}:{oauth_client_secret}";
            string b64creds = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(credentials));

            var UserURL = "https://beta.skritter.com/api/v0/oauth2/token";
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("basic", b64creds);

            var msg = await client.GetStringAsync(UserURL + "?" + query);
            var cachedLogin = JObject.Parse(msg);
            cached_access_token = (string)cachedLogin["access_token"];
            int expires_in = (int)cachedLogin["expires_in"];
            loginGoodUntil = DateTime.UtcNow + TimeSpan.FromSeconds(expires_in - 10);
            return cached_access_token;
        }

        private string refreshToken()
        {
            return token;
        }

        internal async Task<dynamic> GetProgressStats(int days = 1)
        {
            logThis = false;
            if (cachedResult != null && DateTime.UtcNow - lastGet < TimeSpan.FromMinutes(1))
            {
                return cachedResult;
            }

            token = await login();
            lastGet = DateTime.UtcNow;

            var now = DateTime.Now;
            var startDay = now - TimeSpan.FromDays(days - 1);
            var startDayStr = $"{startDay.Year}-{startDay.Month}-{startDay.Day}"; // YYYY-MM-DD
            var endDayStr = $"{now.Year}-{now.Month}-{now.Day}"; // YYYY-MM-DD
            var UserURL = $"https://beta.skritter.com/api/v0/progstats?bearer_token={token}&start={startDayStr}&end={endDayStr}";

            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            try
            {
                var msg = await client.GetStringAsync(UserURL);
                cachedResult = JObject.Parse(msg);
                logThis = true;
                return cachedResult;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error updating Skritter status");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace.ToString());
                return cachedResult;
            }
        }

        internal RGBWColor mapTimeStudiedToColor(TimeSpan time)
        {
            double goodness = time / TimeSpan.FromMinutes(10);
            return Services.Theme.GetColorStyle().MapGoodnessToColor(goodness);
        }

        internal override async Task<RGBWColor[]> GetValueForDates(DateTime start, DateTime end)
        {
            var days = (end - start).Days;
            var result = new RGBWColor[days];
            var user = await GetProgressStats(days);
            lastRefresh = DateTime.UtcNow;

            int i = days - 1;
            foreach (var day in user.ProgressStats)
            {
                var timeStudiedToday = TimeSpan.FromSeconds((double)day.timeStudied.day);
                RGBWColor color = mapTimeStudiedToColor(timeStudiedToday);

                result[i] = color;
                i--;
            }

            if (enable_logging && logThis)
            {
                // TODO: Iterate through stats objects to find today. This is naive and will break.
                var today = user.ProgressStats[days - 1];

                var pt = new InfluxData.Net.InfluxDb.Models.Point()
                {
                    Name = "skritter",
                    Fields = new Dictionary<string, object>()
                {
                    { "timeStudiedToday", today.timeStudied.day },

                    { "word.rune.remembered", today.word.rune.remembered.day },
                    { "word.rune.learned", today.word.rune.learned.day },
                    { "word.rune.studied", today.word.rune.studied.day },
                    { "word.rune.learning", today.word.rune.learning.day },

                    { "word.rdng.remembered", today.word.rdng.remembered.day },
                    { "word.rdng.learned", today.word.rdng.learned.day },
                    { "word.rdng.studied", today.word.rdng.studied.day },
                    { "word.rdng.learning", today.word.rdng.learning.day },

                    { "word.tone.remembered", today.word.tone.remembered.day },
                    { "word.tone.learned", today.word.tone.learned.day },
                    { "word.tone.studied", today.word.tone.studied.day },
                    { "word.tone.learning", today.word.tone.learning.day },

                    { "word.defn.remembered", today.word.defn.remembered.day },
                    { "word.defn.learned", today.word.defn.learned.day },
                    { "word.defn.studied", today.word.defn.studied.day },
                    { "word.defn.learning", today.word.defn.learning.day },

                    { "char.rune.remembered", today["char"].rune.remembered.day },
                    { "char.rune.learned", today["char"].rune.learned.day },
                    { "char.rune.studied", today["char"].rune.studied.day },
                    { "char.rune.learning", today["char"].rune.learning.day },
                    { "char.rdng.remembered", today["char"].rdng.remembered.day },
                    { "char.rdng.learned", today["char"].rdng.learned.day },
                    { "char.rdng.studied", today["char"].rdng.studied.day },
                    { "char.rdng.learning", today["char"].rdng.learning.day },
                    { "char.tone.remembered", today["char"].tone.remembered.day },
                    { "char.tone.learned", today["char"].tone.learned.day },
                    { "char.tone.studied", today["char"].tone.studied.day },
                    { "char.tone.learning", today["char"].tone.learning.day },
                    { "char.defn.remembered", today["char"].defn.remembered.day },
                    { "char.defn.learned", today["char"].defn.learned.day },
                    { "char.defn.studied", today["char"].defn.studied.day },
                    { "char.defn.learning", today["char"].defn.learning.day },
                },
                    Timestamp = lastRefresh
                };

                await Services.InfluxDB.WriteAsync(pt);
            }

            return result;
        }
    }
}
