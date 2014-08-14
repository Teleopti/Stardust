namespace Teleopti.Ccc.Win.Forecasting.Forms
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
			this.components = new System.ComponentModel.Container();
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
			this.labelWorkload.Location = new System.Drawing.Point(3, 10);
			this.labelWorkload.Name = "labelWorkload";
			this.labelWorkload.Size = new System.Drawing.Size(68, 15);
			this.labelWorkload.TabIndex = 8;
			this.labelWorkload.Text = "xxWorkload";
			// 
			// labelSkill
			// 
			this.labelSkill.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.labelSkill.AutoSize = true;
			this.labelSkill.Location = new System.Drawing.Point(3, 47);
			this.labelSkill.Name = "labelSkill";
			this.labelSkill.Size = new System.Drawing.Size(68, 15);
			this.labelSkill.TabIndex = 9;
			this.labelSkill.Text = "xxSkill";
			// 
			// tableLayoutPanel1
			// 
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
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(253, 73);
			this.tableLayoutPanel1.TabIndex = 23;
			// 
			// buttonAdvSkillMonth
			// 
			this.buttonAdvSkillMonth.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.buttonAdvSkillMonth.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvSkillMonth.BackColor = System.Drawing.Color.White;
			this.buttonAdvSkillMonth.BeforeTouchSize = new System.Drawing.Size(37, 35);
			this.buttonAdvSkillMonth.ForeColor = System.Drawing.Color.White;
			this.buttonAdvSkillMonth.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_SkillGridMonth;
			this.buttonAdvSkillMonth.IsBackStageButton = false;
			this.buttonAdvSkillMonth.Location = new System.Drawing.Point(80, 37);
			this.buttonAdvSkillMonth.Margin = new System.Windows.Forms.Padding(1);
			this.buttonAdvSkillMonth.Name = "buttonAdvSkillMonth";
			this.buttonAdvSkillMonth.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Silver;
			this.buttonAdvSkillMonth.Office2010ColorScheme = Syncfusion.Windows.Forms.Office2010Theme.Silver;
			this.buttonAdvSkillMonth.PushButton = true;
			this.buttonAdvSkillMonth.ResetStateOnLostFocus = false;
			this.buttonAdvSkillMonth.Size = new System.Drawing.Size(37, 35);
			this.buttonAdvSkillMonth.TabIndex = 20;
			this.buttonAdvSkillMonth.UseVisualStyle = true;
			this.buttonAdvSkillMonth.UseVisualStyleBackColor = true;
			this.buttonAdvSkillMonth.Click += new System.EventHandler(this.SkillZoomButtonClicked);
			// 
			// buttonAdvSkillWeek
			// 
			this.buttonAdvSkillWeek.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.buttonAdvSkillWeek.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvSkillWeek.BackColor = System.Drawing.Color.White;
			this.buttonAdvSkillWeek.BeforeTouchSize = new System.Drawing.Size(37, 35);
			this.buttonAdvSkillWeek.ForeColor = System.Drawing.Color.White;
			this.buttonAdvSkillWeek.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_SkillGridWeek;
			this.buttonAdvSkillWeek.IsBackStageButton = false;
			this.buttonAdvSkillWeek.Location = new System.Drawing.Point(124, 37);
			this.buttonAdvSkillWeek.Margin = new System.Windows.Forms.Padding(1);
			this.buttonAdvSkillWeek.Name = "buttonAdvSkillWeek";
			this.buttonAdvSkillWeek.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Silver;
			this.buttonAdvSkillWeek.Office2010ColorScheme = Syncfusion.Windows.Forms.Office2010Theme.Silver;
			this.buttonAdvSkillWeek.PushButton = true;
			this.buttonAdvSkillWeek.ResetStateOnLostFocus = false;
			this.buttonAdvSkillWeek.Size = new System.Drawing.Size(37, 35);
			this.buttonAdvSkillWeek.TabIndex = 19;
			this.buttonAdvSkillWeek.UseVisualStyle = true;
			this.buttonAdvSkillWeek.UseVisualStyleBackColor = true;
			this.buttonAdvSkillWeek.Click += new System.EventHandler(this.SkillZoomButtonClicked);
			// 
			// buttonAdvWorkloadIntraday
			// 
			this.buttonAdvWorkloadIntraday.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.buttonAdvWorkloadIntraday.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvWorkloadIntraday.BackColor = System.Drawing.Color.White;
			this.buttonAdvWorkloadIntraday.BeforeTouchSize = new System.Drawing.Size(37, 34);
			this.buttonAdvWorkloadIntraday.Cursor = System.Windows.Forms.Cursors.Default;
			this.buttonAdvWorkloadIntraday.ForeColor = System.Drawing.Color.White;
			this.buttonAdvWorkloadIntraday.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_WorkloadGridIntraday;
			this.buttonAdvWorkloadIntraday.IsBackStageButton = false;
			this.buttonAdvWorkloadIntraday.Location = new System.Drawing.Point(215, 1);
			this.buttonAdvWorkloadIntraday.Margin = new System.Windows.Forms.Padding(1);
			this.buttonAdvWorkloadIntraday.Name = "buttonAdvWorkloadIntraday";
			this.buttonAdvWorkloadIntraday.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Silver;
			this.buttonAdvWorkloadIntraday.Office2010ColorScheme = Syncfusion.Windows.Forms.Office2010Theme.Silver;
			this.buttonAdvWorkloadIntraday.PushButton = true;
			this.buttonAdvWorkloadIntraday.ResetStateOnLostFocus = false;
			this.buttonAdvWorkloadIntraday.Size = new System.Drawing.Size(37, 34);
			this.buttonAdvWorkloadIntraday.TabIndex = 16;
			this.buttonAdvWorkloadIntraday.UseVisualStyle = true;
			this.buttonAdvWorkloadIntraday.UseVisualStyleBackColor = true;
			this.buttonAdvWorkloadIntraday.Click += new System.EventHandler(this.WorkloadZoomButtonClicked);
			// 
			// buttonAdvSkillIntraday
			// 
			this.buttonAdvSkillIntraday.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.buttonAdvSkillIntraday.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvSkillIntraday.BackColor = System.Drawing.Color.White;
			this.buttonAdvSkillIntraday.BeforeTouchSize = new System.Drawing.Size(37, 35);
			this.buttonAdvSkillIntraday.ForeColor = System.Drawing.Color.White;
			this.buttonAdvSkillIntraday.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_SkillGridIntraday;
			this.buttonAdvSkillIntraday.IsBackStageButton = false;
			this.buttonAdvSkillIntraday.Location = new System.Drawing.Point(215, 37);
			this.buttonAdvSkillIntraday.Margin = new System.Windows.Forms.Padding(1);
			this.buttonAdvSkillIntraday.Name = "buttonAdvSkillIntraday";
			this.buttonAdvSkillIntraday.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Silver;
			this.buttonAdvSkillIntraday.Office2010ColorScheme = Syncfusion.Windows.Forms.Office2010Theme.Silver;
			this.buttonAdvSkillIntraday.PushButton = true;
			this.buttonAdvSkillIntraday.ResetStateOnLostFocus = false;
			this.buttonAdvSkillIntraday.Size = new System.Drawing.Size(37, 35);
			this.buttonAdvSkillIntraday.TabIndex = 21;
			this.buttonAdvSkillIntraday.UseVisualStyle = true;
			this.buttonAdvSkillIntraday.UseVisualStyleBackColor = true;
			this.buttonAdvSkillIntraday.Click += new System.EventHandler(this.SkillZoomButtonClicked);
			// 
			// buttonAdvWorkloadDay
			// 
			this.buttonAdvWorkloadDay.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.buttonAdvWorkloadDay.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvWorkloadDay.BackColor = System.Drawing.Color.White;
			this.buttonAdvWorkloadDay.BeforeTouchSize = new System.Drawing.Size(37, 34);
			this.buttonAdvWorkloadDay.Cursor = System.Windows.Forms.Cursors.Default;
			this.buttonAdvWorkloadDay.ForeColor = System.Drawing.Color.White;
			this.buttonAdvWorkloadDay.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_WorkloadGridDay;
			this.buttonAdvWorkloadDay.IsBackStageButton = false;
			this.buttonAdvWorkloadDay.Location = new System.Drawing.Point(168, 1);
			this.buttonAdvWorkloadDay.Margin = new System.Windows.Forms.Padding(1);
			this.buttonAdvWorkloadDay.Name = "buttonAdvWorkloadDay";
			this.buttonAdvWorkloadDay.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Silver;
			this.buttonAdvWorkloadDay.Office2010ColorScheme = Syncfusion.Windows.Forms.Office2010Theme.Silver;
			this.buttonAdvWorkloadDay.PushButton = true;
			this.buttonAdvWorkloadDay.ResetStateOnLostFocus = false;
			this.buttonAdvWorkloadDay.Size = new System.Drawing.Size(37, 34);
			this.buttonAdvWorkloadDay.TabIndex = 18;
			this.buttonAdvWorkloadDay.UseVisualStyle = true;
			this.buttonAdvWorkloadDay.UseVisualStyleBackColor = true;
			this.buttonAdvWorkloadDay.Click += new System.EventHandler(this.WorkloadZoomButtonClicked);
			// 
			// buttonAdvSkillDay
			// 
			this.buttonAdvSkillDay.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.buttonAdvSkillDay.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvSkillDay.BackColor = System.Drawing.Color.White;
			this.buttonAdvSkillDay.BeforeTouchSize = new System.Drawing.Size(37, 35);
			this.buttonAdvSkillDay.ForeColor = System.Drawing.Color.White;
			this.buttonAdvSkillDay.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_SkillGridDay;
			this.buttonAdvSkillDay.IsBackStageButton = false;
			this.buttonAdvSkillDay.Location = new System.Drawing.Point(168, 37);
			this.buttonAdvSkillDay.Margin = new System.Windows.Forms.Padding(1);
			this.buttonAdvSkillDay.Name = "buttonAdvSkillDay";
			this.buttonAdvSkillDay.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Silver;
			this.buttonAdvSkillDay.Office2010ColorScheme = Syncfusion.Windows.Forms.Office2010Theme.Silver;
			this.buttonAdvSkillDay.PushButton = true;
			this.buttonAdvSkillDay.ResetStateOnLostFocus = false;
			this.buttonAdvSkillDay.Size = new System.Drawing.Size(37, 35);
			this.buttonAdvSkillDay.TabIndex = 22;
			this.buttonAdvSkillDay.UseVisualStyle = true;
			this.buttonAdvSkillDay.UseVisualStyleBackColor = true;
			this.buttonAdvSkillDay.Click += new System.EventHandler(this.SkillZoomButtonClicked);
			// 
			// buttonAdvWorkloadMonth
			// 
			this.buttonAdvWorkloadMonth.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.buttonAdvWorkloadMonth.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvWorkloadMonth.BackColor = System.Drawing.Color.White;
			this.buttonAdvWorkloadMonth.BeforeTouchSize = new System.Drawing.Size(37, 34);
			this.buttonAdvWorkloadMonth.ForeColor = System.Drawing.Color.White;
			this.buttonAdvWorkloadMonth.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_WorkloadGridMonth;
			this.buttonAdvWorkloadMonth.IsBackStageButton = false;
			this.buttonAdvWorkloadMonth.Location = new System.Drawing.Point(80, 1);
			this.buttonAdvWorkloadMonth.Margin = new System.Windows.Forms.Padding(1);
			this.buttonAdvWorkloadMonth.Name = "buttonAdvWorkloadMonth";
			this.buttonAdvWorkloadMonth.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Silver;
			this.buttonAdvWorkloadMonth.Office2010ColorScheme = Syncfusion.Windows.Forms.Office2010Theme.Silver;
			this.buttonAdvWorkloadMonth.PushButton = true;
			this.buttonAdvWorkloadMonth.ResetStateOnLostFocus = false;
			this.buttonAdvWorkloadMonth.Size = new System.Drawing.Size(37, 34);
			this.buttonAdvWorkloadMonth.TabIndex = 14;
			this.buttonAdvWorkloadMonth.UseVisualStyle = true;
			this.buttonAdvWorkloadMonth.UseVisualStyleBackColor = true;
			this.buttonAdvWorkloadMonth.Click += new System.EventHandler(this.WorkloadZoomButtonClicked);
			// 
			// buttonAvdWorkloadWeek
			// 
			this.buttonAvdWorkloadWeek.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.buttonAvdWorkloadWeek.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAvdWorkloadWeek.BackColor = System.Drawing.Color.White;
			this.buttonAvdWorkloadWeek.BeforeTouchSize = new System.Drawing.Size(37, 34);
			this.buttonAvdWorkloadWeek.ForeColor = System.Drawing.Color.White;
			this.buttonAvdWorkloadWeek.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_WorkloadGridWeek;
			this.buttonAvdWorkloadWeek.IsBackStageButton = false;
			this.buttonAvdWorkloadWeek.Location = new System.Drawing.Point(124, 1);
			this.buttonAvdWorkloadWeek.Margin = new System.Windows.Forms.Padding(1);
			this.buttonAvdWorkloadWeek.Name = "buttonAvdWorkloadWeek";
			this.buttonAvdWorkloadWeek.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Silver;
			this.buttonAvdWorkloadWeek.Office2010ColorScheme = Syncfusion.Windows.Forms.Office2010Theme.Silver;
			this.buttonAvdWorkloadWeek.PushButton = true;
			this.buttonAvdWorkloadWeek.ResetStateOnLostFocus = false;
			this.buttonAvdWorkloadWeek.Size = new System.Drawing.Size(37, 34);
			this.buttonAvdWorkloadWeek.TabIndex = 12;
			this.buttonAvdWorkloadWeek.UseVisualStyle = true;
			this.buttonAvdWorkloadWeek.UseVisualStyleBackColor = true;
			this.buttonAvdWorkloadWeek.Click += new System.EventHandler(this.WorkloadZoomButtonClicked);
			// 
			// ZoomButtons
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Transparent;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "ZoomButtons";
			this.Size = new System.Drawing.Size(253, 73);
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
