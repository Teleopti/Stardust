(function() {
	'use strict';

	angular.module('wfm.teamSchedule').controller('TeamScheduleWeeklyController', TeamScheduleWeeklyController);

	TeamScheduleWeeklyController.$inject = ['$stateParams', '$q','$locale', '$filter', 'PersonScheduleWeekViewCreator', 'UtilityService', 'weekViewScheduleSvc', 'Toggle', 'TeamSchedule', 'signalRSVC', '$scope'];

	function TeamScheduleWeeklyController(params, $q, $locale, $filter, WeekViewCreator, Util, weekViewScheduleSvc, toggleSvc, teamScheduleSvc, signalR, $scope) {
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
		vm.scheduleFullyLoaded = false;
		vm.agentsPerPageSelection = [20, 50, 100, 500];
		vm.scheduleDate = params.selectedDate || new Date();
		vm.selectedTeamIds = params.selectedTeamIds || [];
		vm.scheduleDateMoment = function () { return moment(vm.scheduleDate); };

		vm.startOfWeek = moment(vm.scheduleDate).startOf('week').toDate();

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
			vm.scheduleFullyLoaded = false;
			vm.loadSchedules();
		};

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
				vm.scheduleFullyLoaded = true;
			}).catch(function() {
				vm.isLoading = false;
			});
		};

		vm.changeSelectedTeams = function(groups) {
			vm.selectedTeamIds = groups;
			params.selectedTeamIds = vm.selectedTeamIds;
			vm.resetSchedulePage();
		};

		vm.init = function() {
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
			vm.weekDays = Util.getWeekdays(vm.scheduleDate);
			vm.paginationOptions.totalPages = 1;

			if (!vm.toggles.DisplayWeekScheduleOnBusinessHierachyEnabled)
				vm.loadSchedules();

			vm.toggles.SeeScheduleChangesByOthers && monitorScheduleChanged();
		};

		$q.all([
			teamScheduleSvc.getAvailableHierarchy(vm.scheduleDateMoment().format("YYYY-MM-DD"))
				.then(function (response) {
					var data = response.data;
					var preSelectedTeamIds = vm.selectedTeamIds.length > 0 ? vm.selectedTeamIds : [data.LogonUserTeamId];

					vm.availableGroups = {
						sites: data.Children,
						preSelectedTeamIds: preSelectedTeamIds
					};
			})
			]).then(vm.init);

		function momentToYYYYMMDD(m) {
			var YYYY = '' + m.year();
			var MM = (m.month() + 1) < 10 ? '0' + (m.month() + 1) : '' + (m.month() + 1);
			var DD = m.date() < 10 ? '0' + m.date() : '' + m.date();
			return YYYY + '-' + MM + '-' + DD;
		}

		function getParamsForLoadingSchedules(options) {
			options = options || {};
			var params = {
				SelectedTeamIds: vm.selectedTeamIds ? vm.selectedTeamIds : [],
				Keyword: options.keyword || vm.searchOptions.keyword,
				Date: options.date || momentToYYYYMMDD(moment(vm.startOfWeek)),
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