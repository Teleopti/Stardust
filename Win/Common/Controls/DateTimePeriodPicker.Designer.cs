namespace Teleopti.Ccc.Win.Common.Controls
{
    partial class DateTimePeriodPicker
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
            this.DateTimeFrom = new Syncfusion.Windows.Forms.Tools.DateTimePickerAdv();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.DateTimeTo = new Syncfusion.Windows.Forms.Tools.DateTimePickerAdv();
            this.chkOverMidnight = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
            this.theDate = new Syncfusion.Windows.Forms.Tools.DateTimePickerAdv();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.DateTimeFrom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DateTimeFrom.Calendar)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DateTimeTo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DateTimeTo.Calendar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkOverMidnight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.theDate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.theDate.Calendar)).BeginInit();
            this.SuspendLayout();
            // 
            // DateTimeFrom
            // 
            this.DateTimeFrom.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
            this.DateTimeFrom.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(193)))), ((int)(((byte)(222)))));
            this.DateTimeFrom.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            // 
            // 
            // 
            this.DateTimeFrom.Calendar.AllowMultipleSelection = false;
            this.DateTimeFrom.Calendar.Culture = new System.Globalization.CultureInfo("en-US");
            this.DateTimeFrom.Calendar.DaysFont = new System.Drawing.Font("Verdana", 8F);
            this.DateTimeFrom.Calendar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DateTimeFrom.Calendar.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DateTimeFrom.Calendar.ForeColor = System.Drawing.SystemColors.ControlText;
            this.DateTimeFrom.Calendar.GridLines = Syncfusion.Windows.Forms.Grid.GridBorderStyle.None;
            this.DateTimeFrom.Calendar.HeaderEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.DateTimeFrom.Calendar.HeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.DateTimeFrom.Calendar.HeaderHeight = 20;
            this.DateTimeFrom.Calendar.HeaderStartColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252)))));
            this.DateTimeFrom.Calendar.HeadForeColor = System.Drawing.SystemColors.ControlText;
            this.DateTimeFrom.Calendar.HeadGradient = true;
            this.DateTimeFrom.Calendar.Location = new System.Drawing.Point(0, 0);
            this.DateTimeFrom.Calendar.Name = "monthCalendar";
            this.DateTimeFrom.Calendar.ScrollButtonSize = new System.Drawing.Size(24, 24);
            this.DateTimeFrom.Calendar.SelectedDates = new System.DateTime[0];
            this.DateTimeFrom.Calendar.Size = new System.Drawing.Size(206, 174);
            this.DateTimeFrom.Calendar.SizeToFit = true;
            this.DateTimeFrom.Calendar.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this.DateTimeFrom.Calendar.TabIndex = 0;
            this.DateTimeFrom.Calendar.WeekFont = new System.Drawing.Font("Verdana", 8F);
            this.DateTimeFrom.Calendar.WeekInterior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.PeachPuff, System.Drawing.Color.AntiqueWhite);
            // 
            // 
            // 
            this.DateTimeFrom.Calendar.NoneButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.DateTimeFrom.Calendar.NoneButton.Location = new System.Drawing.Point(134, 0);
            this.DateTimeFrom.Calendar.NoneButton.Size = new System.Drawing.Size(72, 20);
            this.DateTimeFrom.Calendar.NoneButton.Text = "None";
            this.DateTimeFrom.Calendar.NoneButton.UseVisualStyle = true;
            // 
            // 
            // 
            this.DateTimeFrom.Calendar.TodayButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.DateTimeFrom.Calendar.TodayButton.Location = new System.Drawing.Point(0, 0);
            this.DateTimeFrom.Calendar.TodayButton.Size = new System.Drawing.Size(134, 20);
            this.DateTimeFrom.Calendar.TodayButton.Text = "Today";
            this.DateTimeFrom.Calendar.TodayButton.UseVisualStyle = true;
            this.DateTimeFrom.CalendarTitleBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252)))));
            this.DateTimeFrom.CalendarTitleForeColor = System.Drawing.SystemColors.ControlText;
            this.DateTimeFrom.Culture = new System.Globalization.CultureInfo("en-US");
            this.DateTimeFrom.CustomFormat = "HH:mm ";
            this.DateTimeFrom.DigitYear = 2000;
            this.DateTimeFrom.DropDownImage = null;
            this.DateTimeFrom.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.DateTimeFrom.Location = new System.Drawing.Point(1, 21);
            this.DateTimeFrom.Margin = new System.Windows.Forms.Padding(1);
            this.DateTimeFrom.MinValue = new System.DateTime(((long)(0)));
            this.DateTimeFrom.Name = "DateTimeFrom";
            this.DateTimeFrom.ShowCheckBox = false;
            this.DateTimeFrom.ShowDropButton = false;
            this.DateTimeFrom.ShowUpDown = true;
            this.DateTimeFrom.Size = new System.Drawing.Size(70, 18);
            this.DateTimeFrom.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this.DateTimeFrom.TabIndex = 0;
            this.DateTimeFrom.Value = new System.DateTime(2008, 7, 10, 9, 22, 3, 910);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 72F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.DateTimeTo, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.DateTimeFrom, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.chkOverMidnight, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.theDate, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(145, 63);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // DateTimeTo
            // 
            this.DateTimeTo.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
            this.DateTimeTo.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(193)))), ((int)(((byte)(222)))));
            this.DateTimeTo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            // 
            // 
            // 
            this.DateTimeTo.Calendar.AllowMultipleSelection = false;
            this.DateTimeTo.Calendar.Culture = new System.Globalization.CultureInfo("sv-SE");
            this.DateTimeTo.Calendar.DaysFont = new System.Drawing.Font("Verdana", 8F);
            this.DateTimeTo.Calendar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DateTimeTo.Calendar.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DateTimeTo.Calendar.ForeColor = System.Drawing.SystemColors.ControlText;
            this.DateTimeTo.Calendar.GridLines = Syncfusion.Windows.Forms.Grid.GridBorderStyle.None;
            this.DateTimeTo.Calendar.HeaderEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.DateTimeTo.Calendar.HeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.DateTimeTo.Calendar.HeaderHeight = 20;
            this.DateTimeTo.Calendar.HeaderStartColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252)))));
            this.DateTimeTo.Calendar.HeadForeColor = System.Drawing.SystemColors.ControlText;
            this.DateTimeTo.Calendar.HeadGradient = true;
            this.DateTimeTo.Calendar.Location = new System.Drawing.Point(0, 0);
            this.DateTimeTo.Calendar.Name = "monthCalendar";
            this.DateTimeTo.Calendar.ScrollButtonSize = new System.Drawing.Size(24, 24);
            this.DateTimeTo.Calendar.SelectedDates = new System.DateTime[0];
            this.DateTimeTo.Calendar.Size = new System.Drawing.Size(206, 174);
            this.DateTimeTo.Calendar.SizeToFit = true;
            this.DateTimeTo.Calendar.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this.DateTimeTo.Calendar.TabIndex = 0;
            this.DateTimeTo.Calendar.WeekFont = new System.Drawing.Font("Verdana", 8F);
            this.DateTimeTo.Calendar.WeekInterior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.PeachPuff, System.Drawing.Color.AntiqueWhite);
            // 
            // 
            // 
            this.DateTimeTo.Calendar.NoneButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.DateTimeTo.Calendar.NoneButton.Location = new System.Drawing.Point(134, 0);
            this.DateTimeTo.Calendar.NoneButton.Size = new System.Drawing.Size(72, 20);
            this.DateTimeTo.Calendar.NoneButton.Text = "None";
            this.DateTimeTo.Calendar.NoneButton.UseVisualStyle = true;
            // 
            // 
            // 
            this.DateTimeTo.Calendar.TodayButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.DateTimeTo.Calendar.TodayButton.Location = new System.Drawing.Point(0, 0);
            this.DateTimeTo.Calendar.TodayButton.Size = new System.Drawing.Size(134, 20);
            this.DateTimeTo.Calendar.TodayButton.Text = "Today";
            this.DateTimeTo.Calendar.TodayButton.UseVisualStyle = true;
            this.DateTimeTo.CalendarTitleBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252)))));
            this.DateTimeTo.CalendarTitleForeColor = System.Drawing.SystemColors.ControlText;
            this.DateTimeTo.CustomFormat = "HH:mm";
            this.DateTimeTo.DropDownImage = null;
            this.DateTimeTo.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.DateTimeTo.Location = new System.Drawing.Point(73, 21);
            this.DateTimeTo.Margin = new System.Windows.Forms.Padding(1);
            this.DateTimeTo.MinValue = new System.DateTime(((long)(0)));
            this.DateTimeTo.Name = "DateTimeTo";
            this.DateTimeTo.ShowCheckBox = false;
            this.DateTimeTo.ShowDropButton = false;
            this.DateTimeTo.ShowUpDown = true;
            this.DateTimeTo.Size = new System.Drawing.Size(70, 18);
            this.DateTimeTo.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this.DateTimeTo.TabIndex = 1;
            this.DateTimeTo.Value = new System.DateTime(2008, 7, 10, 9, 22, 3, 910);
            // 
            // chkOverMidnight
            // 
            this.chkOverMidnight.BorderColor = System.Drawing.SystemColors.WindowFrame;
            this.tableLayoutPanel1.SetColumnSpan(this.chkOverMidnight, 2);
            this.chkOverMidnight.GradientEnd = System.Drawing.SystemColors.ControlDark;
            this.chkOverMidnight.GradientStart = System.Drawing.SystemColors.Control;
            this.chkOverMidnight.HotBorderColor = System.Drawing.SystemColors.WindowFrame;
            this.chkOverMidnight.ImageCheckBoxSize = new System.Drawing.Size(13, 13);
            this.chkOverMidnight.Location = new System.Drawing.Point(1, 41);
            this.chkOverMidnight.Margin = new System.Windows.Forms.Padding(1);
            this.chkOverMidnight.Name = "chkOverMidnight";
            this.chkOverMidnight.ShadowColor = System.Drawing.Color.Black;
            this.chkOverMidnight.ShadowOffset = new System.Drawing.Point(2, 2);
            this.chkOverMidnight.Size = new System.Drawing.Size(123, 19);
            this.chkOverMidnight.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Office2007;
            this.chkOverMidnight.TabIndex = 2;
            this.chkOverMidnight.Text = "xxOverMidnight";
            this.chkOverMidnight.ThemesEnabled = false;
            // 
            // theDate
            // 
            this.theDate.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
            this.theDate.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(193)))), ((int)(((byte)(222)))));
            this.theDate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            // 
            // 
            // 
            this.theDate.Calendar.AllowMultipleSelection = false;
            this.theDate.Calendar.Culture = new System.Globalization.CultureInfo("sv-SE");
            this.theDate.Calendar.DaysFont = new System.Drawing.Font("Verdana", 8F);
            this.theDate.Calendar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.theDate.Calendar.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.theDate.Calendar.ForeColor = System.Drawing.SystemColors.ControlText;
            this.theDate.Calendar.GridLines = Syncfusion.Windows.Forms.Grid.GridBorderStyle.None;
            this.theDate.Calendar.HeaderEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.theDate.Calendar.HeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.theDate.Calendar.HeaderHeight = 20;
            this.theDate.Calendar.HeaderStartColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252)))));
            this.theDate.Calendar.HeadForeColor = System.Drawing.SystemColors.ControlText;
            this.theDate.Calendar.HeadGradient = true;
            this.theDate.Calendar.Location = new System.Drawing.Point(0, 0);
            this.theDate.Calendar.Name = "monthCalendar";
            this.theDate.Calendar.ScrollButtonSize = new System.Drawing.Size(24, 24);
            this.theDate.Calendar.SelectedDates = new System.DateTime[0];
            this.theDate.Calendar.Size = new System.Drawing.Size(206, 174);
            this.theDate.Calendar.SizeToFit = true;
            this.theDate.Calendar.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this.theDate.Calendar.TabIndex = 0;
            this.theDate.Calendar.WeekFont = new System.Drawing.Font("Verdana", 8F);
            this.theDate.Calendar.WeekInterior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.PeachPuff, System.Drawing.Color.AntiqueWhite);
            // 
            // 
            // 
            this.theDate.Calendar.NoneButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.theDate.Calendar.NoneButton.Location = new System.Drawing.Point(134, 0);
            this.theDate.Calendar.NoneButton.Size = new System.Drawing.Size(72, 20);
            this.theDate.Calendar.NoneButton.Text = "None";
            this.theDate.Calendar.NoneButton.UseVisualStyle = true;
            // 
            // 
            // 
            this.theDate.Calendar.TodayButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.theDate.Calendar.TodayButton.Location = new System.Drawing.Point(0, 0);
            this.theDate.Calendar.TodayButton.Size = new System.Drawing.Size(134, 20);
            this.theDate.Calendar.TodayButton.Text = "Today";
            this.theDate.Calendar.TodayButton.UseVisualStyle = true;
            this.theDate.CalendarTitleBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252)))));
            this.theDate.CalendarTitleForeColor = System.Drawing.SystemColors.ControlText;
            this.tableLayoutPanel1.SetColumnSpan(this.theDate, 2);
            this.theDate.DropDownImage = null;
            this.theDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.theDate.Location = new System.Drawing.Point(1, 1);
            this.theDate.Margin = new System.Windows.Forms.Padding(1);
            this.theDate.MinValue = new System.DateTime(((long)(0)));
            this.theDate.Name = "theDate";
            this.theDate.ShowCheckBox = false;
            this.theDate.ShowDropButton = false;
            this.theDate.ShowUpDown = true;
            this.theDate.Size = new System.Drawing.Size(142, 18);
            this.theDate.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this.theDate.TabIndex = 3;
            this.theDate.Value = new System.DateTime(2008, 7, 14, 10, 9, 44, 46);
            // 
            // DateTimePeriodPicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "DateTimePeriodPicker";
            this.Size = new System.Drawing.Size(145, 63);
            ((System.ComponentModel.ISupportInitialize)(this.DateTimeFrom.Calendar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DateTimeFrom)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.DateTimeTo.Calendar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DateTimeTo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkOverMidnight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.theDate.Calendar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.theDate)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.DateTimePickerAdv DateTimeFrom;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Syncfusion.Windows.Forms.Tools.DateTimePickerAdv DateTimeTo;
        private Syncfusion.Windows.Forms.Tools.CheckBoxAdv chkOverMidnight;
        private System.Windows.Forms.ToolTip toolTip1;
        private Syncfusion.Windows.Forms.Tools.DateTimePickerAdv theDate;
    }
}
