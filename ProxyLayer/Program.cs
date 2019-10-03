using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using McMaster.Extensions.CommandLineUtils;
using log4net;
using log4net.Config;

namespace ProxyLayer
{
    public class Program
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));

        static ConsoleGui consoleGui = ConsoleGui.GetInstance();
        static Server server = Server.GetInstance();

        static CommandOption optionServerConfigPath;
        static CommandLineApplication app;

        public static void Main(string[] args)
        {
            // Configure Logging
            var logRepo = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepo, new FileInfo("log4net.config.xml"));            

            // Read Command Line Arguments
            app = new CommandLineApplication();

            app.HelpOption();
            optionServerConfigPath = app.Option("-c|--config <CONFIG>",
                "Path to Server Configuration Json File",
                CommandOptionType.SingleValue);

            app.OnExecute(Start);

            app.Execute(args);
        }

        public static void Start()
        {
            log.Info("ProxyLayer Version " + Assembly.GetEntryAssembly().GetName().Version);
            
            string serverConfigPath = optionServerConfigPath.HasValue()
                ? optionServerConfigPath.Value()
                : "";

            if (!optionServerConfigPath.HasValue())
            {
                log.Info("--config argument not supplied, please provide a config file to use");
                app.ShowHelp();
            }
            else
            {
                consoleGui.Start();
                server.Start(serverConfigPath);
            }

            return;
        }

        public static void Stop()
        {
            log.Info("Stopping ProxyLayer");
            server.Stop();
            consoleGui.Stop();
            Environment.Exit(0);
        }
    }
}
