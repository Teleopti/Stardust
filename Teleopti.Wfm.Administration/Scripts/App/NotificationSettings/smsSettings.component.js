(function () {
	'use strict';
	function SmsSettingsController($scope, smsSettingsService) {
		var vm = this;
		vm.error = false;

		vm.LoadSmsSettings = function () {
			console.log('sms', smsSettingsService);
			return smsSettingsService.get().then(function (data) {
				if (data.error) {
					vm.error = true;
					vm.message = data.message;
					//Error when fetching settings, should it be the same error as when the form fails?
				} else {
					vm.smsSettings = data;
				}
				
				return vm.smsSettings;
			});
		}

		vm.PostSmsSettings = function(data) {
			return smsSettingsService.post(vm.smsSettings).then(function (data) {
				if (data.error) {
					vm.error = true;
					vm.message = data.message;
				} else {
					vm.success = true;
					vm.error = false;
					vm.message = data.message;
				}
			});
		}

		vm.LoadSmsSettings();
	}

	SmsSettingsController.$inject = ['$scope', 'smsSettingsService'];

	angular.module('smsSettingsModule').component('smsSettings', {
		templateUrl: '/Scripts/App/NotificationSettings/smsSettings.template.html',
		controller: SmsSettingsController
	});
})();