namespace Teleopti.Ccc.Win.Reporting
{
    partial class ReportDateFromToSelector
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxToColon"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxFromColon")]
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.labelTargetPeriodTo = new System.Windows.Forms.Label();
			this.labelTargetPeriodFrom = new System.Windows.Forms.Label();
			this.dateTimePickerAdvWorkAStartDate = new Syncfusion.Windows.Forms.Tools.DateTimePickerAdv();
			this.dateTimePickerAdvWorkEndPeriod = new Syncfusion.Windows.Forms.Tools.DateTimePickerAdv();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvWorkAStartDate)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvWorkAStartDate.Calendar)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvWorkEndPeriod)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvWorkEndPeriod.Calendar)).BeginInit();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.Controls.Add(this.labelTargetPeriodTo, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.labelTargetPeriodFrom, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.dateTimePickerAdvWorkAStartDate, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.dateTimePickerAdvWorkEndPeriod, 1, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(230, 59);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// labelTargetPeriodTo
			// 
			this.labelTargetPeriodTo.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelTargetPeriodTo.AutoSize = true;
			this.labelTargetPeriodTo.BackColor = System.Drawing.Color.Transparent;
			this.labelTargetPeriodTo.Location = new System.Drawing.Point(3, 36);
			this.labelTargetPeriodTo.Name = "labelTargetPeriodTo";
			this.labelTargetPeriodTo.Size = new System.Drawing.Size(63, 15);
			this.labelTargetPeriodTo.TabIndex = 2;
			this.labelTargetPeriodTo.Text = "xxToColon";
			// 
			// labelTargetPeriodFrom
			// 
			this.labelTargetPeriodFrom.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelTargetPeriodFrom.AutoSize = true;
			this.labelTargetPeriodFrom.BackColor = System.Drawing.Color.Transparent;
			this.labelTargetPeriodFrom.Location = new System.Drawing.Point(3, 7);
			this.labelTargetPeriodFrom.Name = "labelTargetPeriodFrom";
			this.labelTargetPeriodFrom.Size = new System.Drawing.Size(77, 15);
			this.labelTargetPeriodFrom.TabIndex = 1;
			this.labelTargetPeriodFrom.Text = "xxFromColon";
			// 
			// dateTimePickerAdvWorkAStartDate
			// 
			this.dateTimePickerAdvWorkAStartDate.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.White);
			this.dateTimePickerAdvWorkAStartDate.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
			this.dateTimePickerAdvWorkAStartDate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			// 
			// 
			// 
			this.dateTimePickerAdvWorkAStartDate.Calendar.AllowMultipleSelection = false;
			this.dateTimePickerAdvWorkAStartDate.Calendar.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.dateTimePickerAdvWorkAStartDate.Calendar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.dateTimePickerAdvWorkAStartDate.Calendar.BottomHeight = 25;
			this.dateTimePickerAdvWorkAStartDate.Calendar.Culture = new System.Globalization.CultureInfo("sv-SE");
			this.dateTimePickerAdvWorkAStartDate.Calendar.DayNamesColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvWorkAStartDate.Calendar.DayNamesFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dateTimePickerAdvWorkAStartDate.Calendar.DaysFont = new System.Drawing.Font("Segoe UI", 9F);
			this.dateTimePickerAdvWorkAStartDate.Calendar.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dateTimePickerAdvWorkAStartDate.Calendar.Font = new System.Drawing.Font("Tahoma", 8F);
			this.dateTimePickerAdvWorkAStartDate.Calendar.ForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvWorkAStartDate.Calendar.GridLines = Syncfusion.Windows.Forms.Grid.GridBorderStyle.None;
			this.dateTimePickerAdvWorkAStartDate.Calendar.HeaderEndColor = System.Drawing.Color.White;
			this.dateTimePickerAdvWorkAStartDate.Calendar.HeaderStartColor = System.Drawing.Color.White;
			this.dateTimePickerAdvWorkAStartDate.Calendar.HeadForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvWorkAStartDate.Calendar.HighlightColor = System.Drawing.Color.White;
			this.dateTimePickerAdvWorkAStartDate.Calendar.Iso8601CalenderFormat = false;
			this.dateTimePickerAdvWorkAStartDate.Calendar.Location = new System.Drawing.Point(0, 0);
			this.dateTimePickerAdvWorkAStartDate.Calendar.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvWorkAStartDate.Calendar.MinValue = new System.DateTime(1753, 1, 1, 0, 0, 0, 0);
			this.dateTimePickerAdvWorkAStartDate.Calendar.Name = "monthCalendar";
			this.dateTimePickerAdvWorkAStartDate.Calendar.ScrollButtonSize = new System.Drawing.Size(24, 24);
			this.dateTimePickerAdvWorkAStartDate.Calendar.SelectedDates = new System.DateTime[0];
			this.dateTimePickerAdvWorkAStartDate.Calendar.ShowWeekNumbers = true;
			this.dateTimePickerAdvWorkAStartDate.Calendar.Size = new System.Drawing.Size(107, 174);
			this.dateTimePickerAdvWorkAStartDate.Calendar.SizeToFit = true;
			this.dateTimePickerAdvWorkAStartDate.Calendar.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.dateTimePickerAdvWorkAStartDate.Calendar.TabIndex = 0;
			this.dateTimePickerAdvWorkAStartDate.Calendar.WeekFont = new System.Drawing.Font("Verdana", 8F);
			this.dateTimePickerAdvWorkAStartDate.Calendar.WeekInterior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.PeachPuff, System.Drawing.Color.AntiqueWhite);
			// 
			// 
			// 
			this.dateTimePickerAdvWorkAStartDate.Calendar.NoneButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.dateTimePickerAdvWorkAStartDate.Calendar.NoneButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvWorkAStartDate.Calendar.NoneButton.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.dateTimePickerAdvWorkAStartDate.Calendar.NoneButton.ForeColor = System.Drawing.Color.White;
			this.dateTimePickerAdvWorkAStartDate.Calendar.NoneButton.IsBackStageButton = false;
			this.dateTimePickerAdvWorkAStartDate.Calendar.NoneButton.Location = new System.Drawing.Point(23, 0);
			this.dateTimePickerAdvWorkAStartDate.Calendar.NoneButton.Size = new System.Drawing.Size(84, 25);
			this.dateTimePickerAdvWorkAStartDate.Calendar.NoneButton.Text = "xxNone";
			this.dateTimePickerAdvWorkAStartDate.Calendar.NoneButton.UseVisualStyle = true;
			// 
			// 
			// 
			this.dateTimePickerAdvWorkAStartDate.Calendar.TodayButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.dateTimePickerAdvWorkAStartDate.Calendar.TodayButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvWorkAStartDate.Calendar.TodayButton.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.dateTimePickerAdvWorkAStartDate.Calendar.TodayButton.ForeColor = System.Drawing.Color.White;
			this.dateTimePickerAdvWorkAStartDate.Calendar.TodayButton.IsBackStageButton = false;
			this.dateTimePickerAdvWorkAStartDate.Calendar.TodayButton.Location = new System.Drawing.Point(0, 0);
			this.dateTimePickerAdvWorkAStartDate.Calendar.TodayButton.Size = new System.Drawing.Size(23, 25);
			this.dateTimePickerAdvWorkAStartDate.Calendar.TodayButton.Text = "xxToday";
			this.dateTimePickerAdvWorkAStartDate.Calendar.TodayButton.UseVisualStyle = true;
			this.dateTimePickerAdvWorkAStartDate.CalendarFont = new System.Drawing.Font("Tahoma", 8F);
			this.dateTimePickerAdvWorkAStartDate.CalendarSize = new System.Drawing.Size(189, 176);
			this.dateTimePickerAdvWorkAStartDate.CalendarTitleForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvWorkAStartDate.DropDownImage = null;
			this.dateTimePickerAdvWorkAStartDate.DropDownNormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvWorkAStartDate.DropDownPressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvWorkAStartDate.DropDownSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(179)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvWorkAStartDate.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			this.dateTimePickerAdvWorkAStartDate.ForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvWorkAStartDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
			this.dateTimePickerAdvWorkAStartDate.Location = new System.Drawing.Point(118, 3);
			this.dateTimePickerAdvWorkAStartDate.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvWorkAStartDate.MinValue = new System.DateTime(((long)(0)));
			this.dateTimePickerAdvWorkAStartDate.Name = "dateTimePickerAdvWorkAStartDate";
			this.dateTimePickerAdvWorkAStartDate.NullString = "xxNoDateIsSelected";
			this.dateTimePickerAdvWorkAStartDate.ShowCheckBox = false;
			this.dateTimePickerAdvWorkAStartDate.Size = new System.Drawing.Size(109, 21);
			this.dateTimePickerAdvWorkAStartDate.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.dateTimePickerAdvWorkAStartDate.TabIndex = 3;
			this.dateTimePickerAdvWorkAStartDate.ThemesEnabled = true;
			this.dateTimePickerAdvWorkAStartDate.UseCurrentCulture = true;
			this.dateTimePickerAdvWorkAStartDate.Value = new System.DateTime(2008, 3, 28, 14, 41, 50, 358);
			// 
			// dateTimePickerAdvWorkEndPeriod
			// 
			this.dateTimePickerAdvWorkEndPeriod.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
			this.dateTimePickerAdvWorkEndPeriod.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			// 
			// 
			// 
			this.dateTimePickerAdvWorkEndPeriod.Calendar.AllowMultipleSelection = false;
			this.dateTimePickerAdvWorkEndPeriod.Calendar.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.dateTimePickerAdvWorkEndPeriod.Calendar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.dateTimePickerAdvWorkEndPeriod.Calendar.BottomHeight = 25;
			this.dateTimePickerAdvWorkEndPeriod.Calendar.Culture = new System.Globalization.CultureInfo("sv-SE");
			this.dateTimePickerAdvWorkEndPeriod.Calendar.DayNamesColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvWorkEndPeriod.Calendar.DayNamesFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
			this.dateTimePickerAdvWorkEndPeriod.Calendar.DaysFont = new System.Drawing.Font("Segoe UI", 9F);
			this.dateTimePickerAdvWorkEndPeriod.Calendar.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dateTimePickerAdvWorkEndPeriod.Calendar.Font = new System.Drawing.Font("Tahoma", 8F);
			this.dateTimePickerAdvWorkEndPeriod.Calendar.ForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvWorkEndPeriod.Calendar.GridLines = Syncfusion.Windows.Forms.Grid.GridBorderStyle.None;
			this.dateTimePickerAdvWorkEndPeriod.Calendar.HeaderEndColor = System.Drawing.Color.White;
			this.dateTimePickerAdvWorkEndPeriod.Calendar.HeaderStartColor = System.Drawing.Color.White;
			this.dateTimePickerAdvWorkEndPeriod.Calendar.HeadForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvWorkEndPeriod.Calendar.HighlightColor = System.Drawing.Color.White;
			this.dateTimePickerAdvWorkEndPeriod.Calendar.Iso8601CalenderFormat = false;
			this.dateTimePickerAdvWorkEndPeriod.Calendar.Location = new System.Drawing.Point(0, 0);
			this.dateTimePickerAdvWorkEndPeriod.Calendar.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvWorkEndPeriod.Calendar.MinValue = new System.DateTime(1753, 1, 1, 0, 0, 0, 0);
			this.dateTimePickerAdvWorkEndPeriod.Calendar.Name = "monthCalendar";
			this.dateTimePickerAdvWorkEndPeriod.Calendar.ScrollButtonSize = new System.Drawing.Size(24, 24);
			this.dateTimePickerAdvWorkEndPeriod.Calendar.SelectedDates = new System.DateTime[0];
			this.dateTimePickerAdvWorkEndPeriod.Calendar.ShowWeekNumbers = true;
			this.dateTimePickerAdvWorkEndPeriod.Calendar.Size = new System.Drawing.Size(107, 174);
			this.dateTimePickerAdvWorkEndPeriod.Calendar.SizeToFit = true;
			this.dateTimePickerAdvWorkEndPeriod.Calendar.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.dateTimePickerAdvWorkEndPeriod.Calendar.TabIndex = 0;
			this.dateTimePickerAdvWorkEndPeriod.Calendar.WeekFont = new System.Drawing.Font("Verdana", 8F);
			this.dateTimePickerAdvWorkEndPeriod.Calendar.WeekInterior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.PeachPuff, System.Drawing.Color.AntiqueWhite);
			// 
			// 
			// 
			this.dateTimePickerAdvWorkEndPeriod.Calendar.NoneButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.dateTimePickerAdvWorkEndPeriod.Calendar.NoneButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvWorkEndPeriod.Calendar.NoneButton.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.dateTimePickerAdvWorkEndPeriod.Calendar.NoneButton.ForeColor = System.Drawing.Color.White;
			this.dateTimePickerAdvWorkEndPeriod.Calendar.NoneButton.IsBackStageButton = false;
			this.dateTimePickerAdvWorkEndPeriod.Calendar.NoneButton.Location = new System.Drawing.Point(23, 0);
			this.dateTimePickerAdvWorkEndPeriod.Calendar.NoneButton.Size = new System.Drawing.Size(84, 25);
			this.dateTimePickerAdvWorkEndPeriod.Calendar.NoneButton.Text = "xxNone";
			this.dateTimePickerAdvWorkEndPeriod.Calendar.NoneButton.UseVisualStyle = true;
			// 
			// 
			// 
			this.dateTimePickerAdvWorkEndPeriod.Calendar.TodayButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.dateTimePickerAdvWorkEndPeriod.Calendar.TodayButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvWorkEndPeriod.Calendar.TodayButton.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.dateTimePickerAdvWorkEndPeriod.Calendar.TodayButton.ForeColor = System.Drawing.Color.White;
			this.dateTimePickerAdvWorkEndPeriod.Calendar.TodayButton.IsBackStageButton = false;
			this.dateTimePickerAdvWorkEndPeriod.Calendar.TodayButton.Location = new System.Drawing.Point(0, 0);
			this.dateTimePickerAdvWorkEndPeriod.Calendar.TodayButton.Size = new System.Drawing.Size(23, 25);
			this.dateTimePickerAdvWorkEndPeriod.Calendar.TodayButton.Text = "xxToday";
			this.dateTimePickerAdvWorkEndPeriod.Calendar.TodayButton.UseVisualStyle = true;
			this.dateTimePickerAdvWorkEndPeriod.CalendarFont = new System.Drawing.Font("Tahoma", 8F);
			this.dateTimePickerAdvWorkEndPeriod.CalendarSize = new System.Drawing.Size(189, 176);
			this.dateTimePickerAdvWorkEndPeriod.CalendarTitleForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvWorkEndPeriod.DropDownImage = null;
			this.dateTimePickerAdvWorkEndPeriod.DropDownNormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvWorkEndPeriod.DropDownPressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvWorkEndPeriod.DropDownSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(179)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvWorkEndPeriod.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			this.dateTimePickerAdvWorkEndPeriod.ForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvWorkEndPeriod.Format = System.Windows.Forms.DateTimePickerFormat.Short;
			this.dateTimePickerAdvWorkEndPeriod.Location = new System.Drawing.Point(118, 32);
			this.dateTimePickerAdvWorkEndPeriod.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvWorkEndPeriod.MinValue = new System.DateTime(((long)(0)));
			this.dateTimePickerAdvWorkEndPeriod.Name = "dateTimePickerAdvWorkEndPeriod";
			this.dateTimePickerAdvWorkEndPeriod.NullString = "xxNoDateIsSelected";
			this.dateTimePickerAdvWorkEndPeriod.ShowCheckBox = false;
			this.dateTimePickerAdvWorkEndPeriod.Size = new System.Drawing.Size(109, 21);
			this.dateTimePickerAdvWorkEndPeriod.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.dateTimePickerAdvWorkEndPeriod.TabIndex = 4;
			this.dateTimePickerAdvWorkEndPeriod.ThemesEnabled = true;
			this.dateTimePickerAdvWorkEndPeriod.UseCurrentCulture = true;
			this.dateTimePickerAdvWorkEndPeriod.Value = new System.DateTime(2008, 3, 28, 14, 41, 50, 358);
			// 
			// ReportDateFromToSelector
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "ReportDateFromToSelector";
			this.Size = new System.Drawing.Size(230, 59);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvWorkAStartDate.Calendar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvWorkAStartDate)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvWorkEndPeriod.Calendar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvWorkEndPeriod)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label labelTargetPeriodFrom;
        private System.Windows.Forms.Label labelTargetPeriodTo;
        private Syncfusion.Windows.Forms.Tools.DateTimePickerAdv dateTimePickerAdvWorkAStartDate;
        private Syncfusion.Windows.Forms.Tools.DateTimePickerAdv dateTimePickerAdvWorkEndPeriod;
    }
}
