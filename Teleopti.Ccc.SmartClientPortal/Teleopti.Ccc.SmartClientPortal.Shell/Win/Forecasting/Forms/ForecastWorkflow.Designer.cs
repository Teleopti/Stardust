using System;
using Teleopti.Ccc.Win.Main;

namespace Teleopti.Ccc.Win.Forecasting.Forms
{
	partial class ForecastWorkflow
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
			if (disposing)
			{
				releaseManagedResources();
				if(components != null)
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
			this.components = new System.ComponentModel.Container();
			this.tabControlAdv1 = new Syncfusion.Windows.Forms.Tools.TabControlAdv();
			this.tabValidation = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.wfValidate = new Teleopti.Ccc.Win.Forecasting.Forms.WFControls.WFValidate();
			this.tabSeason = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.wfSeasonalityTabs = new Teleopti.Ccc.Win.Forecasting.Forms.WFControls.WFSeasonalityTabs();
			this.tabTemplation = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.wfTemplateTabs = new Teleopti.Ccc.Win.Forecasting.Forms.WFControls.WFTemplateTabs();
			this.btnFinish = new Syncfusion.Windows.Forms.ButtonAdv();
			this.btnForward = new Syncfusion.Windows.Forms.ButtonAdv();
			this.btnBack = new Syncfusion.Windows.Forms.ButtonAdv();
			this.btnCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.gradientPanel1 = new Syncfusion.Windows.Forms.Tools.GradientPanel();
			((System.ComponentModel.ISupportInitialize)(this.tabControlAdv1)).BeginInit();
			this.tabControlAdv1.SuspendLayout();
			this.tabValidation.SuspendLayout();
			this.tabSeason.SuspendLayout();
			this.tabTemplation.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanel1)).BeginInit();
			this.gradientPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControlAdv1
			// 
			this.tabControlAdv1.ActiveTabColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(102)))), ((int)(((byte)(102)))));
			this.tabControlAdv1.BeforeTouchSize = new System.Drawing.Size(1095, 656);
			this.tableLayoutPanel1.SetColumnSpan(this.tabControlAdv1, 4);
			this.tabControlAdv1.Controls.Add(this.tabValidation);
			this.tabControlAdv1.Controls.Add(this.tabSeason);
			this.tabControlAdv1.Controls.Add(this.tabTemplation);
			this.tabControlAdv1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControlAdv1.InactiveTabColor = System.Drawing.Color.White;
			this.tabControlAdv1.Location = new System.Drawing.Point(0, 0);
			this.tabControlAdv1.Margin = new System.Windows.Forms.Padding(0);
			this.tabControlAdv1.Name = "tabControlAdv1";
			this.tabControlAdv1.Padding = new System.Drawing.Point(3, 3);
			this.tabControlAdv1.Size = new System.Drawing.Size(1095, 656);
			this.tabControlAdv1.TabIndex = 0;
			this.tabControlAdv1.TabPanelBackColor = System.Drawing.Color.White;
			this.tabControlAdv1.TabStyle = typeof(Syncfusion.Windows.Forms.Tools.TabRendererMetro);
			this.tabControlAdv1.ThemesEnabled = true;
			// 
			// tabValidation
			// 
			this.tabValidation.BackColor = System.Drawing.Color.White;
			this.tabValidation.Controls.Add(this.wfValidate);
			this.tabValidation.Image = null;
			this.tabValidation.ImageSize = new System.Drawing.Size(16, 16);
			this.tabValidation.Location = new System.Drawing.Point(3, 26);
			this.tabValidation.Margin = new System.Windows.Forms.Padding(0);
			this.tabValidation.Name = "tabValidation";
			this.tabValidation.ShowCloseButton = true;
			this.tabValidation.Size = new System.Drawing.Size(1088, 626);
			this.tabValidation.TabIndex = 7;
			this.tabValidation.Tag = "validation";
			this.tabValidation.Text = "xxValidation";
			this.tabValidation.ThemesEnabled = true;
			// 
			// wfValidate
			// 
			this.wfValidate.BackColor = System.Drawing.Color.White;
			this.wfValidate.Dock = System.Windows.Forms.DockStyle.Fill;
			this.wfValidate.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.wfValidate.Location = new System.Drawing.Point(0, 0);
			this.wfValidate.Margin = new System.Windows.Forms.Padding(0);
			this.wfValidate.Name = "wfValidate";
			this.wfValidate.Size = new System.Drawing.Size(1088, 626);
			this.wfValidate.TabIndex = 0;
			this.wfValidate.Load += new System.EventHandler(this.wfValidate_Load);
			// 
			// tabSeason
			// 
			this.tabSeason.BackColor = System.Drawing.Color.White;
			this.tabSeason.Controls.Add(this.wfSeasonalityTabs);
			this.tabSeason.Image = null;
			this.tabSeason.ImageSize = new System.Drawing.Size(16, 16);
			this.tabSeason.Location = new System.Drawing.Point(3, 26);
			this.tabSeason.Margin = new System.Windows.Forms.Padding(0);
			this.tabSeason.Name = "tabSeason";
			this.tabSeason.ShowCloseButton = true;
			this.tabSeason.Size = new System.Drawing.Size(1088, 626);
			this.tabSeason.TabBackColor = System.Drawing.Color.LightGray;
			this.tabSeason.TabIndex = 1;
			this.tabSeason.Tag = "volume";
			this.tabSeason.Text = "xxVolumeForecast";
			this.tabSeason.ThemesEnabled = true;
			// 
			// wfSeasonalityTabs
			// 
			this.wfSeasonalityTabs.BackColor = System.Drawing.Color.White;
			this.wfSeasonalityTabs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.wfSeasonalityTabs.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.wfSeasonalityTabs.Location = new System.Drawing.Point(0, 0);
			this.wfSeasonalityTabs.Name = "wfSeasonalityTabs";
			this.wfSeasonalityTabs.Size = new System.Drawing.Size(1088, 626);
			this.wfSeasonalityTabs.TabIndex = 0;
			// 
			// tabTemplation
			// 
			this.tabTemplation.BackColor = System.Drawing.Color.White;
			this.tabTemplation.Controls.Add(this.wfTemplateTabs);
			this.tabTemplation.Image = null;
			this.tabTemplation.ImageSize = new System.Drawing.Size(16, 16);
			this.tabTemplation.Location = new System.Drawing.Point(3, 26);
			this.tabTemplation.Margin = new System.Windows.Forms.Padding(0);
			this.tabTemplation.Name = "tabTemplation";
			this.tabTemplation.ShowCloseButton = true;
			this.tabTemplation.Size = new System.Drawing.Size(1088, 626);
			this.tabTemplation.TabBackColor = System.Drawing.Color.LightGray;
			this.tabTemplation.TabIndex = 6;
			this.tabTemplation.Tag = "templation";
			this.tabTemplation.Text = "xxWorkloadDayTemplates";
			this.tabTemplation.ThemesEnabled = true;
			// 
			// wfTemplateTabs
			// 
			this.wfTemplateTabs.BackColor = System.Drawing.Color.White;
			this.wfTemplateTabs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.wfTemplateTabs.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.wfTemplateTabs.Location = new System.Drawing.Point(0, 0);
			this.wfTemplateTabs.Margin = new System.Windows.Forms.Padding(0);
			this.wfTemplateTabs.Name = "wfTemplateTabs";
			this.wfTemplateTabs.Size = new System.Drawing.Size(1088, 626);
			this.wfTemplateTabs.TabIndex = 0;
			this.wfTemplateTabs.Tag = "templation";
			this.wfTemplateTabs.Load += new System.EventHandler(this.wfTemplateTabs_Load);
			// 
			// btnFinish
			// 
			this.btnFinish.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.btnFinish.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.btnFinish.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.btnFinish.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.btnFinish.ForeColor = System.Drawing.Color.White;
			this.btnFinish.IsBackStageButton = false;
			this.btnFinish.Location = new System.Drawing.Point(898, 662);
			this.btnFinish.Margin = new System.Windows.Forms.Padding(3, 3, 10, 3);
			this.btnFinish.Name = "btnFinish";
			this.btnFinish.Size = new System.Drawing.Size(87, 27);
			this.btnFinish.TabIndex = 3;
			this.btnFinish.Text = "xxFinishAmpersand";
			this.btnFinish.UseVisualStyle = true;
			this.btnFinish.Click += new System.EventHandler(this.btnFinishClick);
			// 
			// btnForward
			// 
			this.btnForward.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.btnForward.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.btnForward.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.btnForward.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.btnForward.ForeColor = System.Drawing.Color.White;
			this.btnForward.IsBackStageButton = false;
			this.btnForward.Location = new System.Drawing.Point(798, 662);
			this.btnForward.Margin = new System.Windows.Forms.Padding(3, 3, 10, 3);
			this.btnForward.Name = "btnForward";
			this.btnForward.Size = new System.Drawing.Size(87, 27);
			this.btnForward.TabIndex = 2;
			this.btnForward.Text = "xxNextAmpersandArrow";
			this.btnForward.UseVisualStyle = true;
			this.btnForward.Click += new System.EventHandler(this.forwardClicked);
			// 
			// btnBack
			// 
			this.btnBack.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.btnBack.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.btnBack.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.btnBack.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.btnBack.ForeColor = System.Drawing.Color.White;
			this.btnBack.IsBackStageButton = false;
			this.btnBack.Location = new System.Drawing.Point(698, 662);
			this.btnBack.Margin = new System.Windows.Forms.Padding(3, 3, 10, 3);
			this.btnBack.Name = "btnBack";
			this.btnBack.Size = new System.Drawing.Size(87, 27);
			this.btnBack.TabIndex = 1;
			this.btnBack.Text = "xxBackAmpersandArrow";
			this.btnBack.UseVisualStyle = true;
			this.btnBack.Click += new System.EventHandler(this.backClicked);
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.btnCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.btnCancel.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.ForeColor = System.Drawing.Color.White;
			this.btnCancel.IsBackStageButton = false;
			this.btnCancel.Location = new System.Drawing.Point(998, 662);
			this.btnCancel.Margin = new System.Windows.Forms.Padding(3, 3, 10, 3);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(87, 27);
			this.btnCancel.TabIndex = 0;
			this.btnCancel.Text = "xxCancel";
			this.btnCancel.UseVisualStyle = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancelClick);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanel1.ColumnCount = 4;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.tableLayoutPanel1.Controls.Add(this.btnFinish, 2, 1);
			this.tableLayoutPanel1.Controls.Add(this.btnForward, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.btnCancel, 3, 1);
			this.tableLayoutPanel1.Controls.Add(this.tabControlAdv1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.btnBack, 0, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(1095, 696);
			this.tableLayoutPanel1.TabIndex = 2;
			// 
			// gradientPanel1
			// 
			this.gradientPanel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.None, System.Drawing.Color.White, System.Drawing.Color.White);
			this.gradientPanel1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.gradientPanel1.Controls.Add(this.tableLayoutPanel1);
			this.gradientPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gradientPanel1.Location = new System.Drawing.Point(1, 0);
			this.gradientPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.gradientPanel1.Name = "gradientPanel1";
			this.gradientPanel1.Size = new System.Drawing.Size(1095, 696);
			this.gradientPanel1.TabIndex = 3;
			// 
			// ForecastWorkflow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.BorderColor = System.Drawing.Color.Blue;
			this.CancelButton = this.btnCancel;
			this.CaptionFont = new System.Drawing.Font("Segoe UI", 12F);
			this.ClientSize = new System.Drawing.Size(1097, 696);
			this.Controls.Add(this.gradientPanel1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MinimumSize = new System.Drawing.Size(229, 39);
			this.Name = "ForecastWorkflow";
			this.Padding = new System.Windows.Forms.Padding(1, 0, 1, 0);
			this.ShowIcon = false;
			this.Text = "xxPrepareForecast";
			this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			((System.ComponentModel.ISupportInitialize)(this.tabControlAdv1)).EndInit();
			this.tabControlAdv1.ResumeLayout(false);
			this.tabValidation.ResumeLayout(false);
			this.tabSeason.ResumeLayout(false);
			this.tabTemplation.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.gradientPanel1)).EndInit();
			this.gradientPanel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Syncfusion.Windows.Forms.Tools.TabControlAdv tabControlAdv1;
		private Syncfusion.Windows.Forms.Tools.TabPageAdv tabSeason;
		private Syncfusion.Windows.Forms.ButtonAdv btnFinish;
		private Syncfusion.Windows.Forms.ButtonAdv btnForward;
		private Syncfusion.Windows.Forms.ButtonAdv btnBack;
		private Syncfusion.Windows.Forms.ButtonAdv btnCancel;
		private Teleopti.Ccc.Win.Forecasting.Forms.WFControls.WFSeasonalityTabs wfSeasonalityTabs;
		private Syncfusion.Windows.Forms.Tools.TabPageAdv tabTemplation;
		private Teleopti.Ccc.Win.Forecasting.Forms.WFControls.WFTemplateTabs wfTemplateTabs;
		private Syncfusion.Windows.Forms.Tools.TabPageAdv tabValidation;
		private Teleopti.Ccc.Win.Forecasting.Forms.WFControls.WFValidate wfValidate;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanel1;

	}
}
