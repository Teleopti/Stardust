(function () {
	'use strict';
	function emailSettingsController($scope, emailSettingsService) {
		var vm = this;
		vm.emailSettings = {};
		getEmailSettings();
		console.log(vm.emailSettings);

		function getEmailSettings() {
			return emailSettingsService.get().then(function (data) {
				console.log(data);
				vm.emailSettings = data;
				return vm.emailSettings;
			});
		}

		function parseEmailSettings(config) {
			return config;
		}
	}

	emailSettingsController.$inject = ['$scope', 'emailSettingsService'];

	angular.module('emailSettingsModule').component('emailSettings', {
		templateUrl: '/Scripts/App/NotificationSettings/emailSettings.template.html',
		controller: emailSettingsController
	});
})();