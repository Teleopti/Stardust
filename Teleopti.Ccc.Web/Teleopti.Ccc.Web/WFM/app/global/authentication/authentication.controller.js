(function() {
	'use strict';

	angular.module('wfm.authentication').controller('AuthenticationController', AuthenticationController);

	AuthenticationController.$inject = ['$filter', '$window', 'Toggle'];

	function AuthenticationController($filter, $window, Toggle) {
		var vm = this;

		vm.isToggleActive = function() {
			return Toggle.Wfm_Authentication_ChangePasswordMenu_76666;
		};
	}
})();
