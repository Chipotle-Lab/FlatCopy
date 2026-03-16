namespace FlatCopyProfileExporter;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private TableLayoutPanel rootLayoutPanel = null!;
    private Label titleLabel = null!;
    private Label instructionLabel = null!;
    private TableLayoutPanel selectionLayoutPanel = null!;
    private GroupBox profilesGroupBox = null!;
    private TableLayoutPanel profilesLayoutPanel = null!;
    private CheckedListBox profilesCheckedListBox = null!;
    private FlowLayoutPanel profilesButtonsPanel = null!;
    private Button refreshProfilesButton = null!;
    private Button selectAllProfilesButton = null!;
    private Button clearProfilesButton = null!;
    private GroupBox foldersGroupBox = null!;
    private TableLayoutPanel foldersLayoutPanel = null!;
    private CheckBox copyWholeProfileCheckBox = null!;
    private CheckedListBox foldersCheckedListBox = null!;
    private FlowLayoutPanel foldersButtonsPanel = null!;
    private Button selectAllFoldersButton = null!;
    private Button clearFoldersButton = null!;
    private TableLayoutPanel destinationLayoutPanel = null!;
    private Label destinationLabel = null!;
    private TextBox destinationTextBox = null!;
    private Button browseDestinationButton = null!;
    private CheckBox overwriteCheckBox = null!;
    private FlowLayoutPanel securityOptionsPanel = null!;
    private CheckBox requireBitLockerCheckBox = null!;
    private CheckBox encryptWithEfsCheckBox = null!;
    private FlowLayoutPanel actionsPanel = null!;
    private Button startCopyButton = null!;
    private Button cancelCopyButton = null!;
    private ProgressBar copyProgressBar = null!;
    private Label statusLabel = null!;
    private Label summaryLabel = null!;
    private GroupBox activityGroupBox = null!;
    private TextBox activityTextBox = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        rootLayoutPanel = new TableLayoutPanel();
        titleLabel = new Label();
        instructionLabel = new Label();
        selectionLayoutPanel = new TableLayoutPanel();
        profilesGroupBox = new GroupBox();
        profilesLayoutPanel = new TableLayoutPanel();
        profilesCheckedListBox = new CheckedListBox();
        profilesButtonsPanel = new FlowLayoutPanel();
        refreshProfilesButton = new Button();
        selectAllProfilesButton = new Button();
        clearProfilesButton = new Button();
        foldersGroupBox = new GroupBox();
        foldersLayoutPanel = new TableLayoutPanel();
        copyWholeProfileCheckBox = new CheckBox();
        foldersCheckedListBox = new CheckedListBox();
        foldersButtonsPanel = new FlowLayoutPanel();
        selectAllFoldersButton = new Button();
        clearFoldersButton = new Button();
        destinationLayoutPanel = new TableLayoutPanel();
        destinationLabel = new Label();
        destinationTextBox = new TextBox();
        browseDestinationButton = new Button();
        overwriteCheckBox = new CheckBox();
        securityOptionsPanel = new FlowLayoutPanel();
        requireBitLockerCheckBox = new CheckBox();
        encryptWithEfsCheckBox = new CheckBox();
        actionsPanel = new FlowLayoutPanel();
        startCopyButton = new Button();
        cancelCopyButton = new Button();
        copyProgressBar = new ProgressBar();
        statusLabel = new Label();
        summaryLabel = new Label();
        activityGroupBox = new GroupBox();
        activityTextBox = new TextBox();
        rootLayoutPanel.SuspendLayout();
        selectionLayoutPanel.SuspendLayout();
        profilesGroupBox.SuspendLayout();
        profilesLayoutPanel.SuspendLayout();
        profilesButtonsPanel.SuspendLayout();
        foldersGroupBox.SuspendLayout();
        foldersLayoutPanel.SuspendLayout();
        foldersButtonsPanel.SuspendLayout();
        destinationLayoutPanel.SuspendLayout();
        actionsPanel.SuspendLayout();
        activityGroupBox.SuspendLayout();
        SuspendLayout();
        //
        // rootLayoutPanel
        //
        rootLayoutPanel.ColumnCount = 1;
        rootLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rootLayoutPanel.Controls.Add(titleLabel, 0, 0);
        rootLayoutPanel.Controls.Add(instructionLabel, 0, 1);
        rootLayoutPanel.Controls.Add(selectionLayoutPanel, 0, 2);
        rootLayoutPanel.Controls.Add(destinationLayoutPanel, 0, 3);
        rootLayoutPanel.Controls.Add(actionsPanel, 0, 4);
        rootLayoutPanel.Controls.Add(copyProgressBar, 0, 5);
        rootLayoutPanel.Controls.Add(statusLabel, 0, 6);
        rootLayoutPanel.Controls.Add(summaryLabel, 0, 7);
        rootLayoutPanel.Controls.Add(activityGroupBox, 0, 8);
        rootLayoutPanel.Dock = DockStyle.Fill;
        rootLayoutPanel.Padding = new Padding(12);
        rootLayoutPanel.RowCount = 9;
        rootLayoutPanel.RowStyles.Add(new RowStyle());
        rootLayoutPanel.RowStyles.Add(new RowStyle());
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 44F));
        rootLayoutPanel.RowStyles.Add(new RowStyle());
        rootLayoutPanel.RowStyles.Add(new RowStyle());
        rootLayoutPanel.RowStyles.Add(new RowStyle());
        rootLayoutPanel.RowStyles.Add(new RowStyle());
        rootLayoutPanel.RowStyles.Add(new RowStyle());
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 56F));
        rootLayoutPanel.TabIndex = 0;
        //
        // titleLabel
        //
        titleLabel.AutoSize = true;
        titleLabel.Dock = DockStyle.Fill;
        titleLabel.Font = new Font("Segoe UI", 15F, FontStyle.Bold, GraphicsUnit.Point);
        titleLabel.Location = new Point(15, 12);
        titleLabel.Margin = new Padding(3, 0, 3, 6);
        titleLabel.Size = new Size(1134, 28);
        titleLabel.TabIndex = 0;
        titleLabel.Text = "FlatCopy Profile Exporter";
        //
        // instructionLabel
        //
        instructionLabel.AutoSize = true;
        instructionLabel.Dock = DockStyle.Fill;
        instructionLabel.ForeColor = SystemColors.ControlDarkDark;
        instructionLabel.Location = new Point(15, 46);
        instructionLabel.Margin = new Padding(3, 0, 3, 12);
        instructionLabel.Size = new Size(1134, 30);
        instructionLabel.TabIndex = 1;
        instructionLabel.Text = "Select one or more Windows user profiles, choose the profile folders to copy, and pick a destination folder. Files are copied directly with no compression.";
        //
        // selectionLayoutPanel
        //
        selectionLayoutPanel.ColumnCount = 2;
        selectionLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        selectionLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        selectionLayoutPanel.Controls.Add(profilesGroupBox, 0, 0);
        selectionLayoutPanel.Controls.Add(foldersGroupBox, 1, 0);
        selectionLayoutPanel.Dock = DockStyle.Fill;
        selectionLayoutPanel.Location = new Point(15, 91);
        selectionLayoutPanel.RowCount = 1;
        selectionLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        selectionLayoutPanel.Size = new Size(1134, 260);
        selectionLayoutPanel.TabIndex = 2;
        //
        // profilesGroupBox
        //
        profilesGroupBox.Controls.Add(profilesLayoutPanel);
        profilesGroupBox.Dock = DockStyle.Fill;
        profilesGroupBox.Location = new Point(3, 3);
        profilesGroupBox.Padding = new Padding(10);
        profilesGroupBox.Size = new Size(561, 254);
        profilesGroupBox.TabIndex = 0;
        profilesGroupBox.TabStop = false;
        profilesGroupBox.Text = "User Profiles";
        //
        // profilesLayoutPanel
        //
        profilesLayoutPanel.ColumnCount = 1;
        profilesLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        profilesLayoutPanel.Controls.Add(profilesCheckedListBox, 0, 0);
        profilesLayoutPanel.Controls.Add(profilesButtonsPanel, 0, 1);
        profilesLayoutPanel.Dock = DockStyle.Fill;
        profilesLayoutPanel.Location = new Point(10, 26);
        profilesLayoutPanel.RowCount = 2;
        profilesLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        profilesLayoutPanel.RowStyles.Add(new RowStyle());
        profilesLayoutPanel.Size = new Size(541, 218);
        profilesLayoutPanel.TabIndex = 0;
        //
        // profilesCheckedListBox
        //
        profilesCheckedListBox.CheckOnClick = true;
        profilesCheckedListBox.Dock = DockStyle.Fill;
        profilesCheckedListBox.FormattingEnabled = true;
        profilesCheckedListBox.HorizontalScrollbar = true;
        profilesCheckedListBox.IntegralHeight = false;
        profilesCheckedListBox.Location = new Point(3, 3);
        profilesCheckedListBox.Size = new Size(535, 177);
        profilesCheckedListBox.TabIndex = 0;
        //
        // profilesButtonsPanel
        //
        profilesButtonsPanel.AutoSize = true;
        profilesButtonsPanel.Controls.Add(refreshProfilesButton);
        profilesButtonsPanel.Controls.Add(selectAllProfilesButton);
        profilesButtonsPanel.Controls.Add(clearProfilesButton);
        profilesButtonsPanel.Dock = DockStyle.Fill;
        profilesButtonsPanel.Location = new Point(3, 186);
        profilesButtonsPanel.Size = new Size(535, 29);
        profilesButtonsPanel.TabIndex = 1;
        //
        // refreshProfilesButton
        //
        refreshProfilesButton.AutoSize = true;
        refreshProfilesButton.Location = new Point(3, 3);
        refreshProfilesButton.Size = new Size(134, 23);
        refreshProfilesButton.TabIndex = 0;
        refreshProfilesButton.Text = "Refresh Profiles";
        refreshProfilesButton.UseVisualStyleBackColor = true;
        refreshProfilesButton.Click += RefreshProfilesButton_Click;
        //
        // selectAllProfilesButton
        //
        selectAllProfilesButton.AutoSize = true;
        selectAllProfilesButton.Location = new Point(143, 3);
        selectAllProfilesButton.Size = new Size(68, 23);
        selectAllProfilesButton.TabIndex = 1;
        selectAllProfilesButton.Text = "Select All";
        selectAllProfilesButton.UseVisualStyleBackColor = true;
        selectAllProfilesButton.Click += SelectAllProfilesButton_Click;
        //
        // clearProfilesButton
        //
        clearProfilesButton.AutoSize = true;
        clearProfilesButton.Location = new Point(217, 3);
        clearProfilesButton.Size = new Size(74, 23);
        clearProfilesButton.TabIndex = 2;
        clearProfilesButton.Text = "Clear All";
        clearProfilesButton.UseVisualStyleBackColor = true;
        clearProfilesButton.Click += ClearProfilesButton_Click;
        //
        // foldersGroupBox
        //
        foldersGroupBox.Controls.Add(foldersLayoutPanel);
        foldersGroupBox.Dock = DockStyle.Fill;
        foldersGroupBox.Location = new Point(570, 3);
        foldersGroupBox.Padding = new Padding(10);
        foldersGroupBox.Size = new Size(561, 254);
        foldersGroupBox.TabIndex = 1;
        foldersGroupBox.TabStop = false;
        foldersGroupBox.Text = "Profile Folders";
        //
        // foldersLayoutPanel
        //
        foldersLayoutPanel.ColumnCount = 1;
        foldersLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        foldersLayoutPanel.Controls.Add(copyWholeProfileCheckBox, 0, 0);
        foldersLayoutPanel.Controls.Add(foldersCheckedListBox, 0, 1);
        foldersLayoutPanel.Controls.Add(foldersButtonsPanel, 0, 2);
        foldersLayoutPanel.Dock = DockStyle.Fill;
        foldersLayoutPanel.Location = new Point(10, 26);
        foldersLayoutPanel.RowCount = 3;
        foldersLayoutPanel.RowStyles.Add(new RowStyle());
        foldersLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        foldersLayoutPanel.RowStyles.Add(new RowStyle());
        foldersLayoutPanel.Size = new Size(541, 218);
        foldersLayoutPanel.TabIndex = 0;
        //
        // copyWholeProfileCheckBox
        //
        copyWholeProfileCheckBox.AutoSize = true;
        copyWholeProfileCheckBox.Dock = DockStyle.Fill;
        copyWholeProfileCheckBox.Location = new Point(3, 3);
        copyWholeProfileCheckBox.Size = new Size(535, 19);
        copyWholeProfileCheckBox.TabIndex = 0;
        copyWholeProfileCheckBox.Text = "Copy entire profile (ignores the folder list below)";
        copyWholeProfileCheckBox.UseVisualStyleBackColor = true;
        copyWholeProfileCheckBox.CheckedChanged += CopyWholeProfileCheckBox_CheckedChanged;
        //
        // foldersCheckedListBox
        //
        foldersCheckedListBox.CheckOnClick = true;
        foldersCheckedListBox.Dock = DockStyle.Fill;
        foldersCheckedListBox.FormattingEnabled = true;
        foldersCheckedListBox.IntegralHeight = false;
        foldersCheckedListBox.Location = new Point(3, 28);
        foldersCheckedListBox.Size = new Size(535, 152);
        foldersCheckedListBox.TabIndex = 1;
        //
        // foldersButtonsPanel
        //
        foldersButtonsPanel.AutoSize = true;
        foldersButtonsPanel.Controls.Add(selectAllFoldersButton);
        foldersButtonsPanel.Controls.Add(clearFoldersButton);
        foldersButtonsPanel.Dock = DockStyle.Fill;
        foldersButtonsPanel.Location = new Point(3, 186);
        foldersButtonsPanel.Size = new Size(535, 29);
        foldersButtonsPanel.TabIndex = 2;
        //
        // selectAllFoldersButton
        //
        selectAllFoldersButton.AutoSize = true;
        selectAllFoldersButton.Location = new Point(3, 3);
        selectAllFoldersButton.Size = new Size(68, 23);
        selectAllFoldersButton.TabIndex = 0;
        selectAllFoldersButton.Text = "Select All";
        selectAllFoldersButton.UseVisualStyleBackColor = true;
        selectAllFoldersButton.Click += SelectAllFoldersButton_Click;
        //
        // clearFoldersButton
        //
        clearFoldersButton.AutoSize = true;
        clearFoldersButton.Location = new Point(77, 3);
        clearFoldersButton.Size = new Size(74, 23);
        clearFoldersButton.TabIndex = 1;
        clearFoldersButton.Text = "Clear All";
        clearFoldersButton.UseVisualStyleBackColor = true;
        clearFoldersButton.Click += ClearFoldersButton_Click;
        //
        // destinationLayoutPanel
        //
        destinationLayoutPanel.AutoSize = true;
        destinationLayoutPanel.ColumnCount = 4;
        destinationLayoutPanel.ColumnStyles.Add(new ColumnStyle());
        destinationLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        destinationLayoutPanel.ColumnStyles.Add(new ColumnStyle());
        destinationLayoutPanel.ColumnStyles.Add(new ColumnStyle());
        destinationLayoutPanel.Controls.Add(destinationLabel, 0, 0);
        destinationLayoutPanel.Controls.Add(destinationTextBox, 1, 0);
        destinationLayoutPanel.Controls.Add(browseDestinationButton, 2, 0);
        destinationLayoutPanel.Controls.Add(overwriteCheckBox, 3, 0);
        destinationLayoutPanel.Controls.Add(securityOptionsPanel, 0, 1);
        destinationLayoutPanel.Dock = DockStyle.Fill;
        destinationLayoutPanel.Location = new Point(15, 357);
        destinationLayoutPanel.RowCount = 2;
        destinationLayoutPanel.RowStyles.Add(new RowStyle());
        destinationLayoutPanel.RowStyles.Add(new RowStyle());
        destinationLayoutPanel.Size = new Size(1134, 61);
        destinationLayoutPanel.TabIndex = 3;
        destinationLayoutPanel.SetColumnSpan(securityOptionsPanel, 4);
        //
        // destinationLabel
        //
        destinationLabel.Anchor = AnchorStyles.Left;
        destinationLabel.AutoSize = true;
        destinationLabel.Location = new Point(3, 8);
        destinationLabel.Size = new Size(102, 15);
        destinationLabel.TabIndex = 0;
        destinationLabel.Text = "Destination folder";
        //
        // destinationTextBox
        //
        destinationTextBox.Dock = DockStyle.Fill;
        destinationTextBox.Location = new Point(111, 3);
        destinationTextBox.PlaceholderText = "Choose a folder to receive the copied profile data";
        destinationTextBox.Size = new Size(744, 23);
        destinationTextBox.TabIndex = 1;
        //
        // browseDestinationButton
        //
        browseDestinationButton.AutoSize = true;
        browseDestinationButton.Location = new Point(861, 3);
        browseDestinationButton.Size = new Size(68, 23);
        browseDestinationButton.TabIndex = 2;
        browseDestinationButton.Text = "Browse";
        browseDestinationButton.UseVisualStyleBackColor = true;
        browseDestinationButton.Click += BrowseDestinationButton_Click;
        //
        // overwriteCheckBox
        //
        overwriteCheckBox.Anchor = AnchorStyles.Left;
        overwriteCheckBox.AutoSize = true;
        overwriteCheckBox.Checked = true;
        overwriteCheckBox.CheckState = CheckState.Checked;
        overwriteCheckBox.Location = new Point(935, 6);
        overwriteCheckBox.Size = new Size(168, 19);
        overwriteCheckBox.TabIndex = 3;
        overwriteCheckBox.Text = "Overwrite existing files";
        overwriteCheckBox.UseVisualStyleBackColor = true;
        //
        // securityOptionsPanel
        //
        securityOptionsPanel.AutoSize = true;
        securityOptionsPanel.Controls.Add(requireBitLockerCheckBox);
        securityOptionsPanel.Controls.Add(encryptWithEfsCheckBox);
        securityOptionsPanel.Dock = DockStyle.Fill;
        securityOptionsPanel.Location = new Point(3, 32);
        securityOptionsPanel.Margin = new Padding(3, 3, 3, 0);
        securityOptionsPanel.Size = new Size(1128, 29);
        securityOptionsPanel.TabIndex = 4;
        //
        // requireBitLockerCheckBox
        //
        requireBitLockerCheckBox.AutoSize = true;
        requireBitLockerCheckBox.Location = new Point(3, 3);
        requireBitLockerCheckBox.Size = new Size(246, 19);
        requireBitLockerCheckBox.TabIndex = 0;
        requireBitLockerCheckBox.Text = "Require BitLocker-protected destination";
        requireBitLockerCheckBox.UseVisualStyleBackColor = true;
        //
        // encryptWithEfsCheckBox
        //
        encryptWithEfsCheckBox.AutoSize = true;
        encryptWithEfsCheckBox.Location = new Point(255, 3);
        encryptWithEfsCheckBox.Size = new Size(275, 19);
        encryptWithEfsCheckBox.TabIndex = 1;
        encryptWithEfsCheckBox.Text = "Encrypt copied data with Windows EFS";
        encryptWithEfsCheckBox.UseVisualStyleBackColor = true;
        //
        // actionsPanel
        //
        actionsPanel.AutoSize = true;
        actionsPanel.Controls.Add(startCopyButton);
        actionsPanel.Controls.Add(cancelCopyButton);
        actionsPanel.Dock = DockStyle.Fill;
        actionsPanel.FlowDirection = FlowDirection.RightToLeft;
        actionsPanel.Location = new Point(15, 424);
        actionsPanel.Size = new Size(1134, 35);
        actionsPanel.TabIndex = 4;
        //
        // startCopyButton
        //
        startCopyButton.AutoSize = true;
        startCopyButton.Location = new Point(1020, 3);
        startCopyButton.Size = new Size(111, 29);
        startCopyButton.TabIndex = 0;
        startCopyButton.Text = "Start Copy";
        startCopyButton.UseVisualStyleBackColor = true;
        startCopyButton.Click += StartCopyButton_Click;
        //
        // cancelCopyButton
        //
        cancelCopyButton.AutoSize = true;
        cancelCopyButton.Enabled = false;
        cancelCopyButton.Location = new Point(938, 3);
        cancelCopyButton.Size = new Size(76, 29);
        cancelCopyButton.TabIndex = 1;
        cancelCopyButton.Text = "Cancel";
        cancelCopyButton.UseVisualStyleBackColor = true;
        cancelCopyButton.Click += CancelCopyButton_Click;
        //
        // copyProgressBar
        //
        copyProgressBar.Dock = DockStyle.Fill;
        copyProgressBar.Location = new Point(15, 465);
        copyProgressBar.MarqueeAnimationSpeed = 25;
        copyProgressBar.Size = new Size(1134, 22);
        copyProgressBar.Style = ProgressBarStyle.Continuous;
        copyProgressBar.TabIndex = 5;
        //
        // statusLabel
        //
        statusLabel.AutoSize = true;
        statusLabel.Dock = DockStyle.Fill;
        statusLabel.Location = new Point(15, 490);
        statusLabel.Margin = new Padding(3, 0, 3, 4);
        statusLabel.Size = new Size(1134, 15);
        statusLabel.TabIndex = 6;
        statusLabel.Text = "Ready.";
        //
        // summaryLabel
        //
        summaryLabel.AutoSize = true;
        summaryLabel.Dock = DockStyle.Fill;
        summaryLabel.ForeColor = SystemColors.ControlDarkDark;
        summaryLabel.Location = new Point(15, 509);
        summaryLabel.Margin = new Padding(3, 0, 3, 8);
        summaryLabel.Size = new Size(1134, 15);
        summaryLabel.TabIndex = 7;
        summaryLabel.Text = "No copy job is running.";
        //
        // activityGroupBox
        //
        activityGroupBox.Controls.Add(activityTextBox);
        activityGroupBox.Dock = DockStyle.Fill;
        activityGroupBox.Location = new Point(15, 532);
        activityGroupBox.Padding = new Padding(10);
        activityGroupBox.Size = new Size(1134, 235);
        activityGroupBox.TabIndex = 8;
        activityGroupBox.TabStop = false;
        activityGroupBox.Text = "Activity";
        //
        // activityTextBox
        //
        activityTextBox.Dock = DockStyle.Fill;
        activityTextBox.Location = new Point(10, 26);
        activityTextBox.Multiline = true;
        activityTextBox.ReadOnly = true;
        activityTextBox.ScrollBars = ScrollBars.Vertical;
        activityTextBox.Size = new Size(1114, 228);
        activityTextBox.TabIndex = 0;
        //
        // MainForm
        //
        AcceptButton = startCopyButton;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1164, 779);
        Controls.Add(rootLayoutPanel);
        MinimumSize = new Size(1080, 720);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "FlatCopy Profile Exporter";
        rootLayoutPanel.ResumeLayout(false);
        rootLayoutPanel.PerformLayout();
        selectionLayoutPanel.ResumeLayout(false);
        profilesGroupBox.ResumeLayout(false);
        profilesLayoutPanel.ResumeLayout(false);
        profilesLayoutPanel.PerformLayout();
        profilesButtonsPanel.ResumeLayout(false);
        profilesButtonsPanel.PerformLayout();
        foldersGroupBox.ResumeLayout(false);
        foldersLayoutPanel.ResumeLayout(false);
        foldersLayoutPanel.PerformLayout();
        foldersButtonsPanel.ResumeLayout(false);
        foldersButtonsPanel.PerformLayout();
        destinationLayoutPanel.ResumeLayout(false);
        destinationLayoutPanel.PerformLayout();
        actionsPanel.ResumeLayout(false);
        actionsPanel.PerformLayout();
        activityGroupBox.ResumeLayout(false);
        activityGroupBox.PerformLayout();
        ResumeLayout(false);
    }
}
