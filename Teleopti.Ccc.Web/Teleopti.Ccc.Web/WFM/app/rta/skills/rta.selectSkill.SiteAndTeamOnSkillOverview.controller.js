(function () {
	'use strict';
	angular.module('wfm.rta')
		.controller('RtaSiteAndTeamOnSkillOverviewController', RtaSiteAndTeamOnSkillOverviewController)

		RtaSiteAndTeamOnSkillOverviewController.$inject =
		[
			'$scope',
			'$stateParams',
			'$interval',
			'$filter',
			'$sessionStorage',
			'$state',
			'$translate',
			'$timeout',
			'rtaOrganizationService',
			'rtaService',
			'rtaRouteService',
			'rtaFormatService',
			'rtaAdherenceService',
			'Toggle'
		]

			function RtaSiteAndTeamOnSkillOverviewController (
				$scope,
				$stateParams,
				$interval,
				$filter,
				$sessionStorage,
				$state,
				$translate,
				$timeout,
				rtaOrganizationService,
				rtaService,
				rtaRouteService,
				rtaFormatService,
				rtaAdherenceService,
				toggleService
			) {

				var vm = this;

				vm.skills = [];
				vm.skillAreas = [];
				vm.skillsLoaded = false;
				vm.skillAreasLoaded = false;
				vm.skillId = $stateParams.skillIds || null;
				vm.skillAreaId = $stateParams.skillAreaId || null;
				vm.siteIds = $stateParams.siteIds || [];
				vm.getAdherencePercent = rtaFormatService.numberToPercent;
				vm.selectedItemIds = [];

				var stateForTeamsBySkill = 'rta.teams-by-skill({siteIds: site.Id, skillIds: vm.selectedSkill.Id})';
				var stateForTeamsBySkillArea = 'rta.teams-by-skillArea({siteIds: site.Id, skillAreaId: vm.selectedSkillArea.Id})';
				var stateForAgentsByTeamsAndSkill = 'rta.agents({teamIds: team.Id, skillIds: vm.selectedSkill.Id})';
				var stateForAgentsByTeamsAndSkillArea = 'rta.agents({teamIds: team.Id, skillAreaId: vm.selectedSkillArea.Id})';

				rtaService.getSkills()
					.then(function (skills) {
						vm.skillsLoaded = true;
						vm.skills = skills;
						if (vm.skillId !== null) {
							vm.selectedSkill = getSelected(skills, vm.skillId);
							getSitesOrTeams();
						}
					});

				rtaService.getSkillAreas()
					.then(function (skillAreas) {
						vm.skillAreasLoaded = true;
						vm.skillAreas = skillAreas.SkillAreas;
						if (vm.skillAreaId !== null) {
							vm.selectedSkillArea = getSelected(skillAreas.SkillAreas, vm.skillAreaId);
							getSitesOrTeams();
						}
					});

				function getSelected(outOf, shouldMatch) {
					return outOf.find(function (o) {
						return o.Id === shouldMatch;
					})
				};

				vm.querySearch = function (query, myArray) {
					var results = query ? myArray.filter(createFilterFor(query)) : myArray;
					return results;
				};

				function createFilterFor(query) {
					var lowercaseQuery = angular.lowercase(query);
					return function filterFn(item) {
						var lowercaseName = angular.lowercase(item.Name);
						return (lowercaseName.indexOf(lowercaseQuery) === 0);
					};
				};

				vm.selectedSkillChange = function (skill) {
					if (!skill) return;
					if (skill.Id == vm.skillId) return;

					vm.skillId = skill.Id;
					doWhenSelecting(skill, vm.selectedSkill, "rta.teams-by-skill", goToSitesBySkill);
				};

				vm.selectedSkillAreaChange = function (skillArea) {
					if (!skillArea) return;
					if (skillArea.Id == vm.skillAreaId) return;

					vm.skillAreaId = skillArea.Id;
					doWhenSelecting(skillArea, vm.selectedSkillArea, "rta.teams-by-skillArea", goToSitesBySkillArea);

				};

				function doWhenSelecting(item, selected, teamsStateName, goToSites) {
					selected = item;
					if ($state.current.name !== teamsStateName)
						goToSites(item.Id);
					getSitesOrTeams();
				};

				function getSitesOrTeams() {
					return vm.siteIds.length > 0 ? getTeamsInfo() : getSitesInfo();
				}

				function getTeamsInfo() {
					return getTeamsForSkillOrSkillArea()
						.then(function (teams) {
							vm.teams = teams;
							var teamIds = teams.map(function (team) {
								return team.Id;
							});
							return getAdherenceForTeamsBySkills(vm.siteIds, vm.skillIds)
								.then(function (teamAdherences) {
									updateAdherence(vm.teams, teamAdherences);
								});
						})
				}

				function getSitesInfo() {
					return getSitesForSkillsOrSkillArea()
						.then(function (sites) {
							vm.sites = sites;
							return getAdherenceForSitesBySkills(vm.skillIds);
						}).then(function (siteAdherences) {
							updateAdherence(vm.sites, siteAdherences);
						});
				}

				function getSitesForSkillsOrSkillArea() {
					vm.skillIds = getSkillIds();
					return getSitesForSkills(vm.skillIds);
				}

				function getTeamsForSkillOrSkillArea() {
					vm.skillIds = getSkillIds();
					return getTeamsForSiteAndSkills(vm.skillIds, vm.siteIds);
				}

				function skillIdsFromSkillArea(skillAreaId) {
					return vm.skillAreas.find(function (skillArea) {
						return skillArea.Id === skillAreaId;
					})
						.Skills.map(function (skill) {
							return skill.Id;
						});
				};

				function getSkillIds() {
					return vm.skillAreaId !== null ? skillIdsFromSkillArea(vm.skillAreaId) : [vm.skillId];
				}

				var polling = $interval(function () {
					if (vm.skillId !== null || vm.skillAreaId !== null) {
						if (vm.siteIds.length > 0 && vm.teams !== undefined) {
							getAdherenceForTeamsBySkills(vm.siteIds, vm.skillIds)
								.then(function (teamAdherences) {
									updateAdherence(vm.teams, teamAdherences);
								});
						} else if (vm.sites !== undefined) {
							getAdherenceForSitesBySkills(vm.skillIds)
								.then(function (siteAdherences) {
									updateAdherence(vm.sites, siteAdherences);
								});
						};
					}
				}, 5000);

				function getSitesForSkills(skillIds) {
					return rtaService.getSitesForSkills(skillIds);
				}

				function getTeamsForSiteAndSkills(skillIds, siteIds) {
					return rtaService.getTeamsForSiteAndSkills({
						skillIds: skillIds,
						siteIds: siteIds
					});
				};

				function getAdherenceForSitesBySkills(skillIds) {
					return rtaService.getAdherenceForSitesBySkills(skillIds);
				}

				function getAdherenceForTeamsBySkills(siteIds, skillIds) {
					return rtaService.getAdherenceForTeamsBySkills({
						skillIds: skillIds,
						siteIds: siteIds
					})
				};

				function updateAdherence(item, adh) {
					rtaAdherenceService.updateAdherence(item, adh);
				};

				vm.goToDashboard = function () {
					rtaRouteService.goToSites();
				};

				vm.goToSelectSkill = function () {
					rtaRouteService.goToSelectSkill();
				};

				function goToSitesBySkill(skillId) {
					rtaRouteService.goToSitesBySkill(skillId);
				};

				function goToSitesBySkillArea(skillAreaId) {
					rtaRouteService.goToSitesBySkillArea(skillAreaId);
				};

				vm.urlForSelectSkill = function () {
					return rtaRouteService.urlForSelectSkill();
				};

				vm.goBackToRootWithUrl = vm.skillId !== null ?
					rtaRouteService.urlForSitesBySkills(vm.skillId) :
					rtaRouteService.urlForSitesBySkillArea(vm.skillAreaId);

				vm.getStateForTeams = function () {
					return vm.skillId !== null ? stateForTeamsBySkill : stateForTeamsBySkillArea;
				};

				vm.getStateForAgents = function () {
					return vm.skillId !== null ? stateForAgentsByTeamsAndSkill : stateForAgentsByTeamsAndSkillArea;
				}

				if (vm.siteIds.length > 0) {
					rtaOrganizationService.getSiteName(vm.siteIds)
						.then(function (name) {
							vm.siteName = name;
						});
				};

				vm.toggleSelection = function (itemId) {
					var index = vm.selectedItemIds.indexOf(itemId);
					if (index > -1) {
						vm.selectedItemIds.splice(index, 1);
					} else {
						vm.selectedItemIds.push(itemId);
					}
				}

				vm.openSelectedItems = function () {
					if (vm.selectedItemIds.length > 0)
						goToAgents(vm.selectedItemIds);
				};

				function goToAgents(selectedItemIds) {
					var stateParamsObject = {};
					var skillOrSkillAreaKey = vm.selectedSkill != null ? 'skillIds' : 'skillAreaId';
					var skillOrSkillAreaValue = vm.selectedSkill != null ? vm.skillIds : vm.skillAreaId;
					var sitesOrTeamsKey = vm.sites ? 'siteIds' : 'teamIds';
					var sitesOrTeamsValue = selectedItemIds;
					stateParamsObject[skillOrSkillAreaKey] = skillOrSkillAreaValue;
					stateParamsObject[sitesOrTeamsKey] = sitesOrTeamsValue;
					rtaRouteService.goToAgents(stateParamsObject);
				};

				$scope.$watch(function() {
					return vm.selectedSkill;
				}, function (newValue, oldValue) {
					if (changed(newValue, oldValue) && toggleService.RTA_SiteAndTeamOnSkillOverview_40817) {
						vm.goToDashboard();
					}
				});

				$scope.$watch(function() {
					return vm.selectedSkillArea;
				}, function (newValue, oldValue) {
					if (changed(newValue, oldValue) && toggleService.RTA_SiteAndTeamOnSkillOverview_40817) {
						vm.goToDashboard();
					}
				});

				function changed(newValue, oldValue) {
					return newValue !== oldValue && oldValue != undefined && oldValue != null && newValue == null;
				}

				$scope.$watch(
					function () {
						return $sessionStorage.buid;
					},
					function (newValue, oldValue) {
						if (oldValue !== undefined && newValue !== oldValue) {
							rtaRouteService.goToSites();
						}
					}
				);

				$scope.$on('$destroy', function () {
					$interval.cancel(polling);
				});
			};
})();
