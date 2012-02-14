namespace Teleopti.Support.Tool.Controls.General
{
    partial class DBSelect
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
            this.CAnalyticsDB = new System.Windows.Forms.ComboBox();
            this.CAggDB = new System.Windows.Forms.ComboBox();
            this.CAppDB = new System.Windows.Forms.ComboBox();
            this.LAggregationDB = new System.Windows.Forms.Label();
            this.AnalyticsDb = new System.Windows.Forms.Label();
            this.LApplicationDB = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // CAnalyticsDB
            // 
            this.CAnalyticsDB.FormattingEnabled = true;
            this.CAnalyticsDB.Location = new System.Drawing.Point(17, 114);
            this.CAnalyticsDB.Name = "CAnalyticsDB";
            this.CAnalyticsDB.Size = new System.Drawing.Size(256, 21);
            this.CAnalyticsDB.TabIndex = 2;
            // 
            // CAggDB
            // 
            this.CAggDB.FormattingEnabled = true;
            this.CAggDB.Location = new System.Drawing.Point(17, 67);
            this.CAggDB.Name = "CAggDB";
            this.CAggDB.Size = new System.Drawing.Size(256, 21);
            this.CAggDB.TabIndex = 1;
            // 
            // CAppDB
            // 
            this.CAppDB.FormattingEnabled = true;
            this.CAppDB.Location = new System.Drawing.Point(17, 19);
            this.CAppDB.Name = "CAppDB";
            this.CAppDB.Size = new System.Drawing.Size(256, 21);
            this.CAppDB.TabIndex = 0;
            // 
            // LAggregationDB
            // 
            this.LAggregationDB.AutoSize = true;
            this.LAggregationDB.Location = new System.Drawing.Point(14, 50);
            this.LAggregationDB.Name = "LAggregationDB";
            this.LAggregationDB.Size = new System.Drawing.Size(82, 13);
            this.LAggregationDB.TabIndex = 30;
            this.LAggregationDB.Text = "Aggregation DB";
            // 
            // AnalyticsDb
            // 
            this.AnalyticsDb.AutoSize = true;
            this.AnalyticsDb.Location = new System.Drawing.Point(14, 98);
            this.AnalyticsDb.Name = "AnalyticsDb";
            this.AnalyticsDb.Size = new System.Drawing.Size(67, 13);
            this.AnalyticsDb.TabIndex = 29;
            this.AnalyticsDb.Text = "Analytics DB";
            // 
            // LApplicationDB
            // 
            this.LApplicationDB.AutoSize = true;
            this.LApplicationDB.Location = new System.Drawing.Point(18, 0);
            this.LApplicationDB.Name = "LApplicationDB";
            this.LApplicationDB.Size = new System.Drawing.Size(77, 13);
            this.LApplicationDB.TabIndex = 34;
            this.LApplicationDB.Text = "Application DB";
            // 
            // DBSelect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.LApplicationDB);
            this.Controls.Add(this.CAnalyticsDB);
            this.Controls.Add(this.CAggDB);
            this.Controls.Add(this.CAppDB);
            this.Controls.Add(this.LAggregationDB);
            this.Controls.Add(this.AnalyticsDb);
            this.Name = "DBSelect";
            this.Size = new System.Drawing.Size(286, 151);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox CAnalyticsDB;
        private System.Windows.Forms.ComboBox CAggDB;
        private System.Windows.Forms.ComboBox CAppDB;
        private System.Windows.Forms.Label LAggregationDB;
        private System.Windows.Forms.Label AnalyticsDb;
        private System.Windows.Forms.Label LApplicationDB;
    }
}
