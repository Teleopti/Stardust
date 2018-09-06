(function() {
	'use strict';

	angular.module('wfm.authentication').controller('AuthenticationController', AuthenticationController);

	AuthenticationController.$inject = ['$filter', '$window', 'Toggle', 'CurrentUserInfo'];

	function AuthenticationController($filter, $window, Toggle, currentUserInfo) {
		var vm = this;
		vm.currentUserInfo = currentUserInfo;
		

		vm.isPasswordToggleActive = function() {
			return Toggle.Wfm_Authentication_ChangePasswordMenu_76666;
		};

		vm.isTeleoptiApplicationLogonUser = function() {
			return vm.currentUserInfo.CurrentUserInfo().IsTeleoptiApplicationLogon;
		};
	}
})();
