using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Plasma.Server;

namespace Plasma {
    public class Program {
        public static async Task Main(string[] args) {
            Console.Title = "Plasma Socket -> OSC Server";

            IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) => {
                config.AddEnvironmentVariables();

                if (args is null) {
                    throw new ArgumentNullException("Missing parameters");
                }
                config.AddCommandLine(args);
            })
            .ConfigureServices((context, collection) => {
                collection.AddSingleton<IHostedService, SocketServer>();
                collection.AddLogging();
                collection.Configure<SocketServer>(context.Configuration);
            })
            .ConfigureLogging((context, builder) => {
                builder.ClearProviders();
                builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                builder.AddConsole();
                
            })
            .Build();

            await host.RunAsync();
        }
        private static void OnPacketReceived(object sender, PacketReceivedEventArgs args) {
            Console.WriteLine(args.content);
        }
    }
}

