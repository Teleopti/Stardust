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
				RtaAdherenceService
			) {
				$scope.skills = [];
				$scope.skillAreas = [];
				$scope.skillsLoaded = false;
				$scope.skillAreasLoaded = false;
				$scope.skillId = $stateParams.skillIds || null;
				$scope.skillAreaId = $stateParams.skillAreaId || null;
				$scope.siteIds = $stateParams.siteIds || [];
				$scope.getAdherencePercent = RtaFormatService.numberToPercent;
				$scope.isSitesOverview = $scope.siteIds.length === 0 ? true : false;
				var stateForTeamsBySkill = 'rta.teams-by-skill({siteIds: site.Id, skillIds: selectedSkill.Id})';
				var stateForTeamsBySkillArea = 'rta.teams-by-skillArea({siteIds: site.Id, skillAreaId: selectedSkillArea.Id})';

				RtaService.getSkills()
					.then(function(skills) {
						$scope.skillsLoaded = true;
						$scope.skills = skills;
						if($scope.skillId !== null) {
							$scope.selectedSkill = skills.find(function(sk){
								return sk.Id === $scope.skillId;
							});
							getSitesOrTeamsForSkillOrSkillArea();
						}
					});

				RtaService.getSkillAreas()
					.then(function(skillAreas) {
						$scope.skillAreasLoaded = true;
						$scope.skillAreas = skillAreas.SkillAreas;
						if($scope.skillAreaId !== null) {
							$scope.selectedSkillArea = $scope.skillAreas.find(function(sa){
								return sa.Id === $scope.skillAreaId;
							});
							getSitesOrTeamsForSkillOrSkillArea();
						}
					});

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
					if (skill) {
						$scope.skillId = skill.Id;
						$scope.selectedSkill = skill;
						if($state.current.name !== "rta.teams-by-skill")
							RtaRouteService.goToSitesBySkill($scope.skillId);
						getSitesOrTeamsForSkillOrSkillArea();
					};
				}

				$scope.selectedSkillAreaChange = function(skillArea) {
					if (skillArea) {
						$scope.skillAreaId = skillArea.Id;
						$scope.selectedSkillArea = skillArea;
						if($state.current.name !== "rta.teams-by-skillArea")
							RtaRouteService.goToSitesBySkillArea($scope.skillAreaId);
						getSitesOrTeamsForSkillOrSkillArea();
					};
				}

				$scope.goToOverview = function() {
				 RtaRouteService.goToSites();
				}

				function goToSelectSkill() {
					RtaRouteService.goToSelectSkill();
				}

				$scope.urlForSelectSkill = function() {
					return RtaRouteService.urlForSelectSkill();
				}

				$scope.getStateForTeams = function() {
					if($scope.skillId !== null) {
						return stateForTeamsBySkill;
					}
					return stateForTeamsBySkillArea;
				};

				function getSitesOrTeamsForSkillOrSkillArea() {
					if ($scope.siteIds.length > 0) {
						getTeamsForSitesAndSkillOrSkillArea()
							.then(function(teams) {
								$scope.teams = teams;
								var teamIds = teams.map(function(team) {
									return team.Id;
								});
								return getAdherenceForAllTeamsOnSitesBySkillOrSkillArea($scope.siteIds)
									.then(function(teamAdherences) {
										RtaAdherenceService.updateAdherence($scope.teams, teamAdherences);
									});
							})
					} else {
						getSitesForSkillOrSkillArea()
							.then(function(sites) {
								$scope.sites = sites;
								return getAdherenceForAllSitesBySkillOrSkillArea();
							}).then(function(siteAdherences) {
								RtaAdherenceService.updateAdherence($scope.sites, siteAdherences);
							});
					}
				}

				function getSitesForSkillOrSkillArea() {
					if ($scope.skillId !== null) {
						return RtaService.getSitesForSkill([$scope.skillId]);
					} else {
						return RtaService.getSitesForSkill(getSkillIdsFromSkillArea($scope.skillAreaId));
					}
				}

				function getTeamsForSitesAndSkillOrSkillArea() {
					return $scope.skillId !== null ?
						RtaService.getTeamsForSitesAndSkill({
							skillIds: [$scope.skillId],
							siteIds: $scope.siteIds
						})
						:
						RtaService.getTeamsForSitesAndSkill({
							skillIds: getSkillIdsFromSkillArea($scope.skillAreaId),
							siteIds: $scope.siteIds
						});
				}

				if($scope.siteIds.length > 0) {
					RtaOrganizationService.getSiteName($scope.siteIds)
					.then(function(name) {
						$scope.siteName = name;
					});
				}

				function getAdherenceForAllSitesBySkillOrSkillArea(){
					return $scope.skillId !== null ?
								RtaService.getAdherenceForAllSitesBySkill([$scope.skillId])
								:
								RtaService.getAdherenceForAllSitesBySkill(getSkillIdsFromSkillArea($scope.skillAreaId));
				}

				function getAdherenceForAllTeamsOnSitesBySkillOrSkillArea(siteIds) {
					return $scope.skillId !== null ?
						RtaService.getAdherenceForAllTeamsOnSitesBySkill({
							skillIds: [$scope.skillId],
							siteIds: siteIds
						})
						:
							RtaService.getAdherenceForAllTeamsOnSitesBySkill({
							skillIds: getSkillIdsFromSkillArea($scope.skillAreaId),
							siteIds: siteIds
						});
				}

				function getSkillIdsFromSkillArea(skillAreaId){
					return $scope.skillAreas.find(function(skillArea){
						return skillArea.Id === skillAreaId;
					})
					.Skills.map(function(skill){
						return skill.Id;
					});
				}

				var polling = $interval(function() {
					if ($scope.skillId !== null || $scope.skillAreaId !== null) {
						if ($scope.siteIds.length > 0 && $scope.teams !== undefined) {
							getAdherenceForAllTeamsOnSitesBySkillOrSkillArea($scope.siteIds)
								.then(function(teamAdherences) {
									RtaAdherenceService.updateAdherence($scope.teams, teamAdherences);
								});
						}
						else if ($scope.sites !== undefined) {
							getAdherenceForAllSitesBySkillOrSkillArea()
						    .then(function(siteAdherences) {
									RtaAdherenceService.updateAdherence($scope.sites, siteAdherences);
						});
					};
					}
				}, 5000);

				$scope.$watch('selectedSkill', function (newValue, oldValue) {
					if (newValue !== oldValue && oldValue!= undefined && oldValue!= null && newValue==null) {
							goToSelectSkill();
					}
				});

				$scope.$watch('selectedSkillArea', function (newValue, oldValue) {
					if (newValue !== oldValue && oldValue!= undefined && oldValue!= null && newValue==null) {
							goToSelectSkill();
					}
				});

				$scope.$on('$destroy', function() {
					$interval.cancel(polling);
				});

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

			}
		]);
})();
