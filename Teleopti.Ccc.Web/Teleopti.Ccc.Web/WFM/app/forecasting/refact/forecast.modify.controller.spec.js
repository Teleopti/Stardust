describe('ForecastCtrl', function() {

  var vm, $stateParams, $controller;

  $stateParams = {"workloadId":"b8a74a6c-3125-4c13-a19a-9f0800e35a1f","skill":{"Workload":{"Id":"b8a74a6c-3125-4c13-a19a-9f0800e35a1f","Name":"Channel Sales - Marketing","Accuracies":null},"SkillId":"f08d75b3-fdb4-484a-ae4c-9f0800e2f753","ChartId":"chartb8a74a6c-3125-4c13-a19a-9f0800e35a1f"},"days":[{"date":"2017-09-01T00:00:00","vc":0,"vtc":0,"vtt":0,"vttt":0,"vacw":0,"vtacw":0},{"date":"2017-09-06T00:00:00","vc":57.15742375064427,"vtc":57.15742375064427,"vtt":179.88473929999998,"vttt":179.88473929999998,"vacw":0.7499175,"vtacw":0.7499175},{"date":"2017-09-07T00:00:00","vc":181.0906275877146,"vtc":181.0906275877146,"vtt":179.88473919999998,"vttt":179.88473919999998,"vacw":0.7499174,"vtacw":0.7499174}]};

  beforeEach(function(){

    module('wfm.forecasting');

    inject(function($rootScope, _$controller_){
      scope = $rootScope.$new();
      $controller = _$controller_;

      vm = $controller('ForecastModController', {
        $scope: scope,
        $stateParams: $stateParams
      });
    });
  });

  it('should handle stateParam data', function() {
    expect(vm.selectedWorkload.Id).toBe('b8a74a6c-3125-4c13-a19a-9f0800e35a1f');
    expect(vm.selectedWorkload.Name).toBe('Channel Sales - Marketing');
    expect(vm.selectedWorkload.Days.length).toBe(3);
  });

});
