'use strict';

angular.module('wfm.rta').provider('RtaState', function () {

	this.$get = function () {
		return function (toggleService) {
			toggleService.togglesLoaded.then(function () { });
		};
	};

	this.config = function ($stateProvider) {
		$stateProvider.state('rta',
			{
				url: '/rta',
				templateUrl: 'js/rta/rta.html'
			})
			.state('rta.select-skill',
			{
				url: '/select-skill',
				templateUrl: 'js/rta/skills/rta-selectSkill.html',
				controller: 'RtaSelectSkillCtrl'
			})
			.state('rta.sites',
			{
				templateUrl: 'js/rta/overview/rta-sites.html',
				controller: 'RtaOverviewCtrl',
			})
			.state('rta.teams',
			{
				url: '/teams/:siteId',
				templateUrl: 'js/rta/overview/rta-teams.html',
				controller: 'RtaOverviewCtrl'
			})
			.state('rta.agents-view',
			{
				url: '/agents',
				templateUrl: 'js/rta/agents/rta-agents-RTA_PauseButton_39144.html',
				controller: 'RtaAgentsCtrl'
			})
			.state('rta.agents',
			{
				url: '/agents/:siteId/:teamId?showAllAgents',
				templateUrl: 'js/rta/agents/rta-agents-RTA_PauseButton_39144.html',
				controller: 'RtaAgentsCtrl'
			})
			.state('rta.agents-teams',
			{
				url: '/agents-teams/?teamIds',
				templateUrl: 'js/rta/agents/rta-agents-RTA_PauseButton_39144.html',
				controller: 'RtaAgentsCtrl',
				params: {
					teamIds: {
						array: true
					}
				}
			})
			.state('rta.agents-sites',
			{
				url: '/agents-sites/?siteIds',
				templateUrl: 'js/rta/agents/rta-agents-RTA_PauseButton_39144.html',
				controller: 'RtaAgentsCtrl',
				params: {
					siteIds: {
						array: true
					}
				}
			})
			.state('rta.agents-skill-area',
			{
				url: '/agents-skill-area/:skillAreaId',
				templateUrl: 'js/rta/agents/rta-agents-RTA_MonitorBySkills_39081.html',
				controller: 'RtaAgentsCtrl',
			})
			.state('rta.agents-skill',
			{
				url: '/agents-skill/:skillId',
				templateUrl: 'js/rta/agents/rta-agents-RTA_MonitorBySkills_39081.html',
				controller: 'RtaAgentsCtrl'
			})		
			.state('rta.agent-details',
			{
				url: '/agent-details/:personId',
				templateUrl: 'js/rta/details/rta-agent-details.html',
				controller: 'RtaAgentDetailsCtrl'
			});
	};
});
