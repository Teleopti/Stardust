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
			controller: ['ActivityService', 'guidgenerator', addActivityCtrl],
			controllerAs: 'vm',
			bindToController: true
		};
	}

	function addActivityCtrl(ActivityService, guidgenerator) {
		var vm = this;
		vm.selectedActivityId = null;
		vm.disableNextDay = false;
		vm.isNextDay = false;
		vm.addActivity = addActivity;

		var startTimeMoment = vm.defaultStart !== undefined ? moment(moment(vm.selectedDate()).format("YYYY-MM-DD") + " " + vm.defaultStart()) : moment();
		var endTimeMoment = moment(startTimeMoment).add(1, 'hour');

		vm.timeRange = {
			startTime: startTimeMoment.toDate(),
			endTime: endTimeMoment.toDate()
		};

		ActivityService.fetchAvailableActivities().then(function (activities) {
			vm.activities = activities;
		});

		function addActivity() {
			var trackId = guidgenerator.newGuid();
			ActivityService.addActivity({
				PersonIds: vm.selectedAgents(),
				BelongsToDate: vm.selectedDate(),
				StartTime: moment(vm.timeRange.startTime).format("YYYY-MM-DD HH:mm"),
				EndTime: moment(vm.timeRange.endTime).format("YYYY-MM-DD HH:mm"),
				ActivityId: vm.selectedActivityId,
				TrackedCommandInfo:{TrackId:trackId}
			}).then(function (data) {
				if (vm.actionsAfterActivityApply) {
					vm.actionsAfterActivityApply({
						result: { TrackId: trackId},
						personIds: vm.selectedAgents(),
						successMessageTemplate: 'SuccessfulMessageForAddingActivity',
						failMessageTemplate: ''
					});
				}
			}, function (error) {
				if (vm.actionsAfterActivityApply) {
					vm.actionsAfterActivityApply({
						result: { TrackId: trackId, Errors: error },
						personIds: vm.selectedAgents(),
						successMessageTemplate: '',
						failMessageTemplate: 'FailedMessageForAddingActivity'
					});
				}
			});
		}
	}
})();