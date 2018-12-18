(function () {
	'use strict';
	angular.module('wfm.teamSchedule')
		.directive('scheduleTable', scheduleTableDirective)
		.controller('ScheduleTableController', ScheduleTableController);

	function scheduleTableDirective() {
		return {
			scope: {
				selectMode: '=',
				selectedDate: '=',
				selectedTimezone: '<',
				selectedSortOption: '<',
				showWarnings: '=?',
				paginationOptions: '<?',
				tableStyle: '='
			},
			restrict: 'E',
			controllerAs: 'vm',
			bindToController: true,
			controller: 'ScheduleTableController',
			templateUrl: 'app/teamSchedule/html/scheduletable.html'
		};
	}

	ScheduleTableController.$inject = ['$scope', '$state', 'PersonSelection', 'ScheduleManagement', 'ValidateRulesService', 'ScheduleNoteManagementService', 'Toggle', 'teamsPermissions', 'serviceDateFormatHelper'];
	function ScheduleTableController($scope, $state, personSelectionSvc, ScheduleMgmt, ValidateRulesService, ScheduleNoteMgmt, toggleSvc, teamsPermissions, serviceDateFormatHelper) {
		var vm = this;
		var scheduleInEditing;

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
			return currentProjection.Selectable();
		};

		vm.ToggleProjectionSelection = function (currentProjection, viewDate) {
			if (!vm.canToggleSelection(currentProjection, viewDate)) return;
			currentProjection.ToggleSelection();
			personSelectionSvc.updatePersonProjectionSelection(currentProjection, viewDate);
		};

		vm.togglePerson = function (personSchedule, $event) {
			if ($event === null) {
				personSchedule.IsSelected = !personSchedule.IsSelected;
				vm.updatePersonSelection(personSchedule);
			}
			else if ($event.target instanceof HTMLTableCellElement) {
				personSchedule.IsSelected = !personSchedule.IsSelected;
				vm.updatePersonSelection(personSchedule);
			}
		};

		vm.modifyShiftCategoryForAgent = function ($event, personSchedule) {
			if (!vm.permissions.HasEditShiftCategoryPermission) {
				return;
			}

			$event.stopPropagation();

			if (!personSchedule.IsSelected) {
				vm.togglePerson(personSchedule, null);
			}

			if (personSchedule.IsSelected) {
				var activeCmdLabel = "EditShiftCategory";

				$scope.$emit('teamSchedule.trigger.command', {
					activeCmd: activeCmdLabel,
					needToOpenSidePanel: true
				});
			}
		};

		vm.checkBusinessRulesWarningMessage = function (personId) {
			return ValidateRulesService.checkValidationForPerson(personId);
		};

		vm.checkIsLoadedValidationForPerson = function (personId) {
			return ValidateRulesService.checkIsLoadedValidationForPerson(personId);
		};

		vm.getScheduleNoteForPerson = function (personId) {
			return ScheduleNoteMgmt.getNoteForPerson(personId);
		};

		vm.hasPublicNote = function (personId) {
			return ScheduleNoteMgmt.getNoteForPerson(personId).publicNotes && ScheduleNoteMgmt.getNoteForPerson(personId).publicNotes.length > 0;
		}

		vm.editScheduleNote = function (personId) {
			vm.noteEditorInputOption = {
				selectedDate: vm.selectedDate,
				personId: personId,
				showEditor: true
			};
		};
	
		vm.showEditButton = function (personSchedule) {
			return toggleSvc.WfmTeamSchedule_ShiftEditorInDayView_78295
				&& !personSchedule.IsFullDayAbsence
				&& !(personSchedule.IsProtected && !vm.permissions.HasModifyWriteProtectedSchedulePermission)
				&& !personSchedule.IsDayOff()
				&& !!personSchedule.ActivityCount();
		}

		vm.clickEditButton = function (personSchedule) {
			if (scheduleInEditing !== personSchedule)
				scheduleInEditing = personSchedule;
		}

		vm.isScheduleEditing = function (personSchedule) {
			return scheduleInEditing === personSchedule;
		}

		vm.isNotSameTimezone = function (personTimezone) {
			return vm.selectedTimezone && personTimezone.IanaId !== vm.selectedTimezone;
		}

		vm.getHourPointsForHourLine = function () {
			var pointsCount = vm.scheduleVm.TimeLine.HourPoints.length;
			return angular.copy(vm.scheduleVm.TimeLine.HourPoints).splice(0, pointsCount - 2);
		}

		function isAllInCurrentPageSelected() {
			var isAllSelected = true;
			var selectedPeople = personSelectionSvc.personInfo;
			if (!vm.scheduleVm || !vm.scheduleVm.Schedules) {
				return false;
			}
			for (var i = 0; i < vm.scheduleVm.Schedules.length; i++) {
				var personSchedule = vm.scheduleVm.Schedules[i];
				if (!selectedPeople[personSchedule.PersonId] || !selectedPeople[personSchedule.PersonId].Checked) {
					isAllSelected = false;
					break;
				}
			}

			return isAllSelected;
		}

		vm.init = function () {
			vm.toggleAllInCurrentPage = isAllInCurrentPageSelected();
			vm.scheduleVm = ScheduleMgmt.groupScheduleVm;

			vm.permissions = teamsPermissions.all();

			$scope.$on('teamSchedule.shiftEditor.cancel', function () {
				scheduleInEditing = null;
			});
		};

		vm.init();

		$scope.$watchCollection(function () {
			return angular.isDefined(ScheduleMgmt.groupScheduleVm) ? ScheduleMgmt.groupScheduleVm.Schedules : [];
		}, function (newVal) {
			if (newVal)
				vm.init();
		});
	};
}());
