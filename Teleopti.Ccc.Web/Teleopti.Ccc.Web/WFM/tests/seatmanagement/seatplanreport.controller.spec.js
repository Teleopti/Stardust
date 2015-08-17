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

		controller.teams = [
			teamFactory.CreateTeam('1', true),
			teamFactory.CreateTeam('2', false),
			teamFactory.CreateTeam('3', true)
		];
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

	it('currentPage should be 1 when controller is initialized', inject(function () {
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

	it('should update parent node when selecting children', inject(function () {
		var teamFactory = TeamFactory();
		var site = teamFactory.CreateTeam('1', false),
			team1 = teamFactory.CreateTeam('2', false),
			team2 = teamFactory.CreateTeam('3', false);
		site.Children = [team1, team2];

		controller.teams = [site];
		controller.selectTeam(team1, controller.teams);

		site = controller.teams[0];
		expect(site.selected).toEqual(true);
	}));

	it('should cancel parent node when unselect all children', inject(function () {
		var teamFactory = TeamFactory();
		var site = teamFactory.CreateTeam('1', true),
			team1 = teamFactory.CreateTeam('2', true),
			team2 = teamFactory.CreateTeam('3', false);
		site.Children = [team1, team2];

		controller.teams = [site];
		controller.selectTeam(team1, controller.teams);

		site = controller.teams[0];
		expect(site.selected).toEqual(false);
	}));

	it('should select all children when choose a site', inject(function () {
		var teamFactory = TeamFactory();
		var site = teamFactory.CreateTeam('1', false),
			team1 = teamFactory.CreateTeam('2', false),
			team2 = teamFactory.CreateTeam('3', false);
		site.Children = [team1, team2];

		controller.teams = [site];
		controller.selectTeam(site, controller.teams);

		site = controller.teams[0],
		team1 = site.Children[0],
		team2 = site.Children[1];

		expect(site.selected
			&& team1.selected
			&& team2.selected).toEqual(true);
	}));

	it('should select the second generation children when chose root', inject(function () {
		var teamFactory = TeamFactory();

		var bu = teamFactory.CreateTeam('1', false),
			site1 = teamFactory.CreateTeam('2', false),
			site2 = teamFactory.CreateTeam('3', false),
			team1 = teamFactory.CreateTeam('4', false),
			team2 = teamFactory.CreateTeam('5', false);
		site1.Children = [team1, team2],
		bu.Children = [site1, site2];

		controller.teams = [bu];
		controller.selectTeam(bu, controller.teams);

		bu = controller.teams[0],
		site1 = bu.Children[0],
		site2 = bu.Children[1],
		team1 = site1.Children[0],
		team2 = site1.Children[1];

		expect(bu.selected
			&& site1.selected
			&& team1.selected
			&& team2.selected
			&& site2.selected).toEqual(true);
	}));

	it('should send all chosen ids exclude site id', inject(function () {
		var teamFactory = TeamFactory();
		var site = teamFactory.CreateTeam('1', true),
			team1 = teamFactory.CreateTeam('2', true),
			team2 = teamFactory.CreateTeam('3', false);
		site.Children = [team1, team2];

		controller.teams = [site];
		controller.applyFilter();

		expect(seatBookingsReportRequestParams.teams).toEqual(['2']);
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
				Children: undefined,
				Id: id,
				Name: 'team ' + id,
				selected: isSelected
			};
		}

		return {
			CreateTeam: createTeam
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

		return {
			CreateLocation: createLocation
		}
	};

	function setUpController($controller) {
		return $controller('seatPlanReportCtrl',
		{ seatPlanService: mockSeatPlanService, seatplanTeamAndLocationService: seatplanTeamAndLocationService, reportTake: reportTake });
	};

});
