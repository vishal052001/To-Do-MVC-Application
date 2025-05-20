using System;
using System.IO;

namespace ToDoListAppMVC.Utilities
{
    public static class Logger
    {
        private static readonly string LogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

        static Logger()
        {
            Directory.CreateDirectory(LogDirectory);
        }

        public static void LogInfo(string message, string module = "General")
        {
            Log(message, "INFO", module);
        }

        public static void LogError(string message, Exception ex, string module = "General")
        {
            string fullMessage = $"{message} | Exception: {ex.Message} | StackTrace: {ex.StackTrace}";
            Log(fullMessage, "ERROR", module);
        }
        public static void LogWarning(string message, string module = "General")
        {
            Log(message, "WARN", module);
        }

        private static void Log(string message, string logType, string module)
        {
            try
            {
                string logFile = Path.Combine(LogDirectory, $"{module}Logs.txt");

                using (StreamWriter writer = new StreamWriter(logFile, true))
                {
                    writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{logType}] {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logging failed: {ex.Message}");
            }
        }
    }
}
