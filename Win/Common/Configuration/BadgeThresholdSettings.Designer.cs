using Syncfusion.Windows.Forms.Tools;

namespace Teleopti.Ccc.Win.Common.Configuration
{
	partial class BadgeThresholdSettings
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
            if (disposing)
            {
                if(components!=null) 
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
			this.tableLayoutPanelBody = new System.Windows.Forms.TableLayoutPanel();
			this.labelSetThresholdForAnsweredCalls = new System.Windows.Forms.Label();
			this.labelSetThresholdForAHT = new System.Windows.Forms.Label();
			this.labelSetThresholdForAdherence = new System.Windows.Forms.Label();
			this.doubleTextBoxThresholdForAdherence = new Syncfusion.Windows.Forms.Tools.DoubleTextBox();
			this.labelSetGoldBadgeDaysThreshold = new System.Windows.Forms.Label();
			this.labelSetSilverBadgeDaysThreshold = new System.Windows.Forms.Label();
			this.numericUpDownGoldenBadgeDaysThreshold = new System.Windows.Forms.NumericUpDown();
			this.numericUpDownSilverBadgeDaysThreshold = new System.Windows.Forms.NumericUpDown();
			this.numericUpDownThresholdForAnsweredCalls = new System.Windows.Forms.NumericUpDown();
			this.timeSpanTextBoxThresholdForAHT = new Teleopti.Ccc.Win.Common.Controls.TimeSpanTextBox();
			this.checkBoxEnableBadge = new System.Windows.Forms.CheckBox();
			this.labelCalculationTime = new System.Windows.Forms.Label();
			this.timeSpanTextBoxCalculationTime = new Teleopti.Ccc.Win.Common.Controls.TimeSpanTextBox();
			this.labelSplitter1 = new System.Windows.Forms.Label();
			this.gradientPanelHeader = new Syncfusion.Windows.Forms.Tools.GradientPanel();
			this.tableLayoutPanelHeader = new System.Windows.Forms.TableLayoutPanel();
			this.labelHeader = new System.Windows.Forms.Label();
			this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
			this.label1 = new System.Windows.Forms.Label();
			this.buttonDeleteContract = new Syncfusion.Windows.Forms.ButtonAdv();
			this.labelSplitter2 = new System.Windows.Forms.Label();
			this.tableLayoutPanelBody.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.doubleTextBoxThresholdForAdherence)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownGoldenBadgeDaysThreshold)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownSilverBadgeDaysThreshold)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownThresholdForAnsweredCalls)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanelHeader)).BeginInit();
			this.gradientPanelHeader.SuspendLayout();
			this.tableLayoutPanelHeader.SuspendLayout();
			this.tableLayoutPanel5.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanelBody
			// 
			this.tableLayoutPanelBody.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.tableLayoutPanelBody.ColumnCount = 2;
			this.tableLayoutPanelBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 262F));
			this.tableLayoutPanelBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelBody.Controls.Add(this.labelSetThresholdForAnsweredCalls, 0, 3);
			this.tableLayoutPanelBody.Controls.Add(this.labelSetThresholdForAHT, 0, 4);
			this.tableLayoutPanelBody.Controls.Add(this.labelSetThresholdForAdherence, 0, 5);
			this.tableLayoutPanelBody.Controls.Add(this.doubleTextBoxThresholdForAdherence, 1, 5);
			this.tableLayoutPanelBody.Controls.Add(this.labelSetGoldBadgeDaysThreshold, 0, 8);
			this.tableLayoutPanelBody.Controls.Add(this.labelSetSilverBadgeDaysThreshold, 0, 7);
			this.tableLayoutPanelBody.Controls.Add(this.numericUpDownGoldenBadgeDaysThreshold, 1, 8);
			this.tableLayoutPanelBody.Controls.Add(this.numericUpDownSilverBadgeDaysThreshold, 1, 7);
			this.tableLayoutPanelBody.Controls.Add(this.numericUpDownThresholdForAnsweredCalls, 1, 3);
			this.tableLayoutPanelBody.Controls.Add(this.timeSpanTextBoxThresholdForAHT, 1, 4);
			this.tableLayoutPanelBody.Controls.Add(this.checkBoxEnableBadge, 0, 0);
			this.tableLayoutPanelBody.Controls.Add(this.labelCalculationTime, 0, 1);
			this.tableLayoutPanelBody.Controls.Add(this.timeSpanTextBoxCalculationTime, 1, 1);
			this.tableLayoutPanelBody.Controls.Add(this.labelSplitter1, 0, 2);
			this.tableLayoutPanelBody.Controls.Add(this.labelSplitter2, 0, 6);
			this.tableLayoutPanelBody.Location = new System.Drawing.Point(0, 62);
			this.tableLayoutPanelBody.Name = "tableLayoutPanelBody";
			this.tableLayoutPanelBody.Padding = new System.Windows.Forms.Padding(3);
			this.tableLayoutPanelBody.RowCount = 10;
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 13F));
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 13F));
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelBody.Size = new System.Drawing.Size(609, 528);
			this.tableLayoutPanelBody.TabIndex = 3;
			// 
			// labelSetThresholdForAnsweredCalls
			// 
			this.labelSetThresholdForAnsweredCalls.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelSetThresholdForAnsweredCalls.Location = new System.Drawing.Point(6, 88);
			this.labelSetThresholdForAnsweredCalls.Name = "labelSetThresholdForAnsweredCalls";
			this.labelSetThresholdForAnsweredCalls.Size = new System.Drawing.Size(197, 20);
			this.labelSetThresholdForAnsweredCalls.TabIndex = 0;
			this.labelSetThresholdForAnsweredCalls.Text = "xxSetBadgeThresholdForAnsweredCalls";
			this.labelSetThresholdForAnsweredCalls.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelSetThresholdForAHT
			// 
			this.labelSetThresholdForAHT.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelSetThresholdForAHT.Location = new System.Drawing.Point(6, 122);
			this.labelSetThresholdForAHT.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.labelSetThresholdForAHT.Name = "labelSetThresholdForAHT";
			this.labelSetThresholdForAHT.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this.labelSetThresholdForAHT.Size = new System.Drawing.Size(197, 20);
			this.labelSetThresholdForAHT.TabIndex = 3;
			this.labelSetThresholdForAHT.Text = "xxSetBadgeThresholdForAHT";
			this.labelSetThresholdForAHT.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelSetThresholdForAdherence
			// 
			this.labelSetThresholdForAdherence.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelSetThresholdForAdherence.Location = new System.Drawing.Point(6, 157);
			this.labelSetThresholdForAdherence.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.labelSetThresholdForAdherence.Name = "labelSetThresholdForAdherence";
			this.labelSetThresholdForAdherence.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this.labelSetThresholdForAdherence.Size = new System.Drawing.Size(197, 20);
			this.labelSetThresholdForAdherence.TabIndex = 3;
			this.labelSetThresholdForAdherence.Text = "xxSetBadgeThresholdForAdherence";
			this.labelSetThresholdForAdherence.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// doubleTextBoxThresholdForAdherence
			// 
			this.doubleTextBoxThresholdForAdherence.BackGroundColor = System.Drawing.SystemColors.Window;
			this.doubleTextBoxThresholdForAdherence.BeforeTouchSize = new System.Drawing.Size(174, 23);
			this.doubleTextBoxThresholdForAdherence.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.doubleTextBoxThresholdForAdherence.DoubleValue = 0D;
			this.doubleTextBoxThresholdForAdherence.Location = new System.Drawing.Point(268, 154);
			this.doubleTextBoxThresholdForAdherence.MaxValue = 100D;
			this.doubleTextBoxThresholdForAdherence.Metrocolor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.doubleTextBoxThresholdForAdherence.MinValue = 0D;
			this.doubleTextBoxThresholdForAdherence.Name = "doubleTextBoxThresholdForAdherence";
			this.doubleTextBoxThresholdForAdherence.NullString = "";
			this.doubleTextBoxThresholdForAdherence.Size = new System.Drawing.Size(174, 23);
			this.doubleTextBoxThresholdForAdherence.Style = Syncfusion.Windows.Forms.Tools.TextBoxExt.theme.Default;
			this.doubleTextBoxThresholdForAdherence.TabIndex = 4;
			this.doubleTextBoxThresholdForAdherence.Text = "0.00";
			// 
			// labelSetGoldBadgeDaysThreshold
			// 
			this.labelSetGoldBadgeDaysThreshold.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelSetGoldBadgeDaysThreshold.AutoSize = true;
			this.labelSetGoldBadgeDaysThreshold.Location = new System.Drawing.Point(6, 242);
			this.labelSetGoldBadgeDaysThreshold.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.labelSetGoldBadgeDaysThreshold.Name = "labelSetGoldBadgeDaysThreshold";
			this.labelSetGoldBadgeDaysThreshold.Size = new System.Drawing.Size(169, 15);
			this.labelSetGoldBadgeDaysThreshold.TabIndex = 8;
			this.labelSetGoldBadgeDaysThreshold.Text = "xxSetGoldBadgeDaysThreshold";
			// 
			// labelSetSilverBadgeDaysThreshold
			// 
			this.labelSetSilverBadgeDaysThreshold.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelSetSilverBadgeDaysThreshold.AutoSize = true;
			this.labelSetSilverBadgeDaysThreshold.Location = new System.Drawing.Point(6, 207);
			this.labelSetSilverBadgeDaysThreshold.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.labelSetSilverBadgeDaysThreshold.Name = "labelSetSilverBadgeDaysThreshold";
			this.labelSetSilverBadgeDaysThreshold.Size = new System.Drawing.Size(172, 15);
			this.labelSetSilverBadgeDaysThreshold.TabIndex = 7;
			this.labelSetSilverBadgeDaysThreshold.Text = "xxSetSilverBadgeDaysThreshold";
			// 
			// numericUpDownGoldenBadgeDaysThreshold
			// 
			this.numericUpDownGoldenBadgeDaysThreshold.Location = new System.Drawing.Point(268, 237);
			this.numericUpDownGoldenBadgeDaysThreshold.Name = "numericUpDownGoldenBadgeDaysThreshold";
			this.numericUpDownGoldenBadgeDaysThreshold.Size = new System.Drawing.Size(175, 23);
			this.numericUpDownGoldenBadgeDaysThreshold.TabIndex = 6;
			// 
			// numericUpDownSilverBadgeDaysThreshold
			// 
			this.numericUpDownSilverBadgeDaysThreshold.Location = new System.Drawing.Point(268, 202);
			this.numericUpDownSilverBadgeDaysThreshold.Name = "numericUpDownSilverBadgeDaysThreshold";
			this.numericUpDownSilverBadgeDaysThreshold.Size = new System.Drawing.Size(175, 23);
			this.numericUpDownSilverBadgeDaysThreshold.TabIndex = 5;
			// 
			// numericUpDownThresholdForAnsweredCalls
			// 
			this.numericUpDownThresholdForAnsweredCalls.Location = new System.Drawing.Point(268, 84);
			this.numericUpDownThresholdForAnsweredCalls.Name = "numericUpDownThresholdForAnsweredCalls";
			this.numericUpDownThresholdForAnsweredCalls.Size = new System.Drawing.Size(175, 23);
			this.numericUpDownThresholdForAnsweredCalls.TabIndex = 2;
			// 
			// timeSpanTextBoxThresholdForAHT
			// 
			this.timeSpanTextBoxThresholdForAHT.AlignTextBoxText = System.Windows.Forms.HorizontalAlignment.Left;
			this.timeSpanTextBoxThresholdForAHT.AllowNegativeValues = true;
			this.timeSpanTextBoxThresholdForAHT.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.timeSpanTextBoxThresholdForAHT.DefaultInterpretAsMinutes = true;
			this.timeSpanTextBoxThresholdForAHT.Location = new System.Drawing.Point(268, 122);
			this.timeSpanTextBoxThresholdForAHT.Margin = new System.Windows.Forms.Padding(3, 2, 0, 0);
			this.timeSpanTextBoxThresholdForAHT.MaximumValue = System.TimeSpan.Parse("1.00:00:00");
			this.timeSpanTextBoxThresholdForAHT.Name = "timeSpanTextBoxThresholdForAHT";
			this.timeSpanTextBoxThresholdForAHT.Size = new System.Drawing.Size(237, 25);
			this.timeSpanTextBoxThresholdForAHT.TabIndex = 3;
			this.timeSpanTextBoxThresholdForAHT.TimeFormat = Teleopti.Interfaces.Domain.TimeFormatsType.HoursMinutes;
			this.timeSpanTextBoxThresholdForAHT.TimeSpanBoxHeight = 23;
			this.timeSpanTextBoxThresholdForAHT.TimeSpanBoxWidth = 272;
			// 
			// checkBoxEnableBadge
			// 
			this.checkBoxEnableBadge.AutoSize = true;
			this.checkBoxEnableBadge.Location = new System.Drawing.Point(6, 6);
			this.checkBoxEnableBadge.Name = "checkBoxEnableBadge";
			this.checkBoxEnableBadge.Size = new System.Drawing.Size(104, 19);
			this.checkBoxEnableBadge.TabIndex = 0;
			this.checkBoxEnableBadge.Text = "xxEnableBadge";
			this.checkBoxEnableBadge.UseVisualStyleBackColor = true;
			this.checkBoxEnableBadge.CheckedChanged += new System.EventHandler(this.checkBoxEnableBadge_CheckedChanged);
			// 
			// labelCalculationTime
			// 
			this.labelCalculationTime.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelCalculationTime.Location = new System.Drawing.Point(6, 40);
			this.labelCalculationTime.Name = "labelCalculationTime";
			this.labelCalculationTime.Size = new System.Drawing.Size(197, 20);
			this.labelCalculationTime.TabIndex = 0;
			this.labelCalculationTime.Text = "xxCalculationTime";
			this.labelCalculationTime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// timeSpanTextBoxCalculationTime
			// 
			this.timeSpanTextBoxCalculationTime.AlignTextBoxText = System.Windows.Forms.HorizontalAlignment.Left;
			this.timeSpanTextBoxCalculationTime.AllowNegativeValues = true;
			this.timeSpanTextBoxCalculationTime.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.timeSpanTextBoxCalculationTime.DefaultInterpretAsMinutes = true;
			this.timeSpanTextBoxCalculationTime.Location = new System.Drawing.Point(268, 37);
			this.timeSpanTextBoxCalculationTime.Margin = new System.Windows.Forms.Padding(3, 2, 0, 0);
			this.timeSpanTextBoxCalculationTime.MaximumValue = System.TimeSpan.Parse("23:59:00");
			this.timeSpanTextBoxCalculationTime.Name = "timeSpanTextBoxCalculationTime";
			this.timeSpanTextBoxCalculationTime.Size = new System.Drawing.Size(237, 29);
			this.timeSpanTextBoxCalculationTime.TabIndex = 1;
			this.timeSpanTextBoxCalculationTime.TimeFormat = Teleopti.Interfaces.Domain.TimeFormatsType.HoursMinutes;
			this.timeSpanTextBoxCalculationTime.TimeSpanBoxHeight = 23;
			this.timeSpanTextBoxCalculationTime.TimeSpanBoxWidth = 272;
			// 
			// labelSplitter1
			// 
			this.labelSplitter1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.tableLayoutPanelBody.SetColumnSpan(this.labelSplitter1, 2);
			this.labelSplitter1.Location = new System.Drawing.Point(6, 73);
			this.labelSplitter1.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
			this.labelSplitter1.Name = "labelSplitter1";
			this.labelSplitter1.Size = new System.Drawing.Size(597, 2);
			this.labelSplitter1.TabIndex = 16;
			// 
			// gradientPanelHeader
			// 
			this.gradientPanelHeader.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.gradientPanelHeader.BackColor = System.Drawing.SystemColors.ActiveCaption;
			this.gradientPanelHeader.BorderSingle = System.Windows.Forms.ButtonBorderStyle.None;
			this.gradientPanelHeader.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.gradientPanelHeader.Controls.Add(this.tableLayoutPanelHeader);
			this.gradientPanelHeader.Dock = System.Windows.Forms.DockStyle.Top;
			this.gradientPanelHeader.Location = new System.Drawing.Point(0, 0);
			this.gradientPanelHeader.Name = "gradientPanelHeader";
			this.gradientPanelHeader.Padding = new System.Windows.Forms.Padding(12);
			this.gradientPanelHeader.Size = new System.Drawing.Size(609, 62);
			this.gradientPanelHeader.TabIndex = 0;
			// 
			// tableLayoutPanelHeader
			// 
			this.tableLayoutPanelHeader.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanelHeader.ColumnCount = 1;
			this.tableLayoutPanelHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 673F));
			this.tableLayoutPanelHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelHeader.Controls.Add(this.labelHeader, 1, 0);
			this.tableLayoutPanelHeader.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelHeader.Location = new System.Drawing.Point(12, 12);
			this.tableLayoutPanelHeader.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanelHeader.Name = "tableLayoutPanelHeader";
			this.tableLayoutPanelHeader.RowCount = 1;
			this.tableLayoutPanelHeader.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelHeader.Size = new System.Drawing.Size(585, 38);
			this.tableLayoutPanelHeader.TabIndex = 0;
			// 
			// labelHeader
			// 
			this.labelHeader.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelHeader.AutoSize = true;
			this.labelHeader.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelHeader.ForeColor = System.Drawing.Color.MidnightBlue;
			this.labelHeader.Location = new System.Drawing.Point(3, 6);
			this.labelHeader.Name = "labelHeader";
			this.labelHeader.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
			this.labelHeader.Size = new System.Drawing.Size(302, 25);
			this.labelHeader.TabIndex = 0;
			this.labelHeader.Text = "xxAgentBadgeThresholdSetting";
			// 
			// tableLayoutPanel5
			// 
			this.tableLayoutPanel5.BackColor = System.Drawing.Color.LightSteelBlue;
			this.tableLayoutPanel5.ColumnCount = 3;
			this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel5.Controls.Add(this.label1, 0, 0);
			this.tableLayoutPanel5.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel5.Name = "tableLayoutPanel5";
			this.tableLayoutPanel5.RowCount = 1;
			this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel5.Size = new System.Drawing.Size(200, 100);
			this.tableLayoutPanel5.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
			this.label1.ForeColor = System.Drawing.Color.GhostWhite;
			this.label1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.label1.Location = new System.Drawing.Point(3, 0);
			this.label1.Name = "label1";
			this.label1.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
			this.label1.Size = new System.Drawing.Size(167, 100);
			this.label1.TabIndex = 0;
			this.label1.Text = "xxChooseContractToChange";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// buttonDeleteContract
			// 
			this.buttonDeleteContract.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonDeleteContract.BeforeTouchSize = new System.Drawing.Size(24, 25);
			this.buttonDeleteContract.Image = global::Teleopti.Ccc.Win.Properties.Resources.test_delete_32x32;
			this.buttonDeleteContract.IsBackStageButton = false;
			this.buttonDeleteContract.Location = new System.Drawing.Point(145, 1);
			this.buttonDeleteContract.Margin = new System.Windows.Forms.Padding(3, 1, 0, 3);
			this.buttonDeleteContract.Name = "buttonDeleteContract";
			this.buttonDeleteContract.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
			this.buttonDeleteContract.Size = new System.Drawing.Size(24, 25);
			this.buttonDeleteContract.TabIndex = 7;
			this.buttonDeleteContract.TabStop = false;
			// 
			// labelSplitter2
			// 
			this.labelSplitter2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.labelSplitter2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.tableLayoutPanelBody.SetColumnSpan(this.labelSplitter2, 2);
			this.labelSplitter2.Location = new System.Drawing.Point(6, 194);
			this.labelSplitter2.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
			this.labelSplitter2.Name = "labelSplitter2";
			this.labelSplitter2.Size = new System.Drawing.Size(597, 2);
			this.labelSplitter2.TabIndex = 17;
			// 
			// BadgeThresholdSettings
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Window;
			this.Controls.Add(this.tableLayoutPanelBody);
			this.Controls.Add(this.gradientPanelHeader);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "BadgeThresholdSettings";
			this.Size = new System.Drawing.Size(609, 591);
			this.tableLayoutPanelBody.ResumeLayout(false);
			this.tableLayoutPanelBody.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.doubleTextBoxThresholdForAdherence)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownGoldenBadgeDaysThreshold)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownSilverBadgeDaysThreshold)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownThresholdForAnsweredCalls)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanelHeader)).EndInit();
			this.gradientPanelHeader.ResumeLayout(false);
			this.tableLayoutPanelHeader.ResumeLayout(false);
			this.tableLayoutPanelHeader.PerformLayout();
			this.tableLayoutPanel5.ResumeLayout(false);
			this.tableLayoutPanel5.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanelHeader;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelHeader;
        private System.Windows.Forms.Label labelHeader;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelBody;
		private System.Windows.Forms.Label labelSetThresholdForAnsweredCalls;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.Label label1;
        private Syncfusion.Windows.Forms.ButtonAdv buttonDeleteContract;
		private System.Windows.Forms.Label labelSetThresholdForAHT;
		private System.Windows.Forms.Label labelSetThresholdForAdherence;
		private System.Windows.Forms.Label labelSetSilverBadgeDaysThreshold;
		private System.Windows.Forms.Label labelSetGoldBadgeDaysThreshold;
		private System.Windows.Forms.NumericUpDown numericUpDownGoldenBadgeDaysThreshold;
		private System.Windows.Forms.NumericUpDown numericUpDownSilverBadgeDaysThreshold;
		private System.Windows.Forms.NumericUpDown numericUpDownThresholdForAnsweredCalls;
		private Controls.TimeSpanTextBox timeSpanTextBoxThresholdForAHT;
		private Syncfusion.Windows.Forms.Tools.DoubleTextBox doubleTextBoxThresholdForAdherence;
		private System.Windows.Forms.CheckBox checkBoxEnableBadge;
		private System.Windows.Forms.Label labelCalculationTime;
		private Controls.TimeSpanTextBox timeSpanTextBoxCalculationTime;
		private System.Windows.Forms.Label labelSplitter1;
		private System.Windows.Forms.Label labelSplitter2;
    }
}