using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ColorTurbine
{
    public class HabiticaService
    {
        private class HabiticaApi
        {
            JObject latest;
            internal Dictionary<string, Dictionary<DateTime, double>> taskHistories = new Dictionary<string, Dictionary<DateTime, double>>();
            DateTime lastUpdate;

            string apiuser;
            string apikey;
            string apiurl;
            public HabiticaApi()
            {
                var config = Services.Configuration.GetServiceConfiguration("habitica");
                apiuser = config["api-user"];
                apikey = config["api-key"];
                apiurl = config["api-url"] ?? "https://habitica.com/api/v3/tasks/user";
            }

            private async Task update()
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
                client.DefaultRequestHeaders.Add("x-api-user", apiuser);
                client.DefaultRequestHeaders.Add("x-api-key", apikey);
                latest = JObject.Parse(await client.GetStringAsync(apiurl));
                lastUpdate = DateTime.UtcNow;
                taskHistories.Clear();
            }

            internal async Task<JObject> GetUserTasks()
            {
                if (latest != null && DateTime.UtcNow - lastUpdate < TimeSpan.FromMinutes(5))
                {
                    return latest;
                }
                await update();
                return latest;
            }
        }

        HabiticaApi api = new HabiticaApi();
        public async Task<JObject> GetTask(string taskid)
        {
            var tasks = await api.GetUserTasks();
            var data = tasks["data"];
            foreach(var task in data) {
                if((string)task["id"] == taskid) {
                    return (JObject)task;
                }
            }
            return null;
        }

        DateTime epochToTime(long epoch)
        {
            return (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds(epoch).ToLocalTime();
        }

        public async Task<Dictionary<DateTime, double>> GetTaskHistory(string taskid)
        {
            if(api.taskHistories.ContainsKey(taskid)) {
                return api.taskHistories[taskid];
            }
            var task = await GetTask(taskid);
            var history = task["history"];
            Dictionary<DateTime, double> diff = new Dictionary<DateTime, double>();
            double lastValue = 0;

            // Differential
            foreach (JObject item in history)
            {
                var date = epochToTime((long)item["date"]);
                var value = (double)item["value"];
                if (!diff.ContainsKey(date))
                {
                    diff[date] = 0;
                }

                diff[date] += value - lastValue;
                lastValue = value;
            }

            diff[DateTime.Now] = (double)task["value"] - lastValue;

            Dictionary<DateTime, double> parsedHistory = new Dictionary<DateTime, double>();
            // Aggregate
            foreach (var item in diff)
            {
                var date = item.Key;
                var value = item.Value;
                if (!parsedHistory.ContainsKey(date.Date))
                {
                    parsedHistory[date.Date] = 0;
                }

                parsedHistory[date.Date] += value;
            }
            api.taskHistories[taskid] = parsedHistory;
            return parsedHistory;
        }
    }
}
