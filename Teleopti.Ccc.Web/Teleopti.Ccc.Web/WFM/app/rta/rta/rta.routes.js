'use strict';

angular.module('wfm.rta').config(function ($stateProvider, ToggleProvider) {

	var toggles = ToggleProvider;

	$stateProvider
		.state('rta-without-slash', {
			url: '/rta',
			controller: function ($state) {
				$state.go('rta');
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
				return 'app/rta/rta/overview/view.RTA_UnifiedSkillGroupManagement_45417.html';
			},
			controllerProvider: function () {
				if (toggles.RTA_ConfigurationValidationNotification_46933)
					return 'RtaOverviewController46933 as vm';
				return 'RtaOverviewController39082 as vm';
			},
			resolve: {
				skills: function (rtaService) {
					return rtaService.getSkills();
				},
				skillAreas: function (rtaService) {
					return rtaService.getSkillAreas();
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
				if (toggles.RTA_ViewHistoricalAhderenceForRecentShifts_46786)
					return 'app/rta/rta/agents/rta-agents.RTA_ViewHistoricalAhderenceForRecentShifts_46786.html';
				if (toggles.RTA_MobileFriendlyCheckboxes_46758)
					return 'app/rta/rta/agents/rta-agents.RTA_MobileFriendlyCheckboxes_46758.html';
				if (toggles.RTA_MonitorAgentsWithLongTimeInState_46475)
					return 'app/rta/rta/agents/rta-agents.RTA_MonitorAgentsWithLongTimeInState_46475.html';
				return 'app/rta/rta/agents/rta-agents.RTA_UnifiedSkillGroupManagement_45417.html';
			},
			controllerProvider: function () {
				if (toggles.RTA_ConfigurationValidationNotification_46933)
					return 'RtaAgentsController46933 as vm';
				if (toggles.RTA_ViewHistoricalAhderenceForRecentShifts_46786)
					return 'RtaAgentsController46786 as vm';
				if (toggles.RTA_MobileFriendlyCheckboxes_46758)
					return 'RtaAgentsController46758 as vm';
				if (toggles.RTA_MonitorAgentsWithLongTimeInState_46475)
					return 'RtaAgentsController46475 as vm';
				return 'RtaAgentsController as vm';
			}
		})
		.state('rta-historical-without-date', {
			url: '/rta/agent-historical/:personId?open',
			controller: function ($http, $state, $stateParams) {
				if (!toggles.RTA_ViewHistoricalAhderenceForRecentShifts_46786) {
					$state.go('rta-historical', $stateParams);
					return;
				}

				$http.get('../api/HistoricalAdherence/MostRecentShiftDate',
					{params: {personId: $stateParams.personId}}
				).then(function (response) {
					$stateParams.date = response.data;
					$state.go('rta-historical', $stateParams);
				});
			}
		})
		.state('rta-historical', {
			url: '/rta/agent-historical/:personId/:date?open',
			templateUrl: function () {
				if (toggles.RTA_ViewHistoricalAhderence7DaysBack_46826)
					return 'app/rta/rta/historical/rta-historical.adherence7DaysBack_46826.html';
				return 'app/rta/rta/historical/rta-historical.html';
			},
			controllerProvider: function () {
				if (toggles.RTA_ViewHistoricalAhderence7DaysBack_46826)
					return 'RtaHistoricalController46826 as vm';
				return 'RtaHistoricalController as vm';
			}
		})
		.state('rta-teams-legacy', {
			url: '/rta/teams/?siteIds&skillIds&skillAreaId&open',
			controller: function ($state, $stateParams) {
				$state.go('rta', $stateParams);
			}
		})
		.state("rta-skill-area-config", {
			params: {
				isNewSkillArea: false,
				returnState: "rta-without-slash"
			},
			url: "/rta/skill-area-config",
			templateUrl: "app/global/skill-group/skillgroup.html",
			controller: "SkillGroupController as vm"
		})

});
