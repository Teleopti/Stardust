﻿using System;
using System.Windows.Forms;
using Teleopti.Ccc.WinCode.Scheduling.Editor;

namespace Teleopti.Ccc.Win.Scheduling
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
                if (_splitterManager != null)
                    _splitterManager.Dispose();
                if (schedulerSplitters1!=null)
                {
                    schedulerSplitters1.Dispose();
                    schedulerSplitters1 = null;
                }
                if (_gridChartManager != null)
                    _gridChartManager.Dispose();
                if(_singleAgentRestrictionPresenter != null)
                    _singleAgentRestrictionPresenter.Dispose();
                if (_skillDayGridControl!=null)
                    _skillDayGridControl.Dispose();
                if (_skillIntradayGridControl!=null)
                    _skillIntradayGridControl.Dispose();
                if (_contextMenuSkillGrid != null)
                    _contextMenuSkillGrid.Dispose();
                if (_schedulerMessageBrokerHandler!=null)
                    _schedulerMessageBrokerHandler.Dispose();
                if (_clipboardControl != null)
                    _clipboardControl.Dispose();
                if (_editControl != null)
                    _editControl.Dispose();

                _persister = null;
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
			this.toolStripSpinningProgressControl1 = new Teleopti.Ccc.Win.Common.Controls.SpinningProgress.ToolStripSpinningProgressControl();
			this.toolStripStatusLabelStatus = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
			this.toolStripStatusLabelScheduleTag = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripStatusLabelContractTime = new System.Windows.Forms.ToolStripStatusLabel();
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
			this.toolStripMenuItemAgentInfo = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemViewPointTimeZone = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemLoggedOnUserTimeZone = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemSortBy = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemStartAsc = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemStartTimeDesc = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemEndTimeAsc = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemEndTimeDesc = new System.Windows.Forms.ToolStripMenuItem();
			this.xxContractTimeAscToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.xxContractTimeDescToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemNextAssignment = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemShowAssignmentBefore = new System.Windows.Forms.ToolStripMenuItem();
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
			this.ToolStripMenuItemAllPreferencesRM = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAllAbsencePreferenceRM = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAllDaysOffPreferencesRM = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAllShiftsPreferencesRM = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAllMustHaveRM = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAllFulFilledPreferencesRM = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAllFulFilledAbsencesPreferencesRM = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAllFulFilledDaysOffPreferencesRM = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAllFulFilledShiftsPreferencesRM = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAllFulfilledMustHaveRM = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemLockRotationsRM = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAllRotationsRM = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAllDaysOffRotationsRM = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAllShiftsRotationsRM = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAllFulFilledRotationsRM = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAllFulFilledDaysOffRotationsRM = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAllFulFilledShiftsRotationsRM = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemLockStudentAvailabilityRM = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAllUnavailableStudentAvailabilityRM = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAllAvailableStudentAvailabilityRM = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAllFulFilledStudentAvailabilityRM = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemLockAvailabilityRM = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAllUnAvailableAvailabilityRM = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAllAvailableAvailabilityRM = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAllFulFilledAvailabilityRM = new System.Windows.Forms.ToolStripMenuItem();
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
			this.toolStripMenuItemViewReport = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemScheduledTimePerActivity = new System.Windows.Forms.ToolStripMenuItem();
			this.xxExportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemExportToPDF = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemExportToPDFGraphical = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemExportToPDFShiftsPerDay = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemChangeTagRM = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuStripResultView = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.ToolStripMenuItemDay = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemIntraday = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
			this.ToolStripMenuItemOccupancyAdjustment = new System.Windows.Forms.ToolStripMenuItem();
			this.backgroundWorkerLoadData = new System.ComponentModel.BackgroundWorker();
			this.ribbonControlAdv1 = new Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed();
			this.toolStripTabItemHome = new Syncfusion.Windows.Forms.Tools.ToolStripTabItem();
			this.toolStripExClipboard = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripExEdit2 = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripExScheduleViews = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripPanelItemViews = new Syncfusion.Windows.Forms.Tools.ToolStripPanelItem();
			this.toolStripButtonDayView = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonWeekView = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonFilterAgents = new System.Windows.Forms.ToolStripButton();
			this.toolStripPanelItemViews2 = new Syncfusion.Windows.Forms.Tools.ToolStripPanelItem();
			this.toolStripButtonPeriodView = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonSummaryView = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonRequestView = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonRestrictions = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonAgentInfo = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonFindAgents = new System.Windows.Forms.ToolStripButton();
			this.toolStripExActions = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripPanelItemAssignments = new Syncfusion.Windows.Forms.Tools.ToolStripPanelItem();
			this.toolStripSplitButtonSchedule = new System.Windows.Forms.ToolStripSplitButton();
			this.toolStripMenuItemScheduleSelected = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemScheduleHourlyEmployees = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemReOptimize = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemBackToLegalState = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripDropDownButtonSwap = new System.Windows.Forms.ToolStripDropDownButton();
			this.toolStripMenuItemSwap = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemSwapAndReschedule = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemSwapRaw = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripButtonRefresh = new System.Windows.Forms.ToolStripButton();
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
			this.ToolStripMenuItemAllPreferences = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAllAbsencePreference = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAllDaysOff = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAllShiftsPreferences = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAllMustHave = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAllFulFilledPreferences = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAllFulFilledAbsencesPreferences = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAllFulFilledDaysOffPreferences = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAllFulFilledShiftsPreferences = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAllFulfilledMustHave = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemLockRotations = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAllRotations = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAllDaysOffRotations = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAllFulFilledRotations = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAllFulFilledDaysOffRotations = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAllFulFilledShiftsRotations = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAllShiftsRotations = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemLockStudentAvailability = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAllUnavailableStudentAvailability = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAllAvailableStudentAvailability = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAllFulFilledStudentAvailability = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemLockAvailability = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAllUnavailableAvailability = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAllAvailableAvailability = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripMenuItemAllFulFilledAvailability = new System.Windows.Forms.ToolStripMenuItem();
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
			this.toolStripExLoadOptions = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripPanelItemLoadOptions = new Syncfusion.Windows.Forms.Tools.ToolStripPanelItem();
			this.toolStripButtonShrinkage = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonCalculation = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonValidation = new System.Windows.Forms.ToolStripButton();
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
			this.toolStripTabItemQuickAccess = new Syncfusion.Windows.Forms.Tools.ToolStripTabItem();
			this.toolStripExForQuickAccessItems = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.toolStripButtonQuickAccessSave = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparatorQuickAccess = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripButtonQuickAccessCancel = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonQuickAccessRedo = new System.Windows.Forms.ToolStripButton();
			this.toolStripSplitButtonQuickAccessUndo = new System.Windows.Forms.ToolStripSplitButton();
			this.toolStripMenuItemQuickAccessUndo = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemQuickAccessUndoAll = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripButtonShowTexts = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonMainMenuSave = new System.Windows.Forms.ToolStripButton();
			this.officeDropDownButtonMainMenuExportTo = new Syncfusion.Windows.Forms.Tools.OfficeDropDownButton();
			this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripButtonMainMenuHelp = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripButtonMainMenuClose = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonSystemExit = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonOptions = new System.Windows.Forms.ToolStripButton();
			this.btnFilter = new System.Windows.Forms.ToolStripButton();
			this.btnRightLeft = new System.Windows.Forms.ToolStripButton();
			this.imageListSkillTypeIcons = new System.Windows.Forms.ImageList(this.components);
			this.contextMenuStripRestrictionView = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.toolStripMenuItemUseRotation = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemUseAvailability = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemUsePreference = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemUseStudentAvailability = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemUseSchedule = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemAddPreferenceRestriction = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemAddStudentAvailabilityRestriction = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripMenuItemRestrictionCopy = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemRestrictionPaste = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemRestrictionDelete = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
			this.xxAgentInfoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.schedulerSplitters1 = new Teleopti.Ccc.Win.Scheduling.SchedulerSplitters();
			this.contextMenuStripRequests = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.ToolStripMenuItemViewDetails = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemFindMatching2 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItemViewAllowance = new System.Windows.Forms.ToolStripMenuItem();
			this.xxViewOldRequestsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripExFilterDays = new Syncfusion.Windows.Forms.Tools.ToolStripEx();
			this.xxShowLastNumberOfDays = new System.Windows.Forms.ToolStripLabel();
			this.toolStripComboBoxExFilterDays = new Syncfusion.Windows.Forms.Tools.ToolStripComboBoxEx();
			this.statusStrip1.SuspendLayout();
			this.contextMenuViews.SuspendLayout();
			this.contextMenuStripResultView.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).BeginInit();
			this.ribbonControlAdv1.SuspendLayout();
			this.toolStripTabItemHome.Panel.SuspendLayout();
			this.toolStripExScheduleViews.SuspendLayout();
			this.toolStripExActions.SuspendLayout();
			this.toolStripExLocks.SuspendLayout();
			this.toolStripExTags.SuspendLayout();
			this.toolStripEx1.SuspendLayout();
			this.toolStripExLoadOptions.SuspendLayout();
			this.toolStripTabItemChart.Panel.SuspendLayout();
			this.toolStripExGridRowInChartButtons.SuspendLayout();
			this.toolStripExSkillViews.SuspendLayout();
			this.toolStripTabItem1.Panel.SuspendLayout();
			this.toolStripEx2.SuspendLayout();
			this.toolStripExHandleRequests.SuspendLayout();
			this.toolStripEx3.SuspendLayout();
			this.toolStripTabItemQuickAccess.Panel.SuspendLayout();
			this.toolStripExForQuickAccessItems.SuspendLayout();
			this.contextMenuStripRestrictionView.SuspendLayout();
			this.contextMenuStripRequests.SuspendLayout();
			this.toolStripExFilterDays.SuspendLayout();
			this.SuspendLayout();
			// 
			// statusStrip1
			// 
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSpinningProgressControl1,
            this.toolStripStatusLabelStatus,
            this.toolStripProgressBar1,
            this.toolStripStatusLabelScheduleTag,
            this.toolStripStatusLabelContractTime});
			this.statusStrip1.Location = new System.Drawing.Point(6, 725);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(1211, 26);
			this.statusStrip1.TabIndex = 0;
			this.statusStrip1.Text = "yystatusStrip1";
			// 
			// toolStripSpinningProgressControl1
			// 
			this.toolStripSpinningProgressControl1.ActiveSegmentColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(146)))), ((int)(((byte)(33)))));
			this.toolStripSpinningProgressControl1.BehindTransitionSegmentIsActive = false;
			this.toolStripSpinningProgressControl1.InactiveSegmentColor = System.Drawing.Color.Silver;
			this.toolStripSpinningProgressControl1.Name = "ToolStripSpinningProgress";
			this.SetShortcut(this.toolStripSpinningProgressControl1, System.Windows.Forms.Keys.None);
			this.toolStripSpinningProgressControl1.Size = new System.Drawing.Size(20, 24);
			this.toolStripSpinningProgressControl1.Text = "toolStripSpinningProgressControl1";
			this.toolStripSpinningProgressControl1.TransitionSegment = 11;
			this.toolStripSpinningProgressControl1.TransitionSegmentColor = System.Drawing.Color.FromArgb(((int)(((byte)(129)))), ((int)(((byte)(242)))), ((int)(((byte)(121)))));
			this.toolStripSpinningProgressControl1.Visible = false;
			// 
			// toolStripStatusLabelStatus
			// 
			this.toolStripStatusLabelStatus.AutoSize = false;
			this.toolStripStatusLabelStatus.Name = "toolStripStatusLabelStatus";
			this.SetShortcut(this.toolStripStatusLabelStatus, System.Windows.Forms.Keys.None);
			this.toolStripStatusLabelStatus.Size = new System.Drawing.Size(998, 21);
			this.toolStripStatusLabelStatus.Spring = true;
			this.toolStripStatusLabelStatus.Text = "Ready";
			this.toolStripStatusLabelStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// toolStripProgressBar1
			// 
			this.toolStripProgressBar1.Name = "toolStripProgressBar1";
			this.SetShortcut(this.toolStripProgressBar1, System.Windows.Forms.Keys.None);
			this.toolStripProgressBar1.Size = new System.Drawing.Size(100, 20);
			// 
			// toolStripStatusLabelScheduleTag
			// 
			this.toolStripStatusLabelScheduleTag.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
			this.toolStripStatusLabelScheduleTag.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenInner;
			this.toolStripStatusLabelScheduleTag.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.toolStripStatusLabelScheduleTag.Name = "toolStripStatusLabelScheduleTag";
			this.SetShortcut(this.toolStripStatusLabelScheduleTag, System.Windows.Forms.Keys.None);
			this.toolStripStatusLabelScheduleTag.Size = new System.Drawing.Size(58, 21);
			this.toolStripStatusLabelScheduleTag.Text = "Standard";
			this.toolStripStatusLabelScheduleTag.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// toolStripStatusLabelContractTime
			// 
			this.toolStripStatusLabelContractTime.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
			this.toolStripStatusLabelContractTime.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenInner;
			this.toolStripStatusLabelContractTime.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.toolStripStatusLabelContractTime.Name = "toolStripStatusLabelContractTime";
			this.SetShortcut(this.toolStripStatusLabelContractTime, System.Windows.Forms.Keys.None);
			this.toolStripStatusLabelContractTime.Size = new System.Drawing.Size(38, 21);
			this.toolStripStatusLabelContractTime.Text = "00:00";
			this.toolStripStatusLabelContractTime.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// contextMenuViews
			// 
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
            this.toolStripMenuItemAgentInfo,
            this.toolStripMenuItemViewPointTimeZone,
            this.toolStripMenuItemSortBy,
            this.toolStripSeparator1,
            this.toolStripMenuItemNextAssignment,
            this.toolStripMenuItemShowAssignmentBefore,
            this.toolStripMenuItemUnlock,
            this.toolStripMenuItemLock,
            this.toolStripMenuItemMeetingOrganizer,
            this.toolStripMenuItemViewReport,
            this.xxExportToolStripMenuItem,
            this.toolStripMenuItemChangeTagRM});
			this.contextMenuViews.Name = "contextMenuStrip1";
			this.contextMenuViews.Size = new System.Drawing.Size(283, 666);
			this.contextMenuViews.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuViews_Opening);
			// 
			// toolStripMenuItemCut
			// 
			this.toolStripMenuItemCut.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Cut_small;
			this.toolStripMenuItemCut.Name = "toolStripMenuItemCut";
			this.SetShortcut(this.toolStripMenuItemCut, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemCut.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
			this.toolStripMenuItemCut.Size = new System.Drawing.Size(282, 22);
			this.toolStripMenuItemCut.Text = "xxCut";
			this.toolStripMenuItemCut.Click += new System.EventHandler(this.toolStripMenuItemCut_Click);
			// 
			// toolStripMenuItemCopy
			// 
			this.toolStripMenuItemCopy.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Copy_small;
			this.toolStripMenuItemCopy.Name = "toolStripMenuItemCopy";
			this.SetShortcut(this.toolStripMenuItemCopy, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemCopy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
			this.toolStripMenuItemCopy.Size = new System.Drawing.Size(282, 22);
			this.toolStripMenuItemCopy.Text = "xxCopy";
			this.toolStripMenuItemCopy.Click += new System.EventHandler(this.toolStripMenuItemCopy_Click);
			// 
			// toolStripMenuItemPaste
			// 
			this.toolStripMenuItemPaste.Enabled = false;
			this.toolStripMenuItemPaste.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Paste_small;
			this.toolStripMenuItemPaste.Name = "toolStripMenuItemPaste";
			this.SetShortcut(this.toolStripMenuItemPaste, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemPaste.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
			this.toolStripMenuItemPaste.Size = new System.Drawing.Size(282, 22);
			this.toolStripMenuItemPaste.Text = "xxPaste";
			this.toolStripMenuItemPaste.Click += new System.EventHandler(this.ToolStripMenuItemPaste_Click);
			// 
			// toolStripMenuItemDelete
			// 
			this.toolStripMenuItemDelete.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Delete_small;
			this.toolStripMenuItemDelete.Name = "toolStripMenuItemDelete";
			this.SetShortcut(this.toolStripMenuItemDelete, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemDelete.ShortcutKeys = System.Windows.Forms.Keys.Delete;
			this.toolStripMenuItemDelete.Size = new System.Drawing.Size(282, 22);
			this.toolStripMenuItemDelete.Text = "xxDelete";
			this.toolStripMenuItemDelete.Click += new System.EventHandler(this.toolStripButtonDelete_Click);
			// 
			// toolStripMenuItem2
			// 
			this.toolStripMenuItem2.Name = "toolStripMenuItem2";
			this.SetShortcut(this.toolStripMenuItem2, System.Windows.Forms.Keys.None);
			this.toolStripMenuItem2.Size = new System.Drawing.Size(279, 6);
			// 
			// toolStripMenuItemCutSpecial
			// 
			this.toolStripMenuItemCutSpecial.Name = "toolStripMenuItemCutSpecial";
			this.SetShortcut(this.toolStripMenuItemCutSpecial, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemCutSpecial.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.X)));
			this.toolStripMenuItemCutSpecial.Size = new System.Drawing.Size(282, 22);
			this.toolStripMenuItemCutSpecial.Text = "xxCutSpecialThreeDots";
			this.toolStripMenuItemCutSpecial.Click += new System.EventHandler(this.toolStripMenuItemCutSpecial2_Click);
			// 
			// toolStripMenuItemPasteSpecial
			// 
			this.toolStripMenuItemPasteSpecial.Name = "toolStripMenuItemPasteSpecial";
			this.SetShortcut(this.toolStripMenuItemPasteSpecial, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemPasteSpecial.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.V)));
			this.toolStripMenuItemPasteSpecial.Size = new System.Drawing.Size(282, 22);
			this.toolStripMenuItemPasteSpecial.Text = "xxPasteSpecialThreeDots";
			this.toolStripMenuItemPasteSpecial.Click += new System.EventHandler(this.toolStripMenuItemPasteSpecial2_Click);
			// 
			// toolStripMenuItemPasteShiftFromShifts
			// 
			this.toolStripMenuItemPasteShiftFromShifts.Name = "toolStripMenuItemPasteShiftFromShifts";
			this.SetShortcut(this.toolStripMenuItemPasteShiftFromShifts, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemPasteShiftFromShifts.Size = new System.Drawing.Size(282, 22);
			this.toolStripMenuItemPasteShiftFromShifts.Text = "xxPasteShiftFromShifts";
			this.toolStripMenuItemPasteShiftFromShifts.Click += new System.EventHandler(this.toolStripMenuItemPasteShiftFromShiftsClick);
			// 
			// toolStripMenuItemDeleteSpecial
			// 
			this.toolStripMenuItemDeleteSpecial.Name = "toolStripMenuItemDeleteSpecial";
			this.SetShortcut(this.toolStripMenuItemDeleteSpecial, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemDeleteSpecial.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.Delete)));
			this.toolStripMenuItemDeleteSpecial.Size = new System.Drawing.Size(282, 22);
			this.toolStripMenuItemDeleteSpecial.Text = "xxDeleteSpecialThreeDots";
			this.toolStripMenuItemDeleteSpecial.Click += new System.EventHandler(this.toolStripMenuItemDeleteSpecial2_Click);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.SetShortcut(this.toolStripMenuItem1, System.Windows.Forms.Keys.None);
			this.toolStripMenuItem1.Size = new System.Drawing.Size(279, 6);
			// 
			// ToolStripMenuItemAddActivity
			// 
			this.ToolStripMenuItemAddActivity.Name = "ToolStripMenuItemAddActivity";
			this.SetShortcut(this.ToolStripMenuItemAddActivity, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAddActivity.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.T)));
			this.ToolStripMenuItemAddActivity.Size = new System.Drawing.Size(282, 22);
			this.ToolStripMenuItemAddActivity.Text = "xxAddActivityThreeDots";
			this.ToolStripMenuItemAddActivity.Click += new System.EventHandler(this.ToolStripMenuItemAddActivity_Click);
			// 
			// ToolStripMenuItemAddPersonalActivity
			// 
			this.ToolStripMenuItemAddPersonalActivity.Name = "ToolStripMenuItemAddPersonalActivity";
			this.SetShortcut(this.ToolStripMenuItemAddPersonalActivity, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAddPersonalActivity.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.P)));
			this.ToolStripMenuItemAddPersonalActivity.Size = new System.Drawing.Size(282, 22);
			this.ToolStripMenuItemAddPersonalActivity.Text = "xxAddPersonalActivityThreeDots";
			this.ToolStripMenuItemAddPersonalActivity.Click += new System.EventHandler(this.ToolStripMenuItemAddPersonalActivity_Click);
			// 
			// toolStripMenuItemAddOverTime
			// 
			this.toolStripMenuItemAddOverTime.Name = "toolStripMenuItemAddOverTime";
			this.SetShortcut(this.toolStripMenuItemAddOverTime, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemAddOverTime.ShortcutKeyDisplayString = "Alt+O";
			this.toolStripMenuItemAddOverTime.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.O)));
			this.toolStripMenuItemAddOverTime.Size = new System.Drawing.Size(282, 22);
			this.toolStripMenuItemAddOverTime.Text = "xxAddOverTime";
			this.toolStripMenuItemAddOverTime.Click += new System.EventHandler(this.toolStripMenuItemAddOverTime_Click);
			// 
			// toolStripMenuItemInsertAbsence
			// 
			this.toolStripMenuItemInsertAbsence.Name = "toolStripMenuItemInsertAbsence";
			this.SetShortcut(this.toolStripMenuItemInsertAbsence, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemInsertAbsence.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.A)));
			this.toolStripMenuItemInsertAbsence.Size = new System.Drawing.Size(282, 22);
			this.toolStripMenuItemInsertAbsence.Text = "xxAddAbsenceThreeDots";
			this.toolStripMenuItemInsertAbsence.Click += new System.EventHandler(this.toolStripMenuItemInsertAbsence_Click);
			// 
			// toolStripMenuItemInsertDayOff
			// 
			this.toolStripMenuItemInsertDayOff.Name = "toolStripMenuItemInsertDayOff";
			this.SetShortcut(this.toolStripMenuItemInsertDayOff, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemInsertDayOff.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F)));
			this.toolStripMenuItemInsertDayOff.Size = new System.Drawing.Size(282, 22);
			this.toolStripMenuItemInsertDayOff.Text = "xxAddDayOffThreeDots";
			this.toolStripMenuItemInsertDayOff.Click += new System.EventHandler(this.toolStripButtonInsertDayOff_Click);
			// 
			// toolStripMenuItemAddPreference
			// 
			this.toolStripMenuItemAddPreference.Name = "toolStripMenuItemAddPreference";
			this.SetShortcut(this.toolStripMenuItemAddPreference, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemAddPreference.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.R)));
			this.toolStripMenuItemAddPreference.Size = new System.Drawing.Size(282, 22);
			this.toolStripMenuItemAddPreference.Text = "xxAddPreferenceThreeDots";
			this.toolStripMenuItemAddPreference.Click += new System.EventHandler(this.addPreferenceToolStripMenuItemClick);
			// 
			// toolStripMenuItemAddStudentAvailability
			// 
			this.toolStripMenuItemAddStudentAvailability.Name = "toolStripMenuItemAddStudentAvailability";
			this.SetShortcut(this.toolStripMenuItemAddStudentAvailability, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemAddStudentAvailability.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.H)));
			this.toolStripMenuItemAddStudentAvailability.Size = new System.Drawing.Size(282, 22);
			this.toolStripMenuItemAddStudentAvailability.Text = "xxAddStudentAvailabilityThreeDots";
			this.toolStripMenuItemAddStudentAvailability.Click += new System.EventHandler(this.addStudentAvailabilityToolStripMenuItemClick);
			// 
			// toolStripMenuItemAddOvertimeAvailability
			// 
			this.toolStripMenuItemAddOvertimeAvailability.Name = "toolStripMenuItemAddOvertimeAvailability";
			this.SetShortcut(this.toolStripMenuItemAddOvertimeAvailability, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemAddOvertimeAvailability.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.V)));
			this.toolStripMenuItemAddOvertimeAvailability.Size = new System.Drawing.Size(282, 22);
			this.toolStripMenuItemAddOvertimeAvailability.Text = "xxAddOvertimeAvailabilityThreeDots";
			this.toolStripMenuItemAddOvertimeAvailability.Click += new System.EventHandler(this.addOvertimeAvailabilityToolStripMenuItemClick);
			// 
			// toolStripMenuItem3
			// 
			this.toolStripMenuItem3.Name = "toolStripMenuItem3";
			this.SetShortcut(this.toolStripMenuItem3, System.Windows.Forms.Keys.None);
			this.toolStripMenuItem3.Size = new System.Drawing.Size(279, 6);
			// 
			// toolStripMenuItemFindMatching
			// 
			this.toolStripMenuItemFindMatching.Name = "toolStripMenuItemFindMatching";
			this.SetShortcut(this.toolStripMenuItemFindMatching, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemFindMatching.Size = new System.Drawing.Size(282, 22);
			this.toolStripMenuItemFindMatching.Text = "xxFindMatchingThreeDots";
			this.toolStripMenuItemFindMatching.Visible = false;
			this.toolStripMenuItemFindMatching.Click += new System.EventHandler(this.toolStripMenuItemFindMatching_Click);
			// 
			// toolStripMenuItemViewHistory
			// 
			this.toolStripMenuItemViewHistory.Name = "toolStripMenuItemViewHistory";
			this.SetShortcut(this.toolStripMenuItemViewHistory, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemViewHistory.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.H)));
			this.toolStripMenuItemViewHistory.Size = new System.Drawing.Size(282, 22);
			this.toolStripMenuItemViewHistory.Text = "xxViewScheduleHistory";
			this.toolStripMenuItemViewHistory.Click += new System.EventHandler(this.toolStripMenuItemViewHistory_Click);
			// 
			// toolStripMenuItemAgentInfo
			// 
			this.toolStripMenuItemAgentInfo.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_AgentInfo2;
			this.toolStripMenuItemAgentInfo.Name = "toolStripMenuItemAgentInfo";
			this.SetShortcut(this.toolStripMenuItemAgentInfo, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemAgentInfo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.I)));
			this.toolStripMenuItemAgentInfo.Size = new System.Drawing.Size(282, 22);
			this.toolStripMenuItemAgentInfo.Text = "xxAgentInfo";
			this.toolStripMenuItemAgentInfo.Click += new System.EventHandler(this.toolStripButtonAgentInfo_Click);
			// 
			// toolStripMenuItemViewPointTimeZone
			// 
			this.toolStripMenuItemViewPointTimeZone.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemLoggedOnUserTimeZone});
			this.toolStripMenuItemViewPointTimeZone.Name = "toolStripMenuItemViewPointTimeZone";
			this.SetShortcut(this.toolStripMenuItemViewPointTimeZone, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemViewPointTimeZone.Size = new System.Drawing.Size(282, 22);
			this.toolStripMenuItemViewPointTimeZone.Text = "xxViewPointTimeZone";
			// 
			// toolStripMenuItemLoggedOnUserTimeZone
			// 
			this.toolStripMenuItemLoggedOnUserTimeZone.Checked = true;
			this.toolStripMenuItemLoggedOnUserTimeZone.CheckOnClick = true;
			this.toolStripMenuItemLoggedOnUserTimeZone.CheckState = System.Windows.Forms.CheckState.Checked;
			this.toolStripMenuItemLoggedOnUserTimeZone.Name = "toolStripMenuItemLoggedOnUserTimeZone";
			this.SetShortcut(this.toolStripMenuItemLoggedOnUserTimeZone, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemLoggedOnUserTimeZone.Size = new System.Drawing.Size(110, 22);
			this.toolStripMenuItemLoggedOnUserTimeZone.Text = "qwerty";
			this.toolStripMenuItemLoggedOnUserTimeZone.Click += new System.EventHandler(this.toolStripMenuItemLoggedOnUserTimeZone_Click);
			this.toolStripMenuItemLoggedOnUserTimeZone.MouseUp += new System.Windows.Forms.MouseEventHandler(this.toolStripMenuItemLoggedOnUserTimeZoneMouseUp);
			// 
			// toolStripMenuItemSortBy
			// 
			this.toolStripMenuItemSortBy.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemStartAsc,
            this.toolStripMenuItemStartTimeDesc,
            this.toolStripMenuItemEndTimeAsc,
            this.toolStripMenuItemEndTimeDesc,
            this.xxContractTimeAscToolStripMenuItem,
            this.xxContractTimeDescToolStripMenuItem});
			this.toolStripMenuItemSortBy.Name = "toolStripMenuItemSortBy";
			this.SetShortcut(this.toolStripMenuItemSortBy, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemSortBy.Size = new System.Drawing.Size(282, 22);
			this.toolStripMenuItemSortBy.Text = "xxSortBy";
			// 
			// toolStripMenuItemStartAsc
			// 
			this.toolStripMenuItemStartAsc.Name = "toolStripMenuItemStartAsc";
			this.SetShortcut(this.toolStripMenuItemStartAsc, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemStartAsc.Size = new System.Drawing.Size(182, 22);
			this.toolStripMenuItemStartAsc.Text = "xxStartTimeAsc";
			this.toolStripMenuItemStartAsc.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemStartAscMouseUp);
			// 
			// toolStripMenuItemStartTimeDesc
			// 
			this.toolStripMenuItemStartTimeDesc.Name = "toolStripMenuItemStartTimeDesc";
			this.SetShortcut(this.toolStripMenuItemStartTimeDesc, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemStartTimeDesc.Size = new System.Drawing.Size(182, 22);
			this.toolStripMenuItemStartTimeDesc.Text = "xxStartTimeDesc";
			this.toolStripMenuItemStartTimeDesc.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemStartTimeDescMouseUp);
			// 
			// toolStripMenuItemEndTimeAsc
			// 
			this.toolStripMenuItemEndTimeAsc.Name = "toolStripMenuItemEndTimeAsc";
			this.SetShortcut(this.toolStripMenuItemEndTimeAsc, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemEndTimeAsc.Size = new System.Drawing.Size(182, 22);
			this.toolStripMenuItemEndTimeAsc.Text = "xxEndTimeAsc";
			this.toolStripMenuItemEndTimeAsc.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemEndTimeAscMouseUp);
			// 
			// toolStripMenuItemEndTimeDesc
			// 
			this.toolStripMenuItemEndTimeDesc.Name = "toolStripMenuItemEndTimeDesc";
			this.SetShortcut(this.toolStripMenuItemEndTimeDesc, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemEndTimeDesc.Size = new System.Drawing.Size(182, 22);
			this.toolStripMenuItemEndTimeDesc.Text = "xxEndTimeDesc";
			this.toolStripMenuItemEndTimeDesc.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemEndTimeDescMouseUp);
			// 
			// xxContractTimeAscToolStripMenuItem
			// 
			this.xxContractTimeAscToolStripMenuItem.Name = "xxContractTimeAscToolStripMenuItem";
			this.SetShortcut(this.xxContractTimeAscToolStripMenuItem, System.Windows.Forms.Keys.None);
			this.xxContractTimeAscToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
			this.xxContractTimeAscToolStripMenuItem.Text = "xxContractTimeAsc";
			this.xxContractTimeAscToolStripMenuItem.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemContractTimeAscMouseUp);
			// 
			// xxContractTimeDescToolStripMenuItem
			// 
			this.xxContractTimeDescToolStripMenuItem.Name = "xxContractTimeDescToolStripMenuItem";
			this.SetShortcut(this.xxContractTimeDescToolStripMenuItem, System.Windows.Forms.Keys.None);
			this.xxContractTimeDescToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
			this.xxContractTimeDescToolStripMenuItem.Text = "xxContractTimeDesc";
			this.xxContractTimeDescToolStripMenuItem.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemContractTimeDescMouseUp);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.SetShortcut(this.toolStripSeparator1, System.Windows.Forms.Keys.None);
			this.toolStripSeparator1.Size = new System.Drawing.Size(279, 6);
			// 
			// toolStripMenuItemNextAssignment
			// 
			this.toolStripMenuItemNextAssignment.Name = "toolStripMenuItemNextAssignment";
			this.SetShortcut(this.toolStripMenuItemNextAssignment, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemNextAssignment.Size = new System.Drawing.Size(282, 22);
			this.toolStripMenuItemNextAssignment.Text = "xxShowNextShift";
			this.toolStripMenuItemNextAssignment.Click += new System.EventHandler(this.toolStripMenuItemShowNextAssignment_Click);
			// 
			// toolStripMenuItemShowAssignmentBefore
			// 
			this.toolStripMenuItemShowAssignmentBefore.Name = "toolStripMenuItemShowAssignmentBefore";
			this.SetShortcut(this.toolStripMenuItemShowAssignmentBefore, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemShowAssignmentBefore.Size = new System.Drawing.Size(282, 22);
			this.toolStripMenuItemShowAssignmentBefore.Text = "xxShowPreviousShift";
			this.toolStripMenuItemShowAssignmentBefore.Click += new System.EventHandler(this.toolStripMenuItemShowAssignmentBefore_Click);
			// 
			// toolStripMenuItemUnlock
			// 
			this.toolStripMenuItemUnlock.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemUnlockSelectionRM,
            this.toolStripMenuItemUnlockAllRM});
			this.toolStripMenuItemUnlock.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Unlock;
			this.toolStripMenuItemUnlock.Name = "toolStripMenuItemUnlock";
			this.SetShortcut(this.toolStripMenuItemUnlock, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemUnlock.Size = new System.Drawing.Size(282, 22);
			this.toolStripMenuItemUnlock.Text = "xxUnlock";
			// 
			// toolStripMenuItemUnlockSelectionRM
			// 
			this.toolStripMenuItemUnlockSelectionRM.Name = "toolStripMenuItemUnlockSelectionRM";
			this.SetShortcut(this.toolStripMenuItemUnlockSelectionRM, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemUnlockSelectionRM.Size = new System.Drawing.Size(169, 22);
			this.toolStripMenuItemUnlockSelectionRM.Text = "xxUnlockSelection";
			this.toolStripMenuItemUnlockSelectionRM.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemUnlockSelectionRmMouseUp);
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
			this.toolStripMenuItemLock.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Lock2;
			this.toolStripMenuItemLock.Name = "toolStripMenuItemLock";
			this.SetShortcut(this.toolStripMenuItemLock, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemLock.Size = new System.Drawing.Size(282, 22);
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
			this.ToolStripMenuItemAllRM.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemLockAllRestrictionsMouseUp);
			// 
			// ToolStripMenuItemLockPreferencesRM
			// 
			this.ToolStripMenuItemLockPreferencesRM.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItemAllPreferencesRM,
            this.ToolStripMenuItemAllAbsencePreferenceRM,
            this.ToolStripMenuItemAllDaysOffPreferencesRM,
            this.ToolStripMenuItemAllShiftsPreferencesRM,
            this.ToolStripMenuItemAllMustHaveRM,
            this.ToolStripMenuItemAllFulFilledPreferencesRM,
            this.ToolStripMenuItemAllFulFilledAbsencesPreferencesRM,
            this.ToolStripMenuItemAllFulFilledDaysOffPreferencesRM,
            this.ToolStripMenuItemAllFulFilledShiftsPreferencesRM,
            this.ToolStripMenuItemAllFulfilledMustHaveRM});
			this.ToolStripMenuItemLockPreferencesRM.Name = "ToolStripMenuItemLockPreferencesRM";
			this.SetShortcut(this.ToolStripMenuItemLockPreferencesRM, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemLockPreferencesRM.Size = new System.Drawing.Size(208, 22);
			this.ToolStripMenuItemLockPreferencesRM.Text = "xxLockPreferences";
			// 
			// ToolStripMenuItemAllPreferencesRM
			// 
			this.ToolStripMenuItemAllPreferencesRM.Name = "ToolStripMenuItemAllPreferencesRM";
			this.SetShortcut(this.ToolStripMenuItemAllPreferencesRM, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAllPreferencesRM.Size = new System.Drawing.Size(194, 22);
			this.ToolStripMenuItemAllPreferencesRM.Text = "xxAll";
			this.ToolStripMenuItemAllPreferencesRM.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemAllPreferencesMouseUp);
			// 
			// ToolStripMenuItemAllAbsencePreferenceRM
			// 
			this.ToolStripMenuItemAllAbsencePreferenceRM.Name = "ToolStripMenuItemAllAbsencePreferenceRM";
			this.SetShortcut(this.ToolStripMenuItemAllAbsencePreferenceRM, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAllAbsencePreferenceRM.Size = new System.Drawing.Size(194, 22);
			this.ToolStripMenuItemAllAbsencePreferenceRM.Text = "xxAllAbsences";
			this.ToolStripMenuItemAllAbsencePreferenceRM.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemAllAbsencePreferenceMouseUp);
			// 
			// ToolStripMenuItemAllDaysOffPreferencesRM
			// 
			this.ToolStripMenuItemAllDaysOffPreferencesRM.Name = "ToolStripMenuItemAllDaysOffPreferencesRM";
			this.SetShortcut(this.ToolStripMenuItemAllDaysOffPreferencesRM, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAllDaysOffPreferencesRM.Size = new System.Drawing.Size(194, 22);
			this.ToolStripMenuItemAllDaysOffPreferencesRM.Text = "xxAllDaysOff";
			this.ToolStripMenuItemAllDaysOffPreferencesRM.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemAllDaysOffMouseUp);
			// 
			// ToolStripMenuItemAllShiftsPreferencesRM
			// 
			this.ToolStripMenuItemAllShiftsPreferencesRM.Name = "ToolStripMenuItemAllShiftsPreferencesRM";
			this.SetShortcut(this.ToolStripMenuItemAllShiftsPreferencesRM, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAllShiftsPreferencesRM.Size = new System.Drawing.Size(194, 22);
			this.ToolStripMenuItemAllShiftsPreferencesRM.Text = "xxAllShifts";
			this.ToolStripMenuItemAllShiftsPreferencesRM.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemAllShiftsPreferencesMouseUp);
			// 
			// ToolStripMenuItemAllMustHaveRM
			// 
			this.ToolStripMenuItemAllMustHaveRM.Name = "ToolStripMenuItemAllMustHaveRM";
			this.SetShortcut(this.ToolStripMenuItemAllMustHaveRM, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAllMustHaveRM.Size = new System.Drawing.Size(194, 22);
			this.ToolStripMenuItemAllMustHaveRM.Text = "xxAllMustHave";
			this.ToolStripMenuItemAllMustHaveRM.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemAllMustHaveMouseUp);
			// 
			// ToolStripMenuItemAllFulFilledPreferencesRM
			// 
			this.ToolStripMenuItemAllFulFilledPreferencesRM.Name = "ToolStripMenuItemAllFulFilledPreferencesRM";
			this.SetShortcut(this.ToolStripMenuItemAllFulFilledPreferencesRM, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAllFulFilledPreferencesRM.Size = new System.Drawing.Size(194, 22);
			this.ToolStripMenuItemAllFulFilledPreferencesRM.Text = "xxAllFulFilled";
			this.ToolStripMenuItemAllFulFilledPreferencesRM.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemAllFulFilledPreferencesMouseUp);
			// 
			// ToolStripMenuItemAllFulFilledAbsencesPreferencesRM
			// 
			this.ToolStripMenuItemAllFulFilledAbsencesPreferencesRM.Name = "ToolStripMenuItemAllFulFilledAbsencesPreferencesRM";
			this.SetShortcut(this.ToolStripMenuItemAllFulFilledAbsencesPreferencesRM, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAllFulFilledAbsencesPreferencesRM.Size = new System.Drawing.Size(194, 22);
			this.ToolStripMenuItemAllFulFilledAbsencesPreferencesRM.Text = "xxAllFulFilledAbsences";
			this.ToolStripMenuItemAllFulFilledAbsencesPreferencesRM.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemAllFulFilledAbsencesPreferencesMouseUp);
			// 
			// ToolStripMenuItemAllFulFilledDaysOffPreferencesRM
			// 
			this.ToolStripMenuItemAllFulFilledDaysOffPreferencesRM.Name = "ToolStripMenuItemAllFulFilledDaysOffPreferencesRM";
			this.SetShortcut(this.ToolStripMenuItemAllFulFilledDaysOffPreferencesRM, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAllFulFilledDaysOffPreferencesRM.Size = new System.Drawing.Size(194, 22);
			this.ToolStripMenuItemAllFulFilledDaysOffPreferencesRM.Text = "xxAllFulFilledDaysOff";
			this.ToolStripMenuItemAllFulFilledDaysOffPreferencesRM.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemAllFulFilledDaysOffPreferencesMouseUp);
			// 
			// ToolStripMenuItemAllFulFilledShiftsPreferencesRM
			// 
			this.ToolStripMenuItemAllFulFilledShiftsPreferencesRM.Name = "ToolStripMenuItemAllFulFilledShiftsPreferencesRM";
			this.SetShortcut(this.ToolStripMenuItemAllFulFilledShiftsPreferencesRM, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAllFulFilledShiftsPreferencesRM.Size = new System.Drawing.Size(194, 22);
			this.ToolStripMenuItemAllFulFilledShiftsPreferencesRM.Text = "xxAllFulFilledShifts";
			this.ToolStripMenuItemAllFulFilledShiftsPreferencesRM.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemAllFulFilledShiftsPreferencesMouseUp);
			// 
			// ToolStripMenuItemAllFulfilledMustHaveRM
			// 
			this.ToolStripMenuItemAllFulfilledMustHaveRM.Name = "ToolStripMenuItemAllFulfilledMustHaveRM";
			this.SetShortcut(this.ToolStripMenuItemAllFulfilledMustHaveRM, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAllFulfilledMustHaveRM.Size = new System.Drawing.Size(194, 22);
			this.ToolStripMenuItemAllFulfilledMustHaveRM.Text = "xxAllFulfilledMustHave";
			this.ToolStripMenuItemAllFulfilledMustHaveRM.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemAllFulfilledMustHaveMouseUp);
			// 
			// ToolStripMenuItemLockRotationsRM
			// 
			this.ToolStripMenuItemLockRotationsRM.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItemAllRotationsRM,
            this.ToolStripMenuItemAllDaysOffRotationsRM,
            this.ToolStripMenuItemAllShiftsRotationsRM,
            this.ToolStripMenuItemAllFulFilledRotationsRM,
            this.ToolStripMenuItemAllFulFilledDaysOffRotationsRM,
            this.ToolStripMenuItemAllFulFilledShiftsRotationsRM});
			this.ToolStripMenuItemLockRotationsRM.Name = "ToolStripMenuItemLockRotationsRM";
			this.SetShortcut(this.ToolStripMenuItemLockRotationsRM, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemLockRotationsRM.Size = new System.Drawing.Size(208, 22);
			this.ToolStripMenuItemLockRotationsRM.Text = "xxLockRotations";
			// 
			// ToolStripMenuItemAllRotationsRM
			// 
			this.ToolStripMenuItemAllRotationsRM.Name = "ToolStripMenuItemAllRotationsRM";
			this.SetShortcut(this.ToolStripMenuItemAllRotationsRM, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAllRotationsRM.Size = new System.Drawing.Size(184, 22);
			this.ToolStripMenuItemAllRotationsRM.Text = "xxAll";
			this.ToolStripMenuItemAllRotationsRM.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemAllRotationsMouseUp);
			// 
			// ToolStripMenuItemAllDaysOffRotationsRM
			// 
			this.ToolStripMenuItemAllDaysOffRotationsRM.Name = "ToolStripMenuItemAllDaysOffRotationsRM";
			this.SetShortcut(this.ToolStripMenuItemAllDaysOffRotationsRM, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAllDaysOffRotationsRM.Size = new System.Drawing.Size(184, 22);
			this.ToolStripMenuItemAllDaysOffRotationsRM.Text = "xxAllDaysOff";
			this.ToolStripMenuItemAllDaysOffRotationsRM.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemAllDaysOffRotationsMouseUp);
			// 
			// ToolStripMenuItemAllShiftsRotationsRM
			// 
			this.ToolStripMenuItemAllShiftsRotationsRM.Name = "ToolStripMenuItemAllShiftsRotationsRM";
			this.SetShortcut(this.ToolStripMenuItemAllShiftsRotationsRM, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAllShiftsRotationsRM.Size = new System.Drawing.Size(184, 22);
			this.ToolStripMenuItemAllShiftsRotationsRM.Text = "xxAllShifts";
			this.ToolStripMenuItemAllShiftsRotationsRM.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemAllShiftsRotationsMouseUp);
			// 
			// ToolStripMenuItemAllFulFilledRotationsRM
			// 
			this.ToolStripMenuItemAllFulFilledRotationsRM.Name = "ToolStripMenuItemAllFulFilledRotationsRM";
			this.SetShortcut(this.ToolStripMenuItemAllFulFilledRotationsRM, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAllFulFilledRotationsRM.Size = new System.Drawing.Size(184, 22);
			this.ToolStripMenuItemAllFulFilledRotationsRM.Text = "xxAllFulFilled";
			this.ToolStripMenuItemAllFulFilledRotationsRM.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemAllFulFilledRotationsMouseUp);
			// 
			// ToolStripMenuItemAllFulFilledDaysOffRotationsRM
			// 
			this.ToolStripMenuItemAllFulFilledDaysOffRotationsRM.Name = "ToolStripMenuItemAllFulFilledDaysOffRotationsRM";
			this.SetShortcut(this.ToolStripMenuItemAllFulFilledDaysOffRotationsRM, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAllFulFilledDaysOffRotationsRM.Size = new System.Drawing.Size(184, 22);
			this.ToolStripMenuItemAllFulFilledDaysOffRotationsRM.Text = "xxAllFulFilledDaysOff";
			this.ToolStripMenuItemAllFulFilledDaysOffRotationsRM.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemAllFulFilledDaysOffRotationsMouseUp);
			// 
			// ToolStripMenuItemAllFulFilledShiftsRotationsRM
			// 
			this.ToolStripMenuItemAllFulFilledShiftsRotationsRM.Name = "ToolStripMenuItemAllFulFilledShiftsRotationsRM";
			this.SetShortcut(this.ToolStripMenuItemAllFulFilledShiftsRotationsRM, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAllFulFilledShiftsRotationsRM.Size = new System.Drawing.Size(184, 22);
			this.ToolStripMenuItemAllFulFilledShiftsRotationsRM.Text = "xxAllFulFilledShifts";
			this.ToolStripMenuItemAllFulFilledShiftsRotationsRM.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemAllFulFilledShiftsRotationsMouseUp);
			// 
			// ToolStripMenuItemLockStudentAvailabilityRM
			// 
			this.ToolStripMenuItemLockStudentAvailabilityRM.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItemAllUnavailableStudentAvailabilityRM,
            this.ToolStripMenuItemAllAvailableStudentAvailabilityRM,
            this.ToolStripMenuItemAllFulFilledStudentAvailabilityRM});
			this.ToolStripMenuItemLockStudentAvailabilityRM.Name = "ToolStripMenuItemLockStudentAvailabilityRM";
			this.SetShortcut(this.ToolStripMenuItemLockStudentAvailabilityRM, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemLockStudentAvailabilityRM.Size = new System.Drawing.Size(208, 22);
			this.ToolStripMenuItemLockStudentAvailabilityRM.Text = "xxLockStudentAvailability";
			// 
			// ToolStripMenuItemAllUnavailableStudentAvailabilityRM
			// 
			this.ToolStripMenuItemAllUnavailableStudentAvailabilityRM.Name = "ToolStripMenuItemAllUnavailableStudentAvailabilityRM";
			this.SetShortcut(this.ToolStripMenuItemAllUnavailableStudentAvailabilityRM, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAllUnavailableStudentAvailabilityRM.Size = new System.Drawing.Size(159, 22);
			this.ToolStripMenuItemAllUnavailableStudentAvailabilityRM.Text = "xxAllUnavailable";
			this.ToolStripMenuItemAllUnavailableStudentAvailabilityRM.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemAllUnavailableStudentAvailabilityMouseUp);
			// 
			// ToolStripMenuItemAllAvailableStudentAvailabilityRM
			// 
			this.ToolStripMenuItemAllAvailableStudentAvailabilityRM.Name = "ToolStripMenuItemAllAvailableStudentAvailabilityRM";
			this.SetShortcut(this.ToolStripMenuItemAllAvailableStudentAvailabilityRM, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAllAvailableStudentAvailabilityRM.Size = new System.Drawing.Size(159, 22);
			this.ToolStripMenuItemAllAvailableStudentAvailabilityRM.Text = "xxAllAvailable";
			this.ToolStripMenuItemAllAvailableStudentAvailabilityRM.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemAllAvailableStudentAvailabilityMouseUp);
			// 
			// ToolStripMenuItemAllFulFilledStudentAvailabilityRM
			// 
			this.ToolStripMenuItemAllFulFilledStudentAvailabilityRM.Name = "ToolStripMenuItemAllFulFilledStudentAvailabilityRM";
			this.SetShortcut(this.ToolStripMenuItemAllFulFilledStudentAvailabilityRM, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAllFulFilledStudentAvailabilityRM.Size = new System.Drawing.Size(159, 22);
			this.ToolStripMenuItemAllFulFilledStudentAvailabilityRM.Text = "xxAllFulFilled";
			this.ToolStripMenuItemAllFulFilledStudentAvailabilityRM.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemAllFulFilledStudentAvailabilityMouseUp);
			// 
			// ToolStripMenuItemLockAvailabilityRM
			// 
			this.ToolStripMenuItemLockAvailabilityRM.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItemAllUnAvailableAvailabilityRM,
            this.ToolStripMenuItemAllAvailableAvailabilityRM,
            this.ToolStripMenuItemAllFulFilledAvailabilityRM});
			this.ToolStripMenuItemLockAvailabilityRM.Name = "ToolStripMenuItemLockAvailabilityRM";
			this.SetShortcut(this.ToolStripMenuItemLockAvailabilityRM, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemLockAvailabilityRM.Size = new System.Drawing.Size(208, 22);
			this.ToolStripMenuItemLockAvailabilityRM.Text = "xxLockAvailability";
			// 
			// ToolStripMenuItemAllUnAvailableAvailabilityRM
			// 
			this.ToolStripMenuItemAllUnAvailableAvailabilityRM.Name = "ToolStripMenuItemAllUnAvailableAvailabilityRM";
			this.SetShortcut(this.ToolStripMenuItemAllUnAvailableAvailabilityRM, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAllUnAvailableAvailabilityRM.Size = new System.Drawing.Size(161, 22);
			this.ToolStripMenuItemAllUnAvailableAvailabilityRM.Text = "xxAllUnAvailable";
			this.ToolStripMenuItemAllUnAvailableAvailabilityRM.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemAllUnavailableAvailabilityMouseUp);
			// 
			// ToolStripMenuItemAllAvailableAvailabilityRM
			// 
			this.ToolStripMenuItemAllAvailableAvailabilityRM.Name = "ToolStripMenuItemAllAvailableAvailabilityRM";
			this.SetShortcut(this.ToolStripMenuItemAllAvailableAvailabilityRM, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAllAvailableAvailabilityRM.Size = new System.Drawing.Size(161, 22);
			this.ToolStripMenuItemAllAvailableAvailabilityRM.Text = "xxAllAvailable";
			this.ToolStripMenuItemAllAvailableAvailabilityRM.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemAllAvailableAvailabilityMouseUp);
			// 
			// ToolStripMenuItemAllFulFilledAvailabilityRM
			// 
			this.ToolStripMenuItemAllFulFilledAvailabilityRM.Name = "ToolStripMenuItemAllFulFilledAvailabilityRM";
			this.SetShortcut(this.ToolStripMenuItemAllFulFilledAvailabilityRM, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAllFulFilledAvailabilityRM.Size = new System.Drawing.Size(161, 22);
			this.ToolStripMenuItemAllFulFilledAvailabilityRM.Text = "xxAllFulFilled";
			this.ToolStripMenuItemAllFulFilledAvailabilityRM.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemAllFulFilledAvailabilityMouseUp);
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
			this.toolstripMenuRemoveWriteProtection.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolstripMenuRemoveWriteProtectionMouseUp);
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
			this.toolStripMenuItemMeetingOrganizer.Size = new System.Drawing.Size(282, 22);
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
			// toolStripMenuItemViewReport
			// 
			this.toolStripMenuItemViewReport.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemScheduledTimePerActivity});
			this.toolStripMenuItemViewReport.Name = "toolStripMenuItemViewReport";
			this.SetShortcut(this.toolStripMenuItemViewReport, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemViewReport.Size = new System.Drawing.Size(282, 22);
			this.toolStripMenuItemViewReport.Text = "xxViewReports";
			// 
			// toolStripMenuItemScheduledTimePerActivity
			// 
			this.toolStripMenuItemScheduledTimePerActivity.Name = "toolStripMenuItemScheduledTimePerActivity";
			this.SetShortcut(this.toolStripMenuItemScheduledTimePerActivity, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemScheduledTimePerActivity.Size = new System.Drawing.Size(223, 22);
			this.toolStripMenuItemScheduledTimePerActivity.Text = "xxScheduledTimePerActivity";
			this.toolStripMenuItemScheduledTimePerActivity.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemScheduledTimePerActivityMouseUp);
			// 
			// xxExportToolStripMenuItem
			// 
			this.xxExportToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemExportToPDF,
            this.toolStripMenuItemExportToPDFGraphical,
            this.ToolStripMenuItemExportToPDFShiftsPerDay});
			this.xxExportToolStripMenuItem.Name = "xxExportToolStripMenuItem";
			this.SetShortcut(this.xxExportToolStripMenuItem, System.Windows.Forms.Keys.None);
			this.xxExportToolStripMenuItem.Size = new System.Drawing.Size(282, 22);
			this.xxExportToolStripMenuItem.Text = "xxExport";
			// 
			// toolStripMenuItemExportToPDF
			// 
			this.toolStripMenuItemExportToPDF.Name = "toolStripMenuItemExportToPDF";
			this.SetShortcut(this.toolStripMenuItemExportToPDF, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemExportToPDF.Size = new System.Drawing.Size(218, 22);
			this.toolStripMenuItemExportToPDF.Text = "xxExportToPDF";
			this.toolStripMenuItemExportToPDF.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemExportToPdfMouseUp);
			// 
			// toolStripMenuItemExportToPDFGraphical
			// 
			this.toolStripMenuItemExportToPDFGraphical.Name = "toolStripMenuItemExportToPDFGraphical";
			this.SetShortcut(this.toolStripMenuItemExportToPDFGraphical, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemExportToPDFGraphical.Size = new System.Drawing.Size(218, 22);
			this.toolStripMenuItemExportToPDFGraphical.Text = "xxExportToPDFGraphical";
			this.toolStripMenuItemExportToPDFGraphical.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemExportToPdfGraphicalMouseUp);
			// 
			// ToolStripMenuItemExportToPDFShiftsPerDay
			// 
			this.ToolStripMenuItemExportToPDFShiftsPerDay.Name = "ToolStripMenuItemExportToPDFShiftsPerDay";
			this.SetShortcut(this.ToolStripMenuItemExportToPDFShiftsPerDay, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemExportToPDFShiftsPerDay.Size = new System.Drawing.Size(218, 22);
			this.ToolStripMenuItemExportToPDFShiftsPerDay.Text = "xxExportToPDFShiftsPerDay";
			this.ToolStripMenuItemExportToPDFShiftsPerDay.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemExportToPDFShiftsPerDay_MouseUp);
			// 
			// toolStripMenuItemChangeTagRM
			// 
			this.toolStripMenuItemChangeTagRM.Name = "toolStripMenuItemChangeTagRM";
			this.SetShortcut(this.toolStripMenuItemChangeTagRM, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemChangeTagRM.Size = new System.Drawing.Size(282, 22);
			this.toolStripMenuItemChangeTagRM.Text = "xxChangeTag";
			// 
			// contextMenuStripResultView
			// 
			this.contextMenuStripResultView.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItemDay,
            this.ToolStripMenuItemIntraday,
            this.toolStripMenuItem4,
            this.ToolStripMenuItemOccupancyAdjustment});
			this.contextMenuStripResultView.Name = "contextMenuStripResultView";
			this.contextMenuStripResultView.Size = new System.Drawing.Size(207, 76);
			// 
			// ToolStripMenuItemDay
			// 
			this.ToolStripMenuItemDay.Checked = true;
			this.ToolStripMenuItemDay.CheckState = System.Windows.Forms.CheckState.Checked;
			this.ToolStripMenuItemDay.Enabled = false;
			this.ToolStripMenuItemDay.Name = "ToolStripMenuItemDay";
			this.SetShortcut(this.ToolStripMenuItemDay, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemDay.Size = new System.Drawing.Size(206, 22);
			this.ToolStripMenuItemDay.Text = "xxDay";
			// 
			// ToolStripMenuItemIntraday
			// 
			this.ToolStripMenuItemIntraday.Enabled = false;
			this.ToolStripMenuItemIntraday.Name = "ToolStripMenuItemIntraday";
			this.SetShortcut(this.ToolStripMenuItemIntraday, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemIntraday.Size = new System.Drawing.Size(206, 22);
			this.ToolStripMenuItemIntraday.Text = "xxIntraday";
			// 
			// toolStripMenuItem4
			// 
			this.toolStripMenuItem4.Name = "toolStripMenuItem4";
			this.SetShortcut(this.toolStripMenuItem4, System.Windows.Forms.Keys.None);
			this.toolStripMenuItem4.Size = new System.Drawing.Size(203, 6);
			// 
			// ToolStripMenuItemOccupancyAdjustment
			// 
			this.ToolStripMenuItemOccupancyAdjustment.Enabled = false;
			this.ToolStripMenuItemOccupancyAdjustment.Name = "ToolStripMenuItemOccupancyAdjustment";
			this.SetShortcut(this.ToolStripMenuItemOccupancyAdjustment, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemOccupancyAdjustment.Size = new System.Drawing.Size(206, 22);
			this.ToolStripMenuItemOccupancyAdjustment.Text = "xxOccupancyAdjustment";
			// 
			// backgroundWorkerLoadData
			// 
			this.backgroundWorkerLoadData.WorkerReportsProgress = true;
			this.backgroundWorkerLoadData.WorkerSupportsCancellation = true;
			// 
			// ribbonControlAdv1
			// 
			this.ribbonControlAdv1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.ribbonControlAdv1.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ribbonControlAdv1.Header.AddMainItem(toolStripTabItemHome);
			this.ribbonControlAdv1.Header.AddMainItem(toolStripTabItemChart);
			this.ribbonControlAdv1.Header.AddMainItem(toolStripTabItem1);
			this.ribbonControlAdv1.Header.AddMainItem(toolStripTabItemQuickAccess);
			this.ribbonControlAdv1.Location = new System.Drawing.Point(1, 0);
			this.ribbonControlAdv1.MaximizeToolTip = "Maximize Ribbon";
			this.ribbonControlAdv1.MenuButtonImage = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Menu;
			this.ribbonControlAdv1.MenuButtonText = "";
			this.ribbonControlAdv1.MinimizeToolTip = "Minimize Ribbon";
			this.ribbonControlAdv1.Name = "ribbonControlAdv1";
			this.ribbonControlAdv1.OfficeColorScheme = Syncfusion.Windows.Forms.Tools.ToolStripEx.ColorScheme.Managed;
			// 
			// ribbonControlAdv1.OfficeMenu
			// 
			this.ribbonControlAdv1.OfficeMenu.AutoSize = false;
			this.ribbonControlAdv1.OfficeMenu.AuxPanel.MinimumSize = new System.Drawing.Size(150, 0);
			this.ribbonControlAdv1.OfficeMenu.MainPanel.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonMainMenuSave,
            this.officeDropDownButtonMainMenuExportTo,
            this.toolStripSeparator4,
            this.toolStripButtonMainMenuHelp,
            this.toolStripSeparator3,
            this.toolStripButtonMainMenuClose});
			this.ribbonControlAdv1.OfficeMenu.MainPanel.MinimumSize = new System.Drawing.Size(150, 0);
			this.ribbonControlAdv1.OfficeMenu.Name = "OfficeMenu";
			this.ribbonControlAdv1.OfficeMenu.Size = new System.Drawing.Size(314, 250);
			this.ribbonControlAdv1.OfficeMenu.SystemPanel.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonSystemExit,
            this.toolStripButtonOptions});
			this.ribbonControlAdv1.SelectedTab = this.toolStripTabItemHome;
			this.ribbonControlAdv1.Size = new System.Drawing.Size(1221, 160);
			this.ribbonControlAdv1.SystemText.QuickAccessCustomizeCaptionText = "Customize QuickAccess Toolbar";
			this.ribbonControlAdv1.SystemText.QuickAccessDialogDropDownName = "";
			toolStripTabGroup1.Color = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
			toolStripTabGroup1.Name = "toolStripTabGroupShiftEditor";
			toolStripTabGroup1.Visible = true;
			toolStripTabGroup2.Color = System.Drawing.Color.Blue;
			toolStripTabGroup2.Name = "toolStripTabGroupMainGrid";
			toolStripTabGroup2.Visible = true;
			toolStripTabGroup3.Color = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
			toolStripTabGroup3.Name = "toolStripTabGroupShiftEditor2";
			toolStripTabGroup3.Visible = true;
			this.ribbonControlAdv1.TabGroups.Add(toolStripTabGroup1);
			this.ribbonControlAdv1.TabGroups.Add(toolStripTabGroup2);
			this.ribbonControlAdv1.TabGroups.Add(toolStripTabGroup3);
			this.ribbonControlAdv1.TabIndex = 5;
			this.ribbonControlAdv1.TitleAlignment = Syncfusion.Windows.Forms.Tools.TextAlignment.Center;
			// 
			// toolStripTabItemHome
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripTabItemHome, "");
			this.toolStripTabItemHome.Name = "toolStripTabItemHome";
			// 
			// ribbonControlAdv1.ribbonPanel1
			// 
			this.toolStripTabItemHome.Panel.Controls.Add(this.toolStripExClipboard);
			this.toolStripTabItemHome.Panel.Controls.Add(this.toolStripExEdit2);
			this.toolStripTabItemHome.Panel.Controls.Add(this.toolStripExScheduleViews);
			this.toolStripTabItemHome.Panel.Controls.Add(this.toolStripExActions);
			this.toolStripTabItemHome.Panel.Controls.Add(this.toolStripExLocks);
			this.toolStripTabItemHome.Panel.Controls.Add(this.toolStripExTags);
			this.toolStripTabItemHome.Panel.Controls.Add(this.toolStripEx1);
			this.toolStripTabItemHome.Panel.Controls.Add(this.toolStripExLoadOptions);
			this.toolStripTabItemHome.Panel.Name = "ribbonPanel1";
			this.toolStripTabItemHome.Panel.ScrollPosition = 0;
			this.toolStripTabItemHome.Panel.TabIndex = 2;
			this.toolStripTabItemHome.Panel.Text = "xxHome";
			this.toolStripTabItemHome.Position = 0;
			this.SetShortcut(this.toolStripTabItemHome, System.Windows.Forms.Keys.None);
			this.toolStripTabItemHome.Size = new System.Drawing.Size(51, 19);
			this.toolStripTabItemHome.Text = "xxHome";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripTabItemHome, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripTabItemHome, false);
			this.toolStripTabItemHome.Click += new System.EventHandler(this.toolStripTabItemHome_Click);
			// 
			// toolStripExClipboard
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripExClipboard, "");
			this.toolStripExClipboard.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripExClipboard.ForeColor = System.Drawing.Color.MidnightBlue;
			this.toolStripExClipboard.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripExClipboard.Image = null;
			this.toolStripExClipboard.Location = new System.Drawing.Point(0, 1);
			this.toolStripExClipboard.Name = "toolStripExClipboard";
			this.toolStripExClipboard.ShowItemToolTips = true;
			this.toolStripExClipboard.ShowLauncher = false;
			this.toolStripExClipboard.Size = new System.Drawing.Size(106, 98);
			this.toolStripExClipboard.TabIndex = 2;
			this.toolStripExClipboard.Text = "xxClipboard";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripExClipboard, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripExClipboard, false);
			// 
			// toolStripExEdit2
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripExEdit2, "");
			this.toolStripExEdit2.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripExEdit2.ForeColor = System.Drawing.Color.MidnightBlue;
			this.toolStripExEdit2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripExEdit2.Image = null;
			this.toolStripExEdit2.Location = new System.Drawing.Point(108, 1);
			this.toolStripExEdit2.Name = "toolStripExEdit2";
			this.toolStripExEdit2.ShowLauncher = false;
			this.toolStripExEdit2.Size = new System.Drawing.Size(106, 98);
			this.toolStripExEdit2.TabIndex = 12;
			this.toolStripExEdit2.Text = "Edit";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripExEdit2, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripExEdit2, false);
			// 
			// toolStripExScheduleViews
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripExScheduleViews, "");
			this.toolStripExScheduleViews.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripExScheduleViews.ForeColor = System.Drawing.Color.MidnightBlue;
			this.toolStripExScheduleViews.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripExScheduleViews.Image = null;
			this.toolStripExScheduleViews.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripPanelItemViews,
            this.toolStripPanelItemViews2});
			this.toolStripExScheduleViews.Location = new System.Drawing.Point(216, 1);
			this.toolStripExScheduleViews.Name = "toolStripExScheduleViews";
			this.toolStripExScheduleViews.ShowItemToolTips = true;
			this.toolStripExScheduleViews.ShowLauncher = false;
			this.toolStripExScheduleViews.Size = new System.Drawing.Size(285, 98);
			this.toolStripExScheduleViews.TabIndex = 5;
			this.toolStripExScheduleViews.Text = "xxView";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripExScheduleViews, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripExScheduleViews, false);
			// 
			// toolStripPanelItemViews
			// 
			this.toolStripPanelItemViews.CausesValidation = false;
			this.toolStripPanelItemViews.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.toolStripPanelItemViews.ForeColor = System.Drawing.Color.MidnightBlue;
			this.toolStripPanelItemViews.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonDayView,
            this.toolStripButtonWeekView,
            this.toolStripButtonFilterAgents});
			this.toolStripPanelItemViews.Name = "toolStripPanelItemViews";
			this.SetShortcut(this.toolStripPanelItemViews, System.Windows.Forms.Keys.None);
			this.toolStripPanelItemViews.Size = new System.Drawing.Size(96, 81);
			this.toolStripPanelItemViews.Text = "toolStripPanelItem3";
			this.toolStripPanelItemViews.Transparent = true;
			// 
			// toolStripButtonDayView
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonDayView, "");
			this.toolStripButtonDayView.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_schedule_IntradayView_32x321;
			this.toolStripButtonDayView.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonDayView.Name = "toolStripButtonDayView";
			this.SetShortcut(this.toolStripButtonDayView, System.Windows.Forms.Keys.None);
			this.toolStripButtonDayView.Size = new System.Drawing.Size(56, 20);
			this.toolStripButtonDayView.Text = "xxDay";
			this.toolStripButtonDayView.ToolTipText = "xxDay";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonDayView, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonDayView, false);
			this.toolStripButtonDayView.Click += new System.EventHandler(this.toolStripButtonZoom_Click);
			// 
			// toolStripButtonWeekView
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonWeekView, "");
			this.toolStripButtonWeekView.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_schedule_Weekview_32x321;
			this.toolStripButtonWeekView.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonWeekView.Name = "toolStripButtonWeekView";
			this.SetShortcut(this.toolStripButtonWeekView, System.Windows.Forms.Keys.None);
			this.toolStripButtonWeekView.Size = new System.Drawing.Size(66, 20);
			this.toolStripButtonWeekView.Text = "xxWeek";
			this.toolStripButtonWeekView.ToolTipText = "xxWeek";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonWeekView, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonWeekView, false);
			this.toolStripButtonWeekView.Click += new System.EventHandler(this.toolStripButtonZoom_Click);
			// 
			// toolStripButtonFilterAgents
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonFilterAgents, "");
			this.toolStripButtonFilterAgents.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Filter;
			this.toolStripButtonFilterAgents.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonFilterAgents.Name = "toolStripButtonFilterAgents";
			this.SetShortcut(this.toolStripButtonFilterAgents, System.Windows.Forms.Keys.None);
			this.toolStripButtonFilterAgents.Size = new System.Drawing.Size(92, 20);
			this.toolStripButtonFilterAgents.Text = "xxFilterAgents";
			this.toolStripButtonFilterAgents.ToolTipText = "xxFilterAgents";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonFilterAgents, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonFilterAgents, false);
			this.toolStripButtonFilterAgents.Click += new System.EventHandler(this.toolStripButtonFilterAgents_Click);
			// 
			// toolStripPanelItemViews2
			// 
			this.toolStripPanelItemViews2.CausesValidation = false;
			this.toolStripPanelItemViews2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.toolStripPanelItemViews2.ForeColor = System.Drawing.Color.MidnightBlue;
			this.toolStripPanelItemViews2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonPeriodView,
            this.toolStripButtonSummaryView,
            this.toolStripButtonRequestView,
            this.toolStripButtonRestrictions,
            this.toolStripButtonAgentInfo,
            this.toolStripButtonFindAgents});
			this.toolStripPanelItemViews2.Name = "toolStripPanelItemViews2";
			this.SetShortcut(this.toolStripPanelItemViews2, System.Windows.Forms.Keys.None);
			this.toolStripPanelItemViews2.Size = new System.Drawing.Size(178, 81);
			this.toolStripPanelItemViews2.Text = "toolStripPanelItem3";
			this.toolStripPanelItemViews2.Transparent = true;
			// 
			// toolStripButtonPeriodView
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonPeriodView, "");
			this.toolStripButtonPeriodView.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_schedule_Period_view_32x321;
			this.toolStripButtonPeriodView.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonPeriodView.Name = "toolStripButtonPeriodView";
			this.SetShortcut(this.toolStripButtonPeriodView, System.Windows.Forms.Keys.None);
			this.toolStripButtonPeriodView.Size = new System.Drawing.Size(67, 20);
			this.toolStripButtonPeriodView.Text = "xxPeriod";
			this.toolStripButtonPeriodView.ToolTipText = "xxPeriod";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonPeriodView, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonPeriodView, false);
			this.toolStripButtonPeriodView.Click += new System.EventHandler(this.toolStripButtonZoom_Click);
			// 
			// toolStripButtonSummaryView
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonSummaryView, "");
			this.toolStripButtonSummaryView.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_schedule_Summary_view_32x321;
			this.toolStripButtonSummaryView.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonSummaryView.Name = "toolStripButtonSummaryView";
			this.SetShortcut(this.toolStripButtonSummaryView, System.Windows.Forms.Keys.None);
			this.toolStripButtonSummaryView.Size = new System.Drawing.Size(80, 20);
			this.toolStripButtonSummaryView.Text = "xxSummary";
			this.toolStripButtonSummaryView.ToolTipText = "xxSummary";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonSummaryView, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonSummaryView, false);
			this.toolStripButtonSummaryView.Click += new System.EventHandler(this.toolStripButtonZoom_Click);
			// 
			// toolStripButtonRequestView
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonRequestView, "");
			this.toolStripButtonRequestView.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_agent_request_32x32;
			this.toolStripButtonRequestView.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonRequestView.Name = "toolStripButtonRequestView";
			this.SetShortcut(this.toolStripButtonRequestView, System.Windows.Forms.Keys.None);
			this.toolStripButtonRequestView.Size = new System.Drawing.Size(82, 20);
			this.toolStripButtonRequestView.Text = "xxRequests";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonRequestView, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonRequestView, false);
			this.toolStripButtonRequestView.Click += new System.EventHandler(this.toolStripButtonZoom_Click);
			// 
			// toolStripButtonRestrictions
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonRestrictions, "");
			this.toolStripButtonRestrictions.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Contract;
			this.toolStripButtonRestrictions.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonRestrictions.Name = "toolStripButtonRestrictions";
			this.SetShortcut(this.toolStripButtonRestrictions, System.Windows.Forms.Keys.None);
			this.toolStripButtonRestrictions.Size = new System.Drawing.Size(92, 20);
			this.toolStripButtonRestrictions.Text = "xxRestrictions";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonRestrictions, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonRestrictions, false);
			this.toolStripButtonRestrictions.Click += new System.EventHandler(this.toolStripButtonZoom_Click);
			// 
			// toolStripButtonAgentInfo
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonAgentInfo, "");
			this.toolStripButtonAgentInfo.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_AgentInfo2;
			this.toolStripButtonAgentInfo.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonAgentInfo.Name = "toolStripButtonAgentInfo";
			this.SetShortcut(this.toolStripButtonAgentInfo, System.Windows.Forms.Keys.None);
			this.toolStripButtonAgentInfo.Size = new System.Drawing.Size(83, 20);
			this.toolStripButtonAgentInfo.Text = "xxAgentInfo";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonAgentInfo, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonAgentInfo, false);
			this.toolStripButtonAgentInfo.Click += new System.EventHandler(this.toolStripButtonAgentInfo_Click);
			// 
			// toolStripButtonFindAgents
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonFindAgents, "");
			this.toolStripButtonFindAgents.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_FindAgent;
			this.toolStripButtonFindAgents.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonFindAgents.Name = "toolStripButtonFindAgents";
			this.SetShortcut(this.toolStripButtonFindAgents, ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F))));
			this.toolStripButtonFindAgents.Size = new System.Drawing.Size(57, 20);
			this.toolStripButtonFindAgents.Text = "xxFind";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonFindAgents, false);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonFindAgents, false);
			this.toolStripButtonFindAgents.Click += new System.EventHandler(this.ToolStripMenuItemSearch_Click);
			// 
			// toolStripExActions
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripExActions, "");
			this.toolStripExActions.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripExActions.ForeColor = System.Drawing.Color.MidnightBlue;
			this.toolStripExActions.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripExActions.Image = null;
			this.toolStripExActions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripPanelItemAssignments});
			this.toolStripExActions.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
			this.toolStripExActions.Location = new System.Drawing.Point(503, 1);
			this.toolStripExActions.Name = "toolStripExActions";
			this.toolStripExActions.ShowCaption = true;
			this.toolStripExActions.ShowItemToolTips = true;
			this.toolStripExActions.ShowLauncher = false;
			this.toolStripExActions.Size = new System.Drawing.Size(114, 98);
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
            this.toolStripDropDownButtonSwap,
            this.toolStripButtonRefresh});
			this.toolStripPanelItemAssignments.Name = "toolStripPanelItemAssignments";
			this.SetShortcut(this.toolStripPanelItemAssignments, System.Windows.Forms.Keys.None);
			this.toolStripPanelItemAssignments.Size = new System.Drawing.Size(107, 73);
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
            this.toolStripMenuItemReOptimize,
            this.toolStripSeparator2,
            this.toolStripMenuItemBackToLegalState});
			this.toolStripSplitButtonSchedule.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_SchedulerSchedule;
			this.toolStripSplitButtonSchedule.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripSplitButtonSchedule.Name = "toolStripSplitButtonSchedule";
			this.SetShortcut(this.toolStripSplitButtonSchedule, System.Windows.Forms.Keys.None);
			this.toolStripSplitButtonSchedule.Size = new System.Drawing.Size(103, 20);
			this.toolStripSplitButtonSchedule.Text = "xxSchedule";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripSplitButtonSchedule, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripSplitButtonSchedule, false);
			this.toolStripSplitButtonSchedule.ButtonClick += new System.EventHandler(this.toolStripMenuItemSchedule_Click);
			// 
			// toolStripMenuItemScheduleSelected
			// 
			this.toolStripMenuItemScheduleSelected.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Scheduler_ScheduleSelected;
			this.toolStripMenuItemScheduleSelected.Name = "toolStripMenuItemScheduleSelected";
			this.SetShortcut(this.toolStripMenuItemScheduleSelected, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemScheduleSelected.Size = new System.Drawing.Size(210, 22);
			this.toolStripMenuItemScheduleSelected.Text = "xxScheduleVerb";
			this.toolStripMenuItemScheduleSelected.Click += new System.EventHandler(this.toolStripMenuItemScheduleSelected_Click);
			// 
			// ToolStripMenuItemScheduleHourlyEmployees
			// 
			this.ToolStripMenuItemScheduleHourlyEmployees.Name = "ToolStripMenuItemScheduleHourlyEmployees";
			this.SetShortcut(this.ToolStripMenuItemScheduleHourlyEmployees, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemScheduleHourlyEmployees.Size = new System.Drawing.Size(210, 22);
			this.ToolStripMenuItemScheduleHourlyEmployees.Text = "xxScheduleHourlyEmployees";
			this.ToolStripMenuItemScheduleHourlyEmployees.Click += new System.EventHandler(this.ToolStripMenuItemScheduleHourlyEmployees_Click);
			// 
			// toolStripMenuItemReOptimize
			// 
			this.toolStripMenuItemReOptimize.Name = "toolStripMenuItemReOptimize";
			this.SetShortcut(this.toolStripMenuItemReOptimize, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemReOptimize.Size = new System.Drawing.Size(210, 22);
			this.toolStripMenuItemReOptimize.Text = "xxReOptimize";
			this.toolStripMenuItemReOptimize.Click += new System.EventHandler(this.toolStripMenuItemReOptimize_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.SetShortcut(this.toolStripSeparator2, System.Windows.Forms.Keys.None);
			this.toolStripSeparator2.Size = new System.Drawing.Size(207, 6);
			// 
			// toolStripMenuItemBackToLegalState
			// 
			this.toolStripMenuItemBackToLegalState.Name = "toolStripMenuItemBackToLegalState";
			this.SetShortcut(this.toolStripMenuItemBackToLegalState, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemBackToLegalState.Size = new System.Drawing.Size(210, 22);
			this.toolStripMenuItemBackToLegalState.Text = "xxBackToLegalState";
			this.toolStripMenuItemBackToLegalState.Click += new System.EventHandler(this.toolStripMenuItemBackToLegalState_Click);
			// 
			// toolStripDropDownButtonSwap
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripDropDownButtonSwap, "");
			this.toolStripDropDownButtonSwap.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemSwap,
            this.toolStripMenuItemSwapAndReschedule,
            this.ToolStripMenuItemSwapRaw});
			this.toolStripDropDownButtonSwap.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_SwapShifts_32x32;
			this.toolStripDropDownButtonSwap.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripDropDownButtonSwap.Name = "toolStripDropDownButtonSwap";
			this.SetShortcut(this.toolStripDropDownButtonSwap, System.Windows.Forms.Keys.None);
			this.toolStripDropDownButtonSwap.Size = new System.Drawing.Size(73, 20);
			this.toolStripDropDownButtonSwap.Text = "xxSwap";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripDropDownButtonSwap, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripDropDownButtonSwap, false);
			// 
			// toolStripMenuItemSwap
			// 
			this.toolStripMenuItemSwap.Name = "toolStripMenuItemSwap";
			this.SetShortcut(this.toolStripMenuItemSwap, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemSwap.Size = new System.Drawing.Size(187, 22);
			this.toolStripMenuItemSwap.Text = "xxSwap";
			this.toolStripMenuItemSwap.Click += new System.EventHandler(this.toolStripMenuItemSwap_Click);
			// 
			// toolStripMenuItemSwapAndReschedule
			// 
			this.toolStripMenuItemSwapAndReschedule.Name = "toolStripMenuItemSwapAndReschedule";
			this.SetShortcut(this.toolStripMenuItemSwapAndReschedule, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemSwapAndReschedule.Size = new System.Drawing.Size(187, 22);
			this.toolStripMenuItemSwapAndReschedule.Text = "xxSwapAndReschedule";
			this.toolStripMenuItemSwapAndReschedule.Click += new System.EventHandler(this.toolStripMenuItemSwapAndReschedule_Click);
			// 
			// ToolStripMenuItemSwapRaw
			// 
			this.ToolStripMenuItemSwapRaw.Name = "ToolStripMenuItemSwapRaw";
			this.SetShortcut(this.ToolStripMenuItemSwapRaw, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemSwapRaw.Size = new System.Drawing.Size(187, 22);
			this.ToolStripMenuItemSwapRaw.Text = "xxSwapRaw";
			this.ToolStripMenuItemSwapRaw.Click += new System.EventHandler(this.toolStripMenuItemSwapRawClick);
			// 
			// toolStripButtonRefresh
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonRefresh, "");
			this.toolStripButtonRefresh.Enabled = false;
			this.toolStripButtonRefresh.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Refresh;
			this.toolStripButtonRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonRefresh.Name = "toolStripButtonRefresh";
			this.SetShortcut(this.toolStripButtonRefresh, System.Windows.Forms.Keys.None);
			this.toolStripButtonRefresh.Size = new System.Drawing.Size(74, 20);
			this.toolStripButtonRefresh.Text = "xxRefresh";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonRefresh, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonRefresh, false);
			this.toolStripButtonRefresh.Click += new System.EventHandler(this.toolStripButtonRefresh_Click);
			// 
			// toolStripExLocks
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripExLocks, "");
			this.toolStripExLocks.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripExLocks.ForeColor = System.Drawing.Color.MidnightBlue;
			this.toolStripExLocks.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripExLocks.Image = null;
			this.toolStripExLocks.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripPanelItemLocks});
			this.toolStripExLocks.Location = new System.Drawing.Point(619, 1);
			this.toolStripExLocks.Name = "toolStripExLocks";
			this.toolStripExLocks.ShowCaption = true;
			this.toolStripExLocks.ShowLauncher = false;
			this.toolStripExLocks.Size = new System.Drawing.Size(105, 98);
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
			this.toolStripPanelItemLocks.Size = new System.Drawing.Size(96, 81);
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
			this.toolStripSplitButtonUnlock.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Unlock;
			this.toolStripSplitButtonUnlock.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripSplitButtonUnlock.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripSplitButtonUnlock.Name = "toolStripSplitButtonUnlock";
			this.SetShortcut(this.toolStripSplitButtonUnlock, System.Windows.Forms.Keys.None);
			this.toolStripSplitButtonUnlock.Size = new System.Drawing.Size(92, 20);
			this.toolStripSplitButtonUnlock.Text = "xxUnlock";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripSplitButtonUnlock, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripSplitButtonUnlock, false);
			this.toolStripSplitButtonUnlock.ButtonClick += new System.EventHandler(this.toolStripSplitButtonUnlock_ButtonClick);
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
			this.toolStripMenuItemUnlockSelection.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemUnlockSelectionRmMouseUp);
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
			this.toolStripSplitButtonLock.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Lock2;
			this.toolStripSplitButtonLock.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripSplitButtonLock.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripSplitButtonLock.Name = "toolStripSplitButtonLock";
			this.SetShortcut(this.toolStripSplitButtonLock, System.Windows.Forms.Keys.None);
			this.toolStripSplitButtonLock.Size = new System.Drawing.Size(82, 20);
			this.toolStripSplitButtonLock.Text = "xxLock";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripSplitButtonLock, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripSplitButtonLock, false);
			this.toolStripSplitButtonLock.ButtonClick += new System.EventHandler(this.toolStripSplitButtonLock_ButtonClick);
			// 
			// toolStripMenuItemLockSelection
			// 
			this.toolStripMenuItemLockSelection.Name = "toolStripMenuItemLockSelection";
			this.SetShortcut(this.toolStripMenuItemLockSelection, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemLockSelection.Size = new System.Drawing.Size(197, 22);
			this.toolStripMenuItemLockSelection.Text = "xxLockSelection";
			this.toolStripMenuItemLockSelection.Click += new System.EventHandler(this.toolStripMenuItemLockSelection_Click);
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
			this.ToolStripMenuItemLockAllRestrictions.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemLockAllRestrictionsMouseUp);
			// 
			// ToolStripMenuItemLockPreferences
			// 
			this.ToolStripMenuItemLockPreferences.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItemAllPreferences,
            this.ToolStripMenuItemAllAbsencePreference,
            this.ToolStripMenuItemAllDaysOff,
            this.ToolStripMenuItemAllShiftsPreferences,
            this.ToolStripMenuItemAllMustHave,
            this.ToolStripMenuItemAllFulFilledPreferences,
            this.ToolStripMenuItemAllFulFilledAbsencesPreferences,
            this.ToolStripMenuItemAllFulFilledDaysOffPreferences,
            this.ToolStripMenuItemAllFulFilledShiftsPreferences,
            this.ToolStripMenuItemAllFulfilledMustHave});
			this.ToolStripMenuItemLockPreferences.Name = "ToolStripMenuItemLockPreferences";
			this.SetShortcut(this.ToolStripMenuItemLockPreferences, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemLockPreferences.Size = new System.Drawing.Size(194, 22);
			this.ToolStripMenuItemLockPreferences.Text = "xxLockPreferences";
			// 
			// ToolStripMenuItemAllPreferences
			// 
			this.ToolStripMenuItemAllPreferences.Name = "ToolStripMenuItemAllPreferences";
			this.SetShortcut(this.ToolStripMenuItemAllPreferences, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAllPreferences.Size = new System.Drawing.Size(180, 22);
			this.ToolStripMenuItemAllPreferences.Text = "xxAll";
			this.ToolStripMenuItemAllPreferences.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemAllPreferencesMouseUp);
			// 
			// ToolStripMenuItemAllAbsencePreference
			// 
			this.ToolStripMenuItemAllAbsencePreference.Name = "ToolStripMenuItemAllAbsencePreference";
			this.SetShortcut(this.ToolStripMenuItemAllAbsencePreference, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAllAbsencePreference.Size = new System.Drawing.Size(180, 22);
			this.ToolStripMenuItemAllAbsencePreference.Text = "xxAllAbsences";
			this.ToolStripMenuItemAllAbsencePreference.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemAllAbsencePreferenceMouseUp);
			// 
			// ToolStripMenuItemAllDaysOff
			// 
			this.ToolStripMenuItemAllDaysOff.Name = "ToolStripMenuItemAllDaysOff";
			this.SetShortcut(this.ToolStripMenuItemAllDaysOff, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAllDaysOff.Size = new System.Drawing.Size(180, 22);
			this.ToolStripMenuItemAllDaysOff.Text = "xxAllDaysOff";
			this.ToolStripMenuItemAllDaysOff.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemAllDaysOffMouseUp);
			// 
			// ToolStripMenuItemAllShiftsPreferences
			// 
			this.ToolStripMenuItemAllShiftsPreferences.Name = "ToolStripMenuItemAllShiftsPreferences";
			this.SetShortcut(this.ToolStripMenuItemAllShiftsPreferences, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAllShiftsPreferences.Size = new System.Drawing.Size(180, 22);
			this.ToolStripMenuItemAllShiftsPreferences.Text = "xxAllShifts";
			this.ToolStripMenuItemAllShiftsPreferences.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemAllShiftsPreferencesMouseUp);
			// 
			// ToolStripMenuItemAllMustHave
			// 
			this.ToolStripMenuItemAllMustHave.Name = "ToolStripMenuItemAllMustHave";
			this.SetShortcut(this.ToolStripMenuItemAllMustHave, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAllMustHave.Size = new System.Drawing.Size(180, 22);
			this.ToolStripMenuItemAllMustHave.Text = "xxAllMustHave";
			this.ToolStripMenuItemAllMustHave.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemAllMustHaveMouseUp);
			// 
			// ToolStripMenuItemAllFulFilledPreferences
			// 
			this.ToolStripMenuItemAllFulFilledPreferences.Name = "ToolStripMenuItemAllFulFilledPreferences";
			this.SetShortcut(this.ToolStripMenuItemAllFulFilledPreferences, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAllFulFilledPreferences.Size = new System.Drawing.Size(180, 22);
			this.ToolStripMenuItemAllFulFilledPreferences.Text = "xxAllFulFilled";
			this.ToolStripMenuItemAllFulFilledPreferences.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemAllFulFilledPreferencesMouseUp);
			// 
			// ToolStripMenuItemAllFulFilledAbsencesPreferences
			// 
			this.ToolStripMenuItemAllFulFilledAbsencesPreferences.Name = "ToolStripMenuItemAllFulFilledAbsencesPreferences";
			this.SetShortcut(this.ToolStripMenuItemAllFulFilledAbsencesPreferences, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAllFulFilledAbsencesPreferences.Size = new System.Drawing.Size(180, 22);
			this.ToolStripMenuItemAllFulFilledAbsencesPreferences.Text = "xxAllFulFilledAbsences";
			this.ToolStripMenuItemAllFulFilledAbsencesPreferences.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemAllAbsencePreferenceMouseUp);
			// 
			// ToolStripMenuItemAllFulFilledDaysOffPreferences
			// 
			this.ToolStripMenuItemAllFulFilledDaysOffPreferences.Name = "ToolStripMenuItemAllFulFilledDaysOffPreferences";
			this.SetShortcut(this.ToolStripMenuItemAllFulFilledDaysOffPreferences, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAllFulFilledDaysOffPreferences.Size = new System.Drawing.Size(180, 22);
			this.ToolStripMenuItemAllFulFilledDaysOffPreferences.Text = "xxAllFulFilledDaysOff";
			this.ToolStripMenuItemAllFulFilledDaysOffPreferences.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemAllFulFilledDaysOffPreferencesMouseUp);
			// 
			// ToolStripMenuItemAllFulFilledShiftsPreferences
			// 
			this.ToolStripMenuItemAllFulFilledShiftsPreferences.Name = "ToolStripMenuItemAllFulFilledShiftsPreferences";
			this.SetShortcut(this.ToolStripMenuItemAllFulFilledShiftsPreferences, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAllFulFilledShiftsPreferences.Size = new System.Drawing.Size(180, 22);
			this.ToolStripMenuItemAllFulFilledShiftsPreferences.Text = "xxAllFulFilledShifts";
			this.ToolStripMenuItemAllFulFilledShiftsPreferences.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemAllFulFilledShiftsPreferencesMouseUp);
			// 
			// ToolStripMenuItemAllFulfilledMustHave
			// 
			this.ToolStripMenuItemAllFulfilledMustHave.Name = "ToolStripMenuItemAllFulfilledMustHave";
			this.SetShortcut(this.ToolStripMenuItemAllFulfilledMustHave, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAllFulfilledMustHave.Size = new System.Drawing.Size(180, 22);
			this.ToolStripMenuItemAllFulfilledMustHave.Text = "xxAllFulfilledMustHave";
			this.ToolStripMenuItemAllFulfilledMustHave.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemAllFulfilledMustHaveMouseUp);
			// 
			// ToolStripMenuItemLockRotations
			// 
			this.ToolStripMenuItemLockRotations.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItemAllRotations,
            this.ToolStripMenuItemAllDaysOffRotations,
            this.ToolStripMenuItemAllFulFilledRotations,
            this.ToolStripMenuItemAllFulFilledDaysOffRotations,
            this.ToolStripMenuItemAllFulFilledShiftsRotations,
            this.ToolStripMenuItemAllShiftsRotations});
			this.ToolStripMenuItemLockRotations.Name = "ToolStripMenuItemLockRotations";
			this.SetShortcut(this.ToolStripMenuItemLockRotations, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemLockRotations.Size = new System.Drawing.Size(194, 22);
			this.ToolStripMenuItemLockRotations.Text = "xxLockRotations";
			// 
			// ToolStripMenuItemAllRotations
			// 
			this.ToolStripMenuItemAllRotations.Name = "ToolStripMenuItemAllRotations";
			this.SetShortcut(this.ToolStripMenuItemAllRotations, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAllRotations.Size = new System.Drawing.Size(171, 22);
			this.ToolStripMenuItemAllRotations.Text = "xxAll";
			this.ToolStripMenuItemAllRotations.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemAllRotationsMouseUp);
			// 
			// ToolStripMenuItemAllDaysOffRotations
			// 
			this.ToolStripMenuItemAllDaysOffRotations.Name = "ToolStripMenuItemAllDaysOffRotations";
			this.SetShortcut(this.ToolStripMenuItemAllDaysOffRotations, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAllDaysOffRotations.Size = new System.Drawing.Size(171, 22);
			this.ToolStripMenuItemAllDaysOffRotations.Text = "xxAllDaysOff";
			this.ToolStripMenuItemAllDaysOffRotations.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemAllDaysOffRotationsMouseUp);
			// 
			// ToolStripMenuItemAllFulFilledRotations
			// 
			this.ToolStripMenuItemAllFulFilledRotations.Name = "ToolStripMenuItemAllFulFilledRotations";
			this.SetShortcut(this.ToolStripMenuItemAllFulFilledRotations, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAllFulFilledRotations.Size = new System.Drawing.Size(171, 22);
			this.ToolStripMenuItemAllFulFilledRotations.Text = "xxAllFulFilled";
			this.ToolStripMenuItemAllFulFilledRotations.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemAllFulFilledRotationsMouseUp);
			// 
			// ToolStripMenuItemAllFulFilledDaysOffRotations
			// 
			this.ToolStripMenuItemAllFulFilledDaysOffRotations.Name = "ToolStripMenuItemAllFulFilledDaysOffRotations";
			this.SetShortcut(this.ToolStripMenuItemAllFulFilledDaysOffRotations, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAllFulFilledDaysOffRotations.Size = new System.Drawing.Size(171, 22);
			this.ToolStripMenuItemAllFulFilledDaysOffRotations.Text = "xxAllFulFilledDaysOff";
			this.ToolStripMenuItemAllFulFilledDaysOffRotations.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemAllFulFilledDaysOffRotationsMouseUp);
			// 
			// ToolStripMenuItemAllFulFilledShiftsRotations
			// 
			this.ToolStripMenuItemAllFulFilledShiftsRotations.Name = "ToolStripMenuItemAllFulFilledShiftsRotations";
			this.SetShortcut(this.ToolStripMenuItemAllFulFilledShiftsRotations, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAllFulFilledShiftsRotations.Size = new System.Drawing.Size(171, 22);
			this.ToolStripMenuItemAllFulFilledShiftsRotations.Text = "xxAllFulFilledShifts";
			this.ToolStripMenuItemAllFulFilledShiftsRotations.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemAllFulFilledShiftsRotationsMouseUp);
			// 
			// ToolStripMenuItemAllShiftsRotations
			// 
			this.ToolStripMenuItemAllShiftsRotations.Name = "ToolStripMenuItemAllShiftsRotations";
			this.SetShortcut(this.ToolStripMenuItemAllShiftsRotations, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAllShiftsRotations.Size = new System.Drawing.Size(171, 22);
			this.ToolStripMenuItemAllShiftsRotations.Text = "xxAllShifts";
			this.ToolStripMenuItemAllShiftsRotations.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemAllShiftsRotationsMouseUp);
			// 
			// ToolStripMenuItemLockStudentAvailability
			// 
			this.ToolStripMenuItemLockStudentAvailability.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItemAllUnavailableStudentAvailability,
            this.ToolStripMenuItemAllAvailableStudentAvailability,
            this.ToolStripMenuItemAllFulFilledStudentAvailability});
			this.ToolStripMenuItemLockStudentAvailability.Name = "ToolStripMenuItemLockStudentAvailability";
			this.SetShortcut(this.ToolStripMenuItemLockStudentAvailability, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemLockStudentAvailability.Size = new System.Drawing.Size(194, 22);
			this.ToolStripMenuItemLockStudentAvailability.Text = "xxLockStudentAvailability";
			// 
			// ToolStripMenuItemAllUnavailableStudentAvailability
			// 
			this.ToolStripMenuItemAllUnavailableStudentAvailability.Name = "ToolStripMenuItemAllUnavailableStudentAvailability";
			this.SetShortcut(this.ToolStripMenuItemAllUnavailableStudentAvailability, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAllUnavailableStudentAvailability.Size = new System.Drawing.Size(151, 22);
			this.ToolStripMenuItemAllUnavailableStudentAvailability.Text = "xxAllUnavailable";
			this.ToolStripMenuItemAllUnavailableStudentAvailability.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemAllUnavailableStudentAvailabilityMouseUp);
			// 
			// ToolStripMenuItemAllAvailableStudentAvailability
			// 
			this.ToolStripMenuItemAllAvailableStudentAvailability.Name = "ToolStripMenuItemAllAvailableStudentAvailability";
			this.SetShortcut(this.ToolStripMenuItemAllAvailableStudentAvailability, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAllAvailableStudentAvailability.Size = new System.Drawing.Size(151, 22);
			this.ToolStripMenuItemAllAvailableStudentAvailability.Text = "xxAllAvailable";
			this.ToolStripMenuItemAllAvailableStudentAvailability.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemAllAvailableStudentAvailabilityMouseUp);
			// 
			// ToolStripMenuItemAllFulFilledStudentAvailability
			// 
			this.ToolStripMenuItemAllFulFilledStudentAvailability.Name = "ToolStripMenuItemAllFulFilledStudentAvailability";
			this.SetShortcut(this.ToolStripMenuItemAllFulFilledStudentAvailability, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAllFulFilledStudentAvailability.Size = new System.Drawing.Size(151, 22);
			this.ToolStripMenuItemAllFulFilledStudentAvailability.Text = "xxAllFulFilled";
			this.ToolStripMenuItemAllFulFilledStudentAvailability.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemAllFulFilledStudentAvailabilityMouseUp);
			// 
			// ToolStripMenuItemLockAvailability
			// 
			this.ToolStripMenuItemLockAvailability.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItemAllUnavailableAvailability,
            this.ToolStripMenuItemAllAvailableAvailability,
            this.ToolStripMenuItemAllFulFilledAvailability});
			this.ToolStripMenuItemLockAvailability.Name = "ToolStripMenuItemLockAvailability";
			this.SetShortcut(this.ToolStripMenuItemLockAvailability, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemLockAvailability.Size = new System.Drawing.Size(194, 22);
			this.ToolStripMenuItemLockAvailability.Text = "xxLockAvailability";
			// 
			// ToolStripMenuItemAllUnavailableAvailability
			// 
			this.ToolStripMenuItemAllUnavailableAvailability.Name = "ToolStripMenuItemAllUnavailableAvailability";
			this.SetShortcut(this.ToolStripMenuItemAllUnavailableAvailability, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAllUnavailableAvailability.Size = new System.Drawing.Size(151, 22);
			this.ToolStripMenuItemAllUnavailableAvailability.Text = "xxAllUnavailable";
			this.ToolStripMenuItemAllUnavailableAvailability.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemAllUnavailableAvailabilityMouseUp);
			// 
			// ToolStripMenuItemAllAvailableAvailability
			// 
			this.ToolStripMenuItemAllAvailableAvailability.Name = "ToolStripMenuItemAllAvailableAvailability";
			this.SetShortcut(this.ToolStripMenuItemAllAvailableAvailability, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAllAvailableAvailability.Size = new System.Drawing.Size(151, 22);
			this.ToolStripMenuItemAllAvailableAvailability.Text = "xxAllAvailable";
			this.ToolStripMenuItemAllAvailableAvailability.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemAllAvailableAvailabilityMouseUp);
			// 
			// ToolStripMenuItemAllFulFilledAvailability
			// 
			this.ToolStripMenuItemAllFulFilledAvailability.Name = "ToolStripMenuItemAllFulFilledAvailability";
			this.SetShortcut(this.ToolStripMenuItemAllFulFilledAvailability, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemAllFulFilledAvailability.Size = new System.Drawing.Size(151, 22);
			this.ToolStripMenuItemAllFulFilledAvailability.Text = "xxAllFulFilled";
			this.ToolStripMenuItemAllFulFilledAvailability.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolStripMenuItemAllFulFilledAvailabilityMouseUp);
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
			this.toolStripMenuItemWriteProtectSchedule2.Click += new System.EventHandler(this.toolStripMenuItemWriteProtectSchedule2_Click);
			// 
			// ToolStripMenuItemRemoveWriteProtectionToolBar
			// 
			this.ToolStripMenuItemRemoveWriteProtectionToolBar.Name = "ToolStripMenuItemRemoveWriteProtectionToolBar";
			this.SetShortcut(this.ToolStripMenuItemRemoveWriteProtectionToolBar, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemRemoveWriteProtectionToolBar.Size = new System.Drawing.Size(197, 22);
			this.ToolStripMenuItemRemoveWriteProtectionToolBar.Text = "xxRemoveWriteProtection";
			this.ToolStripMenuItemRemoveWriteProtectionToolBar.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ToolstripMenuRemoveWriteProtectionMouseUp);
			// 
			// toolStripExTags
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripExTags, "");
			this.toolStripExTags.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripExTags.ForeColor = System.Drawing.Color.MidnightBlue;
			this.toolStripExTags.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripExTags.Image = null;
			this.toolStripExTags.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripPanelItemTags});
			this.toolStripExTags.Location = new System.Drawing.Point(726, 1);
			this.toolStripExTags.Name = "toolStripExTags";
			this.toolStripExTags.ShowLauncher = false;
			this.toolStripExTags.Size = new System.Drawing.Size(145, 98);
			this.toolStripExTags.TabIndex = 15;
			this.toolStripExTags.Text = "xxTags";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripExTags, false);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripExTags, false);
			this.toolStripExTags.SizeChanged += new System.EventHandler(this.toolStripExTags_SizeChanged);
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
			this.toolStripPanelItemTags.Size = new System.Drawing.Size(136, 81);
			this.toolStripPanelItemTags.Text = "toolStripPanelItem2";
			this.toolStripPanelItemTags.Transparent = true;
			// 
			// toolStripLabelAutoTag
			// 
			this.toolStripLabelAutoTag.Name = "toolStripLabelAutoTag";
			this.SetShortcut(this.toolStripLabelAutoTag, System.Windows.Forms.Keys.None);
			this.toolStripLabelAutoTag.Size = new System.Drawing.Size(63, 15);
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
			this.toolStripComboBoxAutoTag.SelectedIndexChanged += new System.EventHandler(this.toolStripComboBoxAutoTag_SelectedIndexChanged);
			// 
			// toolStripSplitButtonChangeTag
			// 
			this.toolStripSplitButtonChangeTag.AutoSize = false;
			this.ribbonControlAdv1.SetDescription(this.toolStripSplitButtonChangeTag, "");
			this.toolStripSplitButtonChangeTag.Image = global::Teleopti.Ccc.Win.Properties.Resources.tag_blue;
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
			this.toolStripEx1.ForeColor = System.Drawing.Color.MidnightBlue;
			this.toolStripEx1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripEx1.Image = null;
			this.toolStripEx1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripPanelItem1});
			this.toolStripEx1.Location = new System.Drawing.Point(873, 1);
			this.toolStripEx1.MinimumSize = new System.Drawing.Size(0, 90);
			this.toolStripEx1.Name = "toolStripEx1";
			this.toolStripEx1.ShowLauncher = false;
			this.toolStripEx1.Size = new System.Drawing.Size(107, 98);
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
            this.toolStripButtonShowEditor});
			this.toolStripPanelItem1.Name = "toolStripPanelItem1";
			this.SetShortcut(this.toolStripPanelItem1, System.Windows.Forms.Keys.None);
			this.toolStripPanelItem1.Size = new System.Drawing.Size(98, 81);
			this.toolStripPanelItem1.Text = "toolStripPanelItem1";
			this.toolStripPanelItem1.Transparent = true;
			// 
			// toolStripButtonShowGraph
			// 
			this.toolStripButtonShowGraph.Checked = true;
			this.toolStripButtonShowGraph.CheckState = System.Windows.Forms.CheckState.Checked;
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonShowGraph, "");
			this.toolStripButtonShowGraph.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Realtime_adherence_16x16;
			this.toolStripButtonShowGraph.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonShowGraph.Name = "toolStripButtonShowGraph";
			this.SetShortcut(this.toolStripButtonShowGraph, System.Windows.Forms.Keys.None);
			this.toolStripButtonShowGraph.Size = new System.Drawing.Size(93, 20);
			this.toolStripButtonShowGraph.Text = "xxShowGraph";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonShowGraph, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonShowGraph, false);
			this.toolStripButtonShowGraph.Click += new System.EventHandler(this.toolStripButtonShowGraph_Click);
			// 
			// toolStripButtonShowResult
			// 
			this.toolStripButtonShowResult.Checked = true;
			this.toolStripButtonShowResult.CheckState = System.Windows.Forms.CheckState.Checked;
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonShowResult, "");
			this.toolStripButtonShowResult.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_result_view_16x16;
			this.toolStripButtonShowResult.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonShowResult.Name = "toolStripButtonShowResult";
			this.SetShortcut(this.toolStripButtonShowResult, System.Windows.Forms.Keys.None);
			this.toolStripButtonShowResult.Size = new System.Drawing.Size(94, 20);
			this.toolStripButtonShowResult.Text = "xxShowResult";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonShowResult, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonShowResult, false);
			this.toolStripButtonShowResult.Click += new System.EventHandler(this.toolStripButtonShowResult_Click);
			// 
			// toolStripButtonShowEditor
			// 
			this.toolStripButtonShowEditor.Checked = true;
			this.toolStripButtonShowEditor.CheckState = System.Windows.Forms.CheckState.Checked;
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonShowEditor, "");
			this.toolStripButtonShowEditor.Image = global::Teleopti.Ccc.Win.Properties.Resources.editor;
			this.toolStripButtonShowEditor.ImageTransparentColor = System.Drawing.Color.White;
			this.toolStripButtonShowEditor.Name = "toolStripButtonShowEditor";
			this.SetShortcut(this.toolStripButtonShowEditor, System.Windows.Forms.Keys.None);
			this.toolStripButtonShowEditor.Size = new System.Drawing.Size(91, 20);
			this.toolStripButtonShowEditor.Text = "xxShowEditor";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonShowEditor, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonShowEditor, false);
			this.toolStripButtonShowEditor.Click += new System.EventHandler(this.toolStripButtonShowEditor_Click);
			// 
			// toolStripExLoadOptions
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripExLoadOptions, "");
			this.toolStripExLoadOptions.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripExLoadOptions.ForeColor = System.Drawing.Color.MidnightBlue;
			this.toolStripExLoadOptions.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripExLoadOptions.Image = null;
			this.toolStripExLoadOptions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripPanelItemLoadOptions});
			this.toolStripExLoadOptions.Location = new System.Drawing.Point(982, 1);
			this.toolStripExLoadOptions.Name = "toolStripExLoadOptions";
			this.toolStripExLoadOptions.ShowCaption = true;
			this.toolStripExLoadOptions.ShowLauncher = false;
			this.toolStripExLoadOptions.Size = new System.Drawing.Size(115, 98);
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
			this.toolStripPanelItemLoadOptions.Size = new System.Drawing.Size(106, 81);
			this.toolStripPanelItemLoadOptions.Transparent = true;
			// 
			// toolStripButtonShrinkage
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonShrinkage, "");
			this.toolStripButtonShrinkage.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_shrinkage;
			this.toolStripButtonShrinkage.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonShrinkage.Name = "toolStripButtonShrinkage";
			this.SetShortcut(this.toolStripButtonShrinkage, System.Windows.Forms.Keys.None);
			this.toolStripButtonShrinkage.Size = new System.Drawing.Size(89, 20);
			this.toolStripButtonShrinkage.Text = "xxShrinkage";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonShrinkage, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonShrinkage, false);
			this.toolStripButtonShrinkage.Click += new System.EventHandler(this.toolStripButtonShrinkage_Click);
			// 
			// toolStripButtonCalculation
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonCalculation, "");
			this.toolStripButtonCalculation.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_calculation;
			this.toolStripButtonCalculation.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonCalculation.Name = "toolStripButtonCalculation";
			this.SetShortcut(this.toolStripButtonCalculation, System.Windows.Forms.Keys.None);
			this.toolStripButtonCalculation.Size = new System.Drawing.Size(102, 20);
			this.toolStripButtonCalculation.Text = "xxCalculations";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonCalculation, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonCalculation, false);
			this.toolStripButtonCalculation.Click += new System.EventHandler(this.toolStripButtonCalculation_Click);
			// 
			// toolStripButtonValidation
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonValidation, "");
			this.toolStripButtonValidation.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_validation;
			this.toolStripButtonValidation.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonValidation.Name = "toolStripButtonValidation";
			this.SetShortcut(this.toolStripButtonValidation, System.Windows.Forms.Keys.None);
			this.toolStripButtonValidation.Size = new System.Drawing.Size(95, 20);
			this.toolStripButtonValidation.Text = "xxValidations";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonValidation, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonValidation, false);
			this.toolStripButtonValidation.Click += new System.EventHandler(this.toolStripButtonValidation_Click);
			// 
			// toolStripTabItemChart
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripTabItemChart, "");
			this.toolStripTabItemChart.Name = "toolStripTabItemChart";
			// 
			// ribbonControlAdv1.ribbonPanel2
			// 
			this.toolStripTabItemChart.Panel.Controls.Add(this.toolStripExGridRowInChartButtons);
			this.toolStripTabItemChart.Panel.Controls.Add(this.toolStripExSkillViews);
			this.toolStripTabItemChart.Panel.Name = "ribbonPanel2";
			this.toolStripTabItemChart.Panel.ScrollPosition = 0;
			this.toolStripTabItemChart.Panel.TabIndex = 6;
			this.toolStripTabItemChart.Panel.Text = "xxChart";
			this.toolStripTabItemChart.Position = 1;
			this.SetShortcut(this.toolStripTabItemChart, System.Windows.Forms.Keys.None);
			this.toolStripTabItemChart.Size = new System.Drawing.Size(48, 19);
			this.toolStripTabItemChart.Text = "xxChart";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripTabItemChart, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripTabItemChart, false);
			this.toolStripTabItemChart.Click += new System.EventHandler(this.toolStripTabItemChart_Click);
			// 
			// toolStripExGridRowInChartButtons
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripExGridRowInChartButtons, "");
			this.toolStripExGridRowInChartButtons.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripExGridRowInChartButtons.ForeColor = System.Drawing.Color.MidnightBlue;
			this.toolStripExGridRowInChartButtons.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripExGridRowInChartButtons.Image = null;
			this.toolStripExGridRowInChartButtons.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonGridInChart});
			this.toolStripExGridRowInChartButtons.Location = new System.Drawing.Point(0, 1);
			this.toolStripExGridRowInChartButtons.Name = "toolStripExGridRowInChartButtons";
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
			this.toolStripButtonGridInChart.Click += new System.EventHandler(this.toolStripButtonGridInChart_Click);
			// 
			// toolStripExSkillViews
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripExSkillViews, "");
			this.toolStripExSkillViews.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripExSkillViews.ForeColor = System.Drawing.Color.MidnightBlue;
			this.toolStripExSkillViews.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripExSkillViews.Image = null;
			this.toolStripExSkillViews.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripPanelItem2});
			this.toolStripExSkillViews.Location = new System.Drawing.Point(91, 1);
			this.toolStripExSkillViews.Name = "toolStripExSkillViews";
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
			this.toolStripButtonChartPeriodView.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_schedule_Summary_view_16x16;
			this.toolStripButtonChartPeriodView.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonChartPeriodView.Name = "toolStripButtonChartPeriodView";
			this.SetShortcut(this.toolStripButtonChartPeriodView, System.Windows.Forms.Keys.None);
			this.toolStripButtonChartPeriodView.Size = new System.Drawing.Size(71, 20);
			this.toolStripButtonChartPeriodView.Text = "xxPeriod";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonChartPeriodView, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonChartPeriodView, false);
			this.toolStripButtonChartPeriodView.Click += new System.EventHandler(this.toolStripButtonChartPeriodView_Click);
			// 
			// toolStripButtonChartMonthView
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonChartMonthView, "");
			this.toolStripButtonChartMonthView.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_schedule_Period_view_16x16;
			this.toolStripButtonChartMonthView.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonChartMonthView.Name = "toolStripButtonChartMonthView";
			this.SetShortcut(this.toolStripButtonChartMonthView, System.Windows.Forms.Keys.None);
			this.toolStripButtonChartMonthView.Size = new System.Drawing.Size(73, 20);
			this.toolStripButtonChartMonthView.Text = "xxMonth";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonChartMonthView, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonChartMonthView, false);
			this.toolStripButtonChartMonthView.Click += new System.EventHandler(this.toolStripButtonChartPeriodView_Click);
			// 
			// toolStripButtonChartWeekView
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonChartWeekView, "");
			this.toolStripButtonChartWeekView.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_schedule_DetailView_16x16;
			this.toolStripButtonChartWeekView.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonChartWeekView.Name = "toolStripButtonChartWeekView";
			this.SetShortcut(this.toolStripButtonChartWeekView, System.Windows.Forms.Keys.None);
			this.toolStripButtonChartWeekView.Size = new System.Drawing.Size(66, 20);
			this.toolStripButtonChartWeekView.Text = "xxWeek";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonChartWeekView, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonChartWeekView, false);
			this.toolStripButtonChartWeekView.Click += new System.EventHandler(this.toolStripButtonChartPeriodView_Click);
			// 
			// toolStripButtonChartDayView
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonChartDayView, "");
			this.toolStripButtonChartDayView.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_schedule_Weekview_16x16;
			this.toolStripButtonChartDayView.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonChartDayView.Name = "toolStripButtonChartDayView";
			this.SetShortcut(this.toolStripButtonChartDayView, System.Windows.Forms.Keys.None);
			this.toolStripButtonChartDayView.Size = new System.Drawing.Size(57, 20);
			this.toolStripButtonChartDayView.Text = "xxDay";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonChartDayView, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonChartDayView, false);
			this.toolStripButtonChartDayView.Click += new System.EventHandler(this.toolStripButtonChartPeriodView_Click);
			// 
			// toolStripButtonChartIntradayView
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonChartIntradayView, "");
			this.toolStripButtonChartIntradayView.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_schedule_IntradayView_16x16;
			this.toolStripButtonChartIntradayView.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonChartIntradayView.Name = "toolStripButtonChartIntradayView";
			this.SetShortcut(this.toolStripButtonChartIntradayView, System.Windows.Forms.Keys.None);
			this.toolStripButtonChartIntradayView.Size = new System.Drawing.Size(80, 20);
			this.toolStripButtonChartIntradayView.Text = "xxIntraday";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonChartIntradayView, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonChartIntradayView, false);
			this.toolStripButtonChartIntradayView.Click += new System.EventHandler(this.toolStripButtonChartPeriodView_Click);
			// 
			// toolStripTabItem1
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripTabItem1, "");
			this.toolStripTabItem1.Name = "toolStripTabItem1";
			// 
			// ribbonControlAdv1.ribbonPanel3
			// 
			this.toolStripTabItem1.Panel.Controls.Add(this.toolStripEx2);
			this.toolStripTabItem1.Panel.Controls.Add(this.toolStripExHandleRequests);
			this.toolStripTabItem1.Panel.Controls.Add(this.toolStripEx3);
			this.toolStripTabItem1.Panel.Name = "ribbonPanel3";
			this.toolStripTabItem1.Panel.ScrollPosition = 0;
			this.toolStripTabItem1.Panel.TabIndex = 7;
			this.toolStripTabItem1.Panel.Text = "xxRequests";
			this.toolStripTabItem1.Position = 2;
			this.SetShortcut(this.toolStripTabItem1, System.Windows.Forms.Keys.None);
			this.toolStripTabItem1.Size = new System.Drawing.Size(68, 19);
			this.toolStripTabItem1.Text = "xxRequests";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripTabItem1, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripTabItem1, false);
			this.toolStripTabItem1.Click += new System.EventHandler(this.toolStripTabItem1_Click);
			// 
			// toolStripEx2
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripEx2, "");
			this.toolStripEx2.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripEx2.ForeColor = System.Drawing.Color.MidnightBlue;
			this.toolStripEx2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripEx2.Image = null;
			this.toolStripEx2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonViewDetails,
            this.toolStripButtonViewAllowance,
            this.toolStripButtonViewRequestHistory});
			this.toolStripEx2.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
			this.toolStripEx2.Location = new System.Drawing.Point(0, 1);
			this.toolStripEx2.Name = "toolStripEx2";
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
			this.toolStripButtonViewDetails.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_schedule_DetailView_32x321;
			this.toolStripButtonViewDetails.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonViewDetails.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonViewDetails.Name = "toolStripButtonViewDetails";
			this.SetShortcut(this.toolStripButtonViewDetails, System.Windows.Forms.Keys.None);
			this.toolStripButtonViewDetails.Size = new System.Drawing.Size(81, 0);
			this.toolStripButtonViewDetails.Text = "xxViewDetails";
			this.toolStripButtonViewDetails.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonViewDetails, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonViewDetails, false);
			this.toolStripButtonViewDetails.Click += new System.EventHandler(this.ToolStripMenuItemViewDetails_Click);
			// 
			// toolStripButtonViewAllowance
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonViewAllowance, "");
			this.toolStripButtonViewAllowance.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Advanced_settings2;
			this.toolStripButtonViewAllowance.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonViewAllowance.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonViewAllowance.Name = "toolStripButtonViewAllowance";
			this.SetShortcut(this.toolStripButtonViewAllowance, System.Windows.Forms.Keys.None);
			this.toolStripButtonViewAllowance.Size = new System.Drawing.Size(101, 0);
			this.toolStripButtonViewAllowance.Text = "xxViewAllowance";
			this.toolStripButtonViewAllowance.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonViewAllowance, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonViewAllowance, false);
			this.toolStripButtonViewAllowance.Click += new System.EventHandler(this.toolStripButtonViewAllowance_Click);
			// 
			// toolStripButtonViewRequestHistory
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonViewRequestHistory, "");
			this.toolStripButtonViewRequestHistory.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Agent_Request_OK_32x32;
			this.toolStripButtonViewRequestHistory.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonViewRequestHistory.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonViewRequestHistory.Name = "toolStripButtonViewRequestHistory";
			this.SetShortcut(this.toolStripButtonViewRequestHistory, System.Windows.Forms.Keys.None);
			this.toolStripButtonViewRequestHistory.Size = new System.Drawing.Size(84, 0);
			this.toolStripButtonViewRequestHistory.Text = "xxViewHistory";
			this.toolStripButtonViewRequestHistory.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonViewRequestHistory, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonViewRequestHistory, false);
			this.toolStripButtonViewRequestHistory.Click += new System.EventHandler(this.toolStripViewRequestHistory_Click);
			// 
			// toolStripExHandleRequests
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripExHandleRequests, "");
			this.toolStripExHandleRequests.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripExHandleRequests.ForeColor = System.Drawing.Color.MidnightBlue;
			this.toolStripExHandleRequests.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripExHandleRequests.Image = null;
			this.toolStripExHandleRequests.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonApproveRequest,
            this.toolStripButtonDenyRequest,
            this.toolStripButtonEditNote,
            this.toolStripButtonReplyAndApprove,
            this.toolStripButtonReplyAndDeny});
			this.toolStripExHandleRequests.Location = new System.Drawing.Point(275, 1);
			this.toolStripExHandleRequests.Name = "toolStripExHandleRequests";
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
			this.toolStripButtonApproveRequest.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Agent_NewRequest_32x32;
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
			this.toolStripButtonDenyRequest.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Agent_denyRequest_32x32;
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
			this.toolStripButtonEditNote.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Agent_ContinueDialogue_32x32;
			this.toolStripButtonEditNote.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonEditNote.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonEditNote.Name = "toolStripButtonEditNote";
			this.SetShortcut(this.toolStripButtonEditNote, System.Windows.Forms.Keys.None);
			this.toolStripButtonEditNote.Size = new System.Drawing.Size(50, 0);
			this.toolStripButtonEditNote.Text = "xxReply";
			this.toolStripButtonEditNote.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonEditNote, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonEditNote, false);
			this.toolStripButtonEditNote.Click += new System.EventHandler(this.toolStripButtonEditNote_Click);
			// 
			// toolStripButtonReplyAndApprove
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonReplyAndApprove, "");
			this.toolStripButtonReplyAndApprove.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Agent_NewRequest_32x32;
			this.toolStripButtonReplyAndApprove.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonReplyAndApprove.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonReplyAndApprove.Name = "toolStripButtonReplyAndApprove";
			this.SetShortcut(this.toolStripButtonReplyAndApprove, System.Windows.Forms.Keys.None);
			this.toolStripButtonReplyAndApprove.Size = new System.Drawing.Size(117, 0);
			this.toolStripButtonReplyAndApprove.Text = "xxReplyAndApprove";
			this.toolStripButtonReplyAndApprove.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonReplyAndApprove, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonReplyAndApprove, false);
			this.toolStripButtonReplyAndApprove.Click += new System.EventHandler(this.toolStripButtonReplyAndApprove_Click);
			// 
			// toolStripButtonReplyAndDeny
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonReplyAndDeny, "");
			this.toolStripButtonReplyAndDeny.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Agent_denyRequest_32x32;
			this.toolStripButtonReplyAndDeny.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonReplyAndDeny.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonReplyAndDeny.Name = "toolStripButtonReplyAndDeny";
			this.SetShortcut(this.toolStripButtonReplyAndDeny, System.Windows.Forms.Keys.None);
			this.toolStripButtonReplyAndDeny.Size = new System.Drawing.Size(99, 0);
			this.toolStripButtonReplyAndDeny.Text = "xxReplyAndDeny";
			this.toolStripButtonReplyAndDeny.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonReplyAndDeny, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonReplyAndDeny, false);
			this.toolStripButtonReplyAndDeny.Click += new System.EventHandler(this.toolStripButtonReplyAndDeny_Click);
			// 
			// toolStripEx3
			// 
			this.toolStripEx3.AutoSize = false;
			this.ribbonControlAdv1.SetDescription(this.toolStripEx3, "");
			this.toolStripEx3.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripEx3.ForeColor = System.Drawing.Color.MidnightBlue;
			this.toolStripEx3.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripEx3.Image = null;
			this.toolStripEx3.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripTextBoxFilter,
            this.toolStripButtonFindRequest});
			this.toolStripEx3.Location = new System.Drawing.Point(664, 1);
			this.toolStripEx3.Name = "toolStripEx3";
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
			this.toolStripButtonFindRequest.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Filter;
			this.toolStripButtonFindRequest.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonFindRequest.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonFindRequest.Name = "toolStripButtonFindRequest";
			this.SetShortcut(this.toolStripButtonFindRequest, System.Windows.Forms.Keys.None);
			this.toolStripButtonFindRequest.Size = new System.Drawing.Size(36, 0);
			this.toolStripButtonFindRequest.Text = "toolStripButtonFilterRequest";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonFindRequest, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonFindRequest, false);
			// 
			// toolStripTabItemQuickAccess
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripTabItemQuickAccess, "");
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
			this.toolStripTabItemQuickAccess.Size = new System.Drawing.Size(101, 19);
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
			this.toolStripExForQuickAccessItems.ForeColor = System.Drawing.Color.MidnightBlue;
			this.toolStripExForQuickAccessItems.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripExForQuickAccessItems.Image = null;
			this.toolStripExForQuickAccessItems.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonQuickAccessSave,
            this.toolStripSeparatorQuickAccess,
            this.toolStripButtonQuickAccessCancel,
            this.toolStripButtonQuickAccessRedo,
            this.toolStripSplitButtonQuickAccessUndo,
            this.toolStripButtonShowTexts});
			this.toolStripExForQuickAccessItems.Location = new System.Drawing.Point(0, 1);
			this.toolStripExForQuickAccessItems.Name = "toolStripExForQuickAccessItems";
			this.toolStripExForQuickAccessItems.Size = new System.Drawing.Size(265, 98);
			this.toolStripExForQuickAccessItems.TabIndex = 2;
			this.toolStripExForQuickAccessItems.Text = "xxQuickAccess";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripExForQuickAccessItems, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripExForQuickAccessItems, false);
			this.toolStripExForQuickAccessItems.Visible = false;
			// 
			// toolStripButtonQuickAccessSave
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonQuickAccessSave, "");
			this.toolStripButtonQuickAccessSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonQuickAccessSave.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_save_small;
			this.toolStripButtonQuickAccessSave.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonQuickAccessSave.Name = "toolStripButtonQuickAccessSave";
			this.SetShortcut(this.toolStripButtonQuickAccessSave, System.Windows.Forms.Keys.None);
			this.toolStripButtonQuickAccessSave.Size = new System.Drawing.Size(23, 95);
			this.toolStripButtonQuickAccessSave.Text = "xxSave";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonQuickAccessSave, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonQuickAccessSave, false);
			this.toolStripButtonQuickAccessSave.Click += new System.EventHandler(this.toolStripButtonQuickAccessSave_Click);
			// 
			// toolStripSeparatorQuickAccess
			// 
			this.toolStripSeparatorQuickAccess.Name = "toolStripSeparatorQuickAccess";
			this.SetShortcut(this.toolStripSeparatorQuickAccess, System.Windows.Forms.Keys.None);
			this.toolStripSeparatorQuickAccess.Size = new System.Drawing.Size(6, 98);
			// 
			// toolStripButtonQuickAccessCancel
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonQuickAccessCancel, "");
			this.toolStripButtonQuickAccessCancel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonQuickAccessCancel.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Cancel_16x16;
			this.toolStripButtonQuickAccessCancel.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonQuickAccessCancel.Name = "toolStripButtonQuickAccessCancel";
			this.SetShortcut(this.toolStripButtonQuickAccessCancel, System.Windows.Forms.Keys.None);
			this.toolStripButtonQuickAccessCancel.Size = new System.Drawing.Size(23, 95);
			this.toolStripButtonQuickAccessCancel.Text = "xxCancel";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonQuickAccessCancel, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonQuickAccessCancel, false);
			this.toolStripButtonQuickAccessCancel.Click += new System.EventHandler(this.toolStripButtonQuickAccessCancel_Click);
			// 
			// toolStripButtonQuickAccessRedo
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonQuickAccessRedo, "");
			this.toolStripButtonQuickAccessRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonQuickAccessRedo.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Redo_small;
			this.toolStripButtonQuickAccessRedo.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonQuickAccessRedo.Name = "toolStripButtonQuickAccessRedo";
			this.SetShortcut(this.toolStripButtonQuickAccessRedo, System.Windows.Forms.Keys.None);
			this.toolStripButtonQuickAccessRedo.Size = new System.Drawing.Size(23, 95);
			this.toolStripButtonQuickAccessRedo.Text = "xxRedo";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonQuickAccessRedo, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonQuickAccessRedo, false);
			this.toolStripButtonQuickAccessRedo.MouseUp += new System.Windows.Forms.MouseEventHandler(this.toolStripButtonQuickAccessRedo_Click_1);
			// 
			// toolStripSplitButtonQuickAccessUndo
			// 
			this.toolStripSplitButtonQuickAccessUndo.DefaultItem = this.toolStripMenuItemQuickAccessUndo;
			this.ribbonControlAdv1.SetDescription(this.toolStripSplitButtonQuickAccessUndo, "");
			this.toolStripSplitButtonQuickAccessUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripSplitButtonQuickAccessUndo.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemQuickAccessUndo,
            this.toolStripMenuItemQuickAccessUndoAll});
			this.toolStripSplitButtonQuickAccessUndo.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Undo_small;
			this.toolStripSplitButtonQuickAccessUndo.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripSplitButtonQuickAccessUndo.Name = "toolStripSplitButtonQuickAccessUndo";
			this.SetShortcut(this.toolStripSplitButtonQuickAccessUndo, System.Windows.Forms.Keys.None);
			this.toolStripSplitButtonQuickAccessUndo.Size = new System.Drawing.Size(32, 95);
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
			this.toolStripMenuItemQuickAccessUndo.Click += new System.EventHandler(this.toolStripSplitButtonQuickAccessUndo_ButtonClick);
			// 
			// toolStripMenuItemQuickAccessUndoAll
			// 
			this.toolStripMenuItemQuickAccessUndoAll.Name = "toolStripMenuItemQuickAccessUndoAll";
			this.SetShortcut(this.toolStripMenuItemQuickAccessUndoAll, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemQuickAccessUndoAll.Size = new System.Drawing.Size(127, 22);
			this.toolStripMenuItemQuickAccessUndoAll.Text = "xxUndoAll";
			this.toolStripMenuItemQuickAccessUndoAll.Click += new System.EventHandler(this.toolStripMenuItemQuickAccessUndoAll_Click_1);
			// 
			// toolStripButtonShowTexts
			// 
			this.toolStripButtonShowTexts.Checked = true;
			this.toolStripButtonShowTexts.CheckState = System.Windows.Forms.CheckState.Checked;
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonShowTexts, "");
			this.toolStripButtonShowTexts.Image = global::Teleopti.Ccc.Win.Properties.Resources.text_list_bullets;
			this.toolStripButtonShowTexts.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripButtonShowTexts.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonShowTexts.Name = "toolStripButtonShowTexts";
			this.SetShortcut(this.toolStripButtonShowTexts, System.Windows.Forms.Keys.None);
			this.toolStripButtonShowTexts.Size = new System.Drawing.Size(99, 95);
			this.toolStripButtonShowTexts.Text = "xxShowLabels";
			this.toolStripButtonShowTexts.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonShowTexts, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonShowTexts, false);
			this.toolStripButtonShowTexts.Click += new System.EventHandler(this.toolStripButtonShowTexts_Click);
			// 
			// toolStripButtonMainMenuSave
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonMainMenuSave, "");
			this.toolStripButtonMainMenuSave.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Save;
			this.toolStripButtonMainMenuSave.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripButtonMainMenuSave.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonMainMenuSave.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonMainMenuSave.Name = "toolStripButtonMainMenuSave";
			this.SetShortcut(this.toolStripButtonMainMenuSave, System.Windows.Forms.Keys.None);
			this.toolStripButtonMainMenuSave.Size = new System.Drawing.Size(150, 36);
			this.toolStripButtonMainMenuSave.Text = "xxSave";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonMainMenuSave, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonMainMenuSave, false);
			this.toolStripButtonMainMenuSave.Click += new System.EventHandler(this.toolStripButtonMainMenuSave_Click);
			// 
			// officeDropDownButtonMainMenuExportTo
			// 
			this.officeDropDownButtonMainMenuExportTo.AutoSize = false;
			this.ribbonControlAdv1.SetDescription(this.officeDropDownButtonMainMenuExportTo, "");
			this.officeDropDownButtonMainMenuExportTo.DropDownFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
			this.officeDropDownButtonMainMenuExportTo.DropDownText = "xxExportToScenario";
			this.officeDropDownButtonMainMenuExportTo.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Export2;
			this.officeDropDownButtonMainMenuExportTo.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.officeDropDownButtonMainMenuExportTo.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.officeDropDownButtonMainMenuExportTo.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.officeDropDownButtonMainMenuExportTo.Name = "officeDropDownButtonMainMenuExportTo";
			this.SetShortcut(this.officeDropDownButtonMainMenuExportTo, System.Windows.Forms.Keys.None);
			this.officeDropDownButtonMainMenuExportTo.Size = new System.Drawing.Size(150, 36);
			this.officeDropDownButtonMainMenuExportTo.Text = "xxExportTo";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.officeDropDownButtonMainMenuExportTo, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.officeDropDownButtonMainMenuExportTo, false);
			// 
			// toolStripSeparator4
			// 
			this.toolStripSeparator4.Name = "toolStripSeparator4";
			this.SetShortcut(this.toolStripSeparator4, System.Windows.Forms.Keys.None);
			this.toolStripSeparator4.Size = new System.Drawing.Size(134, 2);
			// 
			// toolStripButtonMainMenuHelp
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonMainMenuHelp, "");
			this.toolStripButtonMainMenuHelp.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonMainMenuHelp.Image")));
			this.toolStripButtonMainMenuHelp.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripButtonMainMenuHelp.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonMainMenuHelp.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonMainMenuHelp.Name = "toolStripButtonMainMenuHelp";
			this.SetShortcut(this.toolStripButtonMainMenuHelp, System.Windows.Forms.Keys.None);
			this.toolStripButtonMainMenuHelp.Size = new System.Drawing.Size(150, 36);
			this.toolStripButtonMainMenuHelp.Text = "xxHelp";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonMainMenuHelp, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonMainMenuHelp, false);
			this.toolStripButtonMainMenuHelp.Click += new System.EventHandler(this.toolStripButtonMainMenuHelp_Click);
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.SetShortcut(this.toolStripSeparator3, System.Windows.Forms.Keys.None);
			this.toolStripSeparator3.Size = new System.Drawing.Size(134, 2);
			// 
			// toolStripButtonMainMenuClose
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonMainMenuClose, "");
			this.toolStripButtonMainMenuClose.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Close;
			this.toolStripButtonMainMenuClose.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolStripButtonMainMenuClose.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.toolStripButtonMainMenuClose.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonMainMenuClose.Name = "toolStripButtonMainMenuClose";
			this.SetShortcut(this.toolStripButtonMainMenuClose, System.Windows.Forms.Keys.None);
			this.toolStripButtonMainMenuClose.Size = new System.Drawing.Size(150, 36);
			this.toolStripButtonMainMenuClose.Text = "xxClose";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonMainMenuClose, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonMainMenuClose, false);
			this.toolStripButtonMainMenuClose.Click += new System.EventHandler(this.toolStripButtonMainMenuClose_Click);
			// 
			// toolStripButtonSystemExit
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonSystemExit, "");
			this.toolStripButtonSystemExit.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Exit;
			this.toolStripButtonSystemExit.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonSystemExit.Name = "toolStripButtonSystemExit";
			this.SetShortcut(this.toolStripButtonSystemExit, System.Windows.Forms.Keys.None);
			this.toolStripButtonSystemExit.Size = new System.Drawing.Size(130, 20);
			this.toolStripButtonSystemExit.Text = "xxExitTELEOPTICCC";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonSystemExit, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonSystemExit, false);
			this.toolStripButtonSystemExit.Click += new System.EventHandler(this.toolStripButtonSystemExit_Click);
			// 
			// toolStripButtonOptions
			// 
			this.ribbonControlAdv1.SetDescription(this.toolStripButtonOptions, "");
			this.toolStripButtonOptions.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Options_32x32;
			this.toolStripButtonOptions.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonOptions.Name = "toolStripButtonOptions";
			this.SetShortcut(this.toolStripButtonOptions, System.Windows.Forms.Keys.None);
			this.toolStripButtonOptions.Size = new System.Drawing.Size(79, 20);
			this.toolStripButtonOptions.Text = "xxOptions";
			this.toolStripButtonOptions.ToolTipText = "xxOptions";
			this.ribbonControlAdv1.SetUseInCustomQuickAccessDialog(this.toolStripButtonOptions, true);
			this.ribbonControlAdv1.SetUseInQuickAccessMenu(this.toolStripButtonOptions, false);
			this.toolStripButtonOptions.Click += new System.EventHandler(this.toolStripButtonOptions_Click);
			// 
			// btnFilter
			// 
			this.btnFilter.AutoSize = false;
			this.btnFilter.Image = global::Teleopti.Ccc.Win.Properties.Resources.ccc_Filter;
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
			this.btnRightLeft.Image = global::Teleopti.Ccc.Win.Properties.Resources.RightToLeft;
			this.btnRightLeft.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.btnRightLeft.Name = "btnRightLeft";
			this.SetShortcut(this.btnRightLeft, System.Windows.Forms.Keys.None);
			this.btnRightLeft.Size = new System.Drawing.Size(70, 17);
			this.btnRightLeft.Tag = "rightleft";
			this.btnRightLeft.Text = "xxRiLe";
			// 
			// imageListSkillTypeIcons
			// 
			this.imageListSkillTypeIcons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListSkillTypeIcons.ImageStream")));
			this.imageListSkillTypeIcons.TransparentColor = System.Drawing.Color.Transparent;
			this.imageListSkillTypeIcons.Images.SetKeyName(0, "ccc_SkillEmail_16x16.png");
			this.imageListSkillTypeIcons.Images.SetKeyName(1, "ccc_Skill_Fax_16x16.png");
			this.imageListSkillTypeIcons.Images.SetKeyName(2, "ccc_SkillTelephone_16x16.png");
			this.imageListSkillTypeIcons.Images.SetKeyName(3, "ccc_Skill_Backoffice_16x16.png");
			this.imageListSkillTypeIcons.Images.SetKeyName(4, "ccc_PeopleScehdulePeriodView.png");
			this.imageListSkillTypeIcons.Images.SetKeyName(5, "desktop.png");
			this.imageListSkillTypeIcons.Images.SetKeyName(6, "skill_retail.png");
			// 
			// contextMenuStripRestrictionView
			// 
			this.contextMenuStripRestrictionView.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemUseRotation,
            this.toolStripMenuItemUseAvailability,
            this.toolStripMenuItemUsePreference,
            this.toolStripMenuItemUseStudentAvailability,
            this.toolStripMenuItemUseSchedule,
            this.toolStripMenuItemAddPreferenceRestriction,
            this.toolStripMenuItemAddStudentAvailabilityRestriction,
            this.toolStripSeparator5,
            this.toolStripMenuItemRestrictionCopy,
            this.toolStripMenuItemRestrictionPaste,
            this.toolStripMenuItemRestrictionDelete,
            this.toolStripSeparator6,
            this.xxAgentInfoToolStripMenuItem});
			this.contextMenuStripRestrictionView.Name = "contextMenuStripRestrictionView";
			this.contextMenuStripRestrictionView.Size = new System.Drawing.Size(260, 280);
			// 
			// toolStripMenuItemUseRotation
			// 
			this.toolStripMenuItemUseRotation.CheckOnClick = true;
			this.toolStripMenuItemUseRotation.Name = "toolStripMenuItemUseRotation";
			this.SetShortcut(this.toolStripMenuItemUseRotation, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemUseRotation.Size = new System.Drawing.Size(259, 22);
			this.toolStripMenuItemUseRotation.Text = "xxRotation";
			this.toolStripMenuItemUseRotation.Visible = false;
			this.toolStripMenuItemUseRotation.Click += new System.EventHandler(this.toolStripMenuItemUseRotation_Click);
			// 
			// toolStripMenuItemUseAvailability
			// 
			this.toolStripMenuItemUseAvailability.Checked = true;
			this.toolStripMenuItemUseAvailability.CheckOnClick = true;
			this.toolStripMenuItemUseAvailability.CheckState = System.Windows.Forms.CheckState.Checked;
			this.toolStripMenuItemUseAvailability.Name = "toolStripMenuItemUseAvailability";
			this.SetShortcut(this.toolStripMenuItemUseAvailability, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemUseAvailability.Size = new System.Drawing.Size(259, 22);
			this.toolStripMenuItemUseAvailability.Text = "xxAvailability";
			this.toolStripMenuItemUseAvailability.Visible = false;
			this.toolStripMenuItemUseAvailability.Click += new System.EventHandler(this.toolStripMenuItemUseAvailability_Click);
			// 
			// toolStripMenuItemUsePreference
			// 
			this.toolStripMenuItemUsePreference.Checked = true;
			this.toolStripMenuItemUsePreference.CheckOnClick = true;
			this.toolStripMenuItemUsePreference.CheckState = System.Windows.Forms.CheckState.Checked;
			this.toolStripMenuItemUsePreference.Name = "toolStripMenuItemUsePreference";
			this.SetShortcut(this.toolStripMenuItemUsePreference, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemUsePreference.Size = new System.Drawing.Size(259, 22);
			this.toolStripMenuItemUsePreference.Text = "xxPreference";
			this.toolStripMenuItemUsePreference.Visible = false;
			this.toolStripMenuItemUsePreference.Click += new System.EventHandler(this.toolStripMenuItemUsePreference_Click);
			// 
			// toolStripMenuItemUseStudentAvailability
			// 
			this.toolStripMenuItemUseStudentAvailability.CheckOnClick = true;
			this.toolStripMenuItemUseStudentAvailability.Name = "toolStripMenuItemUseStudentAvailability";
			this.SetShortcut(this.toolStripMenuItemUseStudentAvailability, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemUseStudentAvailability.Size = new System.Drawing.Size(259, 22);
			this.toolStripMenuItemUseStudentAvailability.Text = "xxStudentAvailability";
			this.toolStripMenuItemUseStudentAvailability.Visible = false;
			this.toolStripMenuItemUseStudentAvailability.Click += new System.EventHandler(this.toolStripMenuItemUseStudentAvailability_Click);
			// 
			// toolStripMenuItemUseSchedule
			// 
			this.toolStripMenuItemUseSchedule.Checked = true;
			this.toolStripMenuItemUseSchedule.CheckOnClick = true;
			this.toolStripMenuItemUseSchedule.CheckState = System.Windows.Forms.CheckState.Checked;
			this.toolStripMenuItemUseSchedule.Name = "toolStripMenuItemUseSchedule";
			this.SetShortcut(this.toolStripMenuItemUseSchedule, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemUseSchedule.Size = new System.Drawing.Size(259, 22);
			this.toolStripMenuItemUseSchedule.Text = "xxSchedule";
			this.toolStripMenuItemUseSchedule.Visible = false;
			this.toolStripMenuItemUseSchedule.Click += new System.EventHandler(this.toolStripMenuItemUseSchedule_Click);
			// 
			// toolStripMenuItemAddPreferenceRestriction
			// 
			this.toolStripMenuItemAddPreferenceRestriction.Name = "toolStripMenuItemAddPreferenceRestriction";
			this.SetShortcut(this.toolStripMenuItemAddPreferenceRestriction, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemAddPreferenceRestriction.Size = new System.Drawing.Size(259, 22);
			this.toolStripMenuItemAddPreferenceRestriction.Text = "xxAddPreferenceThreeDots";
			this.toolStripMenuItemAddPreferenceRestriction.Click += new System.EventHandler(this.addPreferenceToolStripMenuItemClick);
			// 
			// toolStripMenuItemAddStudentAvailabilityRestriction
			// 
			this.toolStripMenuItemAddStudentAvailabilityRestriction.Name = "toolStripMenuItemAddStudentAvailabilityRestriction";
			this.SetShortcut(this.toolStripMenuItemAddStudentAvailabilityRestriction, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemAddStudentAvailabilityRestriction.Size = new System.Drawing.Size(259, 22);
			this.toolStripMenuItemAddStudentAvailabilityRestriction.Text = "xxAddStudentAvailabilityThreeDots";
			this.toolStripMenuItemAddStudentAvailabilityRestriction.Click += new System.EventHandler(this.addStudentAvailabilityToolStripMenuItemClick);
			// 
			// toolStripSeparator5
			// 
			this.toolStripSeparator5.Name = "toolStripSeparator5";
			this.SetShortcut(this.toolStripSeparator5, System.Windows.Forms.Keys.None);
			this.toolStripSeparator5.Size = new System.Drawing.Size(256, 6);
			// 
			// toolStripMenuItemRestrictionCopy
			// 
			this.toolStripMenuItemRestrictionCopy.Name = "toolStripMenuItemRestrictionCopy";
			this.SetShortcut(this.toolStripMenuItemRestrictionCopy, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemRestrictionCopy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
			this.toolStripMenuItemRestrictionCopy.Size = new System.Drawing.Size(259, 22);
			this.toolStripMenuItemRestrictionCopy.Text = "xxCopy";
			this.toolStripMenuItemRestrictionCopy.Click += new System.EventHandler(this.toolStripMenuItemRestrictionCopy_Click);
			// 
			// toolStripMenuItemRestrictionPaste
			// 
			this.toolStripMenuItemRestrictionPaste.Name = "toolStripMenuItemRestrictionPaste";
			this.SetShortcut(this.toolStripMenuItemRestrictionPaste, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemRestrictionPaste.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
			this.toolStripMenuItemRestrictionPaste.Size = new System.Drawing.Size(259, 22);
			this.toolStripMenuItemRestrictionPaste.Text = "xxPaste";
			this.toolStripMenuItemRestrictionPaste.Click += new System.EventHandler(this.toolStripMenuItemRestrictionPaste_Click);
			// 
			// toolStripMenuItemRestrictionDelete
			// 
			this.toolStripMenuItemRestrictionDelete.Name = "toolStripMenuItemRestrictionDelete";
			this.SetShortcut(this.toolStripMenuItemRestrictionDelete, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemRestrictionDelete.ShortcutKeys = System.Windows.Forms.Keys.Delete;
			this.toolStripMenuItemRestrictionDelete.Size = new System.Drawing.Size(259, 22);
			this.toolStripMenuItemRestrictionDelete.Text = "xxDelete";
			this.toolStripMenuItemRestrictionDelete.Click += new System.EventHandler(this.toolStripMenuItemRestrictionDelete_Click);
			// 
			// toolStripSeparator6
			// 
			this.toolStripSeparator6.Name = "toolStripSeparator6";
			this.SetShortcut(this.toolStripSeparator6, System.Windows.Forms.Keys.None);
			this.toolStripSeparator6.Size = new System.Drawing.Size(256, 6);
			// 
			// xxAgentInfoToolStripMenuItem
			// 
			this.xxAgentInfoToolStripMenuItem.Name = "xxAgentInfoToolStripMenuItem";
			this.SetShortcut(this.xxAgentInfoToolStripMenuItem, System.Windows.Forms.Keys.None);
			this.xxAgentInfoToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.I)));
			this.xxAgentInfoToolStripMenuItem.Size = new System.Drawing.Size(259, 22);
			this.xxAgentInfoToolStripMenuItem.Text = "xxAgentInfo";
			this.xxAgentInfoToolStripMenuItem.Click += new System.EventHandler(this.toolStripButtonAgentInfo_Click);
			// 
			// schedulerSplitters1
			// 
			this.schedulerSplitters1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.schedulerSplitters1.Location = new System.Drawing.Point(6, 161);
			this.schedulerSplitters1.Name = "schedulerSplitters1";
			this.schedulerSplitters1.Size = new System.Drawing.Size(1211, 564);
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
			this.contextMenuStripRequests.Size = new System.Drawing.Size(206, 92);
			// 
			// ToolStripMenuItemViewDetails
			// 
			this.ToolStripMenuItemViewDetails.Enabled = false;
			this.ToolStripMenuItemViewDetails.Name = "ToolStripMenuItemViewDetails";
			this.SetShortcut(this.ToolStripMenuItemViewDetails, System.Windows.Forms.Keys.None);
			this.ToolStripMenuItemViewDetails.Size = new System.Drawing.Size(205, 22);
			this.ToolStripMenuItemViewDetails.Text = "xxViewDetails";
			this.ToolStripMenuItemViewDetails.Click += new System.EventHandler(this.ToolStripMenuItemViewDetails_Click);
			// 
			// toolStripMenuItemFindMatching2
			// 
			this.toolStripMenuItemFindMatching2.Name = "toolStripMenuItemFindMatching2";
			this.SetShortcut(this.toolStripMenuItemFindMatching2, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemFindMatching2.Size = new System.Drawing.Size(205, 22);
			this.toolStripMenuItemFindMatching2.Text = "xxFindMatchingTreeDots";
			this.toolStripMenuItemFindMatching2.Visible = false;
			this.toolStripMenuItemFindMatching2.Click += new System.EventHandler(this.toolStripMenuItemFindMatching2_Click);
			// 
			// toolStripMenuItemViewAllowance
			// 
			this.toolStripMenuItemViewAllowance.Enabled = false;
			this.toolStripMenuItemViewAllowance.Name = "toolStripMenuItemViewAllowance";
			this.SetShortcut(this.toolStripMenuItemViewAllowance, System.Windows.Forms.Keys.None);
			this.toolStripMenuItemViewAllowance.Size = new System.Drawing.Size(205, 22);
			this.toolStripMenuItemViewAllowance.Text = "xxViewAllowance";
			this.toolStripMenuItemViewAllowance.Click += new System.EventHandler(this.toolStripMenuItemViewAllowance_Click);
			// 
			// xxViewOldRequestsToolStripMenuItem
			// 
			this.xxViewOldRequestsToolStripMenuItem.Name = "xxViewOldRequestsToolStripMenuItem";
			this.SetShortcut(this.xxViewOldRequestsToolStripMenuItem, System.Windows.Forms.Keys.None);
			this.xxViewOldRequestsToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
			this.xxViewOldRequestsToolStripMenuItem.Text = "xxViewHistory";
			this.xxViewOldRequestsToolStripMenuItem.Click += new System.EventHandler(this.toolStripViewRequestHistory_Click);
			// 
			// toolStripExFilterDays
			// 
			this.toolStripExFilterDays.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripExFilterDays.ForeColor = System.Drawing.Color.MidnightBlue;
			this.toolStripExFilterDays.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStripExFilterDays.Image = null;
			this.toolStripExFilterDays.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.xxShowLastNumberOfDays,
            this.toolStripComboBoxExFilterDays});
			this.toolStripExFilterDays.Location = new System.Drawing.Point(713, 1);
			this.toolStripExFilterDays.Name = "toolStripExFilterDays";
			this.toolStripExFilterDays.ShowLauncher = false;
			this.toolStripExFilterDays.Size = new System.Drawing.Size(38, 98);
			this.toolStripExFilterDays.TabIndex = 3;
			this.toolStripExFilterDays.Text = "xxFilterDays";
			// 
			// xxShowLastNumberOfDays
			// 
			this.xxShowLastNumberOfDays.Name = "xxShowLastNumberOfDays";
			this.SetShortcut(this.xxShowLastNumberOfDays, System.Windows.Forms.Keys.None);
			this.xxShowLastNumberOfDays.Size = new System.Drawing.Size(181, 15);
			this.xxShowLastNumberOfDays.Text = "xxShowLastNumberOfDaysColon";
			// 
			// toolStripComboBoxExFilterDays
			// 
			this.toolStripComboBoxExFilterDays.AutoSize = false;
			this.toolStripComboBoxExFilterDays.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.toolStripComboBoxExFilterDays.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.toolStripComboBoxExFilterDays.Items.AddRange(new object[] {
            "2",
            "14"});
			this.toolStripComboBoxExFilterDays.Margin = new System.Windows.Forms.Padding(7, 2, 2, 2);
			this.toolStripComboBoxExFilterDays.MaxDropDownItems = 4;
			this.toolStripComboBoxExFilterDays.Name = "toolStripComboBoxExFilterDays";
			this.SetShortcut(this.toolStripComboBoxExFilterDays, System.Windows.Forms.Keys.None);
			this.toolStripComboBoxExFilterDays.Size = new System.Drawing.Size(45, 23);
			this.toolStripComboBoxExFilterDays.ToolTipText = "xxFilterNumberOfDaysBackInTime";
			this.toolStripComboBoxExFilterDays.SelectedIndexChanged += new System.EventHandler(this.toolStripComboBoxExFilterDays_SelectedIndexChanged);
			// 
			// SchedulingScreen
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1223, 757);
			this.Controls.Add(this.schedulerSplitters1);
			this.Controls.Add(this.statusStrip1);
			this.Controls.Add(this.ribbonControlAdv1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "SchedulingScreen";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "xxTeleoptiRaptorColonScheduler";
			this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SchedulingScreen_FormClosing);
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SchedulingScreen_FormClosed);
			this.Load += new System.EventHandler(this.SchedulingScreen_Load);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SchedulingScreen_KeyDown);
			this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.SchedulingScreen_KeyUp);
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.contextMenuViews.ResumeLayout(false);
			this.contextMenuStripResultView.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).EndInit();
			this.ribbonControlAdv1.ResumeLayout(false);
			this.ribbonControlAdv1.PerformLayout();
			this.toolStripTabItemHome.Panel.ResumeLayout(false);
			this.toolStripTabItemHome.Panel.PerformLayout();
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
			this.toolStripTabItemChart.Panel.ResumeLayout(false);
			this.toolStripTabItemChart.Panel.PerformLayout();
			this.toolStripExGridRowInChartButtons.ResumeLayout(false);
			this.toolStripExGridRowInChartButtons.PerformLayout();
			this.toolStripExSkillViews.ResumeLayout(false);
			this.toolStripExSkillViews.PerformLayout();
			this.toolStripTabItem1.Panel.ResumeLayout(false);
			this.toolStripTabItem1.Panel.PerformLayout();
			this.toolStripEx2.ResumeLayout(false);
			this.toolStripEx2.PerformLayout();
			this.toolStripExHandleRequests.ResumeLayout(false);
			this.toolStripExHandleRequests.PerformLayout();
			this.toolStripEx3.ResumeLayout(false);
			this.toolStripEx3.PerformLayout();
			this.toolStripTabItemQuickAccess.Panel.ResumeLayout(false);
			this.toolStripExForQuickAccessItems.ResumeLayout(false);
			this.toolStripExForQuickAccessItems.PerformLayout();
			this.contextMenuStripRestrictionView.ResumeLayout(false);
			this.contextMenuStripRequests.ResumeLayout(false);
			this.toolStripExFilterDays.ResumeLayout(false);
			this.toolStripExFilterDays.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

	    #endregion

		private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemInsertAbsence;
        private System.Windows.Forms.ToolStripButton btnFilter;
        private System.Windows.Forms.ToolStripButton btnRightLeft;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelContractTime;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelStatus;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed ribbonControlAdv1;
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
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemAgentInfo;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripResultView;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemDay;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemIntraday;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemOccupancyAdjustment;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemNextAssignment;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemShowAssignmentBefore;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemLock;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemUnlock;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemLockSelectionRM;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemLockFreeDaysRM;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemLockAbsencesRM;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemLockShiftCategoriesRM;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemUnlockAllRM;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemUnlockSelectionRM;
        private System.Windows.Forms.ToolStripButton toolStripButtonMainMenuSave;
        private System.Windows.Forms.ToolStripButton toolStripButtonMainMenuClose;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExClipboard;
        private System.Windows.Forms.ToolStripButton toolStripButtonAgentInfo;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExScheduleViews;
        private Syncfusion.Windows.Forms.Tools.ToolStripPanelItem toolStripPanelItemViews;
        private System.Windows.Forms.ToolStripButton toolStripButtonDayView;
        private System.Windows.Forms.ToolStripButton toolStripButtonWeekView;
        private System.Windows.Forms.ToolStripButton toolStripButtonFilterAgents;
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
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemBackToLegalState;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExEdit2;
        private Syncfusion.Windows.Forms.Tools.ToolStripTabItem toolStripTabItemChart;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButtonSwap;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSwap;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSwapAndReschedule;
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
        private System.Windows.Forms.ToolStripButton toolStripButtonEditNote;
        private System.Windows.Forms.ToolStripButton toolStripButtonReplyAndApprove;
        private System.Windows.Forms.ToolStripButton toolStripButtonReplyAndDeny;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemReOptimize;
        private System.Windows.Forms.ToolStripButton toolStripButtonOptions;
        private System.Windows.Forms.ToolStripButton toolStripButtonSystemExit;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemScheduleHourlyEmployees;
        private Teleopti.Ccc.WpfControls.Controls.Notes.NotesEditor notesEditor;
        //private Teleopti.Ccc.WpfControls.Common.Interop.MultipleHostControl hostedComponent1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemAddOverTime;
        private System.Windows.Forms.ImageList imageListSkillTypeIcons;
        private Teleopti.Ccc.Win.Common.Controls.SpinningProgress.ToolStripSpinningProgressControl toolStripSpinningProgressControl1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemWriteProtectSchedule;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemWriteProtectSchedule2;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem6;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem5;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemViewPointTimeZone;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemLoggedOnUserTimeZone;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private Syncfusion.Windows.Forms.Tools.ToolStripTabItem toolStripTabItemQuickAccess;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExForQuickAccessItems;
        private System.Windows.Forms.ToolStripButton toolStripButtonQuickAccessSave;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorQuickAccess;
        private System.Windows.Forms.ToolStripButton toolStripButtonQuickAccessCancel;
        private System.Windows.Forms.ToolStripButton toolStripButtonQuickAccessRedo;
        private System.Windows.Forms.ToolStripSplitButton toolStripSplitButtonQuickAccessUndo;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemQuickAccessUndo;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemQuickAccessUndoAll;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripRestrictionView;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemUseRotation;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemUsePreference;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemUseAvailability;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemUseStudentAvailability;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemUseSchedule;
        private System.Windows.Forms.ToolStripButton toolStripButtonMainMenuHelp;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private Syncfusion.Windows.Forms.Tools.OfficeDropDownButton officeDropDownButtonMainMenuExportTo;
        private System.Windows.Forms.ToolStripButton toolStripButtonFindAgents;
        private System.Windows.Forms.ToolStripButton toolStripButtonRefresh;
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
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemViewReport;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemScheduledTimePerActivity;
		private Syncfusion.Windows.Forms.Tools.ContextMenuStripEx contextMenuViews;
		private System.Windows.Forms.ContextMenuStrip contextMenuStripRequests;
		private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItemViewDetails;
        private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripEx2;
        private System.Windows.Forms.ToolStripButton toolStripButtonViewDetails;
		private ToolStripMenuItem ToolStripMenuItemSwapRaw;
		private ToolStripMenuItem xxExportToolStripMenuItem;
		private ToolStripMenuItem toolStripMenuItemExportToPDF;
		private ToolStripMenuItem toolStripMenuItemExportToPDFGraphical;
		private ToolStripMenuItem toolStripMenuItemFindMatching;
		private ToolStripMenuItem toolStripMenuItemFindMatching2;
        private ToolStripMenuItem ToolStripMenuItemLockRestrictions;
        private ToolStripMenuItem ToolStripMenuItemLockAllRestrictions;
        private ToolStripMenuItem ToolStripMenuItemLockPreferences;
        private ToolStripMenuItem ToolStripMenuItemAllPreferences;
        private ToolStripMenuItem ToolStripMenuItemAllDaysOff;
        private ToolStripMenuItem ToolStripMenuItemAllShiftsPreferences;
        private ToolStripMenuItem ToolStripMenuItemAllFulFilledPreferences;
        private ToolStripMenuItem ToolStripMenuItemAllFulFilledDaysOffPreferences;
        private ToolStripMenuItem ToolStripMenuItemAllFulFilledShiftsPreferences;
        private ToolStripMenuItem ToolStripMenuItemLockRotations;
        private ToolStripMenuItem ToolStripMenuItemAllRotations;
        private ToolStripMenuItem ToolStripMenuItemAllDaysOffRotations;
        private ToolStripMenuItem ToolStripMenuItemAllFulFilledRotations;
        private ToolStripMenuItem ToolStripMenuItemAllFulFilledDaysOffRotations;
        private ToolStripMenuItem ToolStripMenuItemAllFulFilledShiftsRotations;
        private ToolStripMenuItem ToolStripMenuItemLockStudentAvailability;
        private ToolStripMenuItem ToolStripMenuItemAllUnavailableStudentAvailability;
        private ToolStripMenuItem ToolStripMenuItemAllAvailableStudentAvailability;
        private ToolStripMenuItem ToolStripMenuItemAllFulFilledStudentAvailability;
        private ToolStripMenuItem ToolStripMenuItemLockAvailability;
        private ToolStripMenuItem ToolStripMenuItemAllUnavailableAvailability;
        private ToolStripMenuItem ToolStripMenuItemAllAvailableAvailability;
        private ToolStripMenuItem ToolStripMenuItemAllFulFilledAvailability;
        private ToolStripMenuItem ToolStripMenuItemLockRestrictionsRM;
        private ToolStripMenuItem ToolStripMenuItemAllRM;
        private ToolStripMenuItem ToolStripMenuItemLockPreferencesRM;
        private ToolStripMenuItem ToolStripMenuItemAllPreferencesRM;
        private ToolStripMenuItem ToolStripMenuItemAllDaysOffPreferencesRM;
        private ToolStripMenuItem ToolStripMenuItemAllShiftsPreferencesRM;
        private ToolStripMenuItem ToolStripMenuItemAllFulFilledPreferencesRM;
        private ToolStripMenuItem ToolStripMenuItemAllFulFilledDaysOffPreferencesRM;
        private ToolStripMenuItem ToolStripMenuItemAllFulFilledShiftsPreferencesRM;
        private ToolStripMenuItem ToolStripMenuItemLockRotationsRM;
        private ToolStripMenuItem ToolStripMenuItemAllRotationsRM;
        private ToolStripMenuItem ToolStripMenuItemAllDaysOffRotationsRM;
        private ToolStripMenuItem ToolStripMenuItemAllShiftsRotationsRM;
        private ToolStripMenuItem ToolStripMenuItemAllFulFilledRotationsRM;
        private ToolStripMenuItem ToolStripMenuItemAllFulFilledDaysOffRotationsRM;
        private ToolStripMenuItem ToolStripMenuItemAllFulFilledShiftsRotationsRM;
        private ToolStripMenuItem ToolStripMenuItemLockStudentAvailabilityRM;
        private ToolStripMenuItem ToolStripMenuItemAllUnavailableStudentAvailabilityRM;
        private ToolStripMenuItem ToolStripMenuItemAllAvailableStudentAvailabilityRM;
        private ToolStripMenuItem ToolStripMenuItemAllFulFilledStudentAvailabilityRM;
        private ToolStripMenuItem ToolStripMenuItemLockAvailabilityRM;
        private ToolStripMenuItem ToolStripMenuItemAllUnAvailableAvailabilityRM;
        private ToolStripMenuItem ToolStripMenuItemAllAvailableAvailabilityRM;
        private ToolStripMenuItem ToolStripMenuItemAllFulFilledAvailabilityRM;
        private ToolStripMenuItem ToolStripMenuItemAllShiftsRotations;
        private ToolStripMenuItem ToolStripMenuItemAllMustHaveRM;
        private ToolStripMenuItem ToolStripMenuItemAllFulfilledMustHaveRM;
        private ToolStripMenuItem ToolStripMenuItemAllMustHave;
        private ToolStripMenuItem ToolStripMenuItemAllFulfilledMustHave;
		private ToolStripMenuItem ToolStripMenuItemAddPersonalActivity;
		private ToolStripMenuItem toolStripMenuItemPasteShiftFromShifts;
        private ToolStripMenuItem ToolStripMenuItemAllAbsencePreferenceRM;
        private ToolStripMenuItem ToolStripMenuItemAllFulFilledAbsencesPreferencesRM;
        private ToolStripMenuItem ToolStripMenuItemAllAbsencePreference;
        private ToolStripMenuItem ToolStripMenuItemAllFulFilledAbsencesPreferences;
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
        private ToolStripMenuItem xxContractTimeAscToolStripMenuItem;
        private ToolStripMenuItem toolstripMenuRemoveWriteProtection;
        private ToolStripMenuItem xxContractTimeDescToolStripMenuItem;
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
		private ToolStripSeparator toolStripSeparator6;
		private ToolStripMenuItem xxAgentInfoToolStripMenuItem;
		private ToolStripTextBox toolStripTextBoxFilter;
		private Syncfusion.Windows.Forms.Tools.ToolStripEx toolStripExFilterDays;
		private ToolStripLabel xxShowLastNumberOfDays;
		private Syncfusion.Windows.Forms.Tools.ToolStripComboBoxEx toolStripComboBoxExFilterDays;
		private ToolStripMenuItem toolStripMenuItemAddPreference;
		private ToolStripMenuItem toolStripMenuItemAddStudentAvailability;
		private ToolStripMenuItem toolStripMenuItemAddPreferenceRestriction;
		private ToolStripMenuItem toolStripMenuItemAddStudentAvailabilityRestriction;
		private ToolStripButton toolStripButtonFindRequest;
		private ToolStripMenuItem toolStripMenuItemAddOvertimeAvailability;
        
        
    }
}
