(function () {
	'use strict';
	angular
		.module('wfm.teapot')
		.controller('TeapotController', constructor);

	constructor.$inject = ['$http', '$scope', 'rtaPollingService'];

	function constructor($http, $scope, rtaPollingService) {
		var vm = this;

		var poller = rtaPollingService.create(function () {
			return $http.get('../api/teapot/MakeBadCoffee').then(function (response) {
				vm.teapot = response.data;
			});
		}, 1000).start();

		$scope.$on('$destroy', poller.destroy);
	}

})();
