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
			Syncfusion.Windows.Forms.Tools.ToolStripTabGroup toolStripTabGroup1 = new Syncfusion.Windows.Forms.Tools.ToolStripTabGroup();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WorkShiftsExplorer));
			this.backStageView1 = new Syncfusion.Windows.Forms.BackStageView(this.components);
			this.backStage1 = new Syncfusion.Windows.Forms.BackStage();
			this.backStageButton1 = new Syncfusion.Windows.Forms.BackStageButton();
			this.backStageSeparator1 = new Syncfusion.Windows.Forms.BackStageSeparator();
			this.backStageButton2 = new Syncfusion.Windows.Forms.BackStageButton();
			this.backStageSeparator2 = new Syncfusion.Windows.Forms.BackStageSeparator();
			this.backStageButton3 = new Syncfusion.Windows.Forms.BackStageButton();
			this.backStageButton4 = new Syncfusion.Windows.Forms.BackStageButton();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.splitContainerAdvVertical = new Syncfusion.Windows.Forms.Tools.SplitContainerAdv();
			this.splitContainerAdvHorizontal = new Syncfusion.Windows.Forms.Tools.SplitContainerAdv();
			this.ribbonControlAdv1 = new Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed();
			this.toolStripTabHome = new Syncfusion.Windows.Forms.Tools.ToolStripTabItem();
			this.toolStripExFile = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripButtonSave = new System.Windows.Forms.ToolStripButton();
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
			this.statusStripEx1 = new Syncfusion.Windows.Forms.Tools.StatusStripEx();
			((System.ComponentModel.ISupportInitialize)(this.backStage1)).BeginInit();
			this.backStage1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerAdvVertical)).BeginInit();
			this.splitContainerAdvVertical.Panel2.SuspendLayout();
			this.splitContainerAdvVertical.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerAdvHorizontal)).BeginInit();
			this.splitContainerAdvHorizontal.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).BeginInit();
			this.ribbonControlAdv1.SuspendLayout();
			this.toolStripTabHome.Panel.SuspendLayout();
			this.toolStripExFile.SuspendLayout();
			this.toolStripRefresh.SuspendLayout();
			this.tcShiftBags.SuspendLayout();
			this.tcRename.SuspendLayout();
			this.tcViews.SuspendLayout();
			this.SuspendLayout();
			// 
			// backStageView1
			// 
			this.backStageView1.BackStage = this.backStage1;
			this.backStageView1.HostControl = null;
			this.backStageView1.HostForm = this;
			// 
			// backStage1
			// 
			this.backStage1.ActiveTabFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.backStage1.AllowDrop = true;
			this.backStage1.BeforeTouchSize = new System.Drawing.Size(1203, 691);
			this.backStage1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.backStage1.Controls.Add(this.backStageButton1);
			this.backStage1.Controls.Add(this.backStageSeparator1);
			this.backStage1.Controls.Add(this.backStageButton2);
			this.backStage1.Controls.Add(this.backStageSeparator2);
			this.backStage1.Controls.Add(this.backStageButton3);
			this.backStage1.Controls.Add(this.backStageButton4);
			this.backStage1.ItemSize = new System.Drawing.Size(150, 40);
			this.backStage1.Location = new System.Drawing.Point(1, 51);
			this.backStage1.Name = "backStage1";
			this.backStage1.OfficeColorScheme = Syncfusion.Windows.Forms.Tools.ToolStripEx.ColorScheme.Silver;
			this.backStage1.Size = new System.Drawing.Size(1203, 691);
			this.backStage1.TabIndex = 6;
			// 
			// backStageButton1
			// 
			this.backStageButton1.Accelerator = "";
			this.backStageButton1.BackColor = System.Drawing.Color.Transparent;
			this.backStageButton1.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.backStageButton1.IsBackStageButton = false;
			this.backStageButton1.Location = new System.Drawing.Point(0, 16);
			this.backStageButton1.Name = "backStageButton1";
			this.backStageButton1.Size = new System.Drawing.Size(149, 38);
			this.backStageButton1.TabIndex = 3;
			this.backStageButton1.Text = "xxSave";
			this.backStageButton1.Click += new System.EventHandler(this.backStageButton1Click);
			// 
			// backStageSeparator1
			// 
			this.backStageSeparator1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(204)))), ((int)(((byte)(255)))));
			this.backStageSeparator1.Location = new System.Drawing.Point(25, 60);
			this.backStageSeparator1.Name = "backStageSeparator1";
			this.backStageSeparator1.Size = new System.Drawing.Size(100, 1);
			this.backStageSeparator1.TabIndex = 4;
			this.backStageSeparator1.Text = "backStageSeparator1";
			// 
			// backStageButton2
			// 
			this.backStageButton2.Accelerator = "";
			this.backStageButton2.BackColor = System.Drawing.Color.Transparent;
			this.backStageButton2.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.backStageButton2.IsBackStageButton = false;
			this.backStageButton2.Location = new System.Drawing.Point(0, 66);
			this.backStageButton2.Name = "backStageButton2";
			this.backStageButton2.Size = new System.Drawing.Size(149, 38);
			this.backStageButton2.TabIndex = 5;
			this.backStageButton2.Text = "xxClose";
			this.backStageButton2.Click += new System.EventHandler(this.backStageButton2Click);
			// 
			// backStageSeparator2
			// 
			this.backStageSeparator2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(204)))), ((int)(((byte)(255)))));
			this.backStageSeparator2.Location = new System.Drawing.Point(25, 110);
			this.backStageSeparator2.Name = "backStageSeparator2";
			this.backStageSeparator2.Size = new System.Drawing.Size(100, 1);
			this.backStageSeparator2.TabIndex = 6;
			this.backStageSeparator2.Text = "backStageSeparator2";
			// 
			// backStageButton3
			// 
			this.backStageButton3.Accelerator = "";
			this.backStageButton3.BackColor = System.Drawing.Color.Transparent;
			this.backStageButton3.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.backStageButton3.IsBackStageButton = false;
			this.backStageButton3.Location = new System.Drawing.Point(0, 116);
			this.backStageButton3.Name = "backStageButton3";
			this.backStageButton3.Size = new System.Drawing.Size(149, 38);
			this.backStageButton3.TabIndex = 7;
			this.backStageButton3.Text = "xxOptions";
			this.backStageButton3.Click += new System.EventHandler(this.backStageButton3Click);
			// 
			// backStageButton4
			// 
			this.backStageButton4.Accelerator = "";
			this.backStageButton4.AutoSize = true;
			this.backStageButton4.BackColor = System.Drawing.Color.Transparent;
			this.backStageButton4.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.backStageButton4.IsBackStageButton = false;
			this.backStageButton4.Location = new System.Drawing.Point(0, 154);
			this.backStageButton4.Name = "backStageButton4";
			this.backStageButton4.Size = new System.Drawing.Size(149, 38);
			this.backStageButton4.TabIndex = 8;
			this.backStageButton4.Text = "xxExitTELEOPTICCC";
			this.backStageButton4.VisibleChanged += new System.EventHandler(this.backStageButton4VisibleChanged);
			this.backStageButton4.Click += new System.EventHandler(this.backStageButton4Click);
			// 
			// imageList1
			// 
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			this.imageList1.Images.SetKeyName(0, "ccc_ShiftRuleSet.png");
			this.imageList1.Images.SetKeyName(1, "ccc_ShiftBag.png");
			// 
			// splitContainerAdvVertical
			// 
			this.splitContainerAdvVertical.BeforeTouchSize = 3;
			this.splitContainerAdvVertical.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerAdvVertical.FixedPanel = Syncfusion.Windows.Forms.Tools.Enums.FixedPanel.Panel1;
			this.splitContainerAdvVertical.Location = new System.Drawing.Point(2, 140);
			this.splitContainerAdvVertical.Name = "splitContainerAdvVertical";
			// 
			// splitContainerAdvVertical.Panel1
			// 
			this.splitContainerAdvVertical.Panel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))));
			// 
			// splitContainerAdvVertical.Panel2
			// 
			this.splitContainerAdvVertical.Panel2.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))));
			this.splitContainerAdvVertical.Panel2.Controls.Add(this.splitContainerAdvHorizontal);
			this.splitContainerAdvVertical.PanelToBeCollapsed = Syncfusion.Windows.Forms.Tools.Enums.CollapsedPanel.Panel1;
			this.splitContainerAdvVertical.Size = new System.Drawing.Size(1202, 579);
			this.splitContainerAdvVertical.SplitterDistance = 279;
			this.splitContainerAdvVertical.SplitterWidth = 3;
			this.splitContainerAdvVertical.Style = Syncfusion.Windows.Forms.Tools.Enums.Style.Default;
			this.splitContainerAdvVertical.TabIndex = 1;
			this.splitContainerAdvVertical.Text = "splitContainerAdv1";
			// 
			// splitContainerAdvHorizontal
			// 
			this.splitContainerAdvHorizontal.BackColor = System.Drawing.Color.White;
			this.splitContainerAdvHorizontal.BeforeTouchSize = 3;
			this.splitContainerAdvHorizontal.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerAdvHorizontal.Location = new System.Drawing.Point(0, 0);
			this.splitContainerAdvHorizontal.Name = "splitContainerAdvHorizontal";
			this.splitContainerAdvHorizontal.Orientation = System.Windows.Forms.Orientation.Vertical;
			// 
			// splitContainerAdvHorizontal.Panel1
			// 
			this.splitContainerAdvHorizontal.Panel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))));
			this.splitContainerAdvHorizontal.Panel1.Resize += new System.EventHandler(this.splitContainerAdvHorizontalPanel1Resize);
			// 
			// splitContainerAdvHorizontal.Panel2
			// 
			this.splitContainerAdvHorizontal.Panel2.AutoScroll = true;
			this.splitContainerAdvHorizontal.Panel2.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))));
			this.splitContainerAdvHorizontal.PanelToBeCollapsed = Syncfusion.Windows.Forms.Tools.Enums.CollapsedPanel.Panel2;
			this.splitContainerAdvHorizontal.Size = new System.Drawing.Size(920, 579);
			this.splitContainerAdvHorizontal.SplitterDistance = 290;
			this.splitContainerAdvHorizontal.SplitterWidth = 3;
			this.splitContainerAdvHorizontal.Style = Syncfusion.Windows.Forms.Tools.Enums.Style.Default;
			this.splitContainerAdvHorizontal.TabIndex = 0;
			this.splitContainerAdvHorizontal.Text = "splitContainerAdv2";
			// 
			// ribbonControlAdv1
			// 
			this.ribbonControlAdv1.AllowCollapse = false;
			this.ribbonControlAdv1.AutoSize = true;
			this.ribbonControlAdv1.BackStageView = this.backStageView1;
			this.ribbonControlAdv1.CaptionFont = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ribbonControlAdv1.Header.AddMainItem(toolStripTabHome);
			this.ribbonControlAdv1.HideMenuButtonToolTip = false;
			this.ribbonControlAdv1.Location = new System.Drawing.Point(1, -2);
			this.ribbonControlAdv1.MaximizeToolTip = "Maximize Ribbon";
			this.ribbonControlAdv1.MenuButtonEnabled = true;
			this.ribbonControlAdv1.MenuButtonFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ribbonControlAdv1.MenuButtonText = "ARKIV";
			this.ribbonControlAdv1.MenuButtonVisible = false;
			this.ribbonControlAdv1.MenuButtonWidth = 56;
			this.ribbonControlAdv1.MenuColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.ribbonControlAdv1.MinimizeToolTip = "Minimize Ribbon";
			this.ribbonControlAdv1.MinimumSize = new System.Drawing.Size(0, 63);
			this.ribbonControlAdv1.Name = "ribbonControlAdv1";
			this.ribbonControlAdv1.Office2013ColorScheme = Syncfusion.Windows.Forms.Tools.Office2013ColorScheme.White;
			this.ribbonControlAdv1.OfficeColorScheme = Syncfusion.Windows.Forms.Tools.ToolStripEx.ColorScheme.Silver;
			// 
			// ribbonControlAdv1.OfficeMenu
			// 
			this.ribbonControlAdv1.OfficeMenu.AuxPanel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ribbonControlAdv1.OfficeMenu.AuxPanel.MinimumSize = new System.Drawing.Size(50, 0);
			this.ribbonControlAdv1.OfficeMenu.AuxPanel.Text = "";
			this.ribbonControlAdv1.OfficeMenu.MainPanel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ribbonControlAdv1.OfficeMenu.Name = "OfficeMenu";
			this.ribbonControlAdv1.OfficeMenu.Size = new System.Drawing.Size(65, 65);
			this.ribbonControlAdv1.OfficeMenu.SystemPanel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ribbonControlAdv1.OverFlowButtonToolTip = "Show DropDown";
			this.ribbonControlAdv1.QuickPanelImageLayout = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.ribbonControlAdv1.QuickPanelVisible = false;
			this.ribbonControlAdv1.RibbonHeaderImage = Syncfusion.Windows.Forms.Tools.RibbonHeaderImage.None;
			this.ribbonControlAdv1.RibbonStyle = Syncfusion.Windows.Forms.Tools.RibbonStyle.Office2013;
			this.ribbonControlAdv1.SelectedTab = null;
			this.ribbonControlAdv1.Show2010CustomizeQuickItemDialog = false;
			this.ribbonControlAdv1.ShowQuickItemsDropDownButton = false;
			this.ribbonControlAdv1.ShowRibbonDisplayOptionButton = false;
			this.ribbonControlAdv1.Size = new System.Drawing.Size(1208, 138);
			this.ribbonControlAdv1.SystemText.QuickAccessDialogDropDownName = "StartMenu";
			toolStripTabGroup1.Color = System.Drawing.Color.Empty;
			toolStripTabGroup1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			toolStripTabGroup1.Name = "Settings Tools";
			toolStripTabGroup1.Visible = true;
			this.ribbonControlAdv1.TabGroups.Add(toolStripTabGroup1);
			this.ribbonControlAdv1.TabIndex = 0;
			this.ribbonControlAdv1.Text = "ribbonControlAdv1";
			this.ribbonControlAdv1.TitleAlignment = Syncfusion.Windows.Forms.Tools.TextAlignment.Center;
			this.ribbonControlAdv1.TitleColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(59)))), ((int)(((byte)(59)))));
			this.ribbonControlAdv1.TitleFont = new System.Drawing.Font("Segoe UI", 12F);
			// 
			// toolStripTabHome
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripTabHome, "");
			this.toolStripTabHome.Name = "toolStripTabHome";
			// 
			// ribbonControlAdv1.ribbonPanel1
			// 
			this.toolStripTabHome.Panel.Controls.Add(this.toolStripExFile);
			this.toolStripTabHome.Panel.Controls.Add(this.toolStripRefresh);
			this.toolStripTabHome.Panel.Controls.Add(this.tsClipboard);
			this.toolStripTabHome.Panel.Controls.Add(this.tcEdit);
			this.toolStripTabHome.Panel.Controls.Add(this.tcShiftBags);
			this.toolStripTabHome.Panel.Controls.Add(this.tcRename);
			this.toolStripTabHome.Panel.Controls.Add(this.tcViews);
			this.toolStripTabHome.Panel.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
			this.toolStripTabHome.Panel.Name = "ribbonPanel1";
			this.toolStripTabHome.Panel.ScrollPosition = 0;
			this.toolStripTabHome.Panel.TabIndex = 2;
			this.toolStripTabHome.Panel.Text = "XXHome";
			this.toolStripTabHome.Position = 0;
			this.SetShortcut(this.toolStripTabHome, System.Windows.Forms.Keys.None);
			this.toolStripTabHome.Size = new System.Drawing.Size(69, 25);
			this.toolStripTabHome.Text = "XXHome";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripTabHome, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripTabHome, false);
			// 
			// toolStripExFile
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripExFile, "");
			this.toolStripExFile.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripExFile.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(59)))), ((int)(((byte)(59)))));
			this.toolStripExFile.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripExFile.Image = null;
			this.toolStripExFile.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonSave});
			this.toolStripExFile.LauncherStyle = Syncfusion.Windows.Forms.Tools.LauncherStyle.Metro;
			this.toolStripExFile.Location = new System.Drawing.Point(0, 1);
			this.toolStripExFile.Name = "toolStripExFile";
			this.toolStripExFile.Office12Mode = false;
			this.toolStripExFile.ShowLauncher = false;
			this.toolStripExFile.Size = new System.Drawing.Size(60, 83);
			this.toolStripExFile.TabIndex = 8;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripExFile, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripExFile, false);
			// 
			// toolStripButtonSave
			// 
			this.toolStripButtonSave.AutoToolTip = false;
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonSave, "");
			this.toolStripButtonSave.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Save;
			this.toolStripButtonSave.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonSave.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonSave.Name = "toolStripButtonSave";
			this.toolStripButtonSave.Padding = new System.Windows.Forms.Padding(4);
			this.SetShortcut(this.toolStripButtonSave, System.Windows.Forms.Keys.None);
			this.toolStripButtonSave.Size = new System.Drawing.Size(53, 67);
			this.toolStripButtonSave.Text = "xxSave";
			this.toolStripButtonSave.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			this.toolStripButtonSave.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonSave, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonSave, false);
			this.toolStripButtonSave.Click += new System.EventHandler(this.toolStripButtonSaveClick);
			// 
			// toolStripRefresh
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripRefresh, "");
			this.toolStripRefresh.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripRefresh.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(59)))), ((int)(((byte)(59)))));
			this.toolStripRefresh.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripRefresh.Image = null;
			this.toolStripRefresh.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonRefresh});
			this.toolStripRefresh.Location = new System.Drawing.Point(62, 1);
			this.toolStripRefresh.Name = "toolStripRefresh";
			this.toolStripRefresh.Office12Mode = false;
			this.toolStripRefresh.ShowCaption = true;
			this.toolStripRefresh.ShowLauncher = false;
			this.toolStripRefresh.Size = new System.Drawing.Size(75, 83);
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
			this.toolStripButtonRefresh.Size = new System.Drawing.Size(68, 67);
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
			this.tsClipboard.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(59)))), ((int)(((byte)(59)))));
			this.tsClipboard.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.tsClipboard.Image = null;
			this.tsClipboard.Location = new System.Drawing.Point(139, 1);
			this.tsClipboard.Name = "tsClipboard";
			this.tsClipboard.Office12Mode = false;
			this.tsClipboard.ShowLauncher = false;
			this.tsClipboard.Size = new System.Drawing.Size(106, 83);
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
			this.tcEdit.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(59)))), ((int)(((byte)(59)))));
			this.tcEdit.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.tcEdit.Image = null;
			this.tcEdit.Location = new System.Drawing.Point(247, 1);
			this.tcEdit.Name = "tcEdit";
			this.tcEdit.Office12Mode = false;
			this.tcEdit.ShowLauncher = false;
			this.tcEdit.Size = new System.Drawing.Size(106, 83);
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
			this.tcShiftBags.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(59)))), ((int)(((byte)(59)))));
			this.tcShiftBags.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.tcShiftBags.Image = null;
			this.tcShiftBags.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonAddRuleSet});
			this.tcShiftBags.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
			this.tcShiftBags.Location = new System.Drawing.Point(355, 1);
			this.tcShiftBags.Name = "tcShiftBags";
			this.tcShiftBags.Office12Mode = false;
			this.tcShiftBags.Padding = new System.Windows.Forms.Padding(0);
			this.tcShiftBags.ShowLauncher = false;
			this.tcShiftBags.Size = new System.Drawing.Size(114, 83);
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
			this.toolStripButtonAddRuleSet.Size = new System.Drawing.Size(108, 67);
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
			this.tcRename.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(59)))), ((int)(((byte)(59)))));
			this.tcRename.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.tcRename.Image = null;
			this.tcRename.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonRename});
			this.tcRename.Location = new System.Drawing.Point(471, 1);
			this.tcRename.Name = "tcRename";
			this.tcRename.Office12Mode = false;
			this.tcRename.Padding = new System.Windows.Forms.Padding(0);
			this.tcRename.ShowLauncher = false;
			this.tcRename.Size = new System.Drawing.Size(70, 83);
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
			this.toolStripButtonRename.Size = new System.Drawing.Size(64, 67);
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
			this.tcViews.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(59)))), ((int)(((byte)(59)))));
			this.tcViews.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.tcViews.Image = null;
			this.tcViews.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonGeneral,
            this.toolStripButtonCombined,
            this.toolStripButtonLimitations,
            this.toolStripButtonDateExclusion,
            this.toolStripButtonWeekdayExclusion});
			this.tcViews.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
			this.tcViews.Location = new System.Drawing.Point(543, 1);
			this.tcViews.Name = "tcViews";
			this.tcViews.Office12Mode = false;
			this.tcViews.ShowLauncher = false;
			this.tcViews.Size = new System.Drawing.Size(477, 83);
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
			this.toolStripButtonGeneral.Size = new System.Drawing.Size(61, 67);
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
			this.toolStripButtonCombined.Size = new System.Drawing.Size(99, 67);
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
			this.toolStripButtonLimitations.Size = new System.Drawing.Size(80, 67);
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
			this.toolStripButtonDateExclusion.Size = new System.Drawing.Size(103, 67);
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
			this.toolStripButtonWeekdayExclusion.Size = new System.Drawing.Size(127, 67);
			this.toolStripButtonWeekdayExclusion.Text = "xxAvailabilityWeekday";
			this.toolStripButtonWeekdayExclusion.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			this.toolStripButtonWeekdayExclusion.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonWeekdayExclusion, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonWeekdayExclusion, false);
			this.toolStripButtonWeekdayExclusion.Click += new System.EventHandler(this.toolStripButtonWeekdayExclusionClick);
			// 
			// statusStripEx1
			// 
			this.statusStripEx1.BeforeTouchSize = new System.Drawing.Size(1202, 22);
			this.statusStripEx1.Location = new System.Drawing.Point(1, 720);
			this.statusStripEx1.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(135)))), ((int)(((byte)(206)))), ((int)(((byte)(255)))));
			this.statusStripEx1.Name = "statusStripEx1";
			this.statusStripEx1.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
			this.statusStripEx1.Size = new System.Drawing.Size(1202, 22);
			this.statusStripEx1.TabIndex = 5;
			this.statusStripEx1.Text = "statusStripEx1";
			// 
			// WorkShiftsExplorer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Borders = new System.Windows.Forms.Padding(0);
			this.ClientSize = new System.Drawing.Size(1206, 743);
			this.ColorScheme = Syncfusion.Windows.Forms.Tools.RibbonForm.ColorSchemeType.Silver;
			this.Controls.Add(this.backStage1);
			this.Controls.Add(this.splitContainerAdvVertical);
			this.Controls.Add(this.ribbonControlAdv1);
			this.Controls.Add(this.statusStripEx1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimumSize = new System.Drawing.Size(388, 160);
			this.Name = "WorkShiftsExplorer";
			this.Padding = new System.Windows.Forms.Padding(2);
			this.Text = "xxTeleoptiRaptorColonShifts";
			this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.workShiftsExplorerFormClosing);
			this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.shiftCreatorKeyUp);
			((System.ComponentModel.ISupportInitialize)(this.backStage1)).EndInit();
			this.backStage1.ResumeLayout(false);
			this.backStage1.PerformLayout();
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
			this.toolStripExFile.ResumeLayout(false);
			this.toolStripExFile.PerformLayout();
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

		private Syncfusion.Windows.Forms.BackStageView backStageView1;
		private Syncfusion.Windows.Forms.BackStage backStage1;
		private Syncfusion.Windows.Forms.BackStageButton backStageButton1;
		private Syncfusion.Windows.Forms.BackStageSeparator backStageSeparator1;
		private Syncfusion.Windows.Forms.BackStageButton backStageButton2;
		private Syncfusion.Windows.Forms.BackStageSeparator backStageSeparator2;
		private Syncfusion.Windows.Forms.BackStageButton backStageButton3;
		private Syncfusion.Windows.Forms.BackStageButton backStageButton4;
		private Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed ribbonControlAdv1;
		private Syncfusion.Windows.Forms.Tools.SplitContainerAdv splitContainerAdvVertical;
		private Syncfusion.Windows.Forms.Tools.SplitContainerAdv splitContainerAdvHorizontal;
		private Syncfusion.Windows.Forms.Tools.ToolStripTabItem toolStripTabHome;
		private Syncfusion.Windows.Forms.Tools.ToolStripEx tcViews;
		private System.Windows.Forms.ToolStripButton toolStripButtonGeneral;
		private System.Windows.Forms.ToolStripButton toolStripButtonLimitations;
		private System.Windows.Forms.ToolStripButton toolStripButtonRefresh;
		private System.Windows.Forms.ToolStripButton toolStripButtonDateExclusion;
		private System.Windows.Forms.ToolStripButton toolStripButtonWeekdayExclusion;
		private Syncfusion.Windows.Forms.Tools.ToolStripEx tcRename;
		private System.Windows.Forms.ToolStripButton toolStripButtonCombined;
		private Syncfusion.Windows.Forms.Tools.ToolStripEx tcShiftBags;
		private Syncfusion.Windows.Forms.Tools.ToolStripEx tcEdit;
		private ToolStripButton toolStripButtonRename;
		private ImageList imageList1;
		private Syncfusion.Windows.Forms.Tools.ToolStripEx tsClipboard;
		private ToolStripButton toolStripButtonAddRuleSet;
		private Syncfusion.Windows.Forms.Tools.StatusStripEx statusStripEx1;
		private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripRefresh;
		private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExFile;
		private ToolStripButton toolStripButtonSave;
	}
}
