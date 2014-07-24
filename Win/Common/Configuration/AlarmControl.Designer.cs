namespace Teleopti.Ccc.Win.Common.Configuration
{
    partial class AlarmControl
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
                {
                    components.Dispose();
                }
                if (_view != null) _view.Dispose();
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
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.gridControlAlarmTypes = new Syncfusion.Windows.Forms.Grid.GridControl();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonAdvDelete = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonNew = new Syncfusion.Windows.Forms.ButtonAdv();
            this.gradientPanel1 = new Syncfusion.Windows.Forms.Tools.GradientPanel();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.labelTitle = new System.Windows.Forms.Label();
            this.tableLayoutPanel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControlAlarmTypes)).BeginInit();
            this.tableLayoutPanel6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanel1)).BeginInit();
            this.gradientPanel1.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.AutoScroll = true;
            this.tableLayoutPanel5.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel5.ColumnCount = 1;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Controls.Add(this.gridControlAlarmTypes, 0, 1);
            this.tableLayoutPanel5.Controls.Add(this.tableLayoutPanel6, 0, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(0, 62);
            this.tableLayoutPanel5.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 2;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(525, 505);
            this.tableLayoutPanel5.TabIndex = 2;
            // 
            // gridControlAlarmTypes
            // 
            this.gridControlAlarmTypes.AlphaBlendSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(94)))), ((int)(((byte)(171)))), ((int)(((byte)(222)))));
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
            this.gridControlAlarmTypes.BaseStylesMap.AddRange(new Syncfusion.Windows.Forms.Grid.GridBaseStyle[] {
            gridBaseStyle1,
            gridBaseStyle2,
            gridBaseStyle3,
            gridBaseStyle4});
            this.gridControlAlarmTypes.ColWidthEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridColWidth[] {
            new Syncfusion.Windows.Forms.Grid.GridColWidth(0, 35)});
            this.gridControlAlarmTypes.DefaultGridBorderStyle = Syncfusion.Windows.Forms.Grid.GridBorderStyle.Solid;
            this.gridControlAlarmTypes.DefaultRowHeight = 20;
            this.gridControlAlarmTypes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControlAlarmTypes.ForeColor = System.Drawing.SystemColors.ControlText;
            this.gridControlAlarmTypes.GridOfficeScrollBars = Syncfusion.Windows.Forms.OfficeScrollBars.Metro;
            this.gridControlAlarmTypes.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Metro;
            this.gridControlAlarmTypes.Location = new System.Drawing.Point(3, 43);
            this.gridControlAlarmTypes.MetroScrollBars = true;
            this.gridControlAlarmTypes.Name = "gridControlAlarmTypes";
            this.gridControlAlarmTypes.NumberedRowHeaders = false;
            this.gridControlAlarmTypes.Properties.BackgroundColor = System.Drawing.SystemColors.Window;
            this.gridControlAlarmTypes.Properties.ForceImmediateRepaint = false;
            this.gridControlAlarmTypes.Properties.GridLineColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(212)))), ((int)(((byte)(212)))));
            this.gridControlAlarmTypes.Properties.MarkColHeader = false;
            this.gridControlAlarmTypes.Properties.MarkRowHeader = false;
            this.gridControlAlarmTypes.RowHeightEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridRowHeight[] {
            new Syncfusion.Windows.Forms.Grid.GridRowHeight(0, 29)});
            this.gridControlAlarmTypes.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
            this.gridControlAlarmTypes.Size = new System.Drawing.Size(519, 459);
            this.gridControlAlarmTypes.SmartSizeBox = false;
            this.gridControlAlarmTypes.TabIndex = 0;
            this.gridControlAlarmTypes.ThemesEnabled = true;
            this.gridControlAlarmTypes.UseRightToLeftCompatibleTextBox = true;
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.BackColor = System.Drawing.Color.LightSteelBlue;
            this.tableLayoutPanel6.ColumnCount = 3;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel6.Controls.Add(this.label2, 0, 0);
            this.tableLayoutPanel6.Controls.Add(this.buttonAdvDelete, 2, 0);
            this.tableLayoutPanel6.Controls.Add(this.buttonNew, 1, 0);
            this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel6.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 1;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel6.Size = new System.Drawing.Size(519, 34);
            this.tableLayoutPanel6.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.GhostWhite;
            this.label2.Location = new System.Drawing.Point(3, 7);
            this.label2.Name = "label2";
            this.label2.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.label2.Size = new System.Drawing.Size(94, 20);
            this.label2.TabIndex = 0;
            this.label2.Text = "xxAlarmTypes";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // buttonAdvDelete
            // 
            this.buttonAdvDelete.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.buttonAdvDelete.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
            this.buttonAdvDelete.BackColor = System.Drawing.Color.White;
            this.buttonAdvDelete.BeforeTouchSize = new System.Drawing.Size(28, 28);
            this.buttonAdvDelete.ForeColor = System.Drawing.Color.White;
            this.buttonAdvDelete.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_temp_DeleteGroup2;
            this.buttonAdvDelete.IsBackStageButton = false;
            this.buttonAdvDelete.Location = new System.Drawing.Point(484, 3);
            this.buttonAdvDelete.Margin = new System.Windows.Forms.Padding(0, 1, 3, 0);
            this.buttonAdvDelete.Name = "buttonAdvDelete";
            this.buttonAdvDelete.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.buttonAdvDelete.Size = new System.Drawing.Size(28, 28);
            this.buttonAdvDelete.TabIndex = 7;
            this.buttonAdvDelete.TabStop = false;
            this.buttonAdvDelete.UseVisualStyle = true;
            this.buttonAdvDelete.Click += new System.EventHandler(this.ButtonAdvDeleteClick);
            // 
            // buttonNew
            // 
            this.buttonNew.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.buttonNew.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
            this.buttonNew.BackColor = System.Drawing.Color.White;
            this.buttonNew.BeforeTouchSize = new System.Drawing.Size(28, 28);
            this.buttonNew.Font = new System.Drawing.Font("Tahoma", 8F);
            this.buttonNew.ForeColor = System.Drawing.Color.White;
            this.buttonNew.Image = global::Teleopti.Ccc.Win.Properties.Resources.test_add2;
            this.buttonNew.IsBackStageButton = false;
            this.buttonNew.Location = new System.Drawing.Point(449, 3);
            this.buttonNew.Margin = new System.Windows.Forms.Padding(0, 1, 3, 0);
            this.buttonNew.Name = "buttonNew";
            this.buttonNew.Size = new System.Drawing.Size(28, 28);
            this.buttonNew.TabIndex = 6;
            this.buttonNew.UseVisualStyle = true;
            this.buttonNew.UseVisualStyleBackColor = false;
            this.buttonNew.Click += new System.EventHandler(this.ButtonNewClick);
            // 
            // gradientPanel1
            // 
            this.gradientPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gradientPanel1.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.gradientPanel1.BorderSingle = System.Windows.Forms.ButtonBorderStyle.None;
            this.gradientPanel1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gradientPanel1.Controls.Add(this.tableLayoutPanel4);
            this.gradientPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.gradientPanel1.Location = new System.Drawing.Point(0, 0);
            this.gradientPanel1.Name = "gradientPanel1";
            this.gradientPanel1.Padding = new System.Windows.Forms.Padding(12);
            this.gradientPanel1.Size = new System.Drawing.Size(525, 62);
            this.gradientPanel1.TabIndex = 56;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel4.ColumnCount = 1;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 622F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Controls.Add(this.labelTitle, 1, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(12, 12);
            this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(501, 38);
            this.tableLayoutPanel4.TabIndex = 0;
            // 
            // labelTitle
            // 
            this.labelTitle.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelTitle.AutoSize = true;
            this.labelTitle.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTitle.ForeColor = System.Drawing.Color.MidnightBlue;
            this.labelTitle.Location = new System.Drawing.Point(3, 6);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.labelTitle.Size = new System.Drawing.Size(215, 25);
            this.labelTitle.TabIndex = 0;
            this.labelTitle.Text = "xxManageAlarmTypes";
            // 
            // AlarmControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel5);
            this.Controls.Add(this.gradientPanel1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "AlarmControl";
            this.Size = new System.Drawing.Size(525, 567);
            this.tableLayoutPanel5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridControlAlarmTypes)).EndInit();
            this.tableLayoutPanel6.ResumeLayout(false);
            this.tableLayoutPanel6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanel1)).EndInit();
            this.gradientPanel1.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private System.Windows.Forms.Label label2;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvDelete;
        private Syncfusion.Windows.Forms.Grid.GridControl gridControlAlarmTypes;
        private Syncfusion.Windows.Forms.ButtonAdv buttonNew;
    }
}
