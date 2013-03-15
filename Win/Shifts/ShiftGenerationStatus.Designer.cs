namespace Teleopti.Ccc.Win.Shifts
{
	partial class ShiftGenerationStatus
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
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.buttonAdvCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.pictureBox2 = new System.Windows.Forms.PictureBox();
			this.labelRuleSet = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.BackColor = System.Drawing.Color.Transparent;
			this.label1.Location = new System.Drawing.Point(74, 73);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(137, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Current number of shifts:";
			this.label1.UseWaitCursor = true;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.BackColor = System.Drawing.Color.Transparent;
			this.label2.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(209, 54);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(32, 37);
			this.label2.TabIndex = 2;
			this.label2.Text = "0";
			this.label2.UseWaitCursor = true;
			// 
			// label4
			// 
			this.label4.BackColor = System.Drawing.Color.Transparent;
			this.label4.Location = new System.Drawing.Point(74, 96);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(413, 84);
			this.label4.TabIndex = 3;
			this.label4.Text = "Warning! If this is taking a long time and/or too many shifts are generated  the " +
    "system will be very slow in scheduling and optimization too. Try to reduce the c" +
    "omplexity of the rule sets.";
			this.label4.UseWaitCursor = true;
			// 
			// pictureBox1
			// 
			this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
			this.pictureBox1.Image = global::Teleopti.Ccc.Win.Properties.Resources.circle_ball;
			this.pictureBox1.InitialImage = global::Teleopti.Ccc.Win.Properties.Resources.circle_ball;
			this.pictureBox1.Location = new System.Drawing.Point(27, 63);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(30, 23);
			this.pictureBox1.TabIndex = 6;
			this.pictureBox1.TabStop = false;
			this.pictureBox1.UseWaitCursor = true;
			// 
			// buttonAdvCancel
			// 
			this.buttonAdvCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.buttonAdvCancel.Location = new System.Drawing.Point(328, 189);
			this.buttonAdvCancel.Name = "buttonAdvCancel";
			this.buttonAdvCancel.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
			this.buttonAdvCancel.Size = new System.Drawing.Size(151, 29);
			this.buttonAdvCancel.TabIndex = 7;
			this.buttonAdvCancel.Text = "Cancel generation";
			this.buttonAdvCancel.UseVisualStyle = true;
			this.buttonAdvCancel.UseWaitCursor = true;
			this.buttonAdvCancel.Click += new System.EventHandler(this.buttonAdvCancel_Click);
			// 
			// pictureBox2
			// 
			this.pictureBox2.BackColor = System.Drawing.Color.Transparent;
			this.pictureBox2.Image = global::Teleopti.Ccc.Win.Properties.Resources.warning;
			this.pictureBox2.InitialImage = global::Teleopti.Ccc.Win.Properties.Resources.circle_ball;
			this.pictureBox2.Location = new System.Drawing.Point(12, 96);
			this.pictureBox2.Name = "pictureBox2";
			this.pictureBox2.Size = new System.Drawing.Size(56, 52);
			this.pictureBox2.TabIndex = 8;
			this.pictureBox2.TabStop = false;
			this.pictureBox2.UseWaitCursor = true;
			// 
			// labelRuleSet
			// 
			this.labelRuleSet.AutoSize = true;
			this.labelRuleSet.BackColor = System.Drawing.Color.Transparent;
			this.labelRuleSet.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelRuleSet.Location = new System.Drawing.Point(74, 9);
			this.labelRuleSet.Name = "labelRuleSet";
			this.labelRuleSet.Size = new System.Drawing.Size(0, 21);
			this.labelRuleSet.TabIndex = 9;
			this.labelRuleSet.UseWaitCursor = true;
			// 
			// ShiftGenerationStatus
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(491, 228);
			this.ControlBox = false;
			this.Controls.Add(this.labelRuleSet);
			this.Controls.Add(this.pictureBox2);
			this.Controls.Add(this.buttonAdvCancel);
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Name = "ShiftGenerationStatus";
			this.Opacity = 0.8D;
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "ShiftGenerationStatus";
			this.UseWaitCursor = true;
			this.Load += new System.EventHandler(this.ShiftGenerationStatus_Load);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.PictureBox pictureBox1;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvCancel;
		private System.Windows.Forms.PictureBox pictureBox2;
		private System.Windows.Forms.Label labelRuleSet;
	}
}