using CommandLine;
using IdleSharedLib;
using NLog;
using System;
using System.Linq;
using System.Threading;

namespace IdleLoginService
{
    public class Service
    {
        static Logger logger = LogManager.GetCurrentClassLogger();
        static AppRunner runner;
        static ClientManager manager;

        static void Main(string[] args)
        {
            try
            {
                Parser.Default.ParseArguments<ServiceConfig>(args)
                .WithParsed(LaunchService)
                .WithNotParsed(ParserCMD.ArgumentError);
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
                throw;
            }
        }

        static void LaunchService(ServiceConfig cfg)
        {
            logger.Info("Launched service");
            runner = new AppRunner(cfg.inputExec.ToList());
            manager = new ClientManager(cfg);

            manager.onTimeout += onTimeoutService;
            manager.onPing += (ClientManager client, Message msg) => runner.killAll();

            StartClientUntilSuccess();

            while (true)
            {
                Thread.Sleep(int.MaxValue);
            }
        }

        private static void StartClientUntilSuccess() // TODO: Change naming to something better than this
        {
            var started = false;
            var runAttempts = 0;
            // Initialize client
            while (!started)
            {
                started = manager.Start();
                runAttempts++;

                if (started)
                {
                    logger.Trace("SERVICE - client started");
                    runAttempts = 0;
                }
                // After X attemps run apps from service
                // And try start client again and again, until it will be launched
                else if (runAttempts > 1)
                {
                    runner.runAllOnce();
                }

                Thread.Sleep(10000); // Cooldown before starting again
            }
        }

        private static void onTimeoutService(ClientManager clientManager)
        {
            logger.Trace("SERVICE - onTimeoutService invoked");
            clientManager.Kill();
            StartClientUntilSuccess();
        }
    }
}
