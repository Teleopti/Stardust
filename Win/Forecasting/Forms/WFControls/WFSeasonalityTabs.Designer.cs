

using Teleopti.Ccc.Win.Forecasting.Forms.SeasonPages;

namespace Teleopti.Ccc.Win.Forecasting.Forms.WFControls
{
	partial class WFSeasonalityTabs
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
				unhookEvents();
				releaseManagedResources();
				if (components != null)
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
			this.tabPageDayOfWeek = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.tabPageWeekOfMonth = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.tabPageMonthOfYear = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.tabPageTotal = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.tabSeasonality = new Syncfusion.Windows.Forms.Tools.TabControlAdv();
			this.tabPageDataSummary = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.tabPageTrend = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			((System.ComponentModel.ISupportInitialize)(this.tabSeasonality)).BeginInit();
			this.tabSeasonality.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabPageDayOfWeek
			// 
			this.tabPageDayOfWeek.Image = null;
			this.tabPageDayOfWeek.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageDayOfWeek.Location = new System.Drawing.Point(2, 25);
			this.tabPageDayOfWeek.Name = "tabPageDayOfWeek";
			this.tabPageDayOfWeek.ShowCloseButton = true;
			this.tabPageDayOfWeek.Size = new System.Drawing.Size(1262, 669);
			this.tabPageDayOfWeek.TabIndex = 2;
			this.tabPageDayOfWeek.Text = "xxDayOfWeek";
			this.tabPageDayOfWeek.ThemesEnabled = true;
			// 
			// tabPageWeekOfMonth
			// 
			this.tabPageWeekOfMonth.Image = null;
			this.tabPageWeekOfMonth.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageWeekOfMonth.Location = new System.Drawing.Point(2, 25);
			this.tabPageWeekOfMonth.Name = "tabPageWeekOfMonth";
			this.tabPageWeekOfMonth.ShowCloseButton = true;
			this.tabPageWeekOfMonth.Size = new System.Drawing.Size(1262, 669);
			this.tabPageWeekOfMonth.TabIndex = 1;
			this.tabPageWeekOfMonth.Text = "xxWeekOfMonth";
			this.tabPageWeekOfMonth.ThemesEnabled = true;
			// 
			// tabPageMonthOfYear
			// 
			this.tabPageMonthOfYear.Image = null;
			this.tabPageMonthOfYear.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageMonthOfYear.Location = new System.Drawing.Point(2, 25);
			this.tabPageMonthOfYear.Name = "tabPageMonthOfYear";
			this.tabPageMonthOfYear.ShowCloseButton = true;
			this.tabPageMonthOfYear.Size = new System.Drawing.Size(1262, 669);
			this.tabPageMonthOfYear.TabIndex = 1;
			this.tabPageMonthOfYear.Text = "xxMonthOfYear";
			this.tabPageMonthOfYear.ThemesEnabled = true;
			// 
			// tabPageTotal
			// 
			this.tabPageTotal.Image = null;
			this.tabPageTotal.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageTotal.Location = new System.Drawing.Point(2, 25);
			this.tabPageTotal.Name = "tabPageTotal";
			this.tabPageTotal.ShowCloseButton = true;
			this.tabPageTotal.Size = new System.Drawing.Size(1262, 669);
			this.tabPageTotal.TabIndex = 3;
			this.tabPageTotal.Text = "xxTotal";
			this.tabPageTotal.ThemesEnabled = true;
			// 
			// tabSeasonality
			// 
			this.tabSeasonality.ActiveTabColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(102)))), ((int)(((byte)(102)))));
			this.tabSeasonality.BeforeTouchSize = new System.Drawing.Size(1266, 696);
			this.tabSeasonality.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.tabSeasonality.Controls.Add(this.tabPageDataSummary);
			this.tabSeasonality.Controls.Add(this.tabPageMonthOfYear);
			this.tabSeasonality.Controls.Add(this.tabPageWeekOfMonth);
			this.tabSeasonality.Controls.Add(this.tabPageDayOfWeek);
			this.tabSeasonality.Controls.Add(this.tabPageTrend);
			this.tabSeasonality.Controls.Add(this.tabPageTotal);
			this.tabSeasonality.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabSeasonality.FixedSingleBorderColor = System.Drawing.Color.White;
			this.tabSeasonality.InactiveTabColor = System.Drawing.Color.White;
			this.tabSeasonality.Location = new System.Drawing.Point(0, 0);
			this.tabSeasonality.Name = "tabSeasonality";
			this.tabSeasonality.RotateTextWhenVertical = true;
			this.tabSeasonality.Size = new System.Drawing.Size(1266, 696);
			this.tabSeasonality.TabIndex = 0;
			this.tabSeasonality.TabPanelBackColor = System.Drawing.Color.White;
			this.tabSeasonality.TabStyle = typeof(Syncfusion.Windows.Forms.Tools.TabRendererMetro);
			this.tabSeasonality.ThemesEnabled = true;
			this.tabSeasonality.SelectedIndexChanged += new System.EventHandler(this.tabSeasonalitySelectedIndexChanged);
			// 
			// tabPageDataSummary
			// 
			this.tabPageDataSummary.Image = null;
			this.tabPageDataSummary.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageDataSummary.Location = new System.Drawing.Point(2, 25);
			this.tabPageDataSummary.Name = "tabPageDataSummary";
			this.tabPageDataSummary.ShowCloseButton = true;
			this.tabPageDataSummary.Size = new System.Drawing.Size(1262, 669);
			this.tabPageDataSummary.TabIndex = 6;
			this.tabPageDataSummary.Text = "xxDataSummary";
			this.tabPageDataSummary.ThemesEnabled = true;
			// 
			// tabPageTrend
			// 
			this.tabPageTrend.Image = null;
			this.tabPageTrend.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageTrend.Location = new System.Drawing.Point(2, 25);
			this.tabPageTrend.Name = "tabPageTrend";
			this.tabPageTrend.ShowCloseButton = true;
			this.tabPageTrend.Size = new System.Drawing.Size(1262, 669);
			this.tabPageTrend.TabIndex = 5;
			this.tabPageTrend.Text = "xxTrend";
			this.tabPageTrend.ThemesEnabled = true;
			// 
			// WFSeasonalityTabs
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.tabSeasonality);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.Name = "WFSeasonalityTabs";
			this.Size = new System.Drawing.Size(1266, 696);
			((System.ComponentModel.ISupportInitialize)(this.tabSeasonality)).EndInit();
			this.tabSeasonality.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageDayOfWeek;
		private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageWeekOfMonth;
		private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageMonthOfYear;
		private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageTotal;
		private Syncfusion.Windows.Forms.Tools.TabControlAdv tabSeasonality;
		private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageTrend;
		private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageDataSummary;
	}
}
