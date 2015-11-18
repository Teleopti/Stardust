
'use strict';

angular.module('toggleService', ['ngResource']).service('Toggle', [
	'$resource', '$q',
	function($resource, $q) {
		var that = this;

		that.isFeatureEnabled = $resource('../ToggleHandler/IsEnabled?toggle=:toggle', {
			toggle: "@toggle"
		}, {
			query: {
				method: 'GET',
				params: {},
				isArray: false
			}
		});

		that.toggle = function(toggle) {
			return $q(function(resolve) {
				that.isFeatureEnabled.query({
						toggle: toggle
					}).$promise
					.then(function(data) {
						resolve(data.IsEnabled);
					});
			});
		};

	}
]);
