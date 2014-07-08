using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;

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
			this.shiftPerDateControl1 = new Teleopti.Ccc.Win.Scheduling.PropertyPanel.ShiftPerDateControl();
			this.tabPagePerAgent = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.shiftPerAgentControl1 = new Teleopti.Ccc.Win.Scheduling.PropertyPanel.ShiftPerAgentControl();
			this.tabPageDistribution = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.shiftStatisticsControl1 = new Teleopti.Ccc.Win.Scheduling.PropertyPanel.ShiftStatisticsControl();
			this.perShiftCategoryChartControl1 = new Teleopti.Ccc.Win.Scheduling.PropertyPanel.PerShiftCategoryChartControl();
			((System.ComponentModel.ISupportInitialize)(this.tabControlShiftCategoryDistribution)).BeginInit();
			this.tabControlShiftCategoryDistribution.SuspendLayout();
			this.tabPagePerDate.SuspendLayout();
			this.tabPagePerAgent.SuspendLayout();
			this.tabPageDistribution.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControlShiftCategoryDistribution
			// 
			this.tabControlShiftCategoryDistribution.BeforeTouchSize = new System.Drawing.Size(352, 530);
			this.tabControlShiftCategoryDistribution.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.tabControlShiftCategoryDistribution.Controls.Add(this.tabPagePerDate);
			this.tabControlShiftCategoryDistribution.Controls.Add(this.tabPagePerAgent);
			this.tabControlShiftCategoryDistribution.Controls.Add(this.tabPageDistribution);
			this.tabControlShiftCategoryDistribution.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControlShiftCategoryDistribution.FixedSingleBorderColor = System.Drawing.Color.White;
			this.tabControlShiftCategoryDistribution.InactiveTabColor = System.Drawing.Color.White;
			this.tabControlShiftCategoryDistribution.Location = new System.Drawing.Point(0, 0);
			this.tabControlShiftCategoryDistribution.Name = "tabControlShiftCategoryDistribution";
			this.tabControlShiftCategoryDistribution.Size = new System.Drawing.Size(352, 530);
			this.tabControlShiftCategoryDistribution.TabIndex = 0;
			this.tabControlShiftCategoryDistribution.TabPanelBackColor = System.Drawing.Color.White;
			this.tabControlShiftCategoryDistribution.TabStyle = typeof(Syncfusion.Windows.Forms.Tools.TabRendererMetro);
			// 
			// tabPagePerDate
			// 
			this.tabPagePerDate.Controls.Add(this.shiftPerDateControl1);
			this.tabPagePerDate.Image = null;
			this.tabPagePerDate.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPagePerDate.Location = new System.Drawing.Point(0, 21);
			this.tabPagePerDate.Name = "tabPagePerDate";
			this.tabPagePerDate.Padding = new System.Windows.Forms.Padding(3);
			this.tabPagePerDate.ShowCloseButton = true;
			this.tabPagePerDate.Size = new System.Drawing.Size(352, 509);
			this.tabPagePerDate.TabIndex = 1;
			this.tabPagePerDate.Text = "xxPerDate";
			this.tabPagePerDate.ThemesEnabled = false;
			// 
			// shiftPerDateControl1
			// 
			this.shiftPerDateControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.shiftPerDateControl1.Location = new System.Drawing.Point(3, 3);
			this.shiftPerDateControl1.Name = "shiftPerDateControl1";
			this.shiftPerDateControl1.Size = new System.Drawing.Size(346, 503);
			this.shiftPerDateControl1.TabIndex = 0;
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
			// shiftPerAgentControl1
			// 
			this.shiftPerAgentControl1.BackColor = System.Drawing.SystemColors.Window;
			this.shiftPerAgentControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.shiftPerAgentControl1.Location = new System.Drawing.Point(3, 3);
			this.shiftPerAgentControl1.Name = "shiftPerAgentControl1";
			this.shiftPerAgentControl1.Size = new System.Drawing.Size(343, 500);
			this.shiftPerAgentControl1.TabIndex = 0;
			// 
			// tabPageDistribution
			// 
			this.tabPageDistribution.Controls.Add(this.tableLayoutPanel1);
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
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.shiftStatisticsControl1, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.perShiftCategoryChartControl1, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 60F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(343, 500);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// shiftStatisticsControl1
			// 
			this.shiftStatisticsControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.shiftStatisticsControl1.Location = new System.Drawing.Point(3, 203);
			this.shiftStatisticsControl1.Name = "shiftStatisticsControl1";
			this.shiftStatisticsControl1.Size = new System.Drawing.Size(337, 294);
			this.shiftStatisticsControl1.TabIndex = 0;
			// 
			// perShiftCategoryChartControl1
			// 
			this.perShiftCategoryChartControl1.BackColor = System.Drawing.Color.White;
			this.perShiftCategoryChartControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.perShiftCategoryChartControl1.Location = new System.Drawing.Point(3, 3);
			this.perShiftCategoryChartControl1.Name = "perShiftCategoryChartControl1";
			this.perShiftCategoryChartControl1.Size = new System.Drawing.Size(337, 194);
			this.perShiftCategoryChartControl1.TabIndex = 1;
			// 
			// ShiftCategoryDistributionControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.tabControlShiftCategoryDistribution);
			this.Name = "ShiftCategoryDistributionControl";
			this.Size = new System.Drawing.Size(352, 530);
			((System.ComponentModel.ISupportInitialize)(this.tabControlShiftCategoryDistribution)).EndInit();
			this.tabControlShiftCategoryDistribution.ResumeLayout(false);
			this.tabPagePerDate.ResumeLayout(false);
			this.tabPagePerAgent.ResumeLayout(false);
			this.tabPageDistribution.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Syncfusion.Windows.Forms.Tools.TabControlAdv tabControlShiftCategoryDistribution;
		private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPagePerDate;
		private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPagePerAgent;
		private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageDistribution;
		private ShiftPerAgentControl shiftPerAgentControl1;
		private ShiftPerDateControl shiftPerDateControl1;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private ShiftStatisticsControl shiftStatisticsControl1;
		private PerShiftCategoryChartControl perShiftCategoryChartControl1;
	}
}
