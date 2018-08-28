using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static ColorTurbine.StripManager;

namespace ColorTurbine
{
    public enum StripType
    {
        RGB,
        RGBW,
        RGB2D,
        RGBW2D
    }

    [DebuggerDisplay("{plugin} {enabled}")]
    public class PluginConfig
    {
        public string plugin { get; set; }
        public string name { get; set; }
        public string location { get; set; }
        public string assembly { get; set; }
        public bool? enabled { get; set; }
        public IEnumerable<string> tags { get; set; }

        public dynamic this[string key]
        {
            get
            {
                if (_additionalData.ContainsKey(key))
                    return _additionalData[key];
                return null;
            }
        }

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData = null;
    }

    [DebuggerDisplay("{name} {type} {address}")]
    public class StripConfig
    {
        public string name { get; set; }
        public string address { get; set; }
        public string mdns { get; set; }
        public StripType type { get; set; }
        public int ledCount { get; set; }

        // 2D
        public int width { get; set; }
        public int height { get; set; }
        public bool mirroredX { get; set; }

        public List<PluginConfig> plugins {get;set;}
    }

    public class ServiceConfig
    {
        public dynamic this[string key]
        {
            get
            {
                if (_additionalData.ContainsKey(key))
                    return _additionalData[key];
                return null;
            }
        }

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData = null;
    }

    public class RuntimePluginConfig
    {
        public string assembly { get; set; }

        public string binary { get; set; }

        public string name { get; set; }

        public dynamic this[string key]
        {
            get
            {
                if (_additionalData.ContainsKey(key))
                    return _additionalData[key];
                return null;
            }
        }

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData = null;
    }

    public class Configuration
    {
        public List<StripConfig> strips { get; set; }
        public Dictionary<string, ServiceConfig> services { get; set; }
        public List<RuntimePluginConfig> runtimePlugins { get; set; }
    }

    public class ConfigurationService
    {
        Configuration configuration;
        public ConfigurationService()
        {
            var configfile = "config.json";
            var configContents = File.ReadAllText(configfile);
            configuration = JsonConvert.DeserializeObject<Configuration>(configContents);
        }

        public IPlugin CreatePlugin(IStrip s, PluginConfig pc)
        {
            bool? config = pc.enabled;

            // TODO: Support loading plugins from external assemblies
            // Voodoo: create plugin
                // Voodoo: create plugin
            System.Reflection.Assembly asy;
            if (pc.assembly == null)
            {
                asy = System.Reflection.Assembly.GetExecutingAssembly();
            }
            else
            {
                asy = System.Reflection.Assembly.LoadFrom(pc.assembly);
            }
            IPlugin gin = (IPlugin)asy.CreateInstance(pc.plugin);
            if(gin == null)
            {
                // throw something worthwhile
                throw new Exception($"Could not create plugin: {pc.plugin}");
            }

            gin.Initialize(s, pc); // TODO Move this to constructor
            gin.enabled = config ?? true; // TODO: Plugin should handle this
            if(pc.tags != null)
                gin.tags.AddRange(pc.tags);
            gin.Name = pc.name;
            return gin;
        }

        public RuntimePlugin CreateRuntimePlugin(StripManager m, RuntimePluginConfig rp)
        {
            try
            {
                // Voodoo: create plugin
                System.Reflection.Assembly asy;
                if (rp.assembly == null)
                {
                    asy = System.Reflection.Assembly.GetExecutingAssembly();
                }
                else
                {
                    asy = System.Reflection.Assembly.LoadFrom(rp.assembly);
                }
                RuntimePlugin gin = (RuntimePlugin)asy.CreateInstance(rp.binary);
                gin.Initialize(m, rp);

                return gin;
            }
            catch (Exception e)
            {
                Console.WriteLine("Crashed while creating RuntimePlugin");
                return null;
            }
        }

        public IStrip CreateStrip(StripConfig sc)
        {
            IStrip s;
            switch (sc.type)
            {
                case StripType.RGB:
                    {
                        s = new RGBStrip(sc.name, IPAddress.Parse(sc.address), sc.ledCount);
                    }
                    break;
                case StripType.RGBW:
                    {
                        s = new RGBWStrip(sc.name, IPAddress.Parse(sc.address), sc.ledCount);
                    }
                    break;
                case StripType.RGBW2D:
                    {
                        s = new RGBWStrip2D(sc.name, IPAddress.Parse(sc.address), sc.width, sc.height, sc.mirroredX);
                    }
                    break;
                default:
                    {
                        throw new ArgumentOutOfRangeException("Unsupported strip type");
                    }
            }
            s.Name = sc.name;
            return s;
        }

        public ServiceConfig GetServiceConfiguration(string section)
        {
            if(configuration.services == null || !configuration.services.ContainsKey(section))
                return null;
            return configuration.services?[section];
        }

        public Configuration GetConfiguration()
        {
            return configuration;
        }

        public IEnumerable<StripConfig> GetStrips()
        {
            return configuration.strips;
        }
    }
}