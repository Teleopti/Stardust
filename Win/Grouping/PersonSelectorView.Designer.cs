﻿namespace Teleopti.Ccc.Win.Grouping
{
    partial class PersonSelectorView
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageAdvMainTabPage;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PersonSelectorView));
            this.treeViewAdvMainTabTree = new Syncfusion.Windows.Forms.Tools.TreeViewAdv();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.OpenNewPPLAdmintoolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemSearch = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.RefreshListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ImageList = new System.Windows.Forms.ImageList(this.components);
            this.xdtpDate = new Syncfusion.Windows.Forms.Tools.DateTimePickerAdv();
            this.tabControlAdv = new Syncfusion.Windows.Forms.Tools.TabControlAdv();
            tabPageAdvMainTabPage = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
            tabPageAdvMainTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.treeViewAdvMainTabTree)).BeginInit();
            this.contextMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.xdtpDate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tabControlAdv)).BeginInit();
            this.tabControlAdv.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabPageAdvMainTabPage
            // 
            tabPageAdvMainTabPage.BackColor = System.Drawing.Color.Transparent;
            tabPageAdvMainTabPage.Controls.Add(this.treeViewAdvMainTabTree);
            tabPageAdvMainTabPage.Controls.Add(this.xdtpDate);
            tabPageAdvMainTabPage.Image = null;
            tabPageAdvMainTabPage.ImageSize = new System.Drawing.Size(16, 16);
            tabPageAdvMainTabPage.Location = new System.Drawing.Point(2, 34);
            tabPageAdvMainTabPage.Name = "tabPageAdvMainTabPage";
            tabPageAdvMainTabPage.Size = new System.Drawing.Size(322, 394);
            tabPageAdvMainTabPage.TabIndex = 1;
            tabPageAdvMainTabPage.Text = "xxMain";
            tabPageAdvMainTabPage.ThemesEnabled = false;
            // 
            // treeViewAdvMainTabTree
            // 
            this.treeViewAdvMainTabTree.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.White);
            this.treeViewAdvMainTabTree.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(193)))), ((int)(((byte)(222)))));
            this.treeViewAdvMainTabTree.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.treeViewAdvMainTabTree.ContextMenuStrip = this.contextMenuStrip;
            this.treeViewAdvMainTabTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewAdvMainTabTree.GridOfficeScrollBars = Syncfusion.Windows.Forms.OfficeScrollBars.Office2007;
            // 
            // 
            // 
            this.treeViewAdvMainTabTree.HelpTextControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.treeViewAdvMainTabTree.HelpTextControl.Location = new System.Drawing.Point(0, 0);
            this.treeViewAdvMainTabTree.HelpTextControl.Name = "helpText";
            this.treeViewAdvMainTabTree.HelpTextControl.Size = new System.Drawing.Size(49, 15);
            this.treeViewAdvMainTabTree.HelpTextControl.TabIndex = 0;
            this.treeViewAdvMainTabTree.HelpTextControl.Text = "help text";
            this.treeViewAdvMainTabTree.HideSelection = false;
            this.treeViewAdvMainTabTree.InteractiveCheckBoxes = true;
            this.treeViewAdvMainTabTree.LeftImageList = this.ImageList;
            this.treeViewAdvMainTabTree.Location = new System.Drawing.Point(0, 20);
            this.treeViewAdvMainTabTree.Name = "treeViewAdvMainTabTree";
            this.treeViewAdvMainTabTree.Office2007ScrollBars = true;
            this.treeViewAdvMainTabTree.SelectionMode = Syncfusion.Windows.Forms.Tools.TreeSelectionMode.MultiSelectAll;
            this.treeViewAdvMainTabTree.Size = new System.Drawing.Size(322, 374);
            this.treeViewAdvMainTabTree.SortWithChildNodes = true;
            this.treeViewAdvMainTabTree.TabIndex = 7;
            this.treeViewAdvMainTabTree.Text = "treeViewAdv1";
            // 
            // 
            // 
            this.treeViewAdvMainTabTree.ToolTipControl.BackColor = System.Drawing.SystemColors.Info;
            this.treeViewAdvMainTabTree.ToolTipControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.treeViewAdvMainTabTree.ToolTipControl.Location = new System.Drawing.Point(0, 0);
            this.treeViewAdvMainTabTree.ToolTipControl.Name = "toolTip";
            this.treeViewAdvMainTabTree.ToolTipControl.Size = new System.Drawing.Size(41, 15);
            this.treeViewAdvMainTabTree.ToolTipControl.TabIndex = 1;
            this.treeViewAdvMainTabTree.ToolTipControl.Text = "toolTip";
            this.treeViewAdvMainTabTree.AfterSelect += new System.EventHandler(this.treeViewAdvMainTabTreeAfterSelect);
            this.treeViewAdvMainTabTree.AfterCheck += new Syncfusion.Windows.Forms.Tools.TreeNodeAdvEventHandler(this.treeViewAdvMainTabTreeAfterCheck);
            this.treeViewAdvMainTabTree.DoubleClick += new System.EventHandler(this.openModule);
            this.treeViewAdvMainTabTree.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treeViewAdvMainTabTreeKeyDown);
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.OpenNewPPLAdmintoolStripMenuItem,
            this.toolStripMenuItemSearch,
            this.toolStripSeparator3,
            this.RefreshListToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip1";
            this.contextMenuStrip.Size = new System.Drawing.Size(202, 98);
            this.contextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripOpening);
            // 
            // OpenNewPPLAdmintoolStripMenuItem
            // 
            this.OpenNewPPLAdmintoolStripMenuItem.Name = "OpenNewPPLAdmintoolStripMenuItem";
            this.OpenNewPPLAdmintoolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            this.OpenNewPPLAdmintoolStripMenuItem.Text = "xxOpen";
            this.OpenNewPPLAdmintoolStripMenuItem.Click += new System.EventHandler(this.openModule);
            // 
            // toolStripMenuItemSearch
            // 
            this.toolStripMenuItemSearch.Name = "toolStripMenuItemSearch";
            this.toolStripMenuItemSearch.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.toolStripMenuItemSearch.Size = new System.Drawing.Size(201, 22);
            this.toolStripMenuItemSearch.Text = "xxFindThreeDots";
            this.toolStripMenuItemSearch.Visible = false;
            this.toolStripMenuItemSearch.Click += new System.EventHandler(this.toolStripMenuItemSearchClick);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(198, 6);
            // 
            // RefreshListToolStripMenuItem
            // 
            this.RefreshListToolStripMenuItem.Name = "RefreshListToolStripMenuItem";
            this.RefreshListToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            this.RefreshListToolStripMenuItem.Text = "xxRefresh";
            this.RefreshListToolStripMenuItem.Click += new System.EventHandler(this.refreshListToolStripMenuItemClick);
            // 
            // ImageList
            // 
            this.ImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ImageList.ImageStream")));
            this.ImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.ImageList.Images.SetKeyName(0, "ccc_tree_BU.png");
            this.ImageList.Images.SetKeyName(1, "ccc_Site.png");
            this.ImageList.Images.SetKeyName(2, "ccc_Tree_Team.png");
            this.ImageList.Images.SetKeyName(3, "ccc_tree_Agent.png");
            this.ImageList.Images.SetKeyName(4, "ccc_tree_User.png");
            // 
            // xdtpDate
            // 
            this.xdtpDate.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.White);
            this.xdtpDate.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
            this.xdtpDate.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(193)))), ((int)(((byte)(222)))));
            this.xdtpDate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            // 
            // 
            // 
            this.xdtpDate.Calendar.AllowMultipleSelection = false;
            this.xdtpDate.Calendar.Culture = new System.Globalization.CultureInfo("sv-SE");
            this.xdtpDate.Calendar.DaysFont = new System.Drawing.Font("Verdana", 8F);
            this.xdtpDate.Calendar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.xdtpDate.Calendar.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xdtpDate.Calendar.ForeColor = System.Drawing.SystemColors.ControlText;
            this.xdtpDate.Calendar.GridLines = Syncfusion.Windows.Forms.Grid.GridBorderStyle.None;
            this.xdtpDate.Calendar.HeaderEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.xdtpDate.Calendar.HeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.xdtpDate.Calendar.HeaderHeight = 20;
            this.xdtpDate.Calendar.HeaderStartColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252)))));
            this.xdtpDate.Calendar.HeadForeColor = System.Drawing.SystemColors.ControlText;
            this.xdtpDate.Calendar.HeadGradient = true;
            this.xdtpDate.Calendar.Location = new System.Drawing.Point(0, 0);
            this.xdtpDate.Calendar.MinValue = new System.DateTime(1990, 12, 31, 0, 0, 0, 0);
            this.xdtpDate.Calendar.Name = "monthCalendar";
            this.xdtpDate.Calendar.ScrollButtonSize = new System.Drawing.Size(24, 24);
            this.xdtpDate.Calendar.SelectedDates = new System.DateTime[0];
            this.xdtpDate.Calendar.Size = new System.Drawing.Size(206, 174);
            this.xdtpDate.Calendar.SizeToFit = true;
            this.xdtpDate.Calendar.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this.xdtpDate.Calendar.TabIndex = 0;
            this.xdtpDate.Calendar.ThemedEnabledGrid = true;
            this.xdtpDate.Calendar.WeekFont = new System.Drawing.Font("Verdana", 8F);
            this.xdtpDate.Calendar.WeekInterior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.PeachPuff, System.Drawing.Color.AntiqueWhite);
            // 
            // 
            // 
            this.xdtpDate.Calendar.NoneButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.xdtpDate.Calendar.NoneButton.Location = new System.Drawing.Point(134, 0);
            this.xdtpDate.Calendar.NoneButton.Size = new System.Drawing.Size(72, 20);
            this.xdtpDate.Calendar.NoneButton.Text = "None";
            this.xdtpDate.Calendar.NoneButton.UseVisualStyle = true;
            // 
            // 
            // 
            this.xdtpDate.Calendar.TodayButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.xdtpDate.Calendar.TodayButton.Location = new System.Drawing.Point(0, 0);
            this.xdtpDate.Calendar.TodayButton.Size = new System.Drawing.Size(134, 20);
            this.xdtpDate.Calendar.TodayButton.Text = "Today";
            this.xdtpDate.Calendar.TodayButton.UseVisualStyle = true;
            this.xdtpDate.CalendarTitleBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252)))));
            this.xdtpDate.CalendarTitleForeColor = System.Drawing.SystemColors.ControlText;
            this.xdtpDate.Dock = System.Windows.Forms.DockStyle.Top;
            this.xdtpDate.DropDownImage = null;
            this.xdtpDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.xdtpDate.Location = new System.Drawing.Point(0, 0);
            this.xdtpDate.MinValue = new System.DateTime(1990, 12, 31, 23, 59, 0, 0);
            this.xdtpDate.Name = "xdtpDate";
            this.xdtpDate.NoneButtonVisible = false;
            this.xdtpDate.ShowCheckBox = false;
            this.xdtpDate.Size = new System.Drawing.Size(322, 20);
            this.xdtpDate.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this.xdtpDate.TabIndex = 6;
            this.xdtpDate.ThemedChildControls = true;
            this.xdtpDate.ThemesEnabled = true;
            this.xdtpDate.Value = new System.DateTime(2012, 1, 19, 15, 2, 3, 865);
            this.xdtpDate.ValueChanged += new System.EventHandler(this.xdtpDateValueChanged);
            this.xdtpDate.PopupClosed += new Syncfusion.Windows.Forms.PopupClosedEventHandler(this.xdtpDatePopupClosed);
            // 
            // tabControlAdv
            // 
            this.tabControlAdv.BackColor = System.Drawing.Color.White;
            this.tabControlAdv.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tabControlAdv.BorderWidth = 0;
            this.tabControlAdv.Controls.Add(tabPageAdvMainTabPage);
            this.tabControlAdv.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlAdv.Location = new System.Drawing.Point(0, 0);
            this.tabControlAdv.Name = "tabControlAdv";
            this.tabControlAdv.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.tabControlAdv.ShowCloseButtonForActiveTabOnly = true;
            this.tabControlAdv.ShowScroll = false;
            this.tabControlAdv.Size = new System.Drawing.Size(326, 430);
            this.tabControlAdv.TabIndex = 5;
            this.tabControlAdv.TabPrimitivesHost.TabPrimitives.Add(new Syncfusion.Windows.Forms.Tools.TabPrimitive(Syncfusion.Windows.Forms.Tools.TabPrimitiveType.FirstTab, null, System.Drawing.Color.Empty, true, 1, "TabPrimitive0", ""));
            this.tabControlAdv.TabPrimitivesHost.TabPrimitives.Add(new Syncfusion.Windows.Forms.Tools.TabPrimitive(Syncfusion.Windows.Forms.Tools.TabPrimitiveType.PreviousTab, null, System.Drawing.Color.Empty, true, 1, "TabPrimitive1", ""));
            this.tabControlAdv.TabPrimitivesHost.TabPrimitives.Add(new Syncfusion.Windows.Forms.Tools.TabPrimitive(Syncfusion.Windows.Forms.Tools.TabPrimitiveType.NextTab, null, System.Drawing.Color.Empty, true, 1, "TabPrimitive4", ""));
            this.tabControlAdv.TabPrimitivesHost.TabPrimitives.Add(new Syncfusion.Windows.Forms.Tools.TabPrimitive(Syncfusion.Windows.Forms.Tools.TabPrimitiveType.LastTab, null, System.Drawing.Color.Empty, true, 1, "TabPrimitive5", ""));
            this.tabControlAdv.TabPrimitivesHost.TabPrimitives.Add(new Syncfusion.Windows.Forms.Tools.TabPrimitive(Syncfusion.Windows.Forms.Tools.TabPrimitiveType.DropDown, null, System.Drawing.Color.Empty, true, 1, "TabPrimitive0", ""));
            this.tabControlAdv.TabPrimitivesHost.Visible = true;
            this.tabControlAdv.TabStyle = typeof(Syncfusion.Windows.Forms.Tools.TabRendererIE7);
            this.tabControlAdv.ThemesEnabled = true;
            this.tabControlAdv.SelectedIndexChanged += new System.EventHandler(this.tabControlAdvSelectedIndexChanged);
            // 
            // PersonSelectorView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControlAdv);
            this.Name = "PersonSelectorView";
            this.Size = new System.Drawing.Size(326, 430);
            tabPageAdvMainTabPage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.treeViewAdvMainTabTree)).EndInit();
            this.contextMenuStrip.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.xdtpDate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tabControlAdv)).EndInit();
            this.tabControlAdv.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.TabControlAdv tabControlAdv;
        private Syncfusion.Windows.Forms.Tools.TreeViewAdv treeViewAdvMainTabTree;
        private Syncfusion.Windows.Forms.Tools.DateTimePickerAdv xdtpDate;
        private System.Windows.Forms.ImageList ImageList;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem OpenNewPPLAdmintoolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSearch;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem RefreshListToolStripMenuItem;
    }
}
