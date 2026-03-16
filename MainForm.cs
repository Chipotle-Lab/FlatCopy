using System.Text;

namespace FlatCopyProfileExporter;

public partial class MainForm : Form
{
    private static readonly IReadOnlyList<KnownFolderOption> FolderOptions =
    [
        new("Desktop", "Desktop"),
        new("Documents", "Documents"),
        new("Downloads", "Downloads"),
        new("Pictures", "Pictures"),
        new("Music", "Music"),
        new("Videos", "Videos"),
        new("Favorites", "Favorites"),
        new("Contacts", "Contacts"),
        new("Links", "Links"),
        new("Saved Games", "Saved Games"),
        new("Searches", "Searches")
    ];

    private CancellationTokenSource? _copyCancellationTokenSource;

    public MainForm()
    {
        InitializeComponent();
        LoadFolderOptions();
        LoadProfiles();
        UpdateWholeProfileModeState();
    }

    private void LoadFolderOptions()
    {
        foldersCheckedListBox.Items.Clear();

        foreach (KnownFolderOption option in FolderOptions)
        {
            foldersCheckedListBox.Items.Add(option, true);
        }
    }

    private void LoadProfiles()
    {
        HashSet<string> previouslyCheckedProfiles = profilesCheckedListBox.CheckedItems
            .OfType<UserProfileInfo>()
            .Select(profile => profile.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        profilesCheckedListBox.Items.Clear();

        IReadOnlyList<UserProfileInfo> profiles = ProfileCopyService.FindUserProfiles();

        foreach (UserProfileInfo profile in profiles)
        {
            int index = profilesCheckedListBox.Items.Add(profile);
            if (previouslyCheckedProfiles.Contains(profile.Name))
            {
                profilesCheckedListBox.SetItemChecked(index, true);
            }
        }

        AppendActivity($"Discovered {profiles.Count} user profile(s).");
    }

    private async void StartCopyButton_Click(object? sender, EventArgs e)
    {
        if (_copyCancellationTokenSource is not null)
        {
            return;
        }

        IReadOnlyList<UserProfileInfo> selectedProfiles = profilesCheckedListBox.CheckedItems
            .OfType<UserProfileInfo>()
            .ToList();

        IReadOnlyList<KnownFolderOption> selectedFolders = foldersCheckedListBox.CheckedItems
            .OfType<KnownFolderOption>()
            .ToList();

        string destinationRoot = destinationTextBox.Text.Trim();

        if (selectedProfiles.Count == 0)
        {
            MessageBox.Show(this, "Select at least one user profile.", "Nothing Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        bool copyWholeProfile = copyWholeProfileCheckBox.Checked;
        bool requireBitLocker = requireBitLockerCheckBox.Checked;
        bool encryptWithEfs = encryptWithEfsCheckBox.Checked;

        if (!copyWholeProfile && selectedFolders.Count == 0)
        {
            MessageBox.Show(this, "Select at least one profile folder to copy.", "Nothing Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (string.IsNullOrWhiteSpace(destinationRoot))
        {
            MessageBox.Show(this, "Choose a destination folder first.", "Destination Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (!ValidateDestination(selectedProfiles, destinationRoot))
        {
            return;
        }

        if (requireBitLocker)
        {
            BitLockerCheckResult bitLockerCheck = WindowsSecurityService.GetBitLockerProtectionStatus(destinationRoot);
            if (!bitLockerCheck.CheckedSuccessfully)
            {
                MessageBox.Show(
                    this,
                    $"The destination drive could not be verified for BitLocker protection.\r\n\r\n{bitLockerCheck.Details}",
                    "BitLocker Verification Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (!bitLockerCheck.IsProtected)
            {
                MessageBox.Show(
                    this,
                    $"The destination drive is not BitLocker-protected.\r\n\r\nDrive: {bitLockerCheck.VolumeRoot}",
                    "BitLocker Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
        }

        if (encryptWithEfs)
        {
            EfsSupportResult efsSupport = WindowsSecurityService.GetEfsSupportStatus(destinationRoot);
            if (!efsSupport.IsSupported)
            {
                MessageBox.Show(
                    this,
                    $"Windows EFS is not available for the selected destination.\r\n\r\n{efsSupport.Details}",
                    "EFS Not Available",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
        }

        Directory.CreateDirectory(destinationRoot);

        string logPath = Path.Combine(destinationRoot, $"FlatCopyLog_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
        _copyCancellationTokenSource = new CancellationTokenSource();
        SetBusyState(true);
        ResetProgressDisplay();
        AppendActivity($"Starting copy job. Log file: {logPath}");

        try
        {
            await using StreamWriter logWriter = new(logPath, false, new UTF8Encoding(false))
            {
                AutoFlush = true
            };

            ProfileCopyService.WriteLog(logWriter, "FlatCopy job started.");
            ProfileCopyService.WriteLog(logWriter, $"Destination root: {destinationRoot}");
            ProfileCopyService.WriteLog(logWriter, $"Selected profiles: {string.Join(", ", selectedProfiles.Select(profile => profile.Name))}");
            ProfileCopyService.WriteLog(logWriter, copyWholeProfile
                ? "Selected mode: Copy entire profile"
                : $"Selected folders: {string.Join(", ", selectedFolders.Select(folder => folder.DisplayName))}");
            ProfileCopyService.WriteLog(logWriter, $"Overwrite existing files: {overwriteCheckBox.Checked}");
            ProfileCopyService.WriteLog(logWriter, $"Require BitLocker destination: {requireBitLocker}");
            ProfileCopyService.WriteLog(logWriter, $"Encrypt copied data with Windows EFS: {encryptWithEfs}");

            var statusProgress = new Progress<string>(message =>
            {
                statusLabel.Text = message;
                AppendActivity(message);
            });

            copyProgressBar.Style = ProgressBarStyle.Marquee;
            statusLabel.Text = copyWholeProfile ? "Scanning selected profiles..." : "Scanning selected folders...";
            summaryLabel.Text = copyWholeProfile ? "Building the full-profile copy plan..." : "Building the copy plan...";

            CopyPlan plan = await Task.Run(
                () => ProfileCopyService.BuildCopyPlan(selectedProfiles, selectedFolders, destinationRoot, copyWholeProfile, logWriter, _copyCancellationTokenSource.Token, statusProgress),
                _copyCancellationTokenSource.Token);

            if (plan.Files.Count == 0)
            {
                statusLabel.Text = "Nothing to copy.";
                summaryLabel.Text = copyWholeProfile
                    ? "No files were found in the selected profiles."
                    : "No files were found in the selected profile folders.";
                AppendActivity(copyWholeProfile
                    ? "No files were found in the selected profiles."
                    : "No files were found in the selected profile folders.");
                MessageBox.Show(
                    this,
                    copyWholeProfile
                        ? "No files were found in the selected profiles."
                        : "No files were found in the selected profile folders.",
                    "Nothing To Copy",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            copyProgressBar.Style = ProgressBarStyle.Continuous;
            copyProgressBar.Value = 0;
            summaryLabel.Text = $"{plan.Files.Count:N0} file(s) queued, {FormatBytes(plan.TotalBytes)} total.";

            var copyProgress = new Progress<CopyProgressInfo>(UpdateProgressDisplay);

            CopyExecutionSummary result = await ProfileCopyService.ExecuteCopyPlanAsync(
                plan,
                overwriteCheckBox.Checked,
                logWriter,
                copyProgress,
                statusProgress,
                _copyCancellationTokenSource.Token);

            if (encryptWithEfs)
            {
                copyProgressBar.Style = ProgressBarStyle.Marquee;
                statusLabel.Text = "Encrypting copied data with Windows EFS...";
                summaryLabel.Text = "Applying EFS to the copied output and log file...";
                AppendActivity("Applying Windows EFS encryption to the copied output.");

                await Task.Run(
                    () => WindowsSecurityService.EncryptOutputWithEfs(
                        BuildEfsTargets(selectedProfiles, selectedFolders, destinationRoot, copyWholeProfile),
                        logPath,
                        logWriter,
                        _copyCancellationTokenSource.Token),
                    _copyCancellationTokenSource.Token);
            }

            copyProgressBar.Style = ProgressBarStyle.Continuous;
            copyProgressBar.Value = copyProgressBar.Maximum;
            statusLabel.Text = "Copy complete.";
            summaryLabel.Text = $"{result.CopiedFiles:N0} copied, {result.SkippedFiles:N0} skipped, {result.FailedFiles:N0} failed, {FormatBytes(result.TotalBytesProcessed)} processed.";
            AppendActivity($"Copy completed. {result.CopiedFiles:N0} file(s) copied.");

            MessageBox.Show(
                this,
                $"Copy complete.\r\n\r\nCopied: {result.CopiedFiles:N0}\r\nSkipped: {result.SkippedFiles:N0}\r\nFailed: {result.FailedFiles:N0}\r\nLog: {logPath}",
                "FlatCopy Profile Exporter",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (OperationCanceledException)
        {
            statusLabel.Text = "Copy canceled.";
            summaryLabel.Text = "The copy job was canceled before completion.";
            AppendActivity("Copy job canceled.");
        }
        catch (Exception exception)
        {
            statusLabel.Text = "Copy failed.";
            summaryLabel.Text = exception.Message;
            AppendActivity($"Copy failed: {exception.Message}");
            MessageBox.Show(this, exception.Message, "Copy Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _copyCancellationTokenSource.Dispose();
            _copyCancellationTokenSource = null;
            SetBusyState(false);
        }
    }

    private void UpdateProgressDisplay(CopyProgressInfo progress)
    {
        int progressMaximum = copyProgressBar.Maximum;
        int progressValue = progress.TotalBytes <= 0
            ? 0
            : (int)Math.Min(progressMaximum, progress.BytesProcessed * progressMaximum / progress.TotalBytes);

        copyProgressBar.Value = Math.Max(0, progressValue);
        statusLabel.Text = progress.CurrentItem;
        summaryLabel.Text = $"{progress.FilesProcessed:N0} of {progress.TotalFiles:N0} files processed, {FormatBytes(progress.BytesProcessed)} of {FormatBytes(progress.TotalBytes)}.";
    }

    private bool ValidateDestination(IReadOnlyList<UserProfileInfo> selectedProfiles, string destinationRoot)
    {
        string normalizedDestination = Path.GetFullPath(destinationRoot)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        foreach (UserProfileInfo profile in selectedProfiles)
        {
            string normalizedProfile = Path.GetFullPath(profile.ProfilePath)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            if (normalizedDestination.StartsWith(normalizedProfile, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    this,
                    $"The destination folder cannot be inside the selected profile path:\r\n{profile.ProfilePath}",
                    "Invalid Destination",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return false;
            }
        }

        return true;
    }

    private void ResetProgressDisplay()
    {
        copyProgressBar.Style = ProgressBarStyle.Continuous;
        copyProgressBar.Value = 0;
        statusLabel.Text = "Ready.";
        summaryLabel.Text = "No copy job is running.";
    }

    private void SetBusyState(bool isBusy)
    {
        profilesCheckedListBox.Enabled = !isBusy;
        refreshProfilesButton.Enabled = !isBusy;
        selectAllProfilesButton.Enabled = !isBusy;
        clearProfilesButton.Enabled = !isBusy;
        destinationTextBox.Enabled = !isBusy;
        browseDestinationButton.Enabled = !isBusy;
        overwriteCheckBox.Enabled = !isBusy;
        copyWholeProfileCheckBox.Enabled = !isBusy;
        requireBitLockerCheckBox.Enabled = !isBusy;
        encryptWithEfsCheckBox.Enabled = !isBusy;
        startCopyButton.Enabled = !isBusy;
        cancelCopyButton.Enabled = isBusy;

        if (isBusy)
        {
            foldersCheckedListBox.Enabled = false;
            selectAllFoldersButton.Enabled = false;
            clearFoldersButton.Enabled = false;
        }
        else
        {
            UpdateWholeProfileModeState();
        }
    }

    private void AppendActivity(string message)
    {
        string line = $"[{DateTime.Now:HH:mm:ss}] {message}";

        if (activityTextBox.TextLength > 0)
        {
            activityTextBox.AppendText(Environment.NewLine);
        }

        activityTextBox.AppendText(line);

        if (activityTextBox.Lines.Length > 250)
        {
            activityTextBox.Lines = activityTextBox.Lines.Skip(activityTextBox.Lines.Length - 250).ToArray();
        }

        activityTextBox.SelectionStart = activityTextBox.TextLength;
        activityTextBox.ScrollToCaret();
    }

    private static string FormatBytes(long value)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        double size = value;
        int unitIndex = 0;

        while (size >= 1024 && unitIndex < units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }

        return $"{size:0.##} {units[unitIndex]}";
    }

    private void BrowseDestinationButton_Click(object? sender, EventArgs e)
    {
        using FolderBrowserDialog dialog = new()
        {
            Description = "Choose a destination folder for the copied profile data.",
            ShowNewFolderButton = true,
            UseDescriptionForTitle = true
        };

        if (Directory.Exists(destinationTextBox.Text))
        {
            dialog.SelectedPath = destinationTextBox.Text;
        }

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            destinationTextBox.Text = dialog.SelectedPath;
        }
    }

    private void RefreshProfilesButton_Click(object? sender, EventArgs e)
    {
        LoadProfiles();
    }

    private void SelectAllProfilesButton_Click(object? sender, EventArgs e)
    {
        SetAllCheckedStates(profilesCheckedListBox, true);
    }

    private void ClearProfilesButton_Click(object? sender, EventArgs e)
    {
        SetAllCheckedStates(profilesCheckedListBox, false);
    }

    private void SelectAllFoldersButton_Click(object? sender, EventArgs e)
    {
        SetAllCheckedStates(foldersCheckedListBox, true);
    }

    private void ClearFoldersButton_Click(object? sender, EventArgs e)
    {
        SetAllCheckedStates(foldersCheckedListBox, false);
    }

    private static void SetAllCheckedStates(CheckedListBox checkedListBox, bool isChecked)
    {
        for (int index = 0; index < checkedListBox.Items.Count; index++)
        {
            checkedListBox.SetItemChecked(index, isChecked);
        }
    }

    private static IReadOnlyList<string> BuildEfsTargets(
        IReadOnlyList<UserProfileInfo> selectedProfiles,
        IReadOnlyList<KnownFolderOption> selectedFolders,
        string destinationRoot,
        bool copyWholeProfile)
    {
        HashSet<string> targets = new(StringComparer.OrdinalIgnoreCase);

        foreach (UserProfileInfo profile in selectedProfiles)
        {
            if (copyWholeProfile)
            {
                targets.Add(Path.Combine(destinationRoot, profile.Name));
                continue;
            }

            foreach (KnownFolderOption folder in selectedFolders)
            {
                targets.Add(Path.Combine(destinationRoot, profile.Name, folder.DisplayName));
            }
        }

        return targets.ToList();
    }

    private void CopyWholeProfileCheckBox_CheckedChanged(object? sender, EventArgs e)
    {
        UpdateWholeProfileModeState();
    }

    private void UpdateWholeProfileModeState()
    {
        bool copyWholeProfile = copyWholeProfileCheckBox.Checked;
        foldersCheckedListBox.Enabled = !copyWholeProfile;
        selectAllFoldersButton.Enabled = !copyWholeProfile;
        clearFoldersButton.Enabled = !copyWholeProfile;

        if (copyWholeProfile)
        {
            summaryLabel.Text = "Whole-profile mode is enabled. The folder list is ignored.";
        }
        else if (_copyCancellationTokenSource is null)
        {
            summaryLabel.Text = "No copy job is running.";
        }
    }

    private void CancelCopyButton_Click(object? sender, EventArgs e)
    {
        _copyCancellationTokenSource?.Cancel();
    }
}
