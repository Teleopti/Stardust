'use strict';

var seatPlanService = angular.module('seatPlanService', ['ngResource']);
seatPlanService.factory('SeatPlanService', ['$rootScope','$resource', function ($rootScope, $resource) {

	var seatPlanService = {};
	
	seatPlanService.locations = $resource('../api/SeatPlanner/Locations', {}, {
		get: { method: 'GET', params: {}, isArray: false }
	});

	seatPlanService.teams = $resource('../api/SeatPlanner/Teams', {}, {
		get: { method: 'GET', params: {}, isArray: false }
	});
	
	seatPlanService.seatPlan = $resource('../api/SeatPlanner/SeatPlan', {}, {
		add: { method: 'POST', params: { Teams: {}, Locations: {}, StartDate: {}, EndDate: {} }, isArray: false }
	});

	return seatPlanService;

}]);