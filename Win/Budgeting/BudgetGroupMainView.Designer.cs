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
			this.ribbonControlAdvFixed1 = new Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed();
			this.toolStripTabItem2 = new Syncfusion.Windows.Forms.Tools.ToolStripTabItem();
			this.toolStripEx1 = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripExLoad = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripButtonLoadForecastedHours = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonStaffEmployed = new System.Windows.Forms.ToolStripButton();
			this.toolStripExViews = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripButtonMonthView = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonWeekView = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonDayView = new System.Windows.Forms.ToolStripButton();
			this.toolStripTabItem3 = new Syncfusion.Windows.Forms.Tools.ToolStripTabItem();
			this.toolStripEx2 = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripButtonSave = new System.Windows.Forms.ToolStripButton();
			this.gradientPanelMain = new Syncfusion.Windows.Forms.Tools.GradientPanel();
			this.statusStripEx1 = new Syncfusion.Windows.Forms.Tools.StatusStripEx();
			this.toolStripStatusLabelBudgetGroupMainView = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripSpinningProgressControlStatus = new Teleopti.Ccc.Win.Common.Controls.SpinningProgress.ToolStripSpinningProgressControl();
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdvFixed1)).BeginInit();
			this.ribbonControlAdvFixed1.SuspendLayout();
			this.toolStripTabItem2.Panel.SuspendLayout();
			this.toolStripExLoad.SuspendLayout();
			this.toolStripExViews.SuspendLayout();
			this.toolStripTabItem3.Panel.SuspendLayout();
			this.toolStripEx2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanelMain)).BeginInit();
			this.statusStripEx1.SuspendLayout();
			this.SuspendLayout();
			// 
			// ribbonControlAdvFixed1
			// 
			this.ribbonControlAdvFixed1.CaptionFont = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ribbonControlAdvFixed1.Header.AddMainItem(toolStripTabItem2);
			this.ribbonControlAdvFixed1.Header.AddMainItem(toolStripTabItem3);
			this.ribbonControlAdvFixed1.Header.AddQuickItem(new Syncfusion.Windows.Forms.Tools.QuickButtonReflectable(toolStripButtonSave));
			this.ribbonControlAdvFixed1.HideMenuButtonToolTip = false;
			resources.ApplyResources(this.ribbonControlAdvFixed1, "ribbonControlAdvFixed1");
			this.ribbonControlAdvFixed1.MaximizeToolTip = "Maximize Ribbon";
			this.ribbonControlAdvFixed1.MenuButtonEnabled = true;
			this.ribbonControlAdvFixed1.MenuButtonFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ribbonControlAdvFixed1.MenuButtonText = "";
			this.ribbonControlAdvFixed1.MenuButtonWidth = 56;
			this.ribbonControlAdvFixed1.MenuColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(114)))), ((int)(((byte)(198)))));
			this.ribbonControlAdvFixed1.MinimizeToolTip = "Minimize Ribbon";
			this.ribbonControlAdvFixed1.Name = "ribbonControlAdvFixed1";
			this.ribbonControlAdvFixed1.Office2013ColorScheme = Syncfusion.Windows.Forms.Tools.Office2013ColorScheme.White;
			this.ribbonControlAdvFixed1.OfficeColorScheme = Syncfusion.Windows.Forms.Tools.ToolStripEx.ColorScheme.Silver;
			// 
			// ribbonControlAdvFixed1.OfficeMenu
			// 
			this.ribbonControlAdvFixed1.OfficeMenu.Name = "OfficeMenu";
			this.ribbonControlAdvFixed1.OfficeMenu.ShowItemToolTips = true;
			resources.ApplyResources(this.ribbonControlAdvFixed1.OfficeMenu, "ribbonControlAdvFixed1.OfficeMenu");
			this.ribbonControlAdvFixed1.OverFlowButtonToolTip = "Show DropDown";
			this.ribbonControlAdvFixed1.QuickPanelImageLayout = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.ribbonControlAdvFixed1.RibbonHeaderImage = Syncfusion.Windows.Forms.Tools.RibbonHeaderImage.None;
			this.ribbonControlAdvFixed1.RibbonStyle = Syncfusion.Windows.Forms.Tools.RibbonStyle.Office2013;
			this.ribbonControlAdvFixed1.SelectedTab = this.toolStripTabItem2;
			this.ribbonControlAdvFixed1.Show2010CustomizeQuickItemDialog = false;
			this.ribbonControlAdvFixed1.ShowRibbonDisplayOptionButton = false;
			this.ribbonControlAdvFixed1.SystemText.QuickAccessDialogDropDownName = "Start menu";
			this.ribbonControlAdvFixed1.TitleAlignment = Syncfusion.Windows.Forms.Tools.TextAlignment.Center;
			this.ribbonControlAdvFixed1.TitleColor = System.Drawing.Color.Black;
			// 
			// toolStripTabItem2
			// 
			this.ribbonControlAdvFixed1.SetDescription(this.toolStripTabItem2, "");
			this.toolStripTabItem2.Name = "toolStripTabItem2";
			// 
			// ribbonControlAdvFixed1.ribbonPanel1
			// 
			this.toolStripTabItem2.Panel.Controls.Add(this.toolStripEx1);
			this.toolStripTabItem2.Panel.Controls.Add(this.toolStripExLoad);
			this.toolStripTabItem2.Panel.Controls.Add(this.toolStripExViews);
			this.toolStripTabItem2.Panel.Name = "ribbonPanel1";
			resources.ApplyResources(this.toolStripTabItem2.Panel, "ribbonControlAdvFixed1.ribbonPanel1");
			this.toolStripTabItem2.Panel.ScrollPosition = 0;
			this.toolStripTabItem2.Position = 0;
			this.SetShortcut(this.toolStripTabItem2, System.Windows.Forms.Keys.None);
			resources.ApplyResources(this.toolStripTabItem2, "toolStripTabItem2");
			this.toolStripTabItem2.Tag = "2";
			this.ribbonControlAdvFixed1.SetUseInCustomQuickAccessDialog(this.toolStripTabItem2, true);
			this.ribbonControlAdvFixed1.SetUseInQuickAccessMenu(this.toolStripTabItem2, false);
			// 
			// toolStripEx1
			// 
			resources.ApplyResources(this.toolStripEx1, "toolStripEx1");
			this.ribbonControlAdvFixed1.SetDescription(this.toolStripEx1, "");
			this.toolStripEx1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(59)))), ((int)(((byte)(59)))));
			this.toolStripEx1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripEx1.Image = null;
			this.toolStripEx1.ImageScalingSize = new System.Drawing.Size(25, 25);
			this.toolStripEx1.Name = "toolStripEx1";
			this.toolStripEx1.Office12Mode = false;
			this.toolStripEx1.ShowLauncher = false;
			this.ribbonControlAdvFixed1.SetUseInCustomQuickAccessDialog(this.toolStripEx1, false);
			this.ribbonControlAdvFixed1.SetUseInQuickAccessMenu(this.toolStripEx1, false);
			// 
			// toolStripExLoad
			// 
			this.toolStripExLoad.CanOverflow = false;
			this.ribbonControlAdvFixed1.SetDescription(this.toolStripExLoad, "");
			resources.ApplyResources(this.toolStripExLoad, "toolStripExLoad");
			this.toolStripExLoad.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(59)))), ((int)(((byte)(59)))));
			this.toolStripExLoad.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripExLoad.Image = null;
			this.toolStripExLoad.ImageScalingSize = new System.Drawing.Size(25, 25);
			this.toolStripExLoad.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonLoadForecastedHours,
            this.toolStripButtonStaffEmployed});
			this.toolStripExLoad.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
			this.toolStripExLoad.Name = "toolStripExLoad";
			this.toolStripExLoad.Office12Mode = false;
			this.toolStripExLoad.ShowLauncher = false;
			this.ribbonControlAdvFixed1.SetUseInCustomQuickAccessDialog(this.toolStripExLoad, false);
			this.ribbonControlAdvFixed1.SetUseInQuickAccessMenu(this.toolStripExLoad, false);
			// 
			// toolStripButtonLoadForecastedHours
			// 
			this.ribbonControlAdvFixed1.SetDescription(this.toolStripButtonLoadForecastedHours, "");
			this.toolStripButtonLoadForecastedHours.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Forecasts;
			resources.ApplyResources(this.toolStripButtonLoadForecastedHours, "toolStripButtonLoadForecastedHours");
			this.toolStripButtonLoadForecastedHours.Name = "toolStripButtonLoadForecastedHours";
			this.toolStripButtonLoadForecastedHours.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
			this.SetShortcut(this.toolStripButtonLoadForecastedHours, System.Windows.Forms.Keys.None);
			this.ribbonControlAdvFixed1.SetUseInCustomQuickAccessDialog(this.toolStripButtonLoadForecastedHours, true);
			this.ribbonControlAdvFixed1.SetUseInQuickAccessMenu(this.toolStripButtonLoadForecastedHours, false);
			this.toolStripButtonLoadForecastedHours.Click += new System.EventHandler(this.toolStripButtonLoadForecastedHours_Click);
			// 
			// toolStripButtonStaffEmployed
			// 
			this.ribbonControlAdvFixed1.SetDescription(this.toolStripButtonStaffEmployed, "");
			resources.ApplyResources(this.toolStripButtonStaffEmployed, "toolStripButtonStaffEmployed");
			this.toolStripButtonStaffEmployed.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_People;
			this.toolStripButtonStaffEmployed.Name = "toolStripButtonStaffEmployed";
			this.toolStripButtonStaffEmployed.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
			this.SetShortcut(this.toolStripButtonStaffEmployed, System.Windows.Forms.Keys.None);
			this.ribbonControlAdvFixed1.SetUseInCustomQuickAccessDialog(this.toolStripButtonStaffEmployed, true);
			this.ribbonControlAdvFixed1.SetUseInQuickAccessMenu(this.toolStripButtonStaffEmployed, false);
			this.toolStripButtonStaffEmployed.Click += new System.EventHandler(this.toolStripButtonLoadStaffEmployed_Click);
			// 
			// toolStripExViews
			// 
			this.ribbonControlAdvFixed1.SetDescription(this.toolStripExViews, "");
			resources.ApplyResources(this.toolStripExViews, "toolStripExViews");
			this.toolStripExViews.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(59)))), ((int)(((byte)(59)))));
			this.toolStripExViews.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripExViews.Image = null;
			this.toolStripExViews.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonMonthView,
            this.toolStripButtonWeekView,
            this.toolStripButtonDayView});
			this.toolStripExViews.LauncherStyle = Syncfusion.Windows.Forms.Tools.LauncherStyle.Office2007;
			this.toolStripExViews.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
			this.toolStripExViews.Name = "toolStripExViews";
			this.toolStripExViews.Office12Mode = false;
			this.toolStripExViews.ShowLauncher = false;
			this.ribbonControlAdvFixed1.SetUseInCustomQuickAccessDialog(this.toolStripExViews, false);
			this.ribbonControlAdvFixed1.SetUseInQuickAccessMenu(this.toolStripExViews, false);
			// 
			// toolStripButtonMonthView
			// 
			this.ribbonControlAdvFixed1.SetDescription(this.toolStripButtonMonthView, "");
			this.toolStripButtonMonthView.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_WorkloadGridMonth;
			resources.ApplyResources(this.toolStripButtonMonthView, "toolStripButtonMonthView");
			this.toolStripButtonMonthView.Name = "toolStripButtonMonthView";
			this.SetShortcut(this.toolStripButtonMonthView, System.Windows.Forms.Keys.None);
			this.ribbonControlAdvFixed1.SetUseInCustomQuickAccessDialog(this.toolStripButtonMonthView, true);
			this.ribbonControlAdvFixed1.SetUseInQuickAccessMenu(this.toolStripButtonMonthView, false);
			this.toolStripButtonMonthView.Click += new System.EventHandler(this.toolStripButtonMonthView_Click);
			// 
			// toolStripButtonWeekView
			// 
			this.ribbonControlAdvFixed1.SetDescription(this.toolStripButtonWeekView, "");
			this.toolStripButtonWeekView.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_WorkloadGridWeek;
			resources.ApplyResources(this.toolStripButtonWeekView, "toolStripButtonWeekView");
			this.toolStripButtonWeekView.Name = "toolStripButtonWeekView";
			this.SetShortcut(this.toolStripButtonWeekView, System.Windows.Forms.Keys.None);
			this.ribbonControlAdvFixed1.SetUseInCustomQuickAccessDialog(this.toolStripButtonWeekView, true);
			this.ribbonControlAdvFixed1.SetUseInQuickAccessMenu(this.toolStripButtonWeekView, false);
			this.toolStripButtonWeekView.Click += new System.EventHandler(this.toolStripButtonWeekView_Click);
			// 
			// toolStripButtonDayView
			// 
			this.ribbonControlAdvFixed1.SetDescription(this.toolStripButtonDayView, "");
			this.toolStripButtonDayView.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_WorkloadGridDay;
			resources.ApplyResources(this.toolStripButtonDayView, "toolStripButtonDayView");
			this.toolStripButtonDayView.Name = "toolStripButtonDayView";
			this.SetShortcut(this.toolStripButtonDayView, System.Windows.Forms.Keys.None);
			this.ribbonControlAdvFixed1.SetUseInCustomQuickAccessDialog(this.toolStripButtonDayView, true);
			this.ribbonControlAdvFixed1.SetUseInQuickAccessMenu(this.toolStripButtonDayView, false);
			this.toolStripButtonDayView.Click += new System.EventHandler(this.toolStripButtonDayView_Click);
			// 
			// toolStripTabItem3
			// 
			this.ribbonControlAdvFixed1.SetDescription(this.toolStripTabItem3, "");
			resources.ApplyResources(this.toolStripTabItem3, "toolStripTabItem3");
			this.toolStripTabItem3.Name = "toolStripTabItem3";
			// 
			// ribbonControlAdvFixed1.ribbonPanel2
			// 
			this.toolStripTabItem3.Panel.Controls.Add(this.toolStripEx2);
			this.toolStripTabItem3.Panel.Name = "ribbonPanel2";
			this.toolStripTabItem3.Panel.ScrollPosition = 0;
			resources.ApplyResources(this.toolStripTabItem3.Panel, "ribbonControlAdvFixed1.ribbonPanel2");
			this.toolStripTabItem3.Position = 1;
			this.SetShortcut(this.toolStripTabItem3, System.Windows.Forms.Keys.None);
			this.toolStripTabItem3.Tag = "3";
			this.ribbonControlAdvFixed1.SetUseInCustomQuickAccessDialog(this.toolStripTabItem3, true);
			this.ribbonControlAdvFixed1.SetUseInQuickAccessMenu(this.toolStripTabItem3, false);
			// 
			// toolStripEx2
			// 
			resources.ApplyResources(this.toolStripEx2, "toolStripEx2");
			this.ribbonControlAdvFixed1.SetDescription(this.toolStripEx2, "");
			this.toolStripEx2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(59)))), ((int)(((byte)(59)))));
			this.toolStripEx2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripEx2.Image = null;
			this.toolStripEx2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonSave});
			this.toolStripEx2.Name = "toolStripEx2";
			this.toolStripEx2.Office12Mode = false;
			this.ribbonControlAdvFixed1.SetUseInCustomQuickAccessDialog(this.toolStripEx2, true);
			this.ribbonControlAdvFixed1.SetUseInQuickAccessMenu(this.toolStripEx2, false);
			// 
			// toolStripButtonSave
			// 
			resources.ApplyResources(this.toolStripButtonSave, "toolStripButtonSave");
			this.toolStripButtonSave.AutoToolTip = false;
			this.ribbonControlAdvFixed1.SetDescription(this.toolStripButtonSave, "");
			this.toolStripButtonSave.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Save;
			this.toolStripButtonSave.Name = "toolStripButtonSave";
			this.SetShortcut(this.toolStripButtonSave, System.Windows.Forms.Keys.None);
			this.ribbonControlAdvFixed1.SetUseInCustomQuickAccessDialog(this.toolStripButtonSave, true);
			this.ribbonControlAdvFixed1.SetUseInQuickAccessMenu(this.toolStripButtonSave, false);
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
			this.statusStripEx1.BeforeTouchSize = new System.Drawing.Size(1212, 22);
			this.statusStripEx1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabelBudgetGroupMainView,
            this.toolStripSpinningProgressControlStatus});
			resources.ApplyResources(this.statusStripEx1, "statusStripEx1");
			this.statusStripEx1.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(135)))), ((int)(((byte)(206)))), ((int)(((byte)(255)))));
			this.statusStripEx1.Name = "statusStripEx1";
			this.statusStripEx1.VisualStyle = Syncfusion.Windows.Forms.Tools.StatusStripExStyle.Metro;
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
			this.toolStripSpinningProgressControlStatus.BehindTransitionSegmentIsActive = true;
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
			this.BackColor = System.Drawing.Color.White;
			this.Borders = new System.Windows.Forms.Padding(3, 1, 3, 3);
			this.ColorScheme = Syncfusion.Windows.Forms.Tools.RibbonForm.ColorSchemeType.Silver;
			this.Controls.Add(this.ribbonControlAdvFixed1);
			this.Controls.Add(this.statusStripEx1);
			this.Controls.Add(this.gradientPanelMain);
			this.Name = "BudgetGroupMainView";
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdvFixed1)).EndInit();
			this.ribbonControlAdvFixed1.ResumeLayout(false);
			this.ribbonControlAdvFixed1.PerformLayout();
			this.toolStripTabItem2.Panel.ResumeLayout(false);
			this.toolStripTabItem2.Panel.PerformLayout();
			this.toolStripExLoad.ResumeLayout(false);
			this.toolStripExLoad.PerformLayout();
			this.toolStripExViews.ResumeLayout(false);
			this.toolStripExViews.PerformLayout();
			this.toolStripTabItem3.Panel.ResumeLayout(false);
			this.toolStripEx2.ResumeLayout(false);
			this.toolStripEx2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanelMain)).EndInit();
			this.statusStripEx1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

		private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanelMain;
		private Syncfusion.Windows.Forms.Tools.StatusStripEx statusStripEx1;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelBudgetGroupMainView;
		private Common.Controls.SpinningProgress.ToolStripSpinningProgressControl toolStripSpinningProgressControlStatus;
		private Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed ribbonControlAdvFixed1;
		private Syncfusion.Windows.Forms.Tools.ToolStripTabItem toolStripTabItem2;
		private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripEx1;
		private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExLoad;
		private ToolStripButton toolStripButtonLoadForecastedHours;
		private ToolStripButton toolStripButtonStaffEmployed;
		private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExViews;
		private ToolStripButton toolStripButtonMonthView;
		private ToolStripButton toolStripButtonWeekView;
		private ToolStripButton toolStripButtonDayView;
		private Syncfusion.Windows.Forms.Tools.ToolStripTabItem toolStripTabItem3;
		private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripEx2;
		private ToolStripButton toolStripButtonSave;
    }
}