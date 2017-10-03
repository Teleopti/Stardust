'use strict';

angular
	.module('wfm.rta')
	.provider('RtaState', function () {

		var toggles = {}

		this.$get = function () {
			return function (toggleService) {
				toggleService.togglesLoaded.then(function () {
					toggles = toggleService
				});
			};
		};

		this.config = function ($stateProvider) {
			$stateProvider
				.state('rta-without-slash', {
					url: '/rta',
					controller: function ($state) {
						$state.go('rta')
					}
				})
				.state('rta', {
					url: '/rta/?siteIds&teamIds&skillIds&skillAreaId&open',
					params: {
						siteIds: {array: true},
						teamIds: {array: true},
						skillIds: {array: true},
						es: {array: true}
					},
					templateUrl: function () {
						if (toggles.RTA_RememberMyPartOfTheBusiness_39082)
							return 'app/rta/rta/overview/view.RTA_RememberMyPartOfTheBusiness_39082.html';
						else
							return 'app/rta/rta/overview/view.html';
					},
					controllerProvider: function () {
						if (toggles.RTA_RememberMyPartOfTheBusiness_39082)
							return 'RtaOverviewController39082 as vm';
						else
							return 'RtaOverviewController as vm';
					},
					resolve: {
						skills: function (rtaService) {
							return rtaService.getSkills();
						},
						skillAreas: function (rtaService) {
							return rtaService.getSkillAreas().then(function (result) {
								return result.SkillAreas;
							});
						}
					}
				})
				.state('rta-agents', {
					url: '/rta/agents/?siteIds&teamIds&skillIds&skillAreaId&es&showAllAgents',
					params: {
						siteIds: {array: true},
						teamIds: {array: true},
						skillIds: {array: true},
						es: {array: true}
					},
					templateUrl: function () {
						if (toggles.RTA_RememberMyPartOfTheBusiness_39082)
							return 'app/rta/rta/agents/rta-agents.RTA_RememberMyPartOfTheBusiness_39082.html';
						else
							return 'app/rta/rta/agents/rta-agents.html';
					},
					controller: 'RtaAgentsController as vm'
				})
				.state('rta-historical', {
					url: '/rta/agent-historical/:personId?open',
					templateUrl: 'app/rta/historical/rta-historical.html',
					controller: 'RtaHistoricalController as vm',
				})

				.state('rta-teams-legacy', {
					url: '/rta/teams/?siteIds&skillIds&skillAreaId&open',
					controller: function ($state, $stateParams) {
						$state.go('rta', $stateParams)
					}
				})

				.state('rta-skill-group-config',
					{
						url: '/rta/skill-group-config',
						templateUrl: 'app/global/skill-group/skillgroup.html',
						params: {returnState: 'rta'},
						controller: 'SkillGroupController as vm'
					})

				.state('rta-filter-skill-group-config',
					{
						url: '/rta/agents/skill-group-config',
						templateUrl: 'app/global/skill-group/skillgroup.html',
						params: {returnState: 'rta-agents'},
						controller: 'SkillGroupController as vm'
					})
		};
	});
