(function () {
	'use strict';

	angular.module('adminApp').factory('tenantService', tenantService);

	tenantService.$inject = ['$http', 'tokenHeaderService'];

	function tenantService($http, tokenHeaderService) {
		var vm = this;

		var tenant = {
			loadTenant: loadTenant
		};

		return tenant;

		function loadTenant(tenant) {
			return $http.post('./GetOneTenant', '"' + tenant + '"', tokenHeaderService.getHeaders())
				.then(function (response) {
					return response.data;
				});
		};
	}
})();

