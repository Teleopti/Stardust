namespace Teleopti.Ccc.AgentPortal.Common.Controls
{
    partial class DateSelectorView
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
            this.groupBoxSingelDateSelection = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanelAddDate = new System.Windows.Forms.TableLayoutPanel();
            this.label4 = new System.Windows.Forms.Label();
            this.dateTimePickerAdvCurrentSelectedDate = new Syncfusion.Windows.Forms.Tools.DateTimePickerAdv();
            this.buttonAdvAddDate = new Syncfusion.Windows.Forms.ButtonAdv();
            this.groupBoxMultipleDates = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanelAddDates = new System.Windows.Forms.TableLayoutPanel();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.dateTimePickerFromDate = new Syncfusion.Windows.Forms.Tools.DateTimePickerAdv();
            this.buttonAdvAddDates = new Syncfusion.Windows.Forms.ButtonAdv();
            this.dateTimePickerToDate = new Syncfusion.Windows.Forms.Tools.DateTimePickerAdv();
            this.tableLayoutPanelSelectedDates = new System.Windows.Forms.TableLayoutPanel();
            this.labelSelectDate = new System.Windows.Forms.Label();
            this.listBoxDates = new System.Windows.Forms.ListBox();
            this.buttonAdvDelete = new Syncfusion.Windows.Forms.ButtonAdv();
            this.groupBoxSingelDateSelection.SuspendLayout();
            this.tableLayoutPanelAddDate.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvCurrentSelectedDate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvCurrentSelectedDate.Calendar)).BeginInit();
            this.groupBoxMultipleDates.SuspendLayout();
            this.tableLayoutPanelAddDates.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dateTimePickerFromDate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateTimePickerFromDate.Calendar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateTimePickerToDate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateTimePickerToDate.Calendar)).BeginInit();
            this.tableLayoutPanelSelectedDates.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxSingelDateSelection
            // 
            this.groupBoxSingelDateSelection.Controls.Add(this.tableLayoutPanelAddDate);
            this.groupBoxSingelDateSelection.Location = new System.Drawing.Point(3, 3);
            this.groupBoxSingelDateSelection.Name = "groupBoxSingelDateSelection";
            this.groupBoxSingelDateSelection.Size = new System.Drawing.Size(404, 54);
            this.groupBoxSingelDateSelection.TabIndex = 26;
            this.groupBoxSingelDateSelection.TabStop = false;
            this.groupBoxSingelDateSelection.Text = "xxSelectSingleDate";
            // 
            // tableLayoutPanelAddDate
            // 
            this.tableLayoutPanelAddDate.ColumnCount = 3;
            this.tableLayoutPanelAddDate.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 27.62161F));
            this.tableLayoutPanelAddDate.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 52.26131F));
            this.tableLayoutPanelAddDate.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20.35176F));
            this.tableLayoutPanelAddDate.Controls.Add(this.label4, 0, 0);
            this.tableLayoutPanelAddDate.Controls.Add(this.dateTimePickerAdvCurrentSelectedDate, 1, 0);
            this.tableLayoutPanelAddDate.Controls.Add(this.buttonAdvAddDate, 2, 0);
            this.tableLayoutPanelAddDate.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanelAddDate.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanelAddDate.Name = "tableLayoutPanelAddDate";
            this.tableLayoutPanelAddDate.RowCount = 1;
            this.tableLayoutPanelAddDate.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelAddDate.Size = new System.Drawing.Size(398, 30);
            this.tableLayoutPanelAddDate.TabIndex = 26;
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 8);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(40, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "xxDate";
            // 
            // dateTimePickerAdvCurrentSelectedDate
            // 
            this.dateTimePickerAdvCurrentSelectedDate.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.dateTimePickerAdvCurrentSelectedDate.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
            this.dateTimePickerAdvCurrentSelectedDate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            // 
            // 
            // 
            this.dateTimePickerAdvCurrentSelectedDate.Calendar.AllowMultipleSelection = false;
            this.dateTimePickerAdvCurrentSelectedDate.Calendar.Culture = new System.Globalization.CultureInfo("sv-SE");
            this.dateTimePickerAdvCurrentSelectedDate.Calendar.DaysFont = new System.Drawing.Font("Verdana", 8F);
            this.dateTimePickerAdvCurrentSelectedDate.Calendar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dateTimePickerAdvCurrentSelectedDate.Calendar.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dateTimePickerAdvCurrentSelectedDate.Calendar.ForeColor = System.Drawing.SystemColors.ControlText;
            this.dateTimePickerAdvCurrentSelectedDate.Calendar.GridLines = Syncfusion.Windows.Forms.Grid.GridBorderStyle.None;
            this.dateTimePickerAdvCurrentSelectedDate.Calendar.HeaderEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.dateTimePickerAdvCurrentSelectedDate.Calendar.HeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.dateTimePickerAdvCurrentSelectedDate.Calendar.HeaderHeight = 20;
            this.dateTimePickerAdvCurrentSelectedDate.Calendar.HeaderStartColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252)))));
            this.dateTimePickerAdvCurrentSelectedDate.Calendar.HeadForeColor = System.Drawing.SystemColors.ControlText;
            this.dateTimePickerAdvCurrentSelectedDate.Calendar.HeadGradient = true;
            this.dateTimePickerAdvCurrentSelectedDate.Calendar.Location = new System.Drawing.Point(0, 0);
            this.dateTimePickerAdvCurrentSelectedDate.Calendar.Name = "monthCalendar";
            this.dateTimePickerAdvCurrentSelectedDate.Calendar.Office2007Theme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.dateTimePickerAdvCurrentSelectedDate.Calendar.ScrollButtonSize = new System.Drawing.Size(24, 24);
            this.dateTimePickerAdvCurrentSelectedDate.Calendar.SelectedDates = new System.DateTime[0];
            this.dateTimePickerAdvCurrentSelectedDate.Calendar.ShowWeekNumbers = true;
            this.dateTimePickerAdvCurrentSelectedDate.Calendar.Size = new System.Drawing.Size(206, 174);
            this.dateTimePickerAdvCurrentSelectedDate.Calendar.SizeToFit = true;
            this.dateTimePickerAdvCurrentSelectedDate.Calendar.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this.dateTimePickerAdvCurrentSelectedDate.Calendar.TabIndex = 0;
            this.dateTimePickerAdvCurrentSelectedDate.Calendar.ThemedEnabledGrid = true;
            this.dateTimePickerAdvCurrentSelectedDate.Calendar.WeekFont = new System.Drawing.Font("Verdana", 8F);
            this.dateTimePickerAdvCurrentSelectedDate.Calendar.WeekInterior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.PeachPuff, System.Drawing.Color.AntiqueWhite);
            // 
            // 
            // 
            this.dateTimePickerAdvCurrentSelectedDate.Calendar.NoneButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.dateTimePickerAdvCurrentSelectedDate.Calendar.NoneButton.Location = new System.Drawing.Point(134, 0);
            this.dateTimePickerAdvCurrentSelectedDate.Calendar.NoneButton.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.dateTimePickerAdvCurrentSelectedDate.Calendar.NoneButton.Size = new System.Drawing.Size(72, 20);
            this.dateTimePickerAdvCurrentSelectedDate.Calendar.NoneButton.Text = "None";
            this.dateTimePickerAdvCurrentSelectedDate.Calendar.NoneButton.UseVisualStyle = true;
            // 
            // 
            // 
            this.dateTimePickerAdvCurrentSelectedDate.Calendar.TodayButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.dateTimePickerAdvCurrentSelectedDate.Calendar.TodayButton.Location = new System.Drawing.Point(0, 0);
            this.dateTimePickerAdvCurrentSelectedDate.Calendar.TodayButton.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.dateTimePickerAdvCurrentSelectedDate.Calendar.TodayButton.Size = new System.Drawing.Size(134, 20);
            this.dateTimePickerAdvCurrentSelectedDate.Calendar.TodayButton.Text = "Today";
            this.dateTimePickerAdvCurrentSelectedDate.Calendar.TodayButton.UseVisualStyle = true;
            this.dateTimePickerAdvCurrentSelectedDate.CalendarFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dateTimePickerAdvCurrentSelectedDate.CalendarTitleBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252)))));
            this.dateTimePickerAdvCurrentSelectedDate.CalendarTitleForeColor = System.Drawing.SystemColors.ControlText;
            this.dateTimePickerAdvCurrentSelectedDate.Culture = new System.Globalization.CultureInfo("sv-SE");
            this.dateTimePickerAdvCurrentSelectedDate.DropDownImage = null;
            this.dateTimePickerAdvCurrentSelectedDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dateTimePickerAdvCurrentSelectedDate.Location = new System.Drawing.Point(112, 5);
            this.dateTimePickerAdvCurrentSelectedDate.MinValue = new System.DateTime(((long)(0)));
            this.dateTimePickerAdvCurrentSelectedDate.Name = "dateTimePickerAdvCurrentSelectedDate";
            this.dateTimePickerAdvCurrentSelectedDate.Office2007Theme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.dateTimePickerAdvCurrentSelectedDate.ShowCheckBox = false;
            this.dateTimePickerAdvCurrentSelectedDate.Size = new System.Drawing.Size(201, 20);
            this.dateTimePickerAdvCurrentSelectedDate.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this.dateTimePickerAdvCurrentSelectedDate.TabIndex = 7;
            this.dateTimePickerAdvCurrentSelectedDate.UseCurrentCulture = true;
            this.dateTimePickerAdvCurrentSelectedDate.Value = new System.DateTime(2009, 9, 4, 14, 25, 29, 261);
            // 
            // buttonAdvAddDate
            // 
            this.buttonAdvAddDate.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.buttonAdvAddDate.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonAdvAddDate.Location = new System.Drawing.Point(319, 4);
            this.buttonAdvAddDate.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
            this.buttonAdvAddDate.Name = "buttonAdvAddDate";
            this.buttonAdvAddDate.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.buttonAdvAddDate.Size = new System.Drawing.Size(73, 22);
            this.buttonAdvAddDate.TabIndex = 9;
            this.buttonAdvAddDate.Text = "xxAddDate";
            this.buttonAdvAddDate.UseVisualStyle = true;
            this.buttonAdvAddDate.UseVisualStyleBackColor = false;
            this.buttonAdvAddDate.Click += new System.EventHandler(this.buttonAdvAddDate_Click);
            // 
            // groupBoxMultipleDates
            // 
            this.groupBoxMultipleDates.Controls.Add(this.tableLayoutPanelAddDates);
            this.groupBoxMultipleDates.Location = new System.Drawing.Point(3, 63);
            this.groupBoxMultipleDates.Name = "groupBoxMultipleDates";
            this.groupBoxMultipleDates.Size = new System.Drawing.Size(404, 85);
            this.groupBoxMultipleDates.TabIndex = 27;
            this.groupBoxMultipleDates.TabStop = false;
            this.groupBoxMultipleDates.Text = "xxSelectMultipleDates";
            // 
            // tableLayoutPanelAddDates
            // 
            this.tableLayoutPanelAddDates.ColumnCount = 3;
            this.tableLayoutPanelAddDates.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 27.47525F));
            this.tableLayoutPanelAddDates.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 52.26131F));
            this.tableLayoutPanelAddDates.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20.35176F));
            this.tableLayoutPanelAddDates.Controls.Add(this.label6, 0, 0);
            this.tableLayoutPanelAddDates.Controls.Add(this.label7, 0, 1);
            this.tableLayoutPanelAddDates.Controls.Add(this.dateTimePickerFromDate, 1, 0);
            this.tableLayoutPanelAddDates.Controls.Add(this.buttonAdvAddDates, 2, 1);
            this.tableLayoutPanelAddDates.Controls.Add(this.dateTimePickerToDate, 1, 1);
            this.tableLayoutPanelAddDates.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanelAddDates.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanelAddDates.Name = "tableLayoutPanelAddDates";
            this.tableLayoutPanelAddDates.RowCount = 2;
            this.tableLayoutPanelAddDates.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelAddDates.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelAddDates.Size = new System.Drawing.Size(398, 60);
            this.tableLayoutPanelAddDates.TabIndex = 28;
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 8);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(90, 13);
            this.label6.TabIndex = 6;
            this.label6.Text = "xxDateFromColon";
            // 
            // label7
            // 
            this.label7.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 38);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(80, 13);
            this.label7.TabIndex = 6;
            this.label7.Text = "xxDateToColon";
            // 
            // dateTimePickerFromDate
            // 
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
            this.dateTimePickerFromDate.Culture = new System.Globalization.CultureInfo("sv-SE");
            this.dateTimePickerFromDate.DropDownImage = null;
            this.dateTimePickerFromDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dateTimePickerFromDate.Location = new System.Drawing.Point(112, 3);
            this.dateTimePickerFromDate.MinValue = new System.DateTime(((long)(0)));
            this.dateTimePickerFromDate.Name = "dateTimePickerFromDate";
            this.dateTimePickerFromDate.Office2007Theme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.dateTimePickerFromDate.ShowCheckBox = false;
            this.dateTimePickerFromDate.Size = new System.Drawing.Size(201, 21);
            this.dateTimePickerFromDate.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this.dateTimePickerFromDate.TabIndex = 7;
            this.dateTimePickerFromDate.UseCurrentCulture = true;
            this.dateTimePickerFromDate.Value = new System.DateTime(2009, 9, 7, 13, 47, 33, 849);
            // 
            // buttonAdvAddDates
            // 
            this.buttonAdvAddDates.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonAdvAddDates.Location = new System.Drawing.Point(319, 33);
            this.buttonAdvAddDates.Name = "buttonAdvAddDates";
            this.buttonAdvAddDates.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.buttonAdvAddDates.Size = new System.Drawing.Size(72, 21);
            this.buttonAdvAddDates.TabIndex = 9;
            this.buttonAdvAddDates.Text = "xxAddDates";
            this.buttonAdvAddDates.UseVisualStyle = true;
            this.buttonAdvAddDates.UseVisualStyleBackColor = false;
            this.buttonAdvAddDates.Click += new System.EventHandler(this.buttonAdvAddDates_Click);
            // 
            // dateTimePickerToDate
            // 
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
            this.dateTimePickerToDate.Culture = new System.Globalization.CultureInfo("sv-SE");
            this.dateTimePickerToDate.DropDownImage = null;
            this.dateTimePickerToDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dateTimePickerToDate.Location = new System.Drawing.Point(112, 33);
            this.dateTimePickerToDate.MinValue = new System.DateTime(((long)(0)));
            this.dateTimePickerToDate.Name = "dateTimePickerToDate";
            this.dateTimePickerToDate.Office2007Theme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.dateTimePickerToDate.ShowCheckBox = false;
            this.dateTimePickerToDate.Size = new System.Drawing.Size(201, 21);
            this.dateTimePickerToDate.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this.dateTimePickerToDate.TabIndex = 8;
            this.dateTimePickerToDate.UseCurrentCulture = true;
            this.dateTimePickerToDate.Value = new System.DateTime(2009, 9, 7, 13, 47, 33, 834);
            // 
            // tableLayoutPanelSelectedDates
            // 
            this.tableLayoutPanelSelectedDates.ColumnCount = 3;
            this.tableLayoutPanelSelectedDates.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 27.47525F));
            this.tableLayoutPanelSelectedDates.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 52.26131F));
            this.tableLayoutPanelSelectedDates.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20.35176F));
            this.tableLayoutPanelSelectedDates.Controls.Add(this.labelSelectDate, 0, 0);
            this.tableLayoutPanelSelectedDates.Controls.Add(this.listBoxDates, 1, 0);
            this.tableLayoutPanelSelectedDates.Controls.Add(this.buttonAdvDelete, 2, 0);
            this.tableLayoutPanelSelectedDates.Location = new System.Drawing.Point(6, 154);
            this.tableLayoutPanelSelectedDates.Name = "tableLayoutPanelSelectedDates";
            this.tableLayoutPanelSelectedDates.RowCount = 1;
            this.tableLayoutPanelSelectedDates.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelSelectedDates.Size = new System.Drawing.Size(398, 91);
            this.tableLayoutPanelSelectedDates.TabIndex = 30;
            // 
            // labelSelectDate
            // 
            this.labelSelectDate.AutoSize = true;
            this.labelSelectDate.Location = new System.Drawing.Point(3, 6);
            this.labelSelectDate.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this.labelSelectDate.Name = "labelSelectDate";
            this.labelSelectDate.Size = new System.Drawing.Size(98, 26);
            this.labelSelectDate.TabIndex = 6;
            this.labelSelectDate.Text = "xxSelectedDateParenthesisS";
            // 
            // listBoxDates
            // 
            this.listBoxDates.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxDates.FormattingEnabled = true;
            this.listBoxDates.Location = new System.Drawing.Point(112, 6);
            this.listBoxDates.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this.listBoxDates.Name = "listBoxDates";
            this.listBoxDates.Size = new System.Drawing.Size(201, 82);
            this.listBoxDates.Sorted = true;
            this.listBoxDates.TabIndex = 10;
            // 
            // buttonAdvDelete
            // 
            this.buttonAdvDelete.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonAdvDelete.Location = new System.Drawing.Point(319, 6);
            this.buttonAdvDelete.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this.buttonAdvDelete.Name = "buttonAdvDelete";
            this.buttonAdvDelete.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.buttonAdvDelete.Size = new System.Drawing.Size(72, 21);
            this.buttonAdvDelete.TabIndex = 11;
            this.buttonAdvDelete.Text = "xxDeleteDateParenthesisS";
            this.buttonAdvDelete.UseVisualStyle = true;
            this.buttonAdvDelete.UseVisualStyleBackColor = false;
            this.buttonAdvDelete.Click += new System.EventHandler(this.buttonAdvDelete_Click);
            // 
            // DateSelectorView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.Controls.Add(this.tableLayoutPanelSelectedDates);
            this.Controls.Add(this.groupBoxMultipleDates);
            this.Controls.Add(this.groupBoxSingelDateSelection);
            this.Name = "DateSelectorView";
            this.Padding = new System.Windows.Forms.Padding(3);
            this.Size = new System.Drawing.Size(407, 250);
            this.Load += new System.EventHandler(this.DateSelectorView_Load);
            this.groupBoxSingelDateSelection.ResumeLayout(false);
            this.tableLayoutPanelAddDate.ResumeLayout(false);
            this.tableLayoutPanelAddDate.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvCurrentSelectedDate.Calendar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvCurrentSelectedDate)).EndInit();
            this.groupBoxMultipleDates.ResumeLayout(false);
            this.tableLayoutPanelAddDates.ResumeLayout(false);
            this.tableLayoutPanelAddDates.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dateTimePickerFromDate.Calendar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateTimePickerFromDate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateTimePickerToDate.Calendar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateTimePickerToDate)).EndInit();
            this.tableLayoutPanelSelectedDates.ResumeLayout(false);
            this.tableLayoutPanelSelectedDates.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxSingelDateSelection;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelAddDate;
        private System.Windows.Forms.Label label4;
        private Syncfusion.Windows.Forms.Tools.DateTimePickerAdv dateTimePickerAdvCurrentSelectedDate;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvAddDate;
        private System.Windows.Forms.GroupBox groupBoxMultipleDates;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelAddDates;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private Syncfusion.Windows.Forms.Tools.DateTimePickerAdv dateTimePickerFromDate;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvAddDates;
        private Syncfusion.Windows.Forms.Tools.DateTimePickerAdv dateTimePickerToDate;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelSelectedDates;
        private System.Windows.Forms.Label labelSelectDate;
        private System.Windows.Forms.ListBox listBoxDates;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvDelete;



    }
}
