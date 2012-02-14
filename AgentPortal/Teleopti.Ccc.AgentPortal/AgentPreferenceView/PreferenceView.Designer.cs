using Teleopti.Ccc.AgentPortalCode.AgentPreference;

namespace Teleopti.Ccc.AgentPortal.AgentPreferenceView
{
    partial class PreferenceView
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxAddAbsence")]
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.gridControl1 = new Syncfusion.Windows.Forms.Grid.GridControl();
            this.contextMenuStripPreference = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemAddAbsence = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemAddDayOff = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemAddShiftCategory = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemSaveAsTemplate = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemCut = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemPaste = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.autoLabelPeriodInformation = new Syncfusion.Windows.Forms.Tools.AutoLabel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.labelPeriodCalculationInfo = new System.Windows.Forms.Label();
            this.autoLabelDayOffs = new Syncfusion.Windows.Forms.Tools.AutoLabel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.labelInfo = new System.Windows.Forms.Label();
            this.editExtendedPreferenceView1 = new Teleopti.Ccc.AgentPortal.AgentPreferenceView.EditExtendedPreferenceView();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            this.contextMenuStripPreference.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // gridControl1
            // 
            this.gridControl1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.gridControl1.ColCount = 7;
            this.gridControl1.ContextMenuStrip = this.contextMenuStripPreference;
            this.gridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl1.ExcelLikeCurrentCell = true;
            this.gridControl1.ExcelLikeSelectionFrame = true;
            this.gridControl1.GridOfficeScrollBars = Syncfusion.Windows.Forms.OfficeScrollBars.Office2007;
            this.gridControl1.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Office2007Blue;
            this.gridControl1.HScrollBehavior = Syncfusion.Windows.Forms.Grid.GridScrollbarMode.Automatic;
            this.gridControl1.Location = new System.Drawing.Point(0, 100);
            this.gridControl1.Margin = new System.Windows.Forms.Padding(0);
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.NumberedColHeaders = false;
            this.gridControl1.NumberedRowHeaders = false;
            this.gridControl1.Office2007ScrollBars = true;
            this.gridControl1.Properties.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.gridControl1.ResizeColsBehavior = Syncfusion.Windows.Forms.Grid.GridResizeCellsBehavior.None;
            this.gridControl1.ResizeRowsBehavior = Syncfusion.Windows.Forms.Grid.GridResizeCellsBehavior.None;
            this.gridControl1.RowCount = 1;
            this.gridControl1.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
            this.gridControl1.ShowCurrentCellBorderBehavior = Syncfusion.Windows.Forms.Grid.GridShowCurrentCellBorder.AlwaysVisible;
            this.gridControl1.Size = new System.Drawing.Size(874, 224);
            this.gridControl1.SmartSizeBox = false;
            this.gridControl1.TabIndex = 0;
            this.gridControl1.Text = "gridControl1";
            this.gridControl1.ThemesEnabled = true;
            this.gridControl1.UseRightToLeftCompatibleTextBox = true;
            this.gridControl1.VScrollBehavior = Syncfusion.Windows.Forms.Grid.GridScrollbarMode.Automatic;
            this.gridControl1.QueryCellInfo += new Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventHandler(this.gridControl1_QueryCellInfo);
            // 
            // contextMenuStripPreference
            // 
            this.contextMenuStripPreference.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemAddAbsence,
            this.toolStripMenuItemAddDayOff,
            this.toolStripMenuItemAddShiftCategory,
            this.toolStripMenuItemSaveAsTemplate,
            this.toolStripSeparator1,
            this.toolStripMenuItemCopy,
            this.toolStripMenuItemCut,
            this.toolStripMenuItemPaste,
            this.toolStripMenuItemDelete});
            this.contextMenuStripPreference.Name = "contextMenuStripPreference";
            this.contextMenuStripPreference.Size = new System.Drawing.Size(179, 208);
            // 
            // toolStripMenuItemAddAbsence
            // 
            this.toolStripMenuItemAddAbsence.Name = "toolStripMenuItemAddAbsence";
            this.toolStripMenuItemAddAbsence.Size = new System.Drawing.Size(178, 22);
            this.toolStripMenuItemAddAbsence.Text = "xxAddAbsence";
            // 
            // toolStripMenuItemAddDayOff
            // 
            this.toolStripMenuItemAddDayOff.Name = "toolStripMenuItemAddDayOff";
            this.toolStripMenuItemAddDayOff.Size = new System.Drawing.Size(178, 22);
            this.toolStripMenuItemAddDayOff.Text = "xxAddDayOff";
            // 
            // toolStripMenuItemAddShiftCategory
            // 
            this.toolStripMenuItemAddShiftCategory.Name = "toolStripMenuItemAddShiftCategory";
            this.toolStripMenuItemAddShiftCategory.Size = new System.Drawing.Size(178, 22);
            this.toolStripMenuItemAddShiftCategory.Text = "xxAddShiftCategory";
            // 
            // toolStripMenuItemSaveAsTemplate
            // 
            this.toolStripMenuItemSaveAsTemplate.Name = "toolStripMenuItemSaveAsTemplate";
            this.toolStripMenuItemSaveAsTemplate.Size = new System.Drawing.Size(178, 22);
            this.toolStripMenuItemSaveAsTemplate.Text = "xxSaveAsTemplate";
            this.toolStripMenuItemSaveAsTemplate.Click += new System.EventHandler(this.OnSaveTemplateSelected);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(175, 6);
            // 
            // toolStripMenuItemCopy
            // 
            this.toolStripMenuItemCopy.Name = "toolStripMenuItemCopy";
            this.toolStripMenuItemCopy.Size = new System.Drawing.Size(178, 22);
            this.toolStripMenuItemCopy.Text = "xxCopy";
            // 
            // toolStripMenuItemCut
            // 
            this.toolStripMenuItemCut.Name = "toolStripMenuItemCut";
            this.toolStripMenuItemCut.Size = new System.Drawing.Size(178, 22);
            this.toolStripMenuItemCut.Text = "xxCut";
            // 
            // toolStripMenuItemPaste
            // 
            this.toolStripMenuItemPaste.Enabled = false;
            this.toolStripMenuItemPaste.Name = "toolStripMenuItemPaste";
            this.toolStripMenuItemPaste.Size = new System.Drawing.Size(178, 22);
            this.toolStripMenuItemPaste.Text = "xxPaste";
            // 
            // toolStripMenuItemDelete
            // 
            this.toolStripMenuItemDelete.Name = "toolStripMenuItemDelete";
            this.toolStripMenuItemDelete.Size = new System.Drawing.Size(178, 22);
            this.toolStripMenuItemDelete.Text = "xxDelete";
            // 
            // autoLabelPeriodInformation
            // 
            this.autoLabelPeriodInformation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.autoLabelPeriodInformation.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.autoLabelPeriodInformation.Location = new System.Drawing.Point(3, 0);
            this.autoLabelPeriodInformation.Name = "autoLabelPeriodInformation";
            this.autoLabelPeriodInformation.Size = new System.Drawing.Size(195, 24);
            this.autoLabelPeriodInformation.TabIndex = 1;
            this.autoLabelPeriodInformation.Text = "xxPeriodIsNotValidated";
            this.autoLabelPeriodInformation.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.gridControl1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.editExtendedPreferenceView1, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 104F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(874, 428);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.labelPeriodCalculationInfo, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.autoLabelDayOffs, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.pictureBox1, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.autoLabelPeriodInformation, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.labelInfo, 0, 3);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 4;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(868, 94);
            this.tableLayoutPanel2.TabIndex = 3;
            // 
            // labelPeriodCalculationInfo
            // 
            this.labelPeriodCalculationInfo.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelPeriodCalculationInfo.AutoSize = true;
            this.labelPeriodCalculationInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPeriodCalculationInfo.ForeColor = System.Drawing.Color.Black;
            this.labelPeriodCalculationInfo.Location = new System.Drawing.Point(3, 27);
            this.labelPeriodCalculationInfo.Name = "labelPeriodCalculationInfo";
            this.labelPeriodCalculationInfo.Size = new System.Drawing.Size(0, 17);
            this.labelPeriodCalculationInfo.TabIndex = 5;
            this.labelPeriodCalculationInfo.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // autoLabelDayOffs
            // 
            this.autoLabelDayOffs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.autoLabelDayOffs.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.autoLabelDayOffs.ForeColor = System.Drawing.Color.Red;
            this.autoLabelDayOffs.Location = new System.Drawing.Point(3, 48);
            this.autoLabelDayOffs.Name = "autoLabelDayOffs";
            this.autoLabelDayOffs.Size = new System.Drawing.Size(195, 25);
            this.autoLabelDayOffs.TabIndex = 4;
            this.autoLabelDayOffs.Text = "xxDaysOffNotValid";
            this.autoLabelDayOffs.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(204, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.tableLayoutPanel2.SetRowSpan(this.pictureBox1, 2);
            this.pictureBox1.Size = new System.Drawing.Size(100, 42);
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // labelInfo
            // 
            this.labelInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelInfo.AutoSize = true;
            this.labelInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelInfo.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.labelInfo.Location = new System.Drawing.Point(3, 81);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(65, 13);
            this.labelInfo.TabIndex = 3;
            this.labelInfo.Text = "xxPeriodInfo";
            this.labelInfo.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // editExtendedPreferenceView1
            // 
            this.editExtendedPreferenceView1.Activity = null;
            this.editExtendedPreferenceView1.ActivityEnabled = true;
            this.editExtendedPreferenceView1.ActivityEndTimeLimitationErrorMessage = "";
            this.editExtendedPreferenceView1.ActivityEndTimeLimitationMax = System.TimeSpan.Parse("00:00:00");
            this.editExtendedPreferenceView1.ActivityEndTimeLimitationMin = System.TimeSpan.Parse("00:00:00");
            this.editExtendedPreferenceView1.ActivityStartTimeLimitationErrorMessage = "";
            this.editExtendedPreferenceView1.ActivityStartTimeLimitationMax = System.TimeSpan.Parse("00:00:00");
            this.editExtendedPreferenceView1.ActivityStartTimeLimitationMin = System.TimeSpan.Parse("00:00:00");
            this.editExtendedPreferenceView1.ActivityTimeControlsEnabled = true;
            this.editExtendedPreferenceView1.ActivityTimeLimitationErrorMessage = "";
            this.editExtendedPreferenceView1.ActivityTimeLimitationMax = null;
            this.editExtendedPreferenceView1.ActivityTimeLimitationMin = null;
            this.editExtendedPreferenceView1.ActivityViewVisible = true;
            this.editExtendedPreferenceView1.DayOff = null;
            this.editExtendedPreferenceView1.DayOffEnabled = true;
            this.editExtendedPreferenceView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.editExtendedPreferenceView1.EndTimeLimitationErrorMessage = "";
            this.editExtendedPreferenceView1.EndTimeLimitationMax = System.TimeSpan.Parse("00:00:00");
            this.editExtendedPreferenceView1.EndTimeLimitationMaxNextDay = false;
            this.editExtendedPreferenceView1.EndTimeLimitationMaxNextDayEnabled = true;
            this.editExtendedPreferenceView1.EndTimeLimitationMin = System.TimeSpan.Parse("00:00:00");
            this.editExtendedPreferenceView1.EndTimeLimitationMinNextDay = false;
            this.editExtendedPreferenceView1.EndTimeLimitationMinNextDayEnabled = true;
            this.editExtendedPreferenceView1.Location = new System.Drawing.Point(3, 327);
            this.editExtendedPreferenceView1.Name = "editExtendedPreferenceView1";
            this.editExtendedPreferenceView1.SaveButtonEnabled = false;
            this.editExtendedPreferenceView1.ShiftCategory = null;
            this.editExtendedPreferenceView1.ShiftCategoryEnabled = true;
            this.editExtendedPreferenceView1.ShiftTimeControlsEnabled = true;
            this.editExtendedPreferenceView1.Size = new System.Drawing.Size(868, 98);
            this.editExtendedPreferenceView1.StartTimeLimitationErrorMessage = "";
            this.editExtendedPreferenceView1.StartTimeLimitationMax = System.TimeSpan.Parse("00:00:00");
            this.editExtendedPreferenceView1.StartTimeLimitationMin = System.TimeSpan.Parse("00:00:00");
            this.editExtendedPreferenceView1.TabIndex = 4;
            this.editExtendedPreferenceView1.WorkTimeLimitationErrorMessage = "";
            this.editExtendedPreferenceView1.WorkTimeLimitationMax = null;
            this.editExtendedPreferenceView1.WorkTimeLimitationMin = null;
            // 
            // PreferenceView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "PreferenceView";
            this.Size = new System.Drawing.Size(874, 428);
            this.Load += new System.EventHandler(this.PreferenceView_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            this.contextMenuStripPreference.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Syncfusion.Windows.Forms.Grid.GridControl gridControl1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripPreference;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemAddShiftCategory;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemAddDayOff;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSaveAsTemplate;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemCopy;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemCut;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemPaste;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDelete;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelPeriodInformation;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private EditExtendedPreferenceView editExtendedPreferenceView1;
        private System.Windows.Forms.Label labelInfo;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelDayOffs;
        private System.Windows.Forms.Label labelPeriodCalculationInfo;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemAddAbsence;
    }
}
