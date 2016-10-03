(function() {
	'use strict';

	angular
	.module('toggleService', ['ngResource'])
	.factory('Toggle', Toggle);

	Toggle.$inject = ['$resource', '$q'];

	function Toggle($resource, $q) {
		var service = this;
		var togglesLoaded = $q.all([
			loadAllToggles().then(function(result){
				for (var toggle in result) {
					if (toggle.indexOf('$') !== 0 && result.hasOwnProperty(toggle) && typeof (result[toggle]) === 'boolean') {
						service[toggle] = result[toggle];
					}
				}
			})
		]);

	 service = {
			togglesLoaded: togglesLoaded
		};

		function loadAllToggles() {
			return $resource('../ToggleHandler/AllToggles', {}, {
				query: {
					method: 'GET',
					isArray: false
				}
			}).query().$promise;
		}
		return service;

	}
})();
