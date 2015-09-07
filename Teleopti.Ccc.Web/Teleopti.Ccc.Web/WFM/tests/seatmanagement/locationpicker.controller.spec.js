'use strict';

describe('seatplan report controller tests', function() {

	var $q,
		$rootScope,
		$httpBackend,
		controller,
		seatPlanTranslatorFactory;

	beforeEach(function() {
		module('wfm.seatPlan');
		module('pascalprecht.translate');
	});

	beforeEach(inject(function (_$httpBackend_, _$q_, _$rootScope_, _$controller_, _seatPlanTranslatorFactory_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
		seatPlanTranslatorFactory = _seatPlanTranslatorFactory_;
		controller = setUpController(_$controller_);
	}));

	it('should select location', inject(function() {
		var locationFactory = LocationFactory();
		var location1 = locationFactory.CreateLocation('1', 2, false);
		var location2 = locationFactory.CreateLocation('2', 2, false);
		
		controller.locations = [location1, location2];
		controller.selectedLocations = [];
		controller.toggleLocationSelection(location1);

		expect(controller.selectedLocations.length).toEqual(1);
		expect(controller.selectedLocations[0]).toEqual('1');
		expect(location1.selected).toEqual(true);
		expect(location2.selected).toEqual(false);
	}));

	it('should unselect location', inject(function () {
		var locationFactory = LocationFactory();
		var location1 = locationFactory.CreateLocation('1', 2, true);
		
		controller.locations = [location1];
		controller.selectedLocations = [];
		controller.toggleLocationSelection(location1);
		
		expect(controller.selectedLocations.length).toEqual(0);
		expect(location1.selected).toEqual(false);
		
	}));

	it('should not allow selection of a location with no seats', inject(function () {
		var locationFactory = LocationFactory();
		var location1 = locationFactory.CreateLocation('1', 0, false);

		controller.locations = [location1];
		controller.selectedLocations = [];
		controller.toggleLocationSelection(location1);

		expect(controller.selectedLocations.length).toEqual(0);
		expect(location1.selected).toEqual(false);

	}));

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

	var mockSeatPlanService = {
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
		return $controller('LocationPickerCtrl',
		{
			$scope: scope,
			seatPlanService: mockSeatPlanService,
			translator: seatPlanTranslatorFactory
		});
	};

});