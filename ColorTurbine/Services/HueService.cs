using System;
using System.Linq;
using System.Collections.Generic;
using Q42.HueApi.Interfaces;
using Q42.HueApi;
using System.Threading.Tasks;
using Q42.HueApi.Models.Bridge;
using Q42.HueApi.ColorConverters.OriginalWithModel;

namespace ColorTurbine
{
    public class HueService
    {
        ILocalHueClient client;

        public HueService()
        {
            var config = Services.Configuration.GetServiceConfiguration("hue");

            IBridgeLocator locator = new HttpBridgeLocator();

            // > For Windows 8 and .NET45 projects you can use the SSDPBridgeLocator which actually scans your network. 
            // > See the included BridgeDiscoveryTests and the specific .NET and .WinRT projects
            var bridges = locator.LocateBridgesAsync(TimeSpan.FromSeconds(60));
            bridges.Wait();
            IEnumerable<LocatedBridge> bridgeIPs = bridges.Result;

            client = new LocalHueClient(bridgeIPs.First().IpAddress);

            string appkey = config["appKey"];
            client.Initialize(appkey);
        }
    }
}
