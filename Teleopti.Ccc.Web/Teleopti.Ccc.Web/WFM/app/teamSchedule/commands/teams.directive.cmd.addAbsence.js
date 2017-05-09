(function () {
	'use strict';

	angular.module('wfm.teamSchedule').directive('addAbsence', addAbsenceDirective);

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
				var containerCtrl = ctrls[0],
					selfCtrl = ctrls[1];

				scope.vm.containerCtrl = containerCtrl;

				scope.vm.selectedDate = containerCtrl.getDate;
				scope.vm.trackId = containerCtrl.getTrackId();
				scope.vm.convertTime = containerCtrl.convertTimeToCurrentUserTimezone;
				scope.vm.getActionCb = containerCtrl.getActionCb;
				scope.vm.getCurrentTimezone = containerCtrl.getCurrentTimezone;
				scope.vm.isAddFullDayAbsenceAvailable = function () {
					return containerCtrl.hasPermission('IsAddFullDayAbsenceAvailable');
				};
				scope.vm.checkPersonalAccountEnabled = containerCtrl.hasToggle('CheckPersonalAccountEnabled');
				scope.vm.manageScheduleForDistantTimezonesEnabled = containerCtrl.hasToggle('ManageScheduleForDistantTimezonesEnabled');

				scope.vm.isAddIntradayAbsenceAvailable = function () {
					return containerCtrl.hasPermission('IsAddIntradayAbsenceAvailable');
				};

				scope.vm.timeRange = {
					startTime: selfCtrl.getDefaultAbsenceStartTime(),
					endTime: selfCtrl.getDefaultAbsenceEndTime()
				};

				scope.$watch(function () {
					var format = scope.vm.isFullDayAbsence ? 'YYYY-MM-DD' : 'YYYY-MM-DD HH:mm';
					return {
						startTime: moment(scope.vm.timeRange.startTime).format(format),
						endTime: moment(scope.vm.timeRange.endTime).format(format)
					}
				},
					function (newValue, oldValue) {
						scope.vm.updateInvalidAgents();
					},
					true);

				scope.vm.isFullDayAbsence = scope.vm.isAddFullDayAbsenceAvailable();

				selfCtrl.updateDateAndTimeFormat();
				scope.$on('$localeChangeSuccess', selfCtrl.updateDateAndTimeFormat);

				elem.find('team-schedule-datepicker').on('focus', function (e) {
					e.target.querySelector('input').focus();
				});

				elem.find('uib-timepicker').on('focus', function (e) {
					e.target.querySelector('input').focus();
				});
			}
		};
	}

	addAbsenceCtrl.$inject = ['PersonAbsence', 'PersonSelection', 'ScheduleHelper', 'teamScheduleNotificationService', '$locale', 'CommandCheckService', 'belongsToDateDecider'];

	function addAbsenceCtrl(PersonAbsenceSvc, personSelectionSvc, scheduleHelper, teamScheduleNotificationService, $locale, CommandCheckService, belongsToDateDecider) {
		var vm = this;

		vm.label = 'AddAbsence';
		vm.runningCommand = false;
		vm.selectedAgents = personSelectionSvc.getCheckedPersonInfoList();
		vm.invalidAgents = [];

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
			return (moment(vm.timeRange.endTime).isAfter(moment(vm.timeRange.startTime)));
		};
		vm.isAbsenceDateValid = function () {
			return moment(vm.timeRange.endTime).isSameOrAfter(moment(vm.timeRange.startTime), 'day');
		};

		vm.updateInvalidAgents = function () {
			if (vm.manageScheduleForDistantTimezonesEnabled) {
				if (vm.isFullDayAbsence) {
					determineIsSameTimezoneForFullDayAbsence();
				} else {
					checkIfTimeRangeAllowedForIntradayAbsence();
				}
				return;
			}
		};



		vm.getDefaultAbsenceStartTime = function () {
			var curDateMoment = moment(vm.selectedDate());
			var personIds = vm.selectedAgents.map(function (agent) { return agent.PersonId; });
			var schedules = vm.containerCtrl.scheduleManagementSvc.schedules();
			return scheduleHelper.getEarliestStartOfSelectedSchedules(schedules, curDateMoment, personIds);
		};

		vm.getDefaultAbsenceEndTime = function () {
			return moment(vm.getDefaultAbsenceStartTime()).add(1, 'hour').toDate();
		};

		function determineIsSameTimezoneForFullDayAbsence() {
			vm.invalidAgents = [];
			var invalidAgentNameList = [];
			vm.selectedAgents.forEach(function (agent) {
				if (vm.getCurrentTimezone() != vm.containerCtrl.scheduleManagementSvc.findPersonScheduleVmForPersonId(agent.PersonId).Timezone.IanaId) {
					vm.invalidAgents.push(agent);
					invalidAgentNameList.push(agent.Name);
				}
			});
			vm.invalidAgentNameListString = invalidAgentNameList.join(', ');
		}

		function checkIfTimeRangeAllowedForIntradayAbsence() {
			vm.invalidAgents.length = 0;
			var timeRangeMoment = {
				startTime: moment(vm.timeRange.startTime),
				endTime: moment(vm.timeRange.endTime)
			}
			vm.selectedAgents.forEach(function (agent) {
				var personSchedule = vm.containerCtrl.scheduleManagementSvc.findPersonScheduleVmForPersonId(agent.PersonId);
				if (!belongsToDateDecider
					.checkTimeRangeAllowedForIntradayAbsence(timeRangeMoment,
					belongsToDateDecider.normalizePersonScheduleVm(personSchedule, vm.getCurrentTimezone())))
					vm.invalidAgents.push(agent);
			});
			var invalidAgentNameList = vm.invalidAgents.map(function (agent) {
				return agent.Name;
			});
			vm.invalidAgentNameListString = invalidAgentNameList.join(', ');
		}

		vm.anyValidAgent = function () {
			return vm.invalidAgents.length !== vm.selectedAgents.length;
		};

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
					success: 'AddAbsenceSuccessedResult',
					warning: 'AddAbsenceTotalResult'
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
				requestData.Start = moment(vm.timeRange.startTime).format("YYYY-MM-DD");
				requestData.End = moment(vm.timeRange.endTime).format("YYYY-MM-DD");
			} else {
				requestData.Start = vm.convertTime(moment(vm.timeRange.startTime).format("YYYY-MM-DDTHH:mm"));
				requestData.End = vm.convertTime(moment(vm.timeRange.endTime).format("YYYY-MM-DDTHH:mm"));
			}
			requestData.IsFullDay = vm.isFullDayAbsence;

			vm.runningCommand = true;

			var commandExecutionPromise;

			if (vm.checkPersonalAccountEnabled) {
				commandExecutionPromise = CommandCheckService.checkPersonalAccounts(requestData)
					.then(function (data) {
						return addAbsence(data);
					});
			} else {
				commandExecutionPromise = addAbsence(requestData);
			}

			if (commandExecutionPromise) {
				commandExecutionPromise.then(function () { vm.runningCommand = false; });
			} else {
				vm.runningCommand = false;
			}

		};

		vm.updateDateAndTimeFormat = function () {
			var timeFormat = $locale.DATETIME_FORMATS.shortTime;
			vm.showMeridian = timeFormat.indexOf("h:") >= 0 || timeFormat.indexOf("h.") >= 0;
		};
	}
})();
