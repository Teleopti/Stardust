
'use strict';

angular.module('toggleService', ['ngResource']).service('Toggle', [
	'$resource', '$q',
	function($resource, $q) {
		var that = this;

		that.isFeatureEnabled = $resource('../ToggleHandler/IsEnabled', {}, {
			query: {
				method: 'GET',
				isArray: false
			}
		});

		that.loadAllToggles = $resource('../ToggleHandler/AllToggles', {}, {
			query: {
				method: 'GET',
				isArray: false
			}
		});

		that.togglesLoaded = $q.all([
			that.loadAllToggles.query().$promise.then(function(result) {
				for (var toggle in result) {
					if (!toggle.startsWith('$') && result.hasOwnProperty(toggle) && typeof (result[toggle]) === 'boolean') {
						that[toggle] = result[toggle];
					}
				}
			})
		]);
	}
]);
