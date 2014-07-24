namespace Teleopti.Ccc.Win.Common.Configuration
{
    partial class ActivityControl
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
			if (disposing &&  _gridColumnHelper != null) _gridColumnHelper.Dispose();
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
            this.toolStripMenuItemAddFromClipboard = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanelBody = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanelSubHeader1 = new System.Windows.Forms.TableLayoutPanel();
            this.labelSubHeader1 = new System.Windows.Forms.Label();
            this.tableLayoutPanelSubHeader2 = new System.Windows.Forms.TableLayoutPanel();
            this.labelSubHeader2 = new System.Windows.Forms.Label();
            this.buttonAdvDeleteActivity = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonNewActivity = new Syncfusion.Windows.Forms.ButtonAdv();
            this.gridControlActivities = new Syncfusion.Windows.Forms.Grid.GridControl();
            this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.label2 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.gradientPanelHeader = new Syncfusion.Windows.Forms.Tools.GradientPanel();
            this.tableLayoutPanelHeader = new System.Windows.Forms.TableLayoutPanel();
            this.labelHeader = new System.Windows.Forms.Label();
            this.tableLayoutPanelBody.SuspendLayout();
            this.tableLayoutPanelSubHeader1.SuspendLayout();
            this.tableLayoutPanelSubHeader2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControlActivities)).BeginInit();
            this.contextMenuStrip2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanelHeader)).BeginInit();
            this.gradientPanelHeader.SuspendLayout();
            this.tableLayoutPanelHeader.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripMenuItemAddFromClipboard
            // 
            this.toolStripMenuItemAddFromClipboard.Name = "toolStripMenuItemAddFromClipboard";
            this.toolStripMenuItemAddFromClipboard.Size = new System.Drawing.Size(144, 22);
            this.toolStripMenuItemAddFromClipboard.Text = "xxPasteNew";
            this.toolStripMenuItemAddFromClipboard.Click += new System.EventHandler(this.ToolStripMenuItemAddFromClipboardClick);
            // 
            // tableLayoutPanelBody
            // 
            this.tableLayoutPanelBody.ColumnCount = 1;
            this.tableLayoutPanelBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelBody.Controls.Add(this.tableLayoutPanelSubHeader1, 0, 0);
            this.tableLayoutPanelBody.Controls.Add(this.tableLayoutPanelSubHeader2, 0, 1);
            this.tableLayoutPanelBody.Controls.Add(this.gridControlActivities, 0, 2);
            this.tableLayoutPanelBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelBody.Location = new System.Drawing.Point(0, 62);
            this.tableLayoutPanelBody.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanelBody.Name = "tableLayoutPanelBody";
            this.tableLayoutPanelBody.RowCount = 3;
            this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 0F));
            this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelBody.Size = new System.Drawing.Size(591, 575);
            this.tableLayoutPanelBody.TabIndex = 1;
            // 
            // tableLayoutPanelSubHeader1
            // 
            this.tableLayoutPanelSubHeader1.BackColor = System.Drawing.Color.LightSteelBlue;
            this.tableLayoutPanelSubHeader1.ColumnCount = 3;
            this.tableLayoutPanelSubHeader1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelSubHeader1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelSubHeader1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelSubHeader1.Controls.Add(this.labelSubHeader1, 0, 0);
            this.tableLayoutPanelSubHeader1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelSubHeader1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanelSubHeader1.Name = "tableLayoutPanelSubHeader1";
            this.tableLayoutPanelSubHeader1.RowCount = 1;
            this.tableLayoutPanelSubHeader1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelSubHeader1.Size = new System.Drawing.Size(585, 1);
            this.tableLayoutPanelSubHeader1.TabIndex = 56;
            this.tableLayoutPanelSubHeader1.Visible = false;
            // 
            // labelSubHeader1
            // 
            this.labelSubHeader1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelSubHeader1.AutoSize = true;
            this.labelSubHeader1.BackColor = System.Drawing.Color.Transparent;
            this.labelSubHeader1.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSubHeader1.ForeColor = System.Drawing.Color.GhostWhite;
            this.labelSubHeader1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelSubHeader1.Location = new System.Drawing.Point(3, 0);
            this.labelSubHeader1.Name = "labelSubHeader1";
            this.labelSubHeader1.Size = new System.Drawing.Size(282, 17);
            this.labelSubHeader1.TabIndex = 0;
            this.labelSubHeader1.Text = "xxEnterPropertyValuesForGroupingActivities";
            this.labelSubHeader1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanelSubHeader2
            // 
            this.tableLayoutPanelSubHeader2.BackColor = System.Drawing.Color.LightSteelBlue;
            this.tableLayoutPanelSubHeader2.ColumnCount = 3;
            this.tableLayoutPanelSubHeader2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelSubHeader2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanelSubHeader2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanelSubHeader2.Controls.Add(this.labelSubHeader2, 0, 0);
            this.tableLayoutPanelSubHeader2.Controls.Add(this.buttonAdvDeleteActivity, 2, 0);
            this.tableLayoutPanelSubHeader2.Controls.Add(this.buttonNewActivity, 1, 0);
            this.tableLayoutPanelSubHeader2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelSubHeader2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanelSubHeader2.Name = "tableLayoutPanelSubHeader2";
            this.tableLayoutPanelSubHeader2.RowCount = 1;
            this.tableLayoutPanelSubHeader2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelSubHeader2.Size = new System.Drawing.Size(585, 34);
            this.tableLayoutPanelSubHeader2.TabIndex = 58;
            // 
            // labelSubHeader2
            // 
            this.labelSubHeader2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelSubHeader2.AutoSize = true;
            this.labelSubHeader2.BackColor = System.Drawing.Color.Transparent;
            this.labelSubHeader2.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSubHeader2.ForeColor = System.Drawing.Color.GhostWhite;
            this.labelSubHeader2.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelSubHeader2.Location = new System.Drawing.Point(3, 8);
            this.labelSubHeader2.Name = "labelSubHeader2";
            this.labelSubHeader2.Size = new System.Drawing.Size(224, 17);
            this.labelSubHeader2.TabIndex = 0;
            this.labelSubHeader2.Text = "xxEnterPropertyValuesForActivities";
            this.labelSubHeader2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // buttonAdvDeleteActivity
            // 
            this.buttonAdvDeleteActivity.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.buttonAdvDeleteActivity.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
            this.buttonAdvDeleteActivity.BackColor = System.Drawing.Color.White;
            this.buttonAdvDeleteActivity.BeforeTouchSize = new System.Drawing.Size(28, 28);
            this.buttonAdvDeleteActivity.ForeColor = System.Drawing.Color.White;
            this.buttonAdvDeleteActivity.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_temp_DeleteGroup1;
            this.buttonAdvDeleteActivity.IsBackStageButton = false;
            this.buttonAdvDeleteActivity.Location = new System.Drawing.Point(550, 3);
            this.buttonAdvDeleteActivity.Margin = new System.Windows.Forms.Padding(0, 1, 0, 0);
            this.buttonAdvDeleteActivity.Name = "buttonAdvDeleteActivity";
            this.buttonAdvDeleteActivity.Size = new System.Drawing.Size(28, 28);
            this.buttonAdvDeleteActivity.TabIndex = 7;
            this.buttonAdvDeleteActivity.TabStop = false;
            this.buttonAdvDeleteActivity.UseVisualStyle = true;
            this.buttonAdvDeleteActivity.Click += new System.EventHandler(this.ButtonAdvDeleteActivityClick);
            // 
            // buttonNewActivity
            // 
            this.buttonNewActivity.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.buttonNewActivity.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
            this.buttonNewActivity.BackColor = System.Drawing.Color.White;
            this.buttonNewActivity.BeforeTouchSize = new System.Drawing.Size(28, 28);
            this.buttonNewActivity.ForeColor = System.Drawing.Color.White;
            this.buttonNewActivity.Image = global::Teleopti.Ccc.Win.Properties.Resources.test_add2;
            this.buttonNewActivity.IsBackStageButton = false;
            this.buttonNewActivity.Location = new System.Drawing.Point(515, 3);
            this.buttonNewActivity.Margin = new System.Windows.Forms.Padding(0, 1, 0, 0);
            this.buttonNewActivity.Name = "buttonNewActivity";
            this.buttonNewActivity.Size = new System.Drawing.Size(28, 28);
            this.buttonNewActivity.TabIndex = 6;
            this.buttonNewActivity.UseVisualStyle = true;
            this.buttonNewActivity.UseVisualStyleBackColor = false;
            this.buttonNewActivity.Click += new System.EventHandler(this.ButtonNewActivityClick);
            // 
            // gridControlActivities
            // 
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
            this.gridControlActivities.BaseStylesMap.AddRange(new Syncfusion.Windows.Forms.Grid.GridBaseStyle[] {
            gridBaseStyle1,
            gridBaseStyle2,
            gridBaseStyle3,
            gridBaseStyle4});
            this.gridControlActivities.ColWidthEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridColWidth[] {
            new Syncfusion.Windows.Forms.Grid.GridColWidth(0, 35)});
            this.gridControlActivities.ContextMenuStrip = this.contextMenuStrip2;
            this.gridControlActivities.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControlActivities.ForeColor = System.Drawing.SystemColors.ControlText;
            this.gridControlActivities.Location = new System.Drawing.Point(3, 43);
            this.gridControlActivities.Name = "gridControlActivities";
            this.gridControlActivities.NumberedRowHeaders = false;
            this.gridControlActivities.Properties.BackgroundColor = System.Drawing.SystemColors.Window;
            this.gridControlActivities.Properties.ForceImmediateRepaint = false;
            this.gridControlActivities.Properties.MarkColHeader = false;
            this.gridControlActivities.Properties.MarkRowHeader = false;
            this.gridControlActivities.RowHeightEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridRowHeight[] {
            new Syncfusion.Windows.Forms.Grid.GridRowHeight(0, 21)});
            this.gridControlActivities.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
            this.gridControlActivities.Size = new System.Drawing.Size(585, 529);
            this.gridControlActivities.SmartSizeBox = false;
            this.gridControlActivities.TabIndex = 60;
            this.gridControlActivities.UseRightToLeftCompatibleTextBox = true;
            this.gridControlActivities.FontChanged += new System.EventHandler(this.gridControlActivities_FontChanged);
            // 
            // contextMenuStrip2
            // 
            this.contextMenuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem3});
            this.contextMenuStrip2.Name = "contextMenuStrip1";
            this.contextMenuStrip2.Size = new System.Drawing.Size(137, 26);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(136, 22);
            this.toolStripMenuItem3.Text = "xxPasteNew";
            this.toolStripMenuItem3.Click += new System.EventHandler(this.ToolStripMenuItemAddFromClipboardClick);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.label2.ForeColor = System.Drawing.Color.GhostWhite;
            this.label2.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label2.Location = new System.Drawing.Point(3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(128, 100);
            this.label2.TabIndex = 0;
            this.label2.Text = "xxGrouping Activities";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // gradientPanelHeader
            // 
            this.gradientPanelHeader.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gradientPanelHeader.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.gradientPanelHeader.BorderSingle = System.Windows.Forms.ButtonBorderStyle.None;
            this.gradientPanelHeader.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gradientPanelHeader.Controls.Add(this.tableLayoutPanelHeader);
            this.gradientPanelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.gradientPanelHeader.Location = new System.Drawing.Point(0, 0);
            this.gradientPanelHeader.Name = "gradientPanelHeader";
            this.gradientPanelHeader.Padding = new System.Windows.Forms.Padding(12);
            this.gradientPanelHeader.Size = new System.Drawing.Size(591, 62);
            this.gradientPanelHeader.TabIndex = 56;
            // 
            // tableLayoutPanelHeader
            // 
            this.tableLayoutPanelHeader.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanelHeader.ColumnCount = 1;
            this.tableLayoutPanelHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 601F));
            this.tableLayoutPanelHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelHeader.Controls.Add(this.labelHeader, 1, 0);
            this.tableLayoutPanelHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelHeader.Location = new System.Drawing.Point(12, 12);
            this.tableLayoutPanelHeader.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanelHeader.Name = "tableLayoutPanelHeader";
            this.tableLayoutPanelHeader.RowCount = 1;
            this.tableLayoutPanelHeader.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelHeader.Size = new System.Drawing.Size(567, 38);
            this.tableLayoutPanelHeader.TabIndex = 0;
            // 
            // labelHeader
            // 
            this.labelHeader.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelHeader.AutoSize = true;
            this.labelHeader.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelHeader.ForeColor = System.Drawing.Color.MidnightBlue;
            this.labelHeader.Location = new System.Drawing.Point(3, 6);
            this.labelHeader.Name = "labelHeader";
            this.labelHeader.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.labelHeader.Size = new System.Drawing.Size(191, 25);
            this.labelHeader.TabIndex = 0;
            this.labelHeader.Text = "xxManageActivities";
            // 
            // ActivityControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.tableLayoutPanelBody);
            this.Controls.Add(this.gradientPanelHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "ActivityControl";
            this.Size = new System.Drawing.Size(591, 637);
            this.Layout += new System.Windows.Forms.LayoutEventHandler(this.ActivityControlLayout);
            this.tableLayoutPanelBody.ResumeLayout(false);
            this.tableLayoutPanelSubHeader1.ResumeLayout(false);
            this.tableLayoutPanelSubHeader1.PerformLayout();
            this.tableLayoutPanelSubHeader2.ResumeLayout(false);
            this.tableLayoutPanelSubHeader2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControlActivities)).EndInit();
            this.contextMenuStrip2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanelHeader)).EndInit();
            this.gradientPanelHeader.ResumeLayout(false);
            this.tableLayoutPanelHeader.ResumeLayout(false);
            this.tableLayoutPanelHeader.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelBody;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemAddFromClipboard;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelSubHeader1;
        private System.Windows.Forms.Label labelSubHeader1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelSubHeader2;
        private System.Windows.Forms.Label labelSubHeader2;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvDeleteActivity;
        private Syncfusion.Windows.Forms.ButtonAdv buttonNewActivity;
        private System.Windows.Forms.Label label2;
        private Syncfusion.Windows.Forms.Grid.GridControl gridControlActivities;
        private System.Windows.Forms.ToolTip toolTip1;
        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanelHeader;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelHeader;
        private System.Windows.Forms.Label labelHeader;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
    }
}
