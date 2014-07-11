namespace Teleopti.Ccc.Win.Meetings.Overview
{
    partial class CalendarAndTextPanel
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
			this.gradientPanel1 = new Syncfusion.Windows.Forms.Tools.GradientPanel();
			this.textBox1 = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
			this.monthCalendarAdv1 = new Syncfusion.Windows.Forms.Tools.MonthCalendarAdv();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanel1)).BeginInit();
			this.gradientPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.textBox1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.monthCalendarAdv1)).BeginInit();
			this.SuspendLayout();
			// 
			// gradientPanel1
			// 
			this.gradientPanel1.Border3DStyle = System.Windows.Forms.Border3DStyle.RaisedInner;
			this.gradientPanel1.Controls.Add(this.textBox1);
			this.gradientPanel1.Controls.Add(this.monthCalendarAdv1);
			this.gradientPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gradientPanel1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.gradientPanel1.Location = new System.Drawing.Point(0, 0);
			this.gradientPanel1.Name = "gradientPanel1";
			this.gradientPanel1.Size = new System.Drawing.Size(199, 527);
			this.gradientPanel1.TabIndex = 2;
			// 
			// textBox1
			// 
			this.textBox1.BackColor = System.Drawing.Color.White;
			this.textBox1.BeforeTouchSize = new System.Drawing.Size(195, 340);
			this.textBox1.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.textBox1.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textBox1.Location = new System.Drawing.Point(0, 183);
			this.textBox1.Metrocolor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBox1.Multiline = true;
			this.textBox1.Name = "textBox1";
			this.textBox1.ReadOnly = true;
			this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.textBox1.Size = new System.Drawing.Size(195, 340);
			this.textBox1.Style = Syncfusion.Windows.Forms.Tools.TextBoxExt.theme.Metro;
			this.textBox1.TabIndex = 2;
			// 
			// monthCalendarAdv1
			// 
			this.monthCalendarAdv1.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.monthCalendarAdv1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.monthCalendarAdv1.BottomHeight = 25;
			this.monthCalendarAdv1.Culture = new System.Globalization.CultureInfo("sv-SE");
			this.monthCalendarAdv1.DayNamesColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(165)))), ((int)(((byte)(220)))));
			this.monthCalendarAdv1.DayNamesFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
			this.monthCalendarAdv1.DaysFont = new System.Drawing.Font("Verdana", 8F);
			this.monthCalendarAdv1.DaysHeaderInterior = new Syncfusion.Drawing.BrushInfo(System.Drawing.SystemColors.Window);
			this.monthCalendarAdv1.Dock = System.Windows.Forms.DockStyle.Top;
			this.monthCalendarAdv1.GridLines = Syncfusion.Windows.Forms.Grid.GridBorderStyle.None;
			this.monthCalendarAdv1.HeaderHeight = 34;
			this.monthCalendarAdv1.HeaderStartColor = System.Drawing.Color.White;
			this.monthCalendarAdv1.HighlightColor = System.Drawing.Color.Black;
			this.monthCalendarAdv1.Iso8601CalenderFormat = false;
			this.monthCalendarAdv1.Location = new System.Drawing.Point(0, 0);
			this.monthCalendarAdv1.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(165)))), ((int)(((byte)(220)))));
			this.monthCalendarAdv1.MouseDragMultiselect = true;
			this.monthCalendarAdv1.Name = "monthCalendarAdv1";
			this.monthCalendarAdv1.ScrollButtonSize = new System.Drawing.Size(24, 24);
			this.monthCalendarAdv1.SelectedDates = new System.DateTime[] {
        new System.DateTime(2014, 7, 11, 0, 0, 0, 0)};
			this.monthCalendarAdv1.ShowWeekNumbers = true;
			this.monthCalendarAdv1.Size = new System.Drawing.Size(195, 183);
			this.monthCalendarAdv1.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.monthCalendarAdv1.TabIndex = 1;
			this.monthCalendarAdv1.ThemedEnabledGrid = true;
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
			this.monthCalendarAdv1.NoneButton.Location = new System.Drawing.Point(123, 0);
			this.monthCalendarAdv1.NoneButton.Size = new System.Drawing.Size(72, 20);
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
			this.monthCalendarAdv1.TodayButton.Size = new System.Drawing.Size(195, 20);
			this.monthCalendarAdv1.TodayButton.Text = "Today";
			this.monthCalendarAdv1.TodayButton.UseVisualStyle = true;
			this.monthCalendarAdv1.TodayButton.Visible = false;
			this.monthCalendarAdv1.DateSelected += new System.EventHandler(this.MonthCalendarAdv1DateSelected);
			this.monthCalendarAdv1.DateChanged += new System.EventHandler(this.MonthCalendarAdv1DateChanged);
			// 
			// CalendarAndTextPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.gradientPanel1);
			this.Name = "CalendarAndTextPanel";
			this.Size = new System.Drawing.Size(199, 527);
			((System.ComponentModel.ISupportInitialize)(this.gradientPanel1)).EndInit();
			this.gradientPanel1.ResumeLayout(false);
			this.gradientPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.textBox1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.monthCalendarAdv1)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanel1;
        private Syncfusion.Windows.Forms.Tools.MonthCalendarAdv monthCalendarAdv1;
		  private Syncfusion.Windows.Forms.Tools.TextBoxExt textBox1;
    }
}
