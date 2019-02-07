using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProxyLayer.Formatters;
using ProxyLayer.Middlewares;

namespace ProxyLayer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static IConfiguration Configuration => _configuration;
        private static IConfiguration _configuration;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                options.OutputFormatters.Add(new OctetOutputFormatter());
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddSingleton<IConfiguration>(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            if (Program.UseSSL)
            {
                // Redirect to Https
                app.UseHttpsRedirection();
            }

            // Add Console Logging
            app.UseRequestLogger();

            // Add Request Filter
            app.UseRequestFilter(Configuration);

            // Configure all mappings from appsettings.json
            int count = 0;
            while (!String.IsNullOrEmpty(Configuration["Mappings:" + count + ":Pattern"]))
            {
                // Read Mapping values
                string pattern = Configuration["Mappings:" + count + ":Pattern"];
                int port = Convert.ToInt32(Configuration["Mappings:" + count + ":Port"]);

                // Create Mapping
                Mapping mapping = new Mapping(pattern, port);

                // Configure Mapping
                app.MapWhen(mapping.IsConfiguredPath, builder => builder.RunProxy(mapping.ProxyOptions));

                // For Console Logging
                Console.WriteLine("Mapped " + pattern + " to " + port);

                // Goto Next Mapping
                count++;
            }
                        
            app.UseMvc();
        }
    }
}
