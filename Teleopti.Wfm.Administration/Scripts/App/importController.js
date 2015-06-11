(function () {
	'use strict';

	angular
		 .module('adminApp')
		 .controller('importController', importController, []);

	function importController($scope, $http) {
		$scope.viewName = 'IMPORTERA MERA';
		//$http.get('../api/Home/GetAllTenants').success(function (data) {
		//	$scope.Tenants = data;
		//}).error(function (xhr, ajaxOptions, thrownError) {
		//	console.log(xhr.status + xhr.responseText + thrownError);
		//});
	}

})();