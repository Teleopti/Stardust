using System.Windows.Forms;

namespace Teleopti.Ccc.Win.Common.Configuration
{
    partial class ShiftCategorySettingsControl
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
                if (_gridHelper != null) _gridHelper.Dispose();
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
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle5 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle6 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle7 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle8 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			this.toolStripMenuItemAddFromClipboard = new System.Windows.Forms.ToolStripMenuItem();
			this.tableLayoutPanelBody = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanelSubHeader1 = new System.Windows.Forms.TableLayoutPanel();
			this.buttonAdvDeleteShiftCategory = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonNewShiftCategory = new Syncfusion.Windows.Forms.ButtonAdv();
			this.gridControlShiftCategory = new Syncfusion.Windows.Forms.Grid.GridControl();
			this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.gradientPanelHeader = new Syncfusion.Windows.Forms.Tools.GradientPanel();
			this.tableLayoutPanelHeader = new System.Windows.Forms.TableLayoutPanel();
			this.labelHeader = new System.Windows.Forms.Label();
			this.labelSubHeader2 = new System.Windows.Forms.Label();
			this.tableLayoutPanelBody.SuspendLayout();
			this.tableLayoutPanelSubHeader1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridControlShiftCategory)).BeginInit();
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
			this.toolStripMenuItemAddFromClipboard.Click += new System.EventHandler(this.toolStripMenuItemAddFromClipboardClick);
			// 
			// tableLayoutPanelBody
			// 
			this.tableLayoutPanelBody.ColumnCount = 1;
			this.tableLayoutPanelBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelBody.Controls.Add(this.tableLayoutPanelSubHeader1, 0, 0);
			this.tableLayoutPanelBody.Controls.Add(this.gridControlShiftCategory, 0, 1);
			this.tableLayoutPanelBody.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelBody.Location = new System.Drawing.Point(0, 62);
			this.tableLayoutPanelBody.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanelBody.Name = "tableLayoutPanelBody";
			this.tableLayoutPanelBody.RowCount = 2;
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelBody.Size = new System.Drawing.Size(626, 575);
			this.tableLayoutPanelBody.TabIndex = 1;
			// 
			// tableLayoutPanelSubHeader1
			// 
			this.tableLayoutPanelSubHeader1.BackColor = System.Drawing.Color.LightSteelBlue;
			this.tableLayoutPanelSubHeader1.ColumnCount = 3;
			this.tableLayoutPanelSubHeader1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelSubHeader1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelSubHeader1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelSubHeader1.Controls.Add(this.labelSubHeader2, 0, 0);
			this.tableLayoutPanelSubHeader1.Controls.Add(this.buttonAdvDeleteShiftCategory, 2, 0);
			this.tableLayoutPanelSubHeader1.Controls.Add(this.buttonNewShiftCategory, 1, 0);
			this.tableLayoutPanelSubHeader1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelSubHeader1.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanelSubHeader1.Name = "tableLayoutPanelSubHeader1";
			this.tableLayoutPanelSubHeader1.RowCount = 1;
			this.tableLayoutPanelSubHeader1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelSubHeader1.Size = new System.Drawing.Size(620, 34);
			this.tableLayoutPanelSubHeader1.TabIndex = 0;
			// 
			// buttonAdvDeleteShiftCategory
			// 
			this.buttonAdvDeleteShiftCategory.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.buttonAdvDeleteShiftCategory.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvDeleteShiftCategory.BackColor = System.Drawing.Color.White;
			this.buttonAdvDeleteShiftCategory.BeforeTouchSize = new System.Drawing.Size(28, 28);
			this.buttonAdvDeleteShiftCategory.ForeColor = System.Drawing.Color.White;
			this.buttonAdvDeleteShiftCategory.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_temp_DeleteGroup10;
			this.buttonAdvDeleteShiftCategory.IsBackStageButton = false;
			this.buttonAdvDeleteShiftCategory.Location = new System.Drawing.Point(585, 3);
			this.buttonAdvDeleteShiftCategory.Margin = new System.Windows.Forms.Padding(0, 1, 3, 0);
			this.buttonAdvDeleteShiftCategory.Name = "buttonAdvDeleteShiftCategory";
			this.buttonAdvDeleteShiftCategory.Size = new System.Drawing.Size(28, 28);
			this.buttonAdvDeleteShiftCategory.TabIndex = 0;
			this.buttonAdvDeleteShiftCategory.TabStop = false;
			this.buttonAdvDeleteShiftCategory.UseVisualStyle = true;
			this.buttonAdvDeleteShiftCategory.Click += new System.EventHandler(this.buttonAdvDeleteShiftCategoryClick);
			// 
			// buttonNewShiftCategory
			// 
			this.buttonNewShiftCategory.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.buttonNewShiftCategory.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonNewShiftCategory.BackColor = System.Drawing.Color.White;
			this.buttonNewShiftCategory.BeforeTouchSize = new System.Drawing.Size(28, 28);
			this.buttonNewShiftCategory.ForeColor = System.Drawing.Color.White;
			this.buttonNewShiftCategory.Image = global::Teleopti.Ccc.Win.Properties.Resources.test_add2;
			this.buttonNewShiftCategory.IsBackStageButton = false;
			this.buttonNewShiftCategory.Location = new System.Drawing.Point(550, 3);
			this.buttonNewShiftCategory.Margin = new System.Windows.Forms.Padding(0, 1, 3, 0);
			this.buttonNewShiftCategory.Name = "buttonNewShiftCategory";
			this.buttonNewShiftCategory.Size = new System.Drawing.Size(28, 28);
			this.buttonNewShiftCategory.TabIndex = 1;
			this.buttonNewShiftCategory.UseVisualStyle = true;
			this.buttonNewShiftCategory.UseVisualStyleBackColor = false;
			this.buttonNewShiftCategory.Click += new System.EventHandler(this.buttonNewShiftCategoryClick);
			// 
			// gridControlShiftCategory
			// 
			this.gridControlShiftCategory.AlphaBlendSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(94)))), ((int)(((byte)(171)))), ((int)(((byte)(222)))));
			gridBaseStyle5.Name = "Header";
			gridBaseStyle5.StyleInfo.Borders.Bottom = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
			gridBaseStyle5.StyleInfo.Borders.Left = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
			gridBaseStyle5.StyleInfo.Borders.Right = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
			gridBaseStyle5.StyleInfo.Borders.Top = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
			gridBaseStyle5.StyleInfo.CellType = "Header";
			gridBaseStyle5.StyleInfo.Font.Bold = true;
			gridBaseStyle5.StyleInfo.Interior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(199)))), ((int)(((byte)(184))))), System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(234)))), ((int)(((byte)(216))))));
			gridBaseStyle5.StyleInfo.VerticalAlignment = Syncfusion.Windows.Forms.Grid.GridVerticalAlignment.Middle;
			gridBaseStyle6.Name = "Standard";
			gridBaseStyle6.StyleInfo.Font.Facename = "Tahoma";
			gridBaseStyle6.StyleInfo.Interior = new Syncfusion.Drawing.BrushInfo(System.Drawing.SystemColors.Window);
			gridBaseStyle7.Name = "Column Header";
			gridBaseStyle7.StyleInfo.BaseStyle = "Header";
			gridBaseStyle7.StyleInfo.HorizontalAlignment = Syncfusion.Windows.Forms.Grid.GridHorizontalAlignment.Center;
			gridBaseStyle8.Name = "Row Header";
			gridBaseStyle8.StyleInfo.BaseStyle = "Header";
			gridBaseStyle8.StyleInfo.HorizontalAlignment = Syncfusion.Windows.Forms.Grid.GridHorizontalAlignment.Left;
			gridBaseStyle8.StyleInfo.Interior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Horizontal, System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(199)))), ((int)(((byte)(184))))), System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(234)))), ((int)(((byte)(216))))));
			this.gridControlShiftCategory.BaseStylesMap.AddRange(new Syncfusion.Windows.Forms.Grid.GridBaseStyle[] {
            gridBaseStyle5,
            gridBaseStyle6,
            gridBaseStyle7,
            gridBaseStyle8});
			this.gridControlShiftCategory.ColWidthEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridColWidth[] {
            new Syncfusion.Windows.Forms.Grid.GridColWidth(0, 35)});
			this.gridControlShiftCategory.ContextMenuStrip = this.contextMenuStrip2;
			this.gridControlShiftCategory.DefaultGridBorderStyle = Syncfusion.Windows.Forms.Grid.GridBorderStyle.Solid;
			this.gridControlShiftCategory.DefaultRowHeight = 20;
			this.gridControlShiftCategory.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridControlShiftCategory.ForeColor = System.Drawing.SystemColors.ControlText;
			this.gridControlShiftCategory.GridOfficeScrollBars = Syncfusion.Windows.Forms.OfficeScrollBars.Metro;
			this.gridControlShiftCategory.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Metro;
			this.gridControlShiftCategory.Location = new System.Drawing.Point(3, 43);
			this.gridControlShiftCategory.MetroScrollBars = true;
			this.gridControlShiftCategory.Name = "gridControlShiftCategory";
			this.gridControlShiftCategory.NumberedRowHeaders = false;
			this.gridControlShiftCategory.Properties.BackgroundColor = System.Drawing.SystemColors.Window;
			this.gridControlShiftCategory.Properties.ForceImmediateRepaint = false;
			this.gridControlShiftCategory.Properties.GridLineColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(212)))), ((int)(((byte)(212)))));
			this.gridControlShiftCategory.Properties.MarkColHeader = false;
			this.gridControlShiftCategory.Properties.MarkRowHeader = false;
			this.gridControlShiftCategory.RowHeightEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridRowHeight[] {
            new Syncfusion.Windows.Forms.Grid.GridRowHeight(0, 29)});
			this.gridControlShiftCategory.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
			this.gridControlShiftCategory.Size = new System.Drawing.Size(620, 529);
			this.gridControlShiftCategory.SmartSizeBox = false;
			this.gridControlShiftCategory.TabIndex = 1;
			this.gridControlShiftCategory.ThemesEnabled = true;
			this.gridControlShiftCategory.UseRightToLeftCompatibleTextBox = true;
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
			this.toolStripMenuItem3.Click += new System.EventHandler(this.toolStripMenuItemAddFromClipboardClick);
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
			this.gradientPanelHeader.Size = new System.Drawing.Size(626, 62);
			this.gradientPanelHeader.TabIndex = 0;
			// 
			// tableLayoutPanelHeader
			// 
			this.tableLayoutPanelHeader.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanelHeader.ColumnCount = 1;
			this.tableLayoutPanelHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 603F));
			this.tableLayoutPanelHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelHeader.Controls.Add(this.labelHeader, 1, 0);
			this.tableLayoutPanelHeader.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelHeader.Location = new System.Drawing.Point(12, 12);
			this.tableLayoutPanelHeader.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanelHeader.Name = "tableLayoutPanelHeader";
			this.tableLayoutPanelHeader.RowCount = 1;
			this.tableLayoutPanelHeader.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelHeader.Size = new System.Drawing.Size(602, 38);
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
			this.labelHeader.Size = new System.Drawing.Size(247, 25);
			this.labelHeader.TabIndex = 0;
			this.labelHeader.Text = "xxManageShiftCategories";
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
			this.labelSubHeader2.Size = new System.Drawing.Size(261, 17);
			this.labelSubHeader2.TabIndex = 2;
			this.labelSubHeader2.Text = "xxEnterPropertyValuesForShiftCategories";
			this.labelSubHeader2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// ShiftCategorySettingsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Window;
			this.Controls.Add(this.tableLayoutPanelBody);
			this.Controls.Add(this.gradientPanelHeader);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Margin = new System.Windows.Forms.Padding(0);
			this.Name = "ShiftCategorySettingsControl";
			this.Size = new System.Drawing.Size(626, 637);
			this.Layout += new System.Windows.Forms.LayoutEventHandler(this.shiftCategorySettingsControlLayout);
			this.tableLayoutPanelBody.ResumeLayout(false);
			this.tableLayoutPanelSubHeader1.ResumeLayout(false);
			this.tableLayoutPanelSubHeader1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridControlShiftCategory)).EndInit();
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
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvDeleteShiftCategory;
        private Syncfusion.Windows.Forms.ButtonAdv buttonNewShiftCategory;
        private Syncfusion.Windows.Forms.Grid.GridControl gridControlShiftCategory;
        private System.Windows.Forms.ToolTip toolTip1;
        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanelHeader;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelHeader;
        private System.Windows.Forms.Label labelHeader;
        private ContextMenuStrip contextMenuStrip2;
        private ToolStripMenuItem toolStripMenuItem3;
		  private Label labelSubHeader2;
    }
}
