(function () {

	'use strict';

	angular.module('wfm.teamSchedule').directive('addActivityPanel', addActivityPanel);

	function addActivityPanel() {
		return {
			restrict: 'E',
			scope: {
				selectedAgents: '&',
				selectedDate: '&',
				actionsAfterActivityApply: '&?',
				defaultStart: '&?'
			},
			templateUrl: 'js/teamSchedule/html/addActivityPanel.tpl.html',
			controller: ['ActivityService', addActivityCtrl],
			controllerAs: 'vm',
			bindToController: true
		};
	}

	function addActivityCtrl(ActivityService) {
		var vm = this;
		var startTimeMoment;

		if (vm.defaultStart) {
			startTimeMoment = moment(moment(vm.selectedDate()).format("YYYY-MM-DD") + " " + vm.defaultStart());
		} else {
			startTimeMoment = moment();
		}
		var endTimeMoment = moment(startTimeMoment).add(1, 'hour');
		vm.timeRange = {
			startTime: startTimeMoment.toDate(),
			endTime: endTimeMoment.toDate()
		};
		vm.selectedActivityId = null;
		vm.disableNextDay = false;
		vm.addActivity = addActivity;

		ActivityService.fetchAvailableActivities().then(function (activities) {
			vm.activities = activities;
		});

		function addActivity() {			
			ActivityService.addActivity({
				agents: vm.selectedAgents(),
				date: vm.selectedDate(),
				startTime: vm.timeRange.startTime,
				endTime: vm.timeRange.endTime,
				activity: vm.selectedActivityId

			}).then(function (data) {			
				if (vm.actionsAfterActivityApply) {
					//vm.actionsAfterActivityApply(data);
					vm.actionsAfterActivityApply();
				}
			});
		}	

				
	}

})();