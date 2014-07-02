namespace Teleopti.Ccc.Win.Main.LogonScreens
{
    partial class SelectBuScreen
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
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.labelChooseBu = new System.Windows.Forms.Label();
			this.lbxSelectBu = new System.Windows.Forms.ListBox();
			this.btnBack = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonLogOnCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonLogOnOK = new Syncfusion.Windows.Forms.ButtonAdv();
			this.SuspendLayout();
			// 
			// labelChooseBu
			// 
			this.labelChooseBu.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelChooseBu.BackColor = System.Drawing.Color.Transparent;
			this.labelChooseBu.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelChooseBu.Location = new System.Drawing.Point(0, 0);
			this.labelChooseBu.Name = "labelChooseBu";
			this.labelChooseBu.Padding = new System.Windows.Forms.Padding(0, 15, 0, 0);
			this.labelChooseBu.Size = new System.Drawing.Size(483, 245);
			this.labelChooseBu.TabIndex = 36;
			this.labelChooseBu.Text = "xxChooseBu";
			this.labelChooseBu.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// lbxSelectBu
			// 
			this.lbxSelectBu.DisplayMember = "Name";
			this.lbxSelectBu.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
			this.lbxSelectBu.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lbxSelectBu.FormattingEnabled = true;
			this.lbxSelectBu.ItemHeight = 17;
			this.lbxSelectBu.Location = new System.Drawing.Point(71, 51);
			this.lbxSelectBu.Name = "lbxSelectBu";
			this.lbxSelectBu.Size = new System.Drawing.Size(352, 123);
			this.lbxSelectBu.TabIndex = 37;
			this.lbxSelectBu.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lbxSelectBu_DrawItem);
			// 
			// btnBack
			// 
			this.btnBack.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.btnBack.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.btnBack.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.btnBack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnBack.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnBack.ForeColor = System.Drawing.Color.White;
			this.btnBack.IsBackStageButton = false;
			this.btnBack.Location = new System.Drawing.Point(186, 190);
			this.btnBack.Name = "btnBack";
			this.btnBack.Size = new System.Drawing.Size(75, 23);
			this.btnBack.TabIndex = 42;
			this.btnBack.Text = "xxBack";
			this.btnBack.UseVisualStyleBackColor = false;
			this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
			// 
			// buttonLogOnCancel
			// 
			this.buttonLogOnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonLogOnCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonLogOnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonLogOnCancel.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.buttonLogOnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonLogOnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.buttonLogOnCancel.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonLogOnCancel.ForeColor = System.Drawing.Color.White;
			this.buttonLogOnCancel.IsBackStageButton = false;
			this.buttonLogOnCancel.Location = new System.Drawing.Point(348, 190);
			this.buttonLogOnCancel.Name = "buttonLogOnCancel";
			this.buttonLogOnCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonLogOnCancel.TabIndex = 41;
			this.buttonLogOnCancel.Text = "xxCancel";
			this.buttonLogOnCancel.UseVisualStyleBackColor = false;
			this.buttonLogOnCancel.Click += new System.EventHandler(this.buttonLogOnCancel_Click);
			// 
			// buttonLogOnOK
			// 
			this.buttonLogOnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonLogOnOK.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonLogOnOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonLogOnOK.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.buttonLogOnOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.buttonLogOnOK.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonLogOnOK.ForeColor = System.Drawing.Color.White;
			this.buttonLogOnOK.IsBackStageButton = false;
			this.buttonLogOnOK.Location = new System.Drawing.Point(267, 190);
			this.buttonLogOnOK.Name = "buttonLogOnOK";
			this.buttonLogOnOK.Size = new System.Drawing.Size(75, 23);
			this.buttonLogOnOK.TabIndex = 40;
			this.buttonLogOnOK.Text = "xxOK";
			this.buttonLogOnOK.UseVisualStyleBackColor = false;
			this.buttonLogOnOK.Click += new System.EventHandler(this.buttonLogOnOK_Click);
			// 
			// SelectBuScreen
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.btnBack);
			this.Controls.Add(this.buttonLogOnCancel);
			this.Controls.Add(this.buttonLogOnOK);
			this.Controls.Add(this.lbxSelectBu);
			this.Controls.Add(this.labelChooseBu);
			this.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "SelectBuScreen";
			this.Size = new System.Drawing.Size(483, 296);
			this.ResumeLayout(false);

		}

		#endregion

        private System.Windows.Forms.Label labelChooseBu;
		private System.Windows.Forms.ListBox lbxSelectBu;
		private Syncfusion.Windows.Forms.ButtonAdv btnBack;
		private Syncfusion.Windows.Forms.ButtonAdv buttonLogOnCancel;
		private Syncfusion.Windows.Forms.ButtonAdv buttonLogOnOK;
	}
}
