'use strict';

angular.module('wfm.seatPlan')
	.factory('seatPlanService', ['$resource', function ($resource) {

		var seatPlanService = {};
		
		seatPlanService.addSeatPlan = function (seatPlanCommand) {
			return seatPlanService.seatPlan.add(seatPlanCommand);
		}

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