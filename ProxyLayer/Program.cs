using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ProxyLayer
{
    public class Program
    {
        public static bool UseSSL = false;

        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            string portFlag = args
                .Where(arg => arg.StartsWith("--port="))
                .SingleOrDefault()
                ?.Split("=")[1];
            int port = portFlag != null ? Convert.ToInt32(portFlag) : 5000;

            string sslPortFlag = args
                .Where(arg => arg.StartsWith("--sslPort="))
                .SingleOrDefault()
                ?.Split("=")[1];
            int sslPort = portFlag != null ? Convert.ToInt32(sslPortFlag) : 443;

            string certPath = args
                .Where(arg => arg.StartsWith("--certPath="))
                .SingleOrDefault()
                ?.Split("=")[1]
                ?.Split("'")[0];

            string certPassword = args
                .Where(arg => arg.StartsWith("--certPass="))
                .SingleOrDefault()
                ?.Split("=")[1];

            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Any, port);

                    if (!string.IsNullOrWhiteSpace(certPath))
                    {
                        UseSSL = true;
                        options.Listen(IPAddress.Any, sslPort, listenOptions =>
                        {
                            listenOptions.UseHttps(certPath, certPassword ?? "");
                        });
                    }
                    else
                    {
                        UseSSL = false;
                    }
                });
        }
    }
}
