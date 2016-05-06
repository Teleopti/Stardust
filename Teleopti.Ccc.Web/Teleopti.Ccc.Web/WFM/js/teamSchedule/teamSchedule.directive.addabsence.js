﻿'use strict';

(function () {
	angular.module('wfm.teamSchedule').directive('addAbsence', ['$locale', '$translate', 'guidgenerator', 'PersonAbsence', absencePanel]);

	function absencePanel($locale) {
		return {
			templateUrl: 'js/teamSchedule/html/addabsencepanel.html',
			scope: {
				permissions: '&',
				defaultDateTime: '&',
				actionsAfterAbsenceApply: '&'
			},
			controller: ['$element', '$translate', 'PersonAbsence', 'guidgenerator', 'CommandCommon', 'PersonSelection', 'teamScheduleNotificationService', addAbsenceCtrl],
			controllerAs: 'vm',
			bindToController: true,
			link: function (scope, element, attr) {
				scope.vm.init(scope, $locale);
			}
		};
	}

	function addAbsenceCtrl($element, $translate, personAbsenceSvc, guidgenerator, CommandCommon, personSelectionSvc, NotificationService) {
		var vm = this;
		addTabIndexToControls();
		removePanelTabIndex();
		addFocusListenerToInputs();

		vm.selectedAbsenceStartDate = vm.defaultDateTime();

		vm.selectedAbsenceEndDate = moment(vm.defaultDateTime()).add(1, 'hour').toDate();
		vm.absencePermissions = {
			IsAddIntradayAbsenceAvailable: vm.permissions().IsAddIntradayAbsenceAvailable,
			IsAddFullDayAbsenceAvailable: vm.permissions().IsAddFullDayAbsenceAvailable
		};
		vm.isFullDayAbsence = vm.absencePermissions.IsAddFullDayAbsenceAvailable
				&& !vm.absencePermissions.IsAddIntradayAbsenceAvailable;

		vm.isDataChangeValid = function () {
			return vm.selectedAbsenceId !== undefined && (vm.isAbsenceTimeValid() || vm.isAbsenceDateValid());
		};

		vm.isAbsenceTimeValid = function () {
			return !vm.isFullDayAbsence && (moment(vm.selectedAbsenceEndDate) >= moment(vm.selectedAbsenceStartDate));
		};

		vm.isAbsenceDateValid = function () {
			return vm.isFullDayAbsence && moment(vm.selectedAbsenceEndDate).startOf('day') >= moment(vm.selectedAbsenceStartDate).startOf('day');
		}

		vm.applyAbsence = CommandCommon.wrapPersonWriteProtectionCheck(true, 'AddAbsence', applyAbsence,null, vm.selectedAbsenceStartDate);

		function applyAbsence() {
			var trackId = guidgenerator.newGuid();
			var personIds = personSelectionSvc.getCheckedPersonIds();
			if (vm.isFullDayAbsence) {
				personAbsenceSvc.applyFullDayAbsence.post({
					PersonIds: personIds,
					AbsenceId: vm.selectedAbsenceId,
					StartDate: moment(vm.selectedAbsenceStartDate).format("YYYY-MM-DD"),
					EndDate: moment(vm.selectedAbsenceEndDate).format("YYYY-MM-DD"),
					TrackedCommandInfo: { TrackId: trackId }
				}).$promise.then(onSuccessfullyAppliedAbsence, onFailingToApplyAbsence);
			} else {
				personAbsenceSvc.applyIntradayAbsence.post({
					PersonIds: personIds,
					AbsenceId: vm.selectedAbsenceId,
					StartTime: moment(vm.selectedAbsenceStartDate).format("YYYY-MM-DD HH:mm"),
					EndTime: moment(vm.selectedAbsenceEndDate).format("YYYY-MM-DD HH:mm"),
					TrackedCommandInfo: { TrackId: trackId }
				}).$promise.then(onSuccessfullyAppliedAbsence, onFailingToApplyAbsence);
			}

			function onFailingToApplyAbsence(result) {
				vm.actionsAfterAbsenceApply({
					trackId: trackId,
					personIds: personIds,
				});
				NotificationService.notify('error', 'AddAbsenceFailed');
			}

			function onSuccessfullyAppliedAbsence(result) {
				vm.actionsAfterAbsenceApply({
					trackId: trackId,
					personIds: personIds,
				});
				var total = personSelectionSvc.getTotalSelectedPersonAndProjectionCount().CheckedPersonCount;
				var fail = result.length;
				if (fail === 0) {
					NotificationService.notify('success', 'AddAbsenceSuccessedResult');
				} else {
					var description = NotificationService.notify('warning', 'AddAbsenceTotalResult', [total - fail, fail]);
				}
			}
		}

		function updateDateAndTimeFormat($scope, $locale) {
			var timeFormat = $locale.DATETIME_FORMATS.shortTime;
			$scope.vm.showMeridian = timeFormat.indexOf("h:") >= 0 || timeFormat.indexOf("h.") >= 0;
			$scope.$on('$localeChangeSuccess', updateDateAndTimeFormat);
		}

		vm.init = function ($scope, $locale) {
			updateDateAndTimeFormat($scope, $locale);
			personAbsenceSvc.loadAbsences.query().$promise.then(function (result) {
				vm.AvailableAbsenceTypes = result;
			});
		};

		function addTabIndexToControls() {
			var panel = $element[0];
			var controls = [];
			controls.push(panel.querySelector('select.absence-selector'));
			controls.push.apply(controls, panel.querySelectorAll('team-schedule-datepicker input'));
			controls.push.apply(controls, panel.querySelectorAll('.timepicker input'));
			controls.push(panel.querySelector('#apply-absence-btn'));
			angular.forEach(controls, addTabIndexToElement);
		}

		function addTabIndexToElement(element) {
			var _tabindex = $element.attr('tabIndex');
			angular.element(element).attr('tabIndex', _tabindex);
		}

		function removePanelTabIndex() {
			$element.removeAttr('tabIndex');
		}

		function addFocusListenerToInputs() {
			var inputs = $element[0].querySelectorAll('.timepicker input');
			angular.forEach(inputs, function (input) {
				angular.element(input).on('focus', function (event) {
					event.target.select();
				});
			});
		}

	}
})();
