
namespace Teleopti.Ccc.Win.Forecasting.Forms.SkillPages
{
    partial class MultisiteChildSkills
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
			this.panelMain = new System.Windows.Forms.Panel();
			this.buttonAdvRename = new Syncfusion.Windows.Forms.ButtonAdv();
			this.listBoxChildSkills = new System.Windows.Forms.ListBox();
			this.buttonAdvManageDayTemplates = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvRemove = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvAdd = new Syncfusion.Windows.Forms.ButtonAdv();
			this.panelMain.SuspendLayout();
			this.SuspendLayout();
			// 
			// panelMain
			// 
			this.panelMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.panelMain.Controls.Add(this.buttonAdvRename);
			this.panelMain.Controls.Add(this.listBoxChildSkills);
			this.panelMain.Controls.Add(this.buttonAdvManageDayTemplates);
			this.panelMain.Controls.Add(this.buttonAdvRemove);
			this.panelMain.Controls.Add(this.buttonAdvAdd);
			this.panelMain.Location = new System.Drawing.Point(10, 10);
			this.panelMain.Margin = new System.Windows.Forms.Padding(0);
			this.panelMain.Name = "panelMain";
			this.panelMain.Size = new System.Drawing.Size(364, 280);
			this.panelMain.TabIndex = 13;
			// 
			// buttonAdvRename
			// 
			this.buttonAdvRename.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.buttonAdvRename.AutoSize = true;
			this.buttonAdvRename.Location = new System.Drawing.Point(165, 254);
			this.buttonAdvRename.Name = "buttonAdvRename";
			this.buttonAdvRename.Size = new System.Drawing.Size(67, 23);
			this.buttonAdvRename.TabIndex = 3;
			this.buttonAdvRename.Text = "xxRename";
			this.buttonAdvRename.UseVisualStyle = true;
			this.buttonAdvRename.Click += new System.EventHandler(this.buttonAdvRename_Click);
			// 
			// listBoxChildSkills
			// 
			this.listBoxChildSkills.FormattingEnabled = true;
			this.listBoxChildSkills.Location = new System.Drawing.Point(3, 10);
			this.listBoxChildSkills.Name = "listBoxChildSkills";
			this.listBoxChildSkills.Size = new System.Drawing.Size(361, 238);
			this.listBoxChildSkills.TabIndex = 0;
			this.listBoxChildSkills.DoubleClick += new System.EventHandler(this.listBoxChildSkills_DoubleClick);
			// 
			// buttonAdvManageDayTemplates
			// 
			this.buttonAdvManageDayTemplates.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.buttonAdvManageDayTemplates.AutoSize = true;
			this.buttonAdvManageDayTemplates.Location = new System.Drawing.Point(238, 254);
			this.buttonAdvManageDayTemplates.Name = "buttonAdvManageDayTemplates";
			this.buttonAdvManageDayTemplates.Size = new System.Drawing.Size(126, 23);
			this.buttonAdvManageDayTemplates.TabIndex = 4;
			this.buttonAdvManageDayTemplates.Text = "xxTemplatesThreeDots";
			this.buttonAdvManageDayTemplates.UseVisualStyle = true;
			this.buttonAdvManageDayTemplates.Click += new System.EventHandler(this.buttonAdvManageDayTemplates_Click);
			// 
			// buttonAdvRemove
			// 
			this.buttonAdvRemove.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.buttonAdvRemove.AutoSize = true;
			this.buttonAdvRemove.Location = new System.Drawing.Point(84, 254);
			this.buttonAdvRemove.Name = "buttonAdvRemove";
			this.buttonAdvRemove.Size = new System.Drawing.Size(75, 23);
			this.buttonAdvRemove.TabIndex = 2;
			this.buttonAdvRemove.Text = "xxDelete";
			this.buttonAdvRemove.UseVisualStyle = true;
			this.buttonAdvRemove.Click += new System.EventHandler(this.buttonAdvRemove_Click);
			// 
			// buttonAdvAdd
			// 
			this.buttonAdvAdd.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.buttonAdvAdd.AutoSize = true;
			this.buttonAdvAdd.Location = new System.Drawing.Point(3, 254);
			this.buttonAdvAdd.Name = "buttonAdvAdd";
			this.buttonAdvAdd.Size = new System.Drawing.Size(75, 23);
			this.buttonAdvAdd.TabIndex = 1;
			this.buttonAdvAdd.Text = "xxNew";
			this.buttonAdvAdd.UseVisualStyle = true;
			this.buttonAdvAdd.Click += new System.EventHandler(this.buttonAdvAdd_Click);
			// 
			// MultisiteChildSkills
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.panelMain);
			this.Name = "MultisiteChildSkills";
			this.Padding = new System.Windows.Forms.Padding(10);
			this.Size = new System.Drawing.Size(384, 300);
			this.panelMain.ResumeLayout(false);
			this.panelMain.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelMain;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvAdd;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvManageDayTemplates;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvRemove;
        private System.Windows.Forms.ListBox listBoxChildSkills;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvRename;
    }
}