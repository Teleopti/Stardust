﻿
using Syncfusion.Windows.Forms.Tools;

namespace Teleopti.Ccc.Win.Forecasting.Forms.SkillPages
{
	partial class SkillEmailDistributions
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param pageName="disposing">true if managed resources should be disposed; otherwise, false.</param>
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
            this.panelMain = new System.Windows.Forms.Panel();
            this.tableLayoutPanelMainRtl = new System.Windows.Forms.TableLayoutPanel();
            this.labelShrinkage = new System.Windows.Forms.Label();
            this.labelHandledWithin = new System.Windows.Forms.Label();
            this.timeSpanTextBoxServiceLevelTime = new Teleopti.Ccc.Win.Common.Controls.MaskedTimeSpanTextBox();
            this.percentTextBox1 = new Teleopti.Ccc.Win.Common.Controls.TeleoptiPercentTextBox();
            this.labelEfficiencyPercentage = new System.Windows.Forms.Label();
            this.efficiencyPercentTextBox1 = new Teleopti.Ccc.Win.Common.Controls.TeleoptiPercentTextBox();
            this.tableLayoutPanelMoreOptionsRtl = new System.Windows.Forms.TableLayoutPanel();
            this.labelMinimumAgents = new System.Windows.Forms.Label();
            this.integerTextBoxMaximumAgents = new Syncfusion.Windows.Forms.Tools.IntegerTextBox();
            this.integerTextBoxMinimumAgents = new Syncfusion.Windows.Forms.Tools.IntegerTextBox();
            this.labelMaximumAgents = new System.Windows.Forms.Label();
            this.panelMain.SuspendLayout();
            this.tableLayoutPanelMainRtl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.integerTextBoxMaximumAgents)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.integerTextBoxMinimumAgents)).BeginInit();
            this.SuspendLayout();
            // 
            // panelMain
            // 
            this.panelMain.Controls.Add(this.tableLayoutPanelMainRtl);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(10, 10);
            this.panelMain.Margin = new System.Windows.Forms.Padding(0);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(304, 501);
            this.panelMain.TabIndex = 0;
            // 
            // tableLayoutPanelMainRtl
            // 
            this.tableLayoutPanelMainRtl.ColumnCount = 2;
            this.tableLayoutPanelMainRtl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 68F));
            this.tableLayoutPanelMainRtl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32F));
            this.tableLayoutPanelMainRtl.Controls.Add(this.labelShrinkage, 0, 1);
            this.tableLayoutPanelMainRtl.Controls.Add(this.labelHandledWithin, 0, 0);
            this.tableLayoutPanelMainRtl.Controls.Add(this.timeSpanTextBoxServiceLevelTime, 1, 0);
            this.tableLayoutPanelMainRtl.Controls.Add(this.percentTextBox1, 1, 1);
            this.tableLayoutPanelMainRtl.Controls.Add(this.labelEfficiencyPercentage, 0, 2);
            this.tableLayoutPanelMainRtl.Controls.Add(this.efficiencyPercentTextBox1, 1, 2);
            this.tableLayoutPanelMainRtl.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelMainRtl.Name = "tableLayoutPanelMainRtl";
            this.tableLayoutPanelMainRtl.RowCount = 6;
            this.tableLayoutPanelMainRtl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanelMainRtl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanelMainRtl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanelMainRtl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanelMainRtl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanelMainRtl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelMainRtl.Size = new System.Drawing.Size(291, 193);
            this.tableLayoutPanelMainRtl.TabIndex = 12;
            // 
            // labelShrinkage
            // 
            this.labelShrinkage.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelShrinkage.AutoSize = true;
            this.labelShrinkage.Location = new System.Drawing.Point(15, 41);
            this.labelShrinkage.Margin = new System.Windows.Forms.Padding(15, 0, 3, 0);
            this.labelShrinkage.Name = "labelShrinkage";
            this.labelShrinkage.Size = new System.Drawing.Size(65, 13);
            this.labelShrinkage.TabIndex = 18;
            this.labelShrinkage.Text = "xxShrinkage";
            // 
            // labelHandledWithin
            // 
            this.labelHandledWithin.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelHandledWithin.AutoSize = true;
            this.labelHandledWithin.Location = new System.Drawing.Point(15, 9);
            this.labelHandledWithin.Margin = new System.Windows.Forms.Padding(15, 0, 3, 0);
            this.labelHandledWithin.Name = "labelHandledWithin";
            this.labelHandledWithin.Size = new System.Drawing.Size(113, 13);
            this.labelHandledWithin.TabIndex = 20;
            this.labelHandledWithin.Text = "xxHandledWithinHMM";
            // 
            // timeSpanTextBoxServiceLevelTime
            // 
            this.timeSpanTextBoxServiceLevelTime.AlignTextBoxText = System.Windows.Forms.HorizontalAlignment.Left;
            this.timeSpanTextBoxServiceLevelTime.DefaultInterpretAsMinutes = false;
            this.timeSpanTextBoxServiceLevelTime.Location = new System.Drawing.Point(197, 6);
            this.timeSpanTextBoxServiceLevelTime.Margin = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.timeSpanTextBoxServiceLevelTime.MaximumValue = System.TimeSpan.Parse("2.00:00:00");
            this.timeSpanTextBoxServiceLevelTime.MinimumValue = System.TimeSpan.Parse("00:00:00");
            this.timeSpanTextBoxServiceLevelTime.Name = "timeSpanTextBoxServiceLevelTime";
            this.timeSpanTextBoxServiceLevelTime.Size = new System.Drawing.Size(79, 22);
            this.timeSpanTextBoxServiceLevelTime.TabIndex = 0;
            this.timeSpanTextBoxServiceLevelTime.TimeSpanBoxHeight = 20;
            this.timeSpanTextBoxServiceLevelTime.TimeSpanBoxWidth = 59;
            this.timeSpanTextBoxServiceLevelTime.Value = System.TimeSpan.Parse("00:00:00");
            this.timeSpanTextBoxServiceLevelTime.Validated += new System.EventHandler(this.timeSpanTextBoxServiceLevelTime_TimeSpanBoxTextChanged);
            // 
            // percentTextBox1
            // 
            this.percentTextBox1.AllowNegativePercentage = false;
            this.percentTextBox1.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.percentTextBox1.DefaultValue = 0D;
            this.percentTextBox1.DoubleValue = 0D;
            this.percentTextBox1.ForeColor = System.Drawing.Color.Black;
            this.percentTextBox1.Location = new System.Drawing.Point(197, 37);
            this.percentTextBox1.Margin = new System.Windows.Forms.Padding(0, 5, 3, 3);
            this.percentTextBox1.Maximum = 99D;
            this.percentTextBox1.Minimum = 0D;
            this.percentTextBox1.Name = "percentTextBox1";
            this.percentTextBox1.Size = new System.Drawing.Size(59, 20);
            this.percentTextBox1.TabIndex = 1;
            this.percentTextBox1.Text = "0%";
            // 
            // labelEfficiencyPercentage
            // 
            this.labelEfficiencyPercentage.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelEfficiencyPercentage.AutoSize = true;
            this.labelEfficiencyPercentage.Location = new System.Drawing.Point(15, 73);
            this.labelEfficiencyPercentage.Margin = new System.Windows.Forms.Padding(15, 0, 3, 0);
            this.labelEfficiencyPercentage.Name = "labelEfficiencyPercentage";
            this.labelEfficiencyPercentage.Size = new System.Drawing.Size(63, 13);
            this.labelEfficiencyPercentage.TabIndex = 22;
            this.labelEfficiencyPercentage.Text = "xxEfficiency";
            // 
            // efficiencyPercentTextBox1
            // 
            this.efficiencyPercentTextBox1.AllowNegativePercentage = false;
            this.efficiencyPercentTextBox1.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.efficiencyPercentTextBox1.DefaultValue = 0D;
            this.efficiencyPercentTextBox1.DoubleValue = 0.01D;
            this.efficiencyPercentTextBox1.ForeColor = System.Drawing.Color.Black;
            this.efficiencyPercentTextBox1.Location = new System.Drawing.Point(197, 69);
            this.efficiencyPercentTextBox1.Margin = new System.Windows.Forms.Padding(0, 5, 3, 3);
            this.efficiencyPercentTextBox1.Maximum = 100D;
            this.efficiencyPercentTextBox1.Minimum = 0.01D;
            this.efficiencyPercentTextBox1.Name = "efficiencyPercentTextBox1";
            this.efficiencyPercentTextBox1.Size = new System.Drawing.Size(59, 20);
            this.efficiencyPercentTextBox1.TabIndex = 2;
            this.efficiencyPercentTextBox1.Text = "1 %";
            // 
            // tableLayoutPanelMoreOptionsRtl
            // 
            this.tableLayoutPanelMoreOptionsRtl.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelMoreOptionsRtl.Name = "tableLayoutPanelMoreOptionsRtl";
            this.tableLayoutPanelMoreOptionsRtl.Size = new System.Drawing.Size(200, 100);
            this.tableLayoutPanelMoreOptionsRtl.TabIndex = 0;
            // 
            // labelMinimumAgents
            // 
            this.labelMinimumAgents.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelMinimumAgents.AutoSize = true;
            this.labelMinimumAgents.Location = new System.Drawing.Point(15, 11);
            this.labelMinimumAgents.Margin = new System.Windows.Forms.Padding(15, 0, 3, 0);
            this.labelMinimumAgents.Name = "labelMinimumAgents";
            this.labelMinimumAgents.Size = new System.Drawing.Size(91, 13);
            this.labelMinimumAgents.TabIndex = 8;
            this.labelMinimumAgents.Text = "xxMinimumAgents";
            // 
            // integerTextBoxMaximumAgents
            // 
            this.integerTextBoxMaximumAgents.BackGroundColor = System.Drawing.SystemColors.Window;
            this.integerTextBoxMaximumAgents.IntegerValue = ((long)(1));
            this.integerTextBoxMaximumAgents.Location = new System.Drawing.Point(190, 40);
            this.integerTextBoxMaximumAgents.Margin = new System.Windows.Forms.Padding(0, 5, 3, 3);
            this.integerTextBoxMaximumAgents.MaxValue = ((long)(100000000));
            this.integerTextBoxMaximumAgents.MinValue = ((long)(0));
            this.integerTextBoxMaximumAgents.Name = "integerTextBoxMaximumAgents";
            this.integerTextBoxMaximumAgents.NullString = "0";
            this.integerTextBoxMaximumAgents.OnValidationFailed = Syncfusion.Windows.Forms.Tools.OnValidationFailed.KeepFocus;
            this.integerTextBoxMaximumAgents.OverflowIndicatorToolTipText = null;
            this.integerTextBoxMaximumAgents.Size = new System.Drawing.Size(59, 20);
            this.integerTextBoxMaximumAgents.TabIndex = 14;
            this.integerTextBoxMaximumAgents.Text = "1";
            // 
            // integerTextBoxMinimumAgents
            // 
            this.integerTextBoxMinimumAgents.BackGroundColor = System.Drawing.SystemColors.Window;
            this.integerTextBoxMinimumAgents.IntegerValue = ((long)(1));
            this.integerTextBoxMinimumAgents.Location = new System.Drawing.Point(190, 5);
            this.integerTextBoxMinimumAgents.Margin = new System.Windows.Forms.Padding(0, 5, 3, 3);
            this.integerTextBoxMinimumAgents.MaxValue = ((long)(100000000));
            this.integerTextBoxMinimumAgents.MinValue = ((long)(0));
            this.integerTextBoxMinimumAgents.Name = "integerTextBoxMinimumAgents";
            this.integerTextBoxMinimumAgents.NullString = "0";
            this.integerTextBoxMinimumAgents.OnValidationFailed = Syncfusion.Windows.Forms.Tools.OnValidationFailed.KeepFocus;
            this.integerTextBoxMinimumAgents.OverflowIndicatorToolTipText = null;
            this.integerTextBoxMinimumAgents.Size = new System.Drawing.Size(59, 20);
            this.integerTextBoxMinimumAgents.TabIndex = 13;
            this.integerTextBoxMinimumAgents.Text = "1";
            // 
            // labelMaximumAgents
            // 
            this.labelMaximumAgents.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelMaximumAgents.AutoSize = true;
            this.labelMaximumAgents.Location = new System.Drawing.Point(15, 46);
            this.labelMaximumAgents.Margin = new System.Windows.Forms.Padding(15, 0, 3, 0);
            this.labelMaximumAgents.Name = "labelMaximumAgents";
            this.labelMaximumAgents.Size = new System.Drawing.Size(94, 13);
            this.labelMaximumAgents.TabIndex = 10;
            this.labelMaximumAgents.Text = "xxMaximumAgents";
            // 
            // SkillEmailDistributions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panelMain);
            this.Name = "SkillEmailDistributions";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.Size = new System.Drawing.Size(324, 521);
            this.panelMain.ResumeLayout(false);
            this.tableLayoutPanelMainRtl.ResumeLayout(false);
            this.tableLayoutPanelMainRtl.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.integerTextBoxMaximumAgents)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.integerTextBoxMinimumAgents)).EndInit();
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panelMain;
		private System.Windows.Forms.Label labelMaximumAgents;
		private System.Windows.Forms.Label labelMinimumAgents;
		private Syncfusion.Windows.Forms.Tools.IntegerTextBox integerTextBoxMaximumAgents;
		private Syncfusion.Windows.Forms.Tools.IntegerTextBox integerTextBoxMinimumAgents;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelMainRtl;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelMoreOptionsRtl;
		private System.Windows.Forms.Label labelShrinkage;
		private System.Windows.Forms.Label labelHandledWithin;
		private Teleopti.Ccc.Win.Common.Controls.MaskedTimeSpanTextBox timeSpanTextBoxServiceLevelTime;
		private Teleopti.Ccc.Win.Common.Controls.TeleoptiPercentTextBox percentTextBox1;
		private System.Windows.Forms.Label labelEfficiencyPercentage;
		private Teleopti.Ccc.Win.Common.Controls.TeleoptiPercentTextBox efficiencyPercentTextBox1;
	}
}
