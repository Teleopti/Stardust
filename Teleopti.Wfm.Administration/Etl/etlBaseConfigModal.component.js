(function () {
  'use strict';

  angular.module('adminApp')
  .component('etlModal',
  {
    templateUrl: './Etl/modal.html',
    controller: ['$http', 'tokenHeaderService', ModalCtrl],
    controllerAs: 'ctrl',
    bindings: {
      tenant: '<',
      output: '=',
      callback: '='
    }
  });

  function ModalCtrl($http, tokenHeaderService) {
    var ctrl = this;

    ctrl.baseConfig = null;
	ctrl.configData = null;

	  if (ctrl.tenant) {
		  ctrl.baseConfig = {
			  culture: ctrl.tenant.BaseConfig.CultureId,
			  interval: ctrl.tenant.BaseConfig.IntervalLength,
			  timezone: ctrl.tenant.BaseConfig.TimeZoneCode
		  }
	  }

    (function getConfigurationModel() {
      $http.get("./Etl/GetConfigurationModel", tokenHeaderService.getHeaders())
      .success(function (data) {
        ctrl.configData = data;
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
        ctrl.callback();
      });
    }

  }
})();
