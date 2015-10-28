(function() {
	'use strict';
	angular.module('wfm.intraday')
		.controller('IntradayCtrl', [
		'$scope', '$state', 'IntradayService',
		function ($scope, $state, IntradayService) {
			IntradayService.skillList.query().$promise.then(function(result) {
				$scope.skillList = result;
				$scope.selectedSkill = $scope.skillList[0];
				$scope.skillChange($scope.selectedSkill);
			});
			$scope.skillChange = function (skill) {
				$scope.name = skill.Measures[0].Name;
				$scope.value = skill.Measures[0].Value;
				$scope.stringValue = skill.Measures[0].StringValue;
			}
		}
	]);
})()
