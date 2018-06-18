using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ColorTurbine
{
    public class HabiticaPixel : PixelPlugin
    {
        public override void Initialize(IStrip s, PluginConfig config)
        {
            base.Initialize(s, config);

            this.taskId = config["taskId"];
            this.taskName = config["taskName"];
        }

        string taskId;
        string taskName;

        internal RGBWColor mapGoodnessToColor(double goodness)
        {
            if (goodness >= 1)
            {
                return Services.Theme.GetColorStyle().BestColor;
            }
            else if (goodness >= 0.5)
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
            var history = await Services.Habitica.GetTaskHistory(taskId);
            var days = (end - start).Days;
            RGBWColor[] res = Enumerable.Repeat(Services.Theme.GetColorStyle().BadColor, days).ToArray();
            foreach(var val in history) {
                if(val.Key > start && val.Key < end) {
                    var diff = end.Date - val.Key.Date;
                    var idx = diff.Days;
                    res[idx] = mapGoodnessToColor(val.Value);
                }
            }
            return res;
        }
    }
}
