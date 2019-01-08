(function () {
	'use strict';
	function EmailSettingsController($scope) {
		var vm = this;
		vm.Host = 'Arne';

		// This would be loaded by $http etc.
		console.log('batman nananannanananan');
	}

	EmailSettingsController.$inject = ['$scope'];

	angular.module('emailSettingsModule').component('emailSettings', {
		templateUrl: '/Scripts/App/NotificationSettings/emailSettings.template.html',
		controller: EmailSettingsController
	});
})();