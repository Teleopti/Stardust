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
		vm.dataProtectionQuestionText = '...';
		DataProtectionService.dataProtectionQuestionText.query().$promise.then(function (result) {
			vm.dataProtectionQuestionText = result.text;
		});

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