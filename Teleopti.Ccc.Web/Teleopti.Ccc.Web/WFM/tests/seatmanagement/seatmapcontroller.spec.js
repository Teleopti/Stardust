'use strict'

describe('seatmap controller tests', function() {

	var $q,
		$rootScope,
		$httpBackend,
		$resource,
		controller,
		seatMapCanvasUtilsService;

	beforeEach(function() {
		module('wfm.seatMap');
	});

	beforeEach(inject(function(_$httpBackend_, _$q_, _$rootScope_, _$controller_, _seatMapCanvasUtilsService_, _$resource_) {

		$q = _$q_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;

		seatMapCanvasUtilsService = _seatMapCanvasUtilsService_;
		seatMapCanvasUtilsService.loadSeatMap = function () { };

		controller = setUpController(_$controller_);
		
		$resource = _$resource_;
	}));

	it('should display booking detail of selected seat', function () {
		//Todo
	});

	function setUpController($controller) {
		var scope = $rootScope.$new();
		return $controller('SeatMapCanvasCtrl',
		{
			$scope: scope,
			canvasUtils: seatMapCanvasUtilsService,
		});
	};
});