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

				$scope.goToAgents = function () {
					var selectedSiteIds = $scope.sites.filter(function (site) { return site.isChecked == true; }).map(function (s) { return s.Id; });
					$state.go('rta.agents', {
						siteIds: selectedSiteIds
					});
				}

				$scope.selectSite = function (siteId) {
					selectedSiteId = $scope.isSelected(siteId) ? '' : siteId;
				};

				$scope.isSelected = function (siteId) {
					return selectedSiteId === siteId;
				};

			}
		]);
})();
