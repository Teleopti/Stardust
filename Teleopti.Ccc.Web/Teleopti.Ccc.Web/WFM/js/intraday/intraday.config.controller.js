(function () {
	'use strict';
	angular.module('wfm.intraday')
		.controller('IntradayConfigfCtrl', ['$scope', '$state', function ($scope, $state) {

      $scope.exitConfigMode = function () {
				$state.go('intraday', {});
			};

		}
		]);
})();
