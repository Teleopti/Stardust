(function() {
	'use strict';

	angular.module('wfm.rta').service('RtaRouteService', ['$state',
		function($state) {
			this.goToSites = function() {
				$state.go('rta');
			};

			this.goToTeams = function(siteId) {
				$state.go('rta.teams', {
					siteId: siteId
				});
			};

			this.goToAgents = function(ids) {
				$state.go('rta.agents', ids);
			};

			this.goToSelectSkill = function(){
				$state.go('rta.select-skill');
			};

			this.goToSitesBySkill = function(skillId) {
				$state.go('rta.sites-by-skill', {skillIds: skillId});
			};

			this.goToSitesBySkillArea = function(skillAreaId) {
				$state.go('rta.sites-by-skillArea', {skillAreaId: skillAreaId});
			};

			this.urlForChangingSchedule = function(personId) {
				return "#/teams/?personId=" + personId;
			};

			this.urlForHistoricalAdherence = function (personId) {
				return "#/rta/agent-historical/" + personId;
			};

			this.urlForAgentDetails = function(personId) {
				return "#/rta/agent-details/" + personId;
			};

			this.urlForSites = function(){
				return '#/rta';
			};

			this.urlForTeams = function(siteId){
				return '#/rta/teams/' + siteId;
			};

			this.urlForSelectSkill = function(){
				return '#/rta/select-skill';
			};

		}
	]);
})();
