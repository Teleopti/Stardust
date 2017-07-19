(function () {
	'use strict';

	angular
		.module('wfm.rta')
		.factory('rtaRouteService', rtaRouteService);

	rtaRouteService.$inject = ['$state', 'Toggle'];

	function rtaRouteService($state, Toggle) {

		var service = {
			goToSites: goToSites,
			goToSitesRefact: goToSitesRefact,
			goToTeams: goToTeams,
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

		function goToSites(skillId, skillAreaId) {
			if (angular.isDefined(skillId)) $state.go('rta.sites', { skillIds: skillId, skillAreaId: undefined });
			else if (angular.isDefined(skillAreaId)) $state.go('rta.sites', { skillIds: undefined, skillAreaId: skillAreaId });
			else $state.go('rta');
		}

		function goToSitesRefact() {
			$state.go('refact-rta');
		}

		function goToTeams(siteIds, skillId, skillAreaId) {
			if (angular.isDefined(skillId)) $state.go('rta.teams', { siteIds: siteIds, skillIds: skillId, skillAreaId: undefined });
			else if (angular.isDefined(skillAreaId)) $state.go('rta.teams', { siteIds: siteIds, skillIds: undefined, skillAreaId: skillAreaId });
			else $state.go('rta.teams', { siteIds: siteIds });
		}

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
