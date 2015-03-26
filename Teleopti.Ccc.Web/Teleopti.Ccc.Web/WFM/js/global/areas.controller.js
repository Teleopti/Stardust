'use strict';

var notification = angular.module('wfm.areas', []);
notification.controller('AreasCtrl', [
	'$scope', 'AreasSvrc','FilterSvrc',
	function ($scope, AreasSvrc, FilterSvrc) {
		$scope.areas = [];
		$scope.filters = [];

		$scope.loadAreas = function () {
			AreasSvrc.getAreas.query({}).$promise.then(function (result) {
				for (var i = 0; i < result.length; i++) {
					result[i].filters = [];
				}
				$scope.areas = result;
			});
		};

		$scope.loadFilters = function() {
			FilterSvrc.getFilters.query({}).$promise.then(function(result) {
				$scope.filters = result;
			});
		};

		//should be moved to a seperate service NOT USED FOR NOW
		$scope.loadAreaFilters = function (area) {
			var item = 0;
			for (; item < area._links.length; item++) {
				//for schedule filters
				if (area._links[item].href == 'api/ResourcePlanner/Filter') {
					FilterSvrc.getFilters.query({}).$promise.then(function (result) {
						area.filters.push.apply(area.filters, result);
					});
				}
				//for other filters
			};
		};
	}
]);