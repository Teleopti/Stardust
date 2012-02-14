namespace Teleopti.Ccc.Win.Common
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
            this.ribbonControlAdv1 = new Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed();
            this.gradientPanelMain = new Syncfusion.Windows.Forms.Tools.GradientPanel();
            this.tableLayoutPanel1.SuspendLayout();
            this.contextMenuStripListView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanelMain)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.SystemColors.Window;
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 81F));
            this.tableLayoutPanel1.Controls.Add(this.listViewDetails, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.buttonOk, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.lableMessage, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(6, 34);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 72F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(319, 275);
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
            this.listViewDetails.Location = new System.Drawing.Point(3, 75);
            this.listViewDetails.Name = "listViewDetails";
            this.listViewDetails.Size = new System.Drawing.Size(313, 169);
            this.listViewDetails.TabIndex = 1;
            this.listViewDetails.UseCompatibleStateImageBehavior = false;
            this.listViewDetails.View = System.Windows.Forms.View.Details;
            this.listViewDetails.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listViewDetails_KeyDown);
            this.listViewDetails.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listViewDetails_MouseUp);
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
            this.SetShortcut(this.ToolStripMenuItemCopy, System.Windows.Forms.Keys.None);
            this.ToolStripMenuItemCopy.Size = new System.Drawing.Size(152, 22);
            this.ToolStripMenuItemCopy.Text = "xxCopy";
            this.ToolStripMenuItemCopy.Click += new System.EventHandler(this.ToolStripMenuItemCopy_Click);
            // 
            // buttonOk
            // 
            this.buttonOk.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonOk.Location = new System.Drawing.Point(241, 250);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 22);
            this.buttonOk.TabIndex = 5;
            this.buttonOk.Text = "xxOk";
            this.buttonOk.UseVisualStyle = true;
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click_1);
            // 
            // lableMessage
            // 
            this.lableMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lableMessage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tableLayoutPanel1.SetColumnSpan(this.lableMessage, 3);
            this.lableMessage.Location = new System.Drawing.Point(3, 2);
            this.lableMessage.Name = "lableMessage";
            this.lableMessage.Size = new System.Drawing.Size(313, 67);
            this.lableMessage.TabIndex = 4;
            this.lableMessage.Text = "lableMessage";
            this.lableMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ribbonControlAdv1
            // 
            this.ribbonControlAdv1.Location = new System.Drawing.Point(1, 0);
            this.ribbonControlAdv1.MenuButtonVisible = false;
            this.ribbonControlAdv1.Name = "ribbonControlAdv1";
            // 
            // ribbonControlAdv1.OfficeMenu
            // 
            this.ribbonControlAdv1.OfficeMenu.Name = "OfficeMenu";
            this.ribbonControlAdv1.OfficeMenu.Size = new System.Drawing.Size(12, 65);
            this.ribbonControlAdv1.QuickPanelVisible = false;
            this.ribbonControlAdv1.Size = new System.Drawing.Size(329, 33);
            this.ribbonControlAdv1.SystemText.QuickAccessDialogDropDownName = "Start menu";
            this.ribbonControlAdv1.TabIndex = 4;
            this.ribbonControlAdv1.Text = "ribbonControlAdv1";
            // 
            // gradientPanelMain
            // 
            this.gradientPanelMain.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252))))), System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255))))));
            this.gradientPanelMain.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
            this.gradientPanelMain.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gradientPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gradientPanelMain.Location = new System.Drawing.Point(6, 34);
            this.gradientPanelMain.Name = "gradientPanelMain";
            this.gradientPanelMain.Size = new System.Drawing.Size(319, 275);
            this.gradientPanelMain.TabIndex = 7;
            // 
            // MessageBoxWithListView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(331, 315);
            this.Controls.Add(this.ribbonControlAdv1);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.gradientPanelMain);
            this.Name = "MessageBoxWithListView";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.contextMenuStripListView.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanelMain)).EndInit();
            this.ResumeLayout(false);

		}

		#endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed ribbonControlAdv1;
		private System.Windows.Forms.ContextMenuStrip contextMenuStripListView;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemCopy;
        private System.Windows.Forms.ListView listViewDetails;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.Label lableMessage;
        private Syncfusion.Windows.Forms.ButtonAdv buttonOk;
        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanelMain;
        private System.Windows.Forms.ColumnHeader columnHeader3;


	}
}