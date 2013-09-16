using Syncfusion.Windows.Forms.Tools;

namespace Teleopti.Ccc.Win.Scheduling.PropertyPanel
{
	partial class ShiftCategoryDistributionControl
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
			this.tabControlShiftCategoryDistribution = new Syncfusion.Windows.Forms.Tools.TabControlAdv();
			this.tabPagePerDate = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.tabPagePerAgent = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.tabPageDistribution = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.shiftPerAgentControl1 = new Teleopti.Ccc.Win.Scheduling.PropertyPanel.ShiftPerAgentControl();
			((System.ComponentModel.ISupportInitialize)(this.tabControlShiftCategoryDistribution)).BeginInit();
			this.tabControlShiftCategoryDistribution.SuspendLayout();
			this.tabPagePerAgent.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControlShiftCategoryDistribution
			// 
			this.tabControlShiftCategoryDistribution.ActiveTabFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.tabControlShiftCategoryDistribution.Controls.Add(this.tabPagePerDate);
			this.tabControlShiftCategoryDistribution.Controls.Add(this.tabPagePerAgent);
			this.tabControlShiftCategoryDistribution.Controls.Add(this.tabPageDistribution);
			this.tabControlShiftCategoryDistribution.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControlShiftCategoryDistribution.Location = new System.Drawing.Point(0, 0);
			this.tabControlShiftCategoryDistribution.Name = "tabControlShiftCategoryDistribution";
			this.tabControlShiftCategoryDistribution.Size = new System.Drawing.Size(352, 530);
			this.tabControlShiftCategoryDistribution.TabGap = 10;
			this.tabControlShiftCategoryDistribution.TabIndex = 0;
			this.tabControlShiftCategoryDistribution.TabPanelBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(199)))), ((int)(((byte)(216)))), ((int)(((byte)(237)))));
			this.tabControlShiftCategoryDistribution.TabStyle = typeof(Syncfusion.Windows.Forms.Tools.TabRendererOffice2007);
			// 
			// tabPagePerDate
			// 
			this.tabPagePerDate.Image = null;
			this.tabPagePerDate.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPagePerDate.Location = new System.Drawing.Point(1, 22);
			this.tabPagePerDate.Name = "tabPagePerDate";
			this.tabPagePerDate.Padding = new System.Windows.Forms.Padding(3);
			this.tabPagePerDate.ShowCloseButton = true;
			this.tabPagePerDate.Size = new System.Drawing.Size(349, 506);
			this.tabPagePerDate.TabIndex = 1;
			this.tabPagePerDate.Text = "xxPerDate";
			this.tabPagePerDate.ThemesEnabled = false;
			// 
			// tabPagePerAgent
			// 
			this.tabPagePerAgent.Controls.Add(this.shiftPerAgentControl1);
			this.tabPagePerAgent.Image = null;
			this.tabPagePerAgent.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPagePerAgent.Location = new System.Drawing.Point(1, 22);
			this.tabPagePerAgent.Name = "tabPagePerAgent";
			this.tabPagePerAgent.Padding = new System.Windows.Forms.Padding(3);
			this.tabPagePerAgent.ShowCloseButton = true;
			this.tabPagePerAgent.Size = new System.Drawing.Size(349, 506);
			this.tabPagePerAgent.TabIndex = 1;
			this.tabPagePerAgent.Text = "xxPerAgent";
			this.tabPagePerAgent.ThemesEnabled = false;
			// 
			// tabPageDistribution
			// 
			this.tabPageDistribution.Image = null;
			this.tabPageDistribution.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageDistribution.Location = new System.Drawing.Point(1, 22);
			this.tabPageDistribution.Name = "tabPageDistribution";
			this.tabPageDistribution.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageDistribution.ShowCloseButton = true;
			this.tabPageDistribution.Size = new System.Drawing.Size(349, 506);
			this.tabPageDistribution.TabIndex = 2;
			this.tabPageDistribution.Text = "xxDistribution";
			this.tabPageDistribution.ThemesEnabled = false;
			// 
			// shiftPerAgentControl1
			// 
			this.shiftPerAgentControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.shiftPerAgentControl1.Location = new System.Drawing.Point(3, 3);
			this.shiftPerAgentControl1.Name = "shiftPerAgentControl1";
			this.shiftPerAgentControl1.Size = new System.Drawing.Size(343, 500);
			this.shiftPerAgentControl1.TabIndex = 0;
			// 
			// ShiftCategoryDistributionControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tabControlShiftCategoryDistribution);
			this.Name = "ShiftCategoryDistributionControl";
			this.Size = new System.Drawing.Size(352, 530);
			((System.ComponentModel.ISupportInitialize)(this.tabControlShiftCategoryDistribution)).EndInit();
			this.tabControlShiftCategoryDistribution.ResumeLayout(false);
			this.tabPagePerAgent.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Syncfusion.Windows.Forms.Tools.TabControlAdv tabControlShiftCategoryDistribution;
		private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPagePerDate;
		private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPagePerAgent;
		private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageDistribution;
		private ShiftPerAgentControl shiftPerAgentControl1;
	}
}
