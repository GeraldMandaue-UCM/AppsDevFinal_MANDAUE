using System;
using System.IO;

namespace ConsoleApp1_Mandaue
{
    // =========================================================================
    //  COMPONENT: AuditLogger
    //  Appends a timestamped entry to the audit log file for every action.
    //  Logs: Add, Update, Delete, Read, Error, and System events.
    // =========================================================================
    class AuditLogger
    {
        private string _logFilePath;

        public AuditLogger(string logFilePath)
        {
            _logFilePath = logFilePath;
        }

        public void Log(string action, string details)
        {
            try
            {
                string directory = Path.GetDirectoryName(_logFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                string entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{action,-10}] {details}";
                File.AppendAllText(_logFilePath, entry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                // Do not crash the app if logging fails — just show a warning
                Console.WriteLine($"  [Warning] Audit log write failed: {ex.Message}");
            }
        }

        public string GetLogFilePath()
        {
            return _logFilePath;
        }
    }
}
