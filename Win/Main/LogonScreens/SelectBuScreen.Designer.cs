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
			this.btnBack = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonLogOnOK = new Syncfusion.Windows.Forms.ButtonAdv();
			this.comboBoxAdvBUList = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvBUList)).BeginInit();
			this.SuspendLayout();
			// 
			// labelChooseBu
			// 
			this.labelChooseBu.AutoSize = true;
			this.labelChooseBu.BackColor = System.Drawing.Color.Transparent;
			this.labelChooseBu.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelChooseBu.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.labelChooseBu.Location = new System.Drawing.Point(0, 0);
			this.labelChooseBu.Name = "labelChooseBu";
			this.labelChooseBu.Size = new System.Drawing.Size(177, 25);
			this.labelChooseBu.TabIndex = 36;
			this.labelChooseBu.Text = "Select business unit";
			// 
			// btnBack
			// 
			this.btnBack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnBack.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.btnBack.BackColor = System.Drawing.Color.White;
			this.btnBack.BeforeTouchSize = new System.Drawing.Size(35, 35);
			this.btnBack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnBack.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnBack.ForeColor = System.Drawing.Color.White;
			this.btnBack.Image = global::Teleopti.Ccc.Win.Properties.Resources.left_round_32;
			this.btnBack.IsBackStageButton = false;
			this.btnBack.Location = new System.Drawing.Point(209, 107);
			this.btnBack.Name = "btnBack";
			this.btnBack.Size = new System.Drawing.Size(35, 35);
			this.btnBack.TabIndex = 42;
			this.btnBack.UseVisualStyleBackColor = false;
			this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
			// 
			// buttonLogOnOK
			// 
			this.buttonLogOnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonLogOnOK.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonLogOnOK.BackColor = System.Drawing.Color.White;
			this.buttonLogOnOK.BeforeTouchSize = new System.Drawing.Size(35, 35);
			this.buttonLogOnOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.buttonLogOnOK.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonLogOnOK.ForeColor = System.Drawing.Color.White;
			this.buttonLogOnOK.Image = global::Teleopti.Ccc.Win.Properties.Resources.right_round_32;
			this.buttonLogOnOK.IsBackStageButton = false;
			this.buttonLogOnOK.Location = new System.Drawing.Point(250, 107);
			this.buttonLogOnOK.Name = "buttonLogOnOK";
			this.buttonLogOnOK.Size = new System.Drawing.Size(35, 35);
			this.buttonLogOnOK.TabIndex = 40;
			this.buttonLogOnOK.UseVisualStyleBackColor = false;
			this.buttonLogOnOK.Click += new System.EventHandler(this.buttonLogOnOK_Click);
			// 
			// comboBoxAdvBUList
			// 
			this.comboBoxAdvBUList.BackColor = System.Drawing.Color.White;
			this.comboBoxAdvBUList.BeforeTouchSize = new System.Drawing.Size(223, 23);
			this.comboBoxAdvBUList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAdvBUList.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.comboBoxAdvBUList.Location = new System.Drawing.Point(5, 28);
			this.comboBoxAdvBUList.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.comboBoxAdvBUList.Name = "comboBoxAdvBUList";
			this.comboBoxAdvBUList.Size = new System.Drawing.Size(223, 23);
			this.comboBoxAdvBUList.TabIndex = 44;
			// 
			// SelectBuScreen
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.comboBoxAdvBUList);
			this.Controls.Add(this.btnBack);
			this.Controls.Add(this.buttonLogOnOK);
			this.Controls.Add(this.labelChooseBu);
			this.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "SelectBuScreen";
			this.Size = new System.Drawing.Size(285, 142);
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvBUList)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label labelChooseBu;
		private Syncfusion.Windows.Forms.ButtonAdv btnBack;
		private Syncfusion.Windows.Forms.ButtonAdv buttonLogOnOK;
		private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvBUList;
	}
}
