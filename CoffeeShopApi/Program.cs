using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoffeeShopApi {
    public class Program {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var configuration = new ConfigurationBuilder()
                                            .AddCommandLine(args)
                                            .Build();
                    var hostUrl = configuration["hosturl"];
                    if (string.IsNullOrEmpty(hostUrl))
                        hostUrl = "http://0.0.0.0:6000";

                    webBuilder.UseStartup<Startup>().UseUrls(hostUrl);
                });
    }
}
