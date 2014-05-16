namespace Teleopti.Ccc.AgentPortal.Requests.ShiftTrade
{
    partial class AddDatesView
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
                dateTimePickerToDate.Dispose();
                dateTimePickerFromDate.Dispose();
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
            this.tableLayoutPanelAddDates = new System.Windows.Forms.TableLayoutPanel();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.dateTimePickerFromDate = new Syncfusion.Windows.Forms.Tools.DateTimePickerAdv();
            this.dateTimePickerToDate = new Syncfusion.Windows.Forms.Tools.DateTimePickerAdv();
            this.buttonAdvAddDates = new Syncfusion.Windows.Forms.ButtonAdv();
            this.gradientPanelAddDates = new Syncfusion.Windows.Forms.Tools.GradientPanel();
            this.tableLayoutPanelAddDates.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dateTimePickerFromDate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateTimePickerFromDate.Calendar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateTimePickerToDate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateTimePickerToDate.Calendar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanelAddDates)).BeginInit();
            this.gradientPanelAddDates.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanelAddDates
            // 
            this.tableLayoutPanelAddDates.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanelAddDates.ColumnCount = 2;
            this.tableLayoutPanelAddDates.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
            this.tableLayoutPanelAddDates.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 55F));
            this.tableLayoutPanelAddDates.Controls.Add(this.label6, 0, 0);
            this.tableLayoutPanelAddDates.Controls.Add(this.label7, 0, 1);
            this.tableLayoutPanelAddDates.Controls.Add(this.dateTimePickerFromDate, 1, 0);
            this.tableLayoutPanelAddDates.Controls.Add(this.dateTimePickerToDate, 1, 1);
            this.tableLayoutPanelAddDates.Controls.Add(this.buttonAdvAddDates, 1, 2);
            this.tableLayoutPanelAddDates.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelAddDates.Location = new System.Drawing.Point(5, 5);
            this.tableLayoutPanelAddDates.Name = "tableLayoutPanelAddDates";
            this.tableLayoutPanelAddDates.RowCount = 3;
            this.tableLayoutPanelAddDates.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanelAddDates.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanelAddDates.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelAddDates.Size = new System.Drawing.Size(182, 86);
            this.tableLayoutPanelAddDates.TabIndex = 28;
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 2);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(70, 26);
            this.label6.TabIndex = 6;
            this.label6.Text = "xxDateFromColon";
            // 
            // label7
            // 
            this.label7.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 32);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(74, 26);
            this.label7.TabIndex = 6;
            this.label7.Text = "xxDateToColon";
            // 
            // dateTimePickerFromDate
            // 
            this.dateTimePickerFromDate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.dateTimePickerFromDate.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
            this.dateTimePickerFromDate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            // 
            // 
            // 
            this.dateTimePickerFromDate.Calendar.AllowMultipleSelection = false;
            this.dateTimePickerFromDate.Calendar.Culture = new System.Globalization.CultureInfo("sv-SE");
            this.dateTimePickerFromDate.Calendar.DaysFont = new System.Drawing.Font("Verdana", 8F);
            this.dateTimePickerFromDate.Calendar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dateTimePickerFromDate.Calendar.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dateTimePickerFromDate.Calendar.ForeColor = System.Drawing.SystemColors.ControlText;
            this.dateTimePickerFromDate.Calendar.GridLines = Syncfusion.Windows.Forms.Grid.GridBorderStyle.None;
            this.dateTimePickerFromDate.Calendar.HeaderEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.dateTimePickerFromDate.Calendar.HeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.dateTimePickerFromDate.Calendar.HeaderHeight = 20;
            this.dateTimePickerFromDate.Calendar.HeaderStartColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252)))));
            this.dateTimePickerFromDate.Calendar.HeadForeColor = System.Drawing.SystemColors.ControlText;
            this.dateTimePickerFromDate.Calendar.HeadGradient = true;
            this.dateTimePickerFromDate.Calendar.Location = new System.Drawing.Point(0, 0);
            this.dateTimePickerFromDate.Calendar.Name = "monthCalendar";
            this.dateTimePickerFromDate.Calendar.Office2007Theme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.dateTimePickerFromDate.Calendar.ScrollButtonSize = new System.Drawing.Size(24, 24);
            this.dateTimePickerFromDate.Calendar.SelectedDates = new System.DateTime[0];
            this.dateTimePickerFromDate.Calendar.ShowWeekNumbers = true;
            this.dateTimePickerFromDate.Calendar.Size = new System.Drawing.Size(206, 174);
            this.dateTimePickerFromDate.Calendar.SizeToFit = true;
            this.dateTimePickerFromDate.Calendar.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this.dateTimePickerFromDate.Calendar.TabIndex = 0;
            this.dateTimePickerFromDate.Calendar.ThemedEnabledGrid = true;
            this.dateTimePickerFromDate.Calendar.WeekFont = new System.Drawing.Font("Verdana", 8F);
            this.dateTimePickerFromDate.Calendar.WeekInterior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.PeachPuff, System.Drawing.Color.AntiqueWhite);
            // 
            // 
            // 
            this.dateTimePickerFromDate.Calendar.NoneButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.dateTimePickerFromDate.Calendar.NoneButton.Location = new System.Drawing.Point(134, 0);
            this.dateTimePickerFromDate.Calendar.NoneButton.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.dateTimePickerFromDate.Calendar.NoneButton.Size = new System.Drawing.Size(72, 20);
            this.dateTimePickerFromDate.Calendar.NoneButton.Text = "xxNone";
            this.dateTimePickerFromDate.Calendar.NoneButton.UseVisualStyle = true;
            this.dateTimePickerFromDate.Calendar.NoneButton.Visible = false;
            // 
            // 
            // 
            this.dateTimePickerFromDate.Calendar.TodayButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.dateTimePickerFromDate.Calendar.TodayButton.Location = new System.Drawing.Point(0, 0);
            this.dateTimePickerFromDate.Calendar.TodayButton.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.dateTimePickerFromDate.Calendar.TodayButton.Size = new System.Drawing.Size(134, 20);
            this.dateTimePickerFromDate.Calendar.TodayButton.Text = "xxToday";
            this.dateTimePickerFromDate.Calendar.TodayButton.UseVisualStyle = true;
            this.dateTimePickerFromDate.CalendarFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dateTimePickerFromDate.CalendarTitleBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252)))));
            this.dateTimePickerFromDate.CalendarTitleForeColor = System.Drawing.SystemColors.ControlText;
            this.dateTimePickerFromDate.DropDownImage = null;
            this.dateTimePickerFromDate.EnableNullDate = false;
            this.dateTimePickerFromDate.EnableNullKeys = false;
            this.dateTimePickerFromDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dateTimePickerFromDate.Location = new System.Drawing.Point(84, 4);
            this.dateTimePickerFromDate.MinValue = new System.DateTime(((long)(0)));
            this.dateTimePickerFromDate.Name = "dateTimePickerFromDate";
            this.dateTimePickerFromDate.NoneButtonVisible = false;
            this.dateTimePickerFromDate.Office2007Theme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.dateTimePickerFromDate.ShowCheckBox = false;
            this.dateTimePickerFromDate.Size = new System.Drawing.Size(95, 21);
            this.dateTimePickerFromDate.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this.dateTimePickerFromDate.TabIndex = 7;
            this.dateTimePickerFromDate.UseCurrentCulture = false;
            this.dateTimePickerFromDate.Value = new System.DateTime(2009, 9, 7, 13, 47, 33, 849);
            this.dateTimePickerFromDate.PopupClosed += new Syncfusion.Windows.Forms.PopupClosedEventHandler(this.dateTimePicker_PopupClosed);
            this.dateTimePickerFromDate.ValueChanged += new System.EventHandler(this.dateTimePickerFromDate_ValueChanged);
            this.dateTimePickerFromDate.BeforePopup += new System.EventHandler(this.dateTimePicker_BeforePopup);
            // 
            // dateTimePickerToDate
            // 
            this.dateTimePickerToDate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.dateTimePickerToDate.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
            this.dateTimePickerToDate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            // 
            // 
            // 
            this.dateTimePickerToDate.Calendar.AllowMultipleSelection = false;
            this.dateTimePickerToDate.Calendar.Culture = new System.Globalization.CultureInfo("sv-SE");
            this.dateTimePickerToDate.Calendar.DaysFont = new System.Drawing.Font("Verdana", 8F);
            this.dateTimePickerToDate.Calendar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dateTimePickerToDate.Calendar.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dateTimePickerToDate.Calendar.ForeColor = System.Drawing.SystemColors.ControlText;
            this.dateTimePickerToDate.Calendar.GridLines = Syncfusion.Windows.Forms.Grid.GridBorderStyle.None;
            this.dateTimePickerToDate.Calendar.HeaderEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.dateTimePickerToDate.Calendar.HeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.dateTimePickerToDate.Calendar.HeaderHeight = 20;
            this.dateTimePickerToDate.Calendar.HeaderStartColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252)))));
            this.dateTimePickerToDate.Calendar.HeadForeColor = System.Drawing.SystemColors.ControlText;
            this.dateTimePickerToDate.Calendar.HeadGradient = true;
            this.dateTimePickerToDate.Calendar.Location = new System.Drawing.Point(0, 0);
            this.dateTimePickerToDate.Calendar.Name = "monthCalendar";
            this.dateTimePickerToDate.Calendar.Office2007Theme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.dateTimePickerToDate.Calendar.ScrollButtonSize = new System.Drawing.Size(24, 24);
            this.dateTimePickerToDate.Calendar.SelectedDates = new System.DateTime[0];
            this.dateTimePickerToDate.Calendar.ShowWeekNumbers = true;
            this.dateTimePickerToDate.Calendar.Size = new System.Drawing.Size(206, 174);
            this.dateTimePickerToDate.Calendar.SizeToFit = true;
            this.dateTimePickerToDate.Calendar.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this.dateTimePickerToDate.Calendar.TabIndex = 0;
            this.dateTimePickerToDate.Calendar.ThemedEnabledGrid = true;
            this.dateTimePickerToDate.Calendar.WeekFont = new System.Drawing.Font("Verdana", 8F);
            this.dateTimePickerToDate.Calendar.WeekInterior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.PeachPuff, System.Drawing.Color.AntiqueWhite);
            // 
            // 
            // 
            this.dateTimePickerToDate.Calendar.NoneButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.dateTimePickerToDate.Calendar.NoneButton.Location = new System.Drawing.Point(134, 0);
            this.dateTimePickerToDate.Calendar.NoneButton.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.dateTimePickerToDate.Calendar.NoneButton.Size = new System.Drawing.Size(72, 20);
            this.dateTimePickerToDate.Calendar.NoneButton.Text = "xxNone";
            this.dateTimePickerToDate.Calendar.NoneButton.UseVisualStyle = true;
            this.dateTimePickerToDate.Calendar.NoneButton.Visible = false;
            // 
            // 
            // 
            this.dateTimePickerToDate.Calendar.TodayButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.dateTimePickerToDate.Calendar.TodayButton.Location = new System.Drawing.Point(0, 0);
            this.dateTimePickerToDate.Calendar.TodayButton.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.dateTimePickerToDate.Calendar.TodayButton.Size = new System.Drawing.Size(134, 20);
            this.dateTimePickerToDate.Calendar.TodayButton.Text = "xxToday";
            this.dateTimePickerToDate.Calendar.TodayButton.UseVisualStyle = true;
            this.dateTimePickerToDate.CalendarFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dateTimePickerToDate.CalendarTitleBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252)))));
            this.dateTimePickerToDate.CalendarTitleForeColor = System.Drawing.SystemColors.ControlText;
            this.dateTimePickerToDate.DropDownImage = null;
            this.dateTimePickerToDate.EnableNullDate = false;
            this.dateTimePickerToDate.EnableNullKeys = false;
            this.dateTimePickerToDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dateTimePickerToDate.Location = new System.Drawing.Point(84, 34);
            this.dateTimePickerToDate.MinValue = new System.DateTime(((long)(0)));
            this.dateTimePickerToDate.Name = "dateTimePickerToDate";
            this.dateTimePickerToDate.NoneButtonVisible = false;
            this.dateTimePickerToDate.Office2007Theme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.dateTimePickerToDate.ShowCheckBox = false;
            this.dateTimePickerToDate.Size = new System.Drawing.Size(95, 21);
            this.dateTimePickerToDate.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this.dateTimePickerToDate.TabIndex = 8;
            this.dateTimePickerToDate.UseCurrentCulture = false;
            this.dateTimePickerToDate.Value = new System.DateTime(2009, 9, 7, 13, 47, 33, 834);
            this.dateTimePickerToDate.PopupClosed += new Syncfusion.Windows.Forms.PopupClosedEventHandler(this.dateTimePicker_PopupClosed);
            this.dateTimePickerToDate.BeforePopup += new System.EventHandler(this.dateTimePicker_BeforePopup);
            // 
            // buttonAdvAddDates
            // 
            this.buttonAdvAddDates.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.buttonAdvAddDates.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonAdvAddDates.Location = new System.Drawing.Point(107, 63);
            this.buttonAdvAddDates.Name = "buttonAdvAddDates";
            this.buttonAdvAddDates.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.buttonAdvAddDates.Size = new System.Drawing.Size(72, 20);
            this.buttonAdvAddDates.TabIndex = 9;
            this.buttonAdvAddDates.Text = "xxOk";
            this.buttonAdvAddDates.UseVisualStyle = true;
            this.buttonAdvAddDates.UseVisualStyleBackColor = false;
            this.buttonAdvAddDates.Click += new System.EventHandler(this.buttonAdvAddDates_Click);
            // 
            // gradientPanelAddDates
            // 
            this.gradientPanelAddDates.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.gradientPanelAddDates.BorderColor = System.Drawing.Color.Black;
            this.gradientPanelAddDates.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.gradientPanelAddDates.Controls.Add(this.tableLayoutPanelAddDates);
            this.gradientPanelAddDates.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gradientPanelAddDates.Location = new System.Drawing.Point(0, 0);
            this.gradientPanelAddDates.Margin = new System.Windows.Forms.Padding(0);
            this.gradientPanelAddDates.Name = "gradientPanelAddDates";
            this.gradientPanelAddDates.Padding = new System.Windows.Forms.Padding(5);
            this.gradientPanelAddDates.Size = new System.Drawing.Size(194, 98);
            this.gradientPanelAddDates.TabIndex = 29;
            // 
            // AddDatesView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gradientPanelAddDates);
            this.Name = "AddDatesView";
            this.Size = new System.Drawing.Size(194, 98);
            this.tableLayoutPanelAddDates.ResumeLayout(false);
            this.tableLayoutPanelAddDates.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dateTimePickerFromDate.Calendar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateTimePickerFromDate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateTimePickerToDate.Calendar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateTimePickerToDate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanelAddDates)).EndInit();
            this.gradientPanelAddDates.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelAddDates;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private Syncfusion.Windows.Forms.Tools.DateTimePickerAdv dateTimePickerFromDate;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvAddDates;
        private Syncfusion.Windows.Forms.Tools.DateTimePickerAdv dateTimePickerToDate;
        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanelAddDates;
    }
}
