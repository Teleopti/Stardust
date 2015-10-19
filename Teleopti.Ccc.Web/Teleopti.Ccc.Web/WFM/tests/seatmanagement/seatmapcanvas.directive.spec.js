describe('seatmapcanvas directive', function() {
	var element;

	beforeEach(module('wfm'));
	beforeEach(module('wfm.templates'));

	beforeEach(inject(function($compile, $rootScope, $httpBackend) {
		fabric = {
			CanvasWithViewport: function() {
				return {
					on: jasmine.createSpy(),
					setBackgroundImage: jasmine.createSpy(),
					clear: jasmine.createSpy()
				}
			},
			util: {
				addListener: jasmine.createSpy()
			},
			Object: jasmine.createSpy()
		};
		$httpBackend.expectGET("../api/Global/Language?lang=en").respond(200, 'mock');
		$httpBackend.expectGET("../api/Global/User/CurrentUser").respond(200, 'mock');
		$httpBackend.expectGET("../api/SeatPlanner/SeatMap").respond(200, 'mock');
		element = $compile('<seatmap-canvas />')($rootScope);
		$rootScope.$apply();
	}));

	it('works', function() {
		expect(true).toBeTruthy();
	});
});
