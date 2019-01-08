(function () {
	'use strict';
	function SmsSettingsController($scope) {
		var vm = this;

		// This would be loaded by $http etc.
		console.log('batman sms nananannanananan');
	}

	SmsSettingsController.$inject = ['$scope'];

	angular.module('smsSettingsModule').component('smsSettings', {
		templateUrl: '/Scripts/App/NotificationSettings/smsSettings.template.html',
		controller: SmsSettingsController
	});
})();