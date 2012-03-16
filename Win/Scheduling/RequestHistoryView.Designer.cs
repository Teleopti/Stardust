namespace Teleopti.Ccc.Win.Scheduling
{
    partial class RequestHistoryView
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.comboBoxAdvPersons = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
            this.listViewRequests = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvPersons)).BeginInit();
            this.SuspendLayout();
            // 
            // comboBoxAdvPersons
            // 
            this.comboBoxAdvPersons.Location = new System.Drawing.Point(53, 43);
            this.comboBoxAdvPersons.Name = "comboBoxAdvPersons";
            this.comboBoxAdvPersons.Size = new System.Drawing.Size(259, 21);
            this.comboBoxAdvPersons.TabIndex = 0;
            this.comboBoxAdvPersons.Text = "comboBoxAdv1";
            this.comboBoxAdvPersons.SelectedIndexChanged += new System.EventHandler(this.ComboBoxAdvPersonsSelectedIndexChanged);
            // 
            // listViewRequests
            // 
            this.listViewRequests.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.listViewRequests.Location = new System.Drawing.Point(53, 93);
            this.listViewRequests.Name = "listViewRequests";
            this.listViewRequests.Size = new System.Drawing.Size(259, 448);
            this.listViewRequests.TabIndex = 1;
            this.listViewRequests.UseCompatibleStateImageBehavior = false;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "xxType";
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "xxStatus";
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "xxDate";
            // 
            // RequestHistoryView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1049, 575);
            this.Controls.Add(this.listViewRequests);
            this.Controls.Add(this.comboBoxAdvPersons);
            this.Name = "RequestHistoryView";
            this.Text = "xxRequestHistoryView";
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvPersons)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvPersons;
        private System.Windows.Forms.ListView listViewRequests;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
    }
}