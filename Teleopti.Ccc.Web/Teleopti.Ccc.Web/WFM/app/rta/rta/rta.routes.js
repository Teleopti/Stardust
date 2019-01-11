'use strict';

angular.module('wfm.rta').config(function ($stateProvider, ToggleProvider) {

	var toggles = ToggleProvider;

	$stateProvider
        .state('rta-adjust-adherence', {
            url: '/rta/adjust-adherence',
            templateUrl: function () {
                return 'app/rta/rta/historical-overview/rta.adjust.adherence.html';
            }
        })
		.state('rta-historical-overview', {
			url: '/rta/historical-overview?siteIds&teamIds',
			params: {
				siteIds: {array: true},
				teamIds: {array: true}
			},
			templateUrl: function () {
				return 'app/rta/rta/historical-overview/rta.historical.overview.html';
			},
			controllerProvider: function () {
				return 'RtaHistoricalOverviewController as vm';
			}
		})
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
				if (toggles.RTA_ImproveUsabilityExpandableCards_79025)
					return 'app/rta/rta/overview/view.ImproveUsabilityExpandableCards_79025.html';
				else if (toggles.RTA_RestrictModifySkillGroups_78568)
					return 'app/rta/rta/overview/view.RestrictModifySkillGroups_78568.html';
				else if (toggles.RTA_ReviewHistoricalAdherence_74770)
					return 'app/rta/rta/overview/view.ReviewHistoricalAdherence_74770.html';
				return 'app/rta/rta/overview/view.RTA_ModifySkillGroup_48191.html';
			},
			controllerProvider: function () {
				if (toggles.RTA_RestrictModifySkillGroups_78568)
					return 'RtaOverviewController78568 as vm';
				else if (toggles.RTA_ReviewHistoricalAdherence_74770)
					return 'RtaOverviewController74770 as vm';
				return 'RtaOverviewController46933 as vm';
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
				if (toggles.RTA_RestrictModifySkillGroups_78568)
					return 'app/rta/rta/agents/rta-agents.RestrictModifySkillGroups_78568.html';
				else if (toggles.RTA_ReviewHistoricalAdherence_74770)
					return 'app/rta/rta/agents/rta-agents.ReviewHistoricalAdherence_74770.html';
				return 'app/rta/rta/agents/rta-agents.RTA_ImprovedStateGroupFilter_48724.html';
			},
			controllerProvider: function () {
				if (toggles.RTA_RestrictModifySkillGroups_78568)
					return 'RtaAgentsController78568 as vm';
				else if (toggles.RTA_ReviewHistoricalAdherence_74770)
					return 'RtaAgentsController74770 as vm';
				return 'RtaAgentsController48724 as vm';
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
				if (toggles.RTA_InputValidationForApprovingAdherencePeriods_77045)
					return 'app/rta/rta/historical/rta-historical.inputValidationForApprovingAdherencePeriods_77045.html';
				return 'app/rta/rta/historical/rta-historical.durationOfHistoricalEvents_76470.html';
			},
			controllerProvider: function () {
				if (toggles.RTA_InputValidationForApprovingAdherencePeriods_77045)
					return 'RtaHistoricalController77045 as vm';
				return 'RtaHistoricalController76470 as vm';
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
				returnState: "rta"
			},
			url: "/rta/skill-area-manager",
			templateUrl: "app/global/skill-group/skill-group-manager.html",
			controller: "SkillGroupManagerController as vm"
		})

});
