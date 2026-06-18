using System;
using System.IO;
using System.Text;

namespace RevitSpoolCopy.Models
{
    /// <summary>
    /// Minimal append-only file logger for diagnosing runtime behavior inside Revit.
    /// Log file: %APPDATA%\Autodesk\Revit\Addins\RevitSpoolCopy\log.txt
    /// Thread-safe; never throws (logging failures are swallowed so they can't mask the
    /// original error or crash a command).
    /// </summary>
    public static class Logger
    {
        private static readonly object _lock = new object();

        public static string LogDirectory =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Autodesk", "Revit", "Addins", "RevitSpoolCopy");

        public static string LogFilePath => Path.Combine(LogDirectory, "log.txt");

        public static void Info(string message) => Write("INFO", message);

        public static void Warn(string message) => Write("WARN", message);

        /// <summary>Log an exception with full type, message, and stack trace.</summary>
        public static void Error(string context, Exception ex)
        {
            var sb = new StringBuilder();
            sb.Append(context);
            if (ex != null)
            {
                sb.Append(" -> ").Append(ex.GetType().FullName).Append(": ").Append(ex.Message);
                sb.AppendLine();
                sb.Append(ex.StackTrace);

                var inner = ex.InnerException;
                while (inner != null)
                {
                    sb.AppendLine();
                    sb.Append("  inner -> ").Append(inner.GetType().FullName)
                      .Append(": ").Append(inner.Message);
                    inner = inner.InnerException;
                }
            }
            Write("ERROR", sb.ToString());
        }

        private static void Write(string level, string message)
        {
            try
            {
                lock (_lock)
                {
                    if (!Directory.Exists(LogDirectory))
                        Directory.CreateDirectory(LogDirectory);

                    string line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}{Environment.NewLine}";
                    File.AppendAllText(LogFilePath, line);
                }
            }
            catch
            {
                // Never let logging throw.
            }
        }
    }
}
