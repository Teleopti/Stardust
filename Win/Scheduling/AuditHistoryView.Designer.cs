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
			this.components = new System.ComponentModel.Container();
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle1 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle2 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle3 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle4 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			this.backgroundWorkerDataLoader = new System.ComponentModel.BackgroundWorker();
			this.buttonRestore = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonClose = new Syncfusion.Windows.Forms.ButtonAdv();
			this.statusStrip = new System.Windows.Forms.StatusStrip();
			this.pageOfPagesStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.grid = new Syncfusion.Windows.Forms.Grid.GridControl();
			this.linkLabelPrevious = new System.Windows.Forms.LinkLabel();
			this.linkLabelNext = new System.Windows.Forms.LinkLabel();
			this.statusStrip.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.SuspendLayout();
			// 
			// backgroundWorkerDataLoader
			// 
			this.backgroundWorkerDataLoader.WorkerSupportsCancellation = true;
			this.backgroundWorkerDataLoader.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerDataLoaderDoWork);
			this.backgroundWorkerDataLoader.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerDataLoaderRunWorkerCompleted);
			// 
			// buttonRestore
			// 
			this.buttonRestore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonRestore.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonRestore.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonRestore.BeforeTouchSize = new System.Drawing.Size(100, 27);
			this.tableLayoutPanel1.SetColumnSpan(this.buttonRestore, 2);
			this.buttonRestore.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonRestore.ForeColor = System.Drawing.Color.White;
			this.buttonRestore.IsBackStageButton = false;
			this.buttonRestore.Location = new System.Drawing.Point(604, 415);
			this.buttonRestore.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonRestore.Name = "buttonRestore";
			this.buttonRestore.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
			this.buttonRestore.Size = new System.Drawing.Size(100, 27);
			this.buttonRestore.TabIndex = 13;
			this.buttonRestore.Text = "xxRestore";
			this.buttonRestore.UseVisualStyle = true;
			this.buttonRestore.Click += new System.EventHandler(this.buttonRestoreClick);
			// 
			// buttonClose
			// 
			this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonClose.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonClose.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.tableLayoutPanel1.SetColumnSpan(this.buttonClose, 2);
			this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonClose.ForeColor = System.Drawing.Color.White;
			this.buttonClose.IsBackStageButton = false;
			this.buttonClose.Location = new System.Drawing.Point(743, 415);
			this.buttonClose.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonClose.Name = "buttonClose";
			this.buttonClose.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
			this.buttonClose.Size = new System.Drawing.Size(87, 27);
			this.buttonClose.TabIndex = 14;
			this.buttonClose.Text = "xxClose";
			this.buttonClose.UseVisualStyle = true;
			this.buttonClose.Click += new System.EventHandler(this.buttonCloseClick);
			// 
			// statusStrip
			// 
			this.statusStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Visible;
			this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pageOfPagesStatusLabel});
			this.statusStrip.Location = new System.Drawing.Point(0, 452);
			this.statusStrip.Name = "statusStrip";
			this.statusStrip.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
			this.statusStrip.Size = new System.Drawing.Size(840, 22);
			this.statusStrip.TabIndex = 7;
			this.statusStrip.Text = "statusStrip1";
			// 
			// pageOfPagesStatusLabel
			// 
			this.pageOfPagesStatusLabel.BackColor = System.Drawing.SystemColors.Control;
			this.pageOfPagesStatusLabel.Name = "pageOfPagesStatusLabel";
			this.pageOfPagesStatusLabel.Size = new System.Drawing.Size(77, 17);
			this.pageOfPagesStatusLabel.Text = "pageOfPages";
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 4;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 107F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 103F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 23F));
			this.tableLayoutPanel1.Controls.Add(this.buttonClose, 2, 2);
			this.tableLayoutPanel1.Controls.Add(this.grid, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.linkLabelPrevious, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.linkLabelNext, 2, 1);
			this.tableLayoutPanel1.Controls.Add(this.buttonRestore, 0, 2);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(840, 452);
			this.tableLayoutPanel1.TabIndex = 5;
			// 
			// grid
			// 
			this.grid.AlphaBlendSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(94)))), ((int)(((byte)(171)))), ((int)(((byte)(222)))));
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
			gridBaseStyle3.Name = "Column Header";
			gridBaseStyle3.StyleInfo.BaseStyle = "Header";
			gridBaseStyle3.StyleInfo.HorizontalAlignment = Syncfusion.Windows.Forms.Grid.GridHorizontalAlignment.Center;
			gridBaseStyle4.Name = "Row Header";
			gridBaseStyle4.StyleInfo.BaseStyle = "Header";
			gridBaseStyle4.StyleInfo.HorizontalAlignment = Syncfusion.Windows.Forms.Grid.GridHorizontalAlignment.Left;
			gridBaseStyle4.StyleInfo.Interior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Horizontal, System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(199)))), ((int)(((byte)(184))))), System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(234)))), ((int)(((byte)(216))))));
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
			this.grid.DefaultGridBorderStyle = Syncfusion.Windows.Forms.Grid.GridBorderStyle.Solid;
			this.grid.DefaultRowHeight = 20;
			this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.grid.ExcelLikeCurrentCell = true;
			this.grid.ExcelLikeSelectionFrame = true;
			this.grid.ForeColor = System.Drawing.SystemColors.ControlText;
			this.grid.GridOfficeScrollBars = Syncfusion.Windows.Forms.OfficeScrollBars.Metro;
			this.grid.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Metro;
			this.grid.Location = new System.Drawing.Point(3, 3);
			this.grid.MetroScrollBars = true;
			this.grid.Name = "grid";
			this.grid.NumberedColHeaders = false;
			this.grid.NumberedRowHeaders = false;
			this.grid.Properties.ForceImmediateRepaint = false;
			this.grid.Properties.GridLineColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(212)))), ((int)(((byte)(212)))));
			this.grid.Properties.MarkColHeader = false;
			this.grid.Properties.MarkRowHeader = false;
			this.grid.ReadOnly = true;
			this.grid.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.grid.RowHeightEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridRowHeight[] {
            new Syncfusion.Windows.Forms.Grid.GridRowHeight(0, 29)});
			this.grid.SelectCellsMouseButtonsMask = System.Windows.Forms.MouseButtons.Left;
			this.grid.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
			this.grid.ShowCurrentCellBorderBehavior = Syncfusion.Windows.Forms.Grid.GridShowCurrentCellBorder.AlwaysVisible;
			this.grid.Size = new System.Drawing.Size(834, 376);
			this.grid.SmartSizeBox = false;
			this.grid.TabIndex = 12;
			this.grid.ThemesEnabled = true;
			this.grid.UseRightToLeftCompatibleTextBox = true;
			// 
			// linkLabelPrevious
			// 
			this.linkLabelPrevious.Dock = System.Windows.Forms.DockStyle.Fill;
			this.linkLabelPrevious.Location = new System.Drawing.Point(610, 382);
			this.linkLabelPrevious.Name = "linkLabelPrevious";
			this.linkLabelPrevious.Size = new System.Drawing.Size(101, 24);
			this.linkLabelPrevious.TabIndex = 2;
			this.linkLabelPrevious.TabStop = true;
			this.linkLabelPrevious.Text = "xxPrevious";
			this.linkLabelPrevious.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.linkLabelPrevious.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelEarlierLinkClicked);
			// 
			// linkLabelNext
			// 
			this.linkLabelNext.Dock = System.Windows.Forms.DockStyle.Fill;
			this.linkLabelNext.Location = new System.Drawing.Point(717, 382);
			this.linkLabelNext.Name = "linkLabelNext";
			this.linkLabelNext.Size = new System.Drawing.Size(97, 24);
			this.linkLabelNext.TabIndex = 3;
			this.linkLabelNext.TabStop = true;
			this.linkLabelNext.Text = "xxNext";
			this.linkLabelNext.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.linkLabelNext.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelLaterLinkClicked);
			// 
			// AuditHistoryView
			// 
			this.AcceptButton = this.buttonRestore;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Window;
			this.BorderColor = System.Drawing.Color.Blue;
			this.CancelButton = this.buttonClose;
			this.CaptionFont = new System.Drawing.Font("Segoe UI", 12F);
			this.ClientSize = new System.Drawing.Size(840, 474);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Controls.Add(this.statusStrip);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.HelpButton = false;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(231, 47);
			this.Name = "AuditHistoryView";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "xxAuditHistoryView";
			this.Load += new System.EventHandler(this.auditHistoryViewLoad);
			this.ResizeEnd += new System.EventHandler(this.auditHistoryViewResizeEnd);
			this.statusStrip.ResumeLayout(false);
			this.statusStrip.PerformLayout();
			this.tableLayoutPanel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

		  private System.ComponentModel.BackgroundWorker backgroundWorkerDataLoader;
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
