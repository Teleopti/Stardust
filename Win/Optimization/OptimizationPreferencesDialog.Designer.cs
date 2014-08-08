namespace Teleopti.Ccc.Win.Optimization
{
	partial class OptimizationPreferencesDialog
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxShifts"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Syncfusion.Windows.Forms.Tools.RibbonControlAdv.set_MaximizeToolTip(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Syncfusion.Windows.Forms.Tools.RibbonControlAdv.set_MinimizeToolTip(System.String)")]
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.buttonOK = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.tabControlTopLevel = new Syncfusion.Windows.Forms.Tools.TabControlAdv();
			this.tabPageGeneral = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.generalPreferencesPanel1 = new Teleopti.Ccc.Win.Optimization.GeneralPreferencesPanel();
			this.tabPageDaysOff = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.dayOffPreferencesPanel1 = new Teleopti.Ccc.Win.Optimization.DayOffPreferencesPanel();
			this.tabPageExtra = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.extraPreferencesPanel1 = new Teleopti.Ccc.Win.Optimization.ExtraPreferencesPanel();
			this.tabPageShifts = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.shiftsPreferencesPanel1 = new Teleopti.Ccc.Win.Optimization.ShiftsPreferencesPanel();
			this.tabPageAdvanced = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.advancedPreferencesPanel1 = new Teleopti.Ccc.Win.Optimization.AdvancedPreferencesPanel();
			this.tableLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.tabControlTopLevel)).BeginInit();
			this.tabControlTopLevel.SuspendLayout();
			this.tabPageGeneral.SuspendLayout();
			this.tabPageDaysOff.SuspendLayout();
			this.tabPageExtra.SuspendLayout();
			this.tabPageShifts.SuspendLayout();
			this.tabPageAdvanced.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.tabControlTopLevel, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(498, 573);
			this.tableLayoutPanel1.TabIndex = 4;
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 2;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.tableLayoutPanel2.Controls.Add(this.buttonOK, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.buttonCancel, 1, 0);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 527);
			this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 1;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(498, 46);
			this.tableLayoutPanel2.TabIndex = 4;
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonOK.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonOK.ForeColor = System.Drawing.Color.White;
			this.buttonOK.IsBackStageButton = false;
			this.buttonOK.Location = new System.Drawing.Point(281, 9);
			this.buttonOK.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(87, 27);
			this.buttonOK.TabIndex = 10;
			this.buttonOK.Text = "xxOk";
			this.buttonOK.UseVisualStyle = true;
			this.buttonOK.Click += new System.EventHandler(this.buttonOkClick);
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonCancel.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonCancel.BorderStyleAdv = Syncfusion.Windows.Forms.ButtonAdvBorderStyle.None;
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonCancel.ForeColor = System.Drawing.Color.White;
			this.buttonCancel.IsBackStageButton = false;
			this.buttonCancel.Location = new System.Drawing.Point(401, 9);
			this.buttonCancel.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(87, 27);
			this.buttonCancel.TabIndex = 10;
			this.buttonCancel.Text = "xxCancel";
			this.buttonCancel.UseVisualStyle = true;
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancelClick);
			// 
			// tabControlTopLevel
			// 
			this.tabControlTopLevel.ActiveTabColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(102)))), ((int)(((byte)(102)))));
			this.tabControlTopLevel.BeforeTouchSize = new System.Drawing.Size(492, 521);
			this.tabControlTopLevel.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.tabControlTopLevel.Controls.Add(this.tabPageGeneral);
			this.tabControlTopLevel.Controls.Add(this.tabPageDaysOff);
			this.tabControlTopLevel.Controls.Add(this.tabPageExtra);
			this.tabControlTopLevel.Controls.Add(this.tabPageShifts);
			this.tabControlTopLevel.Controls.Add(this.tabPageAdvanced);
			this.tabControlTopLevel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControlTopLevel.InactiveTabColor = System.Drawing.Color.White;
			this.tabControlTopLevel.ItemSize = new System.Drawing.Size(111, 22);
			this.tabControlTopLevel.Location = new System.Drawing.Point(3, 3);
			this.tabControlTopLevel.Name = "tabControlTopLevel";
			this.tabControlTopLevel.Size = new System.Drawing.Size(492, 521);
			this.tabControlTopLevel.TabIndex = 6;
			this.tabControlTopLevel.TabPanelBackColor = System.Drawing.Color.White;
			this.tabControlTopLevel.TabStyle = typeof(Syncfusion.Windows.Forms.Tools.TabRendererMetro);
			this.tabControlTopLevel.SelectedIndexChanged += new System.EventHandler(this.tabControlTopLevelSelectedIndexChanged);
			// 
			// tabPageGeneral
			// 
			this.tabPageGeneral.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.tabPageGeneral.Controls.Add(this.generalPreferencesPanel1);
			this.tabPageGeneral.Image = null;
			this.tabPageGeneral.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageGeneral.Location = new System.Drawing.Point(0, 21);
			this.tabPageGeneral.Name = "tabPageGeneral";
			this.tabPageGeneral.Padding = new System.Windows.Forms.Padding(10);
			this.tabPageGeneral.ShowCloseButton = true;
			this.tabPageGeneral.Size = new System.Drawing.Size(492, 500);
			this.tabPageGeneral.TabIndex = 1;
			this.tabPageGeneral.Text = "xxGeneral";
			this.tabPageGeneral.ThemesEnabled = false;
			// 
			// generalPreferencesPanel1
			// 
			this.generalPreferencesPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.generalPreferencesPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.generalPreferencesPanel1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.generalPreferencesPanel1.Location = new System.Drawing.Point(10, 10);
			this.generalPreferencesPanel1.Name = "generalPreferencesPanel1";
			this.generalPreferencesPanel1.Size = new System.Drawing.Size(470, 478);
			this.generalPreferencesPanel1.TabIndex = 0;
			// 
			// tabPageDaysOff
			// 
			this.tabPageDaysOff.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.tabPageDaysOff.Controls.Add(this.dayOffPreferencesPanel1);
			this.tabPageDaysOff.Image = null;
			this.tabPageDaysOff.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageDaysOff.Location = new System.Drawing.Point(0, 21);
			this.tabPageDaysOff.Name = "tabPageDaysOff";
			this.tabPageDaysOff.Padding = new System.Windows.Forms.Padding(10);
			this.tabPageDaysOff.ShowCloseButton = true;
			this.tabPageDaysOff.Size = new System.Drawing.Size(492, 500);
			this.tabPageDaysOff.TabIndex = 1;
			this.tabPageDaysOff.Text = "xxDaysOff";
			this.tabPageDaysOff.ThemesEnabled = false;
			// 
			// dayOffPreferencesPanel1
			// 
			this.dayOffPreferencesPanel1.BackColor = System.Drawing.Color.Transparent;
			this.dayOffPreferencesPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dayOffPreferencesPanel1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dayOffPreferencesPanel1.Location = new System.Drawing.Point(10, 10);
			this.dayOffPreferencesPanel1.Name = "dayOffPreferencesPanel1";
			this.dayOffPreferencesPanel1.Size = new System.Drawing.Size(470, 478);
			this.dayOffPreferencesPanel1.TabIndex = 0;
			// 
			// tabPageExtra
			// 
			this.tabPageExtra.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.tabPageExtra.Controls.Add(this.extraPreferencesPanel1);
			this.tabPageExtra.Image = null;
			this.tabPageExtra.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageExtra.Location = new System.Drawing.Point(0, 21);
			this.tabPageExtra.Name = "tabPageExtra";
			this.tabPageExtra.Padding = new System.Windows.Forms.Padding(10);
			this.tabPageExtra.ShowCloseButton = true;
			this.tabPageExtra.Size = new System.Drawing.Size(492, 500);
			this.tabPageExtra.TabIndex = 2;
			this.tabPageExtra.Text = "xxExtra";
			this.tabPageExtra.ThemesEnabled = false;
			// 
			// extraPreferencesPanel1
			// 
			this.extraPreferencesPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.extraPreferencesPanel1.BackColor = System.Drawing.Color.Transparent;
			this.extraPreferencesPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.extraPreferencesPanel1.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.extraPreferencesPanel1.Location = new System.Drawing.Point(10, 10);
			this.extraPreferencesPanel1.Name = "extraPreferencesPanel1";
			this.extraPreferencesPanel1.Size = new System.Drawing.Size(470, 478);
			this.extraPreferencesPanel1.TabIndex = 0;
			// 
			// tabPageShifts
			// 
			this.tabPageShifts.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.tabPageShifts.Controls.Add(this.shiftsPreferencesPanel1);
			this.tabPageShifts.Image = null;
			this.tabPageShifts.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageShifts.Location = new System.Drawing.Point(0, 21);
			this.tabPageShifts.Name = "tabPageShifts";
			this.tabPageShifts.Padding = new System.Windows.Forms.Padding(10);
			this.tabPageShifts.ShowCloseButton = true;
			this.tabPageShifts.Size = new System.Drawing.Size(492, 500);
			this.tabPageShifts.TabIndex = 4;
			this.tabPageShifts.Text = "xxShifts";
			this.tabPageShifts.ThemesEnabled = false;
			// 
			// shiftsPreferencesPanel1
			// 
			this.shiftsPreferencesPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.shiftsPreferencesPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.shiftsPreferencesPanel1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.shiftsPreferencesPanel1.Location = new System.Drawing.Point(10, 10);
			this.shiftsPreferencesPanel1.Name = "shiftsPreferencesPanel1";
			this.shiftsPreferencesPanel1.Size = new System.Drawing.Size(470, 478);
			this.shiftsPreferencesPanel1.TabIndex = 0;
			// 
			// tabPageAdvanced
			// 
			this.tabPageAdvanced.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.tabPageAdvanced.Controls.Add(this.advancedPreferencesPanel1);
			this.tabPageAdvanced.Image = null;
			this.tabPageAdvanced.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageAdvanced.Location = new System.Drawing.Point(0, 21);
			this.tabPageAdvanced.Name = "tabPageAdvanced";
			this.tabPageAdvanced.ShowCloseButton = true;
			this.tabPageAdvanced.Size = new System.Drawing.Size(492, 500);
			this.tabPageAdvanced.TabIndex = 3;
			this.tabPageAdvanced.Text = "xxAdvanced";
			this.tabPageAdvanced.ThemesEnabled = false;
			// 
			// advancedPreferencesPanel1
			// 
			this.advancedPreferencesPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.advancedPreferencesPanel1.BackColor = System.Drawing.Color.Transparent;
			this.advancedPreferencesPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.advancedPreferencesPanel1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.advancedPreferencesPanel1.Location = new System.Drawing.Point(0, 0);
			this.advancedPreferencesPanel1.Name = "advancedPreferencesPanel1";
			this.advancedPreferencesPanel1.Padding = new System.Windows.Forms.Padding(10);
			this.advancedPreferencesPanel1.Size = new System.Drawing.Size(490, 498);
			this.advancedPreferencesPanel1.TabIndex = 0;
			// 
			// OptimizationPreferencesDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.BorderColor = System.Drawing.Color.Blue;
			this.CaptionFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ClientSize = new System.Drawing.Size(498, 573);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.HelpButton = false;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(295, 40);
			this.Name = "OptimizationPreferencesDialog";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "xxSchedulingSessionOptions";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.optimizationPreferencesDialogFormClosing);
			this.Load += new System.EventHandler(this.formLoad);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.tabControlTopLevel)).EndInit();
			this.tabControlTopLevel.ResumeLayout(false);
			this.tabPageGeneral.ResumeLayout(false);
			this.tabPageDaysOff.ResumeLayout(false);
			this.tabPageExtra.ResumeLayout(false);
			this.tabPageShifts.ResumeLayout(false);
			this.tabPageAdvanced.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		private Syncfusion.Windows.Forms.ButtonAdv buttonOK;
		private Syncfusion.Windows.Forms.ButtonAdv buttonCancel;
		  private Syncfusion.Windows.Forms.Tools.TabControlAdv tabControlTopLevel;
		  private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageDaysOff;
		  private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageGeneral;
		  private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageExtra;
		  private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageAdvanced;
		private GeneralPreferencesPanel generalPreferencesPanel1;
		private DayOffPreferencesPanel dayOffPreferencesPanel1;
		private ExtraPreferencesPanel  extraPreferencesPanel1;
		private AdvancedPreferencesPanel advancedPreferencesPanel1;
		  private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageShifts;
		private ShiftsPreferencesPanel shiftsPreferencesPanel1;
	}
}