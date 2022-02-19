using CommandLine;
using IdleSharedLib;
using Newtonsoft.Json;
using NLog;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ZetaIpc.Runtime.Client;

namespace IdleUserApp
{
    class App : IDisposable
    {
        private const int pingPeriod = 5000; // in ms; 5 seconds
        private const int idleCheckPeriod = 1000; // in ms; 1 second

        private static long lastIdleTime;
        private static bool lastCheckIdle;
        private static bool isIdle;
        private static bool isPingSuccess;

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static IpcClient client;
        private static AppRunner runner;
        private static Config config;

        private static Timer idleCheckTimer;
        private static Timer pingPongTimer;
        static void Main(string[] args)
        {
            try
            {
                Parser.Default.ParseArguments<Config>(args)
                .WithParsed(ParsedArgs)
                .WithNotParsed(ParserCMD.ArgumentError);
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
            }
        }

        static void ParsedArgs(Config cfg)
        {
            config = cfg;

            logger.Info($"App start at port {cfg.port}");
            client = new IpcClient();
            client.Initialize(cfg.port, 60000, System.Text.Encoding.UTF8);

            runner = new AppRunner(cfg.inputExec.ToList());

            new Timer(PingPong, null, 0, pingPeriod);
            new Timer(IdleCheck, null, 0, idleCheckPeriod);

            while (true)
            {
                Thread.Sleep(int.MaxValue);
            }
        }

        private static void IdleCheck(object _)
        {
            logger.Trace("Idle check invoked");
            var idle = IdleTimeFinder.GetIdleTime();

            if (idle > config.idle)
            {
                isIdle = true;
                if (isIdle != lastCheckIdle) // Call code block only when status changes
                {
                    logger.Info($"Current idle time ({idle}ms) > Trigger idle ({config.idle}ms)");
                    runner.runAllOnce();
                }
            }
            else
            {
                isIdle = false;
                if (isIdle != lastCheckIdle) // Call code block only when status changes
                {
                    logger.Info($"Activity detected. Time spended in idle: {lastIdleTime}ms");
                    runner.killAll();
                }
            }

            lastIdleTime = idle;
            lastCheckIdle = isIdle;
        }

        static void PingPong(object _)
        {
            logger.Trace("Ping Pong invoked");
            try
            {
                var msg = new Message();
                msg.IsIdle = isIdle;
                msg.ProcessId = Process.GetCurrentProcess().Id;

                client.Send(JsonConvert.SerializeObject(msg));
                isPingSuccess = true;
            }
            catch (WebException)
            {
                logger.Info("Can't connect to Service. App will not start any processes, until establishing connection");
                isPingSuccess = false;
            }
        }

        void IDisposable.Dispose()
        {
            idleCheckTimer.Dispose();
            pingPongTimer.Dispose();
        }
    }
}
