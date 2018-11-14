'use strict';
describe('ForecastRefactController', function () {
  var $httpBackend,
    $controller,
    $interval,
    fakeBackend,
    vm,
    skill,
    scenario;

  beforeEach(function() {
    module('wfm.forecasting');
  });

  beforeEach(inject(function(_$httpBackend_, _$controller_, _$interval_, _fakeForecastingBackend_) {
    $httpBackend = _$httpBackend_;
    $controller = _$controller_;
    $interval = _$interval_;
    fakeBackend = _fakeForecastingBackend_;

    vm = $controller('ForecastRefactController');
    skill = {
      IsPermittedToModifySkill: true,
      Skills: [
        {
          Id: 'SkillId-123',
          Workloads: [
            {
              Id: "WorkloadId-456",
              Name: "WorkloadName-789",
              Accuracies: null
            }
          ]
        }
      ]
    }

    vm.forecastModalObj = {
      SkillId:'SkillId-123',
      ChartId: 'chartWorkloadId-456',
      Workload:[{
        Id:"WorkloadId-456",
        Name:"WorkloadName-789",
        Accuracies:null
      }]
    };

    scenario = {
      Id:"ScenarioId-ABC",
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
    expect(vm.forecastModalObj.SkillId).toEqual('SkillId-123');
    expect(vm.selectedScenario.Id).toEqual('ScenarioId-ABC');
  }));

  it('should not allow forecasts longer than one year', inject(function () {
    vm.forecastPeriod = {
      startDate:  moment().utc().add(1, 'months').startOf('month').toDate(),
      endDate: moment().utc().add(2, 'years').startOf('month').toDate()
    };

    expect(vm.disableMoreThanOneYear()).toEqual(true);
  }));
});
