'use strict';

(function () {

	angular.module('wfm.teamSchedule').directive('addAbsence', ['$locale', absencePanel]);

	function absencePanel($locale) {
		return {
			templateUrl: 'js/teamSchedule/html/addabsencepanel.html',
			scope: {
				selectedDate: '&',
				permissions: '&'
			},
			controller: ['TeamSchedule', 'teamScheduleNotificationService', addAbsenceCtrl],
			controllerAs: 'vm',
			bindToController: true,
			link: function (scope, element, attr) {

				//scope.vm.shortDateFormat = $locale.DATETIME_FORMATS.shortDate;
				//scope.$on('$localeChangeSuccess', function () {
				//	scope.vm.shortDateFormat = $locale.DATETIME_FORMATS.shortDate;
				//});
				//scope.vm.isMiniMode = 'mini' in attr;
			}
		};
	};


	function addAbsenceCtrl(teamScheduleSvc, teamScheduleNotificationService) {
		var vm = this;
		vm.selectedAbsenceStartDate = vm.selectedDate();
		vm.selectedAbsenceEndDate = vm.selectedDate();
		vm.absencePermissions = {
			IsAddIntradayAbsenceAvailable: vm.permissions().IsAddIntradayAbsenceAvailable,
			IsAddFullDayAbsenceAvailable: vm.permissions().IsAddFullDayAbsenceAvailable
		};

		vm.isFullDayAbsence = isFullDayAbsenceDefaultValue();

		vm.isDataChangeValid = function () {
			return vm.selectedAbsenceId !== undefined && (vm.isAbsenceTimeValid() || vm.isAbsenceDateValid());
		};

		vm.isAbsenceTimeValid = function () {
			return !vm.isFullDayAbsence && (moment(vm.selectedAbsenceEndDate) > moment(vm.selectedAbsenceStartDate));
		};

		vm.isAbsenceDateValid = function () {
			return vm.isFullDayAbsence && moment(vm.selectedAbsenceEndDate).startOf('day') >= moment(vm.selectedAbsenceStartDate).startOf('day');
		}

		var handleAddAbsenceResult = function (result) {
			var total = vm.getSelectedPersonIdList().length;

			if (result.length > 0) {
				var successCount = total - result.length;
				teamScheduleNotificationService.notifAbsenceAddedFailed(total, successCount, result.length);
			}
			else {
				teamScheduleNotificationService.notifyAllAbsenceAddedSuccessed(total);
			}

			vm.loadSchedules(vm.paginationOptions.pageNumber);
			vm.setCurrentCommand("");
		}

		function isFullDayAbsenceDefaultValue() {
			return vm.absencePermissions.IsAddFullDayAbsenceAvailable && !vm.absencePermissions.IsAddIntradayAbsenceAvailable;
		};

		vm.cleanUIHistoryAfterApply = function () {
			vm.selectedAbsenceId = '';
			vm.isFullDayAbsence = isFullDayAbsenceDefaultValue();
			vm.selectedAbsenceStartDate = vm.scheduleDate;
			vm.selectedAbsenceEndDate = vm.scheduleDate;
		}

		vm.applyAbsence = function () {
			if (vm.isFullDayAbsence) {
				teamScheduleSvc.applyFullDayAbsence.post({
					PersonIds: vm.getSelectedPersonIdList(),
					AbsenceId: vm.selectedAbsenceId,
					StartDate: moment(vm.selectedAbsenceStartDate).format("YYYY-MM-DD"),
					EndDate: moment(vm.selectedAbsenceEndDate).format("YYYY-MM-DD")
				}).$promise.then(function (result) {
					handleAddAbsenceResult(result);
					vm.cleanUIHistoryAfterApply();
				});
			} else {
				teamScheduleSvc.applyIntradayAbsence.post({
					PersonIds: vm.getSelectedPersonIdList(),
					AbsenceId: vm.selectedAbsenceId,
					StartTime: moment(vm.selectedAbsenceStartDate).format("YYYY-MM-DD HH:mm"),
					EndTime: moment(vm.selectedAbsenceEndDate).format("YYYY-MM-DD HH:mm")
				}).$promise.then(function (result) {
					handleAddAbsenceResult(result);
					vm.cleanUIHistoryAfterApply();
				});
			}
		};
	};

})();