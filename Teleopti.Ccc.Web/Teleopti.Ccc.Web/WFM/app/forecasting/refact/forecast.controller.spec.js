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

    var skill1 = {
      Workload:{
        Id:"b8a74a6c-3125-4c13-a19a-9f0800e35a1f",
        Name:"Channel Sales - Marketing",
        Accuracies:null
      },
      ChartId:"chartb8a74a6c-3125-4c13-a19a-9f0800e35a1f"
    };

  }));

  it("forecasting period should default to next month", inject(function ($controller) {
    vm.init();
    expect(vm.forecastPeriod.startDate).toEqual(moment().utc().add(1, 'months').startOf('month').toDate());
    expect(vm.forecastPeriod.endDate).toEqual(moment().utc().add(2, 'months').startOf('month').toDate());
  }));

  it("forecasting modal should close if ther is no workload", inject(function ($controller) {
    vm.init();
    vm.forecastModal = true;
    vm.forecastingModal();
    expect(vm.forecastModal).toEqual(false);
    expect(vm.forecastPeriod.startDate).toEqual(moment().utc().add(1, 'months').startOf('month').toDate());
    expect(vm.forecastPeriod.endDate).toEqual(moment().utc().add(2, 'months').startOf('month').toDate());
  }));

});
