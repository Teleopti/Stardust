(function () {
	'use strict';
	angular.module('wfm.teamSchedule')
		.directive('scheduleTable', scheduleTableDirective)
		.controller('ScheduleTableController', ['$scope', 'Toggle', 'PersonSelection', 'ScheduleManagement', 'ValidateRulesService', 'ScheduleNoteManagementService','teamsToggles', ScheduleTableController]);

	function scheduleTableDirective() {
		return {
			scope: {
				selectMode: '=',
				selectedDate: '=',
				selectedTimezone: '<',
				showWarnings: '=?',
				cmdConfigurations: '=',
				paginationOptions: '<?'
			},
			restrict: 'E',
			controllerAs: 'vm',
			bindToController: true,
			controller: 'ScheduleTableController',
			templateUrl: 'app/teamSchedule/html/scheduletable.html'
		};
	}

	function ScheduleTableController($scope, toggleSvc, personSelectionSvc, ScheduleMgmt, ValidateRulesService, ScheduleNoteMgmt, teamsToggles) {
		var vm = this;

		vm.updateAllSelectionInCurrentPage = function (isAllSelected) {
			vm.scheduleVm.Schedules.forEach(function (personSchedule) {
				personSchedule.IsSelected = isAllSelected;
				$scope.$evalAsync(function () {
					vm.updatePersonSelection(personSchedule);
				});
			});
		};

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

		vm.canToggleSelection = function (currentProjection, viewDate) {
			if ((!vm.toggles.RemoveAbsenceEnabled && !vm.toggles.RemoveActivityEnabled) || !currentProjection.Selectable())
				return false;

			var isSameDay = currentProjection.Parent && currentProjection.Parent.Date === moment(viewDate).format('YYYY-MM-DD');
			return vm.toggles.ManageScheduleForDistantTimezonesEnabled ? true : isSameDay;
		};

		vm.ToggleProjectionSelection = function (currentProjection, viewDate) {
			if (!vm.canToggleSelection(currentProjection, viewDate)) return;
			currentProjection.ToggleSelection();
			personSelectionSvc.updatePersonProjectionSelection(currentProjection, viewDate);
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
			if (!(vm.toggles.ModifyShiftCategoryEnabled && vm.cmdConfigurations.permissions.HasEditShiftCategoryPermission)) {
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

		vm.hasHiddenScheduleAtStart = function (personSchedule) {
			if (!personSchedule.Shifts) return false;

			var result = false;
			personSchedule.Shifts.forEach(function(shift) {
				if (moment(shift.ProjectionTimeRange.Start) < personSchedule.ViewRange.startMoment)
					result = true;
			});
			return result;
		};

		vm.hasHiddenScheduleAtEnd = function (personSchedule) {
			if (!personSchedule.Shifts) return false;

			var result = false;
			personSchedule.Shifts.forEach(function (shift) {				
				if (moment(shift.ProjectionTimeRange.End) > personSchedule.ViewRange.endMoment)
					result = true;
			});
			return result;
		};


		vm.checkBusinessRulesWarningMessage = function(personId){
			return ValidateRulesService.checkValidationForPerson(personId);
		};

		vm.checkIsLoadedValidationForPerson = function(personId){
			return ValidateRulesService.checkIsLoadedValidationForPerson(personId);
		};

		vm.getScheduleNoteForPerson = function (personId) {
			return ScheduleNoteMgmt.getInternalNoteForPerson(personId);
		};

		vm.editScheduleNote = function (personId) {
			vm.noteEditorInputOption = {
				selectedDate: vm.selectedDate,
				personId: personId,
				showEditor:true
			};
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

			if (vm.paginationOptions && ScheduleMgmt.groupScheduleVm && ScheduleMgmt.groupScheduleVm.Schedules && ScheduleMgmt.groupScheduleVm.Schedules.length > vm.paginationOptions.pageSize) {
				vm.scheduleVm = {
					Schedules: ScheduleMgmt.groupScheduleVm.Schedules.slice((vm.paginationOptions.pageNumber - 1) * vm.paginationOptions.pageSize,
					vm.paginationOptions.pageNumber * vm.paginationOptions.pageSize)
				};

			} else {
				vm.scheduleVm = ScheduleMgmt.groupScheduleVm;
			}

			vm.toggles = teamsToggles.all();

			vm.enbleAllProjectionSelection = vm.toggles.ManageScheduleForDistantTimezonesEnabled;
		};

		vm.init();

		$scope.$watchCollection(function () {
			if (vm.paginationOptions && ScheduleMgmt.groupScheduleVm && ScheduleMgmt.groupScheduleVm.Schedules && ScheduleMgmt.groupScheduleVm.Schedules.length > vm.paginationOptions.pageSize) {				
				return ScheduleMgmt.groupScheduleVm.Schedules.slice((vm.paginationOptions.pageNumber - 1) * vm.paginationOptions.pageSize,
					vm.paginationOptions.pageNumber * vm.paginationOptions.pageSize);
			}

			return angular.isDefined(ScheduleMgmt.groupScheduleVm) ? ScheduleMgmt.groupScheduleVm.Schedules : [];
		}, function (newVal) {
			if (newVal)
				vm.init();
		});
	};
}());
