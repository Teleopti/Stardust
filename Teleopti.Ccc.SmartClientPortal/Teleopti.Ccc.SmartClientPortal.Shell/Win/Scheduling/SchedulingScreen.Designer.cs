using System.Windows.Forms;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.SpinningProgress;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls.Notes;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Editor;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
    partial class SchedulingScreen
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
                if (_scheduleView != null)
                    _scheduleView.Dispose();
                if (schedulerSplitters1!=null)
                {
                    schedulerSplitters1.MultipleHostControl3.GotFocus -= multipleHostControl3OnGotFocus;
                    schedulerSplitters1.Dispose();
                    schedulerSplitters1 = null;
                }
                
                if (_schedulerMessageBrokerHandler!=null)
                    _schedulerMessageBrokerHandler.Dispose();
                if (_clipboardControl != null)
                    _clipboardControl.Dispose();
                if (_editControl != null)
                    _editControl.Dispose();
                if (_cutPasteHandlerFactory!=null)
                    _cutPasteHandlerFactory.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxFind"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxAddPreferenceRestriction"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxAddStudentAvailability"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxOldFilter"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxMonth"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Syncfusion.Windows.Forms.Tools.RibbonControlAdv.set_MaximizeToolTip(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Syncfusion.Windows.Forms.Tools.RibbonControlAdv.set_MinimizeToolTip(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxRestrictionViewTemp"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxExportToPDFShiftsPerDay"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxRemoveWriteProtection"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxTags"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxChangeTag"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxAutoTag"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxUntagged"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxLockTags"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxAllFulFilledAbsences"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxAllAbsences"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxAllMustHave"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxAllFulfilledMustHave"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxLockStudentAvailability"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxLockRotations"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxLockRestrictions"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxLockPreferences"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxLockAvailability"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxAllUnavailable"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxAllUnAvailable"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxAllShifts"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxAllFulFilledShifts"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxAllFulFilledDaysOff"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxAllFulFilled"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxAllDaysOff"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxAllAvailable"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxAll"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxExportToPDFGraphical"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxExport"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxSwapRaw")]
		private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			Syncfusion.Windows.Forms.Tools.ToolStripTabGroup toolStripTabGroup1 = new Syncfusion.Windows.Forms.Tools.ToolStripTabGroup();
			Syncfusion.Windows.Forms.Tools.ToolStripTabGroup toolStripTabGroup2 = new Syncfusion.Windows.Forms.Tools.ToolStripTabGroup();
			Syncfusion.Windows.Forms.Tools.ToolStripTabGroup toolStripTabGroup3 = new Syncfusion.Windows.Forms.Tools.ToolStripTabGroup();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SchedulingScreen));
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.toolStripSpinningProgressControl1 = new Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.SpinningProgress.ToolStripSpinningProgressControl();
			this.toolStripStatusLabelStatus = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripProgressBar1 = new Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.MetroToolStripProgressBar();
			this.toolStripStatusLabelNumberOfAgents = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripSplitButtonTimeZone = new System.Windows.Forms.ToolStripSplitButton();
			this.toolStripStatusLabelScheduleTag = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripStatusLabelContractTime = new System.Windows.Forms.ToolStripSplitButton();
			this.toolStripMenuItemContractTime = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemWorkTime = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemPaidTime = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemOverTime = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuViews = new Syncfusion.Windows.Forms.Tools.ContextMenuStripEx();
			this.toolStripMenuItemCut = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemCopy = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemPaste = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemDelete = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemCutSpecial = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemPasteSpecial = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemPasteShiftFromShifts = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemDeleteSpecial = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.replaceActivityToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAddActivity = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAddPersonalActivity = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemAddOverTime = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemInsertAbsence = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemInsertDayOff = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemAddPreference = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemAddStudentAvailability = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemAddOvertimeAvailability = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemFindMatching = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemViewHistory = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemSwitchToViewPointOfSelectedAgent = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemSortBy = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemStartAsc = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemStartTimeDesc = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemEndTimeAsc = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemEndTimeDesc = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemContractTimeAsc = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemContractTimeDesc = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemSeniorityRankAsc = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemSeniorityRankDesc = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemUnlock = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemUnlockSelectionRM = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemUnlockAllRM = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemLock = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemLockSelectionRM = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemLockFreeDaysRM = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemLockAbsencesRM = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemLockShiftCategoriesRM = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemLockRestrictionsRM = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAllRM = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemLockPreferencesRM = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemLockRotationsRM = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemLockStudentAvailabilityRM = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemLockAvailabilityRM = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemLockTagsRM = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemLockAllTagsRM = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemWriteProtectSchedule = new System.Windows.Forms.ToolStripMenuItem();
			this.toolstripMenuRemoveWriteProtection = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemMeetingOrganizer = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemCreateMeeting = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemEditMeeting = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemRemoveParticipant = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemDeleteMeeting = new System.Windows.Forms.ToolStripMenuItem();
			this.xxExportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemExportToPDF = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemExportToPDFGraphical = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemExportToPDFShiftsPerDay = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemChangeTagRM = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemPublish = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemNotifyAgent = new System.Windows.Forms.ToolStripMenuItem();
			this.backgroundWorkerLoadData = new System.ComponentModel.BackgroundWorker();
			this.ribbonControlAdv1 = new Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.RibbonControlAdvFixed();
			this.backStageView = new Syncfusion.Windows.Forms.BackStageView(this.components);
			this.backStage1 = new Syncfusion.Windows.Forms.BackStage();
			this.backStageButtonMainMenuSave = new Syncfusion.Windows.Forms.BackStageButton();
			this.backStageButtonManiMenuImport = new Syncfusion.Windows.Forms.BackStageButton();
			this.backStageButtonMainMenuCopy = new Syncfusion.Windows.Forms.BackStageButton();
			this.backStageTabExportTo = new Syncfusion.Windows.Forms.BackStageTab();
			this.backStageButtonMainMenuHelp = new Syncfusion.Windows.Forms.BackStageButton();
			this.backStageButtonMainMenuClose = new Syncfusion.Windows.Forms.BackStageButton();
			this.backStageButtonOptions = new Syncfusion.Windows.Forms.BackStageButton();
			this.backStageButtonSystemExit = new Syncfusion.Windows.Forms.BackStageButton();
			this.toolStripTabItemHome = new Syncfusion.Windows.Forms.Tools.ToolStripTabItem();
			this.toolStripExData = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripPanelItemSave = new Syncfusion.Windows.Forms.Tools.ToolStripPanelItem();
			this.toolStripButtonSaveLarge = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonRefreshLarge = new System.Windows.Forms.ToolStripButton();
			this.toolStripExClipboard = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripExEdit2 = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripExScheduleViews = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripPanelItemViews2 = new Syncfusion.Windows.Forms.Tools.ToolStripPanelItem();
			this.toolStripButtonDayView = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonWeekView = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonPeriodView = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonSummaryView = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonRequestView = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonRestrictions = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonFindAgents = new System.Windows.Forms.ToolStripButton();
			this.toolStripExActions = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripPanelItemAssignments = new Syncfusion.Windows.Forms.Tools.ToolStripPanelItem();
			this.toolStripSplitButtonSchedule = new System.Windows.Forms.ToolStripSplitButton();
			this.toolStripMenuItemScheduleSelected = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemScheduleHourlyEmployees = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemScheduleOvertime = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemOptimize = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemShiftBackToLegal = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripDropDownButtonSwap = new System.Windows.Forms.ToolStripSplitButton();
			this.toolStripMenuItemSwap = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemSwapAndReschedule = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemSwapRaw = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripExLocks = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripPanelItemLocks = new Syncfusion.Windows.Forms.Tools.ToolStripPanelItem();
			this.toolStripSplitButtonUnlock = new System.Windows.Forms.ToolStripSplitButton();
			this.toolStripMenuItemUnlockAll = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemUnlockSelection = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSplitButtonLock = new System.Windows.Forms.ToolStripSplitButton();
			this.toolStripMenuItemLockSelection = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemLockAbsence = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemLockDayOff = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemLockShiftCategory = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemLockRestrictions = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemLockAllRestrictions = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemLockPreferences = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemLockRotations = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemLockStudentAvailability = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemLockAvailability = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemLockTags = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemLockAllTags = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemWriteProtectSchedule2 = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemRemoveWriteProtectionToolBar = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripExTags = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripPanelItemTags = new Syncfusion.Windows.Forms.Tools.ToolStripPanelItem();
			this.toolStripLabelAutoTag = new System.Windows.Forms.ToolStripLabel();
			this.toolStripComboBoxAutoTag = new System.Windows.Forms.ToolStripComboBox();
			this.toolStripSplitButtonChangeTag = new System.Windows.Forms.ToolStripSplitButton();
			this.toolStripEx1 = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripPanelItem1 = new Syncfusion.Windows.Forms.Tools.ToolStripPanelItem();
			this.toolStripButtonShowGraph = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonShowResult = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonShowEditor = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonShowPropertyPanel = new System.Windows.Forms.ToolStripButton();
			this.toolStripExLoadOptions = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripPanelItemLoadOptions = new Syncfusion.Windows.Forms.Tools.ToolStripPanelItem();
			this.toolStripButtonShrinkage = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonCalculation = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonValidation = new System.Windows.Forms.ToolStripButton();
			this.toolStripExFilter = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripPanelItem3 = new Syncfusion.Windows.Forms.Tools.ToolStripPanelItem();
			this.toolStripButtonFilterAgents = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonFilterOvertimeAvailability = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonFilterStudentAvailability = new System.Windows.Forms.ToolStripButton();
			this.toolStripTabItemChart = new Syncfusion.Windows.Forms.Tools.ToolStripTabItem();
			this.toolStripExGridRowInChartButtons = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripButtonGridInChart = new System.Windows.Forms.ToolStripButton();
			this.toolStripExSkillViews = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripPanelItem2 = new Syncfusion.Windows.Forms.Tools.ToolStripPanelItem();
			this.toolStripButtonChartPeriodView = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonChartMonthView = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonChartWeekView = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonChartDayView = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonChartIntradayView = new System.Windows.Forms.ToolStripButton();
			this.toolStripTabItem1 = new Syncfusion.Windows.Forms.Tools.ToolStripTabItem();
			this.toolStripExRequestNavigate = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripButtonRequestBack = new System.Windows.Forms.ToolStripButton();
			this.toolStripEx2 = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripButtonViewDetails = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonViewAllowance = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonViewRequestHistory = new System.Windows.Forms.ToolStripButton();
			this.toolStripExHandleRequests = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripButtonApproveRequest = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonDenyRequest = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonEditNote = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonReplyAndApprove = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonReplyAndDeny = new System.Windows.Forms.ToolStripButton();
			this.toolStripEx3 = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripTextBoxFilter = new System.Windows.Forms.ToolStripTextBox();
			this.toolStripButtonFindRequest = new System.Windows.Forms.ToolStripButton();
			this.toolStripEx4 = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripPanelItem4 = new Syncfusion.Windows.Forms.Tools.ToolStripPanelItem();
			this.toolStripButtonFilterAgentsRequestView = new System.Windows.Forms.ToolStripButton();
			this.toolStripTabItemQuickAccess = new Syncfusion.Windows.Forms.Tools.ToolStripTabItem();
			this.toolStripExForQuickAccessItems = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripButtonQuickAccessCancel = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonQuickAccessRedo = new System.Windows.Forms.ToolStripButton();
			this.toolStripSplitButtonQuickAccessUndo = new System.Windows.Forms.ToolStripSplitButton();
			this.toolStripMenuItemQuickAccessUndo = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemQuickAccessUndoAll = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripButtonShowTexts = new System.Windows.Forms.ToolStripButton();
			this.btnFilter = new System.Windows.Forms.ToolStripButton();
			this.btnRightLeft = new System.Windows.Forms.ToolStripButton();
			this.contextMenuStripRestrictionView = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.toolStripMenuItemAddPreferenceRestriction = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemAddStudentAvailabilityRestriction = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemRestrictionCopy = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemRestrictionPaste = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemRestrictionDelete = new System.Windows.Forms.ToolStripMenuItem();
			this.schedulerSplitters1 = new Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SchedulerSplitters();
			this.contextMenuStripRequests = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.ToolStripMenuItemViewDetails = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemFindMatching2 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemViewAllowance = new System.Windows.Forms.ToolStripMenuItem();
			this.xxViewOldRequestsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.flowLayoutExportToScenario = new Syncfusion.Windows.Forms.Tools.FlowLayout(this.components);
			this.statusStrip1.SuspendLayout();
			this.contextMenuViews.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).BeginInit();
			this.ribbonControlAdv1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.backStage1)).BeginInit();
			this.backStage1.SuspendLayout();
			this.toolStripTabItemHome.Panel.SuspendLayout();
			this.toolStripExData.SuspendLayout();
			this.toolStripExScheduleViews.SuspendLayout();
			this.toolStripExActions.SuspendLayout();
			this.toolStripExLocks.SuspendLayout();
			this.toolStripExTags.SuspendLayout();
			this.toolStripEx1.SuspendLayout();
			this.toolStripExLoadOptions.SuspendLayout();
			this.toolStripExFilter.SuspendLayout();
			this.toolStripTabItemChart.Panel.SuspendLayout();
			this.toolStripExGridRowInChartButtons.SuspendLayout();
			this.toolStripExSkillViews.SuspendLayout();
			this.toolStripTabItem1.Panel.SuspendLayout();
			this.toolStripExRequestNavigate.SuspendLayout();
			this.toolStripEx2.SuspendLayout();
			this.toolStripExHandleRequests.SuspendLayout();
			this.toolStripEx3.SuspendLayout();
			this.toolStripEx4.SuspendLayout();
			this.toolStripTabItemQuickAccess.Panel.SuspendLayout();
			this.toolStripExForQuickAccessItems.SuspendLayout();
			this.contextMenuStripRestrictionView.SuspendLayout();
			this.contextMenuStripRequests.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.flowLayoutExportToScenario)).BeginInit();
			this.SuspendLayout();
			// 
			// statusStrip1
			// 
			this.statusStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(165)))), ((int)(((byte)(220)))));
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSpinningProgressControl1,
            this.toolStripStatusLabelStatus,
            this.toolStripProgressBar1,
            this.toolStripStatusLabelNumberOfAgents,
            this.toolStripSplitButtonTimeZone,
            this.toolStripStatusLabelScheduleTag,
            this.toolStripStatusLabelContractTime});
			this.statusStrip1.Location = new System.Drawing.Point(3, 669);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(1229, 26);
			this.statusStrip1.TabIndex = 0;
			this.statusStrip1.Text = "yystatusStrip1";
			// 
			// toolStripSpinningProgressControl1
			// 
			this.toolStripSpinningProgressControl1.ActiveSegmentColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(165)))), ((int)(((byte)(220)))));
			this.toolStripSpinningProgressControl1.BehindTransitionSegmentIsActive = true;
			this.toolStripSpinningProgressControl1.InactiveSegmentColor = System.Drawing.Color.White;
			this.toolStripSpinningProgressControl1.Name = "ToolStripSpinningProgress";
			this.toolStripSpinningProgressControl1.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
			this.SetShortcut(this.toolStripSpinningProgressControl1, System.Windows.Forms.Keys.None);
			this.toolStripSpinningProgressControl1.Size = new System.Drawing.Size(25, 24);
			this.toolStripSpinningProgressControl1.Text = "toolStripSpinningProgressControl1";
			this.toolStripSpinningProgressControl1.TransitionSegment = 10;
			this.toolStripSpinningProgressControl1.TransitionSegmentColor = System.Drawing.Color.SkyBlue;
			this.toolStripSpinningProgressControl1.Visible = false;
			// 
			// toolStripStatusLabelStatus
			// 
			this.toolStripStatusLabelStatus.ForeColor = System.Drawing.Color.White;
			this.toolStripStatusLabelStatus.Name = "toolStripStatusLabelStatus";
			this.toolStripStatusLabelStatus.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
			this.SetShortcut(this.toolStripStatusLabelStatus, System.Windows.Forms.Keys.None);
			this.toolStripStatusLabelStatus.Size = new System.Drawing.Size(1064, 21);
			this.toolStripStatusLabelStatus.Spring = true;
			this.toolStripStatusLabelStatus.Text = "READY";
			this.toolStripStatusLabelStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// toolStripProgressBar1
			// 
			this.toolStripProgressBar1.AutoSize = false;
			this.toolStripProgressBar1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(165)))), ((int)(((byte)(220)))));
			this.toolStripProgressBar1.Maximum = 100;
			this.toolStripProgressBar1.Name = "toolStripProgressBar1";
			this.SetShortcut(this.toolStripProgressBar1, System.Windows.Forms.Keys.None);
			this.toolStripProgressBar1.Size = new System.Drawing.Size(100, 24);
			this.toolStripProgressBar1.Step = 1;
			this.toolStripProgressBar1.Value = 0;
			// 
			// toolStripStatusLabelNumberOfAgents
			// 
			this.toolStripStatusLabelNumberOfAgents.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
			this.toolStripStatusLabelNumberOfAgents.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
			this.toolStripStatusLabelNumberOfAgents.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.toolStripStatusLabelNumberOfAgents.ForeColor = System.Drawing.Color.White;
			this.toolStripStatusLabelNumberOfAgents.Name = "toolStripStatusLabelNumberOfAgents";
			this.SetShortcut(this.toolStripStatusLabelNumberOfAgents, System.Windows.Forms.Keys.None);
			this.toolStripStatusLabelNumberOfAgents.Size = new System.Drawing.Size(4, 21);
			this.toolStripStatusLabelNumberOfAgents.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripStatusLabelNumberOfAgents.Visible = false;
			// 
			// toolStripSplitButtonTimeZone
			// 
			this.toolStripSplitButtonTimeZone.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.toolStripSplitButtonTimeZone.ForeColor = System.Drawing.Color.White;
			this.toolStripSplitButtonTimeZone.Image = ((System.Drawing.Image)(resources.GetObject("toolStripSplitButtonTimeZone.Image")));
			this.toolStripSplitButtonTimeZone.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripSplitButtonTimeZone.Name = "toolStripSplitButtonTimeZone";
			this.SetShortcut(this.toolStripSplitButtonTimeZone, System.Windows.Forms.Keys.None);
			this.toolStripSplitButtonTimeZone.Size = new System.Drawing.Size(32, 24);
			this.toolStripSplitButtonTimeZone.Text = "...";
			this.toolStripSplitButtonTimeZone.Visible = false;
			// 
			// toolStripStatusLabelScheduleTag
			// 
			this.toolStripStatusLabelScheduleTag.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)));
			this.toolStripStatusLabelScheduleTag.BorderStyle = System.Windows.Forms.Border3DStyle.Etched;
			this.toolStripStatusLabelScheduleTag.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.toolStripStatusLabelScheduleTag.ForeColor = System.Drawing.Color.White;
			this.toolStripStatusLabelScheduleTag.Name = "toolStripStatusLabelScheduleTag";
			this.SetShortcut(this.toolStripStatusLabelScheduleTag, System.Windows.Forms.Keys.None);
			this.toolStripStatusLabelScheduleTag.Size = new System.Drawing.Size(20, 21);
			this.toolStripStatusLabelScheduleTag.Text = "...";
			this.toolStripStatusLabelScheduleTag.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripStatusLabelScheduleTag.Visible = false;
			// 
			// toolStripStatusLabelContractTime
			// 
			this.toolStripStatusLabelContractTime.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.toolStripStatusLabelContractTime.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemContractTime,
            this.toolStripMenuItemWorkTime,
            this.toolStripMenuItemPaidTime,
            this.toolStripMenuItemOverTime});
			this.toolStripStatusLabelContractTime.ForeColor = System.Drawing.Color.White;
			this.toolStripStatusLabelContractTime.Name = "toolStripStatusLabelContractTime";
			this.SetShortcut(this.toolStripStatusLabelContractTime, System.Windows.Forms.Keys.None);
			this.toolStripStatusLabelContractTime.Size = new System.Drawing.Size(50, 24);
			this.toolStripStatusLabelContractTime.Text = "00:00";
			this.toolStripStatusLabelContractTime.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// toolStripMenuItemContractTime
			// 
			this.toolStripMenuItemContractTime.Name = "toolStripMenuItemContractTime";
			this.SetShortcut(this.toolStripMenuItemContractTime, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemContractTime.Size = new System.Drawing.Size(157, 22);
			this.toolStripMenuItemContractTime.Text = "xxContractTime";
			this.toolStripMenuItemContractTime.Click += new System.EventHandler(this.toolStripMenuItemContractTimeClick);
			// 
			// toolStripMenuItemWorkTime
			// 
			this.toolStripMenuItemWorkTime.Name = "toolStripMenuItemWorkTime";
			this.SetShortcut(this.toolStripMenuItemWorkTime, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemWorkTime.Size = new System.Drawing.Size(157, 22);
			this.toolStripMenuItemWorkTime.Text = "xxWorkTime";
			this.toolStripMenuItemWorkTime.Click += new System.EventHandler(this.toolStripMenuItemContractTimeClick);
			// 
			// toolStripMenuItemPaidTime
			// 
			this.toolStripMenuItemPaidTime.Name = "toolStripMenuItemPaidTime";
			this.SetShortcut(this.toolStripMenuItemPaidTime, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemPaidTime.Size = new System.Drawing.Size(157, 22);
			this.toolStripMenuItemPaidTime.Text = "xxPaidTime";
			this.toolStripMenuItemPaidTime.Click += new System.EventHandler(this.toolStripMenuItemContractTimeClick);
			// 
			// toolStripMenuItemOverTime
			// 
			this.toolStripMenuItemOverTime.Name = "toolStripMenuItemOverTime";
			this.SetShortcut(this.toolStripMenuItemOverTime, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemOverTime.Size = new System.Drawing.Size(157, 22);
			this.toolStripMenuItemOverTime.Text = "xxOverTime";
			this.toolStripMenuItemOverTime.Click += new System.EventHandler(this.toolStripMenuItemContractTimeClick);
			// 
			// contextMenuViews
			// 
			this.contextMenuViews.DropShadowEnabled = false;
			this.contextMenuViews.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemCut,
            this.toolStripMenuItemCopy,
            this.toolStripMenuItemPaste,
            this.toolStripMenuItemDelete,
            this.toolStripMenuItem2,
            this.toolStripMenuItemCutSpecial,
            this.toolStripMenuItemPasteSpecial,
            this.toolStripMenuItemPasteShiftFromShifts,
            this.toolStripMenuItemDeleteSpecial,
            this.toolStripMenuItem1,
            this.replaceActivityToolStripMenuItem,
            this.ToolStripMenuItemAddActivity,
            this.ToolStripMenuItemAddPersonalActivity,
            this.toolStripMenuItemAddOverTime,
            this.toolStripMenuItemInsertAbsence,
            this.toolStripMenuItemInsertDayOff,
            this.toolStripMenuItemAddPreference,
            this.toolStripMenuItemAddStudentAvailability,
            this.toolStripMenuItemAddOvertimeAvailability,
            this.toolStripMenuItem3,
            this.toolStripMenuItemFindMatching,
            this.toolStripMenuItemViewHistory,
            this.toolStripMenuItemSwitchToViewPointOfSelectedAgent,
            this.toolStripMenuItemSortBy,
            this.toolStripSeparator1,
            this.toolStripMenuItemUnlock,
            this.toolStripMenuItemLock,
            this.toolStripMenuItemMeetingOrganizer,
            this.xxExportToolStripMenuItem,
            this.toolStripMenuItemChangeTagRM,
            this.toolStripMenuItemPublish,
            this.toolStripMenuItemNotifyAgent});
			this.contextMenuViews.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(236)))), ((int)(((byte)(249)))));
			this.contextMenuViews.Name = "contextMenuStrip1";
			this.contextMenuViews.Size = new System.Drawing.Size(306, 644);
			this.contextMenuViews.Style = Syncfusion.Windows.Forms.Tools.ContextMenuStripEx.ContextMenuStyle.Metro;
			this.contextMenuViews.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuViewsOpening);
			// 
			// toolStripMenuItemCut
			// 
			this.toolStripMenuItemCut.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_Cut_small;
			this.toolStripMenuItemCut.Name = "toolStripMenuItemCut";
			this.SetShortcut(this.toolStripMenuItemCut, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemCut.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
			this.toolStripMenuItemCut.Size = new System.Drawing.Size(305, 22);
			this.toolStripMenuItemCut.Text = "xxCut";
			this.toolStripMenuItemCut.Click += new System.EventHandler(this.toolStripMenuItemCutClick);
			// 
			// toolStripMenuItemCopy
			// 
			this.toolStripMenuItemCopy.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_Copy_small;
			this.toolStripMenuItemCopy.Name = "toolStripMenuItemCopy";
			this.SetShortcut(this.toolStripMenuItemCopy, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemCopy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
			this.toolStripMenuItemCopy.Size = new System.Drawing.Size(305, 22);
			this.toolStripMenuItemCopy.Text = "xxCopy";
			this.toolStripMenuItemCopy.Click += new System.EventHandler(this.toolStripMenuItemCopyClick);
			// 
			// toolStripMenuItemPaste
			// 
			this.toolStripMenuItemPaste.Enabled = false;
			this.toolStripMenuItemPaste.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_Paste_small;
			this.toolStripMenuItemPaste.Name = "toolStripMenuItemPaste";
			this.SetShortcut(this.toolStripMenuItemPaste, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemPaste.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
			this.toolStripMenuItemPaste.Size = new System.Drawing.Size(305, 22);
			this.toolStripMenuItemPaste.Text = "xxPaste";
			this.toolStripMenuItemPaste.Click += new System.EventHandler(this.toolStripMenuItemPasteClick);
			// 
			// toolStripMenuItemDelete
			// 
			this.toolStripMenuItemDelete.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_Delete_small;
			this.toolStripMenuItemDelete.Name = "toolStripMenuItemDelete";
			this.SetShortcut(this.toolStripMenuItemDelete, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemDelete.ShortcutKeys = System.Windows.Forms.Keys.Delete;
			this.toolStripMenuItemDelete.Size = new System.Drawing.Size(305, 22);
			this.toolStripMenuItemDelete.Text = "xxDelete";
			this.toolStripMenuItemDelete.Click += new System.EventHandler(this.toolStripButtonDeleteClick);
			// 
			// toolStripMenuItem2
			// 
			this.toolStripMenuItem2.Name = "toolStripMenuItem2";
			this.SetShortcut(this.toolStripMenuItem2, System.Windows.Forms.Keys.None);
			this.toolStripMenuItem2.Size = new System.Drawing.Size(302, 6);
			// 
			// toolStripMenuItemCutSpecial
			// 
			this.toolStripMenuItemCutSpecial.Name = "toolStripMenuItemCutSpecial";
			this.SetShortcut(this.toolStripMenuItemCutSpecial, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemCutSpecial.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.X)));
			this.toolStripMenuItemCutSpecial.Size = new System.Drawing.Size(305, 22);
			this.toolStripMenuItemCutSpecial.Text = "xxCutSpecialThreeDots";
			this.toolStripMenuItemCutSpecial.Click += new System.EventHandler(this.toolStripMenuItemCutSpecial2Click);
			// 
			// toolStripMenuItemPasteSpecial
			// 
			this.toolStripMenuItemPasteSpecial.Name = "toolStripMenuItemPasteSpecial";
			this.SetShortcut(this.toolStripMenuItemPasteSpecial, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemPasteSpecial.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.V)));
			this.toolStripMenuItemPasteSpecial.Size = new System.Drawing.Size(305, 22);
			this.toolStripMenuItemPasteSpecial.Text = "xxPasteSpecialThreeDots";
			this.toolStripMenuItemPasteSpecial.Click += new System.EventHandler(this.toolStripMenuItemPasteSpecial2Click);
			// 
			// toolStripMenuItemPasteShiftFromShifts
			// 
			this.toolStripMenuItemPasteShiftFromShifts.Name = "toolStripMenuItemPasteShiftFromShifts";
			this.SetShortcut(this.toolStripMenuItemPasteShiftFromShifts, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemPasteShiftFromShifts.Size = new System.Drawing.Size(305, 22);
			this.toolStripMenuItemPasteShiftFromShifts.Text = "xxPasteShiftFromShifts";
			this.toolStripMenuItemPasteShiftFromShifts.Click += new System.EventHandler(this.toolStripMenuItemPasteShiftFromShiftsClick);
			// 
			// toolStripMenuItemDeleteSpecial
			// 
			this.toolStripMenuItemDeleteSpecial.Name = "toolStripMenuItemDeleteSpecial";
			this.SetShortcut(this.toolStripMenuItemDeleteSpecial, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemDeleteSpecial.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.Delete)));
			this.toolStripMenuItemDeleteSpecial.Size = new System.Drawing.Size(305, 22);
			this.toolStripMenuItemDeleteSpecial.Text = "xxDeleteSpecialThreeDots";
			this.toolStripMenuItemDeleteSpecial.Click += new System.EventHandler(this.toolStripMenuItemDeleteSpecial2Click);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.SetShortcut(this.toolStripMenuItem1, System.Windows.Forms.Keys.None);
			this.toolStripMenuItem1.Size = new System.Drawing.Size(302, 6);
			// 
			// replaceActivityToolStripMenuItem
			// 
			this.replaceActivityToolStripMenuItem.Name = "replaceActivityToolStripMenuItem";
			this.SetShortcut(this.replaceActivityToolStripMenuItem, System.Windows.Forms.Keys.None);
			this.replaceActivityToolStripMenuItem.Size = new System.Drawing.Size(305, 22);
			this.replaceActivityToolStripMenuItem.Text = "xxReplaceActivity...";
			this.replaceActivityToolStripMenuItem.Visible = false;
			this.replaceActivityToolStripMenuItem.Click += new System.EventHandler(this.replaceActivityToolStripMenuItemClick);
			// 
			// ToolStripMenuItemAddActivity
			// 
			this.ToolStripMenuItemAddActivity.Name = "ToolStripMenuItemAddActivity";
			this.SetShortcut(this.ToolStripMenuItemAddActivity, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAddActivity.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.T)));
			this.ToolStripMenuItemAddActivity.Size = new System.Drawing.Size(305, 22);
			this.ToolStripMenuItemAddActivity.Text = "xxAddActivityThreeDots";
			this.ToolStripMenuItemAddActivity.Click += new System.EventHandler(this.toolStripMenuItemAddActivityClick);
			// 
			// ToolStripMenuItemAddPersonalActivity
			// 
			this.ToolStripMenuItemAddPersonalActivity.Name = "ToolStripMenuItemAddPersonalActivity";
			this.SetShortcut(this.ToolStripMenuItemAddPersonalActivity, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAddPersonalActivity.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.P)));
			this.ToolStripMenuItemAddPersonalActivity.Size = new System.Drawing.Size(305, 22);
			this.ToolStripMenuItemAddPersonalActivity.Text = "xxAddPersonalActivityThreeDots";
			this.ToolStripMenuItemAddPersonalActivity.Click += new System.EventHandler(this.toolStripMenuItemAddPersonalActivityClick);
			// 
			// toolStripMenuItemAddOverTime
			// 
			this.toolStripMenuItemAddOverTime.Name = "toolStripMenuItemAddOverTime";
			this.SetShortcut(this.toolStripMenuItemAddOverTime, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemAddOverTime.ShortcutKeyDisplayString = "Alt+O";
			this.toolStripMenuItemAddOverTime.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.O)));
			this.toolStripMenuItemAddOverTime.Size = new System.Drawing.Size(305, 22);
			this.toolStripMenuItemAddOverTime.Text = "xxAddOverTime";
			this.toolStripMenuItemAddOverTime.Click += new System.EventHandler(this.toolStripMenuItemAddOverTimeClick);
			// 
			// toolStripMenuItemInsertAbsence
			// 
			this.toolStripMenuItemInsertAbsence.Name = "toolStripMenuItemInsertAbsence";
			this.SetShortcut(this.toolStripMenuItemInsertAbsence, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemInsertAbsence.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.A)));
			this.toolStripMenuItemInsertAbsence.Size = new System.Drawing.Size(305, 22);
			this.toolStripMenuItemInsertAbsence.Text = "xxAddAbsenceThreeDots";
			this.toolStripMenuItemInsertAbsence.Click += new System.EventHandler(this.toolStripMenuItemInsertAbsenceClick);
			// 
			// toolStripMenuItemInsertDayOff
			// 
			this.toolStripMenuItemInsertDayOff.Name = "toolStripMenuItemInsertDayOff";
			this.SetShortcut(this.toolStripMenuItemInsertDayOff, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemInsertDayOff.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F)));
			this.toolStripMenuItemInsertDayOff.Size = new System.Drawing.Size(305, 22);
			this.toolStripMenuItemInsertDayOff.Text = "xxAddDayOffThreeDots";
			this.toolStripMenuItemInsertDayOff.Click += new System.EventHandler(this.toolStripButtonInsertDayOffClick);
			// 
			// toolStripMenuItemAddPreference
			// 
			this.toolStripMenuItemAddPreference.Name = "toolStripMenuItemAddPreference";
			this.SetShortcut(this.toolStripMenuItemAddPreference, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemAddPreference.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.R)));
			this.toolStripMenuItemAddPreference.Size = new System.Drawing.Size(305, 22);
			this.toolStripMenuItemAddPreference.Text = "xxAddPreferenceThreeDots";
			this.toolStripMenuItemAddPreference.Click += new System.EventHandler(this.addPreferenceToolStripMenuItemClick);
			// 
			// toolStripMenuItemAddStudentAvailability
			// 
			this.toolStripMenuItemAddStudentAvailability.Name = "toolStripMenuItemAddStudentAvailability";
			this.SetShortcut(this.toolStripMenuItemAddStudentAvailability, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemAddStudentAvailability.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.H)));
			this.toolStripMenuItemAddStudentAvailability.Size = new System.Drawing.Size(305, 22);
			this.toolStripMenuItemAddStudentAvailability.Text = "xxAddStudentAvailabilityThreeDots";
			this.toolStripMenuItemAddStudentAvailability.Click += new System.EventHandler(this.addStudentAvailabilityToolStripMenuItemClick);
			// 
			// toolStripMenuItemAddOvertimeAvailability
			// 
			this.toolStripMenuItemAddOvertimeAvailability.Name = "toolStripMenuItemAddOvertimeAvailability";
			this.SetShortcut(this.toolStripMenuItemAddOvertimeAvailability, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemAddOvertimeAvailability.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.G)));
			this.toolStripMenuItemAddOvertimeAvailability.Size = new System.Drawing.Size(305, 22);
			this.toolStripMenuItemAddOvertimeAvailability.Text = "xxAddOvertimeAvailabilityThreeDots";
			this.toolStripMenuItemAddOvertimeAvailability.Click += new System.EventHandler(this.addOvertimeAvailabilityToolStripMenuItemClick);
			// 
			// toolStripMenuItem3
			// 
			this.toolStripMenuItem3.Name = "toolStripMenuItem3";
			this.SetShortcut(this.toolStripMenuItem3, System.Windows.Forms.Keys.None);
			this.toolStripMenuItem3.Size = new System.Drawing.Size(302, 6);
			// 
			// toolStripMenuItemFindMatching
			// 
			this.toolStripMenuItemFindMatching.Name = "toolStripMenuItemFindMatching";
			this.SetShortcut(this.toolStripMenuItemFindMatching, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemFindMatching.Size = new System.Drawing.Size(305, 22);
			this.toolStripMenuItemFindMatching.Text = "xxFindMatchingThreeDots";
			this.toolStripMenuItemFindMatching.Visible = false;
			this.toolStripMenuItemFindMatching.Click += new System.EventHandler(this.toolStripMenuItemFindMatchingClick);
			// 
			// toolStripMenuItemViewHistory
			// 
			this.toolStripMenuItemViewHistory.Name = "toolStripMenuItemViewHistory";
			this.SetShortcut(this.toolStripMenuItemViewHistory, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemViewHistory.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.H)));
			this.toolStripMenuItemViewHistory.Size = new System.Drawing.Size(305, 22);
			this.toolStripMenuItemViewHistory.Text = "xxViewScheduleHistory";
			this.toolStripMenuItemViewHistory.Click += new System.EventHandler(this.toolStripMenuItemViewHistoryClick);
			// 
			// toolStripMenuItemSwitchToViewPointOfSelectedAgent
			// 
			this.toolStripMenuItemSwitchToViewPointOfSelectedAgent.Name = "toolStripMenuItemSwitchToViewPointOfSelectedAgent";
			this.SetShortcut(this.toolStripMenuItemSwitchToViewPointOfSelectedAgent, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemSwitchToViewPointOfSelectedAgent.Size = new System.Drawing.Size(305, 22);
			this.toolStripMenuItemSwitchToViewPointOfSelectedAgent.Text = "xxSwitchToViewPointOfSelectedAgent";
			this.toolStripMenuItemSwitchToViewPointOfSelectedAgent.Click += new System.EventHandler(this.toolStripMenuItemSwitchViewPointToTimeZoneOfSelectedAgentClick);
			// 
			// toolStripMenuItemSortBy
			// 
			this.toolStripMenuItemSortBy.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemStartAsc,
            this.toolStripMenuItemStartTimeDesc,
            this.toolStripMenuItemEndTimeAsc,
            this.toolStripMenuItemEndTimeDesc,
            this.toolStripMenuItemContractTimeAsc,
            this.toolStripMenuItemContractTimeDesc,
            this.toolStripMenuItemSeniorityRankAsc,
            this.toolStripMenuItemSeniorityRankDesc});
			this.toolStripMenuItemSortBy.Name = "toolStripMenuItemSortBy";
			this.SetShortcut(this.toolStripMenuItemSortBy, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemSortBy.Size = new System.Drawing.Size(305, 22);
			this.toolStripMenuItemSortBy.Text = "xxSortBy";
			// 
			// toolStripMenuItemStartAsc
			// 
			this.toolStripMenuItemStartAsc.Name = "toolStripMenuItemStartAsc";
			this.SetShortcut(this.toolStripMenuItemStartAsc, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemStartAsc.Size = new System.Drawing.Size(198, 22);
			this.toolStripMenuItemStartAsc.Text = "xxStartTimeAsc";
			this.toolStripMenuItemStartAsc.MouseUp += new System.Windows.Forms.MouseEventHandler(this.toolStripMenuItemStartAscMouseUp);
			// 
			// toolStripMenuItemStartTimeDesc
			// 
			this.toolStripMenuItemStartTimeDesc.Name = "toolStripMenuItemStartTimeDesc";
			this.SetShortcut(this.toolStripMenuItemStartTimeDesc, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemStartTimeDesc.Size = new System.Drawing.Size(198, 22);
			this.toolStripMenuItemStartTimeDesc.Text = "xxStartTimeDesc";
			this.toolStripMenuItemStartTimeDesc.MouseUp += new System.Windows.Forms.MouseEventHandler(this.toolStripMenuItemStartTimeDescMouseUp);
			// 
			// toolStripMenuItemEndTimeAsc
			// 
			this.toolStripMenuItemEndTimeAsc.Name = "toolStripMenuItemEndTimeAsc";
			this.SetShortcut(this.toolStripMenuItemEndTimeAsc, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemEndTimeAsc.Size = new System.Drawing.Size(198, 22);
			this.toolStripMenuItemEndTimeAsc.Text = "xxEndTimeAsc";
			this.toolStripMenuItemEndTimeAsc.MouseUp += new System.Windows.Forms.MouseEventHandler(this.toolStripMenuItemEndTimeAscMouseUp);
			// 
			// toolStripMenuItemEndTimeDesc
			// 
			this.toolStripMenuItemEndTimeDesc.Name = "toolStripMenuItemEndTimeDesc";
			this.SetShortcut(this.toolStripMenuItemEndTimeDesc, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemEndTimeDesc.Size = new System.Drawing.Size(198, 22);
			this.toolStripMenuItemEndTimeDesc.Text = "xxEndTimeDesc";
			this.toolStripMenuItemEndTimeDesc.MouseUp += new System.Windows.Forms.MouseEventHandler(this.toolStripMenuItemEndTimeDescMouseUp);
			// 
			// toolStripMenuItemContractTimeAsc
			// 
			this.toolStripMenuItemContractTimeAsc.Name = "toolStripMenuItemContractTimeAsc";
			this.SetShortcut(this.toolStripMenuItemContractTimeAsc, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemContractTimeAsc.Size = new System.Drawing.Size(198, 22);
			this.toolStripMenuItemContractTimeAsc.Text = "xxContractTimeAsc";
			this.toolStripMenuItemContractTimeAsc.MouseUp += new System.Windows.Forms.MouseEventHandler(this.toolStripMenuItemContractTimeAscMouseUp);
			// 
			// toolStripMenuItemContractTimeDesc
			// 
			this.toolStripMenuItemContractTimeDesc.Name = "toolStripMenuItemContractTimeDesc";
			this.SetShortcut(this.toolStripMenuItemContractTimeDesc, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemContractTimeDesc.Size = new System.Drawing.Size(198, 22);
			this.toolStripMenuItemContractTimeDesc.Text = "xxContractTimeDesc";
			this.toolStripMenuItemContractTimeDesc.MouseUp += new System.Windows.Forms.MouseEventHandler(this.toolStripMenuItemContractTimeDescMouseUp);
			// 
			// toolStripMenuItemSeniorityRankAsc
			// 
			this.toolStripMenuItemSeniorityRankAsc.Name = "toolStripMenuItemSeniorityRankAsc";
			this.SetShortcut(this.toolStripMenuItemSeniorityRankAsc, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemSeniorityRankAsc.Size = new System.Drawing.Size(198, 22);
			this.toolStripMenuItemSeniorityRankAsc.Text = "xxSeniorityRankingAsc";
			this.toolStripMenuItemSeniorityRankAsc.MouseUp += new System.Windows.Forms.MouseEventHandler(this.toolStripMenuItemSeniorityRankAscMouseUp);
			// 
			// toolStripMenuItemSeniorityRankDesc
			// 
			this.toolStripMenuItemSeniorityRankDesc.Name = "toolStripMenuItemSeniorityRankDesc";
			this.SetShortcut(this.toolStripMenuItemSeniorityRankDesc, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemSeniorityRankDesc.Size = new System.Drawing.Size(198, 22);
			this.toolStripMenuItemSeniorityRankDesc.Text = "xxSeniorityRankingDesc";
			this.toolStripMenuItemSeniorityRankDesc.MouseUp += new System.Windows.Forms.MouseEventHandler(this.toolStripMenuItemSeniorityRankDescMouseUp);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.SetShortcut(this.toolStripSeparator1, System.Windows.Forms.Keys.None);
			this.toolStripSeparator1.Size = new System.Drawing.Size(302, 6);
			// 
			// toolStripMenuItemUnlock
			// 
			this.toolStripMenuItemUnlock.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemUnlockSelectionRM,
            this.toolStripMenuItemUnlockAllRM});
			this.toolStripMenuItemUnlock.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_Unlock;
			this.toolStripMenuItemUnlock.Name = "toolStripMenuItemUnlock";
			this.SetShortcut(this.toolStripMenuItemUnlock, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemUnlock.Size = new System.Drawing.Size(305, 22);
			this.toolStripMenuItemUnlock.Text = "xxUnlock";
			// 
			// toolStripMenuItemUnlockSelectionRM
			// 
			this.toolStripMenuItemUnlockSelectionRM.Name = "toolStripMenuItemUnlockSelectionRM";
			this.SetShortcut(this.toolStripMenuItemUnlockSelectionRM, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemUnlockSelectionRM.Size = new System.Drawing.Size(169, 22);
			this.toolStripMenuItemUnlockSelectionRM.Text = "xxUnlockSelection";
			this.toolStripMenuItemUnlockSelectionRM.MouseUp += new System.Windows.Forms.MouseEventHandler(this.toolStripMenuItemUnlockSelectionRmMouseUp);
			// 
			// toolStripMenuItemUnlockAllRM
			// 
			this.toolStripMenuItemUnlockAllRM.Name = "toolStripMenuItemUnlockAllRM";
			this.SetShortcut(this.toolStripMenuItemUnlockAllRM, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemUnlockAllRM.Size = new System.Drawing.Size(169, 22);
			this.toolStripMenuItemUnlockAllRM.Text = "xxUnlockAll";
			this.toolStripMenuItemUnlockAllRM.MouseUp += new System.Windows.Forms.MouseEventHandler(this.toolStripMenuItemUnlockAllRmMouseUp);
			// 
			// toolStripMenuItemLock
			// 
			this.toolStripMenuItemLock.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemLockSelectionRM,
            this.toolStripMenuItemLockFreeDaysRM,
            this.toolStripMenuItemLockAbsencesRM,
            this.toolStripMenuItemLockShiftCategoriesRM,
            this.ToolStripMenuItemLockRestrictionsRM,
            this.toolStripMenuItemLockTagsRM,
            this.toolStripMenuItem5,
            this.toolStripMenuItemWriteProtectSchedule,
            this.toolstripMenuRemoveWriteProtection});
			this.toolStripMenuItemLock.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_Lock2;
			this.toolStripMenuItemLock.Name = "toolStripMenuItemLock";
			this.SetShortcut(this.toolStripMenuItemLock, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemLock.Size = new System.Drawing.Size(305, 22);
			this.toolStripMenuItemLock.Text = "xxLock";
			// 
			// toolStripMenuItemLockSelectionRM
			// 
			this.toolStripMenuItemLockSelectionRM.Name = "toolStripMenuItemLockSelectionRM";
			this.SetShortcut(this.toolStripMenuItemLockSelectionRM, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemLockSelectionRM.Size = new System.Drawing.Size(210, 22);
			this.toolStripMenuItemLockSelectionRM.Text = "xxLockSelection";
			this.toolStripMenuItemLockSelectionRM.MouseUp += new System.Windows.Forms.MouseEventHandler(this.toolStripMenuItemLockSelectionRmMouseUp);
			// 
			// toolStripMenuItemLockFreeDaysRM
			// 
			this.toolStripMenuItemLockFreeDaysRM.Name = "toolStripMenuItemLockFreeDaysRM";
			this.SetShortcut(this.toolStripMenuItemLockFreeDaysRM, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemLockFreeDaysRM.Size = new System.Drawing.Size(210, 22);
			this.toolStripMenuItemLockFreeDaysRM.Text = "xxLockFreeDays";
			this.toolStripMenuItemLockFreeDaysRM.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// toolStripMenuItemLockAbsencesRM
			// 
			this.toolStripMenuItemLockAbsencesRM.Name = "toolStripMenuItemLockAbsencesRM";
			this.SetShortcut(this.toolStripMenuItemLockAbsencesRM, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemLockAbsencesRM.Size = new System.Drawing.Size(210, 22);
			this.toolStripMenuItemLockAbsencesRM.Text = "xxLockAbsences";
			// 
			// toolStripMenuItemLockShiftCategoriesRM
			// 
			this.toolStripMenuItemLockShiftCategoriesRM.Name = "toolStripMenuItemLockShiftCategoriesRM";
			this.SetShortcut(this.toolStripMenuItemLockShiftCategoriesRM, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemLockShiftCategoriesRM.Size = new System.Drawing.Size(210, 22);
			this.toolStripMenuItemLockShiftCategoriesRM.Text = "xxLockShiftCategories";
			// 
			// ToolStripMenuItemLockRestrictionsRM
			// 
			this.ToolStripMenuItemLockRestrictionsRM.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItemAllRM,
            this.ToolStripMenuItemLockPreferencesRM,
            this.ToolStripMenuItemLockRotationsRM,
            this.ToolStripMenuItemLockStudentAvailabilityRM,
            this.ToolStripMenuItemLockAvailabilityRM});
			this.ToolStripMenuItemLockRestrictionsRM.Name = "ToolStripMenuItemLockRestrictionsRM";
			this.SetShortcut(this.ToolStripMenuItemLockRestrictionsRM, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemLockRestrictionsRM.Size = new System.Drawing.Size(210, 22);
			this.ToolStripMenuItemLockRestrictionsRM.Text = "xxLockRestrictions";
			// 
			// ToolStripMenuItemAllRM
			// 
			this.ToolStripMenuItemAllRM.Name = "ToolStripMenuItemAllRM";
			this.SetShortcut(this.ToolStripMenuItemAllRM, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAllRM.Size = new System.Drawing.Size(208, 22);
			this.ToolStripMenuItemAllRM.Text = "xxAll";
			this.ToolStripMenuItemAllRM.MouseUp += new System.Windows.Forms.MouseEventHandler(this.toolStripMenuItemLockAllRestrictionsMouseUp);
			// 
			// ToolStripMenuItemLockPreferencesRM
			// 
			this.ToolStripMenuItemLockPreferencesRM.Name = "ToolStripMenuItemLockPreferencesRM";
			this.SetShortcut(this.ToolStripMenuItemLockPreferencesRM, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemLockPreferencesRM.Size = new System.Drawing.Size(208, 22);
			this.ToolStripMenuItemLockPreferencesRM.Text = "xxLockPreferences";
			// 
			// ToolStripMenuItemLockRotationsRM
			// 
			this.ToolStripMenuItemLockRotationsRM.Name = "ToolStripMenuItemLockRotationsRM";
			this.SetShortcut(this.ToolStripMenuItemLockRotationsRM, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemLockRotationsRM.Size = new System.Drawing.Size(208, 22);
			this.ToolStripMenuItemLockRotationsRM.Text = "xxLockRotations";
			// 
			// ToolStripMenuItemLockStudentAvailabilityRM
			// 
			this.ToolStripMenuItemLockStudentAvailabilityRM.Name = "ToolStripMenuItemLockStudentAvailabilityRM";
			this.SetShortcut(this.ToolStripMenuItemLockStudentAvailabilityRM, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemLockStudentAvailabilityRM.Size = new System.Drawing.Size(208, 22);
			this.ToolStripMenuItemLockStudentAvailabilityRM.Text = "xxLockStudentAvailability";
			// 
			// ToolStripMenuItemLockAvailabilityRM
			// 
			this.ToolStripMenuItemLockAvailabilityRM.Name = "ToolStripMenuItemLockAvailabilityRM";
			this.SetShortcut(this.ToolStripMenuItemLockAvailabilityRM, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemLockAvailabilityRM.Size = new System.Drawing.Size(208, 22);
			this.ToolStripMenuItemLockAvailabilityRM.Text = "xxLockAvailability";
			// 
			// toolStripMenuItemLockTagsRM
			// 
			this.toolStripMenuItemLockTagsRM.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemLockAllTagsRM});
			this.toolStripMenuItemLockTagsRM.Name = "toolStripMenuItemLockTagsRM";
			this.SetShortcut(this.toolStripMenuItemLockTagsRM, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemLockTagsRM.Size = new System.Drawing.Size(210, 22);
			this.toolStripMenuItemLockTagsRM.Text = "xxLockTags";
			// 
			// toolStripMenuItemLockAllTagsRM
			// 
			this.toolStripMenuItemLockAllTagsRM.Name = "toolStripMenuItemLockAllTagsRM";
			this.SetShortcut(this.toolStripMenuItemLockAllTagsRM, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemLockAllTagsRM.Size = new System.Drawing.Size(98, 22);
			this.toolStripMenuItemLockAllTagsRM.Text = "xxAll";
			this.toolStripMenuItemLockAllTagsRM.MouseUp += new System.Windows.Forms.MouseEventHandler(this.toolStripMenuItemLockAllTagsMouseUp);
			// 
			// toolStripMenuItem5
			// 
			this.toolStripMenuItem5.Name = "toolStripMenuItem5";
			this.SetShortcut(this.toolStripMenuItem5, System.Windows.Forms.Keys.None);
			this.toolStripMenuItem5.Size = new System.Drawing.Size(207, 6);
			// 
			// toolStripMenuItemWriteProtectSchedule
			// 
			this.toolStripMenuItemWriteProtectSchedule.Name = "toolStripMenuItemWriteProtectSchedule";
			this.SetShortcut(this.toolStripMenuItemWriteProtectSchedule, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemWriteProtectSchedule.Size = new System.Drawing.Size(210, 22);
			this.toolStripMenuItemWriteProtectSchedule.Text = "xxWriteProtectSchedule";
			this.toolStripMenuItemWriteProtectSchedule.MouseUp += new System.Windows.Forms.MouseEventHandler(this.toolStripMenuItemWriteProtectScheduleMouseUp);
			// 
			// toolstripMenuRemoveWriteProtection
			// 
			this.toolstripMenuRemoveWriteProtection.Name = "toolstripMenuRemoveWriteProtection";
			this.SetShortcut(this.toolstripMenuRemoveWriteProtection, System.Windows.Forms.Keys.None);
			this.toolstripMenuRemoveWriteProtection.Size = new System.Drawing.Size(210, 22);
			this.toolstripMenuRemoveWriteProtection.Text = "xxRemoveWriteProtection";
			this.toolstripMenuRemoveWriteProtection.MouseUp += new System.Windows.Forms.MouseEventHandler(this.toolstripMenuRemoveWriteProtectionMouseUp);
			// 
			// toolStripMenuItemMeetingOrganizer
			// 
			this.toolStripMenuItemMeetingOrganizer.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItemCreateMeeting,
            this.toolStripMenuItemEditMeeting,
            this.toolStripMenuItemRemoveParticipant,
            this.toolStripMenuItemDeleteMeeting});
			this.toolStripMenuItemMeetingOrganizer.Name = "toolStripMenuItemMeetingOrganizer";
			this.SetShortcut(this.toolStripMenuItemMeetingOrganizer, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemMeetingOrganizer.Size = new System.Drawing.Size(305, 22);
			this.toolStripMenuItemMeetingOrganizer.Text = "xxMeetingOrganizer";
			// 
			// ToolStripMenuItemCreateMeeting
			// 
			this.ToolStripMenuItemCreateMeeting.Name = "ToolStripMenuItemCreateMeeting";
			this.SetShortcut(this.ToolStripMenuItemCreateMeeting, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemCreateMeeting.Size = new System.Drawing.Size(184, 22);
			this.ToolStripMenuItemCreateMeeting.Text = "xxCreateMeeting";
			this.ToolStripMenuItemCreateMeeting.MouseUp += new System.Windows.Forms.MouseEventHandler(this.toolStripMenuItemCreateMeetingMouseUp);
			// 
			// toolStripMenuItemEditMeeting
			// 
			this.toolStripMenuItemEditMeeting.Name = "toolStripMenuItemEditMeeting";
			this.SetShortcut(this.toolStripMenuItemEditMeeting, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemEditMeeting.Size = new System.Drawing.Size(184, 22);
			this.toolStripMenuItemEditMeeting.Text = "xxEditMeeting";
			this.toolStripMenuItemEditMeeting.MouseUp += new System.Windows.Forms.MouseEventHandler(this.toolStripMenuItemEditMeetingMouseUp);
			// 
			// toolStripMenuItemRemoveParticipant
			// 
			this.toolStripMenuItemRemoveParticipant.Name = "toolStripMenuItemRemoveParticipant";
			this.SetShortcut(this.toolStripMenuItemRemoveParticipant, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemRemoveParticipant.Size = new System.Drawing.Size(184, 22);
			this.toolStripMenuItemRemoveParticipant.Text = "xxRemoveParticipant";
			this.toolStripMenuItemRemoveParticipant.MouseUp += new System.Windows.Forms.MouseEventHandler(this.toolStripMenuItemRemoveParticipantMouseUp);
			// 
			// toolStripMenuItemDeleteMeeting
			// 
			this.toolStripMenuItemDeleteMeeting.Name = "toolStripMenuItemDeleteMeeting";
			this.SetShortcut(this.toolStripMenuItemDeleteMeeting, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemDeleteMeeting.Size = new System.Drawing.Size(184, 22);
			this.toolStripMenuItemDeleteMeeting.Text = "xxDeleteMeeting";
			this.toolStripMenuItemDeleteMeeting.MouseUp += new System.Windows.Forms.MouseEventHandler(this.toolStripMenuItemDeleteMeetingMouseUp);
			// 
			// xxExportToolStripMenuItem
			// 
			this.xxExportToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemExportToPDF,
            this.toolStripMenuItemExportToPDFGraphical,
            this.ToolStripMenuItemExportToPDFShiftsPerDay});
			this.xxExportToolStripMenuItem.Name = "xxExportToolStripMenuItem";
			this.SetShortcut(this.xxExportToolStripMenuItem, System.Windows.Forms.Keys.None);
			this.xxExportToolStripMenuItem.Size = new System.Drawing.Size(305, 22);
			this.xxExportToolStripMenuItem.Text = "xxExport";
			// 
			// toolStripMenuItemExportToPDF
			// 
			this.toolStripMenuItemExportToPDF.Name = "toolStripMenuItemExportToPDF";
			this.SetShortcut(this.toolStripMenuItemExportToPDF, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemExportToPDF.Size = new System.Drawing.Size(217, 22);
			this.toolStripMenuItemExportToPDF.Text = "xxExportToPDF";
			this.toolStripMenuItemExportToPDF.MouseUp += new System.Windows.Forms.MouseEventHandler(this.toolStripMenuItemExportToPdfMouseUp);
			// 
			// toolStripMenuItemExportToPDFGraphical
			// 
			this.toolStripMenuItemExportToPDFGraphical.Name = "toolStripMenuItemExportToPDFGraphical";
			this.SetShortcut(this.toolStripMenuItemExportToPDFGraphical, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemExportToPDFGraphical.Size = new System.Drawing.Size(217, 22);
			this.toolStripMenuItemExportToPDFGraphical.Text = "xxExportToPDFGraphical";
			this.toolStripMenuItemExportToPDFGraphical.MouseUp += new System.Windows.Forms.MouseEventHandler(this.toolStripMenuItemExportToPdfGraphicalMouseUp);
			// 
			// ToolStripMenuItemExportToPDFShiftsPerDay
			// 
			this.ToolStripMenuItemExportToPDFShiftsPerDay.Name = "ToolStripMenuItemExportToPDFShiftsPerDay";
			this.SetShortcut(this.ToolStripMenuItemExportToPDFShiftsPerDay, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemExportToPDFShiftsPerDay.Size = new System.Drawing.Size(217, 22);
			this.ToolStripMenuItemExportToPDFShiftsPerDay.Text = "xxExportToPDFShiftsPerDay";
			this.ToolStripMenuItemExportToPDFShiftsPerDay.MouseUp += new System.Windows.Forms.MouseEventHandler(this.toolStripMenuItemExportToPdfShiftsPerDayMouseUp);
			// 
			// toolStripMenuItemChangeTagRM
			// 
			this.toolStripMenuItemChangeTagRM.Name = "toolStripMenuItemChangeTagRM";
			this.SetShortcut(this.toolStripMenuItemChangeTagRM, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemChangeTagRM.Size = new System.Drawing.Size(305, 22);
			this.toolStripMenuItemChangeTagRM.Text = "xxChangeTag";
			// 
			// toolStripMenuItemPublish
			// 
			this.toolStripMenuItemPublish.Name = "toolStripMenuItemPublish";
			this.SetShortcut(this.toolStripMenuItemPublish, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemPublish.Size = new System.Drawing.Size(305, 22);
			this.toolStripMenuItemPublish.Text = "xxPublish";
			this.toolStripMenuItemPublish.Click += new System.EventHandler(this.publishToolStripMenuItemClick);
			// 
			// toolStripMenuItemNotifyAgent
			// 
			this.toolStripMenuItemNotifyAgent.Name = "toolStripMenuItemNotifyAgent";
			this.SetShortcut(this.toolStripMenuItemNotifyAgent, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemNotifyAgent.Size = new System.Drawing.Size(305, 22);
			this.toolStripMenuItemNotifyAgent.Text = "xxNotifyAgent";
			this.toolStripMenuItemNotifyAgent.Click += new System.EventHandler(this.toolStripMenuItemNotifyAgentClick);
			// 
			// backgroundWorkerLoadData
			// 
			this.backgroundWorkerLoadData.WorkerReportsProgress = true;
			this.backgroundWorkerLoadData.WorkerSupportsCancellation = true;
			// 
			// ribbonControlAdv1
			// 
			this.ribbonControlAdv1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.ribbonControlAdv1.BackStageView = this.backStageView;
			this.ribbonControlAdv1.CaptionFont = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ribbonControlAdv1.Header.AddMainItem(toolStripTabItemHome);
			this.ribbonControlAdv1.Header.AddMainItem(toolStripTabItemChart);
			this.ribbonControlAdv1.Header.AddMainItem(toolStripTabItem1);
			this.ribbonControlAdv1.Header.AddMainItem(toolStripTabItemQuickAccess);
			this.ribbonControlAdv1.HideMenuButtonToolTip = false;
			this.ribbonControlAdv1.LauncherStyle = Syncfusion.Windows.Forms.Tools.LauncherStyle.Metro;
			this.ribbonControlAdv1.Location = new System.Drawing.Point(1, 1);
			this.ribbonControlAdv1.MaximizeToolTip = "Maximize Ribbon";
			this.ribbonControlAdv1.MenuButtonEnabled = true;
			this.ribbonControlAdv1.MenuButtonFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ribbonControlAdv1.MenuButtonText = "xxFile";
			this.ribbonControlAdv1.MenuButtonVisible = false;
			this.ribbonControlAdv1.MenuButtonWidth = 60;
			this.ribbonControlAdv1.MenuColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.ribbonControlAdv1.MinimizeToolTip = "Minimize Ribbon";
			this.ribbonControlAdv1.Name = "ribbonControlAdv1";
			this.ribbonControlAdv1.Office2013ColorScheme = Syncfusion.Windows.Forms.Tools.Office2013ColorScheme.White;
			this.ribbonControlAdv1.OfficeColorScheme = Syncfusion.Windows.Forms.Tools.ToolStripEx.ColorScheme.Silver;
			// 
			// ribbonControlAdv1.OfficeMenu
			// 
			this.ribbonControlAdv1.OfficeMenu.Name = "OfficeMenu";
			this.ribbonControlAdv1.OfficeMenu.Size = new System.Drawing.Size(12, 65);
			this.ribbonControlAdv1.OverFlowButtonToolTip = "Show DropDown";
			this.ribbonControlAdv1.QuickPanelImageLayout = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.ribbonControlAdv1.QuickPanelVisible = false;
			this.ribbonControlAdv1.RibbonHeaderImage = Syncfusion.Windows.Forms.Tools.RibbonHeaderImage.None;
			this.ribbonControlAdv1.RibbonStyle = Syncfusion.Windows.Forms.Tools.RibbonStyle.Office2013;
			this.ribbonControlAdv1.SelectedTab = this.toolStripTabItemHome;
			this.ribbonControlAdv1.Show2010CustomizeQuickItemDialog = false;
			this.ribbonControlAdv1.ShowRibbonDisplayOptionButton = true;
			this.ribbonControlAdv1.Size = new System.Drawing.Size(1235, 146);
			this.ribbonControlAdv1.SystemText.QuickAccessCustomizeCaptionText = "Customize QuickAccess Toolbar";
			this.ribbonControlAdv1.SystemText.QuickAccessDialogDropDownName = "";
			toolStripTabGroup1.Color = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
			toolStripTabGroup1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			toolStripTabGroup1.Name = "toolStripTabGroupShiftEditor";
			toolStripTabGroup1.Visible = true;
			toolStripTabGroup2.Color = System.Drawing.Color.Blue;
			toolStripTabGroup2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			toolStripTabGroup2.Name = "toolStripTabGroupMainGrid";
			toolStripTabGroup2.Visible = true;
			toolStripTabGroup3.Color = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
			toolStripTabGroup3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			toolStripTabGroup3.Name = "toolStripTabGroupShiftEditor2";
			toolStripTabGroup3.Visible = true;
			this.ribbonControlAdv1.TabGroups.Add(toolStripTabGroup1);
			this.ribbonControlAdv1.TabGroups.Add(toolStripTabGroup2);
			this.ribbonControlAdv1.TabGroups.Add(toolStripTabGroup3);
			this.ribbonControlAdv1.TabIndex = 5;
			this.ribbonControlAdv1.TitleAlignment = Syncfusion.Windows.Forms.Tools.TextAlignment.Center;
			this.ribbonControlAdv1.TitleColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(59)))), ((int)(((byte)(59)))));
			this.ribbonControlAdv1.TitleFont = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			// 
			// backStageView
			// 
			this.backStageView.BackStage = this.backStage1;
			this.backStageView.HostControl = null;
			this.backStageView.HostForm = this;
			// 
			// backStage1
			// 
			this.backStage1.AllowDrop = true;
			this.backStage1.BeforeTouchSize = new System.Drawing.Size(1230, 643);
			this.backStage1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.backStage1.Controls.Add(this.backStageButtonMainMenuSave);
			this.backStage1.Controls.Add(this.backStageButtonManiMenuImport);
			this.backStage1.Controls.Add(this.backStageButtonMainMenuCopy);
			this.backStage1.Controls.Add(this.backStageTabExportTo);
			this.backStage1.Controls.Add(this.backStageButtonMainMenuHelp);
			this.backStage1.Controls.Add(this.backStageButtonMainMenuClose);
			this.backStage1.Controls.Add(this.backStageButtonOptions);
			this.backStage1.Controls.Add(this.backStageButtonSystemExit);
			this.backStage1.ItemSize = new System.Drawing.Size(138, 40);
			this.backStage1.Location = new System.Drawing.Point(0, 0);
			this.backStage1.Name = "backStage1";
			this.backStage1.OfficeColorScheme = Syncfusion.Windows.Forms.Tools.ToolStripEx.ColorScheme.Silver;
			this.backStage1.Size = new System.Drawing.Size(1230, 643);
			this.backStage1.TabIndex = 6;
			this.backStage1.Visible = false;
			this.backStage1.VisibleChanged += new System.EventHandler(this.backStage1VisibleChanged);
			// 
			// backStageButtonMainMenuSave
			// 
			this.backStageButtonMainMenuSave.Accelerator = "";
			this.backStageButtonMainMenuSave.BackColor = System.Drawing.Color.Transparent;
			this.backStageButtonMainMenuSave.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.backStageButtonMainMenuSave.IsBackStageButton = false;
			this.backStageButtonMainMenuSave.Location = new System.Drawing.Point(0, 16);
			this.backStageButtonMainMenuSave.Name = "backStageButtonMainMenuSave";
			this.backStageButtonMainMenuSave.Size = new System.Drawing.Size(110, 25);
			this.backStageButtonMainMenuSave.TabIndex = 3;
			this.backStageButtonMainMenuSave.Text = "xxSave";
			this.backStageButtonMainMenuSave.Click += new System.EventHandler(this.toolStripButtonSaveLargeClick);
			// 
			// backStageButtonManiMenuImport
			// 
			this.backStageButtonManiMenuImport.Accelerator = "";
			this.backStageButtonManiMenuImport.BackColor = System.Drawing.Color.Transparent;
			this.backStageButtonManiMenuImport.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.backStageButtonManiMenuImport.IsBackStageButton = false;
			this.backStageButtonManiMenuImport.Location = new System.Drawing.Point(0, 54);
			this.backStageButtonManiMenuImport.Name = "backStageButtonManiMenuImport";
			this.backStageButtonManiMenuImport.Size = new System.Drawing.Size(110, 25);
			this.backStageButtonManiMenuImport.TabIndex = 4;
			this.backStageButtonManiMenuImport.Text = "xxImportSchedule";
			this.backStageButtonManiMenuImport.Click += new System.EventHandler(this.backStageButtonManiMenuImportClick);
			// 
			// backStageButtonMainMenuCopy
			// 
			this.backStageButtonMainMenuCopy.Accelerator = "";
			this.backStageButtonMainMenuCopy.BackColor = System.Drawing.Color.Transparent;
			this.backStageButtonMainMenuCopy.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.backStageButtonMainMenuCopy.IsBackStageButton = false;
			this.backStageButtonMainMenuCopy.Location = new System.Drawing.Point(0, 92);
			this.backStageButtonMainMenuCopy.Name = "backStageButtonMainMenuCopy";
			this.backStageButtonMainMenuCopy.Size = new System.Drawing.Size(110, 25);
			this.backStageButtonMainMenuCopy.TabIndex = 5;
			this.backStageButtonMainMenuCopy.Text = "xxCopySchedule";
			this.backStageButtonMainMenuCopy.Click += new System.EventHandler(this.backStageButtonMainMenuCopyClick);
			// 
			// backStageTabExportTo
			// 
			this.backStageTabExportTo.Accelerator = "";
			this.backStageTabExportTo.BackColor = System.Drawing.Color.White;
			this.backStageTabExportTo.Image = null;
			this.backStageTabExportTo.ImageSize = new System.Drawing.Size(16, 16);
			this.backStageTabExportTo.Location = new System.Drawing.Point(177, 0);
			this.backStageTabExportTo.Name = "backStageTabExportTo";
			this.backStageTabExportTo.Position = new System.Drawing.Point(0, 0);
			this.backStageTabExportTo.ShowCloseButton = true;
			this.backStageTabExportTo.Size = new System.Drawing.Size(1053, 643);
			this.backStageTabExportTo.TabIndex = 9;
			this.backStageTabExportTo.Text = "xxExportTo";
			this.backStageTabExportTo.ThemesEnabled = false;
			// 
			// backStageButtonMainMenuHelp
			// 
			this.backStageButtonMainMenuHelp.Accelerator = "";
			this.backStageButtonMainMenuHelp.BackColor = System.Drawing.Color.Transparent;
			this.backStageButtonMainMenuHelp.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.backStageButtonMainMenuHelp.IsBackStageButton = false;
			this.backStageButtonMainMenuHelp.Location = new System.Drawing.Point(0, 167);
			this.backStageButtonMainMenuHelp.Name = "backStageButtonMainMenuHelp";
			this.backStageButtonMainMenuHelp.Size = new System.Drawing.Size(110, 25);
			this.backStageButtonMainMenuHelp.TabIndex = 7;
			this.backStageButtonMainMenuHelp.Text = "xxHelp";
			this.backStageButtonMainMenuHelp.Click += new System.EventHandler(this.toolStripButtonMainMenuHelpClick);
			// 
			// backStageButtonMainMenuClose
			// 
			this.backStageButtonMainMenuClose.Accelerator = "";
			this.backStageButtonMainMenuClose.BackColor = System.Drawing.Color.Transparent;
			this.backStageButtonMainMenuClose.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.backStageButtonMainMenuClose.IsBackStageButton = false;
			this.backStageButtonMainMenuClose.Location = new System.Drawing.Point(0, 205);
			this.backStageButtonMainMenuClose.Name = "backStageButtonMainMenuClose";
			this.backStageButtonMainMenuClose.Size = new System.Drawing.Size(110, 25);
			this.backStageButtonMainMenuClose.TabIndex = 8;
			this.backStageButtonMainMenuClose.Text = "xxClose";
			this.backStageButtonMainMenuClose.Click += new System.EventHandler(this.toolStripButtonMainMenuCloseClick);
			// 
			// backStageButtonOptions
			// 
			this.backStageButtonOptions.Accelerator = "";
			this.backStageButtonOptions.BackColor = System.Drawing.Color.Transparent;
			this.backStageButtonOptions.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.backStageButtonOptions.IsBackStageButton = false;
			this.backStageButtonOptions.Location = new System.Drawing.Point(0, 243);
			this.backStageButtonOptions.Name = "backStageButtonOptions";
			this.backStageButtonOptions.Size = new System.Drawing.Size(110, 25);
			this.backStageButtonOptions.TabIndex = 9;
			this.backStageButtonOptions.Text = "xxOptions";
			this.backStageButtonOptions.Click += new System.EventHandler(this.toolStripButtonOptionsClick);
			// 
			// backStageButtonSystemExit
			// 
			this.backStageButtonSystemExit.Accelerator = "";
			this.backStageButtonSystemExit.BackColor = System.Drawing.Color.Transparent;
			this.backStageButtonSystemExit.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.backStageButtonSystemExit.IsBackStageButton = false;
			this.backStageButtonSystemExit.Location = new System.Drawing.Point(0, 281);
			this.backStageButtonSystemExit.Name = "backStageButtonSystemExit";
			this.backStageButtonSystemExit.Size = new System.Drawing.Size(110, 25);
			this.backStageButtonSystemExit.TabIndex = 10;
			this.backStageButtonSystemExit.Text = "xxExitTELEOPTICCC";
			this.backStageButtonSystemExit.Click += new System.EventHandler(this.toolStripButtonSystemExitClick);
			// 
			// toolStripTabItemHome
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripTabItemHome, "");
			this.toolStripTabItemHome.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.toolStripTabItemHome.Name = "toolStripTabItemHome";
			// 
			// ribbonControlAdv1.ribbonPanel1
			// 
			this.toolStripTabItemHome.Panel.Controls.Add(this.toolStripExData);
			this.toolStripTabItemHome.Panel.Controls.Add(this.toolStripExClipboard);
			this.toolStripTabItemHome.Panel.Controls.Add(this.toolStripExEdit2);
			this.toolStripTabItemHome.Panel.Controls.Add(this.toolStripExScheduleViews);
			this.toolStripTabItemHome.Panel.Controls.Add(this.toolStripExActions);
			this.toolStripTabItemHome.Panel.Controls.Add(this.toolStripExLocks);
			this.toolStripTabItemHome.Panel.Controls.Add(this.toolStripExTags);
			this.toolStripTabItemHome.Panel.Controls.Add(this.toolStripEx1);
			this.toolStripTabItemHome.Panel.Controls.Add(this.toolStripExLoadOptions);
			this.toolStripTabItemHome.Panel.Controls.Add(this.toolStripExFilter);
			this.toolStripTabItemHome.Panel.Name = "ribbonPanel1";
			this.toolStripTabItemHome.Panel.ScrollPosition = 0;
			this.toolStripTabItemHome.Panel.TabIndex = 2;
			this.toolStripTabItemHome.Panel.Text = "XXHome";
			this.toolStripTabItemHome.Position = 0;
			this.SetShortcut(this.toolStripTabItemHome, System.Windows.Forms.Keys.None);
			this.toolStripTabItemHome.Size = new System.Drawing.Size(74, 25);
			this.toolStripTabItemHome.Text = "XXHome";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripTabItemHome, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripTabItemHome, false);
			this.toolStripTabItemHome.Click += new System.EventHandler(this.toolStripTabItemHomeClick);
			// 
			// toolStripExData
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripExData, "");
			this.toolStripExData.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripExData.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(59)))), ((int)(((byte)(59)))));
			this.toolStripExData.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripExData.Image = null;
			this.toolStripExData.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripPanelItemSave});
			this.toolStripExData.Location = new System.Drawing.Point(0, 1);
			this.toolStripExData.Name = "toolStripExData";
			this.toolStripExData.Office12Mode = false;
			this.toolStripExData.ShowCaption = true;
			this.toolStripExData.ShowLauncher = false;
			this.toolStripExData.Size = new System.Drawing.Size(105, 87);
			this.toolStripExData.TabIndex = 17;
			this.toolStripExData.Text = "xxData";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripExData, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripExData, false);
			// 
			// toolStripPanelItemSave
			// 
			this.toolStripPanelItemSave.CausesValidation = false;
			this.toolStripPanelItemSave.ForeColor = System.Drawing.Color.MidnightBlue;
			this.toolStripPanelItemSave.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonSaveLarge,
            this.toolStripButtonRefreshLarge});
			this.toolStripPanelItemSave.Name = "toolStripPanelItemSave";
			this.toolStripPanelItemSave.RowCount = 2;
			this.SetShortcut(this.toolStripPanelItemSave, System.Windows.Forms.Keys.None);
			this.toolStripPanelItemSave.Size = new System.Drawing.Size(96, 74);
			this.toolStripPanelItemSave.Transparent = true;
			// 
			// toolStripButtonSaveLarge
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonSaveLarge, "");
			this.toolStripButtonSaveLarge.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_Save;
			this.toolStripButtonSaveLarge.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonSaveLarge.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonSaveLarge.Name = "toolStripButtonSaveLarge";
			this.SetShortcut(this.toolStripButtonSaveLarge, ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S))));
			this.toolStripButtonSaveLarge.Size = new System.Drawing.Size(77, 36);
			this.toolStripButtonSaveLarge.Text = "xxSave";
			this.toolStripButtonSaveLarge.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonSaveLarge, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonSaveLarge, false);
			this.toolStripButtonSaveLarge.Click += new System.EventHandler(this.toolStripButtonSaveLargeClick);
			// 
			// toolStripButtonRefreshLarge
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonRefreshLarge, "");
			this.toolStripButtonRefreshLarge.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_Refresh;
			this.toolStripButtonRefreshLarge.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonRefreshLarge.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonRefreshLarge.Name = "toolStripButtonRefreshLarge";
			this.SetShortcut(this.toolStripButtonRefreshLarge, ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R))));
			this.toolStripButtonRefreshLarge.Size = new System.Drawing.Size(92, 36);
			this.toolStripButtonRefreshLarge.Text = "xxRefresh";
			this.toolStripButtonRefreshLarge.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonRefreshLarge, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonRefreshLarge, false);
			this.toolStripButtonRefreshLarge.Click += new System.EventHandler(this.toolStripButtonRefreshLargeClick);
			// 
			// toolStripExClipboard
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripExClipboard, "");
			this.toolStripExClipboard.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripExClipboard.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(59)))), ((int)(((byte)(59)))));
			this.toolStripExClipboard.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripExClipboard.Image = null;
			this.toolStripExClipboard.Location = new System.Drawing.Point(107, 1);
			this.toolStripExClipboard.Name = "toolStripExClipboard";
			this.toolStripExClipboard.Office12Mode = false;
			this.toolStripExClipboard.ShowItemToolTips = true;
			this.toolStripExClipboard.ShowLauncher = false;
			this.toolStripExClipboard.Size = new System.Drawing.Size(106, 87);
			this.toolStripExClipboard.TabIndex = 2;
			this.toolStripExClipboard.Text = "xxClipboard";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripExClipboard, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripExClipboard, false);
			// 
			// toolStripExEdit2
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripExEdit2, "");
			this.toolStripExEdit2.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripExEdit2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(59)))), ((int)(((byte)(59)))));
			this.toolStripExEdit2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripExEdit2.Image = null;
			this.toolStripExEdit2.Location = new System.Drawing.Point(215, 1);
			this.toolStripExEdit2.Name = "toolStripExEdit2";
			this.toolStripExEdit2.Office12Mode = false;
			this.toolStripExEdit2.ShowLauncher = false;
			this.toolStripExEdit2.Size = new System.Drawing.Size(106, 87);
			this.toolStripExEdit2.TabIndex = 12;
			this.toolStripExEdit2.Text = "xxEdit";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripExEdit2, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripExEdit2, false);
			// 
			// toolStripExScheduleViews
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripExScheduleViews, "");
			this.toolStripExScheduleViews.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripExScheduleViews.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(59)))), ((int)(((byte)(59)))));
			this.toolStripExScheduleViews.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripExScheduleViews.Image = null;
			this.toolStripExScheduleViews.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripPanelItemViews2});
			this.toolStripExScheduleViews.Location = new System.Drawing.Point(323, 1);
			this.toolStripExScheduleViews.Name = "toolStripExScheduleViews";
			this.toolStripExScheduleViews.Office12Mode = false;
			this.toolStripExScheduleViews.ShowItemToolTips = true;
			this.toolStripExScheduleViews.ShowLauncher = false;
			this.toolStripExScheduleViews.Size = new System.Drawing.Size(229, 87);
			this.toolStripExScheduleViews.TabIndex = 5;
			this.toolStripExScheduleViews.Text = "xxView";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripExScheduleViews, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripExScheduleViews, false);
			// 
			// toolStripPanelItemViews2
			// 
			this.toolStripPanelItemViews2.CausesValidation = false;
			this.toolStripPanelItemViews2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.toolStripPanelItemViews2.ForeColor = System.Drawing.Color.MidnightBlue;
			this.toolStripPanelItemViews2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonDayView,
            this.toolStripButtonWeekView,
            this.toolStripButtonPeriodView,
            this.toolStripButtonSummaryView,
            this.toolStripButtonRequestView,
            this.toolStripButtonRestrictions,
            this.toolStripButtonFindAgents});
			this.toolStripPanelItemViews2.Name = "toolStripPanelItemViews2";
			this.SetShortcut(this.toolStripPanelItemViews2, System.Windows.Forms.Keys.None);
			this.toolStripPanelItemViews2.Size = new System.Drawing.Size(220, 74);
			this.toolStripPanelItemViews2.Text = "toolStripPanelItem3";
			this.toolStripPanelItemViews2.Transparent = true;
			// 
			// toolStripButtonDayView
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonDayView, "");
			this.toolStripButtonDayView.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_schedule_IntradayView_32x321;
			this.toolStripButtonDayView.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonDayView.Name = "toolStripButtonDayView";
			this.SetShortcut(this.toolStripButtonDayView, System.Windows.Forms.Keys.None);
			this.toolStripButtonDayView.Size = new System.Drawing.Size(56, 20);
			this.toolStripButtonDayView.Text = "xxDay";
			this.toolStripButtonDayView.ToolTipText = "xxDay";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonDayView, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonDayView, false);
			this.toolStripButtonDayView.Click += new System.EventHandler(this.toolStripButtonZoomClick);
			// 
			// toolStripButtonWeekView
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonWeekView, "");
			this.toolStripButtonWeekView.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_schedule_Weekview_32x321;
			this.toolStripButtonWeekView.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonWeekView.Name = "toolStripButtonWeekView";
			this.SetShortcut(this.toolStripButtonWeekView, System.Windows.Forms.Keys.None);
			this.toolStripButtonWeekView.Size = new System.Drawing.Size(66, 20);
			this.toolStripButtonWeekView.Text = "xxWeek";
			this.toolStripButtonWeekView.ToolTipText = "xxWeek";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonWeekView, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonWeekView, false);
			this.toolStripButtonWeekView.Click += new System.EventHandler(this.toolStripButtonZoomClick);
			// 
			// toolStripButtonPeriodView
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonPeriodView, "");
			this.toolStripButtonPeriodView.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_schedule_Period_view_32x321;
			this.toolStripButtonPeriodView.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonPeriodView.Name = "toolStripButtonPeriodView";
			this.SetShortcut(this.toolStripButtonPeriodView, System.Windows.Forms.Keys.None);
			this.toolStripButtonPeriodView.Size = new System.Drawing.Size(67, 20);
			this.toolStripButtonPeriodView.Text = "xxPeriod";
			this.toolStripButtonPeriodView.ToolTipText = "xxPeriod";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonPeriodView, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonPeriodView, false);
			this.toolStripButtonPeriodView.Click += new System.EventHandler(this.toolStripButtonZoomClick);
			// 
			// toolStripButtonSummaryView
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonSummaryView, "");
			this.toolStripButtonSummaryView.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_schedule_Summary_view_32x321;
			this.toolStripButtonSummaryView.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonSummaryView.Name = "toolStripButtonSummaryView";
			this.SetShortcut(this.toolStripButtonSummaryView, System.Windows.Forms.Keys.None);
			this.toolStripButtonSummaryView.Size = new System.Drawing.Size(80, 20);
			this.toolStripButtonSummaryView.Text = "xxSummary";
			this.toolStripButtonSummaryView.ToolTipText = "xxSummary";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonSummaryView, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonSummaryView, false);
			this.toolStripButtonSummaryView.Click += new System.EventHandler(this.toolStripButtonZoomClick);
			// 
			// toolStripButtonRequestView
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonRequestView, "");
			this.toolStripButtonRequestView.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_agent_request_32x32;
			this.toolStripButtonRequestView.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonRequestView.Name = "toolStripButtonRequestView";
			this.SetShortcut(this.toolStripButtonRequestView, System.Windows.Forms.Keys.None);
			this.toolStripButtonRequestView.Size = new System.Drawing.Size(82, 20);
			this.toolStripButtonRequestView.Text = "xxRequests";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonRequestView, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonRequestView, false);
			this.toolStripButtonRequestView.Click += new System.EventHandler(this.toolStripButtonZoomClick);
			// 
			// toolStripButtonRestrictions
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonRestrictions, "");
			this.toolStripButtonRestrictions.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_Contract;
			this.toolStripButtonRestrictions.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonRestrictions.Name = "toolStripButtonRestrictions";
			this.SetShortcut(this.toolStripButtonRestrictions, System.Windows.Forms.Keys.None);
			this.toolStripButtonRestrictions.Size = new System.Drawing.Size(92, 20);
			this.toolStripButtonRestrictions.Text = "xxRestrictions";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonRestrictions, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonRestrictions, false);
			this.toolStripButtonRestrictions.Click += new System.EventHandler(this.toolStripButtonZoomClick);
			// 
			// toolStripButtonFindAgents
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonFindAgents, "");
			this.toolStripButtonFindAgents.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_FindAgent;
			this.toolStripButtonFindAgents.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonFindAgents.Name = "toolStripButtonFindAgents";
			this.SetShortcut(this.toolStripButtonFindAgents, ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F))));
			this.toolStripButtonFindAgents.Size = new System.Drawing.Size(57, 20);
			this.toolStripButtonFindAgents.Text = "xxFind";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonFindAgents, false);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonFindAgents, false);
			this.toolStripButtonFindAgents.Click += new System.EventHandler(this.toolStripMenuItemSearchClick);
			// 
			// toolStripExActions
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripExActions, "");
			this.toolStripExActions.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripExActions.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(59)))), ((int)(((byte)(59)))));
			this.toolStripExActions.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripExActions.Image = null;
			this.toolStripExActions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripPanelItemAssignments});
			this.toolStripExActions.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
			this.toolStripExActions.Location = new System.Drawing.Point(554, 1);
			this.toolStripExActions.Name = "toolStripExActions";
			this.toolStripExActions.Office12Mode = false;
			this.toolStripExActions.ShowCaption = true;
			this.toolStripExActions.ShowItemToolTips = true;
			this.toolStripExActions.ShowLauncher = false;
			this.toolStripExActions.Size = new System.Drawing.Size(114, 87);
			this.toolStripExActions.TabIndex = 7;
			this.toolStripExActions.Text = "xxActions";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripExActions, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripExActions, false);
			// 
			// toolStripPanelItemAssignments
			// 
			this.toolStripPanelItemAssignments.CausesValidation = false;
			this.toolStripPanelItemAssignments.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.toolStripPanelItemAssignments.ForeColor = System.Drawing.Color.MidnightBlue;
			this.toolStripPanelItemAssignments.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSplitButtonSchedule,
            this.toolStripDropDownButtonSwap});
			this.toolStripPanelItemAssignments.Name = "toolStripPanelItemAssignments";
			this.SetShortcut(this.toolStripPanelItemAssignments, System.Windows.Forms.Keys.None);
			this.toolStripPanelItemAssignments.Size = new System.Drawing.Size(107, 50);
			this.toolStripPanelItemAssignments.Text = "toolStripPanelItem1";
			this.toolStripPanelItemAssignments.Transparent = true;
			// 
			// toolStripSplitButtonSchedule
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripSplitButtonSchedule, "");
			this.toolStripSplitButtonSchedule.DropDownButtonWidth = 20;
			this.toolStripSplitButtonSchedule.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemScheduleSelected,
            this.ToolStripMenuItemScheduleHourlyEmployees,
            this.ToolStripMenuItemScheduleOvertime,
            this.toolStripMenuItemOptimize,
            this.toolStripSeparator2,
            this.toolStripMenuItemShiftBackToLegal});
			this.toolStripSplitButtonSchedule.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_SchedulerSchedule;
			this.toolStripSplitButtonSchedule.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripSplitButtonSchedule.Name = "toolStripSplitButtonSchedule";
			this.SetShortcut(this.toolStripSplitButtonSchedule, System.Windows.Forms.Keys.None);
			this.toolStripSplitButtonSchedule.Size = new System.Drawing.Size(103, 20);
			this.toolStripSplitButtonSchedule.Text = "xxSchedule";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripSplitButtonSchedule, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripSplitButtonSchedule, false);
			this.toolStripSplitButtonSchedule.ButtonClick += new System.EventHandler(this.toolStripMenuItemScheduleClick);
			// 
			// toolStripMenuItemScheduleSelected
			// 
			this.toolStripMenuItemScheduleSelected.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_Scheduler_ScheduleSelected;
			this.toolStripMenuItemScheduleSelected.Name = "toolStripMenuItemScheduleSelected";
			this.SetShortcut(this.toolStripMenuItemScheduleSelected, System.Windows.Forms.Keys.F12);
			this.toolStripMenuItemScheduleSelected.ShortcutKeys = System.Windows.Forms.Keys.F12;
			this.toolStripMenuItemScheduleSelected.Size = new System.Drawing.Size(210, 22);
			this.toolStripMenuItemScheduleSelected.Text = "xxScheduleVerb";
			this.toolStripMenuItemScheduleSelected.Click += new System.EventHandler(this.toolStripMenuItemScheduleClick);
			// 
			// ToolStripMenuItemScheduleHourlyEmployees
			// 
			this.ToolStripMenuItemScheduleHourlyEmployees.Name = "ToolStripMenuItemScheduleHourlyEmployees";
			this.SetShortcut(this.ToolStripMenuItemScheduleHourlyEmployees, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemScheduleHourlyEmployees.Size = new System.Drawing.Size(210, 22);
			this.ToolStripMenuItemScheduleHourlyEmployees.Text = "xxScheduleHourlyEmployees";
			this.ToolStripMenuItemScheduleHourlyEmployees.Click += new System.EventHandler(this.toolStripMenuItemScheduleHourlyEmployeesClick);
			// 
			// ToolStripMenuItemScheduleOvertime
			// 
			this.ToolStripMenuItemScheduleOvertime.Name = "ToolStripMenuItemScheduleOvertime";
			this.SetShortcut(this.ToolStripMenuItemScheduleOvertime, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemScheduleOvertime.Size = new System.Drawing.Size(210, 22);
			this.ToolStripMenuItemScheduleOvertime.Text = "xxScheduleOvertime";
			this.ToolStripMenuItemScheduleOvertime.Click += new System.EventHandler(this.toolStripMenuItemScheduleOvertimeClick);
			// 
			// toolStripMenuItemOptimize
			// 
			this.toolStripMenuItemOptimize.Name = "toolStripMenuItemOptimize";
			this.SetShortcut(this.toolStripMenuItemOptimize, System.Windows.Forms.Keys.F11);
			this.toolStripMenuItemOptimize.ShortcutKeys = System.Windows.Forms.Keys.F11;
			this.toolStripMenuItemOptimize.Size = new System.Drawing.Size(210, 22);
			this.toolStripMenuItemOptimize.Text = "xxReOptimize";
			this.toolStripMenuItemOptimize.Click += new System.EventHandler(this.toolStripMenuItemOptimizeClick);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.SetShortcut(this.toolStripSeparator2, System.Windows.Forms.Keys.None);
			this.toolStripSeparator2.Size = new System.Drawing.Size(207, 6);
			// 
			// toolStripMenuItemShiftBackToLegal
			// 
			this.toolStripMenuItemShiftBackToLegal.Name = "toolStripMenuItemShiftBackToLegal";
			this.SetShortcut(this.toolStripMenuItemShiftBackToLegal, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemShiftBackToLegal.Size = new System.Drawing.Size(210, 22);
			this.toolStripMenuItemShiftBackToLegal.Text = "xxShiftBackToLegal";
			this.toolStripMenuItemShiftBackToLegal.Click += new System.EventHandler(this.toolStripMenuItemShiftBackToLegalClick);
			// 
			// toolStripDropDownButtonSwap
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripDropDownButtonSwap, "");
			this.toolStripDropDownButtonSwap.DropDownButtonWidth = 20;
			this.toolStripDropDownButtonSwap.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemSwap,
            this.toolStripMenuItemSwapAndReschedule,
            this.ToolStripMenuItemSwapRaw});
			this.toolStripDropDownButtonSwap.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_SwapShifts_32x32;
			this.toolStripDropDownButtonSwap.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripDropDownButtonSwap.Name = "toolStripDropDownButtonSwap";
			this.SetShortcut(this.toolStripDropDownButtonSwap, System.Windows.Forms.Keys.None);
			this.toolStripDropDownButtonSwap.Size = new System.Drawing.Size(85, 20);
			this.toolStripDropDownButtonSwap.Text = "xxSwap";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripDropDownButtonSwap, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripDropDownButtonSwap, false);
			this.toolStripDropDownButtonSwap.ButtonClick += new System.EventHandler(this.toolStripMenuItemSwapClick);
			// 
			// toolStripMenuItemSwap
			// 
			this.toolStripMenuItemSwap.Name = "toolStripMenuItemSwap";
			this.SetShortcut(this.toolStripMenuItemSwap, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemSwap.Size = new System.Drawing.Size(187, 22);
			this.toolStripMenuItemSwap.Text = "xxSwap";
			this.toolStripMenuItemSwap.Click += new System.EventHandler(this.toolStripMenuItemSwapClick);
			// 
			// toolStripMenuItemSwapAndReschedule
			// 
			this.toolStripMenuItemSwapAndReschedule.Name = "toolStripMenuItemSwapAndReschedule";
			this.SetShortcut(this.toolStripMenuItemSwapAndReschedule, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemSwapAndReschedule.Size = new System.Drawing.Size(187, 22);
			this.toolStripMenuItemSwapAndReschedule.Text = "xxSwapAndReschedule";
			this.toolStripMenuItemSwapAndReschedule.Click += new System.EventHandler(this.toolStripMenuItemSwapAndRescheduleClick);
			// 
			// ToolStripMenuItemSwapRaw
			// 
			this.ToolStripMenuItemSwapRaw.Name = "ToolStripMenuItemSwapRaw";
			this.SetShortcut(this.ToolStripMenuItemSwapRaw, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemSwapRaw.Size = new System.Drawing.Size(187, 22);
			this.ToolStripMenuItemSwapRaw.Text = "xxSwapRaw";
			this.ToolStripMenuItemSwapRaw.Click += new System.EventHandler(this.toolStripMenuItemSwapRawClick);
			// 
			// toolStripExLocks
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripExLocks, "");
			this.toolStripExLocks.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripExLocks.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(59)))), ((int)(((byte)(59)))));
			this.toolStripExLocks.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripExLocks.Image = null;
			this.toolStripExLocks.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripPanelItemLocks});
			this.toolStripExLocks.Location = new System.Drawing.Point(670, 1);
			this.toolStripExLocks.Name = "toolStripExLocks";
			this.toolStripExLocks.Office12Mode = false;
			this.toolStripExLocks.ShowCaption = true;
			this.toolStripExLocks.ShowLauncher = false;
			this.toolStripExLocks.Size = new System.Drawing.Size(105, 87);
			this.toolStripExLocks.TabIndex = 10;
			this.toolStripExLocks.Text = "xxLocks";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripExLocks, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripExLocks, false);
			// 
			// toolStripPanelItemLocks
			// 
			this.toolStripPanelItemLocks.CausesValidation = false;
			this.toolStripPanelItemLocks.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.toolStripPanelItemLocks.ForeColor = System.Drawing.Color.MidnightBlue;
			this.toolStripPanelItemLocks.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSplitButtonUnlock,
            this.toolStripSplitButtonLock});
			this.toolStripPanelItemLocks.Name = "toolStripPanelItemLocks";
			this.SetShortcut(this.toolStripPanelItemLocks, System.Windows.Forms.Keys.None);
			this.toolStripPanelItemLocks.Size = new System.Drawing.Size(96, 74);
			this.toolStripPanelItemLocks.Text = "toolStripPanelItem1";
			this.toolStripPanelItemLocks.Transparent = true;
			// 
			// toolStripSplitButtonUnlock
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripSplitButtonUnlock, "");
			this.toolStripSplitButtonUnlock.DropDownButtonWidth = 20;
			this.toolStripSplitButtonUnlock.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemUnlockAll,
            this.toolStripMenuItemUnlockSelection});
			this.toolStripSplitButtonUnlock.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_Unlock;
			this.toolStripSplitButtonUnlock.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripSplitButtonUnlock.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripSplitButtonUnlock.Name = "toolStripSplitButtonUnlock";
			this.SetShortcut(this.toolStripSplitButtonUnlock, System.Windows.Forms.Keys.None);
			this.toolStripSplitButtonUnlock.Size = new System.Drawing.Size(92, 20);
			this.toolStripSplitButtonUnlock.Text = "xxUnlock";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripSplitButtonUnlock, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripSplitButtonUnlock, false);
			this.toolStripSplitButtonUnlock.ButtonClick += new System.EventHandler(this.toolStripSplitButtonUnlockButtonClick);
			// 
			// toolStripMenuItemUnlockAll
			// 
			this.toolStripMenuItemUnlockAll.Name = "toolStripMenuItemUnlockAll";
			this.SetShortcut(this.toolStripMenuItemUnlockAll, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemUnlockAll.Size = new System.Drawing.Size(162, 22);
			this.toolStripMenuItemUnlockAll.Text = "xxUnlockAll";
			this.toolStripMenuItemUnlockAll.MouseDown += new System.Windows.Forms.MouseEventHandler(this.toolStripMenuItemUnlockAllRmMouseUp);
			// 
			// toolStripMenuItemUnlockSelection
			// 
			this.toolStripMenuItemUnlockSelection.Name = "toolStripMenuItemUnlockSelection";
			this.SetShortcut(this.toolStripMenuItemUnlockSelection, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemUnlockSelection.Size = new System.Drawing.Size(162, 22);
			this.toolStripMenuItemUnlockSelection.Text = "xxUnlockSelection";
			this.toolStripMenuItemUnlockSelection.MouseDown += new System.Windows.Forms.MouseEventHandler(this.toolStripMenuItemUnlockSelectionRmMouseUp);
			// 
			// toolStripSplitButtonLock
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripSplitButtonLock, "");
			this.toolStripSplitButtonLock.DropDownButtonWidth = 20;
			this.toolStripSplitButtonLock.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemLockSelection,
            this.toolStripMenuItemLockAbsence,
            this.toolStripMenuItemLockDayOff,
            this.toolStripMenuItemLockShiftCategory,
            this.ToolStripMenuItemLockRestrictions,
            this.toolStripMenuItemLockTags,
            this.toolStripMenuItem6,
            this.toolStripMenuItemWriteProtectSchedule2,
            this.ToolStripMenuItemRemoveWriteProtectionToolBar});
			this.toolStripSplitButtonLock.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_Lock2;
			this.toolStripSplitButtonLock.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripSplitButtonLock.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripSplitButtonLock.Name = "toolStripSplitButtonLock";
			this.SetShortcut(this.toolStripSplitButtonLock, System.Windows.Forms.Keys.None);
			this.toolStripSplitButtonLock.Size = new System.Drawing.Size(82, 20);
			this.toolStripSplitButtonLock.Text = "xxLock";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripSplitButtonLock, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripSplitButtonLock, false);
			this.toolStripSplitButtonLock.ButtonClick += new System.EventHandler(this.toolStripMenuItemLockSelectionClick);
			// 
			// toolStripMenuItemLockSelection
			// 
			this.toolStripMenuItemLockSelection.Name = "toolStripMenuItemLockSelection";
			this.SetShortcut(this.toolStripMenuItemLockSelection, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemLockSelection.Size = new System.Drawing.Size(197, 22);
			this.toolStripMenuItemLockSelection.Text = "xxLockSelection";
			this.toolStripMenuItemLockSelection.Click += new System.EventHandler(this.toolStripMenuItemLockSelectionClick);
			// 
			// toolStripMenuItemLockAbsence
			// 
			this.toolStripMenuItemLockAbsence.Name = "toolStripMenuItemLockAbsence";
			this.SetShortcut(this.toolStripMenuItemLockAbsence, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemLockAbsence.Size = new System.Drawing.Size(197, 22);
			this.toolStripMenuItemLockAbsence.Text = "xxLockAbsence";
			// 
			// toolStripMenuItemLockDayOff
			// 
			this.toolStripMenuItemLockDayOff.Name = "toolStripMenuItemLockDayOff";
			this.SetShortcut(this.toolStripMenuItemLockDayOff, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemLockDayOff.Size = new System.Drawing.Size(197, 22);
			this.toolStripMenuItemLockDayOff.Text = "xxLockDayOff";
			// 
			// toolStripMenuItemLockShiftCategory
			// 
			this.toolStripMenuItemLockShiftCategory.Name = "toolStripMenuItemLockShiftCategory";
			this.SetShortcut(this.toolStripMenuItemLockShiftCategory, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemLockShiftCategory.Size = new System.Drawing.Size(197, 22);
			this.toolStripMenuItemLockShiftCategory.Text = "xxLockShiftCategory";
			// 
			// ToolStripMenuItemLockRestrictions
			// 
			this.ToolStripMenuItemLockRestrictions.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItemLockAllRestrictions,
            this.ToolStripMenuItemLockPreferences,
            this.ToolStripMenuItemLockRotations,
            this.ToolStripMenuItemLockStudentAvailability,
            this.ToolStripMenuItemLockAvailability});
			this.ToolStripMenuItemLockRestrictions.Name = "ToolStripMenuItemLockRestrictions";
			this.SetShortcut(this.ToolStripMenuItemLockRestrictions, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemLockRestrictions.Size = new System.Drawing.Size(197, 22);
			this.ToolStripMenuItemLockRestrictions.Text = "xxLockRestrictions";
			// 
			// ToolStripMenuItemLockAllRestrictions
			// 
			this.ToolStripMenuItemLockAllRestrictions.Name = "ToolStripMenuItemLockAllRestrictions";
			this.SetShortcut(this.ToolStripMenuItemLockAllRestrictions, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemLockAllRestrictions.Size = new System.Drawing.Size(194, 22);
			this.ToolStripMenuItemLockAllRestrictions.Text = "xxAll";
			this.ToolStripMenuItemLockAllRestrictions.MouseUp += new System.Windows.Forms.MouseEventHandler(this.toolStripMenuItemLockAllRestrictionsMouseUp);
			// 
			// ToolStripMenuItemLockPreferences
			// 
			this.ToolStripMenuItemLockPreferences.Name = "ToolStripMenuItemLockPreferences";
			this.SetShortcut(this.ToolStripMenuItemLockPreferences, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemLockPreferences.Size = new System.Drawing.Size(194, 22);
			this.ToolStripMenuItemLockPreferences.Text = "xxLockPreferences";
			// 
			// ToolStripMenuItemLockRotations
			// 
			this.ToolStripMenuItemLockRotations.Name = "ToolStripMenuItemLockRotations";
			this.SetShortcut(this.ToolStripMenuItemLockRotations, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemLockRotations.Size = new System.Drawing.Size(194, 22);
			this.ToolStripMenuItemLockRotations.Text = "xxLockRotations";
			// 
			// ToolStripMenuItemLockStudentAvailability
			// 
			this.ToolStripMenuItemLockStudentAvailability.Name = "ToolStripMenuItemLockStudentAvailability";
			this.SetShortcut(this.ToolStripMenuItemLockStudentAvailability, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemLockStudentAvailability.Size = new System.Drawing.Size(194, 22);
			this.ToolStripMenuItemLockStudentAvailability.Text = "xxLockStudentAvailability";
			// 
			// ToolStripMenuItemLockAvailability
			// 
			this.ToolStripMenuItemLockAvailability.Name = "ToolStripMenuItemLockAvailability";
			this.SetShortcut(this.ToolStripMenuItemLockAvailability, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemLockAvailability.Size = new System.Drawing.Size(194, 22);
			this.ToolStripMenuItemLockAvailability.Text = "xxLockAvailability";
			// 
			// toolStripMenuItemLockTags
			// 
			this.toolStripMenuItemLockTags.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemLockAllTags});
			this.toolStripMenuItemLockTags.Name = "toolStripMenuItemLockTags";
			this.SetShortcut(this.toolStripMenuItemLockTags, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemLockTags.Size = new System.Drawing.Size(197, 22);
			this.toolStripMenuItemLockTags.Text = "xxLockTags";
			// 
			// toolStripMenuItemLockAllTags
			// 
			this.toolStripMenuItemLockAllTags.Name = "toolStripMenuItemLockAllTags";
			this.SetShortcut(this.toolStripMenuItemLockAllTags, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemLockAllTags.Size = new System.Drawing.Size(95, 22);
			this.toolStripMenuItemLockAllTags.Text = "xxAll";
			this.toolStripMenuItemLockAllTags.MouseUp += new System.Windows.Forms.MouseEventHandler(this.toolStripMenuItemLockAllTagsMouseUp);
			// 
			// toolStripMenuItem6
			// 
			this.toolStripMenuItem6.Name = "toolStripMenuItem6";
			this.SetShortcut(this.toolStripMenuItem6, System.Windows.Forms.Keys.None);
			this.toolStripMenuItem6.Size = new System.Drawing.Size(194, 6);
			// 
			// toolStripMenuItemWriteProtectSchedule2
			// 
			this.toolStripMenuItemWriteProtectSchedule2.Name = "toolStripMenuItemWriteProtectSchedule2";
			this.SetShortcut(this.toolStripMenuItemWriteProtectSchedule2, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemWriteProtectSchedule2.Size = new System.Drawing.Size(197, 22);
			this.toolStripMenuItemWriteProtectSchedule2.Text = "xxWriteProtectSchedule";
			this.toolStripMenuItemWriteProtectSchedule2.Click += new System.EventHandler(this.toolStripMenuItemWriteProtectSchedule2Click);
			// 
			// ToolStripMenuItemRemoveWriteProtectionToolBar
			// 
			this.ToolStripMenuItemRemoveWriteProtectionToolBar.Name = "ToolStripMenuItemRemoveWriteProtectionToolBar";
			this.SetShortcut(this.ToolStripMenuItemRemoveWriteProtectionToolBar, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemRemoveWriteProtectionToolBar.Size = new System.Drawing.Size(197, 22);
			this.ToolStripMenuItemRemoveWriteProtectionToolBar.Text = "xxRemoveWriteProtection";
			this.ToolStripMenuItemRemoveWriteProtectionToolBar.MouseUp += new System.Windows.Forms.MouseEventHandler(this.toolstripMenuRemoveWriteProtectionMouseUp);
			// 
			// toolStripExTags
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripExTags, "");
			this.toolStripExTags.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripExTags.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(59)))), ((int)(((byte)(59)))));
			this.toolStripExTags.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripExTags.Image = null;
			this.toolStripExTags.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripPanelItemTags});
			this.toolStripExTags.Location = new System.Drawing.Point(777, 1);
			this.toolStripExTags.Name = "toolStripExTags";
			this.toolStripExTags.Office12Mode = false;
			this.toolStripExTags.ShowLauncher = false;
			this.toolStripExTags.Size = new System.Drawing.Size(145, 87);
			this.toolStripExTags.TabIndex = 15;
			this.toolStripExTags.Text = "xxTags";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripExTags, false);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripExTags, false);
			this.toolStripExTags.SizeChanged += new System.EventHandler(this.toolStripExTagsSizeChanged);
			// 
			// toolStripPanelItemTags
			// 
			this.toolStripPanelItemTags.CausesValidation = false;
			this.toolStripPanelItemTags.ForeColor = System.Drawing.Color.MidnightBlue;
			this.toolStripPanelItemTags.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabelAutoTag,
            this.toolStripComboBoxAutoTag,
            this.toolStripSplitButtonChangeTag});
			this.toolStripPanelItemTags.Name = "toolStripPanelItemTags";
			this.SetShortcut(this.toolStripPanelItemTags, System.Windows.Forms.Keys.None);
			this.toolStripPanelItemTags.Size = new System.Drawing.Size(136, 74);
			this.toolStripPanelItemTags.Text = "toolStripPanelItem2";
			this.toolStripPanelItemTags.Transparent = true;
			// 
			// toolStripLabelAutoTag
			// 
			this.toolStripLabelAutoTag.Name = "toolStripLabelAutoTag";
			this.SetShortcut(this.toolStripLabelAutoTag, System.Windows.Forms.Keys.None);
			this.toolStripLabelAutoTag.Size = new System.Drawing.Size(62, 15);
			this.toolStripLabelAutoTag.Text = "xxAutoTag";
			// 
			// toolStripComboBoxAutoTag
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripComboBoxAutoTag, "");
			this.toolStripComboBoxAutoTag.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.toolStripComboBoxAutoTag.MaxDropDownItems = 100;
			this.toolStripComboBoxAutoTag.Name = "toolStripComboBoxAutoTag";
			this.SetShortcut(this.toolStripComboBoxAutoTag, System.Windows.Forms.Keys.None);
			this.toolStripComboBoxAutoTag.Size = new System.Drawing.Size(130, 23);
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripComboBoxAutoTag, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripComboBoxAutoTag, false);
			this.toolStripComboBoxAutoTag.SelectedIndexChanged += new System.EventHandler(this.toolStripComboBoxAutoTagSelectedIndexChanged);
			// 
			// toolStripSplitButtonChangeTag
			// 
			this.toolStripSplitButtonChangeTag.AutoSize = false;
			this.ribbonControlAdv1.SetDescription(this.toolStripSplitButtonChangeTag, "");
			this.toolStripSplitButtonChangeTag.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.tag_blue;
			this.toolStripSplitButtonChangeTag.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripSplitButtonChangeTag.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripSplitButtonChangeTag.Margin = new System.Windows.Forms.Padding(0, 5, 0, 2);
			this.toolStripSplitButtonChangeTag.Name = "toolStripSplitButtonChangeTag";
			this.SetShortcut(this.toolStripSplitButtonChangeTag, System.Windows.Forms.Keys.None);
			this.toolStripSplitButtonChangeTag.Size = new System.Drawing.Size(130, 20);
			this.toolStripSplitButtonChangeTag.Text = "xxChangeTag";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripSplitButtonChangeTag, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripSplitButtonChangeTag, false);
			// 
			// toolStripEx1
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripEx1, "");
			this.toolStripEx1.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripEx1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(59)))), ((int)(((byte)(59)))));
			this.toolStripEx1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripEx1.Image = null;
			this.toolStripEx1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripPanelItem1});
			this.toolStripEx1.Location = new System.Drawing.Point(924, 1);
			this.toolStripEx1.Name = "toolStripEx1";
			this.toolStripEx1.Office12Mode = false;
			this.toolStripEx1.Padding = new System.Windows.Forms.Padding(0, 0, 1, 3);
			this.toolStripEx1.ShowCaption = true;
			this.toolStripEx1.ShowLauncher = false;
			this.toolStripEx1.Size = new System.Drawing.Size(189, 87);
			this.toolStripEx1.TabIndex = 13;
			this.toolStripEx1.Text = "xxShow";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripEx1, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripEx1, false);
			// 
			// toolStripPanelItem1
			// 
			this.toolStripPanelItem1.CausesValidation = false;
			this.toolStripPanelItem1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.toolStripPanelItem1.ForeColor = System.Drawing.Color.MidnightBlue;
			this.toolStripPanelItem1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonShowGraph,
            this.toolStripButtonShowResult,
            this.toolStripButtonShowEditor,
            this.toolStripButtonShowPropertyPanel});
			this.toolStripPanelItem1.Name = "toolStripPanelItem1";
			this.toolStripPanelItem1.Padding = new System.Windows.Forms.Padding(2, 2, 2, 0);
			this.SetShortcut(this.toolStripPanelItem1, System.Windows.Forms.Keys.None);
			this.toolStripPanelItem1.Size = new System.Drawing.Size(180, 71);
			this.toolStripPanelItem1.Text = "toolStripPanelItem1";
			this.toolStripPanelItem1.Transparent = true;
			// 
			// toolStripButtonShowGraph
			// 
			this.toolStripButtonShowGraph.Checked = true;
			this.toolStripButtonShowGraph.CheckState = System.Windows.Forms.CheckState.Checked;
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonShowGraph, "");
			this.toolStripButtonShowGraph.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_Realtime_adherence_16x16;
			this.toolStripButtonShowGraph.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonShowGraph.Name = "toolStripButtonShowGraph";
			this.SetShortcut(this.toolStripButtonShowGraph, System.Windows.Forms.Keys.None);
			this.toolStripButtonShowGraph.Size = new System.Drawing.Size(93, 20);
			this.toolStripButtonShowGraph.Text = "xxShowGraph";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonShowGraph, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonShowGraph, false);
			this.toolStripButtonShowGraph.Click += new System.EventHandler(this.toolStripButtonShowGraphClick);
			// 
			// toolStripButtonShowResult
			// 
			this.toolStripButtonShowResult.Checked = true;
			this.toolStripButtonShowResult.CheckState = System.Windows.Forms.CheckState.Checked;
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonShowResult, "");
			this.toolStripButtonShowResult.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_result_view_16x16;
			this.toolStripButtonShowResult.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonShowResult.Name = "toolStripButtonShowResult";
			this.SetShortcut(this.toolStripButtonShowResult, System.Windows.Forms.Keys.None);
			this.toolStripButtonShowResult.Size = new System.Drawing.Size(94, 20);
			this.toolStripButtonShowResult.Text = "xxShowResult";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonShowResult, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonShowResult, false);
			this.toolStripButtonShowResult.Click += new System.EventHandler(this.toolStripButtonShowResultClick);
			// 
			// toolStripButtonShowEditor
			// 
			this.toolStripButtonShowEditor.Checked = true;
			this.toolStripButtonShowEditor.CheckState = System.Windows.Forms.CheckState.Checked;
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonShowEditor, "");
			this.toolStripButtonShowEditor.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.editor;
			this.toolStripButtonShowEditor.ImageTransparentColor = System.Drawing.Color.White;
			this.toolStripButtonShowEditor.Margin = new System.Windows.Forms.Padding(0, 1, 0, 0);
			this.toolStripButtonShowEditor.Name = "toolStripButtonShowEditor";
			this.SetShortcut(this.toolStripButtonShowEditor, System.Windows.Forms.Keys.None);
			this.toolStripButtonShowEditor.Size = new System.Drawing.Size(91, 20);
			this.toolStripButtonShowEditor.Text = "xxShowEditor";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonShowEditor, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonShowEditor, false);
			this.toolStripButtonShowEditor.Click += new System.EventHandler(this.toolStripButtonShowEditorClick);
			// 
			// toolStripButtonShowPropertyPanel
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonShowPropertyPanel, "");
			this.toolStripButtonShowPropertyPanel.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_AgentInfo2;
			this.toolStripButtonShowPropertyPanel.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonShowPropertyPanel.Name = "toolStripButtonShowPropertyPanel";
			this.SetShortcut(this.toolStripButtonShowPropertyPanel, ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.I))));
			this.toolStripButtonShowPropertyPanel.Size = new System.Drawing.Size(82, 20);
			this.toolStripButtonShowPropertyPanel.Text = "xxInfoPanel";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonShowPropertyPanel, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonShowPropertyPanel, false);
			this.toolStripButtonShowPropertyPanel.Click += new System.EventHandler(this.toolStripButtonShowPropertyPanelClick);
			// 
			// toolStripExLoadOptions
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripExLoadOptions, "");
			this.toolStripExLoadOptions.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripExLoadOptions.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(59)))), ((int)(((byte)(59)))));
			this.toolStripExLoadOptions.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripExLoadOptions.Image = null;
			this.toolStripExLoadOptions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripPanelItemLoadOptions});
			this.toolStripExLoadOptions.Location = new System.Drawing.Point(1115, 1);
			this.toolStripExLoadOptions.Name = "toolStripExLoadOptions";
			this.toolStripExLoadOptions.Office12Mode = false;
			this.toolStripExLoadOptions.ShowCaption = true;
			this.toolStripExLoadOptions.ShowLauncher = false;
			this.toolStripExLoadOptions.Size = new System.Drawing.Size(115, 87);
			this.toolStripExLoadOptions.TabIndex = 14;
			this.toolStripExLoadOptions.Text = "xxLoadOptions";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripExLoadOptions, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripExLoadOptions, false);
			// 
			// toolStripPanelItemLoadOptions
			// 
			this.toolStripPanelItemLoadOptions.CausesValidation = false;
			this.toolStripPanelItemLoadOptions.ForeColor = System.Drawing.Color.MidnightBlue;
			this.toolStripPanelItemLoadOptions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonShrinkage,
            this.toolStripButtonCalculation,
            this.toolStripButtonValidation});
			this.toolStripPanelItemLoadOptions.Name = "toolStripPanelItemLoadOptions";
			this.SetShortcut(this.toolStripPanelItemLoadOptions, System.Windows.Forms.Keys.None);
			this.toolStripPanelItemLoadOptions.Size = new System.Drawing.Size(106, 74);
			this.toolStripPanelItemLoadOptions.Transparent = true;
			// 
			// toolStripButtonShrinkage
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonShrinkage, "");
			this.toolStripButtonShrinkage.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_shrinkage;
			this.toolStripButtonShrinkage.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonShrinkage.Name = "toolStripButtonShrinkage";
			this.SetShortcut(this.toolStripButtonShrinkage, System.Windows.Forms.Keys.None);
			this.toolStripButtonShrinkage.Size = new System.Drawing.Size(89, 20);
			this.toolStripButtonShrinkage.Text = "xxShrinkage";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonShrinkage, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonShrinkage, false);
			this.toolStripButtonShrinkage.Click += new System.EventHandler(this.toolStripButtonShrinkageClick);
			// 
			// toolStripButtonCalculation
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonCalculation, "");
			this.toolStripButtonCalculation.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_calculation;
			this.toolStripButtonCalculation.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonCalculation.Name = "toolStripButtonCalculation";
			this.SetShortcut(this.toolStripButtonCalculation, System.Windows.Forms.Keys.None);
			this.toolStripButtonCalculation.Size = new System.Drawing.Size(102, 20);
			this.toolStripButtonCalculation.Text = "xxCalculations";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonCalculation, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonCalculation, false);
			this.toolStripButtonCalculation.Click += new System.EventHandler(this.toolStripButtonCalculationClick);
			// 
			// toolStripButtonValidation
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonValidation, "");
			this.toolStripButtonValidation.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_validation;
			this.toolStripButtonValidation.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonValidation.Name = "toolStripButtonValidation";
			this.SetShortcut(this.toolStripButtonValidation, System.Windows.Forms.Keys.None);
			this.toolStripButtonValidation.Size = new System.Drawing.Size(94, 20);
			this.toolStripButtonValidation.Text = "xxValidations";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonValidation, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonValidation, false);
			this.toolStripButtonValidation.Click += new System.EventHandler(this.toolStripButtonValidationClick);
			// 
			// toolStripExFilter
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripExFilter, "");
			this.toolStripExFilter.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripExFilter.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(59)))), ((int)(((byte)(59)))));
			this.toolStripExFilter.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripExFilter.Image = null;
			this.toolStripExFilter.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripPanelItem3});
			this.toolStripExFilter.Location = new System.Drawing.Point(1232, 1);
			this.toolStripExFilter.Name = "toolStripExFilter";
			this.toolStripExFilter.Office12Mode = false;
			this.toolStripExFilter.ShowLauncher = false;
			this.toolStripExFilter.Size = new System.Drawing.Size(141, 87);
			this.toolStripExFilter.TabIndex = 16;
			this.toolStripExFilter.Text = "xxFilter";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripExFilter, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripExFilter, false);
			// 
			// toolStripPanelItem3
			// 
			this.toolStripPanelItem3.CausesValidation = false;
			this.toolStripPanelItem3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.toolStripPanelItem3.ForeColor = System.Drawing.Color.MidnightBlue;
			this.toolStripPanelItem3.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonFilterAgents,
            this.toolStripButtonFilterOvertimeAvailability,
            this.toolStripButtonFilterStudentAvailability});
			this.toolStripPanelItem3.Name = "toolStripPanelItem3";
			this.SetShortcut(this.toolStripPanelItem3, System.Windows.Forms.Keys.None);
			this.toolStripPanelItem3.Size = new System.Drawing.Size(132, 74);
			this.toolStripPanelItem3.Text = "toolStripPanelItem1";
			this.toolStripPanelItem3.Transparent = true;
			// 
			// toolStripButtonFilterAgents
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonFilterAgents, "");
			this.toolStripButtonFilterAgents.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_Filter;
			this.toolStripButtonFilterAgents.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonFilterAgents.Name = "toolStripButtonFilterAgents";
			this.SetShortcut(this.toolStripButtonFilterAgents, System.Windows.Forms.Keys.None);
			this.toolStripButtonFilterAgents.Size = new System.Drawing.Size(70, 20);
			this.toolStripButtonFilterAgents.Text = "xxAgents";
			this.toolStripButtonFilterAgents.ToolTipText = "xxFilterAgents";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonFilterAgents, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonFilterAgents, false);
			this.toolStripButtonFilterAgents.Click += new System.EventHandler(this.toolStripButtonFilterAgentsClick);
			// 
			// toolStripButtonFilterOvertimeAvailability
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonFilterOvertimeAvailability, "");
			this.toolStripButtonFilterOvertimeAvailability.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_Filter;
			this.toolStripButtonFilterOvertimeAvailability.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonFilterOvertimeAvailability.Name = "toolStripButtonFilterOvertimeAvailability";
			this.SetShortcut(this.toolStripButtonFilterOvertimeAvailability, System.Windows.Forms.Keys.None);
			this.toolStripButtonFilterOvertimeAvailability.Size = new System.Drawing.Size(128, 20);
			this.toolStripButtonFilterOvertimeAvailability.Text = "xxOvertimeAvailability";
			this.toolStripButtonFilterOvertimeAvailability.ToolTipText = "xxFilterOvertimeAvailability";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonFilterOvertimeAvailability, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonFilterOvertimeAvailability, false);
			this.toolStripButtonFilterOvertimeAvailability.Click += new System.EventHandler(this.toolStripButtonFilterOvertimeAvailabilityClick);
			// 
			// toolStripButtonFilterStudentAvailability
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonFilterStudentAvailability, "");
			this.toolStripButtonFilterStudentAvailability.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_Filter;
			this.toolStripButtonFilterStudentAvailability.ImageTransparentColor = System.Drawing.Color.White;
			this.toolStripButtonFilterStudentAvailability.Name = "toolStripButtonFilterStudentAvailability";
			this.SetShortcut(this.toolStripButtonFilterStudentAvailability, System.Windows.Forms.Keys.None);
			this.toolStripButtonFilterStudentAvailability.Size = new System.Drawing.Size(123, 20);
			this.toolStripButtonFilterStudentAvailability.Text = "xxStudentAvailability";
			this.toolStripButtonFilterStudentAvailability.ToolTipText = "xxFilterStudentAvailability";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonFilterStudentAvailability, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonFilterStudentAvailability, false);
			this.toolStripButtonFilterStudentAvailability.Visible = false;
			this.toolStripButtonFilterStudentAvailability.Click += new System.EventHandler(this.toolStripButtonFilterStudentAvailabilityClick);
			// 
			// toolStripTabItemChart
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripTabItemChart, "");
			this.toolStripTabItemChart.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.toolStripTabItemChart.Name = "toolStripTabItemChart";
			// 
			// ribbonControlAdv1.ribbonPanel2
			// 
			this.toolStripTabItemChart.Panel.Controls.Add(this.toolStripExGridRowInChartButtons);
			this.toolStripTabItemChart.Panel.Controls.Add(this.toolStripExSkillViews);
			this.toolStripTabItemChart.Panel.Name = "ribbonPanel2";
			this.toolStripTabItemChart.Panel.ScrollPosition = 0;
			this.toolStripTabItemChart.Panel.TabIndex = 6;
			this.toolStripTabItemChart.Panel.Text = "XXChart";
			this.toolStripTabItemChart.Position = 1;
			this.SetShortcut(this.toolStripTabItemChart, System.Windows.Forms.Keys.None);
			this.toolStripTabItemChart.Size = new System.Drawing.Size(70, 25);
			this.toolStripTabItemChart.Text = "XXChart";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripTabItemChart, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripTabItemChart, false);
			this.toolStripTabItemChart.Click += new System.EventHandler(this.toolStripTabItemChartClick);
			// 
			// toolStripExGridRowInChartButtons
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripExGridRowInChartButtons, "");
			this.toolStripExGridRowInChartButtons.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripExGridRowInChartButtons.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(59)))), ((int)(((byte)(59)))));
			this.toolStripExGridRowInChartButtons.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripExGridRowInChartButtons.Image = null;
			this.toolStripExGridRowInChartButtons.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonGridInChart});
			this.toolStripExGridRowInChartButtons.Location = new System.Drawing.Point(0, 1);
			this.toolStripExGridRowInChartButtons.Name = "toolStripExGridRowInChartButtons";
			this.toolStripExGridRowInChartButtons.Office12Mode = false;
			this.toolStripExGridRowInChartButtons.ShowLauncher = false;
			this.toolStripExGridRowInChartButtons.Size = new System.Drawing.Size(89, 0);
			this.toolStripExGridRowInChartButtons.TabIndex = 11;
			this.toolStripExGridRowInChartButtons.Text = "xxGridRowsInChart";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripExGridRowInChartButtons, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripExGridRowInChartButtons, false);
			// 
			// toolStripButtonGridInChart
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonGridInChart, "");
			this.toolStripButtonGridInChart.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonGridInChart.Name = "toolStripButtonGridInChart";
			this.SetShortcut(this.toolStripButtonGridInChart, System.Windows.Forms.Keys.None);
			this.toolStripButtonGridInChart.Size = new System.Drawing.Size(82, 0);
			this.toolStripButtonGridInChart.Text = "xxGridInChart";
			this.toolStripButtonGridInChart.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonGridInChart, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonGridInChart, false);
			this.toolStripButtonGridInChart.Click += new System.EventHandler(this.toolStripButtonGridInChartClick);
			// 
			// toolStripExSkillViews
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripExSkillViews, "");
			this.toolStripExSkillViews.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripExSkillViews.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(59)))), ((int)(((byte)(59)))));
			this.toolStripExSkillViews.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripExSkillViews.Image = null;
			this.toolStripExSkillViews.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripPanelItem2});
			this.toolStripExSkillViews.Location = new System.Drawing.Point(91, 1);
			this.toolStripExSkillViews.Name = "toolStripExSkillViews";
			this.toolStripExSkillViews.Office12Mode = false;
			this.toolStripExSkillViews.ShowItemToolTips = true;
			this.toolStripExSkillViews.ShowLauncher = false;
			this.toolStripExSkillViews.Size = new System.Drawing.Size(166, 0);
			this.toolStripExSkillViews.TabIndex = 6;
			this.toolStripExSkillViews.Text = "xxSkillViews";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripExSkillViews, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripExSkillViews, false);
			// 
			// toolStripPanelItem2
			// 
			this.toolStripPanelItem2.CausesValidation = false;
			this.toolStripPanelItem2.ForeColor = System.Drawing.Color.MidnightBlue;
			this.toolStripPanelItem2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonChartPeriodView,
            this.toolStripButtonChartMonthView,
            this.toolStripButtonChartWeekView,
            this.toolStripButtonChartDayView,
            this.toolStripButtonChartIntradayView});
			this.toolStripPanelItem2.Name = "toolStripPanelItem2";
			this.SetShortcut(this.toolStripPanelItem2, System.Windows.Forms.Keys.None);
			this.toolStripPanelItem2.Size = new System.Drawing.Size(157, 0);
			this.toolStripPanelItem2.Text = "toolStripPanelItem2";
			this.toolStripPanelItem2.Transparent = true;
			// 
			// toolStripButtonChartPeriodView
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonChartPeriodView, "");
			this.toolStripButtonChartPeriodView.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_schedule_Summary_view_16x16;
			this.toolStripButtonChartPeriodView.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonChartPeriodView.Name = "toolStripButtonChartPeriodView";
			this.SetShortcut(this.toolStripButtonChartPeriodView, System.Windows.Forms.Keys.None);
			this.toolStripButtonChartPeriodView.Size = new System.Drawing.Size(71, 20);
			this.toolStripButtonChartPeriodView.Text = "xxPeriod";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonChartPeriodView, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonChartPeriodView, false);
			this.toolStripButtonChartPeriodView.Click += new System.EventHandler(this.toolStripButtonChartPeriodViewClick);
			// 
			// toolStripButtonChartMonthView
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonChartMonthView, "");
			this.toolStripButtonChartMonthView.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_schedule_Period_view_16x16;
			this.toolStripButtonChartMonthView.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonChartMonthView.Name = "toolStripButtonChartMonthView";
			this.SetShortcut(this.toolStripButtonChartMonthView, System.Windows.Forms.Keys.None);
			this.toolStripButtonChartMonthView.Size = new System.Drawing.Size(73, 20);
			this.toolStripButtonChartMonthView.Text = "xxMonth";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonChartMonthView, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonChartMonthView, false);
			this.toolStripButtonChartMonthView.Click += new System.EventHandler(this.toolStripButtonChartPeriodViewClick);
			// 
			// toolStripButtonChartWeekView
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonChartWeekView, "");
			this.toolStripButtonChartWeekView.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_schedule_DetailView_16x16;
			this.toolStripButtonChartWeekView.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonChartWeekView.Name = "toolStripButtonChartWeekView";
			this.SetShortcut(this.toolStripButtonChartWeekView, System.Windows.Forms.Keys.None);
			this.toolStripButtonChartWeekView.Size = new System.Drawing.Size(66, 20);
			this.toolStripButtonChartWeekView.Text = "xxWeek";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonChartWeekView, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonChartWeekView, false);
			this.toolStripButtonChartWeekView.Click += new System.EventHandler(this.toolStripButtonChartPeriodViewClick);
			// 
			// toolStripButtonChartDayView
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonChartDayView, "");
			this.toolStripButtonChartDayView.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_schedule_Weekview_16x16;
			this.toolStripButtonChartDayView.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonChartDayView.Name = "toolStripButtonChartDayView";
			this.SetShortcut(this.toolStripButtonChartDayView, System.Windows.Forms.Keys.None);
			this.toolStripButtonChartDayView.Size = new System.Drawing.Size(57, 20);
			this.toolStripButtonChartDayView.Text = "xxDay";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonChartDayView, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonChartDayView, false);
			this.toolStripButtonChartDayView.Click += new System.EventHandler(this.toolStripButtonChartPeriodViewClick);
			// 
			// toolStripButtonChartIntradayView
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonChartIntradayView, "");
			this.toolStripButtonChartIntradayView.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_schedule_IntradayView_16x16;
			this.toolStripButtonChartIntradayView.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonChartIntradayView.Name = "toolStripButtonChartIntradayView";
			this.SetShortcut(this.toolStripButtonChartIntradayView, System.Windows.Forms.Keys.None);
			this.toolStripButtonChartIntradayView.Size = new System.Drawing.Size(80, 20);
			this.toolStripButtonChartIntradayView.Text = "xxIntraday";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonChartIntradayView, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonChartIntradayView, false);
			this.toolStripButtonChartIntradayView.Click += new System.EventHandler(this.toolStripButtonChartPeriodViewClick);
			// 
			// toolStripTabItem1
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripTabItem1, "");
			this.toolStripTabItem1.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.toolStripTabItem1.Name = "toolStripTabItem1";
			// 
			// ribbonControlAdv1.ribbonPanel3
			// 
			this.toolStripTabItem1.Panel.Controls.Add(this.toolStripExRequestNavigate);
			this.toolStripTabItem1.Panel.Controls.Add(this.toolStripEx2);
			this.toolStripTabItem1.Panel.Controls.Add(this.toolStripExHandleRequests);
			this.toolStripTabItem1.Panel.Controls.Add(this.toolStripEx3);
			this.toolStripTabItem1.Panel.Controls.Add(this.toolStripEx4);
			this.toolStripTabItem1.Panel.Name = "ribbonPanel3";
			this.toolStripTabItem1.Panel.ScrollPosition = 0;
			this.toolStripTabItem1.Panel.TabIndex = 7;
			this.toolStripTabItem1.Panel.Text = "XXRequestsMenu";
			this.toolStripTabItem1.Position = 2;
			this.SetShortcut(this.toolStripTabItem1, System.Windows.Forms.Keys.None);
			this.toolStripTabItem1.Size = new System.Drawing.Size(119, 25);
			this.toolStripTabItem1.Text = "XXRequestsMenu";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripTabItem1, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripTabItem1, false);
			this.toolStripTabItem1.Visible = false;
			this.toolStripTabItem1.Click += new System.EventHandler(this.toolStripTabItem1Click);
			// 
			// toolStripExRequestNavigate
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripExRequestNavigate, "");
			this.toolStripExRequestNavigate.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripExRequestNavigate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(59)))), ((int)(((byte)(59)))));
			this.toolStripExRequestNavigate.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripExRequestNavigate.Image = null;
			this.toolStripExRequestNavigate.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonRequestBack});
			this.toolStripExRequestNavigate.Location = new System.Drawing.Point(0, 1);
			this.toolStripExRequestNavigate.Name = "toolStripExRequestNavigate";
			this.toolStripExRequestNavigate.Office12Mode = false;
			this.toolStripExRequestNavigate.Padding = new System.Windows.Forms.Padding(15, 0, 15, 0);
			this.toolStripExRequestNavigate.ShowLauncher = false;
			this.toolStripExRequestNavigate.Size = new System.Drawing.Size(82, 0);
			this.toolStripExRequestNavigate.TabIndex = 5;
			this.toolStripExRequestNavigate.Text = "xxNavigate";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripExRequestNavigate, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripExRequestNavigate, false);
			// 
			// toolStripButtonRequestBack
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonRequestBack, "");
			this.toolStripButtonRequestBack.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_Left;
			this.toolStripButtonRequestBack.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonRequestBack.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonRequestBack.Name = "toolStripButtonRequestBack";
			this.SetShortcut(this.toolStripButtonRequestBack, System.Windows.Forms.Keys.None);
			this.toolStripButtonRequestBack.Size = new System.Drawing.Size(46, 0);
			this.toolStripButtonRequestBack.Text = "xxBack";
			this.toolStripButtonRequestBack.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			this.toolStripButtonRequestBack.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonRequestBack, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonRequestBack, false);
			this.toolStripButtonRequestBack.Click += new System.EventHandler(this.toolStripButtonRequestBackClick);
			// 
			// toolStripEx2
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripEx2, "");
			this.toolStripEx2.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripEx2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(59)))), ((int)(((byte)(59)))));
			this.toolStripEx2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripEx2.Image = null;
			this.toolStripEx2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonViewDetails,
            this.toolStripButtonViewAllowance,
            this.toolStripButtonViewRequestHistory});
			this.toolStripEx2.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
			this.toolStripEx2.Location = new System.Drawing.Point(84, 1);
			this.toolStripEx2.Name = "toolStripEx2";
			this.toolStripEx2.Office12Mode = false;
			this.toolStripEx2.ShowLauncher = false;
			this.toolStripEx2.Size = new System.Drawing.Size(273, 0);
			this.toolStripEx2.TabIndex = 4;
			this.toolStripEx2.Text = "xxDetails";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripEx2, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripEx2, false);
			// 
			// toolStripButtonViewDetails
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonViewDetails, "");
			this.toolStripButtonViewDetails.Enabled = false;
			this.toolStripButtonViewDetails.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_schedule_DetailView_32x321;
			this.toolStripButtonViewDetails.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonViewDetails.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonViewDetails.Name = "toolStripButtonViewDetails";
			this.SetShortcut(this.toolStripButtonViewDetails, System.Windows.Forms.Keys.None);
			this.toolStripButtonViewDetails.Size = new System.Drawing.Size(81, 0);
			this.toolStripButtonViewDetails.Text = "xxViewDetails";
			this.toolStripButtonViewDetails.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonViewDetails, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonViewDetails, false);
			this.toolStripButtonViewDetails.Click += new System.EventHandler(this.toolStripMenuItemViewDetailsClick);
			// 
			// toolStripButtonViewAllowance
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonViewAllowance, "");
			this.toolStripButtonViewAllowance.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_Advanced_settings2;
			this.toolStripButtonViewAllowance.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonViewAllowance.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonViewAllowance.Name = "toolStripButtonViewAllowance";
			this.SetShortcut(this.toolStripButtonViewAllowance, System.Windows.Forms.Keys.None);
			this.toolStripButtonViewAllowance.Size = new System.Drawing.Size(101, 0);
			this.toolStripButtonViewAllowance.Text = "xxViewAllowance";
			this.toolStripButtonViewAllowance.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonViewAllowance, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonViewAllowance, false);
			this.toolStripButtonViewAllowance.Click += new System.EventHandler(this.toolStripItemViewAllowanceClick);
			// 
			// toolStripButtonViewRequestHistory
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonViewRequestHistory, "");
			this.toolStripButtonViewRequestHistory.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_Agent_Request_OK_32x32;
			this.toolStripButtonViewRequestHistory.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonViewRequestHistory.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonViewRequestHistory.Name = "toolStripButtonViewRequestHistory";
			this.SetShortcut(this.toolStripButtonViewRequestHistory, System.Windows.Forms.Keys.None);
			this.toolStripButtonViewRequestHistory.Size = new System.Drawing.Size(84, 0);
			this.toolStripButtonViewRequestHistory.Text = "xxViewHistory";
			this.toolStripButtonViewRequestHistory.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonViewRequestHistory, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonViewRequestHistory, false);
			this.toolStripButtonViewRequestHistory.Click += new System.EventHandler(this.toolStripViewRequestHistoryClick);
			// 
			// toolStripExHandleRequests
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripExHandleRequests, "");
			this.toolStripExHandleRequests.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripExHandleRequests.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(59)))), ((int)(((byte)(59)))));
			this.toolStripExHandleRequests.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripExHandleRequests.Image = null;
			this.toolStripExHandleRequests.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonApproveRequest,
            this.toolStripButtonDenyRequest,
            this.toolStripButtonEditNote,
            this.toolStripButtonReplyAndApprove,
            this.toolStripButtonReplyAndDeny});
			this.toolStripExHandleRequests.Location = new System.Drawing.Point(359, 1);
			this.toolStripExHandleRequests.Name = "toolStripExHandleRequests";
			this.toolStripExHandleRequests.Office12Mode = false;
			this.toolStripExHandleRequests.ShowLauncher = false;
			this.toolStripExHandleRequests.Size = new System.Drawing.Size(387, 0);
			this.toolStripExHandleRequests.TabIndex = 1;
			this.toolStripExHandleRequests.Text = "xxHandleRequests";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripExHandleRequests, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripExHandleRequests, false);
			// 
			// toolStripButtonApproveRequest
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonApproveRequest, "");
			this.toolStripButtonApproveRequest.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_Agent_NewRequest_32x32;
			this.toolStripButtonApproveRequest.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonApproveRequest.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonApproveRequest.Name = "toolStripButtonApproveRequest";
			this.SetShortcut(this.toolStripButtonApproveRequest, System.Windows.Forms.Keys.None);
			this.toolStripButtonApproveRequest.Size = new System.Drawing.Size(66, 0);
			this.toolStripButtonApproveRequest.Text = "xxApprove";
			this.toolStripButtonApproveRequest.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonApproveRequest, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonApproveRequest, false);
			this.toolStripButtonApproveRequest.Click += new System.EventHandler(this.toolStripButtonApproveRequestClick);
			// 
			// toolStripButtonDenyRequest
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonDenyRequest, "");
			this.toolStripButtonDenyRequest.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_Agent_denyRequest_32x32;
			this.toolStripButtonDenyRequest.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonDenyRequest.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonDenyRequest.Name = "toolStripButtonDenyRequest";
			this.SetShortcut(this.toolStripButtonDenyRequest, System.Windows.Forms.Keys.None);
			this.toolStripButtonDenyRequest.Size = new System.Drawing.Size(48, 0);
			this.toolStripButtonDenyRequest.Text = "xxDeny";
			this.toolStripButtonDenyRequest.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonDenyRequest, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonDenyRequest, false);
			this.toolStripButtonDenyRequest.Click += new System.EventHandler(this.toolStripButtonDenyRequestClick);
			// 
			// toolStripButtonEditNote
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonEditNote, "");
			this.toolStripButtonEditNote.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_Agent_ContinueDialogue_32x32;
			this.toolStripButtonEditNote.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonEditNote.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonEditNote.Name = "toolStripButtonEditNote";
			this.SetShortcut(this.toolStripButtonEditNote, System.Windows.Forms.Keys.None);
			this.toolStripButtonEditNote.Size = new System.Drawing.Size(50, 0);
			this.toolStripButtonEditNote.Text = "xxReply";
			this.toolStripButtonEditNote.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonEditNote, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonEditNote, false);
			this.toolStripButtonEditNote.Click += new System.EventHandler(this.toolStripButtonEditNoteClick);
			// 
			// toolStripButtonReplyAndApprove
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonReplyAndApprove, "");
			this.toolStripButtonReplyAndApprove.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_Agent_NewRequest_32x32;
			this.toolStripButtonReplyAndApprove.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonReplyAndApprove.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonReplyAndApprove.Name = "toolStripButtonReplyAndApprove";
			this.SetShortcut(this.toolStripButtonReplyAndApprove, System.Windows.Forms.Keys.None);
			this.toolStripButtonReplyAndApprove.Size = new System.Drawing.Size(117, 0);
			this.toolStripButtonReplyAndApprove.Text = "xxReplyAndApprove";
			this.toolStripButtonReplyAndApprove.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonReplyAndApprove, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonReplyAndApprove, false);
			this.toolStripButtonReplyAndApprove.Click += new System.EventHandler(this.toolStripButtonReplyAndApproveClick);
			// 
			// toolStripButtonReplyAndDeny
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonReplyAndDeny, "");
			this.toolStripButtonReplyAndDeny.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_Agent_denyRequest_32x32;
			this.toolStripButtonReplyAndDeny.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonReplyAndDeny.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonReplyAndDeny.Name = "toolStripButtonReplyAndDeny";
			this.SetShortcut(this.toolStripButtonReplyAndDeny, System.Windows.Forms.Keys.None);
			this.toolStripButtonReplyAndDeny.Size = new System.Drawing.Size(99, 0);
			this.toolStripButtonReplyAndDeny.Text = "xxReplyAndDeny";
			this.toolStripButtonReplyAndDeny.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonReplyAndDeny, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonReplyAndDeny, false);
			this.toolStripButtonReplyAndDeny.Click += new System.EventHandler(this.toolStripButtonReplyAndDenyClick);
			// 
			// toolStripEx3
			// 
			this.toolStripEx3.AutoSize = false;
			this.ribbonControlAdv1.SetDescription(this.toolStripEx3, "");
			this.toolStripEx3.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripEx3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(59)))), ((int)(((byte)(59)))));
			this.toolStripEx3.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripEx3.Image = null;
			this.toolStripEx3.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripTextBoxFilter,
            this.toolStripButtonFindRequest});
			this.toolStripEx3.Location = new System.Drawing.Point(748, 1);
			this.toolStripEx3.Name = "toolStripEx3";
			this.toolStripEx3.Office12Mode = false;
			this.toolStripEx3.Padding = new System.Windows.Forms.Padding(15, 0, 15, 0);
			this.toolStripEx3.ShowLauncher = false;
			this.toolStripEx3.Size = new System.Drawing.Size(410, 0);
			this.toolStripEx3.TabIndex = 2;
			this.toolStripEx3.Text = "xxFind";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripEx3, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripEx3, false);
			// 
			// toolStripTextBoxFilter
			// 
			this.toolStripTextBoxFilter.AutoSize = false;
			this.toolStripTextBoxFilter.BackColor = System.Drawing.Color.Gainsboro;
			this.ribbonControlAdv1.SetDescription(this.toolStripTextBoxFilter, "");
			this.toolStripTextBoxFilter.Margin = new System.Windows.Forms.Padding(1, 0, 0, 0);
			this.toolStripTextBoxFilter.Name = "toolStripTextBoxFilter";
			this.toolStripTextBoxFilter.Padding = new System.Windows.Forms.Padding(1, 0, 0, 0);
			this.SetShortcut(this.toolStripTextBoxFilter, System.Windows.Forms.Keys.None);
			this.toolStripTextBoxFilter.Size = new System.Drawing.Size(320, 81);
			this.toolStripTextBoxFilter.ToolTipText = "xxFilter";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripTextBoxFilter, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripTextBoxFilter, false);
			// 
			// toolStripButtonFindRequest
			// 
			this.toolStripButtonFindRequest.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonFindRequest, "");
			this.toolStripButtonFindRequest.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonFindRequest.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_Filter;
			this.toolStripButtonFindRequest.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonFindRequest.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonFindRequest.Name = "toolStripButtonFindRequest";
			this.SetShortcut(this.toolStripButtonFindRequest, System.Windows.Forms.Keys.None);
			this.toolStripButtonFindRequest.Size = new System.Drawing.Size(36, 0);
			this.toolStripButtonFindRequest.Text = "toolStripButtonFilterRequest";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonFindRequest, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonFindRequest, false);
			// 
			// toolStripEx4
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripEx4, "");
			this.toolStripEx4.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripEx4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(59)))), ((int)(((byte)(59)))));
			this.toolStripEx4.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripEx4.Image = null;
			this.toolStripEx4.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripPanelItem4});
			this.toolStripEx4.Location = new System.Drawing.Point(1160, 1);
			this.toolStripEx4.Name = "toolStripEx4";
			this.toolStripEx4.Office12Mode = false;
			this.toolStripEx4.ShowLauncher = false;
			this.toolStripEx4.Size = new System.Drawing.Size(83, 0);
			this.toolStripEx4.TabIndex = 17;
			this.toolStripEx4.Text = "xxFilter";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripEx4, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripEx4, false);
			// 
			// toolStripPanelItem4
			// 
			this.toolStripPanelItem4.CausesValidation = false;
			this.toolStripPanelItem4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.toolStripPanelItem4.ForeColor = System.Drawing.Color.MidnightBlue;
			this.toolStripPanelItem4.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonFilterAgentsRequestView});
			this.toolStripPanelItem4.Name = "toolStripPanelItem4";
			this.SetShortcut(this.toolStripPanelItem4, System.Windows.Forms.Keys.None);
			this.toolStripPanelItem4.Size = new System.Drawing.Size(74, 0);
			this.toolStripPanelItem4.Text = "toolStripPanelItem1";
			this.toolStripPanelItem4.Transparent = true;
			// 
			// toolStripButtonFilterAgentsRequestView
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonFilterAgentsRequestView, "");
			this.toolStripButtonFilterAgentsRequestView.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_Filter;
			this.toolStripButtonFilterAgentsRequestView.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonFilterAgentsRequestView.Name = "toolStripButtonFilterAgentsRequestView";
			this.SetShortcut(this.toolStripButtonFilterAgentsRequestView, System.Windows.Forms.Keys.None);
			this.toolStripButtonFilterAgentsRequestView.Size = new System.Drawing.Size(70, 20);
			this.toolStripButtonFilterAgentsRequestView.Text = "xxAgents";
			this.toolStripButtonFilterAgentsRequestView.ToolTipText = "xxFilterAgents";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonFilterAgentsRequestView, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonFilterAgentsRequestView, false);
			this.toolStripButtonFilterAgentsRequestView.Click += new System.EventHandler(this.toolStripButtonFilterAgentsClick);
			// 
			// toolStripTabItemQuickAccess
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripTabItemQuickAccess, "");
			this.toolStripTabItemQuickAccess.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.toolStripTabItemQuickAccess.Name = "toolStripTabItemQuickAccess";
			// 
			// ribbonControlAdv1.ribbonPanel4
			// 
			this.toolStripTabItemQuickAccess.Panel.Controls.Add(this.toolStripExForQuickAccessItems);
			this.toolStripTabItemQuickAccess.Panel.Name = "ribbonPanel4";
			this.toolStripTabItemQuickAccess.Panel.ScrollPosition = 0;
			this.toolStripTabItemQuickAccess.Panel.TabIndex = 8;
			this.toolStripTabItemQuickAccess.Panel.Text = "QuickAccessItems";
			this.toolStripTabItemQuickAccess.Position = 3;
			this.SetShortcut(this.toolStripTabItemQuickAccess, System.Windows.Forms.Keys.None);
			this.toolStripTabItemQuickAccess.Size = new System.Drawing.Size(123, 25);
			this.toolStripTabItemQuickAccess.Text = "QuickAccessItems";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripTabItemQuickAccess, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripTabItemQuickAccess, false);
			this.toolStripTabItemQuickAccess.Visible = false;
			// 
			// toolStripExForQuickAccessItems
			// 
			this.toolStripExForQuickAccessItems.AutoSize = false;
			this.ribbonControlAdv1.SetDescription(this.toolStripExForQuickAccessItems, "");
			this.toolStripExForQuickAccessItems.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripExForQuickAccessItems.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(59)))), ((int)(((byte)(59)))));
			this.toolStripExForQuickAccessItems.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripExForQuickAccessItems.Image = null;
			this.toolStripExForQuickAccessItems.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonQuickAccessCancel,
            this.toolStripButtonQuickAccessRedo,
            this.toolStripSplitButtonQuickAccessUndo,
            this.toolStripButtonShowTexts});
			this.toolStripExForQuickAccessItems.Location = new System.Drawing.Point(0, 1);
			this.toolStripExForQuickAccessItems.Name = "toolStripExForQuickAccessItems";
			this.toolStripExForQuickAccessItems.Office12Mode = false;
			this.toolStripExForQuickAccessItems.Size = new System.Drawing.Size(265, 87);
			this.toolStripExForQuickAccessItems.TabIndex = 2;
			this.toolStripExForQuickAccessItems.Text = "xxQuickAccess";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripExForQuickAccessItems, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripExForQuickAccessItems, false);
			this.toolStripExForQuickAccessItems.Visible = false;
			// 
			// toolStripButtonQuickAccessCancel
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonQuickAccessCancel, "");
			this.toolStripButtonQuickAccessCancel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonQuickAccessCancel.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_Cancel_16x16;
			this.toolStripButtonQuickAccessCancel.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonQuickAccessCancel.Name = "toolStripButtonQuickAccessCancel";
			this.SetShortcut(this.toolStripButtonQuickAccessCancel, System.Windows.Forms.Keys.None);
			this.toolStripButtonQuickAccessCancel.Size = new System.Drawing.Size(23, 84);
			this.toolStripButtonQuickAccessCancel.Text = "xxStop";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonQuickAccessCancel, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonQuickAccessCancel, false);
			this.toolStripButtonQuickAccessCancel.Click += new System.EventHandler(this.toolStripButtonQuickAccessCancelClick);
			// 
			// toolStripButtonQuickAccessRedo
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonQuickAccessRedo, "");
			this.toolStripButtonQuickAccessRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonQuickAccessRedo.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_Redo_small;
			this.toolStripButtonQuickAccessRedo.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonQuickAccessRedo.Name = "toolStripButtonQuickAccessRedo";
			this.SetShortcut(this.toolStripButtonQuickAccessRedo, System.Windows.Forms.Keys.None);
			this.toolStripButtonQuickAccessRedo.Size = new System.Drawing.Size(23, 84);
			this.toolStripButtonQuickAccessRedo.Text = "xxRedo";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonQuickAccessRedo, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonQuickAccessRedo, false);
			this.toolStripButtonQuickAccessRedo.MouseUp += new System.Windows.Forms.MouseEventHandler(this.toolStripButtonQuickAccessRedoClick1);
			// 
			// toolStripSplitButtonQuickAccessUndo
			// 
			this.toolStripSplitButtonQuickAccessUndo.DefaultItem = this.toolStripMenuItemQuickAccessUndo;
			this.ribbonControlAdv1.SetDescription(this.toolStripSplitButtonQuickAccessUndo, "");
			this.toolStripSplitButtonQuickAccessUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripSplitButtonQuickAccessUndo.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemQuickAccessUndo,
            this.toolStripMenuItemQuickAccessUndoAll});
			this.toolStripSplitButtonQuickAccessUndo.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_Undo_small;
			this.toolStripSplitButtonQuickAccessUndo.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripSplitButtonQuickAccessUndo.Name = "toolStripSplitButtonQuickAccessUndo";
			this.SetShortcut(this.toolStripSplitButtonQuickAccessUndo, System.Windows.Forms.Keys.None);
			this.toolStripSplitButtonQuickAccessUndo.Size = new System.Drawing.Size(32, 84);
			this.toolStripSplitButtonQuickAccessUndo.Text = "xxUndo";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripSplitButtonQuickAccessUndo, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripSplitButtonQuickAccessUndo, false);
			// 
			// toolStripMenuItemQuickAccessUndo
			// 
			this.toolStripMenuItemQuickAccessUndo.Name = "toolStripMenuItemQuickAccessUndo";
			this.SetShortcut(this.toolStripMenuItemQuickAccessUndo, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemQuickAccessUndo.Size = new System.Drawing.Size(127, 22);
			this.toolStripMenuItemQuickAccessUndo.Text = "xxUndo";
			this.toolStripMenuItemQuickAccessUndo.Click += new System.EventHandler(this.toolStripSplitButtonQuickAccessUndoButtonClick);
			// 
			// toolStripMenuItemQuickAccessUndoAll
			// 
			this.toolStripMenuItemQuickAccessUndoAll.Name = "toolStripMenuItemQuickAccessUndoAll";
			this.SetShortcut(this.toolStripMenuItemQuickAccessUndoAll, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemQuickAccessUndoAll.Size = new System.Drawing.Size(127, 22);
			this.toolStripMenuItemQuickAccessUndoAll.Text = "xxUndoAll";
			this.toolStripMenuItemQuickAccessUndoAll.Click += new System.EventHandler(this.toolStripMenuItemQuickAccessUndoAllClick1);
			// 
			// toolStripButtonShowTexts
			// 
			this.toolStripButtonShowTexts.Checked = true;
			this.toolStripButtonShowTexts.CheckState = System.Windows.Forms.CheckState.Checked;
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonShowTexts, "");
			this.toolStripButtonShowTexts.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.text_list_bullets;
			this.toolStripButtonShowTexts.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripButtonShowTexts.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonShowTexts.Name = "toolStripButtonShowTexts";
			this.SetShortcut(this.toolStripButtonShowTexts, System.Windows.Forms.Keys.None);
			this.toolStripButtonShowTexts.Size = new System.Drawing.Size(99, 84);
			this.toolStripButtonShowTexts.Text = "xxShowLabels";
			this.toolStripButtonShowTexts.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonShowTexts, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonShowTexts, false);
			this.toolStripButtonShowTexts.Click += new System.EventHandler(this.toolStripButtonShowTextsClick);
			// 
			// btnFilter
			// 
			this.btnFilter.AutoSize = false;
			this.btnFilter.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_Filter;
			this.btnFilter.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.btnFilter.Name = "btnFilter";
			this.SetShortcut(this.btnFilter, System.Windows.Forms.Keys.None);
			this.btnFilter.Size = new System.Drawing.Size(70, 17);
			this.btnFilter.Tag = "filter";
			this.btnFilter.Text = "xxFilter";
			// 
			// btnRightLeft
			// 
			this.btnRightLeft.AutoSize = false;
			this.btnRightLeft.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.RightToLeft;
			this.btnRightLeft.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.btnRightLeft.Name = "btnRightLeft";
			this.SetShortcut(this.btnRightLeft, System.Windows.Forms.Keys.None);
			this.btnRightLeft.Size = new System.Drawing.Size(70, 17);
			this.btnRightLeft.Tag = "rightleft";
			this.btnRightLeft.Text = "xxRiLe";
			// 
			// contextMenuStripRestrictionView
			// 
			this.contextMenuStripRestrictionView.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemAddPreferenceRestriction,
            this.toolStripMenuItemAddStudentAvailabilityRestriction,
            this.toolStripSeparator5,
            this.toolStripMenuItemRestrictionCopy,
            this.toolStripMenuItemRestrictionPaste,
            this.toolStripMenuItemRestrictionDelete});
			this.contextMenuStripRestrictionView.Name = "contextMenuStripRestrictionView";
			this.contextMenuStripRestrictionView.Size = new System.Drawing.Size(299, 120);
			// 
			// toolStripMenuItemAddPreferenceRestriction
			// 
			this.toolStripMenuItemAddPreferenceRestriction.Name = "toolStripMenuItemAddPreferenceRestriction";
			this.SetShortcut(this.toolStripMenuItemAddPreferenceRestriction, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemAddPreferenceRestriction.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.R)));
			this.toolStripMenuItemAddPreferenceRestriction.Size = new System.Drawing.Size(298, 22);
			this.toolStripMenuItemAddPreferenceRestriction.Text = "xxAddPreferenceThreeDots";
			this.toolStripMenuItemAddPreferenceRestriction.Click += new System.EventHandler(this.addPreferenceToolStripMenuItemClick);
			// 
			// toolStripMenuItemAddStudentAvailabilityRestriction
			// 
			this.toolStripMenuItemAddStudentAvailabilityRestriction.Name = "toolStripMenuItemAddStudentAvailabilityRestriction";
			this.SetShortcut(this.toolStripMenuItemAddStudentAvailabilityRestriction, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemAddStudentAvailabilityRestriction.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.H)));
			this.toolStripMenuItemAddStudentAvailabilityRestriction.Size = new System.Drawing.Size(298, 22);
			this.toolStripMenuItemAddStudentAvailabilityRestriction.Text = "xxAddStudentAvailabilityThreeDots";
			this.toolStripMenuItemAddStudentAvailabilityRestriction.Click += new System.EventHandler(this.addStudentAvailabilityToolStripMenuItemClick);
			// 
			// toolStripSeparator5
			// 
			this.toolStripSeparator5.Name = "toolStripSeparator5";
			this.SetShortcut(this.toolStripSeparator5, System.Windows.Forms.Keys.None);
			this.toolStripSeparator5.Size = new System.Drawing.Size(295, 6);
			// 
			// toolStripMenuItemRestrictionCopy
			// 
			this.toolStripMenuItemRestrictionCopy.Name = "toolStripMenuItemRestrictionCopy";
			this.SetShortcut(this.toolStripMenuItemRestrictionCopy, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemRestrictionCopy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
			this.toolStripMenuItemRestrictionCopy.Size = new System.Drawing.Size(298, 22);
			this.toolStripMenuItemRestrictionCopy.Text = "xxCopy";
			this.toolStripMenuItemRestrictionCopy.Click += new System.EventHandler(this.toolStripMenuItemRestrictionCopyClick);
			// 
			// toolStripMenuItemRestrictionPaste
			// 
			this.toolStripMenuItemRestrictionPaste.Name = "toolStripMenuItemRestrictionPaste";
			this.SetShortcut(this.toolStripMenuItemRestrictionPaste, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemRestrictionPaste.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
			this.toolStripMenuItemRestrictionPaste.Size = new System.Drawing.Size(298, 22);
			this.toolStripMenuItemRestrictionPaste.Text = "xxPaste";
			this.toolStripMenuItemRestrictionPaste.Click += new System.EventHandler(this.toolStripMenuItemRestrictionPasteClick);
			// 
			// toolStripMenuItemRestrictionDelete
			// 
			this.toolStripMenuItemRestrictionDelete.Name = "toolStripMenuItemRestrictionDelete";
			this.SetShortcut(this.toolStripMenuItemRestrictionDelete, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemRestrictionDelete.ShortcutKeys = System.Windows.Forms.Keys.Delete;
			this.toolStripMenuItemRestrictionDelete.Size = new System.Drawing.Size(298, 22);
			this.toolStripMenuItemRestrictionDelete.Text = "xxDelete";
			this.toolStripMenuItemRestrictionDelete.Click += new System.EventHandler(this.toolStripMenuItemRestrictionDeleteClick);
			// 
			// schedulerSplitters1
			// 
			this.schedulerSplitters1.BackColor = System.Drawing.Color.White;
			this.schedulerSplitters1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.schedulerSplitters1.Location = new System.Drawing.Point(3, 146);
			this.schedulerSplitters1.Name = "schedulerSplitters1";
			this.schedulerSplitters1.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
			this.schedulerSplitters1.Size = new System.Drawing.Size(1229, 523);
			this.schedulerSplitters1.TabIndex = 6;
			// 
			// contextMenuStripRequests
			// 
			this.contextMenuStripRequests.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItemViewDetails,
            this.toolStripMenuItemFindMatching2,
            this.toolStripMenuItemViewAllowance,
            this.xxViewOldRequestsToolStripMenuItem});
			this.contextMenuStripRequests.Name = "contextMenuStripRequests";
			this.contextMenuStripRequests.Size = new System.Drawing.Size(205, 92);
			// 
			// ToolStripMenuItemViewDetails
			// 
			this.ToolStripMenuItemViewDetails.Enabled = false;
			this.ToolStripMenuItemViewDetails.Name = "ToolStripMenuItemViewDetails";
			this.SetShortcut(this.ToolStripMenuItemViewDetails, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemViewDetails.Size = new System.Drawing.Size(204, 22);
			this.ToolStripMenuItemViewDetails.Text = "xxViewDetails";
			this.ToolStripMenuItemViewDetails.Click += new System.EventHandler(this.toolStripMenuItemViewDetailsClick);
			// 
			// toolStripMenuItemFindMatching2
			// 
			this.toolStripMenuItemFindMatching2.Name = "toolStripMenuItemFindMatching2";
			this.SetShortcut(this.toolStripMenuItemFindMatching2, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemFindMatching2.Size = new System.Drawing.Size(204, 22);
			this.toolStripMenuItemFindMatching2.Text = "xxFindMatchingTreeDots";
			this.toolStripMenuItemFindMatching2.Visible = false;
			this.toolStripMenuItemFindMatching2.Click += new System.EventHandler(this.toolStripMenuItemFindMatching2Click);
			// 
			// toolStripMenuItemViewAllowance
			// 
			this.toolStripMenuItemViewAllowance.Enabled = false;
			this.toolStripMenuItemViewAllowance.Name = "toolStripMenuItemViewAllowance";
			this.SetShortcut(this.toolStripMenuItemViewAllowance, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemViewAllowance.Size = new System.Drawing.Size(204, 22);
			this.toolStripMenuItemViewAllowance.Text = "xxViewAllowance";
			this.toolStripMenuItemViewAllowance.Click += new System.EventHandler(this.toolStripItemViewAllowanceClick);
			// 
			// xxViewOldRequestsToolStripMenuItem
			// 
			this.xxViewOldRequestsToolStripMenuItem.Name = "xxViewOldRequestsToolStripMenuItem";
			this.SetShortcut(this.xxViewOldRequestsToolStripMenuItem, System.Windows.Forms.Keys.None);
			this.xxViewOldRequestsToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
			this.xxViewOldRequestsToolStripMenuItem.Text = "xxViewHistory";
			this.xxViewOldRequestsToolStripMenuItem.Click += new System.EventHandler(this.toolStripViewRequestHistoryClick);
			// 
			// flowLayoutExportToScenario
			// 
			this.flowLayoutExportToScenario.Alignment = Syncfusion.Windows.Forms.Tools.FlowAlignment.Near;
			this.flowLayoutExportToScenario.ContainerControl = this.backStageTabExportTo;
			this.flowLayoutExportToScenario.HorzNearMargin = 5;
			this.flowLayoutExportToScenario.LayoutMode = Syncfusion.Windows.Forms.Tools.FlowLayoutMode.Vertical;
			// 
			// SchedulingScreen
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Borders = new System.Windows.Forms.Padding(0);
			this.ClientSize = new System.Drawing.Size(1233, 695);
			this.ColorScheme = Syncfusion.Windows.Forms.Tools.RibbonForm.ColorSchemeType.Silver;
			this.Controls.Add(this.backStage1);
			this.Controls.Add(this.schedulerSplitters1);
			this.Controls.Add(this.statusStrip1);
			this.Controls.Add(this.ribbonControlAdv1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "SchedulingScreen";
			this.Padding = new System.Windows.Forms.Padding(3, 0, 1, 0);
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "xxTeleoptiRaptorColonScheduler";
			this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.schedulingScreenFormClosing);
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.schedulingScreenFormClosed);
			this.Load += new System.EventHandler(this.schedulingScreenLoad);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.schedulingScreenKeyDown);
			this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.schedulingScreenKeyUp);
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.contextMenuViews.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).EndInit();
			this.ribbonControlAdv1.ResumeLayout(false);
			this.ribbonControlAdv1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.backStage1)).EndInit();
			this.backStage1.ResumeLayout(false);
			this.toolStripTabItemHome.Panel.ResumeLayout(false);
			this.toolStripTabItemHome.Panel.PerformLayout();
			this.toolStripExData.ResumeLayout(false);
			this.toolStripExData.PerformLayout();
			this.toolStripExScheduleViews.ResumeLayout(false);
			this.toolStripExScheduleViews.PerformLayout();
			this.toolStripExActions.ResumeLayout(false);
			this.toolStripExActions.PerformLayout();
			this.toolStripExLocks.ResumeLayout(false);
			this.toolStripExLocks.PerformLayout();
			this.toolStripExTags.ResumeLayout(false);
			this.toolStripExTags.PerformLayout();
			this.toolStripEx1.ResumeLayout(false);
			this.toolStripEx1.PerformLayout();
			this.toolStripExLoadOptions.ResumeLayout(false);
			this.toolStripExLoadOptions.PerformLayout();
			this.toolStripExFilter.ResumeLayout(false);
			this.toolStripExFilter.PerformLayout();
			this.toolStripTabItemChart.Panel.ResumeLayout(false);
			this.toolStripTabItemChart.Panel.PerformLayout();
			this.toolStripExGridRowInChartButtons.ResumeLayout(false);
			this.toolStripExGridRowInChartButtons.PerformLayout();
			this.toolStripExSkillViews.ResumeLayout(false);
			this.toolStripExSkillViews.PerformLayout();
			this.toolStripTabItem1.Panel.ResumeLayout(false);
			this.toolStripTabItem1.Panel.PerformLayout();
			this.toolStripExRequestNavigate.ResumeLayout(false);
			this.toolStripExRequestNavigate.PerformLayout();
			this.toolStripEx2.ResumeLayout(false);
			this.toolStripEx2.PerformLayout();
			this.toolStripExHandleRequests.ResumeLayout(false);
			this.toolStripExHandleRequests.PerformLayout();
			this.toolStripEx3.ResumeLayout(false);
			this.toolStripEx3.PerformLayout();
			this.toolStripEx4.ResumeLayout(false);
			this.toolStripEx4.PerformLayout();
			this.toolStripTabItemQuickAccess.Panel.ResumeLayout(false);
			this.toolStripExForQuickAccessItems.ResumeLayout(false);
			this.toolStripExForQuickAccessItems.PerformLayout();
			this.contextMenuStripRestrictionView.ResumeLayout(false);
			this.contextMenuStripRequests.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.flowLayoutExportToScenario)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

	    #endregion

		private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemInsertAbsence;
        private System.Windows.Forms.ToolStripButton btnFilter;
		private System.Windows.Forms.ToolStripButton btnRightLeft;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelStatus;
		private MetroToolStripProgressBar toolStripProgressBar1;
        private RibbonControlAdvFixed ribbonControlAdv1;
        private Syncfusion.Windows.Forms.Tools.ToolStripTabItem toolStripTabItemHome;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemCut;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemCopy;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemPaste;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDelete;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemPasteSpecial;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDeleteSpecial;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemCutSpecial;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSwitchToViewPointOfSelectedAgent;
				private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemLock;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemUnlock;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemLockSelectionRM;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemLockFreeDaysRM;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemLockAbsencesRM;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemLockShiftCategoriesRM;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemUnlockAllRM;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemUnlockSelectionRM;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExClipboard;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExScheduleViews;
        private System.Windows.Forms.ToolStripButton toolStripButtonDayView;
		private System.Windows.Forms.ToolStripButton toolStripButtonWeekView;
        private Syncfusion.Windows.Forms.Tools.ToolStripPanelItem toolStripPanelItemViews2;
        private System.Windows.Forms.ToolStripButton toolStripButtonPeriodView;
        private System.Windows.Forms.ToolStripButton toolStripButtonSummaryView;
		private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExSkillViews;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExActions;
        private System.Windows.Forms.ToolStripSplitButton toolStripSplitButtonSchedule;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemScheduleSelected;
        private Syncfusion.Windows.Forms.Tools.ToolStripPanelItem toolStripPanelItemAssignments;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExLocks;
        private Syncfusion.Windows.Forms.Tools.ToolStripPanelItem toolStripPanelItemLocks;
        private System.Windows.Forms.ToolStripSplitButton toolStripSplitButtonUnlock;
        private System.Windows.Forms.ToolStripSplitButton toolStripSplitButtonLock;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemUnlockAll;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemUnlockSelection;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemLockSelection;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemLockAbsence;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemLockDayOff;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemLockShiftCategory;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemInsertDayOff;
        private System.ComponentModel.BackgroundWorker backgroundWorkerLoadData;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExGridRowInChartButtons;
        private System.Windows.Forms.ToolStripButton toolStripButtonGridInChart;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExEdit2;
		private Syncfusion.Windows.Forms.Tools.ToolStripTabItem toolStripTabItemChart;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemMeetingOrganizer;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemEditMeeting;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDeleteMeeting;
        private IWpfShiftEditor wpfShiftEditor1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRemoveParticipant;
        private System.Windows.Forms.ToolStripButton toolStripButtonRequestView;
        private Syncfusion.Windows.Forms.Tools.ToolStripTabItem toolStripTabItem1;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExHandleRequests;
        private System.Windows.Forms.ToolStripButton toolStripButtonDenyRequest;
        private System.Windows.Forms.ToolStripButton toolStripButtonApproveRequest;
		private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripEx3;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemCreateMeeting;
		private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemAddActivity;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemNotifyAgent;
        private System.Windows.Forms.ToolStripButton toolStripButtonEditNote;
        private System.Windows.Forms.ToolStripButton toolStripButtonReplyAndApprove;
        private System.Windows.Forms.ToolStripButton toolStripButtonReplyAndDeny;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemOptimize;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemScheduleHourlyEmployees;
        private NotesEditor notesEditor;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemAddOverTime;
        private ToolStripSpinningProgressControl toolStripSpinningProgressControl1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemWriteProtectSchedule;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemWriteProtectSchedule2;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem6;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem5;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private Syncfusion.Windows.Forms.Tools.ToolStripTabItem toolStripTabItemQuickAccess;
		private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExForQuickAccessItems;
        private System.Windows.Forms.ToolStripButton toolStripButtonQuickAccessCancel;
        private System.Windows.Forms.ToolStripButton toolStripButtonQuickAccessRedo;
        private System.Windows.Forms.ToolStripSplitButton toolStripSplitButtonQuickAccessUndo;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemQuickAccessUndo;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemQuickAccessUndoAll;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripRestrictionView;
		private System.Windows.Forms.ToolStripButton toolStripButtonFindAgents;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripEx1;
        private Syncfusion.Windows.Forms.Tools.ToolStripPanelItem toolStripPanelItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSortBy;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemStartAsc;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemStartTimeDesc;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemEndTimeAsc;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemEndTimeDesc;
        private System.Windows.Forms.ToolStripButton toolStripButtonRestrictions;
        private System.Windows.Forms.ToolStripButton toolStripButtonShowGraph;
        private System.Windows.Forms.ToolStripButton toolStripButtonShowResult;
        private System.Windows.Forms.ToolStripButton toolStripButtonShowEditor;
		private SchedulerSplitters schedulerSplitters1;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExLoadOptions;
        private Syncfusion.Windows.Forms.Tools.ToolStripPanelItem toolStripPanelItemLoadOptions;
        private System.Windows.Forms.ToolStripButton toolStripButtonShrinkage;
        private System.Windows.Forms.ToolStripButton toolStripButtonCalculation;
        private System.Windows.Forms.ToolStripButton toolStripButtonValidation;
        private System.Windows.Forms.ToolStripButton toolStripButtonShowTexts;
		private Syncfusion.Windows.Forms.Tools.ContextMenuStripEx contextMenuViews;
		private System.Windows.Forms.ContextMenuStrip contextMenuStripRequests;
		private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemViewDetails;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripEx2;
		private System.Windows.Forms.ToolStripButton toolStripButtonViewDetails;
		private ToolStripMenuItem xxExportToolStripMenuItem;
		private ToolStripMenuItem toolStripMenuItemExportToPDF;
		private ToolStripMenuItem toolStripMenuItemExportToPDFGraphical;
		private ToolStripMenuItem toolStripMenuItemFindMatching;
		private ToolStripMenuItem toolStripMenuItemFindMatching2;
        private ToolStripMenuItem ToolStripMenuItemLockRestrictions;
        private ToolStripMenuItem ToolStripMenuItemLockAllRestrictions;
        private ToolStripMenuItem ToolStripMenuItemLockPreferences;
        private ToolStripMenuItem ToolStripMenuItemLockRotations;
        private ToolStripMenuItem ToolStripMenuItemLockStudentAvailability;
        private ToolStripMenuItem ToolStripMenuItemLockAvailability;
        private ToolStripMenuItem ToolStripMenuItemLockRestrictionsRM;
        private ToolStripMenuItem ToolStripMenuItemAllRM;
        private ToolStripMenuItem ToolStripMenuItemLockPreferencesRM;
        private ToolStripMenuItem ToolStripMenuItemLockRotationsRM;
        private ToolStripMenuItem ToolStripMenuItemLockStudentAvailabilityRM;
        private ToolStripMenuItem ToolStripMenuItemLockAvailabilityRM;
		private ToolStripMenuItem ToolStripMenuItemAddPersonalActivity;
		private ToolStripMenuItem toolStripMenuItemPasteShiftFromShifts;
        private ToolStripMenuItem toolStripMenuItemViewHistory;
        private ToolStripButton toolStripButtonViewAllowance;
        private ToolStripMenuItem toolStripMenuItemViewAllowance;
        private ToolStripMenuItem toolStripMenuItemLockTags;
        private ToolStripMenuItem toolStripMenuItemLockAllTags;
        private ToolStripMenuItem toolStripMenuItemLockTagsRM;
        private ToolStripMenuItem toolStripMenuItemLockAllTagsRM;
        private ToolStripStatusLabel toolStripStatusLabelScheduleTag;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExTags;
        private Syncfusion.Windows.Forms.Tools.ToolStripPanelItem toolStripPanelItemTags;
        private ToolStripComboBox toolStripComboBoxAutoTag;
        private ToolStripSplitButton toolStripSplitButtonChangeTag;
        private ToolStripLabel toolStripLabelAutoTag;
        private ToolStripMenuItem toolStripMenuItemChangeTagRM;
        private ToolStripMenuItem toolStripMenuItemContractTimeAsc;
        private ToolStripMenuItem toolstripMenuRemoveWriteProtection;
        private ToolStripMenuItem toolStripMenuItemContractTimeDesc;
        private ToolStripMenuItem ToolStripMenuItemRemoveWriteProtectionToolBar;
        private ToolStripMenuItem xxViewOldRequestsToolStripMenuItem;
        private ToolStripMenuItem ToolStripMenuItemExportToPDFShiftsPerDay;
		private ToolStripButton toolStripButtonViewRequestHistory;
		private Syncfusion.Windows.Forms.Tools.ToolStripPanelItem toolStripPanelItem2;
		private ToolStripButton toolStripButtonChartPeriodView;
		private ToolStripButton toolStripButtonChartMonthView;
		private ToolStripButton toolStripButtonChartWeekView;
		private ToolStripButton toolStripButtonChartDayView;
		private ToolStripButton toolStripButtonChartIntradayView;
		private ToolStripSeparator toolStripSeparator5;
		private ToolStripMenuItem toolStripMenuItemRestrictionCopy;
		private ToolStripMenuItem toolStripMenuItemRestrictionPaste;
		private ToolStripMenuItem toolStripMenuItemRestrictionDelete;
		private ToolStripTextBox toolStripTextBoxFilter;
		private ToolStripMenuItem toolStripMenuItemAddPreference;
		private ToolStripMenuItem toolStripMenuItemAddStudentAvailability;
		private ToolStripMenuItem toolStripMenuItemAddPreferenceRestriction;
		private ToolStripMenuItem toolStripMenuItemAddStudentAvailabilityRestriction;
		private ToolStripButton toolStripButtonFindRequest;
		private ToolStripMenuItem toolStripMenuItemAddOvertimeAvailability;
		private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExFilter;
		private Syncfusion.Windows.Forms.Tools.ToolStripPanelItem toolStripPanelItem3;
		private ToolStripButton toolStripButtonFilterAgents;
		private ToolStripButton toolStripButtonFilterOvertimeAvailability;
		private ToolStripButton toolStripButtonFilterStudentAvailability;
		private ToolStripSplitButton toolStripStatusLabelContractTime;
                private ToolStripMenuItem ToolStripMenuItemScheduleOvertime;
		private ToolStripMenuItem toolStripMenuItemContractTime;
		private ToolStripMenuItem toolStripMenuItemWorkTime;
		private ToolStripMenuItem toolStripMenuItemPaidTime;
		private ToolStripMenuItem toolStripMenuItemOverTime;
        private ToolStripButton toolStripButtonShowPropertyPanel;
		private ToolStripSplitButton toolStripDropDownButtonSwap;
		private ToolStripMenuItem toolStripMenuItemSwap;
		private ToolStripMenuItem toolStripMenuItemSwapAndReschedule;
		private ToolStripMenuItem ToolStripMenuItemSwapRaw;
		private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExRequestNavigate;
		private ToolStripButton toolStripButtonRequestBack;
		private ToolStripMenuItem toolStripMenuItemSeniorityRankAsc;
		private ToolStripMenuItem toolStripMenuItemSeniorityRankDesc;
		private Syncfusion.Windows.Forms.BackStage backStage1;
		private Syncfusion.Windows.Forms.BackStageButton backStageButtonMainMenuSave;
		private Syncfusion.Windows.Forms.BackStageButton backStageButtonMainMenuHelp;
		private Syncfusion.Windows.Forms.BackStageButton backStageButtonMainMenuClose;
		private Syncfusion.Windows.Forms.BackStageButton backStageButtonOptions;
		private Syncfusion.Windows.Forms.BackStageButton backStageButtonSystemExit;
		private Syncfusion.Windows.Forms.BackStageView backStageView;
		private Syncfusion.Windows.Forms.BackStageTab backStageTabExportTo;
		private Syncfusion.Windows.Forms.Tools.FlowLayout flowLayoutExportToScenario;
		private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripEx4;
		private ToolStripMenuItem toolStripMenuItemShiftBackToLegal;
		private Syncfusion.Windows.Forms.Tools.ToolStripPanelItem toolStripPanelItem4;
		private ToolStripButton toolStripButtonFilterAgentsRequestView;
		private ToolStripStatusLabel toolStripStatusLabelNumberOfAgents;
		private ToolStripMenuItem toolStripMenuItemPublish;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExData;
		private Syncfusion.Windows.Forms.Tools.ToolStripPanelItem toolStripPanelItemSave;
		private ToolStripButton toolStripButtonSaveLarge;
		private ToolStripButton toolStripButtonRefreshLarge;
		private ToolStripSplitButton toolStripSplitButtonTimeZone;
		private Syncfusion.Windows.Forms.BackStageButton backStageButtonManiMenuImport;
		private Syncfusion.Windows.Forms.BackStageButton backStageButtonMainMenuCopy;
		private ToolStripMenuItem replaceActivityToolStripMenuItem;
	}
}
