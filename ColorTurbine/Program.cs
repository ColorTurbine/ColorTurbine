using System;
using System.IO;
using System.Reflection;
using Hangfire;
using Hangfire.MemoryStorage;

namespace ColorTurbine
{
    class Program
    {
        static void Main(string[] args)
        {
            BackgroundJobServer server = null;
            try
            {
                // Initialize Hangfire
                GlobalConfiguration.Configuration.UseMemoryStorage();
                server = new BackgroundJobServer();

                // Reload config.json on change
                var configWatcher = new FileSystemWatcher();
                configWatcher.Path = Environment.CurrentDirectory;
                configWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.Size;
                configWatcher.Changed += (_, fschange) =>
                {
                    if (fschange.Name != "config.json")
                        return;

                    Console.WriteLine("HACK: Die and let docker-compose reload us to get new configuration.");
                    Environment.Exit(0);
                };
                configWatcher.EnableRaisingEvents = true;

                // Load strips and start main loop
                Console.WriteLine("Initializing");
                var manager = new StripManager();
                manager.LoadConfiguration();

                Console.WriteLine("Running");
                manager.RunForever();
            }
            catch (Exception)
            {
                server?.Dispose();
                throw;
            }
        }
    }
}
