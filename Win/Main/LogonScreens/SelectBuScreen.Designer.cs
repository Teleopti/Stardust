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
            this.labelChooseBu = new System.Windows.Forms.Label();
            this.lbxSelectBu = new System.Windows.Forms.ListBox();
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
            this.labelChooseBu.Padding = new System.Windows.Forms.Padding(0, 20, 0, 0);
            this.labelChooseBu.Size = new System.Drawing.Size(483, 245);
            this.labelChooseBu.TabIndex = 36;
            this.labelChooseBu.Text = "xxChooseBu";
            this.labelChooseBu.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lbxSelectBu
            // 
            this.lbxSelectBu.DisplayMember = "Name";
            this.lbxSelectBu.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbxSelectBu.FormattingEnabled = true;
            this.lbxSelectBu.ItemHeight = 17;
            this.lbxSelectBu.Location = new System.Drawing.Point(71, 51);
            this.lbxSelectBu.Name = "lbxSelectBu";
            this.lbxSelectBu.Size = new System.Drawing.Size(352, 140);
            this.lbxSelectBu.TabIndex = 37;
            // 
            // SelectBuScreen
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
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
	}
}
