namespace Teleopti.Ccc.Win.Scheduling
{
	partial class PublishScheduleDateView
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
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.buttonAdvOk = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.listViewControlSets = new System.Windows.Forms.ListView();
			this.labelControlSets = new System.Windows.Forms.Label();
			this.datePicker = new Syncfusion.Windows.Forms.Tools.DateTimePickerAdv();
			this.labelPublishToDate = new System.Windows.Forms.Label();
			this.tableLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.datePicker)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.datePicker.Calendar)).BeginInit();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 4);
			this.tableLayoutPanel1.Controls.Add(this.listViewControlSets, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.labelControlSets, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.datePicker, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.labelPublishToDate, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 5;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 65F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(316, 296);
			this.tableLayoutPanel1.TabIndex = 2;
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 2;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 61.47959F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 38.52041F));
			this.tableLayoutPanel2.Controls.Add(this.buttonAdvOk, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.buttonAdvCancel, 1, 0);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 234);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 1;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(310, 59);
			this.tableLayoutPanel2.TabIndex = 10;
			// 
			// buttonAdvOk
			// 
			this.buttonAdvOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvOk.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvOk.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvOk.BeforeTouchSize = new System.Drawing.Size(91, 31);
			this.buttonAdvOk.ForeColor = System.Drawing.Color.White;
			this.buttonAdvOk.IsBackStageButton = false;
			this.buttonAdvOk.Location = new System.Drawing.Point(87, 16);
			this.buttonAdvOk.Margin = new System.Windows.Forms.Padding(3, 3, 12, 12);
			this.buttonAdvOk.Name = "buttonAdvOk";
			this.buttonAdvOk.Size = new System.Drawing.Size(91, 31);
			this.buttonAdvOk.TabIndex = 4;
			this.buttonAdvOk.Text = "xxOk";
			this.buttonAdvOk.UseVisualStyle = true;
			this.buttonAdvOk.Click += new System.EventHandler(this.buttonAdvOkClick);
			// 
			// buttonAdvCancel
			// 
			this.buttonAdvCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvCancel.BeforeTouchSize = new System.Drawing.Size(92, 31);
			this.buttonAdvCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonAdvCancel.ForeColor = System.Drawing.Color.White;
			this.buttonAdvCancel.IsBackStageButton = false;
			this.buttonAdvCancel.Location = new System.Drawing.Point(206, 16);
			this.buttonAdvCancel.Margin = new System.Windows.Forms.Padding(3, 3, 12, 12);
			this.buttonAdvCancel.Name = "buttonAdvCancel";
			this.buttonAdvCancel.Size = new System.Drawing.Size(92, 31);
			this.buttonAdvCancel.TabIndex = 5;
			this.buttonAdvCancel.Text = "xxCancel";
			this.buttonAdvCancel.UseVisualStyle = true;
			// 
			// listViewControlSets
			// 
			this.listViewControlSets.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.listViewControlSets.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listViewControlSets.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.listViewControlSets.Location = new System.Drawing.Point(3, 97);
			this.listViewControlSets.MultiSelect = false;
			this.listViewControlSets.Name = "listViewControlSets";
			this.listViewControlSets.Size = new System.Drawing.Size(310, 131);
			this.listViewControlSets.TabIndex = 13;
			this.listViewControlSets.UseCompatibleStateImageBehavior = false;
			this.listViewControlSets.View = System.Windows.Forms.View.Details;
			this.listViewControlSets.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.listViewControlSetsItemSelectionChanged);
			// 
			// labelControlSets
			// 
			this.labelControlSets.AutoSize = true;
			this.labelControlSets.Location = new System.Drawing.Point(3, 68);
			this.labelControlSets.Name = "labelControlSets";
			this.labelControlSets.Size = new System.Drawing.Size(62, 15);
			this.labelControlSets.TabIndex = 11;
			this.labelControlSets.Text = "xxApplyTo";
			// 
			// datePicker
			// 
			this.datePicker.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.datePicker.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.White);
			this.datePicker.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
			this.datePicker.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			// 
			// 
			// 
			this.datePicker.Calendar.AllowMultipleSelection = false;
			this.datePicker.Calendar.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.datePicker.Calendar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.datePicker.Calendar.BottomHeight = 25;
			this.datePicker.Calendar.Culture = new System.Globalization.CultureInfo("sv-SE");
			this.datePicker.Calendar.DayNamesColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.datePicker.Calendar.DayNamesFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.datePicker.Calendar.DaysFont = new System.Drawing.Font("Segoe UI", 9F);
			this.datePicker.Calendar.Dock = System.Windows.Forms.DockStyle.Fill;
			this.datePicker.Calendar.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.datePicker.Calendar.ForeColor = System.Drawing.SystemColors.ControlText;
			this.datePicker.Calendar.GridLines = Syncfusion.Windows.Forms.Grid.GridBorderStyle.None;
			this.datePicker.Calendar.HeaderEndColor = System.Drawing.Color.White;
			this.datePicker.Calendar.HeaderStartColor = System.Drawing.Color.White;
			this.datePicker.Calendar.HeadForeColor = System.Drawing.SystemColors.ControlText;
			this.datePicker.Calendar.HighlightColor = System.Drawing.Color.White;
			this.datePicker.Calendar.Iso8601CalenderFormat = false;
			this.datePicker.Calendar.Location = new System.Drawing.Point(0, 0);
			this.datePicker.Calendar.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.datePicker.Calendar.MinValue = new System.DateTime(1990, 12, 31, 0, 0, 0, 0);
			this.datePicker.Calendar.Name = "monthCalendar";
			this.datePicker.Calendar.ScrollButtonSize = new System.Drawing.Size(24, 24);
			this.datePicker.Calendar.SelectedDates = new System.DateTime[0];
			this.datePicker.Calendar.ShowWeekNumbers = true;
			this.datePicker.Calendar.Size = new System.Drawing.Size(308, 174);
			this.datePicker.Calendar.SizeToFit = true;
			this.datePicker.Calendar.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.datePicker.Calendar.TabIndex = 0;
			this.datePicker.Calendar.ThemedEnabledGrid = true;
			this.datePicker.Calendar.WeekFont = new System.Drawing.Font("Verdana", 8F);
			this.datePicker.Calendar.WeekInterior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.PeachPuff, System.Drawing.Color.AntiqueWhite);
			// 
			// 
			// 
			this.datePicker.Calendar.NoneButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.datePicker.Calendar.NoneButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.datePicker.Calendar.NoneButton.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.datePicker.Calendar.NoneButton.ForeColor = System.Drawing.Color.White;
			this.datePicker.Calendar.NoneButton.IsBackStageButton = false;
			this.datePicker.Calendar.NoneButton.Location = new System.Drawing.Point(224, 0);
			this.datePicker.Calendar.NoneButton.Size = new System.Drawing.Size(84, 25);
			this.datePicker.Calendar.NoneButton.Text = "None";
			this.datePicker.Calendar.NoneButton.UseVisualStyle = true;
			// 
			// 
			// 
			this.datePicker.Calendar.TodayButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.datePicker.Calendar.TodayButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.datePicker.Calendar.TodayButton.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.datePicker.Calendar.TodayButton.ForeColor = System.Drawing.Color.White;
			this.datePicker.Calendar.TodayButton.IsBackStageButton = false;
			this.datePicker.Calendar.TodayButton.Location = new System.Drawing.Point(0, 0);
			this.datePicker.Calendar.TodayButton.Size = new System.Drawing.Size(224, 25);
			this.datePicker.Calendar.TodayButton.Text = "Today";
			this.datePicker.Calendar.TodayButton.UseVisualStyle = true;
			this.datePicker.CalendarFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.datePicker.CalendarForeColor = System.Drawing.SystemColors.ControlText;
			this.datePicker.CalendarSize = new System.Drawing.Size(189, 176);
			this.datePicker.CalendarTitleForeColor = System.Drawing.SystemColors.ControlText;
			this.datePicker.DropDownImage = null;
			this.datePicker.DropDownNormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.datePicker.DropDownPressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.datePicker.DropDownSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(179)))), ((int)(((byte)(255)))));
			this.datePicker.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			this.datePicker.Format = System.Windows.Forms.DateTimePickerFormat.Short;
			this.datePicker.Location = new System.Drawing.Point(3, 25);
			this.datePicker.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.datePicker.MinValue = new System.DateTime(1990, 12, 31, 23, 59, 0, 0);
			this.datePicker.Name = "datePicker";
			this.datePicker.NoneButtonVisible = false;
			this.datePicker.ShowCheckBox = false;
			this.datePicker.Size = new System.Drawing.Size(310, 21);
			this.datePicker.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.datePicker.TabIndex = 9;
			this.datePicker.ThemedChildControls = true;
			this.datePicker.ThemesEnabled = true;
			this.datePicker.Value = new System.DateTime(2012, 1, 19, 15, 2, 3, 865);
			this.datePicker.ValueChanged += new System.EventHandler(this.datePickerValueChanged);
			// 
			// labelPublishToDate
			// 
			this.labelPublishToDate.AutoSize = true;
			this.labelPublishToDate.Location = new System.Drawing.Point(3, 0);
			this.labelPublishToDate.Name = "labelPublishToDate";
			this.labelPublishToDate.Size = new System.Drawing.Size(31, 15);
			this.labelPublishToDate.TabIndex = 12;
			this.labelPublishToDate.Text = "xxTo";
			// 
			// PublishScheduleDateView
			// 
			this.AcceptButton = this.buttonAdvOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.buttonAdvCancel;
			this.CaptionFont = new System.Drawing.Font("Segoe UI", 12F);
			this.ClientSize = new System.Drawing.Size(316, 296);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.HelpButton = false;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "PublishScheduleDateView";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "xxPublishSchedule";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.tableLayoutPanel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.datePicker.Calendar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.datePicker)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvCancel;
		private Syncfusion.Windows.Forms.Tools.DateTimePickerAdv datePicker;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvOk;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		private System.Windows.Forms.Label labelControlSets;
		private System.Windows.Forms.Label labelPublishToDate;
		private System.Windows.Forms.ListView listViewControlSets;
	}
}