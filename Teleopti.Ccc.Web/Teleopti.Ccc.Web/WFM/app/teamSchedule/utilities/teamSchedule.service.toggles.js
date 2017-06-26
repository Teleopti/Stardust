﻿(function () {
	'use strict';

	angular.module('wfm.teamSchedule').service('teamsToggles', teamsToggles);

	teamsToggles.$inject = ['Toggle'];

	function teamsToggles(toggleSvc) {
		var self = this;

		self.all = function getToggles() {
			return {
				Wfm_HideUnusedTeamsAndSites_42690: toggleSvc.Wfm_HideUnusedTeamsAndSites_42690,
				WfmTeamSchedule_MoveOvertimeActivity_44888: toggleSvc.WfmTeamSchedule_MoveOvertimeActivity_44888
			};
		}

	}

})();