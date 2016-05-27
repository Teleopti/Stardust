(function () {
	'use strict';

	angular.module('wfm.teamSchedule').directive('addAbsence', addAbsenceDirective);

	addAbsenceCtrl.$inject = ['PersonAbsence', 'PersonSelection', 'WFMDate', 'ScheduleManagement', 'teamScheduleNotificationService', '$locale'];

	function addAbsenceCtrl(PersonAbsenceSvc, personSelectionSvc, wFMDateSvc, scheduleManagementSvc, teamScheduleNotificationService, $locale) {
		var vm = this;

		vm.label = 'AddAbsence';
	
		vm.selectedAgents = personSelectionSvc.getSelectedPersonInfoList();

		PersonAbsenceSvc.loadAbsences().then(function (absences) {
			vm.availableAbsenceTypes = absences;
			vm.availableAbsenceTypesLoaded = true;
		});

		vm.isTimeRangeValid = function() {
			return vm.isAbsenceTimeValid() || vm.isAbsenceDateValid();
		}

		vm.isAbsenceTimeValid = function () {
			return !vm.isFullDayAbsence && (moment(vm.timeRange.endTime) >= moment(vm.timeRange.startTime));
		};

		vm.isAbsenceDateValid = function () {
			return vm.isFullDayAbsence && moment(vm.timeRange.endTime).startOf('day') >= moment(vm.timeRange.startTime).startOf('day');
		}

		vm.getDefaultAbsenceStartTime = function () {
			var curDateMoment = moment(vm.selectedDate());
			var personIds = vm.selectedAgents.map(function(agent) { return agent.personId; });
			return scheduleManagementSvc.getEarliestStartOfSelectedSchedule(curDateMoment, personIds);
		}
		
		vm.getDefaultAbsenceEndTime = function() {
			return moment(vm.getDefaultAbsenceStartTime()).add(1, 'hour').toDate();
		}

		vm.addAbsence = function () {
			var requestData = {
				PersonIds: vm.selectedAgents.map(function (agent) { return agent.personId; }),
				Date: vm.selectedDate(),	
				AbsenceId: vm.selectedAbsenceId,
				TrackedCommandInfo: { TrackId: vm.trackId }
			};
			var actionPromise;
			if (vm.isFullDayAbsence) {
				requestData.StartDate = moment(vm.timeRange.startTime).format("YYYY-MM-DD");
				requestData.EndDate = moment(vm.timeRange.endTime).format("YYYY-MM-DD");
				actionPromise = PersonAbsenceSvc.addFullDayAbsence(requestData);
			} else {
				requestData.StartTime = moment(vm.timeRange.startTime).format("YYYY-MM-DDTHH:mm");
				requestData.EndTime = moment(vm.timeRange.endTime).format("YYYY-MM-DDTHH:mm");
				actionPromise = PersonAbsenceSvc.addIntradayAbsence(requestData);
			}

			actionPromise.then(function (response) {
				if (vm.getActionCb(vm.label)) {
					vm.getActionCb(vm.label)(vm.trackId, requestData.PersonIds);
				}
				teamScheduleNotificationService.reportActionResult({
					success: 'AddAbsenceSuccessedResult',
					warning: 'AddAbsenceTotalResult'
				}, vm.selectedAgents.map(function (x) {
					return {
						PersonId: x.personId,
						Name: x.name
					}
				}), response.data);
			});
		};

		vm.updateDateAndTimeFormat = updateDateAndTimeFormat;

		function updateDateAndTimeFormat() {
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
			templateUrl: 'js/teamSchedule/html/addAbsence.tpl.html',
			require: ['^teamscheduleCommandContainer', 'addAbsence'],
			link: postlink
		}

		function postlink(scope, elem, attrs, ctrls) {
			var containerCtrl = ctrls[0],
				selfCtrl = ctrls[1];

			scope.vm.selectedDate = containerCtrl.getDate;
			scope.vm.trackId = containerCtrl.getTrackId();
			scope.vm.getActionCb = containerCtrl.getActionCb;
			scope.vm.isAddFullDayAbsenceAvailable = function() {
				return containerCtrl.hasPermission('IsAddFullDayAbsenceAvailable');
			};
			scope.vm.isAddIntradayAbsenceAvailable = function() {
				return containerCtrl.hasPermission('IsAddIntradayAbsenceAvailable');
			}

			scope.vm.timeRange = {
				startTime: selfCtrl.getDefaultAbsenceStartTime(),
				endTime: selfCtrl.getDefaultAbsenceEndTime()
			};

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
		}
	}



})();