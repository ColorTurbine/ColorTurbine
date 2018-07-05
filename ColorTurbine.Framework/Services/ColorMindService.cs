using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

namespace ColorTurbine
{
    public class Palette
    {
        public RGBColor[] palette { get; set; }
    }

    public class ColorMindService
    {
        public async Task<IEnumerable<string>> GetModelList()
        {
            var epurl = "http://colormind.io/list/";

            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
            client.DefaultRequestHeaders.Add("x-api-contact", "george.hahn.vhs@gmail.com");
            var models = JObject.Parse(await client.GetStringAsync(epurl));
            return models["result"].ToObject<string[]>();
        }

        public async Task<Palette> GetRandomPalette(string model = "default")
        {
            try
            {
                var epurl = "http://colormind.io/api/";
                var post = "{\"model\":\"default\"}";

                var client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
                client.DefaultRequestHeaders.Add("x-api-contact", "george.hahn.vhs@gmail.com");
                var resp = await client.PostAsync(epurl, new StringContent(post));
                var models = JObject.Parse(await resp.Content.ReadAsStringAsync());
                byte[][] result = models["result"].ToObject<byte[][]>();
                RGBColor[] ret = new RGBColor[result.Length];
                for (int i = 0; i < result.Length; i++)
                {
                    ret[i] = new RGBColor(result[i][0], result[i][1], result[i][2]);
                }
                return new Palette { palette = ret };
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Palette> GetSuggestedPalette(Palette input)
        {
            // TODO: GetRandomPalette but with a different POST
            return null;
        }
    }
}
