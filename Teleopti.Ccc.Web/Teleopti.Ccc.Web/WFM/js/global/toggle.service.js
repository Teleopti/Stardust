
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

		var loadToggle = function(toggle) {
			return $q(function(resolve) {
				that.isFeatureEnabled.query({
						toggle: toggle
					}).$promise
					.then(function(data) {
						that[toggle] = data.IsEnabled;
						resolve();
					});
			});
		};
		
		that.togglesLoaded = $q.all([
			loadToggle('RTA_AdherenceDetails_34267'),
			loadToggle('Wfm_RTA_ProperAlarm_34975'),
		]);

	}
]);
