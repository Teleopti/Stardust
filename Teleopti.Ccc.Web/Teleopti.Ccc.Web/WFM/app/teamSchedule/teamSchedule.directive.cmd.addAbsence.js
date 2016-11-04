(function () {
	'use strict';

	angular.module('wfm.teamSchedule').directive('addAbsence', addAbsenceDirective);

	addAbsenceCtrl.$inject = ['PersonAbsence', 'PersonSelection',  'ScheduleManagement', 'teamScheduleNotificationService', '$locale', 'CommandCheckService'];

	function addAbsenceCtrl(PersonAbsenceSvc, personSelectionSvc, scheduleManagementSvc, teamScheduleNotificationService, $locale, CommandCheckService) {
		var vm = this;

		vm.label = 'AddAbsence';

		vm.selectedAgents = personSelectionSvc.getCheckedPersonInfoList();

		PersonAbsenceSvc.loadAbsences().then(function (absences) {
			vm.availableAbsenceTypes = absences;
			vm.availableAbsenceTypesLoaded = true;
		});

		vm.isTimeRangeValid = function () {
			return vm.isAbsenceTimeValid() || vm.isAbsenceDateValid();
		};

		vm.isAbsenceTimeValid = function () {
			return !vm.isFullDayAbsence && (moment(vm.timeRange.endTime) >= moment(vm.timeRange.startTime));
		};

		vm.isInputValid = function() {
			if (vm.manageScheduleForDistantTimezonesEnabled) {
				return vm.isTimeRangeValid() && (vm.isFullDayAbsence && determineIsSameTimezone());
			}
			return vm.isTimeRangeValid();
		}

		vm.isAbsenceDateValid = function () {
			return vm.isFullDayAbsence && moment(vm.timeRange.endTime).isSameOrAfter(moment(vm.timeRange.startTime), 'day');
		};

		vm.getDefaultAbsenceStartTime = function() {
			var curDateMoment = moment(vm.selectedDate());
			var personIds = vm.selectedAgents.map(function(agent) { return agent.PersonId; });
			return scheduleManagementSvc.getEarliestStartOfSelectedSchedule(curDateMoment, personIds);
		};

		vm.getDefaultAbsenceEndTime = function() {
			return moment(vm.getDefaultAbsenceStartTime()).add(1, 'hour').toDate();
		};

		vm.addAbsence = function() {
			var requestData = {
				PersonIds: vm.selectedAgents.map(function(agent) { return agent.PersonId; }),
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

			if (vm.checkPersonalAccountEnabled) {
				CommandCheckService.checkPersonalAccounts(requestData)
					.then(function(data) {
						addAbsence(data);
					});
			} else {
				addAbsence(requestData);
			}

		};


		function determineIsSameTimezone() {
			var invalidAgentNameList = [];
			vm.selectedAgents.forEach(function (agent) {
				if (vm.getCurrentTimezone() != scheduleManagementSvc.findPersonScheduleVmForPersonId(agent.PersonId).Timezone.IanaId) {
					invalidAgentNameList.push(agent.Name);
				}
			});
			vm.invalidAgentNameListString = invalidAgentNameList.join(', ');
			return invalidAgentNameList.length == 0;
		}

		function addAbsence(requestData) {
			if (requestData.PersonIds.length === 0) {
				if (vm.getActionCb(vm.label)) {
					vm.getActionCb(vm.label)(vm.trackId, requestData.PersonIds);
				}
				return;
			}
			PersonAbsenceSvc.addAbsence(requestData, vm.isFullDayAbsence).then(function (response) {
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

		vm.updateDateAndTimeFormat = function () {
			var timeFormat = $locale.DATETIME_FORMATS.shortTime;
			vm.showMeridian = timeFormat.indexOf("h:") >= 0 || timeFormat.indexOf("h.") >= 0;
		}
	}


	function addAbsenceDirective() {
		return {
			restrict: 'E',
			scope: {},
			controller: addAbsenceCtrl,
			controllerAs: 'vm',
			bindToController: true,
			templateUrl: 'app/teamSchedule/html/addAbsence.tpl.html',
			require: ['^teamscheduleCommandContainer', 'addAbsence'],
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
					tElement[0].querySelectorAll('md-select.absence-selector'),
					tElement[0].querySelectorAll('team-schedule-datepicker'),
					tElement[0].querySelectorAll('[uib-timepicker]'),
					tElement[0].querySelectorAll('input#is-full-day'),
					tElement[0].querySelectorAll('button#applyAbsence')
				);

				return postlink;
			}
		}

		function postlink(scope, elem, attrs, ctrls) {
			var containerCtrl = ctrls[0],
				selfCtrl = ctrls[1];

			scope.vm.selectedDate = containerCtrl.getDate;
			scope.vm.trackId = containerCtrl.getTrackId();
			scope.vm.convertTime = containerCtrl.convertTimeToCurrentUserTimezone;
			scope.vm.getActionCb = containerCtrl.getActionCb;
			scope.vm.getCurrentTimezone = containerCtrl.getCurrentTimezone;
			scope.vm.isAddFullDayAbsenceAvailable = function () {
				return containerCtrl.hasPermission('IsAddFullDayAbsenceAvailable');
			};
			scope.vm.checkPersonalAccountEnabled = containerCtrl.hasToggle('CheckPersonalAccountEnabled');
			scope.vm.manageScheduleForDistantTimezonesEnabled = containerCtrl
				.hasToggle('ManageScheduleForDistantTimezonesEnabled');

			scope.vm.isAddIntradayAbsenceAvailable = function () {
				return containerCtrl.hasPermission('IsAddIntradayAbsenceAvailable');
			};

			scope.vm.timeRange = {
				startTime: selfCtrl.getDefaultAbsenceStartTime(),
				endTime: selfCtrl.getDefaultAbsenceEndTime()
			};

			scope.$watch(function () { return scope.vm.isFullDayAbsence; }, function (newValue, oldValue) {
				if (newValue) {
					scope.vm.timeRange = {
						startTime: selfCtrl.getDefaultAbsenceStartTime(),
						endTime: selfCtrl.getDefaultAbsenceEndTime()
					};
				}
			});

			scope.vm.isFullDayAbsence = scope.vm.isAddFullDayAbsenceAvailable();

			scope.$on('teamSchedule.command.focus.default', function () {
				var focusTarget = elem[0].querySelector('.focus-default');
				if (focusTarget) angular.element(focusTarget).focus();
			});

			selfCtrl.updateDateAndTimeFormat();
			scope.$on('$localeChangeSuccess', selfCtrl.updateDateAndTimeFormat);

			elem.find('team-schedule-datepicker').on('focus', function (e) {
				e.target.querySelector('input').focus();
			});

			elem.find('uib-timepicker').on('focus', function (e) {
				e.target.querySelector('input').focus();
			});

			elem.removeAttr('tabindex');
		}
	}
})();
