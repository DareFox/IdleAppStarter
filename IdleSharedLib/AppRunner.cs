using System;
using System.Collections.Generic;
using System.Diagnostics;

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
                    item.Kill();
                }
                catch (Exception ex)
                {
                    Logger.Log("err: " + ex);
                }
            }

            Logger.Log("All processes are killed");
            processes.Clear();
        }
    }
}
