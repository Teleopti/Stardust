using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Common.UI.SmartPartControls.SmartParts;
namespace Teleopti.Ccc.Win.Forecasting.Forms
{
    partial class ForecasterNavigator
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
                if (components != null)
                    components.Dispose();
                SmartPartInvoker.ClearAllSmartParts();
                Main.EntityEventAggregator.EntitiesNeedsRefresh -= entitiesNeedsRefresh;
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxQuickForecast")]
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ForecasterNavigator));
			this.contextMenuStripSkillTypes = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.toolStripMenuItemSkillTypesSkillNew = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemSkillTypesMultisiteSkillNew = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparatorExport = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemExport = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemJobHistory = new System.Windows.Forms.ToolStripMenuItem();
			this.imageListSkillTypes = new System.Windows.Forms.ImageList(this.components);
			this.contextMenuStripSkills = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.toolStripMenuItemSkillNew = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemMultisiteSkillNew = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemWorkloadNew = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemManageDayTemplates = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemManageMultisiteDistributions = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuCreateForecast = new System.Windows.Forms.ToolStripMenuItem();
			this.xxQuickForecastToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemSkillsDelete = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemSkillsProperties = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemSkillsImportForecast = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuStripWorkloads = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.toolStripMenuItemWorkloadSkillNew = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemNewWorkload = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemWorkloadPrepareWorkload = new System.Windows.Forms.ToolStripMenuItem();
			this.xxQuickForecastToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.xxEditForecastThreeDotsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemDeleteWorkload = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemWorkloadProperties = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemCopyTo = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuStripQueues = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.toolStripMenuItemQueueWorkloadNew = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemRemoveQueue = new System.Windows.Forms.ToolStripMenuItem();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.treeViewSkills = new Syncfusion.Windows.Forms.Tools.TreeViewAdv();
			this.toolStripSkills = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripLabelSkillActions = new System.Windows.Forms.ToolStripLabel();
			this.toolStripMenuItemActionSkillPrepareSkill = new System.Windows.Forms.ToolStripButton();
			this.toolStripMenuItemActionSkillManageTemplates = new System.Windows.Forms.ToolStripButton();
			this.toolStripMenuItemActionSkillImportForecast = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.skillMenuQuickForecast = new System.Windows.Forms.ToolStripButton();
			this.toolStripMenuItemActionSkillNewSkill = new System.Windows.Forms.ToolStripButton();
			this.toolStripMenuItemActionSkillNewMultisiteSkill = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonManageMultisiteDistributions = new System.Windows.Forms.ToolStripButton();
			this.toolStripMenuItemActionSkillAddNewWorkload = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.SkillsMenuItemJobHistory = new System.Windows.Forms.ToolStripButton();
			this.toolStripMenuItemActionSkillDelete1 = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator13 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemActionSkillProperties = new System.Windows.Forms.ToolStripButton();
			this.toolStripWorkload = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripLabelActions = new System.Windows.Forms.ToolStripLabel();
			this.toolStripMenuItemActionWorkloadNewWorkload = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemActionWorkloadNewSkill = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemActionWorkloadPrepareForecast = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripButtonEditForecast = new System.Windows.Forms.ToolStripButton();
			this.workloadMenuQuickForecast = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator14 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemActionWorkloadDelete = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemActionWorkloadProperties = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripQueues = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripLabel4 = new System.Windows.Forms.ToolStripLabel();
			this.toolStripMenuItemActionQueueSourceNewWorkload = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator15 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemActionQueueSourceDelete = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSkillTypes = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripLabel5 = new System.Windows.Forms.ToolStripLabel();
			this.toolStripMenuItemActionSkillTypeNewSkill = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemActionSkillTypeNewMultisiteSkill = new System.Windows.Forms.ToolStripMenuItem();
			this.skillTypeMenuQuickForecast = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
			this.skillTypeMenuJobHistory = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStrip1 = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
			this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem8 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem7 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItem11 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem9 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItem10 = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuStripSkillTypes.SuspendLayout();
			this.contextMenuStripSkills.SuspendLayout();
			this.contextMenuStripWorkloads.SuspendLayout();
			this.contextMenuStripQueues.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.treeViewSkills)).BeginInit();
			this.toolStripSkills.SuspendLayout();
			this.toolStripWorkload.SuspendLayout();
			this.toolStripQueues.SuspendLayout();
			this.toolStripSkillTypes.SuspendLayout();
			this.toolStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// contextMenuStripSkillTypes
			// 
			this.contextMenuStripSkillTypes.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemSkillTypesSkillNew,
            this.toolStripMenuItemSkillTypesMultisiteSkillNew,
            this.toolStripSeparatorExport,
            this.toolStripMenuItemExport,
            this.toolStripMenuItemJobHistory});
			this.contextMenuStripSkillTypes.Name = "contextMenuStripForecasts";
			this.contextMenuStripSkillTypes.Size = new System.Drawing.Size(230, 98);
			// 
			// toolStripMenuItemSkillTypesSkillNew
			// 
			this.toolStripMenuItemSkillTypesSkillNew.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_SkillGeneral;
			this.toolStripMenuItemSkillTypesSkillNew.Name = "toolStripMenuItemSkillTypesSkillNew";
			this.toolStripMenuItemSkillTypesSkillNew.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
			this.toolStripMenuItemSkillTypesSkillNew.Size = new System.Drawing.Size(229, 22);
			this.toolStripMenuItemSkillTypesSkillNew.Text = "xxNewSkillThreeDots";
			this.toolStripMenuItemSkillTypesSkillNew.Click += new System.EventHandler(this.toolStripMenuItemSkillTypesSkillNewClick);
			// 
			// toolStripMenuItemSkillTypesMultisiteSkillNew
			// 
			this.toolStripMenuItemSkillTypesMultisiteSkillNew.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_SkillMultisite;
			this.toolStripMenuItemSkillTypesMultisiteSkillNew.Name = "toolStripMenuItemSkillTypesMultisiteSkillNew";
			this.toolStripMenuItemSkillTypesMultisiteSkillNew.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
			this.toolStripMenuItemSkillTypesMultisiteSkillNew.Size = new System.Drawing.Size(229, 22);
			this.toolStripMenuItemSkillTypesMultisiteSkillNew.Text = "xxNewMultisiteSkillThreeDots";
			this.toolStripMenuItemSkillTypesMultisiteSkillNew.Click += new System.EventHandler(this.toolStripMenuItemSkillTypesMultisiteSkillNewClick);
			// 
			// toolStripSeparatorExport
			// 
			this.toolStripSeparatorExport.Name = "toolStripSeparatorExport";
			this.toolStripSeparatorExport.Size = new System.Drawing.Size(226, 6);
			// 
			// toolStripMenuItemExport
			// 
			this.toolStripMenuItemExport.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Export2;
			this.toolStripMenuItemExport.Name = "toolStripMenuItemExport";
			this.toolStripMenuItemExport.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
			this.toolStripMenuItemExport.Size = new System.Drawing.Size(229, 22);
			this.toolStripMenuItemExport.Text = "xxExport";
			this.toolStripMenuItemExport.Click += new System.EventHandler(this.toolStripMenuItemExportClick);
			// 
			// toolStripMenuItemJobHistory
			// 
			this.toolStripMenuItemJobHistory.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Reports_variant_32x32;
			this.toolStripMenuItemJobHistory.Name = "toolStripMenuItemJobHistory";
			this.toolStripMenuItemJobHistory.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
			this.toolStripMenuItemJobHistory.Size = new System.Drawing.Size(229, 22);
			this.toolStripMenuItemJobHistory.Text = "xxJobHistory";
			this.toolStripMenuItemJobHistory.Click += new System.EventHandler(this.toolStripMenuItemJobHistoryClick);
			// 
			// imageListSkillTypes
			// 
			this.imageListSkillTypes.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListSkillTypes.ImageStream")));
			this.imageListSkillTypes.TransparentColor = System.Drawing.Color.Transparent;
			this.imageListSkillTypes.Images.SetKeyName(0, "Backoffice");
			this.imageListSkillTypes.Images.SetKeyName(1, "ccc_SkillTime.png");
			this.imageListSkillTypes.Images.SetKeyName(2, "Email");
			this.imageListSkillTypes.Images.SetKeyName(3, "InboundTelephony");
			this.imageListSkillTypes.Images.SetKeyName(4, "Facsimile");
			this.imageListSkillTypes.Images.SetKeyName(5, "ccc_SkillGeneral.png");
			this.imageListSkillTypes.Images.SetKeyName(6, "ccc_SkillGeneral.png");
			this.imageListSkillTypes.Images.SetKeyName(7, "ccc_Workload.png");
			this.imageListSkillTypes.Images.SetKeyName(8, "graphhs.png");
			this.imageListSkillTypes.Images.SetKeyName(9, "ccc_MultiSite_32x32.png");
			this.imageListSkillTypes.Images.SetKeyName(10, "Retail");
			// 
			// contextMenuStripSkills
			// 
			this.contextMenuStripSkills.BackColor = System.Drawing.Color.White;
			this.contextMenuStripSkills.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuCreateForecast,
            this.toolStripMenuItemManageDayTemplates,
            this.toolStripMenuItemSkillsImportForecast,
            this.toolStripSeparator3,
            this.xxQuickForecastToolStripMenuItem,
            this.toolStripMenuItemSkillNew,
            this.toolStripMenuItemMultisiteSkillNew,
            this.toolStripMenuItemWorkloadNew,
            this.toolStripMenuItemManageMultisiteDistributions,
            this.toolStripSeparator9,
            this.toolStripMenuItemSkillsDelete,
            this.toolStripMenuItemSkillsProperties});
			this.contextMenuStripSkills.Name = "contextMenuStripForecasts";
			this.contextMenuStripSkills.Size = new System.Drawing.Size(241, 258);
			// 
			// toolStripMenuItemSkillNew
			// 
			this.toolStripMenuItemSkillNew.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_SkillGeneral;
			this.toolStripMenuItemSkillNew.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemSkillNew.Name = "toolStripMenuItemSkillNew";
			this.toolStripMenuItemSkillNew.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
			this.toolStripMenuItemSkillNew.Size = new System.Drawing.Size(240, 22);
			this.toolStripMenuItemSkillNew.Text = "xxNewSkillThreeDots";
			this.toolStripMenuItemSkillNew.Click += new System.EventHandler(this.toolStripMenuItemSkillsNewClick);
			// 
			// toolStripMenuItemMultisiteSkillNew
			// 
			this.toolStripMenuItemMultisiteSkillNew.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_SkillMultisite;
			this.toolStripMenuItemMultisiteSkillNew.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemMultisiteSkillNew.Name = "toolStripMenuItemMultisiteSkillNew";
			this.toolStripMenuItemMultisiteSkillNew.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
			this.toolStripMenuItemMultisiteSkillNew.Size = new System.Drawing.Size(240, 22);
			this.toolStripMenuItemMultisiteSkillNew.Text = "xxNewMultisiteSkillThreeDots";
			this.toolStripMenuItemMultisiteSkillNew.Click += new System.EventHandler(this.toolStripMenuItemMultisiteSkillNewClick);
			// 
			// toolStripMenuItemWorkloadNew
			// 
			this.toolStripMenuItemWorkloadNew.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Workload;
			this.toolStripMenuItemWorkloadNew.Name = "toolStripMenuItemWorkloadNew";
			this.toolStripMenuItemWorkloadNew.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
			this.toolStripMenuItemWorkloadNew.Size = new System.Drawing.Size(240, 22);
			this.toolStripMenuItemWorkloadNew.Text = "xxNewWorkloadThreeDots";
			this.toolStripMenuItemWorkloadNew.Click += new System.EventHandler(this.toolStripMenuItemWorkloadNewClick);
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(237, 6);
			// 
			// toolStripMenuItemManageDayTemplates
			// 
			this.toolStripMenuItemManageDayTemplates.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Template2;
			this.toolStripMenuItemManageDayTemplates.Name = "toolStripMenuItemManageDayTemplates";
			this.toolStripMenuItemManageDayTemplates.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
			this.toolStripMenuItemManageDayTemplates.Size = new System.Drawing.Size(240, 22);
			this.toolStripMenuItemManageDayTemplates.Text = "xxPrepareSkillThreeDots";
			this.toolStripMenuItemManageDayTemplates.Click += new System.EventHandler(this.toolStripMenuItemManageDayTemplatesClick);
			// 
			// toolStripMenuItemManageMultisiteDistributions
			// 
			this.toolStripMenuItemManageMultisiteDistributions.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_SkillMultiSite_16x16;
			this.toolStripMenuItemManageMultisiteDistributions.Name = "toolStripMenuItemManageMultisiteDistributions";
			this.toolStripMenuItemManageMultisiteDistributions.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
			this.toolStripMenuItemManageMultisiteDistributions.Size = new System.Drawing.Size(240, 22);
			this.toolStripMenuItemManageMultisiteDistributions.Text = "xxManageMultisiteDistributions";
			this.toolStripMenuItemManageMultisiteDistributions.Click += new System.EventHandler(this.toolStripMenuItemManageDistributionClick);
			// 
			// toolStripMenuCreateForecast
			// 
			this.toolStripMenuCreateForecast.Image = global::Teleopti.Ccc.Win.Properties.Resources.Forecasts2_filled_16x16;
			this.toolStripMenuCreateForecast.Name = "toolStripMenuCreateForecast";
			this.toolStripMenuCreateForecast.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
			this.toolStripMenuCreateForecast.Size = new System.Drawing.Size(240, 22);
			this.toolStripMenuCreateForecast.Text = "xxOpenForecastThreeDots";
			this.toolStripMenuCreateForecast.Click += new System.EventHandler(this.toolStripMenuCreateForecastClick);
			// 
			// xxQuickForecastToolStripMenuItem
			// 
			this.xxQuickForecastToolStripMenuItem.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Forecasts_16x16;
			this.xxQuickForecastToolStripMenuItem.Name = "xxQuickForecastToolStripMenuItem";
			this.xxQuickForecastToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
			this.xxQuickForecastToolStripMenuItem.Size = new System.Drawing.Size(240, 22);
			this.xxQuickForecastToolStripMenuItem.Text = "xxQuickForecast";
			this.xxQuickForecastToolStripMenuItem.Click += new System.EventHandler(this.toolStripMenuItemQuickForecastClick);
			// 
			// toolStripSeparator9
			// 
			this.toolStripSeparator9.Name = "toolStripSeparator9";
			this.toolStripSeparator9.Size = new System.Drawing.Size(237, 6);
			// 
			// toolStripMenuItemSkillsDelete
			// 
			this.toolStripMenuItemSkillsDelete.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Delete;
			this.toolStripMenuItemSkillsDelete.Name = "toolStripMenuItemSkillsDelete";
			this.toolStripMenuItemSkillsDelete.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
			this.toolStripMenuItemSkillsDelete.Size = new System.Drawing.Size(240, 22);
			this.toolStripMenuItemSkillsDelete.Text = "xxDelete";
			this.toolStripMenuItemSkillsDelete.Click += new System.EventHandler(this.toolStripMenuItemSkillsDeleteClick);
			// 
			// toolStripMenuItemSkillsProperties
			// 
			this.toolStripMenuItemSkillsProperties.Image = ((System.Drawing.Image)(resources.GetObject("toolStripMenuItemSkillsProperties.Image")));
			this.toolStripMenuItemSkillsProperties.Name = "toolStripMenuItemSkillsProperties";
			this.toolStripMenuItemSkillsProperties.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
			this.toolStripMenuItemSkillsProperties.Size = new System.Drawing.Size(240, 22);
			this.toolStripMenuItemSkillsProperties.Text = "xxPropertiesThreeDots";
			this.toolStripMenuItemSkillsProperties.Click += new System.EventHandler(this.toolStripMenuItemSkillsPropertiesClick);
			// 
			// toolStripMenuItemSkillsImportForecast
			// 
			this.toolStripMenuItemSkillsImportForecast.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_New2;
			this.toolStripMenuItemSkillsImportForecast.Name = "toolStripMenuItemSkillsImportForecast";
			this.toolStripMenuItemSkillsImportForecast.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
			this.toolStripMenuItemSkillsImportForecast.Size = new System.Drawing.Size(240, 22);
			this.toolStripMenuItemSkillsImportForecast.Text = "xxImportForecast";
			this.toolStripMenuItemSkillsImportForecast.Click += new System.EventHandler(this.toolStripMenuItemSkillsImportForecastClick);
			// 
			// contextMenuStripWorkloads
			// 
			this.contextMenuStripWorkloads.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemWorkloadSkillNew,
            this.toolStripMenuItemNewWorkload,
            this.toolStripSeparator5,
            this.toolStripMenuItemWorkloadPrepareWorkload,
            this.xxQuickForecastToolStripMenuItem1,
            this.xxEditForecastThreeDotsToolStripMenuItem,
            this.toolStripSeparator4,
            this.toolStripMenuItemDeleteWorkload,
            this.toolStripMenuItemWorkloadProperties,
            this.toolStripMenuItemCopyTo});
			this.contextMenuStripWorkloads.Name = "contextMenuStripForecasts";
			this.contextMenuStripWorkloads.Size = new System.Drawing.Size(223, 192);
			// 
			// toolStripMenuItemWorkloadSkillNew
			// 
			this.toolStripMenuItemWorkloadSkillNew.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_SkillGeneral;
			this.toolStripMenuItemWorkloadSkillNew.Name = "toolStripMenuItemWorkloadSkillNew";
			this.toolStripMenuItemWorkloadSkillNew.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
			this.toolStripMenuItemWorkloadSkillNew.Size = new System.Drawing.Size(222, 22);
			this.toolStripMenuItemWorkloadSkillNew.Text = "xxNewSkillThreeDots";
			this.toolStripMenuItemWorkloadSkillNew.Visible = false;
			this.toolStripMenuItemWorkloadSkillNew.Click += new System.EventHandler(this.toolStripMenuItemWorkloadSkillNewClick);
			// 
			// toolStripMenuItemNewWorkload
			// 
			this.toolStripMenuItemNewWorkload.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Workload;
			this.toolStripMenuItemNewWorkload.Name = "toolStripMenuItemNewWorkload";
			this.toolStripMenuItemNewWorkload.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
			this.toolStripMenuItemNewWorkload.Size = new System.Drawing.Size(222, 22);
			this.toolStripMenuItemNewWorkload.Text = "xxNewWorkloadThreeDots";
			this.toolStripMenuItemNewWorkload.Click += new System.EventHandler(this.toolStripMenuItemNewWorkloadClick);
			// 
			// toolStripSeparator5
			// 
			this.toolStripSeparator5.Name = "toolStripSeparator5";
			this.toolStripSeparator5.Size = new System.Drawing.Size(219, 6);
			// 
			// toolStripMenuItemWorkloadPrepareWorkload
			// 
			this.toolStripMenuItemWorkloadPrepareWorkload.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_CreateForecast;
			this.toolStripMenuItemWorkloadPrepareWorkload.Name = "toolStripMenuItemWorkloadPrepareWorkload";
			this.toolStripMenuItemWorkloadPrepareWorkload.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
			this.toolStripMenuItemWorkloadPrepareWorkload.Size = new System.Drawing.Size(222, 22);
			this.toolStripMenuItemWorkloadPrepareWorkload.Text = "xxPrepareForecastThreeDots";
			this.toolStripMenuItemWorkloadPrepareWorkload.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.toolStripMenuItemWorkloadPrepareWorkload.Click += new System.EventHandler(this.toolStripMenuItemWorkloadPrepareWorkloadClick);
			// 
			// xxQuickForecastToolStripMenuItem1
			// 
			this.xxQuickForecastToolStripMenuItem1.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Forecasts_16x16;
			this.xxQuickForecastToolStripMenuItem1.Name = "xxQuickForecastToolStripMenuItem1";
			this.xxQuickForecastToolStripMenuItem1.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
			this.xxQuickForecastToolStripMenuItem1.Size = new System.Drawing.Size(222, 22);
			this.xxQuickForecastToolStripMenuItem1.Text = "xxQuickForecast";
			this.xxQuickForecastToolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItemQuickForecastClick);
			// 
			// xxEditForecastThreeDotsToolStripMenuItem
			// 
			this.xxEditForecastThreeDotsToolStripMenuItem.Image = global::Teleopti.Ccc.Win.Properties.Resources.Forecasts2_filled_16x16;
			this.xxEditForecastThreeDotsToolStripMenuItem.Name = "xxEditForecastThreeDotsToolStripMenuItem";
			this.xxEditForecastThreeDotsToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
			this.xxEditForecastThreeDotsToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
			this.xxEditForecastThreeDotsToolStripMenuItem.Text = "xxOpenForecastThreeDots";
			this.xxEditForecastThreeDotsToolStripMenuItem.Click += new System.EventHandler(this.toolStripMenuCreateForecastClick);
			// 
			// toolStripSeparator4
			// 
			this.toolStripSeparator4.Name = "toolStripSeparator4";
			this.toolStripSeparator4.Size = new System.Drawing.Size(219, 6);
			// 
			// toolStripMenuItemDeleteWorkload
			// 
			this.toolStripMenuItemDeleteWorkload.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Delete;
			this.toolStripMenuItemDeleteWorkload.Name = "toolStripMenuItemDeleteWorkload";
			this.toolStripMenuItemDeleteWorkload.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
			this.toolStripMenuItemDeleteWorkload.Size = new System.Drawing.Size(222, 22);
			this.toolStripMenuItemDeleteWorkload.Text = "xxDelete";
			this.toolStripMenuItemDeleteWorkload.Click += new System.EventHandler(this.toolStripMenuItemDeleteWorkloadClick);
			// 
			// toolStripMenuItemWorkloadProperties
			// 
			this.toolStripMenuItemWorkloadProperties.Image = ((System.Drawing.Image)(resources.GetObject("toolStripMenuItemWorkloadProperties.Image")));
			this.toolStripMenuItemWorkloadProperties.Name = "toolStripMenuItemWorkloadProperties";
			this.toolStripMenuItemWorkloadProperties.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
			this.toolStripMenuItemWorkloadProperties.Size = new System.Drawing.Size(222, 22);
			this.toolStripMenuItemWorkloadProperties.Text = "xxPropertiesThreeDots";
			this.toolStripMenuItemWorkloadProperties.Click += new System.EventHandler(this.toolStripMenuItemWorkloadPropertiesClick);
			// 
			// toolStripMenuItemCopyTo
			// 
			this.toolStripMenuItemCopyTo.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Copy_16x16;
			this.toolStripMenuItemCopyTo.Name = "toolStripMenuItemCopyTo";
			this.toolStripMenuItemCopyTo.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
			this.toolStripMenuItemCopyTo.Size = new System.Drawing.Size(222, 22);
			this.toolStripMenuItemCopyTo.Text = "xxCopyToThreeDots";
			this.toolStripMenuItemCopyTo.Click += new System.EventHandler(this.toolStripMenuItemCopyToClick);
			// 
			// contextMenuStripQueues
			// 
			this.contextMenuStripQueues.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemQueueWorkloadNew,
            this.toolStripSeparator10,
            this.toolStripMenuItemRemoveQueue});
			this.contextMenuStripQueues.Name = "contextMenuStripForecasts";
			this.contextMenuStripQueues.Size = new System.Drawing.Size(214, 54);
			// 
			// toolStripMenuItemQueueWorkloadNew
			// 
			this.toolStripMenuItemQueueWorkloadNew.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Workload;
			this.toolStripMenuItemQueueWorkloadNew.Name = "toolStripMenuItemQueueWorkloadNew";
			this.toolStripMenuItemQueueWorkloadNew.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
			this.toolStripMenuItemQueueWorkloadNew.Size = new System.Drawing.Size(213, 22);
			this.toolStripMenuItemQueueWorkloadNew.Text = "xxNewWorkloadThreeDots";
			this.toolStripMenuItemQueueWorkloadNew.Click += new System.EventHandler(this.toolStripMenuItemQueueWorkloadNewClick);
			// 
			// toolStripSeparator10
			// 
			this.toolStripSeparator10.Name = "toolStripSeparator10";
			this.toolStripSeparator10.Size = new System.Drawing.Size(210, 6);
			// 
			// toolStripMenuItemRemoveQueue
			// 
			this.toolStripMenuItemRemoveQueue.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Delete;
			this.toolStripMenuItemRemoveQueue.Name = "toolStripMenuItemRemoveQueue";
			this.toolStripMenuItemRemoveQueue.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
			this.toolStripMenuItemRemoveQueue.Size = new System.Drawing.Size(213, 22);
			this.toolStripMenuItemRemoveQueue.Text = "xxDelete";
			this.toolStripMenuItemRemoveQueue.Click += new System.EventHandler(this.toolStripMenuItemRemoveQueueClick);
			// 
			// splitContainer1
			// 
			this.splitContainer1.BackColor = System.Drawing.Color.White;
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Margin = new System.Windows.Forms.Padding(0);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.treeViewSkills);
			this.splitContainer1.Panel1MinSize = 150;
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.BackColor = System.Drawing.Color.White;
			this.splitContainer1.Panel2.Controls.Add(this.toolStripSkills);
			this.splitContainer1.Panel2.Controls.Add(this.toolStripWorkload);
			this.splitContainer1.Panel2.Controls.Add(this.toolStripQueues);
			this.splitContainer1.Panel2.Controls.Add(this.toolStripSkillTypes);
			this.splitContainer1.Panel2.Controls.Add(this.toolStrip1);
			this.splitContainer1.Panel2.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
			this.splitContainer1.Size = new System.Drawing.Size(254, 853);
			this.splitContainer1.SplitterDistance = 550;
			this.splitContainer1.SplitterWidth = 1;
			this.splitContainer1.TabIndex = 4;
			// 
			// treeViewSkills
			// 
			this.treeViewSkills.BackColor = System.Drawing.Color.White;
			this.treeViewSkills.BeforeTouchSize = new System.Drawing.Size(254, 550);
			this.treeViewSkills.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
			this.treeViewSkills.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.treeViewSkills.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.treeViewSkills.CanSelectDisabledNode = false;
			this.treeViewSkills.ContextMenuStrip = this.contextMenuStripSkillTypes;
			this.treeViewSkills.Dock = System.Windows.Forms.DockStyle.Fill;
			// 
			// 
			// 
			this.treeViewSkills.HelpTextControl.Location = new System.Drawing.Point(0, 0);
			this.treeViewSkills.HelpTextControl.Name = "helpText";
			this.treeViewSkills.HelpTextControl.TabIndex = 0;
			this.treeViewSkills.ItemHeight = 25;
			this.treeViewSkills.LeftImageList = this.imageListSkillTypes;
			this.treeViewSkills.Location = new System.Drawing.Point(0, 0);
			this.treeViewSkills.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.treeViewSkills.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.treeViewSkills.Name = "treeViewSkills";
			this.treeViewSkills.SelectedNodeBackground = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255))))));
			this.treeViewSkills.ShowFocusRect = false;
			this.treeViewSkills.Size = new System.Drawing.Size(254, 550);
			this.treeViewSkills.SortWithChildNodes = true;
			this.treeViewSkills.Style = Syncfusion.Windows.Forms.Tools.TreeStyle.Metro;
			this.treeViewSkills.TabIndex = 1;
			// 
			// 
			// 
			this.treeViewSkills.ToolTipControl.Location = new System.Drawing.Point(0, 0);
			this.treeViewSkills.ToolTipControl.Name = "toolTip";
			this.treeViewSkills.ToolTipControl.TabIndex = 1;
			this.treeViewSkills.BeforeSelect += new Syncfusion.Windows.Forms.Tools.TreeNodeAdvBeforeSelectEventHandler(this.treeViewSkillsBeforeSelectUpdated);
			this.treeViewSkills.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treeViewSkillsMouseDown);
			// 
			// toolStripSkills
			// 
			this.toolStripSkills.BackColor = System.Drawing.Color.White;
			this.toolStripSkills.Dock = System.Windows.Forms.DockStyle.Fill;
			this.toolStripSkills.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			this.toolStripSkills.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.toolStripSkills.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripSkills.Image = null;
			this.toolStripSkills.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabelSkillActions,
            this.toolStripMenuItemActionSkillPrepareSkill,
            this.toolStripMenuItemActionSkillManageTemplates,
            this.toolStripMenuItemActionSkillImportForecast,
            this.toolStripSeparator2,
            this.skillMenuQuickForecast,
            this.toolStripMenuItemActionSkillNewSkill,
            this.toolStripMenuItemActionSkillNewMultisiteSkill,
            this.toolStripButtonManageMultisiteDistributions,
            this.toolStripMenuItemActionSkillAddNewWorkload,
            this.toolStripSeparator1,
            this.SkillsMenuItemJobHistory,
            this.toolStripMenuItemActionSkillDelete1,
            this.toolStripSeparator13,
            this.toolStripMenuItemActionSkillProperties});
			this.toolStripSkills.LauncherStyle = Syncfusion.Windows.Forms.Tools.LauncherStyle.Metro;
			this.toolStripSkills.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
			this.toolStripSkills.Location = new System.Drawing.Point(12, 0);
			this.toolStripSkills.Name = "toolStripSkills";
			this.toolStripSkills.Office12Mode = false;
			this.toolStripSkills.Padding = new System.Windows.Forms.Padding(1);
			this.toolStripSkills.ShowCaption = false;
			this.toolStripSkills.Size = new System.Drawing.Size(242, 302);
			this.toolStripSkills.TabIndex = 5;
			this.toolStripSkills.Text = "xxActions";
			this.toolStripSkills.Visible = false;
			this.toolStripSkills.VisualStyle = Syncfusion.Windows.Forms.Tools.ToolStripExStyle.Metro;
			// 
			// toolStripLabelSkillActions
			// 
			this.toolStripLabelSkillActions.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.toolStripLabelSkillActions.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.toolStripLabelSkillActions.Name = "toolStripLabelSkillActions";
			this.toolStripLabelSkillActions.Size = new System.Drawing.Size(239, 19);
			this.toolStripLabelSkillActions.Text = "xxActions";
			// 
			// toolStripMenuItemActionSkillPrepareSkill
			// 
			this.toolStripMenuItemActionSkillPrepareSkill.BackColor = System.Drawing.Color.White;
			this.toolStripMenuItemActionSkillPrepareSkill.Image = global::Teleopti.Ccc.Win.Properties.Resources.Forecasts2_filled_16x16;
			this.toolStripMenuItemActionSkillPrepareSkill.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemActionSkillPrepareSkill.Name = "toolStripMenuItemActionSkillPrepareSkill";
			this.toolStripMenuItemActionSkillPrepareSkill.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripMenuItemActionSkillPrepareSkill.Size = new System.Drawing.Size(239, 29);
			this.toolStripMenuItemActionSkillPrepareSkill.Text = "xxOpenForecastThreeDots";
			this.toolStripMenuItemActionSkillPrepareSkill.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemActionSkillPrepareSkill.Click += new System.EventHandler(this.toolStripMenuCreateForecastClick);
			// 
			// toolStripMenuItemActionSkillManageTemplates
			// 
			this.toolStripMenuItemActionSkillManageTemplates.BackColor = System.Drawing.Color.White;
			this.toolStripMenuItemActionSkillManageTemplates.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Template_SpecialDays_32x32;
			this.toolStripMenuItemActionSkillManageTemplates.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemActionSkillManageTemplates.Name = "toolStripMenuItemActionSkillManageTemplates";
			this.toolStripMenuItemActionSkillManageTemplates.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripMenuItemActionSkillManageTemplates.Size = new System.Drawing.Size(239, 29);
			this.toolStripMenuItemActionSkillManageTemplates.Text = "xxPrepareSkillThreeDots";
			this.toolStripMenuItemActionSkillManageTemplates.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemActionSkillManageTemplates.Click += new System.EventHandler(this.toolStripMenuItemManageDayTemplatesClick);
			// 
			// toolStripMenuItemActionSkillImportForecast
			// 
			this.toolStripMenuItemActionSkillImportForecast.BackColor = System.Drawing.Color.White;
			this.toolStripMenuItemActionSkillImportForecast.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_New2;
			this.toolStripMenuItemActionSkillImportForecast.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemActionSkillImportForecast.Name = "toolStripMenuItemActionSkillImportForecast";
			this.toolStripMenuItemActionSkillImportForecast.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripMenuItemActionSkillImportForecast.Size = new System.Drawing.Size(239, 29);
			this.toolStripMenuItemActionSkillImportForecast.Text = "xxImportForecast";
			this.toolStripMenuItemActionSkillImportForecast.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemActionSkillImportForecast.Click += new System.EventHandler(this.toolStripMenuItemActionSkillImportForecastClick);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(239, 6);
			// 
			// skillMenuQuickForecast
			// 
			this.skillMenuQuickForecast.BackColor = System.Drawing.Color.White;
			this.skillMenuQuickForecast.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Forecasts_16x16;
			this.skillMenuQuickForecast.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.skillMenuQuickForecast.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.skillMenuQuickForecast.Name = "skillMenuQuickForecast";
			this.skillMenuQuickForecast.Padding = new System.Windows.Forms.Padding(4);
			this.skillMenuQuickForecast.Size = new System.Drawing.Size(239, 29);
			this.skillMenuQuickForecast.Text = "xxQuickForecast";
			this.skillMenuQuickForecast.Click += new System.EventHandler(this.toolStripMenuItemQuickForecastClick);
			// 
			// toolStripMenuItemActionSkillNewSkill
			// 
			this.toolStripMenuItemActionSkillNewSkill.BackColor = System.Drawing.Color.White;
			this.toolStripMenuItemActionSkillNewSkill.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_SkillGeneral;
			this.toolStripMenuItemActionSkillNewSkill.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemActionSkillNewSkill.Name = "toolStripMenuItemActionSkillNewSkill";
			this.toolStripMenuItemActionSkillNewSkill.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripMenuItemActionSkillNewSkill.Size = new System.Drawing.Size(239, 29);
			this.toolStripMenuItemActionSkillNewSkill.Text = "xxNewSkillThreeDots";
			this.toolStripMenuItemActionSkillNewSkill.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemActionSkillNewSkill.Click += new System.EventHandler(this.toolStripMenuItemActionSkillNewSkillClick);
			// 
			// toolStripMenuItemActionSkillNewMultisiteSkill
			// 
			this.toolStripMenuItemActionSkillNewMultisiteSkill.BackColor = System.Drawing.Color.White;
			this.toolStripMenuItemActionSkillNewMultisiteSkill.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_SkillMultisite;
			this.toolStripMenuItemActionSkillNewMultisiteSkill.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemActionSkillNewMultisiteSkill.Name = "toolStripMenuItemActionSkillNewMultisiteSkill";
			this.toolStripMenuItemActionSkillNewMultisiteSkill.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripMenuItemActionSkillNewMultisiteSkill.Size = new System.Drawing.Size(239, 29);
			this.toolStripMenuItemActionSkillNewMultisiteSkill.Text = "xxNewMultisiteSkillThreeDots";
			this.toolStripMenuItemActionSkillNewMultisiteSkill.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemActionSkillNewMultisiteSkill.Click += new System.EventHandler(this.toolStripMenuItemActionSkillNewMultisiteSkillClick);
			// 
			// toolStripButtonManageMultisiteDistributions
			// 
			this.toolStripButtonManageMultisiteDistributions.AutoToolTip = false;
			this.toolStripButtonManageMultisiteDistributions.BackColor = System.Drawing.Color.White;
			this.toolStripButtonManageMultisiteDistributions.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_SkillMultiSite_16x16;
			this.toolStripButtonManageMultisiteDistributions.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripButtonManageMultisiteDistributions.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonManageMultisiteDistributions.Margin = new System.Windows.Forms.Padding(0);
			this.toolStripButtonManageMultisiteDistributions.Name = "toolStripButtonManageMultisiteDistributions";
			this.toolStripButtonManageMultisiteDistributions.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripButtonManageMultisiteDistributions.Size = new System.Drawing.Size(239, 29);
			this.toolStripButtonManageMultisiteDistributions.Text = "xxManageMultisiteDistributions";
			this.toolStripButtonManageMultisiteDistributions.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripButtonManageMultisiteDistributions.Click += new System.EventHandler(this.toolStripMenuItemManageDistributionClick);
			// 
			// toolStripMenuItemActionSkillAddNewWorkload
			// 
			this.toolStripMenuItemActionSkillAddNewWorkload.BackColor = System.Drawing.Color.White;
			this.toolStripMenuItemActionSkillAddNewWorkload.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Workload;
			this.toolStripMenuItemActionSkillAddNewWorkload.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemActionSkillAddNewWorkload.Name = "toolStripMenuItemActionSkillAddNewWorkload";
			this.toolStripMenuItemActionSkillAddNewWorkload.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripMenuItemActionSkillAddNewWorkload.Size = new System.Drawing.Size(239, 29);
			this.toolStripMenuItemActionSkillAddNewWorkload.Text = "xxNewWorkloadThreeDots";
			this.toolStripMenuItemActionSkillAddNewWorkload.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemActionSkillAddNewWorkload.Click += new System.EventHandler(this.toolStripMenuItemActionSkillAddNewWorkloadClick);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(239, 6);
			// 
			// SkillsMenuItemJobHistory
			// 
			this.SkillsMenuItemJobHistory.BackColor = System.Drawing.Color.White;
			this.SkillsMenuItemJobHistory.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Reports_variant_32x32;
			this.SkillsMenuItemJobHistory.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.SkillsMenuItemJobHistory.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.SkillsMenuItemJobHistory.Name = "SkillsMenuItemJobHistory";
			this.SkillsMenuItemJobHistory.Padding = new System.Windows.Forms.Padding(4);
			this.SkillsMenuItemJobHistory.Size = new System.Drawing.Size(110, 29);
			this.SkillsMenuItemJobHistory.Text = "xxJobHistory";
			this.SkillsMenuItemJobHistory.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.SkillsMenuItemJobHistory.Click += new System.EventHandler(this.toolStripMenuItemJobHistoryClick);
			// 
			// toolStripMenuItemActionSkillDelete1
			// 
			this.toolStripMenuItemActionSkillDelete1.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Delete_small;
			this.toolStripMenuItemActionSkillDelete1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemActionSkillDelete1.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripMenuItemActionSkillDelete1.Name = "toolStripMenuItemActionSkillDelete1";
			this.toolStripMenuItemActionSkillDelete1.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripMenuItemActionSkillDelete1.Size = new System.Drawing.Size(107, 29);
			this.toolStripMenuItemActionSkillDelete1.Text = "xxDeleteSkill";
			this.toolStripMenuItemActionSkillDelete1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemActionSkillDelete1.Click += new System.EventHandler(this.toolStripMenuItemActionSkillDeleteClick);
			// 
			// toolStripSeparator13
			// 
			this.toolStripSeparator13.Name = "toolStripSeparator13";
			this.toolStripSeparator13.Size = new System.Drawing.Size(239, 6);
			// 
			// toolStripMenuItemActionSkillProperties
			// 
			this.toolStripMenuItemActionSkillProperties.BackColor = System.Drawing.Color.White;
			this.toolStripMenuItemActionSkillProperties.Image = ((System.Drawing.Image)(resources.GetObject("toolStripMenuItemActionSkillProperties.Image")));
			this.toolStripMenuItemActionSkillProperties.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemActionSkillProperties.Name = "toolStripMenuItemActionSkillProperties";
			this.toolStripMenuItemActionSkillProperties.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripMenuItemActionSkillProperties.Size = new System.Drawing.Size(168, 29);
			this.toolStripMenuItemActionSkillProperties.Text = "xxPropertiesThreeDots";
			this.toolStripMenuItemActionSkillProperties.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemActionSkillProperties.Click += new System.EventHandler(this.toolStripMenuItemActionSkillPropertiesClick);
			// 
			// toolStripWorkload
			// 
			this.toolStripWorkload.BackColor = System.Drawing.Color.White;
			this.toolStripWorkload.Dock = System.Windows.Forms.DockStyle.Fill;
			this.toolStripWorkload.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			this.toolStripWorkload.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.toolStripWorkload.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripWorkload.Image = null;
			this.toolStripWorkload.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabelActions,
            this.toolStripMenuItemActionWorkloadNewWorkload,
            this.toolStripMenuItemActionWorkloadNewSkill,
            this.toolStripSeparator12,
            this.toolStripMenuItemActionWorkloadPrepareForecast,
            this.toolStripButtonEditForecast,
            this.workloadMenuQuickForecast,
            this.toolStripSeparator14,
            this.toolStripMenuItemActionWorkloadDelete,
            this.toolStripMenuItemActionWorkloadProperties});
			this.toolStripWorkload.LauncherStyle = Syncfusion.Windows.Forms.Tools.LauncherStyle.Metro;
			this.toolStripWorkload.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
			this.toolStripWorkload.Location = new System.Drawing.Point(12, 0);
			this.toolStripWorkload.Name = "toolStripWorkload";
			this.toolStripWorkload.Office12Mode = false;
			this.toolStripWorkload.Padding = new System.Windows.Forms.Padding(1);
			this.toolStripWorkload.ShowCaption = false;
			this.toolStripWorkload.Size = new System.Drawing.Size(242, 563);
			this.toolStripWorkload.TabIndex = 6;
			this.toolStripWorkload.Text = "xxActions";
			this.toolStripWorkload.Visible = false;
			this.toolStripWorkload.VisualStyle = Syncfusion.Windows.Forms.Tools.ToolStripExStyle.Metro;
			// 
			// toolStripLabelActions
			// 
			this.toolStripLabelActions.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.toolStripLabelActions.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.toolStripLabelActions.Name = "toolStripLabelActions";
			this.toolStripLabelActions.Size = new System.Drawing.Size(239, 19);
			this.toolStripLabelActions.Text = "xxActions";
			// 
			// toolStripMenuItemActionWorkloadNewWorkload
			// 
			this.toolStripMenuItemActionWorkloadNewWorkload.BackColor = System.Drawing.Color.White;
			this.toolStripMenuItemActionWorkloadNewWorkload.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Workload;
			this.toolStripMenuItemActionWorkloadNewWorkload.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemActionWorkloadNewWorkload.Name = "toolStripMenuItemActionWorkloadNewWorkload";
			this.toolStripMenuItemActionWorkloadNewWorkload.Overflow = System.Windows.Forms.ToolStripItemOverflow.AsNeeded;
			this.toolStripMenuItemActionWorkloadNewWorkload.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripMenuItemActionWorkloadNewWorkload.Size = new System.Drawing.Size(239, 29);
			this.toolStripMenuItemActionWorkloadNewWorkload.Text = "xxNewWorkloadThreeDots";
			this.toolStripMenuItemActionWorkloadNewWorkload.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemActionWorkloadNewWorkload.Click += new System.EventHandler(this.toolStripMenuItemActionWorkloadNewWorkloadClick);
			// 
			// toolStripMenuItemActionWorkloadNewSkill
			// 
			this.toolStripMenuItemActionWorkloadNewSkill.BackColor = System.Drawing.Color.White;
			this.toolStripMenuItemActionWorkloadNewSkill.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_SkillGeneral;
			this.toolStripMenuItemActionWorkloadNewSkill.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemActionWorkloadNewSkill.Name = "toolStripMenuItemActionWorkloadNewSkill";
			this.toolStripMenuItemActionWorkloadNewSkill.Overflow = System.Windows.Forms.ToolStripItemOverflow.AsNeeded;
			this.toolStripMenuItemActionWorkloadNewSkill.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripMenuItemActionWorkloadNewSkill.Size = new System.Drawing.Size(239, 29);
			this.toolStripMenuItemActionWorkloadNewSkill.Text = "xxNewSkillThreeDots";
			this.toolStripMenuItemActionWorkloadNewSkill.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemActionWorkloadNewSkill.Visible = false;
			this.toolStripMenuItemActionWorkloadNewSkill.Click += new System.EventHandler(this.toolStripMenuItemActionWorkloadNewSkillClick);
			// 
			// toolStripSeparator12
			// 
			this.toolStripSeparator12.Name = "toolStripSeparator12";
			this.toolStripSeparator12.Size = new System.Drawing.Size(239, 6);
			// 
			// toolStripMenuItemActionWorkloadPrepareForecast
			// 
			this.toolStripMenuItemActionWorkloadPrepareForecast.BackColor = System.Drawing.Color.White;
			this.toolStripMenuItemActionWorkloadPrepareForecast.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_CreateForecast;
			this.toolStripMenuItemActionWorkloadPrepareForecast.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemActionWorkloadPrepareForecast.Name = "toolStripMenuItemActionWorkloadPrepareForecast";
			this.toolStripMenuItemActionWorkloadPrepareForecast.Overflow = System.Windows.Forms.ToolStripItemOverflow.AsNeeded;
			this.toolStripMenuItemActionWorkloadPrepareForecast.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripMenuItemActionWorkloadPrepareForecast.Size = new System.Drawing.Size(239, 29);
			this.toolStripMenuItemActionWorkloadPrepareForecast.Text = "xxPrepareForecastThreeDots";
			this.toolStripMenuItemActionWorkloadPrepareForecast.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemActionWorkloadPrepareForecast.Click += new System.EventHandler(this.toolStripMenuItemActionWorkloadPrepareForecastClick);
			// 
			// toolStripButtonEditForecast
			// 
			this.toolStripButtonEditForecast.BackColor = System.Drawing.Color.White;
			this.toolStripButtonEditForecast.Image = global::Teleopti.Ccc.Win.Properties.Resources.Forecasts2_filled_16x16;
			this.toolStripButtonEditForecast.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripButtonEditForecast.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonEditForecast.Name = "toolStripButtonEditForecast";
			this.toolStripButtonEditForecast.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripButtonEditForecast.Size = new System.Drawing.Size(239, 29);
			this.toolStripButtonEditForecast.Text = "xxOpenForecastThreeDots";
			this.toolStripButtonEditForecast.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripButtonEditForecast.Click += new System.EventHandler(this.toolStripMenuCreateForecastClick);
			// 
			// workloadMenuQuickForecast
			// 
			this.workloadMenuQuickForecast.BackColor = System.Drawing.Color.White;
			this.workloadMenuQuickForecast.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Forecasts_16x16;
			this.workloadMenuQuickForecast.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.workloadMenuQuickForecast.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.workloadMenuQuickForecast.Name = "workloadMenuQuickForecast";
			this.workloadMenuQuickForecast.Overflow = System.Windows.Forms.ToolStripItemOverflow.AsNeeded;
			this.workloadMenuQuickForecast.Padding = new System.Windows.Forms.Padding(4);
			this.workloadMenuQuickForecast.Size = new System.Drawing.Size(239, 29);
			this.workloadMenuQuickForecast.Text = "xxQuickForecast";
			this.workloadMenuQuickForecast.Click += new System.EventHandler(this.toolStripMenuItemQuickForecastClick);
			// 
			// toolStripSeparator14
			// 
			this.toolStripSeparator14.Name = "toolStripSeparator14";
			this.toolStripSeparator14.Size = new System.Drawing.Size(239, 6);
			// 
			// toolStripMenuItemActionWorkloadDelete
			// 
			this.toolStripMenuItemActionWorkloadDelete.BackColor = System.Drawing.Color.White;
			this.toolStripMenuItemActionWorkloadDelete.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Delete;
			this.toolStripMenuItemActionWorkloadDelete.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemActionWorkloadDelete.Name = "toolStripMenuItemActionWorkloadDelete";
			this.toolStripMenuItemActionWorkloadDelete.Overflow = System.Windows.Forms.ToolStripItemOverflow.AsNeeded;
			this.toolStripMenuItemActionWorkloadDelete.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripMenuItemActionWorkloadDelete.Size = new System.Drawing.Size(239, 29);
			this.toolStripMenuItemActionWorkloadDelete.Text = "xxDelete";
			this.toolStripMenuItemActionWorkloadDelete.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemActionWorkloadDelete.Click += new System.EventHandler(this.toolStripMenuItemActionWorkloadDeleteClick);
			// 
			// toolStripMenuItemActionWorkloadProperties
			// 
			this.toolStripMenuItemActionWorkloadProperties.BackColor = System.Drawing.Color.White;
			this.toolStripMenuItemActionWorkloadProperties.Image = ((System.Drawing.Image)(resources.GetObject("toolStripMenuItemActionWorkloadProperties.Image")));
			this.toolStripMenuItemActionWorkloadProperties.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemActionWorkloadProperties.Name = "toolStripMenuItemActionWorkloadProperties";
			this.toolStripMenuItemActionWorkloadProperties.Overflow = System.Windows.Forms.ToolStripItemOverflow.AsNeeded;
			this.toolStripMenuItemActionWorkloadProperties.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripMenuItemActionWorkloadProperties.Size = new System.Drawing.Size(239, 29);
			this.toolStripMenuItemActionWorkloadProperties.Text = "xxPropertiesThreeDots";
			this.toolStripMenuItemActionWorkloadProperties.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemActionWorkloadProperties.Click += new System.EventHandler(this.toolStripMenuItemActionWorkloadPropertiesClick);
			// 
			// toolStripQueues
			// 
			this.toolStripQueues.BackColor = System.Drawing.Color.White;
			this.toolStripQueues.Dock = System.Windows.Forms.DockStyle.Fill;
			this.toolStripQueues.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			this.toolStripQueues.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.toolStripQueues.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripQueues.Image = null;
			this.toolStripQueues.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel4,
            this.toolStripMenuItemActionQueueSourceNewWorkload,
            this.toolStripSeparator15,
            this.toolStripMenuItemActionQueueSourceDelete});
			this.toolStripQueues.LauncherStyle = Syncfusion.Windows.Forms.Tools.LauncherStyle.Metro;
			this.toolStripQueues.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
			this.toolStripQueues.Location = new System.Drawing.Point(12, 0);
			this.toolStripQueues.Name = "toolStripQueues";
			this.toolStripQueues.Office12Mode = false;
			this.toolStripQueues.Padding = new System.Windows.Forms.Padding(1);
			this.toolStripQueues.ShowCaption = false;
			this.toolStripQueues.Size = new System.Drawing.Size(242, 337);
			this.toolStripQueues.TabIndex = 5;
			this.toolStripQueues.Text = "xxActions";
			this.toolStripQueues.Visible = false;
			this.toolStripQueues.VisualStyle = Syncfusion.Windows.Forms.Tools.ToolStripExStyle.Metro;
			// 
			// toolStripLabel4
			// 
			this.toolStripLabel4.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.toolStripLabel4.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.toolStripLabel4.Name = "toolStripLabel4";
			this.toolStripLabel4.Size = new System.Drawing.Size(239, 19);
			this.toolStripLabel4.Text = "xxActions";
			// 
			// toolStripMenuItemActionQueueSourceNewWorkload
			// 
			this.toolStripMenuItemActionQueueSourceNewWorkload.BackColor = System.Drawing.Color.White;
			this.toolStripMenuItemActionQueueSourceNewWorkload.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Workload;
			this.toolStripMenuItemActionQueueSourceNewWorkload.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemActionQueueSourceNewWorkload.Name = "toolStripMenuItemActionQueueSourceNewWorkload";
			this.toolStripMenuItemActionQueueSourceNewWorkload.Overflow = System.Windows.Forms.ToolStripItemOverflow.AsNeeded;
			this.toolStripMenuItemActionQueueSourceNewWorkload.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripMenuItemActionQueueSourceNewWorkload.Size = new System.Drawing.Size(239, 29);
			this.toolStripMenuItemActionQueueSourceNewWorkload.Text = "xxNewWorkloadThreeDots";
			this.toolStripMenuItemActionQueueSourceNewWorkload.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemActionQueueSourceNewWorkload.Click += new System.EventHandler(this.toolStripMenuItemActionQueueSourceNewWorkloadClick);
			// 
			// toolStripSeparator15
			// 
			this.toolStripSeparator15.Name = "toolStripSeparator15";
			this.toolStripSeparator15.Size = new System.Drawing.Size(239, 6);
			// 
			// toolStripMenuItemActionQueueSourceDelete
			// 
			this.toolStripMenuItemActionQueueSourceDelete.BackColor = System.Drawing.Color.White;
			this.toolStripMenuItemActionQueueSourceDelete.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Delete;
			this.toolStripMenuItemActionQueueSourceDelete.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemActionQueueSourceDelete.Name = "toolStripMenuItemActionQueueSourceDelete";
			this.toolStripMenuItemActionQueueSourceDelete.Overflow = System.Windows.Forms.ToolStripItemOverflow.AsNeeded;
			this.toolStripMenuItemActionQueueSourceDelete.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripMenuItemActionQueueSourceDelete.Size = new System.Drawing.Size(239, 29);
			this.toolStripMenuItemActionQueueSourceDelete.Text = "xxDelete";
			this.toolStripMenuItemActionQueueSourceDelete.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemActionQueueSourceDelete.Click += new System.EventHandler(this.toolStripMenuItemActionQueueSourceDeleteClick);
			// 
			// toolStripSkillTypes
			// 
			this.toolStripSkillTypes.BackColor = System.Drawing.Color.White;
			this.toolStripSkillTypes.Dock = System.Windows.Forms.DockStyle.Fill;
			this.toolStripSkillTypes.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			this.toolStripSkillTypes.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.toolStripSkillTypes.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripSkillTypes.Image = null;
			this.toolStripSkillTypes.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel5,
            this.toolStripMenuItemActionSkillTypeNewSkill,
            this.toolStripMenuItemActionSkillTypeNewMultisiteSkill,
            this.skillTypeMenuQuickForecast,
            this.toolStripSeparator11,
            this.skillTypeMenuJobHistory});
			this.toolStripSkillTypes.LauncherStyle = Syncfusion.Windows.Forms.Tools.LauncherStyle.Metro;
			this.toolStripSkillTypes.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
			this.toolStripSkillTypes.Location = new System.Drawing.Point(12, 0);
			this.toolStripSkillTypes.Name = "toolStripSkillTypes";
			this.toolStripSkillTypes.Office12Mode = false;
			this.toolStripSkillTypes.Padding = new System.Windows.Forms.Padding(1);
			this.toolStripSkillTypes.ShowCaption = false;
			this.toolStripSkillTypes.Size = new System.Drawing.Size(242, 337);
			this.toolStripSkillTypes.TabIndex = 6;
			this.toolStripSkillTypes.Text = "xxActions";
			this.toolStripSkillTypes.Visible = false;
			this.toolStripSkillTypes.VisualStyle = Syncfusion.Windows.Forms.Tools.ToolStripExStyle.Metro;
			// 
			// toolStripLabel5
			// 
			this.toolStripLabel5.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.toolStripLabel5.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.toolStripLabel5.Name = "toolStripLabel5";
			this.toolStripLabel5.Size = new System.Drawing.Size(239, 19);
			this.toolStripLabel5.Text = "xxActions";
			// 
			// toolStripMenuItemActionSkillTypeNewSkill
			// 
			this.toolStripMenuItemActionSkillTypeNewSkill.BackColor = System.Drawing.Color.White;
			this.toolStripMenuItemActionSkillTypeNewSkill.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_SkillGeneral;
			this.toolStripMenuItemActionSkillTypeNewSkill.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemActionSkillTypeNewSkill.Name = "toolStripMenuItemActionSkillTypeNewSkill";
			this.toolStripMenuItemActionSkillTypeNewSkill.Overflow = System.Windows.Forms.ToolStripItemOverflow.AsNeeded;
			this.toolStripMenuItemActionSkillTypeNewSkill.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripMenuItemActionSkillTypeNewSkill.Size = new System.Drawing.Size(239, 29);
			this.toolStripMenuItemActionSkillTypeNewSkill.Text = "xxNewSkillThreeDots";
			this.toolStripMenuItemActionSkillTypeNewSkill.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemActionSkillTypeNewSkill.Click += new System.EventHandler(this.toolStripMenuItemActionSkillTypeNewSkillClick);
			// 
			// toolStripMenuItemActionSkillTypeNewMultisiteSkill
			// 
			this.toolStripMenuItemActionSkillTypeNewMultisiteSkill.BackColor = System.Drawing.Color.White;
			this.toolStripMenuItemActionSkillTypeNewMultisiteSkill.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_SkillMultisite;
			this.toolStripMenuItemActionSkillTypeNewMultisiteSkill.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemActionSkillTypeNewMultisiteSkill.Name = "toolStripMenuItemActionSkillTypeNewMultisiteSkill";
			this.toolStripMenuItemActionSkillTypeNewMultisiteSkill.Overflow = System.Windows.Forms.ToolStripItemOverflow.AsNeeded;
			this.toolStripMenuItemActionSkillTypeNewMultisiteSkill.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripMenuItemActionSkillTypeNewMultisiteSkill.Size = new System.Drawing.Size(239, 29);
			this.toolStripMenuItemActionSkillTypeNewMultisiteSkill.Text = "xxNewMultisiteSkillThreeDots";
			this.toolStripMenuItemActionSkillTypeNewMultisiteSkill.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItemActionSkillTypeNewMultisiteSkill.Click += new System.EventHandler(this.toolStripMenuItemActionSkillTypeNewMultisiteSkillClick);
			// 
			// skillTypeMenuQuickForecast
			// 
			this.skillTypeMenuQuickForecast.BackColor = System.Drawing.Color.White;
			this.skillTypeMenuQuickForecast.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Forecasts_16x16;
			this.skillTypeMenuQuickForecast.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.skillTypeMenuQuickForecast.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.skillTypeMenuQuickForecast.Name = "skillTypeMenuQuickForecast";
			this.skillTypeMenuQuickForecast.Overflow = System.Windows.Forms.ToolStripItemOverflow.AsNeeded;
			this.skillTypeMenuQuickForecast.Padding = new System.Windows.Forms.Padding(4);
			this.skillTypeMenuQuickForecast.Size = new System.Drawing.Size(239, 29);
			this.skillTypeMenuQuickForecast.Text = "xxQuickForecast";
			this.skillTypeMenuQuickForecast.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.skillTypeMenuQuickForecast.Click += new System.EventHandler(this.toolStripMenuItemQuickForecastClick);
			// 
			// toolStripSeparator11
			// 
			this.toolStripSeparator11.Name = "toolStripSeparator11";
			this.toolStripSeparator11.Size = new System.Drawing.Size(239, 6);
			// 
			// skillTypeMenuJobHistory
			// 
			this.skillTypeMenuJobHistory.BackColor = System.Drawing.Color.White;
			this.skillTypeMenuJobHistory.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Reports_variant_32x32;
			this.skillTypeMenuJobHistory.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.skillTypeMenuJobHistory.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.skillTypeMenuJobHistory.Name = "skillTypeMenuJobHistory";
			this.skillTypeMenuJobHistory.Overflow = System.Windows.Forms.ToolStripItemOverflow.AsNeeded;
			this.skillTypeMenuJobHistory.Padding = new System.Windows.Forms.Padding(4);
			this.skillTypeMenuJobHistory.Size = new System.Drawing.Size(239, 29);
			this.skillTypeMenuJobHistory.Text = "xxJobHistory";
			this.skillTypeMenuJobHistory.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.skillTypeMenuJobHistory.Click += new System.EventHandler(this.toolStripMenuItemJobHistoryClick);
			// 
			// toolStrip1
			// 
			this.toolStrip1.BackColor = System.Drawing.Color.White;
			this.toolStrip1.CaptionAlignment = Syncfusion.Windows.Forms.Tools.CaptionAlignment.Center;
			this.toolStrip1.CaptionFont = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.toolStrip1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.toolStrip1.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			this.toolStrip1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip1.Image = null;
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.toolStripMenuItem2,
            this.toolStripMenuItem3,
            this.toolStripMenuItem8,
            this.toolStripMenuItem6,
            this.toolStripSeparator6,
            this.toolStripMenuItem4,
            this.toolStripMenuItem5,
            this.toolStripMenuItem7,
            this.toolStripSeparator7,
            this.toolStripMenuItem11,
            this.toolStripMenuItem9,
            this.toolStripSeparator8,
            this.toolStripMenuItem10});
			this.toolStrip1.LauncherStyle = Syncfusion.Windows.Forms.Tools.LauncherStyle.Metro;
			this.toolStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
			this.toolStrip1.Location = new System.Drawing.Point(12, 0);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Office12Mode = false;
			this.toolStrip1.Padding = new System.Windows.Forms.Padding(1);
			this.toolStrip1.ShowCaption = false;
			this.toolStrip1.ShowLauncher = false;
			this.toolStrip1.Size = new System.Drawing.Size(242, 563);
			this.toolStrip1.TabIndex = 0;
			this.toolStrip1.Text = "xxActions";
			this.toolStrip1.Visible = false;
			this.toolStrip1.VisualStyle = Syncfusion.Windows.Forms.Tools.ToolStripExStyle.Metro;
			// 
			// toolStripLabel1
			// 
			this.toolStripLabel1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.toolStripLabel1.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.toolStripLabel1.Name = "toolStripLabel1";
			this.toolStripLabel1.Size = new System.Drawing.Size(239, 17);
			this.toolStripLabel1.Text = "xxActions";
			// 
			// toolStripMenuItem2
			// 
			this.toolStripMenuItem2.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_CreateForecast;
			this.toolStripMenuItem2.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItem2.Name = "toolStripMenuItem2";
			this.toolStripMenuItem2.Overflow = System.Windows.Forms.ToolStripItemOverflow.AsNeeded;
			this.toolStripMenuItem2.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripMenuItem2.Size = new System.Drawing.Size(239, 29);
			this.toolStripMenuItem2.Text = "xxCreateForecastThreeDots";
			this.toolStripMenuItem2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// toolStripMenuItem3
			// 
			this.toolStripMenuItem3.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Workload;
			this.toolStripMenuItem3.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItem3.Name = "toolStripMenuItem3";
			this.toolStripMenuItem3.Overflow = System.Windows.Forms.ToolStripItemOverflow.AsNeeded;
			this.toolStripMenuItem3.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripMenuItem3.Size = new System.Drawing.Size(239, 29);
			this.toolStripMenuItem3.Text = "xxNewWorkloadThreeDots";
			this.toolStripMenuItem3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// toolStripMenuItem8
			// 
			this.toolStripMenuItem8.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_SkillMultisite;
			this.toolStripMenuItem8.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItem8.Name = "toolStripMenuItem8";
			this.toolStripMenuItem8.Overflow = System.Windows.Forms.ToolStripItemOverflow.AsNeeded;
			this.toolStripMenuItem8.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripMenuItem8.Size = new System.Drawing.Size(239, 29);
			this.toolStripMenuItem8.Text = "xxNewMultisiteSkillThreeDots";
			this.toolStripMenuItem8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// toolStripMenuItem6
			// 
			this.toolStripMenuItem6.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_SkillGeneral;
			this.toolStripMenuItem6.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItem6.Name = "toolStripMenuItem6";
			this.toolStripMenuItem6.Overflow = System.Windows.Forms.ToolStripItemOverflow.AsNeeded;
			this.toolStripMenuItem6.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripMenuItem6.Size = new System.Drawing.Size(239, 29);
			this.toolStripMenuItem6.Text = "xxNewSkillThreeDots";
			this.toolStripMenuItem6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// toolStripSeparator6
			// 
			this.toolStripSeparator6.Name = "toolStripSeparator6";
			this.toolStripSeparator6.Size = new System.Drawing.Size(239, 6);
			// 
			// toolStripMenuItem4
			// 
			this.toolStripMenuItem4.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Template2;
			this.toolStripMenuItem4.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItem4.Name = "toolStripMenuItem4";
			this.toolStripMenuItem4.Overflow = System.Windows.Forms.ToolStripItemOverflow.AsNeeded;
			this.toolStripMenuItem4.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripMenuItem4.Size = new System.Drawing.Size(239, 29);
			this.toolStripMenuItem4.Text = "xxManageDayTemplatesThreeDots";
			this.toolStripMenuItem4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// toolStripMenuItem5
			// 
			this.toolStripMenuItem5.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_LineGraph;
			this.toolStripMenuItem5.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItem5.Name = "toolStripMenuItem5";
			this.toolStripMenuItem5.Overflow = System.Windows.Forms.ToolStripItemOverflow.AsNeeded;
			this.toolStripMenuItem5.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripMenuItem5.Size = new System.Drawing.Size(239, 29);
			this.toolStripMenuItem5.Text = "xxManageDayDistributionsThreeDots";
			this.toolStripMenuItem5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItem5.Visible = false;
			// 
			// toolStripMenuItem7
			// 
			this.toolStripMenuItem7.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_OpenForecastWorkflow;
			this.toolStripMenuItem7.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItem7.Name = "toolStripMenuItem7";
			this.toolStripMenuItem7.Overflow = System.Windows.Forms.ToolStripItemOverflow.AsNeeded;
			this.toolStripMenuItem7.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripMenuItem7.Size = new System.Drawing.Size(239, 29);
			this.toolStripMenuItem7.Text = "xxPrepareForecastThreeDots";
			this.toolStripMenuItem7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// toolStripSeparator7
			// 
			this.toolStripSeparator7.Name = "toolStripSeparator7";
			this.toolStripSeparator7.Size = new System.Drawing.Size(239, 6);
			// 
			// toolStripMenuItem11
			// 
			this.toolStripMenuItem11.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Delete;
			this.toolStripMenuItem11.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItem11.Name = "toolStripMenuItem11";
			this.toolStripMenuItem11.Overflow = System.Windows.Forms.ToolStripItemOverflow.AsNeeded;
			this.toolStripMenuItem11.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripMenuItem11.Size = new System.Drawing.Size(239, 29);
			this.toolStripMenuItem11.Text = "xxDelete";
			this.toolStripMenuItem11.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// toolStripMenuItem9
			// 
			this.toolStripMenuItem9.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Delete;
			this.toolStripMenuItem9.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItem9.Name = "toolStripMenuItem9";
			this.toolStripMenuItem9.Overflow = System.Windows.Forms.ToolStripItemOverflow.AsNeeded;
			this.toolStripMenuItem9.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripMenuItem9.Size = new System.Drawing.Size(239, 29);
			this.toolStripMenuItem9.Text = "xxDelete";
			this.toolStripMenuItem9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// toolStripSeparator8
			// 
			this.toolStripSeparator8.Name = "toolStripSeparator8";
			this.toolStripSeparator8.Size = new System.Drawing.Size(239, 6);
			// 
			// toolStripMenuItem10
			// 
			this.toolStripMenuItem10.Image = ((System.Drawing.Image)(resources.GetObject("toolStripMenuItem10.Image")));
			this.toolStripMenuItem10.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripMenuItem10.Name = "toolStripMenuItem10";
			this.toolStripMenuItem10.Overflow = System.Windows.Forms.ToolStripItemOverflow.AsNeeded;
			this.toolStripMenuItem10.Padding = new System.Windows.Forms.Padding(4);
			this.toolStripMenuItem10.Size = new System.Drawing.Size(239, 29);
			this.toolStripMenuItem10.Text = "xxPropertiesThreeDots";
			this.toolStripMenuItem10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// ForecasterNavigator
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.splitContainer1);
			this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			this.Margin = new System.Windows.Forms.Padding(0);
			this.Name = "ForecasterNavigator";
			this.Padding = new System.Windows.Forms.Padding(0);
			this.Size = new System.Drawing.Size(254, 853);
			this.contextMenuStripSkillTypes.ResumeLayout(false);
			this.contextMenuStripSkills.ResumeLayout(false);
			this.contextMenuStripWorkloads.ResumeLayout(false);
			this.contextMenuStripQueues.ResumeLayout(false);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.treeViewSkills)).EndInit();
			this.toolStripSkills.ResumeLayout(false);
			this.toolStripSkills.PerformLayout();
			this.toolStripWorkload.ResumeLayout(false);
			this.toolStripWorkload.PerformLayout();
			this.toolStripQueues.ResumeLayout(false);
			this.toolStripQueues.PerformLayout();
			this.toolStripSkillTypes.ResumeLayout(false);
			this.toolStripSkillTypes.PerformLayout();
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.ResumeLayout(false);

        }


        #endregion

		  private Syncfusion.Windows.Forms.Tools.TreeViewAdv treeViewSkills;
        private System.Windows.Forms.ImageList imageListSkillTypes;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripSkills;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSkillNew;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSkillsDelete;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSkillsProperties;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripWorkloads;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemNewWorkload;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDeleteWorkload;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemWorkloadProperties;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripQueues;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRemoveQueue;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemWorkloadSkillNew;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemQueueWorkloadNew;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripSkillTypes;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSkillTypesSkillNew;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuCreateForecast;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemWorkloadPrepareWorkload;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemManageDayTemplates;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemMultisiteSkillNew;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSkillTypesMultisiteSkillNew;
        private System.Windows.Forms.SplitContainer splitContainer1;
		private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem6;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem5;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem7;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem8;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem9;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem11;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem10;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
		private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripWorkload;
        private System.Windows.Forms.ToolStripLabel toolStripLabelActions;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemActionWorkloadNewWorkload;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemActionWorkloadNewSkill;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator12;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemActionWorkloadPrepareForecast;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemActionWorkloadDelete;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator14;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemActionWorkloadProperties;
		private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripSkills;
		private System.Windows.Forms.ToolStripLabel toolStripLabelSkillActions;
		private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripSkillTypes;
        private System.Windows.Forms.ToolStripLabel toolStripLabel5;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemActionSkillTypeNewMultisiteSkill;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemActionSkillTypeNewSkill;
		private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripQueues;
        private System.Windows.Forms.ToolStripLabel toolStripLabel4;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemActionQueueSourceNewWorkload;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator15;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemActionQueueSourceDelete;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem xxEditForecastThreeDotsToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton toolStripButtonEditForecast;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemWorkloadNew;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemManageMultisiteDistributions;
        private System.Windows.Forms.ToolStripButton toolStripButtonManageMultisiteDistributions;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemCopyTo;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorExport;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemExport;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemJobHistory;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSkillsImportForecast;
		private System.Windows.Forms.ToolStripMenuItem xxQuickForecastToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem xxQuickForecastToolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem skillTypeMenuQuickForecast;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
		private System.Windows.Forms.ToolStripMenuItem skillTypeMenuJobHistory;
		private System.Windows.Forms.ToolStripMenuItem workloadMenuQuickForecast;
		private System.Windows.Forms.ToolStripLabel toolStripLabel1;
		private ToolStripSeparator toolStripSeparator13;
		private ToolStripButton toolStripMenuItemActionSkillDelete1;
		private ToolStripButton toolStripMenuItemActionSkillPrepareSkill;
		private ToolStripButton toolStripMenuItemActionSkillManageTemplates;
		private ToolStripButton toolStripMenuItemActionSkillImportForecast;
		private ToolStripButton skillMenuQuickForecast;
		private ToolStripButton toolStripMenuItemActionSkillNewSkill;
		private ToolStripButton toolStripMenuItemActionSkillNewMultisiteSkill;
		private ToolStripButton toolStripMenuItemActionSkillAddNewWorkload;
		private ToolStripButton SkillsMenuItemJobHistory;
		private ToolStripButton toolStripMenuItemActionSkillProperties;
    }
}
