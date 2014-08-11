namespace Teleopti.Ccc.Win.Payroll.Forms.PayrollExportPages
{
    partial class DateTimePeriodSelection
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
            this.dateSelectionControlPeriod = new Teleopti.Ccc.Win.Common.Controls.DateSelection.DateSelectionControl();
            this.SuspendLayout();
            // 
            // dateSelectionControlPeriod
            // 
            this.dateSelectionControlPeriod.BackColor = System.Drawing.Color.White;
            this.dateSelectionControlPeriod.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dateSelectionControlPeriod.Location = new System.Drawing.Point(10, 10);
            this.dateSelectionControlPeriod.Name = "dateSelectionControlPeriod";
            this.dateSelectionControlPeriod.ShowAddButtons = false;
            this.dateSelectionControlPeriod.ShowDateSelectionCalendar = false;
            this.dateSelectionControlPeriod.ShowDateSelectionRolling = false;
            this.dateSelectionControlPeriod.ShowTabArea = true;
            this.dateSelectionControlPeriod.Size = new System.Drawing.Size(280, 180);
            this.dateSelectionControlPeriod.TabIndex = 0;
				//this.dateSelectionControlPeriod.TabPanelBackColor = System.Drawing.Color.WhiteSmoke;
				//this.dateSelectionControlPeriod.TabPanelBorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            // 
            // DateTimePeriodSelection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dateSelectionControlPeriod);
            this.Name = "DateTimePeriodSelection";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.Size = new System.Drawing.Size(300, 200);
            this.ResumeLayout(false);

        }

        #endregion

        private Teleopti.Ccc.Win.Common.Controls.DateSelection.DateSelectionControl dateSelectionControlPeriod;

    }
}
