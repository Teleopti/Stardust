(function () {

	'use strict';

	angular.module('wfm.teamSchedule').directive('addActivityPanel', addActivityPanel);

	function addActivityPanel() {
		return {
			restrict: 'E',
			scope: {
				selectedDate: '&',
				actionsAfterActivityApply: '&?',
				defaultStart: '&?'
			},
			templateUrl: 'js/teamSchedule/html/addActivityPanel.tpl.html',
			controller: ['ActivityService', 'guidgenerator', 'CommandCommon', 'PersonSelection', 'teamScheduleNotificationService', addActivityCtrl],
			controllerAs: 'vm',
			bindToController: true
		};
	}

	function addActivityCtrl(activityService, guidgenerator, commandCommon, personSelectionSvc, NotificationSvc) {
		var vm = this;
		var startTimeMoment;

		init();

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
		vm.isNextDay = false;
		vm.selectedActivityId = null;
		vm.disableNextDay = false;
		vm.addActivity = commandCommon.wrapPersonWriteProtectionCheck(true, 'AddActivity', addActivity, null, vm.selectedDate());

		activityService.fetchAvailableActivities().then(function (activities) {
			vm.activities = activities;
		});
		var notAllowed = "";
		vm.peopleNotAllowed = function () {
			if (notAllowed == "")
				return "";
			return notAllowed.substr(0, notAllowed.length - 2);
		};

		vm.isInputValid = function () {
			var ret = true;
			if (vm.timeRange == undefined || vm.selectedActivityId == undefined)
				return ret;
			angular.forEach(vm.selectedAgents, function(selectedAgent) {
				var isAllowed = isNewActivityAllowed(vm.timeRange.startTime, selectedAgent.scheduleEndTime);
				if (!isAllowed) {
					ret = false;
					if (notAllowed.indexOf(selectedAgent.name) == -1) {
						notAllowed += selectedAgent.name + ', ';
					}

				}
			});

			return ret;
		};

		function isNewActivityAllowed(activityStart, scheduleEnd) {
			if (activityStart == undefined || scheduleEnd == undefined) {
				return true;
			}
			var mActivityStart = moment(activityStart);
			var mScheduleEnd = moment(scheduleEnd);
			return !vm.isNextDay || (vm.isNextDay && mActivityStart.isSame(mScheduleEnd, 'day') && (mScheduleEnd.isAfter(mActivityStart)));
		}

		function addActivity() {
			var trackId = guidgenerator.newGuid();
			activityService.addActivity({
				PersonIds: vm.selectedAgents.map(function(agent) { return agent.personId; }),
				BelongsToDate: vm.selectedDate(),
				StartTime: moment(vm.timeRange.startTime).format("YYYY-MM-DD HH:mm"),
				EndTime: moment(vm.timeRange.endTime).format("YYYY-MM-DD HH:mm"),
				ActivityId: vm.selectedActivityId,
				TrackedCommandInfo:{TrackId:trackId}
			}).then(function (response) {
				if (vm.actionsAfterActivityApply) {
					vm.actionsAfterActivityApply({
						trackId: trackId,
						personIds: vm.selectedAgents.map(function (agent) { return agent.personId; }),
					});
				}
				var total = personSelectionSvc.getTotalSelectedPersonAndProjectionCount().checkedPersonCount;
				var fail = response.data.length;
				if (fail === 0) {
					NotificationSvc.notify('success', 'SuccessfulMessageForAddingActivity');
				} else {
					var description = NotificationSvc.notify('warning', 'PartialSuccessMessageForAddingActivity', [total, total - fail, fail]);
					CommandCommon.showCommandFailureDetailsDialog(description, response.data);
				}
			}, function (response) {
				if (vm.actionsAfterActivityApply) {
					vm.actionsAfterActivityApply({
						trackId: trackId,
						personIds: vm.selectedAgents.map(function (agent) { return agent.personId; }),
					});
				}
				NotificationSvc.notify('error', 'FailedMessageForAddingActivity');
			});
		}

		function init() {
			vm.selectedAgents = personSelectionSvc.getCheckedPersonInfoList();
		}
	}
})();