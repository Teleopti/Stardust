﻿'use strict';

angular.module('wfm.seatMap')
	.factory('seatMapService', ['$resource', function ($resource) {
		var seatMapService = {};

		seatMapService.seatMap = $resource('../api/SeatPlanner/SeatMap', {}, {
			get: { method: 'GET', params: {}, isArray: false },
			save: { method: 'POST', params: { Id: {}, Location: {}, SeatMapData: {}, TrackedCommandInfo: {}, ChildLocations: [], Seats: [] }, isArray: false }
		});
		seatMapService.occupancy = $resource('../api/SeatPlanner/Occupancy', {}, {
			get: { method: 'GET', params: {}, isArray: true }
		});

		return seatMapService;
	}]);


