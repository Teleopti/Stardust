using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.SpinningProgress;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
    partial class ExportToScenarioResultView
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
            if (disposing)
            {
                if (components != null)
                    components.Dispose();

                releaseManagedResources();
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
			Syncfusion.Windows.Forms.Grid.GridRangeStyle gridRangeStyle1 = new Syncfusion.Windows.Forms.Grid.GridRangeStyle();
			this.btnOk = new Syncfusion.Windows.Forms.ButtonAdv();
			this.btnCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.lblScenarios = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.lblNoOfAgents = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.groupBoxInfo = new System.Windows.Forms.GroupBox();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.autoLabelData = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.autoLabelScenario = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.groupBoxWarnings = new System.Windows.Forms.GroupBox();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.gridControl1 = new Syncfusion.Windows.Forms.Grid.GridControl();
			this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
			this.panel4 = new System.Windows.Forms.Panel();
			this.panel2 = new System.Windows.Forms.Panel();
			this.panel1 = new System.Windows.Forms.Panel();
			this.spinningProgressControl1 = new SpinningProgressControl();
			this.groupBoxInfo.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			this.groupBoxWarnings.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
			this.tableLayoutPanel3.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnOk
			// 
			this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOk.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.btnOk.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.btnOk.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.btnOk.ForeColor = System.Drawing.Color.White;
			this.btnOk.IsBackStageButton = false;
			this.btnOk.Location = new System.Drawing.Point(602, 527);
			this.btnOk.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.btnOk.Name = "btnOk";
			this.btnOk.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
			this.btnOk.Size = new System.Drawing.Size(87, 27);
			this.btnOk.TabIndex = 6;
			this.btnOk.Text = "xxOk";
			this.btnOk.UseVisualStyle = true;
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.btnCancel.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.btnCancel.ForeColor = System.Drawing.Color.White;
			this.btnCancel.IsBackStageButton = false;
			this.btnCancel.Location = new System.Drawing.Point(722, 527);
			this.btnCancel.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
			this.btnCancel.Size = new System.Drawing.Size(87, 27);
			this.btnCancel.TabIndex = 2;
			this.btnCancel.Text = "xxCancel";
			this.btnCancel.UseVisualStyle = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// lblScenarios
			// 
			this.lblScenarios.BackColor = System.Drawing.Color.Transparent;
			this.lblScenarios.ForeColor = System.Drawing.Color.Black;
			this.lblScenarios.Location = new System.Drawing.Point(103, 0);
			this.lblScenarios.Name = "lblScenarios";
			this.lblScenarios.Size = new System.Drawing.Size(45, 15);
			this.lblScenarios.TabIndex = 15;
			this.lblScenarios.Text = "Header";
			// 
			// lblNoOfAgents
			// 
			this.lblNoOfAgents.ForeColor = System.Drawing.Color.Black;
			this.lblNoOfAgents.Location = new System.Drawing.Point(103, 24);
			this.lblNoOfAgents.Name = "lblNoOfAgents";
			this.lblNoOfAgents.Size = new System.Drawing.Size(65, 15);
			this.lblNoOfAgents.TabIndex = 17;
			this.lblNoOfAgents.Text = "autoLabel1";
			// 
			// groupBoxInfo
			// 
			this.groupBoxInfo.BackColor = System.Drawing.Color.White;
			this.tableLayoutPanel3.SetColumnSpan(this.groupBoxInfo, 3);
			this.groupBoxInfo.Controls.Add(this.tableLayoutPanel2);
			this.groupBoxInfo.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBoxInfo.Font = new System.Drawing.Font("Segoe UI", 14F);
			this.groupBoxInfo.ForeColor = System.Drawing.Color.Navy;
			this.groupBoxInfo.Location = new System.Drawing.Point(5, 10);
			this.groupBoxInfo.Margin = new System.Windows.Forms.Padding(0);
			this.groupBoxInfo.Name = "groupBoxInfo";
			this.groupBoxInfo.Padding = new System.Windows.Forms.Padding(3, 15, 3, 3);
			this.groupBoxInfo.Size = new System.Drawing.Size(814, 91);
			this.groupBoxInfo.TabIndex = 18;
			this.groupBoxInfo.TabStop = false;
			this.groupBoxInfo.Text = "xxInfo";
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel2.BackColor = System.Drawing.Color.White;
			this.tableLayoutPanel2.ColumnCount = 2;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.Controls.Add(this.autoLabelData, 0, 1);
			this.tableLayoutPanel2.Controls.Add(this.autoLabelScenario, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.lblNoOfAgents, 1, 1);
			this.tableLayoutPanel2.Controls.Add(this.lblScenarios, 1, 0);
			this.tableLayoutPanel2.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.tableLayoutPanel2.ForeColor = System.Drawing.Color.Black;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(10, 33);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 2;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(790, 48);
			this.tableLayoutPanel2.TabIndex = 20;
			// 
			// autoLabelData
			// 
			this.autoLabelData.ForeColor = System.Drawing.Color.Black;
			this.autoLabelData.Location = new System.Drawing.Point(3, 24);
			this.autoLabelData.Name = "autoLabelData";
			this.autoLabelData.Size = new System.Drawing.Size(73, 15);
			this.autoLabelData.TabIndex = 19;
			this.autoLabelData.Text = "xxDataColon";
			// 
			// autoLabelScenario
			// 
			this.autoLabelScenario.ForeColor = System.Drawing.Color.Black;
			this.autoLabelScenario.Location = new System.Drawing.Point(3, 0);
			this.autoLabelScenario.Name = "autoLabelScenario";
			this.autoLabelScenario.Size = new System.Drawing.Size(94, 15);
			this.autoLabelScenario.TabIndex = 18;
			this.autoLabelScenario.Text = "xxScenarioColon";
			// 
			// groupBoxWarnings
			// 
			this.groupBoxWarnings.BackColor = System.Drawing.Color.White;
			this.tableLayoutPanel3.SetColumnSpan(this.groupBoxWarnings, 3);
			this.groupBoxWarnings.Controls.Add(this.tableLayoutPanel1);
			this.groupBoxWarnings.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBoxWarnings.Font = new System.Drawing.Font("Segoe UI", 14F);
			this.groupBoxWarnings.ForeColor = System.Drawing.Color.Navy;
			this.groupBoxWarnings.Location = new System.Drawing.Point(5, 101);
			this.groupBoxWarnings.Margin = new System.Windows.Forms.Padding(0);
			this.groupBoxWarnings.Name = "groupBoxWarnings";
			this.groupBoxWarnings.Padding = new System.Windows.Forms.Padding(3, 15, 3, 3);
			this.groupBoxWarnings.Size = new System.Drawing.Size(814, 403);
			this.groupBoxWarnings.TabIndex = 19;
			this.groupBoxWarnings.TabStop = false;
			this.groupBoxWarnings.Text = "xxWarnings";
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.Controls.Add(this.gridControl1, 0, 0);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(8, 29);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(797, 362);
			this.tableLayoutPanel1.TabIndex = 18;
			// 
			// gridControl1
			// 
			this.gridControl1.BackColor = System.Drawing.Color.White;
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
			this.gridControl1.BaseStylesMap.AddRange(new Syncfusion.Windows.Forms.Grid.GridBaseStyle[] {
            gridBaseStyle1,
            gridBaseStyle2,
            gridBaseStyle3,
            gridBaseStyle4});
			this.gridControl1.ColCount = 2;
			this.gridControl1.ColWidthEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridColWidth[] {
            new Syncfusion.Windows.Forms.Grid.GridColWidth(0, 35)});
			this.gridControl1.DefaultColWidth = 80;
			this.gridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridControl1.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.gridControl1.ForeColor = System.Drawing.SystemColors.ControlText;
			this.gridControl1.HScrollPixel = true;
			this.gridControl1.Location = new System.Drawing.Point(3, 3);
			this.gridControl1.Name = "gridControl1";
			this.gridControl1.Properties.BackgroundColor = System.Drawing.Color.White;
			this.gridControl1.Properties.ColHeaders = false;
			this.gridControl1.Properties.DisplayHorzLines = false;
			this.gridControl1.Properties.DisplayVertLines = false;
			this.gridControl1.Properties.ForceImmediateRepaint = false;
			this.gridControl1.Properties.MarkColHeader = false;
			this.gridControl1.Properties.MarkRowHeader = false;
			this.gridControl1.Properties.RowHeaders = false;
			gridRangeStyle1.Range = Syncfusion.Windows.Forms.Grid.GridRangeInfo.Table();
			gridRangeStyle1.StyleInfo.Font.Bold = false;
			gridRangeStyle1.StyleInfo.Font.Facename = "Segoe UI";
			gridRangeStyle1.StyleInfo.Font.Italic = false;
			gridRangeStyle1.StyleInfo.Font.Size = 9F;
			gridRangeStyle1.StyleInfo.Font.Strikeout = false;
			gridRangeStyle1.StyleInfo.Font.Underline = false;
			gridRangeStyle1.StyleInfo.Font.Unit = System.Drawing.GraphicsUnit.Point;
			this.gridControl1.RangeStyles.AddRange(new Syncfusion.Windows.Forms.Grid.GridRangeStyle[] {
            gridRangeStyle1});
			this.gridControl1.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.gridControl1.RowCount = 1;
			this.gridControl1.RowHeightEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridRowHeight[] {
            new Syncfusion.Windows.Forms.Grid.GridRowHeight(0, 21)});
			this.gridControl1.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
			this.gridControl1.Size = new System.Drawing.Size(791, 356);
			this.gridControl1.SmartSizeBox = false;
			this.gridControl1.TabIndex = 0;
			this.gridControl1.UseRightToLeftCompatibleTextBox = true;
			this.gridControl1.VScrollPixel = true;
			// 
			// tableLayoutPanel3
			// 
			this.tableLayoutPanel3.ColumnCount = 4;
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 5F));
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.tableLayoutPanel3.Controls.Add(this.panel4, 0, 3);
			this.tableLayoutPanel3.Controls.Add(this.panel2, 0, 1);
			this.tableLayoutPanel3.Controls.Add(this.panel1, 0, 0);
			this.tableLayoutPanel3.Controls.Add(this.groupBoxWarnings, 1, 2);
			this.tableLayoutPanel3.Controls.Add(this.btnOk, 2, 4);
			this.tableLayoutPanel3.Controls.Add(this.groupBoxInfo, 1, 1);
			this.tableLayoutPanel3.Controls.Add(this.btnCancel, 3, 4);
			this.tableLayoutPanel3.Controls.Add(this.spinningProgressControl1, 1, 4);
			this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel3.Name = "tableLayoutPanel3";
			this.tableLayoutPanel3.RowCount = 5;
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 91F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			this.tableLayoutPanel3.Size = new System.Drawing.Size(819, 564);
			this.tableLayoutPanel3.TabIndex = 20;
			// 
			// panel4
			// 
			this.panel4.BackColor = System.Drawing.Color.White;
			this.tableLayoutPanel3.SetColumnSpan(this.panel4, 4);
			this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel4.Location = new System.Drawing.Point(0, 504);
			this.panel4.Margin = new System.Windows.Forms.Padding(0);
			this.panel4.Name = "panel4";
			this.panel4.Size = new System.Drawing.Size(819, 10);
			this.panel4.TabIndex = 24;
			// 
			// panel2
			// 
			this.panel2.BackColor = System.Drawing.Color.White;
			this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel2.Location = new System.Drawing.Point(0, 10);
			this.panel2.Margin = new System.Windows.Forms.Padding(0);
			this.panel2.Name = "panel2";
			this.tableLayoutPanel3.SetRowSpan(this.panel2, 2);
			this.panel2.Size = new System.Drawing.Size(5, 494);
			this.panel2.TabIndex = 22;
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.White;
			this.tableLayoutPanel3.SetColumnSpan(this.panel1, 4);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Margin = new System.Windows.Forms.Padding(0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(819, 10);
			this.panel1.TabIndex = 21;
			// 
			// spinningProgressControl1
			// 
			this.spinningProgressControl1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.spinningProgressControl1.AutoIncrementFrequency = 100D;
			this.spinningProgressControl1.Location = new System.Drawing.Point(15, 529);
			this.spinningProgressControl1.Margin = new System.Windows.Forms.Padding(10, 0, 0, 0);
			this.spinningProgressControl1.MinimumSize = new System.Drawing.Size(20, 20);
			this.spinningProgressControl1.Name = "spinningProgressControl1";
			this.spinningProgressControl1.Size = new System.Drawing.Size(20, 20);
			this.spinningProgressControl1.TabIndex = 25;
			this.spinningProgressControl1.TransitionSegment = 4;
			// 
			// ExportToScenarioResultView
			// 
			this.AcceptButton = this.btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BorderColor = System.Drawing.Color.Blue;
			this.ClientSize = new System.Drawing.Size(819, 564);
			this.Controls.Add(this.tableLayoutPanel3);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.ForeColor = System.Drawing.Color.Black;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.HelpButton = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(332, 45);
			this.Name = "ExportToScenarioResultView";
			this.ShowIcon = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "xxExportToOtherScenario";
			this.Load += new System.EventHandler(this.exportToScenarioResultViewLoad);
			this.groupBoxInfo.ResumeLayout(false);
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			this.groupBoxWarnings.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
			this.tableLayoutPanel3.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

		private Syncfusion.Windows.Forms.ButtonAdv btnOk;
        private Syncfusion.Windows.Forms.ButtonAdv btnCancel;
        private Syncfusion.Windows.Forms.Tools.AutoLabel lblScenarios;
        private Syncfusion.Windows.Forms.Tools.AutoLabel lblNoOfAgents;
        private System.Windows.Forms.GroupBox groupBoxInfo;
        private System.Windows.Forms.GroupBox groupBoxWarnings;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelData;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelScenario;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Syncfusion.Windows.Forms.Grid.GridControl gridControl1;
		  private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
		  private System.Windows.Forms.Panel panel1;
		  private System.Windows.Forms.Panel panel4;
		  private System.Windows.Forms.Panel panel2;
		  private SpinningProgressControl spinningProgressControl1;
    }
}