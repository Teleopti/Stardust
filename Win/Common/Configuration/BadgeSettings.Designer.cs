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
			this.buttonDeleteContract = new Syncfusion.Windows.Forms.ButtonAdv();
			this.tableLayoutPanelBody = new System.Windows.Forms.TableLayoutPanel();
			this.labelSplitter5 = new System.Windows.Forms.Label();
			this.labelSetBronzeThresholdForAnsweredCalls = new System.Windows.Forms.Label();
			this.labelSetBronzeThresholdForAHT = new System.Windows.Forms.Label();
			this.labelSetBronzeThresholdForAdherence = new System.Windows.Forms.Label();
			this.doubleTextBoxBronzeThresholdForAdherence = new Syncfusion.Windows.Forms.Tools.DoubleTextBox();
			this.labelOneGoldBadgeEqualsSilverBadgeCount = new System.Windows.Forms.Label();
			this.labelOneSilverBadgeEqualsBronzeBadgeCount = new System.Windows.Forms.Label();
			this.numericUpDownGoldToSilverBadgeRate = new System.Windows.Forms.NumericUpDown();
			this.numericUpDownSilverToBronzeBadgeRate = new System.Windows.Forms.NumericUpDown();
			this.numericUpDownBronzeThresholdForAnsweredCalls = new System.Windows.Forms.NumericUpDown();
			this.checkBoxEnableBadge = new System.Windows.Forms.CheckBox();
			this.labelSplitter1 = new System.Windows.Forms.Label();
			this.labelSplitter4 = new System.Windows.Forms.Label();
			this.tableLayoutPanelSubHeader1 = new System.Windows.Forms.TableLayoutPanel();
			this.labelSubHeader1 = new System.Windows.Forms.Label();
			this.reset = new System.Windows.Forms.Button();
			this.checkBoxAHTBadgeEnabled = new System.Windows.Forms.CheckBox();
			this.checkBoxAnsweredCallsBadgeEnabled = new System.Windows.Forms.CheckBox();
			this.checkBoxAdherenceBadgeEnabled = new System.Windows.Forms.CheckBox();
			this.checkBoxCalculateBadgeWithRank = new System.Windows.Forms.CheckBox();
			this.labelSplitter2 = new System.Windows.Forms.Label();
			this.labelSplitter3 = new System.Windows.Forms.Label();
			this.labelSetSilverThresholdForAnsweredCalls = new System.Windows.Forms.Label();
			this.labelSetGoldThresholdForAnsweredCalls = new System.Windows.Forms.Label();
			this.numericUpDownSilverThresholdForAnsweredCalls = new System.Windows.Forms.NumericUpDown();
			this.numericUpDownGoldThresholdForAnsweredCalls = new System.Windows.Forms.NumericUpDown();
			this.labelSetSilverThresholdForAHT = new System.Windows.Forms.Label();
			this.labelSetGoldThresholdForAHT = new System.Windows.Forms.Label();
			this.labelSetSilverThresholdForAdherence = new System.Windows.Forms.Label();
			this.labelSetGoldThresholdForAdherence = new System.Windows.Forms.Label();
			this.doubleTextBoxSilverThresholdForAdherence = new Syncfusion.Windows.Forms.Tools.DoubleTextBox();
			this.doubleTextBoxGoldThresholdForAdherence = new Syncfusion.Windows.Forms.Tools.DoubleTextBox();
			this.labelSetThresholdForAnsweredCalls = new System.Windows.Forms.Label();
			this.numericUpDownThresholdForAnsweredCalls = new System.Windows.Forms.NumericUpDown();
			this.labelSetThresholdForAHT = new System.Windows.Forms.Label();
			this.labelSetThresholdForAdherence = new System.Windows.Forms.Label();
			this.doubleTextBoxThresholdForAdherence = new Syncfusion.Windows.Forms.Tools.DoubleTextBox();
			this.gradientPanelHeader = new Syncfusion.Windows.Forms.Tools.GradientPanel();
			this.tableLayoutPanelHeader = new System.Windows.Forms.TableLayoutPanel();
			this.labelHeader = new System.Windows.Forms.Label();
			this.timeSpanTextBoxBronzeThresholdForAHT = new Teleopti.Ccc.Win.Common.Controls.TimeSpanTextBox();
			this.timeSpanTextBoxSilverThresholdForAHT = new Teleopti.Ccc.Win.Common.Controls.TimeSpanTextBox();
			this.timeSpanTextBoxGoldThresholdForAHT = new Teleopti.Ccc.Win.Common.Controls.TimeSpanTextBox();
			this.timeSpanTextBoxThresholdForAHT = new Teleopti.Ccc.Win.Common.Controls.TimeSpanTextBox();
			this.tableLayoutPanelBody.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.doubleTextBoxBronzeThresholdForAdherence)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownGoldToSilverBadgeRate)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownSilverToBronzeBadgeRate)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownBronzeThresholdForAnsweredCalls)).BeginInit();
			this.tableLayoutPanelSubHeader1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownSilverThresholdForAnsweredCalls)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownGoldThresholdForAnsweredCalls)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.doubleTextBoxSilverThresholdForAdherence)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.doubleTextBoxGoldThresholdForAdherence)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownThresholdForAnsweredCalls)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.doubleTextBoxThresholdForAdherence)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanelHeader)).BeginInit();
			this.gradientPanelHeader.SuspendLayout();
			this.tableLayoutPanelHeader.SuspendLayout();
			this.SuspendLayout();
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
			// tableLayoutPanelBody
			// 
			this.tableLayoutPanelBody.ColumnCount = 3;
			this.tableLayoutPanelBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 296F));
			this.tableLayoutPanelBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 154F));
			this.tableLayoutPanelBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelBody.Controls.Add(this.labelSplitter5, 0, 23);
			this.tableLayoutPanelBody.Controls.Add(this.labelSetBronzeThresholdForAnsweredCalls, 0, 5);
			this.tableLayoutPanelBody.Controls.Add(this.labelSetBronzeThresholdForAHT, 0, 11);
			this.tableLayoutPanelBody.Controls.Add(this.labelSetBronzeThresholdForAdherence, 0, 17);
			this.tableLayoutPanelBody.Controls.Add(this.doubleTextBoxBronzeThresholdForAdherence, 1, 17);
			this.tableLayoutPanelBody.Controls.Add(this.labelOneGoldBadgeEqualsSilverBadgeCount, 0, 22);
			this.tableLayoutPanelBody.Controls.Add(this.labelOneSilverBadgeEqualsBronzeBadgeCount, 0, 21);
			this.tableLayoutPanelBody.Controls.Add(this.numericUpDownGoldToSilverBadgeRate, 1, 22);
			this.tableLayoutPanelBody.Controls.Add(this.numericUpDownSilverToBronzeBadgeRate, 1, 21);
			this.tableLayoutPanelBody.Controls.Add(this.numericUpDownBronzeThresholdForAnsweredCalls, 1, 5);
			this.tableLayoutPanelBody.Controls.Add(this.timeSpanTextBoxBronzeThresholdForAHT, 1, 11);
			this.tableLayoutPanelBody.Controls.Add(this.checkBoxEnableBadge, 0, 1);
			this.tableLayoutPanelBody.Controls.Add(this.labelSplitter1, 0, 2);
			this.tableLayoutPanelBody.Controls.Add(this.labelSplitter4, 0, 20);
			this.tableLayoutPanelBody.Controls.Add(this.tableLayoutPanelSubHeader1, 0, 0);
			this.tableLayoutPanelBody.Controls.Add(this.reset, 0, 25);
			this.tableLayoutPanelBody.Controls.Add(this.checkBoxAHTBadgeEnabled, 0, 9);
			this.tableLayoutPanelBody.Controls.Add(this.checkBoxAnsweredCallsBadgeEnabled, 0, 3);
			this.tableLayoutPanelBody.Controls.Add(this.checkBoxAdherenceBadgeEnabled, 0, 15);
			this.tableLayoutPanelBody.Controls.Add(this.checkBoxCalculateBadgeWithRank, 1, 1);
			this.tableLayoutPanelBody.Controls.Add(this.labelSplitter2, 0, 8);
			this.tableLayoutPanelBody.Controls.Add(this.labelSplitter3, 0, 14);
			this.tableLayoutPanelBody.Controls.Add(this.labelSetSilverThresholdForAnsweredCalls, 0, 6);
			this.tableLayoutPanelBody.Controls.Add(this.labelSetGoldThresholdForAnsweredCalls, 0, 7);
			this.tableLayoutPanelBody.Controls.Add(this.numericUpDownSilverThresholdForAnsweredCalls, 1, 6);
			this.tableLayoutPanelBody.Controls.Add(this.numericUpDownGoldThresholdForAnsweredCalls, 1, 7);
			this.tableLayoutPanelBody.Controls.Add(this.labelSetSilverThresholdForAHT, 0, 12);
			this.tableLayoutPanelBody.Controls.Add(this.labelSetGoldThresholdForAHT, 0, 13);
			this.tableLayoutPanelBody.Controls.Add(this.timeSpanTextBoxSilverThresholdForAHT, 1, 12);
			this.tableLayoutPanelBody.Controls.Add(this.timeSpanTextBoxGoldThresholdForAHT, 1, 13);
			this.tableLayoutPanelBody.Controls.Add(this.labelSetSilverThresholdForAdherence, 0, 18);
			this.tableLayoutPanelBody.Controls.Add(this.labelSetGoldThresholdForAdherence, 0, 19);
			this.tableLayoutPanelBody.Controls.Add(this.doubleTextBoxSilverThresholdForAdherence, 1, 18);
			this.tableLayoutPanelBody.Controls.Add(this.doubleTextBoxGoldThresholdForAdherence, 1, 19);
			this.tableLayoutPanelBody.Controls.Add(this.labelSetThresholdForAnsweredCalls, 0, 4);
			this.tableLayoutPanelBody.Controls.Add(this.numericUpDownThresholdForAnsweredCalls, 1, 4);
			this.tableLayoutPanelBody.Controls.Add(this.labelSetThresholdForAHT, 0, 10);
			this.tableLayoutPanelBody.Controls.Add(this.timeSpanTextBoxThresholdForAHT, 1, 10);
			this.tableLayoutPanelBody.Controls.Add(this.labelSetThresholdForAdherence, 0, 16);
			this.tableLayoutPanelBody.Controls.Add(this.doubleTextBoxThresholdForAdherence, 1, 16);
			this.tableLayoutPanelBody.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelBody.Location = new System.Drawing.Point(0, 62);
			this.tableLayoutPanelBody.Name = "tableLayoutPanelBody";
			this.tableLayoutPanelBody.RowCount = 26;
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 13F));
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 13F));
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 13F));
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 13F));
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 13F));
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelBody.Size = new System.Drawing.Size(609, 733);
			this.tableLayoutPanelBody.TabIndex = 3;
			// 
			// labelSplitter5
			// 
			this.labelSplitter5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.labelSplitter5.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.tableLayoutPanelBody.SetColumnSpan(this.labelSplitter5, 3);
			this.labelSplitter5.Location = new System.Drawing.Point(3, 725);
			this.labelSplitter5.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
			this.labelSplitter5.Name = "labelSplitter5";
			this.labelSplitter5.Size = new System.Drawing.Size(603, 2);
			this.labelSplitter5.TabIndex = 26;
			// 
			// labelSetBronzeThresholdForAnsweredCalls
			// 
			this.labelSetBronzeThresholdForAnsweredCalls.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelSetBronzeThresholdForAnsweredCalls.AutoSize = true;
			this.labelSetBronzeThresholdForAnsweredCalls.Location = new System.Drawing.Point(3, 163);
			this.labelSetBronzeThresholdForAnsweredCalls.Name = "labelSetBronzeThresholdForAnsweredCalls";
			this.labelSetBronzeThresholdForAnsweredCalls.Size = new System.Drawing.Size(249, 15);
			this.labelSetBronzeThresholdForAnsweredCalls.TabIndex = 0;
			this.labelSetBronzeThresholdForAnsweredCalls.Text = "xxSetBadgeBronzeThresholdForAnsweredCalls";
			this.labelSetBronzeThresholdForAnsweredCalls.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelSetBronzeThresholdForAHT
			// 
			this.labelSetBronzeThresholdForAHT.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelSetBronzeThresholdForAHT.AutoSize = true;
			this.labelSetBronzeThresholdForAHT.Location = new System.Drawing.Point(3, 348);
			this.labelSetBronzeThresholdForAHT.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.labelSetBronzeThresholdForAHT.Name = "labelSetBronzeThresholdForAHT";
			this.labelSetBronzeThresholdForAHT.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this.labelSetBronzeThresholdForAHT.Size = new System.Drawing.Size(196, 18);
			this.labelSetBronzeThresholdForAHT.TabIndex = 3;
			this.labelSetBronzeThresholdForAHT.Text = "xxSetBadgeBronzeThresholdForAHT";
			this.labelSetBronzeThresholdForAHT.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelSetBronzeThresholdForAdherence
			// 
			this.labelSetBronzeThresholdForAdherence.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelSetBronzeThresholdForAdherence.AutoSize = true;
			this.labelSetBronzeThresholdForAdherence.Location = new System.Drawing.Point(3, 536);
			this.labelSetBronzeThresholdForAdherence.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.labelSetBronzeThresholdForAdherence.Name = "labelSetBronzeThresholdForAdherence";
			this.labelSetBronzeThresholdForAdherence.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this.labelSetBronzeThresholdForAdherence.Size = new System.Drawing.Size(229, 18);
			this.labelSetBronzeThresholdForAdherence.TabIndex = 3;
			this.labelSetBronzeThresholdForAdherence.Text = "xxSetBadgeBronzeThresholdForAdherence";
			this.labelSetBronzeThresholdForAdherence.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// doubleTextBoxBronzeThresholdForAdherence
			// 
			this.doubleTextBoxBronzeThresholdForAdherence.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.doubleTextBoxBronzeThresholdForAdherence.BackGroundColor = System.Drawing.SystemColors.Window;
			this.doubleTextBoxBronzeThresholdForAdherence.BeforeTouchSize = new System.Drawing.Size(115, 23);
			this.doubleTextBoxBronzeThresholdForAdherence.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.doubleTextBoxBronzeThresholdForAdherence.DoubleValue = 0D;
			this.doubleTextBoxBronzeThresholdForAdherence.Enabled = false;
			this.doubleTextBoxBronzeThresholdForAdherence.Location = new System.Drawing.Point(299, 535);
			this.doubleTextBoxBronzeThresholdForAdherence.MaxValue = 100D;
			this.doubleTextBoxBronzeThresholdForAdherence.Metrocolor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.doubleTextBoxBronzeThresholdForAdherence.MinValue = 0D;
			this.doubleTextBoxBronzeThresholdForAdherence.Name = "doubleTextBoxBronzeThresholdForAdherence";
			this.doubleTextBoxBronzeThresholdForAdherence.NullString = "";
			this.doubleTextBoxBronzeThresholdForAdherence.Size = new System.Drawing.Size(115, 23);
			this.doubleTextBoxBronzeThresholdForAdherence.Style = Syncfusion.Windows.Forms.Tools.TextBoxExt.theme.Default;
			this.doubleTextBoxBronzeThresholdForAdherence.TabIndex = 4;
			this.doubleTextBoxBronzeThresholdForAdherence.Text = "0.00";
			// 
			// labelOneGoldBadgeEqualsSilverBadgeCount
			// 
			this.labelOneGoldBadgeEqualsSilverBadgeCount.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelOneGoldBadgeEqualsSilverBadgeCount.AutoSize = true;
			this.labelOneGoldBadgeEqualsSilverBadgeCount.Location = new System.Drawing.Point(3, 690);
			this.labelOneGoldBadgeEqualsSilverBadgeCount.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.labelOneGoldBadgeEqualsSilverBadgeCount.Name = "labelOneGoldBadgeEqualsSilverBadgeCount";
			this.labelOneGoldBadgeEqualsSilverBadgeCount.Size = new System.Drawing.Size(225, 15);
			this.labelOneGoldBadgeEqualsSilverBadgeCount.TabIndex = 8;
			this.labelOneGoldBadgeEqualsSilverBadgeCount.Text = "xxOneGoldBadgeEqualsSilverBadgeCount";
			// 
			// labelOneSilverBadgeEqualsBronzeBadgeCount
			// 
			this.labelOneSilverBadgeEqualsBronzeBadgeCount.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelOneSilverBadgeEqualsBronzeBadgeCount.AutoSize = true;
			this.labelOneSilverBadgeEqualsBronzeBadgeCount.Location = new System.Drawing.Point(3, 655);
			this.labelOneSilverBadgeEqualsBronzeBadgeCount.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.labelOneSilverBadgeEqualsBronzeBadgeCount.Name = "labelOneSilverBadgeEqualsBronzeBadgeCount";
			this.labelOneSilverBadgeEqualsBronzeBadgeCount.Size = new System.Drawing.Size(236, 15);
			this.labelOneSilverBadgeEqualsBronzeBadgeCount.TabIndex = 7;
			this.labelOneSilverBadgeEqualsBronzeBadgeCount.Text = "xxOneSilverBadgeEqualsBronzeBadgeCount";
			// 
			// numericUpDownGoldToSilverBadgeRate
			// 
			this.numericUpDownGoldToSilverBadgeRate.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.numericUpDownGoldToSilverBadgeRate.Enabled = false;
			this.numericUpDownGoldToSilverBadgeRate.Location = new System.Drawing.Point(299, 688);
			this.numericUpDownGoldToSilverBadgeRate.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
			this.numericUpDownGoldToSilverBadgeRate.Name = "numericUpDownGoldToSilverBadgeRate";
			this.numericUpDownGoldToSilverBadgeRate.Size = new System.Drawing.Size(115, 23);
			this.numericUpDownGoldToSilverBadgeRate.TabIndex = 6;
			this.numericUpDownGoldToSilverBadgeRate.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
			// 
			// numericUpDownSilverToBronzeBadgeRate
			// 
			this.numericUpDownSilverToBronzeBadgeRate.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.numericUpDownSilverToBronzeBadgeRate.Enabled = false;
			this.numericUpDownSilverToBronzeBadgeRate.Location = new System.Drawing.Point(299, 653);
			this.numericUpDownSilverToBronzeBadgeRate.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
			this.numericUpDownSilverToBronzeBadgeRate.Name = "numericUpDownSilverToBronzeBadgeRate";
			this.numericUpDownSilverToBronzeBadgeRate.Size = new System.Drawing.Size(115, 23);
			this.numericUpDownSilverToBronzeBadgeRate.TabIndex = 5;
			this.numericUpDownSilverToBronzeBadgeRate.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
			// 
			// numericUpDownBronzeThresholdForAnsweredCalls
			// 
			this.numericUpDownBronzeThresholdForAnsweredCalls.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.numericUpDownBronzeThresholdForAnsweredCalls.Enabled = false;
			this.numericUpDownBronzeThresholdForAnsweredCalls.Location = new System.Drawing.Point(299, 159);
			this.numericUpDownBronzeThresholdForAnsweredCalls.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
			this.numericUpDownBronzeThresholdForAnsweredCalls.Name = "numericUpDownBronzeThresholdForAnsweredCalls";
			this.numericUpDownBronzeThresholdForAnsweredCalls.Size = new System.Drawing.Size(115, 23);
			this.numericUpDownBronzeThresholdForAnsweredCalls.TabIndex = 2;
			// 
			// checkBoxEnableBadge
			// 
			this.checkBoxEnableBadge.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.checkBoxEnableBadge.AutoSize = true;
			this.checkBoxEnableBadge.Location = new System.Drawing.Point(3, 45);
			this.checkBoxEnableBadge.Name = "checkBoxEnableBadge";
			this.checkBoxEnableBadge.Size = new System.Drawing.Size(104, 19);
			this.checkBoxEnableBadge.TabIndex = 0;
			this.checkBoxEnableBadge.Text = "xxEnableBadge";
			this.checkBoxEnableBadge.UseVisualStyleBackColor = true;
			this.checkBoxEnableBadge.CheckedChanged += new System.EventHandler(this.checkBoxEnableBadge_CheckedChanged);
			// 
			// labelSplitter1
			// 
			this.labelSplitter1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.labelSplitter1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.tableLayoutPanelBody.SetColumnSpan(this.labelSplitter1, 3);
			this.labelSplitter1.Location = new System.Drawing.Point(3, 78);
			this.labelSplitter1.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
			this.labelSplitter1.Name = "labelSplitter1";
			this.labelSplitter1.Size = new System.Drawing.Size(603, 2);
			this.labelSplitter1.TabIndex = 16;
			// 
			// labelSplitter4
			// 
			this.labelSplitter4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.labelSplitter4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.tableLayoutPanelBody.SetColumnSpan(this.labelSplitter4, 3);
			this.labelSplitter4.Location = new System.Drawing.Point(3, 642);
			this.labelSplitter4.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
			this.labelSplitter4.Name = "labelSplitter4";
			this.labelSplitter4.Size = new System.Drawing.Size(603, 2);
			this.labelSplitter4.TabIndex = 17;
			// 
			// tableLayoutPanelSubHeader1
			// 
			this.tableLayoutPanelSubHeader1.BackColor = System.Drawing.Color.LightSteelBlue;
			this.tableLayoutPanelSubHeader1.ColumnCount = 1;
			this.tableLayoutPanelBody.SetColumnSpan(this.tableLayoutPanelSubHeader1, 3);
			this.tableLayoutPanelSubHeader1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelSubHeader1.Controls.Add(this.labelSubHeader1, 0, 0);
			this.tableLayoutPanelSubHeader1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelSubHeader1.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanelSubHeader1.Name = "tableLayoutPanelSubHeader1";
			this.tableLayoutPanelSubHeader1.RowCount = 1;
			this.tableLayoutPanelSubHeader1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelSubHeader1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
			this.tableLayoutPanelSubHeader1.Size = new System.Drawing.Size(603, 34);
			this.tableLayoutPanelSubHeader1.TabIndex = 20;
			// 
			// labelSubHeader1
			// 
			this.labelSubHeader1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelSubHeader1.AutoSize = true;
			this.labelSubHeader1.BackColor = System.Drawing.Color.Transparent;
			this.labelSubHeader1.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelSubHeader1.ForeColor = System.Drawing.Color.Black;
			this.labelSubHeader1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.labelSubHeader1.Location = new System.Drawing.Point(3, 8);
			this.labelSubHeader1.Name = "labelSubHeader1";
			this.labelSubHeader1.Size = new System.Drawing.Size(141, 17);
			this.labelSubHeader1.TabIndex = 1;
			this.labelSubHeader1.Text = "xxAgentBadgeSetting";
			this.labelSubHeader1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// reset
			// 
			this.tableLayoutPanelBody.SetColumnSpan(this.reset, 3);
			this.reset.Location = new System.Drawing.Point(3, 733);
			this.reset.Name = "reset";
			this.reset.Size = new System.Drawing.Size(104, 23);
			this.reset.TabIndex = 24;
			this.reset.Text = "xxResetBadges";
			this.reset.UseVisualStyleBackColor = true;
			this.reset.Click += new System.EventHandler(this.reset_Click);
			// 
			// checkBoxAHTBadgeEnabled
			// 
			this.checkBoxAHTBadgeEnabled.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.checkBoxAHTBadgeEnabled.AutoSize = true;
			this.checkBoxAHTBadgeEnabled.Location = new System.Drawing.Point(3, 279);
			this.checkBoxAHTBadgeEnabled.Name = "checkBoxAHTBadgeEnabled";
			this.checkBoxAHTBadgeEnabled.Size = new System.Drawing.Size(127, 19);
			this.checkBoxAHTBadgeEnabled.TabIndex = 22;
			this.checkBoxAHTBadgeEnabled.Text = "xxUseBadgeforAHT";
			this.checkBoxAHTBadgeEnabled.UseVisualStyleBackColor = true;
			this.checkBoxAHTBadgeEnabled.CheckedChanged += new System.EventHandler(this.checkBoxAHTBadgeEnabled_CheckedChanged);
			// 
			// checkBoxAnsweredCallsBadgeEnabled
			// 
			this.checkBoxAnsweredCallsBadgeEnabled.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.checkBoxAnsweredCallsBadgeEnabled.AutoSize = true;
			this.checkBoxAnsweredCallsBadgeEnabled.Location = new System.Drawing.Point(3, 91);
			this.checkBoxAnsweredCallsBadgeEnabled.Name = "checkBoxAnsweredCallsBadgeEnabled";
			this.checkBoxAnsweredCallsBadgeEnabled.Size = new System.Drawing.Size(182, 19);
			this.checkBoxAnsweredCallsBadgeEnabled.TabIndex = 21;
			this.checkBoxAnsweredCallsBadgeEnabled.Text = "xxUseBadgeForAnsweredCalls";
			this.checkBoxAnsweredCallsBadgeEnabled.UseVisualStyleBackColor = true;
			this.checkBoxAnsweredCallsBadgeEnabled.CheckedChanged += new System.EventHandler(this.checkBoxAnsweredCallsBadgeEnabled_CheckedChanged);
			// 
			// checkBoxAdherenceBadgeEnabled
			// 
			this.checkBoxAdherenceBadgeEnabled.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.checkBoxAdherenceBadgeEnabled.AutoSize = true;
			this.checkBoxAdherenceBadgeEnabled.Location = new System.Drawing.Point(3, 467);
			this.checkBoxAdherenceBadgeEnabled.Name = "checkBoxAdherenceBadgeEnabled";
			this.checkBoxAdherenceBadgeEnabled.Size = new System.Drawing.Size(160, 19);
			this.checkBoxAdherenceBadgeEnabled.TabIndex = 23;
			this.checkBoxAdherenceBadgeEnabled.Text = "xxUseBadgeforAdherence";
			this.checkBoxAdherenceBadgeEnabled.UseVisualStyleBackColor = true;
			this.checkBoxAdherenceBadgeEnabled.CheckedChanged += new System.EventHandler(this.checkBoxAdherenceBadgeEnabled_CheckedChanged);
			// 
			// checkBoxCalculateBadgeWithRank
			// 
			this.checkBoxCalculateBadgeWithRank.AutoSize = true;
			this.tableLayoutPanelBody.SetColumnSpan(this.checkBoxCalculateBadgeWithRank, 2);
			this.checkBoxCalculateBadgeWithRank.Location = new System.Drawing.Point(299, 43);
			this.checkBoxCalculateBadgeWithRank.Name = "checkBoxCalculateBadgeWithRank";
			this.checkBoxCalculateBadgeWithRank.Size = new System.Drawing.Size(242, 19);
			this.checkBoxCalculateBadgeWithRank.TabIndex = 25;
			this.checkBoxCalculateBadgeWithRank.Text = "xxCalculateBadgeWithRank";
			this.checkBoxCalculateBadgeWithRank.UseVisualStyleBackColor = true;
			this.checkBoxCalculateBadgeWithRank.CheckedChanged += new System.EventHandler(this.checkBoxEnableDifferentLevelBadges_CheckedChanged);
			// 
			// labelSplitter2
			// 
			this.labelSplitter2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.labelSplitter2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.tableLayoutPanelBody.SetColumnSpan(this.labelSplitter2, 3);
			this.labelSplitter2.Location = new System.Drawing.Point(3, 266);
			this.labelSplitter2.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
			this.labelSplitter2.Name = "labelSplitter2";
			this.labelSplitter2.Size = new System.Drawing.Size(603, 2);
			this.labelSplitter2.TabIndex = 16;
			// 
			// labelSplitter3
			// 
			this.labelSplitter3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.labelSplitter3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.tableLayoutPanelBody.SetColumnSpan(this.labelSplitter3, 3);
			this.labelSplitter3.Location = new System.Drawing.Point(3, 454);
			this.labelSplitter3.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
			this.labelSplitter3.Name = "labelSplitter3";
			this.labelSplitter3.Size = new System.Drawing.Size(603, 2);
			this.labelSplitter3.TabIndex = 16;
			// 
			// labelSetSilverThresholdForAnsweredCalls
			// 
			this.labelSetSilverThresholdForAnsweredCalls.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelSetSilverThresholdForAnsweredCalls.AutoSize = true;
			this.labelSetSilverThresholdForAnsweredCalls.Location = new System.Drawing.Point(3, 198);
			this.labelSetSilverThresholdForAnsweredCalls.Name = "labelSetSilverThresholdForAnsweredCalls";
			this.labelSetSilverThresholdForAnsweredCalls.Size = new System.Drawing.Size(241, 15);
			this.labelSetSilverThresholdForAnsweredCalls.TabIndex = 0;
			this.labelSetSilverThresholdForAnsweredCalls.Text = "xxSetBadgeSilverThresholdForAnsweredCalls";
			this.labelSetSilverThresholdForAnsweredCalls.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelSetGoldThresholdForAnsweredCalls
			// 
			this.labelSetGoldThresholdForAnsweredCalls.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelSetGoldThresholdForAnsweredCalls.AutoSize = true;
			this.labelSetGoldThresholdForAnsweredCalls.Location = new System.Drawing.Point(3, 233);
			this.labelSetGoldThresholdForAnsweredCalls.Name = "labelSetGoldThresholdForAnsweredCalls";
			this.labelSetGoldThresholdForAnsweredCalls.Size = new System.Drawing.Size(238, 15);
			this.labelSetGoldThresholdForAnsweredCalls.TabIndex = 0;
			this.labelSetGoldThresholdForAnsweredCalls.Text = "xxSetBadgeGoldThresholdForAnsweredCalls";
			this.labelSetGoldThresholdForAnsweredCalls.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// numericUpDownSilverThresholdForAnsweredCalls
			// 
			this.numericUpDownSilverThresholdForAnsweredCalls.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.numericUpDownSilverThresholdForAnsweredCalls.Enabled = false;
			this.numericUpDownSilverThresholdForAnsweredCalls.Location = new System.Drawing.Point(299, 194);
			this.numericUpDownSilverThresholdForAnsweredCalls.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
			this.numericUpDownSilverThresholdForAnsweredCalls.Name = "numericUpDownSilverThresholdForAnsweredCalls";
			this.numericUpDownSilverThresholdForAnsweredCalls.Size = new System.Drawing.Size(115, 23);
			this.numericUpDownSilverThresholdForAnsweredCalls.TabIndex = 2;
			// 
			// numericUpDownGoldThresholdForAnsweredCalls
			// 
			this.numericUpDownGoldThresholdForAnsweredCalls.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.numericUpDownGoldThresholdForAnsweredCalls.Enabled = false;
			this.numericUpDownGoldThresholdForAnsweredCalls.Location = new System.Drawing.Point(299, 229);
			this.numericUpDownGoldThresholdForAnsweredCalls.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
			this.numericUpDownGoldThresholdForAnsweredCalls.Name = "numericUpDownGoldThresholdForAnsweredCalls";
			this.numericUpDownGoldThresholdForAnsweredCalls.Size = new System.Drawing.Size(115, 23);
			this.numericUpDownGoldThresholdForAnsweredCalls.TabIndex = 2;
			// 
			// labelSetSilverThresholdForAHT
			// 
			this.labelSetSilverThresholdForAHT.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelSetSilverThresholdForAHT.AutoSize = true;
			this.labelSetSilverThresholdForAHT.Location = new System.Drawing.Point(3, 383);
			this.labelSetSilverThresholdForAHT.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.labelSetSilverThresholdForAHT.Name = "labelSetSilverThresholdForAHT";
			this.labelSetSilverThresholdForAHT.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this.labelSetSilverThresholdForAHT.Size = new System.Drawing.Size(188, 18);
			this.labelSetSilverThresholdForAHT.TabIndex = 3;
			this.labelSetSilverThresholdForAHT.Text = "xxSetBadgeSilverThresholdForAHT";
			this.labelSetSilverThresholdForAHT.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelSetGoldThresholdForAHT
			// 
			this.labelSetGoldThresholdForAHT.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelSetGoldThresholdForAHT.AutoSize = true;
			this.labelSetGoldThresholdForAHT.Location = new System.Drawing.Point(3, 418);
			this.labelSetGoldThresholdForAHT.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.labelSetGoldThresholdForAHT.Name = "labelSetGoldThresholdForAHT";
			this.labelSetGoldThresholdForAHT.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this.labelSetGoldThresholdForAHT.Size = new System.Drawing.Size(185, 18);
			this.labelSetGoldThresholdForAHT.TabIndex = 3;
			this.labelSetGoldThresholdForAHT.Text = "xxSetBadgeGoldThresholdForAHT";
			this.labelSetGoldThresholdForAHT.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelSetSilverThresholdForAdherence
			// 
			this.labelSetSilverThresholdForAdherence.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelSetSilverThresholdForAdherence.AutoSize = true;
			this.labelSetSilverThresholdForAdherence.Location = new System.Drawing.Point(3, 571);
			this.labelSetSilverThresholdForAdherence.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.labelSetSilverThresholdForAdherence.Name = "labelSetSilverThresholdForAdherence";
			this.labelSetSilverThresholdForAdherence.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this.labelSetSilverThresholdForAdherence.Size = new System.Drawing.Size(221, 18);
			this.labelSetSilverThresholdForAdherence.TabIndex = 3;
			this.labelSetSilverThresholdForAdherence.Text = "xxSetBadgeSilverThresholdForAdherence";
			this.labelSetSilverThresholdForAdherence.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelSetGoldThresholdForAdherence
			// 
			this.labelSetGoldThresholdForAdherence.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelSetGoldThresholdForAdherence.AutoSize = true;
			this.labelSetGoldThresholdForAdherence.Location = new System.Drawing.Point(3, 606);
			this.labelSetGoldThresholdForAdherence.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.labelSetGoldThresholdForAdherence.Name = "labelSetGoldThresholdForAdherence";
			this.labelSetGoldThresholdForAdherence.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this.labelSetGoldThresholdForAdherence.Size = new System.Drawing.Size(218, 18);
			this.labelSetGoldThresholdForAdherence.TabIndex = 3;
			this.labelSetGoldThresholdForAdherence.Text = "xxSetBadgeGoldThresholdForAdherence";
			this.labelSetGoldThresholdForAdherence.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// doubleTextBoxSilverThresholdForAdherence
			// 
			this.doubleTextBoxSilverThresholdForAdherence.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.doubleTextBoxSilverThresholdForAdherence.BackGroundColor = System.Drawing.SystemColors.Window;
			this.doubleTextBoxSilverThresholdForAdherence.BeforeTouchSize = new System.Drawing.Size(115, 23);
			this.doubleTextBoxSilverThresholdForAdherence.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.doubleTextBoxSilverThresholdForAdherence.DoubleValue = 0D;
			this.doubleTextBoxSilverThresholdForAdherence.Enabled = false;
			this.doubleTextBoxSilverThresholdForAdherence.Location = new System.Drawing.Point(299, 570);
			this.doubleTextBoxSilverThresholdForAdherence.MaxValue = 100D;
			this.doubleTextBoxSilverThresholdForAdherence.Metrocolor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.doubleTextBoxSilverThresholdForAdherence.MinValue = 0D;
			this.doubleTextBoxSilverThresholdForAdherence.Name = "doubleTextBoxSilverThresholdForAdherence";
			this.doubleTextBoxSilverThresholdForAdherence.NullString = "";
			this.doubleTextBoxSilverThresholdForAdherence.Size = new System.Drawing.Size(115, 23);
			this.doubleTextBoxSilverThresholdForAdherence.Style = Syncfusion.Windows.Forms.Tools.TextBoxExt.theme.Default;
			this.doubleTextBoxSilverThresholdForAdherence.TabIndex = 4;
			this.doubleTextBoxSilverThresholdForAdherence.Text = "0.00";
			// 
			// doubleTextBoxGoldThresholdForAdherence
			// 
			this.doubleTextBoxGoldThresholdForAdherence.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.doubleTextBoxGoldThresholdForAdherence.BackGroundColor = System.Drawing.SystemColors.Window;
			this.doubleTextBoxGoldThresholdForAdherence.BeforeTouchSize = new System.Drawing.Size(115, 23);
			this.doubleTextBoxGoldThresholdForAdherence.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.doubleTextBoxGoldThresholdForAdherence.DoubleValue = 0D;
			this.doubleTextBoxGoldThresholdForAdherence.Enabled = false;
			this.doubleTextBoxGoldThresholdForAdherence.Location = new System.Drawing.Point(299, 605);
			this.doubleTextBoxGoldThresholdForAdherence.MaxValue = 100D;
			this.doubleTextBoxGoldThresholdForAdherence.Metrocolor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.doubleTextBoxGoldThresholdForAdherence.MinValue = 0D;
			this.doubleTextBoxGoldThresholdForAdherence.Name = "doubleTextBoxGoldThresholdForAdherence";
			this.doubleTextBoxGoldThresholdForAdherence.NullString = "";
			this.doubleTextBoxGoldThresholdForAdherence.Size = new System.Drawing.Size(115, 23);
			this.doubleTextBoxGoldThresholdForAdherence.Style = Syncfusion.Windows.Forms.Tools.TextBoxExt.theme.Default;
			this.doubleTextBoxGoldThresholdForAdherence.TabIndex = 4;
			this.doubleTextBoxGoldThresholdForAdherence.Text = "0.00";
			// 
			// labelSetThresholdForAnsweredCalls
			// 
			this.labelSetThresholdForAnsweredCalls.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelSetThresholdForAnsweredCalls.AutoSize = true;
			this.labelSetThresholdForAnsweredCalls.Location = new System.Drawing.Point(3, 128);
			this.labelSetThresholdForAnsweredCalls.Name = "labelSetThresholdForAnsweredCalls";
			this.labelSetThresholdForAnsweredCalls.Size = new System.Drawing.Size(213, 15);
			this.labelSetThresholdForAnsweredCalls.TabIndex = 0;
			this.labelSetThresholdForAnsweredCalls.Text = "xxSetBadgeThresholdForAnsweredCalls";
			this.labelSetThresholdForAnsweredCalls.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// numericUpDownThresholdForAnsweredCalls
			// 
			this.numericUpDownThresholdForAnsweredCalls.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.numericUpDownThresholdForAnsweredCalls.Enabled = false;
			this.numericUpDownThresholdForAnsweredCalls.Location = new System.Drawing.Point(299, 124);
			this.numericUpDownThresholdForAnsweredCalls.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
			this.numericUpDownThresholdForAnsweredCalls.Name = "numericUpDownThresholdForAnsweredCalls";
			this.numericUpDownThresholdForAnsweredCalls.Size = new System.Drawing.Size(115, 23);
			this.numericUpDownThresholdForAnsweredCalls.TabIndex = 2;
			// 
			// labelSetThresholdForAHT
			// 
			this.labelSetThresholdForAHT.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelSetThresholdForAHT.AutoSize = true;
			this.labelSetThresholdForAHT.Location = new System.Drawing.Point(3, 313);
			this.labelSetThresholdForAHT.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.labelSetThresholdForAHT.Name = "labelSetThresholdForAHT";
			this.labelSetThresholdForAHT.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this.labelSetThresholdForAHT.Size = new System.Drawing.Size(160, 18);
			this.labelSetThresholdForAHT.TabIndex = 3;
			this.labelSetThresholdForAHT.Text = "xxSetBadgeThresholdForAHT";
			this.labelSetThresholdForAHT.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelSetThresholdForAdherence
			// 
			this.labelSetThresholdForAdherence.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelSetThresholdForAdherence.AutoSize = true;
			this.labelSetThresholdForAdherence.Location = new System.Drawing.Point(3, 501);
			this.labelSetThresholdForAdherence.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.labelSetThresholdForAdherence.Name = "labelSetThresholdForAdherence";
			this.labelSetThresholdForAdherence.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this.labelSetThresholdForAdherence.Size = new System.Drawing.Size(193, 18);
			this.labelSetThresholdForAdherence.TabIndex = 3;
			this.labelSetThresholdForAdherence.Text = "xxSetBadgeThresholdForAdherence";
			this.labelSetThresholdForAdherence.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// doubleTextBoxThresholdForAdherence
			// 
			this.doubleTextBoxThresholdForAdherence.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.doubleTextBoxThresholdForAdherence.BackGroundColor = System.Drawing.SystemColors.Window;
			this.doubleTextBoxThresholdForAdherence.BeforeTouchSize = new System.Drawing.Size(115, 23);
			this.doubleTextBoxThresholdForAdherence.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.doubleTextBoxThresholdForAdherence.DoubleValue = 0D;
			this.doubleTextBoxThresholdForAdherence.Enabled = false;
			this.doubleTextBoxThresholdForAdherence.Location = new System.Drawing.Point(299, 500);
			this.doubleTextBoxThresholdForAdherence.MaxValue = 100D;
			this.doubleTextBoxThresholdForAdherence.Metrocolor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.doubleTextBoxThresholdForAdherence.MinValue = 0D;
			this.doubleTextBoxThresholdForAdherence.Name = "doubleTextBoxThresholdForAdherence";
			this.doubleTextBoxThresholdForAdherence.NullString = "";
			this.doubleTextBoxThresholdForAdherence.Size = new System.Drawing.Size(115, 23);
			this.doubleTextBoxThresholdForAdherence.Style = Syncfusion.Windows.Forms.Tools.TextBoxExt.theme.Default;
			this.doubleTextBoxThresholdForAdherence.TabIndex = 4;
			this.doubleTextBoxThresholdForAdherence.Text = "0.00";
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
			this.labelHeader.Size = new System.Drawing.Size(214, 25);
			this.labelHeader.TabIndex = 0;
			this.labelHeader.Text = "xxAgentBadgeSetting";
			// 
			// timeSpanTextBoxBronzeThresholdForAHT
			// 
			this.timeSpanTextBoxBronzeThresholdForAHT.AlignTextBoxText = System.Windows.Forms.HorizontalAlignment.Left;
			this.timeSpanTextBoxBronzeThresholdForAHT.AllowNegativeValues = true;
			this.timeSpanTextBoxBronzeThresholdForAHT.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.timeSpanTextBoxBronzeThresholdForAHT.DefaultInterpretAsMinutes = true;
			this.timeSpanTextBoxBronzeThresholdForAHT.Enabled = false;
			this.timeSpanTextBoxBronzeThresholdForAHT.Location = new System.Drawing.Point(299, 347);
			this.timeSpanTextBoxBronzeThresholdForAHT.Margin = new System.Windows.Forms.Padding(3, 2, 0, 0);
			this.timeSpanTextBoxBronzeThresholdForAHT.MaximumValue = System.TimeSpan.Parse("01:00:00");
			this.timeSpanTextBoxBronzeThresholdForAHT.Name = "timeSpanTextBoxBronzeThresholdForAHT";
			this.timeSpanTextBoxBronzeThresholdForAHT.Size = new System.Drawing.Size(151, 25);
			this.timeSpanTextBoxBronzeThresholdForAHT.TabIndex = 3;
			this.timeSpanTextBoxBronzeThresholdForAHT.TimeFormat = Teleopti.Interfaces.Domain.TimeFormatsType.HoursMinutesSeconds;
			this.timeSpanTextBoxBronzeThresholdForAHT.TimeSpanBoxHeight = 23;
			this.timeSpanTextBoxBronzeThresholdForAHT.TimeSpanBoxWidth = 17822;
			// 
			// timeSpanTextBoxSilverThresholdForAHT
			// 
			this.timeSpanTextBoxSilverThresholdForAHT.AlignTextBoxText = System.Windows.Forms.HorizontalAlignment.Left;
			this.timeSpanTextBoxSilverThresholdForAHT.AllowNegativeValues = true;
			this.timeSpanTextBoxSilverThresholdForAHT.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.timeSpanTextBoxSilverThresholdForAHT.DefaultInterpretAsMinutes = true;
			this.timeSpanTextBoxSilverThresholdForAHT.Enabled = false;
			this.timeSpanTextBoxSilverThresholdForAHT.Location = new System.Drawing.Point(299, 380);
			this.timeSpanTextBoxSilverThresholdForAHT.Margin = new System.Windows.Forms.Padding(3, 2, 0, 0);
			this.timeSpanTextBoxSilverThresholdForAHT.MaximumValue = System.TimeSpan.Parse("01:00:00");
			this.timeSpanTextBoxSilverThresholdForAHT.Name = "timeSpanTextBoxSilverThresholdForAHT";
			this.timeSpanTextBoxSilverThresholdForAHT.Size = new System.Drawing.Size(151, 29);
			this.timeSpanTextBoxSilverThresholdForAHT.TabIndex = 3;
			this.timeSpanTextBoxSilverThresholdForAHT.TimeFormat = Teleopti.Interfaces.Domain.TimeFormatsType.HoursMinutesSeconds;
			this.timeSpanTextBoxSilverThresholdForAHT.TimeSpanBoxHeight = 23;
			this.timeSpanTextBoxSilverThresholdForAHT.TimeSpanBoxWidth = 20792;
			// 
			// timeSpanTextBoxGoldThresholdForAHT
			// 
			this.timeSpanTextBoxGoldThresholdForAHT.AlignTextBoxText = System.Windows.Forms.HorizontalAlignment.Left;
			this.timeSpanTextBoxGoldThresholdForAHT.AllowNegativeValues = true;
			this.timeSpanTextBoxGoldThresholdForAHT.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.timeSpanTextBoxGoldThresholdForAHT.DefaultInterpretAsMinutes = true;
			this.timeSpanTextBoxGoldThresholdForAHT.Enabled = false;
			this.timeSpanTextBoxGoldThresholdForAHT.Location = new System.Drawing.Point(299, 413);
			this.timeSpanTextBoxGoldThresholdForAHT.Margin = new System.Windows.Forms.Padding(3, 2, 0, 0);
			this.timeSpanTextBoxGoldThresholdForAHT.MaximumValue = System.TimeSpan.Parse("01:00:00");
			this.timeSpanTextBoxGoldThresholdForAHT.Name = "timeSpanTextBoxGoldThresholdForAHT";
			this.timeSpanTextBoxGoldThresholdForAHT.Size = new System.Drawing.Size(151, 33);
			this.timeSpanTextBoxGoldThresholdForAHT.TabIndex = 3;
			this.timeSpanTextBoxGoldThresholdForAHT.TimeFormat = Teleopti.Interfaces.Domain.TimeFormatsType.HoursMinutesSeconds;
			this.timeSpanTextBoxGoldThresholdForAHT.TimeSpanBoxHeight = 23;
			this.timeSpanTextBoxGoldThresholdForAHT.TimeSpanBoxWidth = 24257;
			// 
			// timeSpanTextBoxThresholdForAHT
			// 
			this.timeSpanTextBoxThresholdForAHT.AlignTextBoxText = System.Windows.Forms.HorizontalAlignment.Left;
			this.timeSpanTextBoxThresholdForAHT.AllowNegativeValues = true;
			this.timeSpanTextBoxThresholdForAHT.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.timeSpanTextBoxThresholdForAHT.DefaultInterpretAsMinutes = true;
			this.timeSpanTextBoxThresholdForAHT.Enabled = false;
			this.timeSpanTextBoxThresholdForAHT.Location = new System.Drawing.Point(299, 310);
			this.timeSpanTextBoxThresholdForAHT.Margin = new System.Windows.Forms.Padding(3, 2, 0, 0);
			this.timeSpanTextBoxThresholdForAHT.MaximumValue = System.TimeSpan.Parse("01:00:00");
			this.timeSpanTextBoxThresholdForAHT.Name = "timeSpanTextBoxThresholdForAHT";
			this.timeSpanTextBoxThresholdForAHT.Size = new System.Drawing.Size(151, 29);
			this.timeSpanTextBoxThresholdForAHT.TabIndex = 3;
			this.timeSpanTextBoxThresholdForAHT.TimeFormat = Teleopti.Interfaces.Domain.TimeFormatsType.HoursMinutesSeconds;
			this.timeSpanTextBoxThresholdForAHT.TimeSpanBoxHeight = 23;
			this.timeSpanTextBoxThresholdForAHT.TimeSpanBoxWidth = 20792;
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
			this.Size = new System.Drawing.Size(609, 795);
			this.tableLayoutPanelBody.ResumeLayout(false);
			this.tableLayoutPanelBody.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.doubleTextBoxBronzeThresholdForAdherence)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownGoldToSilverBadgeRate)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownSilverToBronzeBadgeRate)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownBronzeThresholdForAnsweredCalls)).EndInit();
			this.tableLayoutPanelSubHeader1.ResumeLayout(false);
			this.tableLayoutPanelSubHeader1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownSilverThresholdForAnsweredCalls)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownGoldThresholdForAnsweredCalls)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.doubleTextBoxSilverThresholdForAdherence)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.doubleTextBoxGoldThresholdForAdherence)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownThresholdForAnsweredCalls)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.doubleTextBoxThresholdForAdherence)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanelHeader)).EndInit();
			this.gradientPanelHeader.ResumeLayout(false);
			this.tableLayoutPanelHeader.ResumeLayout(false);
			this.tableLayoutPanelHeader.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanelHeader;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelHeader;
		private System.Windows.Forms.Label labelHeader;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelBody;
		private System.Windows.Forms.Label labelSetBronzeThresholdForAnsweredCalls;
		private Syncfusion.Windows.Forms.ButtonAdv buttonDeleteContract;
		private System.Windows.Forms.Label labelSetBronzeThresholdForAHT;
		private System.Windows.Forms.Label labelSetBronzeThresholdForAdherence;
		private System.Windows.Forms.Label labelOneSilverBadgeEqualsBronzeBadgeCount;
		private System.Windows.Forms.Label labelOneGoldBadgeEqualsSilverBadgeCount;
		private System.Windows.Forms.NumericUpDown numericUpDownGoldToSilverBadgeRate;
		private System.Windows.Forms.NumericUpDown numericUpDownSilverToBronzeBadgeRate;
		private System.Windows.Forms.NumericUpDown numericUpDownBronzeThresholdForAnsweredCalls;
		private Controls.TimeSpanTextBox timeSpanTextBoxBronzeThresholdForAHT;
		private Syncfusion.Windows.Forms.Tools.DoubleTextBox doubleTextBoxBronzeThresholdForAdherence;
		private System.Windows.Forms.CheckBox checkBoxEnableBadge;
		private System.Windows.Forms.Label labelSplitter4;
		private System.Windows.Forms.Label labelSplitter1;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelSubHeader1;
		private System.Windows.Forms.Label labelSubHeader1;
		private System.Windows.Forms.CheckBox checkBoxAnsweredCallsBadgeEnabled;
		private System.Windows.Forms.CheckBox checkBoxAHTBadgeEnabled;
		private System.Windows.Forms.CheckBox checkBoxAdherenceBadgeEnabled;
		private System.Windows.Forms.Button reset;
		private System.Windows.Forms.CheckBox checkBoxCalculateBadgeWithRank;
		private System.Windows.Forms.Label labelSplitter2;
		private System.Windows.Forms.Label labelSplitter3;
		private System.Windows.Forms.Label labelSetSilverThresholdForAnsweredCalls;
		private System.Windows.Forms.Label labelSetGoldThresholdForAnsweredCalls;
		private System.Windows.Forms.NumericUpDown numericUpDownSilverThresholdForAnsweredCalls;
		private System.Windows.Forms.NumericUpDown numericUpDownGoldThresholdForAnsweredCalls;
		private System.Windows.Forms.Label labelSetSilverThresholdForAHT;
		private System.Windows.Forms.Label labelSetGoldThresholdForAHT;
		private Controls.TimeSpanTextBox timeSpanTextBoxSilverThresholdForAHT;
		private Controls.TimeSpanTextBox timeSpanTextBoxGoldThresholdForAHT;
		private System.Windows.Forms.Label labelSetSilverThresholdForAdherence;
		private System.Windows.Forms.Label labelSetGoldThresholdForAdherence;
		private DoubleTextBox doubleTextBoxSilverThresholdForAdherence;
		private DoubleTextBox doubleTextBoxGoldThresholdForAdherence;
		private System.Windows.Forms.Label labelSplitter5;
		private System.Windows.Forms.Label labelSetThresholdForAnsweredCalls;
		private System.Windows.Forms.NumericUpDown numericUpDownThresholdForAnsweredCalls;
		private System.Windows.Forms.Label labelSetThresholdForAHT;
		private Controls.TimeSpanTextBox timeSpanTextBoxThresholdForAHT;
		private System.Windows.Forms.Label labelSetThresholdForAdherence;
		private DoubleTextBox doubleTextBoxThresholdForAdherence;
	}
}