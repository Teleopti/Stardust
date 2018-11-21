(function() {
	'use strict';

	angular.module('wfm.forecasting').service('ForecastingService',
		[
			'$resource',
			'$http', forecastingService
		]);

	function forecastingService($resource, $http) {
		this.skills = $resource(
			'../api/Forecasting/Skills',
			{},
			{
				query: { method: 'GET', params: {}, isArray: false }
			}
		);

		this.scenarios = $resource(
			'../api/Global/Scenario',
			{},
			{
				query: { method: 'GET', params: {}, isArray: true }
			}
		);

		this.forecast = function (data, successCb, errorCb, finalCb) {
			$http
				.post('../api/Forecasting/Forecast', data)
				.success(successCb)
				.error(errorCb)
				.finally(finalCb);
		};

		this.result = function (data, successCb, errorCb) {
			$http
				.post('../api/Forecasting/LoadForecast', data)
				.success(successCb)
				.error(errorCb);
		};

		this.history = function (data, successCb, errorCb, finalCb) {
			$http
				.post('../api/Forecasting/QueueStatistics', data)
				.success(successCb)
				.error(errorCb)
				.finally(finalCb);
		};

		this.campaign = function (data, successCb, errorCb, finalCb) {
			$http
				.post('../api/Forecasting/Campaign', data)
				.success(successCb)
				.error(errorCb)
				.finally(finalCb);
		};

		this.override = function (data, successCb, errorCb, finalCb) {
			$http
				.post('../api/Forecasting/Override', data)
				.success(successCb)
				.error(errorCb)
				.finally(finalCb);
		};

		this.applyToScenario = function (data, successCb, errorCb) {
			$http
				.post('../api/Forecasting/ApplyForecast', data)
				.success(successCb)
				.error(errorCb);
		};

		this.exportForecast = function (data, successCb, errorCb) {
			$http({
				url: '../api/Forecasting/Export',
				method: 'POST',
				data: data,
				responseType: 'arraybuffer',
				headers: {
					Accept:
						'application/vnd.ms-excel, application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
				}
			})
				.success(successCb)
				.error(errorCb);
		};
	}
})();
