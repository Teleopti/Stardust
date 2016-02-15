(function () {
	'use strict';
	angular.module('wfm.intraday')
		.controller('IntradayConfigCtrl', [
			'$scope', '$state', 'intradayService', '$filter',
			function ($scope, $state, intradayService, $filter) {

				$scope.skills = [];
				$scope.skillAreaName = '';

				$scope.exitConfigMode = function () {
					$state.go('intraday', { isNewSkillArea: false });
				};

				intradayService.getSkills.query().$promise.then(function(skills) {
					$scope.skills = skills;
				});

				$scope.skillSelected = function () {
					var selectedSkills = $filter('filter')($scope.skills, { isSelected: true });
					var selectedSkillIds = selectedSkills.map(function (skill) {
						return skill.Id;
					});
					return selectedSkillIds.length>0;
				}

				$scope.saveSkillArea = function () {
					var selectedSkills = $filter('filter')($scope.skills, { isSelected: true });

					var selectedSkillIds = selectedSkills.map(function (skill) {
						return skill.Id;
					});

					intradayService.createSkillArea.query(
						{
							Name: $scope.skillAreaName,
							Skills: selectedSkillIds
						}
					).$promise.then(function(result) {
						$state.go('intraday', { isNewSkillArea: true });
					});
				};
			}
		]);
})();
