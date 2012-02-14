using IEventHeartbeat=Teleopti.Interfaces.MessageBroker.Events.IEventHeartbeat;

namespace Teleopti.Messaging.Management.Views
{
    partial class HeartbeatView
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
            this.components = new System.ComponentModel.Container();
            this.heartbeatDataGridView = new System.Windows.Forms.DataGridView();
            this.heartbeatIdDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.processIdDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.changedByDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.changedDateTimeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.iEventHeartbeatBindingSource = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.heartbeatDataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.iEventHeartbeatBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // heartbeatDataGridView
            // 
            this.heartbeatDataGridView.AllowUserToAddRows = false;
            this.heartbeatDataGridView.AllowUserToDeleteRows = false;
            this.heartbeatDataGridView.AutoGenerateColumns = false;
            this.heartbeatDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.heartbeatDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.heartbeatDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.heartbeatIdDataGridViewTextBoxColumn,
            this.processIdDataGridViewTextBoxColumn,
            this.changedByDataGridViewTextBoxColumn,
            this.changedDateTimeDataGridViewTextBoxColumn});
            this.heartbeatDataGridView.DataSource = this.iEventHeartbeatBindingSource;
            this.heartbeatDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.heartbeatDataGridView.Location = new System.Drawing.Point(0, 0);
            this.heartbeatDataGridView.Name = "heartbeatDataGridView";
            this.heartbeatDataGridView.Size = new System.Drawing.Size(292, 266);
            this.heartbeatDataGridView.TabIndex = 0;
            // 
            // heartbeatIdDataGridViewTextBoxColumn
            // 
            this.heartbeatIdDataGridViewTextBoxColumn.DataPropertyName = "HeartbeatId";
            this.heartbeatIdDataGridViewTextBoxColumn.HeaderText = "HeartbeatId";
            this.heartbeatIdDataGridViewTextBoxColumn.Name = "heartbeatIdDataGridViewTextBoxColumn";
            this.heartbeatIdDataGridViewTextBoxColumn.Width = 88;
            // 
            // processIdDataGridViewTextBoxColumn
            // 
            this.processIdDataGridViewTextBoxColumn.DataPropertyName = "ProcessId";
            this.processIdDataGridViewTextBoxColumn.HeaderText = "ProcessId";
            this.processIdDataGridViewTextBoxColumn.Name = "processIdDataGridViewTextBoxColumn";
            this.processIdDataGridViewTextBoxColumn.Width = 79;
            // 
            // changedByDataGridViewTextBoxColumn
            // 
            this.changedByDataGridViewTextBoxColumn.DataPropertyName = "ChangedBy";
            this.changedByDataGridViewTextBoxColumn.HeaderText = "ChangedBy";
            this.changedByDataGridViewTextBoxColumn.Name = "changedByDataGridViewTextBoxColumn";
            this.changedByDataGridViewTextBoxColumn.Width = 87;
            // 
            // changedDateTimeDataGridViewTextBoxColumn
            // 
            this.changedDateTimeDataGridViewTextBoxColumn.DataPropertyName = "ChangedDateTime";
            this.changedDateTimeDataGridViewTextBoxColumn.HeaderText = "ChangedDateTime";
            this.changedDateTimeDataGridViewTextBoxColumn.Name = "changedDateTimeDataGridViewTextBoxColumn";
            this.changedDateTimeDataGridViewTextBoxColumn.Width = 121;
            // 
            // iEventHeartbeatBindingSource
            // 
            this.iEventHeartbeatBindingSource.DataSource = typeof(IEventHeartbeat);
            // 
            // HeartbeatView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.Controls.Add(this.heartbeatDataGridView);
            this.Name = "HeartbeatView";
            this.Text = "Heartbeats";
            ((System.ComponentModel.ISupportInitialize)(this.heartbeatDataGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.iEventHeartbeatBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView heartbeatDataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn heartbeatIdDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn processIdDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn changedByDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn changedDateTimeDataGridViewTextBoxColumn;
        private System.Windows.Forms.BindingSource iEventHeartbeatBindingSource;
    }
}