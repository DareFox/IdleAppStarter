using System;
using System.Diagnostics;
using System.IO;

namespace IdleSharedLib
{
    public class Logger
    {
        private static string file = $"{DateTimeOffset.Now.ToUnixTimeSeconds()}-{Environment.ProcessId}-{Process.GetCurrentProcess().ProcessName}.txt";
        private static string dir = Environment.CurrentDirectory + "\\logs\\";
        private static int textSizeLimit = 1_000_000; // ~ 1 MB
        public static void Log(string message)
        {
            var formatedMessage = $"[LOG, {DateTime.Now}] " + message.Trim() + "\n";

            Console.Write(formatedMessage);
            appendToFile(formatedMessage);
        }

        public static void LogToFile(string message)
        {
            var formatedMessage = $"[LOG, {DateTime.Now}] " + message.Trim() + "\n";
            appendToFile(formatedMessage);
        }

        private static void appendToFile(string message)
        {
            Directory.CreateDirectory(dir);
            if (File.Exists(dir + file))
            {
                try
                {
                    var croppedText = File.ReadAllText(dir + file) + message;
                    if (croppedText.Length > textSizeLimit)
                    {
                        croppedText = croppedText.Substring(croppedText.Length - textSizeLimit, textSizeLimit);
                    }
                    File.WriteAllText(dir + file, croppedText);
                } catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            } else {
                File.AppendAllText(dir + file, message);
            }
        }
    }
}
