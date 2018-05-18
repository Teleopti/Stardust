﻿(function () {
	'use strict';

	angular.module('wfm.teamSchedule').directive('addPersonalActivity', ['serviceDateFormatHelper', addPersonalActivity]);

	function addPersonalActivity() {
		return {
			restrict: 'E',
			scope: {},
			controller: addPersonalActivityCtrl,
			controllerAs: 'vm',
			bindToController: true,
			templateUrl: 'app/teamSchedule/commands/teams.directive.cmd.addPersonalActivity.html',
			require: ['^teamscheduleCommandContainer', 'addPersonalActivity']
		};
	}

	addPersonalActivityCtrl.$inject = ['$scope', 'ActivityService', 'PersonSelection', 'UtilityService', 'ScheduleHelper', 'teamScheduleNotificationService', 'CommandCheckService', 'belongsToDateDecider', 'serviceDateFormatHelper'];

	function addPersonalActivityCtrl($scope, activityService, personSelectionSvc, utility, scheduleHelper, teamScheduleNotificationService, CommandCheckService, belongsToDateDecider, serviceDateFormatHelper) {
		var vm = this;

		vm.label = 'AddPersonalActivity';
		var containerCtrl = $scope.$parent.vm;

		vm.containerCtrl = containerCtrl;
		vm.selectedDate = containerCtrl.getDate;
		vm.trackId = containerCtrl.getTrackId();
		vm.currentTimezone = containerCtrl.getCurrentTimezone;
		vm.convertTime = containerCtrl.getServiceTimeInCurrentUserTimezone;
		vm.getActionCb = containerCtrl.getActionCb;

		vm.isNextDay = false;
		vm.disableNextDay = false;
		vm.notAllowedNameListString = '';
		vm.availableActivitiesLoaded = false;
		vm.checkingCommand = false;
		vm.selectedAgents = personSelectionSvc.getCheckedPersonInfoList();
		vm.invalidAgents = [];
		vm.getDefaultActvityStartTime = getDefaultActvityStartTime;
		vm.getDefaultActvityEndTime = getDefaultActvityEndTime;

		vm.timeRange = {
			startTime: vm.getDefaultActvityStartTime(),
			endTime: vm.getDefaultActvityEndTime()
		};

		activityService.fetchAvailableActivities().then(function (activities) {
			vm.availableActivities = activities;
			vm.availableActivitiesLoaded = true;
		});

		function decidePersonBelongsToDates(agents, targetTimeRange) {
			return agents.map(function (agent) {
				var scheduleVm = vm.containerCtrl.scheduleManagementSvc.findPersonScheduleVmForPersonId(agent.PersonId);
				var normalizedVm = belongsToDateDecider
					.normalizePersonScheduleVm(scheduleVm,
					vm.currentTimezone());
				var belongsToDate = belongsToDateDecider.decideBelongsToDate(targetTimeRange,
					normalizedVm,
					vm.selectedDate());

				return {
					Date: belongsToDate,
					PersonId: agent.PersonId
				};
			});
		}

		function getTimeRangeMoment() {
			return { startTime: moment(vm.timeRange.startTime), endTime: moment(vm.timeRange.endTime) };
		}

		vm.anyValidAgent = function () {
			return vm.invalidAgents.length != vm.selectedAgents.length;
		};

		vm.updateInvalidAgents = function () {
			vm.invalidAgents = [];
			if (!$scope.newPersonalActivityForm.$valid)
				return;
			var belongsToDates = decidePersonBelongsToDates(vm.selectedAgents, getTimeRangeMoment());

			for (var i = 0; i < belongsToDates.length; i++) {
				if (!belongsToDates[i].Date)
					vm.invalidAgents.push(vm.selectedAgents[i]);
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

		function addPersonalActivity(requestData) {
			var personIds = requestData.PersonDates.map(function (personDate) {
				return personDate.PersonId;
			});

			if (requestData.PersonDates.length > 0) {
				activityService.addPersonalActivity(requestData).then(function (response) {
					if (vm.getActionCb(vm.label)) {
						vm.getActionCb(vm.label)(vm.trackId, personIds);
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
				StartTime: vm.convertTime(getDateTimeInTimeZone(vm.timeRange.startTime)),
				EndTime: vm.convertTime(getDateTimeInTimeZone(vm.timeRange.endTime)),
				ActivityId: vm.selectedActivityId,
				ActivityType: 2,
				TrackedCommandInfo: {
					TrackId: vm.trackId
				}
			};
		}

		function getDateTimeInTimeZone(dateTime) {
			return serviceDateFormatHelper.getDateTime(moment.tz(dateTime, vm.currentTimezone()));
		}

		vm.addPersonalActivity = function () {
			var requestData = getRequestData();
			vm.checkingCommand = true;
			CommandCheckService.checkAddPersonalActivityOverlapping(requestData).then(function (data) {
				addPersonalActivity(data);
			});
		};
		
		function getDefaultActvityStartTime() {
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

		
		function getDefaultActvityEndTime() {
			return serviceDateFormatHelper.getDateTime(moment(vm.getDefaultActvityStartTime()).add(1, 'hour'));
		};
	}
})();
