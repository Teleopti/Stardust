(function() {
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
			urlForAgentDetails: urlForAgentDetails,
			urlForSites: urlForSites,
			urlForSite: urlForSite,
			urlForTeams: urlForTeams,
			urlForSelectSkill: urlForSelectSkill,
			urlForSitesBySkills: urlForSitesBySkills,
			urlForTeamsBySkills: urlForTeamsBySkills,
			urlForSitesBySkillArea: urlForSitesBySkillArea,
			urlForTeamsBySkillArea: urlForTeamsBySkillArea
		}

		return service;
		////////////////////////

		function goToSites() {
			$state.go('rta');
		};

		function goToTeams(siteId) {
			$state.go('rta.teams', {
				siteId: siteId
			});
		};

		function goToAgents(ids) {
			$state.go('rta.agents', ids);
		};

		function goToSelectSkill() {
			$state.go('rta.select-skill', {
				skillIds: [],
				skillAreaId: [],
				siteIds: [],
				teamIds: []
			});
		};

		function goToSitesBySkill(skillId) {
			$state.go('rta.sites-by-skill', {
				skillIds: skillId
			});
		};

		function goToSitesBySkillArea(skillAreaId) {
			$state.go('rta.sites-by-skillArea', {
				skillAreaId: skillAreaId
			});
		};

		function urlForChangingSchedule(personId) {
			return "#/teams/?personId=" + personId;
		};

		function urlForHistoricalAdherence(personId) {
			return "#/rta/agent-historical/" + personId;
		};

		function urlForAgentDetails(personId) {
			return "#/rta/agent-details/" + personId;
		};

		function urlForSites() {
			return '#/rta';
		};

		function urlForSite(siteId) {
			return '#/rta/teams/' + siteId;
		};

		function urlForTeams(siteId) {
			return '#/rta/teams/' + siteId;
		};

		function urlForSelectSkill() {
			return '#/rta/select-skill';
		};

		function urlForSitesBySkills(skillIds) {
			return '#/rta/sites-by-skill/?skillIds=' + skillIds;
		};

		function urlForTeamsBySkills(siteIds, skillIds) {
			return '#/rta/teams-by-skill/?siteIds=' + siteIds + '&skillIds=' + skillIds;
		};

		function urlForSitesBySkillArea(skillAreaId) {
			return '#/rta/sites-by-skill-area/?skillAreaId=' + skillAreaId;
		};

		function urlForTeamsBySkillArea(siteIds, skillAreaId) {
			return '#/rta/teams-by-skill-area/?siteIds=' + siteIds + '&skillAreaId=' + skillAreaId;
		};
	};
})();
