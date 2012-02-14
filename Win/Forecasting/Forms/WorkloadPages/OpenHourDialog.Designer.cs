namespace Teleopti.Ccc.Win.Forecasting.Forms.WorkloadPages
{
    partial class OpenHourDialog
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
        private void InitializeComponent()
        {
            this.timePicker = new Teleopti.Ccc.Win.Common.Controls.FromToTimePicker();
            this.btnOk = new Syncfusion.Windows.Forms.ButtonAdv();
            this.groupBoxOpenHour = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanelOpenHoursRtl = new System.Windows.Forms.TableLayoutPanel();
            this.autoLabelTo = new Syncfusion.Windows.Forms.Tools.AutoLabel();
            this.autoLabelFrom = new Syncfusion.Windows.Forms.Tools.AutoLabel();
            this.chbClose = new System.Windows.Forms.CheckBox();
            this.btnCancel = new Syncfusion.Windows.Forms.ButtonAdv();
            this.ribbonControlAdv1 = new Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed();
            this.gradientPanel1 = new Syncfusion.Windows.Forms.Tools.GradientPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBoxAdv1 = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
            this.groupBoxOpenHour.SuspendLayout();
            this.tableLayoutPanelOpenHoursRtl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanel1)).BeginInit();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxAdv1)).BeginInit();
            this.SuspendLayout();
            // 
            // timePicker
            // 
            this.timePicker.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanelOpenHoursRtl.SetColumnSpan(this.timePicker, 2);
            this.timePicker.Location = new System.Drawing.Point(0, 18);
            this.timePicker.Margin = new System.Windows.Forms.Padding(0, 3, 3, 5);
            this.timePicker.Name = "timePicker";
            this.timePicker.Size = new System.Drawing.Size(155, 24);
            this.timePicker.TabIndex = 0;
            this.timePicker.WholeDayText = "xxNextDay";
            // 
            // btnOk
            // 
            this.btnOk.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(148, 135);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 2;
            this.btnOk.Text = "xxOk";
            this.btnOk.UseVisualStyle = true;
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // groupBoxOpenHour
            // 
            this.groupBoxOpenHour.Controls.Add(this.tableLayoutPanelOpenHoursRtl);
            this.groupBoxOpenHour.Location = new System.Drawing.Point(24, 39);
            this.groupBoxOpenHour.Name = "groupBoxOpenHour";
            this.groupBoxOpenHour.Size = new System.Drawing.Size(280, 90);
            this.groupBoxOpenHour.TabIndex = 2;
            this.groupBoxOpenHour.TabStop = false;
            this.groupBoxOpenHour.Text = "xxAddOpenHours";
            // 
            // tableLayoutPanelOpenHoursRtl
            // 
            this.tableLayoutPanelOpenHoursRtl.ColumnCount = 3;
            this.tableLayoutPanelOpenHoursRtl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 29F));
            this.tableLayoutPanelOpenHoursRtl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 29F));
            this.tableLayoutPanelOpenHoursRtl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 42F));
            this.tableLayoutPanelOpenHoursRtl.Controls.Add(this.autoLabelTo, 1, 0);
            this.tableLayoutPanelOpenHoursRtl.Controls.Add(this.timePicker, 0, 1);
            this.tableLayoutPanelOpenHoursRtl.Controls.Add(this.autoLabelFrom, 0, 0);
            this.tableLayoutPanelOpenHoursRtl.Controls.Add(this.chbClose, 2, 1);
            this.tableLayoutPanelOpenHoursRtl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelOpenHoursRtl.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanelOpenHoursRtl.Name = "tableLayoutPanelOpenHoursRtl";
            this.tableLayoutPanelOpenHoursRtl.RowCount = 3;
            this.tableLayoutPanelOpenHoursRtl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.tableLayoutPanelOpenHoursRtl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanelOpenHoursRtl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tableLayoutPanelOpenHoursRtl.Size = new System.Drawing.Size(274, 71);
            this.tableLayoutPanelOpenHoursRtl.TabIndex = 5;
            // 
            // autoLabelTo
            // 
            this.autoLabelTo.DX = 0;
            this.autoLabelTo.DY = 0;
            this.autoLabelTo.Location = new System.Drawing.Point(82, 0);
            this.autoLabelTo.Name = "autoLabelTo";
            this.autoLabelTo.Size = new System.Drawing.Size(57, 13);
            this.autoLabelTo.TabIndex = 2;
            this.autoLabelTo.Text = "xxToColon";
            // 
            // autoLabelFrom
            // 
            this.autoLabelFrom.DX = 0;
            this.autoLabelFrom.DY = 0;
            this.autoLabelFrom.Location = new System.Drawing.Point(3, 0);
            this.autoLabelFrom.Name = "autoLabelFrom";
            this.autoLabelFrom.Size = new System.Drawing.Size(67, 13);
            this.autoLabelFrom.TabIndex = 2;
            this.autoLabelFrom.Text = "xxFromColon";
            // 
            // chbClose
            // 
            this.chbClose.AutoSize = true;
            this.chbClose.Location = new System.Drawing.Point(158, 18);
            this.chbClose.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.chbClose.Name = "chbClose";
            this.chbClose.Padding = new System.Windows.Forms.Padding(5, 6, 0, 0);
            this.chbClose.Size = new System.Drawing.Size(73, 23);
            this.chbClose.TabIndex = 1;
            this.chbClose.Text = "xxClosed";
            this.chbClose.UseVisualStyleBackColor = true;
            this.chbClose.CheckedChanged += new System.EventHandler(this.chbClose_CheckedChanged);
            // 
            // btnCancel
            // 
            this.btnCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(229, 135);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "xxCancel";
            this.btnCancel.UseVisualStyle = true;
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // ribbonControlAdv1
            // 
            this.ribbonControlAdv1.Location = new System.Drawing.Point(1, 0);
            this.ribbonControlAdv1.MenuButtonVisible = false;
            this.ribbonControlAdv1.Name = "ribbonControlAdv1";
            // 
            // ribbonControlAdv1.OfficeMenu
            // 
            this.ribbonControlAdv1.OfficeMenu.Name = "OfficeMenu";
            this.ribbonControlAdv1.OfficeMenu.Size = new System.Drawing.Size(12, 65);
            this.ribbonControlAdv1.QuickPanelVisible = false;
            this.ribbonControlAdv1.Size = new System.Drawing.Size(325, 30);
            this.ribbonControlAdv1.SystemText.QuickAccessDialogDropDownName = "xxStartMenu";
            this.ribbonControlAdv1.TabIndex = 4;
            this.ribbonControlAdv1.Text = "yyRibbonControlAdv1";
            // 
            // gradientPanel1
            // 
            this.gradientPanel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FloralWhite, System.Drawing.Color.Gold);
            this.gradientPanel1.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
            this.gradientPanel1.BorderColor = System.Drawing.Color.Black;
            this.gradientPanel1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gradientPanel1.Location = new System.Drawing.Point(0, 0);
            this.gradientPanel1.Name = "gradientPanel1";
            this.gradientPanel1.Size = new System.Drawing.Size(224, 80);
            this.gradientPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel2.ColumnCount = 4;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 52.40174F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 47.59826F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 75F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 112F));
            this.tableLayoutPanel2.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.label3, 0, 2);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(200, 100);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Location = new System.Drawing.Point(3, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(1, 15);
            this.label2.TabIndex = 7;
            this.label2.Text = "xxFromColon";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Location = new System.Drawing.Point(3, 62);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(1, 15);
            this.label3.TabIndex = 8;
            this.label3.Text = "xxToColon";
            this.label3.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // comboBoxAdv1
            // 
            this.comboBoxAdv1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.comboBoxAdv1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(251)))));
            this.comboBoxAdv1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAdv1.IgnoreThemeBackground = true;
            this.comboBoxAdv1.Location = new System.Drawing.Point(131, 8);
            this.comboBoxAdv1.MaxDropDownItems = 16;
            this.comboBoxAdv1.Name = "comboBoxAdv1";
            this.comboBoxAdv1.Size = new System.Drawing.Size(173, 21);
            this.comboBoxAdv1.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this.comboBoxAdv1.SuppressDropDownEvent = true;
            this.comboBoxAdv1.TabIndex = 0;
            // 
            // OpenHourDialog
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(327, 171);
            this.ControlBox = false;
            this.Controls.Add(this.ribbonControlAdv1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.groupBoxOpenHour);
            this.Controls.Add(this.btnOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.Name = "OpenHourDialog";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "xxOpenHours";
            this.Load += new System.EventHandler(this.OpenHourDialog_Load);
            this.groupBoxOpenHour.ResumeLayout(false);
            this.tableLayoutPanelOpenHoursRtl.ResumeLayout(false);
            this.tableLayoutPanelOpenHoursRtl.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanel1)).EndInit();
            this.tableLayoutPanel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxAdv1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Teleopti.Ccc.Win.Common.Controls.FromToTimePicker timePicker;
        private Syncfusion.Windows.Forms.ButtonAdv btnOk;
        private System.Windows.Forms.GroupBox groupBoxOpenHour;
        private Syncfusion.Windows.Forms.ButtonAdv btnCancel;
        private System.Windows.Forms.CheckBox chbClose;
        private Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed ribbonControlAdv1;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelTo;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelFrom;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelOpenHoursRtl;
        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdv1;


    }
}
