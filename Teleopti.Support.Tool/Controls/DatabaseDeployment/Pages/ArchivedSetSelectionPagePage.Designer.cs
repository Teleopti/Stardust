using System;

namespace Teleopti.Support.Tool.Controls.DatabaseDeployment.Pages
{
    partial class ArchivedSetSelectionPagePage
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
            this.labelFileSelection = new System.Windows.Forms.Label();
            this.textBoxArchivedFileSet = new System.Windows.Forms.TextBox();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.textBoxContents = new System.Windows.Forms.TextBox();
            this.linkLabelSkipThis = new System.Windows.Forms.LinkLabel();
            this.openFileDialogArchivedFile = new System.Windows.Forms.OpenFileDialog();
            this.SuspendLayout();
            // 
            // labelFileSelection
            // 
            this.labelFileSelection.AutoSize = true;
            this.labelFileSelection.Location = new System.Drawing.Point(3, 10);
            this.labelFileSelection.Name = "labelFileSelection";
            this.labelFileSelection.Size = new System.Drawing.Size(134, 13);
            this.labelFileSelection.TabIndex = 0;
            this.labelFileSelection.Text = "Archived File Set Selection";
            // 
            // textBoxArchivedFileSet
            // 
            this.textBoxArchivedFileSet.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxArchivedFileSet.Location = new System.Drawing.Point(0, 26);
            this.textBoxArchivedFileSet.Name = "textBoxArchivedFileSet";
            this.textBoxArchivedFileSet.Size = new System.Drawing.Size(629, 20);
            this.textBoxArchivedFileSet.TabIndex = 1;
            this.textBoxArchivedFileSet.TextChanged += new System.EventHandler(this.textBoxArchivedFileSet_TextChanged);
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBrowse.Location = new System.Drawing.Point(635, 26);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(25, 20);
            this.buttonBrowse.TabIndex = 2;
            this.buttonBrowse.Text = "...";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
            // 
            // textBoxContents
            // 
            this.textBoxContents.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxContents.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxContents.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxContents.Enabled = false;
            this.textBoxContents.Location = new System.Drawing.Point(0, 52);
            this.textBoxContents.Multiline = true;
            this.textBoxContents.Name = "textBoxContents";
            this.textBoxContents.ReadOnly = true;
            this.textBoxContents.Size = new System.Drawing.Size(660, 162);
            this.textBoxContents.TabIndex = 3;
            // 
            // linkLabelSkipThis
            // 
            this.linkLabelSkipThis.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabelSkipThis.AutoSize = true;
            this.linkLabelSkipThis.LinkColor = System.Drawing.Color.Blue;
            this.linkLabelSkipThis.Location = new System.Drawing.Point(446, 217);
            this.linkLabelSkipThis.Name = "linkLabelSkipThis";
            this.linkLabelSkipThis.Size = new System.Drawing.Size(214, 13);
            this.linkLabelSkipThis.TabIndex = 4;
            this.linkLabelSkipThis.TabStop = true;
            this.linkLabelSkipThis.Text = "&Skip this step, I\'ll select the files manually >>";
            this.linkLabelSkipThis.VisitedLinkColor = System.Drawing.Color.Blue;
            this.linkLabelSkipThis.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelSkipThis_LinkClicked);
            // 
            // openFileDialogArchivedFile
            // 
            this.openFileDialogArchivedFile.DefaultExt = "zip";
            this.openFileDialogArchivedFile.Filter = "Compressed Archives (*.zip)|*.zip";
            // 
            // ArchivedSetSelectionPagePage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.linkLabelSkipThis);
            this.Controls.Add(this.textBoxContents);
            this.Controls.Add(this.buttonBrowse);
            this.Controls.Add(this.textBoxArchivedFileSet);
            this.Controls.Add(this.labelFileSelection);
            this.Name = "ArchivedSetSelectionPagePage";
            this.Size = new System.Drawing.Size(660, 240);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
	    #endregion

        private System.Windows.Forms.Label labelFileSelection;
        private System.Windows.Forms.TextBox textBoxArchivedFileSet;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.TextBox textBoxContents;
        private System.Windows.Forms.LinkLabel linkLabelSkipThis;
        private System.Windows.Forms.OpenFileDialog openFileDialogArchivedFile;
    }
}
