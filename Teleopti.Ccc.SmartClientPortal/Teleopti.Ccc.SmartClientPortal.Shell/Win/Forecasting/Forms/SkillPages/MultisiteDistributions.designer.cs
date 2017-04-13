using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Forecasting.Forms.SkillPages
{
	partial class MultisiteDistributions
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
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle1 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle2 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle3 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle4 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			Syncfusion.Windows.Forms.Grid.GridRangeStyle gridRangeStyle1 = new Syncfusion.Windows.Forms.Grid.GridRangeStyle();
			Syncfusion.Windows.Forms.Grid.GridRangeStyle gridRangeStyle2 = new Syncfusion.Windows.Forms.Grid.GridRangeStyle();
			Syncfusion.Windows.Forms.Grid.GridRangeStyle gridRangeStyle3 = new Syncfusion.Windows.Forms.Grid.GridRangeStyle();
			this.tableLayoutPanelDistributionsRtl = new System.Windows.Forms.TableLayoutPanel();
			this.lblSubSkillsDistribution = new System.Windows.Forms.Label();
			this.gridSubSkills = new Teleopti.Ccc.Win.Forecasting.Forms.MultisiteDistributionGrid();
			this.tableLayoutPanelDistributionsRtl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridSubSkills)).BeginInit();
			this.SuspendLayout();
			// 
			// tableLayoutPanelDistributionsRtl
			// 
			this.tableLayoutPanelDistributionsRtl.AutoSize = true;
			this.tableLayoutPanelDistributionsRtl.ColumnCount = 1;
			this.tableLayoutPanelDistributionsRtl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F));
			this.tableLayoutPanelDistributionsRtl.Controls.Add(this.lblSubSkillsDistribution, 0, 0);
			this.tableLayoutPanelDistributionsRtl.Controls.Add(this.gridSubSkills, 0, 1);
			this.tableLayoutPanelDistributionsRtl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelDistributionsRtl.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanelDistributionsRtl.Name = "tableLayoutPanelDistributionsRtl";
			this.tableLayoutPanelDistributionsRtl.RowCount = 2;
			this.tableLayoutPanelDistributionsRtl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 31F));
			this.tableLayoutPanelDistributionsRtl.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelDistributionsRtl.Size = new System.Drawing.Size(433, 258);
			this.tableLayoutPanelDistributionsRtl.TabIndex = 4;
			// 
			// lblSubSkillsDistribution
			// 
			this.lblSubSkillsDistribution.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.lblSubSkillsDistribution.AutoSize = true;
			this.lblSubSkillsDistribution.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			this.lblSubSkillsDistribution.Location = new System.Drawing.Point(3, 7);
			this.lblSubSkillsDistribution.Name = "lblSubSkillsDistribution";
			this.lblSubSkillsDistribution.Size = new System.Drawing.Size(171, 17);
			this.lblSubSkillsDistribution.TabIndex = 4;
			this.lblSubSkillsDistribution.Text = "xxSubSkillDistributionsColon";
			// 
			// gridSubSkills
			// 
			this.gridSubSkills.AccessibilityEnabled = true;
			this.gridSubSkills.ActivateCurrentCellBehavior = Syncfusion.Windows.Forms.Grid.GridCellActivateAction.DblClickOnCell;
			this.gridSubSkills.AllowSelection = ((Syncfusion.Windows.Forms.Grid.GridSelectionFlags)(((((Syncfusion.Windows.Forms.Grid.GridSelectionFlags.Cell | Syncfusion.Windows.Forms.Grid.GridSelectionFlags.Multiple) 
			| Syncfusion.Windows.Forms.Grid.GridSelectionFlags.Shift) 
			| Syncfusion.Windows.Forms.Grid.GridSelectionFlags.Keyboard) 
			| Syncfusion.Windows.Forms.Grid.GridSelectionFlags.AlphaBlend)));
			this.gridSubSkills.AlphaBlendSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(94)))), ((int)(((byte)(171)))), ((int)(((byte)(222)))));
			this.gridSubSkills.BackColor = System.Drawing.Color.White;
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
			gridBaseStyle2.StyleInfo.CellType = "TextBox";
			gridBaseStyle2.StyleInfo.Font.Facename = "Tahoma";
			gridBaseStyle2.StyleInfo.Interior = new Syncfusion.Drawing.BrushInfo(System.Drawing.SystemColors.Window);
			gridBaseStyle3.Name = "Column Header";
			gridBaseStyle3.StyleInfo.BaseStyle = "Header";
			gridBaseStyle3.StyleInfo.HorizontalAlignment = Syncfusion.Windows.Forms.Grid.GridHorizontalAlignment.Center;
			gridBaseStyle4.Name = "Row Header";
			gridBaseStyle4.StyleInfo.BaseStyle = "Header";
			gridBaseStyle4.StyleInfo.HorizontalAlignment = Syncfusion.Windows.Forms.Grid.GridHorizontalAlignment.Left;
			gridBaseStyle4.StyleInfo.Interior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Horizontal, System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(199)))), ((int)(((byte)(184))))), System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(234)))), ((int)(((byte)(216))))));
			this.gridSubSkills.BaseStylesMap.AddRange(new Syncfusion.Windows.Forms.Grid.GridBaseStyle[] {
			gridBaseStyle1,
			gridBaseStyle2,
			gridBaseStyle3,
			gridBaseStyle4});
			this.gridSubSkills.ColCount = 2;
			this.gridSubSkills.ColWidthEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridColWidth[] {
			new Syncfusion.Windows.Forms.Grid.GridColWidth(0, 35),
			new Syncfusion.Windows.Forms.Grid.GridColWidth(1, 70),
			new Syncfusion.Windows.Forms.Grid.GridColWidth(2, 100)});
			this.gridSubSkills.DefaultGridBorderStyle = Syncfusion.Windows.Forms.Grid.GridBorderStyle.Solid;
			this.gridSubSkills.DefaultRowHeight = 20;
			this.gridSubSkills.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridSubSkills.ExcelLikeCurrentCell = true;
			this.gridSubSkills.ExcelLikeSelectionFrame = true;
			this.gridSubSkills.Font = new System.Drawing.Font("Arial", 8.25F);
			this.gridSubSkills.GridOfficeScrollBars = Syncfusion.Windows.Forms.OfficeScrollBars.Metro;
			this.gridSubSkills.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Metro;
			this.gridSubSkills.Location = new System.Drawing.Point(3, 34);
			this.gridSubSkills.MetroScrollBars = true;
			this.gridSubSkills.Name = "gridSubSkills";
			this.gridSubSkills.NumberedColHeaders = false;
			this.gridSubSkills.NumberedRowHeaders = false;
			this.gridSubSkills.Properties.BackgroundColor = System.Drawing.Color.White;
			this.gridSubSkills.Properties.ForceImmediateRepaint = false;
			this.gridSubSkills.Properties.GridLineColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(212)))), ((int)(((byte)(212)))));
			this.gridSubSkills.Properties.MarkColHeader = false;
			this.gridSubSkills.Properties.MarkRowHeader = false;
			this.gridSubSkills.Properties.RowHeaders = false;
			gridRangeStyle1.Range = Syncfusion.Windows.Forms.Grid.GridRangeInfo.Table();
			gridRangeStyle1.StyleInfo.Font.Bold = false;
			gridRangeStyle1.StyleInfo.Font.Facename = "Arial";
			gridRangeStyle1.StyleInfo.Font.Italic = false;
			gridRangeStyle1.StyleInfo.Font.Size = 8.25F;
			gridRangeStyle1.StyleInfo.Font.Strikeout = false;
			gridRangeStyle1.StyleInfo.Font.Underline = false;
			gridRangeStyle1.StyleInfo.Font.Unit = System.Drawing.GraphicsUnit.Point;
			gridRangeStyle2.Range = Syncfusion.Windows.Forms.Grid.GridRangeInfo.Cell(0, 1);
			gridRangeStyle2.StyleInfo.CellType = "Header";
			gridRangeStyle2.StyleInfo.Text = "xxName";
			gridRangeStyle3.Range = Syncfusion.Windows.Forms.Grid.GridRangeInfo.Cell(0, 2);
			gridRangeStyle3.StyleInfo.CellType = "Header";
			gridRangeStyle3.StyleInfo.Text = "xxPercentage";
			this.gridSubSkills.RangeStyles.AddRange(new Syncfusion.Windows.Forms.Grid.GridRangeStyle[] {
			gridRangeStyle1,
			gridRangeStyle2,
			gridRangeStyle3});
			this.gridSubSkills.RowCount = 7;
			this.gridSubSkills.RowHeightEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridRowHeight[] {
			new Syncfusion.Windows.Forms.Grid.GridRowHeight(0, 29)});
			this.gridSubSkills.SelectCellsMouseButtonsMask = System.Windows.Forms.MouseButtons.Left;
			this.gridSubSkills.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
			this.gridSubSkills.Size = new System.Drawing.Size(427, 221);
			this.gridSubSkills.SmartSizeBox = false;
			this.gridSubSkills.TabIndex = 1;
			this.gridSubSkills.Text = "gridControl1";
			this.gridSubSkills.ThemesEnabled = true;
			this.gridSubSkills.UseRightToLeftCompatibleTextBox = true;
			// 
			// MultisiteDistributions
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.tableLayoutPanelDistributionsRtl);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.Name = "MultisiteDistributions";
			this.Size = new System.Drawing.Size(433, 258);
			this.tableLayoutPanelDistributionsRtl.ResumeLayout(false);
			this.tableLayoutPanelDistributionsRtl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridSubSkills)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private MultisiteDistributionGrid gridSubSkills;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelDistributionsRtl;
		private System.Windows.Forms.Label lblSubSkillsDistribution;



	}
}