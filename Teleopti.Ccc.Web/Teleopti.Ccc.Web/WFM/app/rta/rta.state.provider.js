'use strict';

angular.module('wfm.rta').provider('RtaState', function() {

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

	//templates for refact
	// var rtaAgentsTemplateUrlRefact = function (elem, attr) {
	// 	if(toggles.RTA_QuicklyChangeAgentsSelection_40610)
	// 		return 'app/rta/refact/agentsrefact/rta-agents-RTA_QuicklyChangeAgentsSelection_40610.refact.html';
	// 	if (toggles.RTA_AgentsOnOrganizationAndSkills_41586)
	// 		return 'app/rta/refact/agentsrefact/rta-agents-AgentsOnOrganizationAndSkills_41586.refact.html';
	// 	if (toggles.RTA_HideAgentsByStateGroup_40469)
	// 		return 'app/rta/refact/agentsrefact/rta-agents-HideAgentsByStateGroup_40469.refact.html';
	// 	return 'app/rta/refact/agentsrefact/rta-agents.refact.html';
	// }
	//
	// var rtaSkillTemplateUrlRefact = function (elem, attr) {
	// 	if(toggles.RTA_QuicklyChangeAgentsSelection_40610)
	// 		return 'app/rta/refact/agentsrefact/rta-agents-RTA_QuicklyChangeAgentsSelection_40610.refact.html';
	// 	return 'app/rta/refact/skillsrefact/rta-selectSkill.refact.html';
	// }
	//
	// var sitesBySkillTemplateRefact = function (elem, attr) {
	// 	return toggles.RTA_SiteAndTeamOnSkillOverview_40817 ? 'app/rta/refact/overviewrefact/rta-sites-SiteOnSkillsOverview.refact.html' : 'app/rta/refact/overviewrefact/rta-sites.refact.html';
	// }

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
				controller: 'RtaAgentsCtrl',//'RtaSelectSkillQuickSelectionCtrl',
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
				controller: 'RtaSiteAndTeamOnSkillOverviewCtrl'
			})
			.state('rta.sites-by-skillArea', {
				url: '/sites-by-skill-area/?skillAreaId',
				templateUrl: 'app/rta/skills/rta-sites-bySkills.html',
				controller: 'RtaSiteAndTeamOnSkillOverviewCtrl'
			})
			.state('rta.teams-by-skill', {
				url: '/teams-by-skill/?siteIds&skillIds',
				templateUrl: 'app/rta/skills/rta-teams-bySkills.html',
				controller: 'RtaSiteAndTeamOnSkillOverviewCtrl'
			})
			.state('rta.teams-by-skillArea', {
				url: '/teams-by-skill-area/?siteIds&skillAreaId',
				templateUrl: 'app/rta/skills/rta-teams-bySkills.html',
				controller: 'RtaSiteAndTeamOnSkillOverviewCtrl'
			})
			.state('rta.sites', {
				templateUrl: sitesBySkillTemplate,
				controller: 'RtaOverviewCtrl',
			})
			.state('rta.teams', {
				url: '/teams/:siteId',
				templateUrl: 'app/rta/overview/rta-teams.html',
				controller: 'RtaOverviewCtrl'
			})
			.state('rta.agents', {
				url: '/agents/?siteIds&teamIds&skillIds&skillAreaId&showAllAgents&es',
				templateUrl: rtaAgentsTemplateUrl,
				controller: 'RtaAgentsCtrl',
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
				controller: 'RtaAgentDetailsCtrl'
			})



		//states for refactored
		// .state('rta.select-skill', {
		// 		url: '/select-skill/?siteIds&teamIds&skillIds&skillAreaId&showAllAgents&es',
		// 		templateUrl: rtaSkillTemplateUrlRefact,
		// 		controller: 'RtaAgentsCtrlRefact as vm', //'RtaSelectSkillQuickSelectionCtrl',
		// 		params: {
		// 			siteIds: {
		// 				array: true
		// 			},
		// 			teamIds: {
		// 				array: true
		// 			},
		// 			skillIds: {
		// 				array: true
		// 			},
		// 			es: {
		// 				array: true
		// 			}
		// 		}
		// 	})
		// 	.state('rta.sites-by-skill', {
		// 		url: '/sites-by-skill/?skillIds',
		// 		templateUrl: 'app/rta/refact/skillsrefact/rta-sites-bySkills.refact.html',
		// 		controller: 'RtaSiteAndTeamOnSkillOverviewCtrlRefact as vm'
		// 	})
		// 	.state('rta.sites-by-skillArea', {
		// 		url: '/sites-by-skill-area/?skillAreaId',
		// 		templateUrl: 'app/rta/refact/skillsrefact/rta-sites-bySkills.refact.html',
		// 		controller: 'RtaSiteAndTeamOnSkillOverviewCtrlRefact as vm'
		// 	})
		// 	.state('rta.teams-by-skill', {
		// 		url: '/teams-by-skill/?siteIds&skillIds',
		// 		templateUrl: 'app/rta/refact/skillsrefact/rta-teams-bySkills.refact.html',
		// 		controller: 'RtaSiteAndTeamOnSkillOverviewCtrlRefact as vm'
		// 	})
		// 	.state('rta.teams-by-skillArea', {
		// 		url: '/teams-by-skill-area/?siteIds&skillAreaId',
		// 		templateUrl: 'app/rta/refact/skillsrefact/rta-teams-bySkills.refact.html',
		// 		controller: 'RtaSiteAndTeamOnSkillOverviewCtrlRefact as vm'
		// 	})
		// 	.state('rta.sites', {
		// 		templateUrl: sitesBySkillTemplateRefact,
		// 		controller: 'RtaOverviewCtrlRefact as vm',
		// 	})
		// 	.state('rta.teams', {
		// 		url: '/teams/:siteId',
		// 		templateUrl: 'app/rta/refact/overviewrefact/rta-teams.refact.html',
		// 		controller: 'RtaOverviewCtrlRefact as vm'
		// 	})
		// 	.state('rta.agents', {
		// 		url: '/agents/?siteIds&teamIds&skillIds&skillAreaId&showAllAgents&es',
		// 		templateUrl: rtaAgentsTemplateUrlRefact,
		// 		controller: 'RtaAgentsCtrlRefact as vm',
		// 		params: {
		// 			siteIds: {
		// 				array: true
		// 			},
		// 			teamIds: {
		// 				array: true
		// 			},
		// 			skillIds: {
		// 				array: true
		// 			},
		// 			es: {
		// 				array: true
		// 			}
		// 		}
		// 	})
		// 	.state('rta.agent-details', {
		// 		url: '/agent-details/:personId',
		// 		templateUrl: 'app/rta/refact/detailsrefact/rta-agent-details.refact.html',
		// 		controller: 'RtaAgentDetailsCtrlRefact as vm'
		// 	});
	};
});
