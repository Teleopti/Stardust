'use strict';

var notification = angular.module('wfm.areas', ['restAreasService']);
notification.controller('AreasCtrl', [
	'$scope', 'AreasSvrc',
	function ($scope, AreasSvrc) {
		$scope.areas = [];
		$scope.filters = [];

		$scope.loadAreas = function () {
			AreasSvrc.getAreas.query({}).$promise.then(function (result) {
				for (var i = 0; i < result.length; i++) {
					result[i].filters = [];
				}
				$scope.areas = result;
				$scope.areasLoaded = true;
			});
		};
	}
]);
