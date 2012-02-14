namespace Teleopti.Ccc.LanguageManager
{
    partial class TranslationTool
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
            this.openFileDialogResX = new System.Windows.Forms.OpenFileDialog();
            this.textBoxResXFile = new System.Windows.Forms.TextBox();
            this.buttonBrowseFile = new System.Windows.Forms.Button();
            this.labelResXFile = new System.Windows.Forms.Label();
            this.comboBoxCultures = new System.Windows.Forms.ComboBox();
            this.labelSynchronizedLanguage = new System.Windows.Forms.Label();
            this.checkBoxNoSync = new System.Windows.Forms.CheckBox();
            this.buttonCreateTranslation = new System.Windows.Forms.Button();
            this.buttonAddNewLanguage = new System.Windows.Forms.Button();
            this.saveFileDialogForTranslation = new System.Windows.Forms.SaveFileDialog();
            this.buttonExportForTranslation = new System.Windows.Forms.Button();
            this.buttonImportFromTranslation = new System.Windows.Forms.Button();
            this.openFileDialogExcel = new System.Windows.Forms.OpenFileDialog();
            this.buttonExportSummary = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // openFileDialogResX
            // 
            this.openFileDialogResX.DefaultExt = "resx";
            this.openFileDialogResX.FileName = "Resources.ResX";
            this.openFileDialogResX.Filter = "Resx files|*.resx";
            // 
            // textBoxResXFile
            // 
            this.textBoxResXFile.Location = new System.Drawing.Point(145, 6);
            this.textBoxResXFile.Name = "textBoxResXFile";
            this.textBoxResXFile.Size = new System.Drawing.Size(239, 20);
            this.textBoxResXFile.TabIndex = 1;
            // 
            // buttonBrowseFile
            // 
            this.buttonBrowseFile.Location = new System.Drawing.Point(390, 4);
            this.buttonBrowseFile.Name = "buttonBrowseFile";
            this.buttonBrowseFile.Size = new System.Drawing.Size(75, 23);
            this.buttonBrowseFile.TabIndex = 2;
            this.buttonBrowseFile.Text = "Browse...";
            this.buttonBrowseFile.UseVisualStyleBackColor = true;
            this.buttonBrowseFile.Click += new System.EventHandler(this.buttonBrowseFile_Click);
            // 
            // labelResXFile
            // 
            this.labelResXFile.Location = new System.Drawing.Point(12, 9);
            this.labelResXFile.Name = "labelResXFile";
            this.labelResXFile.Size = new System.Drawing.Size(110, 21);
            this.labelResXFile.TabIndex = 7;
            this.labelResXFile.Text = "Main ResX file:";
            // 
            // comboBoxCultures
            // 
            this.comboBoxCultures.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.comboBoxCultures.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.comboBoxCultures.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxCultures.Enabled = false;
            this.comboBoxCultures.FormattingEnabled = true;
            this.comboBoxCultures.Location = new System.Drawing.Point(145, 36);
            this.comboBoxCultures.Name = "comboBoxCultures";
            this.comboBoxCultures.Size = new System.Drawing.Size(239, 21);
            this.comboBoxCultures.TabIndex = 3;
            // 
            // labelSynchronizedLanguage
            // 
            this.labelSynchronizedLanguage.Location = new System.Drawing.Point(12, 39);
            this.labelSynchronizedLanguage.Name = "labelSynchronizedLanguage";
            this.labelSynchronizedLanguage.Size = new System.Drawing.Size(127, 24);
            this.labelSynchronizedLanguage.TabIndex = 5;
            this.labelSynchronizedLanguage.Text = "Synchronized language:";
            // 
            // checkBoxNoSync
            // 
            this.checkBoxNoSync.Checked = true;
            this.checkBoxNoSync.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxNoSync.Location = new System.Drawing.Point(390, 34);
            this.checkBoxNoSync.Name = "checkBoxNoSync";
            this.checkBoxNoSync.Size = new System.Drawing.Size(65, 24);
            this.checkBoxNoSync.TabIndex = 4;
            this.checkBoxNoSync.Text = "None";
            this.checkBoxNoSync.UseVisualStyleBackColor = true;
            this.checkBoxNoSync.CheckStateChanged += new System.EventHandler(this.checkBoxNoSync_CheckStateChanged);
            // 
            // buttonCreateTranslation
            // 
            this.buttonCreateTranslation.Location = new System.Drawing.Point(12, 97);
            this.buttonCreateTranslation.Name = "buttonCreateTranslation";
            this.buttonCreateTranslation.Size = new System.Drawing.Size(145, 23);
            this.buttonCreateTranslation.TabIndex = 5;
            this.buttonCreateTranslation.Text = "Add UI texts to resource";
            this.buttonCreateTranslation.UseVisualStyleBackColor = true;
            this.buttonCreateTranslation.Click += new System.EventHandler(this.buttonCreateTranslation_Click);
            // 
            // buttonAddNewLanguage
            // 
            this.buttonAddNewLanguage.Location = new System.Drawing.Point(12, 126);
            this.buttonAddNewLanguage.Name = "buttonAddNewLanguage";
            this.buttonAddNewLanguage.Size = new System.Drawing.Size(145, 23);
            this.buttonAddNewLanguage.TabIndex = 6;
            this.buttonAddNewLanguage.Text = "Add new language";
            this.buttonAddNewLanguage.UseVisualStyleBackColor = true;
            this.buttonAddNewLanguage.Click += new System.EventHandler(this.buttonAddNewLanguage_Click);
            // 
            // saveFileDialogForTranslation
            // 
            this.saveFileDialogForTranslation.DefaultExt = "xls";
            this.saveFileDialogForTranslation.Filter = "Excel files|*.xls";
            // 
            // buttonExportForTranslation
            // 
            this.buttonExportForTranslation.Location = new System.Drawing.Point(320, 97);
            this.buttonExportForTranslation.Name = "buttonExportForTranslation";
            this.buttonExportForTranslation.Size = new System.Drawing.Size(145, 23);
            this.buttonExportForTranslation.TabIndex = 7;
            this.buttonExportForTranslation.Text = "Export for translation";
            this.buttonExportForTranslation.UseVisualStyleBackColor = true;
            this.buttonExportForTranslation.Click += new System.EventHandler(this.buttonExportForTranslation_Click);
            // 
            // buttonImportFromTranslation
            // 
            this.buttonImportFromTranslation.Location = new System.Drawing.Point(320, 126);
            this.buttonImportFromTranslation.Name = "buttonImportFromTranslation";
            this.buttonImportFromTranslation.Size = new System.Drawing.Size(145, 23);
            this.buttonImportFromTranslation.TabIndex = 8;
            this.buttonImportFromTranslation.Text = "Import translated texts";
            this.buttonImportFromTranslation.UseVisualStyleBackColor = true;
            this.buttonImportFromTranslation.Click += new System.EventHandler(this.buttonImportFromTranslation_Click);
            // 
            // openFileDialogExcel
            // 
            this.openFileDialogExcel.DefaultExt = "xls";
            this.openFileDialogExcel.FileName = "Resources.xls";
            this.openFileDialogExcel.Filter = "Excel files|*.xls";
            // 
            // buttonExportSummary
            // 
            this.buttonExportSummary.Location = new System.Drawing.Point(169, 97);
            this.buttonExportSummary.Name = "buttonExportSummary";
            this.buttonExportSummary.Size = new System.Drawing.Size(145, 23);
            this.buttonExportSummary.TabIndex = 9;
            this.buttonExportSummary.Text = "Export language summary";
            this.buttonExportSummary.UseVisualStyleBackColor = true;
            this.buttonExportSummary.Click += new System.EventHandler(this.buttonExportSummary_Click);
            // 
            // TranslationTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(477, 162);
            this.Controls.Add(this.buttonExportSummary);
            this.Controls.Add(this.buttonImportFromTranslation);
            this.Controls.Add(this.buttonExportForTranslation);
            this.Controls.Add(this.buttonAddNewLanguage);
            this.Controls.Add(this.buttonCreateTranslation);
            this.Controls.Add(this.checkBoxNoSync);
            this.Controls.Add(this.labelSynchronizedLanguage);
            this.Controls.Add(this.comboBoxCultures);
            this.Controls.Add(this.labelResXFile);
            this.Controls.Add(this.buttonBrowseFile);
            this.Controls.Add(this.textBoxResXFile);
            this.MaximizeBox = false;
            this.Name = "TranslationTool";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Language Manager";
            this.Load += new System.EventHandler(this.TranslationTool_Load);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.TranslationTool_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialogResX;
        private System.Windows.Forms.TextBox textBoxResXFile;
        private System.Windows.Forms.Button buttonBrowseFile;
        private System.Windows.Forms.Label labelResXFile;
        private System.Windows.Forms.ComboBox comboBoxCultures;
        private System.Windows.Forms.Label labelSynchronizedLanguage;
        private System.Windows.Forms.CheckBox checkBoxNoSync;
        private System.Windows.Forms.Button buttonCreateTranslation;
        private System.Windows.Forms.Button buttonAddNewLanguage;
        private System.Windows.Forms.SaveFileDialog saveFileDialogForTranslation;
        private System.Windows.Forms.Button buttonExportForTranslation;
        private System.Windows.Forms.Button buttonImportFromTranslation;
        private System.Windows.Forms.OpenFileDialog openFileDialogExcel;
        private System.Windows.Forms.Button buttonExportSummary;
    }
}

