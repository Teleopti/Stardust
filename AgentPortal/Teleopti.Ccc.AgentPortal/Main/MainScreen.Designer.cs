
namespace Teleopti.Ccc.AgentPortal.Main
{
    partial class MainScreen
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
            if (disposing)
            {
                UnregisterMessageBrokerSubscriptions();
                if(components != null)
                {
                    components.Dispose();
                }
                if (_clipboardControl != null)
                {
                    _clipboardControl.Dispose();
                }
                if(_clipboardControlStudentAvailability !=null)
                {
                    _clipboardControlStudentAvailability.Dispose();
                }
                if(_scheduleMessengerScreen !=null)
                {
                    _scheduleMessengerScreen.Dispose();
                }
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainScreen));
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.officeButtonExitAgentPortal = new Syncfusion.Windows.Forms.Tools.OfficeButton();
            this.officeButtonAbout = new Syncfusion.Windows.Forms.Tools.OfficeButton();
            this.officeButtonAgentPortalOptions = new Syncfusion.Windows.Forms.Tools.OfficeButton();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.addNewTemplateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editTemplateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteTemplateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripExPreferences = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
            this.splitContainerAdv1 = new Syncfusion.Windows.Forms.Tools.SplitContainerAdv();
            this.panelSchedule = new System.Windows.Forms.Panel();
            this.xpTaskBar1 = new Syncfusion.Windows.Forms.Tools.XPTaskBar();
            this.xpTaskBarBoxLegends = new Syncfusion.Windows.Forms.Tools.XPTaskBarBox();
            this.panelLegends = new System.Windows.Forms.Panel();
            this.mainStatusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabelLicense = new System.Windows.Forms.ToolStripStatusLabel();
            this.ribbonControlAdv1 = new Syncfusion.Windows.Forms.Tools.RibbonControlAdv();
            this.toolStripTabItemHome = new Syncfusion.Windows.Forms.Tools.ToolStripTabItem();
            this.toolStripExView = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
            this.toolStripButtonSchedule = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonPreference = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonViewStudentAvailability = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonScoreCard = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonRequests = new System.Windows.Forms.ToolStripButton();
            this.toolStripSplitButtonReport = new System.Windows.Forms.ToolStripSplitButton();
            this.toolStripButtonASM = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonAccounts = new System.Windows.Forms.ToolStripButton();
            this.toolStripExShow = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
            this.toolStripButtonShowRequests = new System.Windows.Forms.ToolStripButton();
            this.toolStripExMessages = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
            this.toolStripTabItemPreferences = new Syncfusion.Windows.Forms.Tools.ToolStripTabItem();
            this.toolStripExClipboard = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
            this.toolStripEx1 = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
            this.teleoptiToolStripGalleryPreferences = new Teleopti.Ccc.AgentPortal.Common.Controls.ToolStripGallery.TeleoptiToolStripGallery();
            this.toolStripExNavigate = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
            this.toolStripButtonPreviousPeriod = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonNextPeriod = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonValidate = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonMustHave = new System.Windows.Forms.ToolStripButton();
            this.toolStripExTimeBank = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
            this.toolStripButtonPlanningTimeBank = new System.Windows.Forms.ToolStripButton();
            this.toolStripTabItemRequests = new Syncfusion.Windows.Forms.Tools.ToolStripTabItem();
            this.toolStripExRequests = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
            this.toolStripButtonModify = new System.Windows.Forms.ToolStripButton();
            this.toolStripTabItemStudentAvailability = new Syncfusion.Windows.Forms.Tools.ToolStripTabItem();
            this.toolStripExSAClipboard = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
            this.toolStripEx2 = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
            this.toolStripButtonSAPreviousPeriod = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonSANextPeriod = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonSAValidate = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonExport = new System.Windows.Forms.ToolStripButton();
            this.toolStripExPreferences.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerAdv1)).BeginInit();
            this.splitContainerAdv1.Panel1.SuspendLayout();
            this.splitContainerAdv1.Panel2.SuspendLayout();
            this.splitContainerAdv1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.xpTaskBar1)).BeginInit();
            this.xpTaskBar1.SuspendLayout();
            this.xpTaskBarBoxLegends.SuspendLayout();
            this.mainStatusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).BeginInit();
            this.ribbonControlAdv1.SuspendLayout();
            this.toolStripTabItemHome.Panel.SuspendLayout();
            this.toolStripExView.SuspendLayout();
            this.toolStripExShow.SuspendLayout();
            this.toolStripTabItemPreferences.Panel.SuspendLayout();
            this.toolStripEx1.SuspendLayout();
            this.toolStripExNavigate.SuspendLayout();
            this.toolStripExTimeBank.SuspendLayout();
            this.toolStripTabItemRequests.Panel.SuspendLayout();
            this.toolStripExRequests.SuspendLayout();
            this.toolStripTabItemStudentAvailability.Panel.SuspendLayout();
            this.toolStripEx2.SuspendLayout();
            this.SuspendLayout();
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "ccc_Sort.png");
            this.imageList1.Images.SetKeyName(1, "ccc_Contract.png");
            this.imageList1.Images.SetKeyName(2, "ccc_Template_SpecialDays_32x32.png");
            this.imageList1.Images.SetKeyName(3, "ccc_Delete.png");
            this.imageList1.Images.SetKeyName(4, "ccc_Absence.png");
            // 
            // officeButtonExitAgentPortal
            // 
            this.officeButtonExitAgentPortal.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.ImageAndText;
            this.officeButtonExitAgentPortal.Image = global::Teleopti.Ccc.AgentPortal.Properties.Resources.ccc_temp_DeleteGroup;
            this.officeButtonExitAgentPortal.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.officeButtonExitAgentPortal.Name = "officeButtonExitAgentPortal";
            this.SetShortcut(this.officeButtonExitAgentPortal, System.Windows.Forms.Keys.None);
            this.officeButtonExitAgentPortal.Size = new System.Drawing.Size(135, 23);
            this.officeButtonExitAgentPortal.Text = "xxExitTELEOPTICCC";
            this.officeButtonExitAgentPortal.Click += new System.EventHandler(this.officeButtonExitAgentPortal_Click);
            // 
            // officeButtonAbout
            // 
            this.officeButtonAbout.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.ImageAndText;
            this.officeButtonAbout.Image = global::Teleopti.Ccc.AgentPortal.Properties.Resources.information_16;
            this.officeButtonAbout.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.officeButtonAbout.Name = "officeButtonAbout";
            this.SetShortcut(this.officeButtonAbout, System.Windows.Forms.Keys.None);
            this.officeButtonAbout.Size = new System.Drawing.Size(75, 23);
            this.officeButtonAbout.Text = "xxAbout";
            this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.officeButtonAbout, false);
            this.officeButtonAbout.Click += new System.EventHandler(this.officeButtonAbout_Click);
            // 
            // officeButtonAgentPortalOptions
            // 
            this.officeButtonAgentPortalOptions.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.ImageAndText;
            this.officeButtonAgentPortalOptions.Image = global::Teleopti.Ccc.AgentPortal.Properties.Resources.ccc_Settings;
            this.officeButtonAgentPortalOptions.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.officeButtonAgentPortalOptions.Name = "officeButtonAgentPortalOptions";
            this.SetShortcut(this.officeButtonAgentPortalOptions, System.Windows.Forms.Keys.None);
            this.officeButtonAgentPortalOptions.Size = new System.Drawing.Size(84, 23);
            this.officeButtonAgentPortalOptions.Text = "xxOptions";
            this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.officeButtonAgentPortalOptions, false);
            this.officeButtonAgentPortalOptions.Click += new System.EventHandler(this.officeButtonAgentPortalOptions_Click);
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addNewTemplateToolStripMenuItem,
            this.editTemplateToolStripMenuItem,
            this.deleteTemplateToolStripMenuItem});
            this.toolStripButton1.Enabled = false;
            this.toolStripButton1.Image = global::Teleopti.Ccc.AgentPortal.Properties.Resources.ccc_MeetingPlanner;
            this.toolStripButton1.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.SetShortcut(this.toolStripButton1, System.Windows.Forms.Keys.None);
            this.toolStripButton1.Size = new System.Drawing.Size(126, 102);
            this.toolStripButton1.Text = "xxChangeTemplates";
            this.toolStripButton1.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.toolStripButton1.Visible = false;
            // 
            // addNewTemplateToolStripMenuItem
            // 
            this.addNewTemplateToolStripMenuItem.Name = "addNewTemplateToolStripMenuItem";
            this.SetShortcut(this.addNewTemplateToolStripMenuItem, System.Windows.Forms.Keys.None);
            this.addNewTemplateToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.addNewTemplateToolStripMenuItem.Text = "xxAddNewTemplate";
            // 
            // editTemplateToolStripMenuItem
            // 
            this.editTemplateToolStripMenuItem.Name = "editTemplateToolStripMenuItem";
            this.SetShortcut(this.editTemplateToolStripMenuItem, System.Windows.Forms.Keys.None);
            this.editTemplateToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.editTemplateToolStripMenuItem.Text = "xxEditTemplate";
            // 
            // deleteTemplateToolStripMenuItem
            // 
            this.deleteTemplateToolStripMenuItem.Name = "deleteTemplateToolStripMenuItem";
            this.SetShortcut(this.deleteTemplateToolStripMenuItem, System.Windows.Forms.Keys.None);
            this.deleteTemplateToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.deleteTemplateToolStripMenuItem.Text = "xxDeleteTemplate";
            // 
            // toolStripExPreferences
            // 
            this.toolStripExPreferences.AutoSize = false;
            this.toolStripExPreferences.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripExPreferences.ForeColor = System.Drawing.Color.MidnightBlue;
            this.toolStripExPreferences.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripExPreferences.Image = null;
            this.toolStripExPreferences.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1});
            this.toolStripExPreferences.Location = new System.Drawing.Point(689, 1);
            this.toolStripExPreferences.Name = "toolStripExPreferences";
            this.toolStripExPreferences.Size = new System.Drawing.Size(473, 105);
            this.toolStripExPreferences.TabIndex = 3;
            this.toolStripExPreferences.Text = "xxPreferences";
            // 
            // splitContainerAdv1
            // 
            this.splitContainerAdv1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerAdv1.Location = new System.Drawing.Point(6, 161);
            this.splitContainerAdv1.Name = "splitContainerAdv1";
            // 
            // splitContainerAdv1.Panel1
            // 
            this.splitContainerAdv1.Panel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252))))), System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255))))));
            this.splitContainerAdv1.Panel1.Controls.Add(this.panelSchedule);
            // 
            // splitContainerAdv1.Panel2
            // 
            this.splitContainerAdv1.Panel2.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252))))), System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255))))));
            this.splitContainerAdv1.Panel2.Controls.Add(this.xpTaskBar1);
            this.splitContainerAdv1.PanelToBeCollapsed = Syncfusion.Windows.Forms.Tools.Enums.CollapsedPanel.Panel2;
            this.splitContainerAdv1.Size = new System.Drawing.Size(1262, 518);
            this.splitContainerAdv1.SplitterDistance = 1026;
            this.splitContainerAdv1.SplitterWidth = 4;
            this.splitContainerAdv1.Style = Syncfusion.Windows.Forms.Tools.Enums.Style.Office2007Blue;
            this.splitContainerAdv1.TabIndex = 7;
            this.splitContainerAdv1.Text = "splitContainerAdv1";
            // 
            // panelSchedule
            // 
            this.panelSchedule.BackColor = System.Drawing.Color.White;
            this.panelSchedule.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelSchedule.Location = new System.Drawing.Point(0, 0);
            this.panelSchedule.Name = "panelSchedule";
            this.panelSchedule.Size = new System.Drawing.Size(1026, 518);
            this.panelSchedule.TabIndex = 0;
            // 
            // xpTaskBar1
            // 
            this.xpTaskBar1.AutoScroll = true;
            this.xpTaskBar1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.xpTaskBar1.BorderColor = System.Drawing.Color.Black;
            this.xpTaskBar1.Controls.Add(this.xpTaskBarBoxLegends);
            this.xpTaskBar1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.xpTaskBar1.Location = new System.Drawing.Point(0, 0);
            this.xpTaskBar1.MinimumSize = new System.Drawing.Size(0, 0);
            this.xpTaskBar1.Name = "xpTaskBar1";
            this.xpTaskBar1.Padding = new System.Windows.Forms.Padding(8);
            this.xpTaskBar1.Size = new System.Drawing.Size(232, 518);
            this.xpTaskBar1.TabIndex = 0;
            this.xpTaskBar1.ThemesEnabled = true;
            this.xpTaskBar1.VerticalPadding = 12;
            // 
            // xpTaskBarBoxLegends
            // 
            this.xpTaskBarBoxLegends.AnimationDelay = 5;
            this.xpTaskBarBoxLegends.AnimationPositionsCount = 5;
            this.xpTaskBarBoxLegends.BackColor = System.Drawing.Color.Transparent;
            this.xpTaskBarBoxLegends.Controls.Add(this.panelLegends);
            this.xpTaskBarBoxLegends.HeaderBackColor = System.Drawing.Color.Transparent;
            this.xpTaskBarBoxLegends.HeaderImageIndex = -1;
            this.xpTaskBarBoxLegends.HitTaskBoxArea = false;
            this.xpTaskBarBoxLegends.HotTrackColor = System.Drawing.Color.Empty;
            this.xpTaskBarBoxLegends.ItemBackColor = System.Drawing.Color.Transparent;
            this.xpTaskBarBoxLegends.Location = new System.Drawing.Point(8, 8);
            this.xpTaskBarBoxLegends.Name = "xpTaskBarBoxLegends";
            this.xpTaskBarBoxLegends.PreferredChildPanelHeight = 200;
            this.xpTaskBarBoxLegends.Size = new System.Drawing.Size(216, 233);
            this.xpTaskBarBoxLegends.TabIndex = 0;
            this.xpTaskBarBoxLegends.Text = "xxLegends";
            // 
            // panelLegends
            // 
            this.panelLegends.AutoSize = true;
            this.panelLegends.Location = new System.Drawing.Point(2, 31);
            this.panelLegends.Name = "panelLegends";
            this.panelLegends.Size = new System.Drawing.Size(212, 200);
            this.panelLegends.TabIndex = 0;
            // 
            // mainStatusStrip
            // 
            this.mainStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabelLicense});
            this.mainStatusStrip.Location = new System.Drawing.Point(6, 679);
            this.mainStatusStrip.Name = "mainStatusStrip";
            this.mainStatusStrip.Size = new System.Drawing.Size(1262, 22);
            this.mainStatusStrip.TabIndex = 6;
            this.mainStatusStrip.Text = "statusStrip1";
            // 
            // toolStripStatusLabelLicense
            // 
            this.toolStripStatusLabelLicense.Name = "toolStripStatusLabelLicense";
            this.SetShortcut(this.toolStripStatusLabelLicense, System.Windows.Forms.Keys.None);
            this.toolStripStatusLabelLicense.Size = new System.Drawing.Size(0, 17);
            this.toolStripStatusLabelLicense.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ribbonControlAdv1
            // 
            this.ribbonControlAdv1.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ribbonControlAdv1.Header.AddMainItem(toolStripTabItemHome);
            this.ribbonControlAdv1.Header.AddMainItem(toolStripTabItemPreferences);
            this.ribbonControlAdv1.Header.AddMainItem(toolStripTabItemRequests);
            this.ribbonControlAdv1.Header.AddMainItem(toolStripTabItemStudentAvailability);
            this.ribbonControlAdv1.Location = new System.Drawing.Point(1, 0);
            this.ribbonControlAdv1.MaximizeToolTip = "Maximize Ribbon";
            this.ribbonControlAdv1.MenuButtonImage = ((System.Drawing.Image)(resources.GetObject("ribbonControlAdv1.MenuButtonImage")));
            this.ribbonControlAdv1.MenuButtonText = "";
            this.ribbonControlAdv1.MinimizeToolTip = "Minimize Ribbon";
            this.ribbonControlAdv1.Name = "ribbonControlAdv1";
            // 
            // ribbonControlAdv1.OfficeMenu
            // 
            this.ribbonControlAdv1.OfficeMenu.AuxPanel.MinimumSize = new System.Drawing.Size(200, 150);
            this.ribbonControlAdv1.OfficeMenu.AuxPanel.Text = "";
            this.ribbonControlAdv1.OfficeMenu.MainPanel.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonExport});
            this.ribbonControlAdv1.OfficeMenu.Name = "OfficeMenu";
            this.ribbonControlAdv1.OfficeMenu.Size = new System.Drawing.Size(335, 202);
            this.ribbonControlAdv1.OfficeMenu.SystemPanel.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.officeButtonExitAgentPortal,
            this.officeButtonAbout,
            this.officeButtonAgentPortalOptions});
            this.ribbonControlAdv1.QuickPanelVisible = false;
            this.ribbonControlAdv1.SelectedTab = this.toolStripTabItemPreferences;
            this.ribbonControlAdv1.ShowQuickItemsDropDownButton = false;
            this.ribbonControlAdv1.Size = new System.Drawing.Size(1272, 160);
            this.ribbonControlAdv1.SystemText.QuickAccessDialogDropDownName = "Main";
            this.ribbonControlAdv1.TabIndex = 0;
            this.ribbonControlAdv1.Text = "ribbonControlAdv1";
            // 
            // toolStripTabItemHome
            // 
            this.toolStripTabItemHome.Name = "toolStripTabItemHome";
            // 
            // ribbonControlAdv1.ribbonPanel1
            // 
            this.toolStripTabItemHome.Panel.CaptionFont = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripTabItemHome.Panel.Controls.Add(this.toolStripExView);
            this.toolStripTabItemHome.Panel.Controls.Add(this.toolStripExShow);
            this.toolStripTabItemHome.Panel.Controls.Add(this.toolStripExMessages);
            this.toolStripTabItemHome.Panel.Cursor = System.Windows.Forms.Cursors.Default;
            this.toolStripTabItemHome.Panel.Name = "ribbonPanel1";
            this.toolStripTabItemHome.Panel.OfficeColorScheme = Syncfusion.Windows.Forms.Tools.ToolStripEx.ColorScheme.Blue;
            this.toolStripTabItemHome.Panel.ScrollPosition = 0;
            this.toolStripTabItemHome.Panel.TabIndex = 2;
            this.toolStripTabItemHome.Panel.Text = "xxHome";
            this.toolStripTabItemHome.Position = 0;
            this.SetShortcut(this.toolStripTabItemHome, System.Windows.Forms.Keys.None);
            this.toolStripTabItemHome.Size = new System.Drawing.Size(51, 19);
            this.toolStripTabItemHome.Text = "xxHome";
            this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripTabItemHome, false);
            // 
            // toolStripExView
            // 
            this.toolStripExView.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripExView.ForeColor = System.Drawing.Color.MidnightBlue;
            this.toolStripExView.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripExView.Image = null;
            this.toolStripExView.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonSchedule,
            this.toolStripButtonPreference,
            this.toolStripButtonViewStudentAvailability,
            this.toolStripButtonScoreCard,
            this.toolStripButtonRequests,
            this.toolStripSplitButtonReport,
            this.toolStripButtonASM,
            this.toolStripButtonAccounts});
            this.toolStripExView.Location = new System.Drawing.Point(0, 1);
            this.toolStripExView.Name = "toolStripExView";
            this.toolStripExView.ShowLauncher = false;
            this.toolStripExView.Size = new System.Drawing.Size(669, 0);
            this.toolStripExView.TabIndex = 0;
            this.toolStripExView.Text = "xxView";
            this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripExView, false);
            // 
            // toolStripButtonSchedule
            // 
            this.toolStripButtonSchedule.Image = global::Teleopti.Ccc.AgentPortal.Properties.Resources.ccc_PeopleScehdulePeriodView;
            this.toolStripButtonSchedule.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonSchedule.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonSchedule.Name = "toolStripButtonSchedule";
            this.SetShortcut(this.toolStripButtonSchedule, System.Windows.Forms.Keys.None);
            this.toolStripButtonSchedule.Size = new System.Drawing.Size(69, 0);
            this.toolStripButtonSchedule.Text = "xxSchedule";
            this.toolStripButtonSchedule.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonSchedule, false);
            this.toolStripButtonSchedule.Click += new System.EventHandler(this.toolStripButtonSchedule_Click);
            // 
            // toolStripButtonPreference
            // 
            this.toolStripButtonPreference.Image = global::Teleopti.Ccc.AgentPortal.Properties.Resources.ccc_Contract;
            this.toolStripButtonPreference.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonPreference.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonPreference.Name = "toolStripButtonPreference";
            this.SetShortcut(this.toolStripButtonPreference, System.Windows.Forms.Keys.None);
            this.toolStripButtonPreference.Size = new System.Drawing.Size(77, 0);
            this.toolStripButtonPreference.Text = "xxPreference";
            this.toolStripButtonPreference.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonPreference, false);
            this.toolStripButtonPreference.Click += new System.EventHandler(this.toolStripButtonPreference_Click);
            // 
            // toolStripButtonViewStudentAvailability
            // 
            this.toolStripButtonViewStudentAvailability.Image = global::Teleopti.Ccc.AgentPortal.Properties.Resources.ccc_Contract;
            this.toolStripButtonViewStudentAvailability.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonViewStudentAvailability.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonViewStudentAvailability.Name = "toolStripButtonViewStudentAvailability";
            this.SetShortcut(this.toolStripButtonViewStudentAvailability, System.Windows.Forms.Keys.None);
            this.toolStripButtonViewStudentAvailability.Size = new System.Drawing.Size(79, 0);
            this.toolStripButtonViewStudentAvailability.Text = "xxAvailability";
            this.toolStripButtonViewStudentAvailability.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonViewStudentAvailability, false);
            this.toolStripButtonViewStudentAvailability.Click += new System.EventHandler(this.toolStripButtonViewStudentAvailability_Click);
            // 
            // toolStripButtonScoreCard
            // 
            this.toolStripButtonScoreCard.Image = global::Teleopti.Ccc.AgentPortal.Properties.Resources.ccc_Sort;
            this.toolStripButtonScoreCard.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonScoreCard.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonScoreCard.Name = "toolStripButtonScoreCard";
            this.SetShortcut(this.toolStripButtonScoreCard, System.Windows.Forms.Keys.None);
            this.toolStripButtonScoreCard.Size = new System.Drawing.Size(75, 0);
            this.toolStripButtonScoreCard.Text = "xxScoreCard";
            this.toolStripButtonScoreCard.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonScoreCard, false);
            this.toolStripButtonScoreCard.Click += new System.EventHandler(this.toolStripButtonScoreCard_Click);
            // 
            // toolStripButtonRequests
            // 
            this.toolStripButtonRequests.Image = global::Teleopti.Ccc.AgentPortal.Properties.Resources.ccc_temp_Export;
            this.toolStripButtonRequests.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonRequests.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonRequests.Name = "toolStripButtonRequests";
            this.SetShortcut(this.toolStripButtonRequests, System.Windows.Forms.Keys.None);
            this.toolStripButtonRequests.Size = new System.Drawing.Size(95, 0);
            this.toolStripButtonRequests.Text = "xxRequests";
            this.toolStripButtonRequests.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonRequests, false);
            this.toolStripButtonRequests.Click += new System.EventHandler(this.toolStripButtonRequests_Click);
            // 
            // toolStripSplitButtonReport
            // 
            this.toolStripSplitButtonReport.Image = global::Teleopti.Ccc.AgentPortal.Properties.Resources.ccc_Reports;
            this.toolStripSplitButtonReport.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripSplitButtonReport.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripSplitButtonReport.Name = "toolStripSplitButtonReport";
            this.SetShortcut(this.toolStripSplitButtonReport, System.Windows.Forms.Keys.None);
            this.toolStripSplitButtonReport.Size = new System.Drawing.Size(73, 0);
            this.toolStripSplitButtonReport.Text = "xxReports";
            this.toolStripSplitButtonReport.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripSplitButtonReport, false);
            // 
            // toolStripButtonASM
            // 
            this.toolStripButtonASM.Image = global::Teleopti.Ccc.AgentPortal.Properties.Resources.ccc_SkillTime;
            this.toolStripButtonASM.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonASM.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonASM.Name = "toolStripButtonASM";
            this.SetShortcut(this.toolStripButtonASM, System.Windows.Forms.Keys.None);
            this.toolStripButtonASM.Size = new System.Drawing.Size(46, 0);
            this.toolStripButtonASM.Text = "xxASM";
            this.toolStripButtonASM.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonASM, false);
            this.toolStripButtonASM.Click += new System.EventHandler(this.toolStripButtonASM_Click);
            // 
            // toolStripButtonAccounts
            // 
            this.toolStripButtonAccounts.Image = global::Teleopti.Ccc.AgentPortal.Properties.Resources.ccc_PersonalAccount;
            this.toolStripButtonAccounts.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonAccounts.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonAccounts.Name = "toolStripButtonAccounts";
            this.SetShortcut(this.toolStripButtonAccounts, System.Windows.Forms.Keys.None);
            this.toolStripButtonAccounts.Size = new System.Drawing.Size(107, 0);
            this.toolStripButtonAccounts.Text = "xxPersonAccounts";
            this.toolStripButtonAccounts.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonAccounts, false);
            this.toolStripButtonAccounts.Click += new System.EventHandler(this.toolStripButtonAccounts_Click);
            // 
            // toolStripExShow
            // 
            this.toolStripExShow.AutoSize = false;
            this.toolStripExShow.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripExShow.ForeColor = System.Drawing.Color.MidnightBlue;
            this.toolStripExShow.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripExShow.Image = null;
            this.toolStripExShow.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonShowRequests});
            this.toolStripExShow.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStripExShow.Location = new System.Drawing.Point(671, 1);
            this.toolStripExShow.Name = "toolStripExShow";
            this.toolStripExShow.ShowCaption = true;
            this.toolStripExShow.ShowLauncher = false;
            this.toolStripExShow.Size = new System.Drawing.Size(206, 0);
            this.toolStripExShow.TabIndex = 2;
            this.toolStripExShow.Text = "xxShow";
            this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripExShow, false);
            // 
            // toolStripButtonShowRequests
            // 
            this.toolStripButtonShowRequests.CheckOnClick = true;
            this.toolStripButtonShowRequests.Image = global::Teleopti.Ccc.AgentPortal.Properties.Resources.ccc_MeetingPlanner;
            this.toolStripButtonShowRequests.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonShowRequests.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonShowRequests.Name = "toolStripButtonShowRequests";
            this.SetShortcut(this.toolStripButtonShowRequests, System.Windows.Forms.Keys.None);
            this.toolStripButtonShowRequests.Size = new System.Drawing.Size(68, 0);
            this.toolStripButtonShowRequests.Text = "xxRequests";
            this.toolStripButtonShowRequests.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonShowRequests, false);
            this.toolStripButtonShowRequests.Click += new System.EventHandler(this.toolStripButtonShowRequests_Click);
            // 
            // toolStripExMessages
            // 
            this.toolStripExMessages.AutoSize = false;
            this.toolStripExMessages.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripExMessages.ForeColor = System.Drawing.Color.MidnightBlue;
            this.toolStripExMessages.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripExMessages.Image = null;
            this.toolStripExMessages.Location = new System.Drawing.Point(879, 1);
            this.toolStripExMessages.Name = "toolStripExMessages";
            this.toolStripExMessages.Padding = new System.Windows.Forms.Padding(10);
            this.toolStripExMessages.ShowLauncher = false;
            this.toolStripExMessages.Size = new System.Drawing.Size(78, 0);
            this.toolStripExMessages.TabIndex = 4;
            this.toolStripExMessages.Text = "xxMsg";
            this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripExMessages, false);
            // 
            // toolStripTabItemPreferences
            // 
            this.toolStripTabItemPreferences.Name = "toolStripTabItemPreferences";
            // 
            // ribbonControlAdv1.ribbonPanel2
            // 
            this.toolStripTabItemPreferences.Panel.Controls.Add(this.toolStripExClipboard);
            this.toolStripTabItemPreferences.Panel.Controls.Add(this.toolStripEx1);
            this.toolStripTabItemPreferences.Panel.Controls.Add(this.toolStripExNavigate);
            this.toolStripTabItemPreferences.Panel.Controls.Add(this.toolStripExTimeBank);
            this.toolStripTabItemPreferences.Panel.Name = "ribbonPanel2";
            this.toolStripTabItemPreferences.Panel.ScrollPosition = 0;
            this.toolStripTabItemPreferences.Panel.TabIndex = 4;
            this.toolStripTabItemPreferences.Panel.Text = "xxPreferences";
            this.toolStripTabItemPreferences.Position = 1;
            this.SetShortcut(this.toolStripTabItemPreferences, System.Windows.Forms.Keys.None);
            this.toolStripTabItemPreferences.Size = new System.Drawing.Size(80, 19);
            this.toolStripTabItemPreferences.Text = "xxPreferences";
            this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripTabItemPreferences, false);
            this.toolStripTabItemPreferences.Visible = false;
            // 
            // toolStripExClipboard
            // 
            this.toolStripExClipboard.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripExClipboard.ForeColor = System.Drawing.Color.MidnightBlue;
            this.toolStripExClipboard.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripExClipboard.Image = null;
            this.toolStripExClipboard.Location = new System.Drawing.Point(0, 1);
            this.toolStripExClipboard.Name = "toolStripExClipboard";
            this.toolStripExClipboard.ShowLauncher = false;
            this.toolStripExClipboard.Size = new System.Drawing.Size(106, 98);
            this.toolStripExClipboard.TabIndex = 8;
            this.toolStripExClipboard.Text = "xxClipboard";
            this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripExClipboard, false);
            // 
            // toolStripEx1
            // 
            this.toolStripEx1.DefaultDropDownDirection = System.Windows.Forms.ToolStripDropDownDirection.BelowRight;
            this.toolStripEx1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripEx1.ForeColor = System.Drawing.Color.MidnightBlue;
            this.toolStripEx1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripEx1.Image = null;
            this.toolStripEx1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.teleoptiToolStripGalleryPreferences});
            this.toolStripEx1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStripEx1.Location = new System.Drawing.Point(108, 1);
            this.toolStripEx1.Name = "toolStripEx1";
            this.toolStripEx1.ShowLauncher = false;
            this.toolStripEx1.Size = new System.Drawing.Size(382, 98);
            this.toolStripEx1.TabIndex = 4;
            this.toolStripEx1.Text = "xxPreferences";
            this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripEx1, false);
            // 
            // teleoptiToolStripGalleryPreferences
            // 
            this.teleoptiToolStripGalleryPreferences.BorderStyle = Syncfusion.Windows.Forms.Tools.ToolstripGalleryBorderStyle.None;
            this.teleoptiToolStripGalleryPreferences.CaptionText = "";
            this.teleoptiToolStripGalleryPreferences.CheckOnClick = true;
            this.teleoptiToolStripGalleryPreferences.Dimensions = new System.Drawing.Size(5, 1);
            this.teleoptiToolStripGalleryPreferences.DropDownDimensions = new System.Drawing.Size(7, 5);
            this.teleoptiToolStripGalleryPreferences.ImageList = this.imageList1;
            this.teleoptiToolStripGalleryPreferences.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.teleoptiToolStripGalleryPreferences.ItemBackColor = System.Drawing.Color.Empty;
            this.teleoptiToolStripGalleryPreferences.ItemImageSize = new System.Drawing.Size(32, 32);
            this.teleoptiToolStripGalleryPreferences.ItemSize = new System.Drawing.Size(70, 58);
            this.teleoptiToolStripGalleryPreferences.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.teleoptiToolStripGalleryPreferences.Name = "teleoptiToolStripGalleryPreferences";
            this.teleoptiToolStripGalleryPreferences.ParentRibbonTab = this.toolStripTabItemPreferences;
            this.teleoptiToolStripGalleryPreferences.ScrollerType = Syncfusion.Windows.Forms.Tools.ToolStripGalleryScrollerType.Compact;
            this.SetShortcut(this.teleoptiToolStripGalleryPreferences, System.Windows.Forms.Keys.None);
            this.teleoptiToolStripGalleryPreferences.ShowToolTip = true;
            this.teleoptiToolStripGalleryPreferences.Size = new System.Drawing.Size(375, 60);
            this.teleoptiToolStripGalleryPreferences.Text = "yy";
            this.teleoptiToolStripGalleryPreferences.ItemClicked += new System.EventHandler<Teleopti.Ccc.AgentPortal.Common.Controls.ToolStripGallery.ToolStripItemClickedEventArgs>(this.teleoptiToolStripGalleryPreferences_ItemClicked);
            this.teleoptiToolStripGalleryPreferences.GalleryItemClicked += new Syncfusion.Windows.Forms.Tools.ToolStripGalleryItemEventHandler(this.teleoptiToolStripGalleryPreferences_GalleryItemClicked);
            // 
            // toolStripExNavigate
            // 
            this.toolStripExNavigate.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripExNavigate.ForeColor = System.Drawing.Color.MidnightBlue;
            this.toolStripExNavigate.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripExNavigate.Image = null;
            this.toolStripExNavigate.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonPreviousPeriod,
            this.toolStripButtonNextPeriod,
            this.toolStripButtonValidate,
            this.toolStripButtonMustHave});
            this.toolStripExNavigate.Location = new System.Drawing.Point(492, 1);
            this.toolStripExNavigate.Name = "toolStripExNavigate";
            this.toolStripExNavigate.ShowLauncher = false;
            this.toolStripExNavigate.Size = new System.Drawing.Size(413, 98);
            this.toolStripExNavigate.TabIndex = 5;
            this.toolStripExNavigate.Text = "xxPeriods";
            this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripExNavigate, false);
            // 
            // toolStripButtonPreviousPeriod
            // 
            this.toolStripButtonPreviousPeriod.Image = global::Teleopti.Ccc.AgentPortal.Properties.Resources.ccc_Left;
            this.toolStripButtonPreviousPeriod.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonPreviousPeriod.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonPreviousPeriod.Name = "toolStripButtonPreviousPeriod";
            this.toolStripButtonPreviousPeriod.RightToLeftAutoMirrorImage = true;
            this.SetShortcut(this.toolStripButtonPreviousPeriod, System.Windows.Forms.Keys.None);
            this.toolStripButtonPreviousPeriod.Size = new System.Drawing.Size(100, 78);
            this.toolStripButtonPreviousPeriod.Text = "xxPreviousPeriod";
            this.toolStripButtonPreviousPeriod.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.toolStripButtonPreviousPeriod.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonPreviousPeriod, false);
            this.toolStripButtonPreviousPeriod.Click += new System.EventHandler(this.toolStripButtonPreviousPeriod_Click);
            // 
            // toolStripButtonNextPeriod
            // 
            this.toolStripButtonNextPeriod.Image = global::Teleopti.Ccc.AgentPortal.Properties.Resources.ccc_Right;
            this.toolStripButtonNextPeriod.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonNextPeriod.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonNextPeriod.Name = "toolStripButtonNextPeriod";
            this.toolStripButtonNextPeriod.RightToLeftAutoMirrorImage = true;
            this.SetShortcut(this.toolStripButtonNextPeriod, System.Windows.Forms.Keys.None);
            this.toolStripButtonNextPeriod.Size = new System.Drawing.Size(79, 78);
            this.toolStripButtonNextPeriod.Text = "xxNextPeriod";
            this.toolStripButtonNextPeriod.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.toolStripButtonNextPeriod.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonNextPeriod, false);
            this.toolStripButtonNextPeriod.Click += new System.EventHandler(this.toolStripButtonNextPeriod_Click);
            // 
            // toolStripButtonValidate
            // 
            this.toolStripButtonValidate.Enabled = false;
            this.toolStripButtonValidate.Image = global::Teleopti.Ccc.AgentPortal.Properties.Resources.ccc_ForecastValidate;
            this.toolStripButtonValidate.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonValidate.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonValidate.Name = "toolStripButtonValidate";
            this.SetShortcut(this.toolStripButtonValidate, System.Windows.Forms.Keys.None);
            this.toolStripButtonValidate.Size = new System.Drawing.Size(63, 78);
            this.toolStripButtonValidate.Text = "xxValidate";
            this.toolStripButtonValidate.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonValidate, false);
            this.toolStripButtonValidate.Click += new System.EventHandler(this.toolStripButtonValidate_Click);
            // 
            // toolStripButtonMustHave
            // 
            this.toolStripButtonMustHave.CheckOnClick = true;
            this.toolStripButtonMustHave.Image = global::Teleopti.Ccc.AgentPortal.Properties.Resources.heart;
            this.toolStripButtonMustHave.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonMustHave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonMustHave.Name = "toolStripButtonMustHave";
            this.SetShortcut(this.toolStripButtonMustHave, System.Windows.Forms.Keys.None);
            this.toolStripButtonMustHave.Size = new System.Drawing.Size(133, 78);
            this.toolStripButtonMustHave.Text = "xxMustHaveCapitalized";
            this.toolStripButtonMustHave.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonMustHave, false);
            this.toolStripButtonMustHave.Click += new System.EventHandler(this.toolStripButtonMustHave_Click);
            // 
            // toolStripExTimeBank
            // 
            this.toolStripExTimeBank.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripExTimeBank.ForeColor = System.Drawing.Color.MidnightBlue;
            this.toolStripExTimeBank.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripExTimeBank.Image = null;
            this.toolStripExTimeBank.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonPlanningTimeBank});
            this.toolStripExTimeBank.Location = new System.Drawing.Point(907, 1);
            this.toolStripExTimeBank.Name = "toolStripExTimeBank";
            this.toolStripExTimeBank.Size = new System.Drawing.Size(170, 98);
            this.toolStripExTimeBank.TabIndex = 7;
            this.toolStripExTimeBank.Text = "xxPlanningTimeBankCaption";
            this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripExTimeBank, false);
            // 
            // toolStripButtonPlanningTimeBank
            // 
            this.toolStripButtonPlanningTimeBank.Image = global::Teleopti.Ccc.AgentPortal.Properties.Resources.ccc_Agent_schedule_32x32;
            this.toolStripButtonPlanningTimeBank.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonPlanningTimeBank.ImageTransparentColor = System.Drawing.Color.White;
            this.toolStripButtonPlanningTimeBank.Name = "toolStripButtonPlanningTimeBank";
            this.SetShortcut(this.toolStripButtonPlanningTimeBank, System.Windows.Forms.Keys.None);
            this.toolStripButtonPlanningTimeBank.Size = new System.Drawing.Size(163, 77);
            this.toolStripButtonPlanningTimeBank.Text = "xxPlanningTimeBankCaption";
            this.toolStripButtonPlanningTimeBank.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolStripButtonPlanningTimeBank.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonPlanningTimeBank, false);
            this.toolStripButtonPlanningTimeBank.Click += new System.EventHandler(this.ToolStripButtonPlanningTimeBankClick);
            // 
            // toolStripTabItemRequests
            // 
            this.toolStripTabItemRequests.Name = "toolStripTabItemRequests";
            // 
            // ribbonControlAdv1.ribbonPanel3
            // 
            this.toolStripTabItemRequests.Panel.Controls.Add(this.toolStripExRequests);
            this.toolStripTabItemRequests.Panel.Name = "ribbonPanel3";
            this.toolStripTabItemRequests.Panel.ScrollPosition = 0;
            this.toolStripTabItemRequests.Panel.TabIndex = 5;
            this.toolStripTabItemRequests.Panel.Text = "xxRequests";
            this.toolStripTabItemRequests.Position = 2;
            this.SetShortcut(this.toolStripTabItemRequests, System.Windows.Forms.Keys.None);
            this.toolStripTabItemRequests.Size = new System.Drawing.Size(68, 19);
            this.toolStripTabItemRequests.Text = "xxRequests";
            this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripTabItemRequests, false);
            this.toolStripTabItemRequests.Visible = false;
            // 
            // toolStripExRequests
            // 
            this.toolStripExRequests.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripExRequests.ForeColor = System.Drawing.Color.MidnightBlue;
            this.toolStripExRequests.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripExRequests.Image = null;
            this.toolStripExRequests.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonModify});
            this.toolStripExRequests.LauncherStyle = Syncfusion.Windows.Forms.Tools.LauncherStyle.Office2007;
            this.toolStripExRequests.Location = new System.Drawing.Point(0, 1);
            this.toolStripExRequests.Name = "toolStripExRequests";
            this.toolStripExRequests.ShowLauncher = false;
            this.toolStripExRequests.Size = new System.Drawing.Size(57, 0);
            this.toolStripExRequests.TabIndex = 0;
            this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripExRequests, false);
            // 
            // toolStripButtonModify
            // 
            this.toolStripButtonModify.Image = global::Teleopti.Ccc.AgentPortal.Properties.Resources.ccc_Settings;
            this.toolStripButtonModify.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonModify.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonModify.Name = "toolStripButtonModify";
            this.SetShortcut(this.toolStripButtonModify, System.Windows.Forms.Keys.None);
            this.toolStripButtonModify.Size = new System.Drawing.Size(50, 0);
            this.toolStripButtonModify.Text = "xxOpen";
            this.toolStripButtonModify.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonModify, false);
            this.toolStripButtonModify.Click += new System.EventHandler(this.toolStripButtonModifyRequest_Click);
            // 
            // toolStripTabItemStudentAvailability
            // 
            this.toolStripTabItemStudentAvailability.Name = "toolStripTabItemStudentAvailability";
            // 
            // ribbonControlAdv1.ribbonPanel4
            // 
            this.toolStripTabItemStudentAvailability.Panel.Controls.Add(this.toolStripExSAClipboard);
            this.toolStripTabItemStudentAvailability.Panel.Controls.Add(this.toolStripEx2);
            this.toolStripTabItemStudentAvailability.Panel.Name = "ribbonPanel4";
            this.toolStripTabItemStudentAvailability.Panel.ScrollPosition = 0;
            this.toolStripTabItemStudentAvailability.Panel.TabIndex = 6;
            this.toolStripTabItemStudentAvailability.Panel.Text = "xxAvailability";
            this.toolStripTabItemStudentAvailability.Position = 3;
            this.SetShortcut(this.toolStripTabItemStudentAvailability, System.Windows.Forms.Keys.None);
            this.toolStripTabItemStudentAvailability.Size = new System.Drawing.Size(72, 19);
            this.toolStripTabItemStudentAvailability.Text = "xxAvailability";
            this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripTabItemStudentAvailability, false);
            this.toolStripTabItemStudentAvailability.Visible = false;
            // 
            // toolStripExSAClipboard
            // 
            this.toolStripExSAClipboard.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripExSAClipboard.ForeColor = System.Drawing.Color.MidnightBlue;
            this.toolStripExSAClipboard.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripExSAClipboard.Image = null;
            this.toolStripExSAClipboard.Location = new System.Drawing.Point(0, 1);
            this.toolStripExSAClipboard.Name = "toolStripExSAClipboard";
            this.toolStripExSAClipboard.ShowLauncher = false;
            this.toolStripExSAClipboard.Size = new System.Drawing.Size(106, 98);
            this.toolStripExSAClipboard.TabIndex = 9;
            this.toolStripExSAClipboard.Text = "xxClipboard";
            this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripExSAClipboard, false);
            // 
            // toolStripEx2
            // 
            this.toolStripEx2.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripEx2.ForeColor = System.Drawing.Color.MidnightBlue;
            this.toolStripEx2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripEx2.Image = null;
            this.toolStripEx2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonSAPreviousPeriod,
            this.toolStripButtonSANextPeriod,
            this.toolStripButtonSAValidate});
            this.toolStripEx2.Location = new System.Drawing.Point(108, 1);
            this.toolStripEx2.Name = "toolStripEx2";
            this.toolStripEx2.ShowLauncher = false;
            this.toolStripEx2.Size = new System.Drawing.Size(249, 98);
            this.toolStripEx2.TabIndex = 8;
            this.toolStripEx2.Text = "xxPeriods";
            this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripEx2, false);
            // 
            // toolStripButtonSAPreviousPeriod
            // 
            this.toolStripButtonSAPreviousPeriod.Image = global::Teleopti.Ccc.AgentPortal.Properties.Resources.ccc_Left;
            this.toolStripButtonSAPreviousPeriod.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonSAPreviousPeriod.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonSAPreviousPeriod.Name = "toolStripButtonSAPreviousPeriod";
            this.toolStripButtonSAPreviousPeriod.RightToLeftAutoMirrorImage = true;
            this.SetShortcut(this.toolStripButtonSAPreviousPeriod, System.Windows.Forms.Keys.None);
            this.toolStripButtonSAPreviousPeriod.Size = new System.Drawing.Size(100, 78);
            this.toolStripButtonSAPreviousPeriod.Text = "xxPreviousPeriod";
            this.toolStripButtonSAPreviousPeriod.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.toolStripButtonSAPreviousPeriod.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonSAPreviousPeriod, false);
            this.toolStripButtonSAPreviousPeriod.Click += new System.EventHandler(this.toolStripButtonSAPreviousPeriod_Click);
            // 
            // toolStripButtonSANextPeriod
            // 
            this.toolStripButtonSANextPeriod.Image = global::Teleopti.Ccc.AgentPortal.Properties.Resources.ccc_Right;
            this.toolStripButtonSANextPeriod.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonSANextPeriod.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonSANextPeriod.Name = "toolStripButtonSANextPeriod";
            this.toolStripButtonSANextPeriod.RightToLeftAutoMirrorImage = true;
            this.SetShortcut(this.toolStripButtonSANextPeriod, System.Windows.Forms.Keys.None);
            this.toolStripButtonSANextPeriod.Size = new System.Drawing.Size(79, 78);
            this.toolStripButtonSANextPeriod.Text = "xxNextPeriod";
            this.toolStripButtonSANextPeriod.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.toolStripButtonSANextPeriod.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonSANextPeriod, false);
            this.toolStripButtonSANextPeriod.Click += new System.EventHandler(this.toolStripButtonSANextPeriod_Click);
            // 
            // toolStripButtonSAValidate
            // 
            this.toolStripButtonSAValidate.Enabled = false;
            this.toolStripButtonSAValidate.Image = global::Teleopti.Ccc.AgentPortal.Properties.Resources.ccc_ForecastValidate;
            this.toolStripButtonSAValidate.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonSAValidate.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonSAValidate.Name = "toolStripButtonSAValidate";
            this.SetShortcut(this.toolStripButtonSAValidate, System.Windows.Forms.Keys.None);
            this.toolStripButtonSAValidate.Size = new System.Drawing.Size(63, 78);
            this.toolStripButtonSAValidate.Text = "xxValidate";
            this.toolStripButtonSAValidate.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonSAValidate, false);
            this.toolStripButtonSAValidate.Click += new System.EventHandler(this.toolStripButtonSAValidate_Click);
            // 
            // toolStripButtonExport
            // 
            this.toolStripButtonExport.Image = global::Teleopti.Ccc.AgentPortal.Properties.Resources.ccc_Print;
            this.toolStripButtonExport.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolStripButtonExport.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripButtonExport.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonExport.Name = "toolStripButtonExport";
            this.SetShortcut(this.toolStripButtonExport, System.Windows.Forms.Keys.None);
            this.toolStripButtonExport.Size = new System.Drawing.Size(121, 36);
            this.toolStripButtonExport.Text = "xxExportToPDF";
            this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonExport, false);
            this.toolStripButtonExport.Click += new System.EventHandler(this.toolStripButtonExport_Click);
            // 
            // MainScreen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Borders = new System.Windows.Forms.Padding(6, 1, 6, 2);
            this.ClientSize = new System.Drawing.Size(1274, 703);
            this.Controls.Add(this.splitContainerAdv1);
            this.Controls.Add(this.mainStatusStrip);
            this.Controls.Add(this.ribbonControlAdv1);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HelpButtonImage = ((System.Drawing.Image)(resources.GetObject("$this.HelpButtonImage")));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainScreen";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "xxAgentPortal";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainScreen_FormClosing);
            this.Load += new System.EventHandler(this.MainScreen_Load);
            this.toolStripExPreferences.ResumeLayout(false);
            this.toolStripExPreferences.PerformLayout();
            this.splitContainerAdv1.Panel1.ResumeLayout(false);
            this.splitContainerAdv1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerAdv1)).EndInit();
            this.splitContainerAdv1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.xpTaskBar1)).EndInit();
            this.xpTaskBar1.ResumeLayout(false);
            this.xpTaskBarBoxLegends.ResumeLayout(false);
            this.xpTaskBarBoxLegends.PerformLayout();
            this.mainStatusStrip.ResumeLayout(false);
            this.mainStatusStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).EndInit();
            this.ribbonControlAdv1.ResumeLayout(false);
            this.ribbonControlAdv1.PerformLayout();
            this.toolStripTabItemHome.Panel.ResumeLayout(false);
            this.toolStripTabItemHome.Panel.PerformLayout();
            this.toolStripExView.ResumeLayout(false);
            this.toolStripExView.PerformLayout();
            this.toolStripExShow.ResumeLayout(false);
            this.toolStripExShow.PerformLayout();
            this.toolStripTabItemPreferences.Panel.ResumeLayout(false);
            this.toolStripTabItemPreferences.Panel.PerformLayout();
            this.toolStripEx1.ResumeLayout(false);
            this.toolStripEx1.PerformLayout();
            this.toolStripExNavigate.ResumeLayout(false);
            this.toolStripExNavigate.PerformLayout();
            this.toolStripExTimeBank.ResumeLayout(false);
            this.toolStripExTimeBank.PerformLayout();
            this.toolStripTabItemRequests.Panel.ResumeLayout(false);
            this.toolStripTabItemRequests.Panel.PerformLayout();
            this.toolStripExRequests.ResumeLayout(false);
            this.toolStripExRequests.PerformLayout();
            this.toolStripTabItemStudentAvailability.Panel.ResumeLayout(false);
            this.toolStripTabItemStudentAvailability.Panel.PerformLayout();
            this.toolStripEx2.ResumeLayout(false);
            this.toolStripEx2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.RibbonControlAdv ribbonControlAdv1;
        private Syncfusion.Windows.Forms.Tools.ToolStripTabItem toolStripTabItemHome;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExView;
        private System.Windows.Forms.ToolStripButton toolStripButtonSchedule;
        private System.Windows.Forms.ToolStripButton toolStripButtonScoreCard;
        private System.Windows.Forms.ToolStripButton toolStripButtonASM;
        private Syncfusion.Windows.Forms.Tools.OfficeButton officeButtonAgentPortalOptions;
        private Syncfusion.Windows.Forms.Tools.OfficeButton officeButtonExitAgentPortal;
        private System.Windows.Forms.ToolStripButton toolStripButtonRequests;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExShow;
        private System.Windows.Forms.ToolStripButton toolStripButtonShowRequests;
        private System.Windows.Forms.ToolStripSplitButton toolStripSplitButtonReport;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.StatusStrip mainStatusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelLicense;
        private Syncfusion.Windows.Forms.Tools.SplitContainerAdv splitContainerAdv1;
        private System.Windows.Forms.Panel panelSchedule;
        private Syncfusion.Windows.Forms.Tools.XPTaskBar xpTaskBar1;
        private Syncfusion.Windows.Forms.Tools.XPTaskBarBox xpTaskBarBoxLegends;
        private System.Windows.Forms.Panel panelLegends;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExMessages;
        private Syncfusion.Windows.Forms.Tools.OfficeButton officeButtonAbout;
        private System.Windows.Forms.ToolStripButton toolStripButtonExport;
        private System.Windows.Forms.ToolStripButton toolStripButtonPreference;
        private Syncfusion.Windows.Forms.Tools.ToolStripTabItem toolStripTabItemPreferences;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripEx1;
        private Teleopti.Ccc.AgentPortal.Common.Controls.ToolStripGallery.TeleoptiToolStripGallery teleoptiToolStripGalleryPreferences;
        private System.Windows.Forms.ToolStripDropDownButton toolStripButton1;
        private System.Windows.Forms.ToolStripMenuItem addNewTemplateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editTemplateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteTemplateToolStripMenuItem;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExPreferences;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExNavigate;
        private System.Windows.Forms.ToolStripButton toolStripButtonPreviousPeriod;
        private System.Windows.Forms.ToolStripButton toolStripButtonNextPeriod;
        private System.Windows.Forms.ToolStripButton toolStripButtonValidate;
        private System.Windows.Forms.ToolStripButton toolStripButtonMustHave;
        private System.Windows.Forms.ToolStripButton toolStripButtonAccounts;
        private Syncfusion.Windows.Forms.Tools.ToolStripTabItem toolStripTabItemRequests;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExRequests;
        private System.Windows.Forms.ToolStripButton toolStripButtonModify;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExTimeBank;
        private System.Windows.Forms.ToolStripButton toolStripButtonPlanningTimeBank;
        private System.Windows.Forms.ToolStripButton toolStripButtonViewStudentAvailability;
        private Syncfusion.Windows.Forms.Tools.ToolStripTabItem toolStripTabItemStudentAvailability;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExSAClipboard;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripEx2;
        private System.Windows.Forms.ToolStripButton toolStripButtonSAPreviousPeriod;
        private System.Windows.Forms.ToolStripButton toolStripButtonSANextPeriod;
        private System.Windows.Forms.ToolStripButton toolStripButtonSAValidate;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExClipboard;
    }
}
