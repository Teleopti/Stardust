'use strict';
describe('ForecastingStartCtrl', function () {
	var $q,
	    $rootScope,
	    $httpBackend;

	beforeEach(module('wfm.forecasting'));

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
		$controller('ForecastingStartCtrl', { $scope: scope, $state: {} });
		expect(scope.period.startDate.toString()).toBe(moment().utc().add(1, 'months').startOf('month').toDate().toString());
		expect(scope.period.endDate.toString()).toBe(moment().utc().add(2, 'months').startOf('month').toDate().toString());
	}));
});
