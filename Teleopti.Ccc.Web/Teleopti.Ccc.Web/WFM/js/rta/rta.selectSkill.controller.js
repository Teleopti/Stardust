(function () {
	'use strict';
	angular.module('wfm.rta')
		.controller('RtaSelectSkillCtrl', [
			'$scope', '$state', 'RtaService', '$timeout',
			function ($scope, $state, RtaService, $timeout) {
				$scope.skills = [];
				$scope.skillAreas = {
					"HasPermissionToModifySkillArea": true,
					"SkillAreas": [
					{
						"Name": "alina",
						"Id": "d809e2be-60b8-4288-be6c-a64100d200b2",
						"Skills": [{ "Name": "Optimization E-mail skill", "Id": "90149d70-609a-4aa5-a889-9e2c00988e19", "IsDeleted": false }, { "Name": "Import", "Id": "fffcf311-183f-4f66-a829-a64000d3e913", "IsDeleted": false }]
					},
					{
						"Name": "sabbir",
						"Id": "d809e2be-60b8-4288-be6c-a64100d200b3",
						"Skills": [{ "Name": "Optimization E-mail skill", "Id": "90149d70-609a-4aa5-a889-9e2c00988e19", "IsDeleted": false }, { "Name": "Import", "Id": "fffcf311-183f-4f66-a829-a64000d3e913", "IsDeleted": false }]
					}
					]
				};

				RtaService.getSkills()
					.then(function(skills) {
						$scope.skills = skills;
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

				$scope.goToOverview = function(){
					$state.go('rta');
				}

			}
		]);
})();
