namespace Teleopti.Ccc.Win.Common
{
    partial class HandleBusinessRuleResponse
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
			this.listView1 = new System.Windows.Forms.ListView();
			this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.labelHeader = new System.Windows.Forms.Label();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.labelMessage = new System.Windows.Forms.Label();
			this.checkBoxApplyToAll = new System.Windows.Forms.CheckBox();
			this.buttonCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonOK = new Syncfusion.Windows.Forms.ButtonAdv();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// listView1
			// 
			this.listView1.AutoArrange = false;
			this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
			this.tableLayoutPanel1.SetColumnSpan(this.listView1, 3);
			this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listView1.GridLines = true;
			this.listView1.Location = new System.Drawing.Point(3, 32);
			this.listView1.Name = "listView1";
			this.listView1.ShowGroups = false;
			this.listView1.Size = new System.Drawing.Size(829, 296);
			this.listView1.TabIndex = 1;
			this.listView1.UseCompatibleStateImageBehavior = false;
			this.listView1.View = System.Windows.Forms.View.Details;
			this.listView1.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.listView1ItemCheck);
			this.listView1.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listView1ItemChecked);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "xxPerson";
			this.columnHeader1.Width = 100;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "xxErrorMessage";
			this.columnHeader2.Width = 600;
			// 
			// labelHeader
			// 
			this.labelHeader.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.labelHeader, 3);
			this.labelHeader.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
			this.labelHeader.Location = new System.Drawing.Point(3, 6);
			this.labelHeader.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
			this.labelHeader.Name = "labelHeader";
			this.labelHeader.Size = new System.Drawing.Size(156, 17);
			this.labelHeader.TabIndex = 0;
			this.labelHeader.Text = "xxBusinessRulesConflict";
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.tableLayoutPanel1.Controls.Add(this.labelMessage, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.labelHeader, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.listView1, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.checkBoxApplyToAll, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.buttonCancel, 2, 4);
			this.tableLayoutPanel1.Controls.Add(this.buttonOK, 1, 4);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(12);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 5;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 47F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(835, 436);
			this.tableLayoutPanel1.TabIndex = 5;
			// 
			// labelMessage
			// 
			this.labelMessage.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.tableLayoutPanel1.SetColumnSpan(this.labelMessage, 3);
			this.labelMessage.Location = new System.Drawing.Point(3, 363);
			this.labelMessage.Name = "labelMessage";
			this.labelMessage.Size = new System.Drawing.Size(814, 23);
			this.labelMessage.TabIndex = 7;
			this.labelMessage.Text = "xxChooseCancelIfYouWouldLikeToCorrectTheErrorsOrCheckEachErrorToOverrideTheBusine" +
    "ssRules";
			this.labelMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// checkBoxApplyToAll
			// 
			this.checkBoxApplyToAll.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.checkBoxApplyToAll, 3);
			this.checkBoxApplyToAll.Location = new System.Drawing.Point(3, 334);
			this.checkBoxApplyToAll.Name = "checkBoxApplyToAll";
			this.checkBoxApplyToAll.Size = new System.Drawing.Size(179, 19);
			this.checkBoxApplyToAll.TabIndex = 6;
			this.checkBoxApplyToAll.Text = "xxAlwaysOverrideTheseErrors";
			this.checkBoxApplyToAll.UseVisualStyleBackColor = true;
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonCancel.BeforeTouchSize = new System.Drawing.Size(87, 29);
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.ForeColor = System.Drawing.Color.White;
			this.buttonCancel.IsBackStageButton = false;
			this.buttonCancel.Location = new System.Drawing.Point(738, 397);
			this.buttonCancel.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(87, 29);
			this.buttonCancel.TabIndex = 3;
			this.buttonCancel.Text = "xxCancel";
			this.buttonCancel.UseVisualStyle = true;
			this.buttonCancel.UseVisualStyleBackColor = true;
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancelClick);
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonOK.BeforeTouchSize = new System.Drawing.Size(87, 29);
			this.buttonOK.ForeColor = System.Drawing.Color.White;
			this.buttonOK.IsBackStageButton = false;
			this.buttonOK.Location = new System.Drawing.Point(617, 397);
			this.buttonOK.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(87, 29);
			this.buttonOK.TabIndex = 2;
			this.buttonOK.Text = "xxOk";
			this.buttonOK.UseVisualStyle = true;
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.buttonOkClick);
			// 
			// HandleBusinessRuleResponse
			// 
			this.AcceptButton = this.buttonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BorderColor = System.Drawing.Color.Blue;
			this.CancelButton = this.buttonCancel;
			this.CaptionFont = new System.Drawing.Font("Segoe UI", 12F);
			this.ClientSize = new System.Drawing.Size(835, 436);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.HelpButton = false;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "HandleBusinessRuleResponse";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "xxResolveBusinessRulesConflict";
			this.TopMost = true;
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Label labelHeader;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.CheckBox checkBoxApplyToAll;
		private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Label labelMessage;
		private Syncfusion.Windows.Forms.ButtonAdv buttonCancel;
		private Syncfusion.Windows.Forms.ButtonAdv buttonOK;
        private System.Windows.Forms.ColumnHeader columnHeader2;
    }
}