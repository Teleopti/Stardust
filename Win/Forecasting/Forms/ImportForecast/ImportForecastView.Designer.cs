using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Win.Forecasting.Forms.ImportForecast
{
	 partial class ImportForecastView
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
		  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Syncfusion.Windows.Forms.Tools.RibbonControlAdv.set_MaximizeToolTip(System.String)")]
		  private void InitializeComponent()
		  {
			this.components = new System.ComponentModel.Container();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.radioButtonImportStaffing = new Syncfusion.Windows.Forms.Tools.RadioButtonAdv();
			this.lblSkillNameColon = new System.Windows.Forms.Label();
			this.txtSkillName = new System.Windows.Forms.Label();
			this.lblImportFileName = new System.Windows.Forms.Label();
			this.textBoxImportFileName = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
			this.buttonAdvBrowseFile = new Syncfusion.Windows.Forms.ButtonAdv();
			this.radioButtonImportWLAndStaffing = new Syncfusion.Windows.Forms.Tools.RadioButtonAdv();
			this.radioButtonImportWorkload = new Syncfusion.Windows.Forms.Tools.RadioButtonAdv();
			this.labelWorkloadName = new System.Windows.Forms.Label();
			this.lblWorkloadNameColon = new System.Windows.Forms.Label();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.buttonAdvImport = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvClose = new Syncfusion.Windows.Forms.ButtonAdv();
			this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonImportStaffing)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.textBoxImportFileName)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonImportWLAndStaffing)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonImportWorkload)).BeginInit();
			this.tableLayoutPanel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 276F));
			this.tableLayoutPanel1.Controls.Add(this.radioButtonImportStaffing, 2, 1);
			this.tableLayoutPanel1.Controls.Add(this.lblSkillNameColon, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.txtSkillName, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.lblImportFileName, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.textBoxImportFileName, 1, 3);
			this.tableLayoutPanel1.Controls.Add(this.buttonAdvBrowseFile, 2, 3);
			this.tableLayoutPanel1.Controls.Add(this.radioButtonImportWLAndStaffing, 2, 2);
			this.tableLayoutPanel1.Controls.Add(this.radioButtonImportWorkload, 2, 0);
			this.tableLayoutPanel1.Controls.Add(this.labelWorkloadName, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.lblWorkloadNameColon, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 4);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 5;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(775, 201);
			this.tableLayoutPanel1.TabIndex = 1;
			// 
			// radioButtonImportStaffing
			// 
			this.radioButtonImportStaffing.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.radioButtonImportStaffing.BeforeTouchSize = new System.Drawing.Size(271, 20);
			this.radioButtonImportStaffing.DrawFocusRectangle = false;
			this.radioButtonImportStaffing.Location = new System.Drawing.Point(501, 33);
			this.radioButtonImportStaffing.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.radioButtonImportStaffing.Name = "radioButtonImportStaffing";
			this.radioButtonImportStaffing.Size = new System.Drawing.Size(271, 20);
			this.radioButtonImportStaffing.Style = Syncfusion.Windows.Forms.Tools.RadioButtonAdvStyle.Metro;
			this.radioButtonImportStaffing.TabIndex = 1;
			this.radioButtonImportStaffing.TabStop = false;
			this.radioButtonImportStaffing.Text = "xxImportStaffing";
			this.radioButtonImportStaffing.ThemesEnabled = false;
			this.radioButtonImportStaffing.CheckChanged += new System.EventHandler(this.radioButtonImportStaffingCheckedChanged);
			// 
			// lblSkillNameColon
			// 
			this.lblSkillNameColon.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.lblSkillNameColon.AutoSize = true;
			this.lblSkillNameColon.BackColor = System.Drawing.Color.Transparent;
			this.lblSkillNameColon.ForeColor = System.Drawing.SystemColors.ControlText;
			this.lblSkillNameColon.Location = new System.Drawing.Point(3, 7);
			this.lblSkillNameColon.Name = "lblSkillNameColon";
			this.lblSkillNameColon.Size = new System.Drawing.Size(102, 15);
			this.lblSkillNameColon.TabIndex = 0;
			this.lblSkillNameColon.Text = "xxSkillNameColon";
			this.lblSkillNameColon.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// txtSkillName
			// 
			this.txtSkillName.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.txtSkillName.AutoSize = true;
			this.txtSkillName.BackColor = System.Drawing.Color.Transparent;
			this.txtSkillName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtSkillName.ForeColor = System.Drawing.SystemColors.ControlText;
			this.txtSkillName.Location = new System.Drawing.Point(152, 8);
			this.txtSkillName.Name = "txtSkillName";
			this.txtSkillName.Size = new System.Drawing.Size(0, 13);
			this.txtSkillName.TabIndex = 1;
			this.txtSkillName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblImportFileName
			// 
			this.lblImportFileName.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.lblImportFileName.AutoSize = true;
			this.lblImportFileName.BackColor = System.Drawing.Color.Transparent;
			this.lblImportFileName.ForeColor = System.Drawing.SystemColors.ControlText;
			this.lblImportFileName.Location = new System.Drawing.Point(3, 97);
			this.lblImportFileName.Name = "lblImportFileName";
			this.lblImportFileName.Size = new System.Drawing.Size(135, 15);
			this.lblImportFileName.TabIndex = 4;
			this.lblImportFileName.Text = "xxImportFileNameColon";
			this.lblImportFileName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textBoxImportFileName
			// 
			this.textBoxImportFileName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxImportFileName.BackColor = System.Drawing.Color.White;
			this.textBoxImportFileName.BeforeTouchSize = new System.Drawing.Size(343, 23);
			this.textBoxImportFileName.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxImportFileName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textBoxImportFileName.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.textBoxImportFileName.Location = new System.Drawing.Point(152, 93);
			this.textBoxImportFileName.Metrocolor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxImportFileName.Name = "textBoxImportFileName";
			this.textBoxImportFileName.Size = new System.Drawing.Size(343, 23);
			this.textBoxImportFileName.Style = Syncfusion.Windows.Forms.Tools.TextBoxExt.theme.Metro;
			this.textBoxImportFileName.TabIndex = 3;
			this.textBoxImportFileName.TextChanged += new System.EventHandler(this.textBoxImportFileNameTextChanged);
			// 
			// buttonAdvBrowseFile
			// 
			this.buttonAdvBrowseFile.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.buttonAdvBrowseFile.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvBrowseFile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvBrowseFile.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonAdvBrowseFile.ForeColor = System.Drawing.Color.White;
			this.buttonAdvBrowseFile.IsBackStageButton = false;
			this.buttonAdvBrowseFile.Location = new System.Drawing.Point(501, 91);
			this.buttonAdvBrowseFile.Name = "buttonAdvBrowseFile";
			this.buttonAdvBrowseFile.Size = new System.Drawing.Size(87, 27);
			this.buttonAdvBrowseFile.TabIndex = 4;
			this.buttonAdvBrowseFile.Text = "xxBrowse";
			this.buttonAdvBrowseFile.UseVisualStyle = true;
			this.buttonAdvBrowseFile.Click += new System.EventHandler(this.browseImportFileButtonClick);
			// 
			// radioButtonImportWLAndStaffing
			// 
			this.radioButtonImportWLAndStaffing.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.radioButtonImportWLAndStaffing.BeforeTouchSize = new System.Drawing.Size(271, 20);
			this.radioButtonImportWLAndStaffing.DrawFocusRectangle = false;
			this.radioButtonImportWLAndStaffing.Location = new System.Drawing.Point(501, 62);
			this.radioButtonImportWLAndStaffing.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.radioButtonImportWLAndStaffing.Name = "radioButtonImportWLAndStaffing";
			this.radioButtonImportWLAndStaffing.Size = new System.Drawing.Size(271, 20);
			this.radioButtonImportWLAndStaffing.Style = Syncfusion.Windows.Forms.Tools.RadioButtonAdvStyle.Metro;
			this.radioButtonImportWLAndStaffing.TabIndex = 2;
			this.radioButtonImportWLAndStaffing.TabStop = false;
			this.radioButtonImportWLAndStaffing.Text = "xxImportWorkloadAndStaffing";
			this.radioButtonImportWLAndStaffing.ThemesEnabled = false;
			this.radioButtonImportWLAndStaffing.CheckChanged += new System.EventHandler(this.radioButtonImportWlAndStaffingCheckedChanged);
			// 
			// radioButtonImportWorkload
			// 
			this.radioButtonImportWorkload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.radioButtonImportWorkload.BeforeTouchSize = new System.Drawing.Size(271, 20);
			this.radioButtonImportWorkload.Checked = true;
			this.radioButtonImportWorkload.DrawFocusRectangle = false;
			this.radioButtonImportWorkload.Location = new System.Drawing.Point(501, 4);
			this.radioButtonImportWorkload.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.radioButtonImportWorkload.Name = "radioButtonImportWorkload";
			this.radioButtonImportWorkload.Size = new System.Drawing.Size(271, 20);
			this.radioButtonImportWorkload.Style = Syncfusion.Windows.Forms.Tools.RadioButtonAdvStyle.Metro;
			this.radioButtonImportWorkload.TabIndex = 0;
			this.radioButtonImportWorkload.Text = "xxImportWorkload";
			this.radioButtonImportWorkload.ThemesEnabled = false;
			this.radioButtonImportWorkload.CheckChanged += new System.EventHandler(this.radioButtonImportWorkloadCheckedChanged);
			// 
			// labelWorkloadName
			// 
			this.labelWorkloadName.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelWorkloadName.AutoSize = true;
			this.labelWorkloadName.Location = new System.Drawing.Point(152, 36);
			this.labelWorkloadName.Name = "labelWorkloadName";
			this.labelWorkloadName.Size = new System.Drawing.Size(0, 15);
			this.labelWorkloadName.TabIndex = 10;
			// 
			// lblWorkloadNameColon
			// 
			this.lblWorkloadNameColon.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.lblWorkloadNameColon.AutoSize = true;
			this.lblWorkloadNameColon.BackColor = System.Drawing.Color.Transparent;
			this.lblWorkloadNameColon.ForeColor = System.Drawing.SystemColors.ControlText;
			this.lblWorkloadNameColon.Location = new System.Drawing.Point(3, 36);
			this.lblWorkloadNameColon.Name = "lblWorkloadNameColon";
			this.lblWorkloadNameColon.Size = new System.Drawing.Size(132, 15);
			this.lblWorkloadNameColon.TabIndex = 2;
			this.lblWorkloadNameColon.Text = "xxWorkloadNameColon";
			this.lblWorkloadNameColon.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 2;
			this.tableLayoutPanel1.SetColumnSpan(this.tableLayoutPanel2, 3);
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.tableLayoutPanel2.Controls.Add(this.buttonAdvImport, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.buttonAdvClose, 1, 0);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 122);
			this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 1;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(775, 79);
			this.tableLayoutPanel2.TabIndex = 2;
			// 
			// buttonAdvImport
			// 
			this.buttonAdvImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvImport.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvImport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvImport.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonAdvImport.Enabled = false;
			this.buttonAdvImport.ForeColor = System.Drawing.Color.White;
			this.buttonAdvImport.IsBackStageButton = false;
			this.buttonAdvImport.Location = new System.Drawing.Point(558, 42);
			this.buttonAdvImport.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonAdvImport.Name = "buttonAdvImport";
			this.buttonAdvImport.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
			this.buttonAdvImport.Size = new System.Drawing.Size(87, 27);
			this.buttonAdvImport.TabIndex = 0;
			this.buttonAdvImport.Text = "xxImport";
			this.buttonAdvImport.UseVisualStyle = true;
			this.buttonAdvImport.Click += new System.EventHandler(this.buttonAdvImportClick);
			// 
			// buttonAdvClose
			// 
			this.buttonAdvClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvClose.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvClose.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonAdvClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonAdvClose.ForeColor = System.Drawing.Color.White;
			this.buttonAdvClose.IsBackStageButton = false;
			this.buttonAdvClose.Location = new System.Drawing.Point(678, 42);
			this.buttonAdvClose.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonAdvClose.Name = "buttonAdvClose";
			this.buttonAdvClose.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
			this.buttonAdvClose.Size = new System.Drawing.Size(87, 27);
			this.buttonAdvClose.TabIndex = 1;
			this.buttonAdvClose.Text = "xxClose";
			this.buttonAdvClose.UseVisualStyle = true;
			// 
			// ImportForecastView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Window;
			this.CancelButton = this.buttonAdvClose;
			this.CaptionFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ClientSize = new System.Drawing.Size(775, 201);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.MaximizeBox = false;
			this.MinimumSize = new System.Drawing.Size(259, 39);
			this.Name = "ImportForecastView";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "xxImportForecast";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonImportStaffing)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.textBoxImportFileName)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonImportWLAndStaffing)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonImportWorkload)).EndInit();
			this.tableLayoutPanel2.ResumeLayout(false);
			this.ResumeLayout(false);

		  }

		  #endregion

		  private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		  private System.Windows.Forms.Label lblSkillNameColon;
		  private System.Windows.Forms.Label txtSkillName;
		  private System.Windows.Forms.Label lblWorkloadNameColon;
		  private System.Windows.Forms.Label lblImportFileName;
		  private Syncfusion.Windows.Forms.Tools.TextBoxExt textBoxImportFileName;
		  private Syncfusion.Windows.Forms.ButtonAdv buttonAdvBrowseFile;
		  private System.Windows.Forms.OpenFileDialog openFileDialog;
		  private Syncfusion.Windows.Forms.ButtonAdv buttonAdvClose;
		  private Syncfusion.Windows.Forms.ButtonAdv buttonAdvImport;
		  private Syncfusion.Windows.Forms.Tools.RadioButtonAdv  radioButtonImportWorkload;
		  private Syncfusion.Windows.Forms.Tools.RadioButtonAdv radioButtonImportWLAndStaffing;
		  private Syncfusion.Windows.Forms.Tools.RadioButtonAdv radioButtonImportStaffing;
		  private System.Windows.Forms.Label labelWorkloadName;
		  private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
	 }
}