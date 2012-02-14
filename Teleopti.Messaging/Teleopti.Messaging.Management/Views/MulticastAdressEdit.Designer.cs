using IMulticastAddressInfo=Teleopti.Interfaces.MessageBroker.Core.IMessageInformation;

namespace Teleopti.Messaging.Management.Views
{
    partial class MulticastAdressEdit
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MulticastAdressEdit));
            this.dataGridViewAddress = new System.Windows.Forms.DataGridView();
            this.multicastAddressDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.portDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.broadcastDirectionDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.iMulticastAddressInfoBindingSource = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewAddress)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.iMulticastAddressInfoBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridViewAddress
            // 
            this.dataGridViewAddress.AutoGenerateColumns = false;
            this.dataGridViewAddress.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewAddress.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.multicastAddressDataGridViewTextBoxColumn,
            this.portDataGridViewTextBoxColumn,
            this.broadcastDirectionDataGridViewTextBoxColumn});
            this.dataGridViewAddress.DataSource = this.iMulticastAddressInfoBindingSource;
            this.dataGridViewAddress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewAddress.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewAddress.MultiSelect = false;
            this.dataGridViewAddress.Name = "dataGridViewAddress";
            this.dataGridViewAddress.Size = new System.Drawing.Size(292, 266);
            this.dataGridViewAddress.TabIndex = 0;
            // 
            // multicastAddressDataGridViewTextBoxColumn
            // 
            this.multicastAddressDataGridViewTextBoxColumn.DataPropertyName = "MulticastAddress";
            this.multicastAddressDataGridViewTextBoxColumn.HeaderText = "MulticastAddress";
            this.multicastAddressDataGridViewTextBoxColumn.Name = "multicastAddressDataGridViewTextBoxColumn";
            // 
            // portDataGridViewTextBoxColumn
            // 
            this.portDataGridViewTextBoxColumn.DataPropertyName = "Port";
            this.portDataGridViewTextBoxColumn.HeaderText = "Port";
            this.portDataGridViewTextBoxColumn.Name = "portDataGridViewTextBoxColumn";
            // 
            // broadcastDirectionDataGridViewTextBoxColumn
            // 
            this.broadcastDirectionDataGridViewTextBoxColumn.DataPropertyName = "BroadcastDirection";
            this.broadcastDirectionDataGridViewTextBoxColumn.HeaderText = "BroadcastDirection";
            this.broadcastDirectionDataGridViewTextBoxColumn.Name = "broadcastDirectionDataGridViewTextBoxColumn";
            // 
            // iMulticastAddressInfoBindingSource
            // 
            this.iMulticastAddressInfoBindingSource.DataSource = typeof(IMulticastAddressInfo);
            // 
            // MulticastAdressEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.Controls.Add(this.dataGridViewAddress);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MulticastAdressEdit";
            this.Text = "Edit Multicast Addresses";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewAddress)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.iMulticastAddressInfoBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridViewAddress;
        private System.Windows.Forms.DataGridViewTextBoxColumn multicastAddressDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn portDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn broadcastDirectionDataGridViewTextBoxColumn;
        private System.Windows.Forms.BindingSource iMulticastAddressInfoBindingSource;
    }
}