namespace Teleopti.Ccc.Win.Forecasting.Forms.JobHistory
{
    partial class JobHistoryView
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
            Syncfusion.Windows.Forms.Grid.GridStyleInfo gridStyleInfo1 = new Syncfusion.Windows.Forms.Grid.GridStyleInfo();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(JobHistoryView));
            this.gridControlJobHistory = new Syncfusion.Windows.Forms.Grid.GridDataBoundGrid();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonAdvClose = new Syncfusion.Windows.Forms.ButtonAdv();
            this.linkLabelPrevious = new System.Windows.Forms.LinkLabel();
            this.linkLabelNext = new System.Windows.Forms.LinkLabel();
            this.autoLabelPageCount = new Syncfusion.Windows.Forms.Tools.AutoLabel();
            this.ribbonControlAdv1 = new Syncfusion.Windows.Forms.Tools.RibbonControlAdv();
            this.toolStripTabItem1 = new Syncfusion.Windows.Forms.Tools.ToolStripTabItem();
            this.toolStripEx1 = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
            this.toolStripButtonReloadHistory = new System.Windows.Forms.ToolStripButton();
            ((System.ComponentModel.ISupportInitialize)(this.gridControlJobHistory)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).BeginInit();
            this.ribbonControlAdv1.SuspendLayout();
            this.toolStripTabItem1.Panel.SuspendLayout();
            this.toolStripEx1.SuspendLayout();
            this.SuspendLayout();
            // 
            // gridControlJobHistory
            // 
            this.gridControlJobHistory.AllowDragSelectedCols = true;
            this.gridControlJobHistory.AllowResizeToFit = false;
            this.gridControlJobHistory.BackColor = System.Drawing.Color.White;
            this.tableLayoutPanel1.SetColumnSpan(this.gridControlJobHistory, 4);
            this.gridControlJobHistory.DefaultColWidth = 80;
            this.gridControlJobHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControlJobHistory.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.gridControlJobHistory.GridLineColor = System.Drawing.SystemColors.GrayText;
            this.gridControlJobHistory.GridOfficeScrollBars = Syncfusion.Windows.Forms.OfficeScrollBars.Office2010;
            this.gridControlJobHistory.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Office2007Blue;
            this.gridControlJobHistory.Location = new System.Drawing.Point(3, 3);
            this.gridControlJobHistory.MinResizeColSize = 80;
            this.gridControlJobHistory.Name = "gridControlJobHistory";
            this.gridControlJobHistory.Office2007ScrollBars = true;
            this.gridControlJobHistory.Office2007ScrollBarsColorScheme = Syncfusion.Windows.Forms.Office2007ColorScheme.Managed;
            this.gridControlJobHistory.Office2010ScrollBarsColorScheme = Syncfusion.Windows.Forms.Office2010ColorScheme.Managed;
            this.gridControlJobHistory.ShowCurrentCellBorderBehavior = Syncfusion.Windows.Forms.Grid.GridShowCurrentCellBorder.GrayWhenLostFocus;
            this.gridControlJobHistory.Size = new System.Drawing.Size(891, 265);
            this.gridControlJobHistory.SmartSizeBox = false;
            this.gridControlJobHistory.SortBehavior = Syncfusion.Windows.Forms.Grid.GridSortBehavior.SingleClick;
            this.gridControlJobHistory.TabIndex = 1;
            gridStyleInfo1.Font.Bold = false;
            gridStyleInfo1.Font.Facename = "Microsoft Sans Serif";
            gridStyleInfo1.Font.Italic = false;
            gridStyleInfo1.Font.Size = 8.25F;
            gridStyleInfo1.Font.Strikeout = false;
            gridStyleInfo1.Font.Underline = false;
            this.gridControlJobHistory.TableStyle = gridStyleInfo1;
            this.gridControlJobHistory.Text = "gridControlJobHistory";
            this.gridControlJobHistory.ThemesEnabled = true;
            this.gridControlJobHistory.UseListChangedEvent = true;
            this.gridControlJobHistory.UseRightToLeftCompatibleTextBox = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 88F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 88F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 88F));
            this.tableLayoutPanel1.Controls.Add(this.buttonAdvClose, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.gridControlJobHistory, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.linkLabelPrevious, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.linkLabelNext, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.autoLabelPageCount, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(6, 126);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(897, 307);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // buttonAdvClose
            // 
            this.buttonAdvClose.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonAdvClose.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonAdvClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonAdvClose.Location = new System.Drawing.Point(816, 277);
            this.buttonAdvClose.Name = "buttonAdvClose";
            this.buttonAdvClose.Size = new System.Drawing.Size(73, 23);
            this.buttonAdvClose.TabIndex = 2;
            this.buttonAdvClose.Text = "xxClose";
            this.buttonAdvClose.UseVisualStyle = true;
            this.buttonAdvClose.Click += new System.EventHandler(this.buttonAdvClose_Click);
            // 
            // linkLabelPrevious
            // 
            this.linkLabelPrevious.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabelPrevious.AutoSize = true;
            this.linkLabelPrevious.Location = new System.Drawing.Point(636, 271);
            this.linkLabelPrevious.Name = "linkLabelPrevious";
            this.linkLabelPrevious.Size = new System.Drawing.Size(82, 36);
            this.linkLabelPrevious.TabIndex = 7;
            this.linkLabelPrevious.TabStop = true;
            this.linkLabelPrevious.Text = "xxPrevious";
            this.linkLabelPrevious.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.linkLabelPrevious.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelPrevious_LinkClicked);
            // 
            // linkLabelNext
            // 
            this.linkLabelNext.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabelNext.AutoSize = true;
            this.linkLabelNext.Enabled = false;
            this.linkLabelNext.Location = new System.Drawing.Point(724, 271);
            this.linkLabelNext.Name = "linkLabelNext";
            this.linkLabelNext.Size = new System.Drawing.Size(82, 36);
            this.linkLabelNext.TabIndex = 8;
            this.linkLabelNext.TabStop = true;
            this.linkLabelNext.Text = "xxNext";
            this.linkLabelNext.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.linkLabelNext.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelNext_LinkClicked);
            // 
            // autoLabelPageCount
            // 
            this.autoLabelPageCount.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.autoLabelPageCount.Location = new System.Drawing.Point(5, 282);
            this.autoLabelPageCount.Margin = new System.Windows.Forms.Padding(5, 0, 3, 0);
            this.autoLabelPageCount.Name = "autoLabelPageCount";
            this.autoLabelPageCount.Size = new System.Drawing.Size(110, 13);
            this.autoLabelPageCount.TabIndex = 0;
            this.autoLabelPageCount.Text = "ShowingXToYOutOfZ";
            // 
            // ribbonControlAdv1
            // 
            this.ribbonControlAdv1.Header.AddMainItem(toolStripTabItem1);
            this.ribbonControlAdv1.Location = new System.Drawing.Point(1, 0);
            this.ribbonControlAdv1.MenuButtonText = "";
            this.ribbonControlAdv1.MenuButtonVisible = false;
            this.ribbonControlAdv1.Name = "ribbonControlAdv1";
            // 
            // ribbonControlAdv1.OfficeMenu
            // 
            this.ribbonControlAdv1.OfficeMenu.Name = "OfficeMenu";
            this.ribbonControlAdv1.OfficeMenu.Size = new System.Drawing.Size(12, 65);
            this.ribbonControlAdv1.QuickPanelVisible = false;
            this.ribbonControlAdv1.SelectedTab = this.toolStripTabItem1;
            this.ribbonControlAdv1.ShowLauncher = false;
            this.ribbonControlAdv1.ShowMinimizeButton = false;
            this.ribbonControlAdv1.ShowQuickItemsDropDownButton = false;
            this.ribbonControlAdv1.Size = new System.Drawing.Size(907, 125);
            this.ribbonControlAdv1.SystemText.QuickAccessDialogDropDownName = "Start menu";
            this.ribbonControlAdv1.TabIndex = 3;
            this.ribbonControlAdv1.Text = "ribbonControlAdv1";
            // 
            // toolStripTabItem1
            // 
            this.toolStripTabItem1.Margin = new System.Windows.Forms.Padding(0);
            this.toolStripTabItem1.Name = "toolStripTabItem1";
            this.toolStripTabItem1.Padding = new System.Windows.Forms.Padding(0);
            // 
            // ribbonControlAdv1.ribbonPanel1
            // 
            this.toolStripTabItem1.Panel.Controls.Add(this.toolStripEx1);
            this.toolStripTabItem1.Panel.Name = "ribbonPanel1";
            this.toolStripTabItem1.Panel.ScrollPosition = 0;
            this.toolStripTabItem1.Panel.TabIndex = 2;
            this.toolStripTabItem1.Panel.Text = "xxHome";
            this.toolStripTabItem1.Position = 0;
            this.SetShortcut(this.toolStripTabItem1, System.Windows.Forms.Keys.None);
            this.toolStripTabItem1.Size = new System.Drawing.Size(49, 17);
            this.toolStripTabItem1.Text = "xxHome";
            this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripTabItem1, false);
            // 
            // toolStripEx1
            // 
            this.toolStripEx1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripEx1.ForeColor = System.Drawing.Color.MidnightBlue;
            this.toolStripEx1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripEx1.Image = null;
            this.toolStripEx1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonReloadHistory});
            this.toolStripEx1.Location = new System.Drawing.Point(0, 1);
            this.toolStripEx1.Name = "toolStripEx1";
            this.toolStripEx1.Size = new System.Drawing.Size(67, 63);
            this.toolStripEx1.TabIndex = 0;
            // 
            // toolStripButtonReloadHistory
            // 
            this.toolStripButtonReloadHistory.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Refresh;
            this.toolStripButtonReloadHistory.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonReloadHistory.Name = "toolStripButtonReloadHistory";
            this.SetShortcut(this.toolStripButtonReloadHistory, System.Windows.Forms.Keys.None);
            this.toolStripButtonReloadHistory.Size = new System.Drawing.Size(60, 43);
            this.toolStripButtonReloadHistory.Text = "xxRefresh";
            this.toolStripButtonReloadHistory.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.toolStripButtonReloadHistory.Click += new System.EventHandler(this.toolStripButtonReloadHistory_Click);
            // 
            // JobHistoryView
            // 
            this.AcceptButton = this.buttonAdvClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonAdvClose;
            this.ClientSize = new System.Drawing.Size(909, 439);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.ribbonControlAdv1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "JobHistoryView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "xxJobHistory";
            this.LocationChanged += new System.EventHandler(this.JobHistoryView_LocationChanged);
            this.SizeChanged += new System.EventHandler(this.JobHistoryView_SizeChanged);
            ((System.ComponentModel.ISupportInitialize)(this.gridControlJobHistory)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).EndInit();
            this.ribbonControlAdv1.ResumeLayout(false);
            this.ribbonControlAdv1.PerformLayout();
            this.toolStripTabItem1.Panel.ResumeLayout(false);
            this.toolStripTabItem1.Panel.PerformLayout();
            this.toolStripEx1.ResumeLayout(false);
            this.toolStripEx1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Grid.GridDataBoundGrid gridControlJobHistory;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvClose;
        private Syncfusion.Windows.Forms.Tools.RibbonControlAdv ribbonControlAdv1;
        private Syncfusion.Windows.Forms.Tools.ToolStripTabItem toolStripTabItem1;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripEx1;
		private System.Windows.Forms.ToolStripButton toolStripButtonReloadHistory;
		private System.Windows.Forms.LinkLabel linkLabelPrevious;
		private System.Windows.Forms.LinkLabel linkLabelNext;
		private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelPageCount;

    }
}