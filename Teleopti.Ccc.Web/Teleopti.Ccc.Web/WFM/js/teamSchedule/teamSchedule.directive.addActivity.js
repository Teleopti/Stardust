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
		var startTime = moment(vm.selectedDate()).format("YYYY-MM-DD") + " " + vm.defaultStart();
		var endTime = moment(startTime).add(1, 'hour');
		vm.timeRange = {
			startTime: new Date(startTime),
			endTime: new Date(endTime)
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