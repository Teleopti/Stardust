
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.SkillPages
{
    partial class SkillGeneral
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
			this.textBoxName = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
			this.labelName = new System.Windows.Forms.Label();
			this.labelDescription = new System.Windows.Forms.Label();
			this.textBoxDescription = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
			this.labelColor = new System.Windows.Forms.Label();
			this.buttonChangeColor = new Syncfusion.Windows.Forms.ButtonAdv();
			this.labelSkillType = new System.Windows.Forms.Label();
			this.comboBoxSkillType = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.colorDialogSkillColor = new System.Windows.Forms.ColorDialog();
			this.labelSkillActivity = new System.Windows.Forms.Label();
			this.comboBoxSkillActivity = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.pictureBoxDisplayColor = new System.Windows.Forms.PictureBox();
			this.tableLayoutPanelForRtl = new System.Windows.Forms.TableLayoutPanel();
			this.labelMidnightBreakOffset = new System.Windows.Forms.Label();
			this.comboBoxTimeZones = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.labelDefaultResolution = new System.Windows.Forms.Label();
			this.labelTimeZone = new System.Windows.Forms.Label();
			this.labelTotalOpeningHours = new System.Windows.Forms.Label();
			this.office2007OutlookTimePickerMidnightOffsetBreak = new Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Office2007OutlookTimePicker(this.components);
			this.comboBoxAdvIntervalLength = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.labelAbandonRate = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.numericUpDownMaxParallel = new Syncfusion.Windows.Forms.Tools.NumericUpDownExt();
			this.percentTextBoxAbandonRate = new Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.TeleoptiPercentTextBox();
			this.percentIntervalAbandonRate = new System.Windows.Forms.Label();
			this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
			((System.ComponentModel.ISupportInitialize)(this.textBoxName)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.textBoxDescription)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxSkillType)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxSkillActivity)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxDisplayColor)).BeginInit();
			this.tableLayoutPanelForRtl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxTimeZones)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.office2007OutlookTimePickerMidnightOffsetBreak)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvIntervalLength)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxParallel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
			this.SuspendLayout();
			// 
			// textBoxName
			// 
			this.textBoxName.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.textBoxName.BackColor = System.Drawing.Color.White;
			this.textBoxName.BeforeTouchSize = new System.Drawing.Size(222, 23);
			this.textBoxName.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.tableLayoutPanelForRtl.SetColumnSpan(this.textBoxName, 2);
			this.textBoxName.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.textBoxName.Location = new System.Drawing.Point(161, 4);
			this.textBoxName.Metrocolor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxName.Name = "textBoxName";
			this.textBoxName.Size = new System.Drawing.Size(222, 23);
			this.textBoxName.Style = Syncfusion.Windows.Forms.Tools.TextBoxExt.theme.Metro;
			this.textBoxName.TabIndex = 1;
			this.textBoxName.TextChanged += new System.EventHandler(this.textBoxName_TextChanged);
			// 
			// labelName
			// 
			this.labelName.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelName.AutoSize = true;
			this.labelName.Location = new System.Drawing.Point(3, 6);
			this.labelName.Name = "labelName";
			this.labelName.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this.labelName.Size = new System.Drawing.Size(81, 18);
			this.labelName.TabIndex = 0;
			this.labelName.Text = "xxNameColon";
			// 
			// labelDescription
			// 
			this.labelDescription.AutoSize = true;
			this.labelDescription.Location = new System.Drawing.Point(3, 31);
			this.labelDescription.Name = "labelDescription";
			this.labelDescription.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this.labelDescription.Size = new System.Drawing.Size(109, 18);
			this.labelDescription.TabIndex = 2;
			this.labelDescription.Text = "xxDescriptionColon";
			// 
			// textBoxDescription
			// 
			this.textBoxDescription.AcceptsReturn = true;
			this.textBoxDescription.BackColor = System.Drawing.Color.White;
			this.textBoxDescription.BeforeTouchSize = new System.Drawing.Size(222, 23);
			this.textBoxDescription.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxDescription.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.tableLayoutPanelForRtl.SetColumnSpan(this.textBoxDescription, 2);
			this.textBoxDescription.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.textBoxDescription.Location = new System.Drawing.Point(161, 34);
			this.textBoxDescription.MaxLength = 1023;
			this.textBoxDescription.Metrocolor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxDescription.Multiline = true;
			this.textBoxDescription.Name = "textBoxDescription";
			this.textBoxDescription.Size = new System.Drawing.Size(222, 96);
			this.textBoxDescription.Style = Syncfusion.Windows.Forms.Tools.TextBoxExt.theme.Metro;
			this.textBoxDescription.TabIndex = 3;
			this.textBoxDescription.Text = "Description\r\ntest";
			// 
			// labelColor
			// 
			this.labelColor.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelColor.AutoSize = true;
			this.labelColor.Location = new System.Drawing.Point(3, 232);
			this.labelColor.Name = "labelColor";
			this.labelColor.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this.labelColor.Size = new System.Drawing.Size(78, 18);
			this.labelColor.TabIndex = 10;
			this.labelColor.Text = "xxColorColon";
			// 
			// buttonChangeColor
			// 
			this.buttonChangeColor.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.buttonChangeColor.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonChangeColor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonChangeColor.BeforeTouchSize = new System.Drawing.Size(30, 24);
			this.buttonChangeColor.ForeColor = System.Drawing.Color.White;
			this.buttonChangeColor.IsBackStageButton = false;
			this.buttonChangeColor.Location = new System.Drawing.Point(252, 229);
			this.buttonChangeColor.Name = "buttonChangeColor";
			this.buttonChangeColor.Size = new System.Drawing.Size(30, 24);
			this.buttonChangeColor.TabIndex = 11;
			this.buttonChangeColor.Text = "...";
			this.buttonChangeColor.UseVisualStyle = true;
			this.buttonChangeColor.UseVisualStyleBackColor = false;
			this.buttonChangeColor.Click += new System.EventHandler(this.buttonChangeColor_Click);
			// 
			// labelSkillType
			// 
			this.labelSkillType.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelSkillType.AutoSize = true;
			this.labelSkillType.Location = new System.Drawing.Point(3, 170);
			this.labelSkillType.Name = "labelSkillType";
			this.labelSkillType.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this.labelSkillType.Size = new System.Drawing.Size(135, 18);
			this.labelSkillType.TabIndex = 6;
			this.labelSkillType.Text = "xxForecastMethodColon";
			// 
			// comboBoxSkillType
			// 
			this.comboBoxSkillType.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.comboBoxSkillType.BackColor = System.Drawing.Color.White;
			this.comboBoxSkillType.BeforeTouchSize = new System.Drawing.Size(222, 23);
			this.tableLayoutPanelForRtl.SetColumnSpan(this.comboBoxSkillType, 2);
			this.comboBoxSkillType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxSkillType.Location = new System.Drawing.Point(161, 169);
			this.comboBoxSkillType.Name = "comboBoxSkillType";
			this.comboBoxSkillType.Size = new System.Drawing.Size(222, 23);
			this.comboBoxSkillType.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.comboBoxSkillType.TabIndex = 7;
			// 
			// labelSkillActivity
			// 
			this.labelSkillActivity.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelSkillActivity.AutoSize = true;
			this.labelSkillActivity.Location = new System.Drawing.Point(3, 201);
			this.labelSkillActivity.Name = "labelSkillActivity";
			this.labelSkillActivity.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this.labelSkillActivity.Size = new System.Drawing.Size(89, 18);
			this.labelSkillActivity.TabIndex = 8;
			this.labelSkillActivity.Text = "xxActivityColon";
			this.labelSkillActivity.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// comboBoxSkillActivity
			// 
			this.comboBoxSkillActivity.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.comboBoxSkillActivity.BackColor = System.Drawing.Color.White;
			this.comboBoxSkillActivity.BeforeTouchSize = new System.Drawing.Size(222, 23);
			this.tableLayoutPanelForRtl.SetColumnSpan(this.comboBoxSkillActivity, 2);
			this.comboBoxSkillActivity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxSkillActivity.Location = new System.Drawing.Point(161, 200);
			this.comboBoxSkillActivity.Name = "comboBoxSkillActivity";
			this.comboBoxSkillActivity.Size = new System.Drawing.Size(222, 23);
			this.comboBoxSkillActivity.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.comboBoxSkillActivity.TabIndex = 9;
			this.comboBoxSkillActivity.SelectedIndexChanged += new System.EventHandler(this.comboBoxSkillActivity_SelectedIndexChanged);
			// 
			// pictureBoxDisplayColor
			// 
			this.pictureBoxDisplayColor.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.pictureBoxDisplayColor.BackColor = System.Drawing.Color.Black;
			this.pictureBoxDisplayColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pictureBoxDisplayColor.Location = new System.Drawing.Point(164, 232);
			this.pictureBoxDisplayColor.Margin = new System.Windows.Forms.Padding(6);
			this.pictureBoxDisplayColor.Name = "pictureBoxDisplayColor";
			this.pictureBoxDisplayColor.Size = new System.Drawing.Size(79, 19);
			this.pictureBoxDisplayColor.TabIndex = 5;
			this.pictureBoxDisplayColor.TabStop = false;
			// 
			// tableLayoutPanelForRtl
			// 
			this.tableLayoutPanelForRtl.ColumnCount = 3;
			this.tableLayoutPanelForRtl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
			this.tableLayoutPanelForRtl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 23F));
			this.tableLayoutPanelForRtl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 37F));
			this.tableLayoutPanelForRtl.Controls.Add(this.labelMidnightBreakOffset, 0, 7);
			this.tableLayoutPanelForRtl.Controls.Add(this.labelName, 0, 0);
			this.tableLayoutPanelForRtl.Controls.Add(this.comboBoxTimeZones, 1, 6);
			this.tableLayoutPanelForRtl.Controls.Add(this.textBoxName, 1, 0);
			this.tableLayoutPanelForRtl.Controls.Add(this.buttonChangeColor, 2, 5);
			this.tableLayoutPanelForRtl.Controls.Add(this.comboBoxSkillActivity, 1, 4);
			this.tableLayoutPanelForRtl.Controls.Add(this.pictureBoxDisplayColor, 1, 5);
			this.tableLayoutPanelForRtl.Controls.Add(this.labelDefaultResolution, 0, 2);
			this.tableLayoutPanelForRtl.Controls.Add(this.labelColor, 0, 5);
			this.tableLayoutPanelForRtl.Controls.Add(this.comboBoxSkillType, 1, 3);
			this.tableLayoutPanelForRtl.Controls.Add(this.textBoxDescription, 1, 1);
			this.tableLayoutPanelForRtl.Controls.Add(this.labelSkillActivity, 0, 4);
			this.tableLayoutPanelForRtl.Controls.Add(this.labelDescription, 0, 1);
			this.tableLayoutPanelForRtl.Controls.Add(this.labelSkillType, 0, 3);
			this.tableLayoutPanelForRtl.Controls.Add(this.labelTimeZone, 0, 6);
			this.tableLayoutPanelForRtl.Controls.Add(this.labelTotalOpeningHours, 2, 7);
			this.tableLayoutPanelForRtl.Controls.Add(this.office2007OutlookTimePickerMidnightOffsetBreak, 1, 7);
			this.tableLayoutPanelForRtl.Controls.Add(this.comboBoxAdvIntervalLength, 1, 2);
			this.tableLayoutPanelForRtl.Controls.Add(this.labelAbandonRate, 0, 8);
			this.tableLayoutPanelForRtl.Controls.Add(this.label1, 0, 9);
			this.tableLayoutPanelForRtl.Controls.Add(this.numericUpDownMaxParallel, 1, 9);
			this.tableLayoutPanelForRtl.Controls.Add(this.percentTextBoxAbandonRate, 1, 8);
			this.tableLayoutPanelForRtl.Controls.Add(this.percentIntervalAbandonRate, 2, 8);
			this.tableLayoutPanelForRtl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelForRtl.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanelForRtl.Name = "tableLayoutPanelForRtl";
			this.tableLayoutPanelForRtl.RowCount = 10;
			this.tableLayoutPanelForRtl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 31F));
			this.tableLayoutPanelForRtl.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelForRtl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 31F));
			this.tableLayoutPanelForRtl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 31F));
			this.tableLayoutPanelForRtl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 31F));
			this.tableLayoutPanelForRtl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 31F));
			this.tableLayoutPanelForRtl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 31F));
			this.tableLayoutPanelForRtl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 31F));
			this.tableLayoutPanelForRtl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
			this.tableLayoutPanelForRtl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
			this.tableLayoutPanelForRtl.Size = new System.Drawing.Size(396, 395);
			this.tableLayoutPanelForRtl.TabIndex = 14;
			// 
			// labelMidnightBreakOffset
			// 
			this.labelMidnightBreakOffset.AutoSize = true;
			this.labelMidnightBreakOffset.Location = new System.Drawing.Point(3, 294);
			this.labelMidnightBreakOffset.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
			this.labelMidnightBreakOffset.Name = "labelMidnightBreakOffset";
			this.labelMidnightBreakOffset.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this.labelMidnightBreakOffset.Size = new System.Drawing.Size(152, 25);
			this.labelMidnightBreakOffset.TabIndex = 14;
			this.labelMidnightBreakOffset.Text = "xxMidnightBreakOffsetColon";
			// 
			// comboBoxTimeZones
			// 
			this.comboBoxTimeZones.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.comboBoxTimeZones.BackColor = System.Drawing.Color.White;
			this.comboBoxTimeZones.BeforeTouchSize = new System.Drawing.Size(222, 23);
			this.tableLayoutPanelForRtl.SetColumnSpan(this.comboBoxTimeZones, 2);
			this.comboBoxTimeZones.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxTimeZones.Location = new System.Drawing.Point(161, 262);
			this.comboBoxTimeZones.Name = "comboBoxTimeZones";
			this.comboBoxTimeZones.Size = new System.Drawing.Size(222, 23);
			this.comboBoxTimeZones.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.comboBoxTimeZones.TabIndex = 13;
			this.comboBoxTimeZones.DropDown += new System.EventHandler(this.AdjustWidthComboBox_DropDown);
			// 
			// labelDefaultResolution
			// 
			this.labelDefaultResolution.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelDefaultResolution.AutoSize = true;
			this.labelDefaultResolution.Location = new System.Drawing.Point(3, 133);
			this.labelDefaultResolution.Name = "labelDefaultResolution";
			this.labelDefaultResolution.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this.labelDefaultResolution.Size = new System.Drawing.Size(149, 31);
			this.labelDefaultResolution.TabIndex = 4;
			this.labelDefaultResolution.Text = "xxIntervalLengthHMMColon";
			// 
			// labelTimeZone
			// 
			this.labelTimeZone.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelTimeZone.AutoSize = true;
			this.labelTimeZone.Location = new System.Drawing.Point(3, 263);
			this.labelTimeZone.Name = "labelTimeZone";
			this.labelTimeZone.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this.labelTimeZone.Size = new System.Drawing.Size(103, 18);
			this.labelTimeZone.TabIndex = 12;
			this.labelTimeZone.Text = "xxTimeZoneColon";
			// 
			// labelTotalOpeningHours
			// 
			this.labelTotalOpeningHours.AutoSize = true;
			this.labelTotalOpeningHours.Location = new System.Drawing.Point(252, 296);
			this.labelTotalOpeningHours.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
			this.labelTotalOpeningHours.Name = "labelTotalOpeningHours";
			this.labelTotalOpeningHours.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this.labelTotalOpeningHours.Size = new System.Drawing.Size(0, 18);
			this.labelTotalOpeningHours.TabIndex = 16;
			// 
			// office2007OutlookTimePickerMidnightOffsetBreak
			// 
			this.office2007OutlookTimePickerMidnightOffsetBreak.BackColor = System.Drawing.Color.White;
			this.office2007OutlookTimePickerMidnightOffsetBreak.BeforeTouchSize = new System.Drawing.Size(85, 23);
			this.office2007OutlookTimePickerMidnightOffsetBreak.BindableTimeValue = System.TimeSpan.Parse("00:00:00");
			this.office2007OutlookTimePickerMidnightOffsetBreak.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.office2007OutlookTimePickerMidnightOffsetBreak.Location = new System.Drawing.Point(161, 294);
			this.office2007OutlookTimePickerMidnightOffsetBreak.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
			this.office2007OutlookTimePickerMidnightOffsetBreak.MaxValue = System.TimeSpan.Parse("2.00:00:00");
			this.office2007OutlookTimePickerMidnightOffsetBreak.MinValue = System.TimeSpan.Parse("00:00:00");
			this.office2007OutlookTimePickerMidnightOffsetBreak.Name = "office2007OutlookTimePickerMidnightOffsetBreak";
			this.office2007OutlookTimePickerMidnightOffsetBreak.Office2007ColorTheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
			this.office2007OutlookTimePickerMidnightOffsetBreak.Size = new System.Drawing.Size(85, 23);
			this.office2007OutlookTimePickerMidnightOffsetBreak.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.office2007OutlookTimePickerMidnightOffsetBreak.TabIndex = 15;
			this.office2007OutlookTimePickerMidnightOffsetBreak.SelectedValueChanged += new System.EventHandler(this.office2007OutlookTimePickerMidnightOffsetBreak_SelectedValueChanged);
			// 
			// comboBoxAdvIntervalLength
			// 
			this.comboBoxAdvIntervalLength.AllowNewText = false;
			this.comboBoxAdvIntervalLength.BackColor = System.Drawing.Color.White;
			this.comboBoxAdvIntervalLength.BeforeTouchSize = new System.Drawing.Size(82, 23);
			this.comboBoxAdvIntervalLength.Location = new System.Drawing.Point(161, 136);
			this.comboBoxAdvIntervalLength.Name = "comboBoxAdvIntervalLength";
			this.comboBoxAdvIntervalLength.NumberOnly = true;
			this.comboBoxAdvIntervalLength.Size = new System.Drawing.Size(82, 23);
			this.comboBoxAdvIntervalLength.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.comboBoxAdvIntervalLength.TabIndex = 5;
			// 
			// labelAbandonRate
			// 
			this.labelAbandonRate.AutoSize = true;
			this.labelAbandonRate.Location = new System.Drawing.Point(3, 325);
			this.labelAbandonRate.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
			this.labelAbandonRate.Name = "labelAbandonRate";
			this.labelAbandonRate.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this.labelAbandonRate.Size = new System.Drawing.Size(149, 28);
			this.labelAbandonRate.TabIndex = 19;
			this.labelAbandonRate.Text = "xxAbandonRateParenthesisPercentSign";
			this.labelAbandonRate.Click += new System.EventHandler(this.label2_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 359);
			this.label1.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
			this.label1.Name = "label1";
			this.label1.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this.label1.Size = new System.Drawing.Size(105, 18);
			this.label1.TabIndex = 17;
			this.label1.Text = "xxMaxParallelTasks";
			// 
			// numericUpDownMaxParallel
			// 
			this.numericUpDownMaxParallel.BeforeTouchSize = new System.Drawing.Size(85, 23);
			this.numericUpDownMaxParallel.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.numericUpDownMaxParallel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericUpDownMaxParallel.Enabled = false;
			this.numericUpDownMaxParallel.Location = new System.Drawing.Point(161, 359);
			this.numericUpDownMaxParallel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
			this.numericUpDownMaxParallel.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.numericUpDownMaxParallel.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numericUpDownMaxParallel.Name = "numericUpDownMaxParallel";
			this.numericUpDownMaxParallel.Size = new System.Drawing.Size(85, 23);
			this.numericUpDownMaxParallel.TabIndex = 18;
			this.numericUpDownMaxParallel.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numericUpDownMaxParallel.VisualStyle = Syncfusion.Windows.Forms.VisualStyle.Metro;
			// 
			// percentTextBoxAbandonRate
			// 
			this.percentTextBoxAbandonRate.AllowNegativePercentage = true;
			this.percentTextBoxAbandonRate.DefaultValue = 0D;
			this.percentTextBoxAbandonRate.DoubleValue = 0D;
			this.percentTextBoxAbandonRate.ForeColor = System.Drawing.Color.Black;
			this.percentTextBoxAbandonRate.Location = new System.Drawing.Point(161, 322);
			this.percentTextBoxAbandonRate.Maximum = 30D;
			this.percentTextBoxAbandonRate.Minimum = 0D;
			this.percentTextBoxAbandonRate.Name = "percentTextBoxAbandonRate";
			this.percentTextBoxAbandonRate.Size = new System.Drawing.Size(57, 23);
			this.percentTextBoxAbandonRate.TabIndex = 20;
			this.percentTextBoxAbandonRate.Text = "0%";
			// 
			// percentIntervalAbandonRate
			// 
			this.percentIntervalAbandonRate.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.percentIntervalAbandonRate.AutoSize = true;
			this.percentIntervalAbandonRate.Location = new System.Drawing.Point(252, 328);
			this.percentIntervalAbandonRate.Name = "percentIntervalAbandonRate";
			this.percentIntervalAbandonRate.Size = new System.Drawing.Size(40, 15);
			this.percentIntervalAbandonRate.TabIndex = 21;
			this.percentIntervalAbandonRate.Text = "0-30%";
			// 
			// errorProvider1
			// 
			this.errorProvider1.ContainerControl = this;
			// 
			// SkillGeneral
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.tableLayoutPanelForRtl);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.Name = "SkillGeneral";
			this.Size = new System.Drawing.Size(396, 395);
			((System.ComponentModel.ISupportInitialize)(this.textBoxName)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.textBoxDescription)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxSkillType)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxSkillActivity)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxDisplayColor)).EndInit();
			this.tableLayoutPanelForRtl.ResumeLayout(false);
			this.tableLayoutPanelForRtl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxTimeZones)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.office2007OutlookTimePickerMidnightOffsetBreak)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvIntervalLength)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxParallel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.TextBoxExt textBoxName;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.Label labelDescription;
		  private Syncfusion.Windows.Forms.Tools.TextBoxExt textBoxDescription;
        private System.Windows.Forms.Label labelColor;
        private System.Windows.Forms.PictureBox pictureBoxDisplayColor;
		  private Syncfusion.Windows.Forms.ButtonAdv  buttonChangeColor;
        private System.Windows.Forms.Label labelSkillType;
		  private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxSkillType;
        private System.Windows.Forms.ColorDialog colorDialogSkillColor;
        private System.Windows.Forms.Label labelSkillActivity;
		  private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxSkillActivity;
		  private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxTimeZones;
        private System.Windows.Forms.Label labelTimeZone;
        private System.Windows.Forms.Label labelDefaultResolution;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelForRtl;
        private System.Windows.Forms.Label labelMidnightBreakOffset;
        private System.Windows.Forms.Label labelTotalOpeningHours;
        private Office2007OutlookTimePicker office2007OutlookTimePickerMidnightOffsetBreak;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvIntervalLength;
        private System.Windows.Forms.ErrorProvider errorProvider1;
		private System.Windows.Forms.Label label1;
		private Syncfusion.Windows.Forms.Tools.NumericUpDownExt numericUpDownMaxParallel;
		private System.Windows.Forms.Label labelAbandonRate;
		private TeleoptiPercentTextBox percentTextBoxAbandonRate;
		private System.Windows.Forms.Label percentIntervalAbandonRate;
		//private Teleopti.Ccc.Win.Common.Controls.OutlookTimePicker outlookTimePickerMidnightOffsetBreak;
	}
}