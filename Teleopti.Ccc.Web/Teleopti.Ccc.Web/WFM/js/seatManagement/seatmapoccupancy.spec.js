'use strict';

describe('seatmap booking view tests', function () {

	var $q,
		$rootScope,
		$httpBackend,
		controller,
		seatMapCanvasUtilsService,
		seatMapService,
		growl,
		seatMapTranslator,
		deleteResponse;

	beforeEach(function () {
		module('wfm.seatPlan');
		module('wfm.seatMap');
		module('pascalprecht.translate');
		module('ui.router');
	});

	beforeEach(inject(function (_$httpBackend_, _$q_, _$rootScope_, _$controller_, _seatMapCanvasUtilsService_, _seatMapService_, _growl_, _seatMapTranslatorFactory_) {

		$q = _$q_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
		controller = setUpController(_$controller_);
		seatMapCanvasUtilsService = _seatMapCanvasUtilsService_;
		seatMapService = _seatMapService_;
		seatMapTranslator = _seatMapTranslatorFactory_;
		growl = _growl_;
	}));

	it('should get correct display time', function () {
		var displayTime = controller.getDisplayTime(fakeBooking());

		expect(displayTime).toEqual("8:00 AM - 5:00 PM");
	});

	it('should delete seat booking successfully', function () {
		controller.deleteSeatBooking(fakeBooking());
		
		expect(deleteResponse.Id).toEqual(fakeBooking().BookingId);
	});

	it('should get class of previous day correctly', function () {
		controller.occupancyDetails = fakeBooking();

		controller.scheduleDate = new Date("2015-01-01");
		expect(controller.getSeatBookingDetailClass(fakeBooking())).toEqual("seatmap-seatbooking-previousday");

		controller.scheduleDate = new Date("2015-01-02");
		expect(controller.getSeatBookingDetailClass(fakeBooking())).toEqual("");
	});

	function fakeBooking() {
		return {
			"StartDateTime": "2015-01-02T08:00:00",
			"EndDateTime": "2015-01-02T17:00:00",
			"PersonId": "b46a2588-8861-42e3-ab03-9b5e015b257c",
			"FirstName": "Jon",
			"LastName": "Kleinsmith",
			"SeatId": "14a2a684-74bd-4c78-b5c7-7267ac3178a2",
			"BookingId": "6e03b40c-cad5-4f23-935e-a51300bf8f10",
			"SeatName": "1",
			"BelongsToDate": "2015-01-02T00:00:00"
		};
	};

	var mockSeatMapService = {
		occupancy: {
			get: function (param) {
				var queryDeferred = $q.defer();
				var result = [];
				queryDeferred.resolve(result);
				return { $promise: queryDeferred.promise };
			},
			remove: function (param) {
				deleteResponse = param;
				var queryDeferred = $q.defer();
				var result = [];
				queryDeferred.resolve(result);
				return { $promise: queryDeferred.promise };
			}
		}
	};

	function setUpController($controller) {
		var scope = $rootScope.$new();
		return $controller('SeatMapOccupancyCtrl',
		{
			utils: seatMapCanvasUtilsService,
			seatMapService: mockSeatMapService,
			seatmapTranslator: seatMapTranslator
		});
	};
});