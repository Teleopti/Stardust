namespace Teleopti.Ccc.Win.Forecasting.Forms
{
    partial class CascadingSkillsView
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
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.buttonAdvOk = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.labelSourceWorkload = new System.Windows.Forms.Label();
			this.listBoxCascading = new System.Windows.Forms.ListBox();
			this.listBoxNonCascading = new System.Windows.Forms.ListBox();
			this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
			this.buttonAdvMakeCascading = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvMakeNonCascading = new Syncfusion.Windows.Forms.ButtonAdv();
			this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
			this.buttonAdvMoveUp = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvMoveDown = new Syncfusion.Windows.Forms.ButtonAdv();
			this.tableLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			this.tableLayoutPanel3.SuspendLayout();
			this.tableLayoutPanel4.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 5);
			this.tableLayoutPanel1.Controls.Add(this.listBoxCascading, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.listBoxNonCascading, 2, 1);
			this.tableLayoutPanel1.Controls.Add(this.labelSourceWorkload, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel4, 0, 2);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 6;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 569F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(727, 771);
			this.tableLayoutPanel1.TabIndex = 0;
			
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanel2.ColumnCount = 3;
			this.tableLayoutPanel1.SetColumnSpan(this.tableLayoutPanel2, 3);
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 105F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 105F));
			this.tableLayoutPanel2.Controls.Add(this.buttonAdvOk, 1, 0);
			this.tableLayoutPanel2.Controls.Add(this.buttonAdvCancel, 2, 0);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 721);
			this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 1;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(727, 50);
			this.tableLayoutPanel2.TabIndex = 3;
			
			// 
			// buttonAdvOk
			// 
			this.buttonAdvOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvOk.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvOk.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvOk.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonAdvOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonAdvOk.ForeColor = System.Drawing.Color.White;
			this.buttonAdvOk.IsBackStageButton = false;
			this.buttonAdvOk.Location = new System.Drawing.Point(525, 13);
			this.buttonAdvOk.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonAdvOk.Name = "buttonAdvOk";
			this.buttonAdvOk.Size = new System.Drawing.Size(87, 27);
			this.buttonAdvOk.TabIndex = 6;
			this.buttonAdvOk.Text = "xxOk";
			this.buttonAdvOk.UseVisualStyle = true;
			// 
			// buttonAdvCancel
			// 
			this.buttonAdvCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvCancel.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonAdvCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonAdvCancel.ForeColor = System.Drawing.Color.White;
			this.buttonAdvCancel.IsBackStageButton = false;
			this.buttonAdvCancel.Location = new System.Drawing.Point(630, 13);
			this.buttonAdvCancel.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonAdvCancel.Name = "buttonAdvCancel";
			this.buttonAdvCancel.Size = new System.Drawing.Size(87, 27);
			this.buttonAdvCancel.TabIndex = 7;
			this.buttonAdvCancel.Text = "xxCancel";
			this.buttonAdvCancel.UseVisualStyle = true;
			// 
			// labelSourceWorkload
			// 
			this.labelSourceWorkload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.labelSourceWorkload.AutoSize = true;
			this.labelSourceWorkload.Location = new System.Drawing.Point(3, 8);
			this.labelSourceWorkload.Name = "labelSourceWorkload";
			this.labelSourceWorkload.Size = new System.Drawing.Size(317, 15);
			this.labelSourceWorkload.TabIndex = 1;
			this.labelSourceWorkload.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// listBoxCascading
			// 
			this.listBoxCascading.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listBoxCascading.FormattingEnabled = true;
			this.listBoxCascading.ItemHeight = 15;
			this.listBoxCascading.Location = new System.Drawing.Point(3, 35);
			this.listBoxCascading.Name = "listBoxCascading";
			this.listBoxCascading.Size = new System.Drawing.Size(317, 563);
			this.listBoxCascading.TabIndex = 4;
			// 
			// listBoxNonCascading
			// 
			this.listBoxNonCascading.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listBoxNonCascading.FormattingEnabled = true;
			this.listBoxNonCascading.ItemHeight = 15;
			this.listBoxNonCascading.Location = new System.Drawing.Point(406, 35);
			this.listBoxNonCascading.Name = "listBoxNonCascading";
			this.listBoxNonCascading.Size = new System.Drawing.Size(318, 563);
			this.listBoxNonCascading.TabIndex = 5;
			// 
			// tableLayoutPanel3
			// 
			this.tableLayoutPanel3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel3.ColumnCount = 1;
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel3.Controls.Add(this.buttonAdvMakeNonCascading, 0, 1);
			this.tableLayoutPanel3.Controls.Add(this.buttonAdvMakeCascading, 0, 0);
			this.tableLayoutPanel3.Location = new System.Drawing.Point(326, 274);
			this.tableLayoutPanel3.Name = "tableLayoutPanel3";
			this.tableLayoutPanel3.RowCount = 2;
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel3.Size = new System.Drawing.Size(74, 85);
			this.tableLayoutPanel3.TabIndex = 6;
			// 
			// buttonAdvMakeCascading
			// 
			this.buttonAdvMakeCascading.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.buttonAdvMakeCascading.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvMakeCascading.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvMakeCascading.BeforeTouchSize = new System.Drawing.Size(61, 27);
			this.buttonAdvMakeCascading.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonAdvMakeCascading.ForeColor = System.Drawing.Color.White;
			this.buttonAdvMakeCascading.IsBackStageButton = false;
			this.buttonAdvMakeCascading.Location = new System.Drawing.Point(3, 4);
			this.buttonAdvMakeCascading.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonAdvMakeCascading.Name = "buttonAdvMakeCascading";
			this.buttonAdvMakeCascading.Size = new System.Drawing.Size(61, 27);
			this.buttonAdvMakeCascading.TabIndex = 7;
			this.buttonAdvMakeCascading.Text = "<";
			this.buttonAdvMakeCascading.UseVisualStyle = true;
			// 
			// buttonAdvMakeNonCascading
			// 
			this.buttonAdvMakeNonCascading.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvMakeNonCascading.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvMakeNonCascading.BeforeTouchSize = new System.Drawing.Size(61, 27);
			this.buttonAdvMakeNonCascading.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonAdvMakeNonCascading.ForeColor = System.Drawing.Color.White;
			this.buttonAdvMakeNonCascading.IsBackStageButton = false;
			this.buttonAdvMakeNonCascading.Location = new System.Drawing.Point(3, 45);
			this.buttonAdvMakeNonCascading.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonAdvMakeNonCascading.Name = "buttonAdvMakeNonCascading";
			this.buttonAdvMakeNonCascading.Size = new System.Drawing.Size(61, 27);
			this.buttonAdvMakeNonCascading.TabIndex = 8;
			this.buttonAdvMakeNonCascading.Text = ">";
			this.buttonAdvMakeNonCascading.UseVisualStyle = true;
			// 
			// tableLayoutPanel4
			// 
			this.tableLayoutPanel4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel4.ColumnCount = 2;
			this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel4.Controls.Add(this.buttonAdvMoveDown, 0, 0);
			this.tableLayoutPanel4.Controls.Add(this.buttonAdvMoveUp, 0, 0);
			this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 604);
			this.tableLayoutPanel4.Name = "tableLayoutPanel4";
			this.tableLayoutPanel4.RowCount = 1;
			this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel4.Size = new System.Drawing.Size(317, 39);
			this.tableLayoutPanel4.TabIndex = 7;
			// 
			// buttonAdvMoveUp
			// 
			this.buttonAdvMoveUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.buttonAdvMoveUp.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvMoveUp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvMoveUp.BeforeTouchSize = new System.Drawing.Size(87, 26);
			this.buttonAdvMoveUp.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonAdvMoveUp.ForeColor = System.Drawing.Color.White;
			this.buttonAdvMoveUp.IsBackStageButton = false;
			this.buttonAdvMoveUp.Location = new System.Drawing.Point(32, 3);
			this.buttonAdvMoveUp.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonAdvMoveUp.Name = "buttonAdvMoveUp";
			this.buttonAdvMoveUp.Size = new System.Drawing.Size(87, 26);
			this.buttonAdvMoveUp.TabIndex = 7;
			this.buttonAdvMoveUp.Text = "xxMoveUp";
			this.buttonAdvMoveUp.UseVisualStyle = true;
			// 
			// buttonAdvMoveDown
			// 
			this.buttonAdvMoveDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.buttonAdvMoveDown.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvMoveDown.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvMoveDown.BeforeTouchSize = new System.Drawing.Size(87, 26);
			this.buttonAdvMoveDown.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonAdvMoveDown.ForeColor = System.Drawing.Color.White;
			this.buttonAdvMoveDown.IsBackStageButton = false;
			this.buttonAdvMoveDown.Location = new System.Drawing.Point(190, 3);
			this.buttonAdvMoveDown.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonAdvMoveDown.Name = "buttonAdvMoveDown";
			this.buttonAdvMoveDown.Size = new System.Drawing.Size(87, 26);
			this.buttonAdvMoveDown.TabIndex = 8;
			this.buttonAdvMoveDown.Text = "xxMoveDown";
			this.buttonAdvMoveDown.UseVisualStyle = true;
			// 
			// CascadingSkillsView
			// 
			this.AcceptButton = this.buttonAdvOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.CancelButton = this.buttonAdvCancel;
			this.CaptionFont = new System.Drawing.Font("Segoe UI", 12F);
			this.ClientSize = new System.Drawing.Size(727, 771);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.HelpButton = false;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "CascadingSkillsView";
			this.ShowIcon = false;
			this.Text = "xxCascadingSkills";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel3.ResumeLayout(false);
			this.tableLayoutPanel4.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvOk;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvCancel;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		private System.Windows.Forms.Label labelSourceWorkload;
		private System.Windows.Forms.ListBox listBoxCascading;
		private System.Windows.Forms.ListBox listBoxNonCascading;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvMakeNonCascading;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvMakeCascading;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvMoveDown;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvMoveUp;
	}
}