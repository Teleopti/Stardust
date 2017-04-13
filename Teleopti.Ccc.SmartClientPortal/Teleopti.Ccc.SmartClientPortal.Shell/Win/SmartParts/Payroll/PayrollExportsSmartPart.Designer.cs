namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.SmartParts.Payroll
{
    partial class PayrollExportsSmartPart
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
                UnregisterEvents();

                if (components != null)
                    components.Dispose();
                if (_model != null)
                {
                    _model.Dispose();
                    _model = null;
                }
                if (_payrollResults!=null)
                    _payrollResults.Clear();
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
            this.SuspendLayout();
            // 
            // PayrollExportsSmartPart
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.DoubleBuffered = true;
            this.Name = "PayrollExportsSmartPart";
            this.Size = new System.Drawing.Size(687, 659);
            this.SmartPartHeaderTitle = "xxPayrollExports";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

    }
}
