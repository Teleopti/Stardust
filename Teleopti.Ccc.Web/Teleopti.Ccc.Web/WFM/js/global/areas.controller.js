'use strict';

var notification = angular.module('wfm.areas', ['restAreasService']);
notification.controller('AreasCtrl', [
	'$scope', 'AreasSvrc', '$location', '$state',
	function ($scope, AreasSvrc, $location, $state) {
		$scope.areas = [];
		$scope.filters = [];
		$scope.$state = $state;

		$scope.$watch(function () { return $location.search().buid; }, function () {
			var buid = $location.search().buid;
			if (buid) {
				angular.forEach($scope.areas, function (area) {
					area.Params.buid = buid;
				});
			}
		});

		$scope.isActive = function (viewLocation) {
			return "/" + viewLocation === $location.path();
		};

		$scope.loadAreas = function () {
			AreasSvrc.getAreas.query({}).$promise.then(function (result) {
				var buid = $location.search().buid;
				for (var i = 0; i < result.length; i++) {
					result[i].filters = [];
					result[i].Params = {};
					if (buid) {
						result[i].Params.buid = buid;
					}
				}
				$scope.areas = result;
			});
		};
	}
]);