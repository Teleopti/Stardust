using System;
using System.Windows.Forms;

namespace Teleopti.Support.Tool.Controls.DatabaseDeployment.Pages
{
    partial class BackupSelectionPagePage
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelBackupFileSelection = new System.Windows.Forms.Label();
            this.radioButtonFromArchive = new System.Windows.Forms.RadioButton();
            this.comboBoxArchivedFile = new System.Windows.Forms.ComboBox();
            this.radioButtonExistingBackupFile = new System.Windows.Forms.RadioButton();
            this.textBoxExistingBackupFile = new System.Windows.Forms.TextBox();
            this.buttonBrowseBackupFile = new System.Windows.Forms.Button();
            this.radioButtonUseExistingDatabase = new System.Windows.Forms.RadioButton();
            this.comboBoxExistingDatabase = new System.Windows.Forms.ComboBox();
            this.labelRestoredDatabaseName = new System.Windows.Forms.Label();
            this.textBoxRestoredDatabaseName = new System.Windows.Forms.TextBox();
            this.openFileDialogExistingBackup = new System.Windows.Forms.OpenFileDialog();
            this.SuspendLayout();
            // 
            // labelBackupFileSelection
            // 
            this.labelBackupFileSelection.AutoSize = true;
            this.labelBackupFileSelection.Location = new System.Drawing.Point(3, 14);
            this.labelBackupFileSelection.Name = "labelBackupFileSelection";
            this.labelBackupFileSelection.Size = new System.Drawing.Size(105, 13);
            this.labelBackupFileSelection.TabIndex = 0;
            this.labelBackupFileSelection.Text = "Backup file selection";
            // 
            // radioButtonFromArchive
            // 
            this.radioButtonFromArchive.AutoSize = true;
            this.radioButtonFromArchive.Checked = true;
            this.radioButtonFromArchive.Location = new System.Drawing.Point(20, 42);
            this.radioButtonFromArchive.Name = "radioButtonFromArchive";
            this.radioButtonFromArchive.Size = new System.Drawing.Size(86, 17);
            this.radioButtonFromArchive.TabIndex = 1;
            this.radioButtonFromArchive.TabStop = true;
            this.radioButtonFromArchive.Text = "From &archive";
            this.radioButtonFromArchive.UseVisualStyleBackColor = true;
            this.radioButtonFromArchive.CheckedChanged += new System.EventHandler(this.radioButtonFromArchive_CheckedChanged);
            // 
            // comboBoxArchivedFile
            // 
            this.comboBoxArchivedFile.FormattingEnabled = true;
            this.comboBoxArchivedFile.Location = new System.Drawing.Point(163, 41);
            this.comboBoxArchivedFile.Name = "comboBoxArchivedFile";
            this.comboBoxArchivedFile.Size = new System.Drawing.Size(472, 21);
            this.comboBoxArchivedFile.TabIndex = 2;
			this.comboBoxArchivedFile.DropDownStyle = ComboBoxStyle.DropDownList;
			this.comboBoxArchivedFile.SelectedIndexChanged += selectionChanged;
            // 
            // radioButtonExistingBackupFile
            // 
            this.radioButtonExistingBackupFile.AutoSize = true;
            this.radioButtonExistingBackupFile.Location = new System.Drawing.Point(20, 84);
            this.radioButtonExistingBackupFile.Name = "radioButtonExistingBackupFile";
            this.radioButtonExistingBackupFile.Size = new System.Drawing.Size(137, 17);
            this.radioButtonExistingBackupFile.TabIndex = 3;
            this.radioButtonExistingBackupFile.Text = "Use existing &backup file";
            this.radioButtonExistingBackupFile.UseVisualStyleBackColor = true;
            this.radioButtonExistingBackupFile.CheckedChanged += new System.EventHandler(this.radioButtonExistingBackupFile_CheckedChanged);
            // 
            // textBoxExistingBackupFile
            // 
            this.textBoxExistingBackupFile.Location = new System.Drawing.Point(163, 83);
            this.textBoxExistingBackupFile.Name = "textBoxExistingBackupFile";
            this.textBoxExistingBackupFile.Size = new System.Drawing.Size(442, 20);
            this.textBoxExistingBackupFile.TabIndex = 4;
			this.textBoxExistingBackupFile.TextChanged += selectionChanged;
            // 
            // buttonBrowseBackupFile
            // 
            this.buttonBrowseBackupFile.Location = new System.Drawing.Point(611, 83);
            this.buttonBrowseBackupFile.Name = "buttonBrowseBackupFile";
            this.buttonBrowseBackupFile.Size = new System.Drawing.Size(24, 20);
            this.buttonBrowseBackupFile.TabIndex = 5;
            this.buttonBrowseBackupFile.Text = "...";
            this.buttonBrowseBackupFile.UseVisualStyleBackColor = true;
            this.buttonBrowseBackupFile.Click += new System.EventHandler(this.buttonBrowseBackupFile_Click);
            // 
            // radioButtonUseExistingDatabase
            // 
            this.radioButtonUseExistingDatabase.AutoSize = true;
            this.radioButtonUseExistingDatabase.Location = new System.Drawing.Point(20, 127);
            this.radioButtonUseExistingDatabase.Name = "radioButtonUseExistingDatabase";
            this.radioButtonUseExistingDatabase.Size = new System.Drawing.Size(129, 17);
            this.radioButtonUseExistingDatabase.TabIndex = 6;
            this.radioButtonUseExistingDatabase.Text = "Use &existing database";
            this.radioButtonUseExistingDatabase.UseVisualStyleBackColor = true;
            this.radioButtonUseExistingDatabase.CheckedChanged += new System.EventHandler(this.radioButtonUseExistingDatabase_CheckedChanged);
            // 
            // comboBoxExistingDatabase
            // 
            this.comboBoxExistingDatabase.FormattingEnabled = true;
            this.comboBoxExistingDatabase.Location = new System.Drawing.Point(163, 126);
            this.comboBoxExistingDatabase.Name = "comboBoxExistingDatabase";
            this.comboBoxExistingDatabase.Size = new System.Drawing.Size(472, 21);
            this.comboBoxExistingDatabase.TabIndex = 7;
			this.comboBoxExistingDatabase.DropDownStyle = ComboBoxStyle.DropDownList;
			this.comboBoxExistingDatabase.SelectedIndexChanged += selectionChanged;
            // 
            // labelRestoredDatabaseName
            // 
            this.labelRestoredDatabaseName.AutoSize = true;
            this.labelRestoredDatabaseName.Location = new System.Drawing.Point(3, 190);
            this.labelRestoredDatabaseName.Name = "labelRestoredDatabaseName";
            this.labelRestoredDatabaseName.Size = new System.Drawing.Size(148, 13);
            this.labelRestoredDatabaseName.TabIndex = 8;
            this.labelRestoredDatabaseName.Text = "Restored DatabaseInfo Name";
            // 
            // textBoxRestoredDatabaseName
            // 
            this.textBoxRestoredDatabaseName.Location = new System.Drawing.Point(163, 187);
            this.textBoxRestoredDatabaseName.Name = "textBoxRestoredDatabaseName";
            this.textBoxRestoredDatabaseName.Size = new System.Drawing.Size(472, 20);
            this.textBoxRestoredDatabaseName.TabIndex = 9;
            // 
            // openFileDialogExistingBackup
            // 
            this.openFileDialogExistingBackup.DefaultExt = "bak";
            this.openFileDialogExistingBackup.Filter = "SQL Server backup file (*.bak)|*.bak";
            // 
            // BackupSelectionPagePage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.textBoxRestoredDatabaseName);
            this.Controls.Add(this.labelRestoredDatabaseName);
            this.Controls.Add(this.comboBoxExistingDatabase);
            this.Controls.Add(this.radioButtonUseExistingDatabase);
            this.Controls.Add(this.buttonBrowseBackupFile);
            this.Controls.Add(this.textBoxExistingBackupFile);
            this.Controls.Add(this.radioButtonExistingBackupFile);
            this.Controls.Add(this.comboBoxArchivedFile);
            this.Controls.Add(this.radioButtonFromArchive);
            this.Controls.Add(this.labelBackupFileSelection);
            this.Name = "BackupSelectionPagePage";
            this.Size = new System.Drawing.Size(660, 240);
            this.ParentChanged += new System.EventHandler(this.backupSelectionPagePage_ParentChanged);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

	    #endregion

        private System.Windows.Forms.Label labelBackupFileSelection;
        private System.Windows.Forms.RadioButton radioButtonFromArchive;
        private System.Windows.Forms.ComboBox comboBoxArchivedFile;
        private System.Windows.Forms.RadioButton radioButtonExistingBackupFile;
        private System.Windows.Forms.TextBox textBoxExistingBackupFile;
        private System.Windows.Forms.Button buttonBrowseBackupFile;
        private System.Windows.Forms.RadioButton radioButtonUseExistingDatabase;
        private System.Windows.Forms.ComboBox comboBoxExistingDatabase;
        private System.Windows.Forms.Label labelRestoredDatabaseName;
        private System.Windows.Forms.TextBox textBoxRestoredDatabaseName;
        private System.Windows.Forms.OpenFileDialog openFileDialogExistingBackup;
    }
}
