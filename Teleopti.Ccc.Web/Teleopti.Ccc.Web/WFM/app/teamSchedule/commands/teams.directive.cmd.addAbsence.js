﻿(function () {
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
				var selfCtrl = ctrls[1];

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

	addAbsenceCtrl.$inject = ['$scope','PersonAbsence',
		'PersonSelection',
		'ScheduleHelper',
		'teamScheduleNotificationService',
		'$locale',
		'CommandCheckService',
		'belongsToDateDecider',
		'teamsToggles',
		'serviceDateFormatHelper'];

	function addAbsenceCtrl($scope, PersonAbsenceSvc, personSelectionSvc, scheduleHelper, teamScheduleNotificationService, $locale, CommandCheckService, belongsToDateDecider, teamsToggles, serviceDateFormatHelper) {
		var vm = this;
		vm.label = 'AddAbsence';
		var containerCtrl = $scope.$parent.vm;
		vm.containerCtrl = containerCtrl;

		vm.runningCommand = false;
		vm.selectedAgents = personSelectionSvc.getCheckedPersonInfoList();
		vm.invalidAgents = [];
		var toggles = teamsToggles.all();

		vm.selectedDate = containerCtrl.getDate;
		vm.trackId = containerCtrl.getTrackId();
		vm.convertTime = containerCtrl.getServiceTimeInCurrentUserTimezone;
		vm.getActionCb = containerCtrl.getActionCb;
		vm.getCurrentTimezone = containerCtrl.getCurrentTimezone;
		vm.isAddFullDayAbsenceAvailable = function () {
			return containerCtrl.hasPermission('IsAddFullDayAbsenceAvailable');
		};

		vm.getReferencedDate = function (dateTime) {
			return moment(dateTime).format('YYYY-MM-DD');
		}

		vm.isAddIntradayAbsenceAvailable = function () {
			return containerCtrl.hasPermission('IsAddIntradayAbsenceAvailable');
		};

		vm.timeRange = {
			startTime: moment(getDefaultAbsenceStartTime()).format('YYYY-MM-DD HH:mm'),
			endTime: moment(getDefaultAbsenceEndTime()).format('YYYY-MM-DD HH:mm')
		};
		vm.isFullDayAbsence = vm.isAddFullDayAbsenceAvailable();

		$scope.$watch(function() {
				var format = vm.isFullDayAbsence ? 'YYYY-MM-DD' : 'YYYY-MM-DD HH:mm';
				return {
					startTime: moment(vm.timeRange.startTime).format(format),
					endTime: moment(vm.timeRange.endTime).format(format)
				}
			},
			function(newValue, oldValue) {
				vm.updateInvalidAgents();
			},
			true);

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
			console.log('in add absence panel', vm.timeRange);
			if (vm.isFullDayAbsence) {
				determineIsSameTimezoneForFullDayAbsence();
			} else {
				checkIfTimeRangeAllowedForIntradayAbsence();
			}
			return;
		};

		function getDefaultAbsenceStartTime () {
			var curDateMoment = moment(vm.selectedDate());
			var personIds = vm.selectedAgents.map(function (agent) { return agent.PersonId; });
			var schedules = vm.containerCtrl.scheduleManagementSvc.schedules();
			return scheduleHelper.getEarliestStartOfSelectedSchedules(schedules, curDateMoment, personIds);
		};

		function getDefaultAbsenceEndTime () {
			return moment(getDefaultAbsenceStartTime()).add(1, 'hour').toDate();
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

				if (!toggles.WfmTeamSchedule_AddAbsenceFromPartOfDayToXDay_46010 && !belongsToDateDecider
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
				requestData.Start = serviceDateFormatHelper.getDateOnly(vm.timeRange.startTime);
				requestData.End = serviceDateFormatHelper.getDateOnly(vm.timeRange.endTime);
			} else {
				requestData.Start = vm.convertTime(serviceDateFormatHelper.getDateTime(vm.timeRange.startTime));
				requestData.End = vm.convertTime(serviceDateFormatHelper.getDateTime(vm.timeRange.endTime));
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

		vm.updateDateAndTimeFormat = function () {
			var timeFormat = $locale.DATETIME_FORMATS.shortTime;
			vm.showMeridian = timeFormat.indexOf("h:") >= 0 || timeFormat.indexOf("h.") >= 0;
		};
	}
})();
