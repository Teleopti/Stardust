namespace Teleopti.Ccc.Win.Optimization
{
	partial class AdvancedPreferencesPanel
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxUseMaxStddev"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxRMS"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxUseSameDayOffs"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxGroupings"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxNotBreakMaxStaffing")]
		private void InitializeComponent()
		{
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanelRefreshScreen = new System.Windows.Forms.TableLayoutPanel();
			this.labelShift = new System.Windows.Forms.Label();
			this.numericUpDownRefreshRate = new System.Windows.Forms.NumericUpDown();
			this.labelRefreshScreenEveryColon = new System.Windows.Forms.Label();
			this.tableLayoutPanel8 = new System.Windows.Forms.TableLayoutPanel();
			this.checkBoxMaximumSeats = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.checkBoxMaximumStaffing = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.checkBoxMinimumStaffing = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.labelShiftSelection = new System.Windows.Forms.Label();
			this.checkBoxDoNotBreakMaximumSeats = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.checkBoxUseAverageShiftLengths = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
			this.radioButtonStandardDeviation = new Syncfusion.Windows.Forms.Tools.RadioButtonAdv();
			this.checkBoxUseTweakedValues = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.checkBoxUseIntraIntervalDeviation = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.labelTargetValue = new System.Windows.Forms.Label();
			this.radioButtonRootMeanSquare = new Syncfusion.Windows.Forms.Tools.RadioButtonAdv();
			this.radioButtonTeleopti = new Syncfusion.Windows.Forms.Tools.RadioButtonAdv();
			this.tableLayoutPanel2.SuspendLayout();
			this.tableLayoutPanelRefreshScreen.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownRefreshRate)).BeginInit();
			this.tableLayoutPanel8.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxMaximumSeats)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxMaximumStaffing)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxMinimumStaffing)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxDoNotBreakMaximumSeats)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxUseAverageShiftLengths)).BeginInit();
			this.tableLayoutPanel6.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonStandardDeviation)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxUseTweakedValues)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxUseIntraIntervalDeviation)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonRootMeanSquare)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonTeleopti)).BeginInit();
			this.SuspendLayout();
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel2.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanel2.ColumnCount = 1;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanelRefreshScreen, 0, 2);
			this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel8, 0, 1);
			this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel6, 0, 0);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 3;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 198F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 177F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(420, 451);
			this.tableLayoutPanel2.TabIndex = 1;
			// 
			// tableLayoutPanelRefreshScreen
			// 
			this.tableLayoutPanelRefreshScreen.ColumnCount = 3;
			this.tableLayoutPanelRefreshScreen.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 260F));
			this.tableLayoutPanelRefreshScreen.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			this.tableLayoutPanelRefreshScreen.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelRefreshScreen.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanelRefreshScreen.Controls.Add(this.labelShift, 0, 0);
			this.tableLayoutPanelRefreshScreen.Controls.Add(this.numericUpDownRefreshRate, 0, 0);
			this.tableLayoutPanelRefreshScreen.Controls.Add(this.labelRefreshScreenEveryColon, 0, 0);
			this.tableLayoutPanelRefreshScreen.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.tableLayoutPanelRefreshScreen.Location = new System.Drawing.Point(0, 428);
			this.tableLayoutPanelRefreshScreen.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanelRefreshScreen.Name = "tableLayoutPanelRefreshScreen";
			this.tableLayoutPanelRefreshScreen.RowCount = 1;
			this.tableLayoutPanelRefreshScreen.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelRefreshScreen.Size = new System.Drawing.Size(420, 23);
			this.tableLayoutPanelRefreshScreen.TabIndex = 24;
			// 
			// labelShift
			// 
			this.labelShift.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.labelShift.Location = new System.Drawing.Point(313, 3);
			this.labelShift.Name = "labelShift";
			this.labelShift.Size = new System.Drawing.Size(98, 20);
			this.labelShift.TabIndex = 19;
			this.labelShift.Text = "xxShifts";
			this.labelShift.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// numericUpDownRefreshRate
			// 
			this.numericUpDownRefreshRate.Dock = System.Windows.Forms.DockStyle.Fill;
			this.numericUpDownRefreshRate.Location = new System.Drawing.Point(263, 3);
			this.numericUpDownRefreshRate.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
			this.numericUpDownRefreshRate.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numericUpDownRefreshRate.Name = "numericUpDownRefreshRate";
			this.numericUpDownRefreshRate.Size = new System.Drawing.Size(44, 23);
			this.numericUpDownRefreshRate.TabIndex = 19;
			this.numericUpDownRefreshRate.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericUpDownRefreshRate.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// labelRefreshScreenEveryColon
			// 
			this.labelRefreshScreenEveryColon.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelRefreshScreenEveryColon.Location = new System.Drawing.Point(3, 0);
			this.labelRefreshScreenEveryColon.Name = "labelRefreshScreenEveryColon";
			this.labelRefreshScreenEveryColon.Size = new System.Drawing.Size(254, 23);
			this.labelRefreshScreenEveryColon.TabIndex = 17;
			this.labelRefreshScreenEveryColon.Text = "xxRefreshScreenEveryColon";
			this.labelRefreshScreenEveryColon.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// tableLayoutPanel8
			// 
			this.tableLayoutPanel8.ColumnCount = 2;
			this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel8.Controls.Add(this.checkBoxMaximumSeats, 0, 3);
			this.tableLayoutPanel8.Controls.Add(this.checkBoxMaximumStaffing, 0, 2);
			this.tableLayoutPanel8.Controls.Add(this.checkBoxMinimumStaffing, 0, 1);
			this.tableLayoutPanel8.Controls.Add(this.labelShiftSelection, 0, 0);
			this.tableLayoutPanel8.Controls.Add(this.checkBoxDoNotBreakMaximumSeats, 1, 4);
			this.tableLayoutPanel8.Controls.Add(this.checkBoxUseAverageShiftLengths, 0, 5);
			this.tableLayoutPanel8.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel8.Location = new System.Drawing.Point(0, 198);
			this.tableLayoutPanel8.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel8.Name = "tableLayoutPanel8";
			this.tableLayoutPanel8.RowCount = 6;
			this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel8.Size = new System.Drawing.Size(420, 177);
			this.tableLayoutPanel8.TabIndex = 23;
			// 
			// checkBoxMaximumSeats
			// 
			this.checkBoxMaximumSeats.BeforeTouchSize = new System.Drawing.Size(407, 24);
			this.tableLayoutPanel8.SetColumnSpan(this.checkBoxMaximumSeats, 2);
			this.checkBoxMaximumSeats.Dock = System.Windows.Forms.DockStyle.Fill;
			this.checkBoxMaximumSeats.DrawFocusRectangle = false;
			this.checkBoxMaximumSeats.Location = new System.Drawing.Point(10, 93);
			this.checkBoxMaximumSeats.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
			this.checkBoxMaximumSeats.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxMaximumSeats.Name = "checkBoxMaximumSeats";
			this.checkBoxMaximumSeats.Size = new System.Drawing.Size(407, 24);
			this.checkBoxMaximumSeats.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Metro;
			this.checkBoxMaximumSeats.TabIndex = 9;
			this.checkBoxMaximumSeats.Text = "xxMaximumSeats";
			this.checkBoxMaximumSeats.ThemesEnabled = false;
			this.checkBoxMaximumSeats.CheckedChanged += new System.EventHandler(this.checkBoxMaximumSeats_CheckedChanged);
			// 
			// checkBoxMaximumStaffing
			// 
			this.checkBoxMaximumStaffing.BeforeTouchSize = new System.Drawing.Size(407, 24);
			this.tableLayoutPanel8.SetColumnSpan(this.checkBoxMaximumStaffing, 2);
			this.checkBoxMaximumStaffing.Dock = System.Windows.Forms.DockStyle.Fill;
			this.checkBoxMaximumStaffing.DrawFocusRectangle = false;
			this.checkBoxMaximumStaffing.Location = new System.Drawing.Point(10, 63);
			this.checkBoxMaximumStaffing.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
			this.checkBoxMaximumStaffing.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxMaximumStaffing.Name = "checkBoxMaximumStaffing";
			this.checkBoxMaximumStaffing.Size = new System.Drawing.Size(407, 24);
			this.checkBoxMaximumStaffing.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Metro;
			this.checkBoxMaximumStaffing.TabIndex = 7;
			this.checkBoxMaximumStaffing.Text = "xxMaximumStaffing";
			this.checkBoxMaximumStaffing.ThemesEnabled = false;
			// 
			// checkBoxMinimumStaffing
			// 
			this.checkBoxMinimumStaffing.BeforeTouchSize = new System.Drawing.Size(407, 24);
			this.tableLayoutPanel8.SetColumnSpan(this.checkBoxMinimumStaffing, 2);
			this.checkBoxMinimumStaffing.Dock = System.Windows.Forms.DockStyle.Fill;
			this.checkBoxMinimumStaffing.DrawFocusRectangle = false;
			this.checkBoxMinimumStaffing.Location = new System.Drawing.Point(10, 33);
			this.checkBoxMinimumStaffing.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
			this.checkBoxMinimumStaffing.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxMinimumStaffing.Name = "checkBoxMinimumStaffing";
			this.checkBoxMinimumStaffing.Size = new System.Drawing.Size(407, 24);
			this.checkBoxMinimumStaffing.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Metro;
			this.checkBoxMinimumStaffing.TabIndex = 5;
			this.checkBoxMinimumStaffing.Text = "xxMinimumStaffing";
			this.checkBoxMinimumStaffing.ThemesEnabled = false;
			// 
			// labelShiftSelection
			// 
			this.labelShiftSelection.AutoSize = true;
			this.tableLayoutPanel8.SetColumnSpan(this.labelShiftSelection, 2);
			this.labelShiftSelection.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelShiftSelection.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelShiftSelection.Location = new System.Drawing.Point(3, 3);
			this.labelShiftSelection.Margin = new System.Windows.Forms.Padding(3);
			this.labelShiftSelection.Name = "labelShiftSelection";
			this.labelShiftSelection.Size = new System.Drawing.Size(414, 24);
			this.labelShiftSelection.TabIndex = 8;
			this.labelShiftSelection.Text = "xxShiftSelection";
			this.labelShiftSelection.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// checkBoxDoNotBreakMaximumSeats
			// 
			this.checkBoxDoNotBreakMaximumSeats.BeforeTouchSize = new System.Drawing.Size(387, 24);
			this.checkBoxDoNotBreakMaximumSeats.Dock = System.Windows.Forms.DockStyle.Fill;
			this.checkBoxDoNotBreakMaximumSeats.DrawFocusRectangle = false;
			this.checkBoxDoNotBreakMaximumSeats.Location = new System.Drawing.Point(30, 123);
			this.checkBoxDoNotBreakMaximumSeats.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
			this.checkBoxDoNotBreakMaximumSeats.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxDoNotBreakMaximumSeats.Name = "checkBoxDoNotBreakMaximumSeats";
			this.checkBoxDoNotBreakMaximumSeats.Size = new System.Drawing.Size(387, 24);
			this.checkBoxDoNotBreakMaximumSeats.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Metro;
			this.checkBoxDoNotBreakMaximumSeats.TabIndex = 10;
			this.checkBoxDoNotBreakMaximumSeats.Text = "xxDoNotBreakMaximumSeats";
			this.checkBoxDoNotBreakMaximumSeats.ThemesEnabled = false;
			// 
			// checkBoxUseAverageShiftLengths
			// 
			this.checkBoxUseAverageShiftLengths.BeforeTouchSize = new System.Drawing.Size(407, 21);
			this.tableLayoutPanel8.SetColumnSpan(this.checkBoxUseAverageShiftLengths, 2);
			this.checkBoxUseAverageShiftLengths.Dock = System.Windows.Forms.DockStyle.Fill;
			this.checkBoxUseAverageShiftLengths.DrawFocusRectangle = false;
			this.checkBoxUseAverageShiftLengths.Location = new System.Drawing.Point(10, 153);
			this.checkBoxUseAverageShiftLengths.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
			this.checkBoxUseAverageShiftLengths.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxUseAverageShiftLengths.Name = "checkBoxUseAverageShiftLengths";
			this.checkBoxUseAverageShiftLengths.Size = new System.Drawing.Size(407, 21);
			this.checkBoxUseAverageShiftLengths.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Metro;
			this.checkBoxUseAverageShiftLengths.TabIndex = 11;
			this.checkBoxUseAverageShiftLengths.Text = "xxUseAverageShiftLengths";
			this.checkBoxUseAverageShiftLengths.ThemesEnabled = false;
			// 
			// tableLayoutPanel6
			// 
			this.tableLayoutPanel6.ColumnCount = 1;
			this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel6.Controls.Add(this.radioButtonStandardDeviation, 0, 1);
			this.tableLayoutPanel6.Controls.Add(this.checkBoxUseTweakedValues, 0, 5);
			this.tableLayoutPanel6.Controls.Add(this.checkBoxUseIntraIntervalDeviation, 0, 4);
			this.tableLayoutPanel6.Controls.Add(this.labelTargetValue, 0, 0);
			this.tableLayoutPanel6.Controls.Add(this.radioButtonRootMeanSquare, 0, 2);
			this.tableLayoutPanel6.Controls.Add(this.radioButtonTeleopti, 0, 3);
			this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel6.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel6.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel6.Name = "tableLayoutPanel6";
			this.tableLayoutPanel6.RowCount = 7;
			this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel6.Size = new System.Drawing.Size(420, 198);
			this.tableLayoutPanel6.TabIndex = 22;
			// 
			// radioButtonStandardDeviation
			// 
			this.radioButtonStandardDeviation.BeforeTouchSize = new System.Drawing.Size(407, 24);
			this.radioButtonStandardDeviation.Dock = System.Windows.Forms.DockStyle.Fill;
			this.radioButtonStandardDeviation.DrawFocusRectangle = false;
			this.radioButtonStandardDeviation.Location = new System.Drawing.Point(10, 33);
			this.radioButtonStandardDeviation.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
			this.radioButtonStandardDeviation.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.radioButtonStandardDeviation.Name = "radioButtonStandardDeviation";
			this.radioButtonStandardDeviation.Size = new System.Drawing.Size(407, 24);
			this.radioButtonStandardDeviation.Style = Syncfusion.Windows.Forms.Tools.RadioButtonAdvStyle.Metro;
			this.radioButtonStandardDeviation.TabIndex = 12;
			this.radioButtonStandardDeviation.Text = "xxStandardDeviation";
			this.radioButtonStandardDeviation.ThemesEnabled = false;
			// 
			// checkBoxUseTweakedValues
			// 
			this.checkBoxUseTweakedValues.BeforeTouchSize = new System.Drawing.Size(407, 24);
			this.checkBoxUseTweakedValues.Dock = System.Windows.Forms.DockStyle.Fill;
			this.checkBoxUseTweakedValues.DrawFocusRectangle = false;
			this.checkBoxUseTweakedValues.Location = new System.Drawing.Point(10, 153);
			this.checkBoxUseTweakedValues.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
			this.checkBoxUseTweakedValues.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxUseTweakedValues.Name = "checkBoxUseTweakedValues";
			this.checkBoxUseTweakedValues.Size = new System.Drawing.Size(407, 24);
			this.checkBoxUseTweakedValues.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Metro;
			this.checkBoxUseTweakedValues.TabIndex = 11;
			this.checkBoxUseTweakedValues.Text = "xxUseTweakedValues";
			this.checkBoxUseTweakedValues.ThemesEnabled = false;
			// 
			// checkBoxUseIntraIntervalDeviation
			// 
			this.checkBoxUseIntraIntervalDeviation.BeforeTouchSize = new System.Drawing.Size(407, 24);
			this.checkBoxUseIntraIntervalDeviation.Dock = System.Windows.Forms.DockStyle.Fill;
			this.checkBoxUseIntraIntervalDeviation.DrawFocusRectangle = false;
			this.checkBoxUseIntraIntervalDeviation.Location = new System.Drawing.Point(10, 123);
			this.checkBoxUseIntraIntervalDeviation.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
			this.checkBoxUseIntraIntervalDeviation.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxUseIntraIntervalDeviation.Name = "checkBoxUseIntraIntervalDeviation";
			this.checkBoxUseIntraIntervalDeviation.Size = new System.Drawing.Size(407, 24);
			this.checkBoxUseIntraIntervalDeviation.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Metro;
			this.checkBoxUseIntraIntervalDeviation.TabIndex = 10;
			this.checkBoxUseIntraIntervalDeviation.Text = "xxUseMaxStddev";
			this.checkBoxUseIntraIntervalDeviation.ThemesEnabled = false;
			// 
			// labelTargetValue
			// 
			this.labelTargetValue.AutoSize = true;
			this.labelTargetValue.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelTargetValue.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelTargetValue.Location = new System.Drawing.Point(3, 3);
			this.labelTargetValue.Margin = new System.Windows.Forms.Padding(3);
			this.labelTargetValue.Name = "labelTargetValue";
			this.labelTargetValue.Size = new System.Drawing.Size(414, 24);
			this.labelTargetValue.TabIndex = 8;
			this.labelTargetValue.Text = "xxTargetValue";
			this.labelTargetValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// radioButtonRootMeanSquare
			// 
			this.radioButtonRootMeanSquare.BeforeTouchSize = new System.Drawing.Size(407, 24);
			this.radioButtonRootMeanSquare.Dock = System.Windows.Forms.DockStyle.Fill;
			this.radioButtonRootMeanSquare.DrawFocusRectangle = false;
			this.radioButtonRootMeanSquare.Location = new System.Drawing.Point(10, 63);
			this.radioButtonRootMeanSquare.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
			this.radioButtonRootMeanSquare.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.radioButtonRootMeanSquare.Name = "radioButtonRootMeanSquare";
			this.radioButtonRootMeanSquare.Size = new System.Drawing.Size(407, 24);
			this.radioButtonRootMeanSquare.Style = Syncfusion.Windows.Forms.Tools.RadioButtonAdvStyle.Metro;
			this.radioButtonRootMeanSquare.TabIndex = 13;
			this.radioButtonRootMeanSquare.Text = "xxRMS";
			this.radioButtonRootMeanSquare.ThemesEnabled = false;
			// 
			// radioButtonTeleopti
			// 
			this.radioButtonTeleopti.BeforeTouchSize = new System.Drawing.Size(407, 24);
			this.radioButtonTeleopti.Dock = System.Windows.Forms.DockStyle.Fill;
			this.radioButtonTeleopti.DrawFocusRectangle = false;
			this.radioButtonTeleopti.Location = new System.Drawing.Point(10, 93);
			this.radioButtonTeleopti.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
			this.radioButtonTeleopti.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.radioButtonTeleopti.Name = "radioButtonTeleopti";
			this.radioButtonTeleopti.Size = new System.Drawing.Size(407, 24);
			this.radioButtonTeleopti.Style = Syncfusion.Windows.Forms.Tools.RadioButtonAdvStyle.Metro;
			this.radioButtonTeleopti.TabIndex = 14;
			this.radioButtonTeleopti.Text = "xxTeleopti";
			this.radioButtonTeleopti.ThemesEnabled = false;
			// 
			// AdvancedPreferencesPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BackColor = System.Drawing.Color.Transparent;
			this.Controls.Add(this.tableLayoutPanel2);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "AdvancedPreferencesPanel";
			this.Size = new System.Drawing.Size(420, 451);
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanelRefreshScreen.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownRefreshRate)).EndInit();
			this.tableLayoutPanel8.ResumeLayout(false);
			this.tableLayoutPanel8.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxMaximumSeats)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxMaximumStaffing)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxMinimumStaffing)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxDoNotBreakMaximumSeats)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxUseAverageShiftLengths)).EndInit();
			this.tableLayoutPanel6.ResumeLayout(false);
			this.tableLayoutPanel6.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonStandardDeviation)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxUseTweakedValues)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxUseIntraIntervalDeviation)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonRootMeanSquare)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonTeleopti)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
		private System.Windows.Forms.Label labelTargetValue;
		private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxUseTweakedValues;
		private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxUseIntraIntervalDeviation;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel8;
		private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxDoNotBreakMaximumSeats;
		private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxMaximumSeats;
		private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxMaximumStaffing;
		private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxMinimumStaffing;
		private System.Windows.Forms.Label labelShiftSelection;
		private Syncfusion.Windows.Forms.Tools.RadioButtonAdv radioButtonStandardDeviation;
		private Syncfusion.Windows.Forms.Tools.RadioButtonAdv radioButtonRootMeanSquare;
		private Syncfusion.Windows.Forms.Tools.RadioButtonAdv radioButtonTeleopti;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelRefreshScreen;
		private System.Windows.Forms.Label labelShift;
		private System.Windows.Forms.NumericUpDown numericUpDownRefreshRate;
		private System.Windows.Forms.Label labelRefreshScreenEveryColon;
		private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxUseAverageShiftLengths;
	}
}
