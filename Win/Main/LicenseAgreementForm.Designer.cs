namespace Teleopti.Ccc.Win.Main
{
	partial class LicenseAgreementForm
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
			this.buttonAdvCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvApply = new Syncfusion.Windows.Forms.ButtonAdv();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.label1 = new System.Windows.Forms.Label();
			this.textBoxAgreement = new System.Windows.Forms.TextBox();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// buttonAdvCancel
			// 
			this.buttonAdvCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvCancel.AutoSize = true;
			this.buttonAdvCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvCancel.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.buttonAdvCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonAdvCancel.ForeColor = System.Drawing.Color.White;
			this.buttonAdvCancel.IsBackStageButton = false;
			this.buttonAdvCancel.Location = new System.Drawing.Point(402, 536);
			this.buttonAdvCancel.Margin = new System.Windows.Forms.Padding(10);
			this.buttonAdvCancel.Name = "buttonAdvCancel";
			this.buttonAdvCancel.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
			this.buttonAdvCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonAdvCancel.TabIndex = 4;
			this.buttonAdvCancel.Text = "xxCancel";
			this.buttonAdvCancel.UseVisualStyle = true;
			this.buttonAdvCancel.UseVisualStyleBackColor = true;
			// 
			// buttonAdvApply
			// 
			this.buttonAdvApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvApply.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvApply.AutoSize = true;
			this.buttonAdvApply.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvApply.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.buttonAdvApply.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonAdvApply.ForeColor = System.Drawing.Color.White;
			this.buttonAdvApply.IsBackStageButton = false;
			this.buttonAdvApply.Location = new System.Drawing.Point(559, 536);
			this.buttonAdvApply.Margin = new System.Windows.Forms.Padding(10);
			this.buttonAdvApply.Name = "buttonAdvApply";
			this.buttonAdvApply.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
			this.buttonAdvApply.Size = new System.Drawing.Size(75, 23);
			this.buttonAdvApply.TabIndex = 5;
			this.buttonAdvApply.Text = "xxAccept";
			this.buttonAdvApply.UseVisualStyle = true;
			this.buttonAdvApply.UseVisualStyleBackColor = true;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70.20649F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 29.79351F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 156F));
			this.tableLayoutPanel1.Controls.Add(this.buttonAdvApply, 2, 2);
			this.tableLayoutPanel1.Controls.Add(this.buttonAdvCancel, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.textBoxAgreement, 0, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 8.139535F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 91.86047F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 43F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(644, 569);
			this.tableLayoutPanel1.TabIndex = 6;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.label1, 3);
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(10, 10);
			this.label1.Margin = new System.Windows.Forms.Padding(10);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(57, 20);
			this.label1.TabIndex = 6;
			this.label1.Text = "label1";
			// 
			// textBoxAgreement
			// 
			this.textBoxAgreement.BackColor = System.Drawing.SystemColors.Window;
			this.tableLayoutPanel1.SetColumnSpan(this.textBoxAgreement, 3);
			this.textBoxAgreement.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textBoxAgreement.Location = new System.Drawing.Point(3, 45);
			this.textBoxAgreement.Multiline = true;
			this.textBoxAgreement.Name = "textBoxAgreement";
			this.textBoxAgreement.ReadOnly = true;
			this.textBoxAgreement.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textBoxAgreement.Size = new System.Drawing.Size(638, 477);
			this.textBoxAgreement.TabIndex = 7;
			// 
			// LicenseAgreementForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(644, 569);
			this.Controls.Add(this.tableLayoutPanel1);
			this.MinimizeBox = false;
			this.Name = "LicenseAgreementForm";
			this.ShowIcon = false;
			this.Text = "xxLicenseAgreement";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvCancel;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvApply;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textBoxAgreement;
	}
}