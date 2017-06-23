(function () {
	'use strict';

	angular.module('wfm.teamSchedule').service('teamsToggles', teamsToggles);

	teamsToggles.$inject = ['Toggle'];

	function teamsToggles(toggleSvc) {
		var self = this;

		self.all = function getToggles() {
			return {
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
				WfmTeamSchedule_WeekView_OpenDayViewForShiftEditing_40557: toggleSvc.WfmTeamSchedule_WeekView_OpenDayViewForShiftEditing_40557,
				Wfm_HideUnusedTeamsAndSites_42690: toggleSvc.Wfm_HideUnusedTeamsAndSites_42690
			};
		}

	}

})();