(function () {
	'use strict';
	angular.module('wfm.intraday')
		.controller('IntradayCtrl', [
		'$scope', '$state', 'intradayService', '$stateParams',
		function ($scope, $state, intradayService, $stateParams) {

			$scope.SkillAreaId = $stateParams.skillAreaId;
			intradayService.skillList.query().$promise.then(function (result) {
				$scope.skillList = result;
				$scope.selectedSkill = $scope.skillList[0];
				$scope.skillChange($scope.selectedSkill);
			});

			$scope.format = intradayService.formatDateTime;

			$scope.skillChange = function (skill) {
				$scope.selectedSkill = skill;
			}

			$scope.configMode = function () {
				$state.go('intraday.config', {});
			};


			$scope.reload = setInterval(function () {
				intradayService.skillList.query().$promise.then(function (result) {
					result.forEach(function (resultItem) {
						$scope.skillList.forEach(function (skillItem) {
							if (resultItem.SkillName == skillItem.SkillName) {
								skillItem.Severity = resultItem.Severity;
								skillItem.Measures = resultItem.Measures;
								skillItem.LatestDate = resultItem.LatestDate;
							}
						});
					});
				});
			}, 60000);
		}
	]);
})()
