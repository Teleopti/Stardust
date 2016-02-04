(function () {
	'use strict';
	angular.module('wfm.intraday')
		.controller('IntradayConfigCtrl', [
			'$scope', '$state', 'intradayService',
			function ($scope, $state, intradayService) {

				$scope.skills = [];

				$scope.exitConfigMode = function () {
					$state.go('intraday', {});
				};

				intradayService.getSkills().then(function(skills) {
					$scope.skills = skills;
				});
			}
		]);
})();
