using System;
using System.IO;

namespace RoboCopy_X.Services
{
    public class LogService
    {
        private readonly string _logsDirectory;

        public LogService()
        {
            _logsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            EnsureLogsDirectoryExists();
        }

        private void EnsureLogsDirectoryExists()
        {
            try
            {
                if (!Directory.Exists(_logsDirectory))
                {
                    Directory.CreateDirectory(_logsDirectory);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to create logs directory: {ex.Message}");
            }
        }

        public string GetLogFilePath()
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            return Path.Combine(_logsDirectory, $"robocopy_log_{timestamp}.txt");
        }

        public string GetLogsDirectory()
        {
            return _logsDirectory;
        }
    }
}
