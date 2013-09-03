namespace Teleopti.Ccc.AgentPortal.Common.Controls
{
    partial class OutlookDateTimePicker
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
            this.tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
            this._dateTimePickerAdv = new Syncfusion.Windows.Forms.Tools.DateTimePickerAdv();
            this.tableLayoutPanelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._dateTimePickerAdv)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._dateTimePickerAdv.Calendar)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanelMain
            // 
            this.tableLayoutPanelMain.ColumnCount = 3;
            this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 55.55556F));
            this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 44.44444F));
            this.tableLayoutPanelMain.Controls.Add(this._dateTimePickerAdv, 0, 0);
            this.tableLayoutPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelMain.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelMain.Name = "tableLayoutPanelMain";
            this.tableLayoutPanelMain.RowCount = 1;
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelMain.Size = new System.Drawing.Size(272, 23);
            this.tableLayoutPanelMain.TabIndex = 0;
            // 
            // _dateTimePickerAdv
            // 
            this._dateTimePickerAdv.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
            this._dateTimePickerAdv.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(193)))), ((int)(((byte)(222)))));
            this._dateTimePickerAdv.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            // 
            // 
            // 
            this._dateTimePickerAdv.Calendar.AllowMultipleSelection = false;
            this._dateTimePickerAdv.Calendar.Culture = new System.Globalization.CultureInfo("sv-SE");
            this._dateTimePickerAdv.Calendar.DaysFont = new System.Drawing.Font("Verdana", 8F);
            this._dateTimePickerAdv.Calendar.DaysHeaderInterior = new Syncfusion.Drawing.BrushInfo(System.Drawing.SystemColors.Window);
            this._dateTimePickerAdv.Calendar.Dock = System.Windows.Forms.DockStyle.Fill;
            this._dateTimePickerAdv.Calendar.Font = new System.Drawing.Font("Tahoma", 8F);
            this._dateTimePickerAdv.Calendar.ForeColor = System.Drawing.SystemColors.ControlText;
            this._dateTimePickerAdv.Calendar.GridLines = Syncfusion.Windows.Forms.Grid.GridBorderStyle.None;
            this._dateTimePickerAdv.Calendar.HeaderEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this._dateTimePickerAdv.Calendar.HeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this._dateTimePickerAdv.Calendar.HeaderHeight = 20;
            this._dateTimePickerAdv.Calendar.HeaderStartColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252)))));
            this._dateTimePickerAdv.Calendar.HeadForeColor = System.Drawing.SystemColors.ControlText;
            this._dateTimePickerAdv.Calendar.HeadGradient = true;
            this._dateTimePickerAdv.Calendar.Location = new System.Drawing.Point(0, 0);
            this._dateTimePickerAdv.Calendar.MinValue = new System.DateTime(1753, 1, 1, 0, 0, 0, 0);
            this._dateTimePickerAdv.Calendar.Name = "monthCalendar";
            this._dateTimePickerAdv.Calendar.ScrollButtonSize = new System.Drawing.Size(24, 24);
            this._dateTimePickerAdv.Calendar.SelectedDates = new System.DateTime[0];
            this._dateTimePickerAdv.Calendar.Size = new System.Drawing.Size(206, 174);
            this._dateTimePickerAdv.Calendar.SizeToFit = true;
            this._dateTimePickerAdv.Calendar.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this._dateTimePickerAdv.Calendar.TabIndex = 0;
            this._dateTimePickerAdv.Calendar.WeekFont = new System.Drawing.Font("Verdana", 8F);
            this._dateTimePickerAdv.Calendar.WeekInterior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.PeachPuff, System.Drawing.Color.AntiqueWhite);
            // 
            // 
            // 
            this._dateTimePickerAdv.Calendar.NoneButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this._dateTimePickerAdv.Calendar.NoneButton.Location = new System.Drawing.Point(134, 0);
            this._dateTimePickerAdv.Calendar.NoneButton.Size = new System.Drawing.Size(72, 20);
            this._dateTimePickerAdv.Calendar.NoneButton.Text = "xxNone";
            this._dateTimePickerAdv.Calendar.NoneButton.UseVisualStyle = true;
            // 
            // 
            // 
            this._dateTimePickerAdv.Calendar.TodayButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this._dateTimePickerAdv.Calendar.TodayButton.Location = new System.Drawing.Point(0, 0);
            this._dateTimePickerAdv.Calendar.TodayButton.Size = new System.Drawing.Size(134, 20);
            this._dateTimePickerAdv.Calendar.TodayButton.Text = "xxToday";
            this._dateTimePickerAdv.Calendar.TodayButton.UseVisualStyle = true;
            this._dateTimePickerAdv.CalendarFont = new System.Drawing.Font("Tahoma", 8F);
            this._dateTimePickerAdv.CalendarTitleBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252)))));
            this._dateTimePickerAdv.CalendarTitleForeColor = System.Drawing.SystemColors.ControlText;
            this._dateTimePickerAdv.Culture = new System.Globalization.CultureInfo("sv-SE");
            this._dateTimePickerAdv.Dock = System.Windows.Forms.DockStyle.Fill;
            this._dateTimePickerAdv.DropDownImage = null;
            this._dateTimePickerAdv.ForeColor = System.Drawing.SystemColors.ControlText;
            this._dateTimePickerAdv.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this._dateTimePickerAdv.Location = new System.Drawing.Point(0, 0);
            this._dateTimePickerAdv.Margin = new System.Windows.Forms.Padding(0);
            this._dateTimePickerAdv.MinValue = new System.DateTime(((long)(0)));
            this._dateTimePickerAdv.Name = "_dateTimePickerAdv";
            this._dateTimePickerAdv.NullString = "xxNoDateIsSelected";
            this._dateTimePickerAdv.ShowCheckBox = false;
            this._dateTimePickerAdv.Size = new System.Drawing.Size(140, 23);
            this._dateTimePickerAdv.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this._dateTimePickerAdv.TabIndex = 0;
            this._dateTimePickerAdv.ThemesEnabled = true;
            this._dateTimePickerAdv.UseCurrentCulture = true;
            this._dateTimePickerAdv.Value = new System.DateTime(2008, 3, 28, 14, 41, 50, 358);
            // 
            // OutlookDateTimePicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanelMain);
            this.Name = "OutlookDateTimePicker";
            this.Size = new System.Drawing.Size(272, 23);
            this.Load += new System.EventHandler(this.OutlookDateTimePicker_Load);
            this.tableLayoutPanelMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._dateTimePickerAdv.Calendar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._dateTimePickerAdv)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelMain;
    }
}
