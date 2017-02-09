'use strict';

angular
	.module('wfm.rta')
	.provider('RtaState', function () {

		var toggles = {
			RTA_QuicklyChangeAgentsSelection_40610: false
		};
		var rtaAgentsTemplateUrl = function (elem, attr) {
			if (toggles.RTA_AgentsOnOrganizationAndSkills_41586)
				return 'app/rta/agents/rta-agents-AgentsOnOrganizationAndSkills_41586.html';
			return 'app/rta/agents/rta-agents.html';
		}

		var rtaSkillTemplateUrl = function (elem, attr) {
			if (toggles.RTA_QuicklyChangeAgentsSelection_40610)
				return 'app/rta/agents/rta-agents-RTA_QuicklyChangeAgentsSelection_40610.html';
			return 'app/rta/skills/rta-selectSkill.html'
		}

		this.$get = function () {
			return function (toggleService) {
				toggleService.togglesLoaded.then(function () {
					toggles.RTA_QuicklyChangeAgentsSelection_40610 = toggleService.RTA_QuicklyChangeAgentsSelection_40610;
				});
			};
		};

		this.config = function ($stateProvider) {
			$stateProvider.state('rta', {
				url: '/rta',
				templateUrl: 'app/rta/rta.html'
			})
				.state('rta.select-skill', {
					url: '/select-skill/?siteIds&teamIds&skillIds&skillAreaId&showAllAgents&es',
					templateUrl: rtaSkillTemplateUrl,
					controller: 'RtaAgentsController as vm',
					params: {
						siteIds: {
							array: true
						},
						teamIds: {
							array: true
						},
						skillIds: {
							array: true
						},
						es: {
							array: true
						}
					}
				})
				.state('rta.sites', {
					url: '/?skillIds&skillAreaId',
					templateUrl: 'app/rta/overview/rta-sites.html',
					controller: 'RtaOverviewController as vm',
					params: {
						skillIds: {
							array: false
						},
						skillAreaId: {
							array: false
						}
					}
				})
				.state('rta.teams', {
					url: '/teams/?siteIds&skillIds&skillAreaId',
					templateUrl: 'app/rta/overview/rta-teams.html',
					controller: 'RtaOverviewController as vm',
					params: {
						siteIds: {
							array: true
						},
						skillIds: {
							array: false
						},
						skillAreaId: {
							array: false
						}
					}
				})
				.state('rta.agents', {
					url: '/agents/?siteIds&teamIds&skillIds&skillAreaId&showAllAgents&es',
					templateUrl: rtaAgentsTemplateUrl,
					controller: 'RtaAgentsController as vm',
					params: {
						siteIds: {
							array: true
						},
						teamIds: {
							array: true
						},
						skillIds: {
							array: true
						},
						es: {
							array: true
						}
					}
				})
				.state('rta.historical', {
					url: '/agent-historical/:personId',
					templateUrl: 'app/rta/historical/rta-historical-SeeAllOutOfAdherences_39146.html',
					controller: 'RtaHistoricalController as vm',
				});
		};
	});
