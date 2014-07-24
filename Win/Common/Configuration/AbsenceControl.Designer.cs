namespace Teleopti.Ccc.Win.Common.Configuration
{
    partial class AbsenceControl
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
			if (disposing && _gridColumnHelper != null) _gridColumnHelper.Dispose();
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel7 = new System.Windows.Forms.TableLayoutPanel();
            this.labelTimeDirectives = new System.Windows.Forms.Label();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonAdvDelete = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonNew = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.autoLabel4 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
            this.tableLayoutPanelBody = new System.Windows.Forms.TableLayoutPanel();
            this.gridControlAbsences = new Syncfusion.Windows.Forms.Grid.GridControl();
            this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanelSubHeader2 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonAdvDeleteAbsence = new Syncfusion.Windows.Forms.ButtonAdv();
            this.labelSubHeader2 = new System.Windows.Forms.Label();
            this.buttonAdvAddAbsence = new Syncfusion.Windows.Forms.ButtonAdv();
            this.gradientPanelHeader = new Syncfusion.Windows.Forms.Tools.GradientPanel();
            this.tableLayoutPanelHeader = new System.Windows.Forms.TableLayoutPanel();
            this.labelHeader = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel7.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.tableLayoutPanelBody.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControlAbsences)).BeginInit();
            this.contextMenuStrip2.SuspendLayout();
            this.tableLayoutPanelSubHeader2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanelHeader)).BeginInit();
            this.gradientPanelHeader.SuspendLayout();
            this.tableLayoutPanelHeader.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel7, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(200, 100);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel7
            // 
            this.tableLayoutPanel7.BackColor = System.Drawing.Color.LightSteelBlue;
            this.tableLayoutPanel7.ColumnCount = 1;
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel7.Controls.Add(this.labelTimeDirectives, 0, 0);
            this.tableLayoutPanel7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel7.Location = new System.Drawing.Point(3, 43);
            this.tableLayoutPanel7.Name = "tableLayoutPanel7";
            this.tableLayoutPanel7.RowCount = 1;
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel7.Size = new System.Drawing.Size(194, 14);
            this.tableLayoutPanel7.TabIndex = 3;
            this.tableLayoutPanel7.Visible = false;
            // 
            // labelTimeDirectives
            // 
            this.labelTimeDirectives.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelTimeDirectives.AutoSize = true;
            this.labelTimeDirectives.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.labelTimeDirectives.ForeColor = System.Drawing.Color.GhostWhite;
            this.labelTimeDirectives.Location = new System.Drawing.Point(3, 0);
            this.labelTimeDirectives.Name = "labelTimeDirectives";
            this.labelTimeDirectives.Size = new System.Drawing.Size(152, 14);
            this.labelTimeDirectives.TabIndex = 0;
            this.labelTimeDirectives.Text = "xxSchedule your contract weekly";
            this.labelTimeDirectives.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.BackColor = System.Drawing.Color.LightSteelBlue;
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.buttonAdvDelete, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.buttonNew, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(194, 14);
            this.tableLayoutPanel2.TabIndex = 1;
            this.tableLayoutPanel2.Visible = false;
            // 
            // buttonAdvDelete
            // 
            this.buttonAdvDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAdvDelete.BeforeTouchSize = new System.Drawing.Size(24, 24);
            this.buttonAdvDelete.Image = global::Teleopti.Ccc.Win.Properties.Resources.test_delete_32x32;
            this.buttonAdvDelete.IsBackStageButton = false;
            this.buttonAdvDelete.Location = new System.Drawing.Point(170, 3);
            this.buttonAdvDelete.Margin = new System.Windows.Forms.Padding(3, 3, 0, 1);
            this.buttonAdvDelete.Name = "buttonAdvDelete";
            this.buttonAdvDelete.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.buttonAdvDelete.Size = new System.Drawing.Size(24, 24);
            this.buttonAdvDelete.TabIndex = 7;
            this.buttonAdvDelete.TabStop = false;
            // 
            // buttonNew
            // 
            this.buttonNew.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonNew.BackColor = System.Drawing.Color.White;
            this.buttonNew.Image = global::Teleopti.Ccc.Win.Properties.Resources.test_add2;
            this.buttonNew.Location = new System.Drawing.Point(143, 3);
            this.buttonNew.Margin = new System.Windows.Forms.Padding(3, 3, 0, 1);
            this.buttonNew.Name = "buttonNew";
            this.buttonNew.Size = new System.Drawing.Size(24, 24);
            this.buttonNew.TabIndex = 6;
            this.buttonNew.UseVisualStyleBackColor = false;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.label1.ForeColor = System.Drawing.Color.GhostWhite;
            this.label1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(115, 28);
            this.label1.TabIndex = 0;
            this.label1.Text = "xxPersonalize your schedule";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 1;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.Controls.Add(this.autoLabel4, 0, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 2;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(200, 100);
            this.tableLayoutPanel5.TabIndex = 0;
            // 
            // autoLabel4
            // 
            this.autoLabel4.AutoSize = false;
            this.autoLabel4.Location = new System.Drawing.Point(3, 0);
            this.autoLabel4.Name = "autoLabel4";
            this.autoLabel4.Size = new System.Drawing.Size(160, 20);
            this.autoLabel4.TabIndex = 34;
            this.autoLabel4.Text = "xxMy Weekly Contract Schedule";
            this.autoLabel4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanelBody
            // 
            this.tableLayoutPanelBody.ColumnCount = 1;
            this.tableLayoutPanelBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelBody.Controls.Add(this.gridControlAbsences, 0, 1);
            this.tableLayoutPanelBody.Controls.Add(this.tableLayoutPanelSubHeader2, 0, 0);
            this.tableLayoutPanelBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelBody.Location = new System.Drawing.Point(0, 62);
            this.tableLayoutPanelBody.Name = "tableLayoutPanelBody";
            this.tableLayoutPanelBody.RowCount = 2;
            this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelBody.Size = new System.Drawing.Size(697, 503);
            this.tableLayoutPanelBody.TabIndex = 56;
            // 
            // gridControlAbsences
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
            this.gridControlAbsences.BaseStylesMap.AddRange(new Syncfusion.Windows.Forms.Grid.GridBaseStyle[] {
            gridBaseStyle1,
            gridBaseStyle2,
            gridBaseStyle3,
            gridBaseStyle4});
            this.gridControlAbsences.ColWidthEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridColWidth[] {
            new Syncfusion.Windows.Forms.Grid.GridColWidth(0, 35)});
            this.gridControlAbsences.ContextMenuStrip = this.contextMenuStrip2;
            this.gridControlAbsences.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControlAbsences.ForeColor = System.Drawing.SystemColors.ControlText;
            this.gridControlAbsences.Location = new System.Drawing.Point(3, 43);
            this.gridControlAbsences.Name = "gridControlAbsences";
            this.gridControlAbsences.NumberedRowHeaders = false;
            this.gridControlAbsences.Properties.BackgroundColor = System.Drawing.SystemColors.Window;
            this.gridControlAbsences.Properties.ForceImmediateRepaint = false;
            this.gridControlAbsences.Properties.MarkColHeader = false;
            this.gridControlAbsences.Properties.MarkRowHeader = false;
            this.gridControlAbsences.RowHeightEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridRowHeight[] {
            new Syncfusion.Windows.Forms.Grid.GridRowHeight(0, 21)});
            this.gridControlAbsences.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
            this.gridControlAbsences.Size = new System.Drawing.Size(691, 457);
            this.gridControlAbsences.SmartSizeBox = false;
            this.gridControlAbsences.TabIndex = 0;
            this.gridControlAbsences.UseRightToLeftCompatibleTextBox = true;
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
            this.toolStripMenuItem3.Click += new System.EventHandler(this.ToolStripMenuItem3Click);
            // 
            // tableLayoutPanelSubHeader2
            // 
            this.tableLayoutPanelSubHeader2.BackColor = System.Drawing.Color.LightSteelBlue;
            this.tableLayoutPanelSubHeader2.ColumnCount = 3;
            this.tableLayoutPanelSubHeader2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelSubHeader2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanelSubHeader2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanelSubHeader2.Controls.Add(this.buttonAdvDeleteAbsence, 2, 0);
            this.tableLayoutPanelSubHeader2.Controls.Add(this.labelSubHeader2, 0, 0);
            this.tableLayoutPanelSubHeader2.Controls.Add(this.buttonAdvAddAbsence, 1, 0);
            this.tableLayoutPanelSubHeader2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelSubHeader2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanelSubHeader2.Name = "tableLayoutPanelSubHeader2";
            this.tableLayoutPanelSubHeader2.RowCount = 1;
            this.tableLayoutPanelSubHeader2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tableLayoutPanelSubHeader2.Size = new System.Drawing.Size(691, 34);
            this.tableLayoutPanelSubHeader2.TabIndex = 3;
            // 
            // buttonAdvDeleteAbsence
            // 
            this.buttonAdvDeleteAbsence.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.buttonAdvDeleteAbsence.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
            this.buttonAdvDeleteAbsence.BackColor = System.Drawing.Color.White;
            this.buttonAdvDeleteAbsence.BeforeTouchSize = new System.Drawing.Size(28, 28);
            this.buttonAdvDeleteAbsence.ForeColor = System.Drawing.Color.White;
            this.buttonAdvDeleteAbsence.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_temp_DeleteGroup;
            this.buttonAdvDeleteAbsence.IsBackStageButton = false;
            this.buttonAdvDeleteAbsence.Location = new System.Drawing.Point(656, 3);
            this.buttonAdvDeleteAbsence.Margin = new System.Windows.Forms.Padding(0, 1, 3, 0);
            this.buttonAdvDeleteAbsence.Name = "buttonAdvDeleteAbsence";
            this.buttonAdvDeleteAbsence.Size = new System.Drawing.Size(28, 28);
            this.buttonAdvDeleteAbsence.TabIndex = 8;
            this.buttonAdvDeleteAbsence.TabStop = false;
            this.buttonAdvDeleteAbsence.UseVisualStyle = true;
            this.buttonAdvDeleteAbsence.Click += new System.EventHandler(this.ButtonAdvDeleteAbsenceClick);
            // 
            // labelSubHeader2
            // 
            this.labelSubHeader2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelSubHeader2.AutoSize = true;
            this.labelSubHeader2.BackColor = System.Drawing.Color.Transparent;
            this.labelSubHeader2.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSubHeader2.ForeColor = System.Drawing.Color.GhostWhite;
            this.labelSubHeader2.Location = new System.Drawing.Point(3, 8);
            this.labelSubHeader2.Name = "labelSubHeader2";
            this.labelSubHeader2.Size = new System.Drawing.Size(224, 17);
            this.labelSubHeader2.TabIndex = 0;
            this.labelSubHeader2.Text = "xxEnterPropertyValuesForAbsences";
            this.labelSubHeader2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // buttonAdvAddAbsence
            // 
            this.buttonAdvAddAbsence.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.buttonAdvAddAbsence.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
            this.buttonAdvAddAbsence.AutoSize = true;
            this.buttonAdvAddAbsence.BackColor = System.Drawing.Color.White;
            this.buttonAdvAddAbsence.BeforeTouchSize = new System.Drawing.Size(28, 28);
            this.buttonAdvAddAbsence.ForeColor = System.Drawing.Color.White;
            this.buttonAdvAddAbsence.Image = global::Teleopti.Ccc.Win.Properties.Resources.test_add2;
            this.buttonAdvAddAbsence.IsBackStageButton = false;
            this.buttonAdvAddAbsence.Location = new System.Drawing.Point(621, 3);
            this.buttonAdvAddAbsence.Margin = new System.Windows.Forms.Padding(0, 1, 3, 0);
            this.buttonAdvAddAbsence.Name = "buttonAdvAddAbsence";
            this.buttonAdvAddAbsence.Size = new System.Drawing.Size(28, 28);
            this.buttonAdvAddAbsence.TabIndex = 9;
            this.buttonAdvAddAbsence.UseVisualStyle = true;
            this.buttonAdvAddAbsence.UseVisualStyleBackColor = false;
            this.buttonAdvAddAbsence.Click += new System.EventHandler(this.ButtonAddAdvAbsenceClick);
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
            this.gradientPanelHeader.Size = new System.Drawing.Size(697, 62);
            this.gradientPanelHeader.TabIndex = 55;
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
            this.tableLayoutPanelHeader.Size = new System.Drawing.Size(673, 38);
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
            this.labelHeader.Size = new System.Drawing.Size(186, 25);
            this.labelHeader.TabIndex = 0;
            this.labelHeader.Text = "xxManageAbsence";
            // 
            // AbsenceControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.tableLayoutPanelBody);
            this.Controls.Add(this.gradientPanelHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "AbsenceControl";
            this.Size = new System.Drawing.Size(697, 565);
            this.Layout += new System.Windows.Forms.LayoutEventHandler(this.AbsenceControlLayout);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel7.ResumeLayout(false);
            this.tableLayoutPanel7.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanelBody.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridControlAbsences)).EndInit();
            this.contextMenuStrip2.ResumeLayout(false);
            this.tableLayoutPanelSubHeader2.ResumeLayout(false);
            this.tableLayoutPanelSubHeader2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanelHeader)).EndInit();
            this.gradientPanelHeader.ResumeLayout(false);
            this.tableLayoutPanelHeader.ResumeLayout(false);
            this.tableLayoutPanelHeader.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanelHeader;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelHeader;
        private System.Windows.Forms.Label labelHeader;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel7;
        private System.Windows.Forms.Label labelTimeDirectives;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvDelete;
        private System.Windows.Forms.Button buttonNew;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel4;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelBody;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelSubHeader2;
        private System.Windows.Forms.Label labelSubHeader2;
        private Syncfusion.Windows.Forms.Grid.GridControl gridControlAbsences;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvAddAbsence;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvDeleteAbsence;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
    }
}
