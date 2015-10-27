(function() {
	'use strict';
	angular.module('wfm.intraday')
		.controller('IntradayCtrl', [
		'$scope', '$state', 'IntradayService',
		function ($scope, $state, IntradayService) {
			//console.log(IntradayService.skillList);
			$scope.skillList = IntradayService.skillList.query();
			$scope.selectedSkill = $scope.skillList[0];

			
			$scope.skillChange = function(skill) {
				$scope.name = skill.Measures[0].Name;
				$scope.value = skill.Measures[0].Value;
				$scope.stringValue = skill.Measures[0].StringValue;
			}
			//$scope.skillChange($scope.skillList[0]);


		}
	]);
})()
