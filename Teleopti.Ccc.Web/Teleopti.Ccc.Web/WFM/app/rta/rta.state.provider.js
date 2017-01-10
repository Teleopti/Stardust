'use strict';

angular
	.module('wfm.rta')
	.provider('RtaState', function() {

		var toggles = {
			RTA_HideAgentsByStateGroup_40469: false,
			RTA_SiteAndTeamOnSkillOverview_40817: false,
			RTA_AgentsOnOrganizationAndSkills_41586: false,
			RTA_QuicklyChangeAgentsSelection_40610: false
		};
		var rtaAgentsTemplateUrl = function(elem, attr) {
			if (toggles.RTA_QuicklyChangeAgentsSelection_40610)
				return 'app/rta/agents/rta-agents-RTA_QuicklyChangeAgentsSelection_40610.html';
			if (toggles.RTA_AgentsOnOrganizationAndSkills_41586)
				return 'app/rta/agents/rta-agents-AgentsOnOrganizationAndSkills_41586.html';
			if (toggles.RTA_HideAgentsByStateGroup_40469)
				return 'app/rta/agents/rta-agents-HideAgentsByStateGroup_40469.html';
			return 'app/rta/agents/rta-agents.html';
		}

		var rtaSkillTemplateUrl = function(elem, attr) {
			if (toggles.RTA_QuicklyChangeAgentsSelection_40610)
				return 'app/rta/agents/rta-agents-RTA_QuicklyChangeAgentsSelection_40610.html';
			return 'app/rta/skills/rta-selectSkill.html'
		}

		var sitesBySkillTemplate = function(elem, attr) {
			return toggles.RTA_SiteAndTeamOnSkillOverview_40817 ? 'app/rta/overview/rta-sites-SiteOnSkillsOverview.html' : 'app/rta/overview/rta-sites.html';
		}

		this.$get = function() {
			return function(toggleService) {
				toggleService.togglesLoaded.then(function() {
					toggles.RTA_HideAgentsByStateGroup_40469 = toggleService.RTA_HideAgentsByStateGroup_40469;
					toggles.RTA_SiteAndTeamOnSkillOverview_40817 = toggleService.RTA_SiteAndTeamOnSkillOverview_40817;
					toggles.RTA_AgentsOnOrganizationAndSkills_41586 = toggleService.RTA_AgentsOnOrganizationAndSkills_41586;
					toggles.RTA_QuicklyChangeAgentsSelection_40610 = toggleService.RTA_QuicklyChangeAgentsSelection_40610;
				});
			};
		};

		this.config = function($stateProvider) {
			$stateProvider.state('rta', {
					url: '/rta',
					templateUrl: 'app/rta/rta.html'
				})
				.state('rta.select-skill', {
					url: '/select-skill/?siteIds&teamIds&skillIds&skillAreaId&showAllAgents&es',
					templateUrl: rtaSkillTemplateUrl,
					controller: 'RtaAgentsController as vm', //'RtaSelectSkillQuickSelectionCtrl',
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
				.state('rta.sites-by-skill', {
					url: '/sites-by-skill/?skillIds',
					templateUrl: 'app/rta/skills/rta-sites-bySkills.html',
					controller: 'RtaSiteAndTeamOnSkillOverviewController as vm'
				})
				.state('rta.sites-by-skillArea', {
					url: '/sites-by-skill-area/?skillAreaId',
					templateUrl: 'app/rta/skills/rta-sites-bySkills.html',
					controller: 'RtaSiteAndTeamOnSkillOverviewController as vm'
				})
				.state('rta.teams-by-skill', {
					url: '/teams-by-skill/?siteIds&skillIds',
					templateUrl: 'app/rta/skills/rta-teams-bySkills.html',
					controller: 'RtaSiteAndTeamOnSkillOverviewController as vm'
				})
				.state('rta.teams-by-skillArea', {
					url: '/teams-by-skill-area/?siteIds&skillAreaId',
					templateUrl: 'app/rta/skills/rta-teams-bySkills.html',
					controller: 'RtaSiteAndTeamOnSkillOverviewController as vm'
				})
				.state('rta.sites', {
					templateUrl: sitesBySkillTemplate,
					controller: 'RtaOverviewController as vm',
				})
				.state('rta.teams', {
					url: '/teams/:siteId',
					templateUrl: 'app/rta/overview/rta-teams.html',
					controller: 'RtaOverviewController as vm'
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
				})
				.state('rta.agent-details', {
					url: '/agent-details/:personId',
					templateUrl: 'app/rta/details/rta-agent-details.html',
					controller: 'RtaAgentDetailsController as vm'
				});
		};
	});
