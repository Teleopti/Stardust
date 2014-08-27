namespace Teleopti.Ccc.Win.Common.Controls.DateSelection
{
    partial class DateSelectionCalendar : BaseUserControl
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
			this.components = new System.ComponentModel.Container();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.btnApplyPeriod = new Syncfusion.Windows.Forms.ButtonAdv();
			this.monthCalendarAdv1 = new Syncfusion.Windows.Forms.Tools.MonthCalendarAdv();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.monthCalendarAdv1)).BeginInit();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.BackColor = System.Drawing.Color.White;
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.btnApplyPeriod, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.monthCalendarAdv1, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(224, 226);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// btnApplyPeriod
			// 
			this.btnApplyPeriod.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.btnApplyPeriod.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.btnApplyPeriod.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.btnApplyPeriod.BeforeTouchSize = new System.Drawing.Size(156, 27);
			this.btnApplyPeriod.ForeColor = System.Drawing.Color.White;
			this.btnApplyPeriod.IsBackStageButton = false;
			this.btnApplyPeriod.Location = new System.Drawing.Point(34, 194);
			this.btnApplyPeriod.Name = "btnApplyPeriod";
			this.btnApplyPeriod.Size = new System.Drawing.Size(156, 27);
			this.btnApplyPeriod.TabIndex = 8;
			this.btnApplyPeriod.Text = "xxApply";
			this.btnApplyPeriod.UseVisualStyle = true;
			this.btnApplyPeriod.Click += new System.EventHandler(this.btnApplyPeriod_Click);
			// 
			// monthCalendarAdv1
			// 
			this.monthCalendarAdv1.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.monthCalendarAdv1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.monthCalendarAdv1.BottomHeight = 29;
			this.monthCalendarAdv1.Culture = new System.Globalization.CultureInfo("");
			this.monthCalendarAdv1.DayNamesColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(155)))), ((int)(((byte)(255)))));
			this.monthCalendarAdv1.DayNamesFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.monthCalendarAdv1.DaysFont = new System.Drawing.Font("Segoe UI", 9F);
			this.monthCalendarAdv1.DaysHeaderInterior = new Syncfusion.Drawing.BrushInfo(System.Drawing.SystemColors.Window);
			this.monthCalendarAdv1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.monthCalendarAdv1.GridLines = Syncfusion.Windows.Forms.Grid.GridBorderStyle.None;
			this.monthCalendarAdv1.HeaderHeight = 39;
			this.monthCalendarAdv1.HeaderStartColor = System.Drawing.Color.White;
			this.monthCalendarAdv1.HighlightColor = System.Drawing.Color.Black;
			this.monthCalendarAdv1.Iso8601CalenderFormat = false;
			this.monthCalendarAdv1.Location = new System.Drawing.Point(3, 3);
			this.monthCalendarAdv1.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(165)))), ((int)(((byte)(220)))));
			this.monthCalendarAdv1.MouseDragMultiselect = true;
			this.monthCalendarAdv1.Name = "monthCalendarAdv1";
			this.monthCalendarAdv1.ScrollButtonSize = new System.Drawing.Size(28, 28);
			this.monthCalendarAdv1.SelectedDates = new System.DateTime[] {
        new System.DateTime(2014, 8, 8, 0, 0, 0, 0)};
			this.monthCalendarAdv1.ShowWeekNumbers = true;
			this.monthCalendarAdv1.Size = new System.Drawing.Size(218, 185);
			this.monthCalendarAdv1.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.monthCalendarAdv1.TabIndex = 9;
			this.monthCalendarAdv1.WeekFont = new System.Drawing.Font("Verdana", 8F);
			this.monthCalendarAdv1.WeekInterior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.PeachPuff, System.Drawing.Color.AntiqueWhite);
			// 
			// 
			// 
			this.monthCalendarAdv1.NoneButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.monthCalendarAdv1.NoneButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(165)))), ((int)(((byte)(220)))));
			this.monthCalendarAdv1.NoneButton.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.monthCalendarAdv1.NoneButton.ForeColor = System.Drawing.Color.White;
			this.monthCalendarAdv1.NoneButton.IsBackStageButton = false;
			this.monthCalendarAdv1.NoneButton.Location = new System.Drawing.Point(164, 0);
			this.monthCalendarAdv1.NoneButton.Size = new System.Drawing.Size(84, 29);
			this.monthCalendarAdv1.NoneButton.Text = "None";
			this.monthCalendarAdv1.NoneButton.UseVisualStyle = true;
			this.monthCalendarAdv1.NoneButton.Visible = false;
			// 
			// 
			// 
			this.monthCalendarAdv1.TodayButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.monthCalendarAdv1.TodayButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(165)))), ((int)(((byte)(220)))));
			this.monthCalendarAdv1.TodayButton.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.monthCalendarAdv1.TodayButton.ForeColor = System.Drawing.Color.White;
			this.monthCalendarAdv1.TodayButton.IsBackStageButton = false;
			this.monthCalendarAdv1.TodayButton.Location = new System.Drawing.Point(0, 0);
			this.monthCalendarAdv1.TodayButton.Size = new System.Drawing.Size(248, 29);
			this.monthCalendarAdv1.TodayButton.Text = "Today";
			this.monthCalendarAdv1.TodayButton.UseVisualStyle = true;
			this.monthCalendarAdv1.TodayButton.Visible = false;
			this.monthCalendarAdv1.DateCellQueryInfo += new Syncfusion.Windows.Forms.Tools.DateCellQueryInfoEventHandler(this.monthCalendarAdv1_DateCellQueryInfo);
			// 
			// DateSelectionCalendar
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "DateSelectionCalendar";
			this.Size = new System.Drawing.Size(224, 226);
			this.tableLayoutPanel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.monthCalendarAdv1)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Syncfusion.Windows.Forms.ButtonAdv btnApplyPeriod;
        private Syncfusion.Windows.Forms.Tools.MonthCalendarAdv monthCalendarAdv1;
    }
}