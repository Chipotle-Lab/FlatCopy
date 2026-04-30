using System.Diagnostics;

namespace FlatCopyProfileExporter;

internal sealed record BitLockerCheckResult(
    bool CheckedSuccessfully,
    bool IsProtected,
    string VolumeRoot,
    string Details);

internal static class WindowsSecurityService
{
    public static BitLockerCheckResult GetBitLockerProtectionStatus(string destinationRoot)
    {
        string volumeRoot = GetVolumeRoot(destinationRoot);

        if (string.IsNullOrWhiteSpace(volumeRoot))
        {
            return new BitLockerCheckResult(false, false, destinationRoot, "The destination drive root could not be determined.");
        }

        try
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = "powershell.exe",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            startInfo.ArgumentList.Add("-NoProfile");
            startInfo.ArgumentList.Add("-NonInteractive");
            startInfo.ArgumentList.Add("-ExecutionPolicy");
            startInfo.ArgumentList.Add("Bypass");
            startInfo.ArgumentList.Add("-Command");
            startInfo.ArgumentList.Add($"$ErrorActionPreference='Stop'; $vol = Get-BitLockerVolume -MountPoint '{volumeRoot.Replace("'", "''")}'; if ($null -eq $vol) {{ '-1' }} else {{ [int]$vol.ProtectionStatus }}");

            using Process process = Process.Start(startInfo)
                ?? throw new InvalidOperationException("Unable to start PowerShell for BitLocker verification.");

            string standardOutput = process.StandardOutput.ReadToEnd().Trim();
            string standardError = process.StandardError.ReadToEnd().Trim();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                return new BitLockerCheckResult(false, false, volumeRoot, string.IsNullOrWhiteSpace(standardError) ? "BitLocker verification command failed." : standardError);
            }

            bool isProtected = standardOutput == "1";
            bool checkedSuccessfully = standardOutput is "0" or "1";
            string details = checkedSuccessfully
                ? (isProtected ? "BitLocker protection is enabled on the destination drive." : "BitLocker protection is not enabled on the destination drive.")
                : $"Unexpected BitLocker status value: {standardOutput}";

            return new BitLockerCheckResult(checkedSuccessfully, isProtected, volumeRoot, details);
        }
        catch (Exception exception)
        {
            return new BitLockerCheckResult(false, false, volumeRoot, exception.Message);
        }
    }

    private static string GetVolumeRoot(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        string fullPath = Path.GetFullPath(path);
        return Path.GetPathRoot(fullPath)?.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) ?? string.Empty;
    }
}
