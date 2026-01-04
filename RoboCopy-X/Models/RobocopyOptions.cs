using RoboCopy_X.Services;

namespace RoboCopy_X.Models
{
    public class RobocopyOptions
    {
        public string SourcePath { get; set; } = string.Empty;
        public string DestinationPath { get; set; } = string.Empty;
        
        // Copy Options
        public bool CopySubdirectories { get; set; }
        public bool MirrorMode { get; set; }
        public bool ExcludeOlder { get; set; }
        
        // Attributes
        public bool CopyData { get; set; } = true;
        public bool CopyAttributes { get; set; } = true;
        public bool CopyTimestamps { get; set; } = true;
        public bool CopySecurity { get; set; }
        public bool CopyOwner { get; set; }
        public bool CopyAudit { get; set; }
        
        // Performance
        public bool UseMultiThread { get; set; } = true;
        public int ThreadCount { get; set; } = 8;
        
        // Retry
        public int RetryCount { get; set; } = 10;
        public int WaitTime { get; set; } = 5;
        
        // Logging
        public bool Verbose { get; set; }
        public bool NoProgress { get; set; }
        public string LogFilePath { get; set; } = string.Empty;
        
        // Hash Validation
        public bool EnableHashValidation { get; set; }
        public HashValidationService.HashAlgorithmType HashAlgorithm { get; set; } = HashValidationService.HashAlgorithmType.SHA256;
    }
}
