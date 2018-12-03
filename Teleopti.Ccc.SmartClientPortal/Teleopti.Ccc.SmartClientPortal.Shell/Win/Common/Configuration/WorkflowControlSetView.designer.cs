using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.DateSelection;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.DateTimePeriodVisualizer;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
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

				if (_gridHelper != null)
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
			this.buttonNew = new Syncfusion.Windows.Forms.ButtonAdv();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.autoLabelChangeInfo = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.autoLabel5 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.autoLabel1 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.comboBoxAdvWorkflowControlSet = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.comboBoxOvertimeRequestMaximumTimeHandleType = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.comboBoxOvertimeRequestStaffingCheckMethod = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.comboBoxOvertimeRequestMaximumContinuousWorkTimeHandleType = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.textBoxDescription = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
			this.autoLabelInfoAboutChanges = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.tabControlAdvArea = new Syncfusion.Windows.Forms.Tools.TabControlAdv();
			this.tabPageBasic = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.tableLayoutPanelBasic = new System.Windows.Forms.TableLayoutPanel();
			this.twoListSelectorAbsencesForReport = new Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.TwoListSelector();
			this.panel9 = new System.Windows.Forms.Panel();
			this.labelAllowedAbsencesForReport = new System.Windows.Forms.Label();
			this.twoListSelectorAbsences = new Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.TwoListSelector();
			this.panel8 = new System.Windows.Forms.Panel();
			this.labelAbsencesAvailableForExtendedPreference = new System.Windows.Forms.Label();
			this.tableLayoutPanelStudentAvailability = new System.Windows.Forms.TableLayoutPanel();
			this.labelStudentAvailabilityPeriod = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.dateSelectionFromToStudentAvailability = new Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.DateSelection.DateSelectionFromTo();
			this.dateSelectionFromToIsOpenStudentAvailability = new Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.DateSelection.DateSelectionFromTo();
			this.twoListSelectorCategories = new Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.TwoListSelector();
			this.panelVisualizationButtons = new System.Windows.Forms.Panel();
			this.buttonZoomOut = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonZoomIn = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonPanLeft = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonPanRight = new Syncfusion.Windows.Forms.ButtonAdv();
			this.twoListSelectorDayOffs = new Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.TwoListSelector();
			this.dateOnlyPeriodsVisualizer1 = new Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.DateTimePeriodVisualizer.DateOnlyPeriodsVisualizer();
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
			this.integerTextBoxWriteProtect = new Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.NullableIntegerTextBox();
			this.labelPublishSchedules = new System.Windows.Forms.Label();
			this.dateTimePickerAdvPublishedTo = new Syncfusion.Windows.Forms.Tools.DateTimePickerAdv();
			this.tableLayoutPanelOpenPreference = new System.Windows.Forms.TableLayoutPanel();
			this.labelPreferencePeriod = new System.Windows.Forms.Label();
			this.labelIsOpen = new System.Windows.Forms.Label();
			this.dateSelectionFromToPreferencePeriod = new Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.DateSelection.DateSelectionFromTo();
			this.dateSelectionFromToIsOpen = new Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.DateSelection.DateSelectionFromTo();
			this.panelOpenPreference = new System.Windows.Forms.Panel();
			this.labelOpenPreference = new System.Windows.Forms.Label();
			this.panel6 = new System.Windows.Forms.Panel();
			this.label4 = new System.Windows.Forms.Label();
			this.radioButtonAdvFairnessEqual = new Syncfusion.Windows.Forms.Tools.RadioButtonAdv();
			this.panel7 = new System.Windows.Forms.Panel();
			this.labelOpenStudentAvailability = new System.Windows.Forms.Label();
			this.radioButtonAdvSeniority = new Syncfusion.Windows.Forms.Tools.RadioButtonAdv();
			this.tabPageAdvAbsenceRequests = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.tableLayoutPanelAbsenceRequestPeriods = new System.Windows.Forms.TableLayoutPanel();
			this.panel1 = new System.Windows.Forms.Panel();
			this.dateTimePickerAdvViewpoint = new Syncfusion.Windows.Forms.Tools.DateTimePickerAdv();
			this.labelAbsenceRequestsVisualisation = new System.Windows.Forms.Label();
			this.gridControlVisualisation = new Syncfusion.Windows.Forms.Grid.GridControl();
			this.tableLayoutPanelNextPreviousPeriod = new System.Windows.Forms.TableLayoutPanel();
			this.buttonAdvPreviousProjectionPeriod = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvNextProjectionPeriod = new Syncfusion.Windows.Forms.ButtonAdv();
			this.tableLayoutPanelAbsenceRequestMiscellaneous = new System.Windows.Forms.TableLayoutPanel();
			this.labelAbsenceRequestMiscellaneous = new System.Windows.Forms.Label();
			this.tableLayoutPanelWaitlist = new System.Windows.Forms.TableLayoutPanel();
			this.checkBoxEnableAbsenceRequestWaitlisting = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.radioButtonWaitlistFirstComeFirstServed = new System.Windows.Forms.RadioButton();
			this.radioButtonWaitlistBySeniority = new System.Windows.Forms.RadioButton();
			this.tableLayoutPanelAbsenceProbability = new System.Windows.Forms.TableLayoutPanel();
			this.checkBoxEnableAbsenceProbability = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.tableLayoutPanelAbsenceRequestCancellation = new System.Windows.Forms.TableLayoutPanel();
			this.txtAbsenceRequestCancellationThreshold = new Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.NullableIntegerTextBox();
			this.timeSpanTextBoxOvertimeRequestMaximumTime = new TimeSpanTextBox();
			this.timeSpanTextBoxOvertimeRequestMaximumContinuousWorkTime = new TimeSpanTextBox();
			this.timeSpanTextBoxOvertimeRequestMinimumRestTimeThreshold = new TimeSpanTextBox();
			this.labelAbsenceRequestCancellationThreshold = new System.Windows.Forms.Label();
			this.tableLayoutPanelAbsenceRequestExpiration = new System.Windows.Forms.TableLayoutPanel();
			this.txtAbsenceRequestExpiredThreshold = new Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.NullableIntegerTextBox();
			this.labelAbsenceRequestExpriedThreshold = new System.Windows.Forms.Label();
			this.tableLayoutPanelOpenForAbsenceRequests = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanelOpenForOvertimeRequests = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanelOvertimeMaximumSetting = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanelOvertimeStaffingCheckMethodSetting = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanelOvertimeProbablityAndPrimarySkill = new System.Windows.Forms.TableLayoutPanel();
			this.labelOvertimeStaffingCheckMethodSetting = new System.Windows.Forms.Label();
			this.tableLayoutPanelOvertimeMaximumContinuousWorkTimeSetting = new System.Windows.Forms.TableLayoutPanel();
			this.labelOpenForAbsenceRequests = new System.Windows.Forms.Label();
			this.labelOpenForOvertimeRequests = new System.Windows.Forms.Label();
			this.lblOvertimeRequestMaximumTimeHandleType = new System.Windows.Forms.Label();
			this.lblOvertimeRequestMinimumRestTimeThreshold = new System.Windows.Forms.Label();
			this.lblOvertimeRequestMaximumContinuousWorkTimeHandleType = new System.Windows.Forms.Label();
			this.panel10 = new System.Windows.Forms.Panel();
			this.label1 = new System.Windows.Forms.Label();
			this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
			this.daysTbx = new System.Windows.Forms.NumericUpDown();
			this.label2 = new System.Windows.Forms.Label();
			this.buttonDeleteAbsenceRequestPeriod = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonDeleteOvertimeRequestPeriod = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAddAbsenceRequestPeriod = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAddOvertimeRequestPeriod = new Syncfusion.Windows.Forms.ButtonAdv();
			this.gridControlAbsenceRequestOpenPeriods = new Syncfusion.Windows.Forms.Grid.GridControl();
			this.gridControlOvertimeRequestOpenPeriods = new Syncfusion.Windows.Forms.Grid.GridControl();
			this.contextMenuStripOpenPeriodsGrid = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.contextMenuStripOvertimeRequestOpenPeriodsGrid = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.toolStripMenuItemFromToPeriod = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemRollingPeriod = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemDelete = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemMoveUp = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemMoveDown = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemOvertimeRequestFromToPeriod = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemOvertimeRequestRollingPeriod = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemOvertimeRequestDelete = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2OvertimeRequest = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemOvertimeRequestMoveUp = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemOvertimeRequestMoveDown = new System.Windows.Forms.ToolStripMenuItem();
			this.tabPageAdvShiftTradeRequest = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.tableLayoutPanelShiftTrade = new System.Windows.Forms.TableLayoutPanel();
			this.panel5 = new System.Windows.Forms.Panel();
			this.label3 = new System.Windows.Forms.Label();
			this.panelOpenForShiftTrade = new System.Windows.Forms.Panel();
			this.labelOpenForShiftTrade = new System.Windows.Forms.Label();
			this.panelTolerance = new System.Windows.Forms.Panel();
			this.labelTolerance = new System.Windows.Forms.Label();
			this.minMaxIntegerTextBoxControl1 = new Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.MinMaxIntegerTextBoxControl();
			this.twoListSelectorMatchingSkills = new Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.TwoListSelector();
			this.panelMatchingSkills = new System.Windows.Forms.Panel();
			this.labelMatchingSkills = new System.Windows.Forms.Label();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.timeSpanTextBox1 = new Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.TimeSpanTextBox();
			this.labelTolerancePosNeg = new System.Windows.Forms.Label();
			this.labelHMm = new System.Windows.Forms.Label();
			this.checkBoxAdvAutoGrant = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.checkBoxAdvAnonymousTrading = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.checkBoxAdvLockTrading = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.tabPageAdvETOTRequest = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.tableLayoutPanelETOTRequest = new System.Windows.Forms.TableLayoutPanel();
			this.panelOvertimeRequestHeaderBasic = new System.Windows.Forms.Panel();
			this.labelOvertimeRequestBasic = new System.Windows.Forms.Label();
			this.checkBoxAdvOvertimeProbability = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.checkBoxUsePrimarySkill = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.checkBoxAdvOvertimeMaximumEnabled = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.checkBoxAdvOvertimeMaximumContinuousWorkTimeEnabled = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
			((System.ComponentModel.ISupportInitialize)(this.gradientPanelHeader)).BeginInit();
			this.gradientPanelHeader.SuspendLayout();
			this.tableLayoutPanelHeader.SuspendLayout();
			this.tableLayoutPanelMain.SuspendLayout();
			this.tableLayoutPanelSelectWorkloadControlSet.SuspendLayout();
			this.tableLayoutPanelSubHeader1.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvWorkflowControlSet)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxOvertimeRequestMaximumTimeHandleType)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxOvertimeRequestStaffingCheckMethod)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxOvertimeRequestMaximumContinuousWorkTimeHandleType)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.textBoxDescription)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.tabControlAdvArea)).BeginInit();
			this.tabControlAdvArea.SuspendLayout();
			this.tabPageBasic.SuspendLayout();
			this.tableLayoutPanelBasic.SuspendLayout();
			this.panel9.SuspendLayout();
			this.panel8.SuspendLayout();
			this.tableLayoutPanelStudentAvailability.SuspendLayout();
			this.panelVisualizationButtons.SuspendLayout();
			this.panel4.SuspendLayout();
			this.panel3.SuspendLayout();
			this.panel10.SuspendLayout();
			this.panel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvAllowedPreferenceActivity)).BeginInit();
			this.panelBasic.SuspendLayout();
			this.tableLayoutPanelBasicSchedule.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvPublishedTo)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvPublishedTo.Calendar)).BeginInit();
			this.tableLayoutPanelOpenPreference.SuspendLayout();
			this.panelOpenPreference.SuspendLayout();
			this.panel6.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonAdvFairnessEqual)).BeginInit();
			this.panel7.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonAdvSeniority)).BeginInit();
			this.tabPageAdvAbsenceRequests.SuspendLayout();
			this.tableLayoutPanelAbsenceRequestPeriods.SuspendLayout();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvViewpoint)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvViewpoint.Calendar)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.gridControlVisualisation)).BeginInit();
			this.tableLayoutPanelNextPreviousPeriod.SuspendLayout();
			this.tableLayoutPanelAbsenceRequestMiscellaneous.SuspendLayout();
			this.tableLayoutPanelWaitlist.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxEnableAbsenceRequestWaitlisting)).BeginInit();
			this.tableLayoutPanelAbsenceProbability.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxEnableAbsenceProbability)).BeginInit();
			this.tableLayoutPanelAbsenceRequestCancellation.SuspendLayout();
			this.tableLayoutPanelAbsenceRequestExpiration.SuspendLayout();
			this.tableLayoutPanelOpenForAbsenceRequests.SuspendLayout();
			this.tableLayoutPanelOpenForOvertimeRequests.SuspendLayout();
			this.tableLayoutPanelOvertimeMaximumSetting.ResumeLayout(false);
			this.tableLayoutPanelOvertimeStaffingCheckMethodSetting.ResumeLayout(false);
			this.tableLayoutPanelOvertimeProbablityAndPrimarySkill.ResumeLayout(false);
			this.tableLayoutPanelOvertimeMaximumContinuousWorkTimeSetting.ResumeLayout(false);
			this.tableLayoutPanelOvertimeMaximumSetting.PerformLayout();
			this.tableLayoutPanelOvertimeStaffingCheckMethodSetting.PerformLayout();
			this.tableLayoutPanelOvertimeProbablityAndPrimarySkill.PerformLayout();
			this.tableLayoutPanelOvertimeMaximumContinuousWorkTimeSetting.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridControlAbsenceRequestOpenPeriods)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.gridControlOvertimeRequestOpenPeriods)).BeginInit();
			this.contextMenuStripOpenPeriodsGrid.SuspendLayout();
			this.contextMenuStripOvertimeRequestOpenPeriodsGrid.SuspendLayout();
			this.tabPageAdvShiftTradeRequest.SuspendLayout();
			this.tableLayoutPanelShiftTrade.SuspendLayout();
			this.panel5.SuspendLayout();
			this.panelOpenForShiftTrade.SuspendLayout();
			this.panelTolerance.SuspendLayout();
			this.panelMatchingSkills.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvAutoGrant)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvAnonymousTrading)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvLockTrading)).BeginInit();
			this.tabPageAdvETOTRequest.SuspendLayout();
			this.tableLayoutPanelETOTRequest.SuspendLayout();
			this.panelOvertimeRequestHeaderBasic.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvOvertimeProbability)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxUsePrimarySkill)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvOvertimeMaximumEnabled)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvOvertimeMaximumContinuousWorkTimeEnabled)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
			this.SuspendLayout();
			// 
			// gradientPanelHeader
			// 
			this.gradientPanelHeader.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.gradientPanelHeader.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.gradientPanelHeader.BackColor = System.Drawing.SystemColors.ActiveCaption;
			this.gradientPanelHeader.BorderSingle = System.Windows.Forms.ButtonBorderStyle.None;
			this.gradientPanelHeader.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.gradientPanelHeader.Controls.Add(this.tableLayoutPanelHeader);
			this.gradientPanelHeader.Location = new System.Drawing.Point(0, 0);
			this.gradientPanelHeader.Name = "gradientPanelHeader";
			this.gradientPanelHeader.Padding = new System.Windows.Forms.Padding(12);
			this.gradientPanelHeader.Size = new System.Drawing.Size(978, 62);
			this.gradientPanelHeader.TabIndex = 0;
			// 
			// tableLayoutPanelHeader
			// 
			this.tableLayoutPanelHeader.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanelHeader.ColumnCount = 1;
			this.tableLayoutPanelHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 1056F));
			this.tableLayoutPanelHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelHeader.Controls.Add(this.labelHeader, 1, 0);
			this.tableLayoutPanelHeader.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelHeader.Location = new System.Drawing.Point(12, 12);
			this.tableLayoutPanelHeader.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanelHeader.Name = "tableLayoutPanelHeader";
			this.tableLayoutPanelHeader.RowCount = 1;
			this.tableLayoutPanelHeader.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelHeader.Size = new System.Drawing.Size(954, 38);
			this.tableLayoutPanelHeader.TabIndex = 0;
			// 
			// labelHeader
			// 
			this.labelHeader.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelHeader.AutoSize = true;
			this.labelHeader.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelHeader.ForeColor = System.Drawing.Color.MidnightBlue;
			this.labelHeader.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.labelHeader.Location = new System.Drawing.Point(3, 0);
			this.labelHeader.Name = "labelHeader";
			this.labelHeader.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
			this.labelHeader.Size = new System.Drawing.Size(597, 38);
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
			this.tableLayoutPanelMain.Location = new System.Drawing.Point(3, 69);
			this.tableLayoutPanelMain.Name = "tableLayoutPanelMain";
			this.tableLayoutPanelMain.RowCount = 2;
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 150F));
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelMain.Size = new System.Drawing.Size(974, 613);
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
			this.tableLayoutPanelSelectWorkloadControlSet.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanelSelectWorkloadControlSet.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelSelectWorkloadControlSet.Size = new System.Drawing.Size(974, 150);
			this.tableLayoutPanelSelectWorkloadControlSet.TabIndex = 0;
			// 
			// tableLayoutPanelSubHeader1
			// 
			this.tableLayoutPanelSubHeader1.BackColor = System.Drawing.Color.LightSteelBlue;
			this.tableLayoutPanelSubHeader1.ColumnCount = 3;
			this.tableLayoutPanelSubHeader1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelSubHeader1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelSubHeader1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelSubHeader1.Controls.Add(this.labelSubHeader1, 0, 0);
			this.tableLayoutPanelSubHeader1.Controls.Add(this.buttonDelete, 2, 0);
			this.tableLayoutPanelSubHeader1.Controls.Add(this.buttonNew, 1, 0);
			this.tableLayoutPanelSubHeader1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelSubHeader1.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanelSubHeader1.Name = "tableLayoutPanelSubHeader1";
			this.tableLayoutPanelSubHeader1.RowCount = 1;
			this.tableLayoutPanelSubHeader1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelSubHeader1.Size = new System.Drawing.Size(968, 34);
			this.tableLayoutPanelSubHeader1.TabIndex = 0;
			// 
			// labelSubHeader1
			// 
			this.labelSubHeader1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelSubHeader1.AutoSize = true;
			this.labelSubHeader1.BackColor = System.Drawing.Color.Transparent;
			this.labelSubHeader1.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelSubHeader1.ForeColor = System.Drawing.Color.GhostWhite;
			this.labelSubHeader1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.labelSubHeader1.Location = new System.Drawing.Point(3, 0);
			this.labelSubHeader1.Name = "labelSubHeader1";
			this.labelSubHeader1.Size = new System.Drawing.Size(458, 36);
			this.labelSubHeader1.TabIndex = 0;
			this.labelSubHeader1.Text = "xxChooseWorkflowControlSetToEdit";
			this.labelSubHeader1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// buttonDelete
			// 
			this.buttonDelete.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.buttonDelete.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonDelete.BackColor = System.Drawing.Color.White;
			this.buttonDelete.BeforeTouchSize = new System.Drawing.Size(28, 28);
			this.buttonDelete.ForeColor = System.Drawing.Color.White;
			this.buttonDelete.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_temp_DeleteGroup10;
			this.buttonDelete.IsBackStageButton = false;
			this.buttonDelete.Location = new System.Drawing.Point(933, 4);
			this.buttonDelete.Margin = new System.Windows.Forms.Padding(0, 1, 3, 0);
			this.buttonDelete.Name = "buttonDelete";
			this.buttonDelete.Size = new System.Drawing.Size(28, 28);
			this.buttonDelete.TabIndex = 2;
			this.buttonDelete.UseVisualStyle = true;
			this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
			// 
			// buttonNew
			// 
			this.buttonNew.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.buttonNew.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonNew.BackColor = System.Drawing.Color.White;
			this.buttonNew.BeforeTouchSize = new System.Drawing.Size(28, 28);
			this.buttonNew.Font = new System.Drawing.Font("Tahoma", 8F);
			this.buttonNew.ForeColor = System.Drawing.Color.White;
			this.buttonNew.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.test_add2;
			this.buttonNew.IsBackStageButton = false;
			this.buttonNew.Location = new System.Drawing.Point(898, 4);
			this.buttonNew.Margin = new System.Windows.Forms.Padding(0, 1, 3, 0);
			this.buttonNew.Name = "buttonNew";
			this.buttonNew.Size = new System.Drawing.Size(28, 28);
			this.buttonNew.TabIndex = 1;
			this.buttonNew.UseVisualStyle = true;
			this.buttonNew.UseVisualStyleBackColor = false;
			this.buttonNew.Click += new System.EventHandler(this.buttonNew_Click);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 204F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.autoLabelChangeInfo, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.autoLabel5, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.autoLabel1, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.comboBoxAdvWorkflowControlSet, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.textBoxDescription, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.autoLabelInfoAboutChanges, 1, 2);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 40);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 4;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(974, 110);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// autoLabelChangeInfo
			// 
			this.autoLabelChangeInfo.AutoSize = false;
			this.autoLabelChangeInfo.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.autoLabelChangeInfo.Location = new System.Drawing.Point(3, 73);
			this.autoLabelChangeInfo.Margin = new System.Windows.Forms.Padding(3);
			this.autoLabelChangeInfo.Name = "autoLabelChangeInfo";
			this.autoLabelChangeInfo.Size = new System.Drawing.Size(195, 29);
			this.autoLabelChangeInfo.TabIndex = 0;
			this.autoLabelChangeInfo.Text = "xxChangeInfoColon";
			this.autoLabelChangeInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// autoLabel5
			// 
			this.autoLabel5.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.autoLabel5.AutoSize = false;
			this.autoLabel5.Location = new System.Drawing.Point(3, 5);
			this.autoLabel5.Margin = new System.Windows.Forms.Padding(3);
			this.autoLabel5.Name = "autoLabel5";
			this.autoLabel5.Size = new System.Drawing.Size(197, 24);
			this.autoLabel5.TabIndex = 0;
			this.autoLabel5.Text = "xxWorkflowControlSetColon";
			this.autoLabel5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// autoLabel1
			// 
			this.autoLabel1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.autoLabel1.AutoSize = false;
			this.autoLabel1.Location = new System.Drawing.Point(3, 40);
			this.autoLabel1.Margin = new System.Windows.Forms.Padding(3);
			this.autoLabel1.Name = "autoLabel1";
			this.autoLabel1.Size = new System.Drawing.Size(197, 24);
			this.autoLabel1.TabIndex = 0;
			this.autoLabel1.Text = "xxDescriptionColon";
			this.autoLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// comboBoxAdvWorkflowControlSet
			// 
			this.comboBoxAdvWorkflowControlSet.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.comboBoxAdvWorkflowControlSet.BackColor = System.Drawing.Color.White;
			this.comboBoxAdvWorkflowControlSet.BeforeTouchSize = new System.Drawing.Size(252, 40);
			this.comboBoxAdvWorkflowControlSet.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAdvWorkflowControlSet.Location = new System.Drawing.Point(207, 3);
			this.comboBoxAdvWorkflowControlSet.Name = "comboBoxAdvWorkflowControlSet";
			this.comboBoxAdvWorkflowControlSet.Size = new System.Drawing.Size(252, 40);
			this.comboBoxAdvWorkflowControlSet.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.comboBoxAdvWorkflowControlSet.TabIndex = 3;
			this.comboBoxAdvWorkflowControlSet.SelectedIndexChanged += new System.EventHandler(this.comboBoxAdvWorkflowControlSet_SelectedIndexChanged);
			// 
			// textBoxDescription
			// 
			this.textBoxDescription.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.textBoxDescription.BeforeTouchSize = new System.Drawing.Size(251, 39);
			this.textBoxDescription.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.textBoxDescription.Location = new System.Drawing.Point(207, 38);
			this.textBoxDescription.MaxLength = 50;
			this.textBoxDescription.Metrocolor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.textBoxDescription.Name = "textBoxDescription";
			this.textBoxDescription.OverflowIndicatorToolTipText = null;
			this.textBoxDescription.Size = new System.Drawing.Size(251, 39);
			this.textBoxDescription.Style = Syncfusion.Windows.Forms.Tools.TextBoxExt.theme.Default;
			this.textBoxDescription.TabIndex = 4;
			this.textBoxDescription.WordWrap = false;
			this.textBoxDescription.Leave += new System.EventHandler(this.textBoxDescription_Leave);
			// 
			// autoLabelInfoAboutChanges
			// 
			this.autoLabelInfoAboutChanges.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.autoLabelInfoAboutChanges.AutoSize = false;
			this.autoLabelInfoAboutChanges.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.autoLabelInfoAboutChanges.Location = new System.Drawing.Point(207, 75);
			this.autoLabelInfoAboutChanges.Margin = new System.Windows.Forms.Padding(3);
			this.autoLabelInfoAboutChanges.Name = "autoLabelInfoAboutChanges";
			this.autoLabelInfoAboutChanges.Size = new System.Drawing.Size(752, 24);
			this.autoLabelInfoAboutChanges.TabIndex = 0;
			this.autoLabelInfoAboutChanges.Text = "xxInfoAboutChanges";
			this.autoLabelInfoAboutChanges.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// tabControlAdvArea
			// 
			this.tabControlAdvArea.ActiveTabColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(102)))), ((int)(((byte)(102)))));
			this.tabControlAdvArea.ActiveTabFont = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tabControlAdvArea.BeforeTouchSize = new System.Drawing.Size(968, 457);
			this.tabControlAdvArea.Controls.Add(this.tabPageBasic);
			this.tabControlAdvArea.Controls.Add(this.tabPageAdvAbsenceRequests);
			this.tabControlAdvArea.Controls.Add(this.tabPageAdvShiftTradeRequest);
			this.tabControlAdvArea.Controls.Add(this.tabPageAdvETOTRequest);
			this.tabControlAdvArea.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControlAdvArea.InactiveTabColor = System.Drawing.Color.White;
			this.tabControlAdvArea.Location = new System.Drawing.Point(3, 153);
			this.tabControlAdvArea.Name = "tabControlAdvArea";
			this.tabControlAdvArea.Size = new System.Drawing.Size(968, 457);
			this.tabControlAdvArea.TabIndex = 5;
			this.tabControlAdvArea.TabPanelBackColor = System.Drawing.Color.White;
			this.tabControlAdvArea.TabStyle = typeof(Syncfusion.Windows.Forms.Tools.TabRendererMetro);
			// 
			// tabPageBasic
			// 
			this.tabPageBasic.Controls.Add(this.tableLayoutPanelBasic);
			this.tabPageBasic.Image = null;
			this.tabPageBasic.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageBasic.Location = new System.Drawing.Point(1, 45);
			this.tabPageBasic.Name = "tabPageBasic";
			this.tabPageBasic.ShowCloseButton = true;
			this.tabPageBasic.Size = new System.Drawing.Size(965, 410);
			this.tabPageBasic.TabBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(191)))), ((int)(((byte)(234)))));
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
			this.tableLayoutPanelBasic.Controls.Add(this.twoListSelectorAbsencesForReport, 0, 17);
			this.tableLayoutPanelBasic.Controls.Add(this.panel9, 0, 16);
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
			this.tableLayoutPanelBasic.Controls.Add(this.panel6, 0, 18);
			this.tableLayoutPanelBasic.Controls.Add(this.radioButtonAdvFairnessEqual, 0, 19);
			this.tableLayoutPanelBasic.Controls.Add(this.panel7, 0, 4);
			this.tableLayoutPanelBasic.Controls.Add(this.radioButtonAdvSeniority, 0, 20);
			this.tableLayoutPanelBasic.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelBasic.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanelBasic.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanelBasic.Name = "tableLayoutPanelBasic";
			this.tableLayoutPanelBasic.RowCount = 22;
			this.tableLayoutPanelBasic.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 115F));
			this.tableLayoutPanelBasic.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelBasic.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanelBasic.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 70F));
			this.tableLayoutPanelBasic.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanelBasic.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 150F));
			this.tableLayoutPanelBasic.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanelBasic.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 150F));
			this.tableLayoutPanelBasic.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanelBasic.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 185F));
			this.tableLayoutPanelBasic.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanelBasic.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 185F));
			this.tableLayoutPanelBasic.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanelBasic.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 185F));
			this.tableLayoutPanelBasic.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanelBasic.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelBasic.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanelBasic.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 185F));
			this.tableLayoutPanelBasic.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanelBasic.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelBasic.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelBasic.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelBasic.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanelBasic.Size = new System.Drawing.Size(965, 410);
			this.tableLayoutPanelBasic.TabIndex = 0;
			// 
			// twoListSelectorAbsencesForReport
			// 
			this.twoListSelectorAbsencesForReport.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.twoListSelectorAbsencesForReport.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
			this.tableLayoutPanelBasic.SetColumnSpan(this.twoListSelectorAbsencesForReport, 2);
			this.twoListSelectorAbsencesForReport.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.twoListSelectorAbsencesForReport.Location = new System.Drawing.Point(0, 1430);
			this.twoListSelectorAbsencesForReport.Margin = new System.Windows.Forms.Padding(0);
			this.twoListSelectorAbsencesForReport.Name = "twoListSelectorAbsencesForReport";
			this.twoListSelectorAbsencesForReport.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.twoListSelectorAbsencesForReport.Size = new System.Drawing.Size(965, 173);
			this.twoListSelectorAbsencesForReport.TabIndex = 24;
			// 
			// panel9
			// 
			this.panel9.BackColor = System.Drawing.Color.LightSteelBlue;
			this.tableLayoutPanelBasic.SetColumnSpan(this.panel9, 2);
			this.panel9.Controls.Add(this.labelAllowedAbsencesForReport);
			this.panel9.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel9.Location = new System.Drawing.Point(3, 1393);
			this.panel9.Name = "panel9";
			this.panel9.Size = new System.Drawing.Size(959, 34);
			this.panel9.TabIndex = 23;
			// 
			// labelAllowedAbsencesForReport
			// 
			this.labelAllowedAbsencesForReport.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelAllowedAbsencesForReport.AutoSize = true;
			this.labelAllowedAbsencesForReport.BackColor = System.Drawing.Color.Transparent;
			this.labelAllowedAbsencesForReport.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelAllowedAbsencesForReport.ForeColor = System.Drawing.Color.GhostWhite;
			this.labelAllowedAbsencesForReport.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.labelAllowedAbsencesForReport.Location = new System.Drawing.Point(3, 8);
			this.labelAllowedAbsencesForReport.Name = "labelAllowedAbsencesForReport";
			this.labelAllowedAbsencesForReport.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
			this.labelAllowedAbsencesForReport.Size = new System.Drawing.Size(388, 39);
			this.labelAllowedAbsencesForReport.TabIndex = 0;
			this.labelAllowedAbsencesForReport.Text = "xxAbsencesAvailableForReport";
			this.labelAllowedAbsencesForReport.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// twoListSelectorAbsences
			// 
			this.twoListSelectorAbsences.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.twoListSelectorAbsences.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
			this.tableLayoutPanelBasic.SetColumnSpan(this.twoListSelectorAbsences, 2);
			this.twoListSelectorAbsences.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.twoListSelectorAbsences.Location = new System.Drawing.Point(0, 1130);
			this.twoListSelectorAbsences.Margin = new System.Windows.Forms.Padding(0);
			this.twoListSelectorAbsences.Name = "twoListSelectorAbsences";
			this.twoListSelectorAbsences.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.twoListSelectorAbsences.Size = new System.Drawing.Size(965, 173);
			this.twoListSelectorAbsences.TabIndex = 22;
			// 
			// panel8
			// 
			this.panel8.BackColor = System.Drawing.Color.LightSteelBlue;
			this.tableLayoutPanelBasic.SetColumnSpan(this.panel8, 2);
			this.panel8.Controls.Add(this.labelAbsencesAvailableForExtendedPreference);
			this.panel8.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel8.Location = new System.Drawing.Point(3, 1093);
			this.panel8.Name = "panel8";
			this.panel8.Size = new System.Drawing.Size(959, 34);
			this.panel8.TabIndex = 21;
			// 
			// labelAbsencesAvailableForExtendedPreference
			// 
			this.labelAbsencesAvailableForExtendedPreference.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelAbsencesAvailableForExtendedPreference.AutoSize = true;
			this.labelAbsencesAvailableForExtendedPreference.BackColor = System.Drawing.Color.Transparent;
			this.labelAbsencesAvailableForExtendedPreference.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelAbsencesAvailableForExtendedPreference.ForeColor = System.Drawing.Color.GhostWhite;
			this.labelAbsencesAvailableForExtendedPreference.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.labelAbsencesAvailableForExtendedPreference.Location = new System.Drawing.Point(3, 8);
			this.labelAbsencesAvailableForExtendedPreference.Name = "labelAbsencesAvailableForExtendedPreference";
			this.labelAbsencesAvailableForExtendedPreference.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
			this.labelAbsencesAvailableForExtendedPreference.Size = new System.Drawing.Size(549, 39);
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
			this.tableLayoutPanelStudentAvailability.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelStudentAvailability.Location = new System.Drawing.Point(0, 300);
			this.tableLayoutPanelStudentAvailability.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanelStudentAvailability.Name = "tableLayoutPanelStudentAvailability";
			this.tableLayoutPanelStudentAvailability.RowCount = 3;
			this.tableLayoutPanelStudentAvailability.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanelStudentAvailability.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 150F));
			this.tableLayoutPanelStudentAvailability.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelStudentAvailability.Size = new System.Drawing.Size(965, 150);
			this.tableLayoutPanelStudentAvailability.TabIndex = 20;
			// 
			// labelStudentAvailabilityPeriod
			// 
			this.labelStudentAvailabilityPeriod.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelStudentAvailabilityPeriod.AutoSize = true;
			this.labelStudentAvailabilityPeriod.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			this.labelStudentAvailabilityPeriod.Location = new System.Drawing.Point(3, 0);
			this.labelStudentAvailabilityPeriod.Name = "labelStudentAvailabilityPeriod";
			this.labelStudentAvailabilityPeriod.Size = new System.Drawing.Size(323, 30);
			this.labelStudentAvailabilityPeriod.TabIndex = 0;
			this.labelStudentAvailabilityPeriod.Text = "xxStudentAvailabilityPeriod";
			// 
			// label6
			// 
			this.label6.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label6.AutoSize = true;
			this.label6.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			this.label6.Location = new System.Drawing.Point(420, 0);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(121, 30);
			this.label6.TabIndex = 1;
			this.label6.Text = "xxIsOpen";
			// 
			// dateSelectionFromToStudentAvailability
			// 
			this.dateSelectionFromToStudentAvailability.BackColor = System.Drawing.Color.Transparent;
			this.dateSelectionFromToStudentAvailability.ButtonApplyText = "xxApply";
			this.dateSelectionFromToStudentAvailability.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			this.dateSelectionFromToStudentAvailability.HideNoneButtons = true;
			this.dateSelectionFromToStudentAvailability.LabelDateSelectionText = "xxFrom";
			this.dateSelectionFromToStudentAvailability.LabelDateSelectionToText = "xxTo";
			this.dateSelectionFromToStudentAvailability.Location = new System.Drawing.Point(3, 32);
			this.dateSelectionFromToStudentAvailability.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.dateSelectionFromToStudentAvailability.Name = "dateSelectionFromToStudentAvailability";
			this.dateSelectionFromToStudentAvailability.NoneButtonText = "xxNone";
			this.dateSelectionFromToStudentAvailability.NullString = "xxNoDateIsSelected";
			this.dateSelectionFromToStudentAvailability.ShowApplyButton = false;
			this.dateSelectionFromToStudentAvailability.Size = new System.Drawing.Size(187, 116);
			this.dateSelectionFromToStudentAvailability.TabIndex = 7;
			this.dateSelectionFromToStudentAvailability.TodayButtonText = "xxToday";
			this.dateSelectionFromToStudentAvailability.WorkPeriodEnd =
				new Teleopti.Ccc.Domain.InterfaceLegacy.Domain.DateOnly(2018, 1, 1);
			this.dateSelectionFromToStudentAvailability.WorkPeriodStart = new Teleopti.Ccc.Domain.InterfaceLegacy.Domain.DateOnly(2018, 1, 1);
			this.dateSelectionFromToStudentAvailability.Validating += new System.ComponentModel.CancelEventHandler(this.dateSelectionFromToStudentAvailability_Validating);
			this.dateSelectionFromToStudentAvailability.Validated += new System.EventHandler(this.dateSelectionFromToStudentAvailability_Validated);
			// 
			// dateSelectionFromToIsOpenStudentAvailability
			// 
			this.dateSelectionFromToIsOpenStudentAvailability.BackColor = System.Drawing.Color.Transparent;
			this.dateSelectionFromToIsOpenStudentAvailability.ButtonApplyText = "xxApply";
			this.dateSelectionFromToIsOpenStudentAvailability.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			this.dateSelectionFromToIsOpenStudentAvailability.HideNoneButtons = true;
			this.dateSelectionFromToIsOpenStudentAvailability.LabelDateSelectionText = "xxFrom";
			this.dateSelectionFromToIsOpenStudentAvailability.LabelDateSelectionToText = "xxTo";
			this.dateSelectionFromToIsOpenStudentAvailability.Location = new System.Drawing.Point(420, 32);
			this.dateSelectionFromToIsOpenStudentAvailability.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.dateSelectionFromToIsOpenStudentAvailability.Name = "dateSelectionFromToIsOpenStudentAvailability";
			this.dateSelectionFromToIsOpenStudentAvailability.NoneButtonText = "xxNone";
			this.dateSelectionFromToIsOpenStudentAvailability.NullString = "xxNoDateIsSelected";
			this.dateSelectionFromToIsOpenStudentAvailability.ShowApplyButton = false;
			this.dateSelectionFromToIsOpenStudentAvailability.Size = new System.Drawing.Size(187, 116);
			this.dateSelectionFromToIsOpenStudentAvailability.TabIndex = 8;
			this.dateSelectionFromToIsOpenStudentAvailability.TodayButtonText = "xxToday";
			this.dateSelectionFromToIsOpenStudentAvailability.WorkPeriodEnd = new Teleopti.Ccc.Domain.InterfaceLegacy.Domain.DateOnly(2018, 1, 1);
			this.dateSelectionFromToIsOpenStudentAvailability.WorkPeriodStart = new Teleopti.Ccc.Domain.InterfaceLegacy.Domain.DateOnly(2018, 1, 1);
			this.dateSelectionFromToIsOpenStudentAvailability.Validating += new System.ComponentModel.CancelEventHandler(this.dateSelectionFromToIsOpenStudentAvailability_Validating);
			this.dateSelectionFromToIsOpenStudentAvailability.Validated += new System.EventHandler(this.dateSelectionFromToIsOpenStudentAvailability_Validated);
			// 
			// twoListSelectorCategories
			// 
			this.twoListSelectorCategories.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.twoListSelectorCategories.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
			this.tableLayoutPanelBasic.SetColumnSpan(this.twoListSelectorCategories, 2);
			this.twoListSelectorCategories.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.twoListSelectorCategories.Location = new System.Drawing.Point(0, 905);
			this.twoListSelectorCategories.Margin = new System.Windows.Forms.Padding(0);
			this.twoListSelectorCategories.Name = "twoListSelectorCategories";
			this.twoListSelectorCategories.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.twoListSelectorCategories.Size = new System.Drawing.Size(965, 173);
			this.twoListSelectorCategories.TabIndex = 13;
			// 
			// panelVisualizationButtons
			// 
			this.panelVisualizationButtons.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.tableLayoutPanelBasic.SetColumnSpan(this.panelVisualizationButtons, 2);
			this.panelVisualizationButtons.Controls.Add(this.buttonZoomOut);
			this.panelVisualizationButtons.Controls.Add(this.buttonZoomIn);
			this.panelVisualizationButtons.Controls.Add(this.buttonPanLeft);
			this.panelVisualizationButtons.Controls.Add(this.buttonPanRight);
			this.panelVisualizationButtons.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelVisualizationButtons.Location = new System.Drawing.Point(0, 115);
			this.panelVisualizationButtons.Margin = new System.Windows.Forms.Padding(0);
			this.panelVisualizationButtons.Name = "panelVisualizationButtons";
			this.panelVisualizationButtons.Size = new System.Drawing.Size(965, 35);
			this.panelVisualizationButtons.TabIndex = 11;
			// 
			// buttonZoomOut
			// 
			this.buttonZoomOut.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.buttonZoomOut.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonZoomOut.BackColor = System.Drawing.Color.White;
			this.buttonZoomOut.BeforeTouchSize = new System.Drawing.Size(28, 28);
			this.buttonZoomOut.ForeColor = System.Drawing.Color.White;
			this.buttonZoomOut.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.Magifier_zoom_out;
			this.buttonZoomOut.IsBackStageButton = false;
			this.buttonZoomOut.Location = new System.Drawing.Point(766, 3);
			this.buttonZoomOut.Name = "buttonZoomOut";
			this.buttonZoomOut.Size = new System.Drawing.Size(28, 28);
			this.buttonZoomOut.TabIndex = 3;
			this.buttonZoomOut.UseVisualStyle = true;
			this.buttonZoomOut.Click += new System.EventHandler(this.buttonZoomOut_Click);
			// 
			// buttonZoomIn
			// 
			this.buttonZoomIn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.buttonZoomIn.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonZoomIn.BackColor = System.Drawing.Color.White;
			this.buttonZoomIn.BeforeTouchSize = new System.Drawing.Size(28, 28);
			this.buttonZoomIn.ForeColor = System.Drawing.Color.White;
			this.buttonZoomIn.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.Magnifier_zoom_in;
			this.buttonZoomIn.IsBackStageButton = false;
			this.buttonZoomIn.Location = new System.Drawing.Point(809, 3);
			this.buttonZoomIn.Name = "buttonZoomIn";
			this.buttonZoomIn.Size = new System.Drawing.Size(28, 28);
			this.buttonZoomIn.TabIndex = 2;
			this.buttonZoomIn.UseVisualStyle = true;
			this.buttonZoomIn.Click += new System.EventHandler(this.buttonZoomIn_Click);
			// 
			// buttonPanLeft
			// 
			this.buttonPanLeft.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.buttonPanLeft.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonPanLeft.BackColor = System.Drawing.Color.White;
			this.buttonPanLeft.BeforeTouchSize = new System.Drawing.Size(28, 28);
			this.buttonPanLeft.ButtonType = Syncfusion.Windows.Forms.Tools.ButtonTypes.Left;
			this.buttonPanLeft.ForeColor = System.Drawing.Color.White;
			this.buttonPanLeft.IsBackStageButton = false;
			this.buttonPanLeft.Location = new System.Drawing.Point(876, 3);
			this.buttonPanLeft.Name = "buttonPanLeft";
			this.buttonPanLeft.Size = new System.Drawing.Size(28, 28);
			this.buttonPanLeft.TabIndex = 1;
			this.buttonPanLeft.UseVisualStyle = true;
			this.buttonPanLeft.Click += new System.EventHandler(this.buttonPanLeft_Click);
			// 
			// buttonPanRight
			// 
			this.buttonPanRight.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.buttonPanRight.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonPanRight.BackColor = System.Drawing.Color.White;
			this.buttonPanRight.BeforeTouchSize = new System.Drawing.Size(28, 28);
			this.buttonPanRight.ButtonType = Syncfusion.Windows.Forms.Tools.ButtonTypes.Right;
			this.buttonPanRight.ForeColor = System.Drawing.Color.White;
			this.buttonPanRight.IsBackStageButton = false;
			this.buttonPanRight.Location = new System.Drawing.Point(919, 3);
			this.buttonPanRight.Name = "buttonPanRight";
			this.buttonPanRight.Size = new System.Drawing.Size(28, 28);
			this.buttonPanRight.TabIndex = 0;
			this.buttonPanRight.UseVisualStyle = true;
			this.buttonPanRight.Click += new System.EventHandler(this.buttonPanRight_Click);
			// 
			// twoListSelectorDayOffs
			// 
			this.twoListSelectorDayOffs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.twoListSelectorDayOffs.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
			this.tableLayoutPanelBasic.SetColumnSpan(this.twoListSelectorDayOffs, 2);
			this.twoListSelectorDayOffs.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.twoListSelectorDayOffs.Location = new System.Drawing.Point(0, 680);
			this.twoListSelectorDayOffs.Margin = new System.Windows.Forms.Padding(0);
			this.twoListSelectorDayOffs.Name = "twoListSelectorDayOffs";
			this.twoListSelectorDayOffs.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.twoListSelectorDayOffs.Size = new System.Drawing.Size(965, 176);
			this.twoListSelectorDayOffs.TabIndex = 12;
			// 
			// dateOnlyPeriodsVisualizer1
			// 
			this.dateOnlyPeriodsVisualizer1.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.tableLayoutPanelBasic.SetColumnSpan(this.dateOnlyPeriodsVisualizer1, 2);
			//this.dateOnlyPeriodsVisualizer1.ContainedPeriod = ((Teleopti.Ccc.Domain.InterfaceLegacy.Domain.DateOnlyPeriod)(resources.GetObject("dateOnlyPeriodsVisualizer1.ContainedPeriod")));
			this.dateOnlyPeriodsVisualizer1.Culture = new System.Globalization.CultureInfo("sv-SE");
			this.dateOnlyPeriodsVisualizer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dateOnlyPeriodsVisualizer1.Location = new System.Drawing.Point(0, 0);
			this.dateOnlyPeriodsVisualizer1.Margin = new System.Windows.Forms.Padding(0);
			this.dateOnlyPeriodsVisualizer1.MonthsOnEachSide = 1;
			this.dateOnlyPeriodsVisualizer1.Name = "dateOnlyPeriodsVisualizer1";
			this.dateOnlyPeriodsVisualizer1.Size = new System.Drawing.Size(965, 115);
			this.dateOnlyPeriodsVisualizer1.TabIndex = 10;
			// 
			// panel4
			// 
			this.panel4.BackColor = System.Drawing.Color.LightSteelBlue;
			this.tableLayoutPanelBasic.SetColumnSpan(this.panel4, 2);
			this.panel4.Controls.Add(this.labelShiftCategoriesAvailableForExtendedPreference);
			this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel4.Location = new System.Drawing.Point(3, 868);
			this.panel4.Name = "panel4";
			this.panel4.Size = new System.Drawing.Size(959, 34);
			this.panel4.TabIndex = 11;
			// 
			// labelShiftCategoriesAvailableForExtendedPreference
			// 
			this.labelShiftCategoriesAvailableForExtendedPreference.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelShiftCategoriesAvailableForExtendedPreference.AutoSize = true;
			this.labelShiftCategoriesAvailableForExtendedPreference.BackColor = System.Drawing.Color.Transparent;
			this.labelShiftCategoriesAvailableForExtendedPreference.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelShiftCategoriesAvailableForExtendedPreference.ForeColor = System.Drawing.Color.GhostWhite;
			this.labelShiftCategoriesAvailableForExtendedPreference.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.labelShiftCategoriesAvailableForExtendedPreference.Location = new System.Drawing.Point(3, 8);
			this.labelShiftCategoriesAvailableForExtendedPreference.Name = "labelShiftCategoriesAvailableForExtendedPreference";
			this.labelShiftCategoriesAvailableForExtendedPreference.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
			this.labelShiftCategoriesAvailableForExtendedPreference.Size = new System.Drawing.Size(623, 39);
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
			this.panel3.Location = new System.Drawing.Point(3, 643);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(959, 34);
			this.panel3.TabIndex = 10;
			// 
			// labelDaysOffAvailableForExtendedPreferences
			// 
			this.labelDaysOffAvailableForExtendedPreferences.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelDaysOffAvailableForExtendedPreferences.AutoSize = true;
			this.labelDaysOffAvailableForExtendedPreferences.BackColor = System.Drawing.Color.Transparent;
			this.labelDaysOffAvailableForExtendedPreferences.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelDaysOffAvailableForExtendedPreferences.ForeColor = System.Drawing.Color.GhostWhite;
			this.labelDaysOffAvailableForExtendedPreferences.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.labelDaysOffAvailableForExtendedPreferences.Location = new System.Drawing.Point(3, 8);
			this.labelDaysOffAvailableForExtendedPreferences.Name = "labelDaysOffAvailableForExtendedPreferences";
			this.labelDaysOffAvailableForExtendedPreferences.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
			this.labelDaysOffAvailableForExtendedPreferences.Size = new System.Drawing.Size(546, 39);
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
			this.panel2.Location = new System.Drawing.Point(3, 1318);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(959, 34);
			this.panel2.TabIndex = 1;
			// 
			// labelAllowedPreferenceActivity
			// 
			this.labelAllowedPreferenceActivity.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelAllowedPreferenceActivity.AutoSize = true;
			this.labelAllowedPreferenceActivity.BackColor = System.Drawing.Color.Transparent;
			this.labelAllowedPreferenceActivity.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelAllowedPreferenceActivity.ForeColor = System.Drawing.Color.GhostWhite;
			this.labelAllowedPreferenceActivity.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.labelAllowedPreferenceActivity.Location = new System.Drawing.Point(3, 8);
			this.labelAllowedPreferenceActivity.Name = "labelAllowedPreferenceActivity";
			this.labelAllowedPreferenceActivity.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
			this.labelAllowedPreferenceActivity.Size = new System.Drawing.Size(529, 39);
			this.labelAllowedPreferenceActivity.TabIndex = 0;
			this.labelAllowedPreferenceActivity.Text = "xxActivityAvailableForExtendedPreference";
			this.labelAllowedPreferenceActivity.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// comboBoxAdvAllowedPreferenceActivity
			// 
			this.comboBoxAdvAllowedPreferenceActivity.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.comboBoxAdvAllowedPreferenceActivity.BackColor = System.Drawing.Color.White;
			this.comboBoxAdvAllowedPreferenceActivity.BeforeTouchSize = new System.Drawing.Size(199, 19);
			this.comboBoxAdvAllowedPreferenceActivity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAdvAllowedPreferenceActivity.Location = new System.Drawing.Point(3, 1362);
			this.comboBoxAdvAllowedPreferenceActivity.Name = "comboBoxAdvAllowedPreferenceActivity";
			this.comboBoxAdvAllowedPreferenceActivity.Size = new System.Drawing.Size(199, 19);
			this.comboBoxAdvAllowedPreferenceActivity.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.comboBoxAdvAllowedPreferenceActivity.TabIndex = 15;
			this.comboBoxAdvAllowedPreferenceActivity.SelectedIndexChanged += new System.EventHandler(this.comboBoxAdvAllowedPreferenceActivity_SelectedIndexChanged);
			// 
			// panelBasic
			// 
			this.panelBasic.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(191)))), ((int)(((byte)(234)))));
			this.tableLayoutPanelBasic.SetColumnSpan(this.panelBasic, 2);
			this.panelBasic.Controls.Add(this.labelBasic);
			this.panelBasic.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelBasic.Location = new System.Drawing.Point(3, 153);
			this.panelBasic.Name = "panelBasic";
			this.panelBasic.Size = new System.Drawing.Size(959, 34);
			this.panelBasic.TabIndex = 3;
			// 
			// labelBasic
			// 
			this.labelBasic.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelBasic.AutoSize = true;
			this.labelBasic.BackColor = System.Drawing.Color.Transparent;
			this.labelBasic.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelBasic.ForeColor = System.Drawing.Color.GhostWhite;
			this.labelBasic.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.labelBasic.Location = new System.Drawing.Point(3, 8);
			this.labelBasic.Name = "labelBasic";
			this.labelBasic.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
			this.labelBasic.Size = new System.Drawing.Size(104, 39);
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
			this.tableLayoutPanelBasicSchedule.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelBasicSchedule.Location = new System.Drawing.Point(0, 190);
			this.tableLayoutPanelBasicSchedule.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanelBasicSchedule.Name = "tableLayoutPanelBasicSchedule";
			this.tableLayoutPanelBasicSchedule.RowCount = 3;
			this.tableLayoutPanelBasicSchedule.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelBasicSchedule.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelBasicSchedule.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelBasicSchedule.Size = new System.Drawing.Size(965, 70);
			this.tableLayoutPanelBasicSchedule.TabIndex = 4;
			// 
			// labelWriteProtect
			// 
			this.labelWriteProtect.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelWriteProtect.AutoSize = true;
			this.labelWriteProtect.Location = new System.Drawing.Point(3, 0);
			this.labelWriteProtect.Name = "labelWriteProtect";
			this.labelWriteProtect.Size = new System.Drawing.Size(410, 35);
			this.labelWriteProtect.TabIndex = 0;
			this.labelWriteProtect.Text = "xxWriteProtectScheduledOlderThanDaysColon";
			// 
			// integerTextBoxWriteProtect
			// 
			this.integerTextBoxWriteProtect.Location = new System.Drawing.Point(422, 3);
			this.integerTextBoxWriteProtect.MaxLength = 3;
			this.integerTextBoxWriteProtect.Name = "integerTextBoxWriteProtect";
			this.integerTextBoxWriteProtect.Size = new System.Drawing.Size(133, 39);
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
			this.labelPublishSchedules.Size = new System.Drawing.Size(310, 32);
			this.labelPublishSchedules.TabIndex = 2;
			this.labelPublishSchedules.Text = "xxPublishSchedulesToColon";
			// 
			// dateTimePickerAdvPublishedTo
			// 
			this.dateTimePickerAdvPublishedTo.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.dateTimePickerAdvPublishedTo.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
			this.dateTimePickerAdvPublishedTo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			// 
			// 
			// 
			this.dateTimePickerAdvPublishedTo.Calendar.AllowMultipleSelection = false;
			this.dateTimePickerAdvPublishedTo.Calendar.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.dateTimePickerAdvPublishedTo.Calendar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.dateTimePickerAdvPublishedTo.Calendar.BottomHeight = 25;
			this.dateTimePickerAdvPublishedTo.Calendar.Culture = new System.Globalization.CultureInfo("sv-SE");
			this.dateTimePickerAdvPublishedTo.Calendar.Cursor = System.Windows.Forms.Cursors.Default;
			this.dateTimePickerAdvPublishedTo.Calendar.DayNamesColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvPublishedTo.Calendar.DayNamesFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dateTimePickerAdvPublishedTo.Calendar.DaysFont = new System.Drawing.Font("Segoe UI", 9F);
			this.dateTimePickerAdvPublishedTo.Calendar.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dateTimePickerAdvPublishedTo.Calendar.ForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvPublishedTo.Calendar.GridLines = Syncfusion.Windows.Forms.Grid.GridBorderStyle.None;
			this.dateTimePickerAdvPublishedTo.Calendar.HeaderEndColor = System.Drawing.Color.White;
			this.dateTimePickerAdvPublishedTo.Calendar.HeaderStartColor = System.Drawing.Color.White;
			this.dateTimePickerAdvPublishedTo.Calendar.HeadForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvPublishedTo.Calendar.HighlightColor = System.Drawing.Color.White;
			this.dateTimePickerAdvPublishedTo.Calendar.Iso8601CalenderFormat = false;
			this.dateTimePickerAdvPublishedTo.Calendar.Location = new System.Drawing.Point(0, 0);
			this.dateTimePickerAdvPublishedTo.Calendar.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvPublishedTo.Calendar.Name = "monthCalendar";
			this.dateTimePickerAdvPublishedTo.Calendar.ScrollButtonSize = new System.Drawing.Size(24, 24);
			this.dateTimePickerAdvPublishedTo.Calendar.SelectedDates = new System.DateTime[0];
			this.dateTimePickerAdvPublishedTo.Calendar.ShowWeekNumbers = true;
			this.dateTimePickerAdvPublishedTo.Calendar.Size = new System.Drawing.Size(132, 174);
			this.dateTimePickerAdvPublishedTo.Calendar.SizeToFit = true;
			this.dateTimePickerAdvPublishedTo.Calendar.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.dateTimePickerAdvPublishedTo.Calendar.SynchronousPopup = false;
			this.dateTimePickerAdvPublishedTo.Calendar.TabIndex = 0;
			this.dateTimePickerAdvPublishedTo.Calendar.ThemedEnabledScrollButtons = false;
			this.dateTimePickerAdvPublishedTo.Calendar.WeekFont = new System.Drawing.Font("Verdana", 8F);
			this.dateTimePickerAdvPublishedTo.Calendar.WeekInterior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.PeachPuff, System.Drawing.Color.AntiqueWhite);
			// 
			// 
			// 
			this.dateTimePickerAdvPublishedTo.Calendar.NoneButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.dateTimePickerAdvPublishedTo.Calendar.NoneButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvPublishedTo.Calendar.NoneButton.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.dateTimePickerAdvPublishedTo.Calendar.NoneButton.ForeColor = System.Drawing.Color.White;
			this.dateTimePickerAdvPublishedTo.Calendar.NoneButton.IsBackStageButton = false;
			this.dateTimePickerAdvPublishedTo.Calendar.NoneButton.Location = new System.Drawing.Point(48, 0);
			this.dateTimePickerAdvPublishedTo.Calendar.NoneButton.Size = new System.Drawing.Size(84, 25);
			this.dateTimePickerAdvPublishedTo.Calendar.NoneButton.Text = "None";
			this.dateTimePickerAdvPublishedTo.Calendar.NoneButton.UseVisualStyle = true;
			// 
			// 
			// 
			this.dateTimePickerAdvPublishedTo.Calendar.TodayButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.dateTimePickerAdvPublishedTo.Calendar.TodayButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvPublishedTo.Calendar.TodayButton.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.dateTimePickerAdvPublishedTo.Calendar.TodayButton.ForeColor = System.Drawing.Color.White;
			this.dateTimePickerAdvPublishedTo.Calendar.TodayButton.IsBackStageButton = false;
			this.dateTimePickerAdvPublishedTo.Calendar.TodayButton.Location = new System.Drawing.Point(0, 0);
			this.dateTimePickerAdvPublishedTo.Calendar.TodayButton.Size = new System.Drawing.Size(48, 25);
			this.dateTimePickerAdvPublishedTo.Calendar.TodayButton.Text = "Today";
			this.dateTimePickerAdvPublishedTo.Calendar.TodayButton.UseVisualStyle = true;
			this.dateTimePickerAdvPublishedTo.CalendarFont = null;
			this.dateTimePickerAdvPublishedTo.CalendarSize = new System.Drawing.Size(220, 176);
			this.dateTimePickerAdvPublishedTo.CalendarTitleForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvPublishedTo.DropDownImage = null;
			this.dateTimePickerAdvPublishedTo.DropDownNormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvPublishedTo.DropDownPressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvPublishedTo.DropDownSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(179)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvPublishedTo.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			this.dateTimePickerAdvPublishedTo.Format = System.Windows.Forms.DateTimePickerFormat.Short;
			this.dateTimePickerAdvPublishedTo.Location = new System.Drawing.Point(422, 42);
			this.dateTimePickerAdvPublishedTo.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvPublishedTo.MinValue = new System.DateTime(((long)(0)));
			this.dateTimePickerAdvPublishedTo.Name = "dateTimePickerAdvPublishedTo";
			this.dateTimePickerAdvPublishedTo.NullString = "xxNotPublished";
			this.dateTimePickerAdvPublishedTo.ShowCheckBox = false;
			this.dateTimePickerAdvPublishedTo.Size = new System.Drawing.Size(134, 21);
			this.dateTimePickerAdvPublishedTo.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.dateTimePickerAdvPublishedTo.TabIndex = 6;
			this.dateTimePickerAdvPublishedTo.ThemesEnabled = true;
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
			this.tableLayoutPanelOpenPreference.Controls.Add(this.labelPreferencePeriod, 0, 0);
			this.tableLayoutPanelOpenPreference.Controls.Add(this.labelIsOpen, 1, 0);
			this.tableLayoutPanelOpenPreference.Controls.Add(this.dateSelectionFromToPreferencePeriod, 0, 1);
			this.tableLayoutPanelOpenPreference.Controls.Add(this.dateSelectionFromToIsOpen, 1, 1);
			this.tableLayoutPanelOpenPreference.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelOpenPreference.Location = new System.Drawing.Point(0, 490);
			this.tableLayoutPanelOpenPreference.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanelOpenPreference.Name = "tableLayoutPanelOpenPreference";
			this.tableLayoutPanelOpenPreference.RowCount = 2;
			this.tableLayoutPanelOpenPreference.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 18.49315F));
			this.tableLayoutPanelOpenPreference.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 81.50685F));
			this.tableLayoutPanelOpenPreference.Size = new System.Drawing.Size(965, 150);
			this.tableLayoutPanelOpenPreference.TabIndex = 6;
			// 
			// labelPreferencePeriod
			// 
			this.labelPreferencePeriod.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelPreferencePeriod.AutoSize = true;
			this.labelPreferencePeriod.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			this.labelPreferencePeriod.Location = new System.Drawing.Point(3, 0);
			this.labelPreferencePeriod.Name = "labelPreferencePeriod";
			this.labelPreferencePeriod.Size = new System.Drawing.Size(236, 27);
			this.labelPreferencePeriod.TabIndex = 0;
			this.labelPreferencePeriod.Text = "xxPreferencePeriod";
			// 
			// labelIsOpen
			// 
			this.labelIsOpen.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelIsOpen.AutoSize = true;
			this.labelIsOpen.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			this.labelIsOpen.Location = new System.Drawing.Point(420, 0);
			this.labelIsOpen.Name = "labelIsOpen";
			this.labelIsOpen.Size = new System.Drawing.Size(121, 27);
			this.labelIsOpen.TabIndex = 1;
			this.labelIsOpen.Text = "xxIsOpen";
			// 
			// dateSelectionFromToPreferencePeriod
			// 
			this.dateSelectionFromToPreferencePeriod.BackColor = System.Drawing.Color.Transparent;
			this.dateSelectionFromToPreferencePeriod.ButtonApplyText = "xxApply";
			this.dateSelectionFromToPreferencePeriod.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			this.dateSelectionFromToPreferencePeriod.HideNoneButtons = true;
			this.dateSelectionFromToPreferencePeriod.LabelDateSelectionText = "xxFrom";
			this.dateSelectionFromToPreferencePeriod.LabelDateSelectionToText = "xxTo";
			this.dateSelectionFromToPreferencePeriod.Location = new System.Drawing.Point(3, 29);
			this.dateSelectionFromToPreferencePeriod.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.dateSelectionFromToPreferencePeriod.Name = "dateSelectionFromToPreferencePeriod";
			this.dateSelectionFromToPreferencePeriod.NoneButtonText = "xxNone";
			this.dateSelectionFromToPreferencePeriod.NullString = "xxNoDateIsSelected";
			this.dateSelectionFromToPreferencePeriod.ShowApplyButton = false;
			this.dateSelectionFromToPreferencePeriod.Size = new System.Drawing.Size(187, 115);
			this.dateSelectionFromToPreferencePeriod.TabIndex = 7;
			this.dateSelectionFromToPreferencePeriod.TodayButtonText = "xxToday";
			this.dateSelectionFromToPreferencePeriod.WorkPeriodEnd = new Teleopti.Ccc.Domain.InterfaceLegacy.Domain.DateOnly(2018, 1, 1);
			this.dateSelectionFromToPreferencePeriod.WorkPeriodStart = new Teleopti.Ccc.Domain.InterfaceLegacy.Domain.DateOnly(2018, 1, 1);
			this.dateSelectionFromToPreferencePeriod.Validating += new System.ComponentModel.CancelEventHandler(this.dateSelectionFromToPreferencePeriod_Validating);
			this.dateSelectionFromToPreferencePeriod.Validated += new System.EventHandler(this.dateSelectionFromToPreferencePeriod_Validated);
			// 
			// dateSelectionFromToIsOpen
			// 
			this.dateSelectionFromToIsOpen.BackColor = System.Drawing.Color.Transparent;
			this.dateSelectionFromToIsOpen.ButtonApplyText = "xxApply";
			this.dateSelectionFromToIsOpen.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			this.dateSelectionFromToIsOpen.HideNoneButtons = true;
			this.dateSelectionFromToIsOpen.LabelDateSelectionText = "xxFrom";
			this.dateSelectionFromToIsOpen.LabelDateSelectionToText = "xxTo";
			this.dateSelectionFromToIsOpen.Location = new System.Drawing.Point(420, 29);
			this.dateSelectionFromToIsOpen.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.dateSelectionFromToIsOpen.Name = "dateSelectionFromToIsOpen";
			this.dateSelectionFromToIsOpen.NoneButtonText = "xxNone";
			this.dateSelectionFromToIsOpen.NullString = "xxNoDateIsSelected";
			this.dateSelectionFromToIsOpen.ShowApplyButton = false;
			this.dateSelectionFromToIsOpen.Size = new System.Drawing.Size(187, 115);
			this.dateSelectionFromToIsOpen.TabIndex = 8;
			this.dateSelectionFromToIsOpen.TodayButtonText = "xxToday";
//			this.dateSelectionFromToIsOpen.WorkPeriodEnd = ((Teleopti.Ccc.Domain.InterfaceLegacy.Domain.DateOnly)(resources.GetObject("dateSelectionFromToIsOpen.WorkPeriodEnd")));
//			this.dateSelectionFromToIsOpen.WorkPeriodStart = ((Teleopti.Ccc.Domain.InterfaceLegacy.Domain.DateOnly)(resources.GetObject("dateSelectionFromToIsOpen.WorkPeriodStart")));
			this.dateSelectionFromToIsOpen.Validating += new System.ComponentModel.CancelEventHandler(this.dateSelectionFromToIsOpen_Validating);
			this.dateSelectionFromToIsOpen.Validated += new System.EventHandler(this.dateSelectionFromToIsOpen_Validated);
			// 
			// panelOpenPreference
			// 
			this.panelOpenPreference.BackColor = System.Drawing.Color.LightSteelBlue;
			this.tableLayoutPanelBasic.SetColumnSpan(this.panelOpenPreference, 2);
			this.panelOpenPreference.Controls.Add(this.labelOpenPreference);
			this.panelOpenPreference.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelOpenPreference.Location = new System.Drawing.Point(3, 453);
			this.panelOpenPreference.Name = "panelOpenPreference";
			this.panelOpenPreference.Size = new System.Drawing.Size(959, 34);
			this.panelOpenPreference.TabIndex = 5;
			// 
			// labelOpenPreference
			// 
			this.labelOpenPreference.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelOpenPreference.AutoSize = true;
			this.labelOpenPreference.BackColor = System.Drawing.Color.Transparent;
			this.labelOpenPreference.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelOpenPreference.ForeColor = System.Drawing.Color.GhostWhite;
			this.labelOpenPreference.Location = new System.Drawing.Point(3, 8);
			this.labelOpenPreference.Name = "labelOpenPreference";
			this.labelOpenPreference.Size = new System.Drawing.Size(250, 36);
			this.labelOpenPreference.TabIndex = 0;
			this.labelOpenPreference.Text = "xxOpenPreferences";
			// 
			// panel6
			// 
			this.panel6.BackColor = System.Drawing.Color.LightSteelBlue;
			this.tableLayoutPanelBasic.SetColumnSpan(this.panel6, 2);
			this.panel6.Controls.Add(this.label4);
			this.panel6.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel6.Location = new System.Drawing.Point(3, 1618);
			this.panel6.Name = "panel6";
			this.panel6.Size = new System.Drawing.Size(959, 34);
			this.panel6.TabIndex = 16;
			// 
			// label4
			// 
			this.label4.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label4.AutoSize = true;
			this.label4.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label4.ForeColor = System.Drawing.Color.GhostWhite;
			this.label4.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.label4.Location = new System.Drawing.Point(3, 8);
			this.label4.Name = "label4";
			this.label4.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
			this.label4.Size = new System.Drawing.Size(461, 39);
			this.label4.TabIndex = 0;
			this.label4.Text = "xxFairnessSystemUsedForScheduling";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// radioButtonAdvFairnessEqual
			// 
			this.radioButtonAdvFairnessEqual.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.radioButtonAdvFairnessEqual.BeforeTouchSize = new System.Drawing.Size(932, 24);
			this.tableLayoutPanelBasic.SetColumnSpan(this.radioButtonAdvFairnessEqual, 2);
			this.radioButtonAdvFairnessEqual.ImageCheckBoxSize = new System.Drawing.Size(26, 26);
			this.radioButtonAdvFairnessEqual.Location = new System.Drawing.Point(3, 1660);
			this.radioButtonAdvFairnessEqual.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.radioButtonAdvFairnessEqual.Name = "radioButtonAdvFairnessEqual";
			this.radioButtonAdvFairnessEqual.Size = new System.Drawing.Size(932, 24);
			this.radioButtonAdvFairnessEqual.TabIndex = 18;
			this.radioButtonAdvFairnessEqual.TabStop = false;
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
			this.panel7.Location = new System.Drawing.Point(3, 263);
			this.panel7.Name = "panel7";
			this.panel7.Size = new System.Drawing.Size(959, 34);
			this.panel7.TabIndex = 19;
			// 
			// labelOpenStudentAvailability
			// 
			this.labelOpenStudentAvailability.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelOpenStudentAvailability.AutoSize = true;
			this.labelOpenStudentAvailability.BackColor = System.Drawing.Color.Transparent;
			this.labelOpenStudentAvailability.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelOpenStudentAvailability.ForeColor = System.Drawing.Color.GhostWhite;
			this.labelOpenStudentAvailability.Location = new System.Drawing.Point(3, 8);
			this.labelOpenStudentAvailability.Name = "labelOpenStudentAvailability";
			this.labelOpenStudentAvailability.Size = new System.Drawing.Size(339, 36);
			this.labelOpenStudentAvailability.TabIndex = 1;
			this.labelOpenStudentAvailability.Text = "xxOpenStudentAvailability";
			// 
			// radioButtonAdvSeniority
			// 
			this.radioButtonAdvSeniority.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.radioButtonAdvSeniority.BeforeTouchSize = new System.Drawing.Size(932, 24);
			this.tableLayoutPanelBasic.SetColumnSpan(this.radioButtonAdvSeniority, 2);
			this.radioButtonAdvSeniority.ImageCheckBoxSize = new System.Drawing.Size(26, 26);
			this.radioButtonAdvSeniority.Location = new System.Drawing.Point(3, 1695);
			this.radioButtonAdvSeniority.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.radioButtonAdvSeniority.Name = "radioButtonAdvSeniority";
			this.radioButtonAdvSeniority.Size = new System.Drawing.Size(932, 24);
			this.radioButtonAdvSeniority.TabIndex = 25;
			this.radioButtonAdvSeniority.TabStop = false;
			this.radioButtonAdvSeniority.Text = "xxSeniority";
			this.radioButtonAdvSeniority.ThemesEnabled = true;
			this.radioButtonAdvSeniority.CheckChanged += new System.EventHandler(this.radioButtonAdvSeniorityCheckChanged);
			// 
			// tabPageAdvAbsenceRequests
			// 
			this.tabPageAdvAbsenceRequests.Controls.Add(this.tableLayoutPanelAbsenceRequestPeriods);
			this.tabPageAdvAbsenceRequests.Image = null;
			this.tabPageAdvAbsenceRequests.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageAdvAbsenceRequests.Location = new System.Drawing.Point(1, 45);
			this.tabPageAdvAbsenceRequests.Name = "tabPageAdvAbsenceRequests";
			this.tabPageAdvAbsenceRequests.ShowCloseButton = true;
			this.tabPageAdvAbsenceRequests.Size = new System.Drawing.Size(965, 410);
			this.tabPageAdvAbsenceRequests.TabBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(191)))), ((int)(((byte)(234)))));
			this.tabPageAdvAbsenceRequests.TabIndex = 1;
			this.tabPageAdvAbsenceRequests.Text = "xxAbsenceRequests";
			this.tabPageAdvAbsenceRequests.ThemesEnabled = false;
			// 
			// tableLayoutPanelAbsenceRequestPeriods
			// 
			this.tableLayoutPanelAbsenceRequestPeriods.AutoScroll = true;
			this.tableLayoutPanelAbsenceRequestPeriods.AutoSize = true;
			this.tableLayoutPanelAbsenceRequestPeriods.ColumnCount = 1;
			this.tableLayoutPanelAbsenceRequestPeriods.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelAbsenceRequestPeriods.Controls.Add(this.panel1, 0, 0);
			this.tableLayoutPanelAbsenceRequestPeriods.Controls.Add(this.gridControlVisualisation, 0, 1);
			this.tableLayoutPanelAbsenceRequestPeriods.Controls.Add(this.tableLayoutPanelNextPreviousPeriod, 0, 2);
			this.tableLayoutPanelAbsenceRequestPeriods.Controls.Add(this.tableLayoutPanelAbsenceRequestMiscellaneous, 0, 3);
			this.tableLayoutPanelAbsenceRequestPeriods.Controls.Add(this.tableLayoutPanelWaitlist, 0, 4);
			this.tableLayoutPanelAbsenceRequestPeriods.Controls.Add(this.tableLayoutPanelAbsenceProbability, 0, 5);
			this.tableLayoutPanelAbsenceRequestPeriods.Controls.Add(this.tableLayoutPanelAbsenceRequestCancellation, 0, 6);
			this.tableLayoutPanelAbsenceRequestPeriods.Controls.Add(this.tableLayoutPanelAbsenceRequestExpiration, 0, 7);
			this.tableLayoutPanelAbsenceRequestPeriods.Controls.Add(this.tableLayoutPanelOpenForAbsenceRequests, 0, 8);
			this.tableLayoutPanelAbsenceRequestPeriods.Controls.Add(this.gridControlAbsenceRequestOpenPeriods, 0, 9);
			this.tableLayoutPanelAbsenceRequestPeriods.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelAbsenceRequestPeriods.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanelAbsenceRequestPeriods.Name = "tableLayoutPanelAbsenceRequestPeriods";
			this.tableLayoutPanelAbsenceRequestPeriods.RowCount = 10;
			this.tableLayoutPanelAbsenceRequestPeriods.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelAbsenceRequestPeriods.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 115F));
			this.tableLayoutPanelAbsenceRequestPeriods.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelAbsenceRequestPeriods.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanelAbsenceRequestPeriods.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelAbsenceRequestPeriods.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelAbsenceRequestPeriods.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelAbsenceRequestPeriods.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelAbsenceRequestPeriods.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanelAbsenceRequestPeriods.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 150F));
			this.tableLayoutPanelAbsenceRequestPeriods.Size = new System.Drawing.Size(965, 410);
			this.tableLayoutPanelAbsenceRequestPeriods.TabIndex = 0;
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.LightSteelBlue;
			this.panel1.Controls.Add(this.dateTimePickerAdvViewpoint);
			this.panel1.Controls.Add(this.labelAbsenceRequestsVisualisation);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(3, 3);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(959, 34);
			this.panel1.TabIndex = 0;
			// 
			// dateTimePickerAdvViewpoint
			// 
			this.dateTimePickerAdvViewpoint.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.dateTimePickerAdvViewpoint.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
			this.dateTimePickerAdvViewpoint.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			// 
			// 
			// 
			this.dateTimePickerAdvViewpoint.Calendar.AllowMultipleSelection = false;
			this.dateTimePickerAdvViewpoint.Calendar.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.dateTimePickerAdvViewpoint.Calendar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.dateTimePickerAdvViewpoint.Calendar.BottomHeight = 25;
			this.dateTimePickerAdvViewpoint.Calendar.Culture = new System.Globalization.CultureInfo("en-US");
			this.dateTimePickerAdvViewpoint.Calendar.DayNamesColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvViewpoint.Calendar.DayNamesFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dateTimePickerAdvViewpoint.Calendar.DaysFont = new System.Drawing.Font("Segoe UI", 9F);
			this.dateTimePickerAdvViewpoint.Calendar.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dateTimePickerAdvViewpoint.Calendar.ForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvViewpoint.Calendar.GridLines = Syncfusion.Windows.Forms.Grid.GridBorderStyle.None;
			this.dateTimePickerAdvViewpoint.Calendar.HeaderEndColor = System.Drawing.Color.White;
			this.dateTimePickerAdvViewpoint.Calendar.HeaderStartColor = System.Drawing.Color.White;
			this.dateTimePickerAdvViewpoint.Calendar.HeadForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvViewpoint.Calendar.HighlightColor = System.Drawing.Color.White;
			this.dateTimePickerAdvViewpoint.Calendar.Iso8601CalenderFormat = false;
			this.dateTimePickerAdvViewpoint.Calendar.Location = new System.Drawing.Point(0, 0);
			this.dateTimePickerAdvViewpoint.Calendar.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvViewpoint.Calendar.Name = "monthCalendar";
			this.dateTimePickerAdvViewpoint.Calendar.ScrollButtonSize = new System.Drawing.Size(24, 24);
			this.dateTimePickerAdvViewpoint.Calendar.SelectedDates = new System.DateTime[0];
			this.dateTimePickerAdvViewpoint.Calendar.ShowWeekNumbers = true;
			this.dateTimePickerAdvViewpoint.Calendar.Size = new System.Drawing.Size(115, 174);
			this.dateTimePickerAdvViewpoint.Calendar.SizeToFit = true;
			this.dateTimePickerAdvViewpoint.Calendar.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.dateTimePickerAdvViewpoint.Calendar.SynchronousPopup = false;
			this.dateTimePickerAdvViewpoint.Calendar.TabIndex = 0;
			this.dateTimePickerAdvViewpoint.Calendar.WeekFont = new System.Drawing.Font("Segoe UI", 9F);
			this.dateTimePickerAdvViewpoint.Calendar.WeekInterior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.PeachPuff, System.Drawing.Color.AntiqueWhite);
			// 
			// 
			// 
			this.dateTimePickerAdvViewpoint.Calendar.NoneButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.dateTimePickerAdvViewpoint.Calendar.NoneButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvViewpoint.Calendar.NoneButton.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.dateTimePickerAdvViewpoint.Calendar.NoneButton.ForeColor = System.Drawing.Color.White;
			this.dateTimePickerAdvViewpoint.Calendar.NoneButton.IsBackStageButton = false;
			this.dateTimePickerAdvViewpoint.Calendar.NoneButton.Location = new System.Drawing.Point(125, 0);
			this.dateTimePickerAdvViewpoint.Calendar.NoneButton.Size = new System.Drawing.Size(84, 25);
			this.dateTimePickerAdvViewpoint.Calendar.NoneButton.Text = "None";
			this.dateTimePickerAdvViewpoint.Calendar.NoneButton.UseVisualStyle = true;
			this.dateTimePickerAdvViewpoint.Calendar.NoneButton.Visible = false;
			// 
			// 
			// 
			this.dateTimePickerAdvViewpoint.Calendar.TodayButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.dateTimePickerAdvViewpoint.Calendar.TodayButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvViewpoint.Calendar.TodayButton.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.dateTimePickerAdvViewpoint.Calendar.TodayButton.ForeColor = System.Drawing.Color.White;
			this.dateTimePickerAdvViewpoint.Calendar.TodayButton.IsBackStageButton = false;
			this.dateTimePickerAdvViewpoint.Calendar.TodayButton.Location = new System.Drawing.Point(0, 0);
			this.dateTimePickerAdvViewpoint.Calendar.TodayButton.Size = new System.Drawing.Size(115, 25);
			this.dateTimePickerAdvViewpoint.Calendar.TodayButton.Text = "Today";
			this.dateTimePickerAdvViewpoint.Calendar.TodayButton.UseVisualStyle = true;
			this.dateTimePickerAdvViewpoint.CalendarFont = null;
			this.dateTimePickerAdvViewpoint.CalendarSize = new System.Drawing.Size(189, 176);
			this.dateTimePickerAdvViewpoint.CalendarTitleForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvViewpoint.DropDownImage = null;
			this.dateTimePickerAdvViewpoint.DropDownNormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvViewpoint.DropDownPressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvViewpoint.DropDownSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(179)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvViewpoint.EnableNullDate = false;
			this.dateTimePickerAdvViewpoint.EnableNullKeys = false;
			this.dateTimePickerAdvViewpoint.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			this.dateTimePickerAdvViewpoint.Format = System.Windows.Forms.DateTimePickerFormat.Short;
			this.dateTimePickerAdvViewpoint.Location = new System.Drawing.Point(561, 7);
			this.dateTimePickerAdvViewpoint.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvViewpoint.MinValue = new System.DateTime(((long)(0)));
			this.dateTimePickerAdvViewpoint.Name = "dateTimePickerAdvViewpoint";
			this.dateTimePickerAdvViewpoint.NoneButtonVisible = false;
			this.dateTimePickerAdvViewpoint.ShowCheckBox = false;
			this.dateTimePickerAdvViewpoint.Size = new System.Drawing.Size(117, 21);
			this.dateTimePickerAdvViewpoint.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.dateTimePickerAdvViewpoint.TabIndex = 20;
			this.dateTimePickerAdvViewpoint.ThemesEnabled = true;
			this.dateTimePickerAdvViewpoint.UseCurrentCulture = true;
			this.dateTimePickerAdvViewpoint.Value = new System.DateTime(2010, 4, 26, 14, 17, 36, 416);
			this.dateTimePickerAdvViewpoint.ValueChanged += new System.EventHandler(this.dateTimePickerAdvViewpoint_ValueChanged);
			// 
			// labelAbsenceRequestsVisualisation
			// 
			this.labelAbsenceRequestsVisualisation.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelAbsenceRequestsVisualisation.AutoSize = true;
			this.labelAbsenceRequestsVisualisation.BackColor = System.Drawing.Color.Transparent;
			this.labelAbsenceRequestsVisualisation.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelAbsenceRequestsVisualisation.ForeColor = System.Drawing.Color.GhostWhite;
			this.labelAbsenceRequestsVisualisation.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.labelAbsenceRequestsVisualisation.Location = new System.Drawing.Point(3, 8);
			this.labelAbsenceRequestsVisualisation.Name = "labelAbsenceRequestsVisualisation";
			this.labelAbsenceRequestsVisualisation.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
			this.labelAbsenceRequestsVisualisation.Size = new System.Drawing.Size(196, 39);
			this.labelAbsenceRequestsVisualisation.TabIndex = 0;
			this.labelAbsenceRequestsVisualisation.Text = "xxVisualisation";
			this.labelAbsenceRequestsVisualisation.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// gridControlVisualisation
			// 
			this.gridControlVisualisation.AlphaBlendSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(94)))), ((int)(((byte)(171)))), ((int)(((byte)(222)))));
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
			this.gridControlVisualisation.BaseStylesMap.AddRange(new Syncfusion.Windows.Forms.Grid.GridBaseStyle[] {
			gridBaseStyle1,
			gridBaseStyle2,
			gridBaseStyle3,
			gridBaseStyle4});
			this.gridControlVisualisation.ColCount = 2;
			this.gridControlVisualisation.ColWidthEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridColWidth[] {
			new Syncfusion.Windows.Forms.Grid.GridColWidth(0, 35)});
			this.gridControlVisualisation.DefaultGridBorderStyle = Syncfusion.Windows.Forms.Grid.GridBorderStyle.Solid;
			this.gridControlVisualisation.DefaultRowHeight = 20;
			this.gridControlVisualisation.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridControlVisualisation.GridOfficeScrollBars = Syncfusion.Windows.Forms.OfficeScrollBars.Office2007;
			this.gridControlVisualisation.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Metro;
			this.gridControlVisualisation.HScrollPixel = true;
			this.gridControlVisualisation.Location = new System.Drawing.Point(3, 43);
			this.gridControlVisualisation.Name = "gridControlVisualisation";
			this.gridControlVisualisation.NumberedColHeaders = false;
			this.gridControlVisualisation.NumberedRowHeaders = false;
			this.gridControlVisualisation.Office2007ScrollBars = true;
			this.gridControlVisualisation.Properties.BackgroundColor = System.Drawing.SystemColors.Window;
			this.gridControlVisualisation.Properties.ForceImmediateRepaint = false;
			this.gridControlVisualisation.Properties.GridLineColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(212)))), ((int)(((byte)(212)))));
			this.gridControlVisualisation.Properties.MarkColHeader = false;
			this.gridControlVisualisation.Properties.MarkRowHeader = false;
			this.gridControlVisualisation.RowCount = 0;
			this.gridControlVisualisation.RowHeightEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridRowHeight[] {
			new Syncfusion.Windows.Forms.Grid.GridRowHeight(0, 29)});
			this.gridControlVisualisation.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
			this.gridControlVisualisation.Size = new System.Drawing.Size(959, 115);
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
			this.tableLayoutPanelNextPreviousPeriod.Location = new System.Drawing.Point(732, 98);
			this.tableLayoutPanelNextPreviousPeriod.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanelNextPreviousPeriod.Name = "tableLayoutPanelNextPreviousPeriod";
			this.tableLayoutPanelNextPreviousPeriod.RowCount = 1;
			this.tableLayoutPanelNextPreviousPeriod.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanelNextPreviousPeriod.Size = new System.Drawing.Size(233, 32);
			this.tableLayoutPanelNextPreviousPeriod.TabIndex = 0;
			// 
			// buttonAdvPreviousProjectionPeriod
			// 
			this.buttonAdvPreviousProjectionPeriod.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.buttonAdvPreviousProjectionPeriod.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvPreviousProjectionPeriod.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvPreviousProjectionPeriod.BeforeTouchSize = new System.Drawing.Size(87, 25);
			this.buttonAdvPreviousProjectionPeriod.ForeColor = System.Drawing.Color.White;
			this.buttonAdvPreviousProjectionPeriod.IsBackStageButton = false;
			this.buttonAdvPreviousProjectionPeriod.Location = new System.Drawing.Point(0, 3);
			this.buttonAdvPreviousProjectionPeriod.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
			this.buttonAdvPreviousProjectionPeriod.Name = "buttonAdvPreviousProjectionPeriod";
			this.buttonAdvPreviousProjectionPeriod.Size = new System.Drawing.Size(87, 25);
			this.buttonAdvPreviousProjectionPeriod.TabIndex = 22;
			this.buttonAdvPreviousProjectionPeriod.Text = "<<";
			this.buttonAdvPreviousProjectionPeriod.UseVisualStyle = true;
			this.buttonAdvPreviousProjectionPeriod.Click += new System.EventHandler(this.buttonAdvPreviousProjectionPeriod_Click);
			// 
			// buttonAdvNextProjectionPeriod
			// 
			this.buttonAdvNextProjectionPeriod.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.buttonAdvNextProjectionPeriod.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvNextProjectionPeriod.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvNextProjectionPeriod.BeforeTouchSize = new System.Drawing.Size(87, 25);
			this.buttonAdvNextProjectionPeriod.ForeColor = System.Drawing.Color.White;
			this.buttonAdvNextProjectionPeriod.IsBackStageButton = false;
			this.buttonAdvNextProjectionPeriod.Location = new System.Drawing.Point(143, 3);
			this.buttonAdvNextProjectionPeriod.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
			this.buttonAdvNextProjectionPeriod.Name = "buttonAdvNextProjectionPeriod";
			this.buttonAdvNextProjectionPeriod.Size = new System.Drawing.Size(87, 25);
			this.buttonAdvNextProjectionPeriod.TabIndex = 23;
			this.buttonAdvNextProjectionPeriod.Text = ">>";
			this.buttonAdvNextProjectionPeriod.UseVisualStyle = true;
			this.buttonAdvNextProjectionPeriod.Click += new System.EventHandler(this.buttonAdvNextProjectionPeriod_Click);
			// 
			// tableLayoutPanelAbsenceRequestMiscellaneous
			// 
			this.tableLayoutPanelAbsenceRequestMiscellaneous.BackColor = System.Drawing.Color.LightSteelBlue;
			this.tableLayoutPanelAbsenceRequestMiscellaneous.ColumnCount = 1;
			this.tableLayoutPanelAbsenceRequestMiscellaneous.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelAbsenceRequestMiscellaneous.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanelAbsenceRequestMiscellaneous.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanelAbsenceRequestMiscellaneous.Controls.Add(this.labelAbsenceRequestMiscellaneous, 0, 0);
			this.tableLayoutPanelAbsenceRequestMiscellaneous.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelAbsenceRequestMiscellaneous.Location = new System.Drawing.Point(3, 135);
			this.tableLayoutPanelAbsenceRequestMiscellaneous.Name = "tableLayoutPanelAbsenceRequestMiscellaneous";
			this.tableLayoutPanelAbsenceRequestMiscellaneous.RowCount = 1;
			this.tableLayoutPanelAbsenceRequestMiscellaneous.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelAbsenceRequestMiscellaneous.Size = new System.Drawing.Size(959, 34);
			this.tableLayoutPanelAbsenceRequestMiscellaneous.TabIndex = 0;
			// 
			// labelAbsenceRequestMiscellaneous
			// 
			this.labelAbsenceRequestMiscellaneous.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelAbsenceRequestMiscellaneous.AutoSize = true;
			this.labelAbsenceRequestMiscellaneous.BackColor = System.Drawing.Color.Transparent;
			this.labelAbsenceRequestMiscellaneous.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelAbsenceRequestMiscellaneous.ForeColor = System.Drawing.Color.GhostWhite;
			this.labelAbsenceRequestMiscellaneous.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.labelAbsenceRequestMiscellaneous.Location = new System.Drawing.Point(3, 0);
			this.labelAbsenceRequestMiscellaneous.Name = "labelAbsenceRequestMiscellaneous";
			this.labelAbsenceRequestMiscellaneous.Size = new System.Drawing.Size(213, 36);
			this.labelAbsenceRequestMiscellaneous.TabIndex = 0;
			this.labelAbsenceRequestMiscellaneous.Text = "xxMiscellaneous";
			this.labelAbsenceRequestMiscellaneous.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// tableLayoutPanelWaitlist
			// 
			this.tableLayoutPanelWaitlist.ColumnCount = 4;
			this.tableLayoutPanelWaitlist.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelWaitlist.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelWaitlist.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelWaitlist.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelWaitlist.Controls.Add(this.checkBoxEnableAbsenceRequestWaitlisting, 0, 0);
			this.tableLayoutPanelWaitlist.Controls.Add(this.radioButtonWaitlistFirstComeFirstServed, 1, 0);
			this.tableLayoutPanelWaitlist.Controls.Add(this.radioButtonWaitlistBySeniority, 2, 0);
			this.tableLayoutPanelWaitlist.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelWaitlist.Location = new System.Drawing.Point(3, 175);
			this.tableLayoutPanelWaitlist.Name = "tableLayoutPanelWaitlist";
			this.tableLayoutPanelWaitlist.RowCount = 1;
			this.tableLayoutPanelWaitlist.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelWaitlist.Size = new System.Drawing.Size(959, 29);
			this.tableLayoutPanelWaitlist.TabIndex = 29;
			// 
			// checkBoxEnableAbsenceRequestWaitlisting
			// 
			this.checkBoxEnableAbsenceRequestWaitlisting.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.checkBoxEnableAbsenceRequestWaitlisting.BeforeTouchSize = new System.Drawing.Size(181, 23);
			this.checkBoxEnableAbsenceRequestWaitlisting.ImageCheckBoxSize = new System.Drawing.Size(26, 26);
			this.checkBoxEnableAbsenceRequestWaitlisting.Location = new System.Drawing.Point(3, 3);
			this.checkBoxEnableAbsenceRequestWaitlisting.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxEnableAbsenceRequestWaitlisting.Name = "checkBoxEnableAbsenceRequestWaitlisting";
			this.checkBoxEnableAbsenceRequestWaitlisting.Size = new System.Drawing.Size(181, 23);
			this.checkBoxEnableAbsenceRequestWaitlisting.TabIndex = 28;
			this.checkBoxEnableAbsenceRequestWaitlisting.Text = "xxEnableWaitlisting";
			this.checkBoxEnableAbsenceRequestWaitlisting.ThemesEnabled = false;
			this.checkBoxEnableAbsenceRequestWaitlisting.CheckStateChanged += new System.EventHandler(this.checkBoxEnableAbsenceRequestWaitlisting_CheckStateChanged);
			// 
			// radioButtonWaitlistFirstComeFirstServed
			// 
			this.radioButtonWaitlistFirstComeFirstServed.AutoSize = true;
			this.radioButtonWaitlistFirstComeFirstServed.Location = new System.Drawing.Point(190, 3);
			this.radioButtonWaitlistFirstComeFirstServed.Name = "radioButtonWaitlistFirstComeFirstServed";
			this.radioButtonWaitlistFirstComeFirstServed.Size = new System.Drawing.Size(292, 23);
			this.radioButtonWaitlistFirstComeFirstServed.TabIndex = 29;
			this.radioButtonWaitlistFirstComeFirstServed.TabStop = true;
			this.radioButtonWaitlistFirstComeFirstServed.Text = "xxFirstComeFirstServed";
			this.radioButtonWaitlistFirstComeFirstServed.UseVisualStyleBackColor = true;
			this.radioButtonWaitlistFirstComeFirstServed.Click += new System.EventHandler(this.radioButtonWaitlistFirstComeFirstServed_Click);
			// 
			// radioButtonWaitlistBySeniority
			// 
			this.radioButtonWaitlistBySeniority.AutoSize = true;
			this.radioButtonWaitlistBySeniority.Location = new System.Drawing.Point(488, 3);
			this.radioButtonWaitlistBySeniority.Name = "radioButtonWaitlistBySeniority";
			this.radioButtonWaitlistBySeniority.Size = new System.Drawing.Size(188, 23);
			this.radioButtonWaitlistBySeniority.TabIndex = 30;
			this.radioButtonWaitlistBySeniority.TabStop = true;
			this.radioButtonWaitlistBySeniority.Text = "xxBySeniority";
			this.radioButtonWaitlistBySeniority.UseVisualStyleBackColor = true;
			this.radioButtonWaitlistBySeniority.Click += new System.EventHandler(this.radioButtonWaitlistBySeniority_Click);
			// 
			// tableLayoutPanelAbsenceProbability
			// 
			this.tableLayoutPanelAbsenceProbability.ColumnCount = 1;
			this.tableLayoutPanelAbsenceProbability.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelAbsenceProbability.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelAbsenceProbability.Controls.Add(this.checkBoxEnableAbsenceProbability, 0, 0);
			this.tableLayoutPanelAbsenceProbability.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelAbsenceProbability.Location = new System.Drawing.Point(3, 210);
			this.tableLayoutPanelAbsenceProbability.Name = "tableLayoutPanelAbsenceProbability";
			this.tableLayoutPanelAbsenceProbability.RowCount = 1;
			this.tableLayoutPanelAbsenceProbability.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelAbsenceProbability.Size = new System.Drawing.Size(959, 29);
			this.tableLayoutPanelAbsenceProbability.TabIndex = 29;
			// 
			// checkBoxEnableAbsenceProbability
			// 
			this.checkBoxEnableAbsenceProbability.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.checkBoxEnableAbsenceProbability.BeforeTouchSize = new System.Drawing.Size(181, 23);
			this.checkBoxEnableAbsenceProbability.ImageCheckBoxSize = new System.Drawing.Size(26, 26);
			this.checkBoxEnableAbsenceProbability.Location = new System.Drawing.Point(3, 3);
			this.checkBoxEnableAbsenceProbability.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxEnableAbsenceProbability.Name = "checkBoxEnableAbsenceProbability";
			this.checkBoxEnableAbsenceProbability.Size = new System.Drawing.Size(251, 23);
			this.checkBoxEnableAbsenceProbability.TabIndex = 31;
			this.checkBoxEnableAbsenceProbability.Text = "xxEnableAbsenceProbability";
			this.checkBoxEnableAbsenceProbability.ThemesEnabled = false;
			this.checkBoxEnableAbsenceProbability.CheckStateChanged += new System.EventHandler(this.checkBoxEnableAbsenceProbability_CheckStateChanged);
			// 
			// tableLayoutPanelAbsenceRequestCancellation
			// 
			this.tableLayoutPanelAbsenceRequestCancellation.ColumnCount = 2;
			this.tableLayoutPanelAbsenceRequestCancellation.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelAbsenceRequestCancellation.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelAbsenceRequestCancellation.Controls.Add(this.txtAbsenceRequestCancellationThreshold, 0, 0);
			this.tableLayoutPanelAbsenceRequestCancellation.Controls.Add(this.labelAbsenceRequestCancellationThreshold, 0, 0);
			this.tableLayoutPanelAbsenceRequestCancellation.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelAbsenceRequestCancellation.Location = new System.Drawing.Point(3, 245);
			this.tableLayoutPanelAbsenceRequestCancellation.Name = "tableLayoutPanelAbsenceRequestCancellation";
			this.tableLayoutPanelAbsenceRequestCancellation.RowCount = 1;
			this.tableLayoutPanelAbsenceRequestCancellation.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelAbsenceRequestCancellation.Size = new System.Drawing.Size(959, 29);
			this.tableLayoutPanelAbsenceRequestCancellation.TabIndex = 29;
			// 
			// txtAbsenceRequestCancellationThreshold
			// 
			this.txtAbsenceRequestCancellationThreshold.Location = new System.Drawing.Point(458, 3);
			this.txtAbsenceRequestCancellationThreshold.MaxLength = 3;
			this.txtAbsenceRequestCancellationThreshold.Name = "txtAbsenceRequestCancellationThreshold";
			this.txtAbsenceRequestCancellationThreshold.Size = new System.Drawing.Size(70, 39);
			this.txtAbsenceRequestCancellationThreshold.TabIndex = 6;
			this.txtAbsenceRequestCancellationThreshold.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.txtAbsenceRequestCancellationThreshold.Leave += new System.EventHandler(this.txtAbsenceRequestCancellationThreshold_Leave);
			// 
			// labelAbsenceRequestCancellationThreshold
			// 
			this.labelAbsenceRequestCancellationThreshold.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelAbsenceRequestCancellationThreshold.AutoSize = true;
			this.labelAbsenceRequestCancellationThreshold.Location = new System.Drawing.Point(3, 0);
			this.labelAbsenceRequestCancellationThreshold.Name = "labelAbsenceRequestCancellationThreshold";
			this.labelAbsenceRequestCancellationThreshold.Size = new System.Drawing.Size(449, 29);
			this.labelAbsenceRequestCancellationThreshold.TabIndex = 1;
			this.labelAbsenceRequestCancellationThreshold.Text = "xxAbsenceRequestCancellationThreshold";
			// 
			// tableLayoutPanelAbsenceRequestExpiration
			// 
			this.tableLayoutPanelAbsenceRequestExpiration.ColumnCount = 2;
			this.tableLayoutPanelAbsenceRequestExpiration.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelAbsenceRequestExpiration.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelAbsenceRequestExpiration.Controls.Add(this.txtAbsenceRequestExpiredThreshold, 0, 0);
			this.tableLayoutPanelAbsenceRequestExpiration.Controls.Add(this.labelAbsenceRequestExpriedThreshold, 0, 0);
			this.tableLayoutPanelAbsenceRequestExpiration.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelAbsenceRequestExpiration.Location = new System.Drawing.Point(3, 280);
			this.tableLayoutPanelAbsenceRequestExpiration.Name = "tableLayoutPanelAbsenceRequestExpiration";
			this.tableLayoutPanelAbsenceRequestExpiration.RowCount = 1;
			this.tableLayoutPanelAbsenceRequestExpiration.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelAbsenceRequestExpiration.Size = new System.Drawing.Size(959, 29);
			this.tableLayoutPanelAbsenceRequestExpiration.TabIndex = 30;
			// 
			// txtAbsenceRequestExpiredThreshold
			// 
			this.txtAbsenceRequestExpiredThreshold.Location = new System.Drawing.Point(405, 3);
			this.txtAbsenceRequestExpiredThreshold.MaxLength = 3;
			this.txtAbsenceRequestExpiredThreshold.Name = "txtAbsenceRequestExpiredThreshold";
			this.txtAbsenceRequestExpiredThreshold.Size = new System.Drawing.Size(70, 39);
			this.txtAbsenceRequestExpiredThreshold.TabIndex = 6;
			this.txtAbsenceRequestExpiredThreshold.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.txtAbsenceRequestExpiredThreshold.Leave += new System.EventHandler(this.txtAbsenceRequestExpiredThreshold_Leave);
			// 
			// labelAbsenceRequestExpriedThreshold
			// 
			this.labelAbsenceRequestExpriedThreshold.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelAbsenceRequestExpriedThreshold.AutoSize = true;
			this.labelAbsenceRequestExpriedThreshold.Location = new System.Drawing.Point(3, 0);
			this.labelAbsenceRequestExpriedThreshold.Name = "labelAbsenceRequestExpriedThreshold";
			this.labelAbsenceRequestExpriedThreshold.Size = new System.Drawing.Size(396, 29);
			this.labelAbsenceRequestExpriedThreshold.TabIndex = 1;
			this.labelAbsenceRequestExpriedThreshold.Text = "xxAbsenceRequestExpiredThreshold";
			//
			// labelOvertimeStaffingCheckMethodSetting
			this.labelOvertimeStaffingCheckMethodSetting.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelOvertimeStaffingCheckMethodSetting.AutoSize = true;
			this.labelOvertimeStaffingCheckMethodSetting.Location = new System.Drawing.Point(3, 0);
			this.labelOvertimeStaffingCheckMethodSetting.Name = "labelOvertimeStaffingCheckMethodSetting";
			this.labelOvertimeStaffingCheckMethodSetting.Size = new System.Drawing.Size(396, 29);
			this.labelOvertimeStaffingCheckMethodSetting.TabIndex = 1;
			this.labelOvertimeStaffingCheckMethodSetting.Text = "xxOvertimeStaffingCheckMethodSetting";
			//
			// 
			// tableLayoutPanelOpenForAbsenceRequests
			// 
			this.tableLayoutPanelOpenForAbsenceRequests.BackColor = System.Drawing.Color.LightSteelBlue;
			this.tableLayoutPanelOpenForAbsenceRequests.ColumnCount = 3;
			this.tableLayoutPanelOpenForAbsenceRequests.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelOpenForAbsenceRequests.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelOpenForAbsenceRequests.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelOpenForAbsenceRequests.Controls.Add(this.labelOpenForAbsenceRequests, 0, 0);
			this.tableLayoutPanelOpenForAbsenceRequests.Controls.Add(this.buttonDeleteAbsenceRequestPeriod, 2, 0);
			this.tableLayoutPanelOpenForAbsenceRequests.Controls.Add(this.buttonAddAbsenceRequestPeriod, 1, 0);
			this.tableLayoutPanelOpenForAbsenceRequests.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelOpenForAbsenceRequests.Location = new System.Drawing.Point(3, 315);
			this.tableLayoutPanelOpenForAbsenceRequests.Name = "tableLayoutPanelOpenForAbsenceRequests";
			this.tableLayoutPanelOpenForAbsenceRequests.RowCount = 1;
			this.tableLayoutPanelOpenForAbsenceRequests.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelOpenForAbsenceRequests.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
			this.tableLayoutPanelOpenForAbsenceRequests.Size = new System.Drawing.Size(959, 34);
			this.tableLayoutPanelOpenForAbsenceRequests.TabIndex = 0;
			// 
			// labelOpenForAbsenceRequests
			// 
			this.labelOpenForAbsenceRequests.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelOpenForAbsenceRequests.AutoSize = true;
			this.labelOpenForAbsenceRequests.BackColor = System.Drawing.Color.Transparent;
			this.labelOpenForAbsenceRequests.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelOpenForAbsenceRequests.ForeColor = System.Drawing.Color.GhostWhite;
			this.labelOpenForAbsenceRequests.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.labelOpenForAbsenceRequests.Location = new System.Drawing.Point(3, 0);
			this.labelOpenForAbsenceRequests.Name = "labelOpenForAbsenceRequests";
			this.labelOpenForAbsenceRequests.Size = new System.Drawing.Size(358, 36);
			this.labelOpenForAbsenceRequests.TabIndex = 0;
			this.labelOpenForAbsenceRequests.Text = "xxOpenForAbsenceRequests";
			this.labelOpenForAbsenceRequests.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// buttonDeleteAbsenceRequestPeriod
			// 
			this.buttonDeleteAbsenceRequestPeriod.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.buttonDeleteAbsenceRequestPeriod.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonDeleteAbsenceRequestPeriod.BackColor = System.Drawing.Color.White;
			this.buttonDeleteAbsenceRequestPeriod.BeforeTouchSize = new System.Drawing.Size(28, 28);
			this.buttonDeleteAbsenceRequestPeriod.ForeColor = System.Drawing.Color.White;
			this.buttonDeleteAbsenceRequestPeriod.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_temp_DeleteGroup10;
			this.buttonDeleteAbsenceRequestPeriod.IsBackStageButton = false;
			this.buttonDeleteAbsenceRequestPeriod.Location = new System.Drawing.Point(924, 4);
			this.buttonDeleteAbsenceRequestPeriod.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
			this.buttonDeleteAbsenceRequestPeriod.Name = "buttonDeleteAbsenceRequestPeriod";
			this.buttonDeleteAbsenceRequestPeriod.Size = new System.Drawing.Size(28, 28);
			this.buttonDeleteAbsenceRequestPeriod.TabIndex = 25;
			this.buttonDeleteAbsenceRequestPeriod.UseVisualStyle = true;
			this.buttonDeleteAbsenceRequestPeriod.Click += new System.EventHandler(this.buttonAdvDeleteAbsenceRequestPeriod_Click);
			// 
			// buttonAddAbsenceRequestPeriod
			// 
			this.buttonAddAbsenceRequestPeriod.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.buttonAddAbsenceRequestPeriod.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAddAbsenceRequestPeriod.BackColor = System.Drawing.Color.White;
			this.buttonAddAbsenceRequestPeriod.BeforeTouchSize = new System.Drawing.Size(28, 28);
			this.buttonAddAbsenceRequestPeriod.Font = new System.Drawing.Font("Tahoma", 8F);
			this.buttonAddAbsenceRequestPeriod.ForeColor = System.Drawing.Color.White;
			this.buttonAddAbsenceRequestPeriod.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.test_add2;
			this.buttonAddAbsenceRequestPeriod.IsBackStageButton = false;
			this.buttonAddAbsenceRequestPeriod.Location = new System.Drawing.Point(889, 4);
			this.buttonAddAbsenceRequestPeriod.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
			this.buttonAddAbsenceRequestPeriod.Name = "buttonAddAbsenceRequestPeriod";
			this.buttonAddAbsenceRequestPeriod.Size = new System.Drawing.Size(28, 28);
			this.buttonAddAbsenceRequestPeriod.TabIndex = 24;
			this.buttonAddAbsenceRequestPeriod.UseVisualStyle = true;
			this.buttonAddAbsenceRequestPeriod.UseVisualStyleBackColor = false;
			this.buttonAddAbsenceRequestPeriod.Click += new System.EventHandler(this.buttonAddAbsenceRequestPeriod_Click);
			// 
			// tableLayoutPanelOpenForOvertimeRequests
			// 
			this.tableLayoutPanelOpenForOvertimeRequests.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(191)))), ((int)(((byte)(234)))));
			this.tableLayoutPanelOpenForOvertimeRequests.ColumnCount = 3;
			this.tableLayoutPanelOpenForOvertimeRequests.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelOpenForOvertimeRequests.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelOpenForOvertimeRequests.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanelOpenForOvertimeRequests.Controls.Add(this.labelOpenForOvertimeRequests, 0, 0);
			this.tableLayoutPanelOpenForOvertimeRequests.Controls.Add(this.buttonDeleteOvertimeRequestPeriod, 2, 0);
			this.tableLayoutPanelOpenForOvertimeRequests.Controls.Add(this.buttonAddOvertimeRequestPeriod, 1, 0);
			this.tableLayoutPanelOpenForOvertimeRequests.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelOpenForOvertimeRequests.Location = new System.Drawing.Point(3, 325);
			this.tableLayoutPanelOpenForOvertimeRequests.Name = "tableLayoutPanelOpenForOvertimeRequests";
			this.tableLayoutPanelOpenForOvertimeRequests.RowCount = 1;
			this.tableLayoutPanelOpenForOvertimeRequests.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelOpenForOvertimeRequests.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
			this.tableLayoutPanelOpenForOvertimeRequests.Size = new System.Drawing.Size(959, 34);
			this.tableLayoutPanelOpenForOvertimeRequests.TabIndex = 0;
			// 
			// tableLayoutPanelOvertimeMaximumSetting
			// 
			this.tableLayoutPanelOvertimeMaximumSetting.ColumnCount = 5;
			this.tableLayoutPanelOvertimeMaximumSetting.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle() { SizeType = SizeType.AutoSize });
			this.tableLayoutPanelOvertimeMaximumSetting.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelOvertimeMaximumSetting.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelOvertimeMaximumSetting.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelOvertimeMaximumSetting.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelOvertimeMaximumSetting.Controls.Add(this.checkBoxAdvOvertimeMaximumEnabled, 0, 0);
			this.tableLayoutPanelOvertimeMaximumSetting.Controls.Add(this.timeSpanTextBoxOvertimeRequestMaximumTime, 1, 0);
			this.tableLayoutPanelOvertimeMaximumSetting.Controls.Add(this.lblOvertimeRequestMaximumTimeHandleType, 2, 0);
			this.tableLayoutPanelOvertimeMaximumSetting.Controls.Add(this.comboBoxOvertimeRequestMaximumTimeHandleType, 3, 0);
			this.tableLayoutPanelOvertimeMaximumSetting.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelOvertimeMaximumSetting.Location = new System.Drawing.Point(3, 103);
			this.tableLayoutPanelOvertimeMaximumSetting.Name = "tableLayoutPanelOvertimeMaximumSetting";
			this.tableLayoutPanelOvertimeMaximumSetting.RowCount = 1;
			this.tableLayoutPanelOvertimeMaximumSetting.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanelOvertimeMaximumSetting.Size = new System.Drawing.Size(959, 34);
			this.tableLayoutPanelOvertimeMaximumSetting.TabIndex = 0;
			//
			//tableLayoutPanelOvertimeStaffingCheckMethodSetting
			this.tableLayoutPanelOvertimeStaffingCheckMethodSetting.ColumnCount = 3;
			this.tableLayoutPanelOvertimeStaffingCheckMethodSetting.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle() { SizeType = SizeType.AutoSize });
			this.tableLayoutPanelOvertimeStaffingCheckMethodSetting.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelOvertimeStaffingCheckMethodSetting.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelOvertimeStaffingCheckMethodSetting.Controls.Add(this.labelOvertimeStaffingCheckMethodSetting, 0, 0);
			this.tableLayoutPanelOvertimeStaffingCheckMethodSetting.Controls.Add(this.comboBoxOvertimeRequestStaffingCheckMethod, 1, 0);
			this.tableLayoutPanelOvertimeStaffingCheckMethodSetting.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelOvertimeStaffingCheckMethodSetting.Location = new System.Drawing.Point(3, 103);
			this.tableLayoutPanelOvertimeStaffingCheckMethodSetting.Name = "tableLayoutPanelOvertimeStaffingCheckMethodSetting";
			this.tableLayoutPanelOvertimeStaffingCheckMethodSetting.RowCount = 1;
			this.tableLayoutPanelOvertimeStaffingCheckMethodSetting.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanelOvertimeStaffingCheckMethodSetting.Size = new System.Drawing.Size(959, 34);
			this.tableLayoutPanelOvertimeStaffingCheckMethodSetting.TabIndex = 0;
			//
			// tableLayoutPanelOvertimeProbablityAndPrimarySkill
			this.tableLayoutPanelOvertimeProbablityAndPrimarySkill.ColumnCount = 3;
			this.tableLayoutPanelOvertimeProbablityAndPrimarySkill.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(){ Width = 50 });
			this.tableLayoutPanelOvertimeProbablityAndPrimarySkill.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(){ Width = 50 });
			this.tableLayoutPanelOvertimeProbablityAndPrimarySkill.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelOvertimeProbablityAndPrimarySkill.Controls.Add(this.checkBoxAdvOvertimeProbability, 0, 0);
			this.tableLayoutPanelOvertimeProbablityAndPrimarySkill.Controls.Add(this.checkBoxUsePrimarySkill, 1, 0);
			this.tableLayoutPanelOvertimeProbablityAndPrimarySkill.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelOvertimeProbablityAndPrimarySkill.Location = new System.Drawing.Point(3, 103);
			this.tableLayoutPanelOvertimeProbablityAndPrimarySkill.Name = "tableLayoutPanelOvertimeProbablityAndPrimarySkill";
			this.tableLayoutPanelOvertimeProbablityAndPrimarySkill.RowCount = 1;
			this.tableLayoutPanelOvertimeProbablityAndPrimarySkill.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanelOvertimeProbablityAndPrimarySkill.Size = new System.Drawing.Size(959, 34);
			this.tableLayoutPanelOvertimeProbablityAndPrimarySkill.TabIndex = 0;
			//
			//
			// 
			// tableLayoutPanelOvertimeMaximumContinuousWorkTimeSetting
			// 
			this.tableLayoutPanelOvertimeMaximumContinuousWorkTimeSetting.ColumnCount = 7;
			this.tableLayoutPanelOvertimeMaximumContinuousWorkTimeSetting.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle() { SizeType = SizeType.AutoSize });
			this.tableLayoutPanelOvertimeMaximumContinuousWorkTimeSetting.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelOvertimeMaximumContinuousWorkTimeSetting.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelOvertimeMaximumContinuousWorkTimeSetting.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelOvertimeMaximumContinuousWorkTimeSetting.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelOvertimeMaximumContinuousWorkTimeSetting.Controls.Add(this.checkBoxAdvOvertimeMaximumContinuousWorkTimeEnabled, 0, 0);
			this.tableLayoutPanelOvertimeMaximumContinuousWorkTimeSetting.Controls.Add(this.timeSpanTextBoxOvertimeRequestMaximumContinuousWorkTime, 1, 0);
			this.tableLayoutPanelOvertimeMaximumContinuousWorkTimeSetting.Controls.Add(this.lblOvertimeRequestMinimumRestTimeThreshold, 2, 0);
			this.tableLayoutPanelOvertimeMaximumContinuousWorkTimeSetting.Controls.Add(this.timeSpanTextBoxOvertimeRequestMinimumRestTimeThreshold, 3, 0);
			this.tableLayoutPanelOvertimeMaximumContinuousWorkTimeSetting.Controls.Add(this.lblOvertimeRequestMaximumContinuousWorkTimeHandleType, 4, 0);
			this.tableLayoutPanelOvertimeMaximumContinuousWorkTimeSetting.Controls.Add(this.comboBoxOvertimeRequestMaximumContinuousWorkTimeHandleType, 5, 0);
			this.tableLayoutPanelOvertimeMaximumContinuousWorkTimeSetting.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelOvertimeMaximumContinuousWorkTimeSetting.Location = new System.Drawing.Point(3, 103);
			this.tableLayoutPanelOvertimeMaximumContinuousWorkTimeSetting.Name = "tableLayoutPanelOvertimeMaximumContinuousWorkTimeSetting";
			this.tableLayoutPanelOvertimeMaximumContinuousWorkTimeSetting.RowCount = 1;
			this.tableLayoutPanelOvertimeMaximumContinuousWorkTimeSetting.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanelOvertimeMaximumContinuousWorkTimeSetting.Size = new System.Drawing.Size(959, 34);
			// 
			// timeSpanTextBoxOvertimeRequestMaximumTime
			// 
			this.timeSpanTextBoxOvertimeRequestMaximumTime.AlignTextBoxText = System.Windows.Forms.HorizontalAlignment.Center;
			this.timeSpanTextBoxOvertimeRequestMaximumTime.AllowNegativeValues = false;
			this.timeSpanTextBoxOvertimeRequestMaximumTime.DefaultInterpretAsMinutes = false;
			this.timeSpanTextBoxOvertimeRequestMaximumTime.Location = new System.Drawing.Point(183, 4);
			this.timeSpanTextBoxOvertimeRequestMaximumTime.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
			this.timeSpanTextBoxOvertimeRequestMaximumTime.MaximumValue = System.TimeSpan.Parse("10.00:00:00");
			this.timeSpanTextBoxOvertimeRequestMaximumTime.Name = "timeSpanTextBoxOvertimeRequestMaximumTime";
			this.timeSpanTextBoxOvertimeRequestMaximumTime.Size = new System.Drawing.Size(80, 25);
			this.timeSpanTextBoxOvertimeRequestMaximumTime.TabIndex = 2;
			this.timeSpanTextBoxOvertimeRequestMaximumTime.TimeFormat = Teleopti.Ccc.Domain.InterfaceLegacy.Domain.TimeFormatsType.HoursMinutes;
			this.timeSpanTextBoxOvertimeRequestMaximumTime.TimeSpanBoxHeight = 23;
			this.timeSpanTextBoxOvertimeRequestMaximumTime.TimeSpanBoxWidth = 60;
			this.timeSpanTextBoxOvertimeRequestMaximumTime.Leave += timeSpanTextBoxOvertimeRequestMaximumTime_Leave;
			// 
			// timeSpanTextBoxOvertimeRequestMaximumContinuousWorkTime
			// 
			this.timeSpanTextBoxOvertimeRequestMaximumContinuousWorkTime.AlignTextBoxText = System.Windows.Forms.HorizontalAlignment.Center;
			this.timeSpanTextBoxOvertimeRequestMaximumContinuousWorkTime.AllowNegativeValues = false;
			this.timeSpanTextBoxOvertimeRequestMaximumContinuousWorkTime.DefaultInterpretAsMinutes = false;
			this.timeSpanTextBoxOvertimeRequestMaximumContinuousWorkTime.Location = new System.Drawing.Point(183, 4);
			this.timeSpanTextBoxOvertimeRequestMaximumContinuousWorkTime.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
			this.timeSpanTextBoxOvertimeRequestMaximumContinuousWorkTime.MaximumValue = System.TimeSpan.Parse("10.00:00:00");
			this.timeSpanTextBoxOvertimeRequestMaximumContinuousWorkTime.Name = "timeSpanTextBoxOvertimeRequestMaximumContinuousWorkTime";
			this.timeSpanTextBoxOvertimeRequestMaximumContinuousWorkTime.Size = new System.Drawing.Size(80, 25);
			this.timeSpanTextBoxOvertimeRequestMaximumContinuousWorkTime.TabIndex = 5;
			this.timeSpanTextBoxOvertimeRequestMaximumContinuousWorkTime.TimeFormat = Teleopti.Ccc.Domain.InterfaceLegacy.Domain.TimeFormatsType.HoursMinutes;
			this.timeSpanTextBoxOvertimeRequestMaximumContinuousWorkTime.TimeSpanBoxHeight = 23;
			this.timeSpanTextBoxOvertimeRequestMaximumContinuousWorkTime.TimeSpanBoxWidth = 60;
			this.timeSpanTextBoxOvertimeRequestMaximumContinuousWorkTime.Leave += timeSpanTextBoxOvertimeRequestMaximumContinuousWorkTime_Leave;
			// 
			// timeSpanTextBoxOvertimeRequestMinimumRestTimeThreshold
			// 
			this.timeSpanTextBoxOvertimeRequestMinimumRestTimeThreshold.AlignTextBoxText = System.Windows.Forms.HorizontalAlignment.Center;
			this.timeSpanTextBoxOvertimeRequestMinimumRestTimeThreshold.AllowNegativeValues = false;
			this.timeSpanTextBoxOvertimeRequestMinimumRestTimeThreshold.DefaultInterpretAsMinutes = false;
			this.timeSpanTextBoxOvertimeRequestMinimumRestTimeThreshold.Location = new System.Drawing.Point(183, 4);
			this.timeSpanTextBoxOvertimeRequestMinimumRestTimeThreshold.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
			this.timeSpanTextBoxOvertimeRequestMinimumRestTimeThreshold.MaximumValue = System.TimeSpan.Parse("10.00:00:00");
			this.timeSpanTextBoxOvertimeRequestMinimumRestTimeThreshold.Name = "timeSpanTextBoxOvertimeRequestMinimumRestTimeThreshold";
			this.timeSpanTextBoxOvertimeRequestMinimumRestTimeThreshold.Size = new System.Drawing.Size(80, 25);
			this.timeSpanTextBoxOvertimeRequestMinimumRestTimeThreshold.TabIndex = 6;
			this.timeSpanTextBoxOvertimeRequestMinimumRestTimeThreshold.TimeFormat = Teleopti.Ccc.Domain.InterfaceLegacy.Domain.TimeFormatsType.HoursMinutes;
			this.timeSpanTextBoxOvertimeRequestMinimumRestTimeThreshold.TimeSpanBoxHeight = 23;
			this.timeSpanTextBoxOvertimeRequestMinimumRestTimeThreshold.TimeSpanBoxWidth = 60;
			this.timeSpanTextBoxOvertimeRequestMinimumRestTimeThreshold.Leave += timeSpanTextBoxOvertimeRequestMinimumRestTimeThreshold_Leave;
			// 
			// lblOvertimeRequestMaximumTimeHandleType
			// 
			this.lblOvertimeRequestMaximumTimeHandleType.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.lblOvertimeRequestMaximumTimeHandleType.AutoSize = true;
			this.lblOvertimeRequestMaximumTimeHandleType.BackColor = System.Drawing.Color.Transparent;
			this.lblOvertimeRequestMaximumTimeHandleType.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblOvertimeRequestMaximumTimeHandleType.ForeColor = System.Drawing.Color.Black;
			this.lblOvertimeRequestMaximumTimeHandleType.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.lblOvertimeRequestMaximumTimeHandleType.Location = new System.Drawing.Point(266, 8);
			this.lblOvertimeRequestMaximumTimeHandleType.Name = "lblOvertimeRequestMaximumTimeHandleType";
			this.lblOvertimeRequestMaximumTimeHandleType.Size = new System.Drawing.Size(273, 17);
			this.lblOvertimeRequestMaximumTimeHandleType.TabIndex = 0;
			this.lblOvertimeRequestMaximumTimeHandleType.Text = "xxOvertimeRequestMaximumTimeHandleType";
			this.lblOvertimeRequestMaximumTimeHandleType.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblOvertimeRequestMinimumRestTimeThreshold
			// 
			this.lblOvertimeRequestMinimumRestTimeThreshold.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.lblOvertimeRequestMinimumRestTimeThreshold.AutoSize = true;
			this.lblOvertimeRequestMinimumRestTimeThreshold.BackColor = System.Drawing.Color.Transparent;
			this.lblOvertimeRequestMinimumRestTimeThreshold.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblOvertimeRequestMinimumRestTimeThreshold.ForeColor = System.Drawing.Color.Black;
			this.lblOvertimeRequestMinimumRestTimeThreshold.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.lblOvertimeRequestMinimumRestTimeThreshold.Location = new System.Drawing.Point(266, 8);
			this.lblOvertimeRequestMinimumRestTimeThreshold.Name = "lblOvertimeRequestMinimumRestTimeThreshold";
			this.lblOvertimeRequestMinimumRestTimeThreshold.Size = new System.Drawing.Size(273, 17);
			this.lblOvertimeRequestMinimumRestTimeThreshold.TabIndex = 0;
			this.lblOvertimeRequestMinimumRestTimeThreshold.Text = "xxOvertimeRequestMinimumRestTimeThreshold";
			this.lblOvertimeRequestMinimumRestTimeThreshold.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblOvertimeRequestMaximumContinuousWorkTimeHandleType
			// 
			this.lblOvertimeRequestMaximumContinuousWorkTimeHandleType.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.lblOvertimeRequestMaximumContinuousWorkTimeHandleType.AutoSize = true;
			this.lblOvertimeRequestMaximumContinuousWorkTimeHandleType.BackColor = System.Drawing.Color.Transparent;
			this.lblOvertimeRequestMaximumContinuousWorkTimeHandleType.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblOvertimeRequestMaximumContinuousWorkTimeHandleType.ForeColor = System.Drawing.Color.Black;
			this.lblOvertimeRequestMaximumContinuousWorkTimeHandleType.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.lblOvertimeRequestMaximumContinuousWorkTimeHandleType.Location = new System.Drawing.Point(266, 8);
			this.lblOvertimeRequestMaximumContinuousWorkTimeHandleType.Name = "lblOvertimeRequestMaximumContinuousWorkTimeHandleType";
			this.lblOvertimeRequestMaximumContinuousWorkTimeHandleType.Size = new System.Drawing.Size(273, 17);
			this.lblOvertimeRequestMaximumContinuousWorkTimeHandleType.TabIndex = 0;
			this.lblOvertimeRequestMaximumContinuousWorkTimeHandleType.Text = "xxOvertimeRequestMaximumTimeHandleType";
			this.lblOvertimeRequestMaximumContinuousWorkTimeHandleType.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// comboBoxOvertimeRequestMaximumTimeHandleType
			// 
			this.comboBoxOvertimeRequestMaximumTimeHandleType.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.comboBoxOvertimeRequestMaximumTimeHandleType.BackColor = System.Drawing.Color.White;
			this.comboBoxOvertimeRequestMaximumTimeHandleType.BeforeTouchSize = new System.Drawing.Size(150, 23);
			this.comboBoxOvertimeRequestMaximumTimeHandleType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxOvertimeRequestMaximumTimeHandleType.Location = new System.Drawing.Point(545, 6);
			this.comboBoxOvertimeRequestMaximumTimeHandleType.Name = "comboBoxOvertimeRequestMaximumTimeHandleType";
			this.comboBoxOvertimeRequestMaximumTimeHandleType.Size = new System.Drawing.Size(150, 23);
			this.comboBoxOvertimeRequestMaximumTimeHandleType.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.comboBoxOvertimeRequestMaximumTimeHandleType.TabIndex = 3;
			this.comboBoxOvertimeRequestMaximumTimeHandleType.SelectedIndexChanged += ComboBoxOvertimeRequestMaximumTimeHandleType_SelectedIndexChanged;
			//
			// comboBoxOvertimeRequestStaffingCheckMethod
			this.comboBoxOvertimeRequestStaffingCheckMethod.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.comboBoxOvertimeRequestStaffingCheckMethod.BackColor = System.Drawing.Color.White;
			this.comboBoxOvertimeRequestStaffingCheckMethod.BeforeTouchSize = new System.Drawing.Size(150, 23);
			this.comboBoxOvertimeRequestStaffingCheckMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxOvertimeRequestStaffingCheckMethod.Location = new System.Drawing.Point(545, 6);
			this.comboBoxOvertimeRequestStaffingCheckMethod.Name = "comboBoxOvertimeRequestStaffingCheckMethod";
			this.comboBoxOvertimeRequestStaffingCheckMethod.Size = new System.Drawing.Size(150, 23);
			this.comboBoxOvertimeRequestStaffingCheckMethod.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.comboBoxOvertimeRequestStaffingCheckMethod.TabIndex = 3;
			this.comboBoxOvertimeRequestStaffingCheckMethod.SelectedIndexChanged += ComboBoxOvertimeRequestStaffingCheckMethod_SelectedIndexChanged;
			//
			// 
			// comboBoxOvertimeRequestMaximumContinuousWorkTimeHandleType
			// 
			this.comboBoxOvertimeRequestMaximumContinuousWorkTimeHandleType.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.comboBoxOvertimeRequestMaximumContinuousWorkTimeHandleType.BackColor = System.Drawing.Color.White;
			this.comboBoxOvertimeRequestMaximumContinuousWorkTimeHandleType.BeforeTouchSize = new System.Drawing.Size(150, 23);
			this.comboBoxOvertimeRequestMaximumContinuousWorkTimeHandleType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxOvertimeRequestMaximumContinuousWorkTimeHandleType.Location = new System.Drawing.Point(545, 6);
			this.comboBoxOvertimeRequestMaximumContinuousWorkTimeHandleType.Name = "comboBoxOvertimeRequestMaximumContinuousWorkTimeHandleType";
			this.comboBoxOvertimeRequestMaximumContinuousWorkTimeHandleType.Size = new System.Drawing.Size(150, 23);
			this.comboBoxOvertimeRequestMaximumContinuousWorkTimeHandleType.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.comboBoxOvertimeRequestMaximumContinuousWorkTimeHandleType.TabIndex = 7;
			this.comboBoxOvertimeRequestMaximumContinuousWorkTimeHandleType.SelectedIndexChanged += ComboBoxOvertimeRequestMaximumContinuousWorkTimeHandleType_SelectedIndexChanged;
			// 
			// buttonDeleteOvertimeRequestPeriod
			// 
			this.buttonDeleteOvertimeRequestPeriod.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.buttonDeleteOvertimeRequestPeriod.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonDeleteOvertimeRequestPeriod.BackColor = System.Drawing.Color.White;
			this.buttonDeleteOvertimeRequestPeriod.BeforeTouchSize = new System.Drawing.Size(28, 28);
			this.buttonDeleteOvertimeRequestPeriod.ForeColor = System.Drawing.Color.White;
			this.buttonDeleteOvertimeRequestPeriod.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_temp_DeleteGroup10;
			this.buttonDeleteOvertimeRequestPeriod.IsBackStageButton = false;
			this.buttonDeleteOvertimeRequestPeriod.Location = new System.Drawing.Point(924, 4);
			this.buttonDeleteOvertimeRequestPeriod.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
			this.buttonDeleteOvertimeRequestPeriod.Name = "buttonDeleteOvertimeRequestPeriod";
			this.buttonDeleteOvertimeRequestPeriod.Size = new System.Drawing.Size(28, 28);
			this.buttonDeleteOvertimeRequestPeriod.TabIndex = 25;
			this.buttonDeleteOvertimeRequestPeriod.UseVisualStyle = true;
			this.buttonDeleteOvertimeRequestPeriod.Click += new System.EventHandler(this.buttonAdvDeleteOvertimeRequestPeriod_Click);
			// 
			// labelOpenForOvertimeRequests
			// 
			this.labelOpenForOvertimeRequests.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelOpenForOvertimeRequests.AutoSize = true;
			this.labelOpenForOvertimeRequests.BackColor = System.Drawing.Color.Transparent;
			this.labelOpenForOvertimeRequests.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelOpenForOvertimeRequests.ForeColor = System.Drawing.Color.GhostWhite;
			this.labelOpenForOvertimeRequests.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.labelOpenForOvertimeRequests.Location = new System.Drawing.Point(3, 0);
			this.labelOpenForOvertimeRequests.Name = "labelOpenForOvertimeRequests";
			this.labelOpenForOvertimeRequests.Size = new System.Drawing.Size(358, 36);
			this.labelOpenForOvertimeRequests.TabIndex = 0;
			this.labelOpenForOvertimeRequests.Text = "xxOpenForOvertimeRequests";
			this.labelOpenForOvertimeRequests.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// buttonAddOvertimeRequestPeriod
			// 
			this.buttonAddOvertimeRequestPeriod.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.buttonAddOvertimeRequestPeriod.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAddOvertimeRequestPeriod.BackColor = System.Drawing.Color.White;
			this.buttonAddOvertimeRequestPeriod.BeforeTouchSize = new System.Drawing.Size(28, 28);
			this.buttonAddOvertimeRequestPeriod.Font = new System.Drawing.Font("Tahoma", 8F);
			this.buttonAddOvertimeRequestPeriod.ForeColor = System.Drawing.Color.White;
			this.buttonAddOvertimeRequestPeriod.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.test_add2;
			this.buttonAddOvertimeRequestPeriod.IsBackStageButton = false;
			this.buttonAddOvertimeRequestPeriod.Location = new System.Drawing.Point(889, 4);
			this.buttonAddOvertimeRequestPeriod.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
			this.buttonAddOvertimeRequestPeriod.Name = "buttonAddOvertimeRequestPeriod";
			this.buttonAddOvertimeRequestPeriod.Size = new System.Drawing.Size(28, 28);
			this.buttonAddOvertimeRequestPeriod.TabIndex = 24;
			this.buttonAddOvertimeRequestPeriod.UseVisualStyle = true;
			this.buttonAddOvertimeRequestPeriod.UseVisualStyleBackColor = false;
			this.buttonAddOvertimeRequestPeriod.Click += new System.EventHandler(this.buttonAddOvertimeRequestPeriod_Click);
			// 
			// gridControlAbsenceRequestOpenPeriods
			// 
			this.gridControlAbsenceRequestOpenPeriods.AlphaBlendSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(94)))), ((int)(((byte)(171)))), ((int)(((byte)(222)))));
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
			this.gridControlAbsenceRequestOpenPeriods.BaseStylesMap.AddRange(new Syncfusion.Windows.Forms.Grid.GridBaseStyle[] {
			gridBaseStyle5,
			gridBaseStyle6,
			gridBaseStyle7,
			gridBaseStyle8});
			this.gridControlAbsenceRequestOpenPeriods.ColCount = 11;
			this.gridControlAbsenceRequestOpenPeriods.ColWidthEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridColWidth[] {
			new Syncfusion.Windows.Forms.Grid.GridColWidth(0, 35)});
			this.gridControlAbsenceRequestOpenPeriods.ContextMenuStrip = this.contextMenuStripOpenPeriodsGrid;
			this.gridControlAbsenceRequestOpenPeriods.DefaultGridBorderStyle = Syncfusion.Windows.Forms.Grid.GridBorderStyle.Solid;
			this.gridControlAbsenceRequestOpenPeriods.DefaultRowHeight = 20;
			this.gridControlAbsenceRequestOpenPeriods.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridControlAbsenceRequestOpenPeriods.GridOfficeScrollBars = Syncfusion.Windows.Forms.OfficeScrollBars.Office2007;
			this.gridControlAbsenceRequestOpenPeriods.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Metro;
			this.gridControlAbsenceRequestOpenPeriods.Location = new System.Drawing.Point(3, 355);
			this.gridControlAbsenceRequestOpenPeriods.Name = "gridControlAbsenceRequestOpenPeriods";
			this.gridControlAbsenceRequestOpenPeriods.NumberedColHeaders = false;
			this.gridControlAbsenceRequestOpenPeriods.NumberedRowHeaders = false;
			this.gridControlAbsenceRequestOpenPeriods.Office2007ScrollBars = true;
			this.gridControlAbsenceRequestOpenPeriods.Properties.BackgroundColor = System.Drawing.SystemColors.Window;
			this.gridControlAbsenceRequestOpenPeriods.Properties.ForceImmediateRepaint = false;
			this.gridControlAbsenceRequestOpenPeriods.Properties.GridLineColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(212)))), ((int)(((byte)(212)))));
			this.gridControlAbsenceRequestOpenPeriods.Properties.MarkColHeader = false;
			this.gridControlAbsenceRequestOpenPeriods.Properties.MarkRowHeader = false;
			this.gridControlAbsenceRequestOpenPeriods.RowCount = 0;
			this.gridControlAbsenceRequestOpenPeriods.RowHeightEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridRowHeight[] {
			new Syncfusion.Windows.Forms.Grid.GridRowHeight(0, 29)});
			this.gridControlAbsenceRequestOpenPeriods.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
			this.gridControlAbsenceRequestOpenPeriods.Size = new System.Drawing.Size(959, 115);
			this.gridControlAbsenceRequestOpenPeriods.SmartSizeBox = false;
			this.gridControlAbsenceRequestOpenPeriods.TabIndex = 26;
			this.gridControlAbsenceRequestOpenPeriods.Text = "gridControl1";
			this.gridControlAbsenceRequestOpenPeriods.ThemesEnabled = true;
			this.gridControlAbsenceRequestOpenPeriods.UseRightToLeftCompatibleTextBox = true;
			this.gridControlAbsenceRequestOpenPeriods.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gridControlAbsenceRequestOpenPeriods_MouseDown);

			// 
			// gridControlOvertimeRequestOpenPeriods
			// 
			this.gridControlOvertimeRequestOpenPeriods.AlphaBlendSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(94)))), ((int)(((byte)(171)))), ((int)(((byte)(222)))));

			this.gridControlOvertimeRequestOpenPeriods.BaseStylesMap.AddRange(new Syncfusion.Windows.Forms.Grid.GridBaseStyle[] {
			gridBaseStyle5,
			gridBaseStyle6,
			gridBaseStyle7,
			gridBaseStyle8});
			this.gridControlOvertimeRequestOpenPeriods.ColCount = 6;
			this.gridControlOvertimeRequestOpenPeriods.ColWidthEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridColWidth[] {
			new Syncfusion.Windows.Forms.Grid.GridColWidth(0, 35)});
			this.gridControlOvertimeRequestOpenPeriods.ContextMenuStrip = this.contextMenuStripOvertimeRequestOpenPeriodsGrid;
			this.gridControlOvertimeRequestOpenPeriods.DefaultGridBorderStyle = Syncfusion.Windows.Forms.Grid.GridBorderStyle.Solid;
			this.gridControlOvertimeRequestOpenPeriods.DefaultRowHeight = 20;
			this.gridControlOvertimeRequestOpenPeriods.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridControlOvertimeRequestOpenPeriods.GridOfficeScrollBars = Syncfusion.Windows.Forms.OfficeScrollBars.Office2007;
			this.gridControlOvertimeRequestOpenPeriods.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Metro;
			this.gridControlOvertimeRequestOpenPeriods.Location = new System.Drawing.Point(3, 355);
			this.gridControlOvertimeRequestOpenPeriods.Name = "gridControlOvertimeRequestOpenPeriods";
			this.gridControlOvertimeRequestOpenPeriods.NumberedColHeaders = false;
			this.gridControlOvertimeRequestOpenPeriods.NumberedRowHeaders = false;
			this.gridControlOvertimeRequestOpenPeriods.Office2007ScrollBars = true;
			this.gridControlOvertimeRequestOpenPeriods.Properties.BackgroundColor = System.Drawing.SystemColors.Window;
			this.gridControlOvertimeRequestOpenPeriods.Properties.ForceImmediateRepaint = false;
			this.gridControlOvertimeRequestOpenPeriods.Properties.GridLineColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(212)))), ((int)(((byte)(212)))));
			this.gridControlOvertimeRequestOpenPeriods.Properties.MarkColHeader = false;
			this.gridControlOvertimeRequestOpenPeriods.Properties.MarkRowHeader = false;
			this.gridControlOvertimeRequestOpenPeriods.RowCount = 0;
			this.gridControlOvertimeRequestOpenPeriods.RowHeightEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridRowHeight[] {
			new Syncfusion.Windows.Forms.Grid.GridRowHeight(0, 29)});
			this.gridControlOvertimeRequestOpenPeriods.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
			this.gridControlOvertimeRequestOpenPeriods.Size = new System.Drawing.Size(959, 52);
			this.gridControlOvertimeRequestOpenPeriods.SmartSizeBox = false;
			this.gridControlOvertimeRequestOpenPeriods.TabIndex = 26;
			this.gridControlOvertimeRequestOpenPeriods.Text = "gridControl1";
			this.gridControlOvertimeRequestOpenPeriods.ThemesEnabled = true;
			this.gridControlOvertimeRequestOpenPeriods.UseRightToLeftCompatibleTextBox = true;
			this.gridControlOvertimeRequestOpenPeriods.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gridControlOvertimeRequestOpenPeriods_MouseDown);

			// 
			// contextMenuStripOpenPeriodsGrid
			// 
			this.contextMenuStripOpenPeriodsGrid.ImageScalingSize = new System.Drawing.Size(32, 32);
			this.contextMenuStripOpenPeriodsGrid.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.toolStripMenuItemFromToPeriod,
			this.toolStripMenuItemRollingPeriod,
			this.toolStripMenuItemDelete,
			this.toolStripSeparator2,
			this.toolStripMenuItemMoveUp,
			this.toolStripMenuItemMoveDown});
			this.contextMenuStripOpenPeriodsGrid.Name = "contextMenuStripOpenPeriodsGrid";
			this.contextMenuStripOpenPeriodsGrid.ShowImageMargin = false;
			this.contextMenuStripOpenPeriodsGrid.Size = new System.Drawing.Size(278, 190);

			// 
			// contextMenuStripOvertimeRequestOpenPeriodsGrid
			// 
			this.contextMenuStripOvertimeRequestOpenPeriodsGrid.ImageScalingSize = new System.Drawing.Size(32, 32);
			this.contextMenuStripOvertimeRequestOpenPeriodsGrid.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
				this.toolStripMenuItemOvertimeRequestFromToPeriod,
				this.toolStripMenuItemOvertimeRequestRollingPeriod,
				this.toolStripMenuItemOvertimeRequestDelete,
				this.toolStripSeparator2OvertimeRequest,
				this.toolStripMenuItemOvertimeRequestMoveUp,
				this.toolStripMenuItemOvertimeRequestMoveDown});
			this.contextMenuStripOvertimeRequestOpenPeriodsGrid.Name = "contextMenuStripOvertimeRequestOpenPeriodsGrid";
			this.contextMenuStripOvertimeRequestOpenPeriodsGrid.ShowImageMargin = false;
			this.contextMenuStripOvertimeRequestOpenPeriodsGrid.Size = new System.Drawing.Size(278, 190);

			// 
			// toolStripMenuItemOvertimeRequestFromToPeriod
			// 
			this.toolStripMenuItemOvertimeRequestFromToPeriod.Name = "toolStripMenuItemOvertimeRequestFromToPeriod";
			this.toolStripMenuItemOvertimeRequestFromToPeriod.Size = new System.Drawing.Size(277, 36);
			this.toolStripMenuItemOvertimeRequestFromToPeriod.Text = "xxAddFromToPeriod";
			this.toolStripMenuItemOvertimeRequestFromToPeriod.Click += new System.EventHandler(this.toolStripMenuItemOvertimeRequestFromToPeriod_Click);
			// 
			// toolStripMenuItemOvertimeRequestRollingPeriod
			// 
			this.toolStripMenuItemOvertimeRequestRollingPeriod.Name = "toolStripMenuItemOvertimeRequestRollingPeriod";
			this.toolStripMenuItemOvertimeRequestRollingPeriod.Size = new System.Drawing.Size(277, 36);
			this.toolStripMenuItemOvertimeRequestRollingPeriod.Text = "xxAddRollingPeriod";
			this.toolStripMenuItemOvertimeRequestRollingPeriod.Click += new System.EventHandler(this.toolStripMenuItemOvertimeRequestRollingPeriod_Click);
			// 
			// toolStripMenuItemOvertimeRequestDelete
			// 
			this.toolStripMenuItemOvertimeRequestDelete.Name = "toolStripMenuItemOvertimeRequestDelete";
			this.toolStripMenuItemOvertimeRequestDelete.Size = new System.Drawing.Size(277, 36);
			this.toolStripMenuItemOvertimeRequestDelete.Text = "xxDelete";
			this.toolStripMenuItemOvertimeRequestDelete.Click += new System.EventHandler(this.toolStripMenuItemOvertimeRequestDelete_Click);
			// 
			// toolStripSeparator2OvertimeRequest
			// 
			this.toolStripSeparator2OvertimeRequest.Name = "toolStripSeparator2OvertimeRequest";
			this.toolStripSeparator2OvertimeRequest.Size = new System.Drawing.Size(274, 6);
			// 
			// toolStripMenuItemOvertimeRequestMoveUp
			// 
			this.toolStripMenuItemOvertimeRequestMoveUp.Name = "toolStripMenuItemOvertimeRequestMoveUp";
			this.toolStripMenuItemOvertimeRequestMoveUp.Size = new System.Drawing.Size(277, 36);
			this.toolStripMenuItemOvertimeRequestMoveUp.Text = "xxMoveUp";
			this.toolStripMenuItemOvertimeRequestMoveUp.Click += new System.EventHandler(this.toolStripMenuItemOvertimeRequestMoveUp_Click);
			// 
			// toolStripMenuItemOvertimeRequestMoveDown
			// 
			this.toolStripMenuItemOvertimeRequestMoveDown.Name = "toolStripMenuItemOvertimeRequestMoveDown";
			this.toolStripMenuItemOvertimeRequestMoveDown.Size = new System.Drawing.Size(277, 36);
			this.toolStripMenuItemOvertimeRequestMoveDown.Text = "xxMoveDown";
			this.toolStripMenuItemOvertimeRequestMoveDown.Click += new System.EventHandler(this.toolStripMenuItemOvertimeRequestMoveDown_Click);

			// 
			// toolStripMenuItemFromToPeriod
			// 
			this.toolStripMenuItemFromToPeriod.Name = "toolStripMenuItemFromToPeriod";
			this.toolStripMenuItemFromToPeriod.Size = new System.Drawing.Size(277, 36);
			this.toolStripMenuItemFromToPeriod.Text = "xxAddFromToPeriod";
			this.toolStripMenuItemFromToPeriod.Click += new System.EventHandler(this.toolStripMenuItemFromToPeriod_Click);
			// 
			// toolStripMenuItemRollingPeriod
			// 
			this.toolStripMenuItemRollingPeriod.Name = "toolStripMenuItemRollingPeriod";
			this.toolStripMenuItemRollingPeriod.Size = new System.Drawing.Size(277, 36);
			this.toolStripMenuItemRollingPeriod.Text = "xxAddRollingPeriod";
			this.toolStripMenuItemRollingPeriod.Click += new System.EventHandler(this.toolStripMenuItemRollingPeriod_Click);
			// 
			// toolStripMenuItemDelete
			// 
			this.toolStripMenuItemDelete.Name = "toolStripMenuItemDelete";
			this.toolStripMenuItemDelete.Size = new System.Drawing.Size(277, 36);
			this.toolStripMenuItemDelete.Text = "xxDelete";
			this.toolStripMenuItemDelete.Click += new System.EventHandler(this.toolStripMenuItemDelete_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(274, 6);
			// 
			// toolStripMenuItemMoveUp
			// 
			this.toolStripMenuItemMoveUp.Name = "toolStripMenuItemMoveUp";
			this.toolStripMenuItemMoveUp.Size = new System.Drawing.Size(277, 36);
			this.toolStripMenuItemMoveUp.Text = "xxMoveUp";
			this.toolStripMenuItemMoveUp.Click += new System.EventHandler(this.toolStripMenuItemMoveUp_Click);
			// 
			// toolStripMenuItemMoveDown
			// 
			this.toolStripMenuItemMoveDown.Name = "toolStripMenuItemMoveDown";
			this.toolStripMenuItemMoveDown.Size = new System.Drawing.Size(277, 36);
			this.toolStripMenuItemMoveDown.Text = "xxMoveDown";
			this.toolStripMenuItemMoveDown.Click += new System.EventHandler(this.toolStripMenuItemMoveDown_Click);
			// 
			// tabPageAdvShiftTradeRequest
			// 
			this.tabPageAdvShiftTradeRequest.Controls.Add(this.tableLayoutPanelShiftTrade);
			this.tabPageAdvShiftTradeRequest.Image = null;
			this.tabPageAdvShiftTradeRequest.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageAdvShiftTradeRequest.Location = new System.Drawing.Point(1, 45);
			this.tabPageAdvShiftTradeRequest.Name = "tabPageAdvShiftTradeRequest";
			this.tabPageAdvShiftTradeRequest.ShowCloseButton = true;
			this.tabPageAdvShiftTradeRequest.Size = new System.Drawing.Size(965, 410);
			this.tabPageAdvShiftTradeRequest.TabBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(191)))), ((int)(((byte)(234)))));
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
			this.tableLayoutPanelShiftTrade.Controls.Add(this.panel10, 0, 2);
			this.tableLayoutPanelShiftTrade.Controls.Add(this.panel5, 0, 8);
			this.tableLayoutPanelShiftTrade.Controls.Add(this.panelOpenForShiftTrade, 0, 0);
			this.tableLayoutPanelShiftTrade.Controls.Add(this.panelTolerance, 0, 4);
			this.tableLayoutPanelShiftTrade.Controls.Add(this.minMaxIntegerTextBoxControl1, 0, 1);
			this.tableLayoutPanelShiftTrade.Controls.Add(this.twoListSelectorMatchingSkills, 0, 7);
			this.tableLayoutPanelShiftTrade.Controls.Add(this.panelMatchingSkills, 0, 6);
			this.tableLayoutPanelShiftTrade.Controls.Add(this.tableLayoutPanel2, 0, 5);
			this.tableLayoutPanelShiftTrade.Controls.Add(this.checkBoxAdvAutoGrant, 0, 9);
			this.tableLayoutPanelShiftTrade.Controls.Add(this.checkBoxAdvAnonymousTrading, 0, 10);
			this.tableLayoutPanelShiftTrade.Controls.Add(this.checkBoxAdvLockTrading, 0, 11);
			this.tableLayoutPanelShiftTrade.Controls.Add(this.tableLayoutPanel3, 0, 3);
			this.tableLayoutPanelShiftTrade.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelShiftTrade.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanelShiftTrade.Name = "tableLayoutPanelShiftTrade";
			this.tableLayoutPanelShiftTrade.RowCount = 13;
			this.tableLayoutPanelShiftTrade.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanelShiftTrade.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanelShiftTrade.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanelShiftTrade.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanelShiftTrade.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanelShiftTrade.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanelShiftTrade.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanelShiftTrade.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 185F));
			this.tableLayoutPanelShiftTrade.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanelShiftTrade.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanelShiftTrade.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanelShiftTrade.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanelShiftTrade.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelShiftTrade.Size = new System.Drawing.Size(965, 418);
			this.tableLayoutPanelShiftTrade.TabIndex = 0;
			// 
			// panel10
			// 
			this.panel10.BackColor = System.Drawing.Color.LightSteelBlue;
			this.tableLayoutPanelShiftTrade.SetColumnSpan(this.panel10, 2);
			this.panel10.Controls.Add(this.label1);
			this.panel10.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel10.Location = new System.Drawing.Point(3, 83);
			this.panel10.Name = "panel10";
			this.panel10.Size = new System.Drawing.Size(1325, 34);
			this.panel10.TabIndex = 17;
			// 
			// label1
			// 
			this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label1.AutoSize = true;
			this.label1.BackColor = System.Drawing.Color.Transparent;
			this.label1.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.ForeColor = System.Drawing.Color.GhostWhite;
			this.label1.Location = new System.Drawing.Point(3, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(387, 28);
			this.label1.TabIndex = 0;
			this.label1.Text = "xxDescriptionOfMaximumWorkdayRule";
			// 
			// panel5
			// 
			this.panel5.BackColor = System.Drawing.Color.LightSteelBlue;
			this.tableLayoutPanelShiftTrade.SetColumnSpan(this.panel5, 2);
			this.panel5.Controls.Add(this.label3);
			this.panel5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel5.Location = new System.Drawing.Point(3, 388);
			this.panel5.Name = "panel5";
			this.panel5.Size = new System.Drawing.Size(959, 34);
			this.panel5.TabIndex = 9;
			// 
			// label3
			// 
			this.label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label3.AutoSize = true;
			this.label3.BackColor = System.Drawing.Color.Transparent;
			this.label3.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label3.ForeColor = System.Drawing.Color.GhostWhite;
			this.label3.Location = new System.Drawing.Point(3, 8);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(213, 36);
			this.label3.TabIndex = 0;
			this.label3.Text = "xxMiscellaneous";
			// 
			// panelOpenForShiftTrade
			// 
			this.panelOpenForShiftTrade.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(191)))), ((int)(((byte)(234)))));
			this.tableLayoutPanelShiftTrade.SetColumnSpan(this.panelOpenForShiftTrade, 2);
			this.panelOpenForShiftTrade.Controls.Add(this.labelOpenForShiftTrade);
			this.panelOpenForShiftTrade.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelOpenForShiftTrade.Location = new System.Drawing.Point(3, 3);
			this.panelOpenForShiftTrade.Name = "panelOpenForShiftTrade";
			this.panelOpenForShiftTrade.Size = new System.Drawing.Size(959, 34);
			this.panelOpenForShiftTrade.TabIndex = 0;
			// 
			// labelOpenForShiftTrade
			// 
			this.labelOpenForShiftTrade.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelOpenForShiftTrade.AutoSize = true;
			this.labelOpenForShiftTrade.BackColor = System.Drawing.Color.Transparent;
			this.labelOpenForShiftTrade.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelOpenForShiftTrade.ForeColor = System.Drawing.Color.GhostWhite;
			this.labelOpenForShiftTrade.Location = new System.Drawing.Point(3, 8);
			this.labelOpenForShiftTrade.Name = "labelOpenForShiftTrade";
			this.labelOpenForShiftTrade.Size = new System.Drawing.Size(382, 36);
			this.labelOpenForShiftTrade.TabIndex = 0;
			this.labelOpenForShiftTrade.Text = "xxOpenForShiftTradeRequests";
			// 
			// panelTolerance
			// 
			this.panelTolerance.BackColor = System.Drawing.Color.LightSteelBlue;
			this.tableLayoutPanelShiftTrade.SetColumnSpan(this.panelTolerance, 2);
			this.panelTolerance.Controls.Add(this.labelTolerance);
			this.panelTolerance.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelTolerance.Location = new System.Drawing.Point(3, 83);
			this.panelTolerance.Name = "panelTolerance";
			this.panelTolerance.Size = new System.Drawing.Size(959, 34);
			this.panelTolerance.TabIndex = 2;
			// 
			// labelTolerance
			// 
			this.labelTolerance.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelTolerance.AutoSize = true;
			this.labelTolerance.BackColor = System.Drawing.Color.Transparent;
			this.labelTolerance.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelTolerance.ForeColor = System.Drawing.Color.GhostWhite;
			this.labelTolerance.Location = new System.Drawing.Point(3, 8);
			this.labelTolerance.Name = "labelTolerance";
			this.labelTolerance.Size = new System.Drawing.Size(479, 36);
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
			this.minMaxIntegerTextBoxControl1.Location = new System.Drawing.Point(6, 46);
			this.minMaxIntegerTextBoxControl1.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
			this.minMaxIntegerTextBoxControl1.MaxTextBoxValue = 1;
			this.minMaxIntegerTextBoxControl1.MinTextBoxValue = 1;
			this.minMaxIntegerTextBoxControl1.Name = "minMaxIntegerTextBoxControl1";
			this.minMaxIntegerTextBoxControl1.Size = new System.Drawing.Size(560, 28);
			this.minMaxIntegerTextBoxControl1.TabIndex = 5;
			this.minMaxIntegerTextBoxControl1.Validating += new System.ComponentModel.CancelEventHandler(this.minMaxIntegerTextBoxControl1_Validating);
			this.minMaxIntegerTextBoxControl1.Validated += new System.EventHandler(this.minMaxIntegerTextBoxControl1_Validated);
			// 
			// twoListSelectorMatchingSkills
			// 
			this.twoListSelectorMatchingSkills.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.twoListSelectorMatchingSkills.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
			this.tableLayoutPanelShiftTrade.SetColumnSpan(this.twoListSelectorMatchingSkills, 2);
			this.twoListSelectorMatchingSkills.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.twoListSelectorMatchingSkills.Location = new System.Drawing.Point(3, 203);
			this.twoListSelectorMatchingSkills.Name = "twoListSelectorMatchingSkills";
			this.twoListSelectorMatchingSkills.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.twoListSelectorMatchingSkills.Size = new System.Drawing.Size(959, 179);
			this.twoListSelectorMatchingSkills.TabIndex = 11;
			// 
			// panelMatchingSkills
			// 
			this.panelMatchingSkills.BackColor = System.Drawing.Color.LightSteelBlue;
			this.tableLayoutPanelShiftTrade.SetColumnSpan(this.panelMatchingSkills, 2);
			this.panelMatchingSkills.Controls.Add(this.labelMatchingSkills);
			this.panelMatchingSkills.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelMatchingSkills.Location = new System.Drawing.Point(3, 163);
			this.panelMatchingSkills.Name = "panelMatchingSkills";
			this.panelMatchingSkills.Size = new System.Drawing.Size(959, 34);
			this.panelMatchingSkills.TabIndex = 6;
			// 
			// labelMatchingSkills
			// 
			this.labelMatchingSkills.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelMatchingSkills.AutoSize = true;
			this.labelMatchingSkills.BackColor = System.Drawing.Color.Transparent;
			this.labelMatchingSkills.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelMatchingSkills.ForeColor = System.Drawing.Color.GhostWhite;
			this.labelMatchingSkills.Location = new System.Drawing.Point(3, 8);
			this.labelMatchingSkills.Name = "labelMatchingSkills";
			this.labelMatchingSkills.Size = new System.Drawing.Size(221, 36);
			this.labelMatchingSkills.TabIndex = 0;
			this.labelMatchingSkills.Text = "xxMatchingSkills";
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 3;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 86F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 192F));
			this.tableLayoutPanel2.Controls.Add(this.timeSpanTextBox1, 1, 0);
			this.tableLayoutPanel2.Controls.Add(this.labelTolerancePosNeg, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.labelHMm, 2, 0);
			this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 120);
			this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 1;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(360, 35);
			this.tableLayoutPanel2.TabIndex = 8;
			// 
			// timeSpanTextBox1
			// 
			this.timeSpanTextBox1.AlignTextBoxText = System.Windows.Forms.HorizontalAlignment.Right;
			this.timeSpanTextBox1.AllowNegativeValues = false;
			this.timeSpanTextBox1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.timeSpanTextBox1.DefaultInterpretAsMinutes = false;
			this.timeSpanTextBox1.Location = new System.Drawing.Point(86, 3);
			this.timeSpanTextBox1.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this.timeSpanTextBox1.MaximumValue = System.TimeSpan.Parse("4.03:00:00");
			this.timeSpanTextBox1.Name = "timeSpanTextBox1";
			this.timeSpanTextBox1.Size = new System.Drawing.Size(82, 32);
			this.timeSpanTextBox1.TabIndex = 9;
			this.timeSpanTextBox1.TimeFormat = Teleopti.Ccc.Domain.InterfaceLegacy.Domain.TimeFormatsType.HoursMinutes;
			this.timeSpanTextBox1.TimeSpanBoxHeight = 39;
			this.timeSpanTextBox1.TimeSpanBoxWidth = 65535;
			this.timeSpanTextBox1.Leave += new System.EventHandler(this.timeSpanTextBox1_Leave);
			// 
			// labelTolerancePosNeg
			// 
			this.labelTolerancePosNeg.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelTolerancePosNeg.AutoSize = true;
			this.labelTolerancePosNeg.Location = new System.Drawing.Point(3, 0);
			this.labelTolerancePosNeg.Name = "labelTolerancePosNeg";
			this.labelTolerancePosNeg.Size = new System.Drawing.Size(73, 35);
			this.labelTolerancePosNeg.TabIndex = 8;
			this.labelTolerancePosNeg.Text = "xxPosNeg";
			// 
			// labelHMm
			// 
			this.labelHMm.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelHMm.AutoSize = true;
			this.labelHMm.Location = new System.Drawing.Point(168, 1);
			this.labelHMm.Margin = new System.Windows.Forms.Padding(0);
			this.labelHMm.Name = "labelHMm";
			this.labelHMm.Size = new System.Drawing.Size(161, 32);
			this.labelHMm.TabIndex = 9;
			this.labelHMm.Text = "xxHColonMM";
			// 
			// checkBoxAdvAutoGrant
			// 
			this.checkBoxAdvAutoGrant.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.checkBoxAdvAutoGrant.BeforeTouchSize = new System.Drawing.Size(349, 24);
			this.checkBoxAdvAutoGrant.ImageCheckBoxSize = new System.Drawing.Size(26, 26);
			this.checkBoxAdvAutoGrant.Location = new System.Drawing.Point(3, 433);
			this.checkBoxAdvAutoGrant.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxAdvAutoGrant.Name = "checkBoxAdvAutoGrant";
			this.checkBoxAdvAutoGrant.Size = new System.Drawing.Size(349, 24);
			this.checkBoxAdvAutoGrant.TabIndex = 14;
			this.checkBoxAdvAutoGrant.Text = "xxCheckToEnableAutoGrant";
			this.checkBoxAdvAutoGrant.ThemesEnabled = false;
			this.checkBoxAdvAutoGrant.CheckStateChanged += new System.EventHandler(this.checkBoxAdvAutoGrant_CheckStateChanged);
			// 
			// checkBoxAdvAnonymousTrading
			// 
			this.checkBoxAdvAnonymousTrading.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.checkBoxAdvAnonymousTrading.BeforeTouchSize = new System.Drawing.Size(349, 24);
			this.checkBoxAdvAnonymousTrading.ImageCheckBoxSize = new System.Drawing.Size(26, 26);
			this.checkBoxAdvAnonymousTrading.Location = new System.Drawing.Point(3, 472);
			this.checkBoxAdvAnonymousTrading.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxAdvAnonymousTrading.Name = "checkBoxAdvAnonymousTrading";
			this.checkBoxAdvAnonymousTrading.Size = new System.Drawing.Size(349, 24);
			this.checkBoxAdvAnonymousTrading.TabIndex = 15;
			this.checkBoxAdvAnonymousTrading.Text = "xxAnonymousBulletinBoardTrading";
			this.checkBoxAdvAnonymousTrading.ThemesEnabled = false;
			this.checkBoxAdvAnonymousTrading.CheckStateChanged += new System.EventHandler(this.checkBoxAdvAnonymousTrading_CheckStateChanged);
			// 
			// checkBoxAdvLockTrading
			// 
			this.checkBoxAdvLockTrading.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.checkBoxAdvLockTrading.BeforeTouchSize = new System.Drawing.Size(349, 24);
			this.checkBoxAdvLockTrading.ImageCheckBoxSize = new System.Drawing.Size(26, 26);
			this.checkBoxAdvLockTrading.Location = new System.Drawing.Point(3, 511);
			this.checkBoxAdvLockTrading.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxAdvLockTrading.Name = "checkBoxAdvLockTrading";
			this.checkBoxAdvLockTrading.Size = new System.Drawing.Size(349, 24);
			this.checkBoxAdvLockTrading.TabIndex = 16;
			this.checkBoxAdvLockTrading.Text = "xxLockBulletinBoardTrading";
			this.checkBoxAdvLockTrading.ThemesEnabled = false;
			this.checkBoxAdvLockTrading.CheckStateChanged += new System.EventHandler(this.checkBoxAdvLockTrading_CheckStateChanged);
			// 
			// tabPageAdvETOTRequest
			// 
			this.tabPageAdvETOTRequest.Controls.Add(this.tableLayoutPanelETOTRequest);
			this.tabPageAdvETOTRequest.Image = null;
			this.tabPageAdvETOTRequest.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageAdvETOTRequest.Location = new System.Drawing.Point(1, 45);
			this.tabPageAdvETOTRequest.Name = "tabPageAdvETOTRequest";
			this.tabPageAdvETOTRequest.ShowCloseButton = true;
			this.tabPageAdvETOTRequest.Size = new System.Drawing.Size(965, 410);
			this.tabPageAdvETOTRequest.TabBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(191)))), ((int)(((byte)(234)))));
			this.tabPageAdvETOTRequest.TabIndex = 4;
			this.tabPageAdvETOTRequest.Text = "xxETOTRequests";
			this.tabPageAdvETOTRequest.ThemesEnabled = false;
			// 
			// tableLayoutPanelETOTRequest
			// 
			this.tableLayoutPanelETOTRequest.AutoSize = true;
			this.tableLayoutPanelETOTRequest.ColumnCount = 1;
			this.tableLayoutPanelETOTRequest.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelETOTRequest.Controls.Add(this.panelOvertimeRequestHeaderBasic, 0, 0);
			this.tableLayoutPanelETOTRequest.Controls.Add(this.tableLayoutPanelOvertimeStaffingCheckMethodSetting, 0, 1);
			this.tableLayoutPanelETOTRequest.Controls.Add(this.tableLayoutPanelOvertimeProbablityAndPrimarySkill, 0, 2);
			this.tableLayoutPanelETOTRequest.Controls.Add(this.tableLayoutPanelOvertimeMaximumSetting, 0, 3);
			this.tableLayoutPanelETOTRequest.Controls.Add(this.tableLayoutPanelOvertimeMaximumContinuousWorkTimeSetting, 0, 4);
			this.tableLayoutPanelETOTRequest.Controls.Add(this.tableLayoutPanelOpenForOvertimeRequests, 0, 5);
			this.tableLayoutPanelETOTRequest.Controls.Add(this.gridControlOvertimeRequestOpenPeriods, 0, 6);

			this.tableLayoutPanelETOTRequest.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelETOTRequest.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanelETOTRequest.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanelETOTRequest.Name = "tableLayoutPanelETOTRequest";
			this.tableLayoutPanelETOTRequest.RowCount = 8;
			this.tableLayoutPanelETOTRequest.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanelETOTRequest.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanelETOTRequest.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanelETOTRequest.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanelETOTRequest.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanelETOTRequest.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanelETOTRequest.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelETOTRequest.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanelETOTRequest.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanelETOTRequest.Size = new System.Drawing.Size(965, 410);
			this.tableLayoutPanelETOTRequest.TabIndex = 4;
			// 
			// panelOvertimeRequestHeaderBasic
			// 
			this.panelOvertimeRequestHeaderBasic.AutoSize = true;
			this.panelOvertimeRequestHeaderBasic.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(191)))), ((int)(((byte)(234)))));
			this.tableLayoutPanelETOTRequest.SetColumnSpan(this.panelOvertimeRequestHeaderBasic, 2);
			this.panelOvertimeRequestHeaderBasic.Controls.Add(this.labelOvertimeRequestBasic);
			this.panelOvertimeRequestHeaderBasic.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelOvertimeRequestHeaderBasic.Location = new System.Drawing.Point(3, 3);
			this.panelOvertimeRequestHeaderBasic.Name = "panelOvertimeRequestHeaderBasic";
			this.panelOvertimeRequestHeaderBasic.Size = new System.Drawing.Size(959, 34);
			this.panelOvertimeRequestHeaderBasic.TabIndex = 19;
			// 
			// tableLayoutPanel3
			// 
			this.tableLayoutPanel3.ColumnCount = 2;
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel3.Controls.Add(this.daysTbx, 0, 0);
			this.tableLayoutPanel3.Controls.Add(this.label2, 1, 0);
			this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 123);
			this.tableLayoutPanel3.Name = "tableLayoutPanel3";
			this.tableLayoutPanel3.RowCount = 1;
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
			this.tableLayoutPanel3.Size = new System.Drawing.Size(965, 34);
			this.tableLayoutPanel3.TabIndex = 20;
			// 
			// daysTbx
			// 
			this.daysTbx.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.daysTbx.Location = new System.Drawing.Point(3, 3);
			this.daysTbx.Maximum = new decimal(new int[] {
			15,
			0,
			0,
			0});
			this.daysTbx.Minimum = new decimal(new int[] {
			1,
			0,
			0,
			0});
			this.daysTbx.Name = "daysTbx";
			this.daysTbx.Size = new System.Drawing.Size(94, 31);
			this.daysTbx.TabIndex = 20;
			this.daysTbx.Value = new decimal(new int[] {
			1,
			0,
			0,
			0});
			this.daysTbx.Leave += new System.EventHandler(this.daysTbx_Leave);
			// 
			// label2
			// 
			this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(103, 4);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(67, 25);
			this.label2.TabIndex = 21;
			this.label2.Text = "xxDays";
			// 
			// labelOvertimeRequestBasic
			// 
			this.labelOvertimeRequestBasic.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelOvertimeRequestBasic.AutoSize = true;
			this.labelOvertimeRequestBasic.BackColor = System.Drawing.Color.Transparent;
			this.labelOvertimeRequestBasic.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelOvertimeRequestBasic.ForeColor = System.Drawing.Color.GhostWhite;
			this.labelOvertimeRequestBasic.Location = new System.Drawing.Point(3, 8);
			this.labelOvertimeRequestBasic.Name = "labelOvertimeRequestBasic";
			this.labelOvertimeRequestBasic.Size = new System.Drawing.Size(104, 36);
			this.labelOvertimeRequestBasic.TabIndex = 0;
			this.labelOvertimeRequestBasic.Text = "xxBasic";
			// 
			// checkBoxAdvOvertimeProbability
			// 
			this.checkBoxAdvOvertimeProbability.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.checkBoxAdvOvertimeProbability.BeforeTouchSize = new System.Drawing.Size(220, 24);
			this.checkBoxAdvOvertimeProbability.ImageCheckBoxSize = new System.Drawing.Size(26, 26);
			this.checkBoxAdvOvertimeProbability.Location = new System.Drawing.Point(3, 43);
			this.checkBoxAdvOvertimeProbability.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxAdvOvertimeProbability.Name = "checkBoxAdvOvertimeProbability";
			this.checkBoxAdvOvertimeProbability.Size = new System.Drawing.Size(220, 24);
			this.checkBoxAdvOvertimeProbability.TabIndex = 0;
			this.checkBoxAdvOvertimeProbability.Text = "xxEnableOvertimeProbability";
			this.checkBoxAdvOvertimeProbability.ThemesEnabled = false;
			//
			// checkBoxUsePrimarySkill
			this.checkBoxUsePrimarySkill.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.checkBoxUsePrimarySkill.BeforeTouchSize = new System.Drawing.Size(220, 24);
			this.checkBoxUsePrimarySkill.ImageCheckBoxSize = new System.Drawing.Size(26, 26);
			this.checkBoxUsePrimarySkill.Location = new System.Drawing.Point(3, 43);
			this.checkBoxUsePrimarySkill.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxUsePrimarySkill.Name = "checkBoxUsePrimarySkill";
			this.checkBoxUsePrimarySkill.Size = new System.Drawing.Size(220, 24);
			this.checkBoxUsePrimarySkill.TabIndex = 0;
			this.checkBoxUsePrimarySkill.Text = "xxUsePrimarySkill";
			this.checkBoxUsePrimarySkill.ThemesEnabled = false;
			// 
			// checkBoxAdvOvertimeMaximumEnabled
			// 
			this.checkBoxAdvOvertimeMaximumEnabled.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.checkBoxAdvOvertimeMaximumEnabled.BeforeTouchSize = new System.Drawing.Size(250, 24);
			this.checkBoxAdvOvertimeMaximumEnabled.ImageCheckBoxSize = new System.Drawing.Size(26, 26);
			this.checkBoxAdvOvertimeMaximumEnabled.Location = new System.Drawing.Point(3, 73);
			this.checkBoxAdvOvertimeMaximumEnabled.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
			this.checkBoxAdvOvertimeMaximumEnabled.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxAdvOvertimeMaximumEnabled.Name = "checkBoxAdvOvertimeMaximumEnabled";
			this.checkBoxAdvOvertimeMaximumEnabled.Size = new System.Drawing.Size(250, 24);
			this.checkBoxAdvOvertimeMaximumEnabled.Text = "xxOvertimeRequestMaximum";
			this.checkBoxAdvOvertimeMaximumEnabled.TabIndex = 1;
			this.checkBoxAdvOvertimeMaximumEnabled.ThemesEnabled = false;
			this.checkBoxAdvOvertimeMaximumEnabled.CheckStateChanged += new System.EventHandler(this.CheckBoxAdvOvertimeMaximumEnabled_CheckStateChanged);
			// 
			// checkBoxAdvOvertimeMaximumContinuousWorkTimeEnabled
			// 
			this.checkBoxAdvOvertimeMaximumContinuousWorkTimeEnabled.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.checkBoxAdvOvertimeMaximumContinuousWorkTimeEnabled.BeforeTouchSize = new System.Drawing.Size(250, 24);
			this.checkBoxAdvOvertimeMaximumContinuousWorkTimeEnabled.ImageCheckBoxSize = new System.Drawing.Size(26, 26);
			this.checkBoxAdvOvertimeMaximumContinuousWorkTimeEnabled.Location = new System.Drawing.Point(3, 73);
			this.checkBoxAdvOvertimeMaximumContinuousWorkTimeEnabled.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
			this.checkBoxAdvOvertimeMaximumContinuousWorkTimeEnabled.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxAdvOvertimeMaximumContinuousWorkTimeEnabled.Name = "checkBoxAdvOvertimeMaximumContinuousWorkTimeEnabled";
			this.checkBoxAdvOvertimeMaximumContinuousWorkTimeEnabled.Size = new System.Drawing.Size(250, 24);
			this.checkBoxAdvOvertimeMaximumContinuousWorkTimeEnabled.Text = "xxOvertimeRequestMaximumContinuousWorkTime";
			this.checkBoxAdvOvertimeMaximumContinuousWorkTimeEnabled.TabIndex = 4;
			this.checkBoxAdvOvertimeMaximumContinuousWorkTimeEnabled.ThemesEnabled = false;
			this.checkBoxAdvOvertimeMaximumContinuousWorkTimeEnabled.CheckStateChanged += new System.EventHandler(this.checkBoxAdvOvertimeMaximumContinuousWorkTimeEnabled_CheckStateChanged);
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
			// WorkflowControlSetView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoScroll = true;
			this.BackColor = System.Drawing.SystemColors.Window;
			this.Controls.Add(this.tableLayoutPanelMain);
			this.Controls.Add(this.gradientPanelHeader);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "WorkflowControlSetView";
			this.Size = new System.Drawing.Size(978, 682);
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
			this.panel9.ResumeLayout(false);
			this.panel9.PerformLayout();
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
			((System.ComponentModel.ISupportInitialize)(this.radioButtonAdvFairnessEqual)).EndInit();
			this.panel7.ResumeLayout(false);
			this.panel7.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonAdvSeniority)).EndInit();
			this.tabPageAdvAbsenceRequests.ResumeLayout(false);
			this.tabPageAdvAbsenceRequests.PerformLayout();
			this.tableLayoutPanelAbsenceRequestPeriods.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvViewpoint.Calendar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvViewpoint)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.gridControlVisualisation)).EndInit();
			this.tableLayoutPanelNextPreviousPeriod.ResumeLayout(false);
			this.tableLayoutPanelAbsenceRequestMiscellaneous.ResumeLayout(false);
			this.tableLayoutPanelAbsenceRequestMiscellaneous.PerformLayout();
			this.tableLayoutPanelWaitlist.ResumeLayout(false);
			this.tableLayoutPanelWaitlist.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxEnableAbsenceRequestWaitlisting)).EndInit();
			this.tableLayoutPanelAbsenceProbability.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.checkBoxEnableAbsenceProbability)).EndInit();
			this.tableLayoutPanelAbsenceRequestCancellation.ResumeLayout(false);
			this.tableLayoutPanelAbsenceRequestCancellation.PerformLayout();
			this.tableLayoutPanelAbsenceRequestExpiration.ResumeLayout(false);
			this.tableLayoutPanelAbsenceRequestExpiration.PerformLayout();
			this.tableLayoutPanelOpenForAbsenceRequests.ResumeLayout(false);
			this.tableLayoutPanelOpenForOvertimeRequests.ResumeLayout(false);
			this.tableLayoutPanelOpenForAbsenceRequests.PerformLayout();
			this.tableLayoutPanelOpenForOvertimeRequests.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridControlAbsenceRequestOpenPeriods)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.gridControlOvertimeRequestOpenPeriods)).EndInit();
			this.contextMenuStripOpenPeriodsGrid.ResumeLayout(false);
			this.contextMenuStripOvertimeRequestOpenPeriodsGrid.ResumeLayout(false);
			this.tabPageAdvShiftTradeRequest.ResumeLayout(false);
			this.tableLayoutPanelShiftTrade.ResumeLayout(false);
			this.panel10.ResumeLayout(false);
			this.panel10.PerformLayout();
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
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvAnonymousTrading)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvLockTrading)).EndInit();
			this.tableLayoutPanel3.ResumeLayout(false);
			this.tableLayoutPanel3.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.daysTbx)).EndInit();
			this.tabPageAdvETOTRequest.ResumeLayout(false);
			this.tabPageAdvETOTRequest.PerformLayout();
			this.tableLayoutPanelETOTRequest.ResumeLayout(false);
			this.tableLayoutPanelETOTRequest.PerformLayout();
			this.panelOvertimeRequestHeaderBasic.ResumeLayout(false);
			this.panelOvertimeRequestHeaderBasic.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvOvertimeProbability)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxUsePrimarySkill)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvOvertimeMaximumEnabled)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvOvertimeMaximumContinuousWorkTimeEnabled)).EndInit();
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
		private Syncfusion.Windows.Forms.ButtonAdv buttonNew;
		private TableLayoutPanel tableLayoutPanel1;
		private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel5;
		private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel1;
		private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelChangeInfo;
		private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvWorkflowControlSet;
		private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxOvertimeRequestMaximumTimeHandleType;
		private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxOvertimeRequestStaffingCheckMethod;
		private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxOvertimeRequestMaximumContinuousWorkTimeHandleType;
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
		private Syncfusion.Windows.Forms.Grid.GridControl gridControlOvertimeRequestOpenPeriods;
		private TableLayoutPanel tableLayoutPanelOpenForAbsenceRequests;
		private TableLayoutPanel tableLayoutPanelOpenForOvertimeRequests;
		private TableLayoutPanel tableLayoutPanelOvertimeStaffingCheckMethodSetting;
		private TableLayoutPanel tableLayoutPanelOvertimeProbablityAndPrimarySkill;
		private TableLayoutPanel tableLayoutPanelOvertimeMaximumSetting;
		private TableLayoutPanel tableLayoutPanelOvertimeMaximumContinuousWorkTimeSetting;
		private Label labelOpenForAbsenceRequests;
		private Label labelOpenForOvertimeRequests;
		private Label lblOvertimeRequestMaximumTimeHandleType;
		private Label lblOvertimeRequestMinimumRestTimeThreshold;
		private Label lblOvertimeRequestMaximumContinuousWorkTimeHandleType;
		private Syncfusion.Windows.Forms.ButtonAdv buttonDeleteAbsenceRequestPeriod;
		private Syncfusion.Windows.Forms.ButtonAdv buttonDeleteOvertimeRequestPeriod;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAddAbsenceRequestPeriod;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAddOvertimeRequestPeriod;
		private TableLayoutPanel tableLayoutPanelNextPreviousPeriod;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvPreviousProjectionPeriod;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvNextProjectionPeriod;
		private ContextMenuStrip contextMenuStripOpenPeriodsGrid;
		private ContextMenuStrip contextMenuStripOvertimeRequestOpenPeriodsGrid;
		private ToolStripSeparator toolStripSeparator1;
		private ToolStripMenuItem toolStripMenuItemDelete;
		private ToolStripSeparator toolStripSeparator2;
		private ToolStripMenuItem toolStripMenuItemMoveUp;
		private ToolStripMenuItem toolStripMenuItemMoveDown;
		private ToolStripMenuItem toolStripMenuItemFromToPeriod;
		private ToolStripMenuItem toolStripMenuItemRollingPeriod;
		private ToolStripMenuItem toolStripMenuItemOvertimeRequestDelete;
		private ToolStripSeparator toolStripSeparator2OvertimeRequest;
		private ToolStripMenuItem toolStripMenuItemOvertimeRequestMoveUp;
		private ToolStripMenuItem toolStripMenuItemOvertimeRequestMoveDown;
		private ToolStripMenuItem toolStripMenuItemOvertimeRequestFromToPeriod;
		private ToolStripMenuItem toolStripMenuItemOvertimeRequestRollingPeriod;
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
		private DateSelectionFromTo dateSelectionFromToPreferencePeriod;
		private DateSelectionFromTo dateSelectionFromToIsOpen;
		private ErrorProvider errorProvider1;
		private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageAdvShiftTradeRequest;
		private TableLayoutPanel tableLayoutPanelShiftTrade;
		private Panel panelOpenForShiftTrade;
		private Label labelOpenForShiftTrade;
		private Panel panelTolerance;
		private Label labelTolerance;
		private MinMaxIntegerTextBoxControl minMaxIntegerTextBoxControl1;
		private DateOnlyPeriodsVisualizer dateOnlyPeriodsVisualizer1;
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
		private Panel panel9;
		private Label labelAllowedAbsencesForReport;
		private TwoListSelector twoListSelectorAbsencesForReport;
		private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdvAnonymousTrading;
		private Syncfusion.Windows.Forms.Tools.RadioButtonAdv radioButtonAdvSeniority;
		private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdvLockTrading;
		private TableLayoutPanel tableLayoutPanelAbsenceRequestMiscellaneous;
		private Label labelAbsenceRequestMiscellaneous;
		private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxEnableAbsenceRequestWaitlisting;
		private TableLayoutPanel tableLayoutPanelWaitlist;
		private TableLayoutPanel tableLayoutPanelAbsenceRequestCancellation;
		private NullableIntegerTextBox txtAbsenceRequestCancellationThreshold;
		private TimeSpanTextBox timeSpanTextBoxOvertimeRequestMaximumTime;
		private TimeSpanTextBox timeSpanTextBoxOvertimeRequestMaximumContinuousWorkTime;
		private TimeSpanTextBox timeSpanTextBoxOvertimeRequestMinimumRestTimeThreshold;
		private Label labelAbsenceRequestCancellationThreshold;
		private RadioButton radioButtonWaitlistFirstComeFirstServed;
		private RadioButton radioButtonWaitlistBySeniority;
		private TableLayoutPanel tableLayoutPanelAbsenceRequestExpiration;
		private NullableIntegerTextBox txtAbsenceRequestExpiredThreshold;
		private Label labelAbsenceRequestExpriedThreshold;
		private Label labelOvertimeStaffingCheckMethodSetting;
		private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageAdvETOTRequest;
		private TableLayoutPanel tableLayoutPanelETOTRequest;
		private TableLayoutPanel tableLayoutPanelAbsenceProbability;
		private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxEnableAbsenceProbability;
		private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdvOvertimeProbability;
		private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxUsePrimarySkill;
		private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdvOvertimeMaximumEnabled;
		private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdvOvertimeMaximumContinuousWorkTimeEnabled;
		private Panel panelOvertimeRequestHeaderBasic;
		private Label labelOvertimeRequestBasic;
		private Panel panel10;
		private Label label1;
		private TableLayoutPanel tableLayoutPanel3;
		private Label label2;
		private NumericUpDown daysTbx;
	}
}
