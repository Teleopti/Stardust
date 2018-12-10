(function () {
	'use strict';

	angular.module('adminApp')
		.component('etlModal',
			{
				templateUrl: './Etl/modal.html',
				controller: ['$http', 'tokenHeaderService', '$window', ModalCtrl],
				controllerAs: 'ctrl',
				bindings: {
					tenant: '<',
					output: '=',
					callback: '='
				}
			});

	function ModalCtrl($http, tokenHeaderService, $window) {
		var ctrl = this;

		ctrl.baseConfig = null;
		ctrl.configData = null;

		ctrl.$onInit = function () {
			if (ctrl.tenant.IsBaseConfigured === true) {
				ctrl.baseConfig = {
					culture: ctrl.tenant.BaseConfig.CultureId,
					interval: ctrl.tenant.BaseConfig.IntervalLength,
					timezone: ctrl.tenant.BaseConfig.TimeZoneCode
				};
			} else {
				ctrl.baseConfig = {
					culture: 1033,
					interval: 15,
					timezone: "UTC"
				};
			}

			(function getConfigurationModel() {
				if ($window.sessionStorage.configData) {
					ctrl.configData = angular.fromJson($window.sessionStorage.configData);
				} else {
					$http.get("./Etl/GetConfigurationModel", tokenHeaderService.getHeaders())
						.then(function (response) {
							ctrl.configData = response.data;
							$window.sessionStorage.configData = angular.toJson(response.data);
						});
				}
			})();
		};

		ctrl.sendBaseConfig = function () {
			var baseObj = {
				TenantName: ctrl.tenant.TenantName,
				BaseConfig: {
					CultureId: ctrl.baseConfig.culture,
					IntervalLength: ctrl.baseConfig.interval,
					TimeZoneCode: ctrl.baseConfig.timezone
				}
			};

			$http.post("./Etl/SaveConfigurationForTenant", baseObj, tokenHeaderService.getHeaders())
				.then(function (response) {
					ctrl.output = true;
					ctrl.callback();
				});
		};
	}
})();
