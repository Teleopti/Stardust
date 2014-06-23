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
			this.webBrowser1 = new System.Windows.Forms.WebBrowser();
			this.gridWorkspace = new Teleopti.Common.UI.SmartPartControls.SmartParts.GridWorkspace();
			this.ribbonControlAdv1 = new Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed();
			this.toolStripButtonPermissions = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonMyProfile = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonHelp = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonCustomerWeb = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonSystemExit = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonAbout = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonSystemOptions = new System.Windows.Forms.ToolStripButton();
			this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
			this.notifyTimer = new System.Windows.Forms.Timer(this.components);
			this._mainStatusStrip.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
			this.splitContainer.Panel2.SuspendLayout();
			this.splitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).BeginInit();
			this.SuspendLayout();
			// 
			// _mainStatusStrip
			// 
			this._mainStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabelSpring,
            this.toolStripStatusLabelCurrentDatabase,
            this.toolStripStatusLabelLicense,
            this.toolStripStatusLabelRoger65,
            this.toolStripStatusLabelLoggedOnUser});
			this._mainStatusStrip.Location = new System.Drawing.Point(1, 526);
			this._mainStatusStrip.Name = "_mainStatusStrip";
			this._mainStatusStrip.Size = new System.Drawing.Size(820, 24);
			this._mainStatusStrip.TabIndex = 2;
			this._mainStatusStrip.Text = "yystatusStrip1";
			// 
			// toolStripStatusLabelSpring
			// 
			this.toolStripStatusLabelSpring.Name = "toolStripStatusLabelSpring";
			this.SetShortcut(this.toolStripStatusLabelSpring, System.Windows.Forms.Keys.None);
			this.toolStripStatusLabelSpring.Size = new System.Drawing.Size(564, 19);
			this.toolStripStatusLabelSpring.Spring = true;
			this.toolStripStatusLabelSpring.Click += new System.EventHandler(this.toolStripStatusLabelSpring_Click);
			// 
			// toolStripStatusLabelCurrentDatabase
			// 
			this.toolStripStatusLabelCurrentDatabase.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
			this.toolStripStatusLabelCurrentDatabase.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.toolStripStatusLabelCurrentDatabase.Name = "toolStripStatusLabelCurrentDatabase";
			this.SetShortcut(this.toolStripStatusLabelCurrentDatabase, System.Windows.Forms.Keys.None);
			this.toolStripStatusLabelCurrentDatabase.Size = new System.Drawing.Size(121, 19);
			this.toolStripStatusLabelCurrentDatabase.Text = "xxConnectedToColon";
			this.toolStripStatusLabelCurrentDatabase.Visible = false;
			// 
			// toolStripStatusLabelLicense
			// 
			this.toolStripStatusLabelLicense.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
			this.toolStripStatusLabelLicense.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.toolStripStatusLabelLicense.Name = "toolStripStatusLabelLicense";
			this.SetShortcut(this.toolStripStatusLabelLicense, System.Windows.Forms.Keys.None);
			this.toolStripStatusLabelLicense.Size = new System.Drawing.Size(109, 19);
			this.toolStripStatusLabelLicense.Text = "xxLicensedToColon";
			// 
			// toolStripStatusLabelRoger65
			// 
			this.toolStripStatusLabelRoger65.Name = "toolStripStatusLabelRoger65";
			this.SetShortcut(this.toolStripStatusLabelRoger65, System.Windows.Forms.Keys.None);
			this.toolStripStatusLabelRoger65.Size = new System.Drawing.Size(0, 19);
			// 
			// toolStripStatusLabelLoggedOnUser
			// 
			this.toolStripStatusLabelLoggedOnUser.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
			this.toolStripStatusLabelLoggedOnUser.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
			this.toolStripStatusLabelLoggedOnUser.Name = "toolStripStatusLabelLoggedOnUser";
			this.SetShortcut(this.toolStripStatusLabelLoggedOnUser, System.Windows.Forms.Keys.None);
			this.toolStripStatusLabelLoggedOnUser.Size = new System.Drawing.Size(132, 19);
			this.toolStripStatusLabelLoggedOnUser.Text = "xxLoggedOnUserColon";
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
			this.splitContainer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
			this.splitContainer.BeforeTouchSize = 5;
			this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer.Location = new System.Drawing.Point(1, 54);
			this.splitContainer.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.splitContainer.Name = "splitContainer";
			// 
			// splitContainer.Panel1
			// 
			this.splitContainer.Panel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252))))), System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255))))));
			this.splitContainer.Panel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.splitContainer.Panel1MinSize = 30;
			// 
			// splitContainer.Panel2
			// 
			this.splitContainer.Panel2.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252))))), System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255))))));
			this.splitContainer.Panel2.Controls.Add(this.webBrowser1);
			this.splitContainer.Panel2.Controls.Add(this.gridWorkspace);
			this.splitContainer.Panel2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.splitContainer.PanelToBeCollapsed = Syncfusion.Windows.Forms.Tools.Enums.CollapsedPanel.Panel1;
			this.splitContainer.Size = new System.Drawing.Size(820, 472);
			this.splitContainer.SplitterDistance = 171;
			this.splitContainer.SplitterWidth = 5;
			this.splitContainer.Style = Syncfusion.Windows.Forms.Tools.Enums.Style.Office2007Blue;
			this.splitContainer.TabIndex = 3;
			// 
			// webBrowser1
			// 
			this.webBrowser1.Location = new System.Drawing.Point(485, 333);
			this.webBrowser1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
			this.webBrowser1.Name = "webBrowser1";
			this.webBrowser1.Size = new System.Drawing.Size(156, 132);
			this.webBrowser1.TabIndex = 5;
			this.webBrowser1.Visible = false;
			// 
			// gridWorkspace
			// 
			this.gridWorkspace.AutoSize = true;
			this.gridWorkspace.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridWorkspace.GridSize = Teleopti.Common.UI.SmartPartControls.SmartParts.GridSizeType.TwoByOne;
			this.gridWorkspace.Location = new System.Drawing.Point(0, 0);
			this.gridWorkspace.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.gridWorkspace.Name = "gridWorkspace";
			this.gridWorkspace.Size = new System.Drawing.Size(644, 472);
			this.gridWorkspace.TabIndex = 0;
			this.gridWorkspace.Tag = "0";
			this.gridWorkspace.WorkspaceGridSizeChanged += new System.EventHandler<System.EventArgs>(this.GridWorkspace_WorkspaceGridSizeChanged);
			// 
			// ribbonControlAdv1
			// 
			this.ribbonControlAdv1.CaptionFont = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ribbonControlAdv1.HideMenuButtonToolTip = false;
			this.ribbonControlAdv1.LauncherStyle = Syncfusion.Windows.Forms.Tools.LauncherStyle.Metro;
			this.ribbonControlAdv1.Location = new System.Drawing.Point(1, 1);
			this.ribbonControlAdv1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.ribbonControlAdv1.MaximizeToolTip = "Maximize Ribbon";
			this.ribbonControlAdv1.MenuButtonEnabled = true;
			this.ribbonControlAdv1.MenuButtonFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ribbonControlAdv1.MenuButtonText = "xxFILE";
			this.ribbonControlAdv1.MenuButtonWidth = 56;
			this.ribbonControlAdv1.MenuColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(114)))), ((int)(((byte)(198)))));
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
			this.ribbonControlAdv1.OfficeMenu.MainPanel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ribbonControlAdv1.OfficeMenu.MainPanel.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonPermissions,
            this.toolStripButtonMyProfile,
            this.toolStripButtonHelp,
            this.toolStripButtonCustomerWeb});
			this.ribbonControlAdv1.OfficeMenu.Name = "OfficeMenu";
			this.ribbonControlAdv1.OfficeMenu.ShowItemToolTips = true;
			this.ribbonControlAdv1.OfficeMenu.Size = new System.Drawing.Size(366, 205);
			this.ribbonControlAdv1.OfficeMenu.SystemPanel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ribbonControlAdv1.OfficeMenu.SystemPanel.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonSystemExit,
            this.toolStripButtonAbout,
            this.toolStripButtonSystemOptions});
			this.ribbonControlAdv1.OverFlowButtonToolTip = "Show DropDown";
			this.ribbonControlAdv1.QuickPanelImageLayout = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.ribbonControlAdv1.QuickPanelVisible = false;
			this.ribbonControlAdv1.RibbonHeaderImage = Syncfusion.Windows.Forms.Tools.RibbonHeaderImage.None;
			this.ribbonControlAdv1.RibbonStyle = Syncfusion.Windows.Forms.Tools.RibbonStyle.Office2013;
			this.ribbonControlAdv1.SelectedTab = null;
			this.ribbonControlAdv1.Show2010CustomizeQuickItemDialog = false;
			this.ribbonControlAdv1.ShowContextMenu = false;
			this.ribbonControlAdv1.ShowLauncher = false;
			this.ribbonControlAdv1.ShowQuickItemsDropDownButton = false;
			this.ribbonControlAdv1.ShowRibbonDisplayOptionButton = false;
			this.ribbonControlAdv1.Size = new System.Drawing.Size(824, 54);
			this.ribbonControlAdv1.SystemText.QuickAccessDialogDropDownName = "Start menu";
			this.ribbonControlAdv1.TabIndex = 1;
			this.ribbonControlAdv1.Text = "yyribbonControlAdv1";
			this.ribbonControlAdv1.TitleAlignment = Syncfusion.Windows.Forms.Tools.TextAlignment.Center;
			this.ribbonControlAdv1.TitleColor = System.Drawing.Color.Black;
			this.ribbonControlAdv1.TitleFont = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			// 
			// toolStripButtonPermissions
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonPermissions, "");
			this.toolStripButtonPermissions.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonPermissions.Image")));
			this.toolStripButtonPermissions.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripButtonPermissions.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonPermissions.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonPermissions.Name = "toolStripButtonPermissions";
			this.SetShortcut(this.toolStripButtonPermissions, System.Windows.Forms.Keys.None);
			this.toolStripButtonPermissions.Size = new System.Drawing.Size(152, 36);
			this.toolStripButtonPermissions.Text = "xxPermissions";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonPermissions, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonPermissions, false);
			this.toolStripButtonPermissions.Click += new System.EventHandler(this.toolStripButtonPermissons_Click);
			// 
			// toolStripButtonMyProfile
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonMyProfile, "");
			this.toolStripButtonMyProfile.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_PersonalSettings_32x32;
			this.toolStripButtonMyProfile.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripButtonMyProfile.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonMyProfile.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonMyProfile.Name = "toolStripButtonMyProfile";
			this.SetShortcut(this.toolStripButtonMyProfile, System.Windows.Forms.Keys.None);
			this.toolStripButtonMyProfile.Size = new System.Drawing.Size(152, 36);
			this.toolStripButtonMyProfile.Text = "xxMyProfile";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonMyProfile, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonMyProfile, false);
			this.toolStripButtonMyProfile.Click += new System.EventHandler(this.toolStripButtonMyProfile_Click);
			// 
			// toolStripButtonHelp
			// 
			this.toolStripButtonHelp.AutoToolTip = false;
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonHelp, "");
			this.toolStripButtonHelp.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.help_32;
			this.toolStripButtonHelp.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripButtonHelp.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonHelp.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonHelp.Name = "toolStripButtonHelp";
			this.SetShortcut(this.toolStripButtonHelp, System.Windows.Forms.Keys.None);
			this.toolStripButtonHelp.Size = new System.Drawing.Size(152, 36);
			this.toolStripButtonHelp.Text = "xxHelp";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonHelp, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonHelp, false);
			// 
			// toolStripButtonCustomerWeb
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonCustomerWeb, "");
			this.toolStripButtonCustomerWeb.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_People;
			this.toolStripButtonCustomerWeb.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripButtonCustomerWeb.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonCustomerWeb.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonCustomerWeb.Name = "toolStripButtonCustomerWeb";
			this.SetShortcut(this.toolStripButtonCustomerWeb, System.Windows.Forms.Keys.None);
			this.toolStripButtonCustomerWeb.Size = new System.Drawing.Size(152, 36);
			this.toolStripButtonCustomerWeb.Text = "xxSignCustomerWeb";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonCustomerWeb, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonCustomerWeb, false);
			this.toolStripButtonCustomerWeb.Click += new System.EventHandler(this.toolStripButtonCustomerWeb_Click);
			// 
			// toolStripButtonSystemExit
			// 
			this.toolStripButtonSystemExit.AutoToolTip = false;
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonSystemExit, "");
			this.toolStripButtonSystemExit.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonSystemExit.Image")));
			this.toolStripButtonSystemExit.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonSystemExit.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonSystemExit.Name = "toolStripButtonSystemExit";
			this.SetShortcut(this.toolStripButtonSystemExit, System.Windows.Forms.Keys.None);
			this.toolStripButtonSystemExit.Size = new System.Drawing.Size(130, 20);
			this.toolStripButtonSystemExit.Text = "xxExitTELEOPTICCC";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonSystemExit, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonSystemExit, false);
			// 
			// toolStripButtonAbout
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonAbout, "");
			this.toolStripButtonAbout.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.information_16;
			this.toolStripButtonAbout.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonAbout.Name = "toolStripButtonAbout";
			this.SetShortcut(this.toolStripButtonAbout, System.Windows.Forms.Keys.None);
			this.toolStripButtonAbout.Size = new System.Drawing.Size(70, 20);
			this.toolStripButtonAbout.Text = "xxAbout";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonAbout, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonAbout, false);
			this.toolStripButtonAbout.Click += new System.EventHandler(this.toolStripButtonAbout_Click);
			// 
			// toolStripButtonSystemOptions
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonSystemOptions, "");
			this.toolStripButtonSystemOptions.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonSystemOptions.Image")));
			this.toolStripButtonSystemOptions.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonSystemOptions.Name = "toolStripButtonSystemOptions";
			this.SetShortcut(this.toolStripButtonSystemOptions, System.Windows.Forms.Keys.None);
			this.toolStripButtonSystemOptions.Size = new System.Drawing.Size(79, 20);
			this.toolStripButtonSystemOptions.Text = "xxOptions";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonSystemOptions, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonSystemOptions, false);
			this.toolStripButtonSystemOptions.Click += new System.EventHandler(this.toolStripButtonSystemOptions_Click);
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
			// SmartClientShellForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Borders = new System.Windows.Forms.Padding(0);
			this.ClientSize = new System.Drawing.Size(822, 550);
			this.ColorScheme = Syncfusion.Windows.Forms.Tools.RibbonForm.ColorSchemeType.Silver;
			this.Controls.Add(this.splitContainer);
			this.Controls.Add(this.ribbonControlAdv1);
			this.Controls.Add(this._mainStatusStrip);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.IsMdiContainer = true;
			this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.MinimumSize = new System.Drawing.Size(370, 62);
			this.Name = "SmartClientShellForm";
			this.Padding = new System.Windows.Forms.Padding(1, 0, 1, 0);
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "xxTeleoptiRaptorColonMainNavigation";
			this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SmartClientShellForm_FormClosing);
			this.Load += new System.EventHandler(this.SmartClientShellForm_Load);
			this._mainStatusStrip.ResumeLayout(false);
			this._mainStatusStrip.PerformLayout();
			this.splitContainer.Panel2.ResumeLayout(false);
			this.splitContainer.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
			this.splitContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).EndInit();
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
        private System.Windows.Forms.ToolStripButton toolStripButtonSystemExit;
        private System.Windows.Forms.ToolStripButton toolStripButtonPermissions;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelRoger65;
        private System.Windows.Forms.ToolStripButton toolStripButtonSystemOptions;
        private System.Windows.Forms.ToolStripButton toolStripButtonAbout;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelLicense;
        private System.Windows.Forms.ToolStripButton toolStripButtonHelp;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.Timer notifyTimer;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelLoggedOnUser;
        private System.Windows.Forms.ToolStripButton toolStripButtonMyProfile;
        private System.Windows.Forms.ToolStripButton toolStripButtonCustomerWeb;
        private System.Windows.Forms.WebBrowser webBrowser1;
    }
}

