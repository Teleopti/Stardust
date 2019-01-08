(function () {
	'use strict';
	function SmsSettingsController($scope, smsSettingsService) {
		var vm = this;

		vm.LoadSmsSettings = function () {
			console.log('sms', smsSettingsService);
			return smsSettingsService.get().then(function (data) {
				console.log(data);
				vm.smsSettings = data;
				return vm.smsSettings;
			});
		}

		vm.PostSmsSettings = function(data) {
			console.log(data);
		}

		vm.LoadSmsSettings();
	}

	SmsSettingsController.$inject = ['$scope', 'smsSettingsService'];

	angular.module('smsSettingsModule').component('smsSettings', {
		templateUrl: '/Scripts/App/NotificationSettings/smsSettings.template.html',
		controller: SmsSettingsController
	});
})();