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

    public static CopyScanSummary BuildCopyScanSummary(
        IReadOnlyList<UserProfileInfo> selectedProfiles,
        IReadOnlyList<KnownFolderOption> selectedFolders,
        string destinationRoot,
        bool copyWholeProfile,
        TextWriter logWriter,
        CancellationToken cancellationToken,
        IProgress<string>? statusProgress)
    {
        List<CopySourceRoot> roots = BuildCopyRoots(selectedProfiles, selectedFolders, destinationRoot, copyWholeProfile, logWriter);
        int totalFiles = 0;
        int totalDirectories = 0;
        long totalBytes = 0;

        foreach (CopySourceRoot root in roots)
        {
            cancellationToken.ThrowIfCancellationRequested();
            statusProgress?.Report($"Scanning {root.DisplayPath}...");

            foreach (CopyEntry entry in EnumerateCopyEntries(root, logWriter, cancellationToken))
            {
                if (entry.IsDirectory)
                {
                    totalDirectories++;
                    continue;
                }

                totalFiles++;
                totalBytes += entry.Length;
            }
        }

        WriteLog(logWriter, $"Scan complete. Planned {totalFiles:N0} file(s), {totalDirectories:N0} directory(s), {totalBytes:N0} byte(s).");
        return new CopyScanSummary(roots, totalFiles, totalDirectories, totalBytes);
    }

    public static async Task<CopyExecutionSummary> ExecuteCopyAsync(
        CopyScanSummary scanSummary,
        bool overwriteExisting,
        TextWriter logWriter,
        IProgress<CopyProgressInfo>? progress,
        IProgress<string>? statusProgress,
        CancellationToken cancellationToken)
    {
        int copiedFiles = 0;
        int skippedFiles = 0;
        int failedFiles = 0;
        int filesProcessed = 0;
        long bytesProcessed = 0;

        foreach (CopySourceRoot root in scanSummary.Roots)
        {
            foreach (CopyEntry entry in EnumerateCopyEntries(root, logWriter, cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (entry.IsDirectory)
                {
                    Directory.CreateDirectory(entry.DestinationPath);
                    continue;
                }

                statusProgress?.Report($"Copying {entry.DisplayPath}");

                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(entry.DestinationPath)!);

                    if (!overwriteExisting && File.Exists(entry.DestinationPath))
                    {
                        skippedFiles++;
                        bytesProcessed += entry.Length;
                        filesProcessed++;
                        WriteLog(logWriter, $"Skipped existing file: {entry.DestinationPath}");
                        progress?.Report(new CopyProgressInfo(bytesProcessed, scanSummary.TotalBytes, filesProcessed, scanSummary.TotalFiles, $"Skipped {entry.DisplayPath}"));
                        continue;
                    }

                    await CopyFileWithProgressAsync(
                        entry.SourcePath,
                        entry.DestinationPath,
                        overwriteExisting,
                        copiedInCurrentFile =>
                        {
                            progress?.Report(new CopyProgressInfo(
                                bytesProcessed + copiedInCurrentFile,
                                scanSummary.TotalBytes,
                                filesProcessed,
                                scanSummary.TotalFiles,
                                $"Copying {entry.DisplayPath}"));
                        },
                        cancellationToken);

                    copiedFiles++;
                    bytesProcessed += entry.Length;
                    filesProcessed++;
                    WriteLog(logWriter, $"Copied: {entry.SourcePath} -> {entry.DestinationPath} ({entry.Length:N0} bytes)");
                    progress?.Report(new CopyProgressInfo(bytesProcessed, scanSummary.TotalBytes, filesProcessed, scanSummary.TotalFiles, $"Copied {entry.DisplayPath}"));
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    failedFiles++;
                    bytesProcessed += entry.Length;
                    filesProcessed++;
                    WriteLog(logWriter, $"Failed: {entry.SourcePath} -> {entry.DestinationPath} :: {exception.Message}");
                    progress?.Report(new CopyProgressInfo(bytesProcessed, scanSummary.TotalBytes, filesProcessed, scanSummary.TotalFiles, $"Failed {entry.DisplayPath}"));
                }
            }
        }

        WriteLog(logWriter, $"Copy complete. Copied={copiedFiles:N0}, Skipped={skippedFiles:N0}, Failed={failedFiles:N0}, ProcessedBytes={bytesProcessed:N0}");
        return new CopyExecutionSummary(copiedFiles, skippedFiles, failedFiles, bytesProcessed);
    }

    public static void WriteLog(TextWriter writer, string message)
    {
        writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
    }

    private static List<CopySourceRoot> BuildCopyRoots(
        IReadOnlyList<UserProfileInfo> selectedProfiles,
        IReadOnlyList<KnownFolderOption> selectedFolders,
        string destinationRoot,
        bool copyWholeProfile,
        TextWriter logWriter)
    {
        List<CopySourceRoot> roots = [];

        foreach (UserProfileInfo profile in selectedProfiles)
        {
            if (copyWholeProfile)
            {
                TryAddCopyRoot(
                    roots,
                    profile.ProfilePath,
                    Path.Combine(destinationRoot, profile.Name),
                    profile.Name,
                    logWriter);
                continue;
            }

            foreach (KnownFolderOption folder in selectedFolders)
            {
                TryAddCopyRoot(
                    roots,
                    Path.Combine(profile.ProfilePath, folder.RelativePath),
                    Path.Combine(destinationRoot, profile.Name, folder.DisplayName),
                    $"{profile.Name}\\{folder.DisplayName}",
                    logWriter);
            }
        }

        return roots;
    }

    private static void TryAddCopyRoot(
        ICollection<CopySourceRoot> roots,
        string sourceRoot,
        string destinationRoot,
        string displayPath,
        TextWriter logWriter)
    {
        if (!Directory.Exists(sourceRoot))
        {
            WriteLog(logWriter, $"Skipped missing folder: {sourceRoot}");
            return;
        }

        try
        {
            DirectoryInfo directoryInfo = new(sourceRoot);
            if (directoryInfo.Attributes.HasFlag(FileAttributes.ReparsePoint))
            {
                WriteLog(logWriter, $"Skipped reparse-point directory: {sourceRoot}");
                return;
            }

            roots.Add(new CopySourceRoot(sourceRoot, destinationRoot, displayPath));
        }
        catch (Exception exception)
        {
            WriteLog(logWriter, $"Unable to inspect directory {sourceRoot}: {exception.Message}");
        }
    }

    private static IEnumerable<CopyEntry> EnumerateCopyEntries(
        CopySourceRoot root,
        TextWriter logWriter,
        CancellationToken cancellationToken)
    {
        Stack<(string SourcePath, string DestinationPath)> pending = new();
        pending.Push((root.SourcePath, root.DestinationPath));

        while (pending.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            (string currentSourcePath, string currentDestinationPath) = pending.Pop();
            string currentDisplayPath = BuildDisplayPath(root.SourcePath, root.DisplayPath, currentSourcePath);
            yield return new CopyEntry(currentSourcePath, currentDestinationPath, currentDisplayPath, 0, true);

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
                CopyEntry? fileEntry = null;

                try
                {
                    FileInfo fileInfo = new(childFile);
                    string destinationFile = Path.Combine(currentDestinationPath, fileInfo.Name);
                    string displayPath = BuildDisplayPath(root.SourcePath, root.DisplayPath, childFile);
                    fileEntry = new CopyEntry(childFile, destinationFile, displayPath, fileInfo.Length, false);
                }
                catch (Exception exception)
                {
                    WriteLog(logWriter, $"Unable to inspect file {childFile}: {exception.Message}");
                }

                if (fileEntry is not null)
                {
                    yield return fileEntry;
                }
            }
        }
    }

    private static string BuildDisplayPath(string sourceRoot, string displayRoot, string fullPath)
    {
        string relativePath = Path.GetRelativePath(sourceRoot, fullPath);
        return relativePath == "."
            ? displayRoot
            : $"{displayRoot}\\{relativePath}";
    }

    private static async Task CopyFileWithProgressAsync(
        string sourcePath,
        string destinationPath,
        bool overwriteExisting,
        Action<long> reportBytesCopied,
        CancellationToken cancellationToken)
    {
        string destinationDirectory = Path.GetDirectoryName(destinationPath)
            ?? throw new InvalidOperationException($"Unable to determine the destination directory for {destinationPath}.");
        string temporaryDestinationPath = Path.Combine(
            destinationDirectory,
            $"{Path.GetFileName(destinationPath)}.flatcopy-partial-{Guid.NewGuid():N}");

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
            temporaryDestinationPath,
            new FileStreamOptions
            {
                Access = FileAccess.Write,
                Mode = FileMode.CreateNew,
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
            File.SetLastWriteTimeUtc(temporaryDestinationPath, File.GetLastWriteTimeUtc(sourcePath));
            FinalizeCopiedFile(temporaryDestinationPath, destinationPath, overwriteExisting);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer, clearArray: false);
            TryDeleteTemporaryFile(temporaryDestinationPath);
        }
    }

    private static void FinalizeCopiedFile(string temporaryDestinationPath, string destinationPath, bool overwriteExisting)
    {
        if (overwriteExisting && File.Exists(destinationPath))
        {
            File.Replace(temporaryDestinationPath, destinationPath, null, ignoreMetadataErrors: true);
            return;
        }

        File.Move(temporaryDestinationPath, destinationPath, overwriteExisting);
    }

    private static void TryDeleteTemporaryFile(string temporaryDestinationPath)
    {
        try
        {
            if (File.Exists(temporaryDestinationPath))
            {
                File.Delete(temporaryDestinationPath);
            }
        }
        catch
        {
        }
    }
}
