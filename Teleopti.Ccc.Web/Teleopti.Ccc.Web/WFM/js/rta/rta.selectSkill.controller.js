(function () {
	'use strict';
	angular.module('wfm.rta')
		.controller('RtaSelectSkillCtrl', [
			'$scope', '$state', 'RtaService', '$timeout',
			function ($scope, $state, RtaService, $timeout) {
				$scope.skills = [];
				$scope.skillAreas = [];
				$scope.skillsLoaded = false;
				$scope.skillAreasLoaded = false;

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

				$scope.selectedSkillChange = function(item) {
					if (item) {
						 $timeout(function(){
							$state.go('rta.agents-skill', { skillId: item.Id });
						});
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

			}
		]);
})();
