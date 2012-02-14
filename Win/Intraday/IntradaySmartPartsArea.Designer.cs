namespace Teleopti.Ccc.Win.Intraday
{
    partial class IntradaySmartPartsArea
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
            this.gridWorkspace1 = new Teleopti.Common.UI.SmartPartControls.SmartParts.GridWorkspace();
            this.SuspendLayout();
            // 
            // gridWorkspace1
            // 
            this.gridWorkspace1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridWorkspace1.GridSize = Teleopti.Common.UI.SmartPartControls.SmartParts.GridSizeType.TwoByOne;
            this.gridWorkspace1.Location = new System.Drawing.Point(0, 0);
            this.gridWorkspace1.Name = "gridWorkspace1";
            this.gridWorkspace1.Size = new System.Drawing.Size(699, 596);
            this.gridWorkspace1.TabIndex = 0;
            this.gridWorkspace1.Tag = "0";
            // 
            // IntraDaySmartpartsArea
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gridWorkspace1);
            this.Name = "IntraDaySmartpartsArea";
            this.Size = new System.Drawing.Size(699, 596);
            this.ResumeLayout(false);

        }

        #endregion

        private Teleopti.Common.UI.SmartPartControls.SmartParts.GridWorkspace gridWorkspace1;

    }
}
