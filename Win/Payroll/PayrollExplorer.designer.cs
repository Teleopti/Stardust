using System.Windows.Forms;
using Teleopti.Ccc.Win.Common.Controls.DateSelection;

namespace Teleopti.Ccc.Win.Payroll
{
    partial class PayrollExplorer
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
            Syncfusion.Windows.Forms.Tools.TreeNodeAdvStyleInfo treeNodeAdvStyleInfo1 = new Syncfusion.Windows.Forms.Tools.TreeNodeAdvStyleInfo();
            Syncfusion.Windows.Forms.Tools.ToolStripTabGroup toolStripTabGroup1 = new Syncfusion.Windows.Forms.Tools.ToolStripTabGroup();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PayrollExplorer));
            this.tsbExit = new System.Windows.Forms.ToolStripButton();
            this.tsbSystemOptions = new System.Windows.Forms.ToolStripButton();
            this.treeViewBag = new Syncfusion.Windows.Forms.Tools.TreeViewAdv();
            this.tsbWeekView = new System.Windows.Forms.ToolStripButton();
            this.tsDatePeriod = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
            this.splitContainerAdvVertical = new Syncfusion.Windows.Forms.Tools.SplitContainerAdv();
            this.scRight = new Syncfusion.Windows.Forms.Tools.SplitContainerAdv();
            this.ribbonControl = new Syncfusion.Windows.Forms.Tools.RibbonControlAdv();
            this.toolStripTabHome = new Syncfusion.Windows.Forms.Tools.ToolStripTabItem();
            this.tsClipboard = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
            this.tsEdit = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
            this.tsRename = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
            this.tsbRename = new System.Windows.Forms.ToolStripButton();
            this.tsSorting = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
            this.tsbSort = new System.Windows.Forms.ToolStripSplitButton();
            this.toolStripMenuItemSortAsc = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemSortDesc = new System.Windows.Forms.ToolStripMenuItem();
            this.tsViews = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
            this.tsbDayView = new System.Windows.Forms.ToolStripButton();
            this.tsbSave = new System.Windows.Forms.ToolStripButton();
            this.tsbRefresh = new System.Windows.Forms.ToolStripButton();
            this.tsbNew = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbClose = new System.Windows.Forms.ToolStripButton();
            this.statusStripEx1 = new Syncfusion.Windows.Forms.Tools.StatusStripEx();
            this.datePeriod = new Teleopti.Ccc.Win.Common.Controls.DateSelection.DateNavigateControl();
            ((System.ComponentModel.ISupportInitialize)(this.treeViewBag)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerAdvVertical)).BeginInit();
            this.splitContainerAdvVertical.Panel2.SuspendLayout();
            this.splitContainerAdvVertical.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scRight)).BeginInit();
            this.scRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControl)).BeginInit();
            this.ribbonControl.SuspendLayout();
            this.toolStripTabHome.Panel.SuspendLayout();
            this.tsRename.SuspendLayout();
            this.tsSorting.SuspendLayout();
            this.tsViews.SuspendLayout();
            this.SuspendLayout();
            // 
            // tsbExit
            // 
            this.tsbExit.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Exit;
            this.tsbExit.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbExit.Name = "tsbExit";
            this.SetShortcut(this.tsbExit, System.Windows.Forms.Keys.None);
            this.tsbExit.Size = new System.Drawing.Size(113, 20);
            this.tsbExit.Text = "ExitTELEOPTICCC";
            // 
            // tsbSystemOptions
            // 
            this.tsbSystemOptions.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Settings;
            this.tsbSystemOptions.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbSystemOptions.Name = "tsbSystemOptions";
            this.SetShortcut(this.tsbSystemOptions, System.Windows.Forms.Keys.None);
            this.tsbSystemOptions.Size = new System.Drawing.Size(76, 20);
            this.tsbSystemOptions.Text = "xxOptions";
            // 
            // treeViewBag
            // 
            treeNodeAdvStyleInfo1.EnsureDefaultOptionedChild = true;
            this.treeViewBag.BaseStylePairs.AddRange(new Syncfusion.Windows.Forms.Tools.StyleNamePair[] {
            new Syncfusion.Windows.Forms.Tools.StyleNamePair("Standard", treeNodeAdvStyleInfo1)});
            // 
            // 
            // 
            this.treeViewBag.HelpTextControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.treeViewBag.HelpTextControl.Location = new System.Drawing.Point(0, 0);
            this.treeViewBag.HelpTextControl.Name = "helpText";
            this.treeViewBag.HelpTextControl.Size = new System.Drawing.Size(49, 15);
            this.treeViewBag.HelpTextControl.TabIndex = 0;
            this.treeViewBag.HelpTextControl.Text = "help text";
            this.treeViewBag.Location = new System.Drawing.Point(0, 0);
            this.treeViewBag.Name = "treeViewBag";
            this.treeViewBag.Size = new System.Drawing.Size(312, 368);
            // 
            // 
            // 
            this.treeViewBag.ToolTipControl.BackColor = System.Drawing.SystemColors.Info;
            this.treeViewBag.ToolTipControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.treeViewBag.ToolTipControl.Location = new System.Drawing.Point(0, 0);
            this.treeViewBag.ToolTipControl.Name = "toolTip";
            this.treeViewBag.ToolTipControl.Size = new System.Drawing.Size(41, 15);
            this.treeViewBag.ToolTipControl.TabIndex = 1;
            this.treeViewBag.ToolTipControl.Text = "toolTip";
            // 
            // tsbWeekView
            // 
            this.tsbWeekView.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Shift_GeneralView;
            this.tsbWeekView.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbWeekView.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbWeekView.Name = "tsbWeekView";
            this.SetShortcut(this.tsbWeekView, System.Windows.Forms.Keys.None);
            this.tsbWeekView.Size = new System.Drawing.Size(72, 76);
            this.tsbWeekView.Tag = "";
            this.tsbWeekView.Text = "xxWeekView";
            this.tsbWeekView.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.tsbWeekView.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tsbWeekView.ToolTipText = "xxRenameBag";
            this.tsbWeekView.Click += new System.EventHandler(this.toolStripButtonWeekDayView_Click);
            // 
            // tsDatePeriod
            // 
            this.tsDatePeriod.AutoSize = false;
            this.tsDatePeriod.Dock = System.Windows.Forms.DockStyle.None;
            this.tsDatePeriod.ForeColor = System.Drawing.Color.MidnightBlue;
            this.tsDatePeriod.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsDatePeriod.Image = null;
            this.tsDatePeriod.Location = new System.Drawing.Point(597, 1);
            this.tsDatePeriod.Name = "tsDatePeriod";
            this.tsDatePeriod.ShowLauncher = false;
            this.tsDatePeriod.Size = new System.Drawing.Size(157, 96);
            this.tsDatePeriod.TabIndex = 0;
            this.tsDatePeriod.Text = "xxPeriod";
            // 
            // splitContainerAdvVertical
            // 
            this.splitContainerAdvVertical.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerAdvVertical.FixedPanel = Syncfusion.Windows.Forms.Tools.Enums.FixedPanel.Panel1;
            this.splitContainerAdvVertical.IsSplitterFixed = true;
            this.splitContainerAdvVertical.Location = new System.Drawing.Point(6, 164);
            this.splitContainerAdvVertical.Name = "splitContainerAdvVertical";
            // 
            // splitContainerAdvVertical.Panel1
            // 
            this.splitContainerAdvVertical.Panel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252))))), System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255))))));
            // 
            // splitContainerAdvVertical.Panel2
            // 
            this.splitContainerAdvVertical.Panel2.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252))))), System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255))))));
            this.splitContainerAdvVertical.Panel2.Controls.Add(this.scRight);
            this.splitContainerAdvVertical.PanelToBeCollapsed = Syncfusion.Windows.Forms.Tools.Enums.CollapsedPanel.Panel1;
            this.splitContainerAdvVertical.Size = new System.Drawing.Size(1024, 461);
            this.splitContainerAdvVertical.SplitterDistance = 279;
            this.splitContainerAdvVertical.Style = Syncfusion.Windows.Forms.Tools.Enums.Style.Office2007Blue;
            this.splitContainerAdvVertical.TabIndex = 1;
            this.splitContainerAdvVertical.Text = "splitContainerAdv1";
            // 
            // scRight
            // 
            this.scRight.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Horizontal, System.Drawing.Color.FromArgb(((int)(((byte)(227)))), ((int)(((byte)(239)))), ((int)(((byte)(255))))), System.Drawing.Color.FromArgb(((int)(((byte)(101)))), ((int)(((byte)(147)))), ((int)(((byte)(207))))));
            this.scRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scRight.FixedPanel = Syncfusion.Windows.Forms.Tools.Enums.FixedPanel.Panel2;
            this.scRight.HotBackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Horizontal, System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(151)))), ((int)(((byte)(61))))), System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(184)))), ((int)(((byte)(94))))));
            this.scRight.Location = new System.Drawing.Point(0, 0);
            this.scRight.Name = "scRight";
            this.scRight.Orientation = System.Windows.Forms.Orientation.Vertical;
            // 
            // scRight.Panel1
            // 
            this.scRight.Panel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252))))), System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255))))));
            this.scRight.Panel1.Resize += new System.EventHandler(this.scRight_Panel1_Resize);
            // 
            // scRight.Panel2
            // 
            this.scRight.Panel2.AutoScroll = true;
            this.scRight.Panel2.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252))))), System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255))))));
            this.scRight.PanelToBeCollapsed = Syncfusion.Windows.Forms.Tools.Enums.CollapsedPanel.Panel2;
            this.scRight.Size = new System.Drawing.Size(738, 461);
            this.scRight.SplitterDistance = 228;
            this.scRight.Style = Syncfusion.Windows.Forms.Tools.Enums.Style.Office2007Blue;
            this.scRight.TabIndex = 0;
            this.scRight.Text = "splitContainerAdv2";
            // 
            // ribbonControl
            // 
            this.ribbonControl.CaptionFont = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ribbonControl.Header.AddMainItem(toolStripTabHome);
            this.ribbonControl.Header.AddQuickItem(new Syncfusion.Windows.Forms.Tools.QuickButtonReflectable(tsbSave));
            this.ribbonControl.Header.AddQuickItem(new Syncfusion.Windows.Forms.Tools.QuickButtonReflectable(tsbRefresh));
            this.ribbonControl.Location = new System.Drawing.Point(1, 0);
            this.ribbonControl.MenuButtonImage = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Menu;
            this.ribbonControl.Name = "ribbonControl";
            // 
            // ribbonControl.OfficeMenu
            // 
            this.ribbonControl.OfficeMenu.AuxPanel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ribbonControl.OfficeMenu.AuxPanel.MinimumSize = new System.Drawing.Size(200, 0);
            this.ribbonControl.OfficeMenu.AuxPanel.Text = "xxRecentDefinitionSets";
            this.ribbonControl.OfficeMenu.MainPanel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ribbonControl.OfficeMenu.MainPanel.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbNew,
            this.toolStripSeparator1,
            this.tsbSave,
            this.tsbRefresh,
            this.toolStripSeparator2,
            this.tsbClose});
            this.ribbonControl.OfficeMenu.Name = "OfficeMenu";
            this.ribbonControl.OfficeMenu.Size = new System.Drawing.Size(315, 241);
            this.ribbonControl.OfficeMenu.SystemPanel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ribbonControl.OfficeMenu.SystemPanel.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbExit,
            this.tsbSystemOptions});
            this.ribbonControl.Size = new System.Drawing.Size(1034, 163);
            this.ribbonControl.SystemText.QuickAccessDialogDropDownName = "Start menu";
            toolStripTabGroup1.Color = System.Drawing.Color.Empty;
            toolStripTabGroup1.Name = "Settings Tools";
            toolStripTabGroup1.Visible = true;
            this.ribbonControl.TabGroups.Add(toolStripTabGroup1);
            this.ribbonControl.TabIndex = 0;
            this.ribbonControl.Text = "ribbonControlAdv1";
            this.ribbonControl.TitleAlignment = Syncfusion.Windows.Forms.Tools.TextAlignment.Center;
            // 
            // toolStripTabHome
            // 
            this.toolStripTabHome.Name = "toolStripTabHome";
            // 
            // ribbonControl.ribbonPanel1
            // 
            this.toolStripTabHome.Panel.Controls.Add(this.tsClipboard);
            this.toolStripTabHome.Panel.Controls.Add(this.tsEdit);
            this.toolStripTabHome.Panel.Controls.Add(this.tsRename);
            this.toolStripTabHome.Panel.Controls.Add(this.tsSorting);
            this.toolStripTabHome.Panel.Controls.Add(this.tsViews);
            this.toolStripTabHome.Panel.Controls.Add(this.tsDatePeriod);
            this.toolStripTabHome.Panel.Name = "ribbonPanel1";
            this.toolStripTabHome.Panel.ScrollPosition = 0;
            this.toolStripTabHome.Panel.TabIndex = 2;
            this.toolStripTabHome.Panel.Text = "xxHome";
            this.SetShortcut(this.toolStripTabHome, System.Windows.Forms.Keys.None);
            this.toolStripTabHome.Size = new System.Drawing.Size(51, 19);
            this.toolStripTabHome.Text = "xxHome";
            // 
            // tsClipboard
            // 
            this.tsClipboard.ForeColor = System.Drawing.Color.MidnightBlue;
            this.tsClipboard.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsClipboard.Image = null;
            this.tsClipboard.Location = new System.Drawing.Point(0, 1);
            this.tsClipboard.Name = "tsClipboard";
            this.tsClipboard.ShowLauncher = false;
            this.tsClipboard.Size = new System.Drawing.Size(135, 96);
            this.tsClipboard.TabIndex = 7;
            this.tsClipboard.Text = "xxClipboard";
            // 
            // tsEdit
            // 
            this.tsEdit.ForeColor = System.Drawing.Color.MidnightBlue;
            this.tsEdit.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsEdit.Image = null;
            this.tsEdit.Location = new System.Drawing.Point(135, 1);
            this.tsEdit.Name = "tsEdit";
            this.tsEdit.ShowLauncher = false;
            this.tsEdit.Size = new System.Drawing.Size(110, 96);
            this.tsEdit.TabIndex = 6;
            this.tsEdit.Text = "xxEdit";
            // 
            // tsRename
            // 
            this.tsRename.ForeColor = System.Drawing.Color.MidnightBlue;
            this.tsRename.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsRename.Image = null;
            this.tsRename.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbRename});
            this.tsRename.Location = new System.Drawing.Point(245, 1);
            this.tsRename.Name = "tsRename";
            this.tsRename.ShowLauncher = false;
            this.tsRename.Size = new System.Drawing.Size(110, 96);
            this.tsRename.TabIndex = 5;
            this.tsRename.Text = "xxRename";
            // 
            // tsbRename
            // 
            this.tsbRename.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Rename_group_32x32;
            this.tsbRename.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbRename.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbRename.Name = "tsbRename";
            this.SetShortcut(this.tsbRename, System.Windows.Forms.Keys.None);
            this.tsbRename.Size = new System.Drawing.Size(62, 76);
            this.tsbRename.Tag = "";
            this.tsbRename.Text = "xxRename";
            this.tsbRename.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.tsbRename.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tsbRename.ToolTipText = "xxRenameBag";
            this.tsbRename.Click += new System.EventHandler(this.tsbRename_Click);
            // 
            // tsSorting
            // 
            this.tsSorting.AutoSize = false;
            this.tsSorting.Dock = System.Windows.Forms.DockStyle.None;
            this.tsSorting.ForeColor = System.Drawing.Color.MidnightBlue;
            this.tsSorting.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsSorting.Image = null;
            this.tsSorting.ImageScalingSize = new System.Drawing.Size(25, 25);
            this.tsSorting.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbSort});
            this.tsSorting.Location = new System.Drawing.Point(355, 1);
            this.tsSorting.Name = "tsSorting";
            this.tsSorting.ShowLauncher = false;
            this.tsSorting.Size = new System.Drawing.Size(78, 96);
            this.tsSorting.TabIndex = 3;
            this.tsSorting.Text = "xxSorting";
            // 
            // tsbSort
            // 
            this.tsbSort.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemSortAsc,
            this.toolStripMenuItemSortDesc});
            this.tsbSort.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Sort;
            this.tsbSort.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbSort.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbSort.Name = "tsbSort";
            this.SetShortcut(this.tsbSort, System.Windows.Forms.Keys.None);
            this.tsbSort.Size = new System.Drawing.Size(55, 76);
            this.tsbSort.Text = "xxSort";
            this.tsbSort.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.tsbSort.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tsbSort.Click += new System.EventHandler(this.tsbSort_Click);
            // 
            // toolStripMenuItemSortAsc
            // 
            this.toolStripMenuItemSortAsc.Name = "toolStripMenuItemSortAsc";
            this.SetShortcut(this.toolStripMenuItemSortAsc, System.Windows.Forms.Keys.None);
            this.toolStripMenuItemSortAsc.Size = new System.Drawing.Size(140, 22);
            this.toolStripMenuItemSortAsc.Text = "xxSortAtoZ";
            this.toolStripMenuItemSortAsc.Click += new System.EventHandler(this.toolStripMenuItemSortAsc_Click);
            // 
            // toolStripMenuItemSortDesc
            // 
            this.toolStripMenuItemSortDesc.Name = "toolStripMenuItemSortDesc";
            this.SetShortcut(this.toolStripMenuItemSortDesc, System.Windows.Forms.Keys.None);
            this.toolStripMenuItemSortDesc.Size = new System.Drawing.Size(140, 22);
            this.toolStripMenuItemSortDesc.Text = "xxSortZtoA";
            this.toolStripMenuItemSortDesc.Click += new System.EventHandler(this.toolStripMenuItemSortDesc_Click);
            // 
            // tsViews
            // 
            this.tsViews.AutoSize = false;
            this.tsViews.Dock = System.Windows.Forms.DockStyle.None;
            this.tsViews.ForeColor = System.Drawing.Color.MidnightBlue;
            this.tsViews.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsViews.Image = null;
            this.tsViews.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbWeekView,
            this.tsbDayView});
            this.tsViews.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.tsViews.Location = new System.Drawing.Point(433, 1);
            this.tsViews.Name = "tsViews";
            this.tsViews.ShowLauncher = false;
            this.tsViews.Size = new System.Drawing.Size(164, 96);
            this.tsViews.TabIndex = 2;
            this.tsViews.Text = "xxDatePeriod";
            // 
            // tsbDayView
            // 
            this.tsbDayView.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Shift_GeneralView;
            this.tsbDayView.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbDayView.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbDayView.Name = "tsbDayView";
            this.SetShortcut(this.tsbDayView, System.Windows.Forms.Keys.None);
            this.tsbDayView.Size = new System.Drawing.Size(64, 76);
            this.tsbDayView.Tag = "";
            this.tsbDayView.Text = "xxDayView";
            this.tsbDayView.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.tsbDayView.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tsbDayView.ToolTipText = "xxRenameBag";
            this.tsbDayView.Click += new System.EventHandler(this.toolStripButtonDateView_Click);
            // 
            // tsbSave
            // 
            this.tsbSave.AutoToolTip = false;
            this.tsbSave.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Save;
            this.tsbSave.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.tsbSave.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbSave.Name = "tsbSave";
            this.tsbSave.Padding = new System.Windows.Forms.Padding(4);
            this.SetShortcut(this.tsbSave, System.Windows.Forms.Keys.None);
            this.tsbSave.Size = new System.Drawing.Size(101, 44);
            this.tsbSave.Text = "xxSave";
            this.tsbSave.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.tsbSave.Click += new System.EventHandler(this.tsbSave_Click);
            // 
            // tsbRefresh
            // 
            this.tsbRefresh.AutoToolTip = false;
            this.tsbRefresh.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Refresh;
            this.tsbRefresh.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.tsbRefresh.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbRefresh.Name = "tsbRefresh";
            this.tsbRefresh.Padding = new System.Windows.Forms.Padding(4);
            this.SetShortcut(this.tsbRefresh, System.Windows.Forms.Keys.None);
            this.tsbRefresh.Size = new System.Drawing.Size(101, 44);
            this.tsbRefresh.Text = "xxRefresh";
            this.tsbRefresh.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.tsbRefresh.Click += new System.EventHandler(this.tsbRefresh_Click);
            // 
            // tsbNew
            // 
            this.tsbNew.AutoToolTip = false;
            this.tsbNew.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_New2;
            this.tsbNew.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.tsbNew.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbNew.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbNew.Name = "tsbNew";
            this.tsbNew.Padding = new System.Windows.Forms.Padding(4);
            this.SetShortcut(this.tsbNew, System.Windows.Forms.Keys.None);
            this.tsbNew.Size = new System.Drawing.Size(101, 44);
            this.tsbNew.Text = "xxNew";
            this.tsbNew.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.SetShortcut(this.toolStripSeparator1, System.Windows.Forms.Keys.None);
            this.toolStripSeparator1.Size = new System.Drawing.Size(85, 2);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.SetShortcut(this.toolStripSeparator2, System.Windows.Forms.Keys.None);
            this.toolStripSeparator2.Size = new System.Drawing.Size(85, 2);
            // 
            // tsbClose
            // 
            this.tsbClose.AutoToolTip = false;
            this.tsbClose.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Close;
            this.tsbClose.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.tsbClose.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsbClose.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbClose.Name = "tsbClose";
            this.tsbClose.Padding = new System.Windows.Forms.Padding(4);
            this.SetShortcut(this.tsbClose, System.Windows.Forms.Keys.None);
            this.tsbClose.Size = new System.Drawing.Size(101, 44);
            this.tsbClose.Text = "xxClose";
            this.tsbClose.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // statusStripEx1
            // 
            this.statusStripEx1.Location = new System.Drawing.Point(6, 625);
            this.statusStripEx1.Name = "statusStripEx1";
            this.statusStripEx1.Size = new System.Drawing.Size(1024, 22);
            this.statusStripEx1.TabIndex = 5;
            this.statusStripEx1.Text = "statusStripEx1";
            // 
            // datePeriod
            // 
            this.datePeriod.BackColor = System.Drawing.Color.Transparent;
            this.datePeriod.Location = new System.Drawing.Point(0, 0);
            this.datePeriod.Name = "datePeriod";
            this.datePeriod.Size = new System.Drawing.Size(144, 77);
            this.datePeriod.TabIndex = 0;
            // 
            // PayrollExplorer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Borders = new System.Windows.Forms.Padding(6, 1, 6, 2);
            this.ClientSize = new System.Drawing.Size(1036, 649);
            this.Controls.Add(this.splitContainerAdvVertical);
            this.Controls.Add(this.ribbonControl);
            this.Controls.Add(this.statusStripEx1);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PayrollExplorer";
            this.Text = "xxTeleoptiRaptorColonShifts";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            ((System.ComponentModel.ISupportInitialize)(this.treeViewBag)).EndInit();
            this.splitContainerAdvVertical.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerAdvVertical)).EndInit();
            this.splitContainerAdvVertical.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scRight)).EndInit();
            this.scRight.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControl)).EndInit();
            this.ribbonControl.ResumeLayout(false);
            this.ribbonControl.PerformLayout();
            this.toolStripTabHome.Panel.ResumeLayout(false);
            this.toolStripTabHome.Panel.PerformLayout();
            this.tsRename.ResumeLayout(false);
            this.tsRename.PerformLayout();
            this.tsSorting.ResumeLayout(false);
            this.tsSorting.PerformLayout();
            this.tsViews.ResumeLayout(false);
            this.tsViews.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.RibbonControlAdv ribbonControl;
        private Syncfusion.Windows.Forms.Tools.SplitContainerAdv splitContainerAdvVertical;
        private Syncfusion.Windows.Forms.Tools.SplitContainerAdv scRight;
        private Syncfusion.Windows.Forms.Tools.ToolStripTabItem toolStripTabHome;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx tsViews;
        private System.Windows.Forms.ToolStripButton tsbSave;
        private System.Windows.Forms.ToolStripButton tsbRefresh;
        private System.Windows.Forms.ToolStripButton tsbClose;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx tsSorting;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton tsbNew;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx tsRename;
        private ToolStripButton tsbExit;
        private ToolStripButton tsbSystemOptions;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx tsEdit;
        private Syncfusion.Windows.Forms.Tools.StatusStripEx statusStripEx1;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx tsClipboard;
        private ToolStripButton tsbRename;
        private ToolStripSplitButton tsbSort;
        private ToolStripMenuItem toolStripMenuItemSortAsc;
        private ToolStripMenuItem toolStripMenuItemSortDesc;
        private Syncfusion.Windows.Forms.Tools.TreeViewAdv treeViewBag;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx tsDatePeriod;
        private ToolStripButton tsbWeekView;
        private ToolStripButton tsbDayView;
        private DateNavigateControl datePeriod;
    }
}
