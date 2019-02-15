angular
	.module('adminApp')
	.factory('tenantService', tenantService);

tenantService.$inject = ['$http'];

function tenantService($http) {
	return {
		getTenants: getTenants
	};

	function getTenants() {
		return $http.get('/AllTenants')
			.then(getAllTenantsComplete)
			.catch(getAllTenantsFailed);

		function getAllTenantsComplete(response) {
			return response.data;
		}

		function getAllTenantsFailed(error) {
			console.log('XHR Failed .' + error.data);
		}
	}
}