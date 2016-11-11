(function() {
	'use strict';
	angular.module('wfm.rta')
		.controller('RtaSiteAndTeamOnSkillOverviewCtrl', [
			'$scope',
			'$stateParams',
			'$interval',
			'$filter',
			'$sessionStorage',
			'$state',
			'$translate',
			'$timeout',
			'RtaOrganizationService',
			'RtaService',
			'RtaRouteService',
			'RtaFormatService',
			'RtaAdherenceService',
			'Toggle',
			function(
				$scope,
				$stateParams,
				$interval,
				$filter,
				$sessionStorage,
				$state,
				$translate,
				$timeout,
				RtaOrganizationService,
				RtaService,
				RtaRouteService,
				RtaFormatService,
				RtaAdherenceService,
				toggleService
			) {

				$scope.skills = [];
				$scope.skillAreas = [];
				$scope.skillsLoaded = false;
				$scope.skillAreasLoaded = false;
				$scope.skillId = $stateParams.skillIds || null;
				$scope.skillAreaId = $stateParams.skillAreaId || null;
				$scope.siteIds = $stateParams.siteIds || [];
				$scope.getAdherencePercent = RtaFormatService.numberToPercent;
				$scope.selectedItemIds = [];

				var stateForTeamsBySkill = 'rta.teams-by-skill({siteIds: site.Id, skillIds: selectedSkill.Id})';
				var stateForTeamsBySkillArea = 'rta.teams-by-skillArea({siteIds: site.Id, skillAreaId: selectedSkillArea.Id})';
				var stateForAgentsByTeamsAndSkill = 'rta.agents({teamIds: team.Id, skillIds: selectedSkill.Id})';
				var stateForAgentsByTeamsAndSkillArea = 'rta.agents({teamIds: team.Id, skillAreaId: selectedSkillArea.Id})';

				RtaService.getSkills()
					.then(function(skills) {
						$scope.skillsLoaded = true;
						$scope.skills = skills;
						if ($scope.skillId !== null) {
							$scope.selectedSkill = getSelected(skills, $scope.skillId);
							getSitesOrTeams();
						}
					});

				RtaService.getSkillAreas()
					.then(function(skillAreas) {
						$scope.skillAreasLoaded = true;
						$scope.skillAreas = skillAreas.SkillAreas;
						if ($scope.skillAreaId !== null) {
							$scope.selectedSkillArea = getSelected(skillAreas.SkillAreas, $scope.skillAreaId);
							getSitesOrTeams();
						}
					});

				function getSelected(outOf, shouldMatch) {
					return outOf.find(function(o) {
						return o.Id === shouldMatch;
					})
				};

				$scope.querySearch = function(query, myArray) {
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

				$scope.selectedSkillChange = function(skill) {
					if (!skill) return;

					if (skill) {
						$scope.skillId = skill.Id;
						doWhenSelecting(skill, $scope.selectedSkill, "rta.teams-by-skill", goToSitesBySkill);
					};
				};

				$scope.selectedSkillAreaChange = function(skillArea) {
					if (!skillArea) return

					if (skillArea) {
						$scope.skillAreaId = skillArea.Id;
						doWhenSelecting(skillArea, $scope.selectedSkillArea, "rta.teams-by-skillArea", goToSitesBySkillArea);
					};
				};

				function doWhenSelecting(item, selected, teamsStateName, goToSites) {
					selected = item;
					if ($state.current.name !== teamsStateName)
						goToSites(item.Id);
					getSitesOrTeams();
				};

				function getSitesOrTeams() {
					return $scope.siteIds.length > 0 ? getTeamsInfo() : getSitesInfo();
				}

				function getTeamsInfo() {
					return getTeamsForSkillOrSkillArea()
						.then(function(teams) {
							$scope.teams = teams;
							var teamIds = teams.map(function(team) {
								return team.Id;
							});
							return getAdherenceForTeamsBySkills($scope.siteIds, $scope.skillIds)
								.then(function(teamAdherences) {
									updateAdherence($scope.teams, teamAdherences);
								});
						})
				}

				function getSitesInfo() {
					return getSitesForSkillsOrSkillArea()
						.then(function(sites) {
							$scope.sites = sites;
							return getAdherenceForSitesBySkills($scope.skillIds);
						}).then(function(siteAdherences) {
							updateAdherence($scope.sites, siteAdherences);
						});
				}

				function getSitesForSkillsOrSkillArea() {
					$scope.skillIds = getSkillIds();
					return getSitesForSkills($scope.skillIds);
				}

				function getTeamsForSkillOrSkillArea() {
					$scope.skillIds = getSkillIds();
					return getTeamsForSiteAndSkills($scope.skillIds, $scope.siteIds);
				}

				function skillIdsFromSkillArea(skillAreaId) {
					return $scope.skillAreas.find(function(skillArea) {
							return skillArea.Id === skillAreaId;
						})
						.Skills.map(function(skill) {
							return skill.Id;
						});
				};

				function getSkillIds() {
					return $scope.skillAreaId !== null ? skillIdsFromSkillArea($scope.skillAreaId) : [$scope.skillId];
				}

				var polling = $interval(function() {
					if ($scope.skillId !== null || $scope.skillAreaId !== null) {
						if ($scope.siteIds.length > 0 && $scope.teams !== undefined) {
							getAdherenceForTeamsBySkills($scope.siteIds, $scope.skillIds)
								.then(function(teamAdherences) {
									updateAdherence($scope.teams, teamAdherences);
								});
						} else if ($scope.sites !== undefined) {
							getAdherenceForSitesBySkills($scope.skillIds)
								.then(function(siteAdherences) {
									updateAdherence($scope.sites, siteAdherences);
								});
						};
					}
				}, 5000);

				function getSitesForSkills(skillIds) {
					return RtaService.getSitesForSkills(skillIds);
				}

				function getTeamsForSiteAndSkills(skillIds, siteIds) {
					return RtaService.getTeamsForSiteAndSkills({
						skillIds: skillIds,
						siteIds: siteIds
					});
				};

				function getAdherenceForSitesBySkills(skillIds) {
					return RtaService.getAdherenceForSitesBySkills(skillIds);
				}

				function getAdherenceForTeamsBySkills(siteIds, skillIds) {
					return RtaService.getAdherenceForTeamsBySkills({
						skillIds: skillIds,
						siteIds: siteIds
					})
				};

				function updateAdherence(item, adh) {
					RtaAdherenceService.updateAdherence(item, adh);
				};

				$scope.goToOverview = function() {
					RtaRouteService.goToSites();
				};

				$scope.goToSelectSkill = function() {
					RtaRouteService.goToSelectSkill();
				};

				function goToSitesBySkill(skillId) {
					RtaRouteService.goToSitesBySkill(skillId);
				};

				function goToSitesBySkillArea(skillAreaId) {
					RtaRouteService.goToSitesBySkillArea(skillAreaId);
				};

				$scope.urlForSelectSkill = function() {
					return RtaRouteService.urlForSelectSkill();
				};

				$scope.goBackToRootWithUrl = $scope.skillId !== null ?
					RtaRouteService.urlForSitesBySkills($scope.skillId) :
					RtaRouteService.urlForSitesBySkillArea($scope.skillAreaId);

				$scope.getStateForTeams = function() {
					return $scope.skillId !== null ? stateForTeamsBySkill : stateForTeamsBySkillArea;
				};

				$scope.getStateForAgents = function() {
					return $scope.skillId !== null ? stateForAgentsByTeamsAndSkill : stateForAgentsByTeamsAndSkillArea;
				}

				if ($scope.siteIds.length > 0) {
					RtaOrganizationService.getSiteName($scope.siteIds)
						.then(function(name) {
							$scope.siteName = name;
						});
				};

				$scope.toggleSelection = function(itemId) {
					var index = $scope.selectedItemIds.indexOf(itemId);
					if (index > -1) {
						$scope.selectedItemIds.splice(index, 1);
					} else {
						$scope.selectedItemIds.push(itemId);
					}
				}

				$scope.openSelectedItems = function() {
					if ($scope.selectedItemIds.length > 0)
						goToAgents($scope.selectedItemIds);
				};

				function goToAgents(selectedItemIds) {
					var stateParamsObject = {};
					var skillOrSkillAreaKey = $scope.selectedSkill != null ? 'skillIds' : 'skillAreaId';
					var skillOrSkillAreaValue = $scope.selectedSkill != null ? $scope.skillIds : $scope.skillAreaId;
					var sitesOrTeamsKey = $scope.sites ? 'siteIds' : 'teamIds';
					var sitesOrTeamsValue = selectedItemIds;
					stateParamsObject[skillOrSkillAreaKey] = skillOrSkillAreaValue;
					stateParamsObject[sitesOrTeamsKey] = sitesOrTeamsValue;
					RtaRouteService.goToAgents(stateParamsObject);
				};

				$scope.$watch('selectedSkill', function(newValue, oldValue) {
					if (changed(newValue, oldValue) && toggleService.RTA_SiteAndTeamOnSkillOverview_40817) {
						$scope.goToOverview();
					}
				});

				$scope.$watch('selectedSkillArea', function(newValue, oldValue) {
					if (changed(newValue, oldValue) && toggleService.RTA_SiteAndTeamOnSkillOverview_40817) {
						$scope.goToOverview();
					}
				});

				function changed(newValue, oldValue) {
					return newValue !== oldValue && oldValue != undefined && oldValue != null && newValue == null;
				}

				$scope.$watch(
					function() {
						return $sessionStorage.buid;
					},
					function(newValue, oldValue) {
						if (oldValue !== undefined && newValue !== oldValue) {
							RtaRouteService.goToSites();
						}
					}
				);

				$scope.$on('$destroy', function() {
					$interval.cancel(polling);
				});
			}
		]);
})();
