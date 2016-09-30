(function () {
	'use strict';
	angular.module('wfm.teamSchedule')
		.directive('scheduleTable', scheduleTableDirective)
		.controller('scheduleTableCtrl', ['$scope', 'Toggle', 'PersonSelection', 'ScheduleManagement', 'ValidateRulesService', 'ScheduleNoteManagementService', scheduleTableController]);

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
			templateUrl: 'app/teamSchedule/html/scheduletable.html'
		};
	}

	function scheduleTableController($scope, toggleSvc, personSelectionSvc, ScheduleMgmt, ValidateRulesService, ScheduleNoteMgmt) {
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

		vm.canToggleSelection = function (currentProjection, shift, viewDate) {
			if (!vm.toggles.RemoveAbsenceEnabled && !vm.toggles.RemoveActivityEnabled)
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
			vm.scheduleVm = ScheduleMgmt.groupScheduleVm;
			vm.toggles = {
				SeeScheduleChangesByOthers: toggleSvc.WfmTeamSchedule_SeeScheduleChangesByOthers_36303,
				SelectAgentsPerPageEnabled: toggleSvc.WfmTeamSchedule_SetAgentsPerPage_36230,
				
				AbsenceReportingEnabled: toggleSvc.WfmTeamSchedule_AbsenceReporting_35995,
				AddActivityEnabled: toggleSvc.WfmTeamSchedule_AddActivity_37541,
				AddPersonalActivityEnabled: toggleSvc.WfmTeamSchedule_AddPersonalActivity_37742,
				RemoveAbsenceEnabled: toggleSvc.WfmTeamSchedule_RemoveAbsence_36705,
				RemoveActivityEnabled: toggleSvc.WfmTeamSchedule_RemoveActivity_37743,
				SwapShiftEnabled: toggleSvc.WfmTeamSchedule_SwapShifts_36231,
				MoveActivityEnabled: toggleSvc.WfmTeamSchedule_MoveActivity_37744,
				UndoScheduleEnabled: toggleSvc.WfmTeamSchedule_RevertToPreviousSchedule_39002,
				MoveInvalidOverlappedActivityEnabled: toggleSvc.WfmTeamSchedule_MoveInvalidOverlappedActivity_40688,
				
				WeekViewEnabled: toggleSvc.WfmTeamSchedule_WeekView_39870,
				
				AutoMoveOverwrittenActivityForOperationsEnabled: toggleSvc.WfmTeamSchedule_AutoMoveOverwrittenActivityForOperations_40279,
				CheckOverlappingCertainActivitiesEnabled: toggleSvc.WfmTeamSchedule_ShowWarningForOverlappingCertainActivities_39938,
				
				ViewShiftCategoryEnabled: toggleSvc.WfmTeamSchedule_ShowShiftCategory_39796,
				ModifyShiftCategoryEnabled: toggleSvc.WfmTeamSchedule_ModifyShiftCategory_39797,

				ShowContractTimeEnabled: toggleSvc.WfmTeamSchedule_ShowContractTime_38509,
				
				EditAndViewInternalNoteEnabled : toggleSvc.WfmTeamSchedule_EditAndDisplayInternalNotes_40671,
				
				FilterValidationWarningsEnabled: toggleSvc.WfmTeamSchedule_FilterValidationWarnings_40110,
				ShowValidationWarnings: toggleSvc.WfmTeamSchedule_ShowNightlyRestWarning_39619
									 || toggleSvc.WfmTeamSchedule_ShowWeeklyWorktimeWarning_39799
									 || toggleSvc.WfmTeamSchedule_ShowWeeklyRestTimeWarning_39800
									 || toggleSvc.WfmTeamSchedule_ShowDayOffWarning_39801
									 || toggleSvc.WfmTeamSchedule_ShowOverwrittenLayerWarning_40109
			};
		};

		vm.init();

		$scope.$watch(function () {
			return ScheduleMgmt.groupScheduleVm.Schedules;
		}, function (newVal) {
			if (newVal)
				vm.init();
		});
	};
}());
