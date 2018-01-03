(function () {
	'use strict';

	angular
		.module('toggleService', ['ngResource'])
		.provider('Toggle', ToggleProvider);

	var provider;
	var service = {};

	function ToggleProvider() {
		provider = this;
		this.$get = function ($injector) {
			$injector.invoke(ToggleService);
			return service;
		};
	}

	function ToggleService($resource) {

		service.togglesLoaded =
			$resource('../ToggleHandler/AllToggles', {}, {
				query: {
					method: 'GET',
					isArray: false
				}
			}).query().$promise
				.then(function (result) {
					for (var toggle in result) {
						if (toggle.indexOf('$') !== 0 && result.hasOwnProperty(toggle) && typeof (result[toggle]) === 'boolean') {
							service[toggle] = result[toggle];
							provider[toggle] = result[toggle];
						}
					}
				});

		return service;
	}

})();
