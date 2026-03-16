namespace FlatCopyProfileExporter;

internal sealed record UserProfileInfo(string Name, string ProfilePath)
{
    public override string ToString() => $"{Name}  ({ProfilePath})";
}

internal sealed record KnownFolderOption(string DisplayName, string RelativePath)
{
    public override string ToString() => DisplayName;
}

internal sealed record CopyPlanItem(
    string SourcePath,
    string DestinationPath,
    string DisplayPath,
    long Length);

internal sealed class CopyPlan
{
    public HashSet<string> Directories { get; } = new(StringComparer.OrdinalIgnoreCase);
    public List<CopyPlanItem> Files { get; } = [];
    public long TotalBytes { get; set; }
}

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
