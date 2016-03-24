(function () {

	'use strict';

	angular.module('wfm.teamSchedule').directive('addActivityPanel', addActivityPanel);

	function addActivityPanel() {
		return {
			restrict: 'E',
			scope: {
				selectedAgents: '&',
				selectedDate: '&'
			},
			templateUrl: 'js/teamSchedule/html/addActivityPanel.tpl.html',
			controller: ['ActivityService', addActivityCtrl],
			controllerAs: 'vm'
		};
	}

	function addActivityCtrl(ActivityService) {
		var vm = this;
		vm.disableNextDay = false;
		vm.isDataChangeValid = isDataChangeValid;

		ActivityService.fetchAvailableActivities().then(function (activities) {
			vm.activities = activities;
		});

		function isDataChangeValid() {
			return vm.selectedAbsenceId !== undefined;
		}
	}

})();