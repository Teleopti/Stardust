(function () {
    'use strict';
    var tokenKey = 'accessToken';
    angular
		 .module('adminApp')
		 .controller('listController', listController, []);

    function getHeaders() {
    	return {
    		headers: { 'Authorization': 'Bearer ' + sessionStorage.getItem(tokenKey) }
    	};
    }

    function listController($scope, $http) {
    	var token = sessionStorage.getItem(tokenKey);
    	if (token === null) {
		    return;
    	}
		
    	$http.get("./api/Home/GetAllTenants", getHeaders()).success(function (data) {
            $scope.Tenants = data;
        }).error(function (xhr, ajaxOptions, thrownError) {
            console.log(xhr.status + xhr.responseText + thrownError);
        });
    }

    
    
})();