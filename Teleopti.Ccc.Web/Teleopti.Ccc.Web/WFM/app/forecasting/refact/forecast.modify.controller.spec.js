describe('ForecastCtrl', function() {

  var vm,
  $stateParams,
  $controller,
  $httpBackend,
  fakeBackend,
  skill,
  scenario;

  sessionStorage.currentForecastWorkload = '{"ChartId":"123","SkillId":"xyz","Workload":{"Id":"abc","Name":"workloadName","Accuracies":null},"Accuracies":null,"Id":"dfg","Name":"skillName"}';

  beforeEach(function(){

    module('wfm.forecasting');

    inject(function($rootScope, _$controller_, _$httpBackend_,  _fakeForecastingBackend_){
      scope = $rootScope.$new();
      $controller = _$controller_;
      $httpBackend = _$httpBackend_;
      fakeBackend = _fakeForecastingBackend_;

      vm = $controller('ForecastModController');

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

      $httpBackend.whenPOST('../api/Forecasting/ForecastResult').respond(function (method, url, data, headers) {
        return [201, {
          Days: [{
            date:"2018-04-25T00:00:00",
            vacw:4.7,
            vc:1135.2999999999997,
            vtacw:4.7,
            vtc:1135.2999999999997,
            vtt:158.10038509999998,
            vttt:158.10038509999998
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
    var today = moment().utc().format("MMM Do YY");
    var testStartDate = vm.forecastPeriod.startDate.format("MMM Do YY");
    var testEndDate = vm.forecastPeriod.endDate.format("MMM Do YY");
    expect(testStartDate).toEqual(today);
    expect(testEndDate).toEqual(moment().utc().add(6, 'months').format("MMM Do YY"));
  }));

  it('should get correct data for export', inject(function () {
    vm.selectedScenario = scenario;

    expect(vm.selectedWorkload.Id).toEqual('dfg');
    expect(vm.selectedScenario.Id).toEqual('e21d813c-238c-4c3f-9b49-9b5e015ab432');
  }));

});
