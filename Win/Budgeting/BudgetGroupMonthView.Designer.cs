namespace Teleopti.Ccc.Win.Budgeting
{
	partial class BudgetGroupMonthView
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
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle1 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle2 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle3 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle4 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			this.gridControlMonthView = new Syncfusion.Windows.Forms.Grid.GridControl();
			this.budgetGroupMonthViewMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.toolStripMenuItemAddShrinkageRow = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemUpdateShrinkageRow = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemDeleteShrinkageRow = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemAddEfficiencyShrinkageRow = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemUpdateEfficiencyShrinkageRow = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemDeleteEfficiencyShrinkageRow = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemLoadForecast = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemLoadStaffEmployed = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemModifySelection = new System.Windows.Forms.ToolStripMenuItem();
			((System.ComponentModel.ISupportInitialize)(this.gridControlMonthView)).BeginInit();
			this.budgetGroupMonthViewMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// gridControlMonthView
			// 
			this.gridControlMonthView.ActivateCurrentCellBehavior = Syncfusion.Windows.Forms.Grid.GridCellActivateAction.DblClickOnCell;
			this.gridControlMonthView.AlphaBlendSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(94)))), ((int)(((byte)(171)))), ((int)(((byte)(222)))));
			this.gridControlMonthView.BackColor = System.Drawing.Color.White;
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
			gridBaseStyle3.Name = "Column Header";
			gridBaseStyle3.StyleInfo.BaseStyle = "Header";
			gridBaseStyle3.StyleInfo.HorizontalAlignment = Syncfusion.Windows.Forms.Grid.GridHorizontalAlignment.Center;
			gridBaseStyle4.Name = "Row Header";
			gridBaseStyle4.StyleInfo.BaseStyle = "Header";
			gridBaseStyle4.StyleInfo.HorizontalAlignment = Syncfusion.Windows.Forms.Grid.GridHorizontalAlignment.Left;
			gridBaseStyle4.StyleInfo.Interior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Horizontal, System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(199)))), ((int)(((byte)(184))))), System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(234)))), ((int)(((byte)(216))))));
			this.gridControlMonthView.BaseStylesMap.AddRange(new Syncfusion.Windows.Forms.Grid.GridBaseStyle[] {
            gridBaseStyle1,
            gridBaseStyle2,
            gridBaseStyle3,
            gridBaseStyle4});
			this.gridControlMonthView.ColWidthEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridColWidth[] {
            new Syncfusion.Windows.Forms.Grid.GridColWidth(0, 35)});
			this.gridControlMonthView.ContextMenuStrip = this.budgetGroupMonthViewMenu;
			this.gridControlMonthView.DefaultGridBorderStyle = Syncfusion.Windows.Forms.Grid.GridBorderStyle.Solid;
			this.gridControlMonthView.DefaultRowHeight = 20;
			this.gridControlMonthView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridControlMonthView.ExcelLikeCurrentCell = true;
			this.gridControlMonthView.ExcelLikeSelectionFrame = true;
			this.gridControlMonthView.GridOfficeScrollBars = Syncfusion.Windows.Forms.OfficeScrollBars.Metro;
			this.gridControlMonthView.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Metro;
			this.gridControlMonthView.HorizontalThumbTrack = true;
			this.gridControlMonthView.Location = new System.Drawing.Point(0, 0);
			this.gridControlMonthView.MetroScrollBars = true;
			this.gridControlMonthView.MinResizeColSize = 5;
			this.gridControlMonthView.Name = "gridControlMonthView";
			this.gridControlMonthView.Office2007ScrollBarsColorScheme = Syncfusion.Windows.Forms.Office2007ColorScheme.Managed;
			this.gridControlMonthView.Properties.BackgroundColor = System.Drawing.Color.White;
			this.gridControlMonthView.Properties.ForceImmediateRepaint = false;
			this.gridControlMonthView.Properties.GridLineColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(212)))), ((int)(((byte)(212)))));
			this.gridControlMonthView.Properties.MarkColHeader = false;
			this.gridControlMonthView.Properties.MarkRowHeader = false;
			this.gridControlMonthView.ResizeRowsBehavior = Syncfusion.Windows.Forms.Grid.GridResizeCellsBehavior.None;
			this.gridControlMonthView.RowHeightEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridRowHeight[] {
            new Syncfusion.Windows.Forms.Grid.GridRowHeight(0, 29)});
			this.gridControlMonthView.SelectCellsMouseButtonsMask = System.Windows.Forms.MouseButtons.Left;
			this.gridControlMonthView.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
			this.gridControlMonthView.Size = new System.Drawing.Size(665, 248);
			this.gridControlMonthView.SmartSizeBox = false;
			this.gridControlMonthView.TabIndex = 1;
			this.gridControlMonthView.Text = "gridControl1";
			this.gridControlMonthView.ThemesEnabled = true;
			this.gridControlMonthView.UseRightToLeftCompatibleTextBox = true;
			this.gridControlMonthView.VerticalThumbTrack = true;
			this.gridControlMonthView.SelectionChanged += new Syncfusion.Windows.Forms.Grid.GridSelectionChangedEventHandler(this.gridControlMonthView_SelectionChanged);
			this.gridControlMonthView.ClipboardCopy += new Syncfusion.Windows.Forms.Grid.GridCutPasteEventHandler(this.gridControlMonthView_ClipboardCopy);
			this.gridControlMonthView.ClipboardCut += new Syncfusion.Windows.Forms.Grid.GridCutPasteEventHandler(this.gridControlMonthView_ClipboardCut);
			// 
			// budgetGroupMonthViewMenu
			// 
			this.budgetGroupMonthViewMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemAddShrinkageRow,
            this.toolStripMenuItemUpdateShrinkageRow,
            this.toolStripMenuItemDeleteShrinkageRow,
            this.toolStripSeparator1,
            this.toolStripMenuItemAddEfficiencyShrinkageRow,
            this.toolStripMenuItemUpdateEfficiencyShrinkageRow,
            this.toolStripMenuItemDeleteEfficiencyShrinkageRow,
            this.toolStripSeparator2,
            this.toolStripMenuItemLoadForecast,
            this.toolStripMenuItemLoadStaffEmployed,
            this.toolStripSeparator3,
            this.toolStripMenuItemModifySelection});
			this.budgetGroupMonthViewMenu.Name = "budgetGroupMonthViewMenu";
			this.budgetGroupMonthViewMenu.Size = new System.Drawing.Size(249, 220);
			this.budgetGroupMonthViewMenu.Opening += new System.ComponentModel.CancelEventHandler(this.budgetGroupMonthViewMenu_Opening);
			// 
			// toolStripMenuItemAddShrinkageRow
			// 
			this.toolStripMenuItemAddShrinkageRow.Name = "toolStripMenuItemAddShrinkageRow";
			this.toolStripMenuItemAddShrinkageRow.Size = new System.Drawing.Size(248, 22);
			this.toolStripMenuItemAddShrinkageRow.Text = "xxAddShrinkageRow";
			this.toolStripMenuItemAddShrinkageRow.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemAddShrinkageRow.Click += new System.EventHandler(this.toolStripMenuItemAddShrinkageRow_Click);
			// 
			// toolStripMenuItemUpdateShrinkageRow
			// 
			this.toolStripMenuItemUpdateShrinkageRow.Enabled = false;
			this.toolStripMenuItemUpdateShrinkageRow.Name = "toolStripMenuItemUpdateShrinkageRow";
			this.toolStripMenuItemUpdateShrinkageRow.Size = new System.Drawing.Size(248, 22);
			this.toolStripMenuItemUpdateShrinkageRow.Text = "xxUpdateShrinkageRow";
			this.toolStripMenuItemUpdateShrinkageRow.Click += new System.EventHandler(this.toolStripMenuItemRenameShrinkageRow_Click);
			// 
			// toolStripMenuItemDeleteShrinkageRow
			// 
			this.toolStripMenuItemDeleteShrinkageRow.Enabled = false;
			this.toolStripMenuItemDeleteShrinkageRow.Name = "toolStripMenuItemDeleteShrinkageRow";
			this.toolStripMenuItemDeleteShrinkageRow.Size = new System.Drawing.Size(248, 22);
			this.toolStripMenuItemDeleteShrinkageRow.Text = "xxDeleteShrinkageRow";
			this.toolStripMenuItemDeleteShrinkageRow.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemDeleteShrinkageRow.Click += new System.EventHandler(this.toolStripMenuItemDeleteShrinkageRow_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(245, 6);
			// 
			// toolStripMenuItemAddEfficiencyShrinkageRow
			// 
			this.toolStripMenuItemAddEfficiencyShrinkageRow.Name = "toolStripMenuItemAddEfficiencyShrinkageRow";
			this.toolStripMenuItemAddEfficiencyShrinkageRow.Size = new System.Drawing.Size(248, 22);
			this.toolStripMenuItemAddEfficiencyShrinkageRow.Text = "xxAddEfficiencyShrinkageRow";
			this.toolStripMenuItemAddEfficiencyShrinkageRow.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemAddEfficiencyShrinkageRow.Click += new System.EventHandler(this.toolStripMenuItemAddEfficiencyShrinkageRow_Click);
			// 
			// toolStripMenuItemUpdateEfficiencyShrinkageRow
			// 
			this.toolStripMenuItemUpdateEfficiencyShrinkageRow.Enabled = false;
			this.toolStripMenuItemUpdateEfficiencyShrinkageRow.Name = "toolStripMenuItemUpdateEfficiencyShrinkageRow";
			this.toolStripMenuItemUpdateEfficiencyShrinkageRow.Size = new System.Drawing.Size(248, 22);
			this.toolStripMenuItemUpdateEfficiencyShrinkageRow.Text = "xxUpdateEfficiencyShrinkageRow";
			this.toolStripMenuItemUpdateEfficiencyShrinkageRow.Click += new System.EventHandler(this.toolStripMenuRenameEfficiencyShrinkageRow_Click);
			// 
			// toolStripMenuItemDeleteEfficiencyShrinkageRow
			// 
			this.toolStripMenuItemDeleteEfficiencyShrinkageRow.Enabled = false;
			this.toolStripMenuItemDeleteEfficiencyShrinkageRow.Name = "toolStripMenuItemDeleteEfficiencyShrinkageRow";
			this.toolStripMenuItemDeleteEfficiencyShrinkageRow.Size = new System.Drawing.Size(248, 22);
			this.toolStripMenuItemDeleteEfficiencyShrinkageRow.Text = "xxDeleteEfficiencyShrinkageRow";
			this.toolStripMenuItemDeleteEfficiencyShrinkageRow.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemDeleteEfficiencyShrinkageRow.Click += new System.EventHandler(this.toolStripMenuItemDeleteEfficiencyShrinkageRow_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(245, 6);
			// 
			// toolStripMenuItemLoadForecast
			// 
			this.toolStripMenuItemLoadForecast.Name = "toolStripMenuItemLoadForecast";
			this.toolStripMenuItemLoadForecast.Size = new System.Drawing.Size(248, 22);
			this.toolStripMenuItemLoadForecast.Text = "xxLoadForecastedHours";
			this.toolStripMenuItemLoadForecast.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemLoadForecast.Click += new System.EventHandler(this.toolStripMenuItemLoadForecast_Click);
			// 
			// toolStripMenuItemLoadStaffEmployed
			// 
			this.toolStripMenuItemLoadStaffEmployed.Name = "toolStripMenuItemLoadStaffEmployed";
			this.toolStripMenuItemLoadStaffEmployed.Size = new System.Drawing.Size(248, 22);
			this.toolStripMenuItemLoadStaffEmployed.Text = "xxLoadStaffEmployed";
			this.toolStripMenuItemLoadStaffEmployed.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemLoadStaffEmployed.Click += new System.EventHandler(this.toolStripMenuItemLoadStaffEmployed_Click);
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(245, 6);
			// 
			// toolStripMenuItemModifySelection
			// 
			this.toolStripMenuItemModifySelection.Name = "toolStripMenuItemModifySelection";
			this.toolStripMenuItemModifySelection.Size = new System.Drawing.Size(248, 22);
			this.toolStripMenuItemModifySelection.Text = "xxModifySelection";
			this.toolStripMenuItemModifySelection.Click += new System.EventHandler(this.toolStripMenuItemModifySelection_Click);
			this.toolStripMenuItemModifySelection.Paint += new System.Windows.Forms.PaintEventHandler(this.toolStripMenuItemModifySelection_Paint);
			// 
			// BudgetGroupMonthView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.gridControlMonthView);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "BudgetGroupMonthView";
			this.Size = new System.Drawing.Size(665, 248);
			((System.ComponentModel.ISupportInitialize)(this.gridControlMonthView)).EndInit();
			this.budgetGroupMonthViewMenu.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Syncfusion.Windows.Forms.Grid.GridControl gridControlMonthView;
		private System.Windows.Forms.ContextMenuStrip budgetGroupMonthViewMenu;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemAddShrinkageRow;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDeleteShrinkageRow;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemAddEfficiencyShrinkageRow;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDeleteEfficiencyShrinkageRow;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemLoadForecast;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemLoadStaffEmployed;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemModifySelection;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemUpdateShrinkageRow;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemUpdateEfficiencyShrinkageRow;
	}
}
