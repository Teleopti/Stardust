(function () {
	'use strict';
	function SmsSettingsController($scope, smsSettingsService, tenantService, $routeParams) {
		var vm = this;

		vm.tenant = $routeParams.tenant;
		vm.error = false;
		vm.success = false;
		vm.tenantId = -1;
		loadTenant();

		function loadTenant() {
			tenantService.loadTenant(vm.tenant).then(function (data) {
				vm.tenantId = data.Id;
				getSmsSettings();
			});
		}

		function getSmsSettings() {
			return smsSettingsService.get(vm.tenantId).then(function (response) {
				if (!response.Success) {
					vm.error = true;
					vm.message = response.Message;
					//Error when fetching settings, should it be the same error as when the form fails?
				} else {
					vm.smsSettings = response.Data;
				}
				
				return vm.smsSettings;
			});
		}

		vm.PostSmsSettings = function() {
			return smsSettingsService.post(vm.tenantId, vm.smsSettings).then(function (response) {
				if (!response.data.Success) {
					vm.error = true;
					vm.message = response.data.Message;
				} else {
					vm.success = true;
					vm.error = false;
					vm.message = response.data.Message;
				}
			});
		}
	}

	SmsSettingsController.$inject = ['$scope', 'smsSettingsService', 'tenantService', '$routeParams'];

	angular.module('smsSettingsModule').component('smsSettings', {
		templateUrl: '/Scripts/App/NotificationSettings/smsSettings.template.html',
		controller: SmsSettingsController
	});
})();