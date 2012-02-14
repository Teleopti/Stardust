namespace Teleopti.Support.Tool.Controls.General
{
    partial class ProgressItem
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
       // private System.ComponentModel.IContainer components = null;

       

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.subtextLabel = new System.Windows.Forms.Label();
            this.titleLabel = new System.Windows.Forms.Label();
            this.runStep = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::Teleopti.Support.Tool.Properties.Resources.warning;
            this.pictureBox1.Location = new System.Drawing.Point(20, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(24, 16);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            // 
            // subtextLabel
            // 
            this.subtextLabel.AutoSize = true;
            this.subtextLabel.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.subtextLabel.Location = new System.Drawing.Point(3, 41);
            this.subtextLabel.Name = "subtextLabel";
            this.subtextLabel.Size = new System.Drawing.Size(0, 13);
            this.subtextLabel.TabIndex = 2;
            // 
            // titleLabel
            // 
            this.titleLabel.AutoSize = true;
            this.titleLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.titleLabel.Location = new System.Drawing.Point(49, 5);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(74, 13);
            this.titleLabel.TabIndex = 1;
            this.titleLabel.Text = "Executing....";
            // 
            // runStep
            // 
            this.runStep.AutoSize = true;
            this.runStep.Checked = true;
            this.runStep.CheckState = System.Windows.Forms.CheckState.Checked;
            this.runStep.Location = new System.Drawing.Point(4, 4);
            this.runStep.Name = "runStep";
            this.runStep.Size = new System.Drawing.Size(15, 14);
            this.runStep.TabIndex = 4;
            this.runStep.UseVisualStyleBackColor = true;
            // 
            // ProgressItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.runStep);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.subtextLabel);
            this.Controls.Add(this.titleLabel);
            this.Margin = new System.Windows.Forms.Padding(5, 1, 5, 1);
            this.Name = "ProgressItem";
            this.Size = new System.Drawing.Size(676, 26);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label subtextLabel;
        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.CheckBox runStep;

    }
}
