using System;
using System.Diagnostics;
using System.IO;

namespace IdleSharedLib
{
    public class Logger
    {
        private static string file = $"{DateTimeOffset.Now.ToUnixTimeSeconds()}-{Environment.ProcessId}-{Process.GetCurrentProcess().ProcessName}.txt";
        public static void Log(string message)
        {
            var formatedMessage = $"[LOG, {DateTime.Now}] " + message.Trim();
            Console.WriteLine(formatedMessage);

            var dir = $"{Path.GetTempPath()}/IdleAppStarter/";
            

            Directory.CreateDirectory(dir);
            File.AppendAllText(dir + file, formatedMessage + "\n");
        }
    }
}
