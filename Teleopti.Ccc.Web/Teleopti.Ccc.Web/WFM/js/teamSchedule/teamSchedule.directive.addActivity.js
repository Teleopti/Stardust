(function () {

	'use strict';

	angular.module('wfm.teamSchedule').directive('addActivityPanel', addActivityPanel);

	function addActivityPanel() {
		return {
			restrict: 'E',
			scope: {
				selectedDate: '&',
				actionsAfterActivityApply: '&?',
				defaultStart: '&?',
				tabindex: '@?'
			},
			templateUrl: 'js/teamSchedule/html/addActivityPanel.tpl.html',
			controller: ['$element','$translate', 'ActivityService', 'guidgenerator', 'CommandCommon', 'PersonSelection', 'teamScheduleNotificationService', addActivityCtrl],
			controllerAs: 'vm',
			bindToController: true
		};
	}

	function addActivityCtrl($element, $translate, activityService, guidgenerator, commandCommon, personSelectionSvc, NotificationSvc) {
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
		vm.notAllowedToAddActivityWithoutShift = function () {
			if (notAllowed == "")
				return "";
			var agentName = notAllowed.substr(0, notAllowed.length - 2);
			return {
				Message: $translate.instant('CanNotAddActivityToAgentWithoutShift').replace('{0}', agentName),
				AgentName: agentName
		}
		};

		vm.isInputValid = function () {						
			if (vm.timeRange == undefined || vm.selectedActivityId == undefined || vm.timeRange.startTime == undefined) return true;				
			var isValid = true;
			notAllowed = "";
			var formattedStartTime = moment(vm.timeRange.startTime).format('YYYY-MM-DD HH:mm');
			angular.forEach(vm.selectedAgents, function (selectedAgent) {
				if (selectedAgent.scheduleEndTime == undefined) return;			
				var isAllowed = isNewActivityAllowed(formattedStartTime, selectedAgent.scheduleEndTime);
				if (!isAllowed) {				
					isValid = false;
					if (notAllowed.indexOf(selectedAgent.name) == -1) {
						notAllowed += selectedAgent.name + ', ';
					}
				}
			});

			return isValid;
		};

		function isNewActivityAllowed(activityStart, scheduleEnd) {			
			var mActivityStart = moment(activityStart);
			var mScheduleEnd = moment(scheduleEnd);
			return !vm.isNextDay ||  mActivityStart.isSame(mScheduleEnd, 'day') && (mScheduleEnd.isAfter(mActivityStart));
		}

		function addActivity() {
			var trackId = guidgenerator.newGuid();
			activityService.addActivity({
				PersonIds: vm.selectedAgents.map(function(agent) { return agent.personId; }),
				Date: vm.selectedDate(),
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
			$element.removeAttr('tabindex');
		}
	}
})();