namespace Teleopti.Ccc.Win.Common
{
    partial class TimePeriodBox
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
            this.textBoxTimePeriod = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // textBoxTimePeriod
            // 
            this.textBoxTimePeriod.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxTimePeriod.Location = new System.Drawing.Point(0, 0);
            this.textBoxTimePeriod.Name = "textBoxTimePeriod";
            this.textBoxTimePeriod.Size = new System.Drawing.Size(171, 20);
            this.textBoxTimePeriod.TabIndex = 0;
            this.textBoxTimePeriod.TextChanged += new System.EventHandler(this.textBoxTimePeriod_TextChanged);
            this.textBoxTimePeriod.Validated += new System.EventHandler(this.textBoxTimePeriod_Validated);
            // 
            // TimePeriodBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.textBoxTimePeriod);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "TimePeriodBox";
            this.Size = new System.Drawing.Size(171, 20);
            this.Resize += new System.EventHandler(this.TimePeriodBox_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxTimePeriod;
    }
}
