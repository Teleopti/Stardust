'use strict';

describe('seatmap editor controller tests', function () {

	var $q,
		$rootScope,
		controller,
		seatMapServiceParams,
		seatMapCanvasUtilsService,
		seatMapCanvasEditService;
	
	beforeEach(function () {
		module('wfm.seatPlan');
		module('wfm.seatMap');
	});

	beforeEach(inject(function (_$q_, _$rootScope_, _$controller_, _seatMapCanvasUtilsService_, _seatMapCanvasEditService_, seatMapService) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		seatMapCanvasUtilsService = _seatMapCanvasUtilsService_;
		seatMapCanvasEditService = _seatMapCanvasEditService_;
		seatMapCanvasUtilsService.loadSeatMap = function () { };

		setupFakeSeatMapService(seatMapService);
		setUpController(_$controller_);

	}));



	it('should be able to add seats to parentVm Seat array', function (done) {

		controller.addSeat();
		controller.addSeat();
		controller.addSeat();
		controller.addSeat();

		setTimeout(function () {

			controller.save();

			expect(seatMapServiceParams.Id).toEqual(undefined);
			expect(seatMapServiceParams.Location).toEqual(undefined);
			expect(seatMapServiceParams.ChildLocations.length).toEqual(0);
			expect(seatMapServiceParams.Seats.length).toEqual(4);
			seatMapServiceParams.Seats.forEach(function (seat) {
				expect(seat.RoleIdList.length).toEqual(0);
			});

			done();
		}, 4000);

	});


	function setUpController($controller) {

		var scope = $rootScope.$new();
		var canvas = new fabric.CanvasWithViewport('c');

		controller = $controller('SeatMapEditCtrl', {
			$scope: scope,
			utils: seatMapCanvasUtilsService,
			editor: seatMapCanvasEditService
		});

		controller.parentVm = {
			seats: [],
			getCanvas: function () { return canvas; },
			rightPanelOptions: {}
		};
	};

	function setupFakeSeatMapService(seatMapService) {

		seatMapServiceParams = {};

		seatMapService.seatMap = {
			get: function(param) {
				seatMapServiceParams = param;
				var queryDeferred = $q.defer();
				var result = [];
				queryDeferred.resolve(result);
				return { $promise: queryDeferred.promise };
			},
			save: function (param) {
				seatMapServiceParams = param;
				var queryDeferred = $q.defer();
				var result = [];
				queryDeferred.resolve(result);
				return { $promise: queryDeferred.promise };
			}
		};
	}


});