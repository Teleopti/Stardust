using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Optimization
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
			this.label6 = new System.Windows.Forms.Label();
			this.checkBox1 = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.checkBox2 = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
			this.comboBoxAdvActivity = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.checkBoxKeepShiftCategories = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.checkBoxKeepStartTimes = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.checkBoxKeepEndTimes = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.checkBoxKeepActivityLength = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.label2 = new System.Windows.Forms.Label();
			this.twoListSelectorActivities = new Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.TwoListSelector();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.fromToTimePicker1 = new Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.FromToTimePicker();
			this.checkBoxBetween = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			((System.ComponentModel.ISupportInitialize)(this.checkBox1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBox2)).BeginInit();
			this.tableLayoutPanel2.SuspendLayout();
			this.tableLayoutPanel5.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvActivity)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxKeepShiftCategories)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxKeepStartTimes)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxKeepEndTimes)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxKeepActivityLength)).BeginInit();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxBetween)).BeginInit();
			this.SuspendLayout();
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
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel2.ColumnCount = 1;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel5, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.twoListSelectorActivities, 0, 2);
			this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel1, 0, 1);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 3;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 185F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(456, 473);
			this.tableLayoutPanel2.TabIndex = 1;
			// 
			// tableLayoutPanel5
			// 
			this.tableLayoutPanel5.ColumnCount = 3;
			this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 54F));
			this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 131F));
			this.tableLayoutPanel5.Controls.Add(this.comboBoxAdvActivity, 1, 4);
			this.tableLayoutPanel5.Controls.Add(this.checkBoxKeepShiftCategories, 0, 1);
			this.tableLayoutPanel5.Controls.Add(this.checkBoxKeepStartTimes, 0, 2);
			this.tableLayoutPanel5.Controls.Add(this.checkBoxKeepEndTimes, 0, 3);
			this.tableLayoutPanel5.Controls.Add(this.checkBoxKeepActivityLength, 0, 4);
			this.tableLayoutPanel5.Controls.Add(this.label2, 0, 0);
			this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel5.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel5.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel5.Name = "tableLayoutPanel5";
			this.tableLayoutPanel5.RowCount = 6;
			this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel5.Size = new System.Drawing.Size(456, 185);
			this.tableLayoutPanel5.TabIndex = 0;
			// 
			// comboBoxAdvActivity
			// 
			this.comboBoxAdvActivity.BackColor = System.Drawing.Color.White;
			this.comboBoxAdvActivity.BeforeTouchSize = new System.Drawing.Size(179, 23);
			this.tableLayoutPanel5.SetColumnSpan(this.comboBoxAdvActivity, 2);
			this.comboBoxAdvActivity.Dock = System.Windows.Forms.DockStyle.Fill;
			this.comboBoxAdvActivity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAdvActivity.Location = new System.Drawing.Point(274, 123);
			this.comboBoxAdvActivity.Name = "comboBoxAdvActivity";
			this.comboBoxAdvActivity.Size = new System.Drawing.Size(179, 23);
			this.comboBoxAdvActivity.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.comboBoxAdvActivity.TabIndex = 26;
			// 
			// checkBoxKeepShiftCategories
			// 
			this.checkBoxKeepShiftCategories.BeforeTouchSize = new System.Drawing.Size(258, 24);
			this.checkBoxKeepShiftCategories.Dock = System.Windows.Forms.DockStyle.Fill;
			this.checkBoxKeepShiftCategories.DrawFocusRectangle = false;
			this.checkBoxKeepShiftCategories.Location = new System.Drawing.Point(10, 33);
			this.checkBoxKeepShiftCategories.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
			this.checkBoxKeepShiftCategories.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxKeepShiftCategories.Name = "checkBoxKeepShiftCategories";
			this.checkBoxKeepShiftCategories.Size = new System.Drawing.Size(258, 24);
			this.checkBoxKeepShiftCategories.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Metro;
			this.checkBoxKeepShiftCategories.TabIndex = 22;
			this.checkBoxKeepShiftCategories.Text = "xxShiftCategories";
			this.checkBoxKeepShiftCategories.ThemesEnabled = false;
			// 
			// checkBoxKeepStartTimes
			// 
			this.checkBoxKeepStartTimes.BeforeTouchSize = new System.Drawing.Size(258, 24);
			this.checkBoxKeepStartTimes.Dock = System.Windows.Forms.DockStyle.Fill;
			this.checkBoxKeepStartTimes.DrawFocusRectangle = false;
			this.checkBoxKeepStartTimes.Location = new System.Drawing.Point(10, 63);
			this.checkBoxKeepStartTimes.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
			this.checkBoxKeepStartTimes.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxKeepStartTimes.Name = "checkBoxKeepStartTimes";
			this.checkBoxKeepStartTimes.Size = new System.Drawing.Size(258, 24);
			this.checkBoxKeepStartTimes.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Metro;
			this.checkBoxKeepStartTimes.TabIndex = 23;
			this.checkBoxKeepStartTimes.Text = "xxStartTime";
			this.checkBoxKeepStartTimes.ThemesEnabled = false;
			// 
			// checkBoxKeepEndTimes
			// 
			this.checkBoxKeepEndTimes.BeforeTouchSize = new System.Drawing.Size(258, 24);
			this.checkBoxKeepEndTimes.Dock = System.Windows.Forms.DockStyle.Fill;
			this.checkBoxKeepEndTimes.DrawFocusRectangle = false;
			this.checkBoxKeepEndTimes.Location = new System.Drawing.Point(10, 93);
			this.checkBoxKeepEndTimes.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
			this.checkBoxKeepEndTimes.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxKeepEndTimes.Name = "checkBoxKeepEndTimes";
			this.checkBoxKeepEndTimes.Size = new System.Drawing.Size(258, 24);
			this.checkBoxKeepEndTimes.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Metro;
			this.checkBoxKeepEndTimes.TabIndex = 24;
			this.checkBoxKeepEndTimes.Text = "xxEndTime";
			this.checkBoxKeepEndTimes.ThemesEnabled = false;
			// 
			// checkBoxKeepActivityLength
			// 
			this.checkBoxKeepActivityLength.BeforeTouchSize = new System.Drawing.Size(258, 24);
			this.checkBoxKeepActivityLength.Dock = System.Windows.Forms.DockStyle.Fill;
			this.checkBoxKeepActivityLength.DrawFocusRectangle = false;
			this.checkBoxKeepActivityLength.Location = new System.Drawing.Point(10, 123);
			this.checkBoxKeepActivityLength.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
			this.checkBoxKeepActivityLength.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxKeepActivityLength.Name = "checkBoxKeepActivityLength";
			this.checkBoxKeepActivityLength.Size = new System.Drawing.Size(258, 24);
			this.checkBoxKeepActivityLength.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Metro;
			this.checkBoxKeepActivityLength.TabIndex = 25;
			this.checkBoxKeepActivityLength.Text = "xxLengthOfActivity";
			this.checkBoxKeepActivityLength.ThemesEnabled = false;
			this.checkBoxKeepActivityLength.CheckedChanged += new System.EventHandler(this.checkBoxKeepActivityLength_CheckedChanged);
			// 
			// label2
			// 
			this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(3, 7);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(45, 15);
			this.label2.TabIndex = 21;
			this.label2.Text = "xxKeep";
			// 
			// twoListSelectorActivities
			// 
			this.twoListSelectorActivities.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
			this.twoListSelectorActivities.Dock = System.Windows.Forms.DockStyle.Fill;
			this.twoListSelectorActivities.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.twoListSelectorActivities.Location = new System.Drawing.Point(3, 248);
			this.twoListSelectorActivities.Name = "twoListSelectorActivities";
			this.twoListSelectorActivities.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.twoListSelectorActivities.Size = new System.Drawing.Size(450, 222);
			this.twoListSelectorActivities.TabIndex = 26;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 27.00893F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 72.99107F));
			this.tableLayoutPanel1.Controls.Add(this.fromToTimePicker1, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.checkBoxBetween, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 185);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(456, 60);
			this.tableLayoutPanel1.TabIndex = 27;
			// 
			// fromToTimePicker1
			// 
			this.fromToTimePicker1.Location = new System.Drawing.Point(126, 10);
			this.fromToTimePicker1.Margin = new System.Windows.Forms.Padding(3, 10, 3, 3);
			this.fromToTimePicker1.Name = "fromToTimePicker1";
			this.fromToTimePicker1.Size = new System.Drawing.Size(310, 31);
			this.fromToTimePicker1.TabIndex = 10;
			this.fromToTimePicker1.WholeDayCheckboxVisible = true;
			this.fromToTimePicker1.WholeDayText = "xxNextDay";
			// 
			// checkBoxBetween
			// 
			this.checkBoxBetween.BeforeTouchSize = new System.Drawing.Size(116, 28);
			this.checkBoxBetween.DrawFocusRectangle = false;
			this.checkBoxBetween.Location = new System.Drawing.Point(3, 10);
			this.checkBoxBetween.Margin = new System.Windows.Forms.Padding(3, 10, 3, 3);
			this.checkBoxBetween.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxBetween.Name = "checkBoxBetween";
			this.checkBoxBetween.Size = new System.Drawing.Size(116, 28);
			this.checkBoxBetween.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Metro;
			this.checkBoxBetween.TabIndex = 5;
			this.checkBoxBetween.Text = "xxAlterBetween";
			this.checkBoxBetween.ThemesEnabled = false;
			// 
			// ShiftsPreferencesPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.Controls.Add(this.tableLayoutPanel2);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "ShiftsPreferencesPanel";
			this.Size = new System.Drawing.Size(456, 473);
			((System.ComponentModel.ISupportInitialize)(this.checkBox1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBox2)).EndInit();
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel5.ResumeLayout(false);
			this.tableLayoutPanel5.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvActivity)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxKeepShiftCategories)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxKeepStartTimes)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxKeepEndTimes)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxKeepActivityLength)).EndInit();
			this.tableLayoutPanel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.checkBoxBetween)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBox1;
		private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBox2;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
		private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxKeepShiftCategories;
		private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxKeepStartTimes;
		private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxKeepEndTimes;
		private FromToTimePicker fromToTimePicker1;
		private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxBetween;
		private TwoListSelector twoListSelectorActivities;
		private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxKeepActivityLength;
		private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvActivity;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
	}
}
