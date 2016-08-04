(function () {
	'use strict';
	angular.module('wfm.teamSchedule')
		.directive('scheduleTable', scheduleTableDirective)
		.controller('scheduleTableCtrl', ['$scope', 'Toggle', 'PersonSelection', 'ScheduleManagement', 'ValidateRulesService', scheduleTableController]);

	function scheduleTableDirective() {
		return {
			scope: {
				selectMode: '=',
				selectedDate: '=',
				showWarnings: '=?',
				cmdConfigurations: '='
			},
			restrict: 'E',
			controllerAs: 'vm',
			bindToController: true,
			controller: 'scheduleTableCtrl',
			templateUrl: 'js/teamSchedule/html/scheduletable.html'
		};
	}

	function scheduleTableController($scope, toggleSvc, personSelectionSvc, ScheduleMgmt, ValidateRulesService) {
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

			var isSameDay = shift.Date.format('YYYY-MM-DD') === moment(viewDate).format('YYYY-MM-DD');

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
			if($event === null){
				personSchedule.IsSelected = !personSchedule.IsSelected;
				vm.updatePersonSelection(personSchedule);
			}
			else if ($event.target instanceof HTMLTableCellElement) {
				personSchedule.IsSelected = !personSchedule.IsSelected;
				vm.updatePersonSelection(personSchedule);
			}
		};

		vm.modifyShiftCategoryForAgent = function($event, personSchedule){
			if (!(vm.cmdConfigurations.toggles.ModifyShiftCategoryEnabled && vm.cmdConfigurations.permissions.HasEditShiftCategoryPermission)) {
				return;
			}

			$event.stopPropagation();

			if(!personSchedule.IsSelected){
				vm.togglePerson(personSchedule, null);
			}

			if(personSchedule.IsSelected){
				var activeCmdLabel = "EditShiftCategory";

				$scope.$emit('teamSchedule.trigger.command', {
					activeCmd: activeCmdLabel,
					needToOpenSidePanel: true
				});
			}
		};

		vm.checkBusinessRulesWarningMessage = function(personId){
			return ValidateRulesService.checkValidationForPerson(personId);
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

		vm.init = function () {
			vm.toggleAllInCurrentPage = isAllInCurrentPageSelected();
			vm.scheduleVm = ScheduleMgmt.groupScheduleVm;
			vm.toggles = {
				ViewShiftCategoryEnabled: toggleSvc.WfmTeamSchedule_ShowShiftCategory_39796,
				ModifyShiftCategoryEnabled: toggleSvc.WfmTeamSchedule_ModifyShiftCategory_39797
			};
		};
	};
}());