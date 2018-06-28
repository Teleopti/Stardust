(function() {
  'use strict';

  angular.module('wfm.forecasting')
  .controller('r2ForecastRefactController', r2ForecastCtrl);

  r2ForecastCtrl.$inject = ['forecastingService', '$state', '$stateParams', 'NoticeService', '$translate', '$window', 'skillIconService'];

  function r2ForecastCtrl(forecastingService, $state, $stateParams, noticeSvc, $translate, $window, skillIconService) {
    var vm = this;

    vm.skills = [];
    vm.skilltypes = [];
    vm.goToModify = goToModify;
    vm.getSkillIcon = skillIconService.get;

    function init(){
      setReleaseNotification();
      getAllSkills();
    }

    function setReleaseNotification() {
      var message = $translate.instant('WFMReleaseNotificationWithoutOldModuleLink')
        .replace('{0}', $translate.instant('Forecast'))
        .replace('{1}', '<a href="http://www.teleopti.com/wfm/customer-feedback.aspx" target="_blank">')
        .replace('{2}', '</a>');
      noticeSvc.info(message, null, true);
    }

    function getAllSkills() {
      vm.skills = [];
      forecastingService.skills.query().$promise.then(function (result) {
        result.Skills.forEach(function(s){
          s.Workloads.forEach(function(w){
            var temp = {
              Workload: w,
              SkillId: s.Id,
              SkillType:{
                DoDisplayData: checkSupportedTypes(s.SkillType),
                SkillType: s.SkillType
              },
              ChartId: "chart" + w.Id
            };
            vm.skills.push(temp)
            if (!vm.skilltypes.includes(temp.SkillType.SkillType)) {
              vm.skilltypes.push(temp.SkillType.SkillType);
            }
          });
        });
      });
    }

    function checkSupportedTypes(type) {
      var supportedSkillTypes = ['SkillTypeInboundTelephony'];
      for (var i = 0; i < supportedSkillTypes.length; i++) {
        if (supportedSkillTypes[i] === type) {
          return true;
        }
      }
      return false;
    }

    function goToModify(skill) {
      sessionStorage.currentForecastWorkload = JSON.stringify(skill);
      $state.go("modify", {workloadId:skill.Workload.Id});
    }

    init();
  }
})();
