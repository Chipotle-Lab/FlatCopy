namespace FlatCopyProfileExporter;

internal sealed record UserProfileInfo(string Name, string ProfilePath)
{
    public override string ToString() => $"{Name}  ({ProfilePath})";
}

internal sealed record KnownFolderOption(string DisplayName, string RelativePath)
{
    public override string ToString() => DisplayName;
}

internal sealed record CopySourceRoot(
    string SourcePath,
    string DestinationPath,
    string DisplayPath);

internal sealed record CopyEntry(
    string SourcePath,
    string DestinationPath,
    string DisplayPath,
    long Length,
    bool IsDirectory);

internal sealed record CopyScanSummary(
    IReadOnlyList<CopySourceRoot> Roots,
    int TotalFiles,
    int TotalDirectories,
    long TotalBytes);

internal sealed record CopyProgressInfo(
    long BytesProcessed,
    long TotalBytes,
    int FilesProcessed,
    int TotalFiles,
    string CurrentItem);

internal sealed record CopyExecutionSummary(
    int CopiedFiles,
    int SkippedFiles,
    int FailedFiles,
    long TotalBytesProcessed);
