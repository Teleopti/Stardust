(function () {
	'use strict';
	angular.module('wfm.intraday')
		.controller('IntradayConfigCtrl', ['$scope', '$state', function ($scope, $state) {

      $scope.exitConfigMode = function () {
				$state.go('intraday', {});
			};

		}
		]);
})();
