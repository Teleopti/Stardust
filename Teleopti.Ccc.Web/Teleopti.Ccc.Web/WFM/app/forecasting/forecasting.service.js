﻿(function () {
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

				this.scenarios = $resource('../api/Global/Scenario', {}, {
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

				this.campaign = function(data, successCb, errorCb, finalCb) {
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

				this.applyToScenario = function(data, successCb, errorCb) {
					$http.post("../api/Forecasting/ApplyForecast", data)
						.success(successCb)
						.error(errorCb);
				};

				this.evaluate = function(data, successCb, errorCb) {
					$http.post("../api/Forecasting/Evaluate", data)
						.success(successCb)
						.error(errorCb);
				};

				this.queueStatistics = function(data, successCb, errorCb) {
					$http.post("../api/Forecasting/QueueStatistics", data)
						.success(successCb)
						.error(errorCb);
				};

				this.exportForecast = function(data, successCb, errorCb) {
					$http({
						url: '../api/Forecasting/Export',
							method: 'POST',
							data: data,
							responseType: 'arraybuffer',
							headers: {
								'Accept': 'application/vnd.ms-excel, application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
							}
						})
						.success(successCb)
						.error(errorCb);
				};
			}
		]);
})();
