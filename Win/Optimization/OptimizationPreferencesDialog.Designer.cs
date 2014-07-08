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
			this.ribbonControHeader = new Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.buttonOK = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.tabControlTopLevel = new System.Windows.Forms.TabControl();
			this.tabPageGeneral = new System.Windows.Forms.TabPage();
			this.generalPreferencesPanel1 = new Teleopti.Ccc.Win.Optimization.GeneralPreferencesPanel();
			this.tabPageDaysOff = new System.Windows.Forms.TabPage();
			this.dayOffPreferencesPanel1 = new Teleopti.Ccc.Win.Optimization.DayOffPreferencesPanel();
			this.tabPageExtra = new System.Windows.Forms.TabPage();
			this.extraPreferencesPanel1 = new Teleopti.Ccc.Win.Optimization.ExtraPreferencesPanel();
			this.tabPageShifts = new System.Windows.Forms.TabPage();
			this.shiftsPreferencesPanel1 = new Teleopti.Ccc.Win.Optimization.ShiftsPreferencesPanel();
			this.tabPageAdvanced = new System.Windows.Forms.TabPage();
			this.advancedPreferencesPanel1 = new Teleopti.Ccc.Win.Optimization.AdvancedPreferencesPanel();
			((System.ComponentModel.ISupportInitialize)(this.ribbonControHeader)).BeginInit();
			this.tableLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			this.tabControlTopLevel.SuspendLayout();
			this.tabPageGeneral.SuspendLayout();
			this.tabPageDaysOff.SuspendLayout();
			this.tabPageExtra.SuspendLayout();
			this.tabPageShifts.SuspendLayout();
			this.tabPageAdvanced.SuspendLayout();
			this.SuspendLayout();
			// 
			// ribbonControHeader
			// 
			this.ribbonControHeader.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ribbonControHeader.HideMenuButtonToolTip = false;
			this.ribbonControHeader.LauncherStyle = Syncfusion.Windows.Forms.Tools.LauncherStyle.Metro;
			this.ribbonControHeader.Location = new System.Drawing.Point(1, 0);
			this.ribbonControHeader.MaximizeToolTip = "Maximize Ribbon";
			this.ribbonControHeader.MenuButtonEnabled = true;
			this.ribbonControHeader.MenuButtonFont = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ribbonControHeader.MenuButtonText = "";
			this.ribbonControHeader.MenuButtonVisible = false;
			this.ribbonControHeader.MenuColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(114)))), ((int)(((byte)(198)))));
			this.ribbonControHeader.MinimizeToolTip = "Minimize Ribbon";
			this.ribbonControHeader.Name = "ribbonControHeader";
			this.ribbonControHeader.Office2013ColorScheme = Syncfusion.Windows.Forms.Tools.Office2013ColorScheme.White;
			// 
			// ribbonControHeader.OfficeMenu
			// 
			this.ribbonControHeader.OfficeMenu.Name = "OfficeMenu";
			this.ribbonControHeader.OfficeMenu.Size = new System.Drawing.Size(12, 65);
			this.ribbonControHeader.OverFlowButtonToolTip = "Show DropDown";
			this.ribbonControHeader.QuickPanelImageLayout = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.ribbonControHeader.QuickPanelVisible = false;
			this.ribbonControHeader.RibbonHeaderImage = Syncfusion.Windows.Forms.Tools.RibbonHeaderImage.None;
			this.ribbonControHeader.SelectedTab = null;
			this.ribbonControHeader.Show2010CustomizeQuickItemDialog = false;
			this.ribbonControHeader.ShowLauncher = false;
			this.ribbonControHeader.ShowRibbonDisplayOptionButton = true;
			this.ribbonControHeader.Size = new System.Drawing.Size(456, 33);
			this.ribbonControHeader.SystemText.QuickAccessDialogDropDownName = "StartMenu";
			this.ribbonControHeader.TabIndex = 2;
			this.ribbonControHeader.Text = "ribbonControlAdv1";
			this.ribbonControHeader.TitleColor = System.Drawing.Color.Black;
			this.ribbonControHeader.TitleFont = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.tabControlTopLevel, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(6, 34);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(446, 530);
			this.tableLayoutPanel1.TabIndex = 4;
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel2.ColumnCount = 2;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel2.Controls.Add(this.buttonOK, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.buttonCancel, 1, 0);
			this.tableLayoutPanel2.Location = new System.Drawing.Point(268, 496);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 1;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(175, 31);
			this.tableLayoutPanel2.TabIndex = 4;
			// 
			// buttonOK
			// 
			this.buttonOK.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonOK.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonOK.ForeColor = System.Drawing.Color.White;
			this.buttonOK.IsBackStageButton = false;
			this.buttonOK.Location = new System.Drawing.Point(3, 3);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(75, 23);
			this.buttonOK.TabIndex = 10;
			this.buttonOK.Text = "xxOk";
			this.buttonOK.UseVisualStyle = true;
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// buttonCancel
			// 
			this.buttonCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonCancel.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.buttonCancel.BorderStyleAdv = Syncfusion.Windows.Forms.ButtonAdvBorderStyle.None;
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonCancel.ForeColor = System.Drawing.Color.White;
			this.buttonCancel.IsBackStageButton = false;
			this.buttonCancel.Location = new System.Drawing.Point(90, 3);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonCancel.TabIndex = 10;
			this.buttonCancel.Text = "xxCancel";
			this.buttonCancel.UseVisualStyle = true;
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// tabControlTopLevel
			// 
			this.tabControlTopLevel.Controls.Add(this.tabPageGeneral);
			this.tabControlTopLevel.Controls.Add(this.tabPageDaysOff);
			this.tabControlTopLevel.Controls.Add(this.tabPageExtra);
			this.tabControlTopLevel.Controls.Add(this.tabPageShifts);
			this.tabControlTopLevel.Controls.Add(this.tabPageAdvanced);
			this.tabControlTopLevel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControlTopLevel.ItemSize = new System.Drawing.Size(111, 22);
			this.tabControlTopLevel.Location = new System.Drawing.Point(3, 3);
			this.tabControlTopLevel.Name = "tabControlTopLevel";
			this.tabControlTopLevel.SelectedIndex = 0;
			this.tabControlTopLevel.Size = new System.Drawing.Size(440, 484);
			this.tabControlTopLevel.TabIndex = 6;
			this.tabControlTopLevel.SelectedIndexChanged += new System.EventHandler(this.tabControlTopLevel_SelectedIndexChanged);
			// 
			// tabPageGeneral
			// 
			this.tabPageGeneral.Controls.Add(this.generalPreferencesPanel1);
			this.tabPageGeneral.Location = new System.Drawing.Point(4, 26);
			this.tabPageGeneral.Name = "tabPageGeneral";
			this.tabPageGeneral.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageGeneral.Size = new System.Drawing.Size(432, 454);
			this.tabPageGeneral.TabIndex = 0;
			this.tabPageGeneral.Text = "xxGeneral";
			this.tabPageGeneral.UseVisualStyleBackColor = true;
			// 
			// generalPreferencesPanel1
			// 
			this.generalPreferencesPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.generalPreferencesPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.generalPreferencesPanel1.Location = new System.Drawing.Point(3, 3);
			this.generalPreferencesPanel1.Name = "generalPreferencesPanel1";
			this.generalPreferencesPanel1.Size = new System.Drawing.Size(426, 448);
			this.generalPreferencesPanel1.TabIndex = 0;
			// 
			// tabPageDaysOff
			// 
			this.tabPageDaysOff.Controls.Add(this.dayOffPreferencesPanel1);
			this.tabPageDaysOff.ImageKey = "on";
			this.tabPageDaysOff.Location = new System.Drawing.Point(4, 26);
			this.tabPageDaysOff.Name = "tabPageDaysOff";
			this.tabPageDaysOff.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageDaysOff.Size = new System.Drawing.Size(432, 454);
			this.tabPageDaysOff.TabIndex = 1;
			this.tabPageDaysOff.Text = "xxDaysOff";
			this.tabPageDaysOff.UseVisualStyleBackColor = true;
			// 
			// dayOffPreferencesPanel1
			// 
			this.dayOffPreferencesPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dayOffPreferencesPanel1.Location = new System.Drawing.Point(3, 3);
			this.dayOffPreferencesPanel1.Name = "dayOffPreferencesPanel1";
			this.dayOffPreferencesPanel1.Size = new System.Drawing.Size(426, 448);
			this.dayOffPreferencesPanel1.TabIndex = 0;
			// 
			// tabPageExtra
			// 
			this.tabPageExtra.Controls.Add(this.extraPreferencesPanel1);
			this.tabPageExtra.Location = new System.Drawing.Point(4, 26);
			this.tabPageExtra.Name = "tabPageExtra";
			this.tabPageExtra.Size = new System.Drawing.Size(432, 454);
			this.tabPageExtra.TabIndex = 2;
			this.tabPageExtra.Text = "xxExtra";
			this.tabPageExtra.UseVisualStyleBackColor = true;
			// 
			// extraPreferencesPanel1
			// 
			this.extraPreferencesPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.extraPreferencesPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.extraPreferencesPanel1.Location = new System.Drawing.Point(0, 0);
			this.extraPreferencesPanel1.Name = "extraPreferencesPanel1";
			this.extraPreferencesPanel1.Size = new System.Drawing.Size(432, 454);
			this.extraPreferencesPanel1.TabIndex = 0;
			// 
			// tabPageShifts
			// 
			this.tabPageShifts.Controls.Add(this.shiftsPreferencesPanel1);
			this.tabPageShifts.Location = new System.Drawing.Point(4, 26);
			this.tabPageShifts.Name = "tabPageShifts";
			this.tabPageShifts.Size = new System.Drawing.Size(432, 454);
			this.tabPageShifts.TabIndex = 4;
			this.tabPageShifts.Text = "xxShifts";
			this.tabPageShifts.UseVisualStyleBackColor = true;
			// 
			// shiftsPreferencesPanel1
			// 
			this.shiftsPreferencesPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.shiftsPreferencesPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.shiftsPreferencesPanel1.Location = new System.Drawing.Point(0, 0);
			this.shiftsPreferencesPanel1.Name = "shiftsPreferencesPanel1";
			this.shiftsPreferencesPanel1.Size = new System.Drawing.Size(432, 454);
			this.shiftsPreferencesPanel1.TabIndex = 0;
			// 
			// tabPageAdvanced
			// 
			this.tabPageAdvanced.Controls.Add(this.advancedPreferencesPanel1);
			this.tabPageAdvanced.Location = new System.Drawing.Point(4, 26);
			this.tabPageAdvanced.Name = "tabPageAdvanced";
			this.tabPageAdvanced.Size = new System.Drawing.Size(432, 454);
			this.tabPageAdvanced.TabIndex = 3;
			this.tabPageAdvanced.Text = "xxAdvanced";
			this.tabPageAdvanced.UseVisualStyleBackColor = true;
			// 
			// advancedPreferencesPanel1
			// 
			this.advancedPreferencesPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.advancedPreferencesPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.advancedPreferencesPanel1.Location = new System.Drawing.Point(0, 0);
			this.advancedPreferencesPanel1.Name = "advancedPreferencesPanel1";
			this.advancedPreferencesPanel1.Size = new System.Drawing.Size(432, 454);
			this.advancedPreferencesPanel1.TabIndex = 0;
			// 
			// OptimizationPreferencesDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(458, 570);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Controls.Add(this.ribbonControHeader);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "OptimizationPreferencesDialog";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "xxSchedulingSessionOptions";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OptimizationPreferencesDialog_FormClosing);
			this.Load += new System.EventHandler(this.Form_Load);
			((System.ComponentModel.ISupportInitialize)(this.ribbonControHeader)).EndInit();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tabControlTopLevel.ResumeLayout(false);
			this.tabPageGeneral.ResumeLayout(false);
			this.tabPageDaysOff.ResumeLayout(false);
			this.tabPageExtra.ResumeLayout(false);
			this.tabPageShifts.ResumeLayout(false);
			this.tabPageAdvanced.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed ribbonControHeader;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private Syncfusion.Windows.Forms.ButtonAdv buttonOK;
        private Syncfusion.Windows.Forms.ButtonAdv buttonCancel;
        private System.Windows.Forms.TabControl tabControlTopLevel;
        private System.Windows.Forms.TabPage tabPageDaysOff;
        private System.Windows.Forms.TabPage tabPageGeneral;
        private System.Windows.Forms.TabPage tabPageExtra;
        private System.Windows.Forms.TabPage tabPageAdvanced;
        private GeneralPreferencesPanel generalPreferencesPanel1;
        private DayOffPreferencesPanel dayOffPreferencesPanel1;
        private ExtraPreferencesPanel  extraPreferencesPanel1;
        private AdvancedPreferencesPanel advancedPreferencesPanel1;
        private System.Windows.Forms.TabPage tabPageShifts;
        private ShiftsPreferencesPanel shiftsPreferencesPanel1;
    }
}