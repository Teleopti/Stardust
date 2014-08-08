namespace Teleopti.Ccc.Win.Scheduling
{
	partial class RequestReplyStatusChangeDialog
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
			this.textBoxReply = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
			this.textBoxMessage = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
			this.buttonAdvCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvReply = new Syncfusion.Windows.Forms.ButtonAdv();
			this.label1 = new System.Windows.Forms.Label();
			this.labelMessage = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.textBoxReply)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.textBoxMessage)).BeginInit();
			this.tableLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel3.SuspendLayout();
			this.SuspendLayout();
			// 
			// textBoxReply
			// 
			this.textBoxReply.BackColor = System.Drawing.Color.White;
			this.textBoxReply.BeforeTouchSize = new System.Drawing.Size(326, 174);
			this.textBoxReply.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxReply.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textBoxReply.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.textBoxReply.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textBoxReply.Location = new System.Drawing.Point(109, 183);
			this.textBoxReply.Metrocolor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxReply.Multiline = true;
			this.textBoxReply.Name = "textBoxReply";
			this.textBoxReply.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textBoxReply.Size = new System.Drawing.Size(326, 174);
			this.textBoxReply.Style = Syncfusion.Windows.Forms.Tools.TextBoxExt.theme.Metro;
			this.textBoxReply.TabIndex = 0;
			// 
			// textBoxMessage
			// 
			this.textBoxMessage.BackColor = System.Drawing.Color.White;
			this.textBoxMessage.BeforeTouchSize = new System.Drawing.Size(326, 174);
			this.textBoxMessage.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxMessage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textBoxMessage.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.textBoxMessage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textBoxMessage.ForeColor = System.Drawing.Color.DarkSlateGray;
			this.textBoxMessage.Location = new System.Drawing.Point(109, 3);
			this.textBoxMessage.Metrocolor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxMessage.Multiline = true;
			this.textBoxMessage.Name = "textBoxMessage";
			this.textBoxMessage.ReadOnly = true;
			this.textBoxMessage.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textBoxMessage.Size = new System.Drawing.Size(326, 174);
			this.textBoxMessage.Style = Syncfusion.Windows.Forms.Tools.TextBoxExt.theme.Metro;
			this.textBoxMessage.TabIndex = 0;
			this.textBoxMessage.TabStop = false;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.label1, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.labelMessage, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.textBoxReply, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.textBoxMessage, 1, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(438, 410);
			this.tableLayoutPanel1.TabIndex = 5;
			// 
			// tableLayoutPanel3
			// 
			this.tableLayoutPanel3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.tableLayoutPanel3.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanel3.ColumnCount = 2;
			this.tableLayoutPanel1.SetColumnSpan(this.tableLayoutPanel3, 2);
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.tableLayoutPanel3.Controls.Add(this.buttonAdvCancel, 1, 0);
			this.tableLayoutPanel3.Controls.Add(this.buttonAdvReply, 0, 0);
			this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 360);
			this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel3.Name = "tableLayoutPanel3";
			this.tableLayoutPanel3.RowCount = 1;
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel3.Size = new System.Drawing.Size(438, 50);
			this.tableLayoutPanel3.TabIndex = 1;
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
			this.buttonAdvCancel.Location = new System.Drawing.Point(341, 13);
			this.buttonAdvCancel.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonAdvCancel.Name = "buttonAdvCancel";
			this.buttonAdvCancel.Size = new System.Drawing.Size(87, 27);
			this.buttonAdvCancel.TabIndex = 2;
			this.buttonAdvCancel.Text = "xxCancel";
			this.buttonAdvCancel.UseVisualStyle = true;
			this.buttonAdvCancel.UseVisualStyleBackColor = true;
			this.buttonAdvCancel.Click += new System.EventHandler(this.buttonCancelClick);
			// 
			// buttonAdvReply
			// 
			this.buttonAdvReply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvReply.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvReply.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvReply.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonAdvReply.ForeColor = System.Drawing.Color.White;
			this.buttonAdvReply.IsBackStageButton = false;
			this.buttonAdvReply.Location = new System.Drawing.Point(221, 13);
			this.buttonAdvReply.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonAdvReply.Name = "buttonAdvReply";
			this.buttonAdvReply.Size = new System.Drawing.Size(87, 27);
			this.buttonAdvReply.TabIndex = 1;
			this.buttonAdvReply.Text = "xxReply";
			this.buttonAdvReply.UseVisualStyle = true;
			this.buttonAdvReply.UseVisualStyleBackColor = true;
			this.buttonAdvReply.Click += new System.EventHandler(this.buttonReplyClick);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(8, 180);
			this.label1.Margin = new System.Windows.Forms.Padding(8, 0, 3, 0);
			this.label1.Name = "label1";
			this.label1.Padding = new System.Windows.Forms.Padding(0, 9, 0, 0);
			this.label1.Size = new System.Drawing.Size(78, 24);
			this.label1.TabIndex = 9;
			this.label1.Text = "xxReplyColon";
			// 
			// labelMessage
			// 
			this.labelMessage.AutoSize = true;
			this.labelMessage.Location = new System.Drawing.Point(8, 0);
			this.labelMessage.Margin = new System.Windows.Forms.Padding(8, 0, 3, 0);
			this.labelMessage.Name = "labelMessage";
			this.labelMessage.Padding = new System.Windows.Forms.Padding(0, 9, 0, 0);
			this.labelMessage.Size = new System.Drawing.Size(95, 24);
			this.labelMessage.TabIndex = 8;
			this.labelMessage.Text = "xxMessageColon";
			// 
			// RequestReplyStatusChangeDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BorderColor = System.Drawing.Color.Blue;
			this.CaptionFont = new System.Drawing.Font("Segoe UI", 12F);
			this.ClientSize = new System.Drawing.Size(438, 410);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.HelpButton = false;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "RequestReplyStatusChangeDialog";
			this.ShowIcon = false;
			this.Text = "xxEnterReply";
			((System.ComponentModel.ISupportInitialize)(this.textBoxReply)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.textBoxMessage)).EndInit();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.tableLayoutPanel3.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Syncfusion.Windows.Forms.Tools.TextBoxExt textBoxReply;
		  private Syncfusion.Windows.Forms.Tools.TextBoxExt textBoxMessage;
		  private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvCancel;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvReply;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label labelMessage;
	}
}