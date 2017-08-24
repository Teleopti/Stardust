(function () {
	'use strict';

	angular
		.module('wfm.rta')
		.factory('rtaRouteService', rtaRouteService);

	rtaRouteService.$inject = ['$state', 'Toggle'];

	function rtaRouteService($state, Toggle) {

		var service = {
			goToOverview: function() { $state.go('refact-rta'); },
			goToAgents: goToAgents,
			goToSelectSkill: goToSelectSkill,
			urlForChangingSchedule: urlForChangingSchedule,
			urlForHistoricalAdherence: urlForHistoricalAdherence,
			urlForSites: urlForSites,
			urlForSelectSkill: urlForSelectSkill,
			urlForRootInBreadcrumbs: urlForRootInBreadcrumbs,
			urlForTeamsInBreadcrumbs: urlForTeamsInBreadcrumbs
		}

		return service;
		///////////////////////

		function goToAgents(ids) { $state.go('rta.agents', ids); };

		function goToSelectSkill() {
			$state.go('rta.agents', {
				skillIds: [],
				skillAreaId: [],
				siteIds: [],
				teamIds: []
			});
		};

		function urlForChangingSchedule(personId) { return "#/teams/?personId=" + personId; };

		function urlForHistoricalAdherence(personId) { return "#/rta/agent-historical/" + personId; };

		function urlForSelectSkill() { return '#/rta/select-skill'; };

		function urlForRootInBreadcrumbs(info) {
			return urlForSites(info.skillIds[0], info.skillAreaId);
		}

		function urlForTeamsInBreadcrumbs(info, siteId) {
			return urlForTeams(siteId, info.skillIds[0], info.skillAreaId);
		}

		function urlForSites(skillIds, skillAreaId) {
			if (skillIds !== null && angular.isDefined(skillIds)) return '#/rta/?skillIds=' + skillIds;
			else if (skillAreaId !== null && angular.isDefined(skillAreaId)) return '#/rta/?skillAreaId=' + skillAreaId;
			else return '#/rta';
		}

		function urlForTeams(siteIds, skillIds, skillAreaId) {
			if (skillAreaId !== null && angular.isDefined(skillAreaId)) return '#/rta/teams/?siteIds=' + siteIds + '&skillAreaId=' + skillAreaId;
			else if (skillIds !== null && angular.isDefined(skillIds)) return '#/rta/teams/?siteIds=' + siteIds + '&skillIds=' + skillIds;
			else return '#/rta/teams/?siteIds=' + siteIds;
		}
	};
})();
