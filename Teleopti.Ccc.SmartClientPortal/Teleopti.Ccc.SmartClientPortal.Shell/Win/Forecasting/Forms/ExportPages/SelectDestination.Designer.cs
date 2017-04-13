using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Forecasting.Forms.ExportPages
{
    partial class SelectDestination
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
                foreach (GridBoundColumn gridBoundColumn in gridControlDestination.GridBoundColumns)
                {
                    gridBoundColumn.Dispose();
                }
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
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle1 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle2 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle3 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle4 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			this.gridControlDestination = new Syncfusion.Windows.Forms.Grid.GridDataBoundGrid();
			((System.ComponentModel.ISupportInitialize)(this.gridControlDestination)).BeginInit();
			this.SuspendLayout();
			// 
			// gridControlDestination
			// 
			this.gridControlDestination.AllowDragSelectedCols = true;
			this.gridControlDestination.AlphaBlendSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(94)))), ((int)(((byte)(171)))), ((int)(((byte)(222)))));
			gridBaseStyle1.Name = "Column Header";
			gridBaseStyle1.StyleInfo.BaseStyle = "Header";
			gridBaseStyle1.StyleInfo.CellType = "ColumnHeaderCell";
			gridBaseStyle1.StyleInfo.Enabled = false;
			gridBaseStyle1.StyleInfo.Font.Bold = true;
			gridBaseStyle1.StyleInfo.HorizontalAlignment = Syncfusion.Windows.Forms.Grid.GridHorizontalAlignment.Center;
			gridBaseStyle2.Name = "Header";
			gridBaseStyle2.StyleInfo.Borders.Bottom = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
			gridBaseStyle2.StyleInfo.Borders.Left = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
			gridBaseStyle2.StyleInfo.Borders.Right = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
			gridBaseStyle2.StyleInfo.Borders.Top = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
			gridBaseStyle2.StyleInfo.CellType = "Header";
			gridBaseStyle2.StyleInfo.Font.Bold = true;
			gridBaseStyle2.StyleInfo.Interior = new Syncfusion.Drawing.BrushInfo(System.Drawing.SystemColors.Control);
			gridBaseStyle2.StyleInfo.VerticalAlignment = Syncfusion.Windows.Forms.Grid.GridVerticalAlignment.Middle;
			gridBaseStyle3.Name = "Standard";
			gridBaseStyle3.StyleInfo.CheckBoxOptions.CheckedValue = "True";
			gridBaseStyle3.StyleInfo.CheckBoxOptions.UncheckedValue = "False";
			gridBaseStyle3.StyleInfo.Interior = new Syncfusion.Drawing.BrushInfo(System.Drawing.SystemColors.Window);
			gridBaseStyle4.Name = "Row Header";
			gridBaseStyle4.StyleInfo.BaseStyle = "Header";
			gridBaseStyle4.StyleInfo.CellType = "RowHeaderCell";
			gridBaseStyle4.StyleInfo.Enabled = true;
			gridBaseStyle4.StyleInfo.HorizontalAlignment = Syncfusion.Windows.Forms.Grid.GridHorizontalAlignment.Left;
			this.gridControlDestination.BaseStylesMap.AddRange(new Syncfusion.Windows.Forms.Grid.GridBaseStyle[] {
            gridBaseStyle1,
            gridBaseStyle2,
            gridBaseStyle3,
            gridBaseStyle4});
			this.gridControlDestination.DefaultRowHeight = 20;
			this.gridControlDestination.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridControlDestination.GridOfficeScrollBars = Syncfusion.Windows.Forms.OfficeScrollBars.Metro;
			this.gridControlDestination.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Metro;
			this.gridControlDestination.Location = new System.Drawing.Point(0, 0);
			this.gridControlDestination.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.gridControlDestination.MetroScrollBars = true;
			this.gridControlDestination.Name = "gridControlDestination";
			this.gridControlDestination.Properties.ForceImmediateRepaint = false;
			this.gridControlDestination.Properties.GridLineColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(234)))), ((int)(((byte)(234)))));
			this.gridControlDestination.Properties.MarkColHeader = false;
			this.gridControlDestination.Properties.MarkRowHeader = false;
			this.gridControlDestination.ShowCurrentCellBorderBehavior = Syncfusion.Windows.Forms.Grid.GridShowCurrentCellBorder.GrayWhenLostFocus;
			this.gridControlDestination.Size = new System.Drawing.Size(343, 316);
			this.gridControlDestination.SmartSizeBox = false;
			this.gridControlDestination.SortBehavior = Syncfusion.Windows.Forms.Grid.GridSortBehavior.DoubleClick;
			this.gridControlDestination.TabIndex = 0;
			this.gridControlDestination.Text = "gridControlDestination";
			this.gridControlDestination.ThemesEnabled = true;
			this.gridControlDestination.UseRightToLeftCompatibleTextBox = true;
			// 
			// SelectDestination
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.gridControlDestination);
			this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.Name = "SelectDestination";
			this.Size = new System.Drawing.Size(343, 316);
			((System.ComponentModel.ISupportInitialize)(this.gridControlDestination)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Grid.GridDataBoundGrid gridControlDestination;
    }
}
