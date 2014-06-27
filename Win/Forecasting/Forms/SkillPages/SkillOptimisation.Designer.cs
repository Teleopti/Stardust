namespace Teleopti.Ccc.Win.Forecasting.Forms.SkillPages
{
    partial class SkillOptimisation
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
			this.autoLabelPriority = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.autoLabelOverStaffingFactor = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.trackBarExPriority = new Syncfusion.Windows.Forms.Tools.TrackBarEx(1, 7);
			this.trackBarExOverStaffingFactor = new Syncfusion.Windows.Forms.Tools.TrackBarEx(10, 90);
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.autoLabelLow = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.autoLabelHigh = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
			this.autoLabelUnderStaff = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.autoLabelOverStaff = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.tableLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			this.tableLayoutPanel3.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 23.76812F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 76.23188F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 24F));
			this.tableLayoutPanel1.Controls.Add(this.autoLabelPriority, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.autoLabelOverStaffingFactor, 0, 5);
			this.tableLayoutPanel1.Controls.Add(this.trackBarExPriority, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.trackBarExOverStaffingFactor, 1, 5);
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 1, 4);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 7;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(366, 300);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// autoLabelPriority
			// 
			this.autoLabelPriority.Dock = System.Windows.Forms.DockStyle.Fill;
			this.autoLabelPriority.Location = new System.Drawing.Point(3, 40);
			this.autoLabelPriority.Name = "autoLabelPriority";
			this.autoLabelPriority.Size = new System.Drawing.Size(75, 26);
			this.autoLabelPriority.TabIndex = 2;
			this.autoLabelPriority.Text = "xxPriorityColon";
			this.autoLabelPriority.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// autoLabelOverStaffingFactor
			// 
			this.autoLabelOverStaffingFactor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.autoLabelOverStaffingFactor.Location = new System.Drawing.Point(3, 106);
			this.autoLabelOverStaffingFactor.Name = "autoLabelOverStaffingFactor";
			this.autoLabelOverStaffingFactor.Size = new System.Drawing.Size(75, 26);
			this.autoLabelOverStaffingFactor.TabIndex = 3;
			this.autoLabelOverStaffingFactor.Text = "xxAvoidColon";
			this.autoLabelOverStaffingFactor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// trackBarExPriority
			// 
			this.trackBarExPriority.Dock = System.Windows.Forms.DockStyle.Fill;
			this.trackBarExPriority.LargeChange = 3;
			this.trackBarExPriority.Location = new System.Drawing.Point(84, 43);
			this.trackBarExPriority.Name = "trackBarExPriority";
			this.trackBarExPriority.ShowButtons = false;
			this.trackBarExPriority.Size = new System.Drawing.Size(254, 20);
			this.trackBarExPriority.TabIndex = 0;
			this.trackBarExPriority.Text = "xxPriority";
			this.trackBarExPriority.TimerInterval = 100;
			this.trackBarExPriority.TrackBarGradientEnd = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(222)))), ((int)(((byte)(254)))));
			this.trackBarExPriority.TrackBarGradientStart = System.Drawing.Color.White;
			this.trackBarExPriority.Value = 4;
			this.trackBarExPriority.DoubleClick += new System.EventHandler(this.trackBarExPriorityDoubleClick);
			// 
			// trackBarExOverStaffingFactor
			// 
			this.trackBarExOverStaffingFactor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.trackBarExOverStaffingFactor.LargeChange = 20;
	        this.trackBarExOverStaffingFactor.SmallChange = 10;
			this.trackBarExOverStaffingFactor.Location = new System.Drawing.Point(84, 109);
			this.trackBarExOverStaffingFactor.Name = "trackBarExOverStaffingFactor";
			this.trackBarExOverStaffingFactor.ShowButtons = false;
			this.trackBarExOverStaffingFactor.Size = new System.Drawing.Size(254, 20);
			this.trackBarExOverStaffingFactor.TabIndex = 2;
			this.trackBarExOverStaffingFactor.Text = "xxOverStaffingFactor";
			this.trackBarExOverStaffingFactor.TimerInterval = 100;
			this.trackBarExOverStaffingFactor.TrackBarGradientEnd = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(222)))), ((int)(((byte)(254)))));
			this.trackBarExOverStaffingFactor.TrackBarGradientStart = System.Drawing.Color.White;
			this.trackBarExOverStaffingFactor.Value = 50;
			this.trackBarExOverStaffingFactor.DoubleClick += new System.EventHandler(this.trackBarExOverStaffingFactorDoubleClick);
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 2;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel2.Controls.Add(this.autoLabelLow, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.autoLabelHigh, 1, 0);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(84, 23);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 1;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 14F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(254, 14);
			this.tableLayoutPanel2.TabIndex = 6;
			// 
			// autoLabelLow
			// 
			this.autoLabelLow.Dock = System.Windows.Forms.DockStyle.Left;
			this.autoLabelLow.Location = new System.Drawing.Point(3, 0);
			this.autoLabelLow.Name = "autoLabelLow";
			this.autoLabelLow.Size = new System.Drawing.Size(37, 14);
			this.autoLabelLow.TabIndex = 0;
			this.autoLabelLow.Text = "xxLow";
			// 
			// autoLabelHigh
			// 
			this.autoLabelHigh.Dock = System.Windows.Forms.DockStyle.Right;
			this.autoLabelHigh.Location = new System.Drawing.Point(212, 0);
			this.autoLabelHigh.Name = "autoLabelHigh";
			this.autoLabelHigh.Size = new System.Drawing.Size(39, 14);
			this.autoLabelHigh.TabIndex = 1;
			this.autoLabelHigh.Text = "xxHigh";
			// 
			// tableLayoutPanel3
			// 
			this.tableLayoutPanel3.ColumnCount = 2;
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel3.Controls.Add(this.autoLabelUnderStaff, 0, 0);
			this.tableLayoutPanel3.Controls.Add(this.autoLabelOverStaff, 1, 0);
			this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel3.Location = new System.Drawing.Point(84, 89);
			this.tableLayoutPanel3.Name = "tableLayoutPanel3";
			this.tableLayoutPanel3.RowCount = 1;
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 14F));
			this.tableLayoutPanel3.Size = new System.Drawing.Size(254, 14);
			this.tableLayoutPanel3.TabIndex = 7;
			// 
			// autoLabelUnderStaff
			// 
			this.autoLabelUnderStaff.Location = new System.Drawing.Point(3, 0);
			this.autoLabelUnderStaff.Name = "autoLabelUnderStaff";
			this.autoLabelUnderStaff.Size = new System.Drawing.Size(82, 13);
			this.autoLabelUnderStaff.TabIndex = 0;
			this.autoLabelUnderStaff.Text = "xxUnderStaffing";
			// 
			// autoLabelOverStaff
			// 
			this.autoLabelOverStaff.Dock = System.Windows.Forms.DockStyle.Right;
			this.autoLabelOverStaff.Location = new System.Drawing.Point(175, 0);
			this.autoLabelOverStaff.Name = "autoLabelOverStaff";
			this.autoLabelOverStaff.Size = new System.Drawing.Size(76, 14);
			this.autoLabelOverStaff.TabIndex = 1;
			this.autoLabelOverStaff.Text = "xxOverStaffing";
			// 
			// SkillOptimisation
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "SkillOptimisation";
			this.Size = new System.Drawing.Size(366, 300);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			this.tableLayoutPanel3.ResumeLayout(false);
			this.tableLayoutPanel3.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelPriority;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelOverStaffingFactor;
        private Syncfusion.Windows.Forms.Tools.TrackBarEx trackBarExPriority;
        private Syncfusion.Windows.Forms.Tools.TrackBarEx trackBarExOverStaffingFactor;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelLow;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelHigh;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelUnderStaff;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelOverStaff;
    }
}
