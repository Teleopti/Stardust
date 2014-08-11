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
			this.tabControl.ActiveTabColor = System.Drawing.Color.DarkGray;
			this.tabControl.BeforeTouchSize = new System.Drawing.Size(469, 335);
			this.tabControl.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.tabControl.Controls.Add(this.tabPageMonth);
			this.tabControl.Controls.Add(this.tabPageWeek);
			this.tabControl.Controls.Add(this.tabPageDay);
			this.tabControl.Controls.Add(this.tabPageIntraday);
			this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl.InactiveTabColor = System.Drawing.Color.White;
			this.tabControl.ItemSize = new System.Drawing.Size(54, 0);
			this.tabControl.Location = new System.Drawing.Point(0, 0);
			this.tabControl.Name = "tabControl";
			this.tabControl.Size = new System.Drawing.Size(469, 335);
			this.tabControl.TabIndex = 0;
			this.tabControl.TabPanelBackColor = System.Drawing.Color.White;
			this.tabControl.TabStyle = typeof(Syncfusion.Windows.Forms.Tools.TabRendererMetro);
			this.tabControl.ThemesEnabled = true;
			this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tabControl_SelectedIndexChanged);
			// 
			// tabPageMonth
			// 
			this.tabPageMonth.BackColor = System.Drawing.Color.White;
			this.tabPageMonth.Image = null;
			this.tabPageMonth.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageMonth.Location = new System.Drawing.Point(2, 1);
			this.tabPageMonth.Margin = new System.Windows.Forms.Padding(0);
			this.tabPageMonth.Name = "tabPageMonth";
			this.tabPageMonth.ShowCloseButton = true;
			this.tabPageMonth.Size = new System.Drawing.Size(465, 332);
			this.tabPageMonth.TabBackColor = System.Drawing.Color.White;
			this.tabPageMonth.TabFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.tabPageMonth.TabIndex = 1;
			this.tabPageMonth.Text = "xxMonth";
			this.tabPageMonth.ThemesEnabled = true;
			// 
			// tabPageWeek
			// 
			this.tabPageWeek.BackColor = System.Drawing.Color.White;
			this.tabPageWeek.Image = null;
			this.tabPageWeek.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageWeek.Location = new System.Drawing.Point(2, 1);
			this.tabPageWeek.Margin = new System.Windows.Forms.Padding(0);
			this.tabPageWeek.Name = "tabPageWeek";
			this.tabPageWeek.ShowCloseButton = true;
			this.tabPageWeek.Size = new System.Drawing.Size(465, 332);
			this.tabPageWeek.TabBackColor = System.Drawing.Color.White;
			this.tabPageWeek.TabIndex = 1;
			this.tabPageWeek.Text = "xxWeek";
			this.tabPageWeek.ThemesEnabled = true;
			// 
			// tabPageDay
			// 
			this.tabPageDay.BackColor = System.Drawing.Color.White;
			this.tabPageDay.Image = null;
			this.tabPageDay.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageDay.Location = new System.Drawing.Point(2, 1);
			this.tabPageDay.Margin = new System.Windows.Forms.Padding(0);
			this.tabPageDay.Name = "tabPageDay";
			this.tabPageDay.ShowCloseButton = true;
			this.tabPageDay.Size = new System.Drawing.Size(465, 332);
			this.tabPageDay.TabBackColor = System.Drawing.Color.White;
			this.tabPageDay.TabIndex = 2;
			this.tabPageDay.Text = "xxDay";
			this.tabPageDay.ThemesEnabled = true;
			// 
			// tabPageIntraday
			// 
			this.tabPageIntraday.BackColor = System.Drawing.Color.White;
			this.tabPageIntraday.Image = null;
			this.tabPageIntraday.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageIntraday.Location = new System.Drawing.Point(2, 1);
			this.tabPageIntraday.Margin = new System.Windows.Forms.Padding(0);
			this.tabPageIntraday.Name = "tabPageIntraday";
			this.tabPageIntraday.ShowCloseButton = true;
			this.tabPageIntraday.Size = new System.Drawing.Size(465, 332);
			this.tabPageIntraday.TabBackColor = System.Drawing.Color.White;
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
