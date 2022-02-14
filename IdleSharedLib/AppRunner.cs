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

        public AppRunner(List<string> executables)
        {
            _executables = executables;
        }

        public void runAll()
        {
            foreach (var item in _executables)
            {
                try
                {
                    Logger.Log($"Trying to execute {item}");
                    processes.Add(Process.Start(new ProcessStartInfo()
                    {
                        FileName = item,
                        UseShellExecute = false
                    }));
                }
                catch (Exception ex)
                {
                    Logger.Log("err: " + ex);
                }
            }

            Logger.Log("All executables was started");
        }

        public void killAll()
        {
            foreach (var item in processes)
            {
                try
                {
                    Logger.Log($"Trying to kill ${item.ProcessName} (id: {item.Id})");
                    KillProcessAndChildren(item.Id);
                }
                catch (Exception ex)
                {
                    Logger.Log("err: " + ex);
                }
            }

            Logger.Log("All processes are killed");
            processes.Clear();
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
