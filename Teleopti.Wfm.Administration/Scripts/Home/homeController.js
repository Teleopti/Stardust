(function () {
	'use strict';

	angular
		 .module('homeApp')
		 .controller('homeController', homeController, []);

	function homeController($scope, $http) {
		$scope.name = 'ett vanligt namn';
		$http.get('Home/GetAllTenants').success(function (data) {
			$scope.Tenants = data;
		}).error(function (xhr, ajaxOptions, thrownError) {
			console.log(xhr.status + xhr.responseText + thrownError);
		});
	}

})();