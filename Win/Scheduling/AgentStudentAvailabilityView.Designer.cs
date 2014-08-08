namespace Teleopti.Ccc.Win.Scheduling
{
	partial class AgentStudentAvailabilityView
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxNextDay"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxStudentAvailability"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxCancel"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxOk"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxFromColon"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxToColon"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.TimeSpan.Parse(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxNextDayColon"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxAgentStudentAvailabilityView"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.Control.set_Text(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1302:DoNotHardcodeLocaleSpecificStrings", MessageId = "Start menu"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Syncfusion.Windows.Forms.Tools.RibbonControlAdv.set_MinimizeToolTip(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Syncfusion.Windows.Forms.Tools.RibbonControlAdv.set_MaximizeToolTip(System.String)")]
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.checkBoxAdvNextDay = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.outlookTimePickerTo = new Teleopti.Ccc.Win.Common.Controls.OutlookTimePicker();
			this.labelTo = new System.Windows.Forms.Label();
			this.outlookTimePickerFrom = new Teleopti.Ccc.Win.Common.Controls.OutlookTimePicker();
			this.labelFrom = new System.Windows.Forms.Label();
			this.buttonAdvCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvOk = new Syncfusion.Windows.Forms.ButtonAdv();
			this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvNextDay)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerTo)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerFrom)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.tableLayoutPanel1.Controls.Add(this.checkBoxAdvNextDay, 2, 2);
			this.tableLayoutPanel1.Controls.Add(this.outlookTimePickerTo, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.labelTo, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.outlookTimePickerFrom, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.labelFrom, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.buttonAdvCancel, 2, 3);
			this.tableLayoutPanel1.Controls.Add(this.buttonAdvOk, 1, 3);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 4;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(355, 176);
			this.tableLayoutPanel1.TabIndex = 1;
			// 
			// checkBoxAdvNextDay
			// 
			this.checkBoxAdvNextDay.BeforeTouchSize = new System.Drawing.Size(114, 24);
			this.checkBoxAdvNextDay.DrawFocusRectangle = false;
			this.checkBoxAdvNextDay.Location = new System.Drawing.Point(238, 80);
			this.checkBoxAdvNextDay.Margin = new System.Windows.Forms.Padding(3, 10, 3, 3);
			this.checkBoxAdvNextDay.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxAdvNextDay.Name = "checkBoxAdvNextDay";
			this.checkBoxAdvNextDay.Size = new System.Drawing.Size(114, 24);
			this.checkBoxAdvNextDay.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Metro;
			this.checkBoxAdvNextDay.TabIndex = 3;
			this.checkBoxAdvNextDay.Text = "xxNextDay";
			this.checkBoxAdvNextDay.ThemesEnabled = false;
			this.checkBoxAdvNextDay.CheckedChanged += new System.EventHandler(this.checkBoxAdvNextDayCheckedChanged);
			// 
			// outlookTimePickerTo
			// 
			this.outlookTimePickerTo.BackColor = System.Drawing.Color.White;
			this.outlookTimePickerTo.BeforeTouchSize = new System.Drawing.Size(86, 23);
			this.outlookTimePickerTo.DefaultResolution = 0;
			this.outlookTimePickerTo.EnableNull = true;
			this.outlookTimePickerTo.FormatFromCulture = true;
			this.outlookTimePickerTo.Location = new System.Drawing.Point(103, 80);
			this.outlookTimePickerTo.Margin = new System.Windows.Forms.Padding(3, 10, 3, 3);
			this.outlookTimePickerTo.MaxTime = System.TimeSpan.Parse("23:59:00");
			this.outlookTimePickerTo.Name = "outlookTimePickerTo";
			this.outlookTimePickerTo.Size = new System.Drawing.Size(86, 23);
			this.outlookTimePickerTo.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.outlookTimePickerTo.TabIndex = 2;
			this.outlookTimePickerTo.Text = "00:00";
			this.outlookTimePickerTo.TextChanged += new System.EventHandler(this.outlookTimePickerToTextChanged);
			// 
			// labelTo
			// 
			this.labelTo.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.labelTo.AutoSize = true;
			this.labelTo.Location = new System.Drawing.Point(34, 82);
			this.labelTo.Name = "labelTo";
			this.labelTo.Size = new System.Drawing.Size(63, 15);
			this.labelTo.TabIndex = 3;
			this.labelTo.Text = "xxToColon";
			// 
			// outlookTimePickerFrom
			// 
			this.outlookTimePickerFrom.BackColor = System.Drawing.Color.White;
			this.outlookTimePickerFrom.BeforeTouchSize = new System.Drawing.Size(86, 23);
			this.outlookTimePickerFrom.DefaultResolution = 0;
			this.outlookTimePickerFrom.EnableNull = true;
			this.outlookTimePickerFrom.FormatFromCulture = true;
			this.outlookTimePickerFrom.Location = new System.Drawing.Point(103, 40);
			this.outlookTimePickerFrom.Margin = new System.Windows.Forms.Padding(3, 10, 3, 3);
			this.outlookTimePickerFrom.MaxTime = System.TimeSpan.Parse("23:59:00");
			this.outlookTimePickerFrom.Name = "outlookTimePickerFrom";
			this.outlookTimePickerFrom.Size = new System.Drawing.Size(86, 23);
			this.outlookTimePickerFrom.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.outlookTimePickerFrom.TabIndex = 1;
			this.outlookTimePickerFrom.Text = "00:00";
			this.outlookTimePickerFrom.TextChanged += new System.EventHandler(this.outlookTimePickerFromTextChanged);
			// 
			// labelFrom
			// 
			this.labelFrom.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.labelFrom.AutoSize = true;
			this.labelFrom.Location = new System.Drawing.Point(20, 42);
			this.labelFrom.Name = "labelFrom";
			this.labelFrom.Size = new System.Drawing.Size(77, 15);
			this.labelFrom.TabIndex = 2;
			this.labelFrom.Text = "xxFromColon";
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
			this.buttonAdvCancel.Location = new System.Drawing.Point(258, 139);
			this.buttonAdvCancel.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonAdvCancel.Name = "buttonAdvCancel";
			this.buttonAdvCancel.Size = new System.Drawing.Size(87, 27);
			this.buttonAdvCancel.TabIndex = 5;
			this.buttonAdvCancel.Text = "xxCancel";
			this.buttonAdvCancel.UseVisualStyle = true;
			this.buttonAdvCancel.Click += new System.EventHandler(this.buttonAdvCancelClick);
			// 
			// buttonAdvOk
			// 
			this.buttonAdvOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvOk.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvOk.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvOk.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonAdvOk.ForeColor = System.Drawing.Color.White;
			this.buttonAdvOk.IsBackStageButton = false;
			this.buttonAdvOk.Location = new System.Drawing.Point(138, 139);
			this.buttonAdvOk.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonAdvOk.Name = "buttonAdvOk";
			this.buttonAdvOk.Size = new System.Drawing.Size(87, 27);
			this.buttonAdvOk.TabIndex = 4;
			this.buttonAdvOk.Text = "xxOk";
			this.buttonAdvOk.UseVisualStyle = true;
			this.buttonAdvOk.Click += new System.EventHandler(this.buttonAdvOkClick);
			// 
			// errorProvider1
			// 
			this.errorProvider1.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
			this.errorProvider1.ContainerControl = this;
			// 
			// AgentStudentAvailabilityView
			// 
			this.AcceptButton = this.buttonAdvOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BorderColor = System.Drawing.Color.Blue;
			this.CancelButton = this.buttonAdvCancel;
			this.CaptionFont = new System.Drawing.Font("Segoe UI", 12F);
			this.ClientSize = new System.Drawing.Size(355, 176);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.HelpButton = false;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AgentStudentAvailabilityView";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "xxStudentAvailability";
			this.Load += new System.EventHandler(this.agentStudentAvailabilityViewLoad);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvNextDay)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerTo)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerFrom)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvOk;
		private System.Windows.Forms.Label labelFrom;
		private System.Windows.Forms.Label labelTo;
		private Common.Controls.OutlookTimePicker outlookTimePickerTo;
		private Common.Controls.OutlookTimePicker outlookTimePickerFrom;
		private System.Windows.Forms.ErrorProvider errorProvider1;
		private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdvNextDay;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvCancel;
	}
}