
(function () {
	'use strict';

	angular.module('wfm.teamSchedule').directive('addAbsence', ['serviceDateFormatHelper', addAbsenceDirective]);

	function addAbsenceDirective() {
		return {
			restrict: 'E',
			scope: {},
			controller: addAbsenceCtrl,
			controllerAs: 'vm',
			bindToController: true,
			templateUrl: 'app/teamSchedule/commands/teams.directive.cmd.addAbsence.html',
			require: ['^teamscheduleCommandContainer', 'addAbsence'],
			link: function (scope, elem, attrs, ctrls) {
				elem.find('team-schedule-datepicker').on('focus', function (e) {
					e.target.querySelector('input').focus();
				});

				elem.find('uib-timepicker').on('focus', function (e) {
					e.target.querySelector('input').focus();
				});
			}
		};
	}

	addAbsenceCtrl.$inject = ['PersonAbsence',
		'PersonSelection',
		'ScheduleHelper',
		'teamScheduleNotificationService',
		'$locale',
		'$scope',
		'$translate',
		'CommandCheckService',
		'belongsToDateDecider',
		'Toggle',
		'serviceDateFormatHelper'];

	function addAbsenceCtrl(PersonAbsenceSvc,
		personSelectionSvc,
		scheduleHelper,
		teamScheduleNotificationService,
		$locale,
		$scope,
		$translate,
		CommandCheckService,
		belongsToDateDecider,
		Toggle,
		serviceDateFormatHelper) {
		var vm = this;

		var containerCtrl = $scope.$parent.vm;
		vm.containerCtrl = containerCtrl;

		vm.label = 'AddAbsence';
		vm.runningCommand = false;
		vm.selectedAgents = personSelectionSvc.getCheckedPersonInfoList();
		vm.invalidAgents = [];

		vm.selectedDate = containerCtrl.getDate;
		vm.trackId = containerCtrl.getTrackId();
		vm.convertTime = containerCtrl.getServiceTimeInCurrentUserTimezone;
		vm.getActionCb = containerCtrl.getActionCb;
		vm.getCurrentTimezone = containerCtrl.getCurrentTimezone;
		vm.isAddFullDayAbsenceAvailable = function () {
			return containerCtrl.hasPermission('IsAddFullDayAbsenceAvailable');
		};

		vm.isAddIntradayAbsenceAvailable = function () {
			return containerCtrl.hasPermission('IsAddIntradayAbsenceAvailable');
		};

		vm.timeRange = {
			startTime: getDefaultAbsenceStartTime(),
			endTime: getDefaultAbsenceEndTime()
		};

		vm.isFullDayAbsence = vm.isAddFullDayAbsenceAvailable();

		updateDateAndTimeFormat();
		$scope.$on('$localeChangeSuccess', updateDateAndTimeFormat);

		

		PersonAbsenceSvc.loadAbsences().then(function (absences) {
			vm.availableAbsenceTypes = absences;
			vm.availableAbsenceTypesLoaded = true;
		});

		vm.isTimeRangeValid = function () {
			if (!vm.isFullDayAbsence)
				return vm.isAbsenceTimeValid();
			return vm.isAbsenceDateValid();
		};

		vm.isAbsenceTimeValid = function () {
			var currentTimezone = vm.getCurrentTimezone();
			var timeRangeInCurrentTimezone = getTimeRangeInCurrentTimezone();
			var isStartTimeValid = serviceDateFormatHelper.getDateTime(timeRangeInCurrentTimezone.startTime) === vm.timeRange.startTime;
			var isEndTimeValid = serviceDateFormatHelper.getDateTime(timeRangeInCurrentTimezone.endTime) === vm.timeRange.endTime;

			return isStartTimeValid
				&& isEndTimeValid
				&& vm.isEndTimeGreaterThanStartTime();
		};

		vm.isEndTimeGreaterThanStartTime = function () {
			var timeRangeInCurrentTimezone = getTimeRangeInCurrentTimezone();
			return timeRangeInCurrentTimezone.endTime.isAfter(timeRangeInCurrentTimezone.startTime);
		}

		vm.isAbsenceDateValid = function () {
			return moment(vm.timeRange.endTime).isSameOrAfter(moment(vm.timeRange.startTime), 'day');
		};

		function updateInvalidAgents() {
			if (vm.isFullDayAbsence) {
				determineIsSameTimezoneForFullDayAbsence();
			} 
		};

		function getTimeRangeInCurrentTimezone() {
			var currentTimezone = vm.getCurrentTimezone();
			return {
				startTime: moment.tz(vm.timeRange.startTime, currentTimezone),
				endTime: moment.tz(vm.timeRange.endTime, currentTimezone)
			};
		}

		function getDefaultAbsenceStartTime() {
			var curDateMoment = moment(vm.selectedDate());
			var personIds = vm.selectedAgents.map(function (agent) { return agent.PersonId; });
			var schedules = vm.containerCtrl.scheduleManagementSvc.schedules();
			return serviceDateFormatHelper.getDateTime(scheduleHelper.getEarliestStartOfSelectedSchedules(schedules, curDateMoment, personIds));
		};

		function getDefaultAbsenceEndTime() {
			return serviceDateFormatHelper.getDateTime(moment(getDefaultAbsenceStartTime()).add(1, 'hour'));
		};

		function determineIsSameTimezoneForFullDayAbsence() {
			vm.invalidAgents = [];
			var invalidAgentNameList = [];
			vm.selectedAgents.forEach(function (agent) {
				if (vm.getCurrentTimezone() !== vm.containerCtrl.scheduleManagementSvc.findPersonScheduleVmForPersonId(agent.PersonId).Timezone.IanaId) {
					vm.invalidAgents.push(agent);
					invalidAgentNameList.push(agent.Name);
				}
			});
			vm.invalidAgentNameListString = invalidAgentNameList.join(', ');
		}
		
		vm.anyValidAgent = function () {
			updateInvalidAgents();
			return vm.invalidAgents.length !== vm.selectedAgents.length;
		}

		function addAbsence(requestData) {
			if (requestData.PersonIds.length === 0) {
				if (vm.getActionCb(vm.label)) {
					vm.getActionCb(vm.label)(vm.trackId, requestData.PersonIds);
				}
				return null;
			}
			return PersonAbsenceSvc.addAbsence(requestData, vm.isFullDayAbsence).then(function (response) {
				if (vm.getActionCb(vm.label)) {
					vm.getActionCb(vm.label)(vm.trackId, requestData.PersonIds);
				}
				teamScheduleNotificationService.reportActionResult({
					success: $translate.instant('AddAbsenceSuccessedResult'),
					warning: $translate.instant('AddAbsenceTotalResult')
				}, vm.selectedAgents.map(function (x) {
					return {
						PersonId: x.PersonId,
						Name: x.Name
					}
				}), response.data);
			});
		}

		vm.addAbsence = function () {
			var invalidPersonIds = vm.invalidAgents.map(function (agent) {
				return agent.PersonId;
			});
			var personIds = vm.selectedAgents.map(function (agent) { return agent.PersonId; })
				.filter(function (personId) {
					return invalidPersonIds.indexOf(personId) < 0;
				});
			var requestData = {
				PersonIds: personIds,
				Date: vm.selectedDate(),
				AbsenceId: vm.selectedAbsenceId,
				TrackedCommandInfo: { TrackId: vm.trackId }
			};
			if (vm.isFullDayAbsence) {
				requestData.Start = serviceDateFormatHelper.getDateOnly(vm.timeRange.startTime);
				requestData.End = serviceDateFormatHelper.getDateOnly(vm.timeRange.endTime);
			} else {
				requestData.Start = vm.convertTime(getDateTimeInTimeZone(vm.timeRange.startTime));
				requestData.End = vm.convertTime(getDateTimeInTimeZone(vm.timeRange.endTime));
			}
			requestData.IsFullDay = vm.isFullDayAbsence;

			vm.runningCommand = true;

			var commandExecutionPromise;

			commandExecutionPromise = CommandCheckService.checkPersonalAccounts(requestData)
				.then(function (data) {
					return addAbsence(data);
				});

			if (commandExecutionPromise) {
				commandExecutionPromise.then(function () { vm.runningCommand = false; });
			} else {
				vm.runningCommand = false;
			}

		};

		function getDateTimeInTimeZone(dateTime) {
			return serviceDateFormatHelper.getDateTime(moment.tz(dateTime, vm.getCurrentTimezone()));
		}

		function updateDateAndTimeFormat() {
			var timeFormat = $locale.DATETIME_FORMATS.shortTime;
			vm.showMeridian = timeFormat.indexOf("h:") >= 0 || timeFormat.indexOf("h.") >= 0;
		};
	}
})();
