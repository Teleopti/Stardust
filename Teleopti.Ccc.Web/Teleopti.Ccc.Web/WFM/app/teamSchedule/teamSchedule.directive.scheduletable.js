(function () {
	'use strict';
	angular.module('wfm.teamSchedule')
		.directive('scheduleTable', scheduleTableDirective)
		.controller('ScheduleTableController', ['$scope', 'Toggle', 'PersonSelection', 'ScheduleManagement', 'ValidateRulesService', 'ScheduleNoteManagementService', ScheduleTableController]);

	function scheduleTableDirective() {
		return {
			scope: {
				selectMode: '=',
				selectedDate: '=',
				selectedTimezone: '<',
				showWarnings: '=?',
				cmdConfigurations: '='
			},
			restrict: 'E',
			controllerAs: 'vm',
			bindToController: true,
			controller: 'ScheduleTableController',
			templateUrl: 'app/teamSchedule/html/scheduletable.html'
		};
	}

	function ScheduleTableController($scope, toggleSvc, personSelectionSvc, ScheduleMgmt, ValidateRulesService, ScheduleNoteMgmt) {
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
			if (!vm.toggles.RemoveAbsenceEnabled && !vm.toggles.RemoveActivityEnabled)
				return false;

			if(currentProjection.IsOvertime || (currentProjection.ParentPersonAbsences == null && currentProjection.ShiftLayerIds == null)) {
				return false;
			}else{
				var isSameDay = currentProjection.Parent && currentProjection.Parent.Date === moment(viewDate).format('YYYY-MM-DD');
				return vm.toggles.ManageScheduleForDistantTimezonesEnabled ? true : isSameDay;
			}
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
			vm.scheduleVm = ScheduleMgmt.groupScheduleVm;
			vm.toggles = {
				SelectAgentsPerPageEnabled: toggleSvc.WfmTeamSchedule_SetAgentsPerPage_36230,
				SeeScheduleChangesByOthers: toggleSvc.WfmTeamSchedule_SeeScheduleChangesByOthers_36303,
				DisplayScheduleOnBusinessHierachyEnabled: toggleSvc.WfmTeamSchedule_DisplayScheduleOnBusinessHierachy_41260,
				DisplayWeekScheduleOnBusinessHierachyEnabled: toggleSvc.WfmTeamSchedule_DisplayWeekScheduleOnBusinessHierachy_42252,
				
				AbsenceReportingEnabled: toggleSvc.WfmTeamSchedule_AbsenceReporting_35995,
				AddActivityEnabled: toggleSvc.WfmTeamSchedule_AddActivity_37541,
				AddPersonalActivityEnabled: toggleSvc.WfmTeamSchedule_AddPersonalActivity_37742,
				AddOvertimeEnabled: toggleSvc.WfmTeamSchedule_AddOvertime_41696,
				MoveActivityEnabled: toggleSvc.WfmTeamSchedule_MoveActivity_37744,
				MoveInvalidOverlappedActivityEnabled: toggleSvc.WfmTeamSchedule_MoveInvalidOverlappedActivity_40688,
				MoveEntireShiftEnabled: toggleSvc.WfmTeamSchedule_MoveEntireShift_41632,
				SwapShiftEnabled: toggleSvc.WfmTeamSchedule_SwapShifts_36231,
				RemoveAbsenceEnabled: toggleSvc.WfmTeamSchedule_RemoveAbsence_36705,
				RemoveActivityEnabled: toggleSvc.WfmTeamSchedule_RemoveActivity_37743,
				UndoScheduleEnabled: toggleSvc.WfmTeamSchedule_RevertToPreviousSchedule_39002,
				
				ViewShiftCategoryEnabled: toggleSvc.WfmTeamSchedule_ShowShiftCategory_39796,
				ModifyShiftCategoryEnabled: toggleSvc.WfmTeamSchedule_ModifyShiftCategory_39797,
				ShowContractTimeEnabled: toggleSvc.WfmTeamSchedule_ShowContractTime_38509,
				EditAndViewInternalNoteEnabled : toggleSvc.WfmTeamSchedule_EditAndDisplayInternalNotes_40671,
				
				WeekViewEnabled: toggleSvc.WfmTeamSchedule_WeekView_39870,
				ShowWeeklyContractTimeEnabled: toggleSvc.WfmTeamSchedule_WeeklyContractTime_39871,
				
				ShowValidationWarnings: toggleSvc.WfmTeamSchedule_ShowNightlyRestWarning_39619
									 || toggleSvc.WfmTeamSchedule_ShowWeeklyWorktimeWarning_39799
									 || toggleSvc.WfmTeamSchedule_ShowWeeklyRestTimeWarning_39800
									 || toggleSvc.WfmTeamSchedule_ShowDayOffWarning_39801
									 || toggleSvc.WfmTeamSchedule_ShowOverwrittenLayerWarning_40109,
				FilterValidationWarningsEnabled: toggleSvc.WfmTeamSchedule_FilterValidationWarnings_40110,
				
				CheckOverlappingCertainActivitiesEnabled: toggleSvc.WfmTeamSchedule_ShowWarningForOverlappingCertainActivities_39938,
				AutoMoveOverwrittenActivityForOperationsEnabled: toggleSvc.WfmTeamSchedule_AutoMoveOverwrittenActivityForOperations_40279,
				CheckPersonalAccountEnabled: toggleSvc.WfmTeamSchedule_CheckPersonalAccountWhenAddingAbsence_41088,
				
				ViewScheduleOnTimezoneEnabled: toggleSvc.WfmTeamSchedule_ShowScheduleBasedOnTimeZone_40925,
				ManageScheduleForDistantTimezonesEnabled:  toggleSvc.WfmTeamSchedule_ShowShiftsForAgentsInDistantTimeZones_41305,
				
				MoveToBaseLicenseEnabled: toggleSvc.WfmTeamSchedule_MoveToBaseLicense_41039
			};

			vm.enbleAllProjectionSelection = vm.toggles.ManageScheduleForDistantTimezonesEnabled;
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
