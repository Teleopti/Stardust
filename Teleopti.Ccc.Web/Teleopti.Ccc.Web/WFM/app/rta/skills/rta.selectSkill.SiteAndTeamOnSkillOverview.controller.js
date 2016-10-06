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
				RtaAdherenceService
				) {
				$scope.skills = [];
				$scope.skillAreas = [];
				$scope.skillsLoaded = false;
				$scope.skillAreasLoaded = false;
				$scope.skillId = $stateParams.skillId || null;

				$scope.siteIds = $stateParams.siteIds || [];
				$scope.selectedItemIds = [];
				$scope.getAdherencePercent = RtaFormatService.numberToPercent;

				RtaService.getSkills()
					.then(function(skills) {
						$scope.skillsLoaded = true;
						$scope.skills = skills;
					});

				RtaService.getSkillAreas()
					.then(function (skillAreas) {
						$scope.skillAreasLoaded = true;
						$scope.skillAreas = skillAreas.SkillAreas;
					});

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

				$scope.selectedSkillChange = function(skill) {
					if (skill) {
						getSitesOrTeamsForSkill(skill.Id);
					};
				}

				$scope.selectedSkillAreaChange = function (item) {

					if (item) {
						$timeout(function () {
							$state.go('rta.agents-skill-area', { skillAreaId: item.Id });
						});
					};

				}

				$scope.goToOverview = function(){
					$state.go('rta');
				}

				function getSitesOrTeamsForSkill(skillId){
						if ($scope.siteIds.length){
							RtaService.getTeamsForSitesAndSkill({
								skillId: skillId,
								siteIds: $scope.siteIds
							}).then(function (teams) {
								$scope.teams = teams;
								var teamIds = teams.map(function(team){
									return team.Id;
								});
								return RtaService.getAdherenceForAllTeamsOnSitesBySkill({
									skillId: skillId,
									teamIds: teamIds
								}).then(function(teamAdherences){
										RtaAdherenceService.updateAdherence($scope.teams, teamAdherences);
								});
							})
						}
						else{
							RtaService.getSitesForSkill(skillId).then(function (sites) {
								$scope.sites = sites;
								return RtaService.getAdherenceForAllSitesBySkill(skillId);
							}).then(function (siteAdherences) {
								RtaAdherenceService.updateAdherence($scope.sites, siteAdherences);
							});
						}
					}

					var polling = $interval(function () {
						if($scope.skillId!==null){
							getSitesOrTeamsForSkill($scope.skillId);
						}
					}, 5000);

					$scope.$on('$destroy', function () {
						$interval.cancel(polling);
					});


				$scope.toggleSelection = function (itemId) {
					var index = $scope.selectedItemIds.indexOf(itemId);
					if (index > -1) {
						$scope.selectedItemIds.splice(index, 1);
					} else {
						$scope.selectedItemIds.push(itemId);
					}
				}

				$scope.openSelectedTeams = function () {
					if ($scope.selectedItemIds.length > 0)
						$state.go('rta.agents-teams', { teamIds: $scope.selectedItemIds });
				}

				$scope.openSelectedSites = function () {
					if ($scope.selectedItemIds.length > 0)
						$state.go('rta.agents-sites', { siteIds: $scope.selectedItemIds });
				};

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

			}
		]);
})();
