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
			this.toolStripTabItemResponse = new Syncfusion.Windows.Forms.Tools.ToolStripTabItem();
			this.toolStripExMain = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripButtonApprove = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonDeny = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonReply = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonReplyAndApprove = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonReplyAndDeny = new System.Windows.Forms.ToolStripButton();
			this.ribbonControlAdvMain = new Syncfusion.Windows.Forms.Tools.RibbonControlAdv();
			this.tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
			this.textBoxExtName = new System.Windows.Forms.Label();
			this.textBoxExtMessage = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
			this.labelName = new System.Windows.Forms.Label();
			this.textBoxExtSubject = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
			this.labelMessage = new System.Windows.Forms.Label();
			this.labelUserStatus = new System.Windows.Forms.Label();
			this.labelSubject = new System.Windows.Forms.Label();
			this.textBoxExtStatus = new System.Windows.Forms.Label();
			this.toolStripTabItemResponse.Panel.SuspendLayout();
			this.toolStripExMain.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdvMain)).BeginInit();
			this.ribbonControlAdvMain.SuspendLayout();
			this.tableLayoutPanelMain.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.textBoxExtMessage)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.textBoxExtSubject)).BeginInit();
			this.SuspendLayout();
			// 
			// toolStripTabItemResponse
			// 
			this.toolStripTabItemResponse.Name = "toolStripTabItemResponse";
			// 
			// ribbonControlAdvMain.ribbonPanel1
			// 
			this.toolStripTabItemResponse.Panel.CaptionStyle = Syncfusion.Windows.Forms.Tools.CaptionStyle.Bottom;
			this.toolStripTabItemResponse.Panel.Controls.Add(this.toolStripExMain);
			resources.ApplyResources(this.toolStripTabItemResponse.Panel, "ribbonControlAdvMain.ribbonPanel1");
			this.toolStripTabItemResponse.Panel.Name = "ribbonPanel1";
			this.toolStripTabItemResponse.Panel.ScrollPosition = 0;
			this.toolStripTabItemResponse.Position = 0;
			this.SetShortcut(this.toolStripTabItemResponse, System.Windows.Forms.Keys.None);
			resources.ApplyResources(this.toolStripTabItemResponse, "toolStripTabItemResponse");
			// 
			// toolStripExMain
			// 
			this.toolStripExMain.CaptionStyle = Syncfusion.Windows.Forms.Tools.CaptionStyle.Bottom;
			resources.ApplyResources(this.toolStripExMain, "toolStripExMain");
			this.toolStripExMain.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(59)))), ((int)(((byte)(59)))));
			this.toolStripExMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripExMain.Image = null;
			this.toolStripExMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonApprove,
            this.toolStripButtonDeny,
            this.toolStripButtonReply,
            this.toolStripButtonReplyAndApprove,
            this.toolStripButtonReplyAndDeny});
			this.toolStripExMain.Name = "toolStripExMain";
			this.toolStripExMain.Office12Mode = false;
			// 
			// toolStripButtonApprove
			// 
			resources.ApplyResources(this.toolStripButtonApprove, "toolStripButtonApprove");
			this.toolStripButtonApprove.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Agent_Request_OK_32x32;
			this.toolStripButtonApprove.Name = "toolStripButtonApprove";
			this.SetShortcut(this.toolStripButtonApprove, System.Windows.Forms.Keys.None);
			this.toolStripButtonApprove.Click += new System.EventHandler(this.toolStripButtonApproveClick);
			// 
			// toolStripButtonDeny
			// 
			resources.ApplyResources(this.toolStripButtonDeny, "toolStripButtonDeny");
			this.toolStripButtonDeny.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Agent_denyRequest_32x32;
			this.toolStripButtonDeny.Name = "toolStripButtonDeny";
			this.SetShortcut(this.toolStripButtonDeny, System.Windows.Forms.Keys.None);
			this.toolStripButtonDeny.Click += new System.EventHandler(this.toolStripButtonDenyClick);
			// 
			// toolStripButtonReply
			// 
			resources.ApplyResources(this.toolStripButtonReply, "toolStripButtonReply");
			this.toolStripButtonReply.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Agent_ContinueDialogue_32x32;
			this.toolStripButtonReply.Name = "toolStripButtonReply";
			this.SetShortcut(this.toolStripButtonReply, System.Windows.Forms.Keys.None);
			this.toolStripButtonReply.Click += new System.EventHandler(this.toolStripButtonReplyClick);
			// 
			// toolStripButtonReplyAndApprove
			// 
			resources.ApplyResources(this.toolStripButtonReplyAndApprove, "toolStripButtonReplyAndApprove");
			this.toolStripButtonReplyAndApprove.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Agent_NewRequest_32x32;
			this.toolStripButtonReplyAndApprove.Name = "toolStripButtonReplyAndApprove";
			this.SetShortcut(this.toolStripButtonReplyAndApprove, System.Windows.Forms.Keys.None);
			this.toolStripButtonReplyAndApprove.Click += new System.EventHandler(this.toolStripButtonReplyAndApproveClick);
			// 
			// toolStripButtonReplyAndDeny
			// 
			resources.ApplyResources(this.toolStripButtonReplyAndDeny, "toolStripButtonReplyAndDeny");
			this.toolStripButtonReplyAndDeny.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Agent_denyRequest_32x32;
			this.toolStripButtonReplyAndDeny.Name = "toolStripButtonReplyAndDeny";
			this.SetShortcut(this.toolStripButtonReplyAndDeny, System.Windows.Forms.Keys.None);
			this.toolStripButtonReplyAndDeny.Click += new System.EventHandler(this.toolStripButtonReplyAndDenyClick);
			// 
			// ribbonControlAdvMain
			// 
			this.ribbonControlAdvMain.AllowCollapse = false;
			this.ribbonControlAdvMain.CaptionFont = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ribbonControlAdvMain.CaptionStyle = Syncfusion.Windows.Forms.Tools.CaptionStyle.Top;
			this.ribbonControlAdvMain.Header.AddMainItem(toolStripTabItemResponse);
			this.ribbonControlAdvMain.HideMenuButtonToolTip = false;
			resources.ApplyResources(this.ribbonControlAdvMain, "ribbonControlAdvMain");
			this.ribbonControlAdvMain.MaximizeToolTip = "Maximize Ribbon";
			this.ribbonControlAdvMain.MenuButtonEnabled = true;
			this.ribbonControlAdvMain.MenuButtonFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ribbonControlAdvMain.MenuButtonText = "";
			this.ribbonControlAdvMain.MenuButtonWidth = 56;
			this.ribbonControlAdvMain.MenuColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(114)))), ((int)(((byte)(198)))));
			this.ribbonControlAdvMain.MinimizeToolTip = "Minimize Ribbon";
			this.ribbonControlAdvMain.Name = "ribbonControlAdvMain";
			this.ribbonControlAdvMain.Office2013ColorScheme = Syncfusion.Windows.Forms.Tools.Office2013ColorScheme.White;
			this.ribbonControlAdvMain.OfficeColorScheme = Syncfusion.Windows.Forms.Tools.ToolStripEx.ColorScheme.Silver;
			// 
			// ribbonControlAdvMain.OfficeMenu
			// 
			this.ribbonControlAdvMain.OfficeMenu.Name = "OfficeMenu";
			resources.ApplyResources(this.ribbonControlAdvMain.OfficeMenu, "ribbonControlAdvMain.OfficeMenu");
			this.ribbonControlAdvMain.OverFlowButtonToolTip = "Show DropDown";
			this.ribbonControlAdvMain.QuickPanelImageLayout = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.ribbonControlAdvMain.QuickPanelVisible = false;
			this.ribbonControlAdvMain.RibbonHeaderImage = Syncfusion.Windows.Forms.Tools.RibbonHeaderImage.None;
			this.ribbonControlAdvMain.RibbonStyle = Syncfusion.Windows.Forms.Tools.RibbonStyle.Office2013;
			this.ribbonControlAdvMain.SelectedTab = this.toolStripTabItemResponse;
			this.ribbonControlAdvMain.Show2010CustomizeQuickItemDialog = false;
			this.ribbonControlAdvMain.ShowLauncher = false;
			this.ribbonControlAdvMain.ShowQuickItemsDropDownButton = false;
			this.ribbonControlAdvMain.ShowRibbonDisplayOptionButton = false;
			this.ribbonControlAdvMain.SystemText.QuickAccessDialogDropDownName = "Start menu";
			this.ribbonControlAdvMain.TitleAlignment = Syncfusion.Windows.Forms.Tools.TextAlignment.Center;
			this.ribbonControlAdvMain.TitleColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(59)))), ((int)(((byte)(59)))));
			this.ribbonControlAdvMain.TitleFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
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
			this.textBoxExtMessage.BeforeTouchSize = new System.Drawing.Size(809, 127);
			this.textBoxExtMessage.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.textBoxExtMessage.Metrocolor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxExtMessage.Name = "textBoxExtMessage";
			this.textBoxExtMessage.OverflowIndicatorToolTipText = null;
			this.textBoxExtMessage.ReadOnly = true;
			this.textBoxExtMessage.Style = Syncfusion.Windows.Forms.Tools.TextBoxExt.theme.Default;
			// 
			// labelName
			// 
			resources.ApplyResources(this.labelName, "labelName");
			this.labelName.Name = "labelName";
			// 
			// textBoxExtSubject
			// 
			resources.ApplyResources(this.textBoxExtSubject, "textBoxExtSubject");
			this.textBoxExtSubject.BeforeTouchSize = new System.Drawing.Size(809, 127);
			this.textBoxExtSubject.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.textBoxExtSubject.Metrocolor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxExtSubject.Name = "textBoxExtSubject";
			this.textBoxExtSubject.OverflowIndicatorToolTipText = null;
			this.textBoxExtSubject.ReadOnly = true;
			this.textBoxExtSubject.Style = Syncfusion.Windows.Forms.Tools.TextBoxExt.theme.Default;
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
			this.Borders = new System.Windows.Forms.Padding(0);
			this.ColorScheme = Syncfusion.Windows.Forms.Tools.RibbonForm.ColorSchemeType.Silver;
			this.Controls.Add(this.tableLayoutPanelMain);
			this.Controls.Add(this.ribbonControlAdvMain);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.HelpButton = false;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "RequestDetailsView";
			this.ShowIcon = false;
			this.Load += new System.EventHandler(this.requestDetailsViewLoad);
			this.Resize += new System.EventHandler(this.requestDetailsViewResize);
			this.toolStripTabItemResponse.Panel.ResumeLayout(false);
			this.toolStripTabItemResponse.Panel.PerformLayout();
			this.toolStripExMain.ResumeLayout(false);
			this.toolStripExMain.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdvMain)).EndInit();
			this.ribbonControlAdvMain.ResumeLayout(false);
			this.ribbonControlAdvMain.PerformLayout();
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
		private System.Windows.Forms.ToolStripButton toolStripButtonReply;
		private System.Windows.Forms.ToolStripButton toolStripButtonReplyAndApprove;
		private System.Windows.Forms.ToolStripButton toolStripButtonReplyAndDeny;
	}
}