namespace Teleopti.Ccc.Win.Backlog
{
	partial class SkillMapperDialog
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
			this.listBox1 = new System.Windows.Forms.ListBox();
			this.listBox2 = new System.Windows.Forms.ListBox();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonRightUp = new System.Windows.Forms.Button();
			this.buttonRightDown = new System.Windows.Forms.Button();
			this.buttonAR = new System.Windows.Forms.Button();
			this.buttonRR = new System.Windows.Forms.Button();
			this.buttonRL = new System.Windows.Forms.Button();
			this.buttonAL = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// listBox1
			// 
			this.listBox1.FormattingEnabled = true;
			this.listBox1.Location = new System.Drawing.Point(13, 13);
			this.listBox1.Name = "listBox1";
			this.listBox1.Size = new System.Drawing.Size(138, 186);
			this.listBox1.TabIndex = 0;
			// 
			// listBox2
			// 
			this.listBox2.FormattingEnabled = true;
			this.listBox2.Location = new System.Drawing.Point(220, 13);
			this.listBox2.Name = "listBox2";
			this.listBox2.Size = new System.Drawing.Size(138, 186);
			this.listBox2.TabIndex = 1;
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point(315, 227);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonCancel.TabIndex = 2;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Location = new System.Drawing.Point(234, 227);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(75, 23);
			this.buttonOK.TabIndex = 3;
			this.buttonOK.Text = "OK";
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// buttonRightUp
			// 
			this.buttonRightUp.Location = new System.Drawing.Point(364, 68);
			this.buttonRightUp.Name = "buttonRightUp";
			this.buttonRightUp.Size = new System.Drawing.Size(26, 23);
			this.buttonRightUp.TabIndex = 6;
			this.buttonRightUp.Text = "U";
			this.buttonRightUp.UseVisualStyleBackColor = true;
			this.buttonRightUp.Click += new System.EventHandler(this.buttonRightUp_Click);
			// 
			// buttonRightDown
			// 
			this.buttonRightDown.Location = new System.Drawing.Point(364, 97);
			this.buttonRightDown.Name = "buttonRightDown";
			this.buttonRightDown.Size = new System.Drawing.Size(26, 23);
			this.buttonRightDown.TabIndex = 7;
			this.buttonRightDown.Text = "D";
			this.buttonRightDown.UseVisualStyleBackColor = true;
			this.buttonRightDown.Click += new System.EventHandler(this.buttonRightDown_Click);
			// 
			// buttonAR
			// 
			this.buttonAR.Location = new System.Drawing.Point(364, 126);
			this.buttonAR.Name = "buttonAR";
			this.buttonAR.Size = new System.Drawing.Size(26, 23);
			this.buttonAR.TabIndex = 8;
			this.buttonAR.Text = "A";
			this.buttonAR.UseVisualStyleBackColor = true;
			this.buttonAR.Click += new System.EventHandler(this.buttonAR_Click);
			// 
			// buttonRR
			// 
			this.buttonRR.Location = new System.Drawing.Point(364, 155);
			this.buttonRR.Name = "buttonRR";
			this.buttonRR.Size = new System.Drawing.Size(26, 23);
			this.buttonRR.TabIndex = 9;
			this.buttonRR.Text = "R";
			this.buttonRR.UseVisualStyleBackColor = true;
			this.buttonRR.Click += new System.EventHandler(this.buttonRR_Click);
			// 
			// buttonRL
			// 
			this.buttonRL.Location = new System.Drawing.Point(157, 155);
			this.buttonRL.Name = "buttonRL";
			this.buttonRL.Size = new System.Drawing.Size(26, 23);
			this.buttonRL.TabIndex = 11;
			this.buttonRL.Text = "R";
			this.buttonRL.UseVisualStyleBackColor = true;
			this.buttonRL.Click += new System.EventHandler(this.buttonRL_Click);
			// 
			// buttonAL
			// 
			this.buttonAL.Location = new System.Drawing.Point(157, 126);
			this.buttonAL.Name = "buttonAL";
			this.buttonAL.Size = new System.Drawing.Size(26, 23);
			this.buttonAL.TabIndex = 10;
			this.buttonAL.Text = "A";
			this.buttonAL.UseVisualStyleBackColor = true;
			this.buttonAL.Click += new System.EventHandler(this.buttonAL_Click);
			// 
			// SkillMapperDialog
			// 
			this.AcceptButton = this.buttonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(402, 262);
			this.Controls.Add(this.buttonRL);
			this.Controls.Add(this.buttonAL);
			this.Controls.Add(this.buttonRR);
			this.Controls.Add(this.buttonAR);
			this.Controls.Add(this.buttonRightDown);
			this.Controls.Add(this.buttonRightUp);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.listBox2);
			this.Controls.Add(this.listBox1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SkillMapperDialog";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "SkillMapperDialog";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListBox listBox1;
		private System.Windows.Forms.ListBox listBox2;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Button buttonRightUp;
		private System.Windows.Forms.Button buttonRightDown;
		private System.Windows.Forms.Button buttonAR;
		private System.Windows.Forms.Button buttonRR;
		private System.Windows.Forms.Button buttonRL;
		private System.Windows.Forms.Button buttonAL;
	}
}