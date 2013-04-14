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
			this.labelCurrent = new System.Windows.Forms.Label();
			this.labelCount = new System.Windows.Forms.Label();
			this.labelWarning = new System.Windows.Forms.Label();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.buttonAdvCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.pictureBox2 = new System.Windows.Forms.PictureBox();
			this.labelRuleSet = new System.Windows.Forms.Label();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// labelCurrent
			// 
			this.labelCurrent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.labelCurrent.AutoSize = true;
			this.labelCurrent.BackColor = System.Drawing.Color.Transparent;
			this.labelCurrent.Location = new System.Drawing.Point(58, 67);
			this.labelCurrent.Margin = new System.Windows.Forms.Padding(3, 0, 3, 5);
			this.labelCurrent.Name = "labelCurrent";
			this.labelCurrent.Size = new System.Drawing.Size(137, 13);
			this.labelCurrent.TabIndex = 1;
			this.labelCurrent.Text = "Current number of shifts:";
			this.labelCurrent.UseWaitCursor = true;
			// 
			// labelCount
			// 
			this.labelCount.AutoSize = true;
			this.labelCount.BackColor = System.Drawing.Color.Transparent;
			this.labelCount.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelCount.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelCount.Location = new System.Drawing.Point(201, 49);
			this.labelCount.Name = "labelCount";
			this.labelCount.Size = new System.Drawing.Size(241, 36);
			this.labelCount.TabIndex = 2;
			this.labelCount.Text = "0";
			this.labelCount.UseWaitCursor = true;
			// 
			// labelWarning
			// 
			this.labelWarning.AutoSize = true;
			this.labelWarning.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanel1.SetColumnSpan(this.labelWarning, 2);
			this.labelWarning.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelWarning.Location = new System.Drawing.Point(58, 105);
			this.labelWarning.Margin = new System.Windows.Forms.Padding(3, 20, 3, 0);
			this.labelWarning.Name = "labelWarning";
			this.labelWarning.Size = new System.Drawing.Size(384, 56);
			this.labelWarning.TabIndex = 3;
			this.labelWarning.Text = "Warning! If this is taking a long time and/or too many shifts are generated  the " +
    "system will be very slow in scheduling and optimization too. Try to reduce the c" +
    "omplexity of the rule sets.";
			this.labelWarning.UseWaitCursor = true;
			// 
			// pictureBox1
			// 
			this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
			this.pictureBox1.Image = global::Teleopti.Ccc.Win.Properties.Resources.circle_ball;
			this.pictureBox1.InitialImage = global::Teleopti.Ccc.Win.Properties.Resources.circle_ball;
			this.pictureBox1.Location = new System.Drawing.Point(15, 59);
			this.pictureBox1.Margin = new System.Windows.Forms.Padding(15, 3, 3, 3);
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
			this.buttonAdvCancel.Location = new System.Drawing.Point(309, 171);
			this.buttonAdvCancel.Margin = new System.Windows.Forms.Padding(10);
			this.buttonAdvCancel.Name = "buttonAdvCancel";
			this.buttonAdvCancel.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
			this.buttonAdvCancel.Size = new System.Drawing.Size(126, 28);
			this.buttonAdvCancel.TabIndex = 7;
			this.buttonAdvCancel.Text = "Cancel";
			this.buttonAdvCancel.UseVisualStyle = true;
			this.buttonAdvCancel.UseWaitCursor = true;
			this.buttonAdvCancel.Click += new System.EventHandler(this.buttonAdvCancelClick);
			// 
			// pictureBox2
			// 
			this.pictureBox2.BackColor = System.Drawing.Color.Transparent;
			this.pictureBox2.Image = global::Teleopti.Ccc.Win.Properties.Resources.warning;
			this.pictureBox2.InitialImage = global::Teleopti.Ccc.Win.Properties.Resources.circle_ball;
			this.pictureBox2.Location = new System.Drawing.Point(3, 100);
			this.pictureBox2.Margin = new System.Windows.Forms.Padding(3, 15, 3, 3);
			this.pictureBox2.Name = "pictureBox2";
			this.pictureBox2.Size = new System.Drawing.Size(49, 52);
			this.pictureBox2.TabIndex = 8;
			this.pictureBox2.TabStop = false;
			this.pictureBox2.UseWaitCursor = true;
			// 
			// labelRuleSet
			// 
			this.labelRuleSet.AutoSize = true;
			this.labelRuleSet.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanel1.SetColumnSpan(this.labelRuleSet, 2);
			this.labelRuleSet.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelRuleSet.Location = new System.Drawing.Point(58, 15);
			this.labelRuleSet.Margin = new System.Windows.Forms.Padding(3, 15, 3, 0);
			this.labelRuleSet.Name = "labelRuleSet";
			this.labelRuleSet.Size = new System.Drawing.Size(0, 21);
			this.labelRuleSet.TabIndex = 9;
			this.labelRuleSet.UseWaitCursor = true;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 55F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.labelRuleSet, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.buttonAdvCancel, 2, 3);
			this.tableLayoutPanel1.Controls.Add(this.pictureBox2, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.labelWarning, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.pictureBox1, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.labelCurrent, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.labelCount, 2, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 4;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 57.14286F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 42.85714F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 76F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 47F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(445, 209);
			this.tableLayoutPanel1.TabIndex = 10;
			// 
			// ShiftGenerationStatus
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(445, 209);
			this.ControlBox = false;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Name = "ShiftGenerationStatus";
			this.Opacity = 0.8D;
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "ShiftGenerationStatus";
			this.UseWaitCursor = true;
			this.Load += new System.EventHandler(this.shiftGenerationStatusLoad);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label labelCurrent;
		private System.Windows.Forms.Label labelCount;
		private System.Windows.Forms.Label labelWarning;
		private System.Windows.Forms.PictureBox pictureBox1;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvCancel;
		private System.Windows.Forms.PictureBox pictureBox2;
		private System.Windows.Forms.Label labelRuleSet;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
	}
}