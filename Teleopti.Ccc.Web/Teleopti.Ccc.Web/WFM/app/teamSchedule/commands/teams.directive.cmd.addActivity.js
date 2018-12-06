(function () {
	'use strict';

	angular.module('wfm.teamSchedule').directive('addActivity', [addActivityDirective]);

	function addActivityDirective() {
		return {
			restrict: 'E',
			scope: {
				command: '<'
			},
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

		var defaultStartTimeMoment = getDefaultStartMoment();
		vm.timeRange = {
			startTime: serviceDateFormatHelper.getDateTime(defaultStartTimeMoment),
			endTime: serviceDateFormatHelper.getDateTime(defaultStartTimeMoment.add(1, 'hour'))
		};

		function onInit() {
			activityService.fetchAvailableActivities().then(function (activities) {
				vm.availableActivities = activities;
			});
			updateInvalidAgents();
		}

		function decidePersonBelongsToDates(agents, targetTimeRange) {
			return agents.map(function (selectedAgent) {
				var personSchedule = vm.containerCtrl.scheduleManagementSvc.findPersonScheduleVmForPersonId(selectedAgent.PersonId);
				var belongsToDate = belongsToDateDecider.decideBelongsToDate(targetTimeRange,
					belongsToDateDecider.normalizePersonScheduleVm(personSchedule, vm.currentTimezone()),
					vm.selectedDate());

				return {
					Date: belongsToDate,
					PersonId: selectedAgent.PersonId
				};
			});
		}

		function getTimeRangeMoment() {
			var currentTimezone = vm.currentTimezone();
			return {
				startTime: moment.tz(vm.timeRange.startTime, currentTimezone),
				endTime: moment.tz(vm.timeRange.endTime, currentTimezone)
			};
		}

		vm.anyValidAgent = function () {
			
			return vm.invalidAgents.length != vm.selectedAgents.length;
		};

		vm.updateInvalidAgents = updateInvalidAgents;

		function updateInvalidAgents() {
			if ($scope.newActivityForm && !$scope.newActivityForm.$valid) return;

			var belongsToDates = decidePersonBelongsToDates(vm.selectedAgents, getTimeRangeMoment());
			vm.invalidAgents = [];

			for (var i = 0; i < belongsToDates.length; i++) {
				if (!belongsToDates[i].Date) vm.invalidAgents.push(vm.selectedAgents[i]);
			}

			vm.notAllowedNameListString = vm.invalidAgents.map(function (x) { return x.Name; }).join(', ');
		};

		var addActivity = function (requestData) {
			var personIds = requestData.PersonDates.map(function (personDate) {
				return personDate.PersonId;
			});
			if (requestData.PersonDates.length > 0) {
				activityService.addActivity(vm.command.activityType, requestData)
					.then(function (response) {
						if (vm.getActionCb(vm.label)) {
							vm.getActionCb(vm.label)(vm.trackId, personIds);
						}
						teamScheduleNotificationService.reportActionResult({
							success: vm.command.successfulMessage,
							warning: vm.command.warningMessage
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
				StartTime: vm.convertTime(getDateTimeInTimeZone(vm.timeRange.startTime)),
				EndTime: vm.convertTime(getDateTimeInTimeZone(vm.timeRange.endTime)),
				ActivityId: vm.selectedActivityId,
				TrackedCommandInfo: {
					TrackId: vm.trackId
				}
			};
		}

		function getDateTimeInTimeZone(dateTime) {
			return serviceDateFormatHelper.getDateTime(moment.tz(dateTime, vm.currentTimezone()));
		}

		vm.addActivity = function () {
			var requestData = getRequestData();
			vm.checkingCommand = true;
			CommandCheckService.checkAddActivityOverlapping(requestData)
				.then(function (data) {
					addActivity(data);
				});
		};

		function getDefaultStartMoment() {
			var curDate = vm.selectedDate();
			var curDateMoment = moment.tz(curDate, vm.currentTimezone());
			var personIds = vm.selectedAgents.map(function (agent) { return agent.PersonId; });
			var schedules = vm.containerCtrl.scheduleManagementSvc.schedules();

			var overnightEndsMoment = scheduleHelper.getLatestPreviousDayOvernightShiftEndMoment(schedules, curDate, personIds);
			var latestShiftStartMoment = scheduleHelper.getLatestStartMomentOfSelectedSchedules(schedules, curDateMoment, personIds);

			// Set to 08:00 for empty schedule or day off
			var defaultStartMoment = curDateMoment.clone().hour(8).minute(0).second(0);

			if (overnightEndsMoment !== null) {
				defaultStartMoment = overnightEndsMoment.add(1, 'hour');
			}
			if (serviceDateFormatHelper.getDateOnly(utility.nowInTimeZone(vm.currentTimezone())) === vm.selectedDate()) {
				var nextTickTimeMoment = utility.getNextTickMomentNoEarlierThanEight(vm.currentTimezone());
				if (nextTickTimeMoment.isAfter(defaultStartMoment)) {
					defaultStartMoment = nextTickTimeMoment;
				}
			} else {
				if (latestShiftStartMoment !== null) {
					var latestShiftStartPlusOneHour = latestShiftStartMoment.add(1, 'hour');
					if (latestShiftStartPlusOneHour.isSameOrAfter(defaultStartMoment))
						defaultStartMoment = latestShiftStartPlusOneHour;
				}
			}
			return defaultStartMoment;
		}

		

		onInit();
	}
})();