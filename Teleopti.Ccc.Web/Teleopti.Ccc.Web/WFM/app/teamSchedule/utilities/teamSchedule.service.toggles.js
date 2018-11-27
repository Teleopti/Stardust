(function () {
	'use strict';

	angular.module('wfm.teamSchedule').service('teamsToggles', teamsToggles);

	teamsToggles.$inject = ['Toggle'];

	function teamsToggles(toggleSvc) {
		var self = this;

		self.all = function getToggles() {
			return {
				Wfm_HideUnusedTeamsAndSites_42690: toggleSvc.Wfm_HideUnusedTeamsAndSites_42690,
				Wfm_GroupPages_45057: toggleSvc.Wfm_GroupPages_45057,
				WfmTeamSchedule_AddAbsenceFromPartOfDayToXDay_46010: toggleSvc.WfmTeamSchedule_AddAbsenceFromPartOfDayToXDay_46010,
				WfmTeamSchedule_ShowSkillsForSelectedSkillGroupInStaffingInfo_47202: toggleSvc.WfmTeamSchedule_ShowSkillsForSelectedSkillGroupInStaffingInfo_47202,
				WfmTeamSchedule_AddNDeleteDayOff_40555: toggleSvc.WfmTeamSchedule_AddNDeleteDayOff_40555,
				WfmTeamSchedule_RemoveShift_46322: toggleSvc.WfmTeamSchedule_RemoveShift_46322
			};
		}

	}

})();