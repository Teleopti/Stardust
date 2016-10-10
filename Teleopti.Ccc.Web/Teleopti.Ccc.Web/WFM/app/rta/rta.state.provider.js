'use strict';

angular.module('wfm.rta').provider('RtaState', function() {

	var toggles = {
		RTA_HideAgentsByStateGroup_40469: false
	};
	var rtaAgentsTemplateUrl = function(elem, attr) {
		return toggles.RTA_HideAgentsByStateGroup_40469 ? 'app/rta/agents/rta-agents-HideAgentsByStateGroup_40469.html' : 'app/rta/agents/rta-agents.html';
	}

	this.$get = function() {
		return function(toggleService) {
			toggleService.togglesLoaded.then(function() {
				toggles.RTA_HideAgentsByStateGroup_40469 = toggleService.RTA_HideAgentsByStateGroup_40469;
			});
		};
	};

	this.config = function($stateProvider) {
		$stateProvider.state('rta', {
				url: '/rta',
				templateUrl: 'app/rta/rta.html'
			})
			.state('rta.select-skill', {
				url: '/select-skill',
				templateUrl: 'app/rta/skills/rta-selectSkill.html',
				controller: 'RtaSelectSkillCtrl'
			})
			.state('rta.sites', {
				templateUrl: 'app/rta/overview/rta-sites.html',
				controller: 'RtaOverviewCtrl',
			})
			.state('rta.teams', {
				url: '/teams/:siteId',
				templateUrl: 'app/rta/overview/rta-teams.html',
				controller: 'RtaOverviewCtrl'
			})
			.state('rta.agents-view', {
				url: '/agents',
				templateUrl: rtaAgentsTemplateUrl,
				controller: 'RtaAgentsCtrl'
			})

			.state('rta.agents', {
				url: '/agents/:siteId/:teamId?showAllAgents&es',
				templateUrl: rtaAgentsTemplateUrl,
				controller: 'RtaAgentsCtrl',
				params: {
					es: {
						array: true
					}
				}
			})
			.state('rta.agents-teams', {
				url: '/agents-teams/?teamIds&es',
				templateUrl: rtaAgentsTemplateUrl,
				controller: 'RtaAgentsCtrl',
				params: {
					teamIds: {
						array: true
					},
					es: {
						array: true
					}
				}
			})
			.state('rta.agents-sites', {
				url: '/agents-sites/?siteIds&es',
				templateUrl: rtaAgentsTemplateUrl,
				controller: 'RtaAgentsCtrl',
				params: {
					siteIds: {
						array: true
					},
					es: {
						array: true
					}
				}
			})
			.state('rta.agents-skill-area', {
				url: '/agents-skill-area/:skillAreaId?es',
				templateUrl: rtaAgentsTemplateUrl,
				controller: 'RtaAgentsCtrl',
				params: {
					es: {
						array: true
					}
				}
			})
			.state('rta.agents-skill', {
				url: '/agents-skill/:skillId?es',
				templateUrl: rtaAgentsTemplateUrl,
				controller: 'RtaAgentsCtrl',
				params: {
					es: {
						array: true
					}
				}
			})
			.state('rta.historical', {
				url: '/historical/:personId',
				templateUrl: 'app/rta/historical/rta-historical-SeeAllOutOfAdherences_39146.html',
				controller: 'RtaHistoricalController as vm',
			})
			.state('rta.agent-details', {
				url: '/agent-details/:personId',
				templateUrl: 'app/rta/details/rta-agent-details.html',
				controller: 'RtaAgentDetailsCtrl'
			});
	};
});
