(function () {
	'use strict';

	angular
		.module('wfm.rta')
		.factory('rtaRouteService', rtaRouteService);

	rtaRouteService.$inject = ['$state'];

	function rtaRouteService($state) {

		var service = {
			goToSites: goToSites,
			goToTeams: goToTeams,
			goToAgents: goToAgents,
			goToSelectSkill: goToSelectSkill,
			goToSitesBySkill: goToSitesBySkill,
			goToSitesBySkillArea: goToSitesBySkillArea,
			urlForChangingSchedule: urlForChangingSchedule,
			urlForHistoricalAdherence: urlForHistoricalAdherence,
			urlForSites: urlForSites,
			urlForSite: urlForSite,
			urlForTeams: urlForTeams,
			urlForSelectSkill: urlForSelectSkill,
			urlForSitesBySkills: urlForSitesBySkills,
			urlForTeamsBySkills: urlForTeamsBySkills,
			urlForSitesBySkillArea: urlForSitesBySkillArea,
			urlForTeamsBySkillArea: urlForTeamsBySkillArea,
			urlForRootInBreadcrumbs: urlForRootInBreadcrumbs,
			urlForTeamsInBreadcrumbs: urlForTeamsInBreadcrumbs
		}

		return service;
		////////////////////////

		function goToSites() { $state.go('rta'); };

		function goToTeams(siteId) { $state.go('rta.teams', { siteId: siteId }); };

		function goToAgents(ids) { $state.go('rta.agents', ids); };

		function goToSelectSkill() {
			$state.go('rta.select-skill', {
				skillIds: [],
				skillAreaId: [],
				siteIds: [],
				teamIds: []
			});
		};

		function goToSitesBySkill(skillId) { $state.go('rta.sites-by-skill', { skillIds: skillId }); };

		function goToSitesBySkillArea(skillAreaId) { $state.go('rta.sites-by-skillArea', { skillAreaId: skillAreaId }); };

		function urlForChangingSchedule(personId) { return "#/teams/?personId=" + personId; };

		function urlForHistoricalAdherence(personId) { return "#/rta/agent-historical/" + personId; };

		function urlForSites() { return '#/rta'; };

		function urlForSite(siteId) { return '#/rta/teams/' + siteId; };

		function urlForTeams(siteId) { return '#/rta/teams/' + siteId; };

		function urlForSelectSkill() { return '#/rta/select-skill'; };

		function urlForSitesBySkills(skillIds) { return '#/rta/sites-by-skill/?skillIds=' + skillIds; };

		function urlForTeamsBySkills(siteIds, skillIds) { return '#/rta/teams-by-skill/?siteIds=' + siteIds + '&skillIds=' + skillIds; };

		function urlForSitesBySkillArea(skillAreaId) { return '#/rta/sites-by-skill-area/?skillAreaId=' + skillAreaId; };

		function urlForTeamsBySkillArea(siteIds, skillAreaId) {
			return '#/rta/teams-by-skill-area/?siteIds=' + siteIds + '&skillAreaId=' + skillAreaId;
		};

		function urlForRootInBreadcrumbs(info) {
			if (info.skillAreaId != null)
				return urlForSitesBySkillArea(info.skillAreaId);
			if (info.skillIds.length > 0)
				return urlForSitesBySkills(info.skillIds[0]);
			return urlForSites();
		}

		function urlForTeamsInBreadcrumbs(info, siteId) {
			if (info.skillAreaId != null)
				return urlForTeamsBySkillArea(siteId, info.skillAreaId);
			if (info.skillIds.length > 0)
				return urlForTeamsBySkills(siteId, info.skillIds[0]);
			return urlForTeams(siteId);
		}
	};
})();
