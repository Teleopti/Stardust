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
			this.checkBox1 = new System.Windows.Forms.CheckBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
			this.comboBoxAdv2 = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.comboBoxAdv1 = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.comboBoxAdvTag = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.label3 = new System.Windows.Forms.Label();
			this.ribbonControHeader = new Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed();
			this.tableLayoutPanelButtons = new System.Windows.Forms.TableLayoutPanel();
			this.buttonOK = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.tabControlTopLevel = new System.Windows.Forms.TabControl();
			this.tabPageGenaral = new System.Windows.Forms.TabPage();
			this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
			this.label5 = new System.Windows.Forms.Label();
			this.fromToTimePicker1 = new Teleopti.Ccc.Win.Common.Controls.FromToTimePicker();
			this.tableLayoutPanelMain.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdv2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdv1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvTag)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ribbonControHeader)).BeginInit();
			this.tableLayoutPanelButtons.SuspendLayout();
			this.tabControlTopLevel.SuspendLayout();
			this.tabPageGenaral.SuspendLayout();
			this.tableLayoutPanel3.SuspendLayout();
			this.SuspendLayout();
			// 
			// checkBox1
			// 
			this.checkBox1.AutoSize = true;
			this.checkBox1.Checked = true;
			this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
			this.tableLayoutPanelMain.SetColumnSpan(this.checkBox1, 2);
			this.checkBox1.Dock = System.Windows.Forms.DockStyle.Top;
			this.checkBox1.Location = new System.Drawing.Point(6, 71);
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
			this.checkBox1.Size = new System.Drawing.Size(414, 17);
			this.checkBox1.TabIndex = 2;
			this.checkBox1.Text = "Extend the end of existing shifts";
			this.checkBox1.UseVisualStyleBackColor = true;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label1.Location = new System.Drawing.Point(6, 3);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(260, 25);
			this.label1.TabIndex = 5;
			this.label1.Text = "xxTagChangesWithColon";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label2.Location = new System.Drawing.Point(6, 108);
			this.label2.Name = "label2";
			this.label2.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
			this.label2.Size = new System.Drawing.Size(260, 25);
			this.label2.TabIndex = 6;
			this.label2.Text = "xxSkillActivity";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label4.Location = new System.Drawing.Point(6, 133);
			this.label4.Name = "label4";
			this.label4.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
			this.label4.Size = new System.Drawing.Size(260, 25);
			this.label4.TabIndex = 10;
			this.label4.Text = "xxTypeOfOvertime";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// tableLayoutPanelMain
			// 
			this.tableLayoutPanelMain.ColumnCount = 2;
			this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 154F));
			this.tableLayoutPanelMain.Controls.Add(this.comboBoxAdv2, 1, 4);
			this.tableLayoutPanelMain.Controls.Add(this.comboBoxAdv1, 1, 3);
			this.tableLayoutPanelMain.Controls.Add(this.comboBoxAdvTag, 1, 0);
			this.tableLayoutPanelMain.Controls.Add(this.label1, 0, 0);
			this.tableLayoutPanelMain.Controls.Add(this.label4, 0, 4);
			this.tableLayoutPanelMain.Controls.Add(this.label2, 0, 3);
			this.tableLayoutPanelMain.Controls.Add(this.label3, 0, 1);
			this.tableLayoutPanelMain.Controls.Add(this.checkBox1, 0, 2);
			this.tableLayoutPanelMain.Controls.Add(this.label5, 0, 5);
			this.tableLayoutPanelMain.Controls.Add(this.fromToTimePicker1, 0, 6);
			this.tableLayoutPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelMain.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanelMain.Margin = new System.Windows.Forms.Padding(6);
			this.tableLayoutPanelMain.Name = "tableLayoutPanelMain";
			this.tableLayoutPanelMain.Padding = new System.Windows.Forms.Padding(3);
			this.tableLayoutPanelMain.RowCount = 7;
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelMain.Size = new System.Drawing.Size(426, 448);
			this.tableLayoutPanelMain.TabIndex = 11;
			// 
			// comboBoxAdv2
			// 
			this.comboBoxAdv2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(251)))));
			this.comboBoxAdv2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.comboBoxAdv2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAdv2.Location = new System.Drawing.Point(272, 136);
			this.comboBoxAdv2.Name = "comboBoxAdv2";
			this.comboBoxAdv2.Size = new System.Drawing.Size(148, 21);
			this.comboBoxAdv2.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
			this.comboBoxAdv2.TabIndex = 15;
			// 
			// comboBoxAdv1
			// 
			this.comboBoxAdv1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(251)))));
			this.comboBoxAdv1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.comboBoxAdv1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAdv1.Location = new System.Drawing.Point(272, 111);
			this.comboBoxAdv1.Name = "comboBoxAdv1";
			this.comboBoxAdv1.Size = new System.Drawing.Size(148, 21);
			this.comboBoxAdv1.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
			this.comboBoxAdv1.TabIndex = 14;
			// 
			// comboBoxAdvTag
			// 
			this.comboBoxAdvTag.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(251)))));
			this.comboBoxAdvTag.Dock = System.Windows.Forms.DockStyle.Fill;
			this.comboBoxAdvTag.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAdvTag.Location = new System.Drawing.Point(272, 6);
			this.comboBoxAdvTag.Name = "comboBoxAdvTag";
			this.comboBoxAdvTag.Size = new System.Drawing.Size(148, 21);
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
			this.label3.Size = new System.Drawing.Size(414, 40);
			this.label3.TabIndex = 16;
			this.label3.Text = "xxAssignmentOptions";
			this.label3.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
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
			// label5
			// 
			this.label5.AutoSize = true;
			this.tableLayoutPanelMain.SetColumnSpan(this.label5, 2);
			this.label5.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.label5.Location = new System.Drawing.Point(6, 185);
			this.label5.Name = "label5";
			this.label5.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
			this.label5.Size = new System.Drawing.Size(414, 13);
			this.label5.TabIndex = 17;
			this.label5.Text = "xxOvertimeDuration";
			// 
			// fromToTimePicker1
			// 
			this.tableLayoutPanelMain.SetColumnSpan(this.fromToTimePicker1, 2);
			this.fromToTimePicker1.Dock = System.Windows.Forms.DockStyle.Top;
			this.fromToTimePicker1.Location = new System.Drawing.Point(6, 201);
			this.fromToTimePicker1.MinMaxEndTime = ((Teleopti.Interfaces.Domain.MinMax<System.TimeSpan>)(resources.GetObject("fromToTimePicker1.MinMaxEndTime")));
			this.fromToTimePicker1.MinMaxStartTime = ((Teleopti.Interfaces.Domain.MinMax<System.TimeSpan>)(resources.GetObject("fromToTimePicker1.MinMaxStartTime")));
			this.fromToTimePicker1.Name = "fromToTimePicker1";
			this.fromToTimePicker1.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
			this.fromToTimePicker1.Size = new System.Drawing.Size(414, 27);
			this.fromToTimePicker1.TabIndex = 18;
			this.fromToTimePicker1.WholeDayText = "xxNextDay";
			// 
			// OvertimePreferencesDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(458, 570);
			this.Controls.Add(this.tableLayoutPanel3);
			this.Controls.Add(this.ribbonControHeader);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "OvertimePreferencesDialog";
			this.Text = "xxScheduleOvertimeOptions";
			this.tableLayoutPanelMain.ResumeLayout(false);
			this.tableLayoutPanelMain.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdv2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdv1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvTag)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ribbonControHeader)).EndInit();
			this.tableLayoutPanelButtons.ResumeLayout(false);
			this.tabControlTopLevel.ResumeLayout(false);
			this.tabPageGenaral.ResumeLayout(false);
			this.tableLayoutPanel3.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.CheckBox checkBox1;
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
		private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdv2;
		private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdv1;
		private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvTag;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label5;
		private Common.Controls.FromToTimePicker fromToTimePicker1;
	}
}