'use strict';

angular.module('wfm.forecasting')
	.service('Forecasting', [
		'$resource', function($resource) {
			this.skills = $resource('../api/Forecasting/Skills', {}, {
				get: { method: 'GET', params: {}, isArray: true }
			});

			this.status = $resource('../api/Forecasting/Status', {}, {
				get: { method: 'GET', params: {}, isArray: false }
			});

			this.skillList = this.skills.query();
		}
	]);