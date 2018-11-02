(function() {
	'use strict';
	angular.module('wfm.teapot').controller('TeapotController', constructor);

	constructor.$inject = ['$rootScope', '$http', '$scope', 'rtaPollingService', 'versionService'];

	function constructor($rootScope, $http, $scope, rtaPollingService, versionService) {
		var vm = this;

		function getUrl() {
			return versionService.getVersion() == '2.0'
				? '../api/teapot/MakeGoodCoffee'
				: '../api/teapot/MakeBadCoffee';
		}

		var poller = rtaPollingService
			.create(function() {
				return $http.get(getUrl()).then(function(response) {
					vm.teapot = response.data;
				});
			}, 1000)
			.start();

		$scope.$on('$destroy', poller.destroy);
	}
})();
