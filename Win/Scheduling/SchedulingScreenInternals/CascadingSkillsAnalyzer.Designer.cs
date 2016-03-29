﻿namespace Teleopti.Ccc.Win.Scheduling.SchedulingScreenInternals
{
	partial class CascadingSkillsAnalyzer
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
			this.comboBoxIntervals = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.buttonGo = new System.Windows.Forms.Button();
			this.textBoxReport = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// comboBoxIntervals
			// 
			this.comboBoxIntervals.FormattingEnabled = true;
			this.comboBoxIntervals.Location = new System.Drawing.Point(218, 6);
			this.comboBoxIntervals.Name = "comboBoxIntervals";
			this.comboBoxIntervals.Size = new System.Drawing.Size(121, 21);
			this.comboBoxIntervals.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(200, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Select start time of the interval to analyze";
			// 
			// buttonGo
			// 
			this.buttonGo.Location = new System.Drawing.Point(345, 6);
			this.buttonGo.Name = "buttonGo";
			this.buttonGo.Size = new System.Drawing.Size(39, 23);
			this.buttonGo.TabIndex = 2;
			this.buttonGo.Text = "Go";
			this.buttonGo.UseVisualStyleBackColor = true;
			this.buttonGo.Click += new System.EventHandler(this.buttonGo_Click);
			// 
			// textBoxReport
			// 
			this.textBoxReport.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxReport.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.textBoxReport.Location = new System.Drawing.Point(12, 33);
			this.textBoxReport.Multiline = true;
			this.textBoxReport.Name = "textBoxReport";
			this.textBoxReport.ReadOnly = true;
			this.textBoxReport.Size = new System.Drawing.Size(824, 439);
			this.textBoxReport.TabIndex = 3;
			// 
			// CascadingSkillsAnalyzer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(848, 484);
			this.Controls.Add(this.textBoxReport);
			this.Controls.Add(this.buttonGo);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.comboBoxIntervals);
			this.Name = "CascadingSkillsAnalyzer";
			this.Text = "CascadingSkillsAnalyzer";
			this.Load += new System.EventHandler(this.CascadingSkillsAnalyzer_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ComboBox comboBoxIntervals;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button buttonGo;
		private System.Windows.Forms.TextBox textBoxReport;
	}
}