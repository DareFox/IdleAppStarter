using CommandLine;
using IdleSharedLib;
using System;
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
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Config>(args)
                .WithParsed(ParsedArgs);
        }

        static void ParsedArgs(Config cfg)
        {
            Logger.Log($"App start at port {cfg.port}");
            var runner = new AppRunner(cfg.inputExec.ToList());
            var client = new IpcClient();

            client.Initialize(cfg.port, 60000, System.Text.Encoding.UTF8);

            Parallel.Invoke(
                    () => PingPong(client),
                    () => IdleCheck(cfg, runner)
                );
            
        }

        private static void IdleCheck(Config cfg, AppRunner runner)
        {
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
                        Logger.Log($"Current idle time ({idle}ms) > Trigger idle ({cfg.idle}ms)");
                        isIdle = true;
                        runner.runAll();
                    }
                }
                else
                {
                    if (isIdle) // Status was changed
                    {
                        Logger.Log($"Activity detected. Time spended in idle: {lastIdle}ms");
                        isIdle = false;
                        runner.killAll();
                    }
                }

                lastIdle = idle;
            }
        }

        static void PingPong(IpcClient client)
        {
            while(true)
            {
                Thread.Sleep(1000);
                try
                {
                    client.Send($"Ping! ID: {Process.GetCurrentProcess().Id}; isIdle: {isIdle}");
                    isPingSuccess = true;
                } catch (WebException ex)
                {
                    Logger.LogToFile(ex.ToString());
                    Logger.Log("Can't connect to Service. App will not start any processes, until establishing connection");
                    isPingSuccess = false;
                }
            }
        }
    }
}
