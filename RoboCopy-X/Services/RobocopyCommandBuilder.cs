using System;
using System.IO;
using System.Text;
using RoboCopy_X.Models;

namespace RoboCopy_X.Services
{
    public class RobocopyCommandBuilder
    {
        public string BuildCommand(RobocopyOptions options)
        {
            var sb = new StringBuilder();
            
            // Get the source folder name to preserve folder structure
            var sourceFolderName = Path.GetFileName(options.SourcePath.TrimEnd('\\', '/'));
            var destinationWithFolder = Path.Combine(options.DestinationPath, sourceFolderName);
            
            // Add source and destination (destination includes source folder name)
            sb.Append($"\"{options.SourcePath}\" \"{destinationWithFolder}\"");

            // Copy subdirectories
            if (options.CopySubdirectories)
            {
                sb.Append(" /E");
            }

            // Mirror
            if (options.MirrorMode)
            {
                sb.Append(" /MIR");
            }

            // Exclude older
            if (options.ExcludeOlder)
            {
                sb.Append(" /XO");
            }

            // Copy attributes
            var copyFlags = new StringBuilder();
            if (options.CopyData) copyFlags.Append("D");
            if (options.CopyAttributes) copyFlags.Append("A");
            if (options.CopyTimestamps) copyFlags.Append("T");
            if (options.CopySecurity) copyFlags.Append("S");
            if (options.CopyOwner) copyFlags.Append("O");
            if (options.CopyAudit) copyFlags.Append("U");

            if (copyFlags.Length > 0)
            {
                sb.Append($" /COPY:{copyFlags}");
            }

            // Multi-thread
            if (options.UseMultiThread)
            {
                sb.Append($" /MT:{options.ThreadCount}");
            }

            // Retry count
            sb.Append($" /R:{options.RetryCount}");

            // Wait time
            sb.Append($" /W:{options.WaitTime}");

            // Verbose
            if (options.Verbose)
            {
                sb.Append(" /V");
            }

            // No progress
            if (options.NoProgress)
            {
                sb.Append(" /NP");
            }

            // Log file
            if (!string.IsNullOrWhiteSpace(options.LogFilePath))
            {
                sb.Append($" /LOG:\"{options.LogFilePath}\"");
            }

            return sb.ToString();
        }
    }
}
