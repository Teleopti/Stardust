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
		vm.notNowResponse = notNowResponse;

		function sendYesResponse() {
			DataProtectionService.iAgree.query();
			if (yesResponseCallback)
				yesResponseCallback();
		}

		function sendNoResponse() {
			DataProtectionService.iDontAgree.query();
			if (noOrNotNowResponseCallback)
				noOrNotNowResponseCallback();
		}

		function notNowResponse() {
			// Needed for callback function in back end.
			if (noOrNotNowResponseCallback)
				noOrNotNowResponseCallback();
		}
	}
})();