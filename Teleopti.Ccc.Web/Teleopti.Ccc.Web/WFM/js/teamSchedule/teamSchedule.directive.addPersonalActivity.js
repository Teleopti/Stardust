(function () {
	"use strict";

	angular.module('wfm.teamSchedule').directive('addPersonalActivity', addPersonalActivity);

	addPersonalActivityCtrl.$inject = ['$scope', 'guidgenerator', 'ActivityService', 'PersonSelection', 'WFMDate', 'ScheduleManagement'];

	function addPersonalActivityCtrl($scope, guidgenerator, activityService, personSelectionSvc, wFMDateSvc, scheduleManagementSvc) {
		var vm = this;

		vm.isNextDay = false;
		vm.disableNextDay = false;
		vm.notAllowedNameListString = "";
		vm.selectedAgents = personSelectionSvc.getSelectedPersonInfoList();
		
		vm.timeRange = {
			startTime: new Date(),
			endTime: new Date()
		};

		activityService.fetchAvailableActivities().then(function (activities) {
			vm.availableActivities = activities;
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

		vm.addPersonActivity = function () {
			activityService.addPersonActivity({
				PersonIds: vm.selectedAgents.map(function(agent) { return agent.personId; }),
				Date: vm.referenceDay(),
				StartTime: moment(vm.timeRange.startTime).format("YYYY-MM-DDTHH:mm"),
				EndTime: moment(vm.timeRange.endTime).format("YYYY-MM-DDTHH:mm"),
				ActivityId: vm.selectedActivityId,
				TrackedCommandInfo: { TrackId: vm.trackId }
			});
		};

		function getDefaultActvityStartTime() {
			var curDateMoment = moment(vm.referenceDay());
			var overnightEnds = scheduleManagementSvc.getLatestPreviousDayOvernightShiftEnd(curDateMoment, vm.selectedAgents);
			var latestShiftStart = scheduleManagementSvc.getLatestStartOfSelectedSchedule(curDateMoment, vm.selectedAgents);

			var defaultStart = null;
			if (overnightEnds !== null) {
				defaultStart = overnightEnds;
			}

			if (wFMDateSvc.nowInUserTimeZone().format('YYYY-MM-DD') === moment(vm.referenceDay).format('YYYY-MM-DD')) {
				wFMDateSvc.getNextTick();
			}

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
		}
	}
})();