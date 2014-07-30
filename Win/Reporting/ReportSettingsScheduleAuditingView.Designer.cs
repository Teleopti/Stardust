namespace Teleopti.Ccc.Win.Reporting
{
	partial class ReportSettingsScheduleAuditingView
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "resources"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxOK"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxNoDateIsSelected"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Win.Reporting.ReportDateFromToSelector.set_NullString(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.Control.set_Text(System.String)")]
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReportSettingsScheduleAuditingView));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonAdvOk = new Syncfusion.Windows.Forms.ButtonAdv();
            this.reportDateFromToSelectorChangePeriod = new Teleopti.Ccc.Win.Reporting.ReportDateFromToSelector();
            this.reportDateFromToSelectorSchedulePeriod = new Teleopti.Ccc.Win.Reporting.ReportDateFromToSelector();
            this.reportPersonSelector1 = new Teleopti.Ccc.Win.Reporting.ReportPersonSelector();
            this.reportUserSelectorAuditingView1 = new Teleopti.Ccc.Win.Reporting.ReportUserSelectorAuditingView();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.reportDateFromToSelectorChangePeriod, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.reportDateFromToSelectorSchedulePeriod, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.reportPersonSelector1, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.reportUserSelectorAuditingView1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 7;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(656, 598);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 57.70065F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 42.29935F));
            this.tableLayoutPanel2.Controls.Add(this.buttonAdvOk, 1, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 225);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(400, 40);
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
            this.buttonAdvOk.Location = new System.Drawing.Point(310, 6);
            this.buttonAdvOk.Name = "buttonAdvOk";
            this.buttonAdvOk.Size = new System.Drawing.Size(87, 27);
            this.buttonAdvOk.TabIndex = 6;
            this.buttonAdvOk.Text = "xxOK";
            this.buttonAdvOk.UseVisualStyle = true;
            this.buttonAdvOk.Click += new System.EventHandler(this.buttonAdvOkClick);
            // 
            // reportDateFromToSelectorChangePeriod
            // 
            this.reportDateFromToSelectorChangePeriod.EnableNullDates = true;
            this.reportDateFromToSelectorChangePeriod.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.reportDateFromToSelectorChangePeriod.Location = new System.Drawing.Point(3, 37);
            this.reportDateFromToSelectorChangePeriod.Name = "reportDateFromToSelectorChangePeriod";
            this.reportDateFromToSelectorChangePeriod.NullString = "xxNoDateIsSelected";
            this.reportDateFromToSelectorChangePeriod.Size = new System.Drawing.Size(400, 60);
            this.reportDateFromToSelectorChangePeriod.TabIndex = 2;
            this.reportDateFromToSelectorChangePeriod.WorkPeriodEnd = ((Teleopti.Interfaces.Domain.DateOnly)(resources.GetObject("reportDateFromToSelectorChangePeriod.WorkPeriodEnd")));
            this.reportDateFromToSelectorChangePeriod.WorkPeriodStart = ((Teleopti.Interfaces.Domain.DateOnly)(resources.GetObject("reportDateFromToSelectorChangePeriod.WorkPeriodStart")));
            // 
            // reportDateFromToSelectorSchedulePeriod
            // 
            this.reportDateFromToSelectorSchedulePeriod.EnableNullDates = true;
            this.reportDateFromToSelectorSchedulePeriod.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.reportDateFromToSelectorSchedulePeriod.Location = new System.Drawing.Point(3, 123);
            this.reportDateFromToSelectorSchedulePeriod.Name = "reportDateFromToSelectorSchedulePeriod";
            this.reportDateFromToSelectorSchedulePeriod.NullString = "xxNoDateIsSelected";
            this.reportDateFromToSelectorSchedulePeriod.Size = new System.Drawing.Size(400, 60);
            this.reportDateFromToSelectorSchedulePeriod.TabIndex = 11;
            this.reportDateFromToSelectorSchedulePeriod.WorkPeriodEnd = ((Teleopti.Interfaces.Domain.DateOnly)(resources.GetObject("reportDateFromToSelectorSchedulePeriod.WorkPeriodEnd")));
            this.reportDateFromToSelectorSchedulePeriod.WorkPeriodStart = ((Teleopti.Interfaces.Domain.DateOnly)(resources.GetObject("reportDateFromToSelectorSchedulePeriod.WorkPeriodStart")));
            // 
            // reportPersonSelector1
            // 
            this.reportPersonSelector1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.reportPersonSelector1.Location = new System.Drawing.Point(3, 189);
            this.reportPersonSelector1.Name = "reportPersonSelector1";
            this.reportPersonSelector1.Size = new System.Drawing.Size(400, 30);
            this.reportPersonSelector1.TabIndex = 12;
            // 
            // reportUserSelectorAuditingView1
            // 
            this.reportUserSelectorAuditingView1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.reportUserSelectorAuditingView1.Location = new System.Drawing.Point(3, 3);
            this.reportUserSelectorAuditingView1.Name = "reportUserSelectorAuditingView1";
            this.reportUserSelectorAuditingView1.Size = new System.Drawing.Size(400, 28);
            this.reportUserSelectorAuditingView1.TabIndex = 13;
            // 
            // ReportSettingsScheduleAuditingView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "ReportSettingsScheduleAuditingView";
            this.Size = new System.Drawing.Size(656, 598);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvOk;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		private ReportDateFromToSelector reportDateFromToSelectorChangePeriod;
		private ReportDateFromToSelector reportDateFromToSelectorSchedulePeriod;
		private ReportPersonSelector reportPersonSelector1;
		private ReportUserSelectorAuditingView reportUserSelectorAuditingView1;

	}
}
