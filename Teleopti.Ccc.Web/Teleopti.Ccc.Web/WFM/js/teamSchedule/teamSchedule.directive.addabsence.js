'use strict';

(function () {
	angular.module('wfm.teamSchedule').directive('addAbsence', ['$locale', '$translate', 'guidgenerator', 'PersonAbsence', absencePanel]);

	function absencePanel($locale) {
		return {
			templateUrl: 'js/teamSchedule/html/addabsencepanel.html',
			scope: {
				permissions: '&',
				defaultDateTime: '&',
				agentIdList: '&',
				actionsAfterAbsenceApply: '&'
			},
			controller: ['$translate', 'PersonAbsence', 'guidgenerator', addAbsenceCtrl],
			controllerAs: 'vm',
			bindToController: true,
			link: function (scope, element, attr) {
				scope.vm.init(scope, $locale);
			}
		};
	};

	function addAbsenceCtrl($translate, personAbsenceSvc, guidgenerator) {
		var vm = this;

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

		vm.applyAbsence = function () {
			var trackId = guidgenerator.newGuid();
			var personIds = vm.agentIdList();
			var afterAppliedAbsence = function(result) {
				vm.actionsAfterAbsenceApply({
					result: { TrackId: trackId, Errors: result },
					personIds: personIds,
					successMessageTemplate: 'AddAbsenceSuccessedResult',
					failMessageTemplate: 'AddAbsenceTotalResult'
				});
			}
			if (vm.isFullDayAbsence) {
				personAbsenceSvc.applyFullDayAbsence.post({
					PersonIds: personIds,
					AbsenceId: vm.selectedAbsenceId,
					StartDate: moment(vm.selectedAbsenceStartDate).format("YYYY-MM-DD"),
					EndDate: moment(vm.selectedAbsenceEndDate).format("YYYY-MM-DD"),
					TrackedCommandInfo: { TrackId: trackId }
				}).$promise.then(function(result) { afterAppliedAbsence(result) });
			} else {
				personAbsenceSvc.applyIntradayAbsence.post({
					PersonIds: personIds,
					AbsenceId: vm.selectedAbsenceId,
					StartTime: moment(vm.selectedAbsenceStartDate).format("YYYY-MM-DD HH:mm"),
					EndTime: moment(vm.selectedAbsenceEndDate).format("YYYY-MM-DD HH:mm"),
					TrackedCommandInfo: { TrackId: trackId }
				}).$promise.then(function (result) { afterAppliedAbsence(result) });
			}
		};

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
	};
})();