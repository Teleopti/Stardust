using System.Windows.Forms;
using Teleopti.Ccc.WinCode.Budgeting.Presenters;

namespace Teleopti.Ccc.Win.Budgeting
{
    partial class BudgetGroupMainView
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
            if (disposing)
            {
                if (components != null)
                    components.Dispose();

            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void InitializeComponent()
        {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BudgetGroupMainView));
			this.ribbonControlAdv1 = new Syncfusion.Windows.Forms.Tools.RibbonControlAdv();
			this.toolStripTabItem1 = new Syncfusion.Windows.Forms.Tools.ToolStripTabItem();
			this.toolStripEx1 = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripExLoad = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripButtonLoadForecastedHours = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonStaffEmployed = new System.Windows.Forms.ToolStripButton();
			this.toolStripExViews = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripButtonMonthView = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonWeekView = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonDayView = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonSave = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripButtonHelp = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripButtonClose = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonExit = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonOptions = new System.Windows.Forms.ToolStripButton();
			this.gradientPanelMain = new Syncfusion.Windows.Forms.Tools.GradientPanel();
			this.statusStripEx1 = new Syncfusion.Windows.Forms.Tools.StatusStripEx();
			this.toolStripStatusLabelBudgetGroupMainView = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripSpinningProgressControlStatus = new Teleopti.Ccc.Win.Common.Controls.SpinningProgress.ToolStripSpinningProgressControl();
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).BeginInit();
			this.ribbonControlAdv1.SuspendLayout();
			this.toolStripTabItem1.Panel.SuspendLayout();
			this.toolStripExLoad.SuspendLayout();
			this.toolStripExViews.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanelMain)).BeginInit();
			this.statusStripEx1.SuspendLayout();
			this.SuspendLayout();
			// 
			// ribbonControlAdv1
			// 
			this.ribbonControlAdv1.Header.AddMainItem(toolStripTabItem1);
			this.ribbonControlAdv1.Header.AddQuickItem(new Syncfusion.Windows.Forms.Tools.QuickButtonReflectable(toolStripButtonSave));
			resources.ApplyResources(this.ribbonControlAdv1, "ribbonControlAdv1");
			this.ribbonControlAdv1.MenuButtonImage = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Menu;
			this.ribbonControlAdv1.Name = "ribbonControlAdv1";
			// 
			// ribbonControlAdv1.OfficeMenu
			// 
			this.ribbonControlAdv1.OfficeMenu.AuxPanel.MinimumSize = new System.Drawing.Size(150, 0);
			this.ribbonControlAdv1.OfficeMenu.MainPanel.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonSave,
            this.toolStripSeparator1,
            this.toolStripButtonHelp,
            this.toolStripSeparator2,
            this.toolStripButtonClose});
			this.ribbonControlAdv1.OfficeMenu.Name = "OfficeMenu";
			resources.ApplyResources(this.ribbonControlAdv1.OfficeMenu, "ribbonControlAdv1.OfficeMenu");
			this.ribbonControlAdv1.OfficeMenu.SystemPanel.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonExit,
            this.toolStripButtonOptions});
			this.ribbonControlAdv1.SelectedTab = this.toolStripTabItem1;
			this.ribbonControlAdv1.SystemText.QuickAccessDialogDropDownName = "Start menu";
			// 
			// toolStripTabItem1
			// 
			this.toolStripTabItem1.Name = "toolStripTabItem1";
			// 
			// ribbonControlAdv1.ribbonPanel1
			// 
			this.toolStripTabItem1.Panel.Controls.Add(this.toolStripEx1);
			this.toolStripTabItem1.Panel.Controls.Add(this.toolStripExLoad);
			this.toolStripTabItem1.Panel.Controls.Add(this.toolStripExViews);
			this.toolStripTabItem1.Panel.Name = "ribbonPanel1";
			this.toolStripTabItem1.Panel.ScrollPosition = 0;
			resources.ApplyResources(this.toolStripTabItem1.Panel, "ribbonControlAdv1.ribbonPanel1");
			this.toolStripTabItem1.Position = 0;
			this.SetShortcut(this.toolStripTabItem1, System.Windows.Forms.Keys.None);
			resources.ApplyResources(this.toolStripTabItem1, "toolStripTabItem1");
			// 
			// toolStripEx1
			// 
			resources.ApplyResources(this.toolStripEx1, "toolStripEx1");
			this.toolStripEx1.ForeColor = System.Drawing.Color.MidnightBlue;
			this.toolStripEx1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripEx1.Image = null;
			this.toolStripEx1.ImageScalingSize = new System.Drawing.Size(25, 25);
			this.toolStripEx1.Name = "toolStripEx1";
			this.toolStripEx1.ShowLauncher = false;
			// 
			// toolStripExLoad
			// 
			this.toolStripExLoad.CanOverflow = false;
			resources.ApplyResources(this.toolStripExLoad, "toolStripExLoad");
			this.toolStripExLoad.ForeColor = System.Drawing.Color.MidnightBlue;
			this.toolStripExLoad.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripExLoad.Image = null;
			this.toolStripExLoad.ImageScalingSize = new System.Drawing.Size(25, 25);
			this.toolStripExLoad.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonLoadForecastedHours,
            this.toolStripButtonStaffEmployed});
			this.toolStripExLoad.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
			this.toolStripExLoad.Name = "toolStripExLoad";
			this.toolStripExLoad.ShowLauncher = false;
			// 
			// toolStripButtonLoadForecastedHours
			// 
			this.toolStripButtonLoadForecastedHours.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Forecasts;
			resources.ApplyResources(this.toolStripButtonLoadForecastedHours, "toolStripButtonLoadForecastedHours");
			this.toolStripButtonLoadForecastedHours.Name = "toolStripButtonLoadForecastedHours";
			this.toolStripButtonLoadForecastedHours.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
			this.SetShortcut(this.toolStripButtonLoadForecastedHours, System.Windows.Forms.Keys.None);
			this.toolStripButtonLoadForecastedHours.Click += new System.EventHandler(this.toolStripButtonLoadForecastedHours_Click);
			// 
			// toolStripButtonStaffEmployed
			// 
			this.toolStripButtonStaffEmployed.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_People;
			resources.ApplyResources(this.toolStripButtonStaffEmployed, "toolStripButtonStaffEmployed");
			this.toolStripButtonStaffEmployed.Name = "toolStripButtonStaffEmployed";
			this.toolStripButtonStaffEmployed.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
			this.SetShortcut(this.toolStripButtonStaffEmployed, System.Windows.Forms.Keys.None);
			this.toolStripButtonStaffEmployed.Click += new System.EventHandler(this.toolStripButtonLoadStaffEmployed_Click);
			// 
			// toolStripExViews
			// 
			resources.ApplyResources(this.toolStripExViews, "toolStripExViews");
			this.toolStripExViews.ForeColor = System.Drawing.Color.MidnightBlue;
			this.toolStripExViews.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripExViews.Image = null;
			this.toolStripExViews.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonMonthView,
            this.toolStripButtonWeekView,
            this.toolStripButtonDayView});
			this.toolStripExViews.LauncherStyle = Syncfusion.Windows.Forms.Tools.LauncherStyle.Office2007;
			this.toolStripExViews.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
			this.toolStripExViews.Name = "toolStripExViews";
			this.toolStripExViews.ShowLauncher = false;
			// 
			// toolStripButtonMonthView
			// 
			this.toolStripButtonMonthView.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_WorkloadGridMonth;
			resources.ApplyResources(this.toolStripButtonMonthView, "toolStripButtonMonthView");
			this.toolStripButtonMonthView.Name = "toolStripButtonMonthView";
			this.SetShortcut(this.toolStripButtonMonthView, System.Windows.Forms.Keys.None);
			this.toolStripButtonMonthView.Click += new System.EventHandler(this.toolStripButtonMonthView_Click);
			// 
			// toolStripButtonWeekView
			// 
			this.toolStripButtonWeekView.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_WorkloadGridWeek;
			resources.ApplyResources(this.toolStripButtonWeekView, "toolStripButtonWeekView");
			this.toolStripButtonWeekView.Name = "toolStripButtonWeekView";
			this.SetShortcut(this.toolStripButtonWeekView, System.Windows.Forms.Keys.None);
			this.toolStripButtonWeekView.Click += new System.EventHandler(this.toolStripButtonWeekView_Click);
			// 
			// toolStripButtonDayView
			// 
			this.toolStripButtonDayView.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_WorkloadGridDay;
			resources.ApplyResources(this.toolStripButtonDayView, "toolStripButtonDayView");
			this.toolStripButtonDayView.Name = "toolStripButtonDayView";
			this.SetShortcut(this.toolStripButtonDayView, System.Windows.Forms.Keys.None);
			this.toolStripButtonDayView.Click += new System.EventHandler(this.toolStripButtonDayView_Click);
			// 
			// toolStripButtonSave
			// 
			resources.ApplyResources(this.toolStripButtonSave, "toolStripButtonSave");
			this.toolStripButtonSave.AutoToolTip = false;
			this.toolStripButtonSave.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Save;
			this.toolStripButtonSave.Name = "toolStripButtonSave";
			this.SetShortcut(this.toolStripButtonSave, System.Windows.Forms.Keys.None);
			this.toolStripButtonSave.Click += new System.EventHandler(this.btnSave_click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.SetShortcut(this.toolStripSeparator1, System.Windows.Forms.Keys.None);
			resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
			// 
			// toolStripButtonHelp
			// 
			this.toolStripButtonHelp.Image = global::Teleopti.Ccc.Win.Properties.Resources.help_32;
			resources.ApplyResources(this.toolStripButtonHelp, "toolStripButtonHelp");
			this.toolStripButtonHelp.Name = "toolStripButtonHelp";
			this.SetShortcut(this.toolStripButtonHelp, System.Windows.Forms.Keys.None);
			this.toolStripButtonHelp.Click += new System.EventHandler(this.toolStripButtonHelp_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.SetShortcut(this.toolStripSeparator2, System.Windows.Forms.Keys.None);
			resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
			// 
			// toolStripButtonClose
			// 
			this.toolStripButtonClose.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Close;
			resources.ApplyResources(this.toolStripButtonClose, "toolStripButtonClose");
			this.toolStripButtonClose.Name = "toolStripButtonClose";
			this.SetShortcut(this.toolStripButtonClose, System.Windows.Forms.Keys.None);
			this.toolStripButtonClose.Click += new System.EventHandler(this.toolStripButtonClose_Click);
			// 
			// toolStripButtonExit
			// 
			this.toolStripButtonExit.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Exit;
			resources.ApplyResources(this.toolStripButtonExit, "toolStripButtonExit");
			this.toolStripButtonExit.Name = "toolStripButtonExit";
			this.SetShortcut(this.toolStripButtonExit, System.Windows.Forms.Keys.None);
			this.toolStripButtonExit.Click += new System.EventHandler(this.toolStripButtonExit_Click);
			// 
			// toolStripButtonOptions
			// 
			this.toolStripButtonOptions.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Settings;
			resources.ApplyResources(this.toolStripButtonOptions, "toolStripButtonOptions");
			this.toolStripButtonOptions.Name = "toolStripButtonOptions";
			this.SetShortcut(this.toolStripButtonOptions, System.Windows.Forms.Keys.None);
			this.toolStripButtonOptions.Click += new System.EventHandler(this.toolStripButtonOptions_Click);
			// 
			// gradientPanelMain
			// 
			this.gradientPanelMain.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
			this.gradientPanelMain.BorderSingle = System.Windows.Forms.ButtonBorderStyle.None;
			this.gradientPanelMain.BorderStyle = System.Windows.Forms.BorderStyle.None;
			resources.ApplyResources(this.gradientPanelMain, "gradientPanelMain");
			this.gradientPanelMain.Name = "gradientPanelMain";
			// 
			// statusStripEx1
			// 
			this.statusStripEx1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabelBudgetGroupMainView,
            this.toolStripSpinningProgressControlStatus});
			resources.ApplyResources(this.statusStripEx1, "statusStripEx1");
			this.statusStripEx1.Name = "statusStripEx1";
			this.statusStripEx1.SizingGrip = false;
			// 
			// toolStripStatusLabelBudgetGroupMainView
			// 
			this.toolStripStatusLabelBudgetGroupMainView.BackColor = System.Drawing.Color.Transparent;
			this.toolStripStatusLabelBudgetGroupMainView.Name = "toolStripStatusLabelBudgetGroupMainView";
			this.SetShortcut(this.toolStripStatusLabelBudgetGroupMainView, System.Windows.Forms.Keys.None);
			resources.ApplyResources(this.toolStripStatusLabelBudgetGroupMainView, "toolStripStatusLabelBudgetGroupMainView");
			// 
			// toolStripSpinningProgressControlStatus
			// 
			this.toolStripSpinningProgressControlStatus.ActiveSegmentColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(146)))), ((int)(((byte)(33)))));
			this.toolStripSpinningProgressControlStatus.BackColor = System.Drawing.Color.Transparent;
			this.toolStripSpinningProgressControlStatus.BehindTransitionSegmentIsActive = false;
			this.toolStripSpinningProgressControlStatus.InactiveSegmentColor = System.Drawing.Color.FromArgb(((int)(((byte)(218)))), ((int)(((byte)(218)))), ((int)(((byte)(218)))));
			this.toolStripSpinningProgressControlStatus.Name = "ToolStripSpinningProgress";
			this.SetShortcut(this.toolStripSpinningProgressControlStatus, System.Windows.Forms.Keys.None);
			resources.ApplyResources(this.toolStripSpinningProgressControlStatus, "toolStripSpinningProgressControlStatus");
			this.toolStripSpinningProgressControlStatus.TransitionSegment = 1;
			this.toolStripSpinningProgressControlStatus.TransitionSegmentColor = System.Drawing.Color.FromArgb(((int)(((byte)(129)))), ((int)(((byte)(242)))), ((int)(((byte)(121)))));
			// 
			// BudgetGroupMainView
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.statusStripEx1);
			this.Controls.Add(this.gradientPanelMain);
			this.Controls.Add(this.ribbonControlAdv1);
			this.Name = "BudgetGroupMainView";
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).EndInit();
			this.ribbonControlAdv1.ResumeLayout(false);
			this.ribbonControlAdv1.PerformLayout();
			this.toolStripTabItem1.Panel.ResumeLayout(false);
			this.toolStripTabItem1.Panel.PerformLayout();
			this.toolStripExLoad.ResumeLayout(false);
			this.toolStripExLoad.PerformLayout();
			this.toolStripExViews.ResumeLayout(false);
			this.toolStripExViews.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanelMain)).EndInit();
			this.statusStripEx1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.RibbonControlAdv ribbonControlAdv1;
        private Syncfusion.Windows.Forms.Tools.ToolStripTabItem toolStripTabItem1;
        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanelMain;
		private System.Windows.Forms.ToolStripButton toolStripButtonSave;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripEx1;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExLoad;
		private System.Windows.Forms.ToolStripButton toolStripButtonLoadForecastedHours;
		private Syncfusion.Windows.Forms.Tools.StatusStripEx statusStripEx1;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelBudgetGroupMainView;
		private Common.Controls.SpinningProgress.ToolStripSpinningProgressControl toolStripSpinningProgressControlStatus;
		private System.Windows.Forms.ToolStripButton toolStripButtonStaffEmployed;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripButton toolStripButtonHelp;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripButton toolStripButtonClose;
		private System.Windows.Forms.ToolStripButton toolStripButtonExit;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExViews;
        private ToolStripButton toolStripButtonMonthView;
        private ToolStripButton toolStripButtonWeekView;
        private ToolStripButton toolStripButtonDayView;
        private ToolStripButton toolStripButtonOptions;
    }
}