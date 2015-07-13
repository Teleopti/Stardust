'use strict';

angular.module('wfm.forecasting')
	.service('Forecasting', ['$resource', function ($resource) {
		this.skills = $resource('../api/Forecasting/Skills', {}, {
			get: { method: 'GET', params: {}, isArray: true }
		});

		this.isToggleEnabled = $resource('../ToggleHandler/IsEnabled?toggle=:toggle',
		{
			toggle: "@toggle"
		}, {
			query: {
				method: 'GET',
				params: {},
				isArray: false
			}
		});
	}]);