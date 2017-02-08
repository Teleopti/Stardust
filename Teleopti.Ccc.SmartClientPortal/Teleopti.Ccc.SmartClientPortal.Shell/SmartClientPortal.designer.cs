﻿//----------------------------------------------------------------------------------------
// patterns & practices - Smart Client Software Factory - Guidance Package
//
// This file was generated by this guidance package as part of the solution template
//
// The FormShell class represent the main form of your application.
// 
// The default Form supplied in this guidance package provides basic UI elements 
// like:
//      - A MenuStrip
//      - A ToolStrip
//      - A StatusStrip
//      - 2 WorkSpaces (left and right) separated by a spliter
//
// There is also a subscription to the "StatusUpdate" event topic used to change the
// content of the StatusStrip
//
// For more information see: 
// ms-help://MS.VSCC.v80/MS.VSIPCC.v80/ms.practices.scsf.2007may/SCSF/html/03-01-010-How_to_Create_Smart_Client_Solutions.htm
//
// Latest version of this Guidance Package: http://go.microsoft.com/fwlink/?LinkId=62182
//----------------------------------------------------------------------------------------

using System.Windows.Forms;
using EO.WebBrowser;
using Syncfusion.Windows.Forms.Tools;

namespace Teleopti.Ccc.SmartClientPortal.Shell
{
	partial class SmartClientShellForm
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1302:DoNotHardcodeLocaleSpecificStrings", MessageId = "Start menu")]
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SmartClientShellForm));
			this._mainStatusStrip = new System.Windows.Forms.StatusStrip();
			this.toolStripStatusLabelSpring = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripStatusLabelCurrentDatabase = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripStatusLabelLicense = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripStatusLabelRoger65 = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripStatusLabelLoggedOnUser = new System.Windows.Forms.ToolStripStatusLabel();
			this._statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.splitContainer = new Syncfusion.Windows.Forms.Tools.SplitContainerAdv();
			this.outlookBarWorkSpace1 = new Teleopti.Ccc.SmartClientPortal.Shell.Controls.OutlookBarWorkSpace();
			this.webControl1 = new EO.WebBrowser.WinForm.WebControl();
			this.webView1 = new EO.WebBrowser.WebView();
			this.wfmWebControl = new EO.WebBrowser.WinForm.WebControl();
			this.webControlDataProtection = new EO.WebBrowser.WinForm.WebControl();
			this.wfmWebView = new EO.WebBrowser.WebView();
			this.gridWorkspace = new Teleopti.Common.UI.SmartPartControls.SmartParts.GridWorkspace();
			this.ribbonControlAdv1 = new Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed();
			this.backStageViewMain = new Syncfusion.Windows.Forms.BackStageView(this.components);
			this.backStage1 = new Syncfusion.Windows.Forms.BackStage();
			this.backStageButtonOptions = new Syncfusion.Windows.Forms.BackStageButton();
			this.backStageButtonPermissions = new Syncfusion.Windows.Forms.BackStageButton();
			this.backStageButtonMyProfile = new Syncfusion.Windows.Forms.BackStageButton();
			this.backStageButtonHelp = new Syncfusion.Windows.Forms.BackStageButton();
			this.backStageButtonSignCustomerWeb = new Syncfusion.Windows.Forms.BackStageButton();
			this.backStageSeparator1 = new Syncfusion.Windows.Forms.BackStageSeparator();
			this.backStageButtonAbout = new Syncfusion.Windows.Forms.BackStageButton();
			this.backStageButtonExitTELEOPTICCC = new Syncfusion.Windows.Forms.BackStageButton();
			this.tabPageAdv1 = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.toolStripTabItem1 = new Syncfusion.Windows.Forms.Tools.ToolStripTabItem();
			this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
			this.notifyTimer = new System.Windows.Forms.Timer(this.components);
			this.panel1 = new System.Windows.Forms.Panel();
			this.outlookBar1 = new Teleopti.Ccc.SmartClientPortal.Shell.Controls.OutlookBar();
			this.webViewDataProtection = new EO.WebBrowser.WebView();
			this._mainStatusStrip.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
			this.splitContainer.Panel1.SuspendLayout();
			this.splitContainer.Panel2.SuspendLayout();
			this.splitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).BeginInit();
			this.ribbonControlAdv1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.backStage1)).BeginInit();
			this.backStage1.SuspendLayout();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// _mainStatusStrip
			// 
			this._mainStatusStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(165)))), ((int)(((byte)(220)))));
			this._mainStatusStrip.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._mainStatusStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
			this._mainStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabelSpring,
            this.toolStripStatusLabelCurrentDatabase,
            this.toolStripStatusLabelLicense,
            this.toolStripStatusLabelRoger65,
            this.toolStripStatusLabelLoggedOnUser});
			this._mainStatusStrip.Location = new System.Drawing.Point(1, 737);
			this._mainStatusStrip.Name = "_mainStatusStrip";
			this._mainStatusStrip.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
			this._mainStatusStrip.Size = new System.Drawing.Size(1024, 29);
			this._mainStatusStrip.TabIndex = 2;
			this._mainStatusStrip.Text = "yystatusStrip1";
			// 
			// toolStripStatusLabelSpring
			// 
			this.toolStripStatusLabelSpring.ForeColor = System.Drawing.Color.White;
			this.toolStripStatusLabelSpring.Margin = new System.Windows.Forms.Padding(5, 3, 0, 2);
			this.toolStripStatusLabelSpring.Name = "toolStripStatusLabelSpring";
			this.SetShortcut(this.toolStripStatusLabelSpring, System.Windows.Forms.Keys.None);
			this.toolStripStatusLabelSpring.Size = new System.Drawing.Size(693, 24);
			this.toolStripStatusLabelSpring.Spring = true;
			this.toolStripStatusLabelSpring.Text = "xx";
			this.toolStripStatusLabelSpring.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// toolStripStatusLabelCurrentDatabase
			// 
			this.toolStripStatusLabelCurrentDatabase.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
			this.toolStripStatusLabelCurrentDatabase.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
			this.toolStripStatusLabelCurrentDatabase.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.toolStripStatusLabelCurrentDatabase.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.toolStripStatusLabelCurrentDatabase.ForeColor = System.Drawing.Color.White;
			this.toolStripStatusLabelCurrentDatabase.Name = "toolStripStatusLabelCurrentDatabase";
			this.SetShortcut(this.toolStripStatusLabelCurrentDatabase, System.Windows.Forms.Keys.None);
			this.toolStripStatusLabelCurrentDatabase.Size = new System.Drawing.Size(158, 24);
			this.toolStripStatusLabelCurrentDatabase.Text = "XXConnectedToColon";
			this.toolStripStatusLabelCurrentDatabase.Visible = false;
			// 
			// toolStripStatusLabelLicense
			// 
			this.toolStripStatusLabelLicense.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
			this.toolStripStatusLabelLicense.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
			this.toolStripStatusLabelLicense.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.toolStripStatusLabelLicense.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.toolStripStatusLabelLicense.ForeColor = System.Drawing.Color.White;
			this.toolStripStatusLabelLicense.Name = "toolStripStatusLabelLicense";
			this.SetShortcut(this.toolStripStatusLabelLicense, System.Windows.Forms.Keys.None);
			this.toolStripStatusLabelLicense.Size = new System.Drawing.Size(144, 24);
			this.toolStripStatusLabelLicense.Text = "XXLicensedToColon";
			// 
			// toolStripStatusLabelRoger65
			// 
			this.toolStripStatusLabelRoger65.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
			this.toolStripStatusLabelRoger65.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.toolStripStatusLabelRoger65.ForeColor = System.Drawing.Color.White;
			this.toolStripStatusLabelRoger65.Name = "toolStripStatusLabelRoger65";
			this.SetShortcut(this.toolStripStatusLabelRoger65, System.Windows.Forms.Keys.None);
			this.toolStripStatusLabelRoger65.Size = new System.Drawing.Size(69, 24);
			this.toolStripStatusLabelRoger65.Text = "Roger65";
			this.toolStripStatusLabelRoger65.Visible = false;
			// 
			// toolStripStatusLabelLoggedOnUser
			// 
			this.toolStripStatusLabelLoggedOnUser.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
			this.toolStripStatusLabelLoggedOnUser.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.toolStripStatusLabelLoggedOnUser.ForeColor = System.Drawing.Color.White;
			this.toolStripStatusLabelLoggedOnUser.Name = "toolStripStatusLabelLoggedOnUser";
			this.SetShortcut(this.toolStripStatusLabelLoggedOnUser, System.Windows.Forms.Keys.None);
			this.toolStripStatusLabelLoggedOnUser.Size = new System.Drawing.Size(165, 24);
			this.toolStripStatusLabelLoggedOnUser.Text = "XXLoggedOnUserColon";
			// 
			// _statusLabel
			// 
			this._statusLabel.Name = "_statusLabel";
			this.SetShortcut(this._statusLabel, System.Windows.Forms.Keys.None);
			this._statusLabel.Size = new System.Drawing.Size(49, 17);
			this._statusLabel.Text = "xxReady";
			// 
			// splitContainer
			// 
			this.splitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.splitContainer.BackColor = System.Drawing.SystemColors.Control;
			this.splitContainer.BeforeTouchSize = 5;
			this.splitContainer.FixedPanel = Syncfusion.Windows.Forms.Tools.Enums.FixedPanel.Panel1;
			this.splitContainer.Location = new System.Drawing.Point(0, 0);
			this.splitContainer.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.splitContainer.Name = "splitContainer";
			// 
			// splitContainer.Panel1
			// 
			this.splitContainer.Panel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))));
			this.splitContainer.Panel1.Controls.Add(this.outlookBarWorkSpace1);
			this.splitContainer.Panel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.splitContainer.Panel1MinSize = 30;
			// 
			// splitContainer.Panel2
			// 
			this.splitContainer.Panel2.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))));
			this.splitContainer.Panel2.Controls.Add(this.webControl1);
			this.splitContainer.Panel2.Controls.Add(this.wfmWebControl);
			this.splitContainer.Panel2.Controls.Add(this.webControlDataProtection);
			this.splitContainer.Panel2.Controls.Add(this.gridWorkspace);
			this.splitContainer.Panel2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.splitContainer.Size = new System.Drawing.Size(1024, 630);
			this.splitContainer.SplitterDistance = 350;
			this.splitContainer.SplitterWidth = 5;
			this.splitContainer.Style = Syncfusion.Windows.Forms.Tools.Enums.Style.Default;
			this.splitContainer.TabIndex = 3;
			// 
			// outlookBarWorkSpace1
			// 
			this.outlookBarWorkSpace1.BackColor = System.Drawing.Color.White;
			this.outlookBarWorkSpace1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.outlookBarWorkSpace1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.outlookBarWorkSpace1.Location = new System.Drawing.Point(0, 0);
			this.outlookBarWorkSpace1.Name = "outlookBarWorkSpace1";
			this.outlookBarWorkSpace1.Size = new System.Drawing.Size(350, 630);
			this.outlookBarWorkSpace1.TabIndex = 0;
			// 
			// webControl1
			// 
			this.webControl1.BackColor = System.Drawing.Color.White;
			this.webControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.webControl1.Location = new System.Drawing.Point(0, 0);
			this.webControl1.Name = "webControl1";
			this.webControl1.Size = new System.Drawing.Size(669, 630);
			this.webControl1.TabIndex = 0;
			this.webControl1.TabStop = false;
			this.webControl1.Text = "webControl1";
			this.webControl1.Visible = false;
			this.webControl1.WebView = this.webView1;
			// 
			// webView1
			// 
			this.webView1.BeforeContextMenu += new EO.WebBrowser.BeforeContextMenuHandler(this.webView1_BeforeContextMenu);
			this.webView1.Command += new EO.WebBrowser.CommandHandler(this.webView1_Command);
			this.webView1.NewWindow += new EO.WebBrowser.NewWindowHandler(this.webView1NewWindow);
			this.webView1.CertificateError += new EO.WebBrowser.CertificateErrorHandler(this.handlingCertificateErrorsWebView1);
			this.webView1.LoadFailed += new EO.WebBrowser.LoadFailedEventHandler(this.webView1LoadFailed);
			// 
			// wfmWebControl
			// 
			this.wfmWebControl.BackColor = System.Drawing.Color.White;
			this.wfmWebControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.wfmWebControl.Location = new System.Drawing.Point(0, 0);
			this.wfmWebControl.Name = "wfmWebControl";
			this.wfmWebControl.Size = new System.Drawing.Size(669, 630);
			this.wfmWebControl.TabIndex = 1;
			this.wfmWebControl.TabStop = false;
			this.wfmWebControl.Text = "webControl2";
			this.wfmWebControl.WebView = this.wfmWebView;
			// 
			// wfmWebView
			// 
			this.wfmWebView.UrlChanged += new System.EventHandler(this.wfmWebView_UrlChanged);
			this.wfmWebView.CertificateError += new EO.WebBrowser.CertificateErrorHandler(this.handlingCertificateErrorsWfmWebView);
			this.wfmWebView.LoadFailed += new EO.WebBrowser.LoadFailedEventHandler(this.handlingLoadFailedError);
			// 
			// webViewDataProtection
			// 
			this.webViewDataProtection.BeforeContextMenu += new EO.WebBrowser.BeforeContextMenuHandler(webViewDataProtection_BeforeContextMenu);
			this.webViewDataProtection.LoadFailed += new EO.WebBrowser.LoadFailedEventHandler(this.handlingLoadFailedError);
			this.webViewDataProtection.CertificateError += new EO.WebBrowser.CertificateErrorHandler(this.handlingCertificateErrorswebViewDataProtection);
			// 
			// webControlDataProtection
			// 
			this.webControlDataProtection.BackColor = System.Drawing.Color.White;
			this.webControlDataProtection.Dock = System.Windows.Forms.DockStyle.Fill;
			this.webControlDataProtection.Location = new System.Drawing.Point(0, 0);
			this.webControlDataProtection.Name = "webControlDataProtection";
			this.webControlDataProtection.Size = new System.Drawing.Size(669, 630);
			this.webControlDataProtection.TabIndex = 1;
			this.webControlDataProtection.TabStop = false;
			this.webControlDataProtection.Text = "webControlDataProtection";
			this.webControlDataProtection.WebView = this.webViewDataProtection;
			// 
			// gridWorkspace
			// 
			this.gridWorkspace.AutoSize = true;
			this.gridWorkspace.BackColor = System.Drawing.Color.White;
			this.gridWorkspace.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridWorkspace.GridSize = Teleopti.Common.UI.SmartPartControls.SmartParts.GridSizeType.TwoByOne;
			this.gridWorkspace.Location = new System.Drawing.Point(0, 0);
			this.gridWorkspace.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.gridWorkspace.Name = "gridWorkspace";
			this.gridWorkspace.Size = new System.Drawing.Size(669, 630);
			this.gridWorkspace.TabIndex = 0;
			this.gridWorkspace.Tag = "0";
			this.gridWorkspace.WorkspaceGridSizeChanged += new System.EventHandler<System.EventArgs>(this.GridWorkspace_WorkspaceGridSizeChanged);
			// 
			// ribbonControlAdv1
			// 
			this.ribbonControlAdv1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.ribbonControlAdv1.BackStageView = this.backStageViewMain;
			this.ribbonControlAdv1.BorderStyle = Syncfusion.Windows.Forms.Tools.ToolStripBorderStyle.None;
			this.ribbonControlAdv1.CaptionFont = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ribbonControlAdv1.Header.AddMainItem(toolStripTabItem1);
			this.ribbonControlAdv1.HideMenuButtonToolTip = false;
			this.ribbonControlAdv1.LauncherStyle = Syncfusion.Windows.Forms.Tools.LauncherStyle.Metro;
			this.ribbonControlAdv1.Location = new System.Drawing.Point(1, 1);
			this.ribbonControlAdv1.MaximizeToolTip = "Maximize Ribbon";
			this.ribbonControlAdv1.MenuButtonEnabled = true;
			this.ribbonControlAdv1.MenuButtonFont = new System.Drawing.Font("Segoe UI", 9F);
			this.ribbonControlAdv1.MenuButtonText = "XXFILE";
			this.ribbonControlAdv1.MenuButtonVisible = false;
			this.ribbonControlAdv1.MenuButtonWidth = 70;
			this.ribbonControlAdv1.MenuColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.ribbonControlAdv1.MinimizeToolTip = "Minimize Ribbon";
			this.ribbonControlAdv1.Name = "ribbonControlAdv1";
			this.ribbonControlAdv1.Office2013ColorScheme = Syncfusion.Windows.Forms.Tools.Office2013ColorScheme.White;
			this.ribbonControlAdv1.OfficeColorScheme = Syncfusion.Windows.Forms.Tools.ToolStripEx.ColorScheme.Silver;
			// 
			// ribbonControlAdv1.OfficeMenu
			// 
			this.ribbonControlAdv1.OfficeMenu.AuxPanel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ribbonControlAdv1.OfficeMenu.AuxPanel.MinimumSize = new System.Drawing.Size(200, 0);
			this.ribbonControlAdv1.OfficeMenu.AuxPanel.Text = "";
			this.ribbonControlAdv1.OfficeMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.ribbonControlAdv1.OfficeMenu.MainPanel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ribbonControlAdv1.OfficeMenu.Name = "OfficeMenu";
			this.ribbonControlAdv1.OfficeMenu.ShowItemToolTips = true;
			this.ribbonControlAdv1.OfficeMenu.Size = new System.Drawing.Size(366, 205);
			this.ribbonControlAdv1.OfficeMenu.SystemPanel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ribbonControlAdv1.OverFlowButtonToolTip = "Show DropDown";
			this.ribbonControlAdv1.QuickPanelImageLayout = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.ribbonControlAdv1.QuickPanelVisible = false;
			this.ribbonControlAdv1.RibbonHeaderImage = Syncfusion.Windows.Forms.Tools.RibbonHeaderImage.None;
			this.ribbonControlAdv1.RibbonStyle = Syncfusion.Windows.Forms.Tools.RibbonStyle.Office2013;
			this.ribbonControlAdv1.SelectedTab = this.toolStripTabItem1;
			this.ribbonControlAdv1.Show2010CustomizeQuickItemDialog = false;
			this.ribbonControlAdv1.ShowContextMenu = false;
			this.ribbonControlAdv1.ShowLauncher = false;
			this.ribbonControlAdv1.ShowQuickItemsDropDownButton = false;
			this.ribbonControlAdv1.ShowRibbonDisplayOptionButton = false;
			this.ribbonControlAdv1.Size = new System.Drawing.Size(1028, 67);
			this.ribbonControlAdv1.SystemText.QuickAccessDialogDropDownName = "Start menu";
			this.ribbonControlAdv1.TabIndex = 1;
			this.ribbonControlAdv1.Text = "yyribbonControlAdv1";
			this.ribbonControlAdv1.TitleAlignment = Syncfusion.Windows.Forms.Tools.TextAlignment.Center;
			this.ribbonControlAdv1.TitleColor = System.Drawing.Color.Black;
			this.ribbonControlAdv1.TitleFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			// 
			// backStageViewMain
			// 
			this.backStageViewMain.BackStage = this.backStage1;
			this.backStageViewMain.HostControl = null;
			this.backStageViewMain.HostForm = this;
			// 
			// backStage1
			// 
			this.backStage1.ActiveTabFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
			this.backStage1.AllowDrop = true;
			this.backStage1.BeforeTouchSize = new System.Drawing.Size(1023, 714);
			this.backStage1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.backStage1.Controls.Add(this.backStageButtonOptions);
			this.backStage1.Controls.Add(this.backStageButtonPermissions);
			this.backStage1.Controls.Add(this.backStageButtonMyProfile);
			this.backStage1.Controls.Add(this.backStageButtonHelp);
			this.backStage1.Controls.Add(this.backStageButtonSignCustomerWeb);
			this.backStage1.Controls.Add(this.backStageSeparator1);
			this.backStage1.Controls.Add(this.backStageButtonAbout);
			this.backStage1.Controls.Add(this.backStageButtonExitTELEOPTICCC);
			this.backStage1.Controls.Add(this.tabPageAdv1);
			this.backStage1.ItemSize = new System.Drawing.Size(186, 40);
			this.backStage1.Location = new System.Drawing.Point(0, 0);
			this.backStage1.Name = "backStage1";
			this.backStage1.OfficeColorScheme = Syncfusion.Windows.Forms.Tools.ToolStripEx.ColorScheme.Silver;
			this.backStage1.Size = new System.Drawing.Size(1023, 714);
			this.backStage1.TabIndex = 5;
			this.backStage1.Visible = false;
			// 
			// backStageButtonOptions
			// 
			this.backStageButtonOptions.Accelerator = "";
			this.backStageButtonOptions.BackColor = System.Drawing.Color.Transparent;
			this.backStageButtonOptions.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.backStageButtonOptions.IsBackStageButton = false;
			this.backStageButtonOptions.Location = new System.Drawing.Point(0, 16);
			this.backStageButtonOptions.Name = "backStageButtonOptions";
			this.backStageButtonOptions.Size = new System.Drawing.Size(110, 25);
			this.backStageButtonOptions.TabIndex = 16;
			this.backStageButtonOptions.Text = "xxOptions";
			this.backStageButtonOptions.Click += new System.EventHandler(this.toolStripButtonSystemOptions_Click);
			// 
			// backStageButtonPermissions
			// 
			this.backStageButtonPermissions.Accelerator = "";
			this.backStageButtonPermissions.BackColor = System.Drawing.Color.Transparent;
			this.backStageButtonPermissions.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.backStageButtonPermissions.IsBackStageButton = false;
			this.backStageButtonPermissions.Location = new System.Drawing.Point(0, 41);
			this.backStageButtonPermissions.Name = "backStageButtonPermissions";
			this.backStageButtonPermissions.Size = new System.Drawing.Size(110, 25);
			this.backStageButtonPermissions.TabIndex = 11;
			this.backStageButtonPermissions.Text = "xxPermissions";
			this.backStageButtonPermissions.Click += new System.EventHandler(this.toolStripButtonPermissons_Click);
			// 
			// backStageButtonMyProfile
			// 
			this.backStageButtonMyProfile.Accelerator = "";
			this.backStageButtonMyProfile.BackColor = System.Drawing.Color.Transparent;
			this.backStageButtonMyProfile.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.backStageButtonMyProfile.IsBackStageButton = false;
			this.backStageButtonMyProfile.Location = new System.Drawing.Point(0, 66);
			this.backStageButtonMyProfile.Name = "backStageButtonMyProfile";
			this.backStageButtonMyProfile.Size = new System.Drawing.Size(110, 25);
			this.backStageButtonMyProfile.TabIndex = 12;
			this.backStageButtonMyProfile.Text = "xxMyProfile";
			this.backStageButtonMyProfile.Click += new System.EventHandler(this.toolStripButtonMyProfile_Click);
			// 
			// backStageButtonHelp
			// 
			this.backStageButtonHelp.Accelerator = "";
			this.backStageButtonHelp.BackColor = System.Drawing.Color.Transparent;
			this.backStageButtonHelp.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.backStageButtonHelp.IsBackStageButton = false;
			this.backStageButtonHelp.Location = new System.Drawing.Point(0, 91);
			this.backStageButtonHelp.Name = "backStageButtonHelp";
			this.backStageButtonHelp.Size = new System.Drawing.Size(110, 25);
			this.backStageButtonHelp.TabIndex = 13;
			this.backStageButtonHelp.Text = "xxHelp";
			this.backStageButtonHelp.Click += new System.EventHandler(this.toolStripButtonHelp_Click);
			// 
			// backStageButtonSignCustomerWeb
			// 
			this.backStageButtonSignCustomerWeb.Accelerator = "";
			this.backStageButtonSignCustomerWeb.AutoSize = true;
			this.backStageButtonSignCustomerWeb.BackColor = System.Drawing.Color.Transparent;
			this.backStageButtonSignCustomerWeb.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.backStageButtonSignCustomerWeb.IsBackStageButton = false;
			this.backStageButtonSignCustomerWeb.Location = new System.Drawing.Point(-208, 116);
			this.backStageButtonSignCustomerWeb.Name = "backStageButtonSignCustomerWeb";
			this.backStageButtonSignCustomerWeb.Size = new System.Drawing.Size(156, 30);
			this.backStageButtonSignCustomerWeb.TabIndex = 14;
			this.backStageButtonSignCustomerWeb.Text = "xxSignCustomerWeb";
			this.backStageButtonSignCustomerWeb.Click += new System.EventHandler(this.toolStripButtonCustomerWeb_Click);
			// 
			// backStageSeparator1
			// 
			this.backStageSeparator1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(204)))), ((int)(((byte)(255)))));
			this.backStageSeparator1.Location = new System.Drawing.Point(39, 22);
			this.backStageSeparator1.Name = "backStageSeparator1";
			this.backStageSeparator1.Size = new System.Drawing.Size(100, 1);
			this.backStageSeparator1.TabIndex = 15;
			this.backStageSeparator1.Text = "backStageSeparator1";
			// 
			// backStageButtonAbout
			// 
			this.backStageButtonAbout.Accelerator = "";
			this.backStageButtonAbout.BackColor = System.Drawing.Color.Transparent;
			this.backStageButtonAbout.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.backStageButtonAbout.IsBackStageButton = false;
			this.backStageButtonAbout.Location = new System.Drawing.Point(0, 153);
			this.backStageButtonAbout.Name = "backStageButtonAbout";
			this.backStageButtonAbout.Size = new System.Drawing.Size(110, 25);
			this.backStageButtonAbout.TabIndex = 17;
			this.backStageButtonAbout.Text = "xxAbout";
			this.backStageButtonAbout.Click += new System.EventHandler(this.toolStripButtonAbout_Click);
			// 
			// backStageButtonExitTELEOPTICCC
			// 
			this.backStageButtonExitTELEOPTICCC.Accelerator = "";
			this.backStageButtonExitTELEOPTICCC.AutoSize = true;
			this.backStageButtonExitTELEOPTICCC.BackColor = System.Drawing.Color.Transparent;
			this.backStageButtonExitTELEOPTICCC.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.backStageButtonExitTELEOPTICCC.IsBackStageButton = false;
			this.backStageButtonExitTELEOPTICCC.Location = new System.Drawing.Point(-208, 178);
			this.backStageButtonExitTELEOPTICCC.Name = "backStageButtonExitTELEOPTICCC";
			this.backStageButtonExitTELEOPTICCC.Size = new System.Drawing.Size(146, 30);
			this.backStageButtonExitTELEOPTICCC.TabIndex = 18;
			this.backStageButtonExitTELEOPTICCC.Text = "xxExitTELEOPTICCC";
			this.backStageButtonExitTELEOPTICCC.Click += new System.EventHandler(this.toolStripButtonSystemExit_Click);
			// 
			// tabPageAdv1
			// 
			this.tabPageAdv1.Image = null;
			this.tabPageAdv1.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageAdv1.Location = new System.Drawing.Point(185, 0);
			this.tabPageAdv1.Name = "tabPageAdv1";
			this.tabPageAdv1.ShowCloseButton = true;
			this.tabPageAdv1.Size = new System.Drawing.Size(838, 714);
			this.tabPageAdv1.TabIndex = 19;
			this.tabPageAdv1.Text = "tabPageAdv1";
			this.tabPageAdv1.ThemesEnabled = false;
			// 
			// toolStripTabItem1
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripTabItem1, "");
			this.toolStripTabItem1.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.toolStripTabItem1.Name = "toolStripTabItem1";
			// 
			// ribbonControlAdv1.ribbonPanel1
			// 
			this.toolStripTabItem1.Panel.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
			this.toolStripTabItem1.Panel.Name = "ribbonPanel1";
			this.toolStripTabItem1.Panel.Padding = new System.Windows.Forms.Padding(0, 1, 41, 0);
			this.toolStripTabItem1.Panel.ScrollPosition = 0;
			this.toolStripTabItem1.Panel.TabIndex = 2;
			this.toolStripTabItem1.Panel.Text = "XXHOME";
			this.toolStripTabItem1.Position = 0;
			this.SetShortcut(this.toolStripTabItem1, System.Windows.Forms.Keys.None);
			this.toolStripTabItem1.Size = new System.Drawing.Size(90, 25);
			this.toolStripTabItem1.Tag = "1";
			this.toolStripTabItem1.Text = "XXHOME";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripTabItem1, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripTabItem1, false);
			// 
			// notifyIcon
			// 
			this.notifyIcon.Text = "notifyIcon1";
			this.notifyIcon.Visible = true;
			this.notifyIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon_MouseClick);
			// 
			// notifyTimer
			// 
			this.notifyTimer.Enabled = true;
			this.notifyTimer.Interval = 1000;
			this.notifyTimer.Tick += new System.EventHandler(this.notifyTimer_Tick);
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.outlookBar1);
			this.panel1.Controls.Add(this.splitContainer);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(1, 67);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(1024, 670);
			this.panel1.TabIndex = 9;
			// 
			// outlookBar1
			// 
			this.outlookBar1.AutoScroll = true;
			this.outlookBar1.AutoSize = true;
			this.outlookBar1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.outlookBar1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.outlookBar1.Location = new System.Drawing.Point(0, 630);
			this.outlookBar1.MinimumSize = new System.Drawing.Size(175, 40);
			this.outlookBar1.Name = "outlookBar1";
			this.outlookBar1.Padding = new System.Windows.Forms.Padding(12, 0, 12, 0);
			this.outlookBar1.Size = new System.Drawing.Size(1024, 40);
			this.outlookBar1.TabIndex = 7;
			this.outlookBar1.SelectedItemChanged += new System.EventHandler<Teleopti.Ccc.SmartClientPortal.Shell.Controls.SelectedItemChangedEventArgs>(this.outlookBar1_SelectedItemChanged);
			// 
			// SmartClientShellForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Borders = new System.Windows.Forms.Padding(0);
			this.ClientSize = new System.Drawing.Size(1026, 766);
			this.ColorScheme = Syncfusion.Windows.Forms.Tools.RibbonForm.ColorSchemeType.Silver;
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.ribbonControlAdv1);
			this.Controls.Add(this._mainStatusStrip);
			this.Controls.Add(this.backStage1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.MinimumSize = new System.Drawing.Size(507, 410);
			this.Name = "SmartClientShellForm";
			this.Padding = new System.Windows.Forms.Padding(1, 0, 1, 0);
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "xxTeleoptiRaptorColonMainNavigation";
			this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SmartClientShellForm_FormClosing);
			this.Load += new System.EventHandler(this.SmartClientShellForm_Load);
			this.Shown += new System.EventHandler(this.smartClientShellFormShown);
			this._mainStatusStrip.ResumeLayout(false);
			this._mainStatusStrip.PerformLayout();
			this.splitContainer.Panel1.ResumeLayout(false);
			this.splitContainer.Panel2.ResumeLayout(false);
			this.splitContainer.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
			this.splitContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).EndInit();
			this.ribbonControlAdv1.ResumeLayout(false);
			this.ribbonControlAdv1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.backStage1)).EndInit();
			this.backStage1.ResumeLayout(false);
			this.backStage1.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		
		#endregion

		private System.Windows.Forms.StatusStrip _mainStatusStrip;
		private System.Windows.Forms.ToolStripStatusLabel _statusLabel;
		//private System.Windows.Forms.SplitContainer splitContainer;
		private Syncfusion.Windows.Forms.Tools.SplitContainerAdv splitContainer;
		
		private Teleopti.Common.UI.SmartPartControls.SmartParts.GridWorkspace gridWorkspace;
		private Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed ribbonControlAdv1;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelCurrentDatabase;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelSpring;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelRoger65;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelLicense;
		private System.Windows.Forms.NotifyIcon notifyIcon;
		private System.Windows.Forms.Timer notifyTimer;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelLoggedOnUser;
		private Syncfusion.Windows.Forms.BackStageView backStageViewMain;
		private Syncfusion.Windows.Forms.BackStage backStage1;
		private ToolStripTabItem toolStripTabItem1;
		private Controls.OutlookBar outlookBar1;
		private System.Windows.Forms.Panel panel1;
		private Controls.OutlookBarWorkSpace outlookBarWorkSpace1;
		private Syncfusion.Windows.Forms.BackStageButton backStageButtonPermissions;
		private Syncfusion.Windows.Forms.BackStageButton backStageButtonMyProfile;
		private Syncfusion.Windows.Forms.BackStageButton backStageButtonHelp;
		private Syncfusion.Windows.Forms.BackStageButton backStageButtonSignCustomerWeb;
		private Syncfusion.Windows.Forms.BackStageSeparator backStageSeparator1;
		private Syncfusion.Windows.Forms.BackStageButton backStageButtonOptions;
		private Syncfusion.Windows.Forms.BackStageButton backStageButtonAbout;
		private Syncfusion.Windows.Forms.BackStageButton backStageButtonExitTELEOPTICCC;
		private EO.WebBrowser.WinForm.WebControl webControl1;
		private EO.WebBrowser.WebView webView1;
        private TabPageAdv tabPageAdv1;
		  private EO.WebBrowser.WinForm.WebControl wfmWebControl;
		private EO.WebBrowser.WinForm.WebControl webControlDataProtection;
		private EO.WebBrowser.WebView wfmWebView;
		private WebView webViewDataProtection;
	}
}

