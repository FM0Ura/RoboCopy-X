namespace RoboCopy_X.Models
{
    public class AppSettings
    {
        // Appearance
        public string Theme { get; set; } = "Default";
        public bool UseMicaBackdrop { get; set; } = true;
        
        // Behavior
        public bool AutoCloseSuccessNotifications { get; set; } = true;
        public bool ConfirmBeforeExecution { get; set; } = false;
        public bool SaveLastPaths { get; set; } = true;
        
        // Defaults
        public int DefaultThreadCount { get; set; } = 0; // 0 = Auto
        public bool DefaultCopySubdirectories { get; set; } = false;
        public bool DefaultUseMultiThread { get; set; } = true;
        
        // Last used paths
        public string? LastSourcePath { get; set; }
        public string? LastDestinationPath { get; set; }
    }
}
