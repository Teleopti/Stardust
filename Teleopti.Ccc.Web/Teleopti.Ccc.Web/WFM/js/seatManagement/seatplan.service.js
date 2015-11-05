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
			add: { method: 'POST', params: { Teams: {}, Locations: {}, StartDate: {}, EndDate: {}, PersonIds: [], SeatIds: [] }, isArray: false }
		});

		seatPlanService.seatBookingReport = $resource('../api/SeatPlanner/SeatBookingReport', {}, {
			get: { method: 'POST', params: { Teams: {}, Locations: {}, StartDate: {}, EndDate: {}, Skip: {}, Take: {} }, isArray: false }
		});


		seatPlanService.seatPlans = $resource('../api/SeatPlanner/SeatPlan?startDate=:startDate&endDate=:endDate', {},
			{
				startDate: "@startDate",
				endDate: "@endDate"
			},
			{
				query: { method: 'GET', params: {}, isArray: true }
			}
		);

		return seatPlanService;

	}]);