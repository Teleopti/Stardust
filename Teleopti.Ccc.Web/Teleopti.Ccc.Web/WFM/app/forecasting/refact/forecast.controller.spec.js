'use strict';
describe('ForecastCtrl', function () {
  var $q,
  $httpBackend,
  $controller,
  forecastingService,
  $interval,
  fakeBackend,
  vm,
  skill,
  skill1,
  scenario;

  beforeEach(function() {
    module('wfm.forecasting');
  });

  beforeEach(inject(function(_$httpBackend_, _$controller_, _$interval_, _forecastingService_, _fakeForecastingBackend_) {
    $httpBackend = _$httpBackend_;
    $controller = _$controller_;
    $interval = _$interval_;
    fakeBackend = _fakeForecastingBackend_;

    // fakeBackend.clear();
    vm = $controller('ForecastRefactController');
    skill = {
      IsPermittedToModifySkill: true,
      Skills: [
        {
          Id: 'f08d75b3-fdb4-484a-ae4c-9f0800e2f753',
          Workloads: [
            {
              Id: "b8a74a6c-3125-4c13-a19a-9f0800e35a1f",
              Name: "Channel Sales - Marketing",
              Accuracies: null
            }
          ]
        }
      ]
    }

    vm.forecastModalObj = {
      SkillId:'f08d75b3-fdb4-484a-ae4c-9f0800e2f753',
      ChartId: 'chartb8a74a6c-3125-4c13-a19a-9f0800e35a1f',
      Workload:[{
        Id:"b8a74a6c-3125-4c13-a19a-9f0800e35a1f",
        Name:"Channel Sales - Marketing",
        Accuracies:null
      }]
    };

    scenario = {
      Id:"e21d813c-238c-4c3f-9b49-9b5e015ab432",
      Name:"Default",
      DefaultScenario:true
    }

  }));


  it("should get skills, status and scenario", inject(function ($controller) {
    fakeBackend.withSkill(skill);
    fakeBackend.withForecastStatus(true);
    fakeBackend.withScenario(scenario);
    $httpBackend.flush();
    expect(vm.skillMaster.Skills.length).toEqual(1);
    expect(vm.skillMaster.IsPermittedToModify).toEqual(true);
    expect(vm.skillMaster.IsForecastRunning).toEqual(true);
    expect(vm.skillMaster.Scenarios.length).toEqual(1);
  }));

  it("should default forecasting period to next month", inject(function () {
    expect(vm.forecastPeriod.startDate).toEqual(moment().utc().add(1, 'months').startOf('month').toDate());
    expect(vm.forecastPeriod.endDate).toEqual(moment().utc().add(2, 'months').startOf('month').toDate());
  }));

  it("should close forecasting modal if there is no workload", inject(function () {
    vm.forecastModal = true;
    vm.forecastingModal();

    expect(vm.forecastModal).toEqual(false);
    expect(vm.forecastPeriod.startDate).toEqual(moment().utc().add(1, 'months').startOf('month').toDate());
    expect(vm.forecastPeriod.endDate).toEqual(moment().utc().add(2, 'months').startOf('month').toDate());
  }));

  it('should get correct data for export', inject(function () {
    vm.selectedScenario = scenario;
    vm.forecastingModal(false, true);

    expect(vm.exportModal).toEqual(true);
    expect(vm.forecastPeriod.startDate).toEqual(moment().utc().add(1, 'months').startOf('month').toDate());
    expect(vm.forecastPeriod.endDate).toEqual(moment().utc().add(2, 'months').startOf('month').toDate());
    expect(vm.forecastModalObj.SkillId).toEqual('f08d75b3-fdb4-484a-ae4c-9f0800e2f753');
    expect(vm.selectedScenario.Id).toEqual('e21d813c-238c-4c3f-9b49-9b5e015ab432');
  }));

  it('should not allow forecasts longer than one year', inject(function () {
    vm.forecastPeriod = {
      startDate:  moment().utc().add(1, 'months').startOf('month').toDate(),
      endDate: moment().utc().add(2, 'years').startOf('month').toDate()
    };

    expect(vm.disableMoreThanOneYear()).toEqual(true);
  }));

});
