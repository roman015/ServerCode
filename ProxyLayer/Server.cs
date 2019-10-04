using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using log4net;
using log4net.Config;
using System.Threading;
using ProxyLayer.Middlewares;


namespace ProxyLayer
{
    public class Server
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Server));

        private static string ServerConfigurationPath;
        public static IConfiguration ServerConfiguration;
        private static IWebHostBuilder webHostBuilder;
        private static IWebHost webHost;
        private static Thread ServerThread;
        private static Server Singleton;

        private Server() { }

        public static Server GetInstance()
        {
            Singleton = Singleton ?? new Server();
            return Singleton;
        }

        public void Start(string configFilePath)
        {
            ServerConfigurationPath = configFilePath;
            webHostBuilder = CreateWebHostBuilder(ServerConfigurationPath);

            Stop(); // Stop Any Currently Running WebHost

            webHost = webHostBuilder.Build();
            log.Info("Starting WebHost");
            ServerThread = ServerThread ?? new Thread(() =>{
                 webHost.Run();
             });

            if (ServerThread.ThreadState != ThreadState.Running)
            {                
                ServerThread.Start();
                log.Info("WebHost Ready");
            }
            
        }

        public void Stop()
        {
            if (webHost != null)
            {
                log.Debug("Existing WebHost is Being Stopped");
                webHost.StopAsync(new TimeSpan(0, 1, 0)).Wait();
                ServerThread.IsBackground = true;
                log.Debug("Existing WebHost Stopped");
            }
        }

        IWebHostBuilder CreateWebHostBuilder(string configFilePath)
        {

            log.Info("Using Config file " + ServerConfigurationPath);
            ServerConfiguration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(path: configFilePath, optional: false, reloadOnChange: true)
                    .Build();

            int.TryParse(ServerConfiguration["ServerPort"], out int serverPort);
            int.TryParse(ServerConfiguration["SSL:Port"], out int sslPort);
            string certPath = ServerConfiguration["SSL:CertPath"];
            string certPassword = ServerConfiguration["SSL:CertPassword"];

            return WebHost.CreateDefaultBuilder()
                .UseStartup<ServerStartup>()
                .UseKestrel(options =>
                {
                    log.Info("Server Port : " + serverPort);
                    options.ListenAnyIP(serverPort);

                    if (sslPort > 0)
                    {
                        log.Info("SSL Port : " + serverPort);
                        log.Info("Cert Path : " + certPath);
                        log.Info("Cert Pass : " + string.IsNullOrWhiteSpace(certPassword));
                        ServerStartup.IsSSLUsed = true;
                        options.Listen(IPAddress.Any, sslPort, listenOptions =>
                        {
                            listenOptions.UseHttps(certPath, certPassword ?? "");
                        });
                    }
                });
        }
    }

    public class ServerStartup
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ServerStartup));

        public static bool IsSSLUsed = false;
        public IConfiguration Configuration { get; }

        public ServerStartup(IConfiguration configuration)
        {
            Configuration = Server.ServerConfiguration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options => options.AddPolicy("ProxyLayerPolicy", builder =>
            {
                builder.AllowAnyHeader();
                builder.AllowAnyMethod();
                builder.AllowAnyOrigin();
            }));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSingleton<IConfiguration>(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddLog4Net();
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                if (IsSSLUsed)
                {
                    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                    app.UseHsts();

                    app.UseHttpsRedirection();
                }
            }

            // Setup CORS
            // TODO : Limit this 
            app.UseCors("ProxyLayerPolicy");

            // Add Request Filter
            app.UseRequestFilter(Configuration);

            // Configure all mappings from appsettings.json
            int count = 0;
            while (!String.IsNullOrEmpty(Configuration["Mappings:" + count + ":Pattern"]))
            {
                // Read Mapping values
                string pattern = Configuration["Mappings:" + count + ":Pattern"];
                string host = Configuration["Mappings:" + count + ":Host"];
                int port = Convert.ToInt32(Configuration["Mappings:" + count + ":Port"]);

                // Create Mapping
                Mapping mapping = new Mapping(pattern, host, port);

                // Configure Mapping
                app.MapWhen(mapping.IsConfiguredPath, builder => builder.RunProxy(mapping.ProxyOptions));

                // For Console Logging
                log.Info("Mapped " + pattern + " to " + host + ":" + port);

                // Goto Next Mapping
                count++;
            }

            app.UseMvc();
        }
    }

    public class Mapping
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Mapping));
        public string Pattern { get; set; }
        public int Port { get; set; }
        public string Host { get; set; }

        public Mapping(string pattern, string host, int port)
        {
            Pattern = pattern;
            Host = host;
            Port = port;
        }

        public ProxyOptions ProxyOptions => new ProxyOptions()
        {
            Scheme = "http",
            Host = Host,
            Port = Port.ToString(),
        };

        public Func<HttpContext, bool> IsConfiguredPath
        {
            get
            {
                return delegate (HttpContext httpContext)
                {
                    if (Pattern.StartsWith("/"))
                    {
                        return httpContext.Request.Path.Value
                            .StartsWith(Pattern, StringComparison.OrdinalIgnoreCase);
                    }
                    else
                    {
                        return httpContext.Request.Host.Value
                            .StartsWith(Pattern, StringComparison.OrdinalIgnoreCase);
                    }
                };
            }
        }
    }
}