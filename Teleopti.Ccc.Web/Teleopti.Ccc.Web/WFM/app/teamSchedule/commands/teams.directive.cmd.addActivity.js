﻿(function () {
	'use strict';

	angular.module('wfm.teamSchedule').directive('addActivity', ['serviceDateFormatHelper', addActivityDirective]);

	function addActivityDirective(serviceDateFormatHelper) {
		return {
			restrict: 'E',
			scope: {},
			controller: addActivityCtrl,
			controllerAs: 'vm',
			bindToController: true,
			templateUrl: 'app/teamSchedule/commands/teams.directive.cmd.addActivity.html',
			require: ['^teamscheduleCommandContainer', 'addActivity']
		};
	}

	addActivityCtrl.$inject = ['$scope', 'ActivityService', 'PersonSelection', 'UtilityService', 'ScheduleHelper', 'teamScheduleNotificationService', 'CommandCheckService', 'belongsToDateDecider', 'serviceDateFormatHelper'];

	function addActivityCtrl($scope, activityService, personSelectionSvc, utility, scheduleHelper, teamScheduleNotificationService, CommandCheckService, belongsToDateDecider, serviceDateFormatHelper) {
		var vm = this;

		vm.label = 'AddActivity';

		vm.isNextDay = false;
		vm.disableNextDay = false;
		vm.notAllowedNameListString = "";
		vm.availableActivitiesLoaded = false;
		vm.checkingCommand = false;
		vm.selectedAgents = personSelectionSvc.getCheckedPersonInfoList();
		vm.invalidAgents = [];

		var containerCtrl = $scope.$parent.vm;

		vm.containerCtrl = containerCtrl;

		vm.selectedDate = containerCtrl.getDate;
		vm.currentTimezone = containerCtrl.getCurrentTimezone;
		vm.convertTime = containerCtrl.getServiceTimeInCurrentUserTimezone;
		vm.trackId = containerCtrl.getTrackId();
		vm.getActionCb = containerCtrl.getActionCb;

		vm.getDefaultActvityStartTime = getDefaultActvityStartTime;
		vm.getDefaultActvityEndTime = getDefaultActvityEndTime;

		vm.timeRange = {
			startTime: getDefaultActvityStartTime(),
			endTime: getDefaultActvityEndTime()
		};

		vm.updateInvalidAgents = updateInvalidAgents;

		updateInvalidAgents(true);

		activityService.fetchAvailableActivities().then(function (activities) {
			vm.availableActivities = activities;
			vm.availableActivitiesLoaded = true;
		});

		function decidePersonBelongsToDates(agents, targetTimeRange) {
			return agents.map(function (selectedAgent) {
				var belongsToDate = belongsToDateDecider.decideBelongsToDate(targetTimeRange,
					belongsToDateDecider.normalizePersonScheduleVm(vm.containerCtrl.scheduleManagementSvc.findPersonScheduleVmForPersonId(selectedAgent.PersonId), vm.currentTimezone()),
					vm.selectedDate());

				return {
					Date: belongsToDate,
					PersonId: selectedAgent.PersonId
				};
			});
		}

		function getTimeRangeMoment() {
			return { startTime: moment(vm.timeRange.startTime), endTime: moment(vm.timeRange.endTime) };
		}

		vm.anyValidAgent = function () {
			return vm.invalidAgents.length != vm.selectedAgents.length;
		};

		function updateInvalidAgents(isTimeRangeValid) {
			if (!isTimeRangeValid) return;
			var belongsToDates = decidePersonBelongsToDates(vm.selectedAgents, getTimeRangeMoment());
			vm.invalidAgents = [];

			for (var i = 0; i < belongsToDates.length; i++) {
				if (!belongsToDates[i].Date) vm.invalidAgents.push(vm.selectedAgents[i]);
			}

			vm.notAllowedNameListString = vm.invalidAgents.map(function (x) { return x.Name; }).join(', ');
		};

		vm.isNewActivityAllowedForAgent = function (agent, timeRange) {
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

		var addActivity = function (requestData) {
			var personIds = requestData.PersonDates.map(function (personDate) {
				return personDate.PersonId;
			});
			if (requestData.PersonDates.length > 0) {
				activityService.addActivity(requestData)
					.then(function (response) {
						if (vm.getActionCb(vm.label)) {
							vm.getActionCb(vm.label)(vm.trackId, personIds);
						}
						teamScheduleNotificationService.reportActionResult({
							success: 'SuccessfulMessageForAddingActivity',
							warning: 'PartialSuccessMessageForAddingActivity'
						},
							vm.selectedAgents.map(function (x) {
								return {
									PersonId: x.PersonId,
									Name: x.Name
								}
							}),
							response.data);
						vm.checkingCommand = false;
					});
			} else {
				if (vm.getActionCb(vm.label)) {
					vm.getActionCb(vm.label)(vm.trackId, personIds);
				}
				vm.checkingCommand = false;
			}
		};

		function getRequestData() {
			var invalidPersonIds = vm.invalidAgents.map(function (agent) {
				return agent.PersonId;
			});
			var validAgents = vm.selectedAgents.filter(function (agent) {
				return invalidPersonIds.indexOf(agent.PersonId) < 0;
			});

			return {
				PersonDates: decidePersonBelongsToDates(validAgents, getTimeRangeMoment()),
				StartTime: vm.convertTime(serviceDateFormatHelper.getDateTime(vm.timeRange.startTime)),
				EndTime: vm.convertTime(serviceDateFormatHelper.getDateTime(vm.timeRange.endTime)),
				ActivityId: vm.selectedActivityId,
				ActivityType: 1,
				TrackedCommandInfo: {
					TrackId: vm.trackId
				}
			};
		}

		vm.addActivity = function () {
			var requestData = getRequestData();
			vm.checkingCommand = true;
			CommandCheckService.checkAddActivityOverlapping(requestData)
				.then(function (data) {
					addActivity(data);
				});
		};

		function getDefaultActvityStartTime () {
			var curDateMoment = moment(vm.selectedDate());
			var personIds = vm.selectedAgents.map(function (agent) { return agent.PersonId; });
			var schedules = vm.containerCtrl.scheduleManagementSvc.schedules();

			var overnightEnds = scheduleHelper.getLatestPreviousDayOvernightShiftEnd(schedules, curDateMoment, personIds);
			var latestShiftStart = scheduleHelper.getLatestStartOfSelectedSchedules(schedules, curDateMoment, personIds);

			// Set to 08:00 for empty schedule or day off
			var defaultStart = curDateMoment.clone().hour(8).minute(0).second(0).toDate();

			if (overnightEnds !== null) {
				defaultStart = moment(overnightEnds).add(1, 'hour').toDate();
			}
			if (serviceDateFormatHelper.getDateOnly(utility.nowInSelectedTimeZone(vm.currentTimezone())) === vm.selectedDate()) {
				var nextTickTime = new Date(moment(utility.getNextTickNoEarlierThanEight(vm.currentTimezone())).tz(vm.currentTimezone()).format('YYYY-MM-DD HH:mm'));
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

			return serviceDateFormatHelper.getDateTime(defaultStart);
		}

		function getDefaultActvityEndTime () {
			return serviceDateFormatHelper.getDateTime(moment(getDefaultActvityStartTime()).add(1, 'hour'));
		};

	}
})();