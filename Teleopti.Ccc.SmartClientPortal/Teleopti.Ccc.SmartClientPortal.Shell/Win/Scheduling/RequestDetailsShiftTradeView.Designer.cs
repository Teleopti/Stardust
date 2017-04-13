namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
	partial class RequestDetailsShiftTradeView
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private void InitializeComponent()
		{
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle1 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle2 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle3 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle4 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			this.gridControlSchedule = new Syncfusion.Windows.Forms.Grid.GridControl();
			((System.ComponentModel.ISupportInitialize)(this.gridControlSchedule)).BeginInit();
			this.SuspendLayout();
			// 
			// gridControlSchedule
			// 
			gridBaseStyle1.Name = "Column Header";
			gridBaseStyle1.StyleInfo.BaseStyle = "Header";
			gridBaseStyle1.StyleInfo.HorizontalAlignment = Syncfusion.Windows.Forms.Grid.GridHorizontalAlignment.Center;
			gridBaseStyle2.Name = "Header";
			gridBaseStyle2.StyleInfo.Borders.Bottom = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
			gridBaseStyle2.StyleInfo.Borders.Left = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
			gridBaseStyle2.StyleInfo.Borders.Right = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
			gridBaseStyle2.StyleInfo.Borders.Top = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
			gridBaseStyle2.StyleInfo.CellType = "Header";
			gridBaseStyle2.StyleInfo.Font.Bold = true;
			gridBaseStyle2.StyleInfo.Interior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(199)))), ((int)(((byte)(184))))), System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(234)))), ((int)(((byte)(216))))));
			gridBaseStyle2.StyleInfo.MergeCell = Syncfusion.Windows.Forms.Grid.GridMergeCellDirection.RowsInColumn;
			gridBaseStyle2.StyleInfo.VerticalAlignment = Syncfusion.Windows.Forms.Grid.GridVerticalAlignment.Middle;
			gridBaseStyle3.Name = "Standard";
			gridBaseStyle3.StyleInfo.Font.Facename = "Tahoma";
			gridBaseStyle3.StyleInfo.Interior = new Syncfusion.Drawing.BrushInfo(System.Drawing.SystemColors.Window);
			gridBaseStyle4.Name = "Row Header";
			gridBaseStyle4.StyleInfo.BaseStyle = "Header";
			gridBaseStyle4.StyleInfo.HorizontalAlignment = Syncfusion.Windows.Forms.Grid.GridHorizontalAlignment.Left;
			gridBaseStyle4.StyleInfo.Interior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Horizontal, System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(199)))), ((int)(((byte)(184))))), System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(234)))), ((int)(((byte)(216))))));
			this.gridControlSchedule.BaseStylesMap.AddRange(new Syncfusion.Windows.Forms.Grid.GridBaseStyle[] {
            gridBaseStyle1,
            gridBaseStyle2,
            gridBaseStyle3,
            gridBaseStyle4});
			this.gridControlSchedule.ColCount = 2;
			this.gridControlSchedule.ColWidthEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridColWidth[] {
            new Syncfusion.Windows.Forms.Grid.GridColWidth(0, 85),
            new Syncfusion.Windows.Forms.Grid.GridColWidth(1, 120)});
			this.gridControlSchedule.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridControlSchedule.ForeColor = System.Drawing.SystemColors.ControlText;
			this.gridControlSchedule.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Office2007Blue;
			this.gridControlSchedule.HScrollPixel = true;
			this.gridControlSchedule.Location = new System.Drawing.Point(0, 0);
			this.gridControlSchedule.Margin = new System.Windows.Forms.Padding(0);
			this.gridControlSchedule.Name = "gridControlSchedule";
			this.gridControlSchedule.NumberedColHeaders = false;
			this.gridControlSchedule.NumberedRowHeaders = false;
			this.gridControlSchedule.ResizeColsBehavior = ((Syncfusion.Windows.Forms.Grid.GridResizeCellsBehavior)((((Syncfusion.Windows.Forms.Grid.GridResizeCellsBehavior.ResizeSingle | Syncfusion.Windows.Forms.Grid.GridResizeCellsBehavior.OutlineHeaders)
						| Syncfusion.Windows.Forms.Grid.GridResizeCellsBehavior.OutlineBounds)
						| Syncfusion.Windows.Forms.Grid.GridResizeCellsBehavior.AllowDragOutside)));
			this.gridControlSchedule.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.gridControlSchedule.RowCount = 1;
			this.gridControlSchedule.RowHeightEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridRowHeight[] {
            new Syncfusion.Windows.Forms.Grid.GridRowHeight(0, 21)});
			this.gridControlSchedule.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
			this.gridControlSchedule.Size = new System.Drawing.Size(512, 135);
			this.gridControlSchedule.SmartSizeBox = false;
			this.gridControlSchedule.TabIndex = 0;
			this.gridControlSchedule.UseRightToLeftCompatibleTextBox = true;
			// 
			// RequestDetailsShiftTradeView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.gridControlSchedule);
			this.Name = "RequestDetailsShiftTradeView";
			this.Size = new System.Drawing.Size(512, 135);
			((System.ComponentModel.ISupportInitialize)(this.gridControlSchedule)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Syncfusion.Windows.Forms.Grid.GridControl gridControlSchedule;

	}
}
