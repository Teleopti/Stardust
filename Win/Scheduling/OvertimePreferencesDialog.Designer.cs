namespace Teleopti.Ccc.Win.Scheduling
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
            this.checkBoxAllowBreakingNightlyRest = new System.Windows.Forms.CheckBox();
            this.checkBoxAllowBreakingWeeklyRest = new System.Windows.Forms.CheckBox();
            this.checkBoxAllowBreakingMaxTimePerWeek = new System.Windows.Forms.CheckBox();
            this.comboBoxAdvOvertimeType = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
            this.comboBoxAdvActivity = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
            this.comboBoxAdvTag = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
            this.label3 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.fromToTimeDurationPicker1 = new Teleopti.Ccc.Win.Common.Controls.FromToTimeDurationPicker();
            this.ribbonControHeader = new Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed();
            this.tableLayoutPanelButtons = new System.Windows.Forms.TableLayoutPanel();
            this.buttonOK = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonCancel = new Syncfusion.Windows.Forms.ButtonAdv();
            this.tabControlTopLevel = new System.Windows.Forms.TabControl();
            this.tabPageGenaral = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.checkBoxOnAvailableAgentsOnly = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvOvertimeType)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvActivity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvTag)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControHeader)).BeginInit();
            this.tableLayoutPanelButtons.SuspendLayout();
            this.tabControlTopLevel.SuspendLayout();
            this.tabPageGenaral.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(6, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(248, 25);
            this.label1.TabIndex = 5;
            this.label1.Text = "xxTagChangesWithColon";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(6, 52);
            this.label2.Name = "label2";
            this.label2.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.label2.Size = new System.Drawing.Size(248, 25);
            this.label2.TabIndex = 6;
            this.label2.Text = "xxSkillActivityColon";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Location = new System.Drawing.Point(6, 77);
            this.label4.Name = "label4";
            this.label4.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.label4.Size = new System.Drawing.Size(248, 20);
            this.label4.TabIndex = 10;
            this.label4.Text = "xxTypeOfOvertimeColon";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanelMain
            // 
            this.tableLayoutPanelMain.ColumnCount = 2;
            this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 166F));
            this.tableLayoutPanelMain.Controls.Add(this.checkBoxOnAvailableAgentsOnly, 0, 9);
            this.tableLayoutPanelMain.Controls.Add(this.checkBoxAllowBreakingNightlyRest, 0, 7);
            this.tableLayoutPanelMain.Controls.Add(this.checkBoxAllowBreakingWeeklyRest, 0, 8);
            this.tableLayoutPanelMain.Controls.Add(this.checkBoxAllowBreakingMaxTimePerWeek, 0, 6);
            this.tableLayoutPanelMain.Controls.Add(this.comboBoxAdvOvertimeType, 1, 3);
            this.tableLayoutPanelMain.Controls.Add(this.comboBoxAdvActivity, 1, 2);
            this.tableLayoutPanelMain.Controls.Add(this.comboBoxAdvTag, 1, 0);
            this.tableLayoutPanelMain.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanelMain.Controls.Add(this.label4, 0, 3);
            this.tableLayoutPanelMain.Controls.Add(this.label2, 0, 2);
            this.tableLayoutPanelMain.Controls.Add(this.label3, 0, 1);
            this.tableLayoutPanelMain.Controls.Add(this.label5, 0, 4);
            this.tableLayoutPanelMain.Controls.Add(this.fromToTimeDurationPicker1, 0, 5);
            this.tableLayoutPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelMain.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanelMain.Margin = new System.Windows.Forms.Padding(6);
            this.tableLayoutPanelMain.Name = "tableLayoutPanelMain";
            this.tableLayoutPanelMain.Padding = new System.Windows.Forms.Padding(3);
            this.tableLayoutPanelMain.RowCount = 11;
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanelMain.Size = new System.Drawing.Size(426, 448);
            this.tableLayoutPanelMain.TabIndex = 11;
            // 
            // checkBoxAllowBreakingNightlyRest
            // 
            this.checkBoxAllowBreakingNightlyRest.AutoSize = true;
            this.tableLayoutPanelMain.SetColumnSpan(this.checkBoxAllowBreakingNightlyRest, 2);
            this.checkBoxAllowBreakingNightlyRest.Dock = System.Windows.Forms.DockStyle.Top;
            this.checkBoxAllowBreakingNightlyRest.Location = new System.Drawing.Point(6, 196);
            this.checkBoxAllowBreakingNightlyRest.Name = "checkBoxAllowBreakingNightlyRest";
            this.checkBoxAllowBreakingNightlyRest.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.checkBoxAllowBreakingNightlyRest.Size = new System.Drawing.Size(414, 17);
            this.checkBoxAllowBreakingNightlyRest.TabIndex = 21;
            this.checkBoxAllowBreakingNightlyRest.Text = "xxAllowBreakContractNightlyRest";
            this.checkBoxAllowBreakingNightlyRest.UseVisualStyleBackColor = true;
            // 
            // checkBoxAllowBreakingWeeklyRest
            // 
            this.checkBoxAllowBreakingWeeklyRest.AutoSize = true;
            this.tableLayoutPanelMain.SetColumnSpan(this.checkBoxAllowBreakingWeeklyRest, 2);
            this.checkBoxAllowBreakingWeeklyRest.Dock = System.Windows.Forms.DockStyle.Top;
            this.checkBoxAllowBreakingWeeklyRest.Location = new System.Drawing.Point(6, 221);
            this.checkBoxAllowBreakingWeeklyRest.Name = "checkBoxAllowBreakingWeeklyRest";
            this.checkBoxAllowBreakingWeeklyRest.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.checkBoxAllowBreakingWeeklyRest.Size = new System.Drawing.Size(414, 17);
            this.checkBoxAllowBreakingWeeklyRest.TabIndex = 20;
            this.checkBoxAllowBreakingWeeklyRest.Text = "xxAllowBreakContractWeeklyRest";
            this.checkBoxAllowBreakingWeeklyRest.UseVisualStyleBackColor = true;
            // 
            // checkBoxAllowBreakingMaxTimePerWeek
            // 
            this.checkBoxAllowBreakingMaxTimePerWeek.AutoSize = true;
            this.tableLayoutPanelMain.SetColumnSpan(this.checkBoxAllowBreakingMaxTimePerWeek, 2);
            this.checkBoxAllowBreakingMaxTimePerWeek.Dock = System.Windows.Forms.DockStyle.Top;
            this.checkBoxAllowBreakingMaxTimePerWeek.Location = new System.Drawing.Point(6, 170);
            this.checkBoxAllowBreakingMaxTimePerWeek.Name = "checkBoxAllowBreakingMaxTimePerWeek";
            this.checkBoxAllowBreakingMaxTimePerWeek.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.checkBoxAllowBreakingMaxTimePerWeek.Size = new System.Drawing.Size(414, 17);
            this.checkBoxAllowBreakingMaxTimePerWeek.TabIndex = 19;
            this.checkBoxAllowBreakingMaxTimePerWeek.Text = "xxAllowBreakContractMaxWorkTimePerWeek";
            this.checkBoxAllowBreakingMaxTimePerWeek.UseVisualStyleBackColor = true;
            // 
            // comboBoxAdvOvertimeType
            // 
            this.comboBoxAdvOvertimeType.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(251)))));
            this.comboBoxAdvOvertimeType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxAdvOvertimeType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAdvOvertimeType.Location = new System.Drawing.Point(260, 80);
            this.comboBoxAdvOvertimeType.Name = "comboBoxAdvOvertimeType";
            this.comboBoxAdvOvertimeType.Size = new System.Drawing.Size(160, 21);
            this.comboBoxAdvOvertimeType.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this.comboBoxAdvOvertimeType.TabIndex = 15;
            // 
            // comboBoxAdvActivity
            // 
            this.comboBoxAdvActivity.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(251)))));
            this.comboBoxAdvActivity.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxAdvActivity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAdvActivity.Location = new System.Drawing.Point(260, 55);
            this.comboBoxAdvActivity.Name = "comboBoxAdvActivity";
            this.comboBoxAdvActivity.Size = new System.Drawing.Size(160, 21);
            this.comboBoxAdvActivity.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this.comboBoxAdvActivity.TabIndex = 14;
            // 
            // comboBoxAdvTag
            // 
            this.comboBoxAdvTag.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(251)))));
            this.comboBoxAdvTag.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxAdvTag.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAdvTag.Location = new System.Drawing.Point(260, 6);
            this.comboBoxAdvTag.Name = "comboBoxAdvTag";
            this.comboBoxAdvTag.Size = new System.Drawing.Size(160, 21);
            this.comboBoxAdvTag.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this.comboBoxAdvTag.TabIndex = 12;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.tableLayoutPanelMain.SetColumnSpan(this.label3, 2);
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(6, 28);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(414, 24);
            this.label3.TabIndex = 16;
            this.label3.Text = "xxAssignmentOptions";
            this.label3.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.tableLayoutPanelMain.SetColumnSpan(this.label5, 2);
            this.label5.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.label5.Location = new System.Drawing.Point(6, 109);
            this.label5.Name = "label5";
            this.label5.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.label5.Size = new System.Drawing.Size(414, 13);
            this.label5.TabIndex = 17;
            this.label5.Text = "xxOvertimeDurationColon";
            // 
            // fromToTimeDurationPicker1
            // 
            this.tableLayoutPanelMain.SetColumnSpan(this.fromToTimeDurationPicker1, 2);
            this.fromToTimeDurationPicker1.Location = new System.Drawing.Point(12, 131);
            this.fromToTimeDurationPicker1.Margin = new System.Windows.Forms.Padding(9);
            this.fromToTimeDurationPicker1.MinMaxEndTime = ((Teleopti.Interfaces.Domain.MinMax<System.TimeSpan>)(resources.GetObject("fromToTimeDurationPicker1.MinMaxEndTime")));
            this.fromToTimeDurationPicker1.MinMaxStartTime = ((Teleopti.Interfaces.Domain.MinMax<System.TimeSpan>)(resources.GetObject("fromToTimeDurationPicker1.MinMaxStartTime")));
            this.fromToTimeDurationPicker1.Name = "fromToTimeDurationPicker1";
            this.fromToTimeDurationPicker1.Size = new System.Drawing.Size(248, 27);
            this.fromToTimeDurationPicker1.TabIndex = 18;
            this.fromToTimeDurationPicker1.WholeDayText = "xxNextDay";
            // 
            // ribbonControHeader
            // 
            this.ribbonControHeader.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ribbonControHeader.Location = new System.Drawing.Point(1, 0);
            this.ribbonControHeader.MaximizeToolTip = "Maximize Ribbon";
            this.ribbonControHeader.MenuButtonText = "";
            this.ribbonControHeader.MenuButtonVisible = false;
            this.ribbonControHeader.MinimizeToolTip = "Minimize Ribbon";
            this.ribbonControHeader.Name = "ribbonControHeader";
            // 
            // ribbonControHeader.OfficeMenu
            // 
            this.ribbonControHeader.OfficeMenu.Name = "OfficeMenu";
            this.ribbonControHeader.OfficeMenu.Size = new System.Drawing.Size(12, 65);
            this.ribbonControHeader.QuickPanelVisible = false;
            this.ribbonControHeader.SelectedTab = null;
            this.ribbonControHeader.ShowLauncher = false;
            this.ribbonControHeader.Size = new System.Drawing.Size(456, 33);
            this.ribbonControHeader.SystemText.QuickAccessDialogDropDownName = "StartMenu";
            this.ribbonControHeader.TabIndex = 12;
            this.ribbonControHeader.Text = "ribbonControlAdv1";
            // 
            // tableLayoutPanelButtons
            // 
            this.tableLayoutPanelButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanelButtons.ColumnCount = 2;
            this.tableLayoutPanelButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelButtons.Controls.Add(this.buttonOK, 0, 0);
            this.tableLayoutPanelButtons.Controls.Add(this.buttonCancel, 1, 0);
            this.tableLayoutPanelButtons.Location = new System.Drawing.Point(268, 496);
            this.tableLayoutPanelButtons.Name = "tableLayoutPanelButtons";
            this.tableLayoutPanelButtons.RowCount = 1;
            this.tableLayoutPanelButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelButtons.Size = new System.Drawing.Size(175, 31);
            this.tableLayoutPanelButtons.TabIndex = 13;
            // 
            // buttonOK
            // 
            this.buttonOK.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(3, 3);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 10;
            this.buttonOK.Text = "xxOk";
            this.buttonOK.UseVisualStyle = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(90, 3);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 10;
            this.buttonCancel.Text = "xxCancel";
            this.buttonCancel.UseVisualStyle = true;
            // 
            // tabControlTopLevel
            // 
            this.tabControlTopLevel.Controls.Add(this.tabPageGenaral);
            this.tabControlTopLevel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlTopLevel.ItemSize = new System.Drawing.Size(59, 22);
            this.tabControlTopLevel.Location = new System.Drawing.Point(3, 3);
            this.tabControlTopLevel.Name = "tabControlTopLevel";
            this.tabControlTopLevel.SelectedIndex = 0;
            this.tabControlTopLevel.Size = new System.Drawing.Size(440, 484);
            this.tabControlTopLevel.TabIndex = 14;
            // 
            // tabPageGenaral
            // 
            this.tabPageGenaral.Controls.Add(this.tableLayoutPanelMain);
            this.tabPageGenaral.Location = new System.Drawing.Point(4, 26);
            this.tabPageGenaral.Name = "tabPageGenaral";
            this.tabPageGenaral.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageGenaral.Size = new System.Drawing.Size(432, 454);
            this.tabPageGenaral.TabIndex = 0;
            this.tabPageGenaral.Text = "xxGeneral";
            this.tabPageGenaral.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.tabControlTopLevel, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.tableLayoutPanelButtons, 0, 1);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(6, 34);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(446, 530);
            this.tableLayoutPanel3.TabIndex = 15;
            // 
            // checkBoxOnAvailableAgentsOnly
            // 
            this.checkBoxOnAvailableAgentsOnly.AutoSize = true;
            this.tableLayoutPanelMain.SetColumnSpan(this.checkBoxOnAvailableAgentsOnly, 2);
            this.checkBoxOnAvailableAgentsOnly.Dock = System.Windows.Forms.DockStyle.Top;
            this.checkBoxOnAvailableAgentsOnly.Location = new System.Drawing.Point(6, 246);
            this.checkBoxOnAvailableAgentsOnly.Name = "checkBoxOnAvailableAgentsOnly";
            this.checkBoxOnAvailableAgentsOnly.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.checkBoxOnAvailableAgentsOnly.Size = new System.Drawing.Size(414, 17);
            this.checkBoxOnAvailableAgentsOnly.TabIndex = 22;
            this.checkBoxOnAvailableAgentsOnly.Text = "xxOnAvailableAgentsOnly";
            this.checkBoxOnAvailableAgentsOnly.UseVisualStyleBackColor = true;
            // 
            // OvertimePreferencesDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(458, 570);
            this.Controls.Add(this.tableLayoutPanel3);
            this.Controls.Add(this.ribbonControHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OvertimePreferencesDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "xxScheduleOvertimeOptions";
            this.Load += new System.EventHandler(this.OvertimePreferencesDialog_Load);
            this.tableLayoutPanelMain.ResumeLayout(false);
            this.tableLayoutPanelMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvOvertimeType)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvActivity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvTag)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControHeader)).EndInit();
            this.tableLayoutPanelButtons.ResumeLayout(false);
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
		private Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed ribbonControHeader;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelButtons;
		private Syncfusion.Windows.Forms.ButtonAdv buttonOK;
		private Syncfusion.Windows.Forms.ButtonAdv buttonCancel;
		private System.Windows.Forms.TabControl tabControlTopLevel;
		private System.Windows.Forms.TabPage tabPageGenaral;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
		private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvOvertimeType;
		private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvActivity;
		private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvTag;
		private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label5;
        private Common.Controls.FromToTimeDurationPicker fromToTimeDurationPicker1;
        private System.Windows.Forms.CheckBox checkBoxAllowBreakingNightlyRest;
        private System.Windows.Forms.CheckBox checkBoxAllowBreakingWeeklyRest;
        private System.Windows.Forms.CheckBox checkBoxAllowBreakingMaxTimePerWeek;
        private System.Windows.Forms.CheckBox checkBoxOnAvailableAgentsOnly;
	}
}