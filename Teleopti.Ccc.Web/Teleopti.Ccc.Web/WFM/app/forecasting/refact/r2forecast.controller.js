(function() {
  'use strict';

  angular.module('wfm.forecasting')
  .controller('r2ForecastRefactController', r2ForecastCtrl);

  r2ForecastCtrl.$inject = ['forecastingService', '$state', '$stateParams', 'NoticeService', '$translate', '$window'];

  function r2ForecastCtrl(forecastingService, $state, $stateParams, NoticeService, $translate, $window) {
    var vm = this;

    vm.skills = [];
    vm.goToModify = goToModify;

    (function getAllSkills() {
      vm.skills = [];
      forecastingService.skills.query().$promise.then(function (result) {
        result.Skills.forEach(function(s){
          s.Workloads.forEach(function(w){
            var temp = {
              Workload: w,
              SkillId: s.Id,
              ChartId: "chart" + w.Id
            }
            vm.skills.push(temp)
          });
        });
      });
    })();

    function goToModify(skill) {
       sessionStorage.currentForecastWorkload = JSON.stringify(skill);
        $state.go("modify", {workloadId:skill.Workload.Id})
    }

  }
})();
