'use strict';

angular.module('wfm.seatMap')
	.factory('seatMapService', ['$resource', function ($resource) {
		var seatMapService = {};

		seatMapService.seatMap = $resource('../api/SeatPlanner/SeatMap', {}, {
			get: { method: 'GET', params: {}, isArray: false },
			save: { method: 'POST', params: { Id: {}, Location: {}, SeatMapData: {}, TrackedCommandInfo: {}, ChildLocations: [], Seats: [] }, isArray: false }
		});
		seatMapService.occupancy = $resource('../api/SeatPlanner/Occupancy', {}, {
			get: { method: 'POST', params: {Date: {}, SeatIds: [] }, isArray: true },
			remove: { method: 'DELETE', params: {  }, isArray: false }
		});
		seatMapService.selectPeople = $resource('../api/SeatPlanner/SelectPeople',{}, {
			save: { method: 'POST', params: { Date: {}, PersonIds: [] , SeatIds: []}, isArray: false }
		});

		return seatMapService;
	}]);


