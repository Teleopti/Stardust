(function() {
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
				$scope.stringValue = skill.Measures[0].StringValue;
				if (skill.Measures[0].LatestDate = new Date()) {
					$scope.latestDate = "--:--:--";
				} else {
					$scope.latestDate = skill.Measures[0].LatestDate;
				}
				if ($scope.value == 0) {
					$scope.value = "-";
				} else {
					$scope.value = skill.Measures[0].Value;
				}
			}
			
			$scope.reload = setInterval(function () {
				IntradayService.skillList.query().$promise.then(function (result) {
					result.forEach(function (resultItem) {
						$scope.skillList.forEach(function (skillItem)
						{
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
