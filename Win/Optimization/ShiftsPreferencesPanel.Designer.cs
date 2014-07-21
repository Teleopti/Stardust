namespace Teleopti.Ccc.Win.Optimization
{
    partial class ShiftsPreferencesPanel
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
                if (fromToTimePicker1 != null)
                {
                    fromToTimePicker1.StartTime.TextChanged -= startTimeTextChanged;
                    fromToTimePicker1.EndTime.TextChanged -= endTimeTextChanged;
                }
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxShifts"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxShiftCategories"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxNextDay"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxUseAvailability"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxStartTime"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxScheduleOnlyAvailabilityDays"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxKeepShifts"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxKeepShiftCategories"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxAlterBetween"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Win.Common.Controls.FromToTimePicker.set_WholeDayText(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxKeep"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxEndTime"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.Control.set_Text(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxUseSameDayOffs"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxGroupings"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxNotBreakMaxStaffing")]
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ShiftsPreferencesPanel));
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanelTagWith = new System.Windows.Forms.TableLayoutPanel();
			this.label6 = new System.Windows.Forms.Label();
			this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel7 = new System.Windows.Forms.TableLayoutPanel();
			this.checkBox1 = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.checkBox2 = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
			this.checkBoxKeepShifts = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.twoListSelectorActivities = new Teleopti.Ccc.Win.Common.Controls.TwoListSelector();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.fromToTimePicker1 = new Teleopti.Ccc.Win.Common.Controls.FromToTimePicker();
			this.checkBoxBetween = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
			this.comboBoxAdvActivity = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.checkBoxKeepShiftCategories = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.label1 = new System.Windows.Forms.Label();
			this.numericUpDownKeepShifts = new Syncfusion.Windows.Forms.Tools.NumericUpDownExt();
			this.checkBoxKeepStartTimes = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.checkBoxKeepEndTimes = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.checkBoxKeepActivityLength = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.tableLayoutPanel1.SuspendLayout();
			this.tableLayoutPanelTagWith.SuspendLayout();
			this.tableLayoutPanel7.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.checkBox1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBox2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxKeepShifts)).BeginInit();
			this.tableLayoutPanel2.SuspendLayout();
			this.groupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxBetween)).BeginInit();
			this.groupBox1.SuspendLayout();
			this.tableLayoutPanel5.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvActivity)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxKeepShiftCategories)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownKeepShifts)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxKeepStartTimes)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxKeepEndTimes)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxKeepActivityLength)).BeginInit();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanelTagWith, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 5;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 146F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 234F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 92F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(416, 575);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// tableLayoutPanelTagWith
			// 
			this.tableLayoutPanelTagWith.ColumnCount = 2;
			this.tableLayoutPanelTagWith.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelTagWith.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 277F));
			this.tableLayoutPanelTagWith.Controls.Add(this.label6, 0, 0);
			this.tableLayoutPanelTagWith.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelTagWith.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanelTagWith.Name = "tableLayoutPanelTagWith";
			this.tableLayoutPanelTagWith.RowCount = 1;
			this.tableLayoutPanelTagWith.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelTagWith.Size = new System.Drawing.Size(410, 24);
			this.tableLayoutPanelTagWith.TabIndex = 21;
			// 
			// label6
			// 
			this.label6.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(3, 5);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(0, 13);
			this.label6.TabIndex = 0;
			// 
			// tableLayoutPanel4
			// 
			this.tableLayoutPanel4.ColumnCount = 3;
			this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 258F));
			this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 16);
			this.tableLayoutPanel4.Name = "tableLayoutPanel4";
			this.tableLayoutPanel4.RowCount = 5;
			this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 21F));
			this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
			this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 14F));
			this.tableLayoutPanel4.Size = new System.Drawing.Size(404, 121);
			this.tableLayoutPanel4.TabIndex = 0;
			// 
			// tableLayoutPanel7
			// 
			this.tableLayoutPanel7.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel7.ColumnCount = 3;
			this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 9.132421F));
			this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 90.86758F));
			this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 62F));
			this.tableLayoutPanel7.Controls.Add(this.checkBox1, 1, 1);
			this.tableLayoutPanel7.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel7.Name = "tableLayoutPanel7";
			this.tableLayoutPanel7.RowCount = 2;
			this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel7.Size = new System.Drawing.Size(200, 100);
			this.tableLayoutPanel7.TabIndex = 0;
			// 
			// checkBox1
			// 
			this.checkBox1.BeforeTouchSize = new System.Drawing.Size(119, 17);
			this.checkBox1.Location = new System.Drawing.Point(15, 23);
			this.checkBox1.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.Size = new System.Drawing.Size(119, 17);
			this.checkBox1.TabIndex = 8;
			this.checkBox1.Text = "xxScheduleOnlyAvailabilityDays";
			this.checkBox1.ThemesEnabled = false;
			// 
			// checkBox2
			// 
			this.checkBox2.BeforeTouchSize = new System.Drawing.Size(104, 17);
			this.checkBox2.Location = new System.Drawing.Point(3, 3);
			this.checkBox2.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBox2.Name = "checkBox2";
			this.checkBox2.Size = new System.Drawing.Size(104, 17);
			this.checkBox2.TabIndex = 7;
			this.checkBox2.Text = "xxUseAvailability";
			this.checkBox2.ThemesEnabled = false;
			// 
			// tableLayoutPanel3
			// 
			this.tableLayoutPanel3.ColumnCount = 3;
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 272F));
			this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 16);
			this.tableLayoutPanel3.Name = "tableLayoutPanel3";
			this.tableLayoutPanel3.RowCount = 5;
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 21F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 14F));
			this.tableLayoutPanel3.Size = new System.Drawing.Size(418, 121);
			this.tableLayoutPanel3.TabIndex = 0;
			// 
			// checkBoxKeepShifts
			// 
			this.checkBoxKeepShifts.BeforeTouchSize = new System.Drawing.Size(232, 19);
			this.checkBoxKeepShifts.Dock = System.Windows.Forms.DockStyle.Fill;
			this.checkBoxKeepShifts.DrawFocusRectangle = false;
			this.checkBoxKeepShifts.Location = new System.Drawing.Point(3, 3);
			this.checkBoxKeepShifts.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxKeepShifts.Name = "checkBoxKeepShifts";
			this.checkBoxKeepShifts.Size = new System.Drawing.Size(232, 19);
			this.checkBoxKeepShifts.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Metro;
			this.checkBoxKeepShifts.TabIndex = 9;
			this.checkBoxKeepShifts.Text = "xxShifts";
			this.checkBoxKeepShifts.ThemesEnabled = false;
			this.checkBoxKeepShifts.CheckedChanged += new System.EventHandler(this.checkBoxKeepShifts_CheckedChanged);
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel2.ColumnCount = 1;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Controls.Add(this.twoListSelectorActivities, 0, 2);
			this.tableLayoutPanel2.Controls.Add(this.groupBox2, 0, 1);
			this.tableLayoutPanel2.Controls.Add(this.groupBox1, 0, 0);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 3;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 152F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 51F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 221F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(408, 477);
			this.tableLayoutPanel2.TabIndex = 1;
			// 
			// twoListSelectorActivities
			// 
			this.twoListSelectorActivities.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
			this.twoListSelectorActivities.Dock = System.Windows.Forms.DockStyle.Fill;
			this.twoListSelectorActivities.Location = new System.Drawing.Point(3, 206);
			this.twoListSelectorActivities.Name = "twoListSelectorActivities";
			this.twoListSelectorActivities.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.twoListSelectorActivities.Size = new System.Drawing.Size(402, 268);
			this.twoListSelectorActivities.TabIndex = 26;
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.fromToTimePicker1);
			this.groupBox2.Controls.Add(this.checkBoxBetween);
			this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBox2.Location = new System.Drawing.Point(3, 155);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(402, 45);
			this.groupBox2.TabIndex = 25;
			this.groupBox2.TabStop = false;
			// 
			// fromToTimePicker1
			// 
			this.fromToTimePicker1.Location = new System.Drawing.Point(130, 12);
			this.fromToTimePicker1.MinMaxEndTime = ((Teleopti.Interfaces.Domain.MinMax<System.TimeSpan>)(resources.GetObject("fromToTimePicker1.MinMaxEndTime")));
			this.fromToTimePicker1.MinMaxStartTime = ((Teleopti.Interfaces.Domain.MinMax<System.TimeSpan>)(resources.GetObject("fromToTimePicker1.MinMaxStartTime")));
			this.fromToTimePicker1.Name = "fromToTimePicker1";
			this.fromToTimePicker1.Size = new System.Drawing.Size(266, 27);
			this.fromToTimePicker1.TabIndex = 10;
			this.fromToTimePicker1.WholeDayText = "xxNextDay";
			// 
			// checkBoxBetween
			// 
			this.checkBoxBetween.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.checkBoxBetween.BeforeTouchSize = new System.Drawing.Size(128, 24);
			this.checkBoxBetween.DrawFocusRectangle = false;
			this.checkBoxBetween.Location = new System.Drawing.Point(6, 15);
			this.checkBoxBetween.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxBetween.Name = "checkBoxBetween";
			this.checkBoxBetween.Size = new System.Drawing.Size(128, 24);
			this.checkBoxBetween.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Metro;
			this.checkBoxBetween.TabIndex = 5;
			this.checkBoxBetween.Text = "xxAlterBetween";
			this.checkBoxBetween.ThemesEnabled = false;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.tableLayoutPanel5);
			this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBox1.Location = new System.Drawing.Point(3, 3);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(402, 146);
			this.groupBox1.TabIndex = 24;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "xxKeep";
			// 
			// tableLayoutPanel5
			// 
			this.tableLayoutPanel5.ColumnCount = 3;
			this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 46F));
			this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 112F));
			this.tableLayoutPanel5.Controls.Add(this.comboBoxAdvActivity, 1, 4);
			this.tableLayoutPanel5.Controls.Add(this.checkBoxKeepShiftCategories, 0, 1);
			this.tableLayoutPanel5.Controls.Add(this.label1, 2, 0);
			this.tableLayoutPanel5.Controls.Add(this.numericUpDownKeepShifts, 1, 0);
			this.tableLayoutPanel5.Controls.Add(this.checkBoxKeepShifts, 0, 0);
			this.tableLayoutPanel5.Controls.Add(this.checkBoxKeepStartTimes, 0, 2);
			this.tableLayoutPanel5.Controls.Add(this.checkBoxKeepEndTimes, 0, 3);
			this.tableLayoutPanel5.Controls.Add(this.checkBoxKeepActivityLength, 0, 4);
			this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 16);
			this.tableLayoutPanel5.Name = "tableLayoutPanel5";
			this.tableLayoutPanel5.RowCount = 5;
			this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.tableLayoutPanel5.Size = new System.Drawing.Size(396, 127);
			this.tableLayoutPanel5.TabIndex = 0;
			// 
			// comboBoxAdvActivity
			// 
			this.comboBoxAdvActivity.BackColor = System.Drawing.Color.White;
			this.comboBoxAdvActivity.BeforeTouchSize = new System.Drawing.Size(152, 21);
			this.tableLayoutPanel5.SetColumnSpan(this.comboBoxAdvActivity, 2);
			this.comboBoxAdvActivity.Dock = System.Windows.Forms.DockStyle.Fill;
			this.comboBoxAdvActivity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAdvActivity.Location = new System.Drawing.Point(241, 103);
			this.comboBoxAdvActivity.Name = "comboBoxAdvActivity";
			this.comboBoxAdvActivity.Size = new System.Drawing.Size(152, 21);
			this.comboBoxAdvActivity.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.comboBoxAdvActivity.TabIndex = 26;
			// 
			// checkBoxKeepShiftCategories
			// 
			this.checkBoxKeepShiftCategories.BeforeTouchSize = new System.Drawing.Size(232, 19);
			this.checkBoxKeepShiftCategories.Dock = System.Windows.Forms.DockStyle.Fill;
			this.checkBoxKeepShiftCategories.DrawFocusRectangle = false;
			this.checkBoxKeepShiftCategories.Location = new System.Drawing.Point(3, 28);
			this.checkBoxKeepShiftCategories.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxKeepShiftCategories.Name = "checkBoxKeepShiftCategories";
			this.checkBoxKeepShiftCategories.Size = new System.Drawing.Size(232, 19);
			this.checkBoxKeepShiftCategories.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Metro;
			this.checkBoxKeepShiftCategories.TabIndex = 22;
			this.checkBoxKeepShiftCategories.Text = "xxShiftCategories";
			this.checkBoxKeepShiftCategories.ThemesEnabled = false;
			// 
			// label1
			// 
			this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(287, 6);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(15, 13);
			this.label1.TabIndex = 21;
			this.label1.Text = "%";
			// 
			// numericUpDownKeepShifts
			// 
			this.numericUpDownKeepShifts.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.numericUpDownKeepShifts.BeforeTouchSize = new System.Drawing.Size(40, 22);
			this.numericUpDownKeepShifts.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.numericUpDownKeepShifts.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericUpDownKeepShifts.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.numericUpDownKeepShifts.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
			this.numericUpDownKeepShifts.Location = new System.Drawing.Point(241, 3);
			this.numericUpDownKeepShifts.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.numericUpDownKeepShifts.Name = "numericUpDownKeepShifts";
			this.numericUpDownKeepShifts.Size = new System.Drawing.Size(40, 22);
			this.numericUpDownKeepShifts.TabIndex = 17;
			this.numericUpDownKeepShifts.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericUpDownKeepShifts.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
			this.numericUpDownKeepShifts.VisualStyle = Syncfusion.Windows.Forms.VisualStyle.Metro;
			// 
			// checkBoxKeepStartTimes
			// 
			this.checkBoxKeepStartTimes.BeforeTouchSize = new System.Drawing.Size(232, 19);
			this.checkBoxKeepStartTimes.Dock = System.Windows.Forms.DockStyle.Fill;
			this.checkBoxKeepStartTimes.DrawFocusRectangle = false;
			this.checkBoxKeepStartTimes.Location = new System.Drawing.Point(3, 53);
			this.checkBoxKeepStartTimes.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxKeepStartTimes.Name = "checkBoxKeepStartTimes";
			this.checkBoxKeepStartTimes.Size = new System.Drawing.Size(232, 19);
			this.checkBoxKeepStartTimes.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Metro;
			this.checkBoxKeepStartTimes.TabIndex = 23;
			this.checkBoxKeepStartTimes.Text = "xxStartTime";
			this.checkBoxKeepStartTimes.ThemesEnabled = false;
			// 
			// checkBoxKeepEndTimes
			// 
			this.checkBoxKeepEndTimes.BeforeTouchSize = new System.Drawing.Size(232, 19);
			this.checkBoxKeepEndTimes.Dock = System.Windows.Forms.DockStyle.Fill;
			this.checkBoxKeepEndTimes.DrawFocusRectangle = false;
			this.checkBoxKeepEndTimes.Location = new System.Drawing.Point(3, 78);
			this.checkBoxKeepEndTimes.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxKeepEndTimes.Name = "checkBoxKeepEndTimes";
			this.checkBoxKeepEndTimes.Size = new System.Drawing.Size(232, 19);
			this.checkBoxKeepEndTimes.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Metro;
			this.checkBoxKeepEndTimes.TabIndex = 24;
			this.checkBoxKeepEndTimes.Text = "xxEndTime";
			this.checkBoxKeepEndTimes.ThemesEnabled = false;
			// 
			// checkBoxKeepActivityLength
			// 
			this.checkBoxKeepActivityLength.BeforeTouchSize = new System.Drawing.Size(232, 21);
			this.checkBoxKeepActivityLength.Dock = System.Windows.Forms.DockStyle.Fill;
			this.checkBoxKeepActivityLength.DrawFocusRectangle = false;
			this.checkBoxKeepActivityLength.Location = new System.Drawing.Point(3, 103);
			this.checkBoxKeepActivityLength.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxKeepActivityLength.Name = "checkBoxKeepActivityLength";
			this.checkBoxKeepActivityLength.Size = new System.Drawing.Size(232, 21);
			this.checkBoxKeepActivityLength.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Metro;
			this.checkBoxKeepActivityLength.TabIndex = 25;
			this.checkBoxKeepActivityLength.Text = "xxLengthOfActivity";
			this.checkBoxKeepActivityLength.ThemesEnabled = false;
			this.checkBoxKeepActivityLength.CheckedChanged += new System.EventHandler(this.checkBoxKeepActivityLength_CheckedChanged);
			// 
			// ShiftsPreferencesPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.Controls.Add(this.tableLayoutPanel2);
			this.Name = "ShiftsPreferencesPanel";
			this.Size = new System.Drawing.Size(408, 477);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanelTagWith.ResumeLayout(false);
			this.tableLayoutPanelTagWith.PerformLayout();
			this.tableLayoutPanel7.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.checkBox1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBox2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxKeepShifts)).EndInit();
			this.tableLayoutPanel2.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.checkBoxBetween)).EndInit();
			this.groupBox1.ResumeLayout(false);
			this.tableLayoutPanel5.ResumeLayout(false);
			this.tableLayoutPanel5.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvActivity)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxKeepShiftCategories)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownKeepShifts)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxKeepStartTimes)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxKeepEndTimes)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxKeepActivityLength)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel7;
        private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBox1;
        private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBox2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelTagWith;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxKeepShifts;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxKeepShiftCategories;
        private System.Windows.Forms.Label label1;
        private Syncfusion.Windows.Forms.Tools.NumericUpDownExt numericUpDownKeepShifts;
        private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxKeepStartTimes;
        private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxKeepEndTimes;
        private System.Windows.Forms.GroupBox groupBox2;
        private Common.Controls.FromToTimePicker fromToTimePicker1;
        private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxBetween;
        private Common.Controls.TwoListSelector twoListSelectorActivities;
		private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxKeepActivityLength;
		private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvActivity;
    }
}
