(function () {
	'use strict';
	function emailSettingsController($scope, emailSettingsService) {
		var vm = this;
		vm.emailSettings = {
			"host": "",
			"port": "",
			"ssl": "",
			"relay": "",
			"user": "",
			"password": ""
		};
		getEmailSettings();

		function getEmailSettings() {
			return emailSettingsService.get().then(function (data) {
				var config = data.Config;
				vm.emailSettings.host = config.SmtpHost;
				vm.emailSettings.port = config.SmtpPort;
				vm.emailSettings.ssl = config.SmtpUseSsl;
				vm.emailSettings.relay = config.SmtpUseRelay;
				vm.emailSettings.user = config.SmtpUser;
				vm.emailSettings.password = config.SmtpPassword;

				return vm.emailSettings;
			});
		}
	}

	emailSettingsController.$inject = ['$scope', 'emailSettingsService'];

	angular.module('emailSettingsModule').component('emailSettings', {
		templateUrl: '/Scripts/App/NotificationSettings/emailSettings.template.html',
		controller: emailSettingsController
	});
})();