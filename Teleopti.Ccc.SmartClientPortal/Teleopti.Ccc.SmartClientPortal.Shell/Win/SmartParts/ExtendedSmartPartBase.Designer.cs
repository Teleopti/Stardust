namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.SmartParts
{
    partial class ExtendedSmartPartBase
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
                if (components != null)
                    components.Dispose();
                if (HostContainer!=null)
                {
                    HostContainer.Dispose();
                }
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
            this.HostContainer = new System.Windows.Forms.Integration.ElementHost();
            this.SuspendLayout();
            // 
            // HostContainer
            // 
            this.HostContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.HostContainer.Location = new System.Drawing.Point(0, 25);
            this.HostContainer.Name = "HostContainer";
            this.HostContainer.Size = new System.Drawing.Size(401, 223);
            this.HostContainer.TabIndex = 17;
            this.HostContainer.Child = null;
            // 
            // ExtendedSmartPartBase
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.HostContainer);
            this.Name = "ExtendedSmartPartBase";
            this.Controls.SetChildIndex(this.HostContainer, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Integration.ElementHost HostContainer;
    }
}
