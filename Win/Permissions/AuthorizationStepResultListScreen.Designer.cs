namespace Teleopti.Ccc.Win.Permissions
{
    partial class AuthorizationStepResultListScreen
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
            this.xbtnClose = new System.Windows.Forms.Button();
            this.xlblCurrentStep = new System.Windows.Forms.Label();
            this.xgridResultList = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Value = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.xlblCurrentStepDescription = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.xgridResultList)).BeginInit();
            this.SuspendLayout();
            // 
            // xbtnClose
            // 
            this.xbtnClose.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.xbtnClose.Enabled = false;
            this.xbtnClose.Location = new System.Drawing.Point(164, 308);
            this.xbtnClose.Name = "xbtnClose";
            this.xbtnClose.Size = new System.Drawing.Size(83, 23);
            this.xbtnClose.TabIndex = 2;
            this.xbtnClose.Text = "Close";
            this.xbtnClose.UseVisualStyleBackColor = true;
            this.xbtnClose.Click += new System.EventHandler(this.xbtnCancel_Click);
            // 
            // xlblCurrentStep
            // 
            this.xlblCurrentStep.AutoSize = true;
            this.xlblCurrentStep.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xlblCurrentStep.Location = new System.Drawing.Point(12, 9);
            this.xlblCurrentStep.Name = "xlblCurrentStep";
            this.xlblCurrentStep.Size = new System.Drawing.Size(104, 13);
            this.xlblCurrentStep.TabIndex = 9;
            this.xlblCurrentStep.Text = "The Current Step";
            // 
            // xgridResultList
            // 
            this.xgridResultList.AllowUserToAddRows = false;
            this.xgridResultList.AllowUserToDeleteRows = false;
            this.xgridResultList.AllowUserToOrderColumns = true;
            this.xgridResultList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.xgridResultList.ColumnHeadersHeight = 10;
            this.xgridResultList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.xgridResultList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn3,
            this.dataGridViewTextBoxColumn4,
            this.Value});
            this.xgridResultList.Location = new System.Drawing.Point(15, 66);
            this.xgridResultList.Name = "xgridResultList";
            this.xgridResultList.ReadOnly = true;
            this.xgridResultList.RowHeadersWidth = 10;
            this.xgridResultList.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.xgridResultList.RowTemplate.ReadOnly = true;
            this.xgridResultList.ShowCellErrors = false;
            this.xgridResultList.ShowCellToolTips = false;
            this.xgridResultList.ShowEditingIcon = false;
            this.xgridResultList.ShowRowErrors = false;
            this.xgridResultList.Size = new System.Drawing.Size(380, 236);
            this.xgridResultList.TabIndex = 6;
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.DataPropertyName = "AuthorizationName";
            this.dataGridViewTextBoxColumn3.HeaderText = "";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.DataPropertyName = "AuthorizationDescription";
            this.dataGridViewTextBoxColumn4.HeaderText = "";
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            this.dataGridViewTextBoxColumn4.ReadOnly = true;
            this.dataGridViewTextBoxColumn4.Width = 200;
            // 
            // Value
            // 
            this.Value.DataPropertyName = "AuthorizationValue";
            this.Value.HeaderText = "";
            this.Value.Name = "Value";
            this.Value.ReadOnly = true;
            this.Value.Width = 60;
            // 
            // xlblCurrentStepDescription
            // 
            this.xlblCurrentStepDescription.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xlblCurrentStepDescription.Location = new System.Drawing.Point(12, 31);
            this.xlblCurrentStepDescription.Name = "xlblCurrentStepDescription";
            this.xlblCurrentStepDescription.Size = new System.Drawing.Size(379, 32);
            this.xlblCurrentStepDescription.TabIndex = 10;
            this.xlblCurrentStepDescription.Text = "The Current Step";
            // 
            // AuthorizationStepResultListScreen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(410, 343);
            this.Controls.Add(this.xlblCurrentStepDescription);
            this.Controls.Add(this.xlblCurrentStep);
            this.Controls.Add(this.xgridResultList);
            this.Controls.Add(this.xbtnClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AuthorizationStepResultListScreen";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Authorization Step Result List";
            ((System.ComponentModel.ISupportInitialize)(this.xgridResultList)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button xbtnClose;
        private System.Windows.Forms.Label xlblCurrentStep;
        private System.Windows.Forms.DataGridView xgridResultList;
        private System.Windows.Forms.Label xlblCurrentStepDescription;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.DataGridViewTextBoxColumn Value;
    }
}