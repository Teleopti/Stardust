namespace Teleopti.Ccc.Win.Reporting
{
    partial class ReportSettingsScheduledTimePerActivityView
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxOK"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxNoDateIsSelected"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Win.Reporting.ReportDateFromToSelector.set_NullString(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.Control.set_Text(System.String)")]
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReportSettingsScheduledTimePerActivityView));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonAdvOk = new Syncfusion.Windows.Forms.ButtonAdv();
            this.reportScenarioSelector1 = new Teleopti.Ccc.Win.Reporting.ReportScenarioSelector();
            this.reportAgentSelector1 = new Teleopti.Ccc.Win.Reporting.ReportAgentSelector();
            this.reportTimeZoneSelector1 = new Teleopti.Ccc.Win.Reporting.ReportTimeZoneSelector();
            this.twoListSelector1 = new Teleopti.Ccc.Win.Common.Controls.TwoListSelector();
            this.reportDateFromToSelector1 = new Teleopti.Ccc.Win.Reporting.ReportDateFromToSelector();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.reportScenarioSelector1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.reportAgentSelector1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.reportTimeZoneSelector1, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.twoListSelector1, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.reportDateFromToSelector1, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 7;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(765, 568);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 57.70065F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 42.29935F));
            this.tableLayoutPanel2.Controls.Add(this.buttonAdvOk, 1, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 520);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(583, 40);
            this.tableLayoutPanel2.TabIndex = 9;
            // 
            // buttonAdvOk
            // 
            this.buttonAdvOk.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.buttonAdvOk.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
            this.buttonAdvOk.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
            this.buttonAdvOk.BeforeTouchSize = new System.Drawing.Size(87, 27);
            this.buttonAdvOk.ForeColor = System.Drawing.Color.White;
            this.buttonAdvOk.IsBackStageButton = false;
            this.buttonAdvOk.Location = new System.Drawing.Point(493, 6);
            this.buttonAdvOk.Name = "buttonAdvOk";
            this.buttonAdvOk.Size = new System.Drawing.Size(87, 27);
            this.buttonAdvOk.TabIndex = 6;
            this.buttonAdvOk.Text = "xxOK";
            this.buttonAdvOk.UseVisualStyle = true;
            this.buttonAdvOk.Click += new System.EventHandler(this.buttonAdvOkClick);
            // 
            // reportScenarioSelector1
            // 
            this.reportScenarioSelector1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.reportScenarioSelector1.Location = new System.Drawing.Point(3, 3);
            this.reportScenarioSelector1.Name = "reportScenarioSelector1";
            this.reportScenarioSelector1.SelectedItem = null;
            this.reportScenarioSelector1.Size = new System.Drawing.Size(467, 30);
            this.reportScenarioSelector1.TabIndex = 1;
            // 
            // reportAgentSelector1
            // 
            this.reportAgentSelector1.ComponentContext = null;
            this.reportAgentSelector1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.reportAgentSelector1.Location = new System.Drawing.Point(3, 105);
            this.reportAgentSelector1.Name = "reportAgentSelector1";
            this.reportAgentSelector1.ReportApplicationFunction = null;
            this.reportAgentSelector1.SelectedGroupPageKey = null;
            this.reportAgentSelector1.Size = new System.Drawing.Size(467, 30);
            this.reportAgentSelector1.TabIndex = 3;
            // 
            // reportTimeZoneSelector1
            // 
            this.reportTimeZoneSelector1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.reportTimeZoneSelector1.Location = new System.Drawing.Point(3, 141);
            this.reportTimeZoneSelector1.Name = "reportTimeZoneSelector1";
            this.reportTimeZoneSelector1.Size = new System.Drawing.Size(467, 30);
            this.reportTimeZoneSelector1.TabIndex = 4;
            // 
            // twoListSelector1
            // 
            this.twoListSelector1.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
            this.twoListSelector1.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.twoListSelector1.Location = new System.Drawing.Point(3, 177);
            this.twoListSelector1.MaximumSize = new System.Drawing.Size(583, 337);
            this.twoListSelector1.Name = "twoListSelector1";
            this.twoListSelector1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.twoListSelector1.Size = new System.Drawing.Size(583, 337);
            this.twoListSelector1.TabIndex = 5;
            // 
            // reportDateFromToSelector1
            // 
            this.reportDateFromToSelector1.EnableNullDates = true;
            this.reportDateFromToSelector1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.reportDateFromToSelector1.Location = new System.Drawing.Point(3, 39);
            this.reportDateFromToSelector1.Name = "reportDateFromToSelector1";
            this.reportDateFromToSelector1.NullString = "xxNoDateIsSelected";
            this.reportDateFromToSelector1.Size = new System.Drawing.Size(467, 60);
            this.reportDateFromToSelector1.TabIndex = 2;
            this.reportDateFromToSelector1.WorkPeriodEnd = ((Teleopti.Interfaces.Domain.DateOnly)(resources.GetObject("reportDateFromToSelector1.WorkPeriodEnd")));
            this.reportDateFromToSelector1.WorkPeriodStart = ((Teleopti.Interfaces.Domain.DateOnly)(resources.GetObject("reportDateFromToSelector1.WorkPeriodStart")));
            // 
            // ReportSettingsScheduledTimePerActivityView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "ReportSettingsScheduledTimePerActivityView";
            this.Size = new System.Drawing.Size(765, 568);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvOk;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private ReportScenarioSelector reportScenarioSelector1;
        private ReportAgentSelector reportAgentSelector1;
        private ReportTimeZoneSelector reportTimeZoneSelector1;
        private Teleopti.Ccc.Win.Common.Controls.TwoListSelector twoListSelector1;
        private ReportDateFromToSelector reportDateFromToSelector1;

    }
}
