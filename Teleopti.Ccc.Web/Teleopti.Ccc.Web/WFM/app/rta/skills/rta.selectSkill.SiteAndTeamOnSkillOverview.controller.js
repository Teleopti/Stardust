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
						$scope.skillAreaId = null;
						$scope.selectedSkillArea  = null;
						getSitesOrTeamsForSkillOrSkillArea();
					};
				}

				$scope.selectedSkillAreaChange = function(skillArea) {
					if (skillArea) {
						$scope.skillAreaId = skillArea.Id;
						$scope.selectedSkillArea = skillArea;
						$scope.skillId = null;
						$scope.selectedSkill = null;
						getSitesOrTeamsForSkillOrSkillArea();
					};
				}

				$scope.goToOverview = function() {
					$state.go('rta');
				}

				$scope.getStateForTeams = function() {
					if($scope.skillId !== null) {
						return 'rta.teams-by-skill({siteIds: site.Id, skillIds: selectedSkill.Id})';
					}
					return 'rta.teams-by-skillArea({siteIds: site.Id, skillAreaId: selectedSkillArea.Id})';
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
