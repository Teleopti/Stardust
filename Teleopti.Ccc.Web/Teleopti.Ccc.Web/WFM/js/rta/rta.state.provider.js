'use strict';

angular.module('wfm.rta').provider('RtaState', function() {
	var toggles = {
		Wfm_RTA_ProperAlarm_34975: false
	};
	var rtaAgentsTemplate = function(elem, attr) {
		if (toggles.Wfm_RTA_ProperAlarm_34975)
			return 'js/rta/rta-agents-ProperAlarm_34975.html';
		return 'js/rta/rta-agents.html';
	};
	this.$get = function() {
		return function(toggleService) {
			toggleService.togglesLoaded.then(function() {
				toggles.Wfm_RTA_ProperAlarm_34975 = toggleService.Wfm_RTA_ProperAlarm_34975
			});
		};
	};
	this.config = function($stateProvider) {
		$stateProvider.state('rta', {
			url: '/rta',
			templateUrl: 'js/rta/rta.html'
		}).state('rta.sites', {
			url: '/sites',
			templateUrl: 'js/rta/rta-sites.html',
			controller: 'RtaSitesCtrl',
		}).state('rta.teams', {
			url: '/teams/:siteId',
			templateUrl: 'js/rta/rta-teams.html',
			controller: 'RtaTeamsCtrl'
		}).state('rta.agents', {
			url: '/agents/:siteId/:teamId?showAllAgents',
			templateUrl: rtaAgentsTemplate,
			controller: 'RtaAgentsCtrl'
		}).state('rta.agents-teams', {
			url: '/agents-teams/?teamIds',
			templateUrl: rtaAgentsTemplate,
			controller: 'RtaAgentsCtrl',
			params: {
				teamIds: {
					array: true
				}
			}
		}).state('rta.agents-sites', {
			url: '/agents-sites/?siteIds',
			templateUrl: rtaAgentsTemplate,
			controller: 'RtaAgentsCtrl',
			params: {
				siteIds: {
					array: true
				}
			}
		}).state('rta.agent-details', {
			url: '/agent-details/:personId',
			templateUrl: 'js/rta/rta-agent-details.html',
			controller: 'RtaAgentDetailsCtrl'
		});
	};
});
