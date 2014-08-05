namespace Teleopti.Ccc.Win.Forecasting.Forms.WorkloadPages
{
    partial class WorkloadOpenHours
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
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle5 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle6 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle7 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle8 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			Syncfusion.Windows.Forms.Grid.GridRangeStyle gridRangeStyle2 = new Syncfusion.Windows.Forms.Grid.GridRangeStyle();
			this.lblOpenHours = new System.Windows.Forms.Label();
			this.weekOpenHoursGridWorkload = new Teleopti.Ccc.Win.Forecasting.Forms.WeekOpenHoursGrid();
			this.tableLayoutPanelOpenHoursRtl = new System.Windows.Forms.TableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.weekOpenHoursGridWorkload)).BeginInit();
			this.tableLayoutPanelOpenHoursRtl.SuspendLayout();
			this.SuspendLayout();
			// 
			// lblOpenHours
			// 
			this.lblOpenHours.AutoSize = true;
			this.lblOpenHours.Location = new System.Drawing.Point(3, 33);
			this.lblOpenHours.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
			this.lblOpenHours.Name = "lblOpenHours";
			this.lblOpenHours.Size = new System.Drawing.Size(94, 26);
			this.lblOpenHours.TabIndex = 0;
			this.lblOpenHours.Text = "xxOpenHoursColon";
			// 
			// weekOpenHoursGridWorkload
			// 
			this.weekOpenHoursGridWorkload.AccessibilityEnabled = true;
			this.weekOpenHoursGridWorkload.ActivateCurrentCellBehavior = Syncfusion.Windows.Forms.Grid.GridCellActivateAction.DblClickOnCell;
			this.weekOpenHoursGridWorkload.AllowSelection = ((Syncfusion.Windows.Forms.Grid.GridSelectionFlags)(((((Syncfusion.Windows.Forms.Grid.GridSelectionFlags.Cell | Syncfusion.Windows.Forms.Grid.GridSelectionFlags.Multiple) 
            | Syncfusion.Windows.Forms.Grid.GridSelectionFlags.Shift) 
            | Syncfusion.Windows.Forms.Grid.GridSelectionFlags.Keyboard) 
            | Syncfusion.Windows.Forms.Grid.GridSelectionFlags.AlphaBlend)));
			this.weekOpenHoursGridWorkload.AlphaBlendSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(94)))), ((int)(((byte)(171)))), ((int)(((byte)(222)))));
			this.weekOpenHoursGridWorkload.BackColor = System.Drawing.Color.White;
			gridBaseStyle5.Name = "Header";
			gridBaseStyle5.StyleInfo.Borders.Bottom = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
			gridBaseStyle5.StyleInfo.Borders.Left = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
			gridBaseStyle5.StyleInfo.Borders.Right = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
			gridBaseStyle5.StyleInfo.Borders.Top = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
			gridBaseStyle5.StyleInfo.CellType = "Header";
			gridBaseStyle5.StyleInfo.Font.Bold = true;
			gridBaseStyle5.StyleInfo.Interior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(199)))), ((int)(((byte)(184))))), System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(234)))), ((int)(((byte)(216))))));
			gridBaseStyle5.StyleInfo.VerticalAlignment = Syncfusion.Windows.Forms.Grid.GridVerticalAlignment.Middle;
			gridBaseStyle6.Name = "Standard";
			gridBaseStyle6.StyleInfo.CellType = "TextBox";
			gridBaseStyle6.StyleInfo.Font.Facename = "Tahoma";
			gridBaseStyle6.StyleInfo.Interior = new Syncfusion.Drawing.BrushInfo(System.Drawing.SystemColors.Window);
			gridBaseStyle7.Name = "Column Header";
			gridBaseStyle7.StyleInfo.BaseStyle = "Header";
			gridBaseStyle7.StyleInfo.HorizontalAlignment = Syncfusion.Windows.Forms.Grid.GridHorizontalAlignment.Center;
			gridBaseStyle8.Name = "Row Header";
			gridBaseStyle8.StyleInfo.BaseStyle = "Header";
			gridBaseStyle8.StyleInfo.HorizontalAlignment = Syncfusion.Windows.Forms.Grid.GridHorizontalAlignment.Left;
			gridBaseStyle8.StyleInfo.Interior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Horizontal, System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(199)))), ((int)(((byte)(184))))), System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(234)))), ((int)(((byte)(216))))));
			this.weekOpenHoursGridWorkload.BaseStylesMap.AddRange(new Syncfusion.Windows.Forms.Grid.GridBaseStyle[] {
            gridBaseStyle5,
            gridBaseStyle6,
            gridBaseStyle7,
            gridBaseStyle8});
			this.weekOpenHoursGridWorkload.ColCount = 1;
			this.weekOpenHoursGridWorkload.ColWidthEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridColWidth[] {
            new Syncfusion.Windows.Forms.Grid.GridColWidth(0, 35),
            new Syncfusion.Windows.Forms.Grid.GridColWidth(0, 35),
            new Syncfusion.Windows.Forms.Grid.GridColWidth(0, 35)});
			this.weekOpenHoursGridWorkload.DefaultGridBorderStyle = Syncfusion.Windows.Forms.Grid.GridBorderStyle.Solid;
			this.weekOpenHoursGridWorkload.DefaultRowHeight = 20;
			this.weekOpenHoursGridWorkload.Dock = System.Windows.Forms.DockStyle.Fill;
			this.weekOpenHoursGridWorkload.ExcelLikeCurrentCell = true;
			this.weekOpenHoursGridWorkload.ExcelLikeSelectionFrame = true;
			this.weekOpenHoursGridWorkload.Font = new System.Drawing.Font("Arial", 8.25F);
			this.weekOpenHoursGridWorkload.GridOfficeScrollBars = Syncfusion.Windows.Forms.OfficeScrollBars.Metro;
			this.weekOpenHoursGridWorkload.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Metro;
			this.weekOpenHoursGridWorkload.Location = new System.Drawing.Point(108, 30);
			this.weekOpenHoursGridWorkload.MetroScrollBars = true;
			this.weekOpenHoursGridWorkload.Name = "weekOpenHoursGridWorkload";
			this.weekOpenHoursGridWorkload.NumberedColHeaders = false;
			this.weekOpenHoursGridWorkload.NumberedRowHeaders = false;
			this.weekOpenHoursGridWorkload.Office2007ScrollBars = true;
			this.weekOpenHoursGridWorkload.Office2007ScrollBarsColorScheme = Syncfusion.Windows.Forms.Office2007ColorScheme.Managed;
			this.weekOpenHoursGridWorkload.Properties.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(190)))), ((int)(((byte)(206)))), ((int)(((byte)(236)))));
			this.weekOpenHoursGridWorkload.Properties.ForceImmediateRepaint = false;
			this.weekOpenHoursGridWorkload.Properties.GridLineColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(212)))), ((int)(((byte)(212)))));
			this.weekOpenHoursGridWorkload.Properties.MarkColHeader = false;
			this.weekOpenHoursGridWorkload.Properties.MarkRowHeader = false;
			gridRangeStyle2.Range = Syncfusion.Windows.Forms.Grid.GridRangeInfo.Table();
			gridRangeStyle2.StyleInfo.Font.Bold = false;
			gridRangeStyle2.StyleInfo.Font.Facename = "Arial";
			gridRangeStyle2.StyleInfo.Font.Italic = false;
			gridRangeStyle2.StyleInfo.Font.Size = 8.25F;
			gridRangeStyle2.StyleInfo.Font.Strikeout = false;
			gridRangeStyle2.StyleInfo.Font.Underline = false;
			gridRangeStyle2.StyleInfo.Font.Unit = System.Drawing.GraphicsUnit.Point;
			this.weekOpenHoursGridWorkload.RangeStyles.AddRange(new Syncfusion.Windows.Forms.Grid.GridRangeStyle[] {
            gridRangeStyle2});
			this.weekOpenHoursGridWorkload.ResizeColsBehavior = Syncfusion.Windows.Forms.Grid.GridResizeCellsBehavior.None;
			this.weekOpenHoursGridWorkload.ResizeRowsBehavior = Syncfusion.Windows.Forms.Grid.GridResizeCellsBehavior.None;
			this.weekOpenHoursGridWorkload.RowCount = 7;
			this.weekOpenHoursGridWorkload.RowHeightEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridRowHeight[] {
            new Syncfusion.Windows.Forms.Grid.GridRowHeight(0, 29)});
			this.weekOpenHoursGridWorkload.SelectCellsMouseButtonsMask = System.Windows.Forms.MouseButtons.Left;
			this.weekOpenHoursGridWorkload.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
			this.weekOpenHoursGridWorkload.Size = new System.Drawing.Size(219, 171);
			this.weekOpenHoursGridWorkload.SmartSizeBox = false;
			this.weekOpenHoursGridWorkload.TabIndex = 1;
			this.weekOpenHoursGridWorkload.Text = "weekOpenHoursGrid1";
			this.weekOpenHoursGridWorkload.ThemesEnabled = true;
			this.weekOpenHoursGridWorkload.UseRightToLeftCompatibleTextBox = true;
			this.weekOpenHoursGridWorkload.KeyDown += new System.Windows.Forms.KeyEventHandler(this.weekOpenHoursGridWorkload_KeyDown);
			// 
			// tableLayoutPanelOpenHoursRtl
			// 
			this.tableLayoutPanelOpenHoursRtl.ColumnCount = 2;
			this.tableLayoutPanelOpenHoursRtl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32F));
			this.tableLayoutPanelOpenHoursRtl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 68F));
			this.tableLayoutPanelOpenHoursRtl.Controls.Add(this.lblOpenHours, 0, 1);
			this.tableLayoutPanelOpenHoursRtl.Controls.Add(this.weekOpenHoursGridWorkload, 1, 1);
			this.tableLayoutPanelOpenHoursRtl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelOpenHoursRtl.Location = new System.Drawing.Point(10, 10);
			this.tableLayoutPanelOpenHoursRtl.Name = "tableLayoutPanelOpenHoursRtl";
			this.tableLayoutPanelOpenHoursRtl.RowCount = 2;
			this.tableLayoutPanelOpenHoursRtl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
			this.tableLayoutPanelOpenHoursRtl.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelOpenHoursRtl.Size = new System.Drawing.Size(330, 180);
			this.tableLayoutPanelOpenHoursRtl.TabIndex = 2;
			// 
			// WorkloadOpenHours
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.tableLayoutPanelOpenHoursRtl);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "WorkloadOpenHours";
			this.Padding = new System.Windows.Forms.Padding(10);
			this.Size = new System.Drawing.Size(350, 200);
			((System.ComponentModel.ISupportInitialize)(this.weekOpenHoursGridWorkload)).EndInit();
			this.tableLayoutPanelOpenHoursRtl.ResumeLayout(false);
			this.tableLayoutPanelOpenHoursRtl.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblOpenHours;
        private WeekOpenHoursGrid weekOpenHoursGridWorkload;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelOpenHoursRtl;



    }
}
