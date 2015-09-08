(function () {
    'use strict';

    angular
		 .module('adminApp')
		 .controller('listController', listController, ['tokenHeaderService']);

   
    function listController($scope, $http, tokenHeaderService) {
    	
    	$http.get("./api/Home/GetAllTenants", tokenHeaderService.getHeaders()).success(function (data) {
            $scope.Tenants = data;
        }).error(function (xhr, ajaxOptions, thrownError) {
        	console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
        });
    }
 
})();