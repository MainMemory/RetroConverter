
namespace RetroConverter
{
	partial class Form1
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Windows.Forms.Label label2;
			System.Windows.Forms.Label label1;
			System.Windows.Forms.Label label3;
			this.loadProjectButton = new RetroConverter.SplitButton();
			this.levelSelector = new System.Windows.Forms.ComboBox();
			this.convertRSDKButton = new System.Windows.Forms.Button();
			this.openProjectDialog = new System.Windows.Forms.OpenFileDialog();
			this.loadGameConfigButton = new RetroConverter.SplitButton();
			this.versionSelector = new System.Windows.Forms.ComboBox();
			this.openGameConfigDialog = new System.Windows.Forms.OpenFileDialog();
			this.rsdkExportDialog = new System.Windows.Forms.FolderBrowserDialog();
			this.includeObjectsCheckBox = new System.Windows.Forms.CheckBox();
			this.titleCardScriptBox = new System.Windows.Forms.TextBox();
			this.titleCardScriptButton = new System.Windows.Forms.Button();
			this.titleCardScriptDialog = new System.Windows.Forms.OpenFileDialog();
			this.mruMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			label2 = new System.Windows.Forms.Label();
			label1 = new System.Windows.Forms.Label();
			label3 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Location = new System.Drawing.Point(165, 17);
			label2.Name = "label2";
			label2.Size = new System.Drawing.Size(36, 13);
			label2.TabIndex = 1;
			label2.Text = "Level:";
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new System.Drawing.Point(12, 46);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(78, 13);
			label1.TabIndex = 3;
			label1.Text = "RSDK Version:";
			// 
			// label3
			// 
			label3.AutoSize = true;
			label3.Location = new System.Drawing.Point(12, 75);
			label3.Name = "label3";
			label3.Size = new System.Drawing.Size(85, 13);
			label3.TabIndex = 8;
			label3.Text = "Title Card Script:";
			// 
			// loadProjectButton
			// 
			this.loadProjectButton.AutoSize = true;
			this.loadProjectButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.loadProjectButton.Location = new System.Drawing.Point(12, 12);
			this.loadProjectButton.Name = "loadProjectButton";
			this.loadProjectButton.Padding = new System.Windows.Forms.Padding(0, 0, 20, 0);
			this.loadProjectButton.Size = new System.Drawing.Size(147, 23);
			this.loadProjectButton.TabIndex = 0;
			this.loadProjectButton.Text = "Load SonLVL Project...";
			this.loadProjectButton.UseVisualStyleBackColor = true;
			this.loadProjectButton.ShowMenu += new System.EventHandler(this.loadProjectButton_ShowMenu);
			this.loadProjectButton.Click += new System.EventHandler(this.loadProjectButton_Click);
			// 
			// levelSelector
			// 
			this.levelSelector.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.levelSelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.levelSelector.FormattingEnabled = true;
			this.levelSelector.Location = new System.Drawing.Point(207, 14);
			this.levelSelector.Name = "levelSelector";
			this.levelSelector.Size = new System.Drawing.Size(236, 21);
			this.levelSelector.TabIndex = 2;
			this.levelSelector.SelectedIndexChanged += new System.EventHandler(this.levelSelector_SelectedIndexChanged);
			// 
			// convertRSDKButton
			// 
			this.convertRSDKButton.AutoSize = true;
			this.convertRSDKButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.convertRSDKButton.Enabled = false;
			this.convertRSDKButton.Location = new System.Drawing.Point(380, 105);
			this.convertRSDKButton.Name = "convertRSDKButton";
			this.convertRSDKButton.Size = new System.Drawing.Size(63, 23);
			this.convertRSDKButton.TabIndex = 6;
			this.convertRSDKButton.Text = "Convert...";
			this.convertRSDKButton.UseVisualStyleBackColor = true;
			this.convertRSDKButton.Click += new System.EventHandler(this.convertRSDKButton_Click);
			// 
			// openProjectDialog
			// 
			this.openProjectDialog.DefaultExt = "ini";
			this.openProjectDialog.Filter = "SonLVL Projects|*.ini";
			this.openProjectDialog.RestoreDirectory = true;
			// 
			// loadGameConfigButton
			// 
			this.loadGameConfigButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.loadGameConfigButton.AutoSize = true;
			this.loadGameConfigButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.loadGameConfigButton.Location = new System.Drawing.Point(312, 41);
			this.loadGameConfigButton.Name = "loadGameConfigButton";
			this.loadGameConfigButton.Padding = new System.Windows.Forms.Padding(0, 0, 20, 0);
			this.loadGameConfigButton.Size = new System.Drawing.Size(131, 23);
			this.loadGameConfigButton.TabIndex = 5;
			this.loadGameConfigButton.Text = "Load GameConfig...";
			this.loadGameConfigButton.UseVisualStyleBackColor = true;
			this.loadGameConfigButton.ShowMenu += new System.EventHandler(this.loadGameConfigButton_ShowMenu);
			this.loadGameConfigButton.Click += new System.EventHandler(this.loadGameConfigButton_Click);
			// 
			// versionSelector
			// 
			this.versionSelector.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.versionSelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.versionSelector.FormattingEnabled = true;
			this.versionSelector.Items.AddRange(new object[] {
            "v3 (Sonic CD)",
            "v4 (Sonic 1/Sonic 2)",
            "v5 (Sonic Mania)"});
			this.versionSelector.Location = new System.Drawing.Point(96, 43);
			this.versionSelector.Name = "versionSelector";
			this.versionSelector.Size = new System.Drawing.Size(210, 21);
			this.versionSelector.TabIndex = 4;
			this.versionSelector.SelectedIndexChanged += new System.EventHandler(this.versionSelector_SelectedIndexChanged);
			// 
			// openGameConfigDialog
			// 
			this.openGameConfigDialog.DefaultExt = "bin";
			this.openGameConfigDialog.Filter = "GameConfig.bin|GameConfig.bin";
			this.openGameConfigDialog.RestoreDirectory = true;
			// 
			// includeObjectsCheckBox
			// 
			this.includeObjectsCheckBox.AutoSize = true;
			this.includeObjectsCheckBox.Location = new System.Drawing.Point(274, 109);
			this.includeObjectsCheckBox.Name = "includeObjectsCheckBox";
			this.includeObjectsCheckBox.Size = new System.Drawing.Size(100, 17);
			this.includeObjectsCheckBox.TabIndex = 7;
			this.includeObjectsCheckBox.Text = "Include Objects";
			this.includeObjectsCheckBox.UseVisualStyleBackColor = true;
			// 
			// titleCardScriptBox
			// 
			this.titleCardScriptBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.titleCardScriptBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
			this.titleCardScriptBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
			this.titleCardScriptBox.Location = new System.Drawing.Point(103, 72);
			this.titleCardScriptBox.Name = "titleCardScriptBox";
			this.titleCardScriptBox.Size = new System.Drawing.Size(273, 20);
			this.titleCardScriptBox.TabIndex = 9;
			// 
			// titleCardScriptButton
			// 
			this.titleCardScriptButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.titleCardScriptButton.AutoSize = true;
			this.titleCardScriptButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.titleCardScriptButton.Location = new System.Drawing.Point(382, 70);
			this.titleCardScriptButton.Name = "titleCardScriptButton";
			this.titleCardScriptButton.Size = new System.Drawing.Size(61, 23);
			this.titleCardScriptButton.TabIndex = 10;
			this.titleCardScriptButton.Text = "Browse...";
			this.titleCardScriptButton.UseVisualStyleBackColor = true;
			this.titleCardScriptButton.Click += new System.EventHandler(this.titleCardScriptButton_Click);
			// 
			// titleCardScriptDialog
			// 
			this.titleCardScriptDialog.DefaultExt = "txt";
			this.titleCardScriptDialog.Filter = "Scripts|*.txt";
			// 
			// mruMenuStrip
			// 
			this.mruMenuStrip.Name = "mruMenuStrip";
			this.mruMenuStrip.ShowImageMargin = false;
			this.mruMenuStrip.Size = new System.Drawing.Size(36, 4);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ClientSize = new System.Drawing.Size(455, 261);
			this.Controls.Add(this.titleCardScriptButton);
			this.Controls.Add(this.titleCardScriptBox);
			this.Controls.Add(label3);
			this.Controls.Add(this.includeObjectsCheckBox);
			this.Controls.Add(this.versionSelector);
			this.Controls.Add(label1);
			this.Controls.Add(this.loadGameConfigButton);
			this.Controls.Add(this.convertRSDKButton);
			this.Controls.Add(this.levelSelector);
			this.Controls.Add(label2);
			this.Controls.Add(this.loadProjectButton);
			this.Name = "Form1";
			this.Text = "RetroConverter";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
			this.Load += new System.EventHandler(this.Form1_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private SplitButton loadProjectButton;
		private System.Windows.Forms.ComboBox levelSelector;
		private System.Windows.Forms.Button convertRSDKButton;
		private System.Windows.Forms.OpenFileDialog openProjectDialog;
		private SplitButton loadGameConfigButton;
		private System.Windows.Forms.ComboBox versionSelector;
		private System.Windows.Forms.OpenFileDialog openGameConfigDialog;
		private System.Windows.Forms.FolderBrowserDialog rsdkExportDialog;
		private System.Windows.Forms.CheckBox includeObjectsCheckBox;
		private System.Windows.Forms.TextBox titleCardScriptBox;
		private System.Windows.Forms.Button titleCardScriptButton;
		private System.Windows.Forms.OpenFileDialog titleCardScriptDialog;
		private System.Windows.Forms.ContextMenuStrip mruMenuStrip;
	}
}

