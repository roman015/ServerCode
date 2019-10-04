using Terminal.Gui;
using System.Threading;
using System;
using log4net;
using log4net.Core;
using log4net.Appender;
using System.IO;
using System.Text;

namespace ProxyLayer
{
    // Singleton Class used to display Console GUI
    // Creates a separate thread to display the GUI
    public class ConsoleGui
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ConsoleGui));
        public static TextView logTextView;
        private readonly int USAGEPOLLDURATIONMS = 10000;
        public static Label usageLabel;
        private static ConsoleGui Singleton;
        private static Thread GuiThread;

        private ConsoleGui() { }

        public static ConsoleGui GetInstance()
        {
            Singleton = Singleton ?? new ConsoleGui();
            return Singleton;
        }

        public void Start()
        {
            GuiThread = GuiThread ?? new Thread(StartThread);

            if (GuiThread.ThreadState != ThreadState.Running)
            {
                SetupGUI();
                GuiThread.Start();
                log.Info("Console GUI Ready.");
            }
        }

        public void Stop()
        {
            if (GuiThread != null && GuiThread.ThreadState == ThreadState.Running)
            {
                log.Info("Stopping Console GUI");
                GuiThread.IsBackground = true;
                Application.MainLoop.Invoke(() =>
                {
                    Application.MainLoop.Stop();
                });
            }
        }

        private void SetupGUI()
        {
            Application.Init();
            Application.UseSystemConsole = true;
            var top = Application.Top;

            // Creates the top-level window to show
            var win = new Window("ProxyLayer")
            {
                X = 0,
                Y = 1, // Leave one row for the toplevel menu

                // By using Dim.Fill(), it will automatically resize without manual intervention
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            top.Add(win);

            // Creates a menubar
            var menu = new MenuBar(new MenuBarItem[] {
            new MenuBarItem ("_File", new MenuItem [] {
                new MenuItem ("_Quit", "", QuitTriggered)
            })
            });
            top.Add(menu);

            // Label to display CPU, Mem & HDD Usage
            usageLabel = new Label(0, 0, "USAGE - ");
            win.Add(usageLabel);

            // TextView for displaying Logs
            logTextView = new TextView()
            {
                X = 0,
                Y = 1,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            logTextView.Text = "";
            logTextView.ReadOnly = true;
            win.Add(logTextView);
        }

        private void StartThread()
        {
            // Start timer to update Usage Statistics
            Timer usageTimer = new Timer(UpdateUsageTriggered, null, 0, USAGEPOLLDURATIONMS);

            // This is a blocking call
            Application.Run();
        }

        private void UpdateUsageTriggered(object arg)
        {
            Application.MainLoop.Invoke(() =>
            {
                // TODO : GET CPU USAGE
                usageLabel.Text = "USAGE - MEM : " + (GC.GetTotalMemory(false) / 1024) + "KB";
            });
        }

        private void QuitTriggered()
        {
            log.Info("ProxyLayer Terminated Via Quit Menu Option");            
            Program.Stop();
        }
    }

    // Class used to append logs to ConsoleGui
    public class ConsoleGuiAppender : AppenderSkeleton
    {
        private static readonly int MAXGUILOGLENGTH = 6000;
        static StringBuilder logStringBuilder = new StringBuilder();

        protected override void Append(LoggingEvent loggingEvent)
        {
            AppendToLog(RenderLoggingEvent(loggingEvent));
        }

        public static void AppendToLog(String newLog)
        {
            TrimExcessLogs();

            logStringBuilder.Append(
                newLog.Trim().Replace("\r", "")
                + "\n"
            );

            if (ConsoleGui.logTextView != null)
            {
                Application.MainLoop.Invoke(() =>
                {
                    ConsoleGui.logTextView.Text = logStringBuilder.ToString();
                });
            }
        }

        public static void TrimExcessLogs()
        {
            while (logStringBuilder.Length > MAXGUILOGLENGTH)
            {
                int lineEnd = logStringBuilder.ToString().IndexOf("\n");
                if (lineEnd > 0)
                    logStringBuilder = logStringBuilder.Remove(0, lineEnd + 1);
                else
                    logStringBuilder = logStringBuilder.Clear();
            }
        }
    }
}