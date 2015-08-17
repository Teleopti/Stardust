'use strict';

angular.module('wfm.forecasting')
	.service('Forecasting', ['$resource', function ($resource) {
		this.skills = $resource('../api/Forecasting/Skills', {}, {
			get: { method: 'GET', params: {}, isArray: true }
		});
	}]);