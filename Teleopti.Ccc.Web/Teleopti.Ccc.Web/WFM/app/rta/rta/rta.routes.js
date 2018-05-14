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
				return 'app/rta/rta/overview/view.RTA_ModifySkillGroup_48191.html';
			},
			controllerProvider: function () {
				return 'RtaOverviewController46933 as vm';
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
				if (toggles.RTA_ImprovedStateGroupFilter_48724)
					return 'app/rta/rta/agents/rta-agents.RTA_ImprovedStateGroupFilter_48724.html';
				return 'app/rta/rta/agents/rta-agents.RTA_ModifySkillGroup_48191.html';
			},
			controllerProvider: function () {
				if (toggles.RTA_ImprovedStateGroupFilter_48724)
					return 'RtaAgentsController48724 as vm';
				return 'RtaAgentsController46933 as vm';
			}
		})
		.state('rta-historical-without-date', {
			url: '/rta/agent-historical/:personId?open',
			controller: function ($http, $state, $stateParams) {
				$http.get('../api/HistoricalAdherence/MostRecentShiftDate',
					{params: {personId: $stateParams.personId}}
				).then(function (response) {
					$stateParams.date = response.data;
					$state.go('rta-historical', $stateParams, {location: 'replace'});
				});
			}
		})
		.state('rta-historical', {
			url: '/rta/agent-historical/:personId/:date?open',
			templateUrl: function () {
				if (toggles.RTA_RemoveApprovedOOA_47721)
					return 'app/rta/rta/historical/rta-historical.removeApprovedOOA_47721.html';
				return 'app/rta/rta/historical/rta-historical.approvePreviousOOA_47230.html';
			},
			controllerProvider: function () {
				if (toggles.RTA_RestrictModifyAdherenceWithPermission_74898)
					return 'RtaHistoricalController74898 as vm';
				if (toggles.RTA_RemoveApprovedOOA_47721)
					return 'RtaHistoricalController47721 as vm';
				return 'RtaHistoricalController47230 as vm';
			}
		})
		.state('rta-teams-legacy', {
			url: '/rta/teams/?siteIds&skillIds&skillAreaId&open',
			controller: function ($state, $stateParams) {
				$state.go('rta', $stateParams);
			}
		})
		.state("rta-skill-area-manager", {
			params: {
				isNewSkillArea: false,
				returnState: "rta-without-slash"
			},
			url: "/rta/skill-area-manager",
			templateUrl: "app/global/skill-group/skill-group-manager.html",
			controller: "SkillGroupManagerController as vm"
		})

});
