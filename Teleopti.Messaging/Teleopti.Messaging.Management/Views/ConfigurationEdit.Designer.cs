using System.ComponentModel;
using System.Windows.Forms;
using IConfigurationInfo=Teleopti.Interfaces.MessageBroker.Core.IConfigurationInfo;

namespace Teleopti.Messaging.Management.Views
{
    partial class ConfigurationEdit
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;
        private BindingSource iConfigurationInfoBindingSource;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigurationEdit));
            this.dataGridView = new System.Windows.Forms.DataGridView();
            this.configurationIdDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.configurationTypeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.configurationNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.configurationValueDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.configurationDataTypeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.changedByDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.changedDateTimeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.iConfigurationInfoBindingSource = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.iConfigurationInfoBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView
            // 
            this.dataGridView.AutoGenerateColumns = false;
            this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.configurationIdDataGridViewTextBoxColumn,
            this.configurationTypeDataGridViewTextBoxColumn,
            this.configurationNameDataGridViewTextBoxColumn,
            this.configurationValueDataGridViewTextBoxColumn,
            this.configurationDataTypeDataGridViewTextBoxColumn,
            this.changedByDataGridViewTextBoxColumn,
            this.changedDateTimeDataGridViewTextBoxColumn});
            this.dataGridView.DataSource = this.iConfigurationInfoBindingSource;
            this.dataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView.Location = new System.Drawing.Point(0, 0);
            this.dataGridView.MultiSelect = false;
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.Size = new System.Drawing.Size(744, 266);
            this.dataGridView.TabIndex = 0;
            // 
            // configurationIdDataGridViewTextBoxColumn
            // 
            this.configurationIdDataGridViewTextBoxColumn.DataPropertyName = "ConfigurationId";
            this.configurationIdDataGridViewTextBoxColumn.HeaderText = "ConfigurationId";
            this.configurationIdDataGridViewTextBoxColumn.Name = "configurationIdDataGridViewTextBoxColumn";
            // 
            // configurationTypeDataGridViewTextBoxColumn
            // 
            this.configurationTypeDataGridViewTextBoxColumn.DataPropertyName = "ConfigurationType";
            this.configurationTypeDataGridViewTextBoxColumn.HeaderText = "ConfigurationType";
            this.configurationTypeDataGridViewTextBoxColumn.Name = "configurationTypeDataGridViewTextBoxColumn";
            // 
            // configurationNameDataGridViewTextBoxColumn
            // 
            this.configurationNameDataGridViewTextBoxColumn.DataPropertyName = "ConfigurationName";
            this.configurationNameDataGridViewTextBoxColumn.HeaderText = "ConfigurationName";
            this.configurationNameDataGridViewTextBoxColumn.Name = "configurationNameDataGridViewTextBoxColumn";
            // 
            // configurationValueDataGridViewTextBoxColumn
            // 
            this.configurationValueDataGridViewTextBoxColumn.DataPropertyName = "ConfigurationValue";
            this.configurationValueDataGridViewTextBoxColumn.HeaderText = "ConfigurationValue";
            this.configurationValueDataGridViewTextBoxColumn.Name = "configurationValueDataGridViewTextBoxColumn";
            // 
            // configurationDataTypeDataGridViewTextBoxColumn
            // 
            this.configurationDataTypeDataGridViewTextBoxColumn.DataPropertyName = "ConfigurationDataType";
            this.configurationDataTypeDataGridViewTextBoxColumn.HeaderText = "ConfigurationDataType";
            this.configurationDataTypeDataGridViewTextBoxColumn.Name = "configurationDataTypeDataGridViewTextBoxColumn";
            // 
            // changedByDataGridViewTextBoxColumn
            // 
            this.changedByDataGridViewTextBoxColumn.DataPropertyName = "ChangedBy";
            this.changedByDataGridViewTextBoxColumn.HeaderText = "ChangedBy";
            this.changedByDataGridViewTextBoxColumn.Name = "changedByDataGridViewTextBoxColumn";
            // 
            // changedDateTimeDataGridViewTextBoxColumn
            // 
            this.changedDateTimeDataGridViewTextBoxColumn.DataPropertyName = "ChangedDateTime";
            this.changedDateTimeDataGridViewTextBoxColumn.HeaderText = "ChangedDateTime";
            this.changedDateTimeDataGridViewTextBoxColumn.Name = "changedDateTimeDataGridViewTextBoxColumn";
            // 
            // iConfigurationInfoBindingSource
            // 
            this.iConfigurationInfoBindingSource.DataSource = typeof(IConfigurationInfo);
            // 
            // ConfigurationEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(744, 266);
            this.Controls.Add(this.dataGridView);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ConfigurationEdit";
            this.Text = "Edit Configuration";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.iConfigurationInfoBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DataGridView dataGridView;
        private DataGridViewTextBoxColumn configurationIdDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn configurationTypeDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn configurationNameDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn configurationValueDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn configurationDataTypeDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn changedByDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn changedDateTimeDataGridViewTextBoxColumn;

        



    }
}