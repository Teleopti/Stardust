'use strict';

angular.module('toggleService', ['ngResource']).service('Toggle', [
	'$resource', function($resource) {
		var that = this;

		that.isFeatureEnabled = $resource('../ToggleHandler/IsEnabled?toggle=:toggle',
		{
			toggle: "@toggle"
		}, {
			query: {
				method: 'GET',
				params: {},
				isArray: false
			}
		});

		that.toggle = function(toggle, callback) {
			return that.isFeatureEnabled.query({ toggle: toggle }).$promise;
		};

	}
]);
