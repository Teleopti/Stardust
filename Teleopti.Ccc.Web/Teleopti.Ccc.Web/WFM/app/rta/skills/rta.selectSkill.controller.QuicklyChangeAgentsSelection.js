(function () {
	'use strict';
	angular.module('wfm.rta')
		.controller('RtaSelectSkillQuickSelectionCtrl', [
			'RtaService',
			'Toggle',
			'$scope',
			'$state',
			'$timeout',
			function (
				RtaService,
				toggleService,
				$scope,
				$state,
				$timeout) {
				$scope.skills = [];
				$scope.skillAreas = [];
				$scope.skillsLoaded = false;
				$scope.skillAreasLoaded = false;
				var selectedSiteId;
				$scope.selectedTeams = [];
				$scope.selectedSites = [];
				$scope.sites = [];

				var stateGo = function (selection) {
					$state.go('rta.agents', selection);
				}

				toggleService.togglesLoaded.then(init);
				function init() {
					// replace this when changin the behavior of skill/skillArea selction changed
					stateGo = function (selection) {
						$state.go('rta.agents', selection);
					}
				}


				RtaService.getSkills()
					.then(function (skills) {
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


				$scope.selectedSkillChange = function (item) {
					if (item) {
						$timeout(function () {
							$state.go('rta.agents', {
								skillIds: item.Id
							});
						});
					};
				}

				$scope.selectedSkillAreaChange = function (item) {

					if (item) {
						$timeout(function () {
							$state.go('rta.agents', {
								skillAreaId: item.Id
							});
						});
					};
				}

				$scope.goToOverview = function () {
					$state.go('rta');
				}

				RtaService.getOrganization()
					.then(function (organization) {
						$scope.sites = organization;
					});

				$scope.selectSite = function (siteId) {
					selectedSiteId = $scope.isSelected(siteId) ? '' : siteId;
				};

				$scope.isSelected = function (siteId) {
					return selectedSiteId === siteId;
				};

				$scope.goToAgents = function () {
					var obj = {};
					var selectedSiteIds = selectedSites();
					var selectedTeamIds = selectedTeams();
					if (selectedSiteIds.length > 0)
						obj['siteIds'] = selectedSiteIds;
					if (selectedTeamIds.length > 0)
						obj['teamIds'] = selectedTeamIds;
					if (selectedSiteIds.length > 0 || selectedTeamIds.length > 0)
						$state.go('rta.agents', obj);
				}

				function selectedSites() {
					return $scope.sites.filter(function (site) { return site.isChecked == true; }).map(function (s) { return s.Id; });
				}

				function selectedTeams() {
					return flatten(
						$scope.sites.map(function (s) {
							var selectedTeamIds = s.Teams.filter(function (team) {
								return team.isChecked == true;
							})
								.map(function (team) {
									return team.Id;
								});
							return selectedTeamIds;
						}));
				}

				function flatten(collection) {
					return [].concat.apply([], collection)
				}

				$scope.selectionChanged = function (siteId, teamIds) {
					var selectedSite = $scope.sites.find(function (site) {
						return site.Id == siteId;
					});
					if (selectedSite.Teams.length > teamIds.length) {
						selectedSite.isChecked = false;
						selectedSite.Teams.forEach(function (team) {
							team.isChecked = teamIds.indexOf(team.Id) > -1 ? !team.isChecked : team.isChecked;
						})
					}
					else
						selectedSite.isChecked = !selectedSite.isChecked;
				}



				$scope.printOut = function () {
					console.log($scope.selectedTeams);
				}
			}
		]);
})();
