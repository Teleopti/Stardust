namespace Teleopti.Ccc.Win.Common.Controls
{
    partial class FromToTimeDurationPicker
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
			this.office2007OutlookTimeDurationPickerStartTime = new Teleopti.Ccc.Win.Common.Controls.Office2007OutlookTimeDurationPicker(this.components);
			this.office2007OutlookTimeDurationPickerEndTime = new Teleopti.Ccc.Win.Common.Controls.Office2007OutlookTimeDurationPicker(this.components);
			this.checkBoxAdvWholeDay = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.office2007OutlookTimeDurationPickerStartTime)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.office2007OutlookTimeDurationPickerEndTime)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvWholeDay)).BeginInit();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 85F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 85F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.office2007OutlookTimeDurationPickerStartTime, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.office2007OutlookTimeDurationPickerEndTime, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.checkBoxAdvWholeDay, 2, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(288, 27);
			this.tableLayoutPanel1.TabIndex = 1;
			// 
			// office2007OutlookTimeDurationPickerStartTime
			// 
			this.office2007OutlookTimeDurationPickerStartTime.BackColor = System.Drawing.Color.White;
			this.office2007OutlookTimeDurationPickerStartTime.BeforeTouchSize = new System.Drawing.Size(79, 21);
			this.office2007OutlookTimeDurationPickerStartTime.BindableTimeValue = System.TimeSpan.Parse("20:07:00");
			this.office2007OutlookTimeDurationPickerStartTime.Dock = System.Windows.Forms.DockStyle.Left;
			this.office2007OutlookTimeDurationPickerStartTime.Location = new System.Drawing.Point(3, 3);
			this.office2007OutlookTimeDurationPickerStartTime.MaxValue = System.TimeSpan.Parse("2.00:00:00");
			this.office2007OutlookTimeDurationPickerStartTime.MinValue = System.TimeSpan.Parse("00:00:00");
			this.office2007OutlookTimeDurationPickerStartTime.Name = "office2007OutlookTimeDurationPickerStartTime";
			this.office2007OutlookTimeDurationPickerStartTime.Size = new System.Drawing.Size(79, 21);
			this.office2007OutlookTimeDurationPickerStartTime.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.office2007OutlookTimeDurationPickerStartTime.TabIndex = 0;
			this.office2007OutlookTimeDurationPickerStartTime.Text = "office2007OutlookTimeDurationPicker";
			// 
			// office2007OutlookTimeDurationPickerEndTime
			// 
			this.office2007OutlookTimeDurationPickerEndTime.BackColor = System.Drawing.Color.White;
			this.office2007OutlookTimeDurationPickerEndTime.BeforeTouchSize = new System.Drawing.Size(79, 21);
			this.office2007OutlookTimeDurationPickerEndTime.BindableTimeValue = System.TimeSpan.Parse("00:00:00");
			this.office2007OutlookTimeDurationPickerEndTime.Dock = System.Windows.Forms.DockStyle.Left;
			this.office2007OutlookTimeDurationPickerEndTime.Location = new System.Drawing.Point(88, 3);
			this.office2007OutlookTimeDurationPickerEndTime.MaxValue = System.TimeSpan.Parse("2.00:00:00");
			this.office2007OutlookTimeDurationPickerEndTime.MinValue = System.TimeSpan.Parse("00:00:00");
			this.office2007OutlookTimeDurationPickerEndTime.Name = "office2007OutlookTimeDurationPickerEndTime";
			this.office2007OutlookTimeDurationPickerEndTime.Size = new System.Drawing.Size(79, 21);
			this.office2007OutlookTimeDurationPickerEndTime.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.office2007OutlookTimeDurationPickerEndTime.TabIndex = 1;
			this.office2007OutlookTimeDurationPickerEndTime.Text = "office2007OutlookTimeDurationPicker2";
			// 
			// checkBoxAdvWholeDay
			// 
			this.checkBoxAdvWholeDay.BeforeTouchSize = new System.Drawing.Size(150, 21);
			this.checkBoxAdvWholeDay.Dock = System.Windows.Forms.DockStyle.Left;
			this.checkBoxAdvWholeDay.DrawFocusRectangle = false;
			this.checkBoxAdvWholeDay.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.checkBoxAdvWholeDay.Location = new System.Drawing.Point(173, 3);
			this.checkBoxAdvWholeDay.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxAdvWholeDay.Name = "checkBoxAdvWholeDay";
			this.checkBoxAdvWholeDay.Size = new System.Drawing.Size(112, 21);
			this.checkBoxAdvWholeDay.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Metro;
			this.checkBoxAdvWholeDay.TabIndex = 2;
			this.checkBoxAdvWholeDay.Text = "xxNextDay";
			this.checkBoxAdvWholeDay.ThemesEnabled = false;
			this.checkBoxAdvWholeDay.WrapText = false;
			// 
			// FromToTimeDurationPicker
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "FromToTimeDurationPicker";
			this.Size = new System.Drawing.Size(288, 27);
			this.tableLayoutPanel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.office2007OutlookTimeDurationPickerStartTime)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.office2007OutlookTimeDurationPickerEndTime)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvWholeDay)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Office2007OutlookTimeDurationPicker office2007OutlookTimeDurationPickerStartTime;
        private Office2007OutlookTimeDurationPicker office2007OutlookTimeDurationPickerEndTime;
        private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdvWholeDay;
    }
}
