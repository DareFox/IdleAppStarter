using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static IdleSharedLib.WinAPI.ProcessKillExtensions;

namespace IdleSharedLib
{
    public class AppRunner
    {
        private readonly List<string> _executables;
        private List<Process> processes = new List<Process>();
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private bool running;

        public AppRunner(List<string> executables)
        {
            _executables = executables;
        }

        public void runAllOnce()
        {
            if (!running)
            {
                foreach (var item in _executables)
                {
                    try
                    {
                        logger.Info($"Trying to execute {item}");

                        var splittedCommand = item.Split(" ").ToList();

                        var file = splittedCommand[0];
                        var arguments = splittedCommand.Skip(1);

                        var process = Process.Start(new ProcessStartInfo
                        {
                            FileName = file,
                            UseShellExecute = false,
                            Arguments = String.Join(" ", arguments)
                        });

                        processes.Add(process);
                    }
                    catch (Exception ex)
                    {
                        logger.Error("err: " + ex);
                    }
                }

                running = true;
                logger.Info("All executables was started");
            }
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void killAll()
        {
            if (running)
            {
                foreach (var item in processes)
                {
                    try
                    {
                        logger.Info($"Trying to kill ${item.ProcessName} (id: {item.Id})");
                        KillProcessAndChildren(item.Id);
                    }
                    catch (Exception ex)
                    {
                        logger.Error("err: " + ex);
                    }
                }

                logger.Info("All processes are killed");
                processes.Clear();
            }

            running = false;
        }
    }
}
