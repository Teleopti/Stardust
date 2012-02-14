namespace Teleopti.Ccc.Win.Payroll
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
            this.lblName = new System.Windows.Forms.Label();
            this.lblType = new System.Windows.Forms.Label();
            this.txtName = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
            this.cmbType = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
            this.btnOk = new Syncfusion.Windows.Forms.ButtonAdv();
            this.btnCancel = new Syncfusion.Windows.Forms.ButtonAdv();
            this.ribbonControlForm = new Syncfusion.Windows.Forms.Tools.RibbonControlAdv();
            this.textBoxExt1 = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.cmbType)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlForm)).BeginInit();
            this.SuspendLayout();
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(12, 61);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(89, 13);
            this.lblName.TabIndex = 0;
            this.lblName.Text = "xxDefinitionName";
            // 
            // lblType
            // 
            this.lblType.AutoSize = true;
            this.lblType.Location = new System.Drawing.Point(12, 148);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(85, 13);
            this.lblType.TabIndex = 1;
            this.lblType.Text = "xxDefinitionType";
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(119, 61);
            this.txtName.Name = "txtName";
            this.txtName.OverflowIndicatorToolTipText = null;
            this.txtName.Size = new System.Drawing.Size(192, 20);
            this.txtName.TabIndex = 2;
            // 
            // cmbType
            // 
            this.cmbType.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(251)))));
            this.cmbType.IgnoreThemeBackground = true;
            this.cmbType.Location = new System.Drawing.Point(119, 148);
            this.cmbType.Name = "cmbType";
            this.cmbType.Size = new System.Drawing.Size(192, 21);
            this.cmbType.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this.cmbType.TabIndex = 3;
            // 
            // btnOk
            // 
            this.btnOk.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.WindowsXP;
            this.btnOk.Location = new System.Drawing.Point(119, 326);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(81, 25);
            this.btnOk.TabIndex = 4;
            this.btnOk.Text = "xxAdd";
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.WindowsXP;
            this.btnCancel.Location = new System.Drawing.Point(231, 326);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(80, 24);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "xxCancel";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // ribbonControlForm
            // 
            this.ribbonControlForm.CaptionFont = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ribbonControlForm.Location = new System.Drawing.Point(1, 0);
            this.ribbonControlForm.MenuButtonVisible = false;
            this.ribbonControlForm.Name = "ribbonControlForm";
            // 
            // ribbonControlForm.OfficeMenu
            // 
            this.ribbonControlForm.OfficeMenu.Name = "OfficeMenu";
            this.ribbonControlForm.OfficeMenu.Size = new System.Drawing.Size(12, 65);
            this.ribbonControlForm.QuickPanelVisible = false;
            this.ribbonControlForm.Size = new System.Drawing.Size(383, 33);
            this.ribbonControlForm.SystemText.QuickAccessDialogDropDownName = "xxStart menu";
            this.ribbonControlForm.TabIndex = 7;
            this.ribbonControlForm.Text = "ribbonControlAdv1";
            // 
            // textBoxExt1
            // 
            this.textBoxExt1.Location = new System.Drawing.Point(119, 87);
            this.textBoxExt1.Name = "textBoxExt1";
            this.textBoxExt1.OverflowIndicatorToolTipText = null;
            this.textBoxExt1.Size = new System.Drawing.Size(192, 20);
            this.textBoxExt1.TabIndex = 9;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 87);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "xxDefinitionName";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 116);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "xxDefinitionName";
            // 
            // ManageMultiplicatorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(385, 385);
            this.ColorScheme = Syncfusion.Windows.Forms.Tools.RibbonForm.ColorSchemeType.Blue;
            this.ControlBox = false;
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxExt1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ribbonControlForm);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.cmbType);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.lblType);
            this.Controls.Add(this.lblName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "ManageMultiplicatorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "xxManageDefinitionSet";
            ((System.ComponentModel.ISupportInitialize)(this.cmbType)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlForm)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label lblType;
        private Syncfusion.Windows.Forms.Tools.TextBoxExt txtName;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv cmbType;
        private Syncfusion.Windows.Forms.ButtonAdv btnOk;
        private Syncfusion.Windows.Forms.ButtonAdv btnCancel;
        private Syncfusion.Windows.Forms.Tools.RibbonControlAdv ribbonControlForm;
        private Syncfusion.Windows.Forms.Tools.TextBoxExt textBoxExt1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;

    }
}