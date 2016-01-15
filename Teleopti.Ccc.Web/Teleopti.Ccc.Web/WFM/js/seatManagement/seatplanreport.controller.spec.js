'use strict';

describe('seatplan report controller tests', function () {

	var $q,
		$rootScope,
		$httpBackend,
		controller,
		seatBookingsReportRequestParams;

	beforeEach(function () {
		module('wfm.seatPlan');
		module('pascalprecht.translate');
	});

	beforeEach(inject(function (_$httpBackend_, _$q_, _$rootScope_, _$controller_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
		controller = setUpController(_$controller_);
		controller.selectedPeriod = { StartDate: '2015-01-01', EndDate: '2015-02-01' };
	}));

	it('should filter seat bookings by selected period', inject(function () {

		controller.selectedPeriod = { StartDate: '2015-03-01', EndDate: '2015-04-01' };
		controller.applyFilter();

		expect(seatBookingsReportRequestParams.startDate).toEqual('2015-03-01');
		expect(seatBookingsReportRequestParams.endDate).toEqual('2015-04-01');

	}));

	it('should filter seat bookings by selected teams', inject(function () {
		controller.selectedTeams = ['1', '3'];
		controller.applyFilter();

		expect(seatBookingsReportRequestParams.teams).toEqual(['1', '3']);
	}));

	it('should filter seat bookings by selected locations', inject(function () {
		controller.selectedLocations = ['1', '2'];
		controller.applyFilter();

		expect(seatBookingsReportRequestParams.locations).toEqual(['1', '2']);
	}));

	it('pageNumber should be 1 when controller is initialized', inject(function () {
		controller.init();

		expect(controller.paginationOptions.pageNumber).toEqual(1);
	}));
	
	it('should pageNumber equals to 1 after apply filter', inject(function () {
		controller.paginationOptions.pageNumber = 10;
		controller.applyFilter();

		expect(controller.paginationOptions.pageNumber).toEqual(1);
	}));
	

	var mockSeatPlanService = {
		seatBookingReport: {
			get: function (param) {
				var queryDeferred = $q.defer();
				var result = {};
				seatBookingsReportRequestParams = param;
				queryDeferred.resolve(result);
				return { $promise: queryDeferred.promise };
			}
		},
		teams: {
			get: function (param) {
				var queryDeferred = $q.defer();
				var result = [];
				queryDeferred.resolve(result);
				return { $promise: queryDeferred.promise };
			}
		},
		locations: {
			get: function (param) {
				var queryDeferred = $q.defer();
				var result = [];
				queryDeferred.resolve(result);
				return { $promise: queryDeferred.promise };
			}
		}
	};

	

	function setUpController($controller) {

		var scope = $rootScope.$new();
		return $controller('seatPlanReportCtrl',
		{
			$scope : scope,
			seatPlanService: mockSeatPlanService
		});
	};

});
