(function () {

	'use strict';

	angular.module('wfm.teamSchedule').directive('addActivityPanel', addActivityPanel);

	function addActivityPanel() {
		return {
			restrict: 'E',
			scope: {
				selectedAgents: '&',
				selectedDate: '&',
				defaultStart: '&'
			},
			templateUrl: 'js/teamSchedule/html/addActivityPanel.tpl.html',
			controller: ['ActivityService', addActivityCtrl],
			controllerAs: 'vm',
			bindToController: true
		};
	}
	
	function addActivityCtrl(ActivityService) {
		var vm = this;
		vm.timeRange = {
			startTime: new Date(),
			endTime: new Date()
		};
		vm.disableNextDay = false;
		vm.isDataChangeValid = isDataChangeValid;

		ActivityService.fetchAvailableActivities().then(function (activities) {
			vm.activities = activities;
		});
		
		function isDataChangeValid() {
			return vm.selectedActivityId !== undefined && vm.startTime !== undefined && vm.endTime !== undefined;
		}
	}

})();