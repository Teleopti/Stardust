namespace Teleopti.Ccc.Win.Forecasting.Forms.WFControls
{
    partial class WFValidate
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
                ReleaseManagedResources();
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
			this.workflowValidateView = new Teleopti.Ccc.Win.Forecasting.Forms.WFControls.WorkflowValidateView();
			this.SuspendLayout();
			// 
			// workflowValidateView
			// 
			this.workflowValidateView.BackColor = System.Drawing.Color.White;
			this.workflowValidateView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.workflowValidateView.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.workflowValidateView.Location = new System.Drawing.Point(0, 0);
			this.workflowValidateView.Name = "workflowValidateView";
			this.workflowValidateView.Size = new System.Drawing.Size(1060, 600);
			this.workflowValidateView.TabIndex = 0;
			// 
			// WFValidate
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.workflowValidateView);
			this.Name = "WFValidate";
			this.Size = new System.Drawing.Size(1060, 600);
			this.ResumeLayout(false);

        }

        #endregion

        private WorkflowValidateView workflowValidateView;
    }
}
