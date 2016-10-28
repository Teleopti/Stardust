(function () {
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
			function (
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
				toggleService.togglesLoaded.then(function () { //<<..toggle service start here!
					$scope.skills = [];
					$scope.skillAreas = [];
					$scope.skillsLoaded = false;
					$scope.skillAreasLoaded = false;
					$scope.skillId = $stateParams.skillIds || null;
					$scope.skillAreaId = $stateParams.skillAreaId || null;
					$scope.siteIds = $stateParams.siteIds || [];
					$scope.getAdherencePercent = RtaFormatService.numberToPercent;
					$scope.isSitesOverview = $scope.siteIds.length === 0 ? true : false;
					$scope.selectedItemIds = [];

					var stateForTeamsBySkill = 'rta.teams-by-skill({siteIds: site.Id, skillIds: selectedSkill.Id})';
					var stateForTeamsBySkillArea = 'rta.teams-by-skillArea({siteIds: site.Id, skillAreaId: selectedSkillArea.Id})';

					RtaService.getSkills()
						.then(function (skills) {
							$scope.skillsLoaded = true;
							$scope.skills = skills;
							if ($scope.skillId !== null && toggleService.RTA_SiteAndTeamOnSkillOverview_40817) {
								$scope.selectedSkill = getSelected(skills, $scope.skillId);
								getSitesOrTeams();
							}
						});

					RtaService.getSkillAreas()
						.then(function (skillAreas) {
							$scope.skillAreasLoaded = true;
							$scope.skillAreas = skillAreas.SkillAreas;
							if ($scope.skillAreaId !== null && toggleService.RTA_SiteAndTeamOnSkillOverview_40817) {
								$scope.selectedSkillArea = getSelected(skillAreas.SkillAreas, $scope.skillAreaId);
								getSitesOrTeams();
							}
						});

					function getSelected(outOf, shouldMatch) {
						return outOf.find(function (o) {
							return o.Id === shouldMatch;
						})
					};

					$scope.querySearch = function (query, myArray) {
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

					$scope.selectedSkillChange = function (skill) {
						if (!skill) return;

						if (toggleService.RTA_SiteAndTeamOnSkillOverview_40817) {
							$scope.skillId = skill.Id;
							doWhenSelecting(skill, $scope.selectedSkill, "rta.teams-by-skill", goToSitesBySkill);
						}
						else
							$timeout(function () {
								$state.go('rta.agents-skill', { skillId: skill.Id });
							});
					};

					$scope.selectedSkillAreaChange = function (skillArea) {
						if (!skillArea) return

						if (toggleService.RTA_SiteAndTeamOnSkillOverview_40817) {
							$scope.skillAreaId = skillArea.Id;
							doWhenSelecting(skillArea, $scope.selectedSkillArea, "rta.teams-by-skillArea", goToSitesBySkillArea);
						}
						else
							$timeout(function () {
								$state.go('rta.agents-skill-area', { skillAreaId: skillArea.Id });
							});
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
							.then(function (teams) {
								$scope.teams = teams;
								var teamIds = teams.map(function (team) {
									return team.Id;
								});
								return getAdherenceForTeamsBySkills($scope.siteIds, $scope.skillIds)
									.then(function (teamAdherences) {
										updateAdherence($scope.teams, teamAdherences);
									});
							})
					}

					function getSitesInfo() {
						return getSitesForSkillsOrSkillArea()
							.then(function (sites) {
								$scope.sites = sites;
								return getAdherenceForSitesBySkills($scope.skillIds);
							}).then(function (siteAdherences) {
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
						return $scope.skillAreas.find(function (skillArea) {
							return skillArea.Id === skillAreaId;
						})
							.Skills.map(function (skill) {
								return skill.Id;
							});
					};

					function getSkillIds() {
						return $scope.skillAreaId !== null ? skillIdsFromSkillArea($scope.skillAreaId) : [$scope.skillId];
					}

					var polling = toggleService.RTA_SiteAndTeamOnSkillOverview_40817 ? $interval(function () {
						if ($scope.skillId !== null || $scope.skillAreaId !== null) {
							if ($scope.siteIds.length > 0 && $scope.teams !== undefined) {
								getAdherenceForTeamsBySkills($scope.siteIds, $scope.skillIds)
									.then(function (teamAdherences) {
										updateAdherence($scope.teams, teamAdherences);
									});
							}
							else if ($scope.sites !== undefined) {
								getAdherenceForSitesBySkills($scope.skillIds)
									.then(function (siteAdherences) {
										updateAdherence($scope.sites, siteAdherences);
									});
							};
						}
					}, 5000) : {};

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

					$scope.goToOverview = function () {
						RtaRouteService.goToSites();
					};

					function goToSelectSkill() {
						RtaRouteService.goToSelectSkill();
					};

					function goToSitesBySkill(skillId) {
						RtaRouteService.goToSitesBySkill(skillId);
					};

					function goToSitesBySkillArea(skillAreaId) {
						RtaRouteService.goToSitesBySkillArea(skillAreaId);
					};

					$scope.urlForSelectSkill = function () {
						return RtaRouteService.urlForSelectSkill();
					};

					$scope.getStateForTeams = function () {
						return $scope.skillId !== null ? stateForTeamsBySkill : stateForTeamsBySkillArea;
					};

					if ($scope.siteIds.length > 0) {
						RtaOrganizationService.getSiteName($scope.siteIds)
							.then(function (name) {
								$scope.siteName = name;
							});
					};

					$scope.toggleSelection = function (itemId) {
						var index = $scope.selectedItemIds.indexOf(itemId);
						if (index > -1) {
							$scope.selectedItemIds.splice(index, 1);
						} else {
							$scope.selectedItemIds.push(itemId);
						}
					}

					/*go to parent state for now until feature for agents is implemented*/
					$scope.openSelected = function () {
						var stateParamsObj = {};
						if($scope.siteIds.length > 0 ){
							stateParamsObj = { skillIds: getSkillIds(), siteIds: [$scope.siteIds], teamIds: $scope.selectedItemIds }
						}else{
							stateParamsObj = { skillIds: getSkillIds(), siteIds: $scope.selectedItemIds }
						}
						console.log(stateParamsObj);
						goToSelectSkill();
					}

					$scope.$watch('selectedSkill', function (newValue, oldValue) {
						if (changed(newValue, oldValue) && toggleService.RTA_SiteAndTeamOnSkillOverview_40817) {
							goToSelectSkill();
						}
					});

					$scope.$watch('selectedSkillArea', function (newValue, oldValue) {
						if (changed(newValue, oldValue) && toggleService.RTA_SiteAndTeamOnSkillOverview_40817) {
							goToSelectSkill();
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
								RtaRouteService.goToSites();
							}
						}
					);

					$scope.$on('$destroy', function () {
						$interval.cancel(polling);
					});
				}); //toogle service ends here....>>
			}
		]);
})();
