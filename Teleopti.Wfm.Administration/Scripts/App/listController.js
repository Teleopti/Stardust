(function () {
	'use strict';

	angular
		 .module('adminApp')
		 .controller('listController', listController, []);

	function listController($scope, $http) {
		$http.get('../api/Home/GetAllTenants').success(function (data) {
			$scope.Tenants = data;
		}).error(function (xhr, ajaxOptions, thrownError) {
			console.log(xhr.status + xhr.responseText + thrownError);
		});
	}

})();