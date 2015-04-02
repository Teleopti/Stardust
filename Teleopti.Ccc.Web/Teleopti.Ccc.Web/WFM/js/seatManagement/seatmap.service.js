'use strict';

angular.module('wfm.seatMap')
	.factory('seatMapService', ['$resource', function ($resource) {
		var seatMapService = {};

		seatMapService.seatMap = $resource('../api/SeatPlanner/SeatMap', {}, {
			get: { method: 'GET', params: {}, isArray: false }
		});

		return seatMapService;
	}]);


