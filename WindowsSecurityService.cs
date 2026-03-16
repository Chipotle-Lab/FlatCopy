using System.Diagnostics;

namespace FlatCopyProfileExporter;

internal sealed record BitLockerCheckResult(
    bool CheckedSuccessfully,
    bool IsProtected,
    string VolumeRoot,
    string Details);

internal sealed record EfsSupportResult(
    bool IsSupported,
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

    public static EfsSupportResult GetEfsSupportStatus(string destinationRoot)
    {
        string volumeRoot = GetVolumeRoot(destinationRoot);
        if (string.IsNullOrWhiteSpace(volumeRoot))
        {
            return new EfsSupportResult(false, destinationRoot, "The destination drive root could not be determined.");
        }

        try
        {
            DriveInfo driveInfo = new(volumeRoot);
            if (!driveInfo.IsReady)
            {
                return new EfsSupportResult(false, volumeRoot, "The destination drive is not ready.");
            }

            if (!string.Equals(driveInfo.DriveFormat, "NTFS", StringComparison.OrdinalIgnoreCase))
            {
                return new EfsSupportResult(false, volumeRoot, $"Windows EFS requires NTFS. Current format: {driveInfo.DriveFormat}.");
            }

            return new EfsSupportResult(true, volumeRoot, "Windows EFS is available on the destination drive.");
        }
        catch (Exception exception)
        {
            return new EfsSupportResult(false, volumeRoot, exception.Message);
        }
    }

    public static void EncryptOutputWithEfs(
        IReadOnlyList<string> targetDirectories,
        string logPath,
        TextWriter logWriter,
        CancellationToken cancellationToken)
    {
        foreach (string directoryPath in targetDirectories.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!Directory.Exists(directoryPath))
            {
                ProfileCopyService.WriteLog(logWriter, $"Skipped EFS encryption for missing directory: {directoryPath}");
                continue;
            }

            RunCipherCommand(
                ["/e", "/s:" + directoryPath, "/h", "/i"],
                $"Encrypting directory with Windows EFS: {directoryPath}",
                logWriter);
        }

        cancellationToken.ThrowIfCancellationRequested();

        if (File.Exists(logPath))
        {
            RunCipherCommand(
                ["/e", logPath],
                $"Encrypting log file with Windows EFS: {logPath}",
                logWriter);
        }
    }

    private static void RunCipherCommand(
        IReadOnlyList<string> arguments,
        string operationDescription,
        TextWriter logWriter)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = "cipher.exe",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (string argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using Process process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Unable to start cipher.exe for: {operationDescription}");

        string standardOutput = process.StandardOutput.ReadToEnd().Trim();
        string standardError = process.StandardError.ReadToEnd().Trim();
        process.WaitForExit();

        if (!string.IsNullOrWhiteSpace(standardOutput))
        {
            ProfileCopyService.WriteLog(logWriter, $"{operationDescription} :: {standardOutput}");
        }

        if (process.ExitCode != 0)
        {
            string errorMessage = string.IsNullOrWhiteSpace(standardError)
                ? $"{operationDescription} failed with exit code {process.ExitCode}."
                : $"{operationDescription} failed: {standardError}";

            ProfileCopyService.WriteLog(logWriter, errorMessage);
            throw new InvalidOperationException(errorMessage);
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
