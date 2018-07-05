describe('ForecastCtrl', function() {

  var vm,
  $controller,
  $httpBackend,
  fakeBackend,
  skill,
  scenario,
  scenario2;

  sessionStorage.currentForecastWorkload = '{"ChartId":"123","SkillId":"xyz","Workload":{"Id":"abc","Name":"workloadName","Accuracies":null},"Accuracies":null,"Id":"dfg","Name":"skillName"}';

  beforeEach(function(){

    module('wfm.forecasting');

    inject(function($rootScope, _$controller_, _$httpBackend_,  _fakeForecastingBackend_){
      scope = $rootScope.$new();
      $controller = _$controller_;
      $httpBackend = _$httpBackend_;
      fakeBackend = _fakeForecastingBackend_;

      vm = $controller('ForecastModController', {$scope: scope});

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

      scenario = {
        Id:"e21d813c-238c-4c3f-9b49-9b5e015ab432",
        Name:"Default",
        DefaultScenario:true
      }

      scenario2 = {
        Id:"18b34548-3604-49fe-9584-9b5e015ab432",
        Name:"High",
        DefaultScenario:false
      }

      $httpBackend.whenPOST('../api/Forecasting/LoadForecast').respond(function (method, url, data, headers) {
        return [201, {
          ForecastDays: [{
            Date:"2018-04-25T00:00:00",
            AverageAfterTaskTime:4.7,
            Tasks:1135.2999999999997,
            TotalAverageAfterTaskTime:4.7,
            TotalTasks:1135.2999999999997,
            AverageTaskTime:158.10038509999998,
            TotalAverageTaskTime:158.10038509999998
          }],
          WorkloadId: '123'
        }];
      });
    });
  });

  it('should handle stateParam data', function() {
    expect(vm.selectedWorkload.Id).toBe('dfg');
    expect(vm.selectedWorkload.Workload.Id).toBe('abc');
    expect(vm.selectedWorkload.Workload.Name).toBe('workloadName');
    expect(vm.selectedWorkload.Name).toBe('skillName');
  });

  it("should get workload with days", inject(function ($controller) {
    fakeBackend.withSkill(skill);
    fakeBackend.withForecastStatus(true);
    fakeBackend.withScenario(scenario);
    $httpBackend.flush();
    expect(vm.isForecastRunning).toEqual(false);
    expect(vm.selectedWorkload.Days.length).toEqual(1);
    expect(vm.selectedScenario.Id).toEqual(scenario.Id);
  }));

  it("should default forecasting period to next 6 month", inject(function () {
    var today = moment().utc().add(1, 'days').format("MMM Do YY");
    var testStartDate = moment(vm.forecastPeriod.startDate).utc().format("MMM Do YY");
    var testEndDate = moment(vm.forecastPeriod.endDate).utc().format("MMM Do YY");
    expect(testStartDate).toEqual(today);
    expect(testEndDate).toEqual(moment().utc().add(6, 'months').format("MMM Do YY"));
  }));

  it('should get correct data for export', inject(function () {
    vm.selectedScenario = scenario;

    expect(vm.selectedWorkload.Id).toEqual('dfg');
    expect(vm.selectedScenario.Id).toEqual('e21d813c-238c-4c3f-9b49-9b5e015ab432');
  }));

  it('should be able to forecast clean scenario', inject(function () {
    vm.changeScenario(scenario2);

    expect(vm.selectedWorkload.Days.length).toEqual(0);
    expect(vm.selectedScenario.Id).toEqual(scenario2.Id);

    vm.forecastWorkload();
    //should complete forecast without issue
  }));
});
