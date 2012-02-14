namespace Teleopti.Ccc.Win.Reporting
{
    partial class SchedulerReportsViewer
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
                if (components!=null)
                    components.Dispose();

                ReleaseManagedResources();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1302:DoNotHardcodeLocaleSpecificStrings", MessageId = "Start menu")]
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SchedulerReportsViewer));
            this.reportViewerControl1 = new Teleopti.Ccc.OnlineReporting.ReportViewerControl();
            this._reportSettings1 = new Teleopti.Ccc.Win.Reporting.ReportSettingsHostView();
            this.ribbonControlAdv1 = new Syncfusion.Windows.Forms.Tools.RibbonControlAdv();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).BeginInit();
            this.SuspendLayout();
            // 
            // reportViewerControl1
            // 
            this.reportViewerControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.reportViewerControl1.Location = new System.Drawing.Point(6, 71);
            this.reportViewerControl1.Name = "reportViewerControl1";
            this.reportViewerControl1.Size = new System.Drawing.Size(996, 590);
            this.reportViewerControl1.TabIndex = 0;
            // 
            // _reportSettings1
            // 
            this._reportSettings1.BackColor = System.Drawing.SystemColors.Window;
            this._reportSettings1.Dock = System.Windows.Forms.DockStyle.Top;
            this._reportSettings1.Location = new System.Drawing.Point(6, 34);
            this._reportSettings1.Name = "_reportSettings1";
            this._reportSettings1.Size = new System.Drawing.Size(996, 37);
            this._reportSettings1.TabIndex = 1;
            // 
            // ribbonControlAdv1
            // 
            this.ribbonControlAdv1.Location = new System.Drawing.Point(1, 0);
            this.ribbonControlAdv1.MenuButtonVisible = false;
            this.ribbonControlAdv1.Name = "ribbonControlAdv1";
            // 
            // ribbonControlAdv1.OfficeMenu
            // 
            this.ribbonControlAdv1.OfficeMenu.Name = "OfficeMenu";
            this.ribbonControlAdv1.OfficeMenu.Size = new System.Drawing.Size(12, 65);
            this.ribbonControlAdv1.QuickPanelVisible = false;
            this.ribbonControlAdv1.ShowQuickItemsDropDownButton = false;
            this.ribbonControlAdv1.Size = new System.Drawing.Size(1006, 33);
            this.ribbonControlAdv1.SystemText.QuickAccessDialogDropDownName = "Start menu";
            this.ribbonControlAdv1.TabIndex = 2;
            this.ribbonControlAdv1.Text = "ribbonControlAdv1";
            // 
            // SchedulerReportsViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1008, 667);
            this.Controls.Add(this.ribbonControlAdv1);
            this.Controls.Add(this.reportViewerControl1);
            this.Controls.Add(this._reportSettings1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(252, 100);
            this.Name = "SchedulerReportsViewer";
            this.Text = "xxSchedulerReportsViewer";
            this.Resize += new System.EventHandler(this.SchedulerReportsViewerResize);
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Teleopti.Ccc.OnlineReporting.ReportViewerControl reportViewerControl1;
        private ReportSettingsHostView _reportSettings1;
        private Syncfusion.Windows.Forms.Tools.RibbonControlAdv ribbonControlAdv1;

    }
}