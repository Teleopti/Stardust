namespace Teleopti.Ccc.AgentPortal.AgentScheduleMessenger
{
    partial class NextActivityControl
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
            if (disposing)
            {
                UnhookEvents();
                if (components != null)
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
            this.gradientLabelNextActivity = new Syncfusion.Windows.Forms.Tools.GradientLabel();
            this.SuspendLayout();
            // 
            // gradientLabelNextActivity
            // 
            this.gradientLabelNextActivity.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.Transparent);
            this.gradientLabelNextActivity.BorderSides = System.Windows.Forms.Border3DSide.Bottom;
            this.gradientLabelNextActivity.BorderStyle = System.Windows.Forms.Border3DStyle.Adjust;
            this.gradientLabelNextActivity.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gradientLabelNextActivity.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gradientLabelNextActivity.ForeColor = System.Drawing.Color.MidnightBlue;
            this.gradientLabelNextActivity.Location = new System.Drawing.Point(0, 0);
            this.gradientLabelNextActivity.Margin = new System.Windows.Forms.Padding(0);
            this.gradientLabelNextActivity.Name = "gradientLabelNextActivity";
            this.gradientLabelNextActivity.Padding = new System.Windows.Forms.Padding(3);
            this.gradientLabelNextActivity.Size = new System.Drawing.Size(122, 115);
            this.gradientLabelNextActivity.TabIndex = 34;
            this.gradientLabelNextActivity.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.gradientLabelNextActivity.UseMnemonic = false;
            // 
            // NextActivityControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.gradientLabelNextActivity);
            this.Name = "NextActivityControl";
            this.Size = new System.Drawing.Size(122, 115);
            this.Load += new System.EventHandler(this.NextActivityControl_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.GradientLabel gradientLabelNextActivity;
    }
}
