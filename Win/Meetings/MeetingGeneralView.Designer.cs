using System;

namespace Teleopti.Ccc.Win.Meetings
{
    partial class MeetingGeneralView
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
                if(_presenter != null)
                    _presenter.Dispose();
            	_presenter = null;
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
			this.tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanelBottomMain = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.textBoxExtDescription = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.comboBoxAdvActivity = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.autoLabelActivity = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.autoLabelEndTimeFomat = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.autoLabelStartTimeFomat = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.dateTimePickerAdvStartDate = new Syncfusion.Windows.Forms.Tools.DateTimePickerAdv();
			this.autoLabel3 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.autoLabel4 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.dateTimePickerAdvEndDate = new Syncfusion.Windows.Forms.Tools.DateTimePickerAdv();
			this.outlookTimePickerStartTime = new Teleopti.Ccc.Win.Common.Controls.Office2007OutlookTimePicker(this.components);
			this.outlookTimePickerEndTime = new Teleopti.Ccc.Win.Common.Controls.Office2007OutlookTimePicker(this.components);
			this.tableLayoutPanelTopMain = new System.Windows.Forms.TableLayoutPanel();
			this.textBoxExtLocation = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
			this.autoLabel2 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.textBoxExtSubject = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
			this.autoLabel1 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.buttonAdvParticipants = new Syncfusion.Windows.Forms.ButtonAdv();
			this.textBoxExtParticipant = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
			this.tableLayoutPanelMain.SuspendLayout();
			this.tableLayoutPanelBottomMain.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.textBoxExtDescription)).BeginInit();
			this.tableLayoutPanel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvActivity)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvStartDate)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvStartDate.Calendar)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvEndDate)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvEndDate.Calendar)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerStartTime)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerEndTime)).BeginInit();
			this.tableLayoutPanelTopMain.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.textBoxExtLocation)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.textBoxExtSubject)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.textBoxExtParticipant)).BeginInit();
			this.SuspendLayout();
			// 
			// tableLayoutPanelMain
			// 
			this.tableLayoutPanelMain.AutoSize = true;
			this.tableLayoutPanelMain.ColumnCount = 1;
			this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelMain.Controls.Add(this.tableLayoutPanelBottomMain, 0, 1);
			this.tableLayoutPanelMain.Controls.Add(this.tableLayoutPanelTopMain, 0, 0);
			this.tableLayoutPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelMain.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanelMain.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanelMain.Name = "tableLayoutPanelMain";
			this.tableLayoutPanelMain.RowCount = 2;
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 104F));
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelMain.Size = new System.Drawing.Size(993, 667);
			this.tableLayoutPanelMain.TabIndex = 0;
			// 
			// tableLayoutPanelBottomMain
			// 
			this.tableLayoutPanelBottomMain.AutoSize = true;
			this.tableLayoutPanelBottomMain.ColumnCount = 1;
			this.tableLayoutPanelBottomMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelBottomMain.Controls.Add(this.tableLayoutPanel1, 0, 0);
			this.tableLayoutPanelBottomMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelBottomMain.Location = new System.Drawing.Point(0, 104);
			this.tableLayoutPanelBottomMain.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanelBottomMain.Name = "tableLayoutPanelBottomMain";
			this.tableLayoutPanelBottomMain.RowCount = 1;
			this.tableLayoutPanelBottomMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelBottomMain.Size = new System.Drawing.Size(993, 563);
			this.tableLayoutPanelBottomMain.TabIndex = 0;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.AutoSize = true;
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.textBoxExtDescription, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(993, 563);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// textBoxExtDescription
			// 
			this.textBoxExtDescription.BackColor = System.Drawing.Color.White;
			this.textBoxExtDescription.BeforeTouchSize = new System.Drawing.Size(886, 23);
			this.textBoxExtDescription.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxExtDescription.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textBoxExtDescription.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.textBoxExtDescription.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textBoxExtDescription.Location = new System.Drawing.Point(3, 63);
			this.textBoxExtDescription.MaxLength = 1800;
			this.textBoxExtDescription.Metrocolor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxExtDescription.Multiline = true;
			this.textBoxExtDescription.Name = "textBoxExtDescription";
			this.textBoxExtDescription.OverflowIndicatorToolTipText = null;
			this.textBoxExtDescription.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.textBoxExtDescription.Size = new System.Drawing.Size(987, 497);
			this.textBoxExtDescription.Style = Syncfusion.Windows.Forms.Tools.TextBoxExt.theme.Metro;
			this.textBoxExtDescription.TabIndex = 8;
			this.textBoxExtDescription.TextChanged += new System.EventHandler(this.textBoxExtDescriptionTextChanged);
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.AutoSize = true;
			this.tableLayoutPanel2.ColumnCount = 5;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 175F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 117F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Controls.Add(this.comboBoxAdvActivity, 4, 1);
			this.tableLayoutPanel2.Controls.Add(this.autoLabelActivity, 4, 0);
			this.tableLayoutPanel2.Controls.Add(this.autoLabelEndTimeFomat, 3, 1);
			this.tableLayoutPanel2.Controls.Add(this.autoLabelStartTimeFomat, 3, 0);
			this.tableLayoutPanel2.Controls.Add(this.dateTimePickerAdvStartDate, 1, 0);
			this.tableLayoutPanel2.Controls.Add(this.autoLabel3, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.autoLabel4, 0, 1);
			this.tableLayoutPanel2.Controls.Add(this.dateTimePickerAdvEndDate, 1, 1);
			this.tableLayoutPanel2.Controls.Add(this.outlookTimePickerStartTime, 2, 0);
			this.tableLayoutPanel2.Controls.Add(this.outlookTimePickerEndTime, 2, 1);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 2;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.Size = new System.Drawing.Size(993, 60);
			this.tableLayoutPanel2.TabIndex = 0;
			// 
			// comboBoxAdvActivity
			// 
			this.comboBoxAdvActivity.BackColor = System.Drawing.Color.White;
			this.comboBoxAdvActivity.BeforeTouchSize = new System.Drawing.Size(175, 23);
			this.comboBoxAdvActivity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAdvActivity.Location = new System.Drawing.Point(488, 33);
			this.comboBoxAdvActivity.Name = "comboBoxAdvActivity";
			this.comboBoxAdvActivity.Size = new System.Drawing.Size(175, 23);
			this.comboBoxAdvActivity.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.comboBoxAdvActivity.TabIndex = 7;
			// 
			// autoLabelActivity
			// 
			this.autoLabelActivity.Dock = System.Windows.Forms.DockStyle.Left;
			this.autoLabelActivity.Location = new System.Drawing.Point(488, 0);
			this.autoLabelActivity.Name = "autoLabelActivity";
			this.autoLabelActivity.Size = new System.Drawing.Size(89, 30);
			this.autoLabelActivity.TabIndex = 0;
			this.autoLabelActivity.Text = "xxActivityColon";
			this.autoLabelActivity.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// autoLabelEndTimeFomat
			// 
			this.autoLabelEndTimeFomat.Dock = System.Windows.Forms.DockStyle.Fill;
			this.autoLabelEndTimeFomat.Location = new System.Drawing.Point(395, 30);
			this.autoLabelEndTimeFomat.Name = "autoLabelEndTimeFomat";
			this.autoLabelEndTimeFomat.Size = new System.Drawing.Size(87, 30);
			this.autoLabelEndTimeFomat.TabIndex = 0;
			this.autoLabelEndTimeFomat.Text = "xxHHcolonMM";
			this.autoLabelEndTimeFomat.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// autoLabelStartTimeFomat
			// 
			this.autoLabelStartTimeFomat.Dock = System.Windows.Forms.DockStyle.Fill;
			this.autoLabelStartTimeFomat.Location = new System.Drawing.Point(395, 0);
			this.autoLabelStartTimeFomat.Name = "autoLabelStartTimeFomat";
			this.autoLabelStartTimeFomat.Size = new System.Drawing.Size(87, 30);
			this.autoLabelStartTimeFomat.TabIndex = 0;
			this.autoLabelStartTimeFomat.Text = "xxHHcolonMM";
			this.autoLabelStartTimeFomat.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// dateTimePickerAdvStartDate
			// 
			this.dateTimePickerAdvStartDate.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
			this.dateTimePickerAdvStartDate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			// 
			// 
			// 
			this.dateTimePickerAdvStartDate.Calendar.AllowMultipleSelection = false;
			this.dateTimePickerAdvStartDate.Calendar.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.dateTimePickerAdvStartDate.Calendar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.dateTimePickerAdvStartDate.Calendar.BottomHeight = 25;
			this.dateTimePickerAdvStartDate.Calendar.Culture = new System.Globalization.CultureInfo("sv-SE");
			this.dateTimePickerAdvStartDate.Calendar.DayNamesColor = System.Drawing.Color.Empty;
			this.dateTimePickerAdvStartDate.Calendar.DayNamesFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
			this.dateTimePickerAdvStartDate.Calendar.DaysFont = new System.Drawing.Font("Verdana", 8F);
			this.dateTimePickerAdvStartDate.Calendar.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dateTimePickerAdvStartDate.Calendar.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dateTimePickerAdvStartDate.Calendar.ForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvStartDate.Calendar.GridLines = Syncfusion.Windows.Forms.Grid.GridBorderStyle.None;
			this.dateTimePickerAdvStartDate.Calendar.HeaderEndColor = System.Drawing.Color.White;
			this.dateTimePickerAdvStartDate.Calendar.HeaderStartColor = System.Drawing.Color.White;
			this.dateTimePickerAdvStartDate.Calendar.HeadForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvStartDate.Calendar.HighlightColor = System.Drawing.Color.White;
			this.dateTimePickerAdvStartDate.Calendar.Iso8601CalenderFormat = false;
			this.dateTimePickerAdvStartDate.Calendar.Location = new System.Drawing.Point(0, 0);
			this.dateTimePickerAdvStartDate.Calendar.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvStartDate.Calendar.Name = "monthCalendar";
			this.dateTimePickerAdvStartDate.Calendar.ScrollButtonSize = new System.Drawing.Size(24, 24);
			this.dateTimePickerAdvStartDate.Calendar.SelectedDates = new System.DateTime[0];
			this.dateTimePickerAdvStartDate.Calendar.Size = new System.Drawing.Size(167, 174);
			this.dateTimePickerAdvStartDate.Calendar.SizeToFit = true;
			this.dateTimePickerAdvStartDate.Calendar.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.dateTimePickerAdvStartDate.Calendar.TabIndex = 0;
			this.dateTimePickerAdvStartDate.Calendar.ThemedEnabledScrollButtons = false;
			this.dateTimePickerAdvStartDate.Calendar.WeekFont = new System.Drawing.Font("Verdana", 8F);
			this.dateTimePickerAdvStartDate.Calendar.WeekInterior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.PeachPuff, System.Drawing.Color.AntiqueWhite);
			// 
			// 
			// 
			this.dateTimePickerAdvStartDate.Calendar.NoneButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.dateTimePickerAdvStartDate.Calendar.NoneButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvStartDate.Calendar.NoneButton.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.dateTimePickerAdvStartDate.Calendar.NoneButton.ForeColor = System.Drawing.Color.White;
			this.dateTimePickerAdvStartDate.Calendar.NoneButton.IsBackStageButton = false;
			this.dateTimePickerAdvStartDate.Calendar.NoneButton.Location = new System.Drawing.Point(125, 0);
			this.dateTimePickerAdvStartDate.Calendar.NoneButton.Size = new System.Drawing.Size(84, 25);
			this.dateTimePickerAdvStartDate.Calendar.NoneButton.Text = "None";
			this.dateTimePickerAdvStartDate.Calendar.NoneButton.UseVisualStyle = true;
			this.dateTimePickerAdvStartDate.Calendar.NoneButton.Visible = false;
			// 
			// 
			// 
			this.dateTimePickerAdvStartDate.Calendar.TodayButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.dateTimePickerAdvStartDate.Calendar.TodayButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvStartDate.Calendar.TodayButton.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.dateTimePickerAdvStartDate.Calendar.TodayButton.ForeColor = System.Drawing.Color.White;
			this.dateTimePickerAdvStartDate.Calendar.TodayButton.IsBackStageButton = false;
			this.dateTimePickerAdvStartDate.Calendar.TodayButton.Location = new System.Drawing.Point(0, 0);
			this.dateTimePickerAdvStartDate.Calendar.TodayButton.Size = new System.Drawing.Size(167, 25);
			this.dateTimePickerAdvStartDate.Calendar.TodayButton.Text = "Today";
			this.dateTimePickerAdvStartDate.Calendar.TodayButton.UseVisualStyle = true;
			this.dateTimePickerAdvStartDate.CalendarFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dateTimePickerAdvStartDate.CalendarSize = new System.Drawing.Size(189, 176);
			this.dateTimePickerAdvStartDate.CalendarTitleForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvStartDate.ClipboardFormat = System.Windows.Forms.DateTimePickerFormat.Short;
			this.dateTimePickerAdvStartDate.Culture = new System.Globalization.CultureInfo("sv-SE");
			this.dateTimePickerAdvStartDate.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dateTimePickerAdvStartDate.DropDownImage = null;
			this.dateTimePickerAdvStartDate.DropDownNormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvStartDate.DropDownPressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvStartDate.DropDownSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(179)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvStartDate.EnableNullDate = false;
			this.dateTimePickerAdvStartDate.EnableNullKeys = false;
			this.dateTimePickerAdvStartDate.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			this.dateTimePickerAdvStartDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
			this.dateTimePickerAdvStartDate.Location = new System.Drawing.Point(103, 3);
			this.dateTimePickerAdvStartDate.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvStartDate.MinValue = new System.DateTime(((long)(0)));
			this.dateTimePickerAdvStartDate.Name = "dateTimePickerAdvStartDate";
			this.dateTimePickerAdvStartDate.ShowCheckBox = false;
			this.dateTimePickerAdvStartDate.Size = new System.Drawing.Size(169, 24);
			this.dateTimePickerAdvStartDate.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.dateTimePickerAdvStartDate.TabIndex = 4;
			this.dateTimePickerAdvStartDate.ThemesEnabled = true;
			this.dateTimePickerAdvStartDate.Value = new System.DateTime(2008, 8, 5, 8, 1, 24, 984);
			this.dateTimePickerAdvStartDate.PopupClosed += new Syncfusion.Windows.Forms.PopupClosedEventHandler(this.dateTimePickerAdvStartDatePopupClosed);
			// 
			// autoLabel3
			// 
			this.autoLabel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.autoLabel3.Location = new System.Drawing.Point(3, 0);
			this.autoLabel3.Name = "autoLabel3";
			this.autoLabel3.Size = new System.Drawing.Size(94, 30);
			this.autoLabel3.TabIndex = 0;
			this.autoLabel3.Text = "xxStartTimeColon";
			this.autoLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// autoLabel4
			// 
			this.autoLabel4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.autoLabel4.Location = new System.Drawing.Point(3, 30);
			this.autoLabel4.Name = "autoLabel4";
			this.autoLabel4.Size = new System.Drawing.Size(94, 30);
			this.autoLabel4.TabIndex = 0;
			this.autoLabel4.Text = "xxEndTimeColon";
			this.autoLabel4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// dateTimePickerAdvEndDate
			// 
			this.dateTimePickerAdvEndDate.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
			this.dateTimePickerAdvEndDate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			// 
			// 
			// 
			this.dateTimePickerAdvEndDate.Calendar.AllowMultipleSelection = false;
			this.dateTimePickerAdvEndDate.Calendar.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.dateTimePickerAdvEndDate.Calendar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.dateTimePickerAdvEndDate.Calendar.BottomHeight = 25;
			this.dateTimePickerAdvEndDate.Calendar.Culture = new System.Globalization.CultureInfo("sv-SE");
			this.dateTimePickerAdvEndDate.Calendar.DayNamesColor = System.Drawing.Color.Empty;
			this.dateTimePickerAdvEndDate.Calendar.DayNamesFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
			this.dateTimePickerAdvEndDate.Calendar.DaysFont = new System.Drawing.Font("Verdana", 8F);
			this.dateTimePickerAdvEndDate.Calendar.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dateTimePickerAdvEndDate.Calendar.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dateTimePickerAdvEndDate.Calendar.ForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvEndDate.Calendar.GridLines = Syncfusion.Windows.Forms.Grid.GridBorderStyle.None;
			this.dateTimePickerAdvEndDate.Calendar.HeaderEndColor = System.Drawing.Color.White;
			this.dateTimePickerAdvEndDate.Calendar.HeaderStartColor = System.Drawing.Color.White;
			this.dateTimePickerAdvEndDate.Calendar.HeadForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvEndDate.Calendar.HighlightColor = System.Drawing.Color.White;
			this.dateTimePickerAdvEndDate.Calendar.Iso8601CalenderFormat = false;
			this.dateTimePickerAdvEndDate.Calendar.Location = new System.Drawing.Point(0, 0);
			this.dateTimePickerAdvEndDate.Calendar.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvEndDate.Calendar.Name = "monthCalendar";
			this.dateTimePickerAdvEndDate.Calendar.ScrollButtonSize = new System.Drawing.Size(24, 24);
			this.dateTimePickerAdvEndDate.Calendar.SelectedDates = new System.DateTime[0];
			this.dateTimePickerAdvEndDate.Calendar.Size = new System.Drawing.Size(167, 174);
			this.dateTimePickerAdvEndDate.Calendar.SizeToFit = true;
			this.dateTimePickerAdvEndDate.Calendar.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.dateTimePickerAdvEndDate.Calendar.TabIndex = 0;
			this.dateTimePickerAdvEndDate.Calendar.ThemedEnabledScrollButtons = false;
			this.dateTimePickerAdvEndDate.Calendar.WeekFont = new System.Drawing.Font("Verdana", 8F);
			this.dateTimePickerAdvEndDate.Calendar.WeekInterior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.PeachPuff, System.Drawing.Color.AntiqueWhite);
			// 
			// 
			// 
			this.dateTimePickerAdvEndDate.Calendar.NoneButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.dateTimePickerAdvEndDate.Calendar.NoneButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvEndDate.Calendar.NoneButton.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.dateTimePickerAdvEndDate.Calendar.NoneButton.ForeColor = System.Drawing.Color.White;
			this.dateTimePickerAdvEndDate.Calendar.NoneButton.IsBackStageButton = false;
			this.dateTimePickerAdvEndDate.Calendar.NoneButton.Location = new System.Drawing.Point(125, 0);
			this.dateTimePickerAdvEndDate.Calendar.NoneButton.Size = new System.Drawing.Size(84, 25);
			this.dateTimePickerAdvEndDate.Calendar.NoneButton.Text = "None";
			this.dateTimePickerAdvEndDate.Calendar.NoneButton.UseVisualStyle = true;
			this.dateTimePickerAdvEndDate.Calendar.NoneButton.Visible = false;
			// 
			// 
			// 
			this.dateTimePickerAdvEndDate.Calendar.TodayButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.dateTimePickerAdvEndDate.Calendar.TodayButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvEndDate.Calendar.TodayButton.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.dateTimePickerAdvEndDate.Calendar.TodayButton.ForeColor = System.Drawing.Color.White;
			this.dateTimePickerAdvEndDate.Calendar.TodayButton.IsBackStageButton = false;
			this.dateTimePickerAdvEndDate.Calendar.TodayButton.Location = new System.Drawing.Point(0, 0);
			this.dateTimePickerAdvEndDate.Calendar.TodayButton.Size = new System.Drawing.Size(167, 25);
			this.dateTimePickerAdvEndDate.Calendar.TodayButton.Text = "Today";
			this.dateTimePickerAdvEndDate.Calendar.TodayButton.UseVisualStyle = true;
			this.dateTimePickerAdvEndDate.CalendarFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dateTimePickerAdvEndDate.CalendarSize = new System.Drawing.Size(189, 176);
			this.dateTimePickerAdvEndDate.CalendarTitleForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvEndDate.ClipboardFormat = System.Windows.Forms.DateTimePickerFormat.Short;
			this.dateTimePickerAdvEndDate.Culture = new System.Globalization.CultureInfo("sv-SE");
			this.dateTimePickerAdvEndDate.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dateTimePickerAdvEndDate.DropDownImage = null;
			this.dateTimePickerAdvEndDate.DropDownNormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvEndDate.DropDownPressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvEndDate.DropDownSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(179)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvEndDate.Enabled = false;
			this.dateTimePickerAdvEndDate.EnableNullDate = false;
			this.dateTimePickerAdvEndDate.EnableNullKeys = false;
			this.dateTimePickerAdvEndDate.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			this.dateTimePickerAdvEndDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
			this.dateTimePickerAdvEndDate.Location = new System.Drawing.Point(103, 33);
			this.dateTimePickerAdvEndDate.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvEndDate.MinValue = new System.DateTime(((long)(0)));
			this.dateTimePickerAdvEndDate.Name = "dateTimePickerAdvEndDate";
			this.dateTimePickerAdvEndDate.ShowCheckBox = false;
			this.dateTimePickerAdvEndDate.Size = new System.Drawing.Size(169, 24);
			this.dateTimePickerAdvEndDate.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.dateTimePickerAdvEndDate.TabIndex = 0;
			this.dateTimePickerAdvEndDate.ThemesEnabled = true;
			this.dateTimePickerAdvEndDate.Value = new System.DateTime(2008, 8, 5, 8, 1, 24, 984);
			// 
			// outlookTimePickerStartTime
			// 
			this.outlookTimePickerStartTime.BackColor = System.Drawing.Color.White;
			this.outlookTimePickerStartTime.BeforeTouchSize = new System.Drawing.Size(109, 23);
			this.outlookTimePickerStartTime.BindableTimeValue = System.TimeSpan.Parse("00:00:00");
			this.outlookTimePickerStartTime.Location = new System.Drawing.Point(278, 3);
			this.outlookTimePickerStartTime.MaxValue = System.TimeSpan.Parse("2.00:00:00");
			this.outlookTimePickerStartTime.MinValue = System.TimeSpan.Parse("00:00:00");
			this.outlookTimePickerStartTime.Name = "outlookTimePickerStartTime";
			this.outlookTimePickerStartTime.Size = new System.Drawing.Size(109, 23);
			this.outlookTimePickerStartTime.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.outlookTimePickerStartTime.TabIndex = 5;
			this.outlookTimePickerStartTime.Text = "00:00";
			this.outlookTimePickerStartTime.SelectedIndexChanged += new System.EventHandler(this.outlookTimePickerStartTimeSelectedIndexChanged);
			this.outlookTimePickerStartTime.KeyDown += new System.Windows.Forms.KeyEventHandler(this.outlookTimePickerStartTimeKeyDown);
			this.outlookTimePickerStartTime.Leave += new System.EventHandler(this.outlookTimePickerStartTimeLeave);
			// 
			// outlookTimePickerEndTime
			// 
			this.outlookTimePickerEndTime.BackColor = System.Drawing.Color.White;
			this.outlookTimePickerEndTime.BeforeTouchSize = new System.Drawing.Size(109, 23);
			this.outlookTimePickerEndTime.BindableTimeValue = System.TimeSpan.Parse("00:00:00");
			this.outlookTimePickerEndTime.Location = new System.Drawing.Point(278, 33);
			this.outlookTimePickerEndTime.MaxValue = System.TimeSpan.Parse("2.00:00:00");
			this.outlookTimePickerEndTime.MinValue = System.TimeSpan.Parse("00:00:00");
			this.outlookTimePickerEndTime.Name = "outlookTimePickerEndTime";
			this.outlookTimePickerEndTime.Size = new System.Drawing.Size(109, 23);
			this.outlookTimePickerEndTime.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.outlookTimePickerEndTime.TabIndex = 6;
			this.outlookTimePickerEndTime.Text = "00:00";
			this.outlookTimePickerEndTime.KeyDown += new System.Windows.Forms.KeyEventHandler(this.outlookTimePickerEndTimeKeyDown);
			this.outlookTimePickerEndTime.Leave += new System.EventHandler(this.outlookTimePickerEndTimeLeave);
			// 
			// tableLayoutPanelTopMain
			// 
			this.tableLayoutPanelTopMain.AutoSize = true;
			this.tableLayoutPanelTopMain.ColumnCount = 3;
			this.tableLayoutPanelTopMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelTopMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelTopMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelTopMain.Controls.Add(this.textBoxExtLocation, 2, 2);
			this.tableLayoutPanelTopMain.Controls.Add(this.autoLabel2, 1, 2);
			this.tableLayoutPanelTopMain.Controls.Add(this.textBoxExtSubject, 2, 1);
			this.tableLayoutPanelTopMain.Controls.Add(this.autoLabel1, 1, 1);
			this.tableLayoutPanelTopMain.Controls.Add(this.buttonAdvParticipants, 1, 0);
			this.tableLayoutPanelTopMain.Controls.Add(this.textBoxExtParticipant, 2, 0);
			this.tableLayoutPanelTopMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelTopMain.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanelTopMain.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanelTopMain.Name = "tableLayoutPanelTopMain";
			this.tableLayoutPanelTopMain.RowCount = 3;
			this.tableLayoutPanelTopMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelTopMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelTopMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelTopMain.Size = new System.Drawing.Size(993, 104);
			this.tableLayoutPanelTopMain.TabIndex = 0;
			// 
			// textBoxExtLocation
			// 
			this.textBoxExtLocation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxExtLocation.BackColor = System.Drawing.Color.White;
			this.textBoxExtLocation.BeforeTouchSize = new System.Drawing.Size(886, 23);
			this.textBoxExtLocation.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxExtLocation.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textBoxExtLocation.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.textBoxExtLocation.ForeColor = System.Drawing.SystemColors.WindowText;
			this.textBoxExtLocation.Location = new System.Drawing.Point(104, 65);
			this.textBoxExtLocation.MaxLength = 80;
			this.textBoxExtLocation.Metrocolor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxExtLocation.Name = "textBoxExtLocation";
			this.textBoxExtLocation.OverflowIndicatorToolTipText = null;
			this.textBoxExtLocation.Size = new System.Drawing.Size(886, 23);
			this.textBoxExtLocation.Style = Syncfusion.Windows.Forms.Tools.TextBoxExt.theme.Metro;
			this.textBoxExtLocation.TabIndex = 4;
			this.textBoxExtLocation.TextChanged += new System.EventHandler(this.textBoxExtLocationTextChanged);
			// 
			// autoLabel2
			// 
			this.autoLabel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.autoLabel2.Location = new System.Drawing.Point(3, 62);
			this.autoLabel2.Name = "autoLabel2";
			this.autoLabel2.Size = new System.Drawing.Size(95, 42);
			this.autoLabel2.TabIndex = 0;
			this.autoLabel2.Text = "xxLocationColon";
			this.autoLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textBoxExtSubject
			// 
			this.textBoxExtSubject.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxExtSubject.BackColor = System.Drawing.Color.White;
			this.textBoxExtSubject.BeforeTouchSize = new System.Drawing.Size(886, 23);
			this.textBoxExtSubject.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxExtSubject.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textBoxExtSubject.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.textBoxExtSubject.Location = new System.Drawing.Point(104, 36);
			this.textBoxExtSubject.MaxLength = 80;
			this.textBoxExtSubject.Metrocolor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxExtSubject.Name = "textBoxExtSubject";
			this.textBoxExtSubject.OverflowIndicatorToolTipText = null;
			this.textBoxExtSubject.Size = new System.Drawing.Size(886, 23);
			this.textBoxExtSubject.Style = Syncfusion.Windows.Forms.Tools.TextBoxExt.theme.Metro;
			this.textBoxExtSubject.TabIndex = 3;
			this.textBoxExtSubject.TextChanged += new System.EventHandler(this.textBoxExtSubjectTextChanged);
			// 
			// autoLabel1
			// 
			this.autoLabel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.autoLabel1.Location = new System.Drawing.Point(3, 33);
			this.autoLabel1.Name = "autoLabel1";
			this.autoLabel1.Size = new System.Drawing.Size(95, 29);
			this.autoLabel1.TabIndex = 0;
			this.autoLabel1.Text = "xxSubjectColon";
			this.autoLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// buttonAdvParticipants
			// 
			this.buttonAdvParticipants.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvParticipants.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvParticipants.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonAdvParticipants.ForeColor = System.Drawing.Color.White;
			this.buttonAdvParticipants.IsBackStageButton = false;
			this.buttonAdvParticipants.Location = new System.Drawing.Point(3, 3);
			this.buttonAdvParticipants.Name = "buttonAdvParticipants";
			this.buttonAdvParticipants.Size = new System.Drawing.Size(87, 27);
			this.buttonAdvParticipants.TabIndex = 1;
			this.buttonAdvParticipants.Text = "xxToThreeDots";
			this.buttonAdvParticipants.UseVisualStyle = true;
			this.buttonAdvParticipants.Click += new System.EventHandler(this.buttonAdvParticipantsClick);
			// 
			// textBoxExtParticipant
			// 
			this.textBoxExtParticipant.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxExtParticipant.BackColor = System.Drawing.Color.White;
			this.textBoxExtParticipant.BeforeTouchSize = new System.Drawing.Size(886, 23);
			this.textBoxExtParticipant.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxExtParticipant.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textBoxExtParticipant.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.textBoxExtParticipant.Location = new System.Drawing.Point(104, 3);
			this.textBoxExtParticipant.Metrocolor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxExtParticipant.Multiline = true;
			this.textBoxExtParticipant.Name = "textBoxExtParticipant";
			this.textBoxExtParticipant.OverflowIndicatorToolTipText = null;
			this.textBoxExtParticipant.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textBoxExtParticipant.ShortcutsEnabled = false;
			this.textBoxExtParticipant.Size = new System.Drawing.Size(886, 22);
			this.textBoxExtParticipant.Style = Syncfusion.Windows.Forms.Tools.TextBoxExt.theme.Metro;
			this.textBoxExtParticipant.TabIndex = 2;
			this.textBoxExtParticipant.TextChanged += new System.EventHandler(this.meetingPersonTextBoxParticipantTextChanged);
			this.textBoxExtParticipant.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxExtParticipantKeyDown);
			this.textBoxExtParticipant.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.textBoxExtParticipantMouseUp);
			this.textBoxExtParticipant.MouseUp += new System.Windows.Forms.MouseEventHandler(this.textBoxExtParticipantMouseUp);
			// 
			// MeetingGeneralView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.tableLayoutPanelMain);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.Name = "MeetingGeneralView";
			this.Size = new System.Drawing.Size(993, 667);
			this.tableLayoutPanelMain.ResumeLayout(false);
			this.tableLayoutPanelMain.PerformLayout();
			this.tableLayoutPanelBottomMain.ResumeLayout(false);
			this.tableLayoutPanelBottomMain.PerformLayout();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.textBoxExtDescription)).EndInit();
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvActivity)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvStartDate.Calendar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvStartDate)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvEndDate.Calendar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvEndDate)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerStartTime)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerEndTime)).EndInit();
			this.tableLayoutPanelTopMain.ResumeLayout(false);
			this.tableLayoutPanelTopMain.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.textBoxExtLocation)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.textBoxExtSubject)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.textBoxExtParticipant)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelMain;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelBottomMain;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelActivity;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelStartTimeFomat;
        private Syncfusion.Windows.Forms.Tools.DateTimePickerAdv dateTimePickerAdvStartDate;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelTopMain;
        private Syncfusion.Windows.Forms.Tools.TextBoxExt textBoxExtLocation;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel2;
        private Syncfusion.Windows.Forms.Tools.TextBoxExt textBoxExtSubject;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel1;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvParticipants;
        private Syncfusion.Windows.Forms.Tools.TextBoxExt textBoxExtParticipant;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvActivity;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelEndTimeFomat;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel4;
        private Syncfusion.Windows.Forms.Tools.DateTimePickerAdv dateTimePickerAdvEndDate;
        private Syncfusion.Windows.Forms.Tools.TextBoxExt textBoxExtDescription;
        private Teleopti.Ccc.Win.Common.Controls.Office2007OutlookTimePicker outlookTimePickerStartTime;
        private Teleopti.Ccc.Win.Common.Controls.Office2007OutlookTimePicker outlookTimePickerEndTime;
    }
}
