'use strict';

angular.module('wfm.seatMap')
	.factory('seatMapService', ['$resource', function ($resource) {

		var seatMapService = {};

		seatMapService.seatMap = $resource('../api/SeatPlanner/SeatMap', {}, {
			query: { method: 'GET', params:  {id : "@id"}, isArray: false }
		});

		return seatMapService;

	}]);


