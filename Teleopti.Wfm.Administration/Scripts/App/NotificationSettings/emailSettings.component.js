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
		getEmailSettings();

		function getEmailSettings() {
			return emailSettingsService.get().then(function (data) {
				if (data.error) {
					vm.error = true;
					//Error when fetching settings, should it be the same error as when the form fails?
				} else {
					vm.emailSettings = data;
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