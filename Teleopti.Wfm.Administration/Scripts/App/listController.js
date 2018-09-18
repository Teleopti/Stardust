﻿
(function() {
	'use strict';

	angular
		.module('adminApp')
		.controller('listController', listController, ['tokenHeaderService']);

	function listController($scope, $http, tokenHeaderService) {

		$http.get("./AllTenants", tokenHeaderService.getHeaders()).success(function(data) {
			$scope.Tenants = data;
		}).error(function(xhr, ajaxOptions, thrownError) {
			console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
		});

		$http.get("./AllConfigurations", tokenHeaderService.getHeaders()).success(function (data) {
			$scope.Configurations = data;
		}).error(function (xhr, ajaxOptions, thrownError) {
			console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
		});

		$http.get("./Toggle/AllOverrides", tokenHeaderService.getHeaders()).success(function (data) {
			$scope.Overrides = data;
		}).error(function (xhr, ajaxOptions, thrownError) {
			console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
		});
	}

})();
