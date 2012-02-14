namespace Teleopti.Ccc.Win.Common.Controls.DateSelection
{
    partial class DateNavigateControlThinLayout
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
            this.dateTimePickerAdv1 = new Syncfusion.Windows.Forms.Tools.DateTimePickerAdv();
            this.buttonAdv1 = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonAdv2 = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonAdv3 = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonAdv4 = new Syncfusion.Windows.Forms.ButtonAdv();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdv1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdv1.Calendar)).BeginInit();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // dateTimePickerAdv1
            // 
            this.dateTimePickerAdv1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.dateTimePickerAdv1.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
            this.dateTimePickerAdv1.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(193)))), ((int)(((byte)(222)))));
            this.dateTimePickerAdv1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            // 
            // 
            // 
            this.dateTimePickerAdv1.Calendar.AllowMultipleSelection = false;
            this.dateTimePickerAdv1.Calendar.Culture = new System.Globalization.CultureInfo("sv-SE");
            this.dateTimePickerAdv1.Calendar.DaysFont = new System.Drawing.Font("Verdana", 8F);
            this.dateTimePickerAdv1.Calendar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dateTimePickerAdv1.Calendar.Font = new System.Drawing.Font("Tahoma", 8F);
            this.dateTimePickerAdv1.Calendar.ForeColor = System.Drawing.SystemColors.ControlText;
            this.dateTimePickerAdv1.Calendar.GridLines = Syncfusion.Windows.Forms.Grid.GridBorderStyle.None;
            this.dateTimePickerAdv1.Calendar.HeaderEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.dateTimePickerAdv1.Calendar.HeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.dateTimePickerAdv1.Calendar.HeaderHeight = 20;
            this.dateTimePickerAdv1.Calendar.HeaderStartColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252)))));
            this.dateTimePickerAdv1.Calendar.HeadForeColor = System.Drawing.SystemColors.ControlText;
            this.dateTimePickerAdv1.Calendar.HeadGradient = true;
            this.dateTimePickerAdv1.Calendar.Location = new System.Drawing.Point(0, 0);
            this.dateTimePickerAdv1.Calendar.Name = "monthCalendar";
            this.dateTimePickerAdv1.Calendar.Office2007Theme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.dateTimePickerAdv1.Calendar.ScrollButtonSize = new System.Drawing.Size(24, 24);
            this.dateTimePickerAdv1.Calendar.SelectedDates = new System.DateTime[0];
            this.dateTimePickerAdv1.Calendar.Size = new System.Drawing.Size(206, 174);
            this.dateTimePickerAdv1.Calendar.SizeToFit = true;
            this.dateTimePickerAdv1.Calendar.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this.dateTimePickerAdv1.Calendar.TabIndex = 0;
            this.dateTimePickerAdv1.Calendar.ThemedEnabledGrid = true;
            this.dateTimePickerAdv1.Calendar.WeekFont = new System.Drawing.Font("Verdana", 8F);
            this.dateTimePickerAdv1.Calendar.WeekInterior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.PeachPuff, System.Drawing.Color.AntiqueWhite);
            // 
            // 
            // 
            this.dateTimePickerAdv1.Calendar.NoneButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.dateTimePickerAdv1.Calendar.NoneButton.Location = new System.Drawing.Point(134, 0);
            this.dateTimePickerAdv1.Calendar.NoneButton.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.dateTimePickerAdv1.Calendar.NoneButton.Size = new System.Drawing.Size(72, 20);
            this.dateTimePickerAdv1.Calendar.NoneButton.Text = "None";
            this.dateTimePickerAdv1.Calendar.NoneButton.UseVisualStyle = true;
            // 
            // 
            // 
            this.dateTimePickerAdv1.Calendar.TodayButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.dateTimePickerAdv1.Calendar.TodayButton.Location = new System.Drawing.Point(0, 0);
            this.dateTimePickerAdv1.Calendar.TodayButton.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.dateTimePickerAdv1.Calendar.TodayButton.Size = new System.Drawing.Size(134, 20);
            this.dateTimePickerAdv1.Calendar.TodayButton.Text = "Today";
            this.dateTimePickerAdv1.Calendar.TodayButton.UseVisualStyle = true;
            this.dateTimePickerAdv1.CalendarTitleBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252)))));
            this.dateTimePickerAdv1.CalendarTitleForeColor = System.Drawing.SystemColors.ControlText;
            this.dateTimePickerAdv1.Culture = new System.Globalization.CultureInfo("sv-SE");
            this.dateTimePickerAdv1.DropDownImage = null;
            this.dateTimePickerAdv1.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dateTimePickerAdv1.Location = new System.Drawing.Point(72, 2);
            this.dateTimePickerAdv1.Margin = new System.Windows.Forms.Padding(2);
            this.dateTimePickerAdv1.MinValue = new System.DateTime(((long)(0)));
            this.dateTimePickerAdv1.Name = "dateTimePickerAdv1";
            this.dateTimePickerAdv1.Office2007Theme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.dateTimePickerAdv1.ShowCheckBox = false;
            this.dateTimePickerAdv1.Size = new System.Drawing.Size(151, 20);
            this.dateTimePickerAdv1.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this.dateTimePickerAdv1.TabIndex = 0;
            this.dateTimePickerAdv1.ThemedChildControls = true;
            this.dateTimePickerAdv1.ThemesEnabled = true;
            this.dateTimePickerAdv1.UseCurrentCulture = true;
            this.dateTimePickerAdv1.Value = new System.DateTime(2008, 6, 9, 0, 0, 0, 0);
            // 
            // buttonAdv1
            // 
            this.buttonAdv1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonAdv1.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonAdv1.Location = new System.Drawing.Point(227, 2);
            this.buttonAdv1.Margin = new System.Windows.Forms.Padding(2);
            this.buttonAdv1.Name = "buttonAdv1";
            this.buttonAdv1.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.buttonAdv1.Size = new System.Drawing.Size(31, 20);
            this.buttonAdv1.TabIndex = 1;
            this.buttonAdv1.Text = ">";
            this.buttonAdv1.UseVisualStyle = true;
            this.buttonAdv1.Click += new System.EventHandler(this.ForwardDayButtonClicked);
            // 
            // buttonAdv2
            // 
            this.buttonAdv2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonAdv2.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonAdv2.Location = new System.Drawing.Point(262, 2);
            this.buttonAdv2.Margin = new System.Windows.Forms.Padding(2);
            this.buttonAdv2.Name = "buttonAdv2";
            this.buttonAdv2.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.buttonAdv2.Size = new System.Drawing.Size(31, 20);
            this.buttonAdv2.TabIndex = 2;
            this.buttonAdv2.Text = ">>";
            this.buttonAdv2.UseVisualStyle = true;
            this.buttonAdv2.Click += new System.EventHandler(this.ForwardWeekButtonClick);
            // 
            // buttonAdv3
            // 
            this.buttonAdv3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonAdv3.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonAdv3.Location = new System.Drawing.Point(37, 2);
            this.buttonAdv3.Margin = new System.Windows.Forms.Padding(2);
            this.buttonAdv3.Name = "buttonAdv3";
            this.buttonAdv3.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.buttonAdv3.Size = new System.Drawing.Size(31, 20);
            this.buttonAdv3.TabIndex = 3;
            this.buttonAdv3.Text = "<";
            this.buttonAdv3.UseVisualStyle = true;
            this.buttonAdv3.Click += new System.EventHandler(this.BackDayButtonClick);
            // 
            // buttonAdv4
            // 
            this.buttonAdv4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonAdv4.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonAdv4.Location = new System.Drawing.Point(2, 2);
            this.buttonAdv4.Margin = new System.Windows.Forms.Padding(2);
            this.buttonAdv4.Name = "buttonAdv4";
            this.buttonAdv4.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.buttonAdv4.Size = new System.Drawing.Size(31, 20);
            this.buttonAdv4.TabIndex = 4;
            this.buttonAdv4.Text = "<<";
            this.buttonAdv4.UseVisualStyle = true;
            this.buttonAdv4.Click += new System.EventHandler(this.BackwardWeekButtonClick);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 5;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.buttonAdv1, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.buttonAdv3, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.buttonAdv2, 4, 0);
            this.tableLayoutPanel2.Controls.Add(this.dateTimePickerAdv1, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.buttonAdv4, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(294, 24);
            this.tableLayoutPanel2.TabIndex = 6;
            // 
            // DateNavigateControlThinLayout
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.tableLayoutPanel2);
            this.Name = "DateNavigateControlThinLayout";
            this.Size = new System.Drawing.Size(294, 24);
            ((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdv1.Calendar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdv1)).EndInit();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.DateTimePickerAdv dateTimePickerAdv1;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdv1;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdv2;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdv3;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdv4;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
    }
}