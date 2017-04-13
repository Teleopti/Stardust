namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.WorkloadPages
{
    partial class WorkloadGeneral
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param _name="disposing">true if managed resources should be disposed; otherwise, false.</param>
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
			this.labelSkill = new System.Windows.Forms.Label();
			this.textBoxDescription = new System.Windows.Forms.TextBox();
			this.labelDescription = new System.Windows.Forms.Label();
			this.labelName = new System.Windows.Forms.Label();
			this.textBoxName = new System.Windows.Forms.TextBox();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.textBoxSkill = new System.Windows.Forms.TextBox();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// labelSkill
			// 
			this.labelSkill.AutoSize = true;
			this.labelSkill.Location = new System.Drawing.Point(3, 103);
			this.labelSkill.Margin = new System.Windows.Forms.Padding(3, 7, 3, 0);
			this.labelSkill.Name = "labelSkill";
			this.labelSkill.Size = new System.Drawing.Size(70, 15);
			this.labelSkill.TabIndex = 6;
			this.labelSkill.Text = "xxSkillColon";
			// 
			// textBoxDescription
			// 
			this.textBoxDescription.AcceptsReturn = true;
			this.textBoxDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxDescription.Location = new System.Drawing.Point(115, 34);
			this.textBoxDescription.MaxLength = 1023;
			this.textBoxDescription.Multiline = true;
			this.textBoxDescription.Name = "textBoxDescription";
			this.textBoxDescription.Size = new System.Drawing.Size(232, 59);
			this.textBoxDescription.TabIndex = 5;
			// 
			// labelDescription
			// 
			this.labelDescription.AutoSize = true;
			this.labelDescription.Location = new System.Drawing.Point(3, 38);
			this.labelDescription.Margin = new System.Windows.Forms.Padding(3, 7, 3, 0);
			this.labelDescription.Name = "labelDescription";
			this.labelDescription.Size = new System.Drawing.Size(102, 30);
			this.labelDescription.TabIndex = 4;
			this.labelDescription.Text = "xxDescriptionColon";
			// 
			// labelName
			// 
			this.labelName.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelName.AutoSize = true;
			this.labelName.Location = new System.Drawing.Point(3, 11);
			this.labelName.Margin = new System.Windows.Forms.Padding(3, 7, 3, 0);
			this.labelName.Name = "labelName";
			this.labelName.Size = new System.Drawing.Size(81, 15);
			this.labelName.TabIndex = 2;
			this.labelName.Text = "xxNameColon";
			// 
			// textBoxName
			// 
			this.textBoxName.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.textBoxName.Location = new System.Drawing.Point(115, 4);
			this.textBoxName.Name = "textBoxName";
			this.textBoxName.Size = new System.Drawing.Size(215, 23);
			this.textBoxName.TabIndex = 3;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 68F));
			this.tableLayoutPanel1.Controls.Add(this.labelName, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.textBoxName, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.labelSkill, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.labelDescription, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.textBoxDescription, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.textBoxSkill, 1, 2);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 31F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 31F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(350, 231);
			this.tableLayoutPanel1.TabIndex = 8;
			// 
			// textBoxSkill
			// 
			this.textBoxSkill.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxSkill.BackColor = System.Drawing.Color.White;
			this.textBoxSkill.Location = new System.Drawing.Point(115, 99);
			this.textBoxSkill.Name = "textBoxSkill";
			this.textBoxSkill.ReadOnly = true;
			this.textBoxSkill.Size = new System.Drawing.Size(232, 23);
			this.textBoxSkill.TabIndex = 3;
			// 
			// WorkloadGeneral
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.Name = "WorkloadGeneral";
			this.Size = new System.Drawing.Size(350, 231);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.TextBox textBoxDescription;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.Label labelSkill;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TextBox textBoxSkill;
    }
}