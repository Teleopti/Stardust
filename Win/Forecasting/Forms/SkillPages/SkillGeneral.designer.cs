
namespace Teleopti.Ccc.Win.Forecasting.Forms.SkillPages
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
			this.textBoxName = new System.Windows.Forms.TextBox();
			this.labelName = new System.Windows.Forms.Label();
			this.labelDescription = new System.Windows.Forms.Label();
			this.textBoxDescription = new System.Windows.Forms.TextBox();
			this.labelColor = new System.Windows.Forms.Label();
			this.buttonChangeColor = new System.Windows.Forms.Button();
			this.labelSkillType = new System.Windows.Forms.Label();
			this.comboBoxSkillType = new System.Windows.Forms.ComboBox();
			this.colorDialogSkillColor = new System.Windows.Forms.ColorDialog();
			this.labelSkillActivity = new System.Windows.Forms.Label();
			this.comboBoxSkillActivity = new System.Windows.Forms.ComboBox();
			this.pictureBoxDisplayColor = new System.Windows.Forms.PictureBox();
			this.tableLayoutPanelForRtl = new System.Windows.Forms.TableLayoutPanel();
			this.labelMidnightBreakOffset = new System.Windows.Forms.Label();
			this.comboBoxTimeZones = new System.Windows.Forms.ComboBox();
			this.labelDefaultResolution = new System.Windows.Forms.Label();
			this.labelTimeZone = new System.Windows.Forms.Label();
			this.labelTotalOpeningHours = new System.Windows.Forms.Label();
			this.office2007OutlookTimePickerMidnightOffsetBreak = new Teleopti.Ccc.Win.Common.Controls.Office2007OutlookTimePicker(this.components);
			this.comboBoxAdvIntervalLength = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxDisplayColor)).BeginInit();
			this.tableLayoutPanelForRtl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.office2007OutlookTimePickerMidnightOffsetBreak)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvIntervalLength)).BeginInit();
			this.SuspendLayout();
			// 
			// textBoxName
			// 
			this.textBoxName.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.tableLayoutPanelForRtl.SetColumnSpan(this.textBoxName, 2);
			this.textBoxName.Location = new System.Drawing.Point(141, 3);
			this.textBoxName.Name = "textBoxName";
			this.textBoxName.Size = new System.Drawing.Size(191, 20);
			this.textBoxName.TabIndex = 1;
			this.textBoxName.TextChanged += new System.EventHandler(this.textBoxName_TextChanged);
			// 
			// labelName
			// 
			this.labelName.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelName.AutoSize = true;
			this.labelName.Location = new System.Drawing.Point(3, 5);
			this.labelName.Name = "labelName";
			this.labelName.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this.labelName.Size = new System.Drawing.Size(72, 16);
			this.labelName.TabIndex = 0;
			this.labelName.Text = "xxNameColon";
			// 
			// labelDescription
			// 
			this.labelDescription.AutoSize = true;
			this.labelDescription.Location = new System.Drawing.Point(3, 27);
			this.labelDescription.Name = "labelDescription";
			this.labelDescription.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this.labelDescription.Size = new System.Drawing.Size(97, 16);
			this.labelDescription.TabIndex = 2;
			this.labelDescription.Text = "xxDescriptionColon";
			// 
			// textBoxDescription
			// 
			this.textBoxDescription.AcceptsReturn = true;
			this.tableLayoutPanelForRtl.SetColumnSpan(this.textBoxDescription, 2);
			this.textBoxDescription.Location = new System.Drawing.Point(141, 30);
			this.textBoxDescription.MaxLength = 1023;
			this.textBoxDescription.Multiline = true;
			this.textBoxDescription.Name = "textBoxDescription";
			this.textBoxDescription.Size = new System.Drawing.Size(191, 84);
			this.textBoxDescription.TabIndex = 3;
			this.textBoxDescription.Text = "Description\r\ntest";
			// 
			// labelColor
			// 
			this.labelColor.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelColor.AutoSize = true;
			this.labelColor.Location = new System.Drawing.Point(3, 203);
			this.labelColor.Name = "labelColor";
			this.labelColor.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this.labelColor.Size = new System.Drawing.Size(68, 16);
			this.labelColor.TabIndex = 10;
			this.labelColor.Text = "xxColorColon";
			// 
			// buttonChangeColor
			// 
			this.buttonChangeColor.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.buttonChangeColor.Location = new System.Drawing.Point(220, 201);
			this.buttonChangeColor.Name = "buttonChangeColor";
			this.buttonChangeColor.Size = new System.Drawing.Size(26, 21);
			this.buttonChangeColor.TabIndex = 11;
			this.buttonChangeColor.Text = "...";
			this.buttonChangeColor.UseVisualStyleBackColor = true;
			this.buttonChangeColor.Click += new System.EventHandler(this.buttonChangeColor_Click);
			// 
			// labelSkillType
			// 
			this.labelSkillType.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelSkillType.AutoSize = true;
			this.labelSkillType.Location = new System.Drawing.Point(3, 149);
			this.labelSkillType.Name = "labelSkillType";
			this.labelSkillType.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this.labelSkillType.Size = new System.Drawing.Size(121, 16);
			this.labelSkillType.TabIndex = 6;
			this.labelSkillType.Text = "xxForecastMethodColon";
			// 
			// comboBoxSkillType
			// 
			this.comboBoxSkillType.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.tableLayoutPanelForRtl.SetColumnSpan(this.comboBoxSkillType, 2);
			this.comboBoxSkillType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxSkillType.FormattingEnabled = true;
			this.comboBoxSkillType.ItemHeight = 13;
			this.comboBoxSkillType.Location = new System.Drawing.Point(141, 147);
			this.comboBoxSkillType.Name = "comboBoxSkillType";
			this.comboBoxSkillType.Size = new System.Drawing.Size(191, 21);
			this.comboBoxSkillType.TabIndex = 7;
			// 
			// labelSkillActivity
			// 
			this.labelSkillActivity.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelSkillActivity.AutoSize = true;
			this.labelSkillActivity.Location = new System.Drawing.Point(3, 176);
			this.labelSkillActivity.Name = "labelSkillActivity";
			this.labelSkillActivity.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this.labelSkillActivity.Size = new System.Drawing.Size(78, 16);
			this.labelSkillActivity.TabIndex = 8;
			this.labelSkillActivity.Text = "xxActivityColon";
			this.labelSkillActivity.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// comboBoxSkillActivity
			// 
			this.comboBoxSkillActivity.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.tableLayoutPanelForRtl.SetColumnSpan(this.comboBoxSkillActivity, 2);
			this.comboBoxSkillActivity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxSkillActivity.FormattingEnabled = true;
			this.comboBoxSkillActivity.Location = new System.Drawing.Point(141, 174);
			this.comboBoxSkillActivity.Name = "comboBoxSkillActivity";
			this.comboBoxSkillActivity.Size = new System.Drawing.Size(191, 21);
			this.comboBoxSkillActivity.TabIndex = 9;
			this.comboBoxSkillActivity.SelectedIndexChanged += new System.EventHandler(this.comboBoxSkillActivity_SelectedIndexChanged);
			// 
			// pictureBoxDisplayColor
			// 
			this.pictureBoxDisplayColor.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.pictureBoxDisplayColor.BackColor = System.Drawing.Color.Black;
			this.pictureBoxDisplayColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pictureBoxDisplayColor.Location = new System.Drawing.Point(143, 203);
			this.pictureBoxDisplayColor.Margin = new System.Windows.Forms.Padding(5);
			this.pictureBoxDisplayColor.Name = "pictureBoxDisplayColor";
			this.pictureBoxDisplayColor.Size = new System.Drawing.Size(69, 17);
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
			this.tableLayoutPanelForRtl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelForRtl.Location = new System.Drawing.Point(10, 10);
			this.tableLayoutPanelForRtl.Name = "tableLayoutPanelForRtl";
			this.tableLayoutPanelForRtl.RowCount = 8;
			this.tableLayoutPanelForRtl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
			this.tableLayoutPanelForRtl.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelForRtl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
			this.tableLayoutPanelForRtl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
			this.tableLayoutPanelForRtl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
			this.tableLayoutPanelForRtl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
			this.tableLayoutPanelForRtl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
			this.tableLayoutPanelForRtl.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
			this.tableLayoutPanelForRtl.Size = new System.Drawing.Size(346, 280);
			this.tableLayoutPanelForRtl.TabIndex = 14;
			// 
			// labelMidnightBreakOffset
			// 
			this.labelMidnightBreakOffset.AutoSize = true;
			this.labelMidnightBreakOffset.Location = new System.Drawing.Point(3, 257);
			this.labelMidnightBreakOffset.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
			this.labelMidnightBreakOffset.Name = "labelMidnightBreakOffset";
			this.labelMidnightBreakOffset.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this.labelMidnightBreakOffset.Size = new System.Drawing.Size(128, 23);
			this.labelMidnightBreakOffset.TabIndex = 14;
			this.labelMidnightBreakOffset.Text = "xxMidnightBreakOffsetColon";
			// 
			// comboBoxTimeZones
			// 
			this.comboBoxTimeZones.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.tableLayoutPanelForRtl.SetColumnSpan(this.comboBoxTimeZones, 2);
			this.comboBoxTimeZones.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxTimeZones.FormattingEnabled = true;
			this.comboBoxTimeZones.Location = new System.Drawing.Point(141, 228);
			this.comboBoxTimeZones.Name = "comboBoxTimeZones";
			this.comboBoxTimeZones.Size = new System.Drawing.Size(191, 21);
			this.comboBoxTimeZones.TabIndex = 13;
			this.comboBoxTimeZones.DropDown += new System.EventHandler(this.AdjustWidthComboBox_DropDown);
			// 
			// labelDefaultResolution
			// 
			this.labelDefaultResolution.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelDefaultResolution.AutoSize = true;
			this.labelDefaultResolution.Location = new System.Drawing.Point(3, 117);
			this.labelDefaultResolution.Name = "labelDefaultResolution";
			this.labelDefaultResolution.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this.labelDefaultResolution.Size = new System.Drawing.Size(132, 27);
			this.labelDefaultResolution.TabIndex = 4;
			this.labelDefaultResolution.Text = "xxIntervalLengthHMMColon";
			// 
			// labelTimeZone
			// 
			this.labelTimeZone.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelTimeZone.AutoSize = true;
			this.labelTimeZone.Location = new System.Drawing.Point(3, 230);
			this.labelTimeZone.Name = "labelTimeZone";
			this.labelTimeZone.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this.labelTimeZone.Size = new System.Drawing.Size(92, 16);
			this.labelTimeZone.TabIndex = 12;
			this.labelTimeZone.Text = "xxTimeZoneColon";
			// 
			// labelTotalOpeningHours
			// 
			this.labelTotalOpeningHours.AutoSize = true;
			this.labelTotalOpeningHours.Location = new System.Drawing.Point(220, 259);
			this.labelTotalOpeningHours.Margin = new System.Windows.Forms.Padding(3, 7, 3, 0);
			this.labelTotalOpeningHours.Name = "labelTotalOpeningHours";
			this.labelTotalOpeningHours.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this.labelTotalOpeningHours.Size = new System.Drawing.Size(0, 16);
			this.labelTotalOpeningHours.TabIndex = 16;
			// 
			// office2007OutlookTimePickerMidnightOffsetBreak
			// 
			this.office2007OutlookTimePickerMidnightOffsetBreak.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(251)))));
			this.office2007OutlookTimePickerMidnightOffsetBreak.BindableTimeValue = System.TimeSpan.Parse("00:00:00");
			this.office2007OutlookTimePickerMidnightOffsetBreak.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.office2007OutlookTimePickerMidnightOffsetBreak.Location = new System.Drawing.Point(141, 257);
			this.office2007OutlookTimePickerMidnightOffsetBreak.Margin = new System.Windows.Forms.Padding(3, 5, 3, 3);
			this.office2007OutlookTimePickerMidnightOffsetBreak.MaxValue = System.TimeSpan.Parse("2.00:00:00");
			this.office2007OutlookTimePickerMidnightOffsetBreak.Name = "office2007OutlookTimePickerMidnightOffsetBreak";
			this.office2007OutlookTimePickerMidnightOffsetBreak.Office2007ColorTheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
			this.office2007OutlookTimePickerMidnightOffsetBreak.Size = new System.Drawing.Size(73, 21);
			this.office2007OutlookTimePickerMidnightOffsetBreak.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
			this.office2007OutlookTimePickerMidnightOffsetBreak.TabIndex = 15;
			this.office2007OutlookTimePickerMidnightOffsetBreak.SelectedValueChanged += new System.EventHandler(this.office2007OutlookTimePickerMidnightOffsetBreak_SelectedValueChanged);
			// 
			// comboBoxAdvIntervalLength
			// 
			this.comboBoxAdvIntervalLength.AllowNewText = false;
			this.comboBoxAdvIntervalLength.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(251)))));
			this.comboBoxAdvIntervalLength.Location = new System.Drawing.Point(141, 120);
			this.comboBoxAdvIntervalLength.Name = "comboBoxAdvIntervalLength";
			this.comboBoxAdvIntervalLength.NumberOnly = true;
			this.comboBoxAdvIntervalLength.Size = new System.Drawing.Size(71, 21);
			this.comboBoxAdvIntervalLength.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
			this.comboBoxAdvIntervalLength.TabIndex = 5;
			// 
			// SkillGeneral
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanelForRtl);
			this.Name = "SkillGeneral";
			this.Padding = new System.Windows.Forms.Padding(10);
			this.Size = new System.Drawing.Size(366, 300);
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxDisplayColor)).EndInit();
			this.tableLayoutPanelForRtl.ResumeLayout(false);
			this.tableLayoutPanelForRtl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.office2007OutlookTimePickerMidnightOffsetBreak)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvIntervalLength)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.TextBox textBoxDescription;
        private System.Windows.Forms.Label labelColor;
        private System.Windows.Forms.PictureBox pictureBoxDisplayColor;
        private System.Windows.Forms.Button buttonChangeColor;
        private System.Windows.Forms.Label labelSkillType;
        private System.Windows.Forms.ComboBox comboBoxSkillType;
        private System.Windows.Forms.ColorDialog colorDialogSkillColor;
        private System.Windows.Forms.Label labelSkillActivity;
        private System.Windows.Forms.ComboBox comboBoxSkillActivity;
        private System.Windows.Forms.ComboBox comboBoxTimeZones;
        private System.Windows.Forms.Label labelTimeZone;
        private System.Windows.Forms.Label labelDefaultResolution;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelForRtl;
        private System.Windows.Forms.Label labelMidnightBreakOffset;
        private System.Windows.Forms.Label labelTotalOpeningHours;
        private Teleopti.Ccc.Win.Common.Controls.Office2007OutlookTimePicker office2007OutlookTimePickerMidnightOffsetBreak;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvIntervalLength;
        //private Teleopti.Ccc.Win.Common.Controls.OutlookTimePicker outlookTimePickerMidnightOffsetBreak;
    }
}