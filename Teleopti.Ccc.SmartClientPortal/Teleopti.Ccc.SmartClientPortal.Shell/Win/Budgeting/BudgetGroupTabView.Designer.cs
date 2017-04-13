namespace Teleopti.Ccc.Win.Budgeting
{
    partial class BudgetGroupTabView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BudgetGroupTabView));
            this.tabControlAdv = new Syncfusion.Windows.Forms.Tools.TabControlAdv();
            this.tabPageAdvDay = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
            this.tabPageAdvWeek = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
            this.tabPageAdvMonth = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
            this.backgroundWorkerLoadStaffEmployed = new System.ComponentModel.BackgroundWorker();
            this.backgroundWorkerLoadForecastedHours = new System.ComponentModel.BackgroundWorker();
            this.backgroundWorkerLoadBudgetDays = new System.ComponentModel.BackgroundWorker();
            ((System.ComponentModel.ISupportInitialize)(this.tabControlAdv)).BeginInit();
            this.tabControlAdv.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControlAdv
            // 
            resources.ApplyResources(this.tabControlAdv, "tabControlAdv");
            this.tabControlAdv.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(240)))), ((int)(((byte)(249)))));
            this.tabControlAdv.Controls.Add(this.tabPageAdvDay);
            this.tabControlAdv.Controls.Add(this.tabPageAdvWeek);
            this.tabControlAdv.Controls.Add(this.tabPageAdvMonth);
            this.tabControlAdv.Name = "tabControlAdv";
            this.tabControlAdv.TabPanelBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(199)))), ((int)(((byte)(216)))), ((int)(((byte)(237)))));
            this.tabControlAdv.TabStyle = typeof(Syncfusion.Windows.Forms.Tools.TabRendererOffice2007);
            this.tabControlAdv.ThemesEnabled = true;
            // 
            // tabPageAdvDay
            // 
            this.tabPageAdvDay.Image = null;
            this.tabPageAdvDay.ImageSize = new System.Drawing.Size(16, 16);
            resources.ApplyResources(this.tabPageAdvDay, "tabPageAdvDay");
            this.tabPageAdvDay.Name = "tabPageAdvDay";
            this.tabPageAdvDay.ThemesEnabled = true;
            // 
            // tabPageAdvWeek
            // 
            this.tabPageAdvWeek.Image = null;
            this.tabPageAdvWeek.ImageSize = new System.Drawing.Size(16, 16);
            resources.ApplyResources(this.tabPageAdvWeek, "tabPageAdvWeek");
            this.tabPageAdvWeek.Name = "tabPageAdvWeek";
            this.tabPageAdvWeek.ThemesEnabled = false;
            // 
            // tabPageAdvMonth
            // 
            this.tabPageAdvMonth.Image = null;
            this.tabPageAdvMonth.ImageSize = new System.Drawing.Size(16, 16);
            resources.ApplyResources(this.tabPageAdvMonth, "tabPageAdvMonth");
            this.tabPageAdvMonth.Name = "tabPageAdvMonth";
            this.tabPageAdvMonth.ThemesEnabled = false;
            // 
            // backgroundWorkerLoadStaffEmployed
            // 
            this.backgroundWorkerLoadStaffEmployed.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerLoadStaffEmployed_DoWork);
            this.backgroundWorkerLoadStaffEmployed.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerLoadStaffEmployed_RunWorkerCompleted);
            // 
            // backgroundWorkerLoadForecastedHours
            // 
            this.backgroundWorkerLoadForecastedHours.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerLoadForecastedHours_DoWork);
            this.backgroundWorkerLoadForecastedHours.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerLoadForecastedHours_RunWorkerCompleted);
            // 
            // backgroundWorkerLoadBudgetDays
            // 
            this.backgroundWorkerLoadBudgetDays.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerLoadBudgetDays_DoWork);
            this.backgroundWorkerLoadBudgetDays.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerLoadBudgetDays_RunWorkerCompleted);
            // 
            // BudgetGroupTabView
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControlAdv);
            this.Name = "BudgetGroupTabView";
            ((System.ComponentModel.ISupportInitialize)(this.tabControlAdv)).EndInit();
            this.tabControlAdv.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.TabControlAdv tabControlAdv;
        private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageAdvDay;
        private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageAdvWeek;
        private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageAdvMonth;
		private System.ComponentModel.BackgroundWorker backgroundWorkerLoadStaffEmployed;
		private System.ComponentModel.BackgroundWorker backgroundWorkerLoadForecastedHours;
        private System.ComponentModel.BackgroundWorker backgroundWorkerLoadBudgetDays;
    }
}
