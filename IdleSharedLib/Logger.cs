using System;
using System.Diagnostics;
using System.IO;

namespace IdleSharedLib
{
    public class Logger
    {
        private static string file = $"{DateTimeOffset.Now.ToUnixTimeSeconds()}-{Environment.ProcessId}-{Process.GetCurrentProcess().ProcessName}.txt";
        private static string dir = Environment.CurrentDirectory + "/logs";
        public static void Log(string message)
        {
            var formatedMessage = $"[LOG, {DateTime.Now}] " + message.Trim();

            Console.WriteLine(formatedMessage);

            Directory.CreateDirectory(dir);
            File.AppendAllText(dir + file, formatedMessage + "\n");
        }

        public static void LogToFile(string message)
        {
            var formatedMessage = $"[LOG, {DateTime.Now}] " + message.Trim();

            Directory.CreateDirectory(dir);
            File.AppendAllText(dir + file, formatedMessage + "\n");
        }
    }
}
