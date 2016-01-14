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
			templateUrl: 'js/rta/rta-sites.html',
			controller: 'RtaCtrl',
		}).state('rta-teams', {
			url: '/rta/teams/:siteId',
			templateUrl: 'js/rta/rta-teams.html',
			controller: 'RtaTeamsCtrl'
		}).state('rta-agents', {
			url: '/rta/agents/:siteId/:teamId?showAllAgents',
			templateUrl: rtaAgentsTemplate,
			controller: 'RtaAgentsCtrl'
		}).state('rta-agents-teams', {
			url: '/rta/agents-teams/?teamIds',
			templateUrl: rtaAgentsTemplate,
			controller: 'RtaAgentsCtrl',
			params: {
				teamIds: {
					array: true
				}
			}
		}).state('rta-agents-sites', {
			url: '/rta/agents-sites/?siteIds',
			templateUrl: rtaAgentsTemplate,
			controller: 'RtaAgentsCtrl',
			params: {
				siteIds: {
					array: true
				}
			}
		}).state('rta-agent-details', {
			url: '/rta/agent-details/:personId',
			templateUrl: 'js/rta/rta-agent-details.html',
			controller: 'RtaAgentDetailsCtrl'
		});
	};
});
