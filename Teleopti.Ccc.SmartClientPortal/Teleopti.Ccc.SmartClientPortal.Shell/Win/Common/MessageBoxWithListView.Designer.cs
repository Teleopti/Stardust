namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common
{
	partial class MessageBoxWithListView
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
			this.listViewDetails = new System.Windows.Forms.ListView();
			this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.contextMenuStripListView = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.ToolStripMenuItemCopy = new System.Windows.Forms.ToolStripMenuItem();
			this.buttonOk = new Syncfusion.Windows.Forms.ButtonAdv();
			this.lableMessage = new System.Windows.Forms.Label();
			this.tableLayoutPanel1.SuspendLayout();
			this.contextMenuStripListView.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.BackColor = System.Drawing.SystemColors.Window;
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 82F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 123F));
			this.tableLayoutPanel1.Controls.Add(this.listViewDetails, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.buttonOk, 2, 2);
			this.tableLayoutPanel1.Controls.Add(this.lableMessage, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 83F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(386, 367);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// listViewDetails
			// 
			this.listViewDetails.AutoArrange = false;
			this.listViewDetails.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.listViewDetails.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
				this.columnHeader1,
				this.columnHeader2,
				this.columnHeader3});
			this.tableLayoutPanel1.SetColumnSpan(this.listViewDetails, 3);
			this.listViewDetails.ContextMenuStrip = this.contextMenuStripListView;
			this.listViewDetails.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listViewDetails.FullRowSelect = true;
			this.listViewDetails.GridLines = true;
			this.listViewDetails.Location = new System.Drawing.Point(3, 86);
			this.listViewDetails.Name = "listViewDetails";
			this.listViewDetails.Size = new System.Drawing.Size(380, 238);
			this.listViewDetails.TabIndex = 1;
			this.listViewDetails.UseCompatibleStateImageBehavior = false;
			this.listViewDetails.View = System.Windows.Forms.View.Details;
			this.listViewDetails.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listViewDetailsKeyDown);
			this.listViewDetails.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listViewDetailsMouseUp);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "";
			this.columnHeader1.Width = 64;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "xxDate";
			this.columnHeader2.Width = 73;
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "xxScenario";
			this.columnHeader3.Width = 75;
			// 
			// contextMenuStripListView
			// 
			this.contextMenuStripListView.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
				this.ToolStripMenuItemCopy});
			this.contextMenuStripListView.Name = "contextMenuStripListView";
			this.contextMenuStripListView.Size = new System.Drawing.Size(113, 26);
			// 
			// ToolStripMenuItemCopy
			// 
			this.ToolStripMenuItemCopy.Enabled = false;
			this.ToolStripMenuItemCopy.Name = "ToolStripMenuItemCopy";
			this.ToolStripMenuItemCopy.Size = new System.Drawing.Size(112, 22);
			this.ToolStripMenuItemCopy.Text = "xxCopy";
			this.ToolStripMenuItemCopy.Click += new System.EventHandler(this.toolStripMenuItemCopyClick);
			// 
			// buttonOk
			// 
			this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOk.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonOk.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonOk.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonOk.ForeColor = System.Drawing.Color.White;
			this.buttonOk.IsBackStageButton = false;
			this.buttonOk.Location = new System.Drawing.Point(289, 330);
			this.buttonOk.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonOk.Name = "buttonOk";
			this.buttonOk.Size = new System.Drawing.Size(87, 27);
			this.buttonOk.TabIndex = 5;
			this.buttonOk.Text = "xxOk";
			this.buttonOk.UseVisualStyle = true;
			this.buttonOk.UseVisualStyleBackColor = true;
			this.buttonOk.Click += new System.EventHandler(this.buttonOkClick1);
			// 
			// lableMessage
			// 
			this.lableMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.lableMessage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.tableLayoutPanel1.SetColumnSpan(this.lableMessage, 3);
			this.lableMessage.Location = new System.Drawing.Point(3, 3);
			this.lableMessage.Name = "lableMessage";
			this.lableMessage.Size = new System.Drawing.Size(380, 77);
			this.lableMessage.TabIndex = 4;
			this.lableMessage.Text = "lableMessage";
			this.lableMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// MessageBoxWithListView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.CaptionFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ClientSize = new System.Drawing.Size(386, 367);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "MessageBoxWithListView";
			this.ShowIcon = false;
			this.tableLayoutPanel1.ResumeLayout(false);
			this.contextMenuStripListView.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.ContextMenuStrip contextMenuStripListView;
		private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemCopy;
		private System.Windows.Forms.ListView listViewDetails;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.Label lableMessage;
		private Syncfusion.Windows.Forms.ButtonAdv buttonOk;
		private System.Windows.Forms.ColumnHeader columnHeader3;


	}
}