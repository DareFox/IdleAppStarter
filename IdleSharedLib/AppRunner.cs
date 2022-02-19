using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;

namespace IdleSharedLib
{
    public class AppRunner
    {
        private List<string> _executables;
        private List<Process> processes = new List<Process>();
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private bool running = false;

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

                        var process = Process.Start(new ProcessStartInfo()
                        {
                            FileName = item,
                            UseShellExecute = false,
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

        private static void KillProcessAndChildren(int pid)
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher
              ("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection moc = searcher.Get();
            foreach (ManagementObject mo in moc)
            {
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            }
            try
            {
                Process proc = Process.GetProcessById(pid);
                proc.Kill();
            }
            catch (ArgumentException)
            {
                // Process already exited.
            }
        }
    }
}
