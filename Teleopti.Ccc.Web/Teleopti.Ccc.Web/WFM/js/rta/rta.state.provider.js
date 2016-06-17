'use strict';

angular.module('wfm.rta').provider('RtaState', function() {
	var toggles = {
		RTA_AlarmContext_29357: false,
		RTA_PauseButton_9999: false,
		RTA_MonitorBySkills_39081: false
	};
	var rtaAgentsTemplate = function (elem, attr) {
		if (toggles.RTA_AlarmContext_29357) {
			if (toggles.RTA_PauseButton_39144) {
				return 'js/rta/rta-agents-RTA_PauseButton_39144.html';
			}
			if (toggles.RTA_MonitorBySkills_39081) {
				return 'js/rta/rta-agents-RTA_MonitorBySkills_39081.html';
			}
			return 'js/rta/rta-agents-RTA_AlarmContext_29357.html';
		}
		return 'js/rta/rta-agents.html';
	};

	var rtaSitesTemplate = function (elem, attr) {
		if (toggles.RTA_MonitorBySkills_39081) {
			return 'js/rta/rta-sites-RTA_MonitorBySkills_39081.html';
		}
		return 'js/rta/rta-sites.html';
	};

	this.$get = function() {
		return function(toggleService) {
			toggleService.togglesLoaded.then(function () {
				toggles.RTA_AlarmContext_29357 = toggleService.RTA_AlarmContext_29357;
				toggles.RTA_PauseButton_39144 = toggleService.RTA_PauseButton_39144;
				toggles.RTA_MonitorBySkills_39081 = toggleService.RTA_MonitorBySkills_39081;
			});
		};
	};
	this.config = function($stateProvider) {
		$stateProvider.state('rta', {
			url: '/rta',
			templateUrl: 'js/rta/rta.html'
		}).state('rta.sites', {
			templateUrl: rtaSitesTemplate,
			controller: 'RtaSitesCtrl',
		}).state('rta.teams', {
			url: '/teams/:siteId',
			templateUrl: 'js/rta/rta-teams.html',
			controller: 'RtaTeamsCtrl'
		}).state('rta.agents-view', {
				url: '/agents',
				templateUrl: rtaAgentsTemplate,
				controller: 'RtaAgentsCtrl'
		})
		.state('rta.agents', {
			url: '/agents/:siteId/:teamId?showAllAgents',
			templateUrl: rtaAgentsTemplate,
			controller: 'RtaAgentsCtrl'
		})
		.state('rta.agents-teams', {
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
