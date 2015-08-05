'use strict';

angular.module('toggleService', ['ngResource']).service('Toggle', [
	'$resource', function($resource) {
		this.isFeatureEnabled = $resource('../ToggleHandler/IsEnabled?toggle=:toggle',
		{
			toggle: "@toggle"
		}, {
			query: {
				method: 'GET',
				params: {},
				isArray: false
			}
		});
	}
]);