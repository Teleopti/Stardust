namespace Teleopti.Ccc.Win.Forecasting.Forms
{
    partial class CopyToSkillView
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
			this.labelSource = new System.Windows.Forms.Label();
			this.labelSourceWorkload = new System.Windows.Forms.Label();
			this.labelTargetSkill = new System.Windows.Forms.Label();
			this.checkBoxAdvIncludeTemplates = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.checkBoxAdvIncludeQueues = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.comboBoxAdvSkills = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.tableLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvIncludeTemplates)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvIncludeQueues)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvSkills)).BeginInit();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 53.73832F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 46.26168F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 155F));
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 5);
			this.tableLayoutPanel1.Controls.Add(this.labelSource, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.labelSourceWorkload, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.labelTargetSkill, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.checkBoxAdvIncludeTemplates, 1, 3);
			this.tableLayoutPanel1.Controls.Add(this.checkBoxAdvIncludeQueues, 1, 4);
			this.tableLayoutPanel1.Controls.Add(this.comboBoxAdvSkills, 1, 2);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 6;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(488, 228);
			this.tableLayoutPanel1.TabIndex = 0;
			this.tableLayoutPanel1.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanel1_Paint);
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanel2.ColumnCount = 3;
			this.tableLayoutPanel1.SetColumnSpan(this.tableLayoutPanel2, 3);
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 105F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 105F));
			this.tableLayoutPanel2.Controls.Add(this.buttonAdvOk, 1, 0);
			this.tableLayoutPanel2.Controls.Add(this.buttonAdvCancel, 2, 0);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 178);
			this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 1;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(488, 50);
			this.tableLayoutPanel2.TabIndex = 3;
			this.tableLayoutPanel2.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanel2_Paint);
			// 
			// buttonAdvOk
			// 
			this.buttonAdvOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvOk.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvOk.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvOk.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonAdvOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonAdvOk.ForeColor = System.Drawing.Color.White;
			this.buttonAdvOk.IsBackStageButton = false;
			this.buttonAdvOk.Location = new System.Drawing.Point(286, 13);
			this.buttonAdvOk.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonAdvOk.Name = "buttonAdvOk";
			this.buttonAdvOk.Size = new System.Drawing.Size(87, 27);
			this.buttonAdvOk.TabIndex = 6;
			this.buttonAdvOk.Text = "xxOk";
			this.buttonAdvOk.UseVisualStyle = true;
			this.buttonAdvOk.Click += new System.EventHandler(this.buttonAdvOk_Click);
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
			this.buttonAdvCancel.Location = new System.Drawing.Point(391, 13);
			this.buttonAdvCancel.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonAdvCancel.Name = "buttonAdvCancel";
			this.buttonAdvCancel.Size = new System.Drawing.Size(87, 27);
			this.buttonAdvCancel.TabIndex = 7;
			this.buttonAdvCancel.Text = "xxCancel";
			this.buttonAdvCancel.UseVisualStyle = true;
			this.buttonAdvCancel.Click += new System.EventHandler(this.buttonAdvCancel_Click);
			// 
			// labelSource
			// 
			this.labelSource.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelSource.AutoSize = true;
			this.labelSource.Location = new System.Drawing.Point(3, 30);
			this.labelSource.Name = "labelSource";
			this.labelSource.Size = new System.Drawing.Size(85, 15);
			this.labelSource.TabIndex = 0;
			this.labelSource.Text = "xxSourceColon";
			// 
			// labelSourceWorkload
			// 
			this.labelSourceWorkload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.labelSourceWorkload.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.labelSourceWorkload, 2);
			this.labelSourceWorkload.Location = new System.Drawing.Point(181, 30);
			this.labelSourceWorkload.Name = "labelSourceWorkload";
			this.labelSourceWorkload.Size = new System.Drawing.Size(304, 15);
			this.labelSourceWorkload.TabIndex = 1;
			this.labelSourceWorkload.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelTargetSkill
			// 
			this.labelTargetSkill.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelTargetSkill.AutoSize = true;
			this.labelTargetSkill.Location = new System.Drawing.Point(3, 70);
			this.labelTargetSkill.Name = "labelTargetSkill";
			this.labelTargetSkill.Size = new System.Drawing.Size(104, 15);
			this.labelTargetSkill.TabIndex = 2;
			this.labelTargetSkill.Text = "xxTargetSkillColon";
			// 
			// checkBoxAdvIncludeTemplates
			// 
			this.checkBoxAdvIncludeTemplates.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxAdvIncludeTemplates.BeforeTouchSize = new System.Drawing.Size(304, 23);
			this.tableLayoutPanel1.SetColumnSpan(this.checkBoxAdvIncludeTemplates, 2);
			this.checkBoxAdvIncludeTemplates.DrawFocusRectangle = false;
			this.checkBoxAdvIncludeTemplates.Location = new System.Drawing.Point(181, 106);
			this.checkBoxAdvIncludeTemplates.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxAdvIncludeTemplates.Name = "checkBoxAdvIncludeTemplates";
			this.checkBoxAdvIncludeTemplates.Size = new System.Drawing.Size(304, 23);
			this.checkBoxAdvIncludeTemplates.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Metro;
			this.checkBoxAdvIncludeTemplates.TabIndex = 4;
			this.checkBoxAdvIncludeTemplates.Text = "xxIncludeTemplates";
			this.checkBoxAdvIncludeTemplates.ThemesEnabled = false;
			this.checkBoxAdvIncludeTemplates.CheckedChanged += new System.EventHandler(this.checkBoxAdvIncludeTemplates_CheckedChanged);
			// 
			// checkBoxAdvIncludeQueues
			// 
			this.checkBoxAdvIncludeQueues.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxAdvIncludeQueues.BeforeTouchSize = new System.Drawing.Size(304, 21);
			this.tableLayoutPanel1.SetColumnSpan(this.checkBoxAdvIncludeQueues, 2);
			this.checkBoxAdvIncludeQueues.DrawFocusRectangle = false;
			this.checkBoxAdvIncludeQueues.Location = new System.Drawing.Point(181, 147);
			this.checkBoxAdvIncludeQueues.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxAdvIncludeQueues.Name = "checkBoxAdvIncludeQueues";
			this.checkBoxAdvIncludeQueues.Size = new System.Drawing.Size(304, 21);
			this.checkBoxAdvIncludeQueues.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Metro;
			this.checkBoxAdvIncludeQueues.TabIndex = 5;
			this.checkBoxAdvIncludeQueues.Text = "xxIncludeQueues";
			this.checkBoxAdvIncludeQueues.ThemesEnabled = false;
			this.checkBoxAdvIncludeQueues.CheckStateChanged += new System.EventHandler(this.checkBoxAdvIncludeQueues_CheckStateChanged);
			// 
			// comboBoxAdvSkills
			// 
			this.comboBoxAdvSkills.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.comboBoxAdvSkills.BackColor = System.Drawing.Color.White;
			this.comboBoxAdvSkills.BeforeTouchSize = new System.Drawing.Size(304, 23);
			this.tableLayoutPanel1.SetColumnSpan(this.comboBoxAdvSkills, 2);
			this.comboBoxAdvSkills.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAdvSkills.Location = new System.Drawing.Point(181, 67);
			this.comboBoxAdvSkills.Name = "comboBoxAdvSkills";
			this.comboBoxAdvSkills.Size = new System.Drawing.Size(304, 23);
			this.comboBoxAdvSkills.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.comboBoxAdvSkills.TabIndex = 1;
			this.comboBoxAdvSkills.SelectedIndexChanged += new System.EventHandler(this.comboBoxAdvSkills_SelectedIndexChanged);
			// 
			// CopyToSkillView
			// 
			this.AcceptButton = this.buttonAdvOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.CancelButton = this.buttonAdvCancel;
			this.CaptionFont = new System.Drawing.Font("Segoe UI", 12F);
			this.ClientSize = new System.Drawing.Size(488, 228);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.HelpButton = false;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "CopyToSkillView";
			this.ShowIcon = false;
			this.Text = "xxCopyToThreeDots";
			this.Load += new System.EventHandler(this.CopyToSkillView_Load);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.tableLayoutPanel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvIncludeTemplates)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvIncludeQueues)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvSkills)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label labelSource;
        private System.Windows.Forms.Label labelSourceWorkload;
        private System.Windows.Forms.Label labelTargetSkill;
        private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdvIncludeTemplates;
        private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdvIncludeQueues;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvOk;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvCancel;
		private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvSkills;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
    }
}