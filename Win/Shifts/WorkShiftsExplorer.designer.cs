using System.Windows.Forms;

namespace Teleopti.Ccc.Win.Shifts
{
    partial class WorkShiftsExplorer
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WorkShiftsExplorer));
			Syncfusion.Windows.Forms.Tools.ToolStripTabGroup toolStripTabGroup1 = new Syncfusion.Windows.Forms.Tools.ToolStripTabGroup();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.toolStripButtonExitSystem = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonSystemOptions = new System.Windows.Forms.ToolStripButton();
			this.splitContainerAdvVertical = new Syncfusion.Windows.Forms.Tools.SplitContainerAdv();
			this.splitContainerAdvHorizontal = new Syncfusion.Windows.Forms.Tools.SplitContainerAdv();
			this.ribbonControlAdv1 = new Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed();
			this.toolStripTabHome = new Syncfusion.Windows.Forms.Tools.ToolStripTabItem();
			this.toolStripRefresh = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripButtonRefresh = new System.Windows.Forms.ToolStripButton();
			this.tsClipboard = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.tcEdit = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.tcShiftBags = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripButtonAddRuleSet = new System.Windows.Forms.ToolStripButton();
			this.tcRename = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripButtonRename = new System.Windows.Forms.ToolStripButton();
			this.tcViews = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripButtonGeneral = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonCombined = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonLimitations = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonDateExclusion = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonWeekdayExclusion = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonSave = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripButtonHelp = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripButtonCloseExit = new System.Windows.Forms.ToolStripButton();
			this.statusStripEx1 = new Syncfusion.Windows.Forms.Tools.StatusStripEx();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerAdvVertical)).BeginInit();
			this.splitContainerAdvVertical.Panel2.SuspendLayout();
			this.splitContainerAdvVertical.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerAdvHorizontal)).BeginInit();
			this.splitContainerAdvHorizontal.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).BeginInit();
			this.ribbonControlAdv1.SuspendLayout();
			this.toolStripTabHome.Panel.SuspendLayout();
			this.toolStripRefresh.SuspendLayout();
			this.tcShiftBags.SuspendLayout();
			this.tcRename.SuspendLayout();
			this.tcViews.SuspendLayout();
			this.SuspendLayout();
			// 
			// imageList1
			// 
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			this.imageList1.Images.SetKeyName(0, "ccc_ShiftRuleSet.png");
			this.imageList1.Images.SetKeyName(1, "ccc_ShiftBag.png");
			// 
			// toolStripButtonExitSystem
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonExitSystem, "");
			this.toolStripButtonExitSystem.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Exit;
			this.toolStripButtonExitSystem.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonExitSystem.Name = "toolStripButtonExitSystem";
			this.SetShortcut(this.toolStripButtonExitSystem, System.Windows.Forms.Keys.None);
			this.toolStripButtonExitSystem.Size = new System.Drawing.Size(130, 20);
			this.toolStripButtonExitSystem.Text = "xxExitTELEOPTICCC";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonExitSystem, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonExitSystem, false);
			this.toolStripButtonExitSystem.Click += new System.EventHandler(this.toolStripButtonExitSystemClick);
			// 
			// toolStripButtonSystemOptions
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonSystemOptions, "");
			this.toolStripButtonSystemOptions.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Settings;
			this.toolStripButtonSystemOptions.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonSystemOptions.Name = "toolStripButtonSystemOptions";
			this.SetShortcut(this.toolStripButtonSystemOptions, System.Windows.Forms.Keys.None);
			this.toolStripButtonSystemOptions.Size = new System.Drawing.Size(79, 20);
			this.toolStripButtonSystemOptions.Text = "xxOptions";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonSystemOptions, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonSystemOptions, false);
			this.toolStripButtonSystemOptions.Click += new System.EventHandler(this.toolStripButtonSystemOptionsClick);
			// 
			// splitContainerAdvVertical
			// 
			this.splitContainerAdvVertical.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerAdvVertical.FixedPanel = Syncfusion.Windows.Forms.Tools.Enums.FixedPanel.Panel1;
			this.splitContainerAdvVertical.Location = new System.Drawing.Point(6, 142);
			this.splitContainerAdvVertical.Name = "splitContainerAdvVertical";
			// 
			// splitContainerAdvVertical.Panel1
			// 
			this.splitContainerAdvVertical.Panel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252))))), System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255))))));
			// 
			// splitContainerAdvVertical.Panel2
			// 
			this.splitContainerAdvVertical.Panel2.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252))))), System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255))))));
			this.splitContainerAdvVertical.Panel2.Controls.Add(this.splitContainerAdvHorizontal);
			this.splitContainerAdvVertical.PanelToBeCollapsed = Syncfusion.Windows.Forms.Tools.Enums.CollapsedPanel.Panel1;
			this.splitContainerAdvVertical.Size = new System.Drawing.Size(1024, 483);
			this.splitContainerAdvVertical.SplitterDistance = 279;
			this.splitContainerAdvVertical.Style = Syncfusion.Windows.Forms.Tools.Enums.Style.Office2007Blue;
			this.splitContainerAdvVertical.TabIndex = 1;
			this.splitContainerAdvVertical.Text = "splitContainerAdv1";
			// 
			// splitContainerAdvHorizontal
			// 
			this.splitContainerAdvHorizontal.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Horizontal, System.Drawing.Color.FromArgb(((int)(((byte)(227)))), ((int)(((byte)(239)))), ((int)(((byte)(255))))), System.Drawing.Color.FromArgb(((int)(((byte)(101)))), ((int)(((byte)(147)))), ((int)(((byte)(207))))));
			this.splitContainerAdvHorizontal.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerAdvHorizontal.HotBackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Horizontal, System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(151)))), ((int)(((byte)(61))))), System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(184)))), ((int)(((byte)(94))))));
			this.splitContainerAdvHorizontal.Location = new System.Drawing.Point(0, 0);
			this.splitContainerAdvHorizontal.Name = "splitContainerAdvHorizontal";
			this.splitContainerAdvHorizontal.Orientation = System.Windows.Forms.Orientation.Vertical;
			// 
			// splitContainerAdvHorizontal.Panel1
			// 
			this.splitContainerAdvHorizontal.Panel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252))))), System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255))))));
			this.splitContainerAdvHorizontal.Panel1.Resize += new System.EventHandler(this.splitContainerAdvHorizontalPanel1Resize);
			// 
			// splitContainerAdvHorizontal.Panel2
			// 
			this.splitContainerAdvHorizontal.Panel2.AutoScroll = true;
			this.splitContainerAdvHorizontal.Panel2.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252))))), System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255))))));
			this.splitContainerAdvHorizontal.PanelToBeCollapsed = Syncfusion.Windows.Forms.Tools.Enums.CollapsedPanel.Panel2;
			this.splitContainerAdvHorizontal.Size = new System.Drawing.Size(738, 483);
			this.splitContainerAdvHorizontal.SplitterDistance = 244;
			this.splitContainerAdvHorizontal.Style = Syncfusion.Windows.Forms.Tools.Enums.Style.Office2007Blue;
			this.splitContainerAdvHorizontal.TabIndex = 0;
			this.splitContainerAdvHorizontal.Text = "splitContainerAdv2";
			// 
			// ribbonControlAdv1
			// 
			this.ribbonControlAdv1.AllowCollapse = false;
			this.ribbonControlAdv1.AutoSize = true;
			this.ribbonControlAdv1.CaptionFont = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ribbonControlAdv1.Header.AddMainItem(toolStripTabHome);
			this.ribbonControlAdv1.Header.AddQuickItem(new Syncfusion.Windows.Forms.Tools.QuickButtonReflectable(toolStripButtonSave));
			this.ribbonControlAdv1.Location = new System.Drawing.Point(1, 0);
			this.ribbonControlAdv1.MaximizeToolTip = "Maximize Ribbon";
			this.ribbonControlAdv1.MenuButtonImage = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Menu;
			this.ribbonControlAdv1.MenuButtonText = "";
			this.ribbonControlAdv1.MinimizeToolTip = "Minimize Ribbon";
			this.ribbonControlAdv1.Name = "ribbonControlAdv1";
			// 
			// ribbonControlAdv1.OfficeMenu
			// 
			this.ribbonControlAdv1.OfficeMenu.AuxPanel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ribbonControlAdv1.OfficeMenu.AuxPanel.MinimumSize = new System.Drawing.Size(50, 0);
			this.ribbonControlAdv1.OfficeMenu.AuxPanel.Text = "";
			this.ribbonControlAdv1.OfficeMenu.MainPanel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ribbonControlAdv1.OfficeMenu.MainPanel.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonSave,
            this.toolStripSeparator1,
            this.toolStripButtonHelp,
            this.toolStripSeparator2,
            this.toolStripButtonCloseExit});
			this.ribbonControlAdv1.OfficeMenu.Name = "OfficeMenu";
			this.ribbonControlAdv1.OfficeMenu.Size = new System.Drawing.Size(225, 186);
			this.ribbonControlAdv1.OfficeMenu.SystemPanel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ribbonControlAdv1.OfficeMenu.SystemPanel.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonExitSystem,
            this.toolStripButtonSystemOptions});
			this.ribbonControlAdv1.SelectedTab = this.toolStripTabHome;
			this.ribbonControlAdv1.ShowQuickItemsDropDownButton = false;
			this.ribbonControlAdv1.Size = new System.Drawing.Size(1034, 141);
			this.ribbonControlAdv1.SystemText.QuickAccessDialogDropDownName = "StartMenu";
			toolStripTabGroup1.Color = System.Drawing.Color.Empty;
			toolStripTabGroup1.Name = "Settings Tools";
			toolStripTabGroup1.Visible = true;
			this.ribbonControlAdv1.TabGroups.Add(toolStripTabGroup1);
			this.ribbonControlAdv1.TabIndex = 0;
			this.ribbonControlAdv1.Text = "ribbonControlAdv1";
			this.ribbonControlAdv1.TitleAlignment = Syncfusion.Windows.Forms.Tools.TextAlignment.Center;
			// 
			// toolStripTabHome
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripTabHome, "");
			this.toolStripTabHome.Name = "toolStripTabHome";
			// 
			// ribbonControlAdv1.ribbonPanel1
			// 
			this.toolStripTabHome.Panel.Controls.Add(this.toolStripRefresh);
			this.toolStripTabHome.Panel.Controls.Add(this.tsClipboard);
			this.toolStripTabHome.Panel.Controls.Add(this.tcEdit);
			this.toolStripTabHome.Panel.Controls.Add(this.tcShiftBags);
			this.toolStripTabHome.Panel.Controls.Add(this.tcRename);
			this.toolStripTabHome.Panel.Controls.Add(this.tcViews);
			this.toolStripTabHome.Panel.Name = "ribbonPanel1";
			this.toolStripTabHome.Panel.ScrollPosition = 0;
			this.toolStripTabHome.Panel.TabIndex = 2;
			this.toolStripTabHome.Panel.Text = "xxHome";
			this.toolStripTabHome.Position = 0;
			this.SetShortcut(this.toolStripTabHome, System.Windows.Forms.Keys.None);
			this.toolStripTabHome.Size = new System.Drawing.Size(51, 19);
			this.toolStripTabHome.Text = "xxHome";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripTabHome, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripTabHome, false);
			// 
			// toolStripRefresh
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripRefresh, "");
			this.toolStripRefresh.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripRefresh.ForeColor = System.Drawing.Color.MidnightBlue;
			this.toolStripRefresh.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripRefresh.Image = null;
			this.toolStripRefresh.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonRefresh});
			this.toolStripRefresh.Location = new System.Drawing.Point(0, 1);
			this.toolStripRefresh.Name = "toolStripRefresh";
			this.toolStripRefresh.ShowCaption = true;
			this.toolStripRefresh.ShowLauncher = false;
			this.toolStripRefresh.Size = new System.Drawing.Size(106, 79);
			this.toolStripRefresh.TabIndex = 7;
			this.toolStripRefresh.Text = "xxRefresh";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripRefresh, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripRefresh, false);
			// 
			// toolStripButtonRefresh
			// 
			this.toolStripButtonRefresh.AutoToolTip = false;
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonRefresh, "");
			this.toolStripButtonRefresh.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Refresh;
			this.toolStripButtonRefresh.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonRefresh.Name = "toolStripButtonRefresh";
			this.toolStripButtonRefresh.Padding = new System.Windows.Forms.Padding(4);
			this.SetShortcut(this.toolStripButtonRefresh, System.Windows.Forms.Keys.None);
			this.toolStripButtonRefresh.Size = new System.Drawing.Size(68, 59);
			this.toolStripButtonRefresh.Text = "xxRefresh";
			this.toolStripButtonRefresh.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			this.toolStripButtonRefresh.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonRefresh, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonRefresh, false);
			this.toolStripButtonRefresh.Click += new System.EventHandler(this.toolStripButtonRefreshClick);
			// 
			// tsClipboard
			// 
			this.tsClipboard.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.ribbonControlAdv1.SetDescription(this.tsClipboard, "");
			this.tsClipboard.Dock = System.Windows.Forms.DockStyle.None;
			this.tsClipboard.ForeColor = System.Drawing.Color.MidnightBlue;
			this.tsClipboard.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.tsClipboard.Image = null;
			this.tsClipboard.Location = new System.Drawing.Point(108, 1);
			this.tsClipboard.Name = "tsClipboard";
			this.tsClipboard.ShowLauncher = false;
			this.tsClipboard.Size = new System.Drawing.Size(106, 79);
			this.tsClipboard.TabIndex = 6;
			this.tsClipboard.Text = "xxClipboard";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.tsClipboard, false);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.tsClipboard, false);
			// 
			// tcEdit
			// 
			this.tcEdit.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.ribbonControlAdv1.SetDescription(this.tcEdit, "");
			this.tcEdit.Dock = System.Windows.Forms.DockStyle.None;
			this.tcEdit.ForeColor = System.Drawing.Color.MidnightBlue;
			this.tcEdit.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.tcEdit.Image = null;
			this.tcEdit.Location = new System.Drawing.Point(216, 1);
			this.tcEdit.Name = "tcEdit";
			this.tcEdit.ShowLauncher = false;
			this.tcEdit.Size = new System.Drawing.Size(106, 79);
			this.tcEdit.TabIndex = 5;
			this.tcEdit.Text = "xxEdit";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.tcEdit, false);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.tcEdit, false);
			// 
			// tcShiftBags
			// 
			this.tcShiftBags.CaptionAlignment = Syncfusion.Windows.Forms.Tools.CaptionAlignment.Center;
			this.ribbonControlAdv1.SetDescription(this.tcShiftBags, "");
			this.tcShiftBags.Dock = System.Windows.Forms.DockStyle.None;
			this.tcShiftBags.ForeColor = System.Drawing.Color.MidnightBlue;
			this.tcShiftBags.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.tcShiftBags.Image = null;
			this.tcShiftBags.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonAddRuleSet});
			this.tcShiftBags.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
			this.tcShiftBags.Location = new System.Drawing.Point(324, 1);
			this.tcShiftBags.Name = "tcShiftBags";
			this.tcShiftBags.Padding = new System.Windows.Forms.Padding(0);
			this.tcShiftBags.ShowLauncher = false;
			this.tcShiftBags.Size = new System.Drawing.Size(114, 79);
			this.tcShiftBags.TabIndex = 4;
			this.tcShiftBags.Text = "xxShiftBags";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.tcShiftBags, false);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.tcShiftBags, false);
			// 
			// toolStripButtonAddRuleSet
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonAddRuleSet, "");
			this.toolStripButtonAddRuleSet.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_ShiftRuleSet;
			this.toolStripButtonAddRuleSet.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonAddRuleSet.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonAddRuleSet.Name = "toolStripButtonAddRuleSet";
			this.SetShortcut(this.toolStripButtonAddRuleSet, System.Windows.Forms.Keys.None);
			this.toolStripButtonAddRuleSet.Size = new System.Drawing.Size(108, 59);
			this.toolStripButtonAddRuleSet.Text = "xxManageRuleSets";
			this.toolStripButtonAddRuleSet.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			this.toolStripButtonAddRuleSet.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonAddRuleSet, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonAddRuleSet, false);
			this.toolStripButtonAddRuleSet.Click += new System.EventHandler(this.toolStripAssignRuleSetClick);
			// 
			// tcRename
			// 
			this.ribbonControlAdv1.SetDescription(this.tcRename, "");
			this.tcRename.Dock = System.Windows.Forms.DockStyle.None;
			this.tcRename.ForeColor = System.Drawing.Color.MidnightBlue;
			this.tcRename.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.tcRename.Image = null;
			this.tcRename.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonRename});
			this.tcRename.Location = new System.Drawing.Point(440, 1);
			this.tcRename.Name = "tcRename";
			this.tcRename.Padding = new System.Windows.Forms.Padding(0);
			this.tcRename.ShowLauncher = false;
			this.tcRename.Size = new System.Drawing.Size(70, 79);
			this.tcRename.TabIndex = 3;
			this.tcRename.Text = "xxRename";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.tcRename, false);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.tcRename, false);
			// 
			// toolStripButtonRename
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonRename, "");
			this.toolStripButtonRename.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Rename_group_32x32;
			this.toolStripButtonRename.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonRename.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonRename.Name = "toolStripButtonRename";
			this.SetShortcut(this.toolStripButtonRename, System.Windows.Forms.Keys.None);
			this.toolStripButtonRename.Size = new System.Drawing.Size(64, 59);
			this.toolStripButtonRename.Tag = "";
			this.toolStripButtonRename.Text = "xxRename";
			this.toolStripButtonRename.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			this.toolStripButtonRename.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonRename, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonRename, false);
			this.toolStripButtonRename.Click += new System.EventHandler(this.toolStripButtonRenameClick);
			// 
			// tcViews
			// 
			this.ribbonControlAdv1.SetDescription(this.tcViews, "");
			this.tcViews.Dock = System.Windows.Forms.DockStyle.None;
			this.tcViews.ForeColor = System.Drawing.Color.MidnightBlue;
			this.tcViews.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.tcViews.Image = null;
			this.tcViews.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonGeneral,
            this.toolStripButtonCombined,
            this.toolStripButtonLimitations,
            this.toolStripButtonDateExclusion,
            this.toolStripButtonWeekdayExclusion});
			this.tcViews.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
			this.tcViews.Location = new System.Drawing.Point(512, 1);
			this.tcViews.Name = "tcViews";
			this.tcViews.ShowLauncher = false;
			this.tcViews.Size = new System.Drawing.Size(477, 79);
			this.tcViews.TabIndex = 2;
			this.tcViews.Text = "xxViews";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.tcViews, false);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.tcViews, false);
			// 
			// toolStripButtonGeneral
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonGeneral, "");
			this.toolStripButtonGeneral.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_general_settings2;
			this.toolStripButtonGeneral.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonGeneral.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonGeneral.Name = "toolStripButtonGeneral";
			this.SetShortcut(this.toolStripButtonGeneral, System.Windows.Forms.Keys.None);
			this.toolStripButtonGeneral.Size = new System.Drawing.Size(61, 59);
			this.toolStripButtonGeneral.Text = "xxGeneral";
			this.toolStripButtonGeneral.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			this.toolStripButtonGeneral.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonGeneral, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonGeneral, false);
			this.toolStripButtonGeneral.Click += new System.EventHandler(this.toolStripButtonTemplateViewClick);
			// 
			// toolStripButtonCombined
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonCombined, "");
			this.toolStripButtonCombined.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Activities_settings2;
			this.toolStripButtonCombined.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonCombined.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonCombined.Name = "toolStripButtonCombined";
			this.SetShortcut(this.toolStripButtonCombined, System.Windows.Forms.Keys.None);
			this.toolStripButtonCombined.Size = new System.Drawing.Size(99, 59);
			this.toolStripButtonCombined.Text = "xxCombinedGrid";
			this.toolStripButtonCombined.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			this.toolStripButtonCombined.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonCombined, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonCombined, false);
			this.toolStripButtonCombined.Click += new System.EventHandler(this.toolStripButtonCombinedClick);
			// 
			// toolStripButtonLimitations
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonLimitations, "");
			this.toolStripButtonLimitations.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_ShiftLimitationsFilter;
			this.toolStripButtonLimitations.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonLimitations.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonLimitations.Name = "toolStripButtonLimitations";
			this.SetShortcut(this.toolStripButtonLimitations, System.Windows.Forms.Keys.None);
			this.toolStripButtonLimitations.Size = new System.Drawing.Size(80, 59);
			this.toolStripButtonLimitations.Text = "xxLimitations";
			this.toolStripButtonLimitations.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			this.toolStripButtonLimitations.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonLimitations, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonLimitations, false);
			this.toolStripButtonLimitations.Click += new System.EventHandler(this.toolStripButtonLimiterViewClick);
			// 
			// toolStripButtonDateExclusion
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonDateExclusion, "");
			this.toolStripButtonDateExclusion.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_ShiftDateExclusions;
			this.toolStripButtonDateExclusion.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonDateExclusion.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonDateExclusion.Name = "toolStripButtonDateExclusion";
			this.SetShortcut(this.toolStripButtonDateExclusion, System.Windows.Forms.Keys.None);
			this.toolStripButtonDateExclusion.Size = new System.Drawing.Size(103, 59);
			this.toolStripButtonDateExclusion.Text = "xxAvailabilityDate";
			this.toolStripButtonDateExclusion.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			this.toolStripButtonDateExclusion.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonDateExclusion, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonDateExclusion, false);
			this.toolStripButtonDateExclusion.Click += new System.EventHandler(this.toolStripButtonDateExclusionClick);
			// 
			// toolStripButtonWeekdayExclusion
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonWeekdayExclusion, "");
			this.toolStripButtonWeekdayExclusion.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_ShiftWeekExclusions;
			this.toolStripButtonWeekdayExclusion.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonWeekdayExclusion.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonWeekdayExclusion.Name = "toolStripButtonWeekdayExclusion";
			this.SetShortcut(this.toolStripButtonWeekdayExclusion, System.Windows.Forms.Keys.None);
			this.toolStripButtonWeekdayExclusion.Size = new System.Drawing.Size(127, 59);
			this.toolStripButtonWeekdayExclusion.Text = "xxAvailabilityWeekday";
			this.toolStripButtonWeekdayExclusion.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			this.toolStripButtonWeekdayExclusion.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonWeekdayExclusion, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonWeekdayExclusion, false);
			this.toolStripButtonWeekdayExclusion.Click += new System.EventHandler(this.toolStripButtonWeekdayExclusionClick);
			// 
			// toolStripButtonSave
			// 
			this.toolStripButtonSave.AutoToolTip = false;
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonSave, "");
			this.toolStripButtonSave.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Save;
			this.toolStripButtonSave.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripButtonSave.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonSave.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonSave.Name = "toolStripButtonSave";
			this.toolStripButtonSave.Padding = new System.Windows.Forms.Padding(4);
			this.SetShortcut(this.toolStripButtonSave, System.Windows.Forms.Keys.None);
			this.toolStripButtonSave.Size = new System.Drawing.Size(90, 44);
			this.toolStripButtonSave.Text = "xxSave";
			this.toolStripButtonSave.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonSave, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonSave, false);
			this.toolStripButtonSave.Click += new System.EventHandler(this.toolStripButtonSaveClick);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.SetShortcut(this.toolStripSeparator1, System.Windows.Forms.Keys.None);
			this.toolStripSeparator1.Size = new System.Drawing.Size(74, 2);
			// 
			// toolStripButtonHelp
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonHelp, "");
			this.toolStripButtonHelp.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonHelp.Image")));
			this.toolStripButtonHelp.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripButtonHelp.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonHelp.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonHelp.Name = "toolStripButtonHelp";
			this.SetShortcut(this.toolStripButtonHelp, System.Windows.Forms.Keys.None);
			this.toolStripButtonHelp.Size = new System.Drawing.Size(90, 36);
			this.toolStripButtonHelp.Text = "xxHelp";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonHelp, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonHelp, false);
			this.toolStripButtonHelp.Click += new System.EventHandler(this.toolStripButtonHelpClick);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.SetShortcut(this.toolStripSeparator2, System.Windows.Forms.Keys.None);
			this.toolStripSeparator2.Size = new System.Drawing.Size(74, 2);
			// 
			// toolStripButtonCloseExit
			// 
			this.toolStripButtonCloseExit.AutoToolTip = false;
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonCloseExit, "");
			this.toolStripButtonCloseExit.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Close;
			this.toolStripButtonCloseExit.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripButtonCloseExit.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonCloseExit.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonCloseExit.Name = "toolStripButtonCloseExit";
			this.toolStripButtonCloseExit.Padding = new System.Windows.Forms.Padding(4);
			this.SetShortcut(this.toolStripButtonCloseExit, System.Windows.Forms.Keys.None);
			this.toolStripButtonCloseExit.Size = new System.Drawing.Size(90, 44);
			this.toolStripButtonCloseExit.Text = "xxClose";
			this.toolStripButtonCloseExit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonCloseExit, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonCloseExit, false);
			this.toolStripButtonCloseExit.Click += new System.EventHandler(this.toolStripButtonCloseExitClick);
			// 
			// statusStripEx1
			// 
			this.statusStripEx1.Location = new System.Drawing.Point(6, 625);
			this.statusStripEx1.Name = "statusStripEx1";
			this.statusStripEx1.Size = new System.Drawing.Size(1024, 22);
			this.statusStripEx1.TabIndex = 5;
			this.statusStripEx1.Text = "statusStripEx1";
			// 
			// WorkShiftsExplorer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Borders = new System.Windows.Forms.Padding(6, 1, 6, 2);
			this.ClientSize = new System.Drawing.Size(1036, 649);
			this.Controls.Add(this.splitContainerAdvVertical);
			this.Controls.Add(this.ribbonControlAdv1);
			this.Controls.Add(this.statusStripEx1);
			this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "WorkShiftsExplorer";
			this.Text = "xxTeleoptiRaptorColonShifts";
			this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.workShiftsExplorerFormClosing);
			this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.shiftCreatorKeyUp);
			this.splitContainerAdvVertical.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerAdvVertical)).EndInit();
			this.splitContainerAdvVertical.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerAdvHorizontal)).EndInit();
			this.splitContainerAdvHorizontal.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).EndInit();
			this.ribbonControlAdv1.ResumeLayout(false);
			this.ribbonControlAdv1.PerformLayout();
			this.toolStripTabHome.Panel.ResumeLayout(false);
			this.toolStripTabHome.Panel.PerformLayout();
			this.toolStripRefresh.ResumeLayout(false);
			this.toolStripRefresh.PerformLayout();
			this.tcShiftBags.ResumeLayout(false);
			this.tcShiftBags.PerformLayout();
			this.tcRename.ResumeLayout(false);
			this.tcRename.PerformLayout();
			this.tcViews.ResumeLayout(false);
			this.tcViews.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed ribbonControlAdv1;
        private Syncfusion.Windows.Forms.Tools.SplitContainerAdv splitContainerAdvVertical;
        private Syncfusion.Windows.Forms.Tools.SplitContainerAdv splitContainerAdvHorizontal;
        private Syncfusion.Windows.Forms.Tools.ToolStripTabItem toolStripTabHome;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx tcViews;
        private System.Windows.Forms.ToolStripButton toolStripButtonGeneral;
        private System.Windows.Forms.ToolStripButton toolStripButtonLimitations;
        private System.Windows.Forms.ToolStripButton toolStripButtonSave;
        private System.Windows.Forms.ToolStripButton toolStripButtonRefresh;
        private System.Windows.Forms.ToolStripButton toolStripButtonCloseExit;
        private System.Windows.Forms.ToolStripButton toolStripButtonDateExclusion;
        private System.Windows.Forms.ToolStripButton toolStripButtonWeekdayExclusion;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx tcRename;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton toolStripButtonCombined;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx tcShiftBags;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx tcEdit;
        private ToolStripButton toolStripButtonRename;
        private ImageList imageList1;
        private ToolStripButton toolStripButtonExitSystem;
        private ToolStripButton toolStripButtonSystemOptions;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx tsClipboard;
        private ToolStripButton toolStripButtonAddRuleSet;
        private Syncfusion.Windows.Forms.Tools.StatusStripEx statusStripEx1;
        private ToolStripButton toolStripButtonHelp;
        private ToolStripSeparator toolStripSeparator1;
		private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripRefresh;
    }
}
