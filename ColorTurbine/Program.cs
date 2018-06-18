using System;
using System.IO;
using System.Reflection;

namespace ColorTurbine
{
    class Program
    {
        static void Main(string[] args)
        {
            var configWatcher = new FileSystemWatcher();
            configWatcher.Path = Environment.CurrentDirectory;
            configWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.Size;
            configWatcher.Changed += (_, fschange) =>
            {
                if(fschange.Name != "config.json")
                    return;

                Console.WriteLine("HACK: Die and let docker-compose reload us to get new configuration.");
                Environment.Exit(0);
            };
            configWatcher.EnableRaisingEvents = true;

            Console.WriteLine("Initializing");

            var manager = new StripManager();
            manager.LoadConfiguration();

            Console.WriteLine("Running");
            manager.RunForever();
        }
    }
}
