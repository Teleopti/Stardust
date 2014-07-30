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
            this.reportViewerControl1 = new Teleopti.Ccc.OnlineReporting.ReportViewerControl();
            this.reportSettings1 = new Teleopti.Ccc.Win.Reporting.ReportSettingsHostView();
            this.SuspendLayout();
            // 
            // reportViewerControl1
            // 
            this.reportViewerControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.reportViewerControl1.Location = new System.Drawing.Point(0, 63);
            this.reportViewerControl1.Name = "reportViewerControl1";
            this.reportViewerControl1.Size = new System.Drawing.Size(1175, 706);
            this.reportViewerControl1.TabIndex = 0;
            // 
            // reportSettings1
            // 
            this.reportSettings1.BackColor = System.Drawing.SystemColors.Window;
            this.reportSettings1.Dock = System.Windows.Forms.DockStyle.Top;
            this.reportSettings1.Location = new System.Drawing.Point(0, 0);
            this.reportSettings1.Name = "reportSettings1";
            this.reportSettings1.Size = new System.Drawing.Size(1175, 63);
            this.reportSettings1.TabIndex = 1;
            // 
            // SchedulerReportsViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1175, 769);
            this.Controls.Add(this.reportViewerControl1);
            this.Controls.Add(this.reportSettings1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimumSize = new System.Drawing.Size(291, 109);
            this.Name = "SchedulerReportsViewer";
            this.ShowIcon = false;
            this.Text = "xxSchedulerReportsViewer";
            this.Resize += new System.EventHandler(this.SchedulerReportsViewerResize);
            this.ResumeLayout(false);

        }

        #endregion

        private Teleopti.Ccc.OnlineReporting.ReportViewerControl reportViewerControl1;
        private ReportSettingsHostView reportSettings1;

    }
}