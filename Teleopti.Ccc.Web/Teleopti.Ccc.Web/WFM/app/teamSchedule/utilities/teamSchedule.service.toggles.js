(function () {
	'use strict';

	angular.module('wfm.teamSchedule').service('teamsToggles', teamsToggles);

	teamsToggles.$inject = ['Toggle'];

	function teamsToggles(toggleSvc) {
		var self = this;

		self.all = function getToggles() {
			return {
				Wfm_HideUnusedTeamsAndSites_42690: toggleSvc.Wfm_HideUnusedTeamsAndSites_42690,
				WfmTeamSchedule_MoveOvertimeActivity_44888: toggleSvc.WfmTeamSchedule_MoveOvertimeActivity_44888,
				WfmTeamSchedule_SortRows_45056: toggleSvc.WfmTeamSchedule_SortRows_45056,
				WfmTeamSchedule_DisplayAndEditPublicNote_44783: toggleSvc.WfmTeamSchedule_DisplayAndEditPublicNote_44783,
				Wfm_GroupPages_45057: toggleSvc.Wfm_GroupPages_45057
			};
		}

	}

})();