using System.Globalization;
using System.Windows.Forms;

namespace Teleopti.Ccc.Win.Scheduling
{
    partial class RequestAllowanceView
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Syncfusion.Windows.Forms.Tools.RibbonControlAdv.set_MinimizeToolTip(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Syncfusion.Windows.Forms.Tools.RibbonControlAdv.set_MaximizeToolTip(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxRefresh")]
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle1 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle2 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle3 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle4 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			Syncfusion.Windows.Forms.Grid.GridRangeStyle gridRangeStyle1 = new Syncfusion.Windows.Forms.Grid.GridRangeStyle();
			Syncfusion.Windows.Forms.Grid.GridRangeStyle gridRangeStyle2 = new Syncfusion.Windows.Forms.Grid.GridRangeStyle();
			Syncfusion.Windows.Forms.Grid.GridRangeStyle gridRangeStyle3 = new Syncfusion.Windows.Forms.Grid.GridRangeStyle();
			Syncfusion.Windows.Forms.Grid.GridRangeStyle gridRangeStyle4 = new Syncfusion.Windows.Forms.Grid.GridRangeStyle();
			Syncfusion.Windows.Forms.Grid.GridRangeStyle gridRangeStyle5 = new Syncfusion.Windows.Forms.Grid.GridRangeStyle();
			Syncfusion.Windows.Forms.Grid.GridRangeStyle gridRangeStyle6 = new Syncfusion.Windows.Forms.Grid.GridRangeStyle();
			this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.buttonRefresh = new Syncfusion.Windows.Forms.ButtonAdv();
			this.labelBudgetGroup = new System.Windows.Forms.Label();
			this.labelAllowance = new System.Windows.Forms.Label();
			this.radioButtonTotalAllowance = new Syncfusion.Windows.Forms.Tools.RadioButtonAdv();
			this.radioButtonAllowance = new Syncfusion.Windows.Forms.Tools.RadioButtonAdv();
			this.comboBoxAdvBudgetGroup = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.requestAllowanceGridControl = new Teleopti.Ccc.Win.Scheduling.RequestAllowanceGridControl();
			this.tableLayoutPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonTotalAllowance)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonAllowance)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvBudgetGroup)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.requestAllowanceGridControl)).BeginInit();
			this.SuspendLayout();
			// 
			// tableLayoutPanel
			// 
			this.tableLayoutPanel.AutoSize = true;
			this.tableLayoutPanel.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanel.ColumnCount = 5;
			this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
			this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 170F));
			this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
			this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 170F));
			this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel.Controls.Add(this.buttonRefresh, 4, 0);
			this.tableLayoutPanel.Controls.Add(this.labelBudgetGroup, 0, 0);
			this.tableLayoutPanel.Controls.Add(this.labelAllowance, 2, 0);
			this.tableLayoutPanel.Controls.Add(this.radioButtonTotalAllowance, 3, 0);
			this.tableLayoutPanel.Controls.Add(this.radioButtonAllowance, 3, 1);
			this.tableLayoutPanel.Controls.Add(this.comboBoxAdvBudgetGroup, 1, 0);
			this.tableLayoutPanel.Controls.Add(this.requestAllowanceGridControl, 0, 3);
			this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel.Location = new System.Drawing.Point(1, 0);
			this.tableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel.Name = "tableLayoutPanel";
			this.tableLayoutPanel.RowCount = 3;
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 6F));
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 185F));
			this.tableLayoutPanel.Size = new System.Drawing.Size(686, 310);
			this.tableLayoutPanel.TabIndex = 0;
			// 
			// buttonRefresh
			// 
			this.buttonRefresh.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.buttonRefresh.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonRefresh.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonRefresh.BeforeTouchSize = new System.Drawing.Size(99, 28);
			this.buttonRefresh.ForeColor = System.Drawing.Color.White;
			this.buttonRefresh.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.buttonRefresh.IsBackStageButton = false;
			this.buttonRefresh.Location = new System.Drawing.Point(563, 15);
			this.buttonRefresh.Margin = new System.Windows.Forms.Padding(3, 3, 12, 3);
			this.buttonRefresh.MinimumSize = new System.Drawing.Size(99, 28);
			this.buttonRefresh.Name = "buttonRefresh";
			this.tableLayoutPanel.SetRowSpan(this.buttonRefresh, 2);
			this.buttonRefresh.Size = new System.Drawing.Size(99, 28);
			this.buttonRefresh.TabIndex = 4;
			this.buttonRefresh.Text = "xxRefresh";
			this.buttonRefresh.UseVisualStyle = true;
			this.buttonRefresh.Click += new System.EventHandler(this.buttonRefreshClick);
			// 
			// labelBudgetGroup
			// 
			this.labelBudgetGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelBudgetGroup.AutoSize = true;
			this.labelBudgetGroup.Location = new System.Drawing.Point(3, 0);
			this.labelBudgetGroup.Name = "labelBudgetGroup";
			this.tableLayoutPanel.SetRowSpan(this.labelBudgetGroup, 2);
			this.labelBudgetGroup.Size = new System.Drawing.Size(104, 58);
			this.labelBudgetGroup.TabIndex = 0;
			this.labelBudgetGroup.Text = "xxBudgetGroupColon";
			this.labelBudgetGroup.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelAllowance
			// 
			this.labelAllowance.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelAllowance.AutoSize = true;
			this.labelAllowance.Location = new System.Drawing.Point(283, 0);
			this.labelAllowance.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.labelAllowance.Name = "labelAllowance";
			this.tableLayoutPanel.SetRowSpan(this.labelAllowance, 2);
			this.labelAllowance.Size = new System.Drawing.Size(104, 55);
			this.labelAllowance.TabIndex = 3;
			this.labelAllowance.Text = "xxAllowanceColon";
			this.labelAllowance.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// radioButtonTotalAllowance
			// 
			this.radioButtonTotalAllowance.BeforeTouchSize = new System.Drawing.Size(142, 22);
			this.radioButtonTotalAllowance.DrawFocusRectangle = false;
			this.radioButtonTotalAllowance.Location = new System.Drawing.Point(393, 3);
			this.radioButtonTotalAllowance.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.radioButtonTotalAllowance.Name = "radioButtonTotalAllowance";
			this.radioButtonTotalAllowance.Size = new System.Drawing.Size(142, 22);
			this.radioButtonTotalAllowance.Style = Syncfusion.Windows.Forms.Tools.RadioButtonAdvStyle.Metro;
			this.radioButtonTotalAllowance.TabIndex = 1;
			this.radioButtonTotalAllowance.Text = "xxTotalAllowance";
			this.radioButtonTotalAllowance.ThemesEnabled = false;
			this.radioButtonTotalAllowance.CheckChanged += new System.EventHandler(this.radioButtonTotalAllowanceCheckChanged);
			// 
			// radioButtonAllowance
			// 
			this.radioButtonAllowance.BeforeTouchSize = new System.Drawing.Size(142, 22);
			this.radioButtonAllowance.DrawFocusRectangle = false;
			this.radioButtonAllowance.Location = new System.Drawing.Point(393, 32);
			this.radioButtonAllowance.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.radioButtonAllowance.Name = "radioButtonAllowance";
			this.radioButtonAllowance.Size = new System.Drawing.Size(142, 22);
			this.radioButtonAllowance.Style = Syncfusion.Windows.Forms.Tools.RadioButtonAdvStyle.Metro;
			this.radioButtonAllowance.TabIndex = 2;
			this.radioButtonAllowance.Text = "xxAllowance";
			this.radioButtonAllowance.ThemesEnabled = false;
			this.radioButtonAllowance.CheckChanged += new System.EventHandler(this.radioButtonAllowanceCheckChanged);
			// 
			// comboBoxAdvBudgetGroup
			// 
			this.comboBoxAdvBudgetGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.comboBoxAdvBudgetGroup.BackColor = System.Drawing.Color.White;
			this.comboBoxAdvBudgetGroup.BeforeTouchSize = new System.Drawing.Size(164, 23);
			this.comboBoxAdvBudgetGroup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAdvBudgetGroup.Location = new System.Drawing.Point(113, 18);
			this.comboBoxAdvBudgetGroup.Name = "comboBoxAdvBudgetGroup";
			this.tableLayoutPanel.SetRowSpan(this.comboBoxAdvBudgetGroup, 2);
			this.comboBoxAdvBudgetGroup.Size = new System.Drawing.Size(164, 23);
			this.comboBoxAdvBudgetGroup.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.comboBoxAdvBudgetGroup.TabIndex = 0;
			this.comboBoxAdvBudgetGroup.SelectedIndexChanged += new System.EventHandler(this.comboBoxAdvBudgetGroupSelectedIndexChanged);
			// 
			// requestAllowanceGridControl
			// 
			this.requestAllowanceGridControl.ActivateCurrentCellBehavior = Syncfusion.Windows.Forms.Grid.GridCellActivateAction.DblClickOnCell;
			this.requestAllowanceGridControl.AlphaBlendSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(94)))), ((int)(((byte)(171)))), ((int)(((byte)(222)))));
			this.requestAllowanceGridControl.BackColor = System.Drawing.Color.White;
			gridBaseStyle1.Name = "Header";
			gridBaseStyle1.StyleInfo.Borders.Bottom = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
			gridBaseStyle1.StyleInfo.Borders.Left = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
			gridBaseStyle1.StyleInfo.Borders.Right = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
			gridBaseStyle1.StyleInfo.Borders.Top = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
			gridBaseStyle1.StyleInfo.CellType = "Header";
			gridBaseStyle1.StyleInfo.Font.Bold = true;
			gridBaseStyle1.StyleInfo.Font.Facename = "Segoe UI";
			gridBaseStyle1.StyleInfo.Font.Italic = false;
			gridBaseStyle1.StyleInfo.Font.Size = 8F;
			gridBaseStyle1.StyleInfo.Font.Strikeout = false;
			gridBaseStyle1.StyleInfo.Font.Underline = false;
			gridBaseStyle1.StyleInfo.Font.Unit = System.Drawing.GraphicsUnit.Point;
			gridBaseStyle1.StyleInfo.Interior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(199)))), ((int)(((byte)(184))))), System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(234)))), ((int)(((byte)(216))))));
			gridBaseStyle1.StyleInfo.TextColor = System.Drawing.Color.Black;
			gridBaseStyle1.StyleInfo.VerticalAlignment = Syncfusion.Windows.Forms.Grid.GridVerticalAlignment.Middle;
			gridBaseStyle2.Name = "Standard";
			gridBaseStyle2.StyleInfo.Font.Bold = false;
			gridBaseStyle2.StyleInfo.Font.Facename = "Segoe UI";
			gridBaseStyle2.StyleInfo.Font.Italic = false;
			gridBaseStyle2.StyleInfo.Font.Size = 8F;
			gridBaseStyle2.StyleInfo.Font.Strikeout = false;
			gridBaseStyle2.StyleInfo.Font.Underline = false;
			gridBaseStyle2.StyleInfo.Font.Unit = System.Drawing.GraphicsUnit.Point;
			gridBaseStyle2.StyleInfo.Interior = new Syncfusion.Drawing.BrushInfo(System.Drawing.SystemColors.Window);
			gridBaseStyle2.StyleInfo.TextColor = System.Drawing.Color.Black;
			gridBaseStyle3.Name = "Column Header";
			gridBaseStyle3.StyleInfo.BaseStyle = "Header";
			gridBaseStyle3.StyleInfo.HorizontalAlignment = Syncfusion.Windows.Forms.Grid.GridHorizontalAlignment.Center;
			gridBaseStyle4.Name = "Row Header";
			gridBaseStyle4.StyleInfo.BaseStyle = "Header";
			gridBaseStyle4.StyleInfo.Font.Size = 9F;
			gridBaseStyle4.StyleInfo.HorizontalAlignment = Syncfusion.Windows.Forms.Grid.GridHorizontalAlignment.Left;
			gridBaseStyle4.StyleInfo.Interior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Horizontal, System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(199)))), ((int)(((byte)(184))))), System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(234)))), ((int)(((byte)(216))))));
			this.requestAllowanceGridControl.BaseStylesMap.AddRange(new Syncfusion.Windows.Forms.Grid.GridBaseStyle[] {
            gridBaseStyle1,
            gridBaseStyle2,
            gridBaseStyle3,
            gridBaseStyle4});
			this.requestAllowanceGridControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.tableLayoutPanel.SetColumnSpan(this.requestAllowanceGridControl, 5);
			this.requestAllowanceGridControl.DefaultGridBorderStyle = Syncfusion.Windows.Forms.Grid.GridBorderStyle.Solid;
			this.requestAllowanceGridControl.DefaultRowHeight = 20;
			this.requestAllowanceGridControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.requestAllowanceGridControl.ExcelLikeCurrentCell = true;
			this.requestAllowanceGridControl.ExcelLikeSelectionFrame = true;
			this.requestAllowanceGridControl.Font = new System.Drawing.Font("Segoe UI", 8.25F);
			this.requestAllowanceGridControl.GridOfficeScrollBars = Syncfusion.Windows.Forms.OfficeScrollBars.Metro;
			this.requestAllowanceGridControl.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Metro;
			this.requestAllowanceGridControl.HorizontalThumbTrack = true;
			this.requestAllowanceGridControl.Location = new System.Drawing.Point(3, 67);
			this.requestAllowanceGridControl.MetroScrollBars = true;
			this.requestAllowanceGridControl.Name = "requestAllowanceGridControl";
			this.requestAllowanceGridControl.Properties.BackgroundColor = System.Drawing.Color.White;
			this.requestAllowanceGridControl.Properties.ForceImmediateRepaint = false;
			this.requestAllowanceGridControl.Properties.GridLineColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(212)))), ((int)(((byte)(212)))));
			this.requestAllowanceGridControl.Properties.MarkColHeader = false;
			this.requestAllowanceGridControl.Properties.MarkRowHeader = false;
			gridRangeStyle1.Range = Syncfusion.Windows.Forms.Grid.GridRangeInfo.Table();
			gridRangeStyle1.StyleInfo.Font.Bold = false;
			gridRangeStyle1.StyleInfo.Font.Facename = "Segoe UI";
			gridRangeStyle1.StyleInfo.Font.Italic = false;
			gridRangeStyle1.StyleInfo.Font.Size = 8.25F;
			gridRangeStyle1.StyleInfo.Font.Strikeout = false;
			gridRangeStyle1.StyleInfo.Font.Underline = false;
			gridRangeStyle1.StyleInfo.Font.Unit = System.Drawing.GraphicsUnit.Point;
			gridRangeStyle2.Range = Syncfusion.Windows.Forms.Grid.GridRangeInfo.Table();
			gridRangeStyle2.StyleInfo.Font.Bold = false;
			gridRangeStyle2.StyleInfo.Font.Facename = "Segoe UI";
			gridRangeStyle2.StyleInfo.Font.Italic = false;
			gridRangeStyle2.StyleInfo.Font.Size = 8.25F;
			gridRangeStyle2.StyleInfo.Font.Strikeout = false;
			gridRangeStyle2.StyleInfo.Font.Underline = false;
			gridRangeStyle2.StyleInfo.Font.Unit = System.Drawing.GraphicsUnit.Point;
			gridRangeStyle3.Range = Syncfusion.Windows.Forms.Grid.GridRangeInfo.Table();
			gridRangeStyle3.StyleInfo.Font.Bold = false;
			gridRangeStyle3.StyleInfo.Font.Facename = "Segoe UI";
			gridRangeStyle3.StyleInfo.Font.Italic = false;
			gridRangeStyle3.StyleInfo.Font.Size = 8.25F;
			gridRangeStyle3.StyleInfo.Font.Strikeout = false;
			gridRangeStyle3.StyleInfo.Font.Underline = false;
			gridRangeStyle3.StyleInfo.Font.Unit = System.Drawing.GraphicsUnit.Point;
			gridRangeStyle4.Range = Syncfusion.Windows.Forms.Grid.GridRangeInfo.Table();
			gridRangeStyle4.StyleInfo.Font.Bold = false;
			gridRangeStyle4.StyleInfo.Font.Facename = "Segoe UI";
			gridRangeStyle4.StyleInfo.Font.Italic = false;
			gridRangeStyle4.StyleInfo.Font.Size = 8.25F;
			gridRangeStyle4.StyleInfo.Font.Strikeout = false;
			gridRangeStyle4.StyleInfo.Font.Underline = false;
			gridRangeStyle4.StyleInfo.Font.Unit = System.Drawing.GraphicsUnit.Point;
			gridRangeStyle5.Range = Syncfusion.Windows.Forms.Grid.GridRangeInfo.Cell(0, 1);
			gridRangeStyle5.StyleInfo.CellType = "PushButton";
			gridRangeStyle5.StyleInfo.Description = "<";
			gridRangeStyle6.Range = Syncfusion.Windows.Forms.Grid.GridRangeInfo.Cell(0, 7);
			gridRangeStyle6.StyleInfo.CellType = "PushButton";
			gridRangeStyle6.StyleInfo.Description = ">";
			this.requestAllowanceGridControl.RangeStyles.AddRange(new Syncfusion.Windows.Forms.Grid.GridRangeStyle[] {
            gridRangeStyle1,
            gridRangeStyle2,
            gridRangeStyle3,
            gridRangeStyle4,
            gridRangeStyle5,
            gridRangeStyle6});
			this.requestAllowanceGridControl.ResizeRowsBehavior = Syncfusion.Windows.Forms.Grid.GridResizeCellsBehavior.None;
			this.requestAllowanceGridControl.RowHeightEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridRowHeight[] {
            new Syncfusion.Windows.Forms.Grid.GridRowHeight(0, 29)});
			this.requestAllowanceGridControl.SelectCellsMouseButtonsMask = System.Windows.Forms.MouseButtons.Left;
			this.requestAllowanceGridControl.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
			this.requestAllowanceGridControl.Size = new System.Drawing.Size(680, 240);
			this.requestAllowanceGridControl.SmartSizeBox = false;
			this.requestAllowanceGridControl.TabIndex = 3;
			this.requestAllowanceGridControl.ThemesEnabled = true;
			this.requestAllowanceGridControl.UseRightToLeftCompatibleTextBox = true;
			this.requestAllowanceGridControl.VerticalThumbTrack = true;
			// 
			// RequestAllowanceView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BorderColor = System.Drawing.Color.Blue;
			this.CaptionFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ClientSize = new System.Drawing.Size(688, 310);
			this.Controls.Add(this.tableLayoutPanel);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.HelpButton = false;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(700, 350);
			this.Name = "RequestAllowanceView";
			this.Padding = new System.Windows.Forms.Padding(1, 0, 1, 0);
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "xxViewAllowance";
			this.Load += new System.EventHandler(this.requestAllowanceViewLoad);
			this.tableLayoutPanel.ResumeLayout(false);
			this.tableLayoutPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonTotalAllowance)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonAllowance)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvBudgetGroup)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.requestAllowanceGridControl)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.Label labelBudgetGroup;
        private System.Windows.Forms.Label labelAllowance;
        private Syncfusion.Windows.Forms.Tools.RadioButtonAdv radioButtonTotalAllowance;
        private Syncfusion.Windows.Forms.Tools.RadioButtonAdv radioButtonAllowance;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvBudgetGroup;
        private RequestAllowanceGridControl requestAllowanceGridControl;
        private Syncfusion.Windows.Forms.ButtonAdv buttonRefresh;

    }
}