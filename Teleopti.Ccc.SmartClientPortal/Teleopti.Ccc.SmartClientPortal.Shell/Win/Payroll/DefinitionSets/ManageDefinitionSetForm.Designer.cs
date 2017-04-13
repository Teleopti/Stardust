namespace Teleopti.Ccc.Win.Payroll.DefinitionSets
{
    partial class ManageDefinitionSetForm
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
			this.txtName = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
			this.cmbType = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.btnOk = new Syncfusion.Windows.Forms.ButtonAdv();
			this.btnCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.txtName)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.cmbType)).BeginInit();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// lblName
			// 
			this.lblName.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.lblName.AutoSize = true;
			this.lblName.Location = new System.Drawing.Point(6, 30);
			this.lblName.Margin = new System.Windows.Forms.Padding(6);
			this.lblName.Name = "lblName";
			this.lblName.Size = new System.Drawing.Size(101, 15);
			this.lblName.TabIndex = 0;
			this.lblName.Text = "xxDefinitionName";
			// 
			// lblType
			// 
			this.lblType.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.lblType.AutoSize = true;
			this.lblType.Location = new System.Drawing.Point(6, 64);
			this.lblType.Margin = new System.Windows.Forms.Padding(6);
			this.lblType.Name = "lblType";
			this.lblType.Size = new System.Drawing.Size(95, 15);
			this.lblType.TabIndex = 1;
			this.lblType.Text = "xxDefinitionType";
			// 
			// txtName
			// 
			this.txtName.BeforeTouchSize = new System.Drawing.Size(164, 23);
			this.tableLayoutPanel1.SetColumnSpan(this.txtName, 2);
			this.txtName.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.txtName.Location = new System.Drawing.Point(132, 26);
			this.txtName.Margin = new System.Windows.Forms.Padding(6);
			this.txtName.MaxLength = 250;
			this.txtName.Metrocolor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.txtName.Name = "txtName";
			this.txtName.OverflowIndicatorToolTipText = null;
			this.txtName.Size = new System.Drawing.Size(223, 23);
			this.txtName.Style = Syncfusion.Windows.Forms.Tools.TextBoxExt.theme.Default;
			this.txtName.TabIndex = 2;
			// 
			// cmbType
			// 
			this.cmbType.BackColor = System.Drawing.Color.White;
			this.cmbType.BeforeTouchSize = new System.Drawing.Size(224, 23);
			this.tableLayoutPanel1.SetColumnSpan(this.cmbType, 2);
			this.cmbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbType.Location = new System.Drawing.Point(132, 61);
			this.cmbType.Margin = new System.Windows.Forms.Padding(6);
			this.cmbType.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.cmbType.Name = "cmbType";
			this.cmbType.Size = new System.Drawing.Size(224, 23);
			this.cmbType.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.cmbType.TabIndex = 3;
			// 
			// btnOk
			// 
			this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOk.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.btnOk.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.btnOk.BeforeTouchSize = new System.Drawing.Size(94, 29);
			this.btnOk.ForeColor = System.Drawing.Color.White;
			this.btnOk.IsBackStageButton = false;
			this.btnOk.Location = new System.Drawing.Point(142, 125);
			this.btnOk.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(94, 29);
			this.btnOk.TabIndex = 4;
			this.btnOk.Text = "xxAdd";
			this.btnOk.UseVisualStyle = true;
			this.btnOk.Click += new System.EventHandler(this.btnOkClick);
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.btnCancel.BeforeTouchSize = new System.Drawing.Size(93, 28);
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.ForeColor = System.Drawing.Color.White;
			this.btnCancel.IsBackStageButton = false;
			this.btnCancel.Location = new System.Drawing.Point(263, 126);
			this.btnCancel.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(93, 28);
			this.btnCancel.TabIndex = 5;
			this.btnCancel.Text = "xxCancel";
			this.btnCancel.UseVisualStyle = true;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.tableLayoutPanel1.Controls.Add(this.lblName, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.lblType, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.btnCancel, 2, 3);
			this.tableLayoutPanel1.Controls.Add(this.txtName, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.btnOk, 1, 3);
			this.tableLayoutPanel1.Controls.Add(this.cmbType, 1, 2);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 4;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(366, 164);
			this.tableLayoutPanel1.TabIndex = 8;
			// 
			// ManageDefinitionSetForm
			// 
			this.AcceptButton = this.btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.CancelButton = this.btnCancel;
			this.CaptionFont = new System.Drawing.Font("Segoe UI", 12F);
			this.ClientSize = new System.Drawing.Size(366, 164);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ManageDefinitionSetForm";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "xxManageDefinitionSet";
			((System.ComponentModel.ISupportInitialize)(this.txtName)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.cmbType)).EndInit();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label lblType;
        private Syncfusion.Windows.Forms.Tools.TextBoxExt txtName;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv cmbType;
        private Syncfusion.Windows.Forms.ButtonAdv btnOk;
		  private Syncfusion.Windows.Forms.ButtonAdv btnCancel;
		  private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;

    }
}