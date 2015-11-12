(function () {
	'use strict';
	angular.module('wfm.intraday')
		.controller('IntradayCtrl', [
		'$scope', '$state', 'IntradayService',
		function ($scope, $state, IntradayService) {
			IntradayService.skillList.query().$promise.then(function (result) {
				$scope.skillList = result;
				$scope.selectedSkill = $scope.skillList[0];
				$scope.skillChange($scope.selectedSkill);
			});
			$scope.skillChange = function (skill) {
				$scope.name = skill.Measures[0].Name;
				$scope.value = skill.Measures[0].Value;
				$scope.offeredCalls = skill.Measures[0].ActualCalls;
				$scope.forecastedCalls = skill.Measures[0].ForecastedCalls;
				$scope.stringValue = skill.Measures[0].StringValue;
				$scope.latestDate = skill.Measures[0].LatestDate;
				$scope.format = IntradayService.formatDateTime;
			}
			$scope.reload = setInterval(function () {
				IntradayService.skillList.query().$promise.then(function (result) {
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
