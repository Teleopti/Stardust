(function () {
	'use strict';

	angular.module('wfm.teamSchedule').directive('addActivity', addActivityDirective);

	addActivityCtrl.$inject = ['ActivityService', 'PersonSelection', 'WFMDate', 'ScheduleManagement', 'teamScheduleNotificationService', 'CommandCheckService'];

	function addActivityCtrl(activityService, personSelectionSvc, wFMDateSvc, scheduleManagementSvc, teamScheduleNotificationService, CommandCheckService) {
		var vm = this;

		vm.label = 'AddActivity';

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

			var invalidAgents = vm.selectedAgents.filter(function (agent) { return !vm.isNewActivityAllowedForAgent(agent, vm.timeRange); });
			vm.notAllowedNameListString = invalidAgents.map(function (x) { return x.Name; }).join(', ');

			return invalidAgents.length === 0;
		};

		vm.isNewActivityAllowedForAgent = function(agent, timeRange) {
			var mNewActivityStart = moment(timeRange.startTime);
			var mNewActivityEnd = moment(timeRange.endTime);
			var mScheduleStart = moment(agent.ScheduleStartTime);
			var mScheduleEnd = moment(agent.ScheduleEndTime);
			var allowShiftTotalMinutes = 36 * 60;
			var totalMinutes = (mNewActivityEnd.days() - mScheduleStart.days()) * 24 * 60 + (mNewActivityEnd.hours() - mScheduleStart.hours()) * 60 + (mNewActivityEnd.minutes() - mScheduleStart.minutes());

			var withinAllowShiftPeriod = totalMinutes <= allowShiftTotalMinutes;

			if (mNewActivityStart.isSame(moment(vm.selectedDate()), 'day')) {
				return withinAllowShiftPeriod;
			} else {
				return withinAllowShiftPeriod && (mScheduleEnd.isAfter(mNewActivityStart));
			}
		};

		var addActivity = function () {
			var requestData = getRequestData();
			activityService.addActivity(requestData).then(function (response) {
				if (vm.getActionCb(vm.label)) {
					vm.getActionCb(vm.label)(vm.trackId, requestData.PersonIds);
				}
				teamScheduleNotificationService.reportActionResult({
					success: 'SuccessfulMessageForAddingActivity',
					warning: 'PartialSuccessMessageForAddingActivity'
				}, vm.selectedAgents.map(function (x) {
					return {
						PersonId: x.PersonId,
						Name: x.Name
					}
				}), response.data);
			});
		};

		function getRequestData() {
			vm.selectedAgents = personSelectionSvc.getSelectedPersonInfoList();
			return {
				PersonIds: vm.selectedAgents.map(function (agent) {
					return agent.PersonId;
				}),
				Date: vm.selectedDate(),
				StartTime: moment(vm.timeRange.startTime).format("YYYY-MM-DDTHH:mm"),
				EndTime: moment(vm.timeRange.endTime).format("YYYY-MM-DDTHH:mm"),
				ActivityId: vm.selectedActivityId,
				TrackedCommandInfo: {
					TrackId: vm.trackId
				}
			};
		}

		vm.addActivity = vm.checkCommandActivityLayerOrders? function () {
         	CommandCheckService.checkOverlappingCertainActivities(getRequestData()).then(addActivity);
        } : addActivity ;

		vm.getDefaultActvityStartTime =  function () {
			var curDateMoment = moment(vm.selectedDate());
			var personIds = vm.selectedAgents.map(function (agent) { return agent.PersonId; });
			var overnightEnds = scheduleManagementSvc.getLatestPreviousDayOvernightShiftEnd(curDateMoment, personIds);
			var latestShiftStart = scheduleManagementSvc.getLatestStartOfSelectedSchedule(curDateMoment, personIds);

			var defaultStart = curDateMoment.clone().hour(8).minute(0).second(0).toDate();
			if (overnightEnds !== null) {
				defaultStart = moment(overnightEnds).add(1, 'hour').toDate();
			}

			if (moment(wFMDateSvc.nowInUserTimeZone()).format('YYYY-MM-DD') === moment(vm.selectedDate()).format('YYYY-MM-DD')) {
				var nextTickTime = new Date(wFMDateSvc.getNextTickNoEarlierThanEight());
				if (nextTickTime > defaultStart) {
					defaultStart = nextTickTime;
				}
			} else {
				if (latestShiftStart !== null) {
					var latestShiftStartPlusOneHour = moment(latestShiftStart).add(1, 'hour').toDate();
					if (latestShiftStartPlusOneHour >= defaultStart)
						defaultStart = latestShiftStartPlusOneHour;
				}
			}

			return defaultStart;
		}

		vm.getDefaultActvityEndTime = function () {
			return moment(vm.getDefaultActvityStartTime()).add(1, 'hour').toDate();
		};
	}

	function addActivityDirective() {
		return {
			restrict: 'E',
			scope: {},
			controller: addActivityCtrl,
			controllerAs: 'vm',
			bindToController: true,
			templateUrl: 'js/teamSchedule/html/addActivity.tpl.html',
			require: ['^teamscheduleCommandContainer', 'addActivity'],
			compile: function (tElement, tAttrs) {
				var tabindex = angular.isDefined(tAttrs.tabindex) ? tAttrs.tabindex : '0';
				function addTabindexTo() {
					angular.forEach(arguments, function (arg) {
						angular.forEach(arg, function (elem) {
							elem.setAttribute('tabIndex', tabindex);
						});
					});
				}
				addTabindexTo(
					tElement[0].querySelectorAll('md-select.activity-selector'),
					tElement[0].querySelectorAll('activity-time-range-picker'),
					tElement[0].querySelectorAll('button#applyActivity')
				);
				return postlink;
			},
		};

		function postlink(scope, elem, attrs, ctrls) {
			var containerCtrl = ctrls[0],
				selfCtrl = ctrls[1];

			scope.vm.selectedDate = containerCtrl.getDate;
			scope.vm.trackId = containerCtrl.getTrackId();
			scope.vm.getActionCb = containerCtrl.getActionCb;
			scope.vm.checkCommandActivityLayerOrders = containerCtrl.hasToggle('WfmTeamSchedule_ShowWarningForOverlappingCertainActivities_39938');

			scope.vm.timeRange = {
				startTime: selfCtrl.getDefaultActvityStartTime(),
				endTime: selfCtrl.getDefaultActvityEndTime()
			};

			scope.$on('teamSchedule.command.focus.default', function () {
				var focusTarget = elem[0].querySelector('.focus-default');
				if (focusTarget) angular.element(focusTarget).focus();
			});

			elem.removeAttr('tabindex');
		}
	}
})();