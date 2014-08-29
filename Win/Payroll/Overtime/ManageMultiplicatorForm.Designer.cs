using Teleopti.Ccc.Win.Common.Controls;

namespace Teleopti.Ccc.Win.Payroll.Overtime
{
    partial class ManageMultiplicatorForm
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
			this.lblName = new System.Windows.Forms.Label();
			this.lblType = new System.Windows.Forms.Label();
			this.textBoxName = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
			this.cmbType = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.btnOk = new Syncfusion.Windows.Forms.ButtonAdv();
			this.btnCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.textBoxShortName = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.textBoxMultiplicatorValue = new Teleopti.Ccc.Win.Common.Controls.NumericTextBox();
			this.textBoxExportCode = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.colorPickerButton1 = new Syncfusion.Windows.Forms.ColorPickerButton();
			((System.ComponentModel.ISupportInitialize)(this.textBoxName)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.cmbType)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.textBoxShortName)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.textBoxExportCode)).BeginInit();
			this.SuspendLayout();
			// 
			// lblName
			// 
			this.lblName.AutoSize = true;
			this.lblName.Location = new System.Drawing.Point(21, 35);
			this.lblName.Name = "lblName";
			this.lblName.Size = new System.Drawing.Size(49, 15);
			this.lblName.TabIndex = 0;
			this.lblName.Text = "xxName";
			// 
			// lblType
			// 
			this.lblType.AutoSize = true;
			this.lblType.Location = new System.Drawing.Point(21, 97);
			this.lblType.Name = "lblType";
			this.lblType.Size = new System.Drawing.Size(111, 15);
			this.lblType.TabIndex = 1;
			this.lblType.Text = "xxMultiplicatorType";
			// 
			// textBoxName
			// 
			this.textBoxName.BeforeTouchSize = new System.Drawing.Size(164, 23);
			this.textBoxName.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.textBoxName.Location = new System.Drawing.Point(169, 33);
			this.textBoxName.MaxLength = 50;
			this.textBoxName.Metrocolor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxName.Name = "textBoxName";
			this.textBoxName.OverflowIndicatorToolTipText = null;
			this.textBoxName.Size = new System.Drawing.Size(164, 23);
			this.textBoxName.Style = Syncfusion.Windows.Forms.Tools.TextBoxExt.theme.Default;
			this.textBoxName.TabIndex = 1;
			// 
			// cmbType
			// 
			this.cmbType.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(251)))));
			this.cmbType.BeforeTouchSize = new System.Drawing.Size(164, 23);
			this.cmbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbType.Location = new System.Drawing.Point(169, 93);
			this.cmbType.Name = "cmbType";
			this.cmbType.Size = new System.Drawing.Size(164, 23);
			this.cmbType.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
			this.cmbType.TabIndex = 3;
			// 
			// btnOk
			// 
			this.btnOk.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.btnOk.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.btnOk.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOk.ForeColor = System.Drawing.Color.White;
			this.btnOk.IsBackStageButton = false;
			this.btnOk.Location = new System.Drawing.Point(132, 248);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(87, 27);
			this.btnOk.TabIndex = 7;
			this.btnOk.Text = "xxAdd";
			this.btnOk.UseVisualStyle = true;
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.btnCancel.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.ForeColor = System.Drawing.Color.White;
			this.btnCancel.IsBackStageButton = false;
			this.btnCancel.Location = new System.Drawing.Point(246, 248);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(87, 27);
			this.btnCancel.TabIndex = 8;
			this.btnCancel.Text = "xxCancel";
			this.btnCancel.UseVisualStyle = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// textBoxShortName
			// 
			this.textBoxShortName.BeforeTouchSize = new System.Drawing.Size(164, 23);
			this.textBoxShortName.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.textBoxShortName.Location = new System.Drawing.Point(169, 63);
			this.textBoxShortName.MaxLength = 2;
			this.textBoxShortName.Metrocolor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxShortName.Name = "textBoxShortName";
			this.textBoxShortName.OverflowIndicatorToolTipText = null;
			this.textBoxShortName.Size = new System.Drawing.Size(164, 23);
			this.textBoxShortName.Style = Syncfusion.Windows.Forms.Tools.TextBoxExt.theme.Default;
			this.textBoxShortName.TabIndex = 2;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(21, 65);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(77, 15);
			this.label1.TabIndex = 8;
			this.label1.Text = "xxShortName";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(21, 160);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(114, 15);
			this.label2.TabIndex = 10;
			this.label2.Text = "xxMultiplicatorValue";
			// 
			// textBoxMultiplicatorValue
			// 
			this.textBoxMultiplicatorValue.CurrentCulture = new System.Globalization.CultureInfo("sv-SE");
			this.textBoxMultiplicatorValue.DoubleValue = 0D;
			this.textBoxMultiplicatorValue.Location = new System.Drawing.Point(169, 158);
			this.textBoxMultiplicatorValue.MaxLength = 10;
			this.textBoxMultiplicatorValue.MaxValue = 1.7976931348623157E+308D;
			this.textBoxMultiplicatorValue.MinValue = -1.7976931348623157E+308D;
			this.textBoxMultiplicatorValue.Name = "textBoxMultiplicatorValue";
			this.textBoxMultiplicatorValue.Size = new System.Drawing.Size(164, 23);
			this.textBoxMultiplicatorValue.TabIndex = 5;
			this.textBoxMultiplicatorValue.Text = "0";
			// 
			// textBoxExportCode
			// 
			this.textBoxExportCode.BeforeTouchSize = new System.Drawing.Size(164, 23);
			this.textBoxExportCode.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.textBoxExportCode.Location = new System.Drawing.Point(169, 188);
			this.textBoxExportCode.MaxLength = 25;
			this.textBoxExportCode.Metrocolor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxExportCode.Name = "textBoxExportCode";
			this.textBoxExportCode.OverflowIndicatorToolTipText = null;
			this.textBoxExportCode.Size = new System.Drawing.Size(164, 23);
			this.textBoxExportCode.Style = Syncfusion.Windows.Forms.Tools.TextBoxExt.theme.Default;
			this.textBoxExportCode.TabIndex = 6;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(21, 190);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(78, 15);
			this.label3.TabIndex = 12;
			this.label3.Text = "xxExportCode";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(21, 130);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(46, 15);
			this.label4.TabIndex = 14;
			this.label4.Text = "xxColor";
			// 
			// colorPickerButton1
			// 
			this.colorPickerButton1.BackColor = System.Drawing.Color.Cyan;
			this.colorPickerButton1.BeforeTouchSize = new System.Drawing.Size(84, 27);
			this.colorPickerButton1.ColorUISize = new System.Drawing.Size(208, 230);
			this.colorPickerButton1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.colorPickerButton1.IsBackStageButton = false;
			this.colorPickerButton1.Location = new System.Drawing.Point(169, 124);
			this.colorPickerButton1.Name = "colorPickerButton1";
			this.colorPickerButton1.SelectedAsBackcolor = true;
			this.colorPickerButton1.SelectedColor = System.Drawing.Color.Cyan;
			this.colorPickerButton1.Size = new System.Drawing.Size(84, 27);
			this.colorPickerButton1.TabIndex = 4;
			this.colorPickerButton1.UseVisualStyleBackColor = false;
			// 
			// ManageMultiplicatorForm
			// 
			this.AcceptButton = this.btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.CancelButton = this.btnCancel;
			this.CaptionFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ClientSize = new System.Drawing.Size(345, 286);
			this.Controls.Add(this.textBoxShortName);
			this.Controls.Add(this.colorPickerButton1);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.textBoxExportCode);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.textBoxMultiplicatorValue);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.cmbType);
			this.Controls.Add(this.textBoxName);
			this.Controls.Add(this.lblType);
			this.Controls.Add(this.lblName);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.HelpButton = false;
			this.MaximizeBox = false;
			this.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.MinimizeBox = false;
			this.Name = "ManageMultiplicatorForm";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "xxAddMultiplicator";
			((System.ComponentModel.ISupportInitialize)(this.textBoxName)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.cmbType)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.textBoxShortName)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.textBoxExportCode)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label lblType;
        private Syncfusion.Windows.Forms.Tools.TextBoxExt textBoxName;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv cmbType;
        private Syncfusion.Windows.Forms.ButtonAdv btnOk;
		private Syncfusion.Windows.Forms.ButtonAdv btnCancel;
        private Syncfusion.Windows.Forms.Tools.TextBoxExt textBoxShortName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private NumericTextBox textBoxMultiplicatorValue;
        private Syncfusion.Windows.Forms.Tools.TextBoxExt textBoxExportCode;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private Syncfusion.Windows.Forms.ColorPickerButton colorPickerButton1;

    }
}