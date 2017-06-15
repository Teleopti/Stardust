(function () {
	'use strict';

	angular.module('wfm.teamSchedule').service('teamsToggles', teamsToggles);

	teamsToggles.$inject = ['Toggle'];

	function teamsToggles(toggleSvc) {
		var self = this;

		self.all = function getToggles() {
			return {
				SelectAgentsPerPageEnabled: toggleSvc.WfmTeamSchedule_SetAgentsPerPage_36230,
				SeeScheduleChangesByOthers: toggleSvc.WfmTeamSchedule_SeeScheduleChangesByOthers_36303,
				DisplayScheduleOnBusinessHierachyEnabled: toggleSvc.WfmTeamSchedule_DisplayScheduleOnBusinessHierachy_41260,
				DisplayWeekScheduleOnBusinessHierachyEnabled: toggleSvc
					.WfmTeamSchedule_DisplayWeekScheduleOnBusinessHierachy_42252,

				AbsenceReportingEnabled: toggleSvc.WfmTeamSchedule_AbsenceReporting_35995,
				AddActivityEnabled: toggleSvc.WfmTeamSchedule_AddActivity_37541,
				AddPersonalActivityEnabled: toggleSvc.WfmTeamSchedule_AddPersonalActivity_37742,
				AddOvertimeEnabled: toggleSvc.WfmTeamSchedule_AddOvertime_41696,
				MoveActivityEnabled: toggleSvc.WfmTeamSchedule_MoveActivity_37744,
				MoveInvalidOverlappedActivityEnabled: toggleSvc.WfmTeamSchedule_MoveInvalidOverlappedActivity_40688,
				MoveEntireShiftEnabled: toggleSvc.WfmTeamSchedule_MoveEntireShift_41632,
				SwapShiftEnabled: toggleSvc.WfmTeamSchedule_SwapShifts_36231,
				RemoveAbsenceEnabled: toggleSvc.WfmTeamSchedule_RemoveAbsence_36705,
				RemoveActivityEnabled: toggleSvc.WfmTeamSchedule_RemoveActivity_37743,
				RemoveOvertimeEnabled: toggleSvc.WfmTeamSchedule_RemoveOvertime_42481,
				UndoScheduleEnabled: toggleSvc.WfmTeamSchedule_RevertToPreviousSchedule_39002,

				ViewShiftCategoryEnabled: toggleSvc.WfmTeamSchedule_ShowShiftCategory_39796,
				ModifyShiftCategoryEnabled: toggleSvc.WfmTeamSchedule_ModifyShiftCategory_39797,
				ShowContractTimeEnabled: toggleSvc.WfmTeamSchedule_ShowContractTime_38509,
				EditAndViewInternalNoteEnabled: toggleSvc.WfmTeamSchedule_EditAndDisplayInternalNotes_40671,

				WeekViewEnabled: toggleSvc.WfmTeamSchedule_WeekView_39870,
				ShowWeeklyContractTimeEnabled: toggleSvc.WfmTeamSchedule_WeeklyContractTime_39871,

				ShowValidationWarnings: toggleSvc.WfmTeamSchedule_ShowNightlyRestWarning_39619 ||
					toggleSvc.WfmTeamSchedule_ShowWeeklyWorktimeWarning_39799 ||
					toggleSvc.WfmTeamSchedule_ShowWeeklyRestTimeWarning_39800 ||
					toggleSvc.WfmTeamSchedule_ShowDayOffWarning_39801 ||
					toggleSvc.WfmTeamSchedule_ShowOverwrittenLayerWarning_40109,
				FilterValidationWarningsEnabled: toggleSvc.WfmTeamSchedule_FilterValidationWarnings_40110,

				CheckOverlappingCertainActivitiesEnabled: toggleSvc
					.WfmTeamSchedule_ShowWarningForOverlappingCertainActivities_39938,
				AutoMoveOverwrittenActivityForOperationsEnabled: toggleSvc
					.WfmTeamSchedule_AutoMoveOverwrittenActivityForOperations_40279,
				CheckPersonalAccountEnabled: toggleSvc.WfmTeamSchedule_CheckPersonalAccountWhenAddingAbsence_41088,

				ViewScheduleOnTimezoneEnabled: toggleSvc.WfmTeamSchedule_ShowScheduleBasedOnTimeZone_40925,
				ManageScheduleForDistantTimezonesEnabled: toggleSvc.WfmTeamSchedule_ShowShiftsForAgentsInDistantTimeZones_41305,

				MoveToBaseLicenseEnabled: toggleSvc.WfmTeamSchedule_MoveToBaseLicense_41039,
				SaveFavoriteSearchesEnabled: toggleSvc.WfmTeamSchedule_SaveFavoriteSearches_42073,
				WfmTeamSchedule_RemoveOvertime_42481: toggleSvc.WfmTeamSchedule_RemoveOvertime_42481,
				WfmTeamSchedule_SaveFavoriteSearchesInWeekView_42576: toggleSvc.WfmTeamSchedule_SaveFavoriteSearchesInWeekView_42576,
				WfmTeamSchedule_WeekView_OpenDayViewForShiftEditing_40557: toggleSvc.WfmTeamSchedule_WeekView_OpenDayViewForShiftEditing_40557,
				Wfm_HideUnusedTeamsAndSites_42690: toggleSvc.Wfm_HideUnusedTeamsAndSites_42690
			};
		}

	}

})();