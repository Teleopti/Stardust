'use strict';
describe('ForecastingCtrl', function () {
	var $q,
	    $rootScope,
	    $httpBackend;

	beforeEach(module('wfm'));

	beforeEach(inject(function (_$httpBackend_, _$q_, _$rootScope_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
		$httpBackend.expectGET("../api/Global/Language?lang=en").respond(200, 'mock');
		$httpBackend.expectGET("../api/Global/User/CurrentUser").respond(200, 'mock');
	}));

	it("forecasting period should default to next month", inject(function ($controller) {
		var scope = $rootScope.$new();

		expect(scope.period).toBe(undefined);
		$controller('ForecastingCtrl', { $scope: scope, $state: {} });
		expect(scope.period.startDate).toBe(moment().utc().add(1, 'months').startOf('month').toDate());
		expect(scope.period.endDate).toBe(moment().utc().add(2, 'months').startOf('month').toDate());
	}));
});
