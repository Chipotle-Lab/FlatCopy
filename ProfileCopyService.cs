using System.Buffers;

namespace FlatCopyProfileExporter;

internal static class ProfileCopyService
{
    private static readonly string[] ExcludedProfileNames =
    [
        "All Users",
        "Default",
        "Default User",
        "defaultuser0",
        "Public"
    ];

    public static IReadOnlyList<UserProfileInfo> FindUserProfiles()
    {
        string currentUserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string usersRoot = Directory.GetParent(currentUserProfile)?.FullName
            ?? Path.Combine(Environment.GetEnvironmentVariable("SystemDrive") ?? "C:", "Users");

        if (!Directory.Exists(usersRoot))
        {
            return [];
        }

        List<UserProfileInfo> profiles = [];

        foreach (string directoryPath in Directory.EnumerateDirectories(usersRoot))
        {
            string profileName = Path.GetFileName(directoryPath);
            if (ExcludedProfileNames.Contains(profileName, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            DirectoryInfo directoryInfo = new(directoryPath);
            if (directoryInfo.Attributes.HasFlag(FileAttributes.ReparsePoint))
            {
                continue;
            }

            profiles.Add(new UserProfileInfo(profileName, directoryPath));
        }

        profiles.Sort((left, right) => string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase));
        return profiles;
    }

    public static CopyPlan BuildCopyPlan(
        IReadOnlyList<UserProfileInfo> selectedProfiles,
        IReadOnlyList<KnownFolderOption> selectedFolders,
        string destinationRoot,
        bool copyWholeProfile,
        TextWriter logWriter,
        CancellationToken cancellationToken,
        IProgress<string>? statusProgress)
    {
        CopyPlan plan = new();

        foreach (UserProfileInfo profile in selectedProfiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (copyWholeProfile)
            {
                string sourceRoot = profile.ProfilePath;
                string destinationFolder = Path.Combine(destinationRoot, profile.Name);
                statusProgress?.Report($"Scanning {profile.Name}\\entire profile...");
                plan.Directories.Add(destinationFolder);
                CollectItemsRecursive(sourceRoot, destinationFolder, profile.Name, plan, logWriter, cancellationToken);
                continue;
            }

            foreach (KnownFolderOption folder in selectedFolders)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string sourceRoot = Path.Combine(profile.ProfilePath, folder.RelativePath);
                string destinationFolder = Path.Combine(destinationRoot, profile.Name, folder.DisplayName);

                statusProgress?.Report($"Scanning {profile.Name}\\{folder.DisplayName}...");

                if (!Directory.Exists(sourceRoot))
                {
                    WriteLog(logWriter, $"Skipped missing folder: {sourceRoot}");
                    continue;
                }

                plan.Directories.Add(destinationFolder);
                CollectItemsRecursive(sourceRoot, destinationFolder, $"{profile.Name}\\{folder.DisplayName}", plan, logWriter, cancellationToken);
            }
        }

        WriteLog(logWriter, $"Scan complete. Planned {plan.Files.Count:N0} file(s), {plan.Directories.Count:N0} directory(s), {plan.TotalBytes:N0} byte(s).");
        return plan;
    }

    public static async Task<CopyExecutionSummary> ExecuteCopyPlanAsync(
        CopyPlan plan,
        bool overwriteExisting,
        TextWriter logWriter,
        IProgress<CopyProgressInfo>? progress,
        IProgress<string>? statusProgress,
        CancellationToken cancellationToken)
    {
        foreach (string directoryPath in plan.Directories.OrderBy(path => path.Length).ThenBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            cancellationToken.ThrowIfCancellationRequested();
            Directory.CreateDirectory(directoryPath);
        }

        int copiedFiles = 0;
        int skippedFiles = 0;
        int failedFiles = 0;
        int filesProcessed = 0;
        long bytesProcessed = 0;

        foreach (CopyPlanItem item in plan.Files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            statusProgress?.Report($"Copying {item.DisplayPath}");

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(item.DestinationPath)!);

                if (!overwriteExisting && File.Exists(item.DestinationPath))
                {
                    skippedFiles++;
                    bytesProcessed += item.Length;
                    filesProcessed++;
                    WriteLog(logWriter, $"Skipped existing file: {item.DestinationPath}");
                    progress?.Report(new CopyProgressInfo(bytesProcessed, plan.TotalBytes, filesProcessed, plan.Files.Count, $"Skipped {item.DisplayPath}"));
                    continue;
                }

                await CopyFileWithProgressAsync(
                    item.SourcePath,
                    item.DestinationPath,
                    overwriteExisting,
                    copiedInCurrentFile =>
                    {
                        progress?.Report(new CopyProgressInfo(
                            bytesProcessed + copiedInCurrentFile,
                            plan.TotalBytes,
                            filesProcessed,
                            plan.Files.Count,
                            $"Copying {item.DisplayPath}"));
                    },
                    cancellationToken);

                copiedFiles++;
                bytesProcessed += item.Length;
                filesProcessed++;
                WriteLog(logWriter, $"Copied: {item.SourcePath} -> {item.DestinationPath} ({item.Length:N0} bytes)");
                progress?.Report(new CopyProgressInfo(bytesProcessed, plan.TotalBytes, filesProcessed, plan.Files.Count, $"Copied {item.DisplayPath}"));
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                failedFiles++;
                bytesProcessed += item.Length;
                filesProcessed++;
                WriteLog(logWriter, $"Failed: {item.SourcePath} -> {item.DestinationPath} :: {exception.Message}");
                progress?.Report(new CopyProgressInfo(bytesProcessed, plan.TotalBytes, filesProcessed, plan.Files.Count, $"Failed {item.DisplayPath}"));
            }
        }

        WriteLog(logWriter, $"Copy complete. Copied={copiedFiles:N0}, Skipped={skippedFiles:N0}, Failed={failedFiles:N0}, ProcessedBytes={bytesProcessed:N0}");
        return new CopyExecutionSummary(copiedFiles, skippedFiles, failedFiles, bytesProcessed);
    }

    public static void WriteLog(TextWriter writer, string message)
    {
        writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
    }

    private static void CollectItemsRecursive(
        string sourceRoot,
        string destinationRoot,
        string displayPrefix,
        CopyPlan plan,
        TextWriter logWriter,
        CancellationToken cancellationToken)
    {
        Stack<(string SourcePath, string DestinationPath)> pending = new();
        pending.Push((sourceRoot, destinationRoot));

        while (pending.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            (string currentSourcePath, string currentDestinationPath) = pending.Pop();
            plan.Directories.Add(currentDestinationPath);

            IEnumerable<string> childDirectories;
            try
            {
                childDirectories = Directory.EnumerateDirectories(currentSourcePath);
            }
            catch (Exception exception)
            {
                WriteLog(logWriter, $"Unable to enumerate directories under {currentSourcePath}: {exception.Message}");
                continue;
            }

            foreach (string childDirectory in childDirectories)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    DirectoryInfo directoryInfo = new(childDirectory);
                    if (directoryInfo.Attributes.HasFlag(FileAttributes.ReparsePoint))
                    {
                        WriteLog(logWriter, $"Skipped reparse-point directory: {childDirectory}");
                        continue;
                    }

                    pending.Push((childDirectory, Path.Combine(currentDestinationPath, directoryInfo.Name)));
                }
                catch (Exception exception)
                {
                    WriteLog(logWriter, $"Unable to inspect directory {childDirectory}: {exception.Message}");
                }
            }

            IEnumerable<string> childFiles;
            try
            {
                childFiles = Directory.EnumerateFiles(currentSourcePath);
            }
            catch (Exception exception)
            {
                WriteLog(logWriter, $"Unable to enumerate files under {currentSourcePath}: {exception.Message}");
                continue;
            }

            foreach (string childFile in childFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    FileInfo fileInfo = new(childFile);
                    string destinationFile = Path.Combine(currentDestinationPath, fileInfo.Name);
                    string relativePath = Path.GetRelativePath(sourceRoot, childFile);
                    string displayPath = $"{displayPrefix}\\{relativePath}";

                    plan.Files.Add(new CopyPlanItem(childFile, destinationFile, displayPath, fileInfo.Length));
                    plan.TotalBytes += fileInfo.Length;
                }
                catch (Exception exception)
                {
                    WriteLog(logWriter, $"Unable to inspect file {childFile}: {exception.Message}");
                }
            }
        }
    }

    private static async Task CopyFileWithProgressAsync(
        string sourcePath,
        string destinationPath,
        bool overwriteExisting,
        Action<long> reportBytesCopied,
        CancellationToken cancellationToken)
    {
        FileMode destinationMode = overwriteExisting ? FileMode.Create : FileMode.CreateNew;

        await using FileStream sourceStream = new(
            sourcePath,
            new FileStreamOptions
            {
                Access = FileAccess.Read,
                Mode = FileMode.Open,
                Share = FileShare.ReadWrite | FileShare.Delete,
                Options = FileOptions.SequentialScan
            });

        await using FileStream destinationStream = new(
            destinationPath,
            new FileStreamOptions
            {
                Access = FileAccess.Write,
                Mode = destinationMode,
                Share = FileShare.None,
                Options = FileOptions.SequentialScan
            });

        byte[] buffer = ArrayPool<byte>.Shared.Rent(1024 * 1024);
        long totalCopied = 0;

        try
        {
            while (true)
            {
                int bytesRead = await sourceStream.ReadAsync(buffer, cancellationToken);
                if (bytesRead == 0)
                {
                    break;
                }

                await destinationStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                totalCopied += bytesRead;
                reportBytesCopied(totalCopied);
            }

            await destinationStream.FlushAsync(cancellationToken);
            File.SetLastWriteTimeUtc(destinationPath, File.GetLastWriteTimeUtc(sourcePath));
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer, clearArray: false);
        }
    }
}
