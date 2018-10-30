'use strict';

describe('seatmap controller tests', function() {
	var $q, $rootScope, $httpBackend, $resource, controller, parseDate, seatMapCanvasUtilsService;

	beforeEach(function() {
		module('wfm.seatMap');

		module(function($provide) {
			$provide.service('seatMapCanvasUtilsService', function() {
				return {
					loadSeatMap: function(id, date) {
						parseDate = date;
					},
					resize: function(canvas) {}
				};
			});
		});
	});

	beforeEach(inject(function(_$httpBackend_, _$q_, _$rootScope_, _$controller_, _$resource_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
		controller = setUpController(_$controller_);

		$resource = _$resource_;
	}));

	it('should pass selected date by default when refreshing seat map', function() {
		controller.refreshSeatMap();
		expect(parseDate).toEqual(moment(controller.selectedDate).format('YYYY-MM-DD'));
	});

	function setUpController($controller) {
		var scope = $rootScope.$new();
		return $controller('SeatMapCanvasController', {
			$scope: scope,
			canvasUtils: seatMapCanvasUtilsService
		});
	}
});
