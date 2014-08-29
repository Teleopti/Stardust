using System.Windows.Forms;

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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OpenHourDialog));
			this.gradientPanel1 = new Syncfusion.Windows.Forms.Tools.GradientPanel();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.comboBoxAdv1 = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.btnCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.groupBoxOpenHour = new System.Windows.Forms.GroupBox();
			this.tableLayoutPanelOpenHoursRtl = new System.Windows.Forms.TableLayoutPanel();
			this.autoLabelTo = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.timePicker = new Teleopti.Ccc.Win.Common.Controls.FromToTimePicker();
			this.autoLabelFrom = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.chbClose = new System.Windows.Forms.CheckBox();
			this.btnOk = new Syncfusion.Windows.Forms.ButtonAdv();
			this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
			((System.ComponentModel.ISupportInitialize)(this.gradientPanel1)).BeginInit();
			this.tableLayoutPanel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdv1)).BeginInit();
			this.tableLayoutPanel1.SuspendLayout();
			this.groupBoxOpenHour.SuspendLayout();
			this.tableLayoutPanelOpenHoursRtl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
			this.SuspendLayout();
			// 
			// gradientPanel1
			// 
			this.gradientPanel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FloralWhite, System.Drawing.Color.Gold);
			this.gradientPanel1.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
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
			this.comboBoxAdv1.BeforeTouchSize = new System.Drawing.Size(173, 21);
			this.comboBoxAdv1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAdv1.Location = new System.Drawing.Point(131, 8);
			this.comboBoxAdv1.MaxDropDownItems = 16;
			this.comboBoxAdv1.Name = "comboBoxAdv1";
			this.comboBoxAdv1.Size = new System.Drawing.Size(173, 21);
			this.comboBoxAdv1.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
			this.comboBoxAdv1.SuppressDropDownEvent = true;
			this.comboBoxAdv1.TabIndex = 0;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.tableLayoutPanel1.Controls.Add(this.btnCancel, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.groupBoxOpenHour, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.btnOk, 0, 2);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(348, 213);
			this.tableLayoutPanel1.TabIndex = 4;
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.btnCancel.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.ForeColor = System.Drawing.Color.White;
			this.btnCancel.IsBackStageButton = false;
			this.btnCancel.Location = new System.Drawing.Point(251, 176);
			this.btnCancel.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(87, 27);
			this.btnCancel.TabIndex = 3;
			this.btnCancel.Text = "xxCancel";
			this.btnCancel.UseVisualStyle = true;
			this.btnCancel.UseVisualStyleBackColor = true;
			// 
			// groupBoxOpenHour
			// 
			this.tableLayoutPanel1.SetColumnSpan(this.groupBoxOpenHour, 2);
			this.groupBoxOpenHour.Controls.Add(this.tableLayoutPanelOpenHoursRtl);
			this.groupBoxOpenHour.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBoxOpenHour.Location = new System.Drawing.Point(3, 53);
			this.groupBoxOpenHour.Name = "groupBoxOpenHour";
			this.groupBoxOpenHour.Size = new System.Drawing.Size(342, 107);
			this.groupBoxOpenHour.TabIndex = 2;
			this.groupBoxOpenHour.TabStop = false;
			this.groupBoxOpenHour.Text = "xxAddOpenHours";
			this.groupBoxOpenHour.Enter += new System.EventHandler(this.groupBoxOpenHour_Enter);
			// 
			// tableLayoutPanelOpenHoursRtl
			// 
			this.tableLayoutPanelOpenHoursRtl.ColumnCount = 3;
			this.tableLayoutPanelOpenHoursRtl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 29.46428F));
			this.tableLayoutPanelOpenHoursRtl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32.73809F));
			this.tableLayoutPanelOpenHoursRtl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 37.59124F));
			this.tableLayoutPanelOpenHoursRtl.Controls.Add(this.autoLabelTo, 1, 0);
			this.tableLayoutPanelOpenHoursRtl.Controls.Add(this.timePicker, 0, 1);
			this.tableLayoutPanelOpenHoursRtl.Controls.Add(this.autoLabelFrom, 0, 0);
			this.tableLayoutPanelOpenHoursRtl.Controls.Add(this.chbClose, 2, 1);
			this.tableLayoutPanelOpenHoursRtl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelOpenHoursRtl.Location = new System.Drawing.Point(3, 19);
			this.tableLayoutPanelOpenHoursRtl.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanelOpenHoursRtl.Name = "tableLayoutPanelOpenHoursRtl";
			this.tableLayoutPanelOpenHoursRtl.RowCount = 3;
			this.tableLayoutPanelOpenHoursRtl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 17F));
			this.tableLayoutPanelOpenHoursRtl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
			this.tableLayoutPanelOpenHoursRtl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
			this.tableLayoutPanelOpenHoursRtl.Size = new System.Drawing.Size(336, 85);
			this.tableLayoutPanelOpenHoursRtl.TabIndex = 5;
			// 
			// autoLabelTo
			// 
			this.autoLabelTo.Location = new System.Drawing.Point(102, 0);
			this.autoLabelTo.Name = "autoLabelTo";
			this.autoLabelTo.Size = new System.Drawing.Size(63, 15);
			this.autoLabelTo.TabIndex = 2;
			this.autoLabelTo.Text = "xxToColon";
			// 
			// timePicker
			// 
			this.timePicker.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.tableLayoutPanelOpenHoursRtl.SetColumnSpan(this.timePicker, 2);
			this.timePicker.Location = new System.Drawing.Point(0, 20);
			this.timePicker.Margin = new System.Windows.Forms.Padding(0, 3, 3, 6);
			this.timePicker.MinMaxEndTime = ((Teleopti.Interfaces.Domain.MinMax<System.TimeSpan>)(resources.GetObject("timePicker.MinMaxEndTime")));
			this.timePicker.MinMaxStartTime = ((Teleopti.Interfaces.Domain.MinMax<System.TimeSpan>)(resources.GetObject("timePicker.MinMaxStartTime")));
			this.timePicker.Name = "timePicker";
			this.timePicker.Size = new System.Drawing.Size(198, 28);
			this.timePicker.TabIndex = 0;
			this.timePicker.WholeDayText = "xxNextDay";
			// 
			// autoLabelFrom
			// 
			this.autoLabelFrom.Location = new System.Drawing.Point(3, 0);
			this.autoLabelFrom.Name = "autoLabelFrom";
			this.autoLabelFrom.Size = new System.Drawing.Size(77, 15);
			this.autoLabelFrom.TabIndex = 2;
			this.autoLabelFrom.Text = "xxFromColon";
			// 
			// chbClose
			// 
			this.chbClose.AutoSize = true;
			this.chbClose.Location = new System.Drawing.Point(209, 20);
			this.chbClose.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
			this.chbClose.Name = "chbClose";
			this.chbClose.Padding = new System.Windows.Forms.Padding(6, 7, 0, 0);
			this.chbClose.Size = new System.Drawing.Size(78, 26);
			this.chbClose.TabIndex = 1;
			this.chbClose.Text = "xxClosed";
			this.chbClose.UseVisualStyleBackColor = true;
			this.chbClose.CheckedChanged += new System.EventHandler(this.chbClose_CheckedChanged);
			// 
			// btnOk
			// 
			this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOk.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.btnOk.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.btnOk.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.btnOk.ForeColor = System.Drawing.Color.White;
			this.btnOk.IsBackStageButton = false;
			this.btnOk.Location = new System.Drawing.Point(131, 176);
			this.btnOk.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(87, 27);
			this.btnOk.TabIndex = 2;
			this.btnOk.Text = "xxOk";
			this.btnOk.UseVisualStyle = true;
			this.btnOk.UseVisualStyleBackColor = true;
			this.btnOk.Click += new System.EventHandler(this.btnAdd_Click);
			// 
			// errorProvider1
			// 
			this.errorProvider1.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
			this.errorProvider1.ContainerControl = this;
			// 
			// OpenHourDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.CaptionFont = new System.Drawing.Font("Segoe UI", 12F);
			this.ClientSize = new System.Drawing.Size(348, 213);
			this.ControlBox = false;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MaximizeBox = false;
			this.Name = "OpenHourDialog";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "xxOpenHours";
			this.Load += new System.EventHandler(this.OpenHourDialog_Load);
			((System.ComponentModel.ISupportInitialize)(this.gradientPanel1)).EndInit();
			this.tableLayoutPanel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdv1)).EndInit();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.groupBoxOpenHour.ResumeLayout(false);
			this.tableLayoutPanelOpenHoursRtl.ResumeLayout(false);
			this.tableLayoutPanelOpenHoursRtl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

        private Teleopti.Ccc.Win.Common.Controls.FromToTimePicker timePicker;
        private Syncfusion.Windows.Forms.ButtonAdv btnOk;
        private System.Windows.Forms.GroupBox groupBoxOpenHour;
        private Syncfusion.Windows.Forms.ButtonAdv btnCancel;
		private System.Windows.Forms.CheckBox chbClose;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelTo;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelFrom;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelOpenHoursRtl;
        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdv1;
        private System.Windows.Forms.ErrorProvider errorProvider1;
		private TableLayoutPanel tableLayoutPanel1;


    }
}
