(function () {
  'use strict';

  angular.module('adminApp')
  .component('etlModal',
  {
    templateUrl: '/Etl/modal.html',
    controller: ['$http', 'tokenHeaderService', ModalCtrl],
    controllerAs: 'ctrl',
    bindings: {
      tenant: '<',
      output: '='
    }
  });

  function ModalCtrl($http, tokenHeaderService) {
    var ctrl = this;

    ctrl.baseConfig = null;
    ctrl.configData = null;

    (function getConfigurationModel() {
      $http.get("./Etl/GetConfigurationModel", tokenHeaderService.getHeaders())
      .success(function (data) {
          ctrl.configData = data;
          console.log(ctrl.configData);
      });
    })();

    ctrl.sendBaseConfig = function () {
      var baseObj = {
        TenantName: ctrl.tenant.TenantName,
        BaseConfig: {
          CultureId: ctrl.baseConfig.culture,
          IntervalLength: ctrl.baseConfig.interval,
          TimeZoneCode: ctrl.baseConfig.timezone
        }
      }

      $http.post("./Etl/SaveConfigurationForTenant", baseObj, tokenHeaderService.getHeaders())
      .success(function (data) {
        ctrl.output = true;
        console.log('done');
      });
    }

  }
})();
