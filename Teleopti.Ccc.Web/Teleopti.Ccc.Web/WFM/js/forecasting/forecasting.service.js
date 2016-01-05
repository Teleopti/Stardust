(function () {
	'use strict';

	angular.module('wfm.forecasting')
		.service('forecastingService', [
			'$resource', '$http', function($resource, $http) {
				this.skills = $resource('../api/Forecasting/Skills', {}, {
					query: { method: 'GET', params: {}, isArray: false }
				});

				this.status = $resource('../api/Status/Forecasting', {}, {
					get: { method: 'GET', params: {}, isArray: false }
				});

				this.scenarios = $resource('../api/Forecasting/Scenarios', {}, {
					query: { method: 'GET', params: {}, isArray: true }
				});

				this.forecast = function(data, successCb, errorCb, finalCb) {
					$http.post('../api/Forecasting/Forecast', data)
						.success(successCb)
						.error(errorCb)
						.finally(finalCb);
				};

				this.result = function(data, successCb, errorCb) {
					$http.post("../api/Forecasting/ForecastResult", data).
						success(successCb).
						error(errorCb);
				};

				this.applyCampaign = function(data, successCb, errorCb, finalCb) {
					$http.post("../api/Forecasting/Campaign", data)
						.success(successCb)
						.error(errorCb)
						.finally(finalCb);
				};

				this.override = function(data, successCb, errorCb, finalCb) {
					$http.post("../api/Forecasting/Override", data)
						.success(successCb)
						.error(errorCb)
						.finally(finalCb);
				};
			}
		]);
})();