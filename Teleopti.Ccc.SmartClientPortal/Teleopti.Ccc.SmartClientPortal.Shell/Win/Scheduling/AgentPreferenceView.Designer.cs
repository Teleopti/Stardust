﻿using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
	partial class AgentPreferenceView
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxLength"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxDayOff"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxAbsence"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxStandard"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxNextDay"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxShiftCategory"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxExtended"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.TimeSpan.Parse(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxMustHave"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxEnd"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxStart"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxContractTime"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxCancel"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxOk"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxActivity"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Syncfusion.Windows.Forms.Tools.RibbonControlAdv.set_MaximizeToolTip(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxPreference"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.Control.set_Text(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1302:DoNotHardcodeLocaleSpecificStrings", MessageId = "Start menu"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Syncfusion.Windows.Forms.Tools.RibbonControlAdv.set_MinimizeToolTip(System.String)")]
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.tabPageAdvActivity = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.tableLayoutPanelActivity = new System.Windows.Forms.TableLayoutPanel();
			this.comboBoxAdvActivity = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.label8 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.outlookTimePickerActivityEndMin = new OutlookTimePicker();
			this.outlookTimePickerActivityEndMax = new OutlookTimePicker();
			this.label9 = new System.Windows.Forms.Label();
			this.outlookTimePickerActivityLengthMin = new OutlookTimePicker();
			this.outlookTimePickerActivityLengthMax = new OutlookTimePicker();
			this.label10 = new System.Windows.Forms.Label();
			this.outlookTimePickerActivityStartMin = new OutlookTimePicker();
			this.outlookTimePickerActivityStartMax = new OutlookTimePicker();
			this.tabPageAdvExtended = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.tableLayoutPanelExtended = new System.Windows.Forms.TableLayoutPanel();
			this.label5 = new System.Windows.Forms.Label();
			this.outlookTimePickerContractShiftCategoryMin = new OutlookTimePicker();
			this.outlookTimePickerContractShiftCategoryMax = new OutlookTimePicker();
			this.label6 = new System.Windows.Forms.Label();
			this.outlookTimePickerShiftCategoryStartMin = new OutlookTimePicker();
			this.outlookTimePickerShiftCategoryStartMax = new OutlookTimePicker();
			this.label7 = new System.Windows.Forms.Label();
			this.outlookTimePickerShiftCategoryEndMin = new OutlookTimePicker();
			this.checkBoxAdvShiftCategoryNextDayMin = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.outlookTimePickerShiftCategoryEndMax = new OutlookTimePicker();
			this.checkBoxAdvShiftCategoryNextDayMax = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.buttonAdvOk = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.tabPageAdvStandard = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.tableLayoutPanelStandard = new System.Windows.Forms.TableLayoutPanel();
			this.labelShiftCategory = new System.Windows.Forms.Label();
			this.comboBoxAdvShiftCategory = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.comboBoxAdvAbsence = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.labelAbsence = new System.Windows.Forms.Label();
			this.labelDayOff = new System.Windows.Forms.Label();
			this.comboBoxAdvDayOff = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.checkBoxMustHave = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.tabControlAgentInfo = new Syncfusion.Windows.Forms.Tools.TabControlAdv();
			this.errorProviderExtended = new System.Windows.Forms.ErrorProvider(this.components);
			this.errorProviderActivity = new System.Windows.Forms.ErrorProvider(this.components);
			this.tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanelButtons = new System.Windows.Forms.TableLayoutPanel();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.tabPageAdvActivity.SuspendLayout();
			this.tableLayoutPanelActivity.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvActivity)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerActivityEndMin)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerActivityEndMax)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerActivityLengthMin)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerActivityLengthMax)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerActivityStartMin)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerActivityStartMax)).BeginInit();
			this.tabPageAdvExtended.SuspendLayout();
			this.tableLayoutPanelExtended.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerContractShiftCategoryMin)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerContractShiftCategoryMax)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerShiftCategoryStartMin)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerShiftCategoryStartMax)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerShiftCategoryEndMin)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvShiftCategoryNextDayMin)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerShiftCategoryEndMax)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvShiftCategoryNextDayMax)).BeginInit();
			this.tabPageAdvStandard.SuspendLayout();
			this.tableLayoutPanelStandard.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvShiftCategory)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvAbsence)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvDayOff)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxMustHave)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.tabControlAgentInfo)).BeginInit();
			this.tabControlAgentInfo.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.errorProviderExtended)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.errorProviderActivity)).BeginInit();
			this.tableLayoutPanelMain.SuspendLayout();
			this.tableLayoutPanelButtons.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabPageAdvActivity
			// 
			this.tabPageAdvActivity.BackColor = System.Drawing.Color.Transparent;
			this.tabPageAdvActivity.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.tabPageAdvActivity.Controls.Add(this.tableLayoutPanelActivity);
			this.tabPageAdvActivity.Image = null;
			this.tabPageAdvActivity.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageAdvActivity.Location = new System.Drawing.Point(0, 23);
			this.tabPageAdvActivity.Name = "tabPageAdvActivity";
			this.tabPageAdvActivity.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageAdvActivity.ShowCloseButton = true;
			this.tabPageAdvActivity.Size = new System.Drawing.Size(451, 304);
			this.tabPageAdvActivity.TabIndex = 3;
			this.tabPageAdvActivity.Text = "xxActivity";
			this.tabPageAdvActivity.ThemesEnabled = false;
			// 
			// tableLayoutPanelActivity
			// 
			this.tableLayoutPanelActivity.ColumnCount = 5;
			this.tableLayoutPanelActivity.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 128F));
			this.tableLayoutPanelActivity.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 93F));
			this.tableLayoutPanelActivity.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 64F));
			this.tableLayoutPanelActivity.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 93F));
			this.tableLayoutPanelActivity.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 69F));
			this.tableLayoutPanelActivity.Controls.Add(this.comboBoxAdvActivity, 1, 1);
			this.tableLayoutPanelActivity.Controls.Add(this.label8, 0, 1);
			this.tableLayoutPanelActivity.Controls.Add(this.label11, 0, 3);
			this.tableLayoutPanelActivity.Controls.Add(this.outlookTimePickerActivityEndMin, 1, 3);
			this.tableLayoutPanelActivity.Controls.Add(this.outlookTimePickerActivityEndMax, 3, 3);
			this.tableLayoutPanelActivity.Controls.Add(this.label9, 0, 4);
			this.tableLayoutPanelActivity.Controls.Add(this.outlookTimePickerActivityLengthMin, 1, 4);
			this.tableLayoutPanelActivity.Controls.Add(this.outlookTimePickerActivityLengthMax, 3, 4);
			this.tableLayoutPanelActivity.Controls.Add(this.label10, 0, 2);
			this.tableLayoutPanelActivity.Controls.Add(this.outlookTimePickerActivityStartMin, 1, 2);
			this.tableLayoutPanelActivity.Controls.Add(this.outlookTimePickerActivityStartMax, 3, 2);
			this.tableLayoutPanelActivity.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelActivity.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanelActivity.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanelActivity.Name = "tableLayoutPanelActivity";
			this.tableLayoutPanelActivity.RowCount = 6;
			this.tableLayoutPanelActivity.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
			this.tableLayoutPanelActivity.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
			this.tableLayoutPanelActivity.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
			this.tableLayoutPanelActivity.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
			this.tableLayoutPanelActivity.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
			this.tableLayoutPanelActivity.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
			this.tableLayoutPanelActivity.Size = new System.Drawing.Size(443, 296);
			this.tableLayoutPanelActivity.TabIndex = 1;
			// 
			// comboBoxAdvActivity
			// 
			this.comboBoxAdvActivity.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.comboBoxAdvActivity.BackColor = System.Drawing.Color.White;
			this.comboBoxAdvActivity.BeforeTouchSize = new System.Drawing.Size(243, 23);
			this.tableLayoutPanelActivity.SetColumnSpan(this.comboBoxAdvActivity, 3);
			this.comboBoxAdvActivity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAdvActivity.Location = new System.Drawing.Point(131, 35);
			this.comboBoxAdvActivity.Name = "comboBoxAdvActivity";
			this.comboBoxAdvActivity.Size = new System.Drawing.Size(243, 23);
			this.comboBoxAdvActivity.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.comboBoxAdvActivity.TabIndex = 1;
			this.comboBoxAdvActivity.SelectedIndexChanged += new System.EventHandler(this.comboBoxAdvActivitySelectedIndexChanged);
			// 
			// label8
			// 
			this.label8.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(3, 38);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(57, 15);
			this.label8.TabIndex = 0;
			this.label8.Text = "xxActivity";
			// 
			// label11
			// 
			this.label11.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(3, 130);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(37, 15);
			this.label11.TabIndex = 7;
			this.label11.Text = "xxEnd";
			// 
			// outlookTimePickerActivityEndMin
			// 
			this.outlookTimePickerActivityEndMin.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.outlookTimePickerActivityEndMin.BackColor = System.Drawing.Color.White;
			this.outlookTimePickerActivityEndMin.BeforeTouchSize = new System.Drawing.Size(86, 23);
			this.outlookTimePickerActivityEndMin.DefaultResolution = 0;
			this.outlookTimePickerActivityEndMin.EnableNull = true;
			this.outlookTimePickerActivityEndMin.FormatFromCulture = true;
			this.outlookTimePickerActivityEndMin.Location = new System.Drawing.Point(131, 127);
			this.outlookTimePickerActivityEndMin.MaxTime = System.TimeSpan.Parse("23:59:00");
			this.outlookTimePickerActivityEndMin.Name = "outlookTimePickerActivityEndMin";
			this.outlookTimePickerActivityEndMin.Size = new System.Drawing.Size(86, 23);
			this.outlookTimePickerActivityEndMin.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.outlookTimePickerActivityEndMin.TabIndex = 4;
			this.outlookTimePickerActivityEndMin.Text = "00:00";
			this.outlookTimePickerActivityEndMin.TextChanged += new System.EventHandler(this.outlookTimePickerActivityEndMinTextChanged);
			// 
			// outlookTimePickerActivityEndMax
			// 
			this.outlookTimePickerActivityEndMax.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.outlookTimePickerActivityEndMax.BackColor = System.Drawing.Color.White;
			this.outlookTimePickerActivityEndMax.BeforeTouchSize = new System.Drawing.Size(86, 23);
			this.outlookTimePickerActivityEndMax.DefaultResolution = 0;
			this.outlookTimePickerActivityEndMax.EnableNull = true;
			this.outlookTimePickerActivityEndMax.FormatFromCulture = true;
			this.outlookTimePickerActivityEndMax.Location = new System.Drawing.Point(288, 127);
			this.outlookTimePickerActivityEndMax.MaxTime = System.TimeSpan.Parse("23:59:00");
			this.outlookTimePickerActivityEndMax.Name = "outlookTimePickerActivityEndMax";
			this.outlookTimePickerActivityEndMax.Size = new System.Drawing.Size(86, 23);
			this.outlookTimePickerActivityEndMax.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.outlookTimePickerActivityEndMax.TabIndex = 5;
			this.outlookTimePickerActivityEndMax.Text = "00:00";
			this.outlookTimePickerActivityEndMax.TextChanged += new System.EventHandler(this.outlookTimePickerActivityEndMaxTextChanged);
			// 
			// label9
			// 
			this.label9.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(3, 176);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(54, 15);
			this.label9.TabIndex = 5;
			this.label9.Text = "xxLength";
			// 
			// outlookTimePickerActivityLengthMin
			// 
			this.outlookTimePickerActivityLengthMin.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.outlookTimePickerActivityLengthMin.BackColor = System.Drawing.Color.White;
			this.outlookTimePickerActivityLengthMin.BeforeTouchSize = new System.Drawing.Size(86, 23);
			this.outlookTimePickerActivityLengthMin.DefaultResolution = 0;
			this.outlookTimePickerActivityLengthMin.EnableNull = true;
			this.outlookTimePickerActivityLengthMin.FormatFromCulture = true;
			this.outlookTimePickerActivityLengthMin.Location = new System.Drawing.Point(131, 173);
			this.outlookTimePickerActivityLengthMin.MaxTime = System.TimeSpan.Parse("23:59:00");
			this.outlookTimePickerActivityLengthMin.Name = "outlookTimePickerActivityLengthMin";
			this.outlookTimePickerActivityLengthMin.Size = new System.Drawing.Size(86, 23);
			this.outlookTimePickerActivityLengthMin.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.outlookTimePickerActivityLengthMin.TabIndex = 6;
			this.outlookTimePickerActivityLengthMin.Text = "00:00";
			this.outlookTimePickerActivityLengthMin.TextChanged += new System.EventHandler(this.outlookTimePickerActivityLengthMinTextChanged);
			// 
			// outlookTimePickerActivityLengthMax
			// 
			this.outlookTimePickerActivityLengthMax.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.outlookTimePickerActivityLengthMax.BackColor = System.Drawing.Color.White;
			this.outlookTimePickerActivityLengthMax.BeforeTouchSize = new System.Drawing.Size(86, 23);
			this.outlookTimePickerActivityLengthMax.DefaultResolution = 0;
			this.outlookTimePickerActivityLengthMax.EnableNull = true;
			this.outlookTimePickerActivityLengthMax.FormatFromCulture = true;
			this.outlookTimePickerActivityLengthMax.Location = new System.Drawing.Point(288, 173);
			this.outlookTimePickerActivityLengthMax.MaxTime = System.TimeSpan.Parse("23:59:00");
			this.outlookTimePickerActivityLengthMax.Name = "outlookTimePickerActivityLengthMax";
			this.outlookTimePickerActivityLengthMax.Size = new System.Drawing.Size(86, 23);
			this.outlookTimePickerActivityLengthMax.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.outlookTimePickerActivityLengthMax.TabIndex = 7;
			this.outlookTimePickerActivityLengthMax.Text = "00:00";
			this.outlookTimePickerActivityLengthMax.TextChanged += new System.EventHandler(this.outlookTimePickerActivityLengthMaxTextChanged);
			// 
			// label10
			// 
			this.label10.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(3, 84);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(41, 15);
			this.label10.TabIndex = 6;
			this.label10.Text = "xxStart";
			// 
			// outlookTimePickerActivityStartMin
			// 
			this.outlookTimePickerActivityStartMin.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.outlookTimePickerActivityStartMin.BackColor = System.Drawing.Color.White;
			this.outlookTimePickerActivityStartMin.BeforeTouchSize = new System.Drawing.Size(86, 23);
			this.outlookTimePickerActivityStartMin.DefaultResolution = 0;
			this.outlookTimePickerActivityStartMin.EnableNull = true;
			this.outlookTimePickerActivityStartMin.FormatFromCulture = true;
			this.outlookTimePickerActivityStartMin.Location = new System.Drawing.Point(131, 81);
			this.outlookTimePickerActivityStartMin.MaxTime = System.TimeSpan.Parse("23:59:00");
			this.outlookTimePickerActivityStartMin.Name = "outlookTimePickerActivityStartMin";
			this.outlookTimePickerActivityStartMin.Size = new System.Drawing.Size(86, 23);
			this.outlookTimePickerActivityStartMin.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.outlookTimePickerActivityStartMin.TabIndex = 2;
			this.outlookTimePickerActivityStartMin.Text = "00:00";
			this.outlookTimePickerActivityStartMin.TextChanged += new System.EventHandler(this.outlookTimePickerActivityStartMinTextChanged);
			// 
			// outlookTimePickerActivityStartMax
			// 
			this.outlookTimePickerActivityStartMax.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.outlookTimePickerActivityStartMax.BackColor = System.Drawing.Color.White;
			this.outlookTimePickerActivityStartMax.BeforeTouchSize = new System.Drawing.Size(86, 23);
			this.outlookTimePickerActivityStartMax.DefaultResolution = 0;
			this.outlookTimePickerActivityStartMax.EnableNull = true;
			this.outlookTimePickerActivityStartMax.FormatFromCulture = true;
			this.outlookTimePickerActivityStartMax.Location = new System.Drawing.Point(288, 81);
			this.outlookTimePickerActivityStartMax.MaxTime = System.TimeSpan.Parse("23:59:00");
			this.outlookTimePickerActivityStartMax.Name = "outlookTimePickerActivityStartMax";
			this.outlookTimePickerActivityStartMax.Size = new System.Drawing.Size(86, 23);
			this.outlookTimePickerActivityStartMax.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.outlookTimePickerActivityStartMax.TabIndex = 3;
			this.outlookTimePickerActivityStartMax.Text = "00:00";
			this.outlookTimePickerActivityStartMax.TextChanged += new System.EventHandler(this.outlookTimePickerActivityStartMaxTextChanged);
			// 
			// tabPageAdvExtended
			// 
			this.tabPageAdvExtended.BackColor = System.Drawing.Color.Transparent;
			this.tabPageAdvExtended.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.tabPageAdvExtended.Controls.Add(this.tableLayoutPanelExtended);
			this.tabPageAdvExtended.Image = null;
			this.tabPageAdvExtended.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageAdvExtended.Location = new System.Drawing.Point(0, 23);
			this.tabPageAdvExtended.Name = "tabPageAdvExtended";
			this.tabPageAdvExtended.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageAdvExtended.ShowCloseButton = true;
			this.tabPageAdvExtended.Size = new System.Drawing.Size(451, 304);
			this.tabPageAdvExtended.TabIndex = 4;
			this.tabPageAdvExtended.Text = "xxExtended";
			this.tabPageAdvExtended.ThemesEnabled = false;
			// 
			// tableLayoutPanelExtended
			// 
			this.tableLayoutPanelExtended.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanelExtended.ColumnCount = 5;
			this.tableLayoutPanelExtended.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 128F));
			this.tableLayoutPanelExtended.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 93F));
			this.tableLayoutPanelExtended.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 64F));
			this.tableLayoutPanelExtended.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 93F));
			this.tableLayoutPanelExtended.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 69F));
			this.tableLayoutPanelExtended.Controls.Add(this.label5, 0, 3);
			this.tableLayoutPanelExtended.Controls.Add(this.outlookTimePickerContractShiftCategoryMin, 1, 3);
			this.tableLayoutPanelExtended.Controls.Add(this.outlookTimePickerContractShiftCategoryMax, 3, 3);
			this.tableLayoutPanelExtended.Controls.Add(this.label6, 0, 1);
			this.tableLayoutPanelExtended.Controls.Add(this.outlookTimePickerShiftCategoryStartMin, 1, 1);
			this.tableLayoutPanelExtended.Controls.Add(this.outlookTimePickerShiftCategoryStartMax, 3, 1);
			this.tableLayoutPanelExtended.Controls.Add(this.label7, 0, 2);
			this.tableLayoutPanelExtended.Controls.Add(this.outlookTimePickerShiftCategoryEndMin, 1, 2);
			this.tableLayoutPanelExtended.Controls.Add(this.checkBoxAdvShiftCategoryNextDayMin, 2, 2);
			this.tableLayoutPanelExtended.Controls.Add(this.outlookTimePickerShiftCategoryEndMax, 3, 2);
			this.tableLayoutPanelExtended.Controls.Add(this.checkBoxAdvShiftCategoryNextDayMax, 4, 2);
			this.tableLayoutPanelExtended.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelExtended.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanelExtended.Name = "tableLayoutPanelExtended";
			this.tableLayoutPanelExtended.RowCount = 5;
			this.tableLayoutPanelExtended.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
			this.tableLayoutPanelExtended.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
			this.tableLayoutPanelExtended.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
			this.tableLayoutPanelExtended.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
			this.tableLayoutPanelExtended.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelExtended.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanelExtended.Size = new System.Drawing.Size(443, 296);
			this.tableLayoutPanelExtended.TabIndex = 0;
			// 
			// label5
			// 
			this.label5.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(3, 130);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(90, 15);
			this.label5.TabIndex = 5;
			this.label5.Text = "xxContractTime";
			// 
			// outlookTimePickerContractShiftCategoryMin
			// 
			this.outlookTimePickerContractShiftCategoryMin.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.outlookTimePickerContractShiftCategoryMin.BackColor = System.Drawing.Color.White;
			this.outlookTimePickerContractShiftCategoryMin.BeforeTouchSize = new System.Drawing.Size(86, 23);
			this.outlookTimePickerContractShiftCategoryMin.DefaultResolution = 0;
			this.outlookTimePickerContractShiftCategoryMin.EnableNull = true;
			this.outlookTimePickerContractShiftCategoryMin.FormatFromCulture = true;
			this.outlookTimePickerContractShiftCategoryMin.Location = new System.Drawing.Point(131, 127);
			this.outlookTimePickerContractShiftCategoryMin.MaxTime = System.TimeSpan.Parse("23:59:00");
			this.outlookTimePickerContractShiftCategoryMin.Name = "outlookTimePickerContractShiftCategoryMin";
			this.outlookTimePickerContractShiftCategoryMin.Size = new System.Drawing.Size(86, 23);
			this.outlookTimePickerContractShiftCategoryMin.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.outlookTimePickerContractShiftCategoryMin.TabIndex = 7;
			this.outlookTimePickerContractShiftCategoryMin.Text = "00:00";
			this.outlookTimePickerContractShiftCategoryMin.TextChanged += new System.EventHandler(this.outlookTimePickerContractShiftCategoryMinTextChanged);
			// 
			// outlookTimePickerContractShiftCategoryMax
			// 
			this.outlookTimePickerContractShiftCategoryMax.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.outlookTimePickerContractShiftCategoryMax.BackColor = System.Drawing.Color.White;
			this.outlookTimePickerContractShiftCategoryMax.BeforeTouchSize = new System.Drawing.Size(86, 23);
			this.outlookTimePickerContractShiftCategoryMax.DefaultResolution = 0;
			this.outlookTimePickerContractShiftCategoryMax.EnableNull = true;
			this.outlookTimePickerContractShiftCategoryMax.FormatFromCulture = true;
			this.outlookTimePickerContractShiftCategoryMax.Location = new System.Drawing.Point(288, 127);
			this.outlookTimePickerContractShiftCategoryMax.MaxTime = System.TimeSpan.Parse("23:59:00");
			this.outlookTimePickerContractShiftCategoryMax.Name = "outlookTimePickerContractShiftCategoryMax";
			this.outlookTimePickerContractShiftCategoryMax.Size = new System.Drawing.Size(86, 23);
			this.outlookTimePickerContractShiftCategoryMax.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.outlookTimePickerContractShiftCategoryMax.TabIndex = 8;
			this.outlookTimePickerContractShiftCategoryMax.Text = "00:00";
			this.outlookTimePickerContractShiftCategoryMax.TextChanged += new System.EventHandler(this.outlookTimePickerContractShiftCategoryMaxTextChanged);
			// 
			// label6
			// 
			this.label6.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(3, 38);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(41, 15);
			this.label6.TabIndex = 6;
			this.label6.Text = "xxStart";
			// 
			// outlookTimePickerShiftCategoryStartMin
			// 
			this.outlookTimePickerShiftCategoryStartMin.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.outlookTimePickerShiftCategoryStartMin.BackColor = System.Drawing.Color.White;
			this.outlookTimePickerShiftCategoryStartMin.BeforeTouchSize = new System.Drawing.Size(86, 23);
			this.outlookTimePickerShiftCategoryStartMin.DefaultResolution = 0;
			this.outlookTimePickerShiftCategoryStartMin.EnableNull = true;
			this.outlookTimePickerShiftCategoryStartMin.FormatFromCulture = true;
			this.outlookTimePickerShiftCategoryStartMin.Location = new System.Drawing.Point(131, 35);
			this.outlookTimePickerShiftCategoryStartMin.MaxTime = System.TimeSpan.Parse("23:59:00");
			this.outlookTimePickerShiftCategoryStartMin.Name = "outlookTimePickerShiftCategoryStartMin";
			this.outlookTimePickerShiftCategoryStartMin.Size = new System.Drawing.Size(86, 23);
			this.outlookTimePickerShiftCategoryStartMin.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.outlookTimePickerShiftCategoryStartMin.TabIndex = 1;
			this.outlookTimePickerShiftCategoryStartMin.Text = "00:00";
			this.outlookTimePickerShiftCategoryStartMin.TextChanged += new System.EventHandler(this.outlookTimePickerShiftCategoryStartMinTextChanged);
			// 
			// outlookTimePickerShiftCategoryStartMax
			// 
			this.outlookTimePickerShiftCategoryStartMax.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.outlookTimePickerShiftCategoryStartMax.BackColor = System.Drawing.Color.White;
			this.outlookTimePickerShiftCategoryStartMax.BeforeTouchSize = new System.Drawing.Size(86, 23);
			this.outlookTimePickerShiftCategoryStartMax.DefaultResolution = 0;
			this.outlookTimePickerShiftCategoryStartMax.EnableNull = true;
			this.outlookTimePickerShiftCategoryStartMax.FormatFromCulture = true;
			this.outlookTimePickerShiftCategoryStartMax.Location = new System.Drawing.Point(288, 35);
			this.outlookTimePickerShiftCategoryStartMax.MaxTime = System.TimeSpan.Parse("23:59:00");
			this.outlookTimePickerShiftCategoryStartMax.Name = "outlookTimePickerShiftCategoryStartMax";
			this.outlookTimePickerShiftCategoryStartMax.Size = new System.Drawing.Size(86, 23);
			this.outlookTimePickerShiftCategoryStartMax.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.outlookTimePickerShiftCategoryStartMax.TabIndex = 2;
			this.outlookTimePickerShiftCategoryStartMax.Text = "00:00";
			this.outlookTimePickerShiftCategoryStartMax.TextChanged += new System.EventHandler(this.outlookTimePickerShiftCategoryStartMaxTextChanged);
			// 
			// label7
			// 
			this.label7.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(3, 84);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(37, 15);
			this.label7.TabIndex = 7;
			this.label7.Text = "xxEnd";
			// 
			// outlookTimePickerShiftCategoryEndMin
			// 
			this.outlookTimePickerShiftCategoryEndMin.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.outlookTimePickerShiftCategoryEndMin.BackColor = System.Drawing.Color.White;
			this.outlookTimePickerShiftCategoryEndMin.BeforeTouchSize = new System.Drawing.Size(86, 23);
			this.outlookTimePickerShiftCategoryEndMin.DefaultResolution = 0;
			this.outlookTimePickerShiftCategoryEndMin.EnableNull = true;
			this.outlookTimePickerShiftCategoryEndMin.FormatFromCulture = true;
			this.outlookTimePickerShiftCategoryEndMin.Location = new System.Drawing.Point(131, 81);
			this.outlookTimePickerShiftCategoryEndMin.MaxTime = System.TimeSpan.Parse("23:59:00");
			this.outlookTimePickerShiftCategoryEndMin.Name = "outlookTimePickerShiftCategoryEndMin";
			this.outlookTimePickerShiftCategoryEndMin.Size = new System.Drawing.Size(86, 23);
			this.outlookTimePickerShiftCategoryEndMin.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.outlookTimePickerShiftCategoryEndMin.TabIndex = 3;
			this.outlookTimePickerShiftCategoryEndMin.Text = "00:00";
			this.outlookTimePickerShiftCategoryEndMin.TextChanged += new System.EventHandler(this.outlookTimePickerShiftCategoryEndMinTextChanged);
			// 
			// checkBoxAdvShiftCategoryNextDayMin
			// 
			this.checkBoxAdvShiftCategoryNextDayMin.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.checkBoxAdvShiftCategoryNextDayMin.BeforeTouchSize = new System.Drawing.Size(19, 24);
			this.checkBoxAdvShiftCategoryNextDayMin.DrawFocusRectangle = false;
			this.checkBoxAdvShiftCategoryNextDayMin.Location = new System.Drawing.Point(224, 80);
			this.checkBoxAdvShiftCategoryNextDayMin.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxAdvShiftCategoryNextDayMin.Name = "checkBoxAdvShiftCategoryNextDayMin";
			this.checkBoxAdvShiftCategoryNextDayMin.Size = new System.Drawing.Size(19, 24);
			this.checkBoxAdvShiftCategoryNextDayMin.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Metro;
			this.checkBoxAdvShiftCategoryNextDayMin.TabIndex = 4;
			this.checkBoxAdvShiftCategoryNextDayMin.ThemesEnabled = false;
			this.checkBoxAdvShiftCategoryNextDayMin.CheckStateChanged += new System.EventHandler(this.checkBoxAdvShiftCategoryNextDayMinCheckStateChanged);
			// 
			// outlookTimePickerShiftCategoryEndMax
			// 
			this.outlookTimePickerShiftCategoryEndMax.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.outlookTimePickerShiftCategoryEndMax.BackColor = System.Drawing.Color.White;
			this.outlookTimePickerShiftCategoryEndMax.BeforeTouchSize = new System.Drawing.Size(86, 23);
			this.outlookTimePickerShiftCategoryEndMax.DefaultResolution = 0;
			this.outlookTimePickerShiftCategoryEndMax.EnableNull = true;
			this.outlookTimePickerShiftCategoryEndMax.FormatFromCulture = true;
			this.outlookTimePickerShiftCategoryEndMax.Location = new System.Drawing.Point(288, 81);
			this.outlookTimePickerShiftCategoryEndMax.MaxTime = System.TimeSpan.Parse("23:59:00");
			this.outlookTimePickerShiftCategoryEndMax.Name = "outlookTimePickerShiftCategoryEndMax";
			this.outlookTimePickerShiftCategoryEndMax.Size = new System.Drawing.Size(86, 23);
			this.outlookTimePickerShiftCategoryEndMax.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.outlookTimePickerShiftCategoryEndMax.TabIndex = 5;
			this.outlookTimePickerShiftCategoryEndMax.Text = "00:00";
			this.outlookTimePickerShiftCategoryEndMax.TextChanged += new System.EventHandler(this.outlookTimePickerShiftCategoryEndMaxTextChanged);
			// 
			// checkBoxAdvShiftCategoryNextDayMax
			// 
			this.checkBoxAdvShiftCategoryNextDayMax.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.checkBoxAdvShiftCategoryNextDayMax.BeforeTouchSize = new System.Drawing.Size(27, 24);
			this.checkBoxAdvShiftCategoryNextDayMax.DrawFocusRectangle = false;
			this.checkBoxAdvShiftCategoryNextDayMax.Location = new System.Drawing.Point(381, 80);
			this.checkBoxAdvShiftCategoryNextDayMax.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxAdvShiftCategoryNextDayMax.Name = "checkBoxAdvShiftCategoryNextDayMax";
			this.checkBoxAdvShiftCategoryNextDayMax.Size = new System.Drawing.Size(27, 24);
			this.checkBoxAdvShiftCategoryNextDayMax.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Metro;
			this.checkBoxAdvShiftCategoryNextDayMax.TabIndex = 6;
			this.checkBoxAdvShiftCategoryNextDayMax.ThemesEnabled = false;
			this.checkBoxAdvShiftCategoryNextDayMax.CheckStateChanged += new System.EventHandler(this.checkBoxAdvShiftCategoryNextDayMaxCheckStateChanged);
			// 
			// buttonAdvOk
			// 
			this.buttonAdvOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvOk.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvOk.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvOk.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonAdvOk.ForeColor = System.Drawing.Color.White;
			this.buttonAdvOk.IsBackStageButton = false;
			this.buttonAdvOk.Location = new System.Drawing.Point(240, 16);
			this.buttonAdvOk.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonAdvOk.Name = "buttonAdvOk";
			this.buttonAdvOk.Size = new System.Drawing.Size(87, 27);
			this.buttonAdvOk.TabIndex = 11;
			this.buttonAdvOk.Text = "xxOk";
			this.buttonAdvOk.UseVisualStyle = true;
			this.buttonAdvOk.Click += new System.EventHandler(this.buttonAdvOkClick);
			// 
			// buttonAdvCancel
			// 
			this.buttonAdvCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvCancel.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonAdvCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonAdvCancel.ForeColor = System.Drawing.Color.White;
			this.buttonAdvCancel.IsBackStageButton = false;
			this.buttonAdvCancel.Location = new System.Drawing.Point(360, 16);
			this.buttonAdvCancel.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonAdvCancel.Name = "buttonAdvCancel";
			this.buttonAdvCancel.Size = new System.Drawing.Size(87, 27);
			this.buttonAdvCancel.TabIndex = 12;
			this.buttonAdvCancel.Text = "xxCancel";
			this.buttonAdvCancel.UseVisualStyle = true;
			this.buttonAdvCancel.Click += new System.EventHandler(this.buttonAdvCancelClick);
			// 
			// tabPageAdvStandard
			// 
			this.tabPageAdvStandard.BackColor = System.Drawing.Color.Transparent;
			this.tabPageAdvStandard.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.tabPageAdvStandard.Controls.Add(this.tableLayoutPanelStandard);
			this.tabPageAdvStandard.Image = null;
			this.tabPageAdvStandard.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageAdvStandard.Location = new System.Drawing.Point(0, 23);
			this.tabPageAdvStandard.Name = "tabPageAdvStandard";
			this.tabPageAdvStandard.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageAdvStandard.ShowCloseButton = true;
			this.tabPageAdvStandard.Size = new System.Drawing.Size(451, 304);
			this.tabPageAdvStandard.TabIndex = 2;
			this.tabPageAdvStandard.Text = "xxStandard";
			this.tabPageAdvStandard.ThemesEnabled = false;
			// 
			// tableLayoutPanelStandard
			// 
			this.tableLayoutPanelStandard.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanelStandard.ColumnCount = 2;
			this.tableLayoutPanelStandard.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 128F));
			this.tableLayoutPanelStandard.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelStandard.Controls.Add(this.labelShiftCategory, 0, 1);
			this.tableLayoutPanelStandard.Controls.Add(this.comboBoxAdvShiftCategory, 1, 1);
			this.tableLayoutPanelStandard.Controls.Add(this.comboBoxAdvAbsence, 1, 3);
			this.tableLayoutPanelStandard.Controls.Add(this.labelAbsence, 0, 3);
			this.tableLayoutPanelStandard.Controls.Add(this.labelDayOff, 0, 2);
			this.tableLayoutPanelStandard.Controls.Add(this.comboBoxAdvDayOff, 1, 2);
			this.tableLayoutPanelStandard.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelStandard.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanelStandard.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanelStandard.Name = "tableLayoutPanelStandard";
			this.tableLayoutPanelStandard.RowCount = 5;
			this.tableLayoutPanelStandard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
			this.tableLayoutPanelStandard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
			this.tableLayoutPanelStandard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
			this.tableLayoutPanelStandard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
			this.tableLayoutPanelStandard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
			this.tableLayoutPanelStandard.Size = new System.Drawing.Size(443, 296);
			this.tableLayoutPanelStandard.TabIndex = 0;
			// 
			// labelShiftCategory
			// 
			this.labelShiftCategory.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelShiftCategory.AutoSize = true;
			this.labelShiftCategory.Location = new System.Drawing.Point(3, 38);
			this.labelShiftCategory.Name = "labelShiftCategory";
			this.labelShiftCategory.Size = new System.Drawing.Size(89, 15);
			this.labelShiftCategory.TabIndex = 0;
			this.labelShiftCategory.Text = "xxShiftCategory";
			// 
			// comboBoxAdvShiftCategory
			// 
			this.comboBoxAdvShiftCategory.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.comboBoxAdvShiftCategory.BackColor = System.Drawing.Color.White;
			this.comboBoxAdvShiftCategory.BeforeTouchSize = new System.Drawing.Size(245, 23);
			this.comboBoxAdvShiftCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAdvShiftCategory.Location = new System.Drawing.Point(131, 35);
			this.comboBoxAdvShiftCategory.Name = "comboBoxAdvShiftCategory";
			this.comboBoxAdvShiftCategory.Size = new System.Drawing.Size(245, 23);
			this.comboBoxAdvShiftCategory.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.comboBoxAdvShiftCategory.TabIndex = 1;
			this.comboBoxAdvShiftCategory.SelectedIndexChanged += new System.EventHandler(this.comboBoxAdvShiftCategorySelectedIndexChanged);
			// 
			// comboBoxAdvAbsence
			// 
			this.comboBoxAdvAbsence.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.comboBoxAdvAbsence.BackColor = System.Drawing.Color.White;
			this.comboBoxAdvAbsence.BeforeTouchSize = new System.Drawing.Size(245, 23);
			this.comboBoxAdvAbsence.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAdvAbsence.Location = new System.Drawing.Point(131, 127);
			this.comboBoxAdvAbsence.Name = "comboBoxAdvAbsence";
			this.comboBoxAdvAbsence.Size = new System.Drawing.Size(245, 23);
			this.comboBoxAdvAbsence.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.comboBoxAdvAbsence.TabIndex = 3;
			this.comboBoxAdvAbsence.SelectedIndexChanged += new System.EventHandler(this.comboBoxAdvAbsenceSelectedIndexChanged);
			// 
			// labelAbsence
			// 
			this.labelAbsence.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelAbsence.AutoSize = true;
			this.labelAbsence.Location = new System.Drawing.Point(3, 130);
			this.labelAbsence.Name = "labelAbsence";
			this.labelAbsence.Size = new System.Drawing.Size(62, 15);
			this.labelAbsence.TabIndex = 1;
			this.labelAbsence.Text = "xxAbsence";
			// 
			// labelDayOff
			// 
			this.labelDayOff.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelDayOff.AutoSize = true;
			this.labelDayOff.Location = new System.Drawing.Point(3, 84);
			this.labelDayOff.Name = "labelDayOff";
			this.labelDayOff.Size = new System.Drawing.Size(54, 15);
			this.labelDayOff.TabIndex = 2;
			this.labelDayOff.Text = "xxDayOff";
			// 
			// comboBoxAdvDayOff
			// 
			this.comboBoxAdvDayOff.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.comboBoxAdvDayOff.BackColor = System.Drawing.Color.White;
			this.comboBoxAdvDayOff.BeforeTouchSize = new System.Drawing.Size(245, 23);
			this.comboBoxAdvDayOff.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAdvDayOff.Location = new System.Drawing.Point(131, 81);
			this.comboBoxAdvDayOff.Name = "comboBoxAdvDayOff";
			this.comboBoxAdvDayOff.Size = new System.Drawing.Size(245, 23);
			this.comboBoxAdvDayOff.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.comboBoxAdvDayOff.TabIndex = 2;
			this.comboBoxAdvDayOff.SelectedIndexChanged += new System.EventHandler(this.comboBoxAdvDayOffSelectedIndexChanged);
			// 
			// checkBoxMustHave
			// 
			this.checkBoxMustHave.BeforeTouchSize = new System.Drawing.Size(196, 24);
			this.checkBoxMustHave.DrawFocusRectangle = false;
			this.checkBoxMustHave.Location = new System.Drawing.Point(3, 10);
			this.checkBoxMustHave.Margin = new System.Windows.Forms.Padding(3, 10, 3, 3);
			this.checkBoxMustHave.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxMustHave.Name = "checkBoxMustHave";
			this.checkBoxMustHave.Size = new System.Drawing.Size(196, 24);
			this.checkBoxMustHave.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Metro;
			this.checkBoxMustHave.TabIndex = 10;
			this.checkBoxMustHave.Text = "xxMustHave";
			this.checkBoxMustHave.ThemesEnabled = false;
			// 
			// tabControlAgentInfo
			// 
			this.tabControlAgentInfo.ActiveTabColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(102)))), ((int)(((byte)(102)))));
			this.tabControlAgentInfo.BackColor = System.Drawing.Color.White;
			this.tabControlAgentInfo.BeforeTouchSize = new System.Drawing.Size(451, 327);
			this.tabControlAgentInfo.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.tabControlAgentInfo.BorderWidth = 1;
			this.tabControlAgentInfo.Controls.Add(this.tabPageAdvStandard);
			this.tabControlAgentInfo.Controls.Add(this.tabPageAdvExtended);
			this.tabControlAgentInfo.Controls.Add(this.tabPageAdvActivity);
			this.tabControlAgentInfo.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControlAgentInfo.InactiveTabColor = System.Drawing.Color.White;
			this.tabControlAgentInfo.Location = new System.Drawing.Point(3, 3);
			this.tabControlAgentInfo.Name = "tabControlAgentInfo";
			this.tabControlAgentInfo.Size = new System.Drawing.Size(451, 327);
			this.tabControlAgentInfo.TabIndex = 1;
			this.tabControlAgentInfo.TabPanelBackColor = System.Drawing.Color.White;
			this.tabControlAgentInfo.TabStyle = typeof(Syncfusion.Windows.Forms.Tools.TabRendererMetro);
			// 
			// errorProviderExtended
			// 
			this.errorProviderExtended.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
			this.errorProviderExtended.ContainerControl = this;
			// 
			// errorProviderActivity
			// 
			this.errorProviderActivity.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
			this.errorProviderActivity.ContainerControl = this;
			// 
			// tableLayoutPanelMain
			// 
			this.tableLayoutPanelMain.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanelMain.ColumnCount = 1;
			this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanelMain.Controls.Add(this.tabControlAgentInfo, 0, 0);
			this.tableLayoutPanelMain.Controls.Add(this.tableLayoutPanelButtons, 0, 1);
			this.tableLayoutPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelMain.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanelMain.Name = "tableLayoutPanelMain";
			this.tableLayoutPanelMain.RowCount = 2;
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 86.28049F));
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 13.71951F));
			this.tableLayoutPanelMain.Size = new System.Drawing.Size(457, 386);
			this.tableLayoutPanelMain.TabIndex = 11;
			// 
			// tableLayoutPanelButtons
			// 
			this.tableLayoutPanelButtons.ColumnCount = 3;
			this.tableLayoutPanelButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.tableLayoutPanelButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.tableLayoutPanelButtons.Controls.Add(this.buttonAdvOk, 1, 0);
			this.tableLayoutPanelButtons.Controls.Add(this.checkBoxMustHave, 0, 0);
			this.tableLayoutPanelButtons.Controls.Add(this.buttonAdvCancel, 2, 0);
			this.tableLayoutPanelButtons.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelButtons.Location = new System.Drawing.Point(0, 333);
			this.tableLayoutPanelButtons.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanelButtons.Name = "tableLayoutPanelButtons";
			this.tableLayoutPanelButtons.RowCount = 1;
			this.tableLayoutPanelButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelButtons.Size = new System.Drawing.Size(457, 53);
			this.tableLayoutPanelButtons.TabIndex = 3;
			// 
			// AgentPreferenceView
			// 
			this.AcceptButton = this.buttonAdvOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BorderColor = System.Drawing.Color.Blue;
			this.CancelButton = this.buttonAdvCancel;
			this.CaptionFont = new System.Drawing.Font("Segoe UI", 12F);
			this.ClientSize = new System.Drawing.Size(457, 386);
			this.Controls.Add(this.tableLayoutPanelMain);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.HelpButton = false;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AgentPreferenceView";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "xxPreference";
			this.Load += new System.EventHandler(this.agentPreferenceViewLoad);
			this.tabPageAdvActivity.ResumeLayout(false);
			this.tableLayoutPanelActivity.ResumeLayout(false);
			this.tableLayoutPanelActivity.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvActivity)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerActivityEndMin)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerActivityEndMax)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerActivityLengthMin)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerActivityLengthMax)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerActivityStartMin)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerActivityStartMax)).EndInit();
			this.tabPageAdvExtended.ResumeLayout(false);
			this.tableLayoutPanelExtended.ResumeLayout(false);
			this.tableLayoutPanelExtended.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerContractShiftCategoryMin)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerContractShiftCategoryMax)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerShiftCategoryStartMin)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerShiftCategoryStartMax)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerShiftCategoryEndMin)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvShiftCategoryNextDayMin)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerShiftCategoryEndMax)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvShiftCategoryNextDayMax)).EndInit();
			this.tabPageAdvStandard.ResumeLayout(false);
			this.tableLayoutPanelStandard.ResumeLayout(false);
			this.tableLayoutPanelStandard.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvShiftCategory)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvAbsence)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvDayOff)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxMustHave)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.tabControlAgentInfo)).EndInit();
			this.tabControlAgentInfo.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.errorProviderExtended)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.errorProviderActivity)).EndInit();
			this.tableLayoutPanelMain.ResumeLayout(false);
			this.tableLayoutPanelButtons.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageAdvActivity;
		private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageAdvExtended;
		private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageAdvStandard;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelStandard;
		private System.Windows.Forms.Label labelShiftCategory;
		private System.Windows.Forms.Label labelAbsence;
		private System.Windows.Forms.Label labelDayOff;
		private Syncfusion.Windows.Forms.Tools.TabControlAdv tabControlAgentInfo;
		private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvShiftCategory;
		private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvAbsence;
		private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvDayOff;
		private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxMustHave;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvOk;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvCancel;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelExtended;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private OutlookTimePicker outlookTimePickerContractShiftCategoryMin;
		private OutlookTimePicker outlookTimePickerContractShiftCategoryMax;
		private OutlookTimePicker outlookTimePickerShiftCategoryStartMin;
		private OutlookTimePicker outlookTimePickerShiftCategoryStartMax;
		private OutlookTimePicker outlookTimePickerShiftCategoryEndMin;
		private OutlookTimePicker outlookTimePickerShiftCategoryEndMax;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelActivity;
		private System.Windows.Forms.Label label8;
		private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvActivity;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label11;
		private OutlookTimePicker outlookTimePickerActivityLengthMin;
		private OutlookTimePicker outlookTimePickerActivityLengthMax;
		private OutlookTimePicker outlookTimePickerActivityStartMin;
		private OutlookTimePicker outlookTimePickerActivityStartMax;
		private OutlookTimePicker outlookTimePickerActivityEndMin;
		private OutlookTimePicker outlookTimePickerActivityEndMax;
		private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdvShiftCategoryNextDayMax;
		private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdvShiftCategoryNextDayMin;
		private System.Windows.Forms.ErrorProvider errorProviderExtended;
		private System.Windows.Forms.ErrorProvider errorProviderActivity;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelMain;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelButtons;
		private System.Windows.Forms.ToolTip toolTip;

	}
}