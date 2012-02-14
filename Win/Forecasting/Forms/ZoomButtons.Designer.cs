﻿namespace Teleopti.Ccc.Win.Forecasting.Forms
{
    partial class ZoomButtons
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
            this.labelWorkload = new System.Windows.Forms.Label();
            this.labelSkill = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonAdvSkillMonth = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonAdvSkillWeek = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonAdvWorkloadIntraday = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonAdvSkillIntraday = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonAdvWorkloadDay = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonAdvSkillDay = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonAdvWorkloadMonth = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonAvdWorkloadWeek = new Syncfusion.Windows.Forms.ButtonAdv();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelWorkload
            // 
            this.labelWorkload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.labelWorkload.AutoSize = true;
            this.labelWorkload.Location = new System.Drawing.Point(3, 11);
            this.labelWorkload.Name = "labelWorkload";
            this.labelWorkload.Size = new System.Drawing.Size(63, 13);
            this.labelWorkload.TabIndex = 8;
            this.labelWorkload.Text = "xxWorkload";
            // 
            // labelSkill
            // 
            this.labelSkill.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.labelSkill.AutoSize = true;
            this.labelSkill.Location = new System.Drawing.Point(3, 48);
            this.labelSkill.Name = "labelSkill";
            this.labelSkill.Size = new System.Drawing.Size(63, 13);
            this.labelSkill.TabIndex = 9;
            this.labelSkill.Text = "xxSkill";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 5;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.Controls.Add(this.buttonAdvSkillMonth, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.labelSkill, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.buttonAdvSkillWeek, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.labelWorkload, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.buttonAdvWorkloadIntraday, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.buttonAdvSkillIntraday, 4, 1);
            this.tableLayoutPanel1.Controls.Add(this.buttonAdvWorkloadDay, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.buttonAdvSkillDay, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.buttonAdvWorkloadMonth, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.buttonAvdWorkloadWeek, 2, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 1);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(217, 73);
            this.tableLayoutPanel1.TabIndex = 23;
            // 
            // buttonAdvSkillMonth
            // 
            this.buttonAdvSkillMonth.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.buttonAdvSkillMonth.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonAdvSkillMonth.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_SkillGridMonth;
            this.buttonAdvSkillMonth.Location = new System.Drawing.Point(73, 38);
            this.buttonAdvSkillMonth.Margin = new System.Windows.Forms.Padding(1);
            this.buttonAdvSkillMonth.Name = "buttonAdvSkillMonth";
            this.buttonAdvSkillMonth.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.buttonAdvSkillMonth.PushButton = true;
            this.buttonAdvSkillMonth.ResetStateOnLostFocus = false;
            this.buttonAdvSkillMonth.Size = new System.Drawing.Size(32, 32);
            this.buttonAdvSkillMonth.TabIndex = 20;
            this.buttonAdvSkillMonth.UseVisualStyle = true;
            this.buttonAdvSkillMonth.UseVisualStyleBackColor = true;
            this.buttonAdvSkillMonth.Click += new System.EventHandler(this.SkillZoomButtonClicked);
            // 
            // buttonAdvSkillWeek
            // 
            this.buttonAdvSkillWeek.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.buttonAdvSkillWeek.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonAdvSkillWeek.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_SkillGridWeek;
            this.buttonAdvSkillWeek.Location = new System.Drawing.Point(110, 38);
            this.buttonAdvSkillWeek.Margin = new System.Windows.Forms.Padding(1);
            this.buttonAdvSkillWeek.Name = "buttonAdvSkillWeek";
            this.buttonAdvSkillWeek.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.buttonAdvSkillWeek.PushButton = true;
            this.buttonAdvSkillWeek.ResetStateOnLostFocus = false;
            this.buttonAdvSkillWeek.Size = new System.Drawing.Size(32, 32);
            this.buttonAdvSkillWeek.TabIndex = 19;
            this.buttonAdvSkillWeek.UseVisualStyle = true;
            this.buttonAdvSkillWeek.UseVisualStyleBackColor = true;
            this.buttonAdvSkillWeek.Click += new System.EventHandler(this.SkillZoomButtonClicked);
            // 
            // buttonAdvWorkloadIntraday
            // 
            this.buttonAdvWorkloadIntraday.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.buttonAdvWorkloadIntraday.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonAdvWorkloadIntraday.Cursor = System.Windows.Forms.Cursors.Default;
            this.buttonAdvWorkloadIntraday.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_WorkloadGridIntraday;
            this.buttonAdvWorkloadIntraday.Location = new System.Drawing.Point(184, 2);
            this.buttonAdvWorkloadIntraday.Margin = new System.Windows.Forms.Padding(1);
            this.buttonAdvWorkloadIntraday.Name = "buttonAdvWorkloadIntraday";
            this.buttonAdvWorkloadIntraday.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.buttonAdvWorkloadIntraday.PushButton = true;
            this.buttonAdvWorkloadIntraday.ResetStateOnLostFocus = false;
            this.buttonAdvWorkloadIntraday.Size = new System.Drawing.Size(32, 32);
            this.buttonAdvWorkloadIntraday.TabIndex = 16;
            this.buttonAdvWorkloadIntraday.UseVisualStyle = true;
            this.buttonAdvWorkloadIntraday.UseVisualStyleBackColor = true;
            this.buttonAdvWorkloadIntraday.Click += new System.EventHandler(this.WorkloadZoomButtonClicked);
            // 
            // buttonAdvSkillIntraday
            // 
            this.buttonAdvSkillIntraday.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.buttonAdvSkillIntraday.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonAdvSkillIntraday.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_SkillGridIntraday;
            this.buttonAdvSkillIntraday.Location = new System.Drawing.Point(184, 38);
            this.buttonAdvSkillIntraday.Margin = new System.Windows.Forms.Padding(1);
            this.buttonAdvSkillIntraday.Name = "buttonAdvSkillIntraday";
            this.buttonAdvSkillIntraday.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.buttonAdvSkillIntraday.PushButton = true;
            this.buttonAdvSkillIntraday.ResetStateOnLostFocus = false;
            this.buttonAdvSkillIntraday.Size = new System.Drawing.Size(32, 32);
            this.buttonAdvSkillIntraday.TabIndex = 21;
            this.buttonAdvSkillIntraday.UseVisualStyle = true;
            this.buttonAdvSkillIntraday.UseVisualStyleBackColor = true;
            this.buttonAdvSkillIntraday.Click += new System.EventHandler(this.SkillZoomButtonClicked);
            // 
            // buttonAdvWorkloadDay
            // 
            this.buttonAdvWorkloadDay.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.buttonAdvWorkloadDay.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonAdvWorkloadDay.Cursor = System.Windows.Forms.Cursors.Default;
            this.buttonAdvWorkloadDay.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_WorkloadGridDay;
            this.buttonAdvWorkloadDay.Location = new System.Drawing.Point(147, 2);
            this.buttonAdvWorkloadDay.Margin = new System.Windows.Forms.Padding(1);
            this.buttonAdvWorkloadDay.Name = "buttonAdvWorkloadDay";
            this.buttonAdvWorkloadDay.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.buttonAdvWorkloadDay.PushButton = true;
            this.buttonAdvWorkloadDay.ResetStateOnLostFocus = false;
            this.buttonAdvWorkloadDay.Size = new System.Drawing.Size(32, 32);
            this.buttonAdvWorkloadDay.TabIndex = 18;
            this.buttonAdvWorkloadDay.UseVisualStyle = true;
            this.buttonAdvWorkloadDay.UseVisualStyleBackColor = true;
            this.buttonAdvWorkloadDay.Click += new System.EventHandler(this.WorkloadZoomButtonClicked);
            // 
            // buttonAdvSkillDay
            // 
            this.buttonAdvSkillDay.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.buttonAdvSkillDay.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonAdvSkillDay.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_SkillGridDay;
            this.buttonAdvSkillDay.Location = new System.Drawing.Point(147, 38);
            this.buttonAdvSkillDay.Margin = new System.Windows.Forms.Padding(1);
            this.buttonAdvSkillDay.Name = "buttonAdvSkillDay";
            this.buttonAdvSkillDay.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.buttonAdvSkillDay.PushButton = true;
            this.buttonAdvSkillDay.ResetStateOnLostFocus = false;
            this.buttonAdvSkillDay.Size = new System.Drawing.Size(32, 32);
            this.buttonAdvSkillDay.TabIndex = 22;
            this.buttonAdvSkillDay.UseVisualStyle = true;
            this.buttonAdvSkillDay.UseVisualStyleBackColor = true;
            this.buttonAdvSkillDay.Click += new System.EventHandler(this.SkillZoomButtonClicked);
            // 
            // buttonAdvWorkloadMonth
            // 
            this.buttonAdvWorkloadMonth.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.buttonAdvWorkloadMonth.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonAdvWorkloadMonth.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_WorkloadGridMonth;
            this.buttonAdvWorkloadMonth.Location = new System.Drawing.Point(73, 2);
            this.buttonAdvWorkloadMonth.Margin = new System.Windows.Forms.Padding(1);
            this.buttonAdvWorkloadMonth.Name = "buttonAdvWorkloadMonth";
            this.buttonAdvWorkloadMonth.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.buttonAdvWorkloadMonth.PushButton = true;
            this.buttonAdvWorkloadMonth.ResetStateOnLostFocus = false;
            this.buttonAdvWorkloadMonth.Size = new System.Drawing.Size(32, 32);
            this.buttonAdvWorkloadMonth.TabIndex = 14;
            this.buttonAdvWorkloadMonth.UseVisualStyle = true;
            this.buttonAdvWorkloadMonth.UseVisualStyleBackColor = true;
            this.buttonAdvWorkloadMonth.Click += new System.EventHandler(this.WorkloadZoomButtonClicked);
            // 
            // buttonAvdWorkloadWeek
            // 
            this.buttonAvdWorkloadWeek.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.buttonAvdWorkloadWeek.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonAvdWorkloadWeek.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_WorkloadGridWeek;
            this.buttonAvdWorkloadWeek.Location = new System.Drawing.Point(110, 2);
            this.buttonAvdWorkloadWeek.Margin = new System.Windows.Forms.Padding(1);
            this.buttonAvdWorkloadWeek.Name = "buttonAvdWorkloadWeek";
            this.buttonAvdWorkloadWeek.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.buttonAvdWorkloadWeek.PushButton = true;
            this.buttonAvdWorkloadWeek.ResetStateOnLostFocus = false;
            this.buttonAvdWorkloadWeek.Size = new System.Drawing.Size(32, 32);
            this.buttonAvdWorkloadWeek.TabIndex = 12;
            this.buttonAvdWorkloadWeek.UseVisualStyle = true;
            this.buttonAvdWorkloadWeek.UseVisualStyleBackColor = true;
            this.buttonAvdWorkloadWeek.Click += new System.EventHandler(this.WorkloadZoomButtonClicked);
            // 
            // ZoomButtons
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "ZoomButtons";
            this.Size = new System.Drawing.Size(217, 75);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvWorkloadDay;
        private System.Windows.Forms.Label labelWorkload;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAvdWorkloadWeek;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvWorkloadMonth;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvSkillIntraday;
        private System.Windows.Forms.Label labelSkill;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvSkillDay;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvSkillWeek;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvSkillMonth;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvWorkloadIntraday;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}
