using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Forecasting.Forms.ExportPages
{
    partial class SelectDestination
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
                foreach (GridBoundColumn gridBoundColumn in gridControlDestination.GridBoundColumns)
                {
                    gridBoundColumn.Dispose();
                }
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
            this.gridControlDestination = new Syncfusion.Windows.Forms.Grid.GridDataBoundGrid();
            ((System.ComponentModel.ISupportInitialize)(this.gridControlDestination)).BeginInit();
            this.SuspendLayout();
            // 
            // gridControlDestination
            // 
            this.gridControlDestination.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControlDestination.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Office2007Blue;
            this.gridControlDestination.Location = new System.Drawing.Point(0, 0);
            this.gridControlDestination.Name = "gridControlDestination";
            this.gridControlDestination.Size = new System.Drawing.Size(294, 242);
            this.gridControlDestination.SmartSizeBox = false;
            this.gridControlDestination.TabIndex = 0;
            this.gridControlDestination.Text = "gridControlDestination";
            this.gridControlDestination.ThemesEnabled = true;
            this.gridControlDestination.UseRightToLeftCompatibleTextBox = true;
            // 
            // SelectDestination
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gridControlDestination);
            this.Name = "SelectDestination";
            this.Size = new System.Drawing.Size(294, 242);
            ((System.ComponentModel.ISupportInitialize)(this.gridControlDestination)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Grid.GridDataBoundGrid gridControlDestination;
    }
}
