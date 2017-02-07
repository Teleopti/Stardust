(function () {
	'use strict';

	angular
	  .module('wfm.dataProtection')
	  .controller('DataProtectionCtrl', DataProtectionCtrl);

	DataProtectionCtrl.inject = ['DataProtectionService'];
	function DataProtectionCtrl(DataProtectionService) {
		var vm = this;

		vm.sendYesResponse = sendYesResponse;
		vm.sendNoResponse = sendNoResponse;

		function sendYesResponse() {
			DataProtectionService.iAgree.query();
		}

		function sendNoResponse() {
			DataProtectionService.iDontAgree.query();
		}
	}
})();