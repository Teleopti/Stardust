'use strict';

describe('seatplan report controller tests', function () {

	var $q,
		$rootScope,
		$httpBackend,
		controller,
		reportTake,
		seatplanTeamAndLocationService,
		seatBookingsReportRequestParams;

	beforeEach(function () {
		module('wfm.seatPlan');
		module('pascalprecht.translate');
	});

	beforeEach(inject(function (_$httpBackend_, _$q_, _$rootScope_, _$controller_, _seatplanTeamAndLocationService_, _reportTake_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
		reportTake = _reportTake_;
		seatplanTeamAndLocationService = _seatplanTeamAndLocationService_;
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

		var teamFactory = TeamFactory();

		controller.teams = [teamFactory.CreateTeam('1', true), teamFactory.CreateTeam('2', false), teamFactory.CreateTeam('3', true)];
		controller.applyFilter();
		expect(seatBookingsReportRequestParams.teams).toEqual(['1', '3']);
	}));

	it('should filter seat bookings by selected locations', inject(function () {

		var locationFactory = LocationFactory();

		controller.locations = [
			locationFactory.CreateLocation('1', 10, true),
			locationFactory.CreateLocation('2', 100, true),
			locationFactory.CreateLocation('3', 1000, false)
		];
		controller.applyFilter();

		expect(seatBookingsReportRequestParams.locations).toEqual(['1', '2']);

	}));

	it('currentPage should be 1 when controller is initialized', inject(function() {
		controller.init();
		expect(controller.currentPage).toEqual(1);
	}));

	it('should filter by paging ', inject(function () {
		var goToPage = 10;

		controller.totalPages = 20;
		controller.paging(goToPage);

		expect(controller.currentPage).toEqual(10);
		expect(seatBookingsReportRequestParams.skip).toEqual((goToPage - 1) * reportTake);
	}));

	it('should page number within page range', inject(function () {
		controller.currentPage = 20;
		controller.totalPages = 20;
		controller.paging(controller.currentPage + 1);

		expect(controller.currentPage).toEqual(20);
	}));

	it('should currentPage equals to 1 after apply filter', inject(function () {
		controller.currentPage = 10;
		controller.applyFilter();
		expect(controller.currentPage).toEqual(1);
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

	function TeamFactory() {

		function createTeam(id, isSelected) {
			return {
				Children: [],
				Id: id,
				Name: 'team ' + id,
				selected: isSelected
			};
		}

		function setChildren(parent, children) {
			parent.Children.push(children);
		}

		return {
			CreateTeam: createTeam,
			SetChildren: setChildren
		};
	};

	function LocationFactory() {

		function createSeats(numberOfSeats) {
			var seats = [];
			for (var i = 0; i < numberOfSeats; i++) {
				seats[i] = { Id: i, Name: "Seat" + i }
			}
			return seats;
		}

		function createLocation(id, numberOfSeats, isSelected) {
			return {
				ParentId: "00000000-0000-0000-0000-000000000000",
				SeatMapJsonData: null,
				BreadcrumbInfo: null,
				Id: id,
				Name: 'Location ' + id,
				Children: [],
				Seats: createSeats(numberOfSeats),
				selected: isSelected
			}
		}

		function setChildren(parent, children) {
			parent.Children.push(children);
		}

		return {
			CreateLocation: createLocation,
			SetChildren: setChildren
		}
	};

	function setUpController($controller) {
		return $controller('seatPlanReportCtrl',
		{ seatPlanService: mockSeatPlanService, seatplanTeamAndLocationService: seatplanTeamAndLocationService, reportTake: reportTake });
	};

});
