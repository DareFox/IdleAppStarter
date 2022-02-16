using CommandLine;
using IdleSharedLib;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ZetaIpc.Runtime.Client;

namespace IdleUserApp
{
    class App
    {
        static bool isIdle = false;
        static bool isPingSuccess = false;
        static Logger logger = LogManager.GetCurrentClassLogger();
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Config>(args)
                .WithParsed(ParsedArgs)
                .WithNotParsed(ParserCMD.ArgumentError);
        }

        static void ParsedArgs(Config cfg)
        {
            logger.Info($"App start at port {cfg.port}");
            var runner = new AppRunner(cfg.inputExec.ToList());
            var client = new IpcClient();

            client.Initialize(cfg.port, 60000, System.Text.Encoding.UTF8);

            logger.Debug("Invoking pararell functions");
            Parallel.Invoke(
                    () => PingPong(client),
                    () => IdleCheck(cfg, runner)
                );
        }

        private static void IdleCheck(Config cfg, AppRunner runner)
        {
            logger.Info("Idle check invoked");
            var lastIdle = 0L;
            while (true)
            {
                Thread.Sleep(1000);
                var idle = IdleTimeFinder.GetIdleTime();

                // TODO: Simplify if's
                if (idle > cfg.idle)
                {
                    if (!isIdle && isPingSuccess) // Status was changed
                    {
                        logger.Info($"Current idle time ({idle}ms) > Trigger idle ({cfg.idle}ms)");
                        isIdle = true;
                        runner.runAll();
                    }
                }
                else
                {
                    if (isIdle) // Status was changed
                    {
                        logger.Info($"Activity detected. Time spended in idle: {lastIdle}ms");
                        isIdle = false;
                        runner.killAll();
                    }
                }

                lastIdle = idle;
            }
        }

        static void PingPong(IpcClient client)
        {
            logger.Debug("Ping Pong invoked");
            while(true)
            {
                Thread.Sleep(5000);
                try
                {
                    var msg = new Message();
                    msg.IsIdle = isIdle;
                    msg.ProcessId = Process.GetCurrentProcess().Id;

                    client.Send($"{JsonConvert.SerializeObject(msg)}");
                    isPingSuccess = true;
                } catch (WebException ex)
                {
                    logger.Debug(ex.ToString());
                    logger.Info("Can't connect to Service. App will not start any processes, until establishing connection");
                    isPingSuccess = false;
                }
            }
        }
    }
}
