(function () {
	"use strict";

	angular.module('wfm.teamSchedule').directive('addPersonalActivity', addPersonalActivity);

	addPersonalActivityCtrl.$inject = ['$scope', 'ActivityService', 'PersonSelection', 'WFMDate', 'ScheduleManagement', 'teamScheduleNotificationService'];

	function addPersonalActivityCtrl($scope, activityService, personSelectionSvc, wFMDateSvc, scheduleManagementSvc, teamScheduleNotificationService) {
		var vm = this;

		vm.label = 'AddPersonalActivity';

		vm.isNextDay = false;
		vm.disableNextDay = false;
		vm.notAllowedNameListString = "";
		vm.availableActivitiesLoaded = false;
		vm.selectedAgents = personSelectionSvc.getSelectedPersonInfoList();

		activityService.fetchAvailableActivities().then(function (activities) {
			vm.availableActivities = activities;
			vm.availableActivitiesLoaded = true;
		});

		vm.isInputValid = function () {
			if (vm.timeRange == undefined || vm.selectedActivityId == undefined || vm.timeRange.startTime == undefined) return false;
		
			var invalidAgents = vm.selectedAgents.filter(function (agent) { return !vm.isNewActivityAllowedForAgent(agent, vm.timeRange.startTime); });
			vm.notAllowedNameListString = invalidAgents.map(function (x) { return x.name; }).join(', ');

			return invalidAgents.length === 0;
		};

		vm.isNewActivityAllowedForAgent = function(agent, activityStart) {
			var mActivityStart = moment(activityStart);
			var mScheduleEnd = moment(agent.scheduleEndTime);

			return !vm.isNextDay || mActivityStart.isSame(mScheduleEnd, 'day') && (mScheduleEnd.isAfter(mActivityStart));
		}
		
		vm.addPersonalActivity = function () {
			var requestData = {
				PersonIds: vm.selectedAgents.map(function(agent) { return agent.personId; }),
				Date: vm.referenceDay(),
				StartTime: moment(vm.timeRange.startTime).format("YYYY-MM-DDTHH:mm"),
				EndTime: moment(vm.timeRange.endTime).format("YYYY-MM-DDTHH:mm"),
				PersonalActivityId: vm.selectedActivityId,
				TrackedCommandInfo: { TrackId: vm.trackId }
			};

			activityService.addPersonalActivity(requestData).then(function (response) {				
				if (vm.getActionCb(vm.label)) {					
					vm.getActionCb(vm.label)(vm.TrackId, requestData.PersonIds);
				}
				teamScheduleNotificationService.reportActionResult({
					success: 'SuccessfulMessageForAddingActivity',
					warning: 'PartialSuccessMessageForAddingActivity'
				}, vm.selectedAgents.map(function(x) {
					return {
						PersonId: x.personId,
						Name: x.name
					}
				}), response.data);
			});
		};

		vm.getDefaultActvityStartTime = getDefaultActvityStartTime;

		function getDefaultActvityStartTime() {
			var curDateMoment = moment(vm.referenceDay());
			var overnightEnds = scheduleManagementSvc.getLatestPreviousDayOvernightShiftEnd(curDateMoment, vm.selectedAgents);
			var latestShiftStart = scheduleManagementSvc.getLatestStartOfSelectedSchedule(curDateMoment, vm.selectedAgents);

			var defaultStart = curDateMoment.clone().hour(8).minute(0).second(0).toDate();
			if (overnightEnds !== null) {
				defaultStart = moment(overnightEnds).add(1, 'hour').toDate();
			}

			if (moment(wFMDateSvc.nowInUserTimeZone()).format('YYYY-MM-DD') === moment(vm.referenceDay()).format('YYYY-MM-DD')) {
				var nextTickTime = new Date(wFMDateSvc.getNextTickNoEarlierThanEight());
				if (nextTickTime > defaultStart) {
					defaultStart = nextTickTime;
				}
			} else {
				if (latestShiftStart !== null) {
					var latestShiftStartPlusOneHour = moment(latestShiftStart).add(1, 'hour').toDate();
					if (latestShiftStartPlusOneHour >= defaultStart)
						defaultStart = latestShiftStart;
				} 
			}

			return defaultStart;
		}

		vm.getDefaultActvityEndTime = function () {
			return moment(getDefaultActvityStartTime()).add(1, 'hour').toDate();
		}
	}

	function addPersonalActivity() {
		return {
			restrict: 'E',
			scope: {},
			controller: addPersonalActivityCtrl,
			controllerAs: 'vm',
			bindToController: true,
			templateUrl: 'js/teamSchedule/html/addPersonalActivity.html',
			require: ['^teamscheduleCommandContainer', 'addPersonalActivity'],
			link: postlink
		}

		function postlink(scope, elem, attrs, ctrls) {
			var containerCtrl = ctrls[0],
				selfCtrl = ctrls[1];

			scope.vm.referenceDay = containerCtrl.getDate;
			scope.vm.trackId = containerCtrl.getTrackId();
			scope.vm.getActionCb = containerCtrl.getActionCb;

			scope.vm.timeRange = {
				startTime: selfCtrl.getDefaultActvityStartTime(),
				endTime: selfCtrl.getDefaultActvityEndTime()
			};

			scope.$on('teamSchedule.command.focus.default', function () {
				var focusTarget = elem[0].querySelector('.focus-default');
				if (focusTarget) angular.element(focusTarget).focus();
			});
		}
	}
})();