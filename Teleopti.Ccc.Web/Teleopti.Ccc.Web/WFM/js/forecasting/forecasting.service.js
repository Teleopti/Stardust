'use strict';

var forecastingService = angular.module('forecastingService', ['ngResource']);
forecastingService.service('Forecasting', ['$resource', function ($resource) {
	this.skills = $resource('../api/Forecasting/Skills', {}, {
		get: { method: 'GET', params: {}, isArray: true }
	});

	this.measureForecastMethod = $resource('../api/Forecasting/MeasureForecastMethod', {}, {
		get: { method: 'GET', params: {}, isArray: true }
	});

}]);