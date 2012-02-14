using Teleopti.Ccc.WinCode.Scheduling;

namespace Teleopti.Ccc.Win.Scheduling
{
	partial class RequestDetailsView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RequestDetailsView));
            this.ribbonControlAdvMain = new Syncfusion.Windows.Forms.Tools.RibbonControlAdv();
            this.toolStripTabItemResponse = new Syncfusion.Windows.Forms.Tools.ToolStripTabItem();
            this.toolStripExMain = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
            this.toolStripButtonApprove = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonDeny = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonReply = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonReplyAndApprove = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonReplyAndDeny = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonClose = new System.Windows.Forms.ToolStripButton();
            this.tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
            this.textBoxExtName = new System.Windows.Forms.Label();
            this.textBoxExtMessage = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
            this.labelName = new System.Windows.Forms.Label();
            this.textBoxExtSubject = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
            this.labelMessage = new System.Windows.Forms.Label();
            this.labelUserStatus = new System.Windows.Forms.Label();
            this.labelSubject = new System.Windows.Forms.Label();
            this.textBoxExtStatus = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdvMain)).BeginInit();
            this.ribbonControlAdvMain.SuspendLayout();
            this.toolStripTabItemResponse.Panel.SuspendLayout();
            this.toolStripExMain.SuspendLayout();
            this.tableLayoutPanelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textBoxExtMessage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textBoxExtSubject)).BeginInit();
            this.SuspendLayout();
            // 
            // ribbonControlAdvMain
            // 
            this.ribbonControlAdvMain.AllowCollapse = false;
            this.ribbonControlAdvMain.CaptionStyle = Syncfusion.Windows.Forms.Tools.CaptionStyle.Top;
            this.ribbonControlAdvMain.Header.AddMainItem(toolStripTabItemResponse);
            resources.ApplyResources(this.ribbonControlAdvMain, "ribbonControlAdvMain");
            this.ribbonControlAdvMain.MenuButtonImage = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Menu;
            this.ribbonControlAdvMain.Name = "ribbonControlAdvMain";
            // 
            // ribbonControlAdvMain.OfficeMenu
            // 
            this.ribbonControlAdvMain.OfficeMenu.MainPanel.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonClose});
            this.ribbonControlAdvMain.OfficeMenu.Name = "OfficeMenu";
            resources.ApplyResources(this.ribbonControlAdvMain.OfficeMenu, "ribbonControlAdvMain.OfficeMenu");
            this.ribbonControlAdvMain.ShowLauncher = false;
            this.ribbonControlAdvMain.SystemText.QuickAccessDialogDropDownName = "Start menu";
            // 
            // toolStripTabItemResponse
            // 
            this.toolStripTabItemResponse.Name = "toolStripTabItemResponse";
            // 
            // ribbonControlAdvMain.ribbonPanel1
            // 
            this.toolStripTabItemResponse.Panel.CaptionStyle = Syncfusion.Windows.Forms.Tools.CaptionStyle.Bottom;
            this.toolStripTabItemResponse.Panel.Controls.Add(this.toolStripExMain);
            this.toolStripTabItemResponse.Panel.Name = "ribbonPanel1";
            this.toolStripTabItemResponse.Panel.ScrollPosition = 0;
            resources.ApplyResources(this.toolStripTabItemResponse.Panel, "ribbonControlAdvMain.ribbonPanel1");
            this.toolStripTabItemResponse.Position = 0;
            resources.ApplyResources(this.toolStripTabItemResponse, "toolStripTabItemResponse");
            // 
            // toolStripExMain
            // 
            this.toolStripExMain.CaptionStyle = Syncfusion.Windows.Forms.Tools.CaptionStyle.Bottom;
            resources.ApplyResources(this.toolStripExMain, "toolStripExMain");
            this.toolStripExMain.ForeColor = System.Drawing.Color.MidnightBlue;
            this.toolStripExMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripExMain.Image = null;
            this.toolStripExMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonApprove,
            this.toolStripButtonDeny,
            this.toolStripButtonReply,
            this.toolStripButtonReplyAndApprove,
            this.toolStripButtonReplyAndDeny});
            this.toolStripExMain.Name = "toolStripExMain";
            // 
            // toolStripButtonApprove
            // 
            resources.ApplyResources(this.toolStripButtonApprove, "toolStripButtonApprove");
            this.toolStripButtonApprove.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Agent_Request_OK_32x32;
            this.toolStripButtonApprove.Name = "toolStripButtonApprove";
            this.toolStripButtonApprove.Click += new System.EventHandler(this.toolStripButtonApprove_Click);
            // 
            // toolStripButtonDeny
            // 
            resources.ApplyResources(this.toolStripButtonDeny, "toolStripButtonDeny");
            this.toolStripButtonDeny.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Agent_denyRequest_32x32;
            this.toolStripButtonDeny.Name = "toolStripButtonDeny";
            this.toolStripButtonDeny.Click += new System.EventHandler(this.toolStripButtonDeny_Click);
            // 
            // toolStripButtonReply
            // 
            resources.ApplyResources(this.toolStripButtonReply, "toolStripButtonReply");
            this.toolStripButtonReply.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Agent_ContinueDialogue_32x32;
            this.toolStripButtonReply.Name = "toolStripButtonReply";
            this.toolStripButtonReply.Click += new System.EventHandler(this.toolStripButtonReply_Click);
            // 
            // toolStripButtonReplyAndApprove
            // 
            resources.ApplyResources(this.toolStripButtonReplyAndApprove, "toolStripButtonReplyAndApprove");
            this.toolStripButtonReplyAndApprove.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Agent_NewRequest_32x32;
            this.toolStripButtonReplyAndApprove.Name = "toolStripButtonReplyAndApprove";
            this.toolStripButtonReplyAndApprove.Click += new System.EventHandler(this.toolStripButtonReplyAndApprove_Click);
            // 
            // toolStripButtonReplyAndDeny
            // 
            resources.ApplyResources(this.toolStripButtonReplyAndDeny, "toolStripButtonReplyAndDeny");
            this.toolStripButtonReplyAndDeny.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Agent_denyRequest_32x32;
            this.toolStripButtonReplyAndDeny.Name = "toolStripButtonReplyAndDeny";
            this.toolStripButtonReplyAndDeny.Click += new System.EventHandler(this.toolStripButtonReplyAndDeny_Click);
            // 
            // toolStripButtonClose
            // 
            this.toolStripButtonClose.BackColor = System.Drawing.SystemColors.Window;
            this.toolStripButtonClose.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Close;
            resources.ApplyResources(this.toolStripButtonClose, "toolStripButtonClose");
            this.toolStripButtonClose.Name = "toolStripButtonClose";
            this.toolStripButtonClose.Click += new System.EventHandler(this.toolStripButtonClose_Click);
            // 
            // tableLayoutPanelMain
            // 
            this.tableLayoutPanelMain.BackColor = System.Drawing.SystemColors.Window;
            resources.ApplyResources(this.tableLayoutPanelMain, "tableLayoutPanelMain");
            this.tableLayoutPanelMain.Controls.Add(this.textBoxExtName, 1, 0);
            this.tableLayoutPanelMain.Controls.Add(this.textBoxExtMessage, 1, 2);
            this.tableLayoutPanelMain.Controls.Add(this.labelName, 0, 0);
            this.tableLayoutPanelMain.Controls.Add(this.textBoxExtSubject, 1, 1);
            this.tableLayoutPanelMain.Controls.Add(this.labelMessage, 0, 2);
            this.tableLayoutPanelMain.Controls.Add(this.labelUserStatus, 0, 3);
            this.tableLayoutPanelMain.Controls.Add(this.labelSubject, 0, 1);
            this.tableLayoutPanelMain.Controls.Add(this.textBoxExtStatus, 1, 3);
            this.tableLayoutPanelMain.Name = "tableLayoutPanelMain";
            // 
            // textBoxExtName
            // 
            resources.ApplyResources(this.textBoxExtName, "textBoxExtName");
            this.textBoxExtName.Name = "textBoxExtName";
            // 
            // textBoxExtMessage
            // 
            this.textBoxExtMessage.AcceptsReturn = true;
            resources.ApplyResources(this.textBoxExtMessage, "textBoxExtMessage");
            this.textBoxExtMessage.Name = "textBoxExtMessage";
            this.textBoxExtMessage.OverflowIndicatorToolTipText = null;
            this.textBoxExtMessage.ReadOnly = true;
            // 
            // labelName
            // 
            resources.ApplyResources(this.labelName, "labelName");
            this.labelName.Name = "labelName";
            // 
            // textBoxExtSubject
            // 
            resources.ApplyResources(this.textBoxExtSubject, "textBoxExtSubject");
            this.textBoxExtSubject.Name = "textBoxExtSubject";
            this.textBoxExtSubject.OverflowIndicatorToolTipText = null;
            this.textBoxExtSubject.ReadOnly = true;
            // 
            // labelMessage
            // 
            resources.ApplyResources(this.labelMessage, "labelMessage");
            this.labelMessage.Name = "labelMessage";
            // 
            // labelUserStatus
            // 
            resources.ApplyResources(this.labelUserStatus, "labelUserStatus");
            this.labelUserStatus.Name = "labelUserStatus";
            // 
            // labelSubject
            // 
            resources.ApplyResources(this.labelSubject, "labelSubject");
            this.labelSubject.Name = "labelSubject";
            // 
            // textBoxExtStatus
            // 
            resources.ApplyResources(this.textBoxExtStatus, "textBoxExtStatus");
            this.textBoxExtStatus.Name = "textBoxExtStatus";
            // 
            // RequestDetailsView
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanelMain);
            this.Controls.Add(this.ribbonControlAdvMain);
            this.HelpButton = false;
            this.Name = "RequestDetailsView";
            this.Load += new System.EventHandler(this.RequestDetailsView_Load);
            this.Resize += new System.EventHandler(this.RequestDetailsView_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdvMain)).EndInit();
            this.ribbonControlAdvMain.ResumeLayout(false);
            this.ribbonControlAdvMain.PerformLayout();
            this.toolStripTabItemResponse.Panel.ResumeLayout(false);
            this.toolStripTabItemResponse.Panel.PerformLayout();
            this.toolStripExMain.ResumeLayout(false);
            this.toolStripExMain.PerformLayout();
            this.tableLayoutPanelMain.ResumeLayout(false);
            this.tableLayoutPanelMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textBoxExtMessage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textBoxExtSubject)).EndInit();
            this.ResumeLayout(false);

		}

		#endregion

		private Syncfusion.Windows.Forms.Tools.RibbonControlAdv ribbonControlAdvMain;
		private Syncfusion.Windows.Forms.Tools.ToolStripTabItem toolStripTabItemResponse;
		private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExMain;
		private System.Windows.Forms.ToolStripButton toolStripButtonApprove;
		private System.Windows.Forms.ToolStripButton toolStripButtonDeny;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelMain;
		private System.Windows.Forms.Label textBoxExtName;
		private Syncfusion.Windows.Forms.Tools.TextBoxExt textBoxExtMessage;
		private System.Windows.Forms.Label labelName;
		private Syncfusion.Windows.Forms.Tools.TextBoxExt textBoxExtSubject;
		private System.Windows.Forms.Label labelMessage;
		private System.Windows.Forms.Label labelUserStatus;
		private System.Windows.Forms.Label labelSubject;
		private System.Windows.Forms.Label textBoxExtStatus;
		private System.Windows.Forms.ToolStripButton toolStripButtonClose;
		private System.Windows.Forms.ToolStripButton toolStripButtonReply;
		private System.Windows.Forms.ToolStripButton toolStripButtonReplyAndApprove;
		private System.Windows.Forms.ToolStripButton toolStripButtonReplyAndDeny;
	}
}