'use strict';

(function () {
	angular.module('wfm.teamSchedule').directive('addAbsence', ['$locale', '$translate', absencePanel]);

	function absencePanel($locale) {
		return {
			templateUrl: 'js/teamSchedule/html/addabsencepanel.html',
			scope: {
				permissions: '&',
				defaultDateTime: '&',
				agentIdList: '&',
				actionsAfterAbsenceApply: '&'
			},
			controller: ['$translate', 'TeamSchedule', addAbsenceCtrl],
			controllerAs: 'vm',
			bindToController: true,
			link: function (scope, element, attr) {
				scope.vm.init(scope, $locale);
			}
		};
	};


	function addAbsenceCtrl($translate, teamScheduleSvc) {
		var vm = this;

		vm.selectedAbsenceStartDate = vm.defaultDateTime();
		vm.selectedAbsenceEndDate = moment(vm.defaultDateTime()).add(1, 'hour').toDate();
		vm.absencePermissions = {
			IsAddIntradayAbsenceAvailable: vm.permissions().IsAddIntradayAbsenceAvailable,
			IsAddFullDayAbsenceAvailable: vm.permissions().IsAddFullDayAbsenceAvailable
		};
		vm.isFullDayAbsence = isFullDayAbsenceDefaultValue();

		vm.isDataChangeValid = function () {
			return vm.selectedAbsenceId !== undefined && (vm.isAbsenceTimeValid() || vm.isAbsenceDateValid());
		};

		vm.isAbsenceTimeValid = function () {
			return !vm.isFullDayAbsence && (moment(vm.selectedAbsenceEndDate) >= moment(vm.selectedAbsenceStartDate));
		};

		vm.isAbsenceDateValid = function () {
			return vm.isFullDayAbsence && moment(vm.selectedAbsenceEndDate).startOf('day') >= moment(vm.selectedAbsenceStartDate).startOf('day');
		}

		vm.applyAbsence = function() {
			if (vm.isFullDayAbsence) {
				teamScheduleSvc.applyFullDayAbsence.post({
					PersonIds: vm.agentIdList(),
					AbsenceId: vm.selectedAbsenceId,
					StartDate: moment(vm.selectedAbsenceStartDate).format("YYYY-MM-DD"),
					EndDate: moment(vm.selectedAbsenceEndDate).format("YYYY-MM-DD"),
					TrackedCommandInfo: { TrackId: "" } // TODO: Generate unique track id
				}).$promise.then(function(result) {
					vm.actionsAfterAbsenceApply({
						result: result,
						successMessageTemplate: 'AddAbsenceSuccessedResult',
						failMessageTemplate: 'AddAbsenceTotalResult'
					});
				});
			} else {
				teamScheduleSvc.applyIntradayAbsence.post({
					PersonIds: vm.agentIdList(),
					AbsenceId: vm.selectedAbsenceId,
					StartTime: moment(vm.selectedAbsenceStartDate).format("YYYY-MM-DD HH:mm"),
					EndTime: moment(vm.selectedAbsenceEndDate).format("YYYY-MM-DD HH:mm"),
					TrackedCommandInfo: { TrackId: "" } // TODO: Generate unique track id
				}).$promise.then(function(result) {
					vm.actionsAfterAbsenceApply({
						result: result,
						successMessageTemplate: 'AddAbsenceSuccessedResult',
						failMessageTemplate: 'AddAbsenceTotalResult'
					});
				});
			}
		};

		vm.init = function ($scope, $locale) {
			updateDateAndTimeFormat($scope, $locale);
			teamScheduleSvc.PromiseForloadedAvailableAbsenceTypes(updateAvailableAbsenceTypes);
		};

		function isFullDayAbsenceDefaultValue() {
			return vm.absencePermissions.IsAddFullDayAbsenceAvailable && !vm.absencePermissions.IsAddIntradayAbsenceAvailable;
		};

		function updateAvailableAbsenceTypes(result) {
			vm.AvailableAbsenceTypes = result;
		};

		function updateDateAndTimeFormat($scope, $locale) {
			var timeFormat = $locale.DATETIME_FORMATS.shortTime;
			$scope.vm.showMeridian = timeFormat.indexOf("h:") >= 0 || timeFormat.indexOf("h.") >= 0;
			$scope.$on('$localeChangeSuccess', updateDateAndTimeFormat);
		}
	};
})();