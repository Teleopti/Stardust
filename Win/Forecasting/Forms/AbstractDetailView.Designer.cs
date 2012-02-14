namespace Teleopti.Ccc.Win.Forecasting.Forms
{
    partial class AbstractDetailView
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
                UnhookEvents();
                ReleaseManagedResources();
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
            this.tabControl = new Syncfusion.Windows.Forms.Tools.TabControlAdv();
            this.tabPageMonth = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
            this.tabPageWeek = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
            this.tabPageDay = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
            this.tabPageIntraday = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
            ((System.ComponentModel.ISupportInitialize)(this.tabControl)).BeginInit();
            this.tabControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.ActiveTabFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.tabControl.Controls.Add(this.tabPageMonth);
            this.tabControl.Controls.Add(this.tabPageWeek);
            this.tabControl.Controls.Add(this.tabPageDay);
            this.tabControl.Controls.Add(this.tabPageIntraday);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.ItemSize = new System.Drawing.Size(54, 0);
            this.tabControl.Location = new System.Drawing.Point(3, 3);
            this.tabControl.Name = "tabControl";
            this.tabControl.Size = new System.Drawing.Size(463, 329);
            this.tabControl.TabGap = 10;
            this.tabControl.TabIndex = 0;
            this.tabControl.TabPanelBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(199)))), ((int)(((byte)(216)))), ((int)(((byte)(237)))));
            this.tabControl.TabStyle = typeof(Syncfusion.Windows.Forms.Tools.TabRendererOffice2007);
            this.tabControl.ThemesEnabled = true;
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tabControl_SelectedIndexChanged);
            // 
            // tabPageMonth
            // 
            this.tabPageMonth.Location = new System.Drawing.Point(3, 9);
            this.tabPageMonth.Margin = new System.Windows.Forms.Padding(0);
            this.tabPageMonth.Name = "tabPageMonth";
            this.tabPageMonth.Size = new System.Drawing.Size(456, 316);
            this.tabPageMonth.TabFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.tabPageMonth.TabIndex = 1;
            this.tabPageMonth.Text = "xxMonth";
            this.tabPageMonth.ThemesEnabled = true;
            // 
            // tabPageWeek
            // 
            this.tabPageWeek.Location = new System.Drawing.Point(3, 31);
            this.tabPageWeek.Margin = new System.Windows.Forms.Padding(0);
            this.tabPageWeek.Name = "tabPageWeek";
            this.tabPageWeek.Size = new System.Drawing.Size(456, 294);
            this.tabPageWeek.TabIndex = 1;
            this.tabPageWeek.Text = "xxWeek";
            this.tabPageWeek.ThemesEnabled = true;
            // 
            // tabPageDay
            // 
            this.tabPageDay.Location = new System.Drawing.Point(3, 31);
            this.tabPageDay.Margin = new System.Windows.Forms.Padding(0);
            this.tabPageDay.Name = "tabPageDay";
            this.tabPageDay.Size = new System.Drawing.Size(456, 294);
            this.tabPageDay.TabIndex = 2;
            this.tabPageDay.Text = "xxDay";
            this.tabPageDay.ThemesEnabled = true;
            // 
            // tabPageIntraday
            // 
            this.tabPageIntraday.Location = new System.Drawing.Point(3, 31);
            this.tabPageIntraday.Margin = new System.Windows.Forms.Padding(0);
            this.tabPageIntraday.Name = "tabPageIntraday";
            this.tabPageIntraday.Size = new System.Drawing.Size(456, 294);
            this.tabPageIntraday.TabIndex = 3;
            this.tabPageIntraday.Text = "xxIntraday";
            this.tabPageIntraday.ThemesEnabled = true;
            // 
            // AbstractDetailView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl);
            this.Name = "AbstractDetailView";
            this.Size = new System.Drawing.Size(469, 335);
            ((System.ComponentModel.ISupportInitialize)(this.tabControl)).EndInit();
            this.tabControl.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.TabControlAdv  tabControl;
        protected  Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageMonth;
        protected Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageWeek;
        protected Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageDay;
        protected Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageIntraday;
    }
}
