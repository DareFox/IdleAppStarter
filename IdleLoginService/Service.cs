﻿using CommandLine;
using IdleSharedLib;
using NLog;
using System;
using System.Linq;
using System.Threading;
using ZetaIpc.Runtime.Server;

namespace IdleLoginService
{
    public class Service
    {
        static bool isServiceRunApps = false;
        static DateTime lastPing = DateTime.Now;
        static Logger logger = LogManager.GetCurrentClassLogger();
        static void Main(string[] args)
        {

            Parser.Default.ParseArguments<Config>(args)
                .WithParsed(WithParsed);
            try
            {
                Parser.Default.ParseArguments<Config>(args)
                .WithParsed(WithParsed)
                .WithNotParsed(ParserCMD.ArgumentError);
            } catch(Exception ex)
            {
                logger.Info(ex.ToString());
                throw;
            }
        }

        static void WithParsed(Config cfg)
        {
            logger.Info($"Service start at port {cfg.port}");
            var appStarter = new AppRunner(cfg.inputExec.ToList());
            var server = new IpcServer();
            server.Start(cfg.port);

            server.ReceivedRequest += RequestHandler;

            Parallel.Invoke(
                () => pingObserver(appStarter)
            );
        }

        private static void pingObserver(AppRunner appRunner)
        {
            double previousSeconds = 0.0;
            while(true)
            {
                Thread.Sleep(250);

                // Check if any user app is active
                // If no apps are not active, we suppose that we on logon screen
                // So we run apps from service instead of app
                double secondsFromLastPing = (DateTime.Now - lastPing).TotalSeconds;
                if (secondsFromLastPing >= 45)
                {
                    if (!isServiceRunApps)
                    {
                        logger.Info($"Service didn't get ping in {secondsFromLastPing} seconds. Starting executables");

                        // Don't run twice, they are already started
                        appRunner.runAll();

                        // Change status
                        isServiceRunApps = true;
                    }
                }
                else
                {
                    if (isServiceRunApps)
                    {
                        logger.Info($"Service got response from client app. Time spended in idle status: {previousSeconds} seconds. Killing all processes");

                        // Don't kill apps twice, they are already dead
                        appRunner.killAll();

                        // Change status
                        isServiceRunApps = false;
                    }
                }

                previousSeconds = secondsFromLastPing;
            }
        }

        private static void RequestHandler(object sender, ReceivedRequestEventArgs e)
        {
            lastPing = DateTime.Now;
            logger.Debug(e.Request);
        }
    }
}
