
namespace Teleopti.Ccc.Win.Forecasting.Forms.SkillPages
{
	partial class SkillThresholds
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SkillThresholds));
			this.labelSeriousUnderstaffing = new System.Windows.Forms.Label();
			this.tableLayoutPanelForRtl = new System.Windows.Forms.TableLayoutPanel();
			this.percentTextBoxOverstaffing = new Teleopti.Ccc.Win.Common.Controls.TeleoptiPercentTextBox();
			this.labelOverstaffing = new System.Windows.Forms.Label();
			this.labelUnderstaffing = new System.Windows.Forms.Label();
			this.percentTextBoxSeriousUnderstaffing = new Teleopti.Ccc.Win.Common.Controls.TeleoptiPercentTextBox();
			this.percentTextBoxUnderstaffing = new Teleopti.Ccc.Win.Common.Controls.TeleoptiPercentTextBox();
			this.percentTextBoxUnderstaffingFor = new Teleopti.Ccc.Win.Common.Controls.TeleoptiPercentTextBox();
			this.labelFor = new System.Windows.Forms.Label();
			this.tableLayoutPanelForRtl.SuspendLayout();
			this.SuspendLayout();
			// 
			// labelSeriousUnderstaffing
			// 
			resources.ApplyResources(this.labelSeriousUnderstaffing, "labelSeriousUnderstaffing");
			this.labelSeriousUnderstaffing.Name = "labelSeriousUnderstaffing";
			// 
			// tableLayoutPanelForRtl
			// 
			resources.ApplyResources(this.tableLayoutPanelForRtl, "tableLayoutPanelForRtl");
			this.tableLayoutPanelForRtl.Controls.Add(this.percentTextBoxOverstaffing, 1, 2);
			this.tableLayoutPanelForRtl.Controls.Add(this.labelOverstaffing, 0, 2);
			this.tableLayoutPanelForRtl.Controls.Add(this.labelUnderstaffing, 0, 1);
			this.tableLayoutPanelForRtl.Controls.Add(this.labelSeriousUnderstaffing, 0, 0);
			this.tableLayoutPanelForRtl.Controls.Add(this.percentTextBoxSeriousUnderstaffing, 1, 0);
			this.tableLayoutPanelForRtl.Controls.Add(this.percentTextBoxUnderstaffing, 1, 1);
			this.tableLayoutPanelForRtl.Controls.Add(this.percentTextBoxUnderstaffingFor, 3, 1);
			this.tableLayoutPanelForRtl.Controls.Add(this.labelFor, 2, 1);
			this.tableLayoutPanelForRtl.Name = "tableLayoutPanelForRtl";
			// 
			// percentTextBoxOverstaffing
			// 
			this.percentTextBoxOverstaffing.AllowNegativePercentage = true;
			this.percentTextBoxOverstaffing.DefaultValue = 0D;
			this.percentTextBoxOverstaffing.DoubleValue = 0D;
			this.percentTextBoxOverstaffing.ForeColor = System.Drawing.Color.Black;
			resources.ApplyResources(this.percentTextBoxOverstaffing, "percentTextBoxOverstaffing");
			this.percentTextBoxOverstaffing.Maximum = 100D;
			this.percentTextBoxOverstaffing.Minimum = -100D;
			this.percentTextBoxOverstaffing.Name = "percentTextBoxOverstaffing";
			// 
			// labelOverstaffing
			// 
			resources.ApplyResources(this.labelOverstaffing, "labelOverstaffing");
			this.labelOverstaffing.Name = "labelOverstaffing";
			// 
			// labelUnderstaffing
			// 
			resources.ApplyResources(this.labelUnderstaffing, "labelUnderstaffing");
			this.labelUnderstaffing.Name = "labelUnderstaffing";
			// 
			// percentTextBoxSeriousUnderstaffing
			// 
			this.percentTextBoxSeriousUnderstaffing.AllowNegativePercentage = true;
			this.percentTextBoxSeriousUnderstaffing.DefaultValue = 0D;
			this.percentTextBoxSeriousUnderstaffing.DoubleValue = 0D;
			this.percentTextBoxSeriousUnderstaffing.ForeColor = System.Drawing.Color.Black;
			resources.ApplyResources(this.percentTextBoxSeriousUnderstaffing, "percentTextBoxSeriousUnderstaffing");
			this.percentTextBoxSeriousUnderstaffing.Maximum = 100D;
			this.percentTextBoxSeriousUnderstaffing.Minimum = -100D;
			this.percentTextBoxSeriousUnderstaffing.Name = "percentTextBoxSeriousUnderstaffing";
			// 
			// percentTextBoxUnderstaffing
			// 
			this.percentTextBoxUnderstaffing.AllowNegativePercentage = true;
			this.percentTextBoxUnderstaffing.DefaultValue = 0D;
			this.percentTextBoxUnderstaffing.DoubleValue = 0D;
			this.percentTextBoxUnderstaffing.ForeColor = System.Drawing.Color.Black;
			resources.ApplyResources(this.percentTextBoxUnderstaffing, "percentTextBoxUnderstaffing");
			this.percentTextBoxUnderstaffing.Maximum = 100D;
			this.percentTextBoxUnderstaffing.Minimum = -100D;
			this.percentTextBoxUnderstaffing.Name = "percentTextBoxUnderstaffing";
			// 
			// percentTextBoxUnderstaffingFor
			// 
			this.percentTextBoxUnderstaffingFor.AllowNegativePercentage = true;
			this.percentTextBoxUnderstaffingFor.DefaultValue = 0D;
			this.percentTextBoxUnderstaffingFor.DoubleValue = 1D;
			this.percentTextBoxUnderstaffingFor.ForeColor = System.Drawing.Color.Black;
			resources.ApplyResources(this.percentTextBoxUnderstaffingFor, "percentTextBoxUnderstaffingFor");
			this.percentTextBoxUnderstaffingFor.Maximum = 100D;
			this.percentTextBoxUnderstaffingFor.Minimum = -100D;
			this.percentTextBoxUnderstaffingFor.Name = "percentTextBoxUnderstaffingFor";
			// 
			// labelFor
			// 
			resources.ApplyResources(this.labelFor, "labelFor");
			this.labelFor.Name = "labelFor";
			// 
			// SkillThresholds
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.tableLayoutPanelForRtl);
			this.Name = "SkillThresholds";
			this.tableLayoutPanelForRtl.ResumeLayout(false);
			this.tableLayoutPanelForRtl.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label labelSeriousUnderstaffing;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelForRtl;
		private System.Windows.Forms.Label labelUnderstaffing;
		private Teleopti.Ccc.Win.Common.Controls.TeleoptiPercentTextBox percentTextBoxSeriousUnderstaffing;
		private Teleopti.Ccc.Win.Common.Controls.TeleoptiPercentTextBox percentTextBoxUnderstaffing;
		private Common.Controls.TeleoptiPercentTextBox percentTextBoxUnderstaffingFor;
		private System.Windows.Forms.Label labelFor;
		private Common.Controls.TeleoptiPercentTextBox percentTextBoxOverstaffing;
		private System.Windows.Forms.Label labelOverstaffing;
	}
}