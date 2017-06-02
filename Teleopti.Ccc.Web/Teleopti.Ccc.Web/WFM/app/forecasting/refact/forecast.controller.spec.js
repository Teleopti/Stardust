'use strict';
describe('ForecastCtrl', function () {
  var $q,
  $httpBackend,
  $controller,
  forecastingService,
  $interval,
  vm;

  beforeEach(function() {
		module('wfm.forecasting');
	});

	beforeEach(inject(function(_$httpBackend_, _$controller_, _$interval_, _forecastingService_) {
		$httpBackend = _$httpBackend_;
		$controller = _$controller_;
		$interval = _$interval_;

		// fakeBackend.clear();
		vm = $controller('ForecastRefactCtrl');

    //default stuff
	}));

  // it("should get skills", inject(function ($controller) {
  //   vm.init();
  // }));

  it("forecasting period should default to next month", inject(function ($controller) {
    vm.init();
    expect(vm.forecastPeriod.startDate).toEqual(moment().utc().add(1, 'months').startOf('month').toDate());
    expect(vm.forecastPeriod.endDate).toEqual(moment().utc().add(2, 'months').startOf('month').toDate());
  }));


});
