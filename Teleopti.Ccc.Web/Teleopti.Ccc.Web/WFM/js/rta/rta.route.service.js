(function() {
	'use strict';

	angular.module('wfm.rta').service('RtaRouteService', ['$state',
		function($state) {
			this.goToSites = function() {
				$state.go('rta');
			};

			this.goToTeams = function(siteId) {
				$state.go('rta-teams', {
					siteId: siteId
				});
			};

			this.urlForChangingSchedule = function(businessUnitId, teamId, personId) {
				return "/Anywhere#teamschedule/" + businessUnitId + "/" + teamId + "/" + personId + "/" + moment().format("YYYYMMDD");
			};
		}
	]);
})();
