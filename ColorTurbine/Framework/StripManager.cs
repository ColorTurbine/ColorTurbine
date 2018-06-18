using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ColorTurbine
{
    public class StripManager
    {
        private List<IStrip> strips = new List<IStrip>();
        public List<IPlugin> plugins = new List<IPlugin>();
        public List<RuntimePlugin> runtimePlugins = new List<RuntimePlugin>();

        private Dictionary<string, IStrip> stripsByName = new Dictionary<string, IStrip>();
        private Dictionary<IStrip, List<IPlugin>> pluginsByStrip = new Dictionary<IStrip, List<IPlugin>>();

        public IStrip AddStrip(string name, IStrip strip)
        {
            strips.Add(strip);
            stripsByName.Add(name, strip);
            return strip;
        }


        // Indexable by name
        public IPlugin GetPluginByName(string name)
        {
            return plugins.Find(x => x.Name == name);
        }

        public IEnumerable<IPlugin> GetPluginsForStrip(IStrip strip)
        {
            return pluginsByStrip[strip];
        }

        public IEnumerable<IPlugin> GetPluginsByTag(string tag)
        {
            return plugins.Where(x => x.tags.Contains(tag));
        }

        // TODO Move strips to `Strips` inner class and index on that
        // Indexable by name
        public IStrip this[string key]
        {
            get
            {
                return stripsByName[key];
            }
            set
            {
                stripsByName[key] = value;
            }
        }

        public void LoadConfiguration()
        {
            var config = Services.Configuration.GetConfiguration();
            foreach (var sc in config.strips)
            {
                var s = Services.Configuration.CreateStrip(sc);
                this.AddStrip(sc.name, s);
                if(sc.plugins == null)
                {
                    continue;
                }
                
                foreach(var pc in sc.plugins)
                {
                    var gin = Services.Configuration.CreatePlugin(s, pc);
                    plugins.Add(gin);
                    if(!pluginsByStrip.ContainsKey(s))
                    {
                        pluginsByStrip.Add(s, new List<IPlugin>());
                    }
                    pluginsByStrip[s].Add(gin);
                }
            }

            foreach (var rp in config.runtimePlugins)
            {
                var gin = Services.Configuration.CreateRuntimePlugin(this, rp);
                runtimePlugins.Add(gin);
            }
        }

        public void RunForever()
        {
            // Kickoff scheduler
            if (!threadRunning && (pluginThread == null || !pluginThread.IsAlive))
            {
                threadRunning = true;
                pluginThread = new Thread(new ThreadStart(PluginThread));
                pluginThread.Start();
            }

            pluginThread.Join();
        }

        // Debugging
        Stopwatch sw;
        bool debugEnabled = false;
        long average = 0;
        long min = long.MaxValue;
        long max = 0;
        int debugCount = 0;
        int renderCount = 0;
        int paintCount = 0;

        DateTime refreshtimer;
        bool forceRefresh;

        Dictionary<IPlugin, PluginInfo> pluginInfo = new Dictionary<IPlugin, PluginInfo>();

        private class PluginInfo
        {
            public PluginInfo(string name) => this.name = name;

            string name;
            int crashCount;

            Queue<DateTime> lastFiveCrashes = new Queue<DateTime>(5);
            DateTime lastCrash;

            public void Crashed(Exception e)
            {
                if (lastFiveCrashes.Count() == 5)
                    lastFiveCrashes.Dequeue();

                lastCrash = DateTime.UtcNow;
                lastFiveCrashes.Enqueue(lastCrash);

                crashCount++;

                // Record log crash reason

                Console.WriteLine($"Plugin crashed: {name}");
                Console.WriteLine(e.ToString());
            }

            public bool DisabledForInstability()
            {
                if (lastFiveCrashes.Count == 0)
                    return false;

                if (DateTime.UtcNow - lastCrash < TimeSpan.FromMinutes(1))
                {
                    return true;
                }

                if (DateTime.UtcNow - lastFiveCrashes.Peek() < TimeSpan.FromMinutes(15))
                {
                    return true;
                }

                return false;
            }

            internal void RecordRenderDuration(TimeSpan elapsed)
            {
                if (elapsed > TimeSpan.FromMilliseconds(2))
                {
                    // Todo: If a plugin takes too long to execute, kill it or move it to a different thread
                    Console.WriteLine($"Plugin {name} took {elapsed.TotalMilliseconds} ms to complete");
                }
            }
        }

        private void HandlePluginCrash(IPlugin plugin, Exception e)
        {
            var thisPlugin = pluginInfo[plugin];
            thisPlugin.Crashed(e);
        }

        private async Task RenderActivePlugins()
        {
            Stopwatch rendertimer = new Stopwatch();
            foreach (var plugin in plugins)
            {
                var info = pluginInfo[plugin];
                if (!plugin.enabled || info.DisabledForInstability())
                {
                    continue;
                }

                try
                {
                    if (plugin.NeedsRender())
                    {
                        rendertimer.Reset();
                        rendertimer.Start();
                        await plugin.Render();
                        rendertimer.Stop();

                        info.RecordRenderDuration(rendertimer.Elapsed);

                        // Smudge plugin's strip
                        plugin.strip.dirty = true;

                        // Debugging
                        renderCount++;
                    }
                    // TODO: else notRenderCount++;
                }
                catch (Exception e)
                {
                    HandlePluginCrash(plugin, e);
                }
            }
        }

        // TODO: Separate into paint thread and render thread

        Thread pluginThread;
        bool threadRunning = false;

        private void PluginThread()
        {
            sw = new Stopwatch();

            foreach (var plugin in plugins)
            {
                pluginInfo[plugin] = new PluginInfo(plugin.GetType().Name);
            }

            while (true)
            {
                forceRefresh = false;
                renderCount = 0;
                paintCount = 0;
                sw.Reset();
                sw.Start();

                // Five second tick
                if (DateTime.UtcNow - refreshtimer > TimeSpan.FromSeconds(5))
                {
                    refreshtimer = DateTime.UtcNow;
                    forceRefresh = true;
                }

                lock (plugins)
                {
                    RenderActivePlugins().Wait();

                    // Clear smudged strips
                    foreach (var strip in strips)
                    {
                        if (strip.dirty)
                        {
                            strip.Clear();
                            paintCount++; // Debugging
                        }
                    }

                    foreach (var plugin in plugins)
                    {
                        if (!plugin.enabled || pluginInfo[plugin].DisabledForInstability())
                        {
                            continue;
                        }
                        try
                        {
                            // Render
                            if (plugin.strip.dirty)
                            {
                                plugin.Paint();
                            }
                        }
                        catch (Exception e)
                        {
                            HandlePluginCrash(plugin, e);
                        }
                    }
                }

                foreach (var strip in strips)
                {
                    strip.Send(forceRefresh);
                }

                sw.Stop();
                var time = sw.ElapsedMilliseconds;

                var sleep = 33 - (int)time;

                if (debugEnabled)
                {
                    min = Math.Min(sw.ElapsedTicks, min);
                    max = Math.Max(sw.ElapsedTicks, max);
                    average += sw.ElapsedTicks;

                    if (debugCount++ >= 10)
                    {
                        debugCount = 0;

                        Console.WriteLine($"Sleep {sleep} | Rendered {renderCount} frame(s), strips {paintCount}, Average {average / 10} ({TimeSpan.FromTicks(average / 10).TotalMilliseconds} ms), min {min} ({TimeSpan.FromTicks(min).TotalMilliseconds} ms), max {max} ({TimeSpan.FromTicks(max).TotalMilliseconds} ms) ticks");
                        average = 0;
                        min = int.MaxValue;
                        max = 0;
                    }
                }

                if (sleep > 0)
                {
                    Thread.Sleep(sleep);
                }
            }
        }
    }
}
