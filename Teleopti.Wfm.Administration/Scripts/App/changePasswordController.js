(function () {
	'use strict';

	angular
		 .module('adminApp')
		 .controller('changePasswordController', changePasswordController, []);

	function changePasswordController($scope, $http) {
		var vm = this;
		vm.viewName = 'Here we kan change our pasword later on';

	}

})();