namespace Teleopti.Ccc.AgentPortal.Requests
{
	partial class PersonAccountView
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
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.gridControlPersonAccounts = new Syncfusion.Windows.Forms.Grid.GridControl();
			this.dateTimePickerAdvSelectedDate = new Syncfusion.Windows.Forms.Tools.DateTimePickerAdv();
			this.autoLabelDateSelection = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridControlPersonAccounts)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvSelectedDate)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvSelectedDate.Calendar)).BeginInit();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.gridControlPersonAccounts, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.dateTimePickerAdvSelectedDate, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.autoLabelDateSelection, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(463, 161);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// gridControlPersonAccounts
			// 
			this.tableLayoutPanel1.SetColumnSpan(this.gridControlPersonAccounts, 2);
			this.gridControlPersonAccounts.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridControlPersonAccounts.GridOfficeScrollBars = Syncfusion.Windows.Forms.OfficeScrollBars.Office2007;
			this.gridControlPersonAccounts.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Office2007Blue;
			this.gridControlPersonAccounts.Location = new System.Drawing.Point(3, 33);
			this.gridControlPersonAccounts.Name = "gridControlPersonAccounts";
			this.gridControlPersonAccounts.NumberedColHeaders = false;
			this.gridControlPersonAccounts.NumberedRowHeaders = false;
			this.gridControlPersonAccounts.Office2007ScrollBars = true;
			this.gridControlPersonAccounts.Properties.BackgroundColor = System.Drawing.Color.White;
			this.gridControlPersonAccounts.Properties.RowHeaders = false;
			this.gridControlPersonAccounts.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
			this.gridControlPersonAccounts.Size = new System.Drawing.Size(457, 125);
			this.gridControlPersonAccounts.SmartSizeBox = false;
			this.gridControlPersonAccounts.TabIndex = 0;
			this.gridControlPersonAccounts.Text = "gridControlPersonAccounts";
			this.gridControlPersonAccounts.UseRightToLeftCompatibleTextBox = true;
			this.gridControlPersonAccounts.RowsRemoving += new Syncfusion.Windows.Forms.Grid.GridRangeRemovingEventHandler(this.gridControlPersonAccounts_RowsRemoving);
			// 
			// dateTimePickerAdvSelectedDate
			// 
			this.dateTimePickerAdvSelectedDate.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.dateTimePickerAdvSelectedDate.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
			this.dateTimePickerAdvSelectedDate.BorderColor = System.Drawing.Color.Empty;
			this.dateTimePickerAdvSelectedDate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			// 
			// 
			// 
			this.dateTimePickerAdvSelectedDate.Calendar.AllowMultipleSelection = false;
			this.dateTimePickerAdvSelectedDate.Calendar.Culture = new System.Globalization.CultureInfo("sv-SE");
			this.dateTimePickerAdvSelectedDate.Calendar.DaysFont = new System.Drawing.Font("Verdana", 8F);
			this.dateTimePickerAdvSelectedDate.Calendar.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dateTimePickerAdvSelectedDate.Calendar.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dateTimePickerAdvSelectedDate.Calendar.ForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvSelectedDate.Calendar.GridLines = Syncfusion.Windows.Forms.Grid.GridBorderStyle.None;
			this.dateTimePickerAdvSelectedDate.Calendar.HeaderEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvSelectedDate.Calendar.HeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.dateTimePickerAdvSelectedDate.Calendar.HeaderHeight = 20;
			this.dateTimePickerAdvSelectedDate.Calendar.HeaderStartColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252)))));
			this.dateTimePickerAdvSelectedDate.Calendar.HeadForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvSelectedDate.Calendar.HeadGradient = true;
			this.dateTimePickerAdvSelectedDate.Calendar.Iso8601CalenderFormat = false;
			this.dateTimePickerAdvSelectedDate.Calendar.Location = new System.Drawing.Point(0, 0);
			this.dateTimePickerAdvSelectedDate.Calendar.Name = "monthCalendar";
			this.dateTimePickerAdvSelectedDate.Calendar.ScrollButtonSize = new System.Drawing.Size(24, 24);
			this.dateTimePickerAdvSelectedDate.Calendar.SelectedDates = new System.DateTime[0];
			this.dateTimePickerAdvSelectedDate.Calendar.Size = new System.Drawing.Size(194, 174);
			this.dateTimePickerAdvSelectedDate.Calendar.SizeToFit = true;
			this.dateTimePickerAdvSelectedDate.Calendar.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
			this.dateTimePickerAdvSelectedDate.Calendar.TabIndex = 0;
			this.dateTimePickerAdvSelectedDate.Calendar.WeekFont = new System.Drawing.Font("Verdana", 8F);
			this.dateTimePickerAdvSelectedDate.Calendar.WeekInterior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.PeachPuff, System.Drawing.Color.AntiqueWhite);
			// 
			// 
			// 
			this.dateTimePickerAdvSelectedDate.Calendar.NoneButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.dateTimePickerAdvSelectedDate.Calendar.NoneButton.Location = new System.Drawing.Point(122, 0);
			this.dateTimePickerAdvSelectedDate.Calendar.NoneButton.Size = new System.Drawing.Size(72, 20);
			this.dateTimePickerAdvSelectedDate.Calendar.NoneButton.Text = "None";
			this.dateTimePickerAdvSelectedDate.Calendar.NoneButton.UseVisualStyle = true;
			this.dateTimePickerAdvSelectedDate.Calendar.NoneButton.Visible = false;
			// 
			// 
			// 
			this.dateTimePickerAdvSelectedDate.Calendar.TodayButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.dateTimePickerAdvSelectedDate.Calendar.TodayButton.Location = new System.Drawing.Point(0, 0);
			this.dateTimePickerAdvSelectedDate.Calendar.TodayButton.Size = new System.Drawing.Size(194, 20);
			this.dateTimePickerAdvSelectedDate.Calendar.TodayButton.Text = "Today";
			this.dateTimePickerAdvSelectedDate.Calendar.TodayButton.UseVisualStyle = true;
			this.dateTimePickerAdvSelectedDate.Calendar.Click += new System.EventHandler(this.dateTimePickerAdvSelectedDate_Calendar_Click);
			this.dateTimePickerAdvSelectedDate.CalendarTitleBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252)))));
			this.dateTimePickerAdvSelectedDate.CalendarTitleForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvSelectedDate.Culture = new System.Globalization.CultureInfo("sv-SE");
			this.dateTimePickerAdvSelectedDate.DropDownImage = null;
			this.dateTimePickerAdvSelectedDate.EnableNullDate = false;
			this.dateTimePickerAdvSelectedDate.EnableNullKeys = false;
			this.dateTimePickerAdvSelectedDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
			this.dateTimePickerAdvSelectedDate.Location = new System.Drawing.Point(143, 5);
			this.dateTimePickerAdvSelectedDate.MinValue = new System.DateTime(((long)(0)));
			this.dateTimePickerAdvSelectedDate.Name = "dateTimePickerAdvSelectedDate";
			this.dateTimePickerAdvSelectedDate.NoneButtonVisible = false;
			this.dateTimePickerAdvSelectedDate.ShowCheckBox = false;
			this.dateTimePickerAdvSelectedDate.Size = new System.Drawing.Size(114, 20);
			this.dateTimePickerAdvSelectedDate.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
			this.dateTimePickerAdvSelectedDate.TabIndex = 1;
			this.dateTimePickerAdvSelectedDate.Value = new System.DateTime(2010, 2, 11, 15, 15, 38, 986);
			this.dateTimePickerAdvSelectedDate.ValueChanged += new System.EventHandler(this.dateTimePickerAdvSelectedDate_ValueChanged);
			this.dateTimePickerAdvSelectedDate.Leave += new System.EventHandler(this.dateTimePickerAdvSelectedDate_Leave);
			// 
			// autoLabelDateSelection
			// 
			this.autoLabelDateSelection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.autoLabelDateSelection.Location = new System.Drawing.Point(3, 8);
			this.autoLabelDateSelection.Name = "autoLabelDateSelection";
			this.autoLabelDateSelection.Size = new System.Drawing.Size(134, 13);
			this.autoLabelDateSelection.TabIndex = 2;
			this.autoLabelDateSelection.Text = "xxSelectedDateColon";
			// 
			// PersonAccountView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "PersonAccountView";
			this.Size = new System.Drawing.Size(463, 161);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridControlPersonAccounts)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvSelectedDate.Calendar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvSelectedDate)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private Syncfusion.Windows.Forms.Grid.GridControl gridControlPersonAccounts;
		private Syncfusion.Windows.Forms.Tools.DateTimePickerAdv dateTimePickerAdvSelectedDate;
		private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelDateSelection;
	}
}
