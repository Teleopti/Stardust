﻿(function() {
	'use strict';

	angular.module('wfm.teamSchedule').controller('TeamScheduleWeeklyCtrl', TeamScheduleWeeklyCtrl);

	TeamScheduleWeeklyCtrl.$inject = ['$stateParams', '$locale', '$filter', 'PersonScheduleWeekViewCreator', 'UtilityService', 'weekViewScheduleSvc', 'Toggle', 'TeamSchedule', 'signalRSVC', '$scope'];

	function TeamScheduleWeeklyCtrl(params, $locale, $filter, WeekViewCreator, Util, weekViewScheduleSvc, toggleSvc, teamScheduleSvc, signalR, $scope) {
		var vm = this;
		vm.searchOptions = {
			keyword: angular.isDefined(params.keyword) && params.keyword !== '' ? params.keyword : '',
			searchKeywordChanged: false,
			searchFields : [
				'FirstName', 'LastName', 'EmploymentNumber', 'Organization', 'Role', 'Contract', 'ContractSchedule', 'ShiftBags',
				'PartTimePercentage', 'Skill', 'BudgetGroup', 'Note'
			]
		};

		vm.isLoading = false;
		vm.agentsPerPageSelection = [20, 50, 100, 500];

		vm.onKeyWordInSearchInputChanged = function() {
			vm.loadSchedules();
		};

		vm.selectorChanged = function () {
			teamScheduleSvc.updateAgentsPerPageSetting.post({ agents: vm.paginationOptions.pageSize }).$promise.then(function () {
				vm.resetSchedulePage();
			});
		};

		vm.resetSchedulePage = function () {
			vm.paginationOptions.pageNumber = 1;			
			vm.loadSchedules();
		};

		vm.scheduleDate = params.selectedDate || new Date();

		vm.startOfWeek = moment(vm.scheduleDate).startOf('week').toDate();

		vm.onStartOfWeekChanged = function () {
			vm.scheduleDate = new Date(vm.startOfWeek.getTime());
			if (!moment(vm.startOfWeek).startOf('week').isSame(vm.startOfWeek, 'day')) {
				vm.startOfWeek = moment(vm.startOfWeek).startOf('week').toDate();
			}
			vm.weekDays = Util.getWeekdays(vm.startOfWeek);
			vm.loadSchedules();
		};
	
		vm.paginationOptions = {
			pageSize: 20,
			pageNumber: 1,
			totalPages: 0
		};

		vm.loadSchedules = function () {		
			vm.isLoading = true;			
			var inputForm = getParamsForLoadingSchedules();
			weekViewScheduleSvc.getSchedules(inputForm).then(function (data) {				
				vm.groupWeeks = WeekViewCreator.Create(data.PersonWeekSchedules);
				vm.paginationOptions.totalPages = vm.paginationOptions.pageSize > 0? Math.ceil(data.Total / (vm.paginationOptions.pageSize + 0.01) ) : 0;
				vm.isLoading = false;
			}).catch(function() {
				vm.isLoading = false;
			});
		};

		init();

		function init() {
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
				ShowWeeklyContractTimeEnabled: toggleSvc.WfmTeamSchedule_WeeklyContractTime_39871,
				
				EditAndViewInternalNoteEnabled : toggleSvc.WfmTeamSchedule_EditAndDisplayInternalNotes_40671,
				
				FilterValidationWarningsEnabled: toggleSvc.WfmTeamSchedule_FilterValidationWarnings_40110,
				ShowValidationWarnings: toggleSvc.WfmTeamSchedule_ShowNightlyRestWarning_39619
									 || toggleSvc.WfmTeamSchedule_ShowWeeklyWorktimeWarning_39799
									 || toggleSvc.WfmTeamSchedule_ShowWeeklyRestTimeWarning_39800
									 || toggleSvc.WfmTeamSchedule_ShowDayOffWarning_39801
									 || toggleSvc.WfmTeamSchedule_ShowOverwrittenLayerWarning_40109
			};
			vm.weekDays = Util.getWeekdays(vm.scheduleDate);
			vm.paginationOptions.totalPages = 1;
			vm.loadSchedules();

			vm.toggles.SeeScheduleChangesByOthers && monitorScheduleChanged();
		}

		function getParamsForLoadingSchedules(options) {
			options = options || {};
			var params = {
				Keyword: options.keyword || vm.searchOptions.keyword,
				Date: options.date || moment(vm.startOfWeek).format('YYYY-MM-DD'),
				PageSize: options.pageSize || vm.paginationOptions.pageSize,
				CurrentPageIndex: options.currentPageIndex || vm.paginationOptions.pageNumber
			};
			return params;
		}

		function monitorScheduleChanged() {
			var options = {DomainType: 'IScheduleChangedInDefaultScenario'};
			signalR.subscribeBatchMessage(options, scheduleChangedEventHandler, 300);
		}

		function scheduleChangedEventHandler() {
			$scope.$evalAsync(vm.loadSchedules);
		}

	}
})()