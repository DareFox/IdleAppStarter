using IdleSharedLib;
using IdleSharedLib.WinAPI;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using ZetaIpc.Runtime.Server;
using static IdleSharedLib.WinAPI.ProcessExtensions;

namespace IdleLoginService
{
    class ClientManager : IDisposable
    {
        // Program arguments
        private int port;
        private long idleTime;
        private List<string> executables;

        private Logger logger = LogManager.GetCurrentClassLogger();
        private long timeoutMS; // in milliseconds, 1000ms = 1sec
        private IpcServer server;

        // Process control
        private Nullable<int> clientID = 0;
        private Timer timeoutTimer;

        /// <summary>
        /// Called on ping from client. Event provides this instance of ClientManager
        /// </summary>
        public event Action<ClientManager, Message> onPing;

        /// <summary>
        /// Called when client isn't responding. Event provides this instance of ClientManager
        /// </summary>
        public event Action<ClientManager> onTimeout;

        public ClientManager(ServiceConfig cfg)
        {
            port = cfg.port;
            idleTime = cfg.idle;
            executables = cfg.inputExec.ToList();
            server = new IpcServer();
            timeoutMS = cfg.connectionTimeoutMS;

            server.Start(port);
            server.ReceivedRequest += MessageHandler;
        }

        /// <summary>
        /// Create client-side process.
        /// </summary>
        /// <returns>If process was started successfully — returns true. If not — false</returns>
        public bool Start()
        {
            var success = true;
            try
            {
                var exeWithQuotes = executables.Select(exeWithArgs => "\"" + exeWithArgs + "\"");

                var process = StartProcessAsCurrentUser(
                    appPath: "app/IdleUserApp.exe",
                    cmdLine: $"app/IdleUserApp.exe -i {idleTime} -p {port} -e {string.Join(" ", exeWithQuotes)}",
                    visible: false);

                clientID = (int)process.dwProcessId;
                timeoutTimer = new Timer(Timeout, null, timeoutMS, 0);
            }
            catch (SessionUserTokenException)
            {
                logger.Info("Can't create client. Maybe IdleLoginService was executed not from SYSTEM user or there's no active user sessions right now");
                clientID = null;
                success = false;
            }
            return success;
        }

        /// <summary>
        /// Kill client-side process
        /// </summary>
        public void Kill()
        {
            if (clientID != null)
            {
                try
                {
                    var process = Process.GetProcessById((int)clientID);

                    // After client crash, another process can have same id as client previously
                    // So also check by process name.
                    // If it's client process name - kill it, else - do nothing
                    if (process.ProcessName == "IdleUserApp")
                    {
                        ProcessKillExtensions.KillProcessAndChildren((int)clientID);
                    }
                } catch (ArgumentException)
                {
                    logger.Info($"Process with an id of {clientID} is not running");
                }
            }
        }

        private void Timeout(object? state)
        {
            logger.Info($"Timeout from client id {clientID}");
            onTimeout.Invoke(this);
        }

        private void MessageHandler(object sender, ReceivedRequestEventArgs e)
        {
            logger.Info(e.Request);
            timeoutTimer?.Change(timeoutMS, 0);
            onPing.Invoke(this, JsonConvert.DeserializeObject<Message>(e.Request));
        }

        public void Dispose()
        {
            timeoutTimer.Dispose();
        }
    }
}
