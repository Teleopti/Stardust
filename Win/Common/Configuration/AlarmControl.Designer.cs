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
			this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
			this.teleoptiGridControl1 = new Teleopti.Ccc.Win.Common.Controls.TeleoptiGridControl();
			this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
			this.label2 = new System.Windows.Forms.Label();
			this.buttonAdvDelete = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonNew = new System.Windows.Forms.Button();
			this.gradientPanel1 = new Syncfusion.Windows.Forms.Tools.GradientPanel();
			this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
			this.labelTitle = new System.Windows.Forms.Label();
			this.tableLayoutPanel5.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.teleoptiGridControl1)).BeginInit();
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
			this.tableLayoutPanel5.Controls.Add(this.teleoptiGridControl1, 0, 1);
			this.tableLayoutPanel5.Controls.Add(this.tableLayoutPanel6, 0, 0);
			this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel5.Location = new System.Drawing.Point(0, 55);
			this.tableLayoutPanel5.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel5.Name = "tableLayoutPanel5";
			this.tableLayoutPanel5.RowCount = 2;
			this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 33F));
			this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel5.Size = new System.Drawing.Size(450, 436);
			this.tableLayoutPanel5.TabIndex = 2;
			// 
			// teleoptiGridControl1
			// 
			this.teleoptiGridControl1.ActivateCurrentCellBehavior = Syncfusion.Windows.Forms.Grid.GridCellActivateAction.DblClickOnCell;
			this.teleoptiGridControl1.BackColor = System.Drawing.Color.White;
			this.teleoptiGridControl1.ColWidthEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridColWidth[] {
            new Syncfusion.Windows.Forms.Grid.GridColWidth(0, 35)});
			this.teleoptiGridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.teleoptiGridControl1.ExcelLikeCurrentCell = true;
			this.teleoptiGridControl1.ExcelLikeSelectionFrame = true;
			this.teleoptiGridControl1.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Office2007Blue;
			this.teleoptiGridControl1.HorizontalThumbTrack = true;
			this.teleoptiGridControl1.Location = new System.Drawing.Point(3, 36);
			this.teleoptiGridControl1.Name = "teleoptiGridControl1";
			this.teleoptiGridControl1.NumberedRowHeaders = false;
			this.teleoptiGridControl1.Office2007ScrollBars = true;
			this.teleoptiGridControl1.Office2007ScrollBarsColorScheme = Syncfusion.Windows.Forms.Office2007ColorScheme.Managed;
			this.teleoptiGridControl1.Properties.BackgroundColor = System.Drawing.Color.White;
			this.teleoptiGridControl1.ResizeRowsBehavior = Syncfusion.Windows.Forms.Grid.GridResizeCellsBehavior.None;
			this.teleoptiGridControl1.RowHeightEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridRowHeight[] {
            new Syncfusion.Windows.Forms.Grid.GridRowHeight(0, 21)});
			this.teleoptiGridControl1.SelectCellsMouseButtonsMask = System.Windows.Forms.MouseButtons.Left;
			this.teleoptiGridControl1.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
			this.teleoptiGridControl1.Size = new System.Drawing.Size(444, 397);
			this.teleoptiGridControl1.SmartSizeBox = false;
			this.teleoptiGridControl1.TabIndex = 58;
			this.teleoptiGridControl1.Text = "teleoptiGridControl1";
			this.teleoptiGridControl1.ThemesEnabled = true;
			this.teleoptiGridControl1.UseRightToLeftCompatibleTextBox = true;
			this.teleoptiGridControl1.VerticalThumbTrack = true;
			// 
			// tableLayoutPanel6
			// 
			this.tableLayoutPanel6.BackColor = System.Drawing.Color.LightSteelBlue;
			this.tableLayoutPanel6.ColumnCount = 3;
			this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel6.Controls.Add(this.label2, 0, 0);
			this.tableLayoutPanel6.Controls.Add(this.buttonAdvDelete, 2, 0);
			this.tableLayoutPanel6.Controls.Add(this.buttonNew, 1, 0);
			this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel6.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanel6.Name = "tableLayoutPanel6";
			this.tableLayoutPanel6.RowCount = 1;
			this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel6.Size = new System.Drawing.Size(444, 27);
			this.tableLayoutPanel6.TabIndex = 1;
			// 
			// label2
			// 
			this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
			this.label2.ForeColor = System.Drawing.Color.GhostWhite;
			this.label2.Location = new System.Drawing.Point(3, 6);
			this.label2.Name = "label2";
			this.label2.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
			this.label2.Size = new System.Drawing.Size(89, 16);
			this.label2.TabIndex = 0;
			this.label2.Text = "xxAlarmTypes";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// buttonAdvDelete
			// 
			this.buttonAdvDelete.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.buttonAdvDelete.Image = global::Teleopti.Ccc.Win.Properties.Resources.test_delete_32x32;
			this.buttonAdvDelete.Location = new System.Drawing.Point(420, 1);
			this.buttonAdvDelete.Margin = new System.Windows.Forms.Padding(3, 1, 0, 3);
			this.buttonAdvDelete.Name = "buttonAdvDelete";
			this.buttonAdvDelete.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
			this.buttonAdvDelete.Size = new System.Drawing.Size(24, 24);
			this.buttonAdvDelete.TabIndex = 7;
			this.buttonAdvDelete.TabStop = false;
			this.buttonAdvDelete.Click += new System.EventHandler(this.ButtonAdvDeleteClick);
			// 
			// buttonNew
			// 
			this.buttonNew.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.buttonNew.BackColor = System.Drawing.Color.Transparent;
			this.buttonNew.Font = new System.Drawing.Font("Tahoma", 8F);
			this.buttonNew.Image = global::Teleopti.Ccc.Win.Properties.Resources.test_add2;
			this.buttonNew.Location = new System.Drawing.Point(393, 1);
			this.buttonNew.Margin = new System.Windows.Forms.Padding(3, 1, 0, 3);
			this.buttonNew.Name = "buttonNew";
			this.buttonNew.Size = new System.Drawing.Size(24, 24);
			this.buttonNew.TabIndex = 6;
			this.buttonNew.UseVisualStyleBackColor = false;
			this.buttonNew.Click += new System.EventHandler(this.ButtonNewClick);
			// 
			// gradientPanel1
			// 
			this.gradientPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.gradientPanel1.BackColor = System.Drawing.SystemColors.ActiveCaption;
			this.gradientPanel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.LightSteelBlue, System.Drawing.Color.White);
			this.gradientPanel1.BorderSingle = System.Windows.Forms.ButtonBorderStyle.None;
			this.gradientPanel1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.gradientPanel1.Controls.Add(this.tableLayoutPanel4);
			this.gradientPanel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.gradientPanel1.Location = new System.Drawing.Point(0, 0);
			this.gradientPanel1.Name = "gradientPanel1";
			this.gradientPanel1.Padding = new System.Windows.Forms.Padding(10);
			this.gradientPanel1.Size = new System.Drawing.Size(450, 55);
			this.gradientPanel1.TabIndex = 56;
			// 
			// tableLayoutPanel4
			// 
			this.tableLayoutPanel4.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanel4.ColumnCount = 1;
			this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 533F));
			this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel4.Controls.Add(this.labelTitle, 1, 0);
			this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel4.Location = new System.Drawing.Point(10, 10);
			this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel4.Name = "tableLayoutPanel4";
			this.tableLayoutPanel4.RowCount = 1;
			this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel4.Size = new System.Drawing.Size(430, 35);
			this.tableLayoutPanel4.TabIndex = 0;
			// 
			// labelTitle
			// 
			this.labelTitle.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelTitle.AutoSize = true;
			this.labelTitle.Font = new System.Drawing.Font("Tahoma", 11.25F);
			this.labelTitle.ForeColor = System.Drawing.Color.MidnightBlue;
			this.labelTitle.Location = new System.Drawing.Point(3, 8);
			this.labelTitle.Name = "labelTitle";
			this.labelTitle.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
			this.labelTitle.Size = new System.Drawing.Size(159, 18);
			this.labelTitle.TabIndex = 0;
			this.labelTitle.Text = "xxManageAlarmTypes";
			// 
			// AlarmControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanel5);
			this.Controls.Add(this.gradientPanel1);
			this.Name = "AlarmControl";
			this.Size = new System.Drawing.Size(450, 491);
			this.tableLayoutPanel5.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.teleoptiGridControl1)).EndInit();
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
        private Teleopti.Ccc.Win.Common.Controls.TeleoptiGridControl teleoptiGridControl1;
        private System.Windows.Forms.Button buttonNew;
    }
}
