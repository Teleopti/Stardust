namespace Teleopti.Ccc.Win.Common.Controls
{
    partial class FromToTimePicker
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
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.office2007OutlookTimePickerStartTime = new Teleopti.Ccc.Win.Common.Controls.Office2007OutlookTimePicker(this.components);
			this.office2007OutlookTimePickerEndTime = new Teleopti.Ccc.Win.Common.Controls.Office2007OutlookTimePicker(this.components);
			this.checkBoxAdvWholeDay = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.office2007OutlookTimePickerStartTime)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.office2007OutlookTimePickerEndTime)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvWholeDay)).BeginInit();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 85F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 85F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.office2007OutlookTimePickerStartTime, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.office2007OutlookTimePickerEndTime, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.checkBoxAdvWholeDay, 2, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(288, 27);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// office2007OutlookTimePickerStartTime
			// 
			this.office2007OutlookTimePickerStartTime.BackColor = System.Drawing.Color.White;
			this.office2007OutlookTimePickerStartTime.BeforeTouchSize = new System.Drawing.Size(79, 21);
			this.office2007OutlookTimePickerStartTime.BindableTimeValue = System.TimeSpan.Parse("20:07:00");
			this.office2007OutlookTimePickerStartTime.Dock = System.Windows.Forms.DockStyle.Left;
			this.office2007OutlookTimePickerStartTime.Location = new System.Drawing.Point(3, 3);
			this.office2007OutlookTimePickerStartTime.MaxValue = System.TimeSpan.Parse("2.00:00:00");
			this.office2007OutlookTimePickerStartTime.MinValue = System.TimeSpan.Parse("00:00:00");
			this.office2007OutlookTimePickerStartTime.Name = "office2007OutlookTimePickerStartTime";
			this.office2007OutlookTimePickerStartTime.Size = new System.Drawing.Size(79, 21);
			this.office2007OutlookTimePickerStartTime.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.office2007OutlookTimePickerStartTime.TabIndex = 0;
			this.office2007OutlookTimePickerStartTime.Text = "office2007OutlookTimePicker";
			// 
			// office2007OutlookTimePickerEndTime
			// 
			this.office2007OutlookTimePickerEndTime.BackColor = System.Drawing.Color.White;
			this.office2007OutlookTimePickerEndTime.BeforeTouchSize = new System.Drawing.Size(79, 21);
			this.office2007OutlookTimePickerEndTime.BindableTimeValue = System.TimeSpan.Parse("00:00:00");
			this.office2007OutlookTimePickerEndTime.Dock = System.Windows.Forms.DockStyle.Left;
			this.office2007OutlookTimePickerEndTime.Location = new System.Drawing.Point(88, 3);
			this.office2007OutlookTimePickerEndTime.MaxValue = System.TimeSpan.Parse("2.00:00:00");
			this.office2007OutlookTimePickerEndTime.MinValue = System.TimeSpan.Parse("00:00:00");
			this.office2007OutlookTimePickerEndTime.Name = "office2007OutlookTimePickerEndTime";
			this.office2007OutlookTimePickerEndTime.Size = new System.Drawing.Size(79, 21);
			this.office2007OutlookTimePickerEndTime.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.office2007OutlookTimePickerEndTime.TabIndex = 1;
			this.office2007OutlookTimePickerEndTime.Text = "office2007OutlookTimePicker2";
			// 
			// checkBoxAdvWholeDay
			// 
			this.checkBoxAdvWholeDay.BeforeTouchSize = new System.Drawing.Size(150, 21);
			this.checkBoxAdvWholeDay.Dock = System.Windows.Forms.DockStyle.Left;
			this.checkBoxAdvWholeDay.DrawFocusRectangle = false;
			this.checkBoxAdvWholeDay.Location = new System.Drawing.Point(173, 3);
			this.checkBoxAdvWholeDay.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxAdvWholeDay.Name = "checkBoxAdvWholeDay";
			this.checkBoxAdvWholeDay.Size = new System.Drawing.Size(112, 21);
			this.checkBoxAdvWholeDay.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Metro;
			this.checkBoxAdvWholeDay.TabIndex = 2;
			this.checkBoxAdvWholeDay.Text = "xxNextDay";
			this.checkBoxAdvWholeDay.ThemesEnabled = false;
			// 
			// FromToTimePicker
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "FromToTimePicker";
			this.Size = new System.Drawing.Size(288, 27);
			this.tableLayoutPanel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.office2007OutlookTimePickerStartTime)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.office2007OutlookTimePickerEndTime)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvWholeDay)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Office2007OutlookTimePicker office2007OutlookTimePickerStartTime;
        private Office2007OutlookTimePicker office2007OutlookTimePickerEndTime;
        private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdvWholeDay;

    }
}
