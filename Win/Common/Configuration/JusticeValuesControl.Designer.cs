﻿namespace Teleopti.Ccc.Win.Common.Configuration
{
    partial class FairnessValuesControl
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
                if (_gridColumnHelper!=null) _gridColumnHelper.Dispose();
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
			this.tableLayoutPanelBody = new System.Windows.Forms.TableLayoutPanel();
			this.grid = new Syncfusion.Windows.Forms.Grid.GridControl();
			this.tableLayoutPanelSubHeader1 = new System.Windows.Forms.TableLayoutPanel();
			this.labelSubHeader1 = new System.Windows.Forms.Label();
			this.gradientPanelHeader = new Syncfusion.Windows.Forms.Tools.GradientPanel();
			this.tableLayoutPanelHeader = new System.Windows.Forms.TableLayoutPanel();
			this.labelHeader = new System.Windows.Forms.Label();
			this.tableLayoutPanelBody.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.tableLayoutPanelSubHeader1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanelHeader)).BeginInit();
			this.gradientPanelHeader.SuspendLayout();
			this.tableLayoutPanelHeader.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanelBody
			// 
			this.tableLayoutPanelBody.BackColor = System.Drawing.SystemColors.Window;
			this.tableLayoutPanelBody.ColumnCount = 1;
			this.tableLayoutPanelBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelBody.Controls.Add(this.grid, 0, 1);
			this.tableLayoutPanelBody.Controls.Add(this.tableLayoutPanelSubHeader1, 0, 0);
			this.tableLayoutPanelBody.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelBody.Location = new System.Drawing.Point(0, 55);
			this.tableLayoutPanelBody.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanelBody.Name = "tableLayoutPanelBody";
			this.tableLayoutPanelBody.RowCount = 2;
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 33F));
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanelBody.Size = new System.Drawing.Size(654, 450);
			this.tableLayoutPanelBody.TabIndex = 59;
			// 
			// grid
			// 
			gridBaseStyle1.Name = "Header";
			gridBaseStyle1.StyleInfo.Borders.Bottom = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
			gridBaseStyle1.StyleInfo.Borders.Left = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
			gridBaseStyle1.StyleInfo.Borders.Right = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
			gridBaseStyle1.StyleInfo.Borders.Top = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
			gridBaseStyle1.StyleInfo.CellType = "Header";
			gridBaseStyle1.StyleInfo.CellValueType = typeof(string);
			gridBaseStyle1.StyleInfo.Font.Bold = true;
			gridBaseStyle1.StyleInfo.Interior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(199)))), ((int)(((byte)(184))))), System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(234)))), ((int)(((byte)(216))))));
			gridBaseStyle1.StyleInfo.VerticalAlignment = Syncfusion.Windows.Forms.Grid.GridVerticalAlignment.Middle;
			gridBaseStyle2.Name = "Standard";
			gridBaseStyle2.StyleInfo.CellType = "CheckBox";
			gridBaseStyle2.StyleInfo.CellValueType = typeof(bool);
			gridBaseStyle2.StyleInfo.CheckBoxOptions.CheckedValue = "True";
			gridBaseStyle2.StyleInfo.CheckBoxOptions.UncheckedValue = "False";
			gridBaseStyle2.StyleInfo.DataSource = null;
			gridBaseStyle2.StyleInfo.Font.Facename = "Tahoma";
			gridBaseStyle2.StyleInfo.HorizontalAlignment = Syncfusion.Windows.Forms.Grid.GridHorizontalAlignment.Center;
			gridBaseStyle2.StyleInfo.Interior = new Syncfusion.Drawing.BrushInfo(System.Drawing.SystemColors.Window);
			gridBaseStyle3.Name = "Row Header";
			gridBaseStyle3.StyleInfo.BaseStyle = "Header";
			gridBaseStyle3.StyleInfo.HorizontalAlignment = Syncfusion.Windows.Forms.Grid.GridHorizontalAlignment.Left;
			gridBaseStyle3.StyleInfo.Interior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Horizontal, System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(199)))), ((int)(((byte)(184))))), System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(234)))), ((int)(((byte)(216))))));
			gridBaseStyle4.Name = "Column Header";
			gridBaseStyle4.StyleInfo.BaseStyle = "Header";
			gridBaseStyle4.StyleInfo.CellValueType = typeof(string);
			gridBaseStyle4.StyleInfo.HorizontalAlignment = Syncfusion.Windows.Forms.Grid.GridHorizontalAlignment.Center;
			this.grid.BaseStylesMap.AddRange(new Syncfusion.Windows.Forms.Grid.GridBaseStyle[] {
            gridBaseStyle1,
            gridBaseStyle2,
            gridBaseStyle3,
            gridBaseStyle4});
			this.grid.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.grid.ColCount = 7;
			this.grid.ColWidthEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridColWidth[] {
            new Syncfusion.Windows.Forms.Grid.GridColWidth(0, 35)});
			this.grid.DefaultGridBorderStyle = Syncfusion.Windows.Forms.Grid.GridBorderStyle.Solid;
			this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.grid.ForeColor = System.Drawing.SystemColors.ControlText;
			this.grid.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Office2007Blue;
			this.grid.Location = new System.Drawing.Point(2, 36);
			this.grid.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
			this.grid.Name = "grid";
			this.grid.Office2007ScrollBars = true;
			this.grid.Properties.BackgroundColor = System.Drawing.SystemColors.Window;
			this.grid.RowCount = 1;
			this.grid.RowHeightEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridRowHeight[] {
            new Syncfusion.Windows.Forms.Grid.GridRowHeight(0, 21)});
			this.grid.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
			this.grid.Size = new System.Drawing.Size(650, 415);
			this.grid.SmartSizeBox = false;
			this.grid.TabIndex = 60;
			this.grid.ThemesEnabled = true;
			this.grid.UseRightToLeftCompatibleTextBox = true;
			// 
			// tableLayoutPanelSubHeader1
			// 
			this.tableLayoutPanelSubHeader1.BackColor = System.Drawing.Color.LightSteelBlue;
			this.tableLayoutPanelSubHeader1.ColumnCount = 1;
			this.tableLayoutPanelSubHeader1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelSubHeader1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanelSubHeader1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanelSubHeader1.Controls.Add(this.labelSubHeader1, 0, 0);
			this.tableLayoutPanelSubHeader1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelSubHeader1.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanelSubHeader1.Name = "tableLayoutPanelSubHeader1";
			this.tableLayoutPanelSubHeader1.RowCount = 1;
			this.tableLayoutPanelSubHeader1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelSubHeader1.Size = new System.Drawing.Size(648, 27);
			this.tableLayoutPanelSubHeader1.TabIndex = 59;
			// 
			// labelSubHeader1
			// 
			this.labelSubHeader1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelSubHeader1.AutoSize = true;
			this.labelSubHeader1.BackColor = System.Drawing.Color.Transparent;
			this.labelSubHeader1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
			this.labelSubHeader1.ForeColor = System.Drawing.Color.GhostWhite;
			this.labelSubHeader1.Location = new System.Drawing.Point(3, 7);
			this.labelSubHeader1.Name = "labelSubHeader1";
			this.labelSubHeader1.Size = new System.Drawing.Size(254, 13);
			this.labelSubHeader1.TabIndex = 0;
			this.labelSubHeader1.Text = "xxEnterValuesForTheDifferentCombinations";
			this.labelSubHeader1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// gradientPanelHeader
			// 
			this.gradientPanelHeader.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.gradientPanelHeader.BackColor = System.Drawing.SystemColors.ActiveCaption;
			this.gradientPanelHeader.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.LightSteelBlue, System.Drawing.Color.White);
			this.gradientPanelHeader.BorderSingle = System.Windows.Forms.ButtonBorderStyle.None;
			this.gradientPanelHeader.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.gradientPanelHeader.Controls.Add(this.tableLayoutPanelHeader);
			this.gradientPanelHeader.Dock = System.Windows.Forms.DockStyle.Top;
			this.gradientPanelHeader.Location = new System.Drawing.Point(0, 0);
			this.gradientPanelHeader.Margin = new System.Windows.Forms.Padding(0, 0, 0, 3);
			this.gradientPanelHeader.Name = "gradientPanelHeader";
			this.gradientPanelHeader.Padding = new System.Windows.Forms.Padding(10);
			this.gradientPanelHeader.Size = new System.Drawing.Size(654, 55);
			this.gradientPanelHeader.TabIndex = 60;
			// 
			// tableLayoutPanelHeader
			// 
			this.tableLayoutPanelHeader.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanelHeader.ColumnCount = 1;
			this.tableLayoutPanelHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 634F));
			this.tableLayoutPanelHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelHeader.Controls.Add(this.labelHeader, 1, 0);
			this.tableLayoutPanelHeader.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelHeader.Location = new System.Drawing.Point(10, 10);
			this.tableLayoutPanelHeader.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanelHeader.Name = "tableLayoutPanelHeader";
			this.tableLayoutPanelHeader.RowCount = 1;
			this.tableLayoutPanelHeader.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelHeader.Size = new System.Drawing.Size(634, 35);
			this.tableLayoutPanelHeader.TabIndex = 0;
			// 
			// labelHeader
			// 
			this.labelHeader.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelHeader.AutoSize = true;
			this.labelHeader.Font = new System.Drawing.Font("Tahoma", 11.25F);
			this.labelHeader.ForeColor = System.Drawing.Color.MidnightBlue;
			this.labelHeader.Location = new System.Drawing.Point(3, 8);
			this.labelHeader.Name = "labelHeader";
			this.labelHeader.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
			this.labelHeader.Size = new System.Drawing.Size(166, 18);
			this.labelHeader.TabIndex = 0;
			this.labelHeader.Text = "xxManageJusticeValues";
			// 
			// FairnessValuesControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanelBody);
			this.Controls.Add(this.gradientPanelHeader);
			this.Name = "FairnessValuesControl";
			this.Size = new System.Drawing.Size(654, 505);
			this.tableLayoutPanelBody.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.tableLayoutPanelSubHeader1.ResumeLayout(false);
			this.tableLayoutPanelSubHeader1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanelHeader)).EndInit();
			this.gradientPanelHeader.ResumeLayout(false);
			this.tableLayoutPanelHeader.ResumeLayout(false);
			this.tableLayoutPanelHeader.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelBody;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelSubHeader1;
		private System.Windows.Forms.Label labelSubHeader1;
        private Syncfusion.Windows.Forms.Grid.GridControl grid;
		private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanelHeader;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelHeader;
		private System.Windows.Forms.Label labelHeader;
    }
}
