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

				$scope.skillId = $stateParams.skillId || null;
				$scope.skillAreaId = $stateParams.skillAreaId || null;

				$scope.siteIds = $stateParams.siteIds || [];
				$scope.selectedItemIds = [];
				$scope.getAdherencePercent = RtaFormatService.numberToPercent;
				
				RtaService.getSkills()
					.then(function(skills) {
						$scope.skillsLoaded = true;
						$scope.skills = skills;
					});

				RtaService.getSkillAreas()
					.then(function(skillAreas) {
						$scope.skillAreasLoaded = true;
						$scope.skillAreas = skillAreas.SkillAreas;
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
						getSitesOrTeamsForSkillOrSkillArea();
					};
				}

				$scope.selectedSkillAreaChange = function(skillArea) {
					if (skillArea) {
						$scope.skillAreaId = skillArea.Id;
						getSitesOrTeamsForSkillOrSkillArea();
					};
				}

				$scope.goToOverview = function() {
					$state.go('rta');
				}

				function getSitesOrTeamsForSkillOrSkillArea() {
					if ($scope.siteIds.length) {
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
						return RtaService.getSitesForSkillArea($scope.skillAreaId);
					}
				}

				function getTeamsForSitesAndSkillOrSkillArea() {
					return $scope.skillId !== null ?
						RtaService.getTeamsForSitesAndSkill({
							skillIds: [$scope.skillId],
							siteIds: $scope.siteIds
						})
						:
						RtaService.getTeamsForSitesAndSkillArea({
							skillAreaId: $scope.skillAreaId,
							siteIds: $scope.siteIds
						});
				}

				function getAdherenceForAllSitesBySkillOrSkillArea(){
					return $scope.skillId !== null ?
								RtaService.getAdherenceForAllSitesBySkill([$scope.skillId])
								:
								RtaService.getAdherenceForAllSitesBySkillArea($scope.skillAreaId);
				}

				function getAdherenceForAllTeamsOnSitesBySkillOrSkillArea(siteId) {
					return $scope.skillId !== null ?
						RtaService.getAdherenceForAllTeamsOnSitesBySkill({
							skillIds: [$scope.skillId],
							siteId: siteId
						})
						:
						RtaService.getAdherenceForAllTeamsOnSitesBySkillArea({
							skillAreaId: $scope.skillAreaId,
							siteId: siteId
						});
				}

				var polling = $interval(function() {
					if ($scope.skillId !== null || $scope.skillAreaId !== null) {
						getSitesOrTeamsForSkillOrSkillArea();
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
