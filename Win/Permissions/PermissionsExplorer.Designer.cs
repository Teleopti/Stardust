

namespace Teleopti.Ccc.Win.Permissions
{
    partial class PermissionsExplorer
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
            PermissionsExplorerStateHolder.Dispose();
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
            Syncfusion.Windows.Forms.Tools.TreeNodeAdvStyleInfo treeNodeAdvStyleInfo1 = new Syncfusion.Windows.Forms.Tools.TreeNodeAdvStyleInfo();
            Syncfusion.Windows.Forms.Tools.TreeNodeAdvStyleInfo treeNodeAdvStyleInfo2 = new Syncfusion.Windows.Forms.Tools.TreeNodeAdvStyleInfo();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PermissionsExplorer));
            this.ExplorerRibbon = new Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed();
            this.HomeTab = new Syncfusion.Windows.Forms.Tools.ToolStripTabItem();
            this.toolStripExClipboard = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
            this.toolStripExRoles = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
            this.toolStripExRename = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
            this.toolStripButtonRenameRole = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonSave = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonNew = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonCloseExit = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonExitSystem = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonSystemOptions = new System.Windows.Forms.ToolStripButton();
            this.HorizontalSplitter = new Syncfusion.Windows.Forms.Tools.SplitContainerAdv();
            this.RolesBar = new Syncfusion.Windows.Forms.Tools.GroupBar();
            this.listViewRoles = new System.Windows.Forms.ListView();
            this.contextMenuStripClipboard = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.RolesBarItem = new Syncfusion.Windows.Forms.Tools.GroupBarItem();
            this.VerticalLeftSplitter = new Syncfusion.Windows.Forms.Tools.SplitContainerAdv();
            this.PeopleAssignedBar = new Syncfusion.Windows.Forms.Tools.GroupBar();
            this.listViewPeople = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.PeopleBarItem = new Syncfusion.Windows.Forms.Tools.GroupBarItem();
            this.VerticalRightSplitter = new Syncfusion.Windows.Forms.Tools.SplitContainerAdv();
            this.FunctionsAssignedBar = new Syncfusion.Windows.Forms.Tools.GroupBar();
            this.treeViewFunctions = new Syncfusion.Windows.Forms.Tools.TreeViewAdv();
            this.FunctionsBarItem = new Syncfusion.Windows.Forms.Tools.GroupBarItem();
            this.DataAssignedBar = new Syncfusion.Windows.Forms.Tools.GroupBar();
            this.treeViewData = new Syncfusion.Windows.Forms.Tools.TreeViewAdv();
            this.DataBarItem = new Syncfusion.Windows.Forms.Tools.GroupBarItem();
            this.toolStripTabItem5 = new Syncfusion.Windows.Forms.Tools.ToolStripTabItem();
            this.statusStripEx1 = new Syncfusion.Windows.Forms.Tools.StatusStripEx();
            ((System.ComponentModel.ISupportInitialize)(this.ExplorerRibbon)).BeginInit();
            this.ExplorerRibbon.SuspendLayout();
            this.HomeTab.Panel.SuspendLayout();
            this.toolStripExRename.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.HorizontalSplitter)).BeginInit();
            this.HorizontalSplitter.Panel1.SuspendLayout();
            this.HorizontalSplitter.Panel2.SuspendLayout();
            this.HorizontalSplitter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RolesBar)).BeginInit();
            this.RolesBar.SuspendLayout();
            this.contextMenuStripClipboard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.VerticalLeftSplitter)).BeginInit();
            this.VerticalLeftSplitter.Panel1.SuspendLayout();
            this.VerticalLeftSplitter.Panel2.SuspendLayout();
            this.VerticalLeftSplitter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PeopleAssignedBar)).BeginInit();
            this.PeopleAssignedBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.VerticalRightSplitter)).BeginInit();
            this.VerticalRightSplitter.Panel1.SuspendLayout();
            this.VerticalRightSplitter.Panel2.SuspendLayout();
            this.VerticalRightSplitter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.FunctionsAssignedBar)).BeginInit();
            this.FunctionsAssignedBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.treeViewFunctions)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DataAssignedBar)).BeginInit();
            this.DataAssignedBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.treeViewData)).BeginInit();
            this.SuspendLayout();
            // 
            // ExplorerRibbon
            // 
            this.ExplorerRibbon.CaptionFont = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ExplorerRibbon.Header.AddMainItem(HomeTab);
            this.ExplorerRibbon.Header.AddQuickItem(new Syncfusion.Windows.Forms.Tools.QuickButtonReflectable(toolStripButtonSave));
            this.ExplorerRibbon.Location = new System.Drawing.Point(1, 0);
            this.ExplorerRibbon.MenuButtonImage = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Menu;
            this.ExplorerRibbon.Name = "ExplorerRibbon";
            // 
            // ExplorerRibbon.OfficeMenu
            // 
            this.ExplorerRibbon.OfficeMenu.AuxPanel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ExplorerRibbon.OfficeMenu.AuxPanel.MinimumSize = new System.Drawing.Size(200, 0);
            this.ExplorerRibbon.OfficeMenu.MainPanel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ExplorerRibbon.OfficeMenu.MainPanel.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonNew,
            this.toolStripSeparator2,
            this.toolStripButtonSave,
            this.toolStripSeparator1,
            this.toolStripButtonCloseExit});
            this.ExplorerRibbon.OfficeMenu.Name = "OfficeMenu";
            this.ExplorerRibbon.OfficeMenu.Size = new System.Drawing.Size(304, 194);
            this.ExplorerRibbon.OfficeMenu.SystemPanel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ExplorerRibbon.OfficeMenu.SystemPanel.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonExitSystem,
            this.toolStripButtonSystemOptions});
            this.ExplorerRibbon.Size = new System.Drawing.Size(912, 152);
            this.ExplorerRibbon.SystemText.QuickAccessDialogDropDownName = "xxStartMenu";
            toolStripTabGroup1.Color = System.Drawing.Color.MediumSlateBlue;
            toolStripTabGroup1.Name = "Options";
            toolStripTabGroup1.Visible = true;
            this.ExplorerRibbon.TabGroups.Add(toolStripTabGroup1);
            this.ExplorerRibbon.TabIndex = 0;
            this.ExplorerRibbon.TitleAlignment = Syncfusion.Windows.Forms.Tools.TextAlignment.Center;
            // 
            // HomeTab
            // 
            this.ExplorerRibbon.SetDescription(this.HomeTab, "");
            this.HomeTab.Name = "HomeTab";
            // 
            // ExplorerRibbon.ribbonPanel1
            // 
            this.HomeTab.Panel.Controls.Add(this.toolStripExClipboard);
            this.HomeTab.Panel.Controls.Add(this.toolStripExRoles);
            this.HomeTab.Panel.Controls.Add(this.toolStripExRename);
            this.HomeTab.Panel.Name = "ribbonPanel1";
            this.HomeTab.Panel.ScrollPosition = 0;
            this.HomeTab.Panel.TabIndex = 2;
            this.HomeTab.Panel.Text = "xxHome";
            this.HomeTab.Position = 0;
            this.HomeTab.Size = new System.Drawing.Size(51, 19);
            this.HomeTab.Text = "xxHome";
            this.ExplorerRibbon.SetUseInCustomQuickAccessDialog(this.HomeTab, true);
            this.ExplorerRibbon.SetUseInQuickAccessMenu(this.HomeTab, false);
            // 
            // toolStripExClipboard
            // 
            this.toolStripExClipboard.AutoSize = false;
            this.ExplorerRibbon.SetDescription(this.toolStripExClipboard, "");
            this.toolStripExClipboard.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripExClipboard.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripExClipboard.ForeColor = System.Drawing.Color.MidnightBlue;
            this.toolStripExClipboard.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripExClipboard.Image = null;
            this.toolStripExClipboard.Location = new System.Drawing.Point(0, 1);
            this.toolStripExClipboard.Name = "toolStripExClipboard";
            this.toolStripExClipboard.ShowItemToolTips = true;
            this.toolStripExClipboard.ShowLauncher = false;
            this.toolStripExClipboard.Size = new System.Drawing.Size(95, 90);
            this.toolStripExClipboard.TabIndex = 13;
            this.toolStripExClipboard.Text = "xxClipboard";
            this.ExplorerRibbon.SetUseInCustomQuickAccessDialog(this.toolStripExClipboard, true);
            this.ExplorerRibbon.SetUseInQuickAccessMenu(this.toolStripExClipboard, false);
            // 
            // toolStripExRoles
            // 
            this.toolStripExRoles.AutoSize = false;
            this.ExplorerRibbon.SetDescription(this.toolStripExRoles, "");
            this.toolStripExRoles.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripExRoles.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripExRoles.ForeColor = System.Drawing.Color.MidnightBlue;
            this.toolStripExRoles.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripExRoles.Image = null;
            this.toolStripExRoles.Location = new System.Drawing.Point(97, 1);
            this.toolStripExRoles.Name = "toolStripExRoles";
            this.toolStripExRoles.ShowLauncher = false;
            this.toolStripExRoles.Size = new System.Drawing.Size(95, 90);
            this.toolStripExRoles.TabIndex = 11;
            this.toolStripExRoles.Text = "xxRoles";
            this.ExplorerRibbon.SetUseInCustomQuickAccessDialog(this.toolStripExRoles, true);
            this.ExplorerRibbon.SetUseInQuickAccessMenu(this.toolStripExRoles, false);
            // 
            // toolStripExRename
            // 
            this.toolStripExRename.AutoSize = false;
            this.ExplorerRibbon.SetDescription(this.toolStripExRename, "");
            this.toolStripExRename.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripExRename.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripExRename.ForeColor = System.Drawing.Color.MidnightBlue;
            this.toolStripExRename.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripExRename.Image = null;
            this.toolStripExRename.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonRenameRole});
            this.toolStripExRename.Location = new System.Drawing.Point(194, 1);
            this.toolStripExRename.Name = "toolStripExRename";
            this.toolStripExRename.ShowLauncher = false;
            this.toolStripExRename.Size = new System.Drawing.Size(105, 90);
            this.toolStripExRename.TabIndex = 10;
            this.toolStripExRename.Text = "xxRename";
            this.ExplorerRibbon.SetUseInCustomQuickAccessDialog(this.toolStripExRename, true);
            this.ExplorerRibbon.SetUseInQuickAccessMenu(this.toolStripExRename, false);
            // 
            // toolStripButtonRenameRole
            // 
            this.ExplorerRibbon.SetDescription(this.toolStripButtonRenameRole, "");
            this.toolStripButtonRenameRole.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_User;
            this.toolStripButtonRenameRole.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonRenameRole.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonRenameRole.Name = "toolStripButtonRenameRole";
            this.toolStripButtonRenameRole.Size = new System.Drawing.Size(83, 70);
            this.toolStripButtonRenameRole.Text = "xxRenameRole";
            this.toolStripButtonRenameRole.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.ExplorerRibbon.SetUseInCustomQuickAccessDialog(this.toolStripButtonRenameRole, true);
            this.ExplorerRibbon.SetUseInQuickAccessMenu(this.toolStripButtonRenameRole, false);
            this.toolStripButtonRenameRole.Click += new System.EventHandler(this.toolStripButtonRenameRoleClick);
            // 
            // toolStripButtonSave
            // 
            this.toolStripButtonSave.AutoToolTip = false;
            this.ExplorerRibbon.SetDescription(this.toolStripButtonSave, "");
            this.toolStripButtonSave.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Save;
            this.toolStripButtonSave.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolStripButtonSave.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonSave.Name = "toolStripButtonSave";
            this.toolStripButtonSave.Padding = new System.Windows.Forms.Padding(4);
            this.toolStripButtonSave.Size = new System.Drawing.Size(90, 44);
            this.toolStripButtonSave.Text = "xxSave";
            this.toolStripButtonSave.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ExplorerRibbon.SetUseInCustomQuickAccessDialog(this.toolStripButtonSave, true);
            this.ExplorerRibbon.SetUseInQuickAccessMenu(this.toolStripButtonSave, true);
            this.toolStripButtonSave.Click += new System.EventHandler(this.toolStripButtonSaveClick);
            // 
            // toolStripButtonNew
            // 
            this.toolStripButtonNew.AutoToolTip = false;
            this.ExplorerRibbon.SetDescription(this.toolStripButtonNew, "");
            this.toolStripButtonNew.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_New2;
            this.toolStripButtonNew.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolStripButtonNew.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonNew.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonNew.Name = "toolStripButtonNew";
            this.toolStripButtonNew.Padding = new System.Windows.Forms.Padding(4);
            this.toolStripButtonNew.Size = new System.Drawing.Size(90, 44);
            this.toolStripButtonNew.Text = "xxNew";
            this.toolStripButtonNew.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ExplorerRibbon.SetUseInCustomQuickAccessDialog(this.toolStripButtonNew, true);
            this.ExplorerRibbon.SetUseInQuickAccessMenu(this.toolStripButtonNew, false);
            this.toolStripButtonNew.Click += new System.EventHandler(this.toolStripButtonNewClick);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(74, 2);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(74, 2);
            // 
            // toolStripButtonCloseExit
            // 
            this.toolStripButtonCloseExit.AutoToolTip = false;
            this.ExplorerRibbon.SetDescription(this.toolStripButtonCloseExit, "");
            this.toolStripButtonCloseExit.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Close;
            this.toolStripButtonCloseExit.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolStripButtonCloseExit.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonCloseExit.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonCloseExit.Name = "toolStripButtonCloseExit";
            this.toolStripButtonCloseExit.Padding = new System.Windows.Forms.Padding(4);
            this.toolStripButtonCloseExit.Size = new System.Drawing.Size(90, 44);
            this.toolStripButtonCloseExit.Text = "xxClose";
            this.toolStripButtonCloseExit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ExplorerRibbon.SetUseInCustomQuickAccessDialog(this.toolStripButtonCloseExit, true);
            this.ExplorerRibbon.SetUseInQuickAccessMenu(this.toolStripButtonCloseExit, false);
            this.toolStripButtonCloseExit.Click += new System.EventHandler(this.toolStripButtonCloseExitClick);
            // 
            // toolStripButtonExitSystem
            // 
            this.toolStripButtonExitSystem.AutoToolTip = false;
            this.ExplorerRibbon.SetDescription(this.toolStripButtonExitSystem, "");
            this.toolStripButtonExitSystem.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Exit;
            this.toolStripButtonExitSystem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonExitSystem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonExitSystem.Name = "toolStripButtonExitSystem";
            this.toolStripButtonExitSystem.Size = new System.Drawing.Size(130, 20);
            this.toolStripButtonExitSystem.Text = "xxExitTELEOPTICCC";
            this.ExplorerRibbon.SetUseInCustomQuickAccessDialog(this.toolStripButtonExitSystem, true);
            this.ExplorerRibbon.SetUseInQuickAccessMenu(this.toolStripButtonExitSystem, false);
            this.toolStripButtonExitSystem.Click += new System.EventHandler(this.toolStripButtonExitSystemClick);
            // 
            // toolStripButtonSystemOptions
            // 
            this.ExplorerRibbon.SetDescription(this.toolStripButtonSystemOptions, "");
            this.toolStripButtonSystemOptions.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Settings;
            this.toolStripButtonSystemOptions.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonSystemOptions.Name = "toolStripButtonSystemOptions";
            this.toolStripButtonSystemOptions.Size = new System.Drawing.Size(79, 20);
            this.toolStripButtonSystemOptions.Text = "xxOptions";
            this.ExplorerRibbon.SetUseInCustomQuickAccessDialog(this.toolStripButtonSystemOptions, true);
            this.ExplorerRibbon.SetUseInQuickAccessMenu(this.toolStripButtonSystemOptions, false);
            this.toolStripButtonSystemOptions.Click += new System.EventHandler(toolStripButtonSystemOptionsClick);
            // 
            // HorizontalSplitter
            // 
            this.HorizontalSplitter.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Horizontal, System.Drawing.Color.FromArgb(((int)(((byte)(227)))), ((int)(((byte)(239)))), ((int)(((byte)(255))))), System.Drawing.Color.FromArgb(((int)(((byte)(101)))), ((int)(((byte)(147)))), ((int)(((byte)(207))))));
            this.HorizontalSplitter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.HorizontalSplitter.HotBackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Horizontal, System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(151)))), ((int)(((byte)(61))))), System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(184)))), ((int)(((byte)(94))))));
            this.HorizontalSplitter.Location = new System.Drawing.Point(6, 153);
            this.HorizontalSplitter.Name = "HorizontalSplitter";
            this.HorizontalSplitter.Orientation = System.Windows.Forms.Orientation.Vertical;
            // 
            // HorizontalSplitter.Panel1
            // 
            this.HorizontalSplitter.Panel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252))))), System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255))))));
            this.HorizontalSplitter.Panel1.Controls.Add(this.RolesBar);
            this.HorizontalSplitter.Panel1MinSize = 0;
            // 
            // HorizontalSplitter.Panel2
            // 
            this.HorizontalSplitter.Panel2.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252))))), System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255))))));
            this.HorizontalSplitter.Panel2.Controls.Add(this.VerticalLeftSplitter);
            this.HorizontalSplitter.Panel2MinSize = 0;
            this.HorizontalSplitter.PanelToBeCollapsed = Syncfusion.Windows.Forms.Tools.Enums.CollapsedPanel.Panel1;
            this.HorizontalSplitter.Size = new System.Drawing.Size(902, 438);
            this.HorizontalSplitter.SplitterDistance = 160;
            this.HorizontalSplitter.Style = Syncfusion.Windows.Forms.Tools.Enums.Style.Office2007Blue;
            this.HorizontalSplitter.TabIndex = 1;
            this.HorizontalSplitter.Text = "yysplitContainerAdv1";
            // 
            // RolesBar
            // 
            this.RolesBar.AllowDrop = true;
            this.RolesBar.AnimatedSelection = false;
            this.RolesBar.BarHighlight = false;
            this.RolesBar.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.RolesBar.Controls.Add(this.listViewRoles);
            this.RolesBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RolesBar.FlatLook = true;
            this.RolesBar.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.RolesBar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(77)))), ((int)(((byte)(140)))));
            this.RolesBar.GroupBarItems.AddRange(new Syncfusion.Windows.Forms.Tools.GroupBarItem[] {
            this.RolesBarItem});
            this.RolesBar.Location = new System.Drawing.Point(0, 0);
            this.RolesBar.Name = "RolesBar";
            this.RolesBar.PopupClientSize = new System.Drawing.Size(0, 0);
            this.RolesBar.SelectedItem = 0;
            this.RolesBar.Size = new System.Drawing.Size(902, 160);
            this.RolesBar.TabIndex = 0;
            this.RolesBar.TextAlign = Syncfusion.Windows.Forms.Tools.TextAlignment.Left;
            this.RolesBar.VisualStyle = Syncfusion.Windows.Forms.VisualStyle.VS2005;
            // 
            // listViewRoles
            // 
            this.listViewRoles.BackColor = System.Drawing.Color.White;
            this.listViewRoles.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listViewRoles.ContextMenuStrip = this.contextMenuStripClipboard;
            this.listViewRoles.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listViewRoles.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(77)))), ((int)(((byte)(140)))));
            this.listViewRoles.FullRowSelect = true;
            this.listViewRoles.HideSelection = false;
            this.listViewRoles.LabelEdit = true;
            this.listViewRoles.Location = new System.Drawing.Point(0, 22);
            this.listViewRoles.Name = "listViewRoles";
            this.listViewRoles.Size = new System.Drawing.Size(902, 138);
            this.listViewRoles.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.listViewRoles.TabIndex = 0;
            this.listViewRoles.UseCompatibleStateImageBehavior = false;
            this.listViewRoles.View = System.Windows.Forms.View.List;
            // 
            // contextMenuStripClipboard
            // 
            this.contextMenuStripClipboard.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem});
            this.contextMenuStripClipboard.Name = "contextMenuStripClipboard";
            this.contextMenuStripClipboard.Size = new System.Drawing.Size(140, 48);
            this.contextMenuStripClipboard.Opening += new System.ComponentModel.CancelEventHandler(contextMenuStripClipboardOpening);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(139, 22);
            this.copyToolStripMenuItem.Text = "Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItemClick);
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.pasteToolStripMenuItem.Size = new System.Drawing.Size(139, 22);
            this.pasteToolStripMenuItem.Text = "Paste";
            this.pasteToolStripMenuItem.Click += new System.EventHandler(this.pasteToolStripMenuItemClick);
            // 
            // RolesBarItem
            // 
            this.RolesBarItem.Client = this.listViewRoles;
            this.RolesBarItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(77)))), ((int)(((byte)(140)))));
            this.RolesBarItem.Text = "xxNoRolesAvailable";
            // 
            // VerticalLeftSplitter
            // 
            this.VerticalLeftSplitter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.VerticalLeftSplitter.Location = new System.Drawing.Point(0, 0);
            this.VerticalLeftSplitter.Name = "VerticalLeftSplitter";
            // 
            // VerticalLeftSplitter.Panel1
            // 
            this.VerticalLeftSplitter.Panel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252))))), System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255))))));
            this.VerticalLeftSplitter.Panel1.Controls.Add(this.PeopleAssignedBar);
            this.VerticalLeftSplitter.Panel1MinSize = 0;
            // 
            // VerticalLeftSplitter.Panel2
            // 
            this.VerticalLeftSplitter.Panel2.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252))))), System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255))))));
            this.VerticalLeftSplitter.Panel2.Controls.Add(this.VerticalRightSplitter);
            this.VerticalLeftSplitter.Panel2MinSize = 0;
            this.VerticalLeftSplitter.PanelToBeCollapsed = Syncfusion.Windows.Forms.Tools.Enums.CollapsedPanel.Panel2;
            this.VerticalLeftSplitter.Size = new System.Drawing.Size(902, 271);
            this.VerticalLeftSplitter.SplitterDistance = 350;
            this.VerticalLeftSplitter.Style = Syncfusion.Windows.Forms.Tools.Enums.Style.Office2007Blue;
            this.VerticalLeftSplitter.TabIndex = 0;
            this.VerticalLeftSplitter.Text = "yysplitContainerAdv3";
            // 
            // PeopleAssignedBar
            // 
            this.PeopleAssignedBar.AllowDrop = true;
            this.PeopleAssignedBar.AnimatedSelection = false;
            this.PeopleAssignedBar.BackColor = System.Drawing.SystemColors.Control;
            this.PeopleAssignedBar.BarHighlight = false;
            this.PeopleAssignedBar.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.PeopleAssignedBar.Controls.Add(this.listViewPeople);
            this.PeopleAssignedBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PeopleAssignedBar.FlatLook = true;
            this.PeopleAssignedBar.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.PeopleAssignedBar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(77)))), ((int)(((byte)(140)))));
            this.PeopleAssignedBar.GroupBarItems.AddRange(new Syncfusion.Windows.Forms.Tools.GroupBarItem[] {
            this.PeopleBarItem});
            this.PeopleAssignedBar.Location = new System.Drawing.Point(0, 0);
            this.PeopleAssignedBar.Name = "PeopleAssignedBar";
            this.PeopleAssignedBar.PopupClientSize = new System.Drawing.Size(0, 0);
            this.PeopleAssignedBar.SelectedItem = 0;
            this.PeopleAssignedBar.Size = new System.Drawing.Size(350, 271);
            this.PeopleAssignedBar.TabIndex = 0;
            this.PeopleAssignedBar.Text = "yygroupBar3";
            this.PeopleAssignedBar.TextAlign = Syncfusion.Windows.Forms.Tools.TextAlignment.Left;
            this.PeopleAssignedBar.VisualStyle = Syncfusion.Windows.Forms.VisualStyle.VS2005;
            // 
            // listViewPeople
            // 
            this.listViewPeople.BackColor = System.Drawing.Color.White;
            this.listViewPeople.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listViewPeople.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.listViewPeople.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listViewPeople.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(77)))), ((int)(((byte)(140)))));
            this.listViewPeople.FullRowSelect = true;
            this.listViewPeople.LabelWrap = false;
            this.listViewPeople.Location = new System.Drawing.Point(0, 22);
            this.listViewPeople.Name = "listViewPeople";
            this.listViewPeople.ShowGroups = false;
            this.listViewPeople.Size = new System.Drawing.Size(350, 249);
            this.listViewPeople.TabIndex = 0;
            this.listViewPeople.UseCompatibleStateImageBehavior = false;
            this.listViewPeople.View = System.Windows.Forms.View.Details;
            this.listViewPeople.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listViewPeopleColumnClick);
            this.listViewPeople.KeyUp += new System.Windows.Forms.KeyEventHandler(this.listViewPeopleKeyUp);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "xxFirstName";
            this.columnHeader1.Width = 137;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "xxLastName";
            this.columnHeader2.Width = 114;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "xxTeam";
            this.columnHeader3.Width = 111;
            // 
            // PeopleBarItem
            // 
            this.PeopleBarItem.Client = this.listViewPeople;
            this.PeopleBarItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(77)))), ((int)(((byte)(140)))));
            this.PeopleBarItem.Text = "xxNoPeopleAssigned";
            // 
            // VerticalRightSplitter
            // 
            this.VerticalRightSplitter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.VerticalRightSplitter.Location = new System.Drawing.Point(0, 0);
            this.VerticalRightSplitter.Name = "VerticalRightSplitter";
            // 
            // VerticalRightSplitter.Panel1
            // 
            this.VerticalRightSplitter.Panel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252))))), System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255))))));
            this.VerticalRightSplitter.Panel1.Controls.Add(this.FunctionsAssignedBar);
            this.VerticalRightSplitter.Panel1MinSize = 0;
            // 
            // VerticalRightSplitter.Panel2
            // 
            this.VerticalRightSplitter.Panel2.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252))))), System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255))))));
            this.VerticalRightSplitter.Panel2.Controls.Add(this.DataAssignedBar);
            this.VerticalRightSplitter.Panel2MinSize = 0;
            this.VerticalRightSplitter.PanelToBeCollapsed = Syncfusion.Windows.Forms.Tools.Enums.CollapsedPanel.Panel2;
            this.VerticalRightSplitter.Size = new System.Drawing.Size(545, 271);
            this.VerticalRightSplitter.SplitterDistance = 280;
            this.VerticalRightSplitter.Style = Syncfusion.Windows.Forms.Tools.Enums.Style.Office2007Blue;
            this.VerticalRightSplitter.TabIndex = 0;
            this.VerticalRightSplitter.Text = "yysplitContainerAdv2";
            // 
            // FunctionsAssignedBar
            // 
            this.FunctionsAssignedBar.AllowDrop = true;
            this.FunctionsAssignedBar.AnimatedSelection = false;
            this.FunctionsAssignedBar.BarHighlight = false;
            this.FunctionsAssignedBar.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.FunctionsAssignedBar.Controls.Add(this.treeViewFunctions);
            this.FunctionsAssignedBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FunctionsAssignedBar.FlatLook = true;
            this.FunctionsAssignedBar.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.FunctionsAssignedBar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(77)))), ((int)(((byte)(140)))));
            this.FunctionsAssignedBar.GroupBarItems.AddRange(new Syncfusion.Windows.Forms.Tools.GroupBarItem[] {
            this.FunctionsBarItem});
            this.FunctionsAssignedBar.Location = new System.Drawing.Point(0, 0);
            this.FunctionsAssignedBar.Name = "FunctionsAssignedBar";
            this.FunctionsAssignedBar.PopupClientSize = new System.Drawing.Size(0, 0);
            this.FunctionsAssignedBar.SelectedItem = 0;
            this.FunctionsAssignedBar.Size = new System.Drawing.Size(280, 271);
            this.FunctionsAssignedBar.TabIndex = 0;
            this.FunctionsAssignedBar.Text = "yygroupBar2";
            this.FunctionsAssignedBar.TextAlign = Syncfusion.Windows.Forms.Tools.TextAlignment.Left;
            this.FunctionsAssignedBar.VisualStyle = Syncfusion.Windows.Forms.VisualStyle.VS2005;
            // 
            // treeViewFunctions
            // 
            this.treeViewFunctions.AccelerateScrolling = Syncfusion.Windows.Forms.AccelerateScrollingBehavior.Immediate;
            this.treeViewFunctions.BackColor = System.Drawing.Color.White;
            treeNodeAdvStyleInfo1.EnsureDefaultOptionedChild = true;
            this.treeViewFunctions.BaseStylePairs.AddRange(new Syncfusion.Windows.Forms.Tools.StyleNamePair[] {
            new Syncfusion.Windows.Forms.Tools.StyleNamePair("Standard", treeNodeAdvStyleInfo1)});
            this.treeViewFunctions.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.treeViewFunctions.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            // 
            // 
            // 
            this.treeViewFunctions.HelpTextControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.treeViewFunctions.HelpTextControl.Location = new System.Drawing.Point(0, 0);
            this.treeViewFunctions.HelpTextControl.Name = "helpText";
            this.treeViewFunctions.HelpTextControl.Size = new System.Drawing.Size(62, 15);
            this.treeViewFunctions.HelpTextControl.TabIndex = 0;
            this.treeViewFunctions.HelpTextControl.Text = "xxHelpText";
            this.treeViewFunctions.Location = new System.Drawing.Point(0, 22);
            this.treeViewFunctions.Name = "treeViewFunctions";
            this.treeViewFunctions.SelectionMode = Syncfusion.Windows.Forms.Tools.TreeSelectionMode.MultiSelectSameLevel;
            this.treeViewFunctions.ShowCheckBoxes = true;
            this.treeViewFunctions.Size = new System.Drawing.Size(280, 249);
            this.treeViewFunctions.TabIndex = 0;
            // 
            // 
            // 
            this.treeViewFunctions.ToolTipControl.BackColor = System.Drawing.SystemColors.Info;
            this.treeViewFunctions.ToolTipControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.treeViewFunctions.ToolTipControl.Location = new System.Drawing.Point(0, 0);
            this.treeViewFunctions.ToolTipControl.Name = "toolTip";
            this.treeViewFunctions.ToolTipControl.Size = new System.Drawing.Size(55, 15);
            this.treeViewFunctions.ToolTipControl.TabIndex = 1;
            this.treeViewFunctions.ToolTipControl.Text = "xxToolTip";
            // 
            // FunctionsBarItem
            // 
            this.FunctionsBarItem.Client = this.treeViewFunctions;
            this.FunctionsBarItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(77)))), ((int)(((byte)(140)))));
            this.FunctionsBarItem.Text = "xxNoFunctionsAssigned";
            // 
            // DataAssignedBar
            // 
            this.DataAssignedBar.AllowDrop = true;
            this.DataAssignedBar.AnimatedSelection = false;
            this.DataAssignedBar.BarHighlight = false;
            this.DataAssignedBar.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.DataAssignedBar.Controls.Add(this.treeViewData);
            this.DataAssignedBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DataAssignedBar.FlatLook = true;
            this.DataAssignedBar.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.DataAssignedBar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(77)))), ((int)(((byte)(140)))));
            this.DataAssignedBar.GroupBarItems.AddRange(new Syncfusion.Windows.Forms.Tools.GroupBarItem[] {
            this.DataBarItem});
            this.DataAssignedBar.Location = new System.Drawing.Point(0, 0);
            this.DataAssignedBar.Name = "DataAssignedBar";
            this.DataAssignedBar.PopupClientSize = new System.Drawing.Size(0, 0);
            this.DataAssignedBar.SelectedItem = 0;
            this.DataAssignedBar.Size = new System.Drawing.Size(258, 271);
            this.DataAssignedBar.TabIndex = 0;
            this.DataAssignedBar.Text = "yygroupBar4";
            this.DataAssignedBar.TextAlign = Syncfusion.Windows.Forms.Tools.TextAlignment.Left;
            this.DataAssignedBar.VisualStyle = Syncfusion.Windows.Forms.VisualStyle.VS2005;
            // 
            // treeViewData
            // 
            this.treeViewData.AccelerateScrolling = Syncfusion.Windows.Forms.AccelerateScrollingBehavior.Immediate;
            this.treeViewData.BackColor = System.Drawing.Color.White;
            treeNodeAdvStyleInfo2.EnsureDefaultOptionedChild = true;
            this.treeViewData.BaseStylePairs.AddRange(new Syncfusion.Windows.Forms.Tools.StyleNamePair[] {
            new Syncfusion.Windows.Forms.Tools.StyleNamePair("Standard", treeNodeAdvStyleInfo2)});
            this.treeViewData.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.treeViewData.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            // 
            // 
            // 
            this.treeViewData.HelpTextControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.treeViewData.HelpTextControl.Location = new System.Drawing.Point(1, 23);
            this.treeViewData.HelpTextControl.Name = "helpText";
            this.treeViewData.HelpTextControl.Size = new System.Drawing.Size(62, 15);
            this.treeViewData.HelpTextControl.TabIndex = 0;
            this.treeViewData.HelpTextControl.Text = "xxHelpText";
            this.treeViewData.HelpTextControl.Visible = true;
            this.treeViewData.Location = new System.Drawing.Point(0, 22);
            this.treeViewData.Name = "treeViewData";
            this.treeViewData.Office2007ScrollBars = true;
            this.treeViewData.SelectionMode = Syncfusion.Windows.Forms.Tools.TreeSelectionMode.MultiSelectSameLevel;
            this.treeViewData.ShowCheckBoxes = true;
            this.treeViewData.Size = new System.Drawing.Size(258, 249);
            this.treeViewData.TabIndex = 0;
            // 
            // 
            // 
            this.treeViewData.ToolTipControl.BackColor = System.Drawing.SystemColors.Info;
            this.treeViewData.ToolTipControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.treeViewData.ToolTipControl.Location = new System.Drawing.Point(0, 0);
            this.treeViewData.ToolTipControl.Name = "toolTip";
            this.treeViewData.ToolTipControl.Size = new System.Drawing.Size(55, 15);
            this.treeViewData.ToolTipControl.TabIndex = 1;
            this.treeViewData.ToolTipControl.Text = "xxToolTip";
            // 
            // DataBarItem
            // 
            this.DataBarItem.Client = this.treeViewData;
            this.DataBarItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(77)))), ((int)(((byte)(140)))));
            this.DataBarItem.Text = "xxNoDataAssigned";
            // 
            // toolStripTabItem5
            // 
            this.toolStripTabItem5.Name = "toolStripTabItem5";
            // 
            // 
            // 
            this.toolStripTabItem5.Panel.Name = "";
            this.toolStripTabItem5.Panel.ScrollPosition = 0;
            this.toolStripTabItem5.Panel.TabIndex = 0;
            this.toolStripTabItem5.Panel.Text = "xxHome";
            this.toolStripTabItem5.Position = -1;
            this.toolStripTabItem5.Size = new System.Drawing.Size(40, 19);
            this.toolStripTabItem5.Text = "xxHome";
            // 
            // statusStripEx1
            // 
            this.statusStripEx1.Location = new System.Drawing.Point(6, 591);
            this.statusStripEx1.Name = "statusStripEx1";
            this.statusStripEx1.Size = new System.Drawing.Size(902, 22);
            this.statusStripEx1.TabIndex = 5;
            this.statusStripEx1.Text = "statusStripEx1";
            // 
            // PermissionsExplorer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Borders = new System.Windows.Forms.Padding(6, 1, 6, 2);
            this.ClientSize = new System.Drawing.Size(914, 615);
            this.ContextMenuStrip = this.contextMenuStripClipboard;
            this.Controls.Add(this.statusStripEx1);
            this.Controls.Add(this.HorizontalSplitter);
            this.Controls.Add(this.ExplorerRibbon);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PermissionsExplorer";
            this.Text = "xxTeleoptiRaptorColonPermissions";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.permissionsExplorerFormClosing);
            this.Load += new System.EventHandler(this.permissionsExplorerLoad);
            ((System.ComponentModel.ISupportInitialize)(this.ExplorerRibbon)).EndInit();
            this.ExplorerRibbon.ResumeLayout(false);
            this.ExplorerRibbon.PerformLayout();
            this.HomeTab.Panel.ResumeLayout(false);
            this.toolStripExRename.ResumeLayout(false);
            this.toolStripExRename.PerformLayout();
            this.HorizontalSplitter.Panel1.ResumeLayout(false);
            this.HorizontalSplitter.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.HorizontalSplitter)).EndInit();
            this.HorizontalSplitter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.RolesBar)).EndInit();
            this.RolesBar.ResumeLayout(false);
            this.contextMenuStripClipboard.ResumeLayout(false);
            this.VerticalLeftSplitter.Panel1.ResumeLayout(false);
            this.VerticalLeftSplitter.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.VerticalLeftSplitter)).EndInit();
            this.VerticalLeftSplitter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.PeopleAssignedBar)).EndInit();
            this.PeopleAssignedBar.ResumeLayout(false);
            this.VerticalRightSplitter.Panel1.ResumeLayout(false);
            this.VerticalRightSplitter.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.VerticalRightSplitter)).EndInit();
            this.VerticalRightSplitter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.FunctionsAssignedBar)).EndInit();
            this.FunctionsAssignedBar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.treeViewFunctions)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DataAssignedBar)).EndInit();
            this.DataAssignedBar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.treeViewData)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed ExplorerRibbon;
        private Syncfusion.Windows.Forms.Tools.SplitContainerAdv HorizontalSplitter;
        private Syncfusion.Windows.Forms.Tools.SplitContainerAdv VerticalRightSplitter;
        private Syncfusion.Windows.Forms.Tools.SplitContainerAdv VerticalLeftSplitter;
        private Syncfusion.Windows.Forms.Tools.ToolStripTabItem HomeTab;
        private Syncfusion.Windows.Forms.Tools.ToolStripTabItem toolStripTabItem5;
        private Syncfusion.Windows.Forms.Tools.GroupBar RolesBar;
        private Syncfusion.Windows.Forms.Tools.GroupBar FunctionsAssignedBar;
        private Syncfusion.Windows.Forms.Tools.GroupBarItem FunctionsBarItem;
        private Syncfusion.Windows.Forms.Tools.GroupBar PeopleAssignedBar;
        private Syncfusion.Windows.Forms.Tools.GroupBarItem PeopleBarItem;
        private Syncfusion.Windows.Forms.Tools.GroupBar DataAssignedBar;
        private Syncfusion.Windows.Forms.Tools.GroupBarItem DataBarItem;
        private Syncfusion.Windows.Forms.Tools.GroupBarItem RolesBarItem;
        private System.Windows.Forms.ListView listViewRoles;
        private Syncfusion.Windows.Forms.Tools.TreeViewAdv treeViewFunctions;
        private Syncfusion.Windows.Forms.Tools.TreeViewAdv treeViewData;
        private System.Windows.Forms.ListView listViewPeople;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ToolStripButton toolStripButtonSave;
        private System.Windows.Forms.ToolStripButton toolStripButtonCloseExit;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExRoles;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExRename;
        private System.Windows.Forms.ToolStripButton toolStripButtonRenameRole;
        private Syncfusion.Windows.Forms.Tools.StatusStripEx statusStripEx1;
        private System.Windows.Forms.ToolStripButton toolStripButtonExitSystem;
        private System.Windows.Forms.ToolStripButton toolStripButtonNew;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExClipboard;
        private System.Windows.Forms.ToolStripButton toolStripButtonSystemOptions;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripClipboard;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
    }
}
