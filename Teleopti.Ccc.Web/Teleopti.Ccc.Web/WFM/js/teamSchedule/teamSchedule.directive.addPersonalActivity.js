(function () {
	"use strict";

	angular.module('wfm.teamSchedule').directive('addPersonalActivity', addPersonalActivity);

	addPersonalActivityCtrl.$inject = ['$scope', 'guidgenerator', 'ActivityService', 'PersonSelection'];

	function addPersonalActivityCtrl($scope, guidgenerator, activityService, personSelectionSvc) {
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
			var trackId = guidgenerator.newGuid();
			
			//activityService.addPersonActivity({
			//	PersonIds: vm.selectedAgents.map(function (agent) { return agent.personId; }),
			//	Date: $scope.getDate(),
			//	StartTime: moment(vm.timeRange.startTime).format("YYYY-MM-DDTHH:mm"),
			//	EndTime: moment(vm.timeRange.endTime).format("YYYY-MM-DDTHH:mm"),
			//	ActivityId: vm.selectedActivityId,
			//	TrackedCommandInfo: { TrackId: trackId }
			//});
		};
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

			scope.getDate = containerCtrl.getDate;
		}
	}
})();