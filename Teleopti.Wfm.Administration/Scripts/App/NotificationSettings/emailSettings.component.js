(function() {
	'use strict';
	function emailSettingsController($scope, emailSettingsService, tenantService, $routeParams) {
		var vm = this;
		vm.emailSettings = {
			host: '',
			port: 0,
			ssl: false,
			relay: false,
			user: '',
			password: ''
		};
		vm.tenant = $routeParams.tenant;
		vm.error = false;
		vm.success = false;
		vm.tenantId = -1;
		loadTenant();

		function loadTenant() {
			tenantService.loadTenant(vm.tenant).then(function(data) {
				vm.tenantId = data.Id;
				getEmailSettings();
			});
		}

		function getEmailSettings() {
			return emailSettingsService.get(vm.tenantId).then(function(response) {
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
			return emailSettingsService.post(vm.tenantId, vm.emailSettings).then(function(response) {
				if (!response.data.Success) {
					vm.error = true;
					vm.message = response.data.Message;
				} else {
					vm.success = true;
					vm.error = false;
					vm.message = response.data.Message;
				}
			});
		};
	}

	emailSettingsController.$inject = ['$scope', 'emailSettingsService', 'tenantService', '$routeParams'];

	angular.module('emailSettingsModule').component('emailSettings', {
		templateUrl: '/Scripts/App/NotificationSettings/emailSettings.template.html',
		controller: emailSettingsController
	});
})();
