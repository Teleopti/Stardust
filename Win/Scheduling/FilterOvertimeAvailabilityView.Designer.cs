﻿using System.Drawing;

namespace Teleopti.Ccc.Win.Scheduling
{
	partial class FilterOvertimeAvailabilityView
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
                if ((components != null))
                    components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxOk"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxMain"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxCancel"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "toolTip"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ReportPersonsSelectionView"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.Control.set_Text(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Syncfusion.Windows.Forms.ToolTipAdv.set_Text(System.String)")]
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FilterOvertimeAvailabilityView));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonOk = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonCancel = new Syncfusion.Windows.Forms.ButtonAdv();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.labelDate = new System.Windows.Forms.Label();
            this.datePicker = new Syncfusion.Windows.Forms.Tools.DateTimePickerAdv();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.checkBoxAdvNextDay = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
            this.outlookTimePickerTo = new Teleopti.Ccc.Win.Common.Controls.OutlookTimePicker();
            this.labelTo = new System.Windows.Forms.Label();
            this.outlookTimePickerFrom = new Teleopti.Ccc.Win.Common.Controls.OutlookTimePicker();
            this.labelFrom = new System.Windows.Forms.Label();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.datePicker)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.datePicker.Calendar)).BeginInit();
            this.tableLayoutPanel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvNextDay)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerTo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerFrom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 86.20689F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 13.7931F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(327, 262);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.BackColor = System.Drawing.Color.White;
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 62.71186F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 37.28814F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 102F));
            this.tableLayoutPanel2.Controls.Add(this.buttonOk, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.buttonCancel, 2, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 225);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(327, 37);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.buttonOk.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonOk.Location = new System.Drawing.Point(144, 7);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 4;
            this.buttonOk.Text = "xxOk";
            this.buttonOk.UseVisualStyle = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.buttonCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(227, 7);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "xxCancel";
            this.buttonCancel.UseVisualStyle = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tableLayoutPanel3);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(327, 225);
            this.panel1.TabIndex = 1;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.tableLayoutPanel5, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.tableLayoutPanel4, 0, 1);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 3;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 27.33485F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 72.66515F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(327, 225);
            this.tableLayoutPanel3.TabIndex = 0;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 2;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 21.49533F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 78.50467F));
            this.tableLayoutPanel5.Controls.Add(this.labelDate, 0, 0);
            this.tableLayoutPanel5.Controls.Add(this.datePicker, 1, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 1;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(321, 50);
            this.tableLayoutPanel5.TabIndex = 1;
            // 
            // labelDate
            // 
            this.labelDate.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelDate.AutoSize = true;
            this.labelDate.Location = new System.Drawing.Point(3, 12);
            this.labelDate.Name = "labelDate";
            this.labelDate.Size = new System.Drawing.Size(61, 26);
            this.labelDate.TabIndex = 9;
            this.labelDate.Text = "xxDateColon";
            // 
            // datePicker
            // 
            this.datePicker.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.datePicker.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.White);
            this.datePicker.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
            this.datePicker.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(193)))), ((int)(((byte)(222)))));
            this.datePicker.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            // 
            // 
            // 
            this.datePicker.Calendar.AllowMultipleSelection = false;
            this.datePicker.Calendar.Culture = new System.Globalization.CultureInfo("sv-SE");
            this.datePicker.Calendar.DaysFont = new System.Drawing.Font("Verdana", 8F);
            this.datePicker.Calendar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.datePicker.Calendar.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.datePicker.Calendar.ForeColor = System.Drawing.SystemColors.ControlText;
            this.datePicker.Calendar.GridLines = Syncfusion.Windows.Forms.Grid.GridBorderStyle.None;
            this.datePicker.Calendar.HeaderEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.datePicker.Calendar.HeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.datePicker.Calendar.HeaderHeight = 20;
            this.datePicker.Calendar.HeaderStartColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252)))));
            this.datePicker.Calendar.HeadForeColor = System.Drawing.SystemColors.ControlText;
            this.datePicker.Calendar.HeadGradient = true;
            this.datePicker.Calendar.Iso8601CalenderFormat = false;
            this.datePicker.Calendar.Location = new System.Drawing.Point(0, 0);
            this.datePicker.Calendar.MinValue = new System.DateTime(1990, 12, 31, 0, 0, 0, 0);
            this.datePicker.Calendar.Name = "monthCalendar";
            this.datePicker.Calendar.ScrollButtonSize = new System.Drawing.Size(24, 24);
            this.datePicker.Calendar.SelectedDates = new System.DateTime[0];
            this.datePicker.Calendar.Size = new System.Drawing.Size(138, 174);
            this.datePicker.Calendar.SizeToFit = true;
            this.datePicker.Calendar.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this.datePicker.Calendar.TabIndex = 0;
            this.datePicker.Calendar.ThemedEnabledGrid = true;
            this.datePicker.Calendar.WeekFont = new System.Drawing.Font("Verdana", 8F);
            this.datePicker.Calendar.WeekInterior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.PeachPuff, System.Drawing.Color.AntiqueWhite);
            // 
            // 
            // 
            this.datePicker.Calendar.NoneButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.datePicker.Calendar.NoneButton.Location = new System.Drawing.Point(66, 0);
            this.datePicker.Calendar.NoneButton.Size = new System.Drawing.Size(72, 20);
            this.datePicker.Calendar.NoneButton.Text = "None";
            this.datePicker.Calendar.NoneButton.UseVisualStyle = true;
            // 
            // 
            // 
            this.datePicker.Calendar.TodayButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.datePicker.Calendar.TodayButton.Location = new System.Drawing.Point(0, 0);
            this.datePicker.Calendar.TodayButton.Size = new System.Drawing.Size(66, 20);
            this.datePicker.Calendar.TodayButton.Text = "Today";
            this.datePicker.Calendar.TodayButton.UseVisualStyle = true;
            this.datePicker.CalendarForeColor = System.Drawing.SystemColors.ControlText;
            this.datePicker.CalendarTitleBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252)))));
            this.datePicker.CalendarTitleForeColor = System.Drawing.SystemColors.ControlText;
            this.datePicker.DropDownImage = null;
            this.datePicker.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.datePicker.Location = new System.Drawing.Point(72, 15);
            this.datePicker.MinValue = new System.DateTime(1990, 12, 31, 23, 59, 0, 0);
            this.datePicker.Name = "datePicker";
            this.datePicker.NoneButtonVisible = false;
            this.datePicker.ShowCheckBox = false;
            this.datePicker.Size = new System.Drawing.Size(246, 20);
            this.datePicker.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this.datePicker.TabIndex = 8;
            this.datePicker.ThemedChildControls = true;
            this.datePicker.ThemesEnabled = true;
            this.datePicker.Value = new System.DateTime(2012, 1, 19, 15, 2, 3, 865);
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 4;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 68F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 96F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 154F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel4.Controls.Add(this.checkBoxAdvNextDay, 2, 1);
            this.tableLayoutPanel4.Controls.Add(this.outlookTimePickerTo, 1, 1);
            this.tableLayoutPanel4.Controls.Add(this.labelTo, 0, 1);
            this.tableLayoutPanel4.Controls.Add(this.outlookTimePickerFrom, 1, 0);
            this.tableLayoutPanel4.Controls.Add(this.labelFrom, 0, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 59);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 3;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 14F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(321, 142);
            this.tableLayoutPanel4.TabIndex = 3;
            // 
            // checkBoxAdvNextDay
            // 
            this.checkBoxAdvNextDay.Location = new System.Drawing.Point(167, 38);
            this.checkBoxAdvNextDay.Name = "checkBoxAdvNextDay";
            this.checkBoxAdvNextDay.Size = new System.Drawing.Size(108, 21);
            this.checkBoxAdvNextDay.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Office2007;
            this.checkBoxAdvNextDay.TabIndex = 3;
            this.checkBoxAdvNextDay.Text = "xxNextDay";
            this.checkBoxAdvNextDay.ThemesEnabled = true;
            // 
            // outlookTimePickerTo
            // 
            this.outlookTimePickerTo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(251)))));
            this.outlookTimePickerTo.DefaultResolution = 0;
            this.outlookTimePickerTo.EnableNull = true;
            this.outlookTimePickerTo.FormatFromCulture = true;
            this.outlookTimePickerTo.Location = new System.Drawing.Point(71, 38);
            this.outlookTimePickerTo.MaxTime = System.TimeSpan.Parse("23:59:00");
            this.outlookTimePickerTo.Name = "outlookTimePickerTo";
            this.outlookTimePickerTo.Size = new System.Drawing.Size(74, 21);
            this.outlookTimePickerTo.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this.outlookTimePickerTo.TabIndex = 2;
            this.outlookTimePickerTo.Text = "00:00";
            this.outlookTimePickerTo.TextChanged += new System.EventHandler(this.outlookTimePickerTo_TextChanged);
            // 
            // labelTo
            // 
            this.labelTo.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelTo.AutoSize = true;
            this.labelTo.Location = new System.Drawing.Point(3, 46);
            this.labelTo.Name = "labelTo";
            this.labelTo.Size = new System.Drawing.Size(57, 13);
            this.labelTo.TabIndex = 3;
            this.labelTo.Text = "xxToColon";
            // 
            // outlookTimePickerFrom
            // 
            this.outlookTimePickerFrom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(251)))));
            this.outlookTimePickerFrom.DefaultResolution = 0;
            this.outlookTimePickerFrom.EnableNull = true;
            this.outlookTimePickerFrom.FormatFromCulture = true;
            this.outlookTimePickerFrom.Location = new System.Drawing.Point(71, 3);
            this.outlookTimePickerFrom.MaxTime = System.TimeSpan.Parse("23:59:00");
            this.outlookTimePickerFrom.Name = "outlookTimePickerFrom";
            this.outlookTimePickerFrom.Size = new System.Drawing.Size(74, 21);
            this.outlookTimePickerFrom.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this.outlookTimePickerFrom.TabIndex = 1;
            this.outlookTimePickerFrom.Text = "00:00";
            this.outlookTimePickerFrom.TextChanged += new System.EventHandler(this.outlookTimePickerFrom_TextChanged);
            // 
            // labelFrom
            // 
            this.labelFrom.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelFrom.AutoSize = true;
            this.labelFrom.Location = new System.Drawing.Point(3, 4);
            this.labelFrom.Name = "labelFrom";
            this.labelFrom.Size = new System.Drawing.Size(61, 26);
            this.labelFrom.TabIndex = 2;
            this.labelFrom.Text = "xxFromColon";
            // 
            // errorProvider1
            // 
            this.errorProvider1.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.errorProvider1.ContainerControl = this;
            // 
            // FilterOvertimeAvailabilityView
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(327, 262);
            this.ControlBox = false;
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.HelpButton = false;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FilterOvertimeAvailabilityView";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "xxFilterOvertimeAvailability";
            this.Load += new System.EventHandler(this.FilterOvertimeAvailabilityView_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.datePicker.Calendar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.datePicker)).EndInit();
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvNextDay)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerTo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerFrom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private Syncfusion.Windows.Forms.ButtonAdv buttonOk;
        private Syncfusion.Windows.Forms.ButtonAdv buttonCancel;
        private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
		private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdvNextDay;
		private Common.Controls.OutlookTimePicker outlookTimePickerTo;
		private System.Windows.Forms.Label labelTo;
		private Common.Controls.OutlookTimePicker outlookTimePickerFrom;
		private System.Windows.Forms.Label labelFrom;
		private System.Windows.Forms.ErrorProvider errorProvider1;
		private Syncfusion.Windows.Forms.Tools.DateTimePickerAdv datePicker;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.Label labelDate;
    }
}