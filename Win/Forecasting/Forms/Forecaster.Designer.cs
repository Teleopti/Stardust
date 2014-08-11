using Teleopti.Ccc.Win.Main;
namespace Teleopti.Ccc.Win.Forecasting.Forms
{
    partial class Forecaster
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnhookEvents();
                ReleaseManagedResources();
                if(components != null)
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Forecaster));
			Syncfusion.Windows.Forms.Tools.ToolStripTabGroup toolStripTabGroup2 = new Syncfusion.Windows.Forms.Tools.ToolStripTabGroup();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.toolStripBtnCreateWorkloadTemplate = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonExit = new System.Windows.Forms.ToolStripButton();
			this.toolStripExCurrentChart = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.splitContainerWorkloadSkill = new Syncfusion.Windows.Forms.Tools.SplitContainerAdv();
			this.splitContainer2 = new Syncfusion.Windows.Forms.Tools.SplitContainerAdv();
			this.tabControlWorkloads = new Syncfusion.Windows.Forms.Tools.TabControlAdv();
			this.tabPage1 = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.tabControlAdvMultisiteSkill = new Syncfusion.Windows.Forms.Tools.TabControlAdv();
			this.tabPageAdvMultisiteSkill = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.ribbonControlAdv1 = new Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed();
			this.backStageView = new Syncfusion.Windows.Forms.BackStageView(this.components);
			this.backStage1 = new Syncfusion.Windows.Forms.BackStage();
			this.backStageButtonSave = new Syncfusion.Windows.Forms.BackStageButton();
			this.backStageTabExportTo = new Syncfusion.Windows.Forms.BackStageTab();
			this.backStageButtonClose = new Syncfusion.Windows.Forms.BackStageButton();
			this.backStageButtonOptions = new Syncfusion.Windows.Forms.BackStageButton();
			this.backStageButton4 = new Syncfusion.Windows.Forms.BackStageButton();
			this.toolStripTabItemHome = new Syncfusion.Windows.Forms.Tools.ToolStripTabItem();
			this.toolStripEx1 = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripExDatePicker = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripExZoomBtns = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripExWorkflow = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripButtonForecastWorkflow = new System.Windows.Forms.ToolStripButton();
			this.toolStripExNumber = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripButtonIncreaseDecimals = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonDecreaseDecimals = new System.Windows.Forms.ToolStripButton();
			this.toolStripExShow = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripButtonShowGraph = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonShowSkillView = new System.Windows.Forms.ToolStripButton();
			this.toolStripTabItemChart = new Syncfusion.Windows.Forms.Tools.ToolStripTabItem();
			this.toolStripExChartViews = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripExGridRowInChartButtons = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripExOutput = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripButtonPrint = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonPrintPreview = new System.Windows.Forms.ToolStripButton();
			this.toolStripTabItemSkill = new Syncfusion.Windows.Forms.Tools.ToolStripTabItem();
			this.teleoptiToolStripSkill = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.teleoptiToolStripGallerySkill = new Teleopti.Ccc.Win.Common.Controls.ToolStripGallery.TeleoptiToolStripGallery();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripBtnCreateSkillTemplate = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonResetSkillTemplates = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonLongtermSkillTemplates = new System.Windows.Forms.ToolStripButton();
			this.toolStripTabItemWorkload = new Syncfusion.Windows.Forms.Tools.ToolStripTabItem();
			this.teleoptiToolStripWorkload = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.teleoptiToolStripGalleryWorkload = new Teleopti.Ccc.Win.Common.Controls.ToolStripGallery.TeleoptiToolStripGallery();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripButtonCreateNewTemplate = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonResetWorkloadTemplates = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonLongtermWorkloadTemplates = new System.Windows.Forms.ToolStripButton();
			this.toolStripTabItemMultisite = new Syncfusion.Windows.Forms.Tools.ToolStripTabItem();
			this.teleoptiToolStripMultisiteSkill = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.teleoptiToolStripGalleryMultisiteSkill = new Teleopti.Ccc.Win.Common.Controls.ToolStripGallery.TeleoptiToolStripGallery();
			this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripBtnCreateMultisiteTemplate = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonResetMultisiteSkillTemplates = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonMultisiteSkillLongtermTemplates = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonSave2 = new System.Windows.Forms.ToolStripButton();
			this.officeDropDownButtonSaveToScenario = new Syncfusion.Windows.Forms.Tools.OfficeDropDownButton();
			this.toolStripTextBoxNewScenario = new System.Windows.Forms.ToolStripTextBox();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripButtonHelp = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripButtonClose = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonSystemOptions = new System.Windows.Forms.ToolStripButton();
			this.statusStripEx1 = new Syncfusion.Windows.Forms.Tools.StatusStripEx();
			this.toolStripProgressBarMain = new System.Windows.Forms.ToolStripProgressBar();
			this.toolStripStatusLabelInfo = new System.Windows.Forms.ToolStripStatusLabel();
			this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
			this.backgroundWorkerSave = new System.ComponentModel.BackgroundWorker();
			this.backgroundWorkerApplyStandardTemplates = new System.ComponentModel.BackgroundWorker();
			this.flowLayoutExportToScenario = new Syncfusion.Windows.Forms.Tools.FlowLayout(this.components);
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerWorkloadSkill)).BeginInit();
			this.splitContainerWorkloadSkill.Panel1.SuspendLayout();
			this.splitContainerWorkloadSkill.Panel2.SuspendLayout();
			this.splitContainerWorkloadSkill.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.tabControlWorkloads)).BeginInit();
			this.tabControlWorkloads.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.tabControlAdvMultisiteSkill)).BeginInit();
			this.tabControlAdvMultisiteSkill.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).BeginInit();
			this.ribbonControlAdv1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.backStage1)).BeginInit();
			this.backStage1.SuspendLayout();
			this.toolStripTabItemHome.Panel.SuspendLayout();
			this.toolStripExWorkflow.SuspendLayout();
			this.toolStripExNumber.SuspendLayout();
			this.toolStripExShow.SuspendLayout();
			this.toolStripTabItemChart.Panel.SuspendLayout();
			this.toolStripExOutput.SuspendLayout();
			this.toolStripTabItemSkill.Panel.SuspendLayout();
			this.teleoptiToolStripSkill.SuspendLayout();
			this.toolStripTabItemWorkload.Panel.SuspendLayout();
			this.teleoptiToolStripWorkload.SuspendLayout();
			this.toolStripTabItemMultisite.Panel.SuspendLayout();
			this.teleoptiToolStripMultisiteSkill.SuspendLayout();
			this.statusStripEx1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.flowLayoutExportToScenario)).BeginInit();
			this.SuspendLayout();
			// 
			// imageList1
			// 
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			this.imageList1.Images.SetKeyName(0, "ccc_Template_1_32x32.png");
			this.imageList1.Images.SetKeyName(1, "ccc_Template_2_32x32.png");
			this.imageList1.Images.SetKeyName(2, "ccc_Template_3_32x32.png");
			this.imageList1.Images.SetKeyName(3, "ccc_Template_4_32x32.png");
			this.imageList1.Images.SetKeyName(4, "ccc_Template_5_32x32.png");
			this.imageList1.Images.SetKeyName(5, "ccc_Template_6_32x32.png");
			this.imageList1.Images.SetKeyName(6, "ccc_Template_7_32x32.png");
			this.imageList1.Images.SetKeyName(7, "ccc_Template_SpecialDays_32x32.png");
			// 
			// toolStripBtnCreateWorkloadTemplate
			// 
			this.toolStripBtnCreateWorkloadTemplate.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripBtnCreateWorkloadTemplate.Name = "toolStripBtnCreateWorkloadTemplate";
			this.SetShortcut(this.toolStripBtnCreateWorkloadTemplate, System.Windows.Forms.Keys.None);
			this.toolStripBtnCreateWorkloadTemplate.Size = new System.Drawing.Size(79, 0);
			this.toolStripBtnCreateWorkloadTemplate.Text = "xxNew";
			this.toolStripBtnCreateWorkloadTemplate.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			// 
			// toolStripButtonExit
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonExit, "");
			this.toolStripButtonExit.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Exit;
			this.toolStripButtonExit.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonExit.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonExit.Name = "toolStripButtonExit";
			this.SetShortcut(this.toolStripButtonExit, System.Windows.Forms.Keys.None);
			this.toolStripButtonExit.Size = new System.Drawing.Size(130, 20);
			this.toolStripButtonExit.Text = "xxExitTELEOPTICCC";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonExit, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonExit, false);
			this.toolStripButtonExit.Click += new System.EventHandler(this.toolStripButtonExit_Click);
			// 
			// toolStripExCurrentChart
			// 
			this.toolStripExCurrentChart.ForeColor = System.Drawing.Color.MidnightBlue;
			this.toolStripExCurrentChart.Image = null;
			this.toolStripExCurrentChart.Location = new System.Drawing.Point(0, 0);
			this.toolStripExCurrentChart.Name = "toolStripExCurrentChart";
			this.toolStripExCurrentChart.Size = new System.Drawing.Size(100, 25);
			this.toolStripExCurrentChart.TabIndex = 0;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.BackColor = System.Drawing.Color.White;
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.splitContainerWorkloadSkill, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(1, 160);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(1272, 586);
			this.tableLayoutPanel1.TabIndex = 3;
			// 
			// splitContainerWorkloadSkill
			// 
			this.splitContainerWorkloadSkill.BackColor = System.Drawing.Color.White;
			this.splitContainerWorkloadSkill.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.DarkGray);
			this.splitContainerWorkloadSkill.BeforeTouchSize = 3;
			this.splitContainerWorkloadSkill.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerWorkloadSkill.Location = new System.Drawing.Point(3, 3);
			this.splitContainerWorkloadSkill.Name = "splitContainerWorkloadSkill";
			this.splitContainerWorkloadSkill.Orientation = System.Windows.Forms.Orientation.Vertical;
			// 
			// splitContainerWorkloadSkill.Panel1
			// 
			this.splitContainerWorkloadSkill.Panel1.BackColor = System.Drawing.Color.White;
			this.splitContainerWorkloadSkill.Panel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))));
			this.splitContainerWorkloadSkill.Panel1.Controls.Add(this.splitContainer2);
			// 
			// splitContainerWorkloadSkill.Panel2
			// 
			this.splitContainerWorkloadSkill.Panel2.BackColor = System.Drawing.Color.White;
			this.splitContainerWorkloadSkill.Panel2.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))));
			this.splitContainerWorkloadSkill.Panel2.Controls.Add(this.tabControlAdvMultisiteSkill);
			this.splitContainerWorkloadSkill.PanelToBeCollapsed = Syncfusion.Windows.Forms.Tools.Enums.CollapsedPanel.Panel1;
			this.splitContainerWorkloadSkill.Size = new System.Drawing.Size(1266, 580);
			this.splitContainerWorkloadSkill.SplitterDistance = 416;
			this.splitContainerWorkloadSkill.SplitterWidth = 3;
			this.splitContainerWorkloadSkill.Style = Syncfusion.Windows.Forms.Tools.Enums.Style.Default;
			this.splitContainerWorkloadSkill.TabIndex = 6;
			// 
			// splitContainer2
			// 
			this.splitContainer2.BackColor = System.Drawing.Color.White;
			this.splitContainer2.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.DarkGray);
			this.splitContainer2.BeforeTouchSize = 3;
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.Location = new System.Drawing.Point(0, 0);
			this.splitContainer2.Name = "splitContainer2";
			this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Vertical;
			// 
			// splitContainer2.Panel1
			// 
			this.splitContainer2.Panel1.BackColor = System.Drawing.Color.White;
			this.splitContainer2.Panel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))));
			this.splitContainer2.Panel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			// 
			// splitContainer2.Panel2
			// 
			this.splitContainer2.Panel2.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128))))));
			this.splitContainer2.Panel2.Controls.Add(this.tabControlWorkloads);
			this.splitContainer2.PanelToBeCollapsed = Syncfusion.Windows.Forms.Tools.Enums.CollapsedPanel.Panel2;
			this.splitContainer2.Size = new System.Drawing.Size(1266, 416);
			this.splitContainer2.SplitterDistance = 242;
			this.splitContainer2.SplitterWidth = 3;
			this.splitContainer2.Style = Syncfusion.Windows.Forms.Tools.Enums.Style.Default;
			this.splitContainer2.TabIndex = 0;
			// 
			// tabControlWorkloads
			// 
			this.tabControlWorkloads.ActiveTabColor = System.Drawing.Color.DarkGray;
			this.tabControlWorkloads.BackColor = System.Drawing.Color.White;
			this.tabControlWorkloads.BeforeTouchSize = new System.Drawing.Size(1266, 171);
			this.tabControlWorkloads.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.tabControlWorkloads.Controls.Add(this.tabPage1);
			this.tabControlWorkloads.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControlWorkloads.FixedSingleBorderColor = System.Drawing.SystemColors.Control;
			this.tabControlWorkloads.InactiveTabColor = System.Drawing.Color.White;
			this.tabControlWorkloads.Location = new System.Drawing.Point(0, 0);
			this.tabControlWorkloads.Name = "tabControlWorkloads";
			this.tabControlWorkloads.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Silver;
			this.tabControlWorkloads.Size = new System.Drawing.Size(1266, 171);
			this.tabControlWorkloads.TabIndex = 6;
			this.tabControlWorkloads.TabPanelBackColor = System.Drawing.Color.White;
			this.tabControlWorkloads.TabStyle = typeof(Syncfusion.Windows.Forms.Tools.TabRendererMetro);
			this.tabControlWorkloads.ThemesEnabled = true;
			this.tabControlWorkloads.SelectedIndexChanged += new System.EventHandler(this.tabControlWorkloads_SelectedIndexChanged);
			// 
			// tabPage1
			// 
			this.tabPage1.BackColor = System.Drawing.Color.White;
			this.tabPage1.Image = null;
			this.tabPage1.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPage1.Location = new System.Drawing.Point(2, 23);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.ShowCloseButton = true;
			this.tabPage1.Size = new System.Drawing.Size(1262, 146);
			this.tabPage1.TabFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.tabPage1.TabIndex = 1;
			this.tabPage1.ThemesEnabled = false;
			// 
			// tabControlAdvMultisiteSkill
			// 
			this.tabControlAdvMultisiteSkill.ActiveTabColor = System.Drawing.Color.DarkGray;
			this.tabControlAdvMultisiteSkill.BackColor = System.Drawing.Color.White;
			this.tabControlAdvMultisiteSkill.BeforeTouchSize = new System.Drawing.Size(1266, 161);
			this.tabControlAdvMultisiteSkill.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.tabControlAdvMultisiteSkill.Controls.Add(this.tabPageAdvMultisiteSkill);
			this.tabControlAdvMultisiteSkill.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControlAdvMultisiteSkill.FixedSingleBorderColor = System.Drawing.SystemColors.Control;
			this.tabControlAdvMultisiteSkill.InactiveTabColor = System.Drawing.Color.White;
			this.tabControlAdvMultisiteSkill.Location = new System.Drawing.Point(0, 0);
			this.tabControlAdvMultisiteSkill.Name = "tabControlAdvMultisiteSkill";
			this.tabControlAdvMultisiteSkill.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Silver;
			this.tabControlAdvMultisiteSkill.Size = new System.Drawing.Size(1266, 161);
			this.tabControlAdvMultisiteSkill.TabIndex = 7;
			this.tabControlAdvMultisiteSkill.TabPanelBackColor = System.Drawing.Color.White;
			this.tabControlAdvMultisiteSkill.TabStyle = typeof(Syncfusion.Windows.Forms.Tools.TabRendererMetro);
			this.tabControlAdvMultisiteSkill.ThemesEnabled = true;
			this.tabControlAdvMultisiteSkill.SelectedIndexChanged += new System.EventHandler(this.tabControlAdvMultisiteSkill_SelectedIndexChanged);
			// 
			// tabPageAdvMultisiteSkill
			// 
			this.tabPageAdvMultisiteSkill.Image = null;
			this.tabPageAdvMultisiteSkill.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageAdvMultisiteSkill.Location = new System.Drawing.Point(2, 23);
			this.tabPageAdvMultisiteSkill.Name = "tabPageAdvMultisiteSkill";
			this.tabPageAdvMultisiteSkill.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageAdvMultisiteSkill.ShowCloseButton = true;
			this.tabPageAdvMultisiteSkill.Size = new System.Drawing.Size(1262, 136);
			this.tabPageAdvMultisiteSkill.TabFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.tabPageAdvMultisiteSkill.TabIndex = 1;
			this.tabPageAdvMultisiteSkill.ThemesEnabled = true;
			// 
			// ribbonControlAdv1
			// 
			this.ribbonControlAdv1.BackStageView = this.backStageView;
			this.ribbonControlAdv1.CaptionFont = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ribbonControlAdv1.Header.AddMainItem(toolStripTabItemHome);
			this.ribbonControlAdv1.Header.AddMainItem(toolStripTabItemChart);
			this.ribbonControlAdv1.Header.AddMainItem(toolStripTabItemSkill);
			this.ribbonControlAdv1.Header.AddMainItem(toolStripTabItemWorkload);
			this.ribbonControlAdv1.Header.AddMainItem(toolStripTabItemMultisite);
			this.ribbonControlAdv1.Header.AddQuickItem(new Syncfusion.Windows.Forms.Tools.QuickButtonReflectable(toolStripButtonSave2));
			this.ribbonControlAdv1.HideMenuButtonToolTip = false;
			this.ribbonControlAdv1.Location = new System.Drawing.Point(1, 1);
			this.ribbonControlAdv1.MaximizeToolTip = "Maximize Ribbon";
			this.ribbonControlAdv1.MenuButtonEnabled = true;
			this.ribbonControlAdv1.MenuButtonFont = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ribbonControlAdv1.MenuButtonText = "File";
			this.ribbonControlAdv1.MenuButtonWidth = 60;
			this.ribbonControlAdv1.MenuColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.ribbonControlAdv1.MinimizeToolTip = "Minimize Ribbon";
			this.ribbonControlAdv1.Name = "ribbonControlAdv1";
			this.ribbonControlAdv1.Office2013ColorScheme = Syncfusion.Windows.Forms.Tools.Office2013ColorScheme.White;
			this.ribbonControlAdv1.OfficeColorScheme = Syncfusion.Windows.Forms.Tools.ToolStripEx.ColorScheme.Silver;
			// 
			// ribbonControlAdv1.OfficeMenu
			// 
			this.ribbonControlAdv1.OfficeMenu.AutoSize = false;
			this.ribbonControlAdv1.OfficeMenu.AuxPanel.MinimumSize = new System.Drawing.Size(150, 0);
			this.ribbonControlAdv1.OfficeMenu.AuxPanel.Text = "";
			this.ribbonControlAdv1.OfficeMenu.MainPanel.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonSave2,
            this.officeDropDownButtonSaveToScenario,
            this.toolStripSeparator3,
            this.toolStripButtonHelp,
            this.toolStripSeparator5,
            this.toolStripButtonClose});
			this.ribbonControlAdv1.OfficeMenu.MainPanel.MinimumSize = new System.Drawing.Size(150, 0);
			this.ribbonControlAdv1.OfficeMenu.MinimumSize = new System.Drawing.Size(300, 250);
			this.ribbonControlAdv1.OfficeMenu.Name = "OfficeMenu";
			this.ribbonControlAdv1.OfficeMenu.Size = new System.Drawing.Size(314, 250);
			this.ribbonControlAdv1.OfficeMenu.SystemPanel.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonExit,
            this.toolStripButtonSystemOptions});
			this.ribbonControlAdv1.OverFlowButtonToolTip = "Show DropDown";
			this.ribbonControlAdv1.QuickPanelImageLayout = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.ribbonControlAdv1.QuickPanelVisible = false;
			this.ribbonControlAdv1.RibbonHeaderImage = Syncfusion.Windows.Forms.Tools.RibbonHeaderImage.None;
			this.ribbonControlAdv1.RibbonStyle = Syncfusion.Windows.Forms.Tools.RibbonStyle.Office2013;
			this.ribbonControlAdv1.SelectedTab = this.toolStripTabItemHome;
			this.ribbonControlAdv1.Show2010CustomizeQuickItemDialog = false;
			this.ribbonControlAdv1.ShowRibbonDisplayOptionButton = false;
			this.ribbonControlAdv1.Size = new System.Drawing.Size(1276, 160);
			this.ribbonControlAdv1.SystemText.QuickAccessDialogDropDownName = "xxStartMenu";
			toolStripTabGroup2.Color = System.Drawing.Color.LightGray;
			toolStripTabGroup2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			toolStripTabGroup2.Name = "xxTemplates";
			toolStripTabGroup2.Visible = true;
			this.ribbonControlAdv1.TabGroups.Add(toolStripTabGroup2);
			this.ribbonControlAdv1.TabGroups.SetTabGroup(toolStripTabItemSkill, toolStripTabGroup2);
			this.ribbonControlAdv1.TabGroups.SetTabGroup(toolStripTabItemWorkload, toolStripTabGroup2);
			this.ribbonControlAdv1.TabGroups.SetTabGroup(toolStripTabItemMultisite, toolStripTabGroup2);
			this.ribbonControlAdv1.TabIndex = 4;
			this.ribbonControlAdv1.Text = "yyRibbonControlAdv1";
			this.ribbonControlAdv1.TitleAlignment = Syncfusion.Windows.Forms.Tools.TextAlignment.Center;
			this.ribbonControlAdv1.TitleColor = System.Drawing.Color.Black;
			// 
			// backStageView
			// 
			this.backStageView.BackStage = this.backStage1;
			this.backStageView.HostControl = null;
			this.backStageView.HostForm = this;
			// 
			// backStage1
			// 
			this.backStage1.ActiveTabFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
			this.backStage1.AllowDrop = true;
			this.backStage1.BeforeTouchSize = new System.Drawing.Size(1271, 716);
			this.backStage1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.backStage1.Controls.Add(this.backStageButtonSave);
			this.backStage1.Controls.Add(this.backStageTabExportTo);
			this.backStage1.Controls.Add(this.backStageButtonClose);
			this.backStage1.Controls.Add(this.backStageButtonOptions);
			this.backStage1.Controls.Add(this.backStageButton4);
			this.backStage1.ItemSize = new System.Drawing.Size(138, 40);
			this.backStage1.Location = new System.Drawing.Point(0, 0);
			this.backStage1.Name = "backStage1";
			this.backStage1.OfficeColorScheme = Syncfusion.Windows.Forms.Tools.ToolStripEx.ColorScheme.Silver;
			this.backStage1.Size = new System.Drawing.Size(1271, 716);
			this.backStage1.TabIndex = 124;
			this.backStage1.Visible = false;
			// 
			// backStageButtonSave
			// 
			this.backStageButtonSave.Accelerator = "";
			this.backStageButtonSave.BackColor = System.Drawing.Color.Transparent;
			this.backStageButtonSave.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.backStageButtonSave.IsBackStageButton = false;
			this.backStageButtonSave.Location = new System.Drawing.Point(0, 16);
			this.backStageButtonSave.Name = "backStageButtonSave";
			this.backStageButtonSave.Size = new System.Drawing.Size(110, 25);
			this.backStageButtonSave.TabIndex = 3;
			this.backStageButtonSave.Text = "xxSave";
			this.backStageButtonSave.Click += new System.EventHandler(this.backStageButtonSave_Click);
			// 
			// backStageTabExportTo
			// 
			this.backStageTabExportTo.Accelerator = "";
			this.backStageTabExportTo.BackColor = System.Drawing.Color.White;
			this.backStageTabExportTo.Image = null;
			this.backStageTabExportTo.ImageSize = new System.Drawing.Size(16, 16);
			this.backStageTabExportTo.Location = new System.Drawing.Point(137, 0);
			this.backStageTabExportTo.Name = "backStageTabExportTo";
			this.backStageTabExportTo.Position = new System.Drawing.Point(0, 0);
			this.backStageTabExportTo.ShowCloseButton = true;
			this.backStageTabExportTo.Size = new System.Drawing.Size(1134, 716);
			this.backStageTabExportTo.TabIndex = 4;
			this.backStageTabExportTo.Text = "xxExport";
			this.backStageTabExportTo.ThemesEnabled = false;
			// 
			// backStageButtonClose
			// 
			this.backStageButtonClose.Accelerator = "";
			this.backStageButtonClose.BackColor = System.Drawing.Color.Transparent;
			this.backStageButtonClose.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.backStageButtonClose.IsBackStageButton = false;
			this.backStageButtonClose.Location = new System.Drawing.Point(0, 78);
			this.backStageButtonClose.Name = "backStageButtonClose";
			this.backStageButtonClose.Size = new System.Drawing.Size(110, 25);
			this.backStageButtonClose.TabIndex = 5;
			this.backStageButtonClose.Text = "xxClose";
			this.backStageButtonClose.Click += new System.EventHandler(this.backStageButtonClose_Click);
			// 
			// backStageButtonOptions
			// 
			this.backStageButtonOptions.Accelerator = "";
			this.backStageButtonOptions.BackColor = System.Drawing.Color.Transparent;
			this.backStageButtonOptions.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.backStageButtonOptions.IsBackStageButton = false;
			this.backStageButtonOptions.Location = new System.Drawing.Point(0, 103);
			this.backStageButtonOptions.Name = "backStageButtonOptions";
			this.backStageButtonOptions.Size = new System.Drawing.Size(110, 25);
			this.backStageButtonOptions.TabIndex = 6;
			this.backStageButtonOptions.Text = "xxOptions";
			this.backStageButtonOptions.Click += new System.EventHandler(this.backStageButtonOptions_Click);
			// 
			// backStageButton4
			// 
			this.backStageButton4.Accelerator = "";
			this.backStageButton4.BackColor = System.Drawing.Color.Transparent;
			this.backStageButton4.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.backStageButton4.IsBackStageButton = false;
			this.backStageButton4.Location = new System.Drawing.Point(0, 128);
			this.backStageButton4.Name = "backStageButton4";
			this.backStageButton4.Size = new System.Drawing.Size(110, 25);
			this.backStageButton4.TabIndex = 7;
			this.backStageButton4.Text = "xxExitTELEOPTICCC";
			this.backStageButton4.Click += new System.EventHandler(this.backStageButton4_Click);
			// 
			// toolStripTabItemHome
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripTabItemHome, "");
			this.toolStripTabItemHome.Name = "toolStripTabItemHome";
			// 
			// ribbonControlAdv1.ribbonPanel1
			// 
			this.toolStripTabItemHome.Panel.Controls.Add(this.toolStripEx1);
			this.toolStripTabItemHome.Panel.Controls.Add(this.toolStripExDatePicker);
			this.toolStripTabItemHome.Panel.Controls.Add(this.toolStripExZoomBtns);
			this.toolStripTabItemHome.Panel.Controls.Add(this.toolStripExWorkflow);
			this.toolStripTabItemHome.Panel.Controls.Add(this.toolStripExNumber);
			this.toolStripTabItemHome.Panel.Controls.Add(this.toolStripExShow);
			this.toolStripTabItemHome.Panel.Name = "ribbonPanel1";
			this.toolStripTabItemHome.Panel.ScrollPosition = 0;
			this.toolStripTabItemHome.Panel.TabIndex = 2;
			this.toolStripTabItemHome.Panel.Text = "XXHome";
			this.toolStripTabItemHome.Position = 0;
			this.SetShortcut(this.toolStripTabItemHome, System.Windows.Forms.Keys.None);
			this.toolStripTabItemHome.Size = new System.Drawing.Size(69, 25);
			this.toolStripTabItemHome.Text = "XXHome";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripTabItemHome, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripTabItemHome, false);
			// 
			// toolStripEx1
			// 
			this.toolStripEx1.AutoSize = false;
			this.ribbonControlAdv1.SetDescription(this.toolStripEx1, "");
			this.toolStripEx1.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripEx1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(59)))), ((int)(((byte)(59)))));
			this.toolStripEx1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripEx1.Image = null;
			this.toolStripEx1.ImageScalingSize = new System.Drawing.Size(25, 25);
			this.toolStripEx1.Location = new System.Drawing.Point(0, 1);
			this.toolStripEx1.Name = "toolStripEx1";
			this.toolStripEx1.Office12Mode = false;
			this.toolStripEx1.ShowLauncher = false;
			this.toolStripEx1.Size = new System.Drawing.Size(125, 101);
			this.toolStripEx1.TabIndex = 1;
			this.toolStripEx1.Text = "xxClipboard";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripEx1, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripEx1, false);
			// 
			// toolStripExDatePicker
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripExDatePicker, "");
			this.toolStripExDatePicker.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripExDatePicker.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(59)))), ((int)(((byte)(59)))));
			this.toolStripExDatePicker.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripExDatePicker.Image = null;
			this.toolStripExDatePicker.Location = new System.Drawing.Point(127, 1);
			this.toolStripExDatePicker.Name = "toolStripExDatePicker";
			this.toolStripExDatePicker.Office12Mode = false;
			this.toolStripExDatePicker.ShowCaption = true;
			this.toolStripExDatePicker.ShowLauncher = false;
			this.toolStripExDatePicker.Size = new System.Drawing.Size(106, 101);
			this.toolStripExDatePicker.TabIndex = 3;
			this.toolStripExDatePicker.Text = "xxDateNavigation";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripExDatePicker, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripExDatePicker, false);
			// 
			// toolStripExZoomBtns
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripExZoomBtns, "");
			this.toolStripExZoomBtns.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripExZoomBtns.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(59)))), ((int)(((byte)(59)))));
			this.toolStripExZoomBtns.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripExZoomBtns.Image = null;
			this.toolStripExZoomBtns.Location = new System.Drawing.Point(235, 1);
			this.toolStripExZoomBtns.Name = "toolStripExZoomBtns";
			this.toolStripExZoomBtns.Office12Mode = false;
			this.toolStripExZoomBtns.ShowLauncher = false;
			this.toolStripExZoomBtns.Size = new System.Drawing.Size(106, 101);
			this.toolStripExZoomBtns.TabIndex = 2;
			this.toolStripExZoomBtns.Text = "xxViews";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripExZoomBtns, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripExZoomBtns, false);
			// 
			// toolStripExWorkflow
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripExWorkflow, "");
			this.toolStripExWorkflow.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripExWorkflow.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(59)))), ((int)(((byte)(59)))));
			this.toolStripExWorkflow.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripExWorkflow.Image = null;
			this.toolStripExWorkflow.ImageScalingSize = new System.Drawing.Size(25, 25);
			this.toolStripExWorkflow.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonForecastWorkflow});
			this.toolStripExWorkflow.Location = new System.Drawing.Point(343, 1);
			this.toolStripExWorkflow.Name = "toolStripExWorkflow";
			this.toolStripExWorkflow.Office12Mode = false;
			this.toolStripExWorkflow.ShowLauncher = false;
			this.toolStripExWorkflow.Size = new System.Drawing.Size(122, 101);
			this.toolStripExWorkflow.TabIndex = 8;
			this.toolStripExWorkflow.Text = "xxWorkflow";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripExWorkflow, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripExWorkflow, false);
			// 
			// toolStripButtonForecastWorkflow
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonForecastWorkflow, "");
			this.toolStripButtonForecastWorkflow.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_OpenForecastWorkflow;
			this.toolStripButtonForecastWorkflow.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonForecastWorkflow.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonForecastWorkflow.Name = "toolStripButtonForecastWorkflow";
			this.SetShortcut(this.toolStripButtonForecastWorkflow, System.Windows.Forms.Keys.None);
			this.toolStripButtonForecastWorkflow.Size = new System.Drawing.Size(115, 85);
			this.toolStripButtonForecastWorkflow.Text = "xxManageWorkload";
			this.toolStripButtonForecastWorkflow.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonForecastWorkflow, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonForecastWorkflow, false);
			this.toolStripButtonForecastWorkflow.Click += new System.EventHandler(this.toolStripButtonForecastWorkflow_Click);
			// 
			// toolStripExNumber
			// 
			this.toolStripExNumber.AutoSize = false;
			this.ribbonControlAdv1.SetDescription(this.toolStripExNumber, "");
			this.toolStripExNumber.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripExNumber.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(59)))), ((int)(((byte)(59)))));
			this.toolStripExNumber.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripExNumber.Image = null;
			this.toolStripExNumber.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonIncreaseDecimals,
            this.toolStripButtonDecreaseDecimals});
			this.toolStripExNumber.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Table;
			this.toolStripExNumber.Location = new System.Drawing.Point(467, 1);
			this.toolStripExNumber.Name = "toolStripExNumber";
			this.toolStripExNumber.Office12Mode = false;
			this.toolStripExNumber.ShowLauncher = false;
			this.toolStripExNumber.Size = new System.Drawing.Size(167, 101);
			this.toolStripExNumber.TabIndex = 9;
			this.toolStripExNumber.Text = "xxNumber";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripExNumber, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripExNumber, false);
			// 
			// toolStripButtonIncreaseDecimals
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonIncreaseDecimals, "");
			this.toolStripButtonIncreaseDecimals.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Add;
			this.toolStripButtonIncreaseDecimals.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
			this.toolStripButtonIncreaseDecimals.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonIncreaseDecimals.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonIncreaseDecimals.Name = "toolStripButtonIncreaseDecimals";
			this.SetShortcut(this.toolStripButtonIncreaseDecimals, System.Windows.Forms.Keys.None);
			this.toolStripButtonIncreaseDecimals.Size = new System.Drawing.Size(144, 36);
			this.toolStripButtonIncreaseDecimals.Text = "xxIncreaseDecimals";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonIncreaseDecimals, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonIncreaseDecimals, false);
			this.toolStripButtonIncreaseDecimals.Click += new System.EventHandler(this.toolStripButtonIncreaseDecimals_Click);
			// 
			// toolStripButtonDecreaseDecimals
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonDecreaseDecimals, "");
			this.toolStripButtonDecreaseDecimals.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Remove;
			this.toolStripButtonDecreaseDecimals.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
			this.toolStripButtonDecreaseDecimals.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonDecreaseDecimals.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonDecreaseDecimals.Name = "toolStripButtonDecreaseDecimals";
			this.SetShortcut(this.toolStripButtonDecreaseDecimals, System.Windows.Forms.Keys.None);
			this.toolStripButtonDecreaseDecimals.Size = new System.Drawing.Size(148, 36);
			this.toolStripButtonDecreaseDecimals.Text = "xxDecreaseDecimals";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonDecreaseDecimals, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonDecreaseDecimals, false);
			this.toolStripButtonDecreaseDecimals.Click += new System.EventHandler(this.toolStripButtonDecreaseDecimals_Click);
			// 
			// toolStripExShow
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripExShow, "");
			this.toolStripExShow.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripExShow.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(59)))), ((int)(((byte)(59)))));
			this.toolStripExShow.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripExShow.Image = null;
			this.toolStripExShow.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonShowGraph,
            this.toolStripButtonShowSkillView});
			this.toolStripExShow.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
			this.toolStripExShow.Location = new System.Drawing.Point(636, 1);
			this.toolStripExShow.Name = "toolStripExShow";
			this.toolStripExShow.Office12Mode = false;
			this.toolStripExShow.ShowLauncher = false;
			this.toolStripExShow.Size = new System.Drawing.Size(119, 101);
			this.toolStripExShow.TabIndex = 10;
			this.toolStripExShow.Text = "xxShow";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripExShow, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripExShow, false);
			// 
			// toolStripButtonShowGraph
			// 
			this.toolStripButtonShowGraph.Checked = true;
			this.toolStripButtonShowGraph.CheckState = System.Windows.Forms.CheckState.Checked;
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonShowGraph, "");
			this.toolStripButtonShowGraph.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Realtime_adherence_32x32;
			this.toolStripButtonShowGraph.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripButtonShowGraph.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonShowGraph.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonShowGraph.Name = "toolStripButtonShowGraph";
			this.SetShortcut(this.toolStripButtonShowGraph, System.Windows.Forms.Keys.None);
			this.toolStripButtonShowGraph.Size = new System.Drawing.Size(114, 36);
			this.toolStripButtonShowGraph.Text = "xxShowGraph";
			this.toolStripButtonShowGraph.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonShowGraph, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonShowGraph, false);
			this.toolStripButtonShowGraph.Click += new System.EventHandler(this.toolStripButtonShowGraph_Click);
			// 
			// toolStripButtonShowSkillView
			// 
			this.toolStripButtonShowSkillView.Checked = true;
			this.toolStripButtonShowSkillView.CheckState = System.Windows.Forms.CheckState.Checked;
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonShowSkillView, "");
			this.toolStripButtonShowSkillView.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_SkillGeneral;
			this.toolStripButtonShowSkillView.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripButtonShowSkillView.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonShowSkillView.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonShowSkillView.Name = "toolStripButtonShowSkillView";
			this.SetShortcut(this.toolStripButtonShowSkillView, System.Windows.Forms.Keys.None);
			this.toolStripButtonShowSkillView.Size = new System.Drawing.Size(114, 36);
			this.toolStripButtonShowSkillView.Text = "xxSkill";
			this.toolStripButtonShowSkillView.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonShowSkillView, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonShowSkillView, false);
			this.toolStripButtonShowSkillView.Click += new System.EventHandler(this.toolStripButtonShowSkillView_Click);
			// 
			// toolStripTabItemChart
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripTabItemChart, "");
			this.toolStripTabItemChart.Name = "toolStripTabItemChart";
			// 
			// ribbonControlAdv1.ribbonPanel2
			// 
			this.toolStripTabItemChart.Panel.Controls.Add(this.toolStripExChartViews);
			this.toolStripTabItemChart.Panel.Controls.Add(this.toolStripExGridRowInChartButtons);
			this.toolStripTabItemChart.Panel.Controls.Add(this.toolStripExOutput);
			this.toolStripTabItemChart.Panel.Name = "ribbonPanel2";
			this.toolStripTabItemChart.Panel.ScrollPosition = 0;
			this.toolStripTabItemChart.Panel.TabIndex = 5;
			this.toolStripTabItemChart.Panel.Text = "XXChart";
			this.toolStripTabItemChart.Position = 1;
			this.SetShortcut(this.toolStripTabItemChart, System.Windows.Forms.Keys.None);
			this.toolStripTabItemChart.Size = new System.Drawing.Size(66, 25);
			this.toolStripTabItemChart.Text = "XXChart";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripTabItemChart, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripTabItemChart, false);
			// 
			// toolStripExChartViews
			// 
			this.toolStripExChartViews.AutoSize = false;
			this.ribbonControlAdv1.SetDescription(this.toolStripExChartViews, "");
			this.toolStripExChartViews.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripExChartViews.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(59)))), ((int)(((byte)(59)))));
			this.toolStripExChartViews.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripExChartViews.Image = null;
			this.toolStripExChartViews.Location = new System.Drawing.Point(0, 1);
			this.toolStripExChartViews.Name = "toolStripExChartViews";
			this.toolStripExChartViews.Office12Mode = false;
			this.toolStripExChartViews.ShowLauncher = false;
			this.toolStripExChartViews.Size = new System.Drawing.Size(223, 0);
			this.toolStripExChartViews.TabIndex = 4;
			this.toolStripExChartViews.Text = "xxChartViews";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripExChartViews, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripExChartViews, false);
			// 
			// toolStripExGridRowInChartButtons
			// 
			this.toolStripExGridRowInChartButtons.AutoSize = false;
			this.ribbonControlAdv1.SetDescription(this.toolStripExGridRowInChartButtons, "");
			this.toolStripExGridRowInChartButtons.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripExGridRowInChartButtons.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(59)))), ((int)(((byte)(59)))));
			this.toolStripExGridRowInChartButtons.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripExGridRowInChartButtons.Image = null;
			this.toolStripExGridRowInChartButtons.Location = new System.Drawing.Point(225, 1);
			this.toolStripExGridRowInChartButtons.Name = "toolStripExGridRowInChartButtons";
			this.toolStripExGridRowInChartButtons.Office12Mode = false;
			this.toolStripExGridRowInChartButtons.ShowLauncher = false;
			this.toolStripExGridRowInChartButtons.Size = new System.Drawing.Size(233, 0);
			this.toolStripExGridRowInChartButtons.TabIndex = 7;
			this.toolStripExGridRowInChartButtons.Text = "xxGridRowsInChart";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripExGridRowInChartButtons, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripExGridRowInChartButtons, false);
			// 
			// toolStripExOutput
			// 
			this.toolStripExOutput.AutoSize = false;
			this.ribbonControlAdv1.SetDescription(this.toolStripExOutput, "");
			this.toolStripExOutput.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripExOutput.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(59)))), ((int)(((byte)(59)))));
			this.toolStripExOutput.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripExOutput.Image = null;
			this.toolStripExOutput.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonPrint,
            this.toolStripButtonPrintPreview});
			this.toolStripExOutput.Location = new System.Drawing.Point(460, 1);
			this.toolStripExOutput.Name = "toolStripExOutput";
			this.toolStripExOutput.Office12Mode = false;
			this.toolStripExOutput.ShowLauncher = false;
			this.toolStripExOutput.Size = new System.Drawing.Size(147, 0);
			this.toolStripExOutput.TabIndex = 6;
			this.toolStripExOutput.Text = "xxOutput";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripExOutput, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripExOutput, false);
			// 
			// toolStripButtonPrint
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonPrint, "");
			this.toolStripButtonPrint.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Print;
			this.toolStripButtonPrint.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonPrint.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonPrint.Name = "toolStripButtonPrint";
			this.SetShortcut(this.toolStripButtonPrint, System.Windows.Forms.Keys.None);
			this.toolStripButtonPrint.Size = new System.Drawing.Size(46, 0);
			this.toolStripButtonPrint.Text = "xxPrint";
			this.toolStripButtonPrint.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonPrint, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonPrint, false);
			this.toolStripButtonPrint.Click += new System.EventHandler(this.toolStripButtonPrint_Click);
			// 
			// toolStripButtonPrintPreview
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonPrintPreview, "");
			this.toolStripButtonPrintPreview.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_PrintPreview;
			this.toolStripButtonPrintPreview.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonPrintPreview.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonPrintPreview.Name = "toolStripButtonPrintPreview";
			this.SetShortcut(this.toolStripButtonPrintPreview, System.Windows.Forms.Keys.None);
			this.toolStripButtonPrintPreview.Size = new System.Drawing.Size(87, 0);
			this.toolStripButtonPrintPreview.Text = "xxPrintPreview";
			this.toolStripButtonPrintPreview.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonPrintPreview, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonPrintPreview, false);
			this.toolStripButtonPrintPreview.Click += new System.EventHandler(this.toolStripButtonPrintPreview_Click);
			// 
			// toolStripTabItemSkill
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripTabItemSkill, "");
			this.toolStripTabItemSkill.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripTabItemSkill.Name = "toolStripTabItemSkill";
			// 
			// ribbonControlAdv1.ribbonPanel3
			// 
			this.toolStripTabItemSkill.Panel.Controls.Add(this.teleoptiToolStripSkill);
			this.toolStripTabItemSkill.Panel.Name = "ribbonPanel3";
			this.toolStripTabItemSkill.Panel.ScrollPosition = 0;
			this.toolStripTabItemSkill.Panel.TabIndex = 3;
			this.toolStripTabItemSkill.Panel.Text = "XXSkill";
			this.toolStripTabItemSkill.Position = 2;
			this.SetShortcut(this.toolStripTabItemSkill, System.Windows.Forms.Keys.None);
			this.toolStripTabItemSkill.Size = new System.Drawing.Size(46, 19);
			this.toolStripTabItemSkill.Text = "XXSkill";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripTabItemSkill, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripTabItemSkill, false);
			// 
			// teleoptiToolStripSkill
			// 
			this.teleoptiToolStripSkill.DefaultDropDownDirection = System.Windows.Forms.ToolStripDropDownDirection.BelowRight;
			this.ribbonControlAdv1.SetDescription(this.teleoptiToolStripSkill, "");
			this.teleoptiToolStripSkill.Dock = System.Windows.Forms.DockStyle.None;
			this.teleoptiToolStripSkill.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(59)))), ((int)(((byte)(59)))));
			this.teleoptiToolStripSkill.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.teleoptiToolStripSkill.Image = null;
			this.teleoptiToolStripSkill.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.teleoptiToolStripGallerySkill,
            this.toolStripSeparator2,
            this.toolStripBtnCreateSkillTemplate,
            this.toolStripButtonResetSkillTemplates,
            this.toolStripButtonLongtermSkillTemplates});
			this.teleoptiToolStripSkill.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
			this.teleoptiToolStripSkill.Location = new System.Drawing.Point(0, 1);
			this.teleoptiToolStripSkill.Name = "teleoptiToolStripSkill";
			this.teleoptiToolStripSkill.Office12Mode = false;
			this.teleoptiToolStripSkill.ShowLauncher = false;
			this.teleoptiToolStripSkill.Size = new System.Drawing.Size(785, 0);
			this.teleoptiToolStripSkill.TabIndex = 2;
			this.teleoptiToolStripSkill.Text = "xxTemplates";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.teleoptiToolStripSkill, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.teleoptiToolStripSkill, false);
			// 
			// teleoptiToolStripGallerySkill
			// 
			this.teleoptiToolStripGallerySkill.BorderStyle = Syncfusion.Windows.Forms.Tools.ToolstripGalleryBorderStyle.None;
			this.teleoptiToolStripGallerySkill.CaptionText = "";
			this.teleoptiToolStripGallerySkill.CheckOnClick = true;
			this.teleoptiToolStripGallerySkill.Dimensions = new System.Drawing.Size(7, 1);
			this.teleoptiToolStripGallerySkill.DropDownDimensions = new System.Drawing.Size(7, 2);
			this.teleoptiToolStripGallerySkill.ImageList = this.imageList1;
			this.teleoptiToolStripGallerySkill.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.teleoptiToolStripGallerySkill.ItemBackColor = System.Drawing.Color.Empty;
			this.teleoptiToolStripGallerySkill.ItemImageSize = new System.Drawing.Size(32, 32);
			this.teleoptiToolStripGallerySkill.ItemSize = new System.Drawing.Size(70, 58);
			this.teleoptiToolStripGallerySkill.Margin = new System.Windows.Forms.Padding(5, 7, 0, 0);
			this.teleoptiToolStripGallerySkill.Name = "teleoptiToolStripGallerySkill";
			this.teleoptiToolStripGallerySkill.ParentRibbonTab = this.toolStripTabItemSkill;
			this.teleoptiToolStripGallerySkill.ScrollerType = Syncfusion.Windows.Forms.Tools.ToolStripGalleryScrollerType.Compact;
			this.SetShortcut(this.teleoptiToolStripGallerySkill, System.Windows.Forms.Keys.None);
			this.teleoptiToolStripGallerySkill.ShowToolTip = true;
			this.teleoptiToolStripGallerySkill.Size = new System.Drawing.Size(519, 60);
			this.teleoptiToolStripGallerySkill.Text = "yy";
			this.teleoptiToolStripGallerySkill.ItemClicked += new System.EventHandler<Teleopti.Ccc.Win.Common.Controls.ToolStripGallery.ToolStripItemClickedEventArgs>(this.teleoptiToolStripGallerySkill_ItemClicked);
			this.teleoptiToolStripGallerySkill.GalleryItemClicked += new Syncfusion.Windows.Forms.Tools.ToolStripGalleryItemEventHandler(this.teleoptiToolStripGallerySkill_GalleryItemClicked);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.SetShortcut(this.toolStripSeparator2, System.Windows.Forms.Keys.None);
			this.toolStripSeparator2.Size = new System.Drawing.Size(6, 0);
			// 
			// toolStripBtnCreateSkillTemplate
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripBtnCreateSkillTemplate, "");
			this.toolStripBtnCreateSkillTemplate.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_New_Template;
			this.toolStripBtnCreateSkillTemplate.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripBtnCreateSkillTemplate.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripBtnCreateSkillTemplate.Name = "toolStripBtnCreateSkillTemplate";
			this.SetShortcut(this.toolStripBtnCreateSkillTemplate, System.Windows.Forms.Keys.None);
			this.toolStripBtnCreateSkillTemplate.Size = new System.Drawing.Size(45, 0);
			this.toolStripBtnCreateSkillTemplate.Text = "xxNew";
			this.toolStripBtnCreateSkillTemplate.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripBtnCreateSkillTemplate, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripBtnCreateSkillTemplate, false);
			this.toolStripBtnCreateSkillTemplate.Click += new System.EventHandler(this.toolStripBtnCreateSkillTemplate_Click);
			// 
			// toolStripButtonResetSkillTemplates
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonResetSkillTemplates, "");
			this.toolStripButtonResetSkillTemplates.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_ResetTemplate;
			this.toolStripButtonResetSkillTemplates.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonResetSkillTemplates.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonResetSkillTemplates.Name = "toolStripButtonResetSkillTemplates";
			this.SetShortcut(this.toolStripButtonResetSkillTemplates, System.Windows.Forms.Keys.None);
			this.toolStripButtonResetSkillTemplates.Size = new System.Drawing.Size(99, 0);
			this.toolStripButtonResetSkillTemplates.Text = "xxApplyStandard";
			this.toolStripButtonResetSkillTemplates.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonResetSkillTemplates, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonResetSkillTemplates, false);
			this.toolStripButtonResetSkillTemplates.Click += new System.EventHandler(this.toolStripButtonResetSkillTemplates_Click);
			// 
			// toolStripButtonLongtermSkillTemplates
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonLongtermSkillTemplates, "");
			this.toolStripButtonLongtermSkillTemplates.Enabled = false;
			this.toolStripButtonLongtermSkillTemplates.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Template2;
			this.toolStripButtonLongtermSkillTemplates.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonLongtermSkillTemplates.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonLongtermSkillTemplates.Name = "toolStripButtonLongtermSkillTemplates";
			this.SetShortcut(this.toolStripButtonLongtermSkillTemplates, System.Windows.Forms.Keys.None);
			this.toolStripButtonLongtermSkillTemplates.Size = new System.Drawing.Size(104, 0);
			this.toolStripButtonLongtermSkillTemplates.Text = "xxApplyLongterm";
			this.toolStripButtonLongtermSkillTemplates.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonLongtermSkillTemplates, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonLongtermSkillTemplates, false);
			// 
			// toolStripTabItemWorkload
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripTabItemWorkload, "");
			this.toolStripTabItemWorkload.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripTabItemWorkload.Name = "toolStripTabItemWorkload";
			// 
			// ribbonControlAdv1.ribbonPanel4
			// 
			this.toolStripTabItemWorkload.Panel.Controls.Add(this.teleoptiToolStripWorkload);
			this.toolStripTabItemWorkload.Panel.Name = "ribbonPanel4";
			this.toolStripTabItemWorkload.Panel.ScrollPosition = 0;
			this.toolStripTabItemWorkload.Panel.TabIndex = 4;
			this.toolStripTabItemWorkload.Panel.Text = "XXWorkload";
			this.toolStripTabItemWorkload.Position = 3;
			this.SetShortcut(this.toolStripTabItemWorkload, System.Windows.Forms.Keys.None);
			this.toolStripTabItemWorkload.Size = new System.Drawing.Size(73, 19);
			this.toolStripTabItemWorkload.Text = "XXWorkload";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripTabItemWorkload, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripTabItemWorkload, false);
			// 
			// teleoptiToolStripWorkload
			// 
			this.teleoptiToolStripWorkload.DefaultDropDownDirection = System.Windows.Forms.ToolStripDropDownDirection.BelowRight;
			this.ribbonControlAdv1.SetDescription(this.teleoptiToolStripWorkload, "");
			this.teleoptiToolStripWorkload.Dock = System.Windows.Forms.DockStyle.None;
			this.teleoptiToolStripWorkload.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(59)))), ((int)(((byte)(59)))));
			this.teleoptiToolStripWorkload.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.teleoptiToolStripWorkload.Image = null;
			this.teleoptiToolStripWorkload.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.teleoptiToolStripGalleryWorkload,
            this.toolStripSeparator1,
            this.toolStripButtonCreateNewTemplate,
            this.toolStripButtonResetWorkloadTemplates,
            this.toolStripButtonLongtermWorkloadTemplates});
			this.teleoptiToolStripWorkload.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
			this.teleoptiToolStripWorkload.Location = new System.Drawing.Point(0, 1);
			this.teleoptiToolStripWorkload.Name = "teleoptiToolStripWorkload";
			this.teleoptiToolStripWorkload.Office12Mode = false;
			this.teleoptiToolStripWorkload.ShowLauncher = false;
			this.teleoptiToolStripWorkload.Size = new System.Drawing.Size(785, 0);
			this.teleoptiToolStripWorkload.TabIndex = 3;
			this.teleoptiToolStripWorkload.Text = "xxTemplates";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.teleoptiToolStripWorkload, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.teleoptiToolStripWorkload, false);
			// 
			// teleoptiToolStripGalleryWorkload
			// 
			this.teleoptiToolStripGalleryWorkload.BorderStyle = Syncfusion.Windows.Forms.Tools.ToolstripGalleryBorderStyle.None;
			this.teleoptiToolStripGalleryWorkload.CaptionText = "";
			this.teleoptiToolStripGalleryWorkload.CheckOnClick = true;
			this.teleoptiToolStripGalleryWorkload.Dimensions = new System.Drawing.Size(7, 1);
			this.teleoptiToolStripGalleryWorkload.DropDownDimensions = new System.Drawing.Size(7, 2);
			this.teleoptiToolStripGalleryWorkload.ImageList = this.imageList1;
			this.teleoptiToolStripGalleryWorkload.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.teleoptiToolStripGalleryWorkload.ItemBackColor = System.Drawing.Color.Empty;
			this.teleoptiToolStripGalleryWorkload.ItemImageSize = new System.Drawing.Size(32, 32);
			this.teleoptiToolStripGalleryWorkload.ItemSize = new System.Drawing.Size(70, 58);
			this.teleoptiToolStripGalleryWorkload.Margin = new System.Windows.Forms.Padding(5, 7, 0, 0);
			this.teleoptiToolStripGalleryWorkload.Name = "teleoptiToolStripGalleryWorkload";
			this.teleoptiToolStripGalleryWorkload.ParentRibbonTab = this.toolStripTabItemWorkload;
			this.teleoptiToolStripGalleryWorkload.ScrollerType = Syncfusion.Windows.Forms.Tools.ToolStripGalleryScrollerType.Compact;
			this.SetShortcut(this.teleoptiToolStripGalleryWorkload, System.Windows.Forms.Keys.None);
			this.teleoptiToolStripGalleryWorkload.ShowToolTip = true;
			this.teleoptiToolStripGalleryWorkload.Size = new System.Drawing.Size(519, 60);
			this.teleoptiToolStripGalleryWorkload.Text = "yy";
			this.teleoptiToolStripGalleryWorkload.ItemClicked += new System.EventHandler<Teleopti.Ccc.Win.Common.Controls.ToolStripGallery.ToolStripItemClickedEventArgs>(this.teleoptiToolStripGalleryWorkload_ItemClicked);
			this.teleoptiToolStripGalleryWorkload.GalleryItemClicked += new Syncfusion.Windows.Forms.Tools.ToolStripGalleryItemEventHandler(this.teleoptiToolStripGalleryWorkload_GalleryItemClicked);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.SetShortcut(this.toolStripSeparator1, System.Windows.Forms.Keys.None);
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 0);
			// 
			// toolStripButtonCreateNewTemplate
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonCreateNewTemplate, "");
			this.toolStripButtonCreateNewTemplate.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_New_Template;
			this.toolStripButtonCreateNewTemplate.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonCreateNewTemplate.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonCreateNewTemplate.Name = "toolStripButtonCreateNewTemplate";
			this.SetShortcut(this.toolStripButtonCreateNewTemplate, System.Windows.Forms.Keys.None);
			this.toolStripButtonCreateNewTemplate.Size = new System.Drawing.Size(45, 0);
			this.toolStripButtonCreateNewTemplate.Text = "xxNew";
			this.toolStripButtonCreateNewTemplate.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonCreateNewTemplate, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonCreateNewTemplate, false);
			this.toolStripButtonCreateNewTemplate.Click += new System.EventHandler(this.toolStripButtonCreateNewTemplate_Click);
			// 
			// toolStripButtonResetWorkloadTemplates
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonResetWorkloadTemplates, "");
			this.toolStripButtonResetWorkloadTemplates.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_ResetTemplate;
			this.toolStripButtonResetWorkloadTemplates.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonResetWorkloadTemplates.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonResetWorkloadTemplates.Name = "toolStripButtonResetWorkloadTemplates";
			this.SetShortcut(this.toolStripButtonResetWorkloadTemplates, System.Windows.Forms.Keys.None);
			this.toolStripButtonResetWorkloadTemplates.Size = new System.Drawing.Size(99, 0);
			this.toolStripButtonResetWorkloadTemplates.Text = "xxApplyStandard";
			this.toolStripButtonResetWorkloadTemplates.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonResetWorkloadTemplates, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonResetWorkloadTemplates, false);
			this.toolStripButtonResetWorkloadTemplates.Click += new System.EventHandler(this.toolStripButtonResetWorkloadTemplates_Click);
			// 
			// toolStripButtonLongtermWorkloadTemplates
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonLongtermWorkloadTemplates, "");
			this.toolStripButtonLongtermWorkloadTemplates.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Template2;
			this.toolStripButtonLongtermWorkloadTemplates.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonLongtermWorkloadTemplates.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonLongtermWorkloadTemplates.Name = "toolStripButtonLongtermWorkloadTemplates";
			this.SetShortcut(this.toolStripButtonLongtermWorkloadTemplates, System.Windows.Forms.Keys.None);
			this.toolStripButtonLongtermWorkloadTemplates.Size = new System.Drawing.Size(104, 0);
			this.toolStripButtonLongtermWorkloadTemplates.Text = "xxApplyLongterm";
			this.toolStripButtonLongtermWorkloadTemplates.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonLongtermWorkloadTemplates, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonLongtermWorkloadTemplates, false);
			this.toolStripButtonLongtermWorkloadTemplates.Click += new System.EventHandler(this.toolStripButtonLongtermWorkloadTemplates_Click);
			// 
			// toolStripTabItemMultisite
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripTabItemMultisite, "");
			this.toolStripTabItemMultisite.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripTabItemMultisite.Name = "toolStripTabItemMultisite";
			// 
			// ribbonControlAdv1.ribbonPanel5
			// 
			this.toolStripTabItemMultisite.Panel.Controls.Add(this.teleoptiToolStripMultisiteSkill);
			this.toolStripTabItemMultisite.Panel.Name = "ribbonPanel5";
			this.toolStripTabItemMultisite.Panel.ScrollPosition = 0;
			this.toolStripTabItemMultisite.Panel.TabIndex = 4;
			this.toolStripTabItemMultisite.Panel.Text = "XXMultisiteSkill";
			this.toolStripTabItemMultisite.Position = 4;
			this.SetShortcut(this.toolStripTabItemMultisite, System.Windows.Forms.Keys.None);
			this.toolStripTabItemMultisite.Size = new System.Drawing.Size(84, 19);
			this.toolStripTabItemMultisite.Text = "XXMultisiteSkill";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripTabItemMultisite, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripTabItemMultisite, false);
			// 
			// teleoptiToolStripMultisiteSkill
			// 
			this.teleoptiToolStripMultisiteSkill.DefaultDropDownDirection = System.Windows.Forms.ToolStripDropDownDirection.BelowRight;
			this.ribbonControlAdv1.SetDescription(this.teleoptiToolStripMultisiteSkill, "");
			this.teleoptiToolStripMultisiteSkill.Dock = System.Windows.Forms.DockStyle.None;
			this.teleoptiToolStripMultisiteSkill.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(59)))), ((int)(((byte)(59)))));
			this.teleoptiToolStripMultisiteSkill.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.teleoptiToolStripMultisiteSkill.Image = null;
			this.teleoptiToolStripMultisiteSkill.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.teleoptiToolStripGalleryMultisiteSkill,
            this.toolStripSeparator4,
            this.toolStripBtnCreateMultisiteTemplate,
            this.toolStripButtonResetMultisiteSkillTemplates,
            this.toolStripButtonMultisiteSkillLongtermTemplates});
			this.teleoptiToolStripMultisiteSkill.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
			this.teleoptiToolStripMultisiteSkill.Location = new System.Drawing.Point(0, 1);
			this.teleoptiToolStripMultisiteSkill.Name = "teleoptiToolStripMultisiteSkill";
			this.teleoptiToolStripMultisiteSkill.Office12Mode = false;
			this.teleoptiToolStripMultisiteSkill.ShowLauncher = false;
			this.teleoptiToolStripMultisiteSkill.Size = new System.Drawing.Size(785, 0);
			this.teleoptiToolStripMultisiteSkill.TabIndex = 4;
			this.teleoptiToolStripMultisiteSkill.Text = "xxTemplates";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.teleoptiToolStripMultisiteSkill, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.teleoptiToolStripMultisiteSkill, false);
			// 
			// teleoptiToolStripGalleryMultisiteSkill
			// 
			this.teleoptiToolStripGalleryMultisiteSkill.BorderStyle = Syncfusion.Windows.Forms.Tools.ToolstripGalleryBorderStyle.None;
			this.teleoptiToolStripGalleryMultisiteSkill.CaptionText = "";
			this.teleoptiToolStripGalleryMultisiteSkill.CheckOnClick = true;
			this.teleoptiToolStripGalleryMultisiteSkill.Dimensions = new System.Drawing.Size(7, 1);
			this.teleoptiToolStripGalleryMultisiteSkill.DropDownDimensions = new System.Drawing.Size(7, 2);
			this.teleoptiToolStripGalleryMultisiteSkill.ImageList = this.imageList1;
			this.teleoptiToolStripGalleryMultisiteSkill.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.teleoptiToolStripGalleryMultisiteSkill.ItemBackColor = System.Drawing.Color.Empty;
			this.teleoptiToolStripGalleryMultisiteSkill.ItemImageSize = new System.Drawing.Size(32, 32);
			this.teleoptiToolStripGalleryMultisiteSkill.ItemSize = new System.Drawing.Size(70, 58);
			this.teleoptiToolStripGalleryMultisiteSkill.Margin = new System.Windows.Forms.Padding(5, 7, 0, 0);
			this.teleoptiToolStripGalleryMultisiteSkill.Name = "teleoptiToolStripGalleryMultisiteSkill";
			this.teleoptiToolStripGalleryMultisiteSkill.ParentRibbonTab = this.toolStripTabItemMultisite;
			this.teleoptiToolStripGalleryMultisiteSkill.ScrollerType = Syncfusion.Windows.Forms.Tools.ToolStripGalleryScrollerType.Compact;
			this.SetShortcut(this.teleoptiToolStripGalleryMultisiteSkill, System.Windows.Forms.Keys.None);
			this.teleoptiToolStripGalleryMultisiteSkill.ShowToolTip = true;
			this.teleoptiToolStripGalleryMultisiteSkill.Size = new System.Drawing.Size(519, 60);
			this.teleoptiToolStripGalleryMultisiteSkill.Text = "yy";
			this.teleoptiToolStripGalleryMultisiteSkill.ItemClicked += new System.EventHandler<Teleopti.Ccc.Win.Common.Controls.ToolStripGallery.ToolStripItemClickedEventArgs>(this.teleoptiToolStripGalleryMultisiteSkill_ItemClicked);
			this.teleoptiToolStripGalleryMultisiteSkill.GalleryItemClicked += new Syncfusion.Windows.Forms.Tools.ToolStripGalleryItemEventHandler(this.teleoptiToolStripGalleryMultisiteSkill_GalleryItemClicked);
			// 
			// toolStripSeparator4
			// 
			this.toolStripSeparator4.Name = "toolStripSeparator4";
			this.SetShortcut(this.toolStripSeparator4, System.Windows.Forms.Keys.None);
			this.toolStripSeparator4.Size = new System.Drawing.Size(6, 0);
			// 
			// toolStripBtnCreateMultisiteTemplate
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripBtnCreateMultisiteTemplate, "");
			this.toolStripBtnCreateMultisiteTemplate.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_New_Template;
			this.toolStripBtnCreateMultisiteTemplate.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripBtnCreateMultisiteTemplate.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripBtnCreateMultisiteTemplate.Name = "toolStripBtnCreateMultisiteTemplate";
			this.SetShortcut(this.toolStripBtnCreateMultisiteTemplate, System.Windows.Forms.Keys.None);
			this.toolStripBtnCreateMultisiteTemplate.Size = new System.Drawing.Size(45, 0);
			this.toolStripBtnCreateMultisiteTemplate.Text = "xxNew";
			this.toolStripBtnCreateMultisiteTemplate.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripBtnCreateMultisiteTemplate, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripBtnCreateMultisiteTemplate, false);
			this.toolStripBtnCreateMultisiteTemplate.Click += new System.EventHandler(this.toolStripButtonCreateNewMultisiteTemplate_Click);
			// 
			// toolStripButtonResetMultisiteSkillTemplates
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonResetMultisiteSkillTemplates, "");
			this.toolStripButtonResetMultisiteSkillTemplates.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_ResetTemplate;
			this.toolStripButtonResetMultisiteSkillTemplates.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonResetMultisiteSkillTemplates.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonResetMultisiteSkillTemplates.Name = "toolStripButtonResetMultisiteSkillTemplates";
			this.SetShortcut(this.toolStripButtonResetMultisiteSkillTemplates, System.Windows.Forms.Keys.None);
			this.toolStripButtonResetMultisiteSkillTemplates.Size = new System.Drawing.Size(99, 0);
			this.toolStripButtonResetMultisiteSkillTemplates.Text = "xxApplyStandard";
			this.toolStripButtonResetMultisiteSkillTemplates.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonResetMultisiteSkillTemplates, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonResetMultisiteSkillTemplates, false);
			this.toolStripButtonResetMultisiteSkillTemplates.Click += new System.EventHandler(this.toolStripButtonResetMultisiteSkillTemplates_Click);
			// 
			// toolStripButtonMultisiteSkillLongtermTemplates
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonMultisiteSkillLongtermTemplates, "");
			this.toolStripButtonMultisiteSkillLongtermTemplates.Enabled = false;
			this.toolStripButtonMultisiteSkillLongtermTemplates.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Template2;
			this.toolStripButtonMultisiteSkillLongtermTemplates.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonMultisiteSkillLongtermTemplates.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonMultisiteSkillLongtermTemplates.Name = "toolStripButtonMultisiteSkillLongtermTemplates";
			this.SetShortcut(this.toolStripButtonMultisiteSkillLongtermTemplates, System.Windows.Forms.Keys.None);
			this.toolStripButtonMultisiteSkillLongtermTemplates.Size = new System.Drawing.Size(104, 0);
			this.toolStripButtonMultisiteSkillLongtermTemplates.Text = "xxApplyLongterm";
			this.toolStripButtonMultisiteSkillLongtermTemplates.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonMultisiteSkillLongtermTemplates, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonMultisiteSkillLongtermTemplates, false);
			// 
			// toolStripButtonSave2
			// 
			this.toolStripButtonSave2.AutoSize = false;
			this.toolStripButtonSave2.AutoToolTip = false;
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonSave2, "");
			this.toolStripButtonSave2.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Save;
			this.toolStripButtonSave2.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripButtonSave2.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonSave2.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonSave2.Name = "toolStripButtonSave2";
			this.SetShortcut(this.toolStripButtonSave2, System.Windows.Forms.Keys.None);
			this.toolStripButtonSave2.Size = new System.Drawing.Size(150, 36);
			this.toolStripButtonSave2.Text = "xxSave";
			this.toolStripButtonSave2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonSave2, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonSave2, false);
			this.toolStripButtonSave2.Click += new System.EventHandler(this.btnSave_click);
			// 
			// officeDropDownButtonSaveToScenario
			// 
			this.officeDropDownButtonSaveToScenario.AutoSize = false;
			this.ribbonControlAdv1.SetDescription(this.officeDropDownButtonSaveToScenario, "");
			this.officeDropDownButtonSaveToScenario.DropDownFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
			this.officeDropDownButtonSaveToScenario.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripTextBoxNewScenario});
			this.officeDropDownButtonSaveToScenario.DropDownText = "xxSaveAsScenario";
			this.officeDropDownButtonSaveToScenario.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Save;
			this.officeDropDownButtonSaveToScenario.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.officeDropDownButtonSaveToScenario.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.officeDropDownButtonSaveToScenario.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.officeDropDownButtonSaveToScenario.Name = "officeDropDownButtonSaveToScenario";
			this.SetShortcut(this.officeDropDownButtonSaveToScenario, System.Windows.Forms.Keys.None);
			this.officeDropDownButtonSaveToScenario.Size = new System.Drawing.Size(150, 36);
			this.officeDropDownButtonSaveToScenario.Text = "xxSaveAs";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.officeDropDownButtonSaveToScenario, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.officeDropDownButtonSaveToScenario, false);
			// 
			// toolStripTextBoxNewScenario
			// 
			this.toolStripTextBoxNewScenario.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
			this.toolStripTextBoxNewScenario.Name = "toolStripTextBoxNewScenario";
			this.toolStripTextBoxNewScenario.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
			this.SetShortcut(this.toolStripTextBoxNewScenario, System.Windows.Forms.Keys.None);
			this.toolStripTextBoxNewScenario.Size = new System.Drawing.Size(150, 23);
			this.toolStripTextBoxNewScenario.Text = "(New Scenario)";
			this.toolStripTextBoxNewScenario.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.toolStripTextBoxNewScenario_KeyPress);
			this.toolStripTextBoxNewScenario.TextChanged += new System.EventHandler(this.toolStripTextBoxNewScenario_TextChanged);
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.SetShortcut(this.toolStripSeparator3, System.Windows.Forms.Keys.None);
			this.toolStripSeparator3.Size = new System.Drawing.Size(134, 2);
			// 
			// toolStripButtonHelp
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonHelp, "");
			this.toolStripButtonHelp.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonHelp.Image")));
			this.toolStripButtonHelp.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripButtonHelp.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonHelp.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonHelp.Name = "toolStripButtonHelp";
			this.SetShortcut(this.toolStripButtonHelp, System.Windows.Forms.Keys.None);
			this.toolStripButtonHelp.Size = new System.Drawing.Size(150, 36);
			this.toolStripButtonHelp.Text = "xxHelp";
			this.toolStripButtonHelp.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonHelp, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonHelp, false);
			this.toolStripButtonHelp.Click += new System.EventHandler(this.toolStripButtonHelp_Click);
			// 
			// toolStripSeparator5
			// 
			this.toolStripSeparator5.Name = "toolStripSeparator5";
			this.SetShortcut(this.toolStripSeparator5, System.Windows.Forms.Keys.None);
			this.toolStripSeparator5.Size = new System.Drawing.Size(134, 2);
			// 
			// toolStripButtonClose
			// 
			this.toolStripButtonClose.AutoSize = false;
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonClose, "");
			this.toolStripButtonClose.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Close;
			this.toolStripButtonClose.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripButtonClose.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonClose.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonClose.Name = "toolStripButtonClose";
			this.SetShortcut(this.toolStripButtonClose, System.Windows.Forms.Keys.None);
			this.toolStripButtonClose.Size = new System.Drawing.Size(150, 36);
			this.toolStripButtonClose.Text = "xxClose";
			this.toolStripButtonClose.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripButtonClose.ToolTipText = "xxClose";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonClose, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonClose, false);
			this.toolStripButtonClose.Click += new System.EventHandler(this.toolStripButtonClose_Click);
			// 
			// toolStripButtonSystemOptions
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonSystemOptions, "");
			this.toolStripButtonSystemOptions.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Settings;
			this.toolStripButtonSystemOptions.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonSystemOptions.Name = "toolStripButtonSystemOptions";
			this.SetShortcut(this.toolStripButtonSystemOptions, System.Windows.Forms.Keys.None);
			this.toolStripButtonSystemOptions.Size = new System.Drawing.Size(79, 20);
			this.toolStripButtonSystemOptions.Text = "xxOptions";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonSystemOptions, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonSystemOptions, false);
			this.toolStripButtonSystemOptions.Click += new System.EventHandler(this.toolStripButtonSystemOptions_Click);
			// 
			// statusStripEx1
			// 
			this.statusStripEx1.BeforeTouchSize = new System.Drawing.Size(1272, 22);
			this.statusStripEx1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBarMain,
            this.toolStripStatusLabelInfo});
			this.statusStripEx1.Location = new System.Drawing.Point(1, 745);
			this.statusStripEx1.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.statusStripEx1.Name = "statusStripEx1";
			this.statusStripEx1.Size = new System.Drawing.Size(1272, 22);
			this.statusStripEx1.TabIndex = 123;
			this.statusStripEx1.Text = "statusStripEx1";
			this.statusStripEx1.VisualStyle = Syncfusion.Windows.Forms.Tools.StatusStripExStyle.Metro;
			// 
			// toolStripProgressBarMain
			// 
			this.toolStripProgressBarMain.Maximum = 30;
			this.toolStripProgressBarMain.Name = "toolStripProgressBarMain";
			this.SetShortcut(this.toolStripProgressBarMain, System.Windows.Forms.Keys.None);
			this.toolStripProgressBarMain.Size = new System.Drawing.Size(100, 15);
			this.toolStripProgressBarMain.Step = 6;
			// 
			// toolStripStatusLabelInfo
			// 
			this.toolStripStatusLabelInfo.AutoSize = false;
			this.toolStripStatusLabelInfo.Name = "toolStripStatusLabelInfo";
			this.SetShortcut(this.toolStripStatusLabelInfo, System.Windows.Forms.Keys.None);
			this.toolStripStatusLabelInfo.Size = new System.Drawing.Size(300, 15);
			this.toolStripStatusLabelInfo.Text = "starting...";
			this.toolStripStatusLabelInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// backgroundWorker1
			// 
			this.backgroundWorker1.WorkerReportsProgress = true;
			this.backgroundWorker1.WorkerSupportsCancellation = true;
			// 
			// backgroundWorkerSave
			// 
			this.backgroundWorkerSave.WorkerReportsProgress = true;
			this.backgroundWorkerSave.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerSave_DoWork);
			this.backgroundWorkerSave.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorkerSave_ProgressChanged);
			this.backgroundWorkerSave.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerSave_RunWorkerCompleted);
			// 
			// backgroundWorkerApplyStandardTemplates
			// 
			this.backgroundWorkerApplyStandardTemplates.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerApplyStandardTemplates_DoWork);
			this.backgroundWorkerApplyStandardTemplates.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerApplyStandardTemplates_RunWorkerCompleted);
			// 
			// flowLayoutExportToScenario
			// 
			this.flowLayoutExportToScenario.Alignment = Syncfusion.Windows.Forms.Tools.FlowAlignment.Near;
			this.flowLayoutExportToScenario.ContainerControl = this.backStageTabExportTo;
			this.flowLayoutExportToScenario.HorzNearMargin = 5;
			this.flowLayoutExportToScenario.LayoutMode = Syncfusion.Windows.Forms.Tools.FlowLayoutMode.Vertical;
			// 
			// Forecaster
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.Borders = new System.Windows.Forms.Padding(0);
			this.ClientSize = new System.Drawing.Size(1274, 768);
			this.ColorScheme = Syncfusion.Windows.Forms.Tools.RibbonForm.ColorSchemeType.Silver;
			this.Controls.Add(this.backStage1);
			this.Controls.Add(this.statusStripEx1);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Controls.Add(this.ribbonControlAdv1);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "Forecaster";
			this.Padding = new System.Windows.Forms.Padding(1, 0, 1, 0);
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "xxTeleoptiRaptorColonForecaster";
			this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			this.tableLayoutPanel1.ResumeLayout(false);
			this.splitContainerWorkloadSkill.Panel1.ResumeLayout(false);
			this.splitContainerWorkloadSkill.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerWorkloadSkill)).EndInit();
			this.splitContainerWorkloadSkill.ResumeLayout(false);
			this.splitContainer2.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
			this.splitContainer2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.tabControlWorkloads)).EndInit();
			this.tabControlWorkloads.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.tabControlAdvMultisiteSkill)).EndInit();
			this.tabControlAdvMultisiteSkill.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).EndInit();
			this.ribbonControlAdv1.ResumeLayout(false);
			this.ribbonControlAdv1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.backStage1)).EndInit();
			this.backStage1.ResumeLayout(false);
			this.toolStripTabItemHome.Panel.ResumeLayout(false);
			this.toolStripTabItemHome.Panel.PerformLayout();
			this.toolStripExWorkflow.ResumeLayout(false);
			this.toolStripExWorkflow.PerformLayout();
			this.toolStripExNumber.ResumeLayout(false);
			this.toolStripExNumber.PerformLayout();
			this.toolStripExShow.ResumeLayout(false);
			this.toolStripExShow.PerformLayout();
			this.toolStripTabItemChart.Panel.ResumeLayout(false);
			this.toolStripExOutput.ResumeLayout(false);
			this.toolStripExOutput.PerformLayout();
			this.toolStripTabItemSkill.Panel.ResumeLayout(false);
			this.toolStripTabItemSkill.Panel.PerformLayout();
			this.teleoptiToolStripSkill.ResumeLayout(false);
			this.teleoptiToolStripSkill.PerformLayout();
			this.toolStripTabItemWorkload.Panel.ResumeLayout(false);
			this.toolStripTabItemWorkload.Panel.PerformLayout();
			this.teleoptiToolStripWorkload.ResumeLayout(false);
			this.teleoptiToolStripWorkload.PerformLayout();
			this.toolStripTabItemMultisite.Panel.ResumeLayout(false);
			this.toolStripTabItemMultisite.Panel.PerformLayout();
			this.teleoptiToolStripMultisiteSkill.ResumeLayout(false);
			this.teleoptiToolStripMultisiteSkill.PerformLayout();
			this.statusStripEx1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.flowLayoutExportToScenario)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Syncfusion.Windows.Forms.Tools.SplitContainerAdv  splitContainerWorkloadSkill;
        private Syncfusion.Windows.Forms.Tools.SplitContainerAdv splitContainer2;
        private Syncfusion.Windows.Forms.Tools.TabControlAdv tabControlWorkloads;
        private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPage1;
        private Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed ribbonControlAdv1;
        private Syncfusion.Windows.Forms.Tools.ToolStripTabItem toolStripTabItemHome;
        private Syncfusion.Windows.Forms.Tools.ToolStripTabItem toolStripTabItemSkill;
        private Syncfusion.Windows.Forms.Tools.ToolStripTabItem toolStripTabItemWorkload;
        private Syncfusion.Windows.Forms.Tools.ToolStripTabItem toolStripTabItemMultisite;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ToolStripButton toolStripBtnCreateWorkloadTemplate;
        private System.Windows.Forms.ToolStripButton toolStripButtonExit;
        private Syncfusion.Windows.Forms.Tools.TabControlAdv tabControlAdvMultisiteSkill;
        private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageAdvMultisiteSkill;     
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripEx1;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExWorkflow;
        private System.Windows.Forms.ToolStripButton toolStripButtonSave2;
        private System.Windows.Forms.ToolStripButton toolStripButtonForecastWorkflow;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExZoomBtns;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExDatePicker;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExCurrentChart;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExChartViews;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExOutput;
        private System.Windows.Forms.ToolStripButton toolStripButtonPrint;
        private Syncfusion.Windows.Forms.Tools.StatusStripEx statusStripEx1;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBarMain;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelInfo;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExGridRowInChartButtons;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExNumber;
        private System.Windows.Forms.ToolStripButton toolStripButtonIncreaseDecimals;
        private System.Windows.Forms.ToolStripButton toolStripButtonDecreaseDecimals;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx teleoptiToolStripSkill;
        private Common.Controls.ToolStripGallery.TeleoptiToolStripGallery teleoptiToolStripGallerySkill;
        private System.Windows.Forms.ToolStripButton toolStripBtnCreateSkillTemplate;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx teleoptiToolStripWorkload;
        private Common.Controls.ToolStripGallery.TeleoptiToolStripGallery teleoptiToolStripGalleryWorkload;
        private System.Windows.Forms.ToolStripButton toolStripButtonCreateNewTemplate;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx teleoptiToolStripMultisiteSkill;
        private Common.Controls.ToolStripGallery.TeleoptiToolStripGallery teleoptiToolStripGalleryMultisiteSkill;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripButton toolStripBtnCreateMultisiteTemplate;
        private Syncfusion.Windows.Forms.Tools.ToolStripTabItem toolStripTabItemChart;
        private System.Windows.Forms.ToolStripButton toolStripButtonSystemOptions;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton toolStripButtonClose;
        private System.Windows.Forms.ToolStripButton toolStripButtonResetSkillTemplates;
        private System.Windows.Forms.ToolStripButton toolStripButtonResetWorkloadTemplates;
        private System.Windows.Forms.ToolStripButton toolStripButtonResetMultisiteSkillTemplates;
        private Syncfusion.Windows.Forms.Tools.OfficeDropDownButton officeDropDownButtonSaveToScenario;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBoxNewScenario;
        private System.Windows.Forms.ToolStripButton toolStripButtonHelp;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripButton toolStripButtonPrintPreview;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExShow;
        private System.Windows.Forms.ToolStripButton toolStripButtonShowGraph;
        private System.Windows.Forms.ToolStripButton toolStripButtonShowSkillView;
        private System.Windows.Forms.ToolStripButton toolStripButtonLongtermSkillTemplates;
        private System.Windows.Forms.ToolStripButton toolStripButtonLongtermWorkloadTemplates;
        private System.Windows.Forms.ToolStripButton toolStripButtonMultisiteSkillLongtermTemplates;
        private System.ComponentModel.BackgroundWorker backgroundWorkerSave;
		  private System.ComponentModel.BackgroundWorker backgroundWorkerApplyStandardTemplates;
		  private Syncfusion.Windows.Forms.BackStageView backStageView;
		  private Syncfusion.Windows.Forms.BackStage backStage1;
		  private Syncfusion.Windows.Forms.BackStageButton backStageButtonSave;
		  private Syncfusion.Windows.Forms.BackStageTab backStageTabExportTo;
		  private Syncfusion.Windows.Forms.BackStageButton backStageButtonClose;
		  private Syncfusion.Windows.Forms.BackStageButton backStageButtonOptions;
		  private Syncfusion.Windows.Forms.BackStageButton backStageButton4;
		  private Syncfusion.Windows.Forms.Tools.FlowLayout flowLayoutExportToScenario;
    }
}
