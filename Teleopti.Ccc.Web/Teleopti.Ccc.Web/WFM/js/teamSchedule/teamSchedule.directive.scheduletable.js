'use strict';

(function () {
	angular.module('wfm.teamSchedule')
		.directive('scheduleTable', scheduleTableDirective)
		.controller('scheduleTableCtrl', ['Toggle', 'PersonSelection', '$scope', 'ScheduleManagement', 'ValidateRulesService', scheduleTableController]);

	function scheduleTableDirective() {
		return {
			scope: {
				selectMode: '=',
				selectedDate: '=',
				showWarnings: '=?'
			},
			restrict: 'E',
			controllerAs: 'vm',
			bindToController: true,
			controller: 'scheduleTableCtrl',
			templateUrl: "js/teamSchedule/html/scheduletable.html"
		};
	};
	function scheduleTableController(toggleSvc, personSelectionSvc, $scope, ScheduleMgmt, ValidateRulesService) {
		var vm = this;
		vm.ShowContractTimeEnabled = toggleSvc.WfmTeamSchedule_ShowContractTime_38509;
		vm.updateAllSelectionInCurrentPage = function (isAllSelected) {
			vm.scheduleVm.Schedules.forEach(function (personSchedule) {
				personSchedule.IsSelected = isAllSelected;
				$scope.$evalAsync(function () {
					vm.updatePersonSelection(personSchedule);
				});
			});

		};
		vm.init = init;

		$scope.$watch(function () {
			return ScheduleMgmt.groupScheduleVm.Schedules;
		}, function (newVal) {
			if (newVal)
				vm.init();
		});

		$scope.$watch(function () {
			return isAllInCurrentPageSelected();
		}, function (newVal) {
			vm.toggleAllInCurrentPage = newVal;
		});

		vm.totalSelectedProjections = function () {
			return personSelectionSvc.getTotalSelectedPersonAndProjectionCount().CheckedPersonCount +
				personSelectionSvc.getTotalSelectedPersonAndProjectionCount().SelectedActivityInfo.ActivityCount +
				personSelectionSvc.getTotalSelectedPersonAndProjectionCount().SelectedAbsenceInfo.AbsenceCount;
		};

		vm.updatePersonSelection = function (personSchedule) {
			personSelectionSvc.updatePersonSelection(personSchedule);
			personSelectionSvc.toggleAllPersonProjections(personSchedule, vm.selectedDate);
		};

		vm.canToggleSelection = function (currentProjection, shift, viewDate) {
			if (!toggleSvc.WfmTeamSchedule_RemoveAbsence_36705 && !toggleSvc.WfmTeamSchedule_RemoveActivity_37743)
				return false;

			var isSameDay = shift.Date.format("YYYY-MM-DD") === moment(viewDate).format("YYYY-MM-DD");

			if (!isSameDay || currentProjection.IsOvertime || (currentProjection.ParentPersonAbsences == null && currentProjection.ShiftLayerIds == null)) {
				return false;
			}

			return true;
		};


		vm.ToggleProjectionSelection = function (currentProjection, personSchedule, shift, viewDate) {
			if (!vm.canToggleSelection(currentProjection, shift, viewDate)) return;
			currentProjection.ToggleSelection();
			personSelectionSvc.updatePersonProjectionSelection(currentProjection, personSchedule);
		};

		vm.togglePerson = function (personSchedule, $event) {
			if ($event.target instanceof HTMLTableCellElement) {
				personSchedule.IsSelected = !personSchedule.IsSelected;
				vm.updatePersonSelection(personSchedule);
			}
		};

		vm.checkNightRestWarningMessage = function(personId){
			return ValidateRulesService.checkValidationForPerson(personId);
		};

		vm.validationIsDone = function () {
			return ValidateRulesService.ValidationIsDone;
		};

		function isAllInCurrentPageSelected() {
			var isAllSelected = true;
			var selectedPeople = personSelectionSvc.personInfo;
			if (!vm.scheduleVm || !vm.scheduleVm.Schedules) {
				return false;
			}
			for (var i = 0; i < vm.scheduleVm.Schedules.length; i++) {
				var personSchedule = vm.scheduleVm.Schedules[i];
				if (!selectedPeople[personSchedule.PersonId]) {
					isAllSelected = false;
					break;
				}
			}

			return isAllSelected;
		}

		function init() {
			vm.toggleAllInCurrentPage = isAllInCurrentPageSelected();
			vm.scheduleVm = ScheduleMgmt.groupScheduleVm;
		}
	};
}());