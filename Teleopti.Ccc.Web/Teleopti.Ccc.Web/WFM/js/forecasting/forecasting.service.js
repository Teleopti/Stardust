'use strict';

angular.module('wfm.forecasting')
	.service('Forecasting', ['$resource', function ($resource) {
		this.skills = $resource('../api/Forecasting/Skills', {}, {
			get: { method: 'GET', params: {}, isArray: true }
		});
	}]);

angular.module('wfm.forecasting')
	.factory('RunningLock', ['$rootScope', 'Hub', function ($rootScope, Hub) {

		var runningLock = this;
		runningLock.isLock = false;

		var hub = new Hub('forecastHub', {
			rootPath: "../signalr",
			listeners: {
				'lockForecast': function () {
					runningLock.isLock = true;
					$rootScope.$apply();
				},
				'unlockForecast': function () {
					runningLock.isLock = false;
					$rootScope.$apply();
				}
			},

			errorHandler: function (error) {
				console.log(error);
			}
		});

		return runningLock;
	}]);