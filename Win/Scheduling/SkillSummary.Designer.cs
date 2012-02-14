namespace Teleopti.Ccc.Win.Scheduling
{
    partial class SkillSummary
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
			this.buttonAdvOk = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.labelSkill = new System.Windows.Forms.Label();
			this.labelSummeryName = new System.Windows.Forms.Label();
			this.checkedListBoxSkill = new System.Windows.Forms.CheckedListBox();
			this.textBoxSummeryName = new System.Windows.Forms.TextBox();
			this.ribbonControlAdv1 = new Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed();
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).BeginInit();
			this.SuspendLayout();
			// 
			// buttonAdvOk
			// 
			this.buttonAdvOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvOk.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.buttonAdvOk.Location = new System.Drawing.Point(239, 301);
			this.buttonAdvOk.Name = "buttonAdvOk";
			this.buttonAdvOk.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
			this.buttonAdvOk.Size = new System.Drawing.Size(75, 23);
			this.buttonAdvOk.TabIndex = 3;
			this.buttonAdvOk.Text = "xxOk";
			this.buttonAdvOk.UseVisualStyle = true;
			this.buttonAdvOk.Click += new System.EventHandler(this.buttonAdvOk_Click);
			// 
			// buttonAdvCancel
			// 
			this.buttonAdvCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.buttonAdvCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonAdvCancel.Location = new System.Drawing.Point(320, 301);
			this.buttonAdvCancel.Name = "buttonAdvCancel";
			this.buttonAdvCancel.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
			this.buttonAdvCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonAdvCancel.TabIndex = 4;
			this.buttonAdvCancel.Text = "xxCancel";
			this.buttonAdvCancel.UseVisualStyle = true;
			// 
			// labelSkill
			// 
			this.labelSkill.AutoSize = true;
			this.labelSkill.Location = new System.Drawing.Point(18, 128);
			this.labelSkill.Name = "labelSkill";
			this.labelSkill.Size = new System.Drawing.Size(41, 13);
			this.labelSkill.TabIndex = 6;
			this.labelSkill.Text = "xxSkills";
			// 
			// labelSummeryName
			// 
			this.labelSummeryName.AutoSize = true;
			this.labelSummeryName.Location = new System.Drawing.Point(14, 86);
			this.labelSummeryName.Name = "labelSummeryName";
			this.labelSummeryName.Size = new System.Drawing.Size(88, 13);
			this.labelSummeryName.TabIndex = 5;
			this.labelSummeryName.Text = "xxSummeryName";
			// 
			// checkedListBoxSkill
			// 
			this.checkedListBoxSkill.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.checkedListBoxSkill.CheckOnClick = true;
			this.checkedListBoxSkill.FormattingEnabled = true;
			this.checkedListBoxSkill.Location = new System.Drawing.Point(143, 126);
			this.checkedListBoxSkill.Name = "checkedListBoxSkill";
			this.checkedListBoxSkill.Size = new System.Drawing.Size(252, 154);
			this.checkedListBoxSkill.TabIndex = 2;
			// 
			// textBoxSummeryName
			// 
			this.textBoxSummeryName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxSummeryName.Location = new System.Drawing.Point(143, 81);
			this.textBoxSummeryName.Name = "textBoxSummeryName";
			this.textBoxSummeryName.Size = new System.Drawing.Size(252, 20);
			this.textBoxSummeryName.TabIndex = 1;
			// 
			// ribbonControlAdv1
			// 
			this.ribbonControlAdv1.Location = new System.Drawing.Point(1, 0);
			this.ribbonControlAdv1.MenuButtonVisible = false;
			this.ribbonControlAdv1.Name = "ribbonControlAdv1";
			// 
			// ribbonControlAdv1.OfficeMenu
			// 
			this.ribbonControlAdv1.OfficeMenu.Name = "OfficeMenu";
			this.ribbonControlAdv1.OfficeMenu.Size = new System.Drawing.Size(12, 65);
			this.ribbonControlAdv1.QuickPanelVisible = false;
			this.ribbonControlAdv1.ShowLauncher = false;
			this.ribbonControlAdv1.Size = new System.Drawing.Size(409, 50);
			this.ribbonControlAdv1.SystemText.QuickAccessDialogDropDownName = "xxStart menu";
			this.ribbonControlAdv1.TabIndex = 0;
			this.ribbonControlAdv1.Text = "xxSkillSummery";
			// 
			// SkillSummery
			// 
			this.AcceptButton = this.buttonAdvOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.buttonAdvCancel;
			this.ClientSize = new System.Drawing.Size(411, 340);
			this.Controls.Add(this.labelSkill);
			this.Controls.Add(this.labelSummeryName);
			this.Controls.Add(this.buttonAdvCancel);
			this.Controls.Add(this.buttonAdvOk);
			this.Controls.Add(this.checkedListBoxSkill);
			this.Controls.Add(this.textBoxSummeryName);
			this.Controls.Add(this.ribbonControlAdv1);
			this.Name = "SkillSummary";
			this.Text = "xxSkillSummery";
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed ribbonControlAdv1;
        private System.Windows.Forms.TextBox textBoxSummeryName;
        private System.Windows.Forms.CheckedListBox checkedListBoxSkill;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvOk;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvCancel;
        private System.Windows.Forms.Label labelSummeryName;
        private System.Windows.Forms.Label labelSkill;
    }
}