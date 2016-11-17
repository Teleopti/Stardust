(function() {
	'use strict';
	angular.module('wfm.rta')
		.controller('RtaSelectSkillQuickSelectionCtrl', [
			'$scope', '$state', 'RtaService', '$timeout',
			function($scope, $state, RtaService, $timeout) {
				$scope.skills = [];
				$scope.skillAreas = [];
				$scope.skillsLoaded = false;
				$scope.skillAreasLoaded = false;
				var selectedSiteId;
				$scope.selectedTeams = [];
				$scope.selectedSites = [];
				$scope.sites = [];


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

				$scope.selectedSkillChange = function(item) {
					if (item) {
						$timeout(function() {
							$state.go('rta.agents', {
								skillIds: item.Id
							});
						});
					};

				}

				$scope.selectedSkillAreaChange = function(item) {

					if (item) {
						$timeout(function() {
							$state.go('rta.agents', {
								skillAreaId: item.Id
							});
						});
					};

				}

				$scope.goToOverview = function() {
					$state.go('rta');
				}

				RtaService.getOrganization()
					.then(function(organization){
					console.log(organization);
					$scope.sites = organization;
				});

				$scope.goToAgents = function() {
					var selectedSiteIds = $scope.sites.filter(function(site){ return site.isChecked == true;}).map(function(s){ return s.Id;});
					$state.go('rta.agents', {
						siteIds: selectedSiteIds
					});
				}


				// $scope.sites = [{
				// 	Id: 'LondonGuid',
				// 	Name: 'London',
				// 	Teams: [{
				// 		Id: '1',
				// 		Name: 'Team Preferences'
				// 	}, {
				// 		Id: '2',
				// 		Name: 'Team Students'
				// 	}]
				// }, {
				// 	Id: 'ParisGuid',
				// 	Name: 'Paris',
				// 	Teams: [{
				// 		Id: '3',
				// 		Name: 'Team Red'
				// 	}, {
				// 		Id: '4',
				// 		Name: 'Team Green'
				// 	}]
				// }, {
				// 	Id: 'StockholmGuid',
				// 	Name: 'Stockholm',
				// 	Teams: [{
				// 		Id: '5',
				// 		Name: 'Team Stockholm 1'
				// 	}, {
				// 		Id: '6',
				// 		Name: 'Team Stockholm 2'
				// 	}, {
				// 		Id: '7',
				// 		Name: 'Team Stockholm 3'
				// 	}, {
				// 		Id: '8',
				// 		Name: 'Team Stockholm 4'
				// 	}, {
				// 		Id: '9',
				// 		Name: 'Team Stockholm 5'
				// 	}, {
				// 		Id: '10',
				// 		Name: 'Team Stockholm 6'
				// 	}, ]
				// }, ];

				$scope.selectSite = function(siteId) {
					selectedSiteId = $scope.isSelected(siteId) ? '' : siteId;
				};

				$scope.isSelected = function(siteId) {
					return selectedSiteId === siteId;
				};

			}
		]);
})();
