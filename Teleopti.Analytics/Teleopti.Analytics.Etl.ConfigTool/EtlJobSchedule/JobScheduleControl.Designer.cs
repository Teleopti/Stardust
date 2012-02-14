namespace Teleopti.Analytics.Etl.ConfigTool.EtlJobSchedule
{
    partial class JobScheduleControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(JobScheduleControl));
            this.toolStripButtonNew = new System.Windows.Forms.ToolStripButton();
            this.dataGridViewJobSchedules = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumScheduleName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumJob = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumEnabled = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.toolStripButtonEdit = new System.Windows.Forms.ToolStripButton();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButtonRemove = new System.Windows.Forms.ToolStripButton();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewJobSchedules)).BeginInit();
            this.toolStrip1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripButtonNew
            // 
            this.toolStripButtonNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButtonNew.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonNew.Image")));
            this.toolStripButtonNew.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonNew.Name = "toolStripButtonNew";
            this.toolStripButtonNew.Size = new System.Drawing.Size(44, 22);
            this.toolStripButtonNew.Text = "New...";
            this.toolStripButtonNew.Click += new System.EventHandler(this.toolStripButtonNew_Click);
            // 
            // dataGridViewJobSchedules
            // 
            this.dataGridViewJobSchedules.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumScheduleName,
            this.dataGridViewTextBoxColumJob,
            this.dataGridViewTextBoxColumEnabled,
            this.dataGridViewTextBoxColumDescription});
            this.dataGridViewJobSchedules.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewJobSchedules.Location = new System.Drawing.Point(3, 28);
            this.dataGridViewJobSchedules.Name = "dataGridViewJobSchedules";
            this.dataGridViewJobSchedules.ReadOnly = true;
            this.dataGridViewJobSchedules.Size = new System.Drawing.Size(648, 452);
            this.dataGridViewJobSchedules.TabIndex = 0;
            this.dataGridViewJobSchedules.DoubleClick += new System.EventHandler(this.dataGridViewJobSchedules_DoubleClick);
            this.dataGridViewJobSchedules.SelectionChanged += new System.EventHandler(this.dataGridViewJobSchedules_SelectionChanged);
            // 
            // dataGridViewTextBoxColumScheduleName
            // 
            this.dataGridViewTextBoxColumScheduleName.HeaderText = "Schedule Name";
            this.dataGridViewTextBoxColumScheduleName.Name = "dataGridViewTextBoxColumScheduleName";
            this.dataGridViewTextBoxColumScheduleName.ReadOnly = true;
            this.dataGridViewTextBoxColumScheduleName.Width = 300;
            // 
            // dataGridViewTextBoxColumJob
            // 
            this.dataGridViewTextBoxColumJob.HeaderText = "Job";
            this.dataGridViewTextBoxColumJob.Name = "dataGridViewTextBoxColumJob";
            this.dataGridViewTextBoxColumJob.ReadOnly = true;
            this.dataGridViewTextBoxColumJob.Width = 150;
            // 
            // dataGridViewTextBoxColumEnabled
            // 
            this.dataGridViewTextBoxColumEnabled.HeaderText = "Enabled";
            this.dataGridViewTextBoxColumEnabled.Name = "dataGridViewTextBoxColumEnabled";
            this.dataGridViewTextBoxColumEnabled.ReadOnly = true;
            this.dataGridViewTextBoxColumEnabled.Width = 50;
            // 
            // dataGridViewTextBoxColumDescription
            // 
            this.dataGridViewTextBoxColumDescription.HeaderText = "Description";
            this.dataGridViewTextBoxColumDescription.Name = "dataGridViewTextBoxColumDescription";
            this.dataGridViewTextBoxColumDescription.ReadOnly = true;
            this.dataGridViewTextBoxColumDescription.Width = 600;
            // 
            // toolStripButtonEdit
            // 
            this.toolStripButtonEdit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButtonEdit.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonEdit.Image")));
            this.toolStripButtonEdit.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonEdit.Name = "toolStripButtonEdit";
            this.toolStripButtonEdit.Size = new System.Drawing.Size(40, 22);
            this.toolStripButtonEdit.Text = "Edit...";
            this.toolStripButtonEdit.Click += new System.EventHandler(this.toolStripButtonEdit_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonNew,
            this.toolStripButtonEdit,
            this.toolStripButtonRemove});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(654, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButtonRemove
            // 
            this.toolStripButtonRemove.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButtonRemove.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonRemove.Image")));
            this.toolStripButtonRemove.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonRemove.Name = "toolStripButtonRemove";
            this.toolStripButtonRemove.Size = new System.Drawing.Size(54, 22);
            this.toolStripButtonRemove.Text = "Remove";
            this.toolStripButtonRemove.ToolTipText = "Remove";
            this.toolStripButtonRemove.Click += new System.EventHandler(this.toolStripButtonRemove_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.toolStrip1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.dataGridViewJobSchedules, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(654, 483);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // JobScheduleControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "JobScheduleControl";
            this.Size = new System.Drawing.Size(654, 483);
            this.Load += new System.EventHandler(this.JobSheduleControl_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewJobSchedules)).EndInit();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStripButton toolStripButtonNew;
        private System.Windows.Forms.DataGridView dataGridViewJobSchedules;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumScheduleName;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumJob;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumEnabled;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumDescription;
        private System.Windows.Forms.ToolStripButton toolStripButtonEdit;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButtonRemove;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}
