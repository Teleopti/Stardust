(function () {
	'use strict';

	angular
		 .module('adminApp')
		 .controller('emptyController', emptyController, []);

	function emptyController($scope, $http) {
		$scope.viewName = 'TOMT OCH INNEHÅLLSLÖST';
		//$http.get('../api/Home/GetAllTenants').success(function (data) {
		//	$scope.Tenants = data;
		//}).error(function (xhr, ajaxOptions, thrownError) {
		//	console.log(xhr.status + xhr.responseText + thrownError);
		//});
	}

})();