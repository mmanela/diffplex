using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using DiffPlex.DiffBuilder;
using DiffPlex;

namespace WebDiffer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .ConfigureServices((services) =>
                {
                    services.AddScoped<ISideBySideDiffBuilder, SideBySideDiffBuilder>();
                    services.AddScoped<IDiffer, Differ>();
                })
                .Build();

            host.Run();
        }
    }
}
