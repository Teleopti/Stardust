namespace Teleopti.Ccc.Win.Common.Controls
{
    partial class FilterBoxItemAdvanced
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
            this.labelPreHeader = new System.Windows.Forms.Label();
            this.comboBoxAdvFilterOperand = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
            this.comboBoxAdvColumns = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
            this.buttonRemove = new System.Windows.Forms.Button();
            this.textBoxExt1 = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
            this.timeSpanTextBox1 = new Teleopti.Ccc.Win.Common.Controls.TimeSpanTextBox();
            this.dateTimePickerAdvDate = new Syncfusion.Windows.Forms.Tools.DateTimePickerAdv();
            this.office2007OutlookTimePicker1 = new Teleopti.Ccc.Win.Common.Controls.Office2007OutlookTimePicker(this.components);
            this.comboBoxAdvCriteriaList = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvFilterOperand)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvColumns)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textBoxExt1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvDate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvDate.Calendar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.office2007OutlookTimePicker1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvCriteriaList)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelPreHeader
            // 
            this.labelPreHeader.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.labelPreHeader.AutoSize = true;
            this.labelPreHeader.Location = new System.Drawing.Point(66, 7);
            this.labelPreHeader.Name = "labelPreHeader";
            this.labelPreHeader.Size = new System.Drawing.Size(25, 13);
            this.labelPreHeader.TabIndex = 0;
            this.labelPreHeader.Text = "ooo";
            // 
            // comboBoxAdvFilterOperand
            // 
            this.comboBoxAdvFilterOperand.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(251)))));
            this.comboBoxAdvFilterOperand.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAdvFilterOperand.IgnoreThemeBackground = true;
            this.comboBoxAdvFilterOperand.Location = new System.Drawing.Point(297, 3);
            this.comboBoxAdvFilterOperand.Name = "comboBoxAdvFilterOperand";
            this.comboBoxAdvFilterOperand.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.comboBoxAdvFilterOperand.Size = new System.Drawing.Size(42, 21);
            this.comboBoxAdvFilterOperand.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this.comboBoxAdvFilterOperand.SuppressDropDownEvent = true;
            this.comboBoxAdvFilterOperand.TabIndex = 5;
            // 
            // comboBoxAdvColumns
            // 
            this.comboBoxAdvColumns.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(251)))));
            this.comboBoxAdvColumns.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAdvColumns.IgnoreThemeBackground = true;
            this.comboBoxAdvColumns.Location = new System.Drawing.Point(153, 3);
            this.comboBoxAdvColumns.Name = "comboBoxAdvColumns";
            this.comboBoxAdvColumns.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.comboBoxAdvColumns.Size = new System.Drawing.Size(138, 21);
            this.comboBoxAdvColumns.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this.comboBoxAdvColumns.SuppressDropDownEvent = true;
            this.comboBoxAdvColumns.TabIndex = 6;
            this.comboBoxAdvColumns.SelectedIndexChanged += new System.EventHandler(this.comboBoxAdvColumns_SelectedIndexChanged);
            // 
            // buttonRemove
            // 
            this.buttonRemove.BackColor = System.Drawing.Color.Transparent;
            this.buttonRemove.FlatAppearance.BorderSize = 0;
            this.buttonRemove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonRemove.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Exit;
            this.buttonRemove.Location = new System.Drawing.Point(531, 3);
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.Size = new System.Drawing.Size(23, 22);
            this.buttonRemove.TabIndex = 7;
            this.buttonRemove.UseVisualStyleBackColor = false;
            this.buttonRemove.Click += new System.EventHandler(this.buttonRemoveClick);
            // 
            // textBoxExt1
            // 
            this.textBoxExt1.Location = new System.Drawing.Point(0, 1);
            this.textBoxExt1.Name = "textBoxExt1";
            this.textBoxExt1.OverflowIndicatorToolTipText = null;
            this.textBoxExt1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.textBoxExt1.Size = new System.Drawing.Size(176, 20);
            this.textBoxExt1.TabIndex = 8;
            this.textBoxExt1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxExt1_KeyPress);
            // 
            // timeSpanTextBox1
            // 
            this.timeSpanTextBox1.AllowNegativeValues = true;
            this.timeSpanTextBox1.Location = new System.Drawing.Point(3, 0);
            this.timeSpanTextBox1.Margin = new System.Windows.Forms.Padding(0);
            this.timeSpanTextBox1.Name = "timeSpanTextBox1";
            this.timeSpanTextBox1.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.timeSpanTextBox1.Size = new System.Drawing.Size(74, 22);
            this.timeSpanTextBox1.TabIndex = 9;
            // 
            // dateTimePickerAdvDate
            // 
            this.dateTimePickerAdvDate.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
            this.dateTimePickerAdvDate.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(193)))), ((int)(((byte)(222)))));
            this.dateTimePickerAdvDate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            // 
            // 
            // 
            this.dateTimePickerAdvDate.Calendar.AllowMultipleSelection = false;
            this.dateTimePickerAdvDate.Calendar.Culture = new System.Globalization.CultureInfo("sv-SE");
            this.dateTimePickerAdvDate.Calendar.DaysFont = new System.Drawing.Font("Verdana", 8F);
            this.dateTimePickerAdvDate.Calendar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dateTimePickerAdvDate.Calendar.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dateTimePickerAdvDate.Calendar.ForeColor = System.Drawing.SystemColors.ControlText;
            this.dateTimePickerAdvDate.Calendar.GridLines = Syncfusion.Windows.Forms.Grid.GridBorderStyle.None;
            this.dateTimePickerAdvDate.Calendar.HeaderEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.dateTimePickerAdvDate.Calendar.HeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.dateTimePickerAdvDate.Calendar.HeaderHeight = 20;
            this.dateTimePickerAdvDate.Calendar.HeaderStartColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252)))));
            this.dateTimePickerAdvDate.Calendar.HeadForeColor = System.Drawing.SystemColors.ControlText;
            this.dateTimePickerAdvDate.Calendar.HeadGradient = true;
            this.dateTimePickerAdvDate.Calendar.Location = new System.Drawing.Point(0, 0);
            this.dateTimePickerAdvDate.Calendar.Name = "monthCalendar";
            this.dateTimePickerAdvDate.Calendar.ScrollButtonSize = new System.Drawing.Size(24, 24);
            this.dateTimePickerAdvDate.Calendar.SelectedDates = new System.DateTime[0];
            this.dateTimePickerAdvDate.Calendar.Size = new System.Drawing.Size(206, 174);
            this.dateTimePickerAdvDate.Calendar.SizeToFit = true;
            this.dateTimePickerAdvDate.Calendar.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this.dateTimePickerAdvDate.Calendar.TabIndex = 0;
            this.dateTimePickerAdvDate.Calendar.WeekFont = new System.Drawing.Font("Verdana", 8F);
            this.dateTimePickerAdvDate.Calendar.WeekInterior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.PeachPuff, System.Drawing.Color.AntiqueWhite);
            // 
            // 
            // 
            this.dateTimePickerAdvDate.Calendar.NoneButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.dateTimePickerAdvDate.Calendar.NoneButton.Location = new System.Drawing.Point(134, 0);
            this.dateTimePickerAdvDate.Calendar.NoneButton.Size = new System.Drawing.Size(72, 20);
            this.dateTimePickerAdvDate.Calendar.NoneButton.Text = "None";
            this.dateTimePickerAdvDate.Calendar.NoneButton.UseVisualStyle = true;
            // 
            // 
            // 
            this.dateTimePickerAdvDate.Calendar.TodayButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.dateTimePickerAdvDate.Calendar.TodayButton.Location = new System.Drawing.Point(0, 0);
            this.dateTimePickerAdvDate.Calendar.TodayButton.Size = new System.Drawing.Size(134, 20);
            this.dateTimePickerAdvDate.Calendar.TodayButton.Text = "Today";
            this.dateTimePickerAdvDate.Calendar.TodayButton.UseVisualStyle = true;
            this.dateTimePickerAdvDate.CalendarFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dateTimePickerAdvDate.CalendarTitleBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252)))));
            this.dateTimePickerAdvDate.CalendarTitleForeColor = System.Drawing.SystemColors.ControlText;
            this.dateTimePickerAdvDate.Culture = new System.Globalization.CultureInfo("sv-SE");
            this.dateTimePickerAdvDate.DropDownImage = null;
            this.dateTimePickerAdvDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dateTimePickerAdvDate.Location = new System.Drawing.Point(2, 3);
            this.dateTimePickerAdvDate.MinValue = new System.DateTime(((long)(0)));
            this.dateTimePickerAdvDate.Name = "dateTimePickerAdvDate";
            this.dateTimePickerAdvDate.ShowCheckBox = false;
            this.dateTimePickerAdvDate.Size = new System.Drawing.Size(94, 20);
            this.dateTimePickerAdvDate.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this.dateTimePickerAdvDate.TabIndex = 10;
            this.dateTimePickerAdvDate.UseCurrentCulture = true;
            this.dateTimePickerAdvDate.Value = new System.DateTime(2008, 8, 6, 13, 39, 31, 156);
            // 
            // office2007OutlookTimePicker1
            // 
            this.office2007OutlookTimePicker1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(251)))));
            this.office2007OutlookTimePicker1.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
            this.office2007OutlookTimePicker1.IgnoreThemeBackground = true;
            this.office2007OutlookTimePicker1.Location = new System.Drawing.Point(0, 3);
            this.office2007OutlookTimePicker1.Name = "office2007OutlookTimePicker1";
            this.office2007OutlookTimePicker1.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.office2007OutlookTimePicker1.Size = new System.Drawing.Size(121, 19);
            this.office2007OutlookTimePicker1.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this.office2007OutlookTimePicker1.TabIndex = 11;
            this.office2007OutlookTimePicker1.Text = "office2007OutlookTimePicker1";
            this.office2007OutlookTimePicker1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // comboBoxAdvCriteriaList
            // 
            this.comboBoxAdvCriteriaList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(251)))));
            this.comboBoxAdvCriteriaList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAdvCriteriaList.IgnoreThemeBackground = true;
            this.comboBoxAdvCriteriaList.Location = new System.Drawing.Point(0, 1);
            this.comboBoxAdvCriteriaList.Name = "comboBoxAdvCriteriaList";
            this.comboBoxAdvCriteriaList.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.comboBoxAdvCriteriaList.Size = new System.Drawing.Size(176, 21);
            this.comboBoxAdvCriteriaList.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this.comboBoxAdvCriteriaList.SuppressDropDownEvent = true;
            this.comboBoxAdvCriteriaList.TabIndex = 12;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 6;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 1.323629F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.4902F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.66845F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.607199F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.11688F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5.519481F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.buttonRemove, 5, 0);
            this.tableLayoutPanel1.Controls.Add(this.comboBoxAdvColumns, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.comboBoxAdvFilterOperand, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.labelPreHeader, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(561, 28);
            this.tableLayoutPanel1.TabIndex = 13;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.comboBoxAdvCriteriaList);
            this.panel1.Controls.Add(this.textBoxExt1);
            this.panel1.Controls.Add(this.timeSpanTextBox1);
            this.panel1.Controls.Add(this.dateTimePickerAdvDate);
            this.panel1.Controls.Add(this.office2007OutlookTimePicker1);
            this.panel1.Location = new System.Drawing.Point(345, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(180, 22);
            this.panel1.TabIndex = 7;
            // 
            // FilterBoxItemAdvanced
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Margin = new System.Windows.Forms.Padding(0, 0, 0, 2);
            this.Name = "FilterBoxItemAdvanced";
            this.Size = new System.Drawing.Size(561, 28);
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvFilterOperand)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvColumns)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textBoxExt1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvDate.Calendar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvDate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.office2007OutlookTimePicker1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvCriteriaList)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelPreHeader;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvFilterOperand;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvColumns;
        private System.Windows.Forms.Button buttonRemove;
        private Syncfusion.Windows.Forms.Tools.TextBoxExt textBoxExt1;
        private TimeSpanTextBox timeSpanTextBox1;
        private Syncfusion.Windows.Forms.Tools.DateTimePickerAdv dateTimePickerAdvDate;
        private Office2007OutlookTimePicker office2007OutlookTimePicker1;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvCriteriaList;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;

    }
}
