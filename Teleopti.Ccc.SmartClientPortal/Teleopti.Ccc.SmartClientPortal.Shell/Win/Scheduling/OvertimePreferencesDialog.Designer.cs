using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
	partial class OvertimePreferencesDialog
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OvertimePreferencesDialog));
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
			this.comboBoxShiftBags = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.label6 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.comboBoxAdvActivity = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.label5 = new System.Windows.Forms.Label();
			this.fromToTimeDurationPicker1 = new FromToTimeDurationPicker();
			this.labelSpecifiedPeriod = new System.Windows.Forms.Label();
			this.fromToTimePickerSpecifiedPeriod = new FromToTimePicker();
			this.label7 = new System.Windows.Forms.Label();
			this.comboBoxAdvTag = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.comboBoxAdvOvertimeType = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.checkBoxAllowBreakingMaxTimePerWeek = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.checkBoxAllowBreakingNightlyRest = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.checkBoxAllowBreakingWeeklyRest = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.checkBoxOnAvailableAgentsOnly = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.labelUseSkills = new System.Windows.Forms.Label();
			this.radioButtonAll = new System.Windows.Forms.RadioButton();
			this.radioButtonPrimary = new System.Windows.Forms.RadioButton();
			this.tableLayoutPanelButtons = new System.Windows.Forms.TableLayoutPanel();
			this.buttonOK = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.tabControlTopLevel = new Syncfusion.Windows.Forms.Tools.TabControlAdv();
			this.tabPageGenaral = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanelMain.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxShiftBags)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvActivity)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvTag)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvOvertimeType)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAllowBreakingMaxTimePerWeek)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAllowBreakingNightlyRest)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAllowBreakingWeeklyRest)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxOnAvailableAgentsOnly)).BeginInit();
			this.tableLayoutPanelButtons.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.tabControlTopLevel)).BeginInit();
			this.tabControlTopLevel.SuspendLayout();
			this.tabPageGenaral.SuspendLayout();
			this.tableLayoutPanel3.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(6, 367);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(140, 15);
			this.label1.TabIndex = 5;
			this.label1.Text = "xxTagChangesWithColon";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label2
			// 
			this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(6, 38);
			this.label2.Name = "label2";
			this.label2.Padding = new System.Windows.Forms.Padding(14, 0, 0, 0);
			this.label2.Size = new System.Drawing.Size(124, 15);
			this.label2.TabIndex = 6;
			this.label2.Text = "xxSkillActivityColon";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label4
			// 
			this.label4.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(6, 338);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(137, 15);
			this.label4.TabIndex = 10;
			this.label4.Text = "xxTypeOfOvertimeColon";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// tableLayoutPanelMain
			// 
			this.tableLayoutPanelMain.ColumnCount = 2;
			this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 210F));
			this.tableLayoutPanelMain.Controls.Add(this.comboBoxShiftBags, 1, 8);
			this.tableLayoutPanelMain.Controls.Add(this.label6, 0, 6);
			this.tableLayoutPanelMain.Controls.Add(this.label3, 0, 0);
			this.tableLayoutPanelMain.Controls.Add(this.label2, 0, 1);
			this.tableLayoutPanelMain.Controls.Add(this.comboBoxAdvActivity, 1, 1);
			this.tableLayoutPanelMain.Controls.Add(this.label5, 0, 2);
			this.tableLayoutPanelMain.Controls.Add(this.fromToTimeDurationPicker1, 1, 2);
			this.tableLayoutPanelMain.Controls.Add(this.labelSpecifiedPeriod, 0, 3);
			this.tableLayoutPanelMain.Controls.Add(this.fromToTimePickerSpecifiedPeriod, 1, 3);
			this.tableLayoutPanelMain.Controls.Add(this.label7, 0, 8);
			this.tableLayoutPanelMain.Controls.Add(this.label1, 0, 15);
			this.tableLayoutPanelMain.Controls.Add(this.comboBoxAdvTag, 1, 15);
			this.tableLayoutPanelMain.Controls.Add(this.label4, 0, 14);
			this.tableLayoutPanelMain.Controls.Add(this.comboBoxAdvOvertimeType, 1, 14);
			this.tableLayoutPanelMain.Controls.Add(this.checkBoxAllowBreakingMaxTimePerWeek, 0, 10);
			this.tableLayoutPanelMain.Controls.Add(this.checkBoxAllowBreakingNightlyRest, 0, 11);
			this.tableLayoutPanelMain.Controls.Add(this.checkBoxAllowBreakingWeeklyRest, 0, 12);
			this.tableLayoutPanelMain.Controls.Add(this.checkBoxOnAvailableAgentsOnly, 0, 13);
			this.tableLayoutPanelMain.Controls.Add(this.labelUseSkills, 0, 16);
			this.tableLayoutPanelMain.Controls.Add(this.radioButtonAll, 0, 17);
			this.tableLayoutPanelMain.Controls.Add(this.radioButtonPrimary, 0, 18);
			this.tableLayoutPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelMain.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanelMain.Margin = new System.Windows.Forms.Padding(7);
			this.tableLayoutPanelMain.Name = "tableLayoutPanelMain";
			this.tableLayoutPanelMain.Padding = new System.Windows.Forms.Padding(3);
			this.tableLayoutPanelMain.RowCount = 19;
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 21F));
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 39F));
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
			this.tableLayoutPanelMain.Size = new System.Drawing.Size(494, 520);
			this.tableLayoutPanelMain.TabIndex = 11;
			// 
			// comboBoxShiftBags
			// 
			this.comboBoxShiftBags.BackColor = System.Drawing.Color.White;
			this.comboBoxShiftBags.BeforeTouchSize = new System.Drawing.Size(204, 23);
			this.comboBoxShiftBags.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxShiftBags.Location = new System.Drawing.Point(284, 169);
			this.comboBoxShiftBags.Name = "comboBoxShiftBags";
			this.comboBoxShiftBags.Size = new System.Drawing.Size(204, 23);
			this.comboBoxShiftBags.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.comboBoxShiftBags.TabIndex = 26;
			// 
			// label6
			// 
			this.label6.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(6, 145);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(110, 15);
			this.label6.TabIndex = 27;
			this.label6.Text = "xxNonWorkingDays";
			this.label6.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// label3
			// 
			this.label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label3.AutoSize = true;
			this.tableLayoutPanelMain.SetColumnSpan(this.label3, 2);
			this.label3.Location = new System.Drawing.Point(6, 9);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(116, 15);
			this.label3.TabIndex = 16;
			this.label3.Text = "xxExtendExistingShift";
			this.label3.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// comboBoxAdvActivity
			// 
			this.comboBoxAdvActivity.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.comboBoxAdvActivity.BackColor = System.Drawing.Color.White;
			this.comboBoxAdvActivity.BeforeTouchSize = new System.Drawing.Size(204, 23);
			this.comboBoxAdvActivity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAdvActivity.Location = new System.Drawing.Point(284, 35);
			this.comboBoxAdvActivity.Name = "comboBoxAdvActivity";
			this.comboBoxAdvActivity.Size = new System.Drawing.Size(204, 23);
			this.comboBoxAdvActivity.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.comboBoxAdvActivity.TabIndex = 14;
			// 
			// label5
			// 
			this.label5.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(6, 68);
			this.label5.Name = "label5";
			this.label5.Padding = new System.Windows.Forms.Padding(14, 0, 0, 0);
			this.label5.Size = new System.Drawing.Size(158, 15);
			this.label5.TabIndex = 17;
			this.label5.Text = "xxOvertimeDurationColon";
			// 
			// fromToTimeDurationPicker1
			// 
			this.fromToTimeDurationPicker1.Location = new System.Drawing.Point(284, 63);
			this.fromToTimeDurationPicker1.MinMaxEndTime = ((MinMax<System.TimeSpan>)(resources.GetObject("fromToTimeDurationPicker1.MinMaxEndTime")));
			this.fromToTimeDurationPicker1.MinMaxStartTime = ((MinMax<System.TimeSpan>)(resources.GetObject("fromToTimeDurationPicker1.MinMaxStartTime")));
			this.fromToTimeDurationPicker1.Name = "fromToTimeDurationPicker1";
			this.fromToTimeDurationPicker1.Size = new System.Drawing.Size(204, 26);
			this.fromToTimeDurationPicker1.TabIndex = 18;
			this.fromToTimeDurationPicker1.WholeDayText = "xxNextDay";
			// 
			// labelSpecifiedPeriod
			// 
			this.labelSpecifiedPeriod.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelSpecifiedPeriod.AutoSize = true;
			this.labelSpecifiedPeriod.Location = new System.Drawing.Point(6, 100);
			this.labelSpecifiedPeriod.Name = "labelSpecifiedPeriod";
			this.labelSpecifiedPeriod.Padding = new System.Windows.Forms.Padding(14, 0, 0, 0);
			this.labelSpecifiedPeriod.Size = new System.Drawing.Size(108, 15);
			this.labelSpecifiedPeriod.TabIndex = 23;
			this.labelSpecifiedPeriod.Text = "xxBetweenColon";
			// 
			// fromToTimePickerSpecifiedPeriod
			// 
			this.fromToTimePickerSpecifiedPeriod.Location = new System.Drawing.Point(284, 95);
			this.fromToTimePickerSpecifiedPeriod.MinMaxEndTime = ((MinMax<System.TimeSpan>)(resources.GetObject("fromToTimePickerSpecifiedPeriod.MinMaxEndTime")));
			this.fromToTimePickerSpecifiedPeriod.MinMaxStartTime = ((MinMax<System.TimeSpan>)(resources.GetObject("fromToTimePickerSpecifiedPeriod.MinMaxStartTime")));
			this.fromToTimePickerSpecifiedPeriod.Name = "fromToTimePickerSpecifiedPeriod";
			this.fromToTimePickerSpecifiedPeriod.Size = new System.Drawing.Size(204, 26);
			this.fromToTimePickerSpecifiedPeriod.TabIndex = 24;
			this.fromToTimePickerSpecifiedPeriod.WholeDayCheckboxVisible = true;
			this.fromToTimePickerSpecifiedPeriod.WholeDayText = "xxNextDay";
			// 
			// label7
			// 
			this.label7.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(6, 172);
			this.label7.Name = "label7";
			this.label7.Padding = new System.Windows.Forms.Padding(14, 0, 0, 0);
			this.label7.Size = new System.Drawing.Size(139, 15);
			this.label7.TabIndex = 28;
			this.label7.Text = "xxUseShiftsFromColon";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// comboBoxAdvTag
			// 
			this.comboBoxAdvTag.BackColor = System.Drawing.Color.White;
			this.comboBoxAdvTag.BeforeTouchSize = new System.Drawing.Size(204, 23);
			this.comboBoxAdvTag.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAdvTag.Location = new System.Drawing.Point(284, 363);
			this.comboBoxAdvTag.Name = "comboBoxAdvTag";
			this.comboBoxAdvTag.Size = new System.Drawing.Size(204, 23);
			this.comboBoxAdvTag.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.comboBoxAdvTag.TabIndex = 12;
			// 
			// comboBoxAdvOvertimeType
			// 
			this.comboBoxAdvOvertimeType.BackColor = System.Drawing.Color.White;
			this.comboBoxAdvOvertimeType.BeforeTouchSize = new System.Drawing.Size(204, 23);
			this.comboBoxAdvOvertimeType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAdvOvertimeType.Location = new System.Drawing.Point(284, 334);
			this.comboBoxAdvOvertimeType.Name = "comboBoxAdvOvertimeType";
			this.comboBoxAdvOvertimeType.Size = new System.Drawing.Size(204, 23);
			this.comboBoxAdvOvertimeType.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.comboBoxAdvOvertimeType.TabIndex = 15;
			// 
			// checkBoxAllowBreakingMaxTimePerWeek
			// 
			this.checkBoxAllowBreakingMaxTimePerWeek.BeforeTouchSize = new System.Drawing.Size(482, 18);
			this.tableLayoutPanelMain.SetColumnSpan(this.checkBoxAllowBreakingMaxTimePerWeek, 2);
			this.checkBoxAllowBreakingMaxTimePerWeek.DrawFocusRectangle = false;
			this.checkBoxAllowBreakingMaxTimePerWeek.Location = new System.Drawing.Point(6, 218);
			this.checkBoxAllowBreakingMaxTimePerWeek.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxAllowBreakingMaxTimePerWeek.Name = "checkBoxAllowBreakingMaxTimePerWeek";
			this.checkBoxAllowBreakingMaxTimePerWeek.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
			this.checkBoxAllowBreakingMaxTimePerWeek.Size = new System.Drawing.Size(482, 18);
			this.checkBoxAllowBreakingMaxTimePerWeek.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Metro;
			this.checkBoxAllowBreakingMaxTimePerWeek.TabIndex = 19;
			this.checkBoxAllowBreakingMaxTimePerWeek.Text = "xxAllowBreakContractMaxWorkTimePerWeek";
			this.checkBoxAllowBreakingMaxTimePerWeek.ThemesEnabled = false;
			// 
			// checkBoxAllowBreakingNightlyRest
			// 
			this.checkBoxAllowBreakingNightlyRest.BeforeTouchSize = new System.Drawing.Size(482, 19);
			this.tableLayoutPanelMain.SetColumnSpan(this.checkBoxAllowBreakingNightlyRest, 2);
			this.checkBoxAllowBreakingNightlyRest.DrawFocusRectangle = false;
			this.checkBoxAllowBreakingNightlyRest.Location = new System.Drawing.Point(6, 242);
			this.checkBoxAllowBreakingNightlyRest.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxAllowBreakingNightlyRest.Name = "checkBoxAllowBreakingNightlyRest";
			this.checkBoxAllowBreakingNightlyRest.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
			this.checkBoxAllowBreakingNightlyRest.Size = new System.Drawing.Size(482, 19);
			this.checkBoxAllowBreakingNightlyRest.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Metro;
			this.checkBoxAllowBreakingNightlyRest.TabIndex = 21;
			this.checkBoxAllowBreakingNightlyRest.Text = "xxAllowBreakContractNightlyRest";
			this.checkBoxAllowBreakingNightlyRest.ThemesEnabled = false;
			// 
			// checkBoxAllowBreakingWeeklyRest
			// 
			this.checkBoxAllowBreakingWeeklyRest.BeforeTouchSize = new System.Drawing.Size(482, 19);
			this.tableLayoutPanelMain.SetColumnSpan(this.checkBoxAllowBreakingWeeklyRest, 2);
			this.checkBoxAllowBreakingWeeklyRest.DrawFocusRectangle = false;
			this.checkBoxAllowBreakingWeeklyRest.Location = new System.Drawing.Point(6, 267);
			this.checkBoxAllowBreakingWeeklyRest.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxAllowBreakingWeeklyRest.Name = "checkBoxAllowBreakingWeeklyRest";
			this.checkBoxAllowBreakingWeeklyRest.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
			this.checkBoxAllowBreakingWeeklyRest.Size = new System.Drawing.Size(482, 19);
			this.checkBoxAllowBreakingWeeklyRest.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Metro;
			this.checkBoxAllowBreakingWeeklyRest.TabIndex = 20;
			this.checkBoxAllowBreakingWeeklyRest.Text = "xxAllowBreakContractWeeklyRest";
			this.checkBoxAllowBreakingWeeklyRest.ThemesEnabled = false;
			// 
			// checkBoxOnAvailableAgentsOnly
			// 
			this.checkBoxOnAvailableAgentsOnly.BeforeTouchSize = new System.Drawing.Size(482, 19);
			this.tableLayoutPanelMain.SetColumnSpan(this.checkBoxOnAvailableAgentsOnly, 2);
			this.checkBoxOnAvailableAgentsOnly.DrawFocusRectangle = false;
			this.checkBoxOnAvailableAgentsOnly.Location = new System.Drawing.Point(6, 292);
			this.checkBoxOnAvailableAgentsOnly.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxOnAvailableAgentsOnly.Name = "checkBoxOnAvailableAgentsOnly";
			this.checkBoxOnAvailableAgentsOnly.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
			this.checkBoxOnAvailableAgentsOnly.Size = new System.Drawing.Size(482, 19);
			this.checkBoxOnAvailableAgentsOnly.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Metro;
			this.checkBoxOnAvailableAgentsOnly.TabIndex = 22;
			this.checkBoxOnAvailableAgentsOnly.Text = "xxOnAvailableAgentsOnly";
			this.checkBoxOnAvailableAgentsOnly.ThemesEnabled = false;
			// 
			// labelUseSkills
			// 
			this.labelUseSkills.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.labelUseSkills.AutoSize = true;
			this.labelUseSkills.Location = new System.Drawing.Point(6, 406);
			this.labelUseSkills.Name = "labelUseSkills";
			this.labelUseSkills.Padding = new System.Windows.Forms.Padding(0, 0, 0, 7);
			this.labelUseSkills.Size = new System.Drawing.Size(94, 22);
			this.labelUseSkills.TabIndex = 29;
			this.labelUseSkills.Text = "xxUseSkillsColon";
			this.labelUseSkills.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// radioButtonAll
			// 
			this.radioButtonAll.AutoSize = true;
			this.radioButtonAll.Checked = true;
			this.radioButtonAll.Location = new System.Drawing.Point(6, 431);
			this.radioButtonAll.Name = "radioButtonAll";
			this.radioButtonAll.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.radioButtonAll.Size = new System.Drawing.Size(52, 19);
			this.radioButtonAll.TabIndex = 30;
			this.radioButtonAll.TabStop = true;
			this.radioButtonAll.Text = "xxAll";
			this.radioButtonAll.UseVisualStyleBackColor = true;
			// 
			// radioButtonPrimary
			// 
			this.radioButtonPrimary.AutoSize = true;
			this.radioButtonPrimary.Location = new System.Drawing.Point(6, 456);
			this.radioButtonPrimary.Name = "radioButtonPrimary";
			this.radioButtonPrimary.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.radioButtonPrimary.Size = new System.Drawing.Size(79, 19);
			this.radioButtonPrimary.TabIndex = 31;
			this.radioButtonPrimary.Text = "xxPrimary";
			this.radioButtonPrimary.UseVisualStyleBackColor = true;
			// 
			// tableLayoutPanelButtons
			// 
			this.tableLayoutPanelButtons.ColumnCount = 2;
			this.tableLayoutPanelButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.tableLayoutPanelButtons.Controls.Add(this.buttonOK, 0, 0);
			this.tableLayoutPanelButtons.Controls.Add(this.buttonCancel, 1, 0);
			this.tableLayoutPanelButtons.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelButtons.Location = new System.Drawing.Point(0, 555);
			this.tableLayoutPanelButtons.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanelButtons.Name = "tableLayoutPanelButtons";
			this.tableLayoutPanelButtons.RowCount = 1;
			this.tableLayoutPanelButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelButtons.Size = new System.Drawing.Size(508, 46);
			this.tableLayoutPanelButtons.TabIndex = 13;
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonOK.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonOK.BorderStyleAdv = Syncfusion.Windows.Forms.ButtonAdvBorderStyle.Flat;
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.ForeColor = System.Drawing.Color.White;
			this.buttonOK.IsBackStageButton = false;
			this.buttonOK.Location = new System.Drawing.Point(291, 9);
			this.buttonOK.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(87, 27);
			this.buttonOK.TabIndex = 10;
			this.buttonOK.Text = "xxOk";
			this.buttonOK.UseVisualStyle = true;
			this.buttonOK.Click += new System.EventHandler(this.buttonOkClick);
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonCancel.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.ForeColor = System.Drawing.Color.White;
			this.buttonCancel.IsBackStageButton = false;
			this.buttonCancel.Location = new System.Drawing.Point(411, 9);
			this.buttonCancel.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(87, 27);
			this.buttonCancel.TabIndex = 10;
			this.buttonCancel.Text = "xxCancel";
			this.buttonCancel.UseVisualStyle = true;
			// 
			// tabControlTopLevel
			// 
			this.tabControlTopLevel.ActiveTabColor = System.Drawing.Color.DarkGray;
			this.tabControlTopLevel.BeforeTouchSize = new System.Drawing.Size(502, 549);
			this.tabControlTopLevel.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.tabControlTopLevel.Controls.Add(this.tabPageGenaral);
			this.tabControlTopLevel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControlTopLevel.InactiveTabColor = System.Drawing.Color.White;
			this.tabControlTopLevel.ItemSize = new System.Drawing.Size(59, 22);
			this.tabControlTopLevel.Location = new System.Drawing.Point(3, 3);
			this.tabControlTopLevel.Name = "tabControlTopLevel";
			this.tabControlTopLevel.Size = new System.Drawing.Size(502, 549);
			this.tabControlTopLevel.TabIndex = 14;
			this.tabControlTopLevel.TabPanelBackColor = System.Drawing.Color.White;
			this.tabControlTopLevel.TabStyle = typeof(Syncfusion.Windows.Forms.Tools.TabRendererMetro);
			// 
			// tabPageGenaral
			// 
			this.tabPageGenaral.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.tabPageGenaral.Controls.Add(this.tableLayoutPanelMain);
			this.tabPageGenaral.Image = null;
			this.tabPageGenaral.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageGenaral.Location = new System.Drawing.Point(0, 21);
			this.tabPageGenaral.Name = "tabPageGenaral";
			this.tabPageGenaral.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageGenaral.ShowCloseButton = true;
			this.tabPageGenaral.Size = new System.Drawing.Size(502, 528);
			this.tabPageGenaral.TabIndex = 1;
			this.tabPageGenaral.Text = "xxGeneral";
			this.tabPageGenaral.ThemesEnabled = false;
			// 
			// tableLayoutPanel3
			// 
			this.tableLayoutPanel3.ColumnCount = 1;
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel3.Controls.Add(this.tabControlTopLevel, 0, 0);
			this.tableLayoutPanel3.Controls.Add(this.tableLayoutPanelButtons, 0, 1);
			this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel3.Name = "tableLayoutPanel3";
			this.tableLayoutPanel3.RowCount = 2;
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
			this.tableLayoutPanel3.Size = new System.Drawing.Size(508, 601);
			this.tableLayoutPanel3.TabIndex = 15;
			// 
			// OvertimePreferencesDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BorderColor = System.Drawing.Color.Blue;
			this.CaptionFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ClientSize = new System.Drawing.Size(508, 601);
			this.Controls.Add(this.tableLayoutPanel3);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.HelpButton = false;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(276, 40);
			this.Name = "OvertimePreferencesDialog";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "xxScheduleOvertimeOptions";
			this.Load += new System.EventHandler(this.overtimePreferencesDialogLoad);
			this.tableLayoutPanelMain.ResumeLayout(false);
			this.tableLayoutPanelMain.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxShiftBags)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvActivity)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvTag)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvOvertimeType)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAllowBreakingMaxTimePerWeek)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAllowBreakingNightlyRest)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAllowBreakingWeeklyRest)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxOnAvailableAgentsOnly)).EndInit();
			this.tableLayoutPanelButtons.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.tabControlTopLevel)).EndInit();
			this.tabControlTopLevel.ResumeLayout(false);
			this.tabPageGenaral.ResumeLayout(false);
			this.tableLayoutPanel3.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelMain;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelButtons;
		private Syncfusion.Windows.Forms.ButtonAdv buttonOK;
		private Syncfusion.Windows.Forms.ButtonAdv buttonCancel;
		private Syncfusion.Windows.Forms.Tools.TabControlAdv tabControlTopLevel;
		private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageGenaral;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
		private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvOvertimeType;
		private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvActivity;
		private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvTag;
		private System.Windows.Forms.Label label5;
		private FromToTimeDurationPicker fromToTimeDurationPicker1;
		  private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAllowBreakingNightlyRest;
		  private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAllowBreakingWeeklyRest;
		  private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAllowBreakingMaxTimePerWeek;
		  private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxOnAvailableAgentsOnly;
		  private System.Windows.Forms.Label labelSpecifiedPeriod;
		  private FromToTimePicker fromToTimePickerSpecifiedPeriod;
		private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxShiftBags;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label labelUseSkills;
		private System.Windows.Forms.RadioButton radioButtonAll;
		private System.Windows.Forms.RadioButton radioButtonPrimary;
	}
}