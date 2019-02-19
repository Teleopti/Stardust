(function () {
	'use strict';
	angular.module('wfm.teamSchedule')
		.directive('scheduleTable', scheduleTableDirective)
		.controller('ScheduleTableController', ScheduleTableController);

	function scheduleTableDirective() {
		return {
			scope: {
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

	angular.module('wfm.teamSchedule')
		.component('scheduleList', {
			controller: ScheduleTableController,
			templateUrl: 'app/teamSchedule/html/schedulelist.html',
			bindings: {
				selectedDate: '=',
				selectedTimezone: '<',
				selectedSortOption: '<',
				showWarnings: '=?',
				paginationOptions: '<?',
				scheduleBodyHeight: '<'
			},
			controllerAs: 'vm'
		});


	angular.module('wfm.teamSchedule').directive('updateSize', [updateSizeDirective]);

	function updateSizeDirective() {
		return {
			restrict: 'A',
			require: '^mdVirtualRepeatContainer',
			link: function (scope, element, attributes, mdVirtualRepeatContainer) {
				scope.$watch(function () {
					return attributes.updateSize;
				}, function (value) {
					if (!value) return;
					var size = parseInt(value);
					if (size % 2 !== 0) size += 1;

					element[0].style.height = size + 'px';
					mdVirtualRepeatContainer.setSize_(size);
					mdVirtualRepeatContainer.updateSize();
				});

			}
		};
	}

	ScheduleTableController.$inject = ['$scope', 'PersonSelection', 'ScheduleManagement', 'ValidateRulesService', 'ScheduleNoteManagementService', 'Toggle', 'teamsPermissions', 'serviceDateFormatHelper'];
	function ScheduleTableController($scope, personSelectionSvc, ScheduleMgmt, ValidateRulesService, ScheduleNoteMgmt, toggleSvc, teamsPermissions, serviceDateFormatHelper) {
		var vm = this;
		vm.scheduleInEditing = null;

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

		vm.$onChanges = function (changesObj) {
			if (changesObj.scheduleBodyHeight
				&& changesObj.scheduleBodyHeight.currentValue !== changesObj.scheduleBodyHeight.previousValue) {

			}
		}

		vm.totalSelectedProjections = function () {
			var totalSelection = personSelectionSvc.getTotalSelectedPersonAndProjectionCount();
			return totalSelection.CheckedPersonCount +
				totalSelection.SelectedActivityInfo.ActivityCount +
				totalSelection.SelectedAbsenceInfo.AbsenceCount;
		};

		vm.updatePersonSelection = function (personSchedule, $event) {
			if ($event) {
				$event.stopPropagation();
			}
			personSelectionSvc.updatePersonSelection(personSchedule);
			personSelectionSvc.toggleAllPersonProjections(personSchedule, vm.selectedDate);
		};

		vm.ToggleProjectionSelection = function (currentProjection, viewDate, $event) {
			if ($event) {
				$event.stopPropagation();
			}
			if (!currentProjection.Selectable()) return;
			currentProjection.ToggleSelection();
			personSelectionSvc.updatePersonProjectionSelection(currentProjection, viewDate);
		};

		vm.togglePerson = function (personSchedule, $event) {
			if ($event === null || $event.target instanceof HTMLTableCellElement) {
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

		vm.getScheduleNoteForPerson = function (personId) {
			return ScheduleNoteMgmt.getNoteForPerson(personId);
		};

		vm.editScheduleNote = function (personId, $event) {
			if ($event)
				$event.stopPropagation();
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

		vm.clickEditButton = function (personSchedule, $event) {
			if ($event)
				$event.stopPropagation();
			if (vm.scheduleInEditing !== personSchedule) {
				if (vm.scheduleInEditing) {
					$scope.$emit('teamSchedule.shiftEditor.close', { isOpeningNew: true });
				}
				vm.scheduleInEditing = personSchedule;
			}
		}

		vm.isScheduleInEditing = function (personSchedule) {
			return vm.scheduleInEditing && vm.scheduleInEditing.PersonId === personSchedule.PersonId;
		}

		vm.getHourPointsForHourLine = function () {
			var pointsCount = vm.scheduleVm.TimeLine.HourPoints.length;
			return angular.copy(vm.scheduleVm.TimeLine.HourPoints).splice(0, pointsCount - 2);
		}

		vm.getRawSchedule = function (personId) {
			return ScheduleMgmt.getRawScheduleByPersonId(vm.selectedDate, personId);
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
		};
		vm.init();

		$scope.$on('teamSchedule.shiftEditor.close', function (e, d) {
			if (!(d && d.isOpeningNew))
				vm.scheduleInEditing = null;
		});

		$scope.$watchCollection(function () {
			return angular.isDefined(ScheduleMgmt.groupScheduleVm) ? ScheduleMgmt.groupScheduleVm.Schedules : [];
		}, function (newVal) {
			if (newVal)
				vm.init();
		});
	};
}());
