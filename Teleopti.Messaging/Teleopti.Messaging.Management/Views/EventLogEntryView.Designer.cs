using System.Windows.Forms;
using ILogbookEntry=Teleopti.Interfaces.MessageBroker.Core.ILogbookEntry;

namespace Teleopti.Messaging.Management.Views
{
    partial class EventLogEntryView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EventLogEntryView));
            this.groupBoxLogView = new System.Windows.Forms.GroupBox();
            this.dataGridViewLog = new System.Windows.Forms.DataGridView();
            this.logDateTimeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.logTypeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.classTypeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.logMessageDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.userNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.iLogbookEntryBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.textBoxEventLogEntryType = new System.Windows.Forms.TextBox();
            this.labelType = new System.Windows.Forms.Label();
            this.labelUser = new System.Windows.Forms.Label();
            this.textBoxUser = new System.Windows.Forms.TextBox();
            this.groupBoxLogView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewLog)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.iLogbookEntryBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxLogView
            // 
            this.groupBoxLogView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxLogView.Controls.Add(this.dataGridViewLog);
            this.groupBoxLogView.Controls.Add(this.textBoxEventLogEntryType);
            this.groupBoxLogView.Controls.Add(this.labelType);
            this.groupBoxLogView.Controls.Add(this.labelUser);
            this.groupBoxLogView.Controls.Add(this.textBoxUser);
            this.groupBoxLogView.Location = new System.Drawing.Point(12, 12);
            this.groupBoxLogView.Name = "groupBoxLogView";
            this.groupBoxLogView.Size = new System.Drawing.Size(857, 318);
            this.groupBoxLogView.TabIndex = 0;
            this.groupBoxLogView.TabStop = false;
            this.groupBoxLogView.Text = "Log Book";
            // 
            // dataGridViewLog
            // 
            this.dataGridViewLog.AllowUserToAddRows = false;
            this.dataGridViewLog.AllowUserToDeleteRows = false;
            this.dataGridViewLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewLog.AutoGenerateColumns = false;
            this.dataGridViewLog.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewLog.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.logDateTimeDataGridViewTextBoxColumn,
            this.logTypeDataGridViewTextBoxColumn,
            this.classTypeDataGridViewTextBoxColumn,
            this.logMessageDataGridViewTextBoxColumn,
            this.userNameDataGridViewTextBoxColumn});
            this.dataGridViewLog.DataSource = this.iLogbookEntryBindingSource;
            this.dataGridViewLog.Location = new System.Drawing.Point(6, 45);
            this.dataGridViewLog.Name = "dataGridViewLog";
            this.dataGridViewLog.Size = new System.Drawing.Size(845, 267);
            this.dataGridViewLog.TabIndex = 5;
            // 
            // logDateTimeDataGridViewTextBoxColumn
            // 
            this.logDateTimeDataGridViewTextBoxColumn.DataPropertyName = "LogDateTime";
            this.logDateTimeDataGridViewTextBoxColumn.HeaderText = "LogDateTime";
            this.logDateTimeDataGridViewTextBoxColumn.Name = "logDateTimeDataGridViewTextBoxColumn";
            this.logDateTimeDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // logTypeDataGridViewTextBoxColumn
            // 
            this.logTypeDataGridViewTextBoxColumn.DataPropertyName = "LogType";
            this.logTypeDataGridViewTextBoxColumn.HeaderText = "LogType";
            this.logTypeDataGridViewTextBoxColumn.Name = "logTypeDataGridViewTextBoxColumn";
            this.logTypeDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // classTypeDataGridViewTextBoxColumn
            // 
            this.classTypeDataGridViewTextBoxColumn.DataPropertyName = "ClassType";
            this.classTypeDataGridViewTextBoxColumn.HeaderText = "ClassType";
            this.classTypeDataGridViewTextBoxColumn.Name = "classTypeDataGridViewTextBoxColumn";
            this.classTypeDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // logMessageDataGridViewTextBoxColumn
            // 
            this.logMessageDataGridViewTextBoxColumn.DataPropertyName = "LogMessage";
            this.logMessageDataGridViewTextBoxColumn.HeaderText = "LogMessage";
            this.logMessageDataGridViewTextBoxColumn.Name = "logMessageDataGridViewTextBoxColumn";
            this.logMessageDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // userNameDataGridViewTextBoxColumn
            // 
            this.userNameDataGridViewTextBoxColumn.DataPropertyName = "UserName";
            this.userNameDataGridViewTextBoxColumn.HeaderText = "UserName";
            this.userNameDataGridViewTextBoxColumn.Name = "userNameDataGridViewTextBoxColumn";
            this.userNameDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // iLogbookEntryBindingSource
            // 
            this.iLogbookEntryBindingSource.DataSource = typeof(ILogbookEntry);
            // 
            // textBoxEventLogEntryType
            // 
            this.textBoxEventLogEntryType.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.textBoxEventLogEntryType.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.textBoxEventLogEntryType.Location = new System.Drawing.Point(190, 19);
            this.textBoxEventLogEntryType.Name = "textBoxEventLogEntryType";
            this.textBoxEventLogEntryType.Size = new System.Drawing.Size(151, 20);
            this.textBoxEventLogEntryType.TabIndex = 3;
            // 
            // labelType
            // 
            this.labelType.AutoSize = true;
            this.labelType.Location = new System.Drawing.Point(153, 22);
            this.labelType.Name = "labelType";
            this.labelType.Size = new System.Drawing.Size(31, 13);
            this.labelType.TabIndex = 2;
            this.labelType.Text = "Type";
            // 
            // labelUser
            // 
            this.labelUser.AutoSize = true;
            this.labelUser.Location = new System.Drawing.Point(6, 22);
            this.labelUser.Name = "labelUser";
            this.labelUser.Size = new System.Drawing.Size(29, 13);
            this.labelUser.TabIndex = 1;
            this.labelUser.Text = "User";
            // 
            // textBoxUser
            // 
            this.textBoxUser.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.textBoxUser.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.textBoxUser.Location = new System.Drawing.Point(47, 19);
            this.textBoxUser.Name = "textBoxUser";
            this.textBoxUser.Size = new System.Drawing.Size(100, 20);
            this.textBoxUser.TabIndex = 0;
            // 
            // EventLogEntryView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(881, 342);
            this.Controls.Add(this.groupBoxLogView);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(440, 300);
            this.Name = "EventLogEntryView";
            this.Text = " Log Book ";
            this.groupBoxLogView.ResumeLayout(false);
            this.groupBoxLogView.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewLog)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.iLogbookEntryBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private GroupBox groupBoxLogView;
        private TextBox textBoxUser;
        private Label labelUser;
        private TextBox textBoxEventLogEntryType;
        private Label labelType;
        private DataGridView dataGridViewLog;
        private DataGridViewTextBoxColumn logDateTimeDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn logTypeDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn classTypeDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn logMessageDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn userNameDataGridViewTextBoxColumn;
        private BindingSource iLogbookEntryBindingSource;

    }
}