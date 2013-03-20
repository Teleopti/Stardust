namespace Teleopti.Ccc.Win.Common.PropertyPageAndWizard
{
    partial class WizardWelcome
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
			this.labelWelcome = new System.Windows.Forms.Label();
			this.labelDescription = new System.Windows.Forms.Label();
			this.checkBoxUseWizard = new System.Windows.Forms.CheckBox();
			this.pictureBoxTeleoptiLogotype = new System.Windows.Forms.PictureBox();
			this.panelWelcome = new System.Windows.Forms.Panel();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxTeleoptiLogotype)).BeginInit();
			this.panelWelcome.SuspendLayout();
			this.SuspendLayout();
			// 
			// labelWelcome
			// 
			this.labelWelcome.AutoSize = true;
			this.labelWelcome.Font = new System.Drawing.Font("Segoe UI", 14F);
			this.labelWelcome.Location = new System.Drawing.Point(15, 12);
			this.labelWelcome.Name = "labelWelcome";
			this.labelWelcome.Size = new System.Drawing.Size(178, 25);
			this.labelWelcome.TabIndex = 0;
			this.labelWelcome.Text = "Welcome to Wizard";
			// 
			// labelDescription
			// 
			this.labelDescription.AutoSize = true;
			this.labelDescription.Location = new System.Drawing.Point(15, 57);
			this.labelDescription.Name = "labelDescription";
			this.labelDescription.Size = new System.Drawing.Size(157, 15);
			this.labelDescription.TabIndex = 1;
			this.labelDescription.Text = "This Wizard helps you with...";
			// 
			// checkBoxUseWizard
			// 
			this.checkBoxUseWizard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.checkBoxUseWizard.AutoSize = true;
			this.checkBoxUseWizard.Checked = true;
			this.checkBoxUseWizard.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxUseWizard.Location = new System.Drawing.Point(15, 173);
			this.checkBoxUseWizard.Name = "checkBoxUseWizard";
			this.checkBoxUseWizard.Size = new System.Drawing.Size(91, 19);
			this.checkBoxUseWizard.TabIndex = 2;
			this.checkBoxUseWizard.Text = "xxUseWizard";
			this.checkBoxUseWizard.UseVisualStyleBackColor = true;
			this.checkBoxUseWizard.Visible = false;
			// 
			// pictureBoxTeleoptiLogotype
			// 
			this.pictureBoxTeleoptiLogotype.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.pictureBoxTeleoptiLogotype.Location = new System.Drawing.Point(205, 152);
			this.pictureBoxTeleoptiLogotype.Name = "pictureBoxTeleoptiLogotype";
			this.pictureBoxTeleoptiLogotype.Size = new System.Drawing.Size(105, 39);
			this.pictureBoxTeleoptiLogotype.TabIndex = 3;
			this.pictureBoxTeleoptiLogotype.TabStop = false;
			// 
			// panelWelcome
			// 
			this.panelWelcome.BackColor = System.Drawing.Color.White;
			this.panelWelcome.Controls.Add(this.labelWelcome);
			this.panelWelcome.Controls.Add(this.checkBoxUseWizard);
			this.panelWelcome.Controls.Add(this.pictureBoxTeleoptiLogotype);
			this.panelWelcome.Controls.Add(this.labelDescription);
			this.panelWelcome.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelWelcome.Location = new System.Drawing.Point(12, 12);
			this.panelWelcome.Name = "panelWelcome";
			this.panelWelcome.Padding = new System.Windows.Forms.Padding(12, 12, 12, 12);
			this.panelWelcome.Size = new System.Drawing.Size(326, 207);
			this.panelWelcome.TabIndex = 4;
			// 
			// WizardWelcome
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.Controls.Add(this.panelWelcome);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.Margin = new System.Windows.Forms.Padding(12, 12, 12, 12);
			this.Name = "WizardWelcome";
			this.Padding = new System.Windows.Forms.Padding(12, 12, 12, 12);
			this.Size = new System.Drawing.Size(350, 231);
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxTeleoptiLogotype)).EndInit();
			this.panelWelcome.ResumeLayout(false);
			this.panelWelcome.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelWelcome;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.CheckBox checkBoxUseWizard;
        private System.Windows.Forms.PictureBox pictureBoxTeleoptiLogotype;
        private System.Windows.Forms.Panel panelWelcome;
    }
}