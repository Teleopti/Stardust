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
			controller: ['ActivityService', 'guidgenerator', 'CommandCommon', 'PersonSelection', addActivityCtrl],
			controllerAs: 'vm',
			bindToController: true
		};
	}

	function addActivityCtrl(ActivityService, guidgenerator, CommandCommon, personSelectionSvc) {
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
		vm.addActivity = CommandCommon.wrapPersonWriteProtectionCheck(true, 'AddActivity', addActivity);

		ActivityService.fetchAvailableActivities().then(function (activities) {
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
			angular.forEach(vm.selectedAgents, function(schedule) {
				var isAllowed = isNewActivityAllowed(vm.timeRange.startTime, schedule.scheduleEndTime);
				if (isAllowed != undefined && !isAllowed) {
					ret = false;
					if (notAllowed.indexOf(schedule.name) == -1) {
						notAllowed += schedule.name + ', ';
					}
						
				}
			});
			
			return ret;
		}

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
			ActivityService.addActivity({
				PersonIds: vm.selectedAgents.map(function(agent) { return agent.personId; }),
				BelongsToDate: vm.selectedDate(),
				StartTime: moment(vm.timeRange.startTime).format("YYYY-MM-DD HH:mm"),
				EndTime: moment(vm.timeRange.endTime).format("YYYY-MM-DD HH:mm"),
				ActivityId: vm.selectedActivityId,
				TrackedCommandInfo:{TrackId:trackId}
			}).then(function (data) {
				if (vm.actionsAfterActivityApply) {
					vm.actionsAfterActivityApply({
						result: { TrackId: trackId},
						personIds: vm.selectedAgents.map(function (agent) { return agent.personId; }),
						successMessageTemplate: 'SuccessfulMessageForAddingActivity',
						failMessageTemplate: ''
					});
				}
			}, function (error) {
				if (vm.actionsAfterActivityApply) {
					vm.actionsAfterActivityApply({
						result: { TrackId: trackId, Errors: error },
						personIds: vm.selectedAgents.map(function (agent) { return agent.personId; }),
						successMessageTemplate: '',
						failMessageTemplate: 'FailedMessageForAddingActivity'
					});
				}
			});
		}
		
		function init() {
			vm.selectedAgents = personSelectionSvc.getCheckedPersonInfoList();
		}
	}
})();