using System;
using System.Windows.Forms;
using Teleopti.Ccc.Win.Common.Controls;

namespace Teleopti.Ccc.Win.Common.Configuration
{
    partial class WorkflowControlSetView
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
                releaseMangedResources();

                if (_gridHelper!=null)
                    _gridHelper.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxPointsPerShiftCategory"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxFairnessSystemUsedForScheduling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxEqualOfEachShiftCategory")]
		private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WorkflowControlSetView));
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle1 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle2 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle3 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle4 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle5 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle6 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle7 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle8 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			this.gradientPanelHeader = new Syncfusion.Windows.Forms.Tools.GradientPanel();
			this.tableLayoutPanelHeader = new System.Windows.Forms.TableLayoutPanel();
			this.labelHeader = new System.Windows.Forms.Label();
			this.tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanelSelectWorkloadControlSet = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanelSubHeader1 = new System.Windows.Forms.TableLayoutPanel();
			this.labelSubHeader1 = new System.Windows.Forms.Label();
			this.buttonDelete = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonNew = new System.Windows.Forms.Button();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.autoLabelChangeInfo = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.autoLabel5 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.autoLabel1 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.comboBoxAdvWorkflowControlSet = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.textBoxDescription = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
			this.autoLabelInfoAboutChanges = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.tabControlAdvArea = new Syncfusion.Windows.Forms.Tools.TabControlAdv();
			this.tabPageBasic = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.tableLayoutPanelBasic = new System.Windows.Forms.TableLayoutPanel();
			this.twoListSelectorAbsences = new Teleopti.Ccc.Win.Common.Controls.TwoListSelector();
			this.panel8 = new System.Windows.Forms.Panel();
			this.labelAbsencesAvailableForExtendedPreference = new System.Windows.Forms.Label();
			this.tableLayoutPanelStudentAvailability = new System.Windows.Forms.TableLayoutPanel();
			this.labelStudentAvailabilityPeriod = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.dateSelectionFromToStudentAvailability = new Teleopti.Ccc.Win.Common.Controls.DateSelection.DateSelectionFromTo();
			this.dateSelectionFromToIsOpenStudentAvailability = new Teleopti.Ccc.Win.Common.Controls.DateSelection.DateSelectionFromTo();
			this.twoListSelectorCategories = new Teleopti.Ccc.Win.Common.Controls.TwoListSelector();
			this.panelVisualizationButtons = new System.Windows.Forms.Panel();
			this.buttonZoomOut = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonZoomIn = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonPanLeft = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonPanRight = new Syncfusion.Windows.Forms.ButtonAdv();
			this.twoListSelectorDayOffs = new Teleopti.Ccc.Win.Common.Controls.TwoListSelector();
			this.dateOnlyPeriodsVisualizer1 = new Teleopti.Ccc.Win.Common.Controls.DateTimePeriodVisualizer.DateOnlyPeriodsVisualizer();
			this.panel4 = new System.Windows.Forms.Panel();
			this.labelShiftCategoriesAvailableForExtendedPreference = new System.Windows.Forms.Label();
			this.panel3 = new System.Windows.Forms.Panel();
			this.labelDaysOffAvailableForExtendedPreferences = new System.Windows.Forms.Label();
			this.panel2 = new System.Windows.Forms.Panel();
			this.labelAllowedPreferenceActivity = new System.Windows.Forms.Label();
			this.comboBoxAdvAllowedPreferenceActivity = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.panelBasic = new System.Windows.Forms.Panel();
			this.labelBasic = new System.Windows.Forms.Label();
			this.tableLayoutPanelBasicSchedule = new System.Windows.Forms.TableLayoutPanel();
			this.labelWriteProtect = new System.Windows.Forms.Label();
			this.integerTextBoxWriteProtect = new Teleopti.Ccc.Win.Common.Controls.NullableIntegerTextBox();
			this.labelPublishSchedules = new System.Windows.Forms.Label();
			this.dateTimePickerAdvPublishedTo = new Syncfusion.Windows.Forms.Tools.DateTimePickerAdv();
			this.tableLayoutPanelOpenPreference = new System.Windows.Forms.TableLayoutPanel();
			this.labelPreferencePeriod = new System.Windows.Forms.Label();
			this.labelIsOpen = new System.Windows.Forms.Label();
			this.dateSelectionFromToPreferencePeriod = new Teleopti.Ccc.Win.Common.Controls.DateSelection.DateSelectionFromTo();
			this.dateSelectionFromToIsOpen = new Teleopti.Ccc.Win.Common.Controls.DateSelection.DateSelectionFromTo();
			this.panelOpenPreference = new System.Windows.Forms.Panel();
			this.labelOpenPreference = new System.Windows.Forms.Label();
			this.panel6 = new System.Windows.Forms.Panel();
			this.label4 = new System.Windows.Forms.Label();
			this.radioButtonAdvFairnessPoints = new Syncfusion.Windows.Forms.Tools.RadioButtonAdv();
			this.radioButtonAdvFairnessEqual = new Syncfusion.Windows.Forms.Tools.RadioButtonAdv();
			this.panel7 = new System.Windows.Forms.Panel();
			this.labelOpenStudentAvailability = new System.Windows.Forms.Label();
			this.tabPageAdvAbsenceRequests = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.tableLayoutPanelAbsenceRequestPeriods = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanelOpenForAbsenceRequests = new System.Windows.Forms.TableLayoutPanel();
			this.labelOpenForAbsenceRequests = new System.Windows.Forms.Label();
			this.buttonDeleteAbsenceRequestPeriod = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAddAbsenceRequestPeriod = new System.Windows.Forms.Button();
			this.gridControlAbsenceRequestOpenPeriods = new Syncfusion.Windows.Forms.Grid.GridControl();
			this.contextMenuStripOpenPeriodsGrid = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.toolStripMenuItemFromToPeriod = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemRollingPeriod = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemDelete = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemMoveUp = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemMoveDown = new System.Windows.Forms.ToolStripMenuItem();
			this.panel1 = new System.Windows.Forms.Panel();
			this.dateTimePickerAdvViewpoint = new Syncfusion.Windows.Forms.Tools.DateTimePickerAdv();
			this.labelAbsenceRequestsVisualisation = new System.Windows.Forms.Label();
			this.gridControlVisualisation = new Syncfusion.Windows.Forms.Grid.GridControl();
			this.tableLayoutPanelNextPreviousPeriod = new System.Windows.Forms.TableLayoutPanel();
			this.buttonAdvPreviousProjectionPeriod = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvNextProjectionPeriod = new Syncfusion.Windows.Forms.ButtonAdv();
			this.tabPageAdvShiftTradeRequest = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.tableLayoutPanelShiftTrade = new System.Windows.Forms.TableLayoutPanel();
			this.panel5 = new System.Windows.Forms.Panel();
			this.label3 = new System.Windows.Forms.Label();
			this.panelOpenForShiftTrade = new System.Windows.Forms.Panel();
			this.labelOpenForShiftTrade = new System.Windows.Forms.Label();
			this.panelTolerance = new System.Windows.Forms.Panel();
			this.labelTolerance = new System.Windows.Forms.Label();
			this.minMaxIntegerTextBoxControl1 = new Teleopti.Ccc.Win.Common.Controls.MinMaxIntegerTextBoxControl();
			this.twoListSelectorMatchingSkills = new Teleopti.Ccc.Win.Common.Controls.TwoListSelector();
			this.panelMatchingSkills = new System.Windows.Forms.Panel();
			this.labelMatchingSkills = new System.Windows.Forms.Label();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.timeSpanTextBox1 = new Teleopti.Ccc.Win.Common.Controls.TimeSpanTextBox();
			this.labelTolerancePosNeg = new System.Windows.Forms.Label();
			this.labelHMm = new System.Windows.Forms.Label();
			this.checkBoxAdvAutoGrant = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
			this.labelMinimumTimePerWeek = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.textBoxExtMinTimePerWeek = new Teleopti.Ccc.Win.Common.Controls.TimeSpanTextBox();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanelHeader)).BeginInit();
			this.gradientPanelHeader.SuspendLayout();
			this.tableLayoutPanelHeader.SuspendLayout();
			this.tableLayoutPanelMain.SuspendLayout();
			this.tableLayoutPanelSelectWorkloadControlSet.SuspendLayout();
			this.tableLayoutPanelSubHeader1.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvWorkflowControlSet)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.textBoxDescription)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.tabControlAdvArea)).BeginInit();
			this.tabControlAdvArea.SuspendLayout();
			this.tabPageBasic.SuspendLayout();
			this.tableLayoutPanelBasic.SuspendLayout();
			this.panel8.SuspendLayout();
			this.tableLayoutPanelStudentAvailability.SuspendLayout();
			this.panelVisualizationButtons.SuspendLayout();
			this.panel4.SuspendLayout();
			this.panel3.SuspendLayout();
			this.panel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvAllowedPreferenceActivity)).BeginInit();
			this.panelBasic.SuspendLayout();
			this.tableLayoutPanelBasicSchedule.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvPublishedTo)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvPublishedTo.Calendar)).BeginInit();
			this.tableLayoutPanelOpenPreference.SuspendLayout();
			this.panelOpenPreference.SuspendLayout();
			this.panel6.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonAdvFairnessPoints)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonAdvFairnessEqual)).BeginInit();
			this.panel7.SuspendLayout();
			this.tabPageAdvAbsenceRequests.SuspendLayout();
			this.tableLayoutPanelAbsenceRequestPeriods.SuspendLayout();
			this.tableLayoutPanelOpenForAbsenceRequests.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridControlAbsenceRequestOpenPeriods)).BeginInit();
			this.contextMenuStripOpenPeriodsGrid.SuspendLayout();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvViewpoint)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvViewpoint.Calendar)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.gridControlVisualisation)).BeginInit();
			this.tableLayoutPanelNextPreviousPeriod.SuspendLayout();
			this.tabPageAdvShiftTradeRequest.SuspendLayout();
			this.tableLayoutPanelShiftTrade.SuspendLayout();
			this.panel5.SuspendLayout();
			this.panelOpenForShiftTrade.SuspendLayout();
			this.panelTolerance.SuspendLayout();
			this.panelMatchingSkills.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvAutoGrant)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
			this.SuspendLayout();
			// 
			// gradientPanelHeader
			// 
			this.gradientPanelHeader.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gradientPanelHeader.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.gradientPanelHeader.BackColor = System.Drawing.SystemColors.ActiveCaption;
			this.gradientPanelHeader.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.LightSteelBlue, System.Drawing.Color.White);
			this.gradientPanelHeader.BorderSingle = System.Windows.Forms.ButtonBorderStyle.None;
			this.gradientPanelHeader.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.gradientPanelHeader.Controls.Add(this.tableLayoutPanelHeader);
			this.gradientPanelHeader.Location = new System.Drawing.Point(0, 0);
			this.gradientPanelHeader.Name = "gradientPanelHeader";
			this.gradientPanelHeader.Padding = new System.Windows.Forms.Padding(10);
			this.gradientPanelHeader.Size = new System.Drawing.Size(838, 55);
			this.gradientPanelHeader.TabIndex = 0;
			// 
			// tableLayoutPanelHeader
			// 
			this.tableLayoutPanelHeader.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanelHeader.ColumnCount = 1;
			this.tableLayoutPanelHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 905F));
			this.tableLayoutPanelHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelHeader.Controls.Add(this.labelHeader, 1, 0);
			this.tableLayoutPanelHeader.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelHeader.Location = new System.Drawing.Point(10, 10);
			this.tableLayoutPanelHeader.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanelHeader.Name = "tableLayoutPanelHeader";
			this.tableLayoutPanelHeader.RowCount = 1;
			this.tableLayoutPanelHeader.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelHeader.Size = new System.Drawing.Size(818, 35);
			this.tableLayoutPanelHeader.TabIndex = 0;
			// 
			// labelHeader
			// 
			this.labelHeader.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelHeader.AutoSize = true;
			this.labelHeader.Font = new System.Drawing.Font("Tahoma", 11.25F);
			this.labelHeader.ForeColor = System.Drawing.Color.MidnightBlue;
			this.labelHeader.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.labelHeader.Location = new System.Drawing.Point(3, 8);
			this.labelHeader.Name = "labelHeader";
			this.labelHeader.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
			this.labelHeader.Size = new System.Drawing.Size(213, 18);
			this.labelHeader.TabIndex = 0;
			this.labelHeader.Text = "xxManageWorkflowControlSets";
			// 
			// tableLayoutPanelMain
			// 
			this.tableLayoutPanelMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanelMain.AutoScroll = true;
			this.tableLayoutPanelMain.ColumnCount = 1;
			this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelMain.Controls.Add(this.tableLayoutPanelSelectWorkloadControlSet, 0, 0);
			this.tableLayoutPanelMain.Controls.Add(this.tabControlAdvArea, 0, 1);
			this.tableLayoutPanelMain.Location = new System.Drawing.Point(3, 60);
			this.tableLayoutPanelMain.Name = "tableLayoutPanelMain";
			this.tableLayoutPanelMain.RowCount = 2;
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 150F));
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelMain.Size = new System.Drawing.Size(835, 531);
			this.tableLayoutPanelMain.TabIndex = 0;
			// 
			// tableLayoutPanelSelectWorkloadControlSet
			// 
			this.tableLayoutPanelSelectWorkloadControlSet.AllowDrop = true;
			this.tableLayoutPanelSelectWorkloadControlSet.ColumnCount = 1;
			this.tableLayoutPanelSelectWorkloadControlSet.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelSelectWorkloadControlSet.Controls.Add(this.tableLayoutPanelSubHeader1, 0, 0);
			this.tableLayoutPanelSelectWorkloadControlSet.Controls.Add(this.tableLayoutPanel1, 0, 1);
			this.tableLayoutPanelSelectWorkloadControlSet.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelSelectWorkloadControlSet.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanelSelectWorkloadControlSet.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanelSelectWorkloadControlSet.Name = "tableLayoutPanelSelectWorkloadControlSet";
			this.tableLayoutPanelSelectWorkloadControlSet.RowCount = 2;
			this.tableLayoutPanelSelectWorkloadControlSet.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 33F));
			this.tableLayoutPanelSelectWorkloadControlSet.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelSelectWorkloadControlSet.Size = new System.Drawing.Size(835, 150);
			this.tableLayoutPanelSelectWorkloadControlSet.TabIndex = 0;
			// 
			// tableLayoutPanelSubHeader1
			// 
			this.tableLayoutPanelSubHeader1.BackColor = System.Drawing.Color.LightSteelBlue;
			this.tableLayoutPanelSubHeader1.ColumnCount = 3;
			this.tableLayoutPanelSubHeader1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelSubHeader1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelSubHeader1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelSubHeader1.Controls.Add(this.labelSubHeader1, 0, 0);
			this.tableLayoutPanelSubHeader1.Controls.Add(this.buttonDelete, 2, 0);
			this.tableLayoutPanelSubHeader1.Controls.Add(this.buttonNew, 1, 0);
			this.tableLayoutPanelSubHeader1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelSubHeader1.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanelSubHeader1.Name = "tableLayoutPanelSubHeader1";
			this.tableLayoutPanelSubHeader1.RowCount = 1;
			this.tableLayoutPanelSubHeader1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelSubHeader1.Size = new System.Drawing.Size(829, 27);
			this.tableLayoutPanelSubHeader1.TabIndex = 0;
			// 
			// labelSubHeader1
			// 
			this.labelSubHeader1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelSubHeader1.AutoSize = true;
			this.labelSubHeader1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
			this.labelSubHeader1.ForeColor = System.Drawing.Color.GhostWhite;
			this.labelSubHeader1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.labelSubHeader1.Location = new System.Drawing.Point(3, 7);
			this.labelSubHeader1.Name = "labelSubHeader1";
			this.labelSubHeader1.Size = new System.Drawing.Size(210, 13);
			this.labelSubHeader1.TabIndex = 0;
			this.labelSubHeader1.Text = "xxChooseWorkflowControlSetToEdit";
			this.labelSubHeader1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// buttonDelete
			// 
			this.buttonDelete.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.buttonDelete.BeforeTouchSize = new System.Drawing.Size(24, 24);
			this.buttonDelete.Image = global::Teleopti.Ccc.Win.Properties.Resources.test_delete_32x32;
			this.buttonDelete.IsBackStageButton = false;
			this.buttonDelete.Location = new System.Drawing.Point(805, 1);
			this.buttonDelete.Margin = new System.Windows.Forms.Padding(2, 1, 0, 3);
			this.buttonDelete.Name = "buttonDelete";
			this.buttonDelete.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
			this.buttonDelete.Size = new System.Drawing.Size(24, 24);
			this.buttonDelete.TabIndex = 2;
			this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
			// 
			// buttonNew
			// 
			this.buttonNew.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.buttonNew.BackColor = System.Drawing.Color.White;
			this.buttonNew.Font = new System.Drawing.Font("Tahoma", 8F);
			this.buttonNew.Image = global::Teleopti.Ccc.Win.Properties.Resources.test_add2;
			this.buttonNew.Location = new System.Drawing.Point(779, 1);
			this.buttonNew.Margin = new System.Windows.Forms.Padding(2, 1, 0, 3);
			this.buttonNew.Name = "buttonNew";
			this.buttonNew.Size = new System.Drawing.Size(24, 24);
			this.buttonNew.TabIndex = 1;
			this.buttonNew.UseVisualStyleBackColor = false;
			this.buttonNew.Click += new System.EventHandler(this.buttonNew_Click);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 175F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.autoLabelChangeInfo, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.autoLabel5, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.autoLabel1, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.comboBoxAdvWorkflowControlSet, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.textBoxDescription, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.autoLabelInfoAboutChanges, 1, 2);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 36);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(829, 111);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// autoLabelChangeInfo
			// 
			this.autoLabelChangeInfo.AutoSize = false;
			this.autoLabelChangeInfo.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.autoLabelChangeInfo.Location = new System.Drawing.Point(3, 60);
			this.autoLabelChangeInfo.Name = "autoLabelChangeInfo";
			this.autoLabelChangeInfo.Size = new System.Drawing.Size(167, 32);
			this.autoLabelChangeInfo.TabIndex = 0;
			this.autoLabelChangeInfo.Text = "xxChangeInfoColon";
			this.autoLabelChangeInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// autoLabel5
			// 
			this.autoLabel5.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.autoLabel5.AutoSize = false;
			this.autoLabel5.Font = new System.Drawing.Font("Tahoma", 8F);
			this.autoLabel5.Location = new System.Drawing.Point(3, 4);
			this.autoLabel5.Name = "autoLabel5";
			this.autoLabel5.Size = new System.Drawing.Size(169, 21);
			this.autoLabel5.TabIndex = 0;
			this.autoLabel5.Text = "xxWorkflowControlSetColon";
			this.autoLabel5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// autoLabel1
			// 
			this.autoLabel1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.autoLabel1.AutoSize = false;
			this.autoLabel1.Font = new System.Drawing.Font("Tahoma", 8F);
			this.autoLabel1.Location = new System.Drawing.Point(3, 34);
			this.autoLabel1.Name = "autoLabel1";
			this.autoLabel1.Size = new System.Drawing.Size(169, 21);
			this.autoLabel1.TabIndex = 0;
			this.autoLabel1.Text = "xxDescriptionColon";
			this.autoLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// comboBoxAdvWorkflowControlSet
			// 
			this.comboBoxAdvWorkflowControlSet.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.comboBoxAdvWorkflowControlSet.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(251)))));
			this.comboBoxAdvWorkflowControlSet.BeforeTouchSize = new System.Drawing.Size(216, 21);
			this.comboBoxAdvWorkflowControlSet.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAdvWorkflowControlSet.Location = new System.Drawing.Point(178, 4);
			this.comboBoxAdvWorkflowControlSet.Name = "comboBoxAdvWorkflowControlSet";
			this.comboBoxAdvWorkflowControlSet.Size = new System.Drawing.Size(216, 21);
			this.comboBoxAdvWorkflowControlSet.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
			this.comboBoxAdvWorkflowControlSet.TabIndex = 3;
			this.comboBoxAdvWorkflowControlSet.SelectedIndexChanged += new System.EventHandler(this.comboBoxAdvWorkflowControlSet_SelectedIndexChanged);
			// 
			// textBoxDescription
			// 
			this.textBoxDescription.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.textBoxDescription.BeforeTouchSize = new System.Drawing.Size(100, 20);
			this.textBoxDescription.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.textBoxDescription.Location = new System.Drawing.Point(178, 35);
			this.textBoxDescription.MaxLength = 50;
			this.textBoxDescription.Metrocolor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxDescription.Name = "textBoxDescription";
			this.textBoxDescription.OverflowIndicatorToolTipText = null;
			this.textBoxDescription.Size = new System.Drawing.Size(216, 20);
			this.textBoxDescription.Style = Syncfusion.Windows.Forms.Tools.TextBoxExt.theme.Default;
			this.textBoxDescription.TabIndex = 4;
			this.textBoxDescription.WordWrap = false;
			this.textBoxDescription.Leave += new System.EventHandler(this.textBoxDescription_Leave);
			// 
			// autoLabelInfoAboutChanges
			// 
			this.autoLabelInfoAboutChanges.AutoSize = false;
			this.autoLabelInfoAboutChanges.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.autoLabelInfoAboutChanges.Location = new System.Drawing.Point(175, 60);
			this.autoLabelInfoAboutChanges.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
			this.autoLabelInfoAboutChanges.Name = "autoLabelInfoAboutChanges";
			this.autoLabelInfoAboutChanges.Size = new System.Drawing.Size(645, 32);
			this.autoLabelInfoAboutChanges.TabIndex = 0;
			this.autoLabelInfoAboutChanges.Text = "xxInfoAboutChanges";
			this.autoLabelInfoAboutChanges.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// tabControlAdvArea
			// 
			this.tabControlAdvArea.ActiveTabFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.tabControlAdvArea.BeforeTouchSize = new System.Drawing.Size(829, 375);
			this.tabControlAdvArea.Controls.Add(this.tabPageBasic);
			this.tabControlAdvArea.Controls.Add(this.tabPageAdvAbsenceRequests);
			this.tabControlAdvArea.Controls.Add(this.tabPageAdvShiftTradeRequest);
			this.tabControlAdvArea.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControlAdvArea.Location = new System.Drawing.Point(3, 153);
			this.tabControlAdvArea.Name = "tabControlAdvArea";
			this.tabControlAdvArea.Size = new System.Drawing.Size(829, 375);
			this.tabControlAdvArea.TabGap = 10;
			this.tabControlAdvArea.TabIndex = 5;
			this.tabControlAdvArea.TabPanelBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(199)))), ((int)(((byte)(216)))), ((int)(((byte)(237)))));
			this.tabControlAdvArea.TabStyle = typeof(Syncfusion.Windows.Forms.Tools.TabRendererOffice2007);
			// 
			// tabPageBasic
			// 
			this.tabPageBasic.Controls.Add(this.tableLayoutPanelBasic);
			this.tabPageBasic.Image = null;
			this.tabPageBasic.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageBasic.Location = new System.Drawing.Point(1, 22);
			this.tabPageBasic.Name = "tabPageBasic";
			this.tabPageBasic.ShowCloseButton = true;
			this.tabPageBasic.Size = new System.Drawing.Size(826, 351);
			this.tabPageBasic.TabIndex = 2;
			this.tabPageBasic.Text = "xxBasic";
			this.tabPageBasic.ThemesEnabled = false;
			// 
			// tableLayoutPanelBasic
			// 
			this.tableLayoutPanelBasic.AutoScroll = true;
			this.tableLayoutPanelBasic.ColumnCount = 2;
			this.tableLayoutPanelBasic.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 43.37349F));
			this.tableLayoutPanelBasic.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 56.62651F));
			this.tableLayoutPanelBasic.Controls.Add(this.twoListSelectorAbsences, 0, 13);
			this.tableLayoutPanelBasic.Controls.Add(this.panel8, 0, 12);
			this.tableLayoutPanelBasic.Controls.Add(this.tableLayoutPanelStudentAvailability, 0, 5);
			this.tableLayoutPanelBasic.Controls.Add(this.twoListSelectorCategories, 0, 11);
			this.tableLayoutPanelBasic.Controls.Add(this.panelVisualizationButtons, 0, 1);
			this.tableLayoutPanelBasic.Controls.Add(this.twoListSelectorDayOffs, 0, 9);
			this.tableLayoutPanelBasic.Controls.Add(this.dateOnlyPeriodsVisualizer1, 0, 0);
			this.tableLayoutPanelBasic.Controls.Add(this.panel4, 0, 10);
			this.tableLayoutPanelBasic.Controls.Add(this.panel3, 0, 8);
			this.tableLayoutPanelBasic.Controls.Add(this.panel2, 0, 14);
			this.tableLayoutPanelBasic.Controls.Add(this.comboBoxAdvAllowedPreferenceActivity, 0, 15);
			this.tableLayoutPanelBasic.Controls.Add(this.panelBasic, 0, 2);
			this.tableLayoutPanelBasic.Controls.Add(this.tableLayoutPanelBasicSchedule, 0, 3);
			this.tableLayoutPanelBasic.Controls.Add(this.tableLayoutPanelOpenPreference, 0, 7);
			this.tableLayoutPanelBasic.Controls.Add(this.panelOpenPreference, 0, 6);
			this.tableLayoutPanelBasic.Controls.Add(this.panel6, 0, 16);
			this.tableLayoutPanelBasic.Controls.Add(this.radioButtonAdvFairnessPoints, 0, 17);
			this.tableLayoutPanelBasic.Controls.Add(this.radioButtonAdvFairnessEqual, 0, 18);
			this.tableLayoutPanelBasic.Controls.Add(this.panel7, 0, 4);
			this.tableLayoutPanelBasic.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelBasic.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanelBasic.Name = "tableLayoutPanelBasic";
			this.tableLayoutPanelBasic.RowCount = 20;
			this.tableLayoutPanelBasic.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.tableLayoutPanelBasic.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.tableLayoutPanelBasic.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelBasic.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 66F));
			this.tableLayoutPanelBasic.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanelBasic.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 154F));
			this.tableLayoutPanelBasic.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanelBasic.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 154F));
			this.tableLayoutPanelBasic.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanelBasic.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 160F));
			this.tableLayoutPanelBasic.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanelBasic.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 160F));
			this.tableLayoutPanelBasic.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanelBasic.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 160F));
			this.tableLayoutPanelBasic.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanelBasic.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanelBasic.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanelBasic.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanelBasic.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanelBasic.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelBasic.Size = new System.Drawing.Size(826, 351);
			this.tableLayoutPanelBasic.TabIndex = 0;
			// 
			// twoListSelectorAbsences
			// 
			this.twoListSelectorAbsences.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
			this.tableLayoutPanelBasic.SetColumnSpan(this.twoListSelectorAbsences, 2);
			this.twoListSelectorAbsences.Dock = System.Windows.Forms.DockStyle.Fill;
			this.twoListSelectorAbsences.Location = new System.Drawing.Point(3, 1002);
			this.twoListSelectorAbsences.Name = "twoListSelectorAbsences";
			this.twoListSelectorAbsences.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.twoListSelectorAbsences.Size = new System.Drawing.Size(820, 154);
			this.twoListSelectorAbsences.TabIndex = 22;
			// 
			// panel8
			// 
			this.panel8.BackColor = System.Drawing.Color.LightSteelBlue;
			this.tableLayoutPanelBasic.SetColumnSpan(this.panel8, 2);
			this.panel8.Controls.Add(this.labelAbsencesAvailableForExtendedPreference);
			this.panel8.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel8.Location = new System.Drawing.Point(3, 972);
			this.panel8.Name = "panel8";
			this.panel8.Size = new System.Drawing.Size(820, 24);
			this.panel8.TabIndex = 21;
			// 
			// labelAbsencesAvailableForExtendedPreference
			// 
			this.labelAbsencesAvailableForExtendedPreference.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelAbsencesAvailableForExtendedPreference.AutoSize = true;
			this.labelAbsencesAvailableForExtendedPreference.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
			this.labelAbsencesAvailableForExtendedPreference.ForeColor = System.Drawing.Color.GhostWhite;
			this.labelAbsencesAvailableForExtendedPreference.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.labelAbsencesAvailableForExtendedPreference.Location = new System.Drawing.Point(3, 4);
			this.labelAbsencesAvailableForExtendedPreference.Name = "labelAbsencesAvailableForExtendedPreference";
			this.labelAbsencesAvailableForExtendedPreference.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
			this.labelAbsencesAvailableForExtendedPreference.Size = new System.Drawing.Size(260, 16);
			this.labelAbsencesAvailableForExtendedPreference.TabIndex = 0;
			this.labelAbsencesAvailableForExtendedPreference.Text = "xxAbsencesAvailableForExtendedPreference";
			this.labelAbsencesAvailableForExtendedPreference.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// tableLayoutPanelStudentAvailability
			// 
			this.tableLayoutPanelStudentAvailability.ColumnCount = 2;
			this.tableLayoutPanelBasic.SetColumnSpan(this.tableLayoutPanelStudentAvailability, 2);
			this.tableLayoutPanelStudentAvailability.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 43.21941F));
			this.tableLayoutPanelStudentAvailability.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 56.78059F));
			this.tableLayoutPanelStudentAvailability.Controls.Add(this.labelStudentAvailabilityPeriod, 0, 0);
			this.tableLayoutPanelStudentAvailability.Controls.Add(this.label6, 1, 0);
			this.tableLayoutPanelStudentAvailability.Controls.Add(this.dateSelectionFromToStudentAvailability, 0, 1);
			this.tableLayoutPanelStudentAvailability.Controls.Add(this.dateSelectionFromToIsOpenStudentAvailability, 1, 1);
			this.tableLayoutPanelStudentAvailability.Location = new System.Drawing.Point(3, 254);
			this.tableLayoutPanelStudentAvailability.Name = "tableLayoutPanelStudentAvailability";
			this.tableLayoutPanelStudentAvailability.RowCount = 2;
			this.tableLayoutPanelStudentAvailability.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 18.49315F));
			this.tableLayoutPanelStudentAvailability.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 81.50685F));
			this.tableLayoutPanelStudentAvailability.Size = new System.Drawing.Size(803, 146);
			this.tableLayoutPanelStudentAvailability.TabIndex = 20;
			// 
			// labelStudentAvailabilityPeriod
			// 
			this.labelStudentAvailabilityPeriod.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelStudentAvailabilityPeriod.AutoSize = true;
			this.labelStudentAvailabilityPeriod.Location = new System.Drawing.Point(3, 7);
			this.labelStudentAvailabilityPeriod.Name = "labelStudentAvailabilityPeriod";
			this.labelStudentAvailabilityPeriod.Size = new System.Drawing.Size(133, 13);
			this.labelStudentAvailabilityPeriod.TabIndex = 0;
			this.labelStudentAvailabilityPeriod.Text = "xxStudentAvailabilityPeriod";
			// 
			// label6
			// 
			this.label6.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(350, 7);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(51, 13);
			this.label6.TabIndex = 1;
			this.label6.Text = "xxIsOpen";
			// 
			// dateSelectionFromToStudentAvailability
			// 
			this.dateSelectionFromToStudentAvailability.BackColor = System.Drawing.Color.Transparent;
			this.dateSelectionFromToStudentAvailability.ButtonApplyText = "xxApply";
			this.dateSelectionFromToStudentAvailability.HideNoneButtons = true;
			this.dateSelectionFromToStudentAvailability.LabelDateSelectionText = "xxFrom";
			this.dateSelectionFromToStudentAvailability.LabelDateSelectionToText = "xxTo";
			this.dateSelectionFromToStudentAvailability.Location = new System.Drawing.Point(3, 30);
			this.dateSelectionFromToStudentAvailability.Name = "dateSelectionFromToStudentAvailability";
			this.dateSelectionFromToStudentAvailability.NoneButtonText = "xxNone";
			this.dateSelectionFromToStudentAvailability.NullString = "xxNoDateIsSelected";
			this.dateSelectionFromToStudentAvailability.ShowApplyButton = false;
			this.dateSelectionFromToStudentAvailability.Size = new System.Drawing.Size(160, 113);
			this.dateSelectionFromToStudentAvailability.TabIndex = 7;
			this.dateSelectionFromToStudentAvailability.TodayButtonText = "xxToday";
			this.dateSelectionFromToStudentAvailability.WorkPeriodEnd = ((Teleopti.Interfaces.Domain.DateOnly)(resources.GetObject("dateSelectionFromToStudentAvailability.WorkPeriodEnd")));
			this.dateSelectionFromToStudentAvailability.WorkPeriodStart = ((Teleopti.Interfaces.Domain.DateOnly)(resources.GetObject("dateSelectionFromToStudentAvailability.WorkPeriodStart")));
			this.dateSelectionFromToStudentAvailability.Validating += new System.ComponentModel.CancelEventHandler(this.dateSelectionFromToStudentAvailability_Validating);
			this.dateSelectionFromToStudentAvailability.Validated += new System.EventHandler(this.dateSelectionFromToStudentAvailability_Validated);
			// 
			// dateSelectionFromToIsOpenStudentAvailability
			// 
			this.dateSelectionFromToIsOpenStudentAvailability.BackColor = System.Drawing.Color.Transparent;
			this.dateSelectionFromToIsOpenStudentAvailability.ButtonApplyText = "xxApply";
			this.dateSelectionFromToIsOpenStudentAvailability.HideNoneButtons = true;
			this.dateSelectionFromToIsOpenStudentAvailability.LabelDateSelectionText = "xxFrom";
			this.dateSelectionFromToIsOpenStudentAvailability.LabelDateSelectionToText = "xxTo";
			this.dateSelectionFromToIsOpenStudentAvailability.Location = new System.Drawing.Point(350, 30);
			this.dateSelectionFromToIsOpenStudentAvailability.Name = "dateSelectionFromToIsOpenStudentAvailability";
			this.dateSelectionFromToIsOpenStudentAvailability.NoneButtonText = "xxNone";
			this.dateSelectionFromToIsOpenStudentAvailability.NullString = "xxNoDateIsSelected";
			this.dateSelectionFromToIsOpenStudentAvailability.ShowApplyButton = false;
			this.dateSelectionFromToIsOpenStudentAvailability.Size = new System.Drawing.Size(160, 113);
			this.dateSelectionFromToIsOpenStudentAvailability.TabIndex = 8;
			this.dateSelectionFromToIsOpenStudentAvailability.TodayButtonText = "xxToday";
			this.dateSelectionFromToIsOpenStudentAvailability.WorkPeriodEnd = ((Teleopti.Interfaces.Domain.DateOnly)(resources.GetObject("dateSelectionFromToIsOpenStudentAvailability.WorkPeriodEnd")));
			this.dateSelectionFromToIsOpenStudentAvailability.WorkPeriodStart = ((Teleopti.Interfaces.Domain.DateOnly)(resources.GetObject("dateSelectionFromToIsOpenStudentAvailability.WorkPeriodStart")));
			this.dateSelectionFromToIsOpenStudentAvailability.Validating += new System.ComponentModel.CancelEventHandler(this.dateSelectionFromToIsOpenStudentAvailability_Validating);
			this.dateSelectionFromToIsOpenStudentAvailability.Validated += new System.EventHandler(this.dateSelectionFromToIsOpenStudentAvailability_Validated);
			// 
			// twoListSelectorCategories
			// 
			this.twoListSelectorCategories.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
			this.tableLayoutPanelBasic.SetColumnSpan(this.twoListSelectorCategories, 2);
			this.twoListSelectorCategories.Dock = System.Windows.Forms.DockStyle.Fill;
			this.twoListSelectorCategories.Location = new System.Drawing.Point(3, 812);
			this.twoListSelectorCategories.Name = "twoListSelectorCategories";
			this.twoListSelectorCategories.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.twoListSelectorCategories.Size = new System.Drawing.Size(820, 154);
			this.twoListSelectorCategories.TabIndex = 13;
			// 
			// panelVisualizationButtons
			// 
			this.tableLayoutPanelBasic.SetColumnSpan(this.panelVisualizationButtons, 2);
			this.panelVisualizationButtons.Controls.Add(this.buttonZoomOut);
			this.panelVisualizationButtons.Controls.Add(this.buttonZoomIn);
			this.panelVisualizationButtons.Controls.Add(this.buttonPanLeft);
			this.panelVisualizationButtons.Controls.Add(this.buttonPanRight);
			this.panelVisualizationButtons.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelVisualizationButtons.Location = new System.Drawing.Point(0, 100);
			this.panelVisualizationButtons.Margin = new System.Windows.Forms.Padding(0);
			this.panelVisualizationButtons.Name = "panelVisualizationButtons";
			this.panelVisualizationButtons.Size = new System.Drawing.Size(826, 25);
			this.panelVisualizationButtons.TabIndex = 11;
			// 
			// buttonZoomOut
			// 
			this.buttonZoomOut.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonZoomOut.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.buttonZoomOut.BeforeTouchSize = new System.Drawing.Size(31, 18);
			this.buttonZoomOut.Image = global::Teleopti.Ccc.Win.Properties.Resources.Magifier_zoom_out;
			this.buttonZoomOut.IsBackStageButton = false;
			this.buttonZoomOut.Location = new System.Drawing.Point(656, 3);
			this.buttonZoomOut.Name = "buttonZoomOut";
			this.buttonZoomOut.Size = new System.Drawing.Size(31, 18);
			this.buttonZoomOut.TabIndex = 3;
			this.buttonZoomOut.UseVisualStyle = true;
			this.buttonZoomOut.Click += new System.EventHandler(this.buttonZoomOut_Click);
			// 
			// buttonZoomIn
			// 
			this.buttonZoomIn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonZoomIn.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.buttonZoomIn.BeforeTouchSize = new System.Drawing.Size(31, 18);
			this.buttonZoomIn.Image = global::Teleopti.Ccc.Win.Properties.Resources.Magnifier_zoom_in;
			this.buttonZoomIn.IsBackStageButton = false;
			this.buttonZoomIn.Location = new System.Drawing.Point(693, 3);
			this.buttonZoomIn.Name = "buttonZoomIn";
			this.buttonZoomIn.Size = new System.Drawing.Size(31, 18);
			this.buttonZoomIn.TabIndex = 2;
			this.buttonZoomIn.UseVisualStyle = true;
			this.buttonZoomIn.Click += new System.EventHandler(this.buttonZoomIn_Click);
			// 
			// buttonPanLeft
			// 
			this.buttonPanLeft.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonPanLeft.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.buttonPanLeft.BeforeTouchSize = new System.Drawing.Size(31, 18);
			this.buttonPanLeft.ButtonType = Syncfusion.Windows.Forms.Tools.ButtonTypes.Left;
			this.buttonPanLeft.IsBackStageButton = false;
			this.buttonPanLeft.Location = new System.Drawing.Point(750, 3);
			this.buttonPanLeft.Name = "buttonPanLeft";
			this.buttonPanLeft.Size = new System.Drawing.Size(31, 18);
			this.buttonPanLeft.TabIndex = 1;
			this.buttonPanLeft.UseVisualStyle = true;
			this.buttonPanLeft.Click += new System.EventHandler(this.buttonPanLeft_Click);
			// 
			// buttonPanRight
			// 
			this.buttonPanRight.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonPanRight.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.buttonPanRight.BeforeTouchSize = new System.Drawing.Size(31, 18);
			this.buttonPanRight.ButtonType = Syncfusion.Windows.Forms.Tools.ButtonTypes.Right;
			this.buttonPanRight.IsBackStageButton = false;
			this.buttonPanRight.Location = new System.Drawing.Point(787, 3);
			this.buttonPanRight.Name = "buttonPanRight";
			this.buttonPanRight.Size = new System.Drawing.Size(31, 18);
			this.buttonPanRight.TabIndex = 0;
			this.buttonPanRight.UseVisualStyle = true;
			this.buttonPanRight.Click += new System.EventHandler(this.buttonPanRight_Click);
			// 
			// twoListSelectorDayOffs
			// 
			this.twoListSelectorDayOffs.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
			this.tableLayoutPanelBasic.SetColumnSpan(this.twoListSelectorDayOffs, 2);
			this.twoListSelectorDayOffs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.twoListSelectorDayOffs.Location = new System.Drawing.Point(3, 622);
			this.twoListSelectorDayOffs.Name = "twoListSelectorDayOffs";
			this.twoListSelectorDayOffs.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.twoListSelectorDayOffs.Size = new System.Drawing.Size(820, 154);
			this.twoListSelectorDayOffs.TabIndex = 12;
			// 
			// dateOnlyPeriodsVisualizer1
			// 
			this.dateOnlyPeriodsVisualizer1.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.tableLayoutPanelBasic.SetColumnSpan(this.dateOnlyPeriodsVisualizer1, 2);
			this.dateOnlyPeriodsVisualizer1.ContainedPeriod = ((Teleopti.Interfaces.Domain.DateOnlyPeriod)(resources.GetObject("dateOnlyPeriodsVisualizer1.ContainedPeriod")));
			this.dateOnlyPeriodsVisualizer1.Culture = new System.Globalization.CultureInfo("sv-SE");
			this.dateOnlyPeriodsVisualizer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dateOnlyPeriodsVisualizer1.Location = new System.Drawing.Point(0, 0);
			this.dateOnlyPeriodsVisualizer1.Margin = new System.Windows.Forms.Padding(0);
			this.dateOnlyPeriodsVisualizer1.MonthsOnEachSide = 1;
			this.dateOnlyPeriodsVisualizer1.Name = "dateOnlyPeriodsVisualizer1";
			this.dateOnlyPeriodsVisualizer1.Size = new System.Drawing.Size(826, 100);
			this.dateOnlyPeriodsVisualizer1.TabIndex = 10;
			// 
			// panel4
			// 
			this.panel4.BackColor = System.Drawing.Color.LightSteelBlue;
			this.tableLayoutPanelBasic.SetColumnSpan(this.panel4, 2);
			this.panel4.Controls.Add(this.labelShiftCategoriesAvailableForExtendedPreference);
			this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel4.Location = new System.Drawing.Point(3, 782);
			this.panel4.Name = "panel4";
			this.panel4.Size = new System.Drawing.Size(820, 24);
			this.panel4.TabIndex = 11;
			// 
			// labelShiftCategoriesAvailableForExtendedPreference
			// 
			this.labelShiftCategoriesAvailableForExtendedPreference.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelShiftCategoriesAvailableForExtendedPreference.AutoSize = true;
			this.labelShiftCategoriesAvailableForExtendedPreference.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
			this.labelShiftCategoriesAvailableForExtendedPreference.ForeColor = System.Drawing.Color.GhostWhite;
			this.labelShiftCategoriesAvailableForExtendedPreference.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.labelShiftCategoriesAvailableForExtendedPreference.Location = new System.Drawing.Point(3, 4);
			this.labelShiftCategoriesAvailableForExtendedPreference.Name = "labelShiftCategoriesAvailableForExtendedPreference";
			this.labelShiftCategoriesAvailableForExtendedPreference.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
			this.labelShiftCategoriesAvailableForExtendedPreference.Size = new System.Drawing.Size(293, 16);
			this.labelShiftCategoriesAvailableForExtendedPreference.TabIndex = 0;
			this.labelShiftCategoriesAvailableForExtendedPreference.Text = "xxShiftCategoriesAvailableForExtendedPreference";
			this.labelShiftCategoriesAvailableForExtendedPreference.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// panel3
			// 
			this.panel3.BackColor = System.Drawing.Color.LightSteelBlue;
			this.tableLayoutPanelBasic.SetColumnSpan(this.panel3, 2);
			this.panel3.Controls.Add(this.labelDaysOffAvailableForExtendedPreferences);
			this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel3.Location = new System.Drawing.Point(3, 592);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(820, 24);
			this.panel3.TabIndex = 10;
			// 
			// labelDaysOffAvailableForExtendedPreferences
			// 
			this.labelDaysOffAvailableForExtendedPreferences.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelDaysOffAvailableForExtendedPreferences.AutoSize = true;
			this.labelDaysOffAvailableForExtendedPreferences.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
			this.labelDaysOffAvailableForExtendedPreferences.ForeColor = System.Drawing.Color.GhostWhite;
			this.labelDaysOffAvailableForExtendedPreferences.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.labelDaysOffAvailableForExtendedPreferences.Location = new System.Drawing.Point(3, 4);
			this.labelDaysOffAvailableForExtendedPreferences.Name = "labelDaysOffAvailableForExtendedPreferences";
			this.labelDaysOffAvailableForExtendedPreferences.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
			this.labelDaysOffAvailableForExtendedPreferences.Size = new System.Drawing.Size(256, 16);
			this.labelDaysOffAvailableForExtendedPreferences.TabIndex = 0;
			this.labelDaysOffAvailableForExtendedPreferences.Text = "xxDaysOffAvailableForExtendedPreferences";
			this.labelDaysOffAvailableForExtendedPreferences.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// panel2
			// 
			this.panel2.BackColor = System.Drawing.Color.LightSteelBlue;
			this.tableLayoutPanelBasic.SetColumnSpan(this.panel2, 2);
			this.panel2.Controls.Add(this.labelAllowedPreferenceActivity);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel2.Location = new System.Drawing.Point(3, 1162);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(820, 24);
			this.panel2.TabIndex = 1;
			// 
			// labelAllowedPreferenceActivity
			// 
			this.labelAllowedPreferenceActivity.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelAllowedPreferenceActivity.AutoSize = true;
			this.labelAllowedPreferenceActivity.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
			this.labelAllowedPreferenceActivity.ForeColor = System.Drawing.Color.GhostWhite;
			this.labelAllowedPreferenceActivity.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.labelAllowedPreferenceActivity.Location = new System.Drawing.Point(3, 4);
			this.labelAllowedPreferenceActivity.Name = "labelAllowedPreferenceActivity";
			this.labelAllowedPreferenceActivity.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
			this.labelAllowedPreferenceActivity.Size = new System.Drawing.Size(250, 16);
			this.labelAllowedPreferenceActivity.TabIndex = 0;
			this.labelAllowedPreferenceActivity.Text = "xxActivityAvailableForExtendedPreference";
			this.labelAllowedPreferenceActivity.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// comboBoxAdvAllowedPreferenceActivity
			// 
			this.comboBoxAdvAllowedPreferenceActivity.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(251)))));
			this.comboBoxAdvAllowedPreferenceActivity.BeforeTouchSize = new System.Drawing.Size(171, 19);
			this.comboBoxAdvAllowedPreferenceActivity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAdvAllowedPreferenceActivity.Location = new System.Drawing.Point(3, 1192);
			this.comboBoxAdvAllowedPreferenceActivity.Name = "comboBoxAdvAllowedPreferenceActivity";
			this.comboBoxAdvAllowedPreferenceActivity.Size = new System.Drawing.Size(171, 19);
			this.comboBoxAdvAllowedPreferenceActivity.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
			this.comboBoxAdvAllowedPreferenceActivity.TabIndex = 15;
			this.comboBoxAdvAllowedPreferenceActivity.SelectedIndexChanged += new System.EventHandler(this.comboBoxAdvAllowedPreferenceActivity_SelectedIndexChanged);
			// 
			// panelBasic
			// 
			this.panelBasic.BackColor = System.Drawing.Color.LightSteelBlue;
			this.tableLayoutPanelBasic.SetColumnSpan(this.panelBasic, 2);
			this.panelBasic.Controls.Add(this.labelBasic);
			this.panelBasic.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelBasic.Location = new System.Drawing.Point(3, 128);
			this.panelBasic.Name = "panelBasic";
			this.panelBasic.Size = new System.Drawing.Size(820, 24);
			this.panelBasic.TabIndex = 3;
			// 
			// labelBasic
			// 
			this.labelBasic.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.labelBasic.AutoSize = true;
			this.labelBasic.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
			this.labelBasic.ForeColor = System.Drawing.Color.GhostWhite;
			this.labelBasic.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.labelBasic.Location = new System.Drawing.Point(3, 4);
			this.labelBasic.Name = "labelBasic";
			this.labelBasic.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
			this.labelBasic.Size = new System.Drawing.Size(50, 16);
			this.labelBasic.TabIndex = 0;
			this.labelBasic.Text = "xxBasic";
			this.labelBasic.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// tableLayoutPanelBasicSchedule
			// 
			this.tableLayoutPanelBasicSchedule.ColumnCount = 2;
			this.tableLayoutPanelBasic.SetColumnSpan(this.tableLayoutPanelBasicSchedule, 2);
			this.tableLayoutPanelBasicSchedule.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 43.43991F));
			this.tableLayoutPanelBasicSchedule.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 56.56009F));
			this.tableLayoutPanelBasicSchedule.Controls.Add(this.labelWriteProtect, 0, 0);
			this.tableLayoutPanelBasicSchedule.Controls.Add(this.integerTextBoxWriteProtect, 1, 0);
			this.tableLayoutPanelBasicSchedule.Controls.Add(this.labelPublishSchedules, 0, 1);
			this.tableLayoutPanelBasicSchedule.Controls.Add(this.dateTimePickerAdvPublishedTo, 1, 1);
			this.tableLayoutPanelBasicSchedule.Location = new System.Drawing.Point(3, 158);
			this.tableLayoutPanelBasicSchedule.Name = "tableLayoutPanelBasicSchedule";
			this.tableLayoutPanelBasicSchedule.RowCount = 2;
			this.tableLayoutPanelBasicSchedule.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelBasicSchedule.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelBasicSchedule.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanelBasicSchedule.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanelBasicSchedule.Size = new System.Drawing.Size(803, 60);
			this.tableLayoutPanelBasicSchedule.TabIndex = 4;
			// 
			// labelWriteProtect
			// 
			this.labelWriteProtect.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelWriteProtect.AutoSize = true;
			this.labelWriteProtect.Location = new System.Drawing.Point(3, 6);
			this.labelWriteProtect.Name = "labelWriteProtect";
			this.labelWriteProtect.Size = new System.Drawing.Size(228, 13);
			this.labelWriteProtect.TabIndex = 0;
			this.labelWriteProtect.Text = "xxWriteProtectScheduledOlderThanDaysColon";
			// 
			// integerTextBoxWriteProtect
			// 
			this.integerTextBoxWriteProtect.Location = new System.Drawing.Point(351, 3);
			this.integerTextBoxWriteProtect.MaxLength = 3;
			this.integerTextBoxWriteProtect.Name = "integerTextBoxWriteProtect";
			this.integerTextBoxWriteProtect.Size = new System.Drawing.Size(115, 20);
			this.integerTextBoxWriteProtect.TabIndex = 5;
			this.integerTextBoxWriteProtect.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.integerTextBoxWriteProtect.Leave += new System.EventHandler(this.integerTextBoxWriteProtect_Leave);
			// 
			// labelPublishSchedules
			// 
			this.labelPublishSchedules.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelPublishSchedules.AutoSize = true;
			this.labelPublishSchedules.Location = new System.Drawing.Point(3, 36);
			this.labelPublishSchedules.Name = "labelPublishSchedules";
			this.labelPublishSchedules.Size = new System.Drawing.Size(141, 13);
			this.labelPublishSchedules.TabIndex = 2;
			this.labelPublishSchedules.Text = "xxPublishSchedulesToColon";
			// 
			// dateTimePickerAdvPublishedTo
			// 
			this.dateTimePickerAdvPublishedTo.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
			this.dateTimePickerAdvPublishedTo.BorderColor = System.Drawing.Color.Empty;
			this.dateTimePickerAdvPublishedTo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			// 
			// 
			// 
			this.dateTimePickerAdvPublishedTo.Calendar.AllowMultipleSelection = false;
			this.dateTimePickerAdvPublishedTo.Calendar.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.dateTimePickerAdvPublishedTo.Calendar.Culture = new System.Globalization.CultureInfo("sv-SE");
			this.dateTimePickerAdvPublishedTo.Calendar.DaysFont = new System.Drawing.Font("Verdana", 8F);
			this.dateTimePickerAdvPublishedTo.Calendar.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dateTimePickerAdvPublishedTo.Calendar.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dateTimePickerAdvPublishedTo.Calendar.ForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvPublishedTo.Calendar.GridLines = Syncfusion.Windows.Forms.Grid.GridBorderStyle.None;
			this.dateTimePickerAdvPublishedTo.Calendar.HeaderEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvPublishedTo.Calendar.HeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.dateTimePickerAdvPublishedTo.Calendar.HeaderHeight = 20;
			this.dateTimePickerAdvPublishedTo.Calendar.HeaderStartColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252)))));
			this.dateTimePickerAdvPublishedTo.Calendar.HeadForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvPublishedTo.Calendar.HeadGradient = true;
			this.dateTimePickerAdvPublishedTo.Calendar.Iso8601CalenderFormat = false;
			this.dateTimePickerAdvPublishedTo.Calendar.Location = new System.Drawing.Point(0, 0);
			this.dateTimePickerAdvPublishedTo.Calendar.Name = "monthCalendar";
			this.dateTimePickerAdvPublishedTo.Calendar.ScrollButtonSize = new System.Drawing.Size(24, 24);
			this.dateTimePickerAdvPublishedTo.Calendar.SelectedDates = new System.DateTime[0];
			this.dateTimePickerAdvPublishedTo.Calendar.Size = new System.Drawing.Size(194, 174);
			this.dateTimePickerAdvPublishedTo.Calendar.SizeToFit = true;
			this.dateTimePickerAdvPublishedTo.Calendar.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
			this.dateTimePickerAdvPublishedTo.Calendar.TabIndex = 0;
			this.dateTimePickerAdvPublishedTo.Calendar.ThemedEnabledScrollButtons = false;
			this.dateTimePickerAdvPublishedTo.Calendar.WeekFont = new System.Drawing.Font("Verdana", 8F);
			this.dateTimePickerAdvPublishedTo.Calendar.WeekInterior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.PeachPuff, System.Drawing.Color.AntiqueWhite);
			// 
			// 
			// 
			this.dateTimePickerAdvPublishedTo.Calendar.NoneButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.dateTimePickerAdvPublishedTo.Calendar.NoneButton.BackColor = System.Drawing.SystemColors.Window;
			this.dateTimePickerAdvPublishedTo.Calendar.NoneButton.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.dateTimePickerAdvPublishedTo.Calendar.NoneButton.IsBackStageButton = false;
			this.dateTimePickerAdvPublishedTo.Calendar.NoneButton.Location = new System.Drawing.Point(122, 0);
			this.dateTimePickerAdvPublishedTo.Calendar.NoneButton.Size = new System.Drawing.Size(72, 20);
			this.dateTimePickerAdvPublishedTo.Calendar.NoneButton.Text = "None";
			this.dateTimePickerAdvPublishedTo.Calendar.NoneButton.UseVisualStyle = true;
			// 
			// 
			// 
			this.dateTimePickerAdvPublishedTo.Calendar.TodayButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.dateTimePickerAdvPublishedTo.Calendar.TodayButton.BackColor = System.Drawing.SystemColors.Window;
			this.dateTimePickerAdvPublishedTo.Calendar.TodayButton.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.dateTimePickerAdvPublishedTo.Calendar.TodayButton.IsBackStageButton = false;
			this.dateTimePickerAdvPublishedTo.Calendar.TodayButton.Location = new System.Drawing.Point(0, 0);
			this.dateTimePickerAdvPublishedTo.Calendar.TodayButton.Size = new System.Drawing.Size(122, 20);
			this.dateTimePickerAdvPublishedTo.Calendar.TodayButton.Text = "Today";
			this.dateTimePickerAdvPublishedTo.Calendar.TodayButton.UseVisualStyle = true;
			this.dateTimePickerAdvPublishedTo.CalendarSize = new System.Drawing.Size(189, 176);
			this.dateTimePickerAdvPublishedTo.CalendarTitleBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252)))));
			this.dateTimePickerAdvPublishedTo.CalendarTitleForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvPublishedTo.DropDownImage = null;
			this.dateTimePickerAdvPublishedTo.DropDownNormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(181)))), ((int)(((byte)(203)))), ((int)(((byte)(232)))));
			this.dateTimePickerAdvPublishedTo.Format = System.Windows.Forms.DateTimePickerFormat.Short;
			this.dateTimePickerAdvPublishedTo.Location = new System.Drawing.Point(351, 29);
			this.dateTimePickerAdvPublishedTo.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(165)))), ((int)(((byte)(220)))));
			this.dateTimePickerAdvPublishedTo.MinValue = new System.DateTime(((long)(0)));
			this.dateTimePickerAdvPublishedTo.Name = "dateTimePickerAdvPublishedTo";
			this.dateTimePickerAdvPublishedTo.NullString = "xxNotPublished";
			this.dateTimePickerAdvPublishedTo.ShowCheckBox = false;
			this.dateTimePickerAdvPublishedTo.Size = new System.Drawing.Size(115, 20);
			this.dateTimePickerAdvPublishedTo.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
			this.dateTimePickerAdvPublishedTo.TabIndex = 6;
			this.dateTimePickerAdvPublishedTo.Value = new System.DateTime(2010, 5, 26, 13, 43, 9, 200);
			this.dateTimePickerAdvPublishedTo.ValueChanged += new System.EventHandler(this.dateTimePickerAdvPublishedTo_ValueChanged);
			// 
			// tableLayoutPanelOpenPreference
			// 
			this.tableLayoutPanelOpenPreference.ColumnCount = 3;
			this.tableLayoutPanelBasic.SetColumnSpan(this.tableLayoutPanelOpenPreference, 2);
			this.tableLayoutPanelOpenPreference.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 43.26403F));
			this.tableLayoutPanelOpenPreference.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 28.36799F));
			this.tableLayoutPanelOpenPreference.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 28.36799F));
			this.tableLayoutPanelOpenPreference.Controls.Add(this.textBoxExtMinTimePerWeek, 2, 1);
			this.tableLayoutPanelOpenPreference.Controls.Add(this.labelMinimumTimePerWeek, 2, 0);
			this.tableLayoutPanelOpenPreference.Controls.Add(this.labelPreferencePeriod, 0, 0);
			this.tableLayoutPanelOpenPreference.Controls.Add(this.labelIsOpen, 1, 0);
			this.tableLayoutPanelOpenPreference.Controls.Add(this.dateSelectionFromToPreferencePeriod, 0, 1);
			this.tableLayoutPanelOpenPreference.Controls.Add(this.dateSelectionFromToIsOpen, 1, 1);
			this.tableLayoutPanelOpenPreference.Location = new System.Drawing.Point(3, 438);
			this.tableLayoutPanelOpenPreference.Name = "tableLayoutPanelOpenPreference";
			this.tableLayoutPanelOpenPreference.RowCount = 2;
			this.tableLayoutPanelOpenPreference.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 18.49315F));
			this.tableLayoutPanelOpenPreference.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 81.50685F));
			this.tableLayoutPanelOpenPreference.Size = new System.Drawing.Size(803, 146);
			this.tableLayoutPanelOpenPreference.TabIndex = 6;
			// 
			// labelPreferencePeriod
			// 
			this.labelPreferencePeriod.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelPreferencePeriod.AutoSize = true;
			this.labelPreferencePeriod.Location = new System.Drawing.Point(3, 7);
			this.labelPreferencePeriod.Name = "labelPreferencePeriod";
			this.labelPreferencePeriod.Size = new System.Drawing.Size(99, 13);
			this.labelPreferencePeriod.TabIndex = 0;
			this.labelPreferencePeriod.Text = "xxPreferencePeriod";
			// 
			// labelIsOpen
			// 
			this.labelIsOpen.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelIsOpen.AutoSize = true;
			this.labelIsOpen.Location = new System.Drawing.Point(350, 7);
			this.labelIsOpen.Name = "labelIsOpen";
			this.labelIsOpen.Size = new System.Drawing.Size(51, 13);
			this.labelIsOpen.TabIndex = 1;
			this.labelIsOpen.Text = "xxIsOpen";
			// 
			// dateSelectionFromToPreferencePeriod
			// 
			this.dateSelectionFromToPreferencePeriod.BackColor = System.Drawing.Color.Transparent;
			this.dateSelectionFromToPreferencePeriod.ButtonApplyText = "xxApply";
			this.dateSelectionFromToPreferencePeriod.HideNoneButtons = true;
			this.dateSelectionFromToPreferencePeriod.LabelDateSelectionText = "xxFrom";
			this.dateSelectionFromToPreferencePeriod.LabelDateSelectionToText = "xxTo";
			this.dateSelectionFromToPreferencePeriod.Location = new System.Drawing.Point(3, 30);
			this.dateSelectionFromToPreferencePeriod.Name = "dateSelectionFromToPreferencePeriod";
			this.dateSelectionFromToPreferencePeriod.NoneButtonText = "xxNone";
			this.dateSelectionFromToPreferencePeriod.NullString = "xxNoDateIsSelected";
			this.dateSelectionFromToPreferencePeriod.ShowApplyButton = false;
			this.dateSelectionFromToPreferencePeriod.Size = new System.Drawing.Size(160, 113);
			this.dateSelectionFromToPreferencePeriod.TabIndex = 7;
			this.dateSelectionFromToPreferencePeriod.TodayButtonText = "xxToday";
			this.dateSelectionFromToPreferencePeriod.WorkPeriodEnd = ((Teleopti.Interfaces.Domain.DateOnly)(resources.GetObject("dateSelectionFromToPreferencePeriod.WorkPeriodEnd")));
			this.dateSelectionFromToPreferencePeriod.WorkPeriodStart = ((Teleopti.Interfaces.Domain.DateOnly)(resources.GetObject("dateSelectionFromToPreferencePeriod.WorkPeriodStart")));
			this.dateSelectionFromToPreferencePeriod.Validating += new System.ComponentModel.CancelEventHandler(this.dateSelectionFromToPreferencePeriod_Validating);
			this.dateSelectionFromToPreferencePeriod.Validated += new System.EventHandler(this.dateSelectionFromToPreferencePeriod_Validated);
			// 
			// dateSelectionFromToIsOpen
			// 
			this.dateSelectionFromToIsOpen.BackColor = System.Drawing.Color.Transparent;
			this.dateSelectionFromToIsOpen.ButtonApplyText = "xxApply";
			this.dateSelectionFromToIsOpen.HideNoneButtons = true;
			this.dateSelectionFromToIsOpen.LabelDateSelectionText = "xxFrom";
			this.dateSelectionFromToIsOpen.LabelDateSelectionToText = "xxTo";
			this.dateSelectionFromToIsOpen.Location = new System.Drawing.Point(350, 30);
			this.dateSelectionFromToIsOpen.Name = "dateSelectionFromToIsOpen";
			this.dateSelectionFromToIsOpen.NoneButtonText = "xxNone";
			this.dateSelectionFromToIsOpen.NullString = "xxNoDateIsSelected";
			this.dateSelectionFromToIsOpen.ShowApplyButton = false;
			this.dateSelectionFromToIsOpen.Size = new System.Drawing.Size(160, 113);
			this.dateSelectionFromToIsOpen.TabIndex = 8;
			this.dateSelectionFromToIsOpen.TodayButtonText = "xxToday";
			this.dateSelectionFromToIsOpen.WorkPeriodEnd = ((Teleopti.Interfaces.Domain.DateOnly)(resources.GetObject("dateSelectionFromToIsOpen.WorkPeriodEnd")));
			this.dateSelectionFromToIsOpen.WorkPeriodStart = ((Teleopti.Interfaces.Domain.DateOnly)(resources.GetObject("dateSelectionFromToIsOpen.WorkPeriodStart")));
			this.dateSelectionFromToIsOpen.Validating += new System.ComponentModel.CancelEventHandler(this.dateSelectionFromToIsOpen_Validating);
			this.dateSelectionFromToIsOpen.Validated += new System.EventHandler(this.dateSelectionFromToIsOpen_Validated);
			// 
			// panelOpenPreference
			// 
			this.panelOpenPreference.BackColor = System.Drawing.Color.LightSteelBlue;
			this.tableLayoutPanelBasic.SetColumnSpan(this.panelOpenPreference, 2);
			this.panelOpenPreference.Controls.Add(this.labelOpenPreference);
			this.panelOpenPreference.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelOpenPreference.Location = new System.Drawing.Point(3, 408);
			this.panelOpenPreference.Name = "panelOpenPreference";
			this.panelOpenPreference.Size = new System.Drawing.Size(820, 24);
			this.panelOpenPreference.TabIndex = 5;
			// 
			// labelOpenPreference
			// 
			this.labelOpenPreference.AutoSize = true;
			this.labelOpenPreference.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
			this.labelOpenPreference.ForeColor = System.Drawing.Color.GhostWhite;
			this.labelOpenPreference.Location = new System.Drawing.Point(6, 4);
			this.labelOpenPreference.Name = "labelOpenPreference";
			this.labelOpenPreference.Size = new System.Drawing.Size(118, 13);
			this.labelOpenPreference.TabIndex = 0;
			this.labelOpenPreference.Text = "xxOpenPreferences";
			// 
			// panel6
			// 
			this.panel6.BackColor = System.Drawing.Color.LightSteelBlue;
			this.tableLayoutPanelBasic.SetColumnSpan(this.panel6, 2);
			this.panel6.Controls.Add(this.label4);
			this.panel6.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel6.Location = new System.Drawing.Point(3, 1222);
			this.panel6.Name = "panel6";
			this.panel6.Size = new System.Drawing.Size(820, 24);
			this.panel6.TabIndex = 16;
			// 
			// label4
			// 
			this.label4.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label4.AutoSize = true;
			this.label4.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
			this.label4.ForeColor = System.Drawing.Color.GhostWhite;
			this.label4.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.label4.Location = new System.Drawing.Point(3, 4);
			this.label4.Name = "label4";
			this.label4.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
			this.label4.Size = new System.Drawing.Size(218, 16);
			this.label4.TabIndex = 0;
			this.label4.Text = "xxFairnessSystemUsedForScheduling";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// radioButtonAdvFairnessPoints
			// 
			this.radioButtonAdvFairnessPoints.BeforeTouchSize = new System.Drawing.Size(150, 21);
			this.radioButtonAdvFairnessPoints.Checked = true;
			this.tableLayoutPanelBasic.SetColumnSpan(this.radioButtonAdvFairnessPoints, 2);
			this.radioButtonAdvFairnessPoints.Location = new System.Drawing.Point(3, 1252);
			this.radioButtonAdvFairnessPoints.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.radioButtonAdvFairnessPoints.Name = "radioButtonAdvFairnessPoints";
			this.radioButtonAdvFairnessPoints.Size = new System.Drawing.Size(799, 21);
			this.radioButtonAdvFairnessPoints.TabIndex = 17;
			this.radioButtonAdvFairnessPoints.Text = "xxPointsPerShiftCategory";
			this.radioButtonAdvFairnessPoints.ThemesEnabled = true;
			this.radioButtonAdvFairnessPoints.CheckChanged += new System.EventHandler(this.radioButtonAdvFairnessPointsCheckChanged);
			// 
			// radioButtonAdvFairnessEqual
			// 
			this.radioButtonAdvFairnessEqual.BeforeTouchSize = new System.Drawing.Size(150, 21);
			this.tableLayoutPanelBasic.SetColumnSpan(this.radioButtonAdvFairnessEqual, 2);
			this.radioButtonAdvFairnessEqual.Location = new System.Drawing.Point(3, 1282);
			this.radioButtonAdvFairnessEqual.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.radioButtonAdvFairnessEqual.Name = "radioButtonAdvFairnessEqual";
			this.radioButtonAdvFairnessEqual.Size = new System.Drawing.Size(799, 21);
			this.radioButtonAdvFairnessEqual.TabIndex = 18;
			this.radioButtonAdvFairnessEqual.Text = "xxEqualOfEachShiftCategory";
			this.radioButtonAdvFairnessEqual.ThemesEnabled = true;
			this.radioButtonAdvFairnessEqual.CheckChanged += new System.EventHandler(this.radioButtonAdvFairnessEqualCheckChanged);
			// 
			// panel7
			// 
			this.panel7.BackColor = System.Drawing.Color.LightSteelBlue;
			this.tableLayoutPanelBasic.SetColumnSpan(this.panel7, 2);
			this.panel7.Controls.Add(this.labelOpenStudentAvailability);
			this.panel7.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel7.Location = new System.Drawing.Point(3, 224);
			this.panel7.Name = "panel7";
			this.panel7.Size = new System.Drawing.Size(820, 24);
			this.panel7.TabIndex = 19;
			// 
			// labelOpenStudentAvailability
			// 
			this.labelOpenStudentAvailability.AutoSize = true;
			this.labelOpenStudentAvailability.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
			this.labelOpenStudentAvailability.ForeColor = System.Drawing.Color.GhostWhite;
			this.labelOpenStudentAvailability.Location = new System.Drawing.Point(6, 4);
			this.labelOpenStudentAvailability.Name = "labelOpenStudentAvailability";
			this.labelOpenStudentAvailability.Size = new System.Drawing.Size(158, 13);
			this.labelOpenStudentAvailability.TabIndex = 1;
			this.labelOpenStudentAvailability.Text = "xxOpenStudentAvailability";
			// 
			// tabPageAdvAbsenceRequests
			// 
			this.tabPageAdvAbsenceRequests.Controls.Add(this.tableLayoutPanelAbsenceRequestPeriods);
			this.tabPageAdvAbsenceRequests.Image = null;
			this.tabPageAdvAbsenceRequests.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageAdvAbsenceRequests.Location = new System.Drawing.Point(1, 22);
			this.tabPageAdvAbsenceRequests.Name = "tabPageAdvAbsenceRequests";
			this.tabPageAdvAbsenceRequests.ShowCloseButton = true;
			this.tabPageAdvAbsenceRequests.Size = new System.Drawing.Size(826, 351);
			this.tabPageAdvAbsenceRequests.TabIndex = 1;
			this.tabPageAdvAbsenceRequests.Text = "xxAbsenceRequests";
			this.tabPageAdvAbsenceRequests.ThemesEnabled = false;
			// 
			// tableLayoutPanelAbsenceRequestPeriods
			// 
			this.tableLayoutPanelAbsenceRequestPeriods.ColumnCount = 1;
			this.tableLayoutPanelAbsenceRequestPeriods.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelAbsenceRequestPeriods.Controls.Add(this.tableLayoutPanelOpenForAbsenceRequests, 0, 3);
			this.tableLayoutPanelAbsenceRequestPeriods.Controls.Add(this.gridControlAbsenceRequestOpenPeriods, 0, 4);
			this.tableLayoutPanelAbsenceRequestPeriods.Controls.Add(this.panel1, 0, 0);
			this.tableLayoutPanelAbsenceRequestPeriods.Controls.Add(this.gridControlVisualisation, 0, 1);
			this.tableLayoutPanelAbsenceRequestPeriods.Controls.Add(this.tableLayoutPanelNextPreviousPeriod, 0, 2);
			this.tableLayoutPanelAbsenceRequestPeriods.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelAbsenceRequestPeriods.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanelAbsenceRequestPeriods.Name = "tableLayoutPanelAbsenceRequestPeriods";
			this.tableLayoutPanelAbsenceRequestPeriods.RowCount = 5;
			this.tableLayoutPanelAbsenceRequestPeriods.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
			this.tableLayoutPanelAbsenceRequestPeriods.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
			this.tableLayoutPanelAbsenceRequestPeriods.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
			this.tableLayoutPanelAbsenceRequestPeriods.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanelAbsenceRequestPeriods.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 70F));
			this.tableLayoutPanelAbsenceRequestPeriods.Size = new System.Drawing.Size(826, 351);
			this.tableLayoutPanelAbsenceRequestPeriods.TabIndex = 0;
			// 
			// tableLayoutPanelOpenForAbsenceRequests
			// 
			this.tableLayoutPanelOpenForAbsenceRequests.BackColor = System.Drawing.Color.LightSteelBlue;
			this.tableLayoutPanelOpenForAbsenceRequests.ColumnCount = 3;
			this.tableLayoutPanelOpenForAbsenceRequests.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelOpenForAbsenceRequests.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelOpenForAbsenceRequests.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelOpenForAbsenceRequests.Controls.Add(this.labelOpenForAbsenceRequests, 0, 0);
			this.tableLayoutPanelOpenForAbsenceRequests.Controls.Add(this.buttonDeleteAbsenceRequestPeriod, 2, 0);
			this.tableLayoutPanelOpenForAbsenceRequests.Controls.Add(this.buttonAddAbsenceRequestPeriod, 1, 0);
			this.tableLayoutPanelOpenForAbsenceRequests.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelOpenForAbsenceRequests.Location = new System.Drawing.Point(3, 135);
			this.tableLayoutPanelOpenForAbsenceRequests.Name = "tableLayoutPanelOpenForAbsenceRequests";
			this.tableLayoutPanelOpenForAbsenceRequests.RowCount = 1;
			this.tableLayoutPanelOpenForAbsenceRequests.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelOpenForAbsenceRequests.Size = new System.Drawing.Size(820, 24);
			this.tableLayoutPanelOpenForAbsenceRequests.TabIndex = 0;
			// 
			// labelOpenForAbsenceRequests
			// 
			this.labelOpenForAbsenceRequests.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelOpenForAbsenceRequests.AutoSize = true;
			this.labelOpenForAbsenceRequests.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
			this.labelOpenForAbsenceRequests.ForeColor = System.Drawing.Color.GhostWhite;
			this.labelOpenForAbsenceRequests.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.labelOpenForAbsenceRequests.Location = new System.Drawing.Point(3, 4);
			this.labelOpenForAbsenceRequests.Name = "labelOpenForAbsenceRequests";
			this.labelOpenForAbsenceRequests.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
			this.labelOpenForAbsenceRequests.Size = new System.Drawing.Size(169, 16);
			this.labelOpenForAbsenceRequests.TabIndex = 0;
			this.labelOpenForAbsenceRequests.Text = "xxOpenForAbsenceRequests";
			this.labelOpenForAbsenceRequests.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// buttonDeleteAbsenceRequestPeriod
			// 
			this.buttonDeleteAbsenceRequestPeriod.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.buttonDeleteAbsenceRequestPeriod.BeforeTouchSize = new System.Drawing.Size(24, 24);
			this.buttonDeleteAbsenceRequestPeriod.Image = global::Teleopti.Ccc.Win.Properties.Resources.test_delete_32x32;
			this.buttonDeleteAbsenceRequestPeriod.IsBackStageButton = false;
			this.buttonDeleteAbsenceRequestPeriod.Location = new System.Drawing.Point(796, 0);
			this.buttonDeleteAbsenceRequestPeriod.Margin = new System.Windows.Forms.Padding(0);
			this.buttonDeleteAbsenceRequestPeriod.Name = "buttonDeleteAbsenceRequestPeriod";
			this.buttonDeleteAbsenceRequestPeriod.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
			this.buttonDeleteAbsenceRequestPeriod.Size = new System.Drawing.Size(24, 24);
			this.buttonDeleteAbsenceRequestPeriod.TabIndex = 25;
			this.buttonDeleteAbsenceRequestPeriod.Click += new System.EventHandler(this.buttonAdvDeleteAbsenceRequestPeriod_Click);
			// 
			// buttonAddAbsenceRequestPeriod
			// 
			this.buttonAddAbsenceRequestPeriod.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.buttonAddAbsenceRequestPeriod.BackColor = System.Drawing.Color.White;
			this.buttonAddAbsenceRequestPeriod.Font = new System.Drawing.Font("Tahoma", 8F);
			this.buttonAddAbsenceRequestPeriod.Image = global::Teleopti.Ccc.Win.Properties.Resources.test_add2;
			this.buttonAddAbsenceRequestPeriod.Location = new System.Drawing.Point(772, 0);
			this.buttonAddAbsenceRequestPeriod.Margin = new System.Windows.Forms.Padding(0);
			this.buttonAddAbsenceRequestPeriod.Name = "buttonAddAbsenceRequestPeriod";
			this.buttonAddAbsenceRequestPeriod.Size = new System.Drawing.Size(24, 24);
			this.buttonAddAbsenceRequestPeriod.TabIndex = 24;
			this.buttonAddAbsenceRequestPeriod.UseVisualStyleBackColor = false;
			this.buttonAddAbsenceRequestPeriod.Click += new System.EventHandler(this.buttonAddAbsenceRequestPeriod_Click);
			// 
			// gridControlAbsenceRequestOpenPeriods
			// 
			gridBaseStyle1.Name = "Header";
			gridBaseStyle1.StyleInfo.Borders.Bottom = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
			gridBaseStyle1.StyleInfo.Borders.Left = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
			gridBaseStyle1.StyleInfo.Borders.Right = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
			gridBaseStyle1.StyleInfo.Borders.Top = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
			gridBaseStyle1.StyleInfo.CellType = "Header";
			gridBaseStyle1.StyleInfo.Font.Bold = true;
			gridBaseStyle1.StyleInfo.Interior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(199)))), ((int)(((byte)(184))))), System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(234)))), ((int)(((byte)(216))))));
			gridBaseStyle1.StyleInfo.VerticalAlignment = Syncfusion.Windows.Forms.Grid.GridVerticalAlignment.Middle;
			gridBaseStyle2.Name = "Standard";
			gridBaseStyle2.StyleInfo.Font.Facename = "Tahoma";
			gridBaseStyle2.StyleInfo.Interior = new Syncfusion.Drawing.BrushInfo(System.Drawing.SystemColors.Window);
			gridBaseStyle3.Name = "Column Header";
			gridBaseStyle3.StyleInfo.BaseStyle = "Header";
			gridBaseStyle3.StyleInfo.HorizontalAlignment = Syncfusion.Windows.Forms.Grid.GridHorizontalAlignment.Center;
			gridBaseStyle4.Name = "Row Header";
			gridBaseStyle4.StyleInfo.BaseStyle = "Header";
			gridBaseStyle4.StyleInfo.HorizontalAlignment = Syncfusion.Windows.Forms.Grid.GridHorizontalAlignment.Left;
			gridBaseStyle4.StyleInfo.Interior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Horizontal, System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(199)))), ((int)(((byte)(184))))), System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(234)))), ((int)(((byte)(216))))));
			this.gridControlAbsenceRequestOpenPeriods.BaseStylesMap.AddRange(new Syncfusion.Windows.Forms.Grid.GridBaseStyle[] {
            gridBaseStyle1,
            gridBaseStyle2,
            gridBaseStyle3,
            gridBaseStyle4});
			this.gridControlAbsenceRequestOpenPeriods.ColCount = 11;
			this.gridControlAbsenceRequestOpenPeriods.ColWidthEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridColWidth[] {
            new Syncfusion.Windows.Forms.Grid.GridColWidth(0, 35)});
			this.gridControlAbsenceRequestOpenPeriods.ContextMenuStrip = this.contextMenuStripOpenPeriodsGrid;
			this.gridControlAbsenceRequestOpenPeriods.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridControlAbsenceRequestOpenPeriods.GridOfficeScrollBars = Syncfusion.Windows.Forms.OfficeScrollBars.Office2007;
			this.gridControlAbsenceRequestOpenPeriods.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Office2007Blue;
			this.gridControlAbsenceRequestOpenPeriods.Location = new System.Drawing.Point(3, 165);
			this.gridControlAbsenceRequestOpenPeriods.Name = "gridControlAbsenceRequestOpenPeriods";
			this.gridControlAbsenceRequestOpenPeriods.NumberedColHeaders = false;
			this.gridControlAbsenceRequestOpenPeriods.NumberedRowHeaders = false;
			this.gridControlAbsenceRequestOpenPeriods.Office2007ScrollBars = true;
			this.gridControlAbsenceRequestOpenPeriods.Properties.BackgroundColor = System.Drawing.SystemColors.Window;
			this.gridControlAbsenceRequestOpenPeriods.Properties.ForceImmediateRepaint = false;
			this.gridControlAbsenceRequestOpenPeriods.Properties.MarkColHeader = false;
			this.gridControlAbsenceRequestOpenPeriods.Properties.MarkRowHeader = false;
			this.gridControlAbsenceRequestOpenPeriods.RowCount = 0;
			this.gridControlAbsenceRequestOpenPeriods.RowHeightEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridRowHeight[] {
            new Syncfusion.Windows.Forms.Grid.GridRowHeight(0, 21)});
			this.gridControlAbsenceRequestOpenPeriods.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
			this.gridControlAbsenceRequestOpenPeriods.Size = new System.Drawing.Size(820, 183);
			this.gridControlAbsenceRequestOpenPeriods.SmartSizeBox = false;
			this.gridControlAbsenceRequestOpenPeriods.TabIndex = 26;
			this.gridControlAbsenceRequestOpenPeriods.Text = "gridControl1";
			this.gridControlAbsenceRequestOpenPeriods.ThemesEnabled = true;
			this.gridControlAbsenceRequestOpenPeriods.UseRightToLeftCompatibleTextBox = true;
			this.gridControlAbsenceRequestOpenPeriods.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gridControlAbsenceRequestOpenPeriods_MouseDown);
			// 
			// contextMenuStripOpenPeriodsGrid
			// 
			this.contextMenuStripOpenPeriodsGrid.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemFromToPeriod,
            this.toolStripMenuItemRollingPeriod,
            this.toolStripMenuItemDelete,
            this.toolStripSeparator2,
            this.toolStripMenuItemMoveUp,
            this.toolStripMenuItemMoveDown});
			this.contextMenuStripOpenPeriodsGrid.Name = "contextMenuStripOpenPeriodsGrid";
			this.contextMenuStripOpenPeriodsGrid.ShowImageMargin = false;
			this.contextMenuStripOpenPeriodsGrid.Size = new System.Drawing.Size(158, 120);
			// 
			// toolStripMenuItemFromToPeriod
			// 
			this.toolStripMenuItemFromToPeriod.Name = "toolStripMenuItemFromToPeriod";
			this.toolStripMenuItemFromToPeriod.Size = new System.Drawing.Size(157, 22);
			this.toolStripMenuItemFromToPeriod.Text = "xxAddFromToPeriod";
			this.toolStripMenuItemFromToPeriod.Click += new System.EventHandler(this.toolStripMenuItemFromToPeriod_Click);
			// 
			// toolStripMenuItemRollingPeriod
			// 
			this.toolStripMenuItemRollingPeriod.Name = "toolStripMenuItemRollingPeriod";
			this.toolStripMenuItemRollingPeriod.Size = new System.Drawing.Size(157, 22);
			this.toolStripMenuItemRollingPeriod.Text = "xxAddRollingPeriod";
			this.toolStripMenuItemRollingPeriod.Click += new System.EventHandler(this.toolStripMenuItemRollingPeriod_Click);
			// 
			// toolStripMenuItemDelete
			// 
			this.toolStripMenuItemDelete.Name = "toolStripMenuItemDelete";
			this.toolStripMenuItemDelete.Size = new System.Drawing.Size(157, 22);
			this.toolStripMenuItemDelete.Text = "xxDelete";
			this.toolStripMenuItemDelete.Click += new System.EventHandler(this.toolStripMenuItemDelete_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(154, 6);
			// 
			// toolStripMenuItemMoveUp
			// 
			this.toolStripMenuItemMoveUp.Name = "toolStripMenuItemMoveUp";
			this.toolStripMenuItemMoveUp.Size = new System.Drawing.Size(157, 22);
			this.toolStripMenuItemMoveUp.Text = "xxMoveUp";
			this.toolStripMenuItemMoveUp.Click += new System.EventHandler(this.toolStripMenuItemMoveUp_Click);
			// 
			// toolStripMenuItemMoveDown
			// 
			this.toolStripMenuItemMoveDown.Name = "toolStripMenuItemMoveDown";
			this.toolStripMenuItemMoveDown.Size = new System.Drawing.Size(157, 22);
			this.toolStripMenuItemMoveDown.Text = "xxMoveDown";
			this.toolStripMenuItemMoveDown.Click += new System.EventHandler(this.toolStripMenuItemMoveDown_Click);
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.LightSteelBlue;
			this.panel1.Controls.Add(this.dateTimePickerAdvViewpoint);
			this.panel1.Controls.Add(this.labelAbsenceRequestsVisualisation);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Margin = new System.Windows.Forms.Padding(0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(826, 24);
			this.panel1.TabIndex = 0;
			// 
			// dateTimePickerAdvViewpoint
			// 
			this.dateTimePickerAdvViewpoint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.dateTimePickerAdvViewpoint.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
			this.dateTimePickerAdvViewpoint.BorderColor = System.Drawing.Color.Empty;
			this.dateTimePickerAdvViewpoint.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			// 
			// 
			// 
			this.dateTimePickerAdvViewpoint.Calendar.AllowMultipleSelection = false;
			this.dateTimePickerAdvViewpoint.Calendar.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.dateTimePickerAdvViewpoint.Calendar.Culture = new System.Globalization.CultureInfo("en-US");
			this.dateTimePickerAdvViewpoint.Calendar.DaysFont = new System.Drawing.Font("Verdana", 8F);
			this.dateTimePickerAdvViewpoint.Calendar.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dateTimePickerAdvViewpoint.Calendar.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dateTimePickerAdvViewpoint.Calendar.ForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvViewpoint.Calendar.GridLines = Syncfusion.Windows.Forms.Grid.GridBorderStyle.None;
			this.dateTimePickerAdvViewpoint.Calendar.HeaderEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvViewpoint.Calendar.HeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.dateTimePickerAdvViewpoint.Calendar.HeaderHeight = 20;
			this.dateTimePickerAdvViewpoint.Calendar.HeaderStartColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252)))));
			this.dateTimePickerAdvViewpoint.Calendar.HeadForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvViewpoint.Calendar.HeadGradient = true;
			this.dateTimePickerAdvViewpoint.Calendar.Iso8601CalenderFormat = false;
			this.dateTimePickerAdvViewpoint.Calendar.Location = new System.Drawing.Point(0, 0);
			this.dateTimePickerAdvViewpoint.Calendar.Name = "monthCalendar";
			this.dateTimePickerAdvViewpoint.Calendar.ScrollButtonSize = new System.Drawing.Size(24, 24);
			this.dateTimePickerAdvViewpoint.Calendar.SelectedDates = new System.DateTime[0];
			this.dateTimePickerAdvViewpoint.Calendar.Size = new System.Drawing.Size(194, 174);
			this.dateTimePickerAdvViewpoint.Calendar.SizeToFit = true;
			this.dateTimePickerAdvViewpoint.Calendar.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
			this.dateTimePickerAdvViewpoint.Calendar.TabIndex = 0;
			this.dateTimePickerAdvViewpoint.Calendar.WeekFont = new System.Drawing.Font("Verdana", 8F);
			this.dateTimePickerAdvViewpoint.Calendar.WeekInterior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.PeachPuff, System.Drawing.Color.AntiqueWhite);
			// 
			// 
			// 
			this.dateTimePickerAdvViewpoint.Calendar.NoneButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.dateTimePickerAdvViewpoint.Calendar.NoneButton.BackColor = System.Drawing.SystemColors.Window;
			this.dateTimePickerAdvViewpoint.Calendar.NoneButton.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.dateTimePickerAdvViewpoint.Calendar.NoneButton.IsBackStageButton = false;
			this.dateTimePickerAdvViewpoint.Calendar.NoneButton.Location = new System.Drawing.Point(122, 0);
			this.dateTimePickerAdvViewpoint.Calendar.NoneButton.Size = new System.Drawing.Size(72, 20);
			this.dateTimePickerAdvViewpoint.Calendar.NoneButton.Text = "None";
			this.dateTimePickerAdvViewpoint.Calendar.NoneButton.UseVisualStyle = true;
			this.dateTimePickerAdvViewpoint.Calendar.NoneButton.Visible = false;
			// 
			// 
			// 
			this.dateTimePickerAdvViewpoint.Calendar.TodayButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.dateTimePickerAdvViewpoint.Calendar.TodayButton.BackColor = System.Drawing.SystemColors.Window;
			this.dateTimePickerAdvViewpoint.Calendar.TodayButton.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.dateTimePickerAdvViewpoint.Calendar.TodayButton.IsBackStageButton = false;
			this.dateTimePickerAdvViewpoint.Calendar.TodayButton.Location = new System.Drawing.Point(0, 0);
			this.dateTimePickerAdvViewpoint.Calendar.TodayButton.Size = new System.Drawing.Size(194, 20);
			this.dateTimePickerAdvViewpoint.Calendar.TodayButton.Text = "Today";
			this.dateTimePickerAdvViewpoint.Calendar.TodayButton.UseVisualStyle = true;
			this.dateTimePickerAdvViewpoint.CalendarSize = new System.Drawing.Size(189, 176);
			this.dateTimePickerAdvViewpoint.CalendarTitleBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252)))));
			this.dateTimePickerAdvViewpoint.CalendarTitleForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvViewpoint.DropDownImage = null;
			this.dateTimePickerAdvViewpoint.DropDownNormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(181)))), ((int)(((byte)(203)))), ((int)(((byte)(232)))));
			this.dateTimePickerAdvViewpoint.EnableNullDate = false;
			this.dateTimePickerAdvViewpoint.EnableNullKeys = false;
			this.dateTimePickerAdvViewpoint.Format = System.Windows.Forms.DateTimePickerFormat.Short;
			this.dateTimePickerAdvViewpoint.Location = new System.Drawing.Point(485, 2);
			this.dateTimePickerAdvViewpoint.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(165)))), ((int)(((byte)(220)))));
			this.dateTimePickerAdvViewpoint.MinValue = new System.DateTime(((long)(0)));
			this.dateTimePickerAdvViewpoint.Name = "dateTimePickerAdvViewpoint";
			this.dateTimePickerAdvViewpoint.NoneButtonVisible = false;
			this.dateTimePickerAdvViewpoint.ShowCheckBox = false;
			this.dateTimePickerAdvViewpoint.Size = new System.Drawing.Size(101, 20);
			this.dateTimePickerAdvViewpoint.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
			this.dateTimePickerAdvViewpoint.TabIndex = 20;
			this.dateTimePickerAdvViewpoint.UseCurrentCulture = true;
			this.dateTimePickerAdvViewpoint.Value = new System.DateTime(2010, 4, 26, 14, 17, 36, 416);
			this.dateTimePickerAdvViewpoint.ValueChanged += new System.EventHandler(this.dateTimePickerAdvViewpoint_ValueChanged);
			// 
			// labelAbsenceRequestsVisualisation
			// 
			this.labelAbsenceRequestsVisualisation.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelAbsenceRequestsVisualisation.AutoSize = true;
			this.labelAbsenceRequestsVisualisation.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
			this.labelAbsenceRequestsVisualisation.ForeColor = System.Drawing.Color.GhostWhite;
			this.labelAbsenceRequestsVisualisation.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.labelAbsenceRequestsVisualisation.Location = new System.Drawing.Point(5, 6);
			this.labelAbsenceRequestsVisualisation.Name = "labelAbsenceRequestsVisualisation";
			this.labelAbsenceRequestsVisualisation.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
			this.labelAbsenceRequestsVisualisation.Size = new System.Drawing.Size(92, 16);
			this.labelAbsenceRequestsVisualisation.TabIndex = 0;
			this.labelAbsenceRequestsVisualisation.Text = "xxVisualisation";
			this.labelAbsenceRequestsVisualisation.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// gridControlVisualisation
			// 
			gridBaseStyle5.Name = "Header";
			gridBaseStyle5.StyleInfo.Borders.Bottom = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
			gridBaseStyle5.StyleInfo.Borders.Left = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
			gridBaseStyle5.StyleInfo.Borders.Right = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
			gridBaseStyle5.StyleInfo.Borders.Top = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
			gridBaseStyle5.StyleInfo.CellType = "Header";
			gridBaseStyle5.StyleInfo.Font.Bold = true;
			gridBaseStyle5.StyleInfo.Interior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(199)))), ((int)(((byte)(184))))), System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(234)))), ((int)(((byte)(216))))));
			gridBaseStyle5.StyleInfo.VerticalAlignment = Syncfusion.Windows.Forms.Grid.GridVerticalAlignment.Middle;
			gridBaseStyle6.Name = "Standard";
			gridBaseStyle6.StyleInfo.Font.Facename = "Tahoma";
			gridBaseStyle6.StyleInfo.Interior = new Syncfusion.Drawing.BrushInfo(System.Drawing.SystemColors.Window);
			gridBaseStyle7.Name = "Column Header";
			gridBaseStyle7.StyleInfo.BaseStyle = "Header";
			gridBaseStyle7.StyleInfo.HorizontalAlignment = Syncfusion.Windows.Forms.Grid.GridHorizontalAlignment.Center;
			gridBaseStyle8.Name = "Row Header";
			gridBaseStyle8.StyleInfo.BaseStyle = "Header";
			gridBaseStyle8.StyleInfo.HorizontalAlignment = Syncfusion.Windows.Forms.Grid.GridHorizontalAlignment.Left;
			gridBaseStyle8.StyleInfo.Interior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Horizontal, System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(199)))), ((int)(((byte)(184))))), System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(234)))), ((int)(((byte)(216))))));
			this.gridControlVisualisation.BaseStylesMap.AddRange(new Syncfusion.Windows.Forms.Grid.GridBaseStyle[] {
            gridBaseStyle5,
            gridBaseStyle6,
            gridBaseStyle7,
            gridBaseStyle8});
			this.gridControlVisualisation.ColCount = 2;
			this.gridControlVisualisation.ColWidthEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridColWidth[] {
            new Syncfusion.Windows.Forms.Grid.GridColWidth(0, 35)});
			this.gridControlVisualisation.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridControlVisualisation.GridOfficeScrollBars = Syncfusion.Windows.Forms.OfficeScrollBars.Office2007;
			this.gridControlVisualisation.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Office2007Blue;
			this.gridControlVisualisation.HScrollPixel = true;
			this.gridControlVisualisation.Location = new System.Drawing.Point(3, 27);
			this.gridControlVisualisation.Name = "gridControlVisualisation";
			this.gridControlVisualisation.NumberedColHeaders = false;
			this.gridControlVisualisation.NumberedRowHeaders = false;
			this.gridControlVisualisation.Office2007ScrollBars = true;
			this.gridControlVisualisation.Properties.BackgroundColor = System.Drawing.SystemColors.Window;
			this.gridControlVisualisation.Properties.ForceImmediateRepaint = false;
			this.gridControlVisualisation.Properties.MarkColHeader = false;
			this.gridControlVisualisation.Properties.MarkRowHeader = false;
			this.gridControlVisualisation.RowCount = 0;
			this.gridControlVisualisation.RowHeightEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridRowHeight[] {
            new Syncfusion.Windows.Forms.Grid.GridRowHeight(0, 21)});
			this.gridControlVisualisation.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
			this.gridControlVisualisation.Size = new System.Drawing.Size(820, 74);
			this.gridControlVisualisation.SmartSizeBox = false;
			this.gridControlVisualisation.TabIndex = 21;
			this.gridControlVisualisation.TabStop = false;
			this.gridControlVisualisation.Text = "gridControl1";
			this.gridControlVisualisation.ThemesEnabled = true;
			this.gridControlVisualisation.UseRightToLeftCompatibleTextBox = true;
			this.gridControlVisualisation.VScrollPixel = true;
			// 
			// tableLayoutPanelNextPreviousPeriod
			// 
			this.tableLayoutPanelNextPreviousPeriod.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.tableLayoutPanelNextPreviousPeriod.ColumnCount = 2;
			this.tableLayoutPanelNextPreviousPeriod.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanelNextPreviousPeriod.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanelNextPreviousPeriod.Controls.Add(this.buttonAdvPreviousProjectionPeriod, 0, 0);
			this.tableLayoutPanelNextPreviousPeriod.Controls.Add(this.buttonAdvNextProjectionPeriod, 1, 0);
			this.tableLayoutPanelNextPreviousPeriod.Location = new System.Drawing.Point(626, 104);
			this.tableLayoutPanelNextPreviousPeriod.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanelNextPreviousPeriod.Name = "tableLayoutPanelNextPreviousPeriod";
			this.tableLayoutPanelNextPreviousPeriod.RowCount = 1;
			this.tableLayoutPanelNextPreviousPeriod.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanelNextPreviousPeriod.Size = new System.Drawing.Size(200, 28);
			this.tableLayoutPanelNextPreviousPeriod.TabIndex = 0;
			// 
			// buttonAdvPreviousProjectionPeriod
			// 
			this.buttonAdvPreviousProjectionPeriod.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.buttonAdvPreviousProjectionPeriod.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.buttonAdvPreviousProjectionPeriod.BeforeTouchSize = new System.Drawing.Size(75, 22);
			this.buttonAdvPreviousProjectionPeriod.IsBackStageButton = false;
			this.buttonAdvPreviousProjectionPeriod.Location = new System.Drawing.Point(0, 3);
			this.buttonAdvPreviousProjectionPeriod.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
			this.buttonAdvPreviousProjectionPeriod.Name = "buttonAdvPreviousProjectionPeriod";
			this.buttonAdvPreviousProjectionPeriod.Size = new System.Drawing.Size(75, 22);
			this.buttonAdvPreviousProjectionPeriod.TabIndex = 22;
			this.buttonAdvPreviousProjectionPeriod.Text = "<<";
			this.buttonAdvPreviousProjectionPeriod.UseVisualStyle = true;
			this.buttonAdvPreviousProjectionPeriod.Click += new System.EventHandler(this.buttonAdvPreviousProjectionPeriod_Click);
			// 
			// buttonAdvNextProjectionPeriod
			// 
			this.buttonAdvNextProjectionPeriod.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.buttonAdvNextProjectionPeriod.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.buttonAdvNextProjectionPeriod.BeforeTouchSize = new System.Drawing.Size(75, 22);
			this.buttonAdvNextProjectionPeriod.IsBackStageButton = false;
			this.buttonAdvNextProjectionPeriod.Location = new System.Drawing.Point(122, 3);
			this.buttonAdvNextProjectionPeriod.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
			this.buttonAdvNextProjectionPeriod.Name = "buttonAdvNextProjectionPeriod";
			this.buttonAdvNextProjectionPeriod.Size = new System.Drawing.Size(75, 22);
			this.buttonAdvNextProjectionPeriod.TabIndex = 23;
			this.buttonAdvNextProjectionPeriod.Text = ">>";
			this.buttonAdvNextProjectionPeriod.UseVisualStyle = true;
			this.buttonAdvNextProjectionPeriod.Click += new System.EventHandler(this.buttonAdvNextProjectionPeriod_Click);
			// 
			// tabPageAdvShiftTradeRequest
			// 
			this.tabPageAdvShiftTradeRequest.Controls.Add(this.tableLayoutPanelShiftTrade);
			this.tabPageAdvShiftTradeRequest.Image = null;
			this.tabPageAdvShiftTradeRequest.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageAdvShiftTradeRequest.Location = new System.Drawing.Point(1, 22);
			this.tabPageAdvShiftTradeRequest.Name = "tabPageAdvShiftTradeRequest";
			this.tabPageAdvShiftTradeRequest.ShowCloseButton = true;
			this.tabPageAdvShiftTradeRequest.Size = new System.Drawing.Size(826, 351);
			this.tabPageAdvShiftTradeRequest.TabIndex = 3;
			this.tabPageAdvShiftTradeRequest.Text = "xxShiftTradeRequests";
			this.tabPageAdvShiftTradeRequest.ThemesEnabled = false;
			// 
			// tableLayoutPanelShiftTrade
			// 
			this.tableLayoutPanelShiftTrade.AutoScroll = true;
			this.tableLayoutPanelShiftTrade.ColumnCount = 2;
			this.tableLayoutPanelShiftTrade.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelShiftTrade.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelShiftTrade.Controls.Add(this.panel5, 0, 6);
			this.tableLayoutPanelShiftTrade.Controls.Add(this.panelOpenForShiftTrade, 0, 0);
			this.tableLayoutPanelShiftTrade.Controls.Add(this.panelTolerance, 0, 2);
			this.tableLayoutPanelShiftTrade.Controls.Add(this.minMaxIntegerTextBoxControl1, 0, 1);
			this.tableLayoutPanelShiftTrade.Controls.Add(this.twoListSelectorMatchingSkills, 0, 5);
			this.tableLayoutPanelShiftTrade.Controls.Add(this.panelMatchingSkills, 0, 4);
			this.tableLayoutPanelShiftTrade.Controls.Add(this.tableLayoutPanel2, 0, 3);
			this.tableLayoutPanelShiftTrade.Controls.Add(this.checkBoxAdvAutoGrant, 0, 7);
			this.tableLayoutPanelShiftTrade.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelShiftTrade.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanelShiftTrade.Name = "tableLayoutPanelShiftTrade";
			this.tableLayoutPanelShiftTrade.RowCount = 8;
			this.tableLayoutPanelShiftTrade.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanelShiftTrade.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 33F));
			this.tableLayoutPanelShiftTrade.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanelShiftTrade.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
			this.tableLayoutPanelShiftTrade.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanelShiftTrade.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 160F));
			this.tableLayoutPanelShiftTrade.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanelShiftTrade.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanelShiftTrade.Size = new System.Drawing.Size(826, 351);
			this.tableLayoutPanelShiftTrade.TabIndex = 0;
			// 
			// panel5
			// 
			this.panel5.BackColor = System.Drawing.Color.LightSteelBlue;
			this.tableLayoutPanelShiftTrade.SetColumnSpan(this.panel5, 2);
			this.panel5.Controls.Add(this.label3);
			this.panel5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel5.Location = new System.Drawing.Point(3, 322);
			this.panel5.Name = "panel5";
			this.panel5.Size = new System.Drawing.Size(820, 24);
			this.panel5.TabIndex = 9;
			// 
			// label3
			// 
			this.label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
			this.label3.ForeColor = System.Drawing.Color.GhostWhite;
			this.label3.Location = new System.Drawing.Point(3, 4);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(80, 13);
			this.label3.TabIndex = 0;
			this.label3.Text = "xxAutoGrant";
			// 
			// panelOpenForShiftTrade
			// 
			this.panelOpenForShiftTrade.BackColor = System.Drawing.Color.LightSteelBlue;
			this.tableLayoutPanelShiftTrade.SetColumnSpan(this.panelOpenForShiftTrade, 2);
			this.panelOpenForShiftTrade.Controls.Add(this.labelOpenForShiftTrade);
			this.panelOpenForShiftTrade.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelOpenForShiftTrade.Location = new System.Drawing.Point(3, 3);
			this.panelOpenForShiftTrade.Name = "panelOpenForShiftTrade";
			this.panelOpenForShiftTrade.Size = new System.Drawing.Size(820, 24);
			this.panelOpenForShiftTrade.TabIndex = 0;
			// 
			// labelOpenForShiftTrade
			// 
			this.labelOpenForShiftTrade.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelOpenForShiftTrade.AutoSize = true;
			this.labelOpenForShiftTrade.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
			this.labelOpenForShiftTrade.ForeColor = System.Drawing.Color.GhostWhite;
			this.labelOpenForShiftTrade.Location = new System.Drawing.Point(3, 4);
			this.labelOpenForShiftTrade.Name = "labelOpenForShiftTrade";
			this.labelOpenForShiftTrade.Size = new System.Drawing.Size(180, 13);
			this.labelOpenForShiftTrade.TabIndex = 0;
			this.labelOpenForShiftTrade.Text = "xxOpenForShiftTradeRequests";
			// 
			// panelTolerance
			// 
			this.panelTolerance.BackColor = System.Drawing.Color.LightSteelBlue;
			this.tableLayoutPanelShiftTrade.SetColumnSpan(this.panelTolerance, 2);
			this.panelTolerance.Controls.Add(this.labelTolerance);
			this.panelTolerance.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelTolerance.Location = new System.Drawing.Point(3, 66);
			this.panelTolerance.Name = "panelTolerance";
			this.panelTolerance.Size = new System.Drawing.Size(820, 24);
			this.panelTolerance.TabIndex = 2;
			// 
			// labelTolerance
			// 
			this.labelTolerance.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelTolerance.AutoSize = true;
			this.labelTolerance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
			this.labelTolerance.ForeColor = System.Drawing.Color.GhostWhite;
			this.labelTolerance.Location = new System.Drawing.Point(3, 4);
			this.labelTolerance.Name = "labelTolerance";
			this.labelTolerance.Size = new System.Drawing.Size(224, 13);
			this.labelTolerance.TabIndex = 0;
			this.labelTolerance.Text = "xxToleranceForMatchingContractTime";
			// 
			// minMaxIntegerTextBoxControl1
			// 
			this.tableLayoutPanelShiftTrade.SetColumnSpan(this.minMaxIntegerTextBoxControl1, 2);
			this.minMaxIntegerTextBoxControl1.LabelFromText = "xxFrom";
			this.minMaxIntegerTextBoxControl1.LabelMaxDaysText = "xxDays";
			this.minMaxIntegerTextBoxControl1.LabelMinDaysText = "xxDays";
			this.minMaxIntegerTextBoxControl1.LabelToText = "xxTo";
			this.minMaxIntegerTextBoxControl1.Location = new System.Drawing.Point(3, 33);
			this.minMaxIntegerTextBoxControl1.MaxTextBoxValue = 1;
			this.minMaxIntegerTextBoxControl1.MinTextBoxValue = 1;
			this.minMaxIntegerTextBoxControl1.Name = "minMaxIntegerTextBoxControl1";
			this.minMaxIntegerTextBoxControl1.Size = new System.Drawing.Size(480, 27);
			this.minMaxIntegerTextBoxControl1.TabIndex = 5;
			this.minMaxIntegerTextBoxControl1.Validating += new System.ComponentModel.CancelEventHandler(this.minMaxIntegerTextBoxControl1_Validating);
			this.minMaxIntegerTextBoxControl1.Validated += new System.EventHandler(this.minMaxIntegerTextBoxControl1_Validated);
			// 
			// twoListSelectorMatchingSkills
			// 
			this.twoListSelectorMatchingSkills.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
			this.tableLayoutPanelShiftTrade.SetColumnSpan(this.twoListSelectorMatchingSkills, 2);
			this.twoListSelectorMatchingSkills.Dock = System.Windows.Forms.DockStyle.Fill;
			this.twoListSelectorMatchingSkills.Location = new System.Drawing.Point(3, 162);
			this.twoListSelectorMatchingSkills.Name = "twoListSelectorMatchingSkills";
			this.twoListSelectorMatchingSkills.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.twoListSelectorMatchingSkills.Size = new System.Drawing.Size(820, 154);
			this.twoListSelectorMatchingSkills.TabIndex = 11;
			// 
			// panelMatchingSkills
			// 
			this.panelMatchingSkills.BackColor = System.Drawing.Color.LightSteelBlue;
			this.tableLayoutPanelShiftTrade.SetColumnSpan(this.panelMatchingSkills, 2);
			this.panelMatchingSkills.Controls.Add(this.labelMatchingSkills);
			this.panelMatchingSkills.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelMatchingSkills.Location = new System.Drawing.Point(3, 132);
			this.panelMatchingSkills.Name = "panelMatchingSkills";
			this.panelMatchingSkills.Size = new System.Drawing.Size(820, 24);
			this.panelMatchingSkills.TabIndex = 6;
			// 
			// labelMatchingSkills
			// 
			this.labelMatchingSkills.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelMatchingSkills.AutoSize = true;
			this.labelMatchingSkills.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
			this.labelMatchingSkills.ForeColor = System.Drawing.Color.GhostWhite;
			this.labelMatchingSkills.Location = new System.Drawing.Point(3, 4);
			this.labelMatchingSkills.Name = "labelMatchingSkills";
			this.labelMatchingSkills.Size = new System.Drawing.Size(102, 13);
			this.labelMatchingSkills.TabIndex = 0;
			this.labelMatchingSkills.Text = "xxMatchingSkills";
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 3;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 74F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 165F));
			this.tableLayoutPanel2.Controls.Add(this.timeSpanTextBox1, 1, 0);
			this.tableLayoutPanel2.Controls.Add(this.labelTolerancePosNeg, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.labelHMm, 2, 0);
			this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 96);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 1;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(309, 30);
			this.tableLayoutPanel2.TabIndex = 8;
			// 
			// timeSpanTextBox1
			// 
			this.timeSpanTextBox1.AlignTextBoxText = System.Windows.Forms.HorizontalAlignment.Right;
			this.timeSpanTextBox1.AllowNegativeValues = false;
			this.timeSpanTextBox1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.timeSpanTextBox1.DefaultInterpretAsMinutes = false;
			this.timeSpanTextBox1.Location = new System.Drawing.Point(74, 3);
			this.timeSpanTextBox1.Margin = new System.Windows.Forms.Padding(0);
			this.timeSpanTextBox1.MaximumValue = System.TimeSpan.Parse("4.03:00:00");
			this.timeSpanTextBox1.Name = "timeSpanTextBox1";
			this.timeSpanTextBox1.Size = new System.Drawing.Size(70, 24);
			this.timeSpanTextBox1.TabIndex = 9;
			this.timeSpanTextBox1.TimeSpanBoxHeight = 20;
			this.timeSpanTextBox1.TimeSpanBoxWidth = 64;
			this.timeSpanTextBox1.Leave += new System.EventHandler(this.timeSpanTextBox1_Leave);
			// 
			// labelTolerancePosNeg
			// 
			this.labelTolerancePosNeg.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelTolerancePosNeg.AutoSize = true;
			this.labelTolerancePosNeg.Location = new System.Drawing.Point(3, 8);
			this.labelTolerancePosNeg.Name = "labelTolerancePosNeg";
			this.labelTolerancePosNeg.Size = new System.Drawing.Size(55, 13);
			this.labelTolerancePosNeg.TabIndex = 8;
			this.labelTolerancePosNeg.Text = "xxPosNeg";
			// 
			// labelHMm
			// 
			this.labelHMm.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelHMm.AutoSize = true;
			this.labelHMm.Location = new System.Drawing.Point(144, 8);
			this.labelHMm.Margin = new System.Windows.Forms.Padding(0);
			this.labelHMm.Name = "labelHMm";
			this.labelHMm.Size = new System.Drawing.Size(70, 13);
			this.labelHMm.TabIndex = 9;
			this.labelHMm.Text = "xxHColonMM";
			// 
			// checkBoxAdvAutoGrant
			// 
			this.checkBoxAdvAutoGrant.BeforeTouchSize = new System.Drawing.Size(150, 21);
			this.checkBoxAdvAutoGrant.Location = new System.Drawing.Point(3, 352);
			this.checkBoxAdvAutoGrant.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxAdvAutoGrant.Name = "checkBoxAdvAutoGrant";
			this.checkBoxAdvAutoGrant.Size = new System.Drawing.Size(299, 21);
			this.checkBoxAdvAutoGrant.TabIndex = 14;
			this.checkBoxAdvAutoGrant.Text = "xxCheckToEnableAutoGrant";
			this.checkBoxAdvAutoGrant.ThemesEnabled = false;
			this.checkBoxAdvAutoGrant.CheckStateChanged += new System.EventHandler(this.checkBoxAdvAutoGrant_CheckStateChanged);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(154, 6);
			// 
			// errorProvider1
			// 
			this.errorProvider1.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
			this.errorProvider1.ContainerControl = this;
			// 
			// labelMinimumTimePerWeek
			// 
			this.labelMinimumTimePerWeek.AutoSize = false;
			this.labelMinimumTimePerWeek.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelMinimumTimePerWeek.Font = new System.Drawing.Font("Tahoma", 8F);
			this.labelMinimumTimePerWeek.Location = new System.Drawing.Point(577, 0);
			this.labelMinimumTimePerWeek.Name = "labelMinimumTimePerWeek";
			this.labelMinimumTimePerWeek.Size = new System.Drawing.Size(223, 27);
			this.labelMinimumTimePerWeek.TabIndex = 10;
			this.labelMinimumTimePerWeek.Text = "xxMinimumTimePerWeekColon";
			this.labelMinimumTimePerWeek.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textBoxExtMinTimePerWeek
			// 
			this.textBoxExtMinTimePerWeek.AlignTextBoxText = System.Windows.Forms.HorizontalAlignment.Left;
			this.textBoxExtMinTimePerWeek.AllowNegativeValues = true;
			this.textBoxExtMinTimePerWeek.DefaultInterpretAsMinutes = false;
			this.textBoxExtMinTimePerWeek.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.textBoxExtMinTimePerWeek.Location = new System.Drawing.Point(580, 30);
			this.textBoxExtMinTimePerWeek.Margin = new System.Windows.Forms.Padding(6, 3, 0, 0);
			this.textBoxExtMinTimePerWeek.MaximumValue = System.TimeSpan.Parse("3.12:00:00");
			this.textBoxExtMinTimePerWeek.Name = "textBoxExtMinTimePerWeek";
			this.textBoxExtMinTimePerWeek.Size = new System.Drawing.Size(74, 22);
			this.textBoxExtMinTimePerWeek.TabIndex = 11;
			this.textBoxExtMinTimePerWeek.TimeSpanBoxHeight = 20;
			this.textBoxExtMinTimePerWeek.TimeSpanBoxWidth = 33;
			this.textBoxExtMinTimePerWeek.Validating += textBoxExtMinTimePerWeek_Validating;
			this.textBoxExtMinTimePerWeek.Validated += textBoxExtMinTimePerWeek_Validated;
			// 
			// WorkflowControlSetView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoScroll = true;
			this.BackColor = System.Drawing.SystemColors.Window;
			this.Controls.Add(this.tableLayoutPanelMain);
			this.Controls.Add(this.gradientPanelHeader);
			this.Name = "WorkflowControlSetView";
			this.Size = new System.Drawing.Size(838, 591);
			((System.ComponentModel.ISupportInitialize)(this.gradientPanelHeader)).EndInit();
			this.gradientPanelHeader.ResumeLayout(false);
			this.tableLayoutPanelHeader.ResumeLayout(false);
			this.tableLayoutPanelHeader.PerformLayout();
			this.tableLayoutPanelMain.ResumeLayout(false);
			this.tableLayoutPanelSelectWorkloadControlSet.ResumeLayout(false);
			this.tableLayoutPanelSubHeader1.ResumeLayout(false);
			this.tableLayoutPanelSubHeader1.PerformLayout();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvWorkflowControlSet)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.textBoxDescription)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.tabControlAdvArea)).EndInit();
			this.tabControlAdvArea.ResumeLayout(false);
			this.tabPageBasic.ResumeLayout(false);
			this.tableLayoutPanelBasic.ResumeLayout(false);
			this.panel8.ResumeLayout(false);
			this.panel8.PerformLayout();
			this.tableLayoutPanelStudentAvailability.ResumeLayout(false);
			this.tableLayoutPanelStudentAvailability.PerformLayout();
			this.panelVisualizationButtons.ResumeLayout(false);
			this.panel4.ResumeLayout(false);
			this.panel4.PerformLayout();
			this.panel3.ResumeLayout(false);
			this.panel3.PerformLayout();
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvAllowedPreferenceActivity)).EndInit();
			this.panelBasic.ResumeLayout(false);
			this.panelBasic.PerformLayout();
			this.tableLayoutPanelBasicSchedule.ResumeLayout(false);
			this.tableLayoutPanelBasicSchedule.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvPublishedTo.Calendar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvPublishedTo)).EndInit();
			this.tableLayoutPanelOpenPreference.ResumeLayout(false);
			this.tableLayoutPanelOpenPreference.PerformLayout();
			this.panelOpenPreference.ResumeLayout(false);
			this.panelOpenPreference.PerformLayout();
			this.panel6.ResumeLayout(false);
			this.panel6.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonAdvFairnessPoints)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonAdvFairnessEqual)).EndInit();
			this.panel7.ResumeLayout(false);
			this.panel7.PerformLayout();
			this.tabPageAdvAbsenceRequests.ResumeLayout(false);
			this.tableLayoutPanelAbsenceRequestPeriods.ResumeLayout(false);
			this.tableLayoutPanelOpenForAbsenceRequests.ResumeLayout(false);
			this.tableLayoutPanelOpenForAbsenceRequests.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridControlAbsenceRequestOpenPeriods)).EndInit();
			this.contextMenuStripOpenPeriodsGrid.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvViewpoint.Calendar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvViewpoint)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.gridControlVisualisation)).EndInit();
			this.tableLayoutPanelNextPreviousPeriod.ResumeLayout(false);
			this.tabPageAdvShiftTradeRequest.ResumeLayout(false);
			this.tableLayoutPanelShiftTrade.ResumeLayout(false);
			this.panel5.ResumeLayout(false);
			this.panel5.PerformLayout();
			this.panelOpenForShiftTrade.ResumeLayout(false);
			this.panelOpenForShiftTrade.PerformLayout();
			this.panelTolerance.ResumeLayout(false);
			this.panelTolerance.PerformLayout();
			this.panelMatchingSkills.ResumeLayout(false);
			this.panelMatchingSkills.PerformLayout();
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvAutoGrant)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanelHeader;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelHeader;
        private System.Windows.Forms.Label labelHeader;
        private TableLayoutPanel tableLayoutPanelMain;
        private TableLayoutPanel tableLayoutPanelSelectWorkloadControlSet;
        private TableLayoutPanel tableLayoutPanelSubHeader1;
        private Label labelSubHeader1;
        private Syncfusion.Windows.Forms.ButtonAdv buttonDelete;
        private Button buttonNew;
        private TableLayoutPanel tableLayoutPanel1;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel5;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel1;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelChangeInfo;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvWorkflowControlSet;
        private Syncfusion.Windows.Forms.Tools.TextBoxExt textBoxDescription;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelInfoAboutChanges;
        private ToolTip toolTip1;
        private Syncfusion.Windows.Forms.Tools.TabControlAdv tabControlAdvArea;
        private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageAdvAbsenceRequests;
        private TableLayoutPanel tableLayoutPanelAbsenceRequestPeriods;
        private Panel panel1;
        private Label labelAbsenceRequestsVisualisation;
        private Syncfusion.Windows.Forms.Tools.DateTimePickerAdv dateTimePickerAdvViewpoint;
        private Syncfusion.Windows.Forms.Grid.GridControl gridControlVisualisation;
        private Syncfusion.Windows.Forms.Grid.GridControl gridControlAbsenceRequestOpenPeriods;
        private TableLayoutPanel tableLayoutPanelOpenForAbsenceRequests;
        private Label labelOpenForAbsenceRequests;
        private Syncfusion.Windows.Forms.ButtonAdv buttonDeleteAbsenceRequestPeriod;
        private Button buttonAddAbsenceRequestPeriod;
        private TableLayoutPanel tableLayoutPanelNextPreviousPeriod;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvPreviousProjectionPeriod;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvNextProjectionPeriod;
        private ContextMenuStrip contextMenuStripOpenPeriodsGrid;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem toolStripMenuItemDelete;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem toolStripMenuItemMoveUp;
        private ToolStripMenuItem toolStripMenuItemMoveDown;
        private ToolStripMenuItem toolStripMenuItemFromToPeriod;
        private ToolStripMenuItem toolStripMenuItemRollingPeriod;
        private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageBasic;
        private TableLayoutPanel tableLayoutPanelBasic;
        private Panel panel2;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvAllowedPreferenceActivity;
        private Label labelAllowedPreferenceActivity;
        private Panel panelBasic;
        private Label labelBasic;
        private TableLayoutPanel tableLayoutPanelBasicSchedule;
        private Label labelWriteProtect;
        private NullableIntegerTextBox integerTextBoxWriteProtect;
        private Label labelPublishSchedules;
        private Syncfusion.Windows.Forms.Tools.DateTimePickerAdv dateTimePickerAdvPublishedTo;
        private TableLayoutPanel tableLayoutPanelOpenPreference;
        private Panel panelOpenPreference;
        private Label labelOpenPreference;
        private Label labelPreferencePeriod;
        private Label labelIsOpen;
        private Teleopti.Ccc.Win.Common.Controls.DateSelection.DateSelectionFromTo dateSelectionFromToPreferencePeriod;
        private Teleopti.Ccc.Win.Common.Controls.DateSelection.DateSelectionFromTo dateSelectionFromToIsOpen;
        private ErrorProvider errorProvider1;
        private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageAdvShiftTradeRequest;
        private TableLayoutPanel tableLayoutPanelShiftTrade;
        private Panel panelOpenForShiftTrade;
        private Label labelOpenForShiftTrade;
        private Panel panelTolerance;
        private Label labelTolerance;
        private MinMaxIntegerTextBoxControl minMaxIntegerTextBoxControl1;
        private Teleopti.Ccc.Win.Common.Controls.DateTimePeriodVisualizer.DateOnlyPeriodsVisualizer dateOnlyPeriodsVisualizer1;
        private Panel panelVisualizationButtons;
        private Syncfusion.Windows.Forms.ButtonAdv buttonPanRight;
        private Syncfusion.Windows.Forms.ButtonAdv buttonZoomIn;
        private Syncfusion.Windows.Forms.ButtonAdv buttonPanLeft;
        private Syncfusion.Windows.Forms.ButtonAdv buttonZoomOut;
        private Panel panel3;
        private Label labelDaysOffAvailableForExtendedPreferences;
        private Panel panel4;
        private Label labelShiftCategoriesAvailableForExtendedPreference;
        private TwoListSelector twoListSelectorDayOffs;
        private TwoListSelector twoListSelectorCategories;
        private TwoListSelector twoListSelectorMatchingSkills;
        private Panel panelMatchingSkills;
        private Label labelMatchingSkills;
        private TimeSpanTextBox timeSpanTextBox1;
        private TableLayoutPanel tableLayoutPanel2;
        private Label labelTolerancePosNeg;
        private Label labelHMm;
        private Panel panel5;
        private Label label3;
        private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdvAutoGrant;
		private Panel panel6;
		private Label label4;
		private Syncfusion.Windows.Forms.Tools.RadioButtonAdv radioButtonAdvFairnessPoints;
		private Syncfusion.Windows.Forms.Tools.RadioButtonAdv radioButtonAdvFairnessEqual;
        private Panel panel7;
        private Label labelOpenStudentAvailability;
        private TableLayoutPanel tableLayoutPanelStudentAvailability;
        private Label labelStudentAvailabilityPeriod;
        private Label label6;
        private Controls.DateSelection.DateSelectionFromTo dateSelectionFromToStudentAvailability;
        private Controls.DateSelection.DateSelectionFromTo dateSelectionFromToIsOpenStudentAvailability;
        private Panel panel8;
        private Label labelAbsencesAvailableForExtendedPreference;
		  private TwoListSelector twoListSelectorAbsences;
		  private Syncfusion.Windows.Forms.Tools.AutoLabel labelMinimumTimePerWeek;
		  private TimeSpanTextBox textBoxExtMinTimePerWeek;
    }
}
