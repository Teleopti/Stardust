(function () {
	'use strict';
	function emailSettingsController($scope, emailSettingsService) {
		var vm = this;
		vm.emailSettings = {
			"host": "",
			"port": 0,
			"ssl": false,
			"relay": false,
			"user": "",
			"password": ""
		};
		vm.error = false;
		vm.success = false;
		getEmailSettings();

		function getEmailSettings() {
			return emailSettingsService.get(123).then(function (response) {
				if (!response.Success) {
					vm.error = true;
					vm.message = response.Message;
					//Error when fetching settings, should it be the same error as when the form fails?
				} else {
					vm.emailSettings = response.Data;
				}
			});
		}

		vm.postData = function() {
			return emailSettingsService.post(vm.emailSettings).then(function (data) {
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
	}

	emailSettingsController.$inject = ['$scope', 'emailSettingsService'];

	angular.module('emailSettingsModule').component('emailSettings', {
		templateUrl: '/Scripts/App/NotificationSettings/emailSettings.template.html',
		controller: emailSettingsController
	});
})();