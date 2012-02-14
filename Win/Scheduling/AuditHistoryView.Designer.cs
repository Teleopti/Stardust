namespace Teleopti.Ccc.Win.Scheduling
{
    partial class AuditHistoryView
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
            Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle1 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
            Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle2 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
            Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle3 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
            Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle4 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
            this.backgroundWorkerDataLoader = new System.ComponentModel.BackgroundWorker();
            this.restrictionSummaryGrid1 = new Teleopti.Ccc.Win.Scheduling.SingleAgentRestriction.RestrictionSummaryGrid();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.pageOfPagesStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.ribbonControlHeader = new Syncfusion.Windows.Forms.Tools.RibbonControlAdv();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonClose = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonRestore = new Syncfusion.Windows.Forms.ButtonAdv();
            this.grid = new Syncfusion.Windows.Forms.Grid.GridControl();
            this.linkLabelPrevious = new System.Windows.Forms.LinkLabel();
            this.linkLabelNext = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.restrictionSummaryGrid1)).BeginInit();
            this.statusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlHeader)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.SuspendLayout();
            // 
            // backgroundWorkerDataLoader
            // 
            this.backgroundWorkerDataLoader.WorkerSupportsCancellation = true;
            this.backgroundWorkerDataLoader.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerDataLoader_DoWork);
            this.backgroundWorkerDataLoader.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerDataLoader_RunWorkerCompleted);
            // 
            // restrictionSummaryGrid1
            // 
            this.restrictionSummaryGrid1.CoveredRanges.AddRange(new Syncfusion.Windows.Forms.Grid.GridRangeInfo[] {
            Syncfusion.Windows.Forms.Grid.GridRangeInfo.Cells(0, 2, 0, 6),
            Syncfusion.Windows.Forms.Grid.GridRangeInfo.Cells(0, 7, 0, 8),
            Syncfusion.Windows.Forms.Grid.GridRangeInfo.Cells(0, 9, 0, 12)});
            this.restrictionSummaryGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.restrictionSummaryGrid1.ExcelLikeCurrentCell = true;
            this.restrictionSummaryGrid1.GridOfficeScrollBars = Syncfusion.Windows.Forms.OfficeScrollBars.Office2007;
            this.restrictionSummaryGrid1.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Office2007Blue;
            this.restrictionSummaryGrid1.HeaderCount = 0;
            this.restrictionSummaryGrid1.HScrollBehavior = Syncfusion.Windows.Forms.Grid.GridScrollbarMode.Automatic;
            this.restrictionSummaryGrid1.HScrollPixel = true;
            this.restrictionSummaryGrid1.Location = new System.Drawing.Point(0, 0);
            this.restrictionSummaryGrid1.Name = "restrictionSummaryGrid1";
            this.restrictionSummaryGrid1.NumberedColHeaders = false;
            this.restrictionSummaryGrid1.NumberedRowHeaders = false;
            this.restrictionSummaryGrid1.Office2007ScrollBars = true;
            this.restrictionSummaryGrid1.Properties.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.restrictionSummaryGrid1.Properties.FixedLinesColor = System.Drawing.Color.Red;
            this.restrictionSummaryGrid1.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
            this.restrictionSummaryGrid1.Size = new System.Drawing.Size(489, 344);
            this.restrictionSummaryGrid1.SmartSizeBox = false;
            this.restrictionSummaryGrid1.Text = "gridControl1";
            this.restrictionSummaryGrid1.ThemesEnabled = true;
            this.restrictionSummaryGrid1.UseRightToLeftCompatibleTextBox = true;
            // 
            // buttonRestore
            // 
            this.buttonRestore.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonRestore.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonRestore.Dock = System.Windows.Forms.DockStyle.Top;
            this.buttonRestore.Location = new System.Drawing.Point(613, 376);
            this.buttonRestore.Name = "buttonRestore";
            this.buttonRestore.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.buttonRestore.Size = new System.Drawing.Size(82, 23);
            this.buttonRestore.TabIndex = 1;
            this.buttonRestore.Text = "xxRestore";
            this.buttonRestore.UseVisualStyle = true;
            this.buttonRestore.Click += new System.EventHandler(this.ButtonRestoreClick);
            // 
            // buttonClose
            // 
            this.buttonClose.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonClose.Dock = System.Windows.Forms.DockStyle.Top;
            this.buttonClose.Location = new System.Drawing.Point(701, 376);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.buttonClose.Size = new System.Drawing.Size(82, 23);
            this.buttonClose.TabIndex = 0;
            this.buttonClose.Text = "xxClose";
            this.buttonClose.UseVisualStyle = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // statusStrip
            // 
            this.statusStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Visible;
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pageOfPagesStatusLabel});
            this.statusStrip.Location = new System.Drawing.Point(6, 445);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(806, 22);
            this.statusStrip.TabIndex = 7;
            this.statusStrip.Text = "statusStrip1";
            // 
            // pageOfPagesStatusLabel
            // 
            this.pageOfPagesStatusLabel.BackColor = System.Drawing.SystemColors.Control;
            this.pageOfPagesStatusLabel.Name = "pageOfPagesStatusLabel";
            this.SetShortcut(this.pageOfPagesStatusLabel, System.Windows.Forms.Keys.None);
            this.pageOfPagesStatusLabel.Size = new System.Drawing.Size(77, 17);
            this.pageOfPagesStatusLabel.Text = "pageOfPages";
            // 
            // ribbonControlHeader
            // 
            this.ribbonControlHeader.Location = new System.Drawing.Point(1, 0);
            this.ribbonControlHeader.MenuButtonVisible = false;
            this.ribbonControlHeader.Name = "ribbonControlHeader";
            // 
            // ribbonControlHeader.OfficeMenu
            // 
            this.ribbonControlHeader.OfficeMenu.Name = "OfficeMenu";
            this.ribbonControlHeader.OfficeMenu.Size = new System.Drawing.Size(12, 65);
            this.ribbonControlHeader.QuickPanelVisible = false;
            this.ribbonControlHeader.SelectedTab = null;
            this.ribbonControlHeader.Size = new System.Drawing.Size(810, 33);
            this.ribbonControlHeader.SystemText.QuickAccessDialogDropDownName = "Start menu";
            this.ribbonControlHeader.TabIndex = 4;
            this.ribbonControlHeader.Text = "ribbonControlAdv1";
            this.ribbonControlHeader.TitleAlignment = Syncfusion.Windows.Forms.Tools.TextAlignment.Center;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 88F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 88F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.buttonClose, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.buttonRestore, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.grid, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.linkLabelPrevious, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.linkLabelNext, 2, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(6, 34);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(800, 411);
            this.tableLayoutPanel1.TabIndex = 13;
            // 
            // buttonClose
            // 
            this.buttonClose.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonClose.Dock = System.Windows.Forms.DockStyle.Top;
            this.buttonClose.Location = new System.Drawing.Point(695, 379);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.buttonClose.Size = new System.Drawing.Size(82, 23);
            this.buttonClose.TabIndex = 14;
            this.buttonClose.Text = "xxClose";
            this.buttonClose.UseVisualStyle = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // buttonRestore
            // 
            this.buttonRestore.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonRestore.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonRestore.Dock = System.Windows.Forms.DockStyle.Top;
            this.buttonRestore.Location = new System.Drawing.Point(607, 379);
            this.buttonRestore.Name = "buttonRestore";
            this.buttonRestore.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.buttonRestore.Size = new System.Drawing.Size(82, 23);
            this.buttonRestore.TabIndex = 13;
            this.buttonRestore.Text = "xxRestore";
            this.buttonRestore.UseVisualStyle = true;
            this.buttonRestore.Click += new System.EventHandler(this.ButtonRestoreClick);
            // 
            // grid
            // 
            gridBaseStyle1.Name = "Header";
            gridBaseStyle1.StyleInfo.Borders.Bottom = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
            gridBaseStyle1.StyleInfo.Borders.Left = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
            gridBaseStyle1.StyleInfo.Borders.Right = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
            gridBaseStyle1.StyleInfo.Borders.Top = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
            gridBaseStyle1.StyleInfo.CellType = "Header";
            gridBaseStyle1.StyleInfo.Font.Bold = true;
            gridBaseStyle1.StyleInfo.Interior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(199)))), ((int)(((byte)(184))))), System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(234)))), ((int)(((byte)(216))))));
            gridBaseStyle1.StyleInfo.VerticalAlignment = Syncfusion.Windows.Forms.Grid.GridVerticalAlignment.Middle;
            gridBaseStyle2.Name = "Standard";
            gridBaseStyle2.StyleInfo.Font.Facename = "Tahoma";
            gridBaseStyle2.StyleInfo.Interior = new Syncfusion.Drawing.BrushInfo(System.Drawing.SystemColors.Window);
            gridBaseStyle2.StyleInfo.TextAlign = Syncfusion.Windows.Forms.Grid.GridTextAlign.Default;
            gridBaseStyle3.Name = "Row Header";
            gridBaseStyle3.StyleInfo.BaseStyle = "Header";
            gridBaseStyle3.StyleInfo.HorizontalAlignment = Syncfusion.Windows.Forms.Grid.GridHorizontalAlignment.Left;
            gridBaseStyle3.StyleInfo.Interior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Horizontal, System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(199)))), ((int)(((byte)(184))))), System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(234)))), ((int)(((byte)(216))))));
            gridBaseStyle4.Name = "Column Header";
            gridBaseStyle4.StyleInfo.BaseStyle = "Header";
            gridBaseStyle4.StyleInfo.HorizontalAlignment = Syncfusion.Windows.Forms.Grid.GridHorizontalAlignment.Center;
            this.grid.BaseStylesMap.AddRange(new Syncfusion.Windows.Forms.Grid.GridBaseStyle[] {
            gridBaseStyle1,
            gridBaseStyle2,
            gridBaseStyle3,
            gridBaseStyle4});
            this.grid.ColCount = 1;
            this.tableLayoutPanel1.SetColumnSpan(this.grid, 4);
            this.grid.ColWidthEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridColWidth[] {
            new Syncfusion.Windows.Forms.Grid.GridColWidth(0, 35)});
            this.grid.DefaultColWidth = 400;
            this.grid.DefaultRowHeight = 28;
            this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grid.ExcelLikeCurrentCell = true;
            this.grid.ExcelLikeSelectionFrame = true;
            this.grid.ForeColor = System.Drawing.SystemColors.ControlText;
            this.grid.GridOfficeScrollBars = Syncfusion.Windows.Forms.OfficeScrollBars.Office2007;
            this.grid.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Office2007Blue;
            this.grid.Location = new System.Drawing.Point(3, 3);
            this.grid.Name = "grid";
            this.grid.NumberedColHeaders = false;
            this.grid.NumberedRowHeaders = false;
            this.grid.ReadOnly = true;
            this.grid.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.grid.RowHeightEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridRowHeight[] {
            new Syncfusion.Windows.Forms.Grid.GridRowHeight(0, 28)});
            this.grid.SelectCellsMouseButtonsMask = System.Windows.Forms.MouseButtons.Left;
            this.grid.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
            this.grid.ShowCurrentCellBorderBehavior = Syncfusion.Windows.Forms.Grid.GridShowCurrentCellBorder.AlwaysVisible;
            this.grid.Size = new System.Drawing.Size(800, 342);
            this.grid.SmartSizeBox = false;
            this.grid.TabIndex = 12;
            this.grid.ThemesEnabled = true;
            this.grid.UseRightToLeftCompatibleTextBox = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 88F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 88F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.buttonClose, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.grid, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.buttonRestore, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.linkLabelPrevious, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.linkLabelNext, 2, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(6, 34);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(806, 408);
            this.tableLayoutPanel1.TabIndex = 5;
            // 
            // linkLabelPrevious
            // 
            this.linkLabelPrevious.AutoSize = true;
            this.linkLabelPrevious.Dock = System.Windows.Forms.DockStyle.Fill;
            this.linkLabelPrevious.Location = new System.Drawing.Point(613, 348);
            this.linkLabelPrevious.Name = "linkLabelPrevious";
            this.linkLabelPrevious.Size = new System.Drawing.Size(82, 25);
            this.linkLabelPrevious.TabIndex = 2;
            this.linkLabelPrevious.TabStop = true;
            this.linkLabelPrevious.Text = "xxPrevious";
            this.linkLabelPrevious.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.linkLabelPrevious.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelEarlier_LinkClicked);
            // 
            // linkLabelNext
            // 
            this.linkLabelNext.AutoSize = true;
            this.linkLabelNext.Dock = System.Windows.Forms.DockStyle.Fill;
            this.linkLabelNext.Location = new System.Drawing.Point(701, 348);
            this.linkLabelNext.Name = "linkLabelNext";
            this.linkLabelNext.Size = new System.Drawing.Size(82, 25);
            this.linkLabelNext.TabIndex = 3;
            this.linkLabelNext.TabStop = true;
            this.linkLabelNext.Text = "xxNext";
            this.linkLabelNext.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.linkLabelNext.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelLater_LinkClicked);
            // 
            // ribbonControlHeader
            // 
            this.ribbonControlHeader.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ribbonControlHeader.Location = new System.Drawing.Point(1, 0);
            this.ribbonControlHeader.MenuButtonVisible = false;
            this.ribbonControlHeader.Name = "ribbonControlHeader";
            // 
            // ribbonControlHeader.OfficeMenu
            // 
            this.ribbonControlHeader.OfficeMenu.Name = "OfficeMenu";
            this.ribbonControlHeader.OfficeMenu.Size = new System.Drawing.Size(12, 65);
            this.ribbonControlHeader.QuickPanelVisible = false;
            this.ribbonControlHeader.SelectedTab = null;
            this.ribbonControlHeader.Size = new System.Drawing.Size(816, 33);
            this.ribbonControlHeader.SystemText.QuickAccessDialogDropDownName = "Start menu";
            this.ribbonControlHeader.TabIndex = 4;
            this.ribbonControlHeader.Text = "ribbonControlAdv1";
            this.ribbonControlHeader.TitleAlignment = Syncfusion.Windows.Forms.Tools.TextAlignment.Center;
            // 
            // AuditHistoryView
            // 
            this.AcceptButton = this.buttonRestore;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.CancelButton = this.buttonClose;
            this.ClientSize = new System.Drawing.Size(818, 473);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.ribbonControlHeader);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AuditHistoryView";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "xxAuditHistoryView";
            this.Load += new System.EventHandler(this.AuditHistoryView_Load);
            ((System.ComponentModel.ISupportInitialize)(this.restrictionSummaryGrid1)).EndInit();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlHeader)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.ComponentModel.BackgroundWorker backgroundWorkerDataLoader;
        private Syncfusion.Windows.Forms.Tools.RibbonControlAdv ribbonControlHeader;
        private SingleAgentRestriction.RestrictionSummaryGrid restrictionSummaryGrid1;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel pageOfPagesStatusLabel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Syncfusion.Windows.Forms.ButtonAdv buttonClose;
        private Syncfusion.Windows.Forms.ButtonAdv buttonRestore;
        private Syncfusion.Windows.Forms.Grid.GridControl grid;
        private System.Windows.Forms.LinkLabel linkLabelPrevious;
        private System.Windows.Forms.LinkLabel linkLabelNext;
    }
}
