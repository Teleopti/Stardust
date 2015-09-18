'use strict';

angular.module('wfm.forecasting')
	.service('Forecasting', [
		'$resource', function($resource) {
			this.skills = $resource('../api/Forecasting/Skills', {}, {
				get: { method: 'GET', params: {}, isArray: true }
			});

			this.status = $resource('../api/Status/Forecasting', {}, {
				get: { method: 'GET', params: {}, isArray: false }
			});

			this.scenarios = $resource('../api/Forecasting/Scenarios', {}, {
				get: { method: 'GET', params: {}, isArray: false }
			});

			this.scenarioList = this.scenarios.query();

		}
	]);