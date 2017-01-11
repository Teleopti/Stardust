(function () {
	'use strict';
	angular.module('wfm.teamSchedule').controller('TeamScheduleController', [
		'$scope',
		'$q',
		'$translate',
		'$stateParams',
		'$state',
		'$mdSidenav',
		'TeamSchedule',
		'GroupScheduleFactory',
		'PersonSelection',
		'ScheduleManagement',
		'Toggle',
		'signalRSVC',
		'NoticeService',
		'ValidateRulesService',
		'CommandCheckService',
		'ScheduleNoteManagementService',
		TeamScheduleController]);

	function TeamScheduleController($scope, $q, $translate, $stateParams, $state, $mdSidenav, teamScheduleSvc, groupScheduleFactory, personSelectionSvc, scheduleMgmtSvc, toggleSvc, signalRSVC, NoticeService, ValidateRulesService, CommandCheckService, ScheduleNoteManagementService) {
		var vm = this;

		vm.isLoading = false;
		vm.scheduleFullyLoaded = false;
		vm.scheduleDateMoment = function () { return moment(vm.scheduleDate); };
		vm.availableTimezones = [];
		vm.availableGroups = [];
		vm.currentTimezone;
		vm.availableGroups = [];

		vm.toggleForSelectAgentsPerPageEnabled = false;
		vm.onlyLoadScheduleWithAbsence = false;
		vm.permissionsAndTogglesLoaded = false;
		vm.lastCommandTrackId = '';
		vm.showDatePicker = false;

		vm.initFavoriteSearches = $q.defer();

		vm.onFavoriteSearchesLoaded = function (defaultSearch) {
			vm.initFavoriteSearches.resolve(defaultSearch);
		};

		vm.applyFavorite = function (teamIds, searchTerm) {
			vm.selectedTeamIds = teamIds;
			vm.searchOptions.keyword = searchTerm;
			vm.resetSchedulePage();
		};

		vm.getSearch = function () {
			return {
				teamIds: vm.selectedTeamIds,
				searchTerm: vm.searchOptions.keyword
			};
		};

		vm.paginationOptions = {
			pageSize: 20,
			pageNumber: 1,
			totalPages: 0
		};

		vm.searchOptions = {
			keyword: getDefaultKeyword() || undefined,
			searchKeywordChanged: false
		};

		vm.hasSelectedAllPeopleInEveryPage = false;
		vm.agentsPerPageSelection = [20, 50, 100, 500];

		function getDefaultKeyword() {
			if ($stateParams.keyword)
				return $stateParams.keyword;
		}

		var commandContainerId = 'teamschedule-command-container';
		var settingsContainerId = 'teamschedule-settings-container';

		vm.triggerCommand = function (label, needToOpenSidePanel) {
			closeSettingsSidenav();
			$mdSidenav(commandContainerId).close().then(function () {
				vm.onCommandContainerReady = function () {
					$scope.$applyAsync(function () {
						if (needToOpenSidePanel)
							openSidePanel();
					});
					return true;
				};

				$scope.$broadcast('teamSchedule.init.command', {
					activeCmd: label
				});
			});
		};

		vm.openSettingsPanel = function () {
			closeAllCommandSidenav();
			$mdSidenav(settingsContainerId).toggle();
		};

		vm.commonCommandCallback = function (trackId, personIds) {
			$mdSidenav(commandContainerId).isOpen() && $mdSidenav(commandContainerId).close();

			vm.lastCommandTrackId = trackId != null ? trackId : null;
			personIds && vm.updateSchedules(personIds);
			vm.checkValidationWarningForCommandTargets(personIds);
		};

		$scope.$on('teamSchedule.trigger.command', function (e, d) {
			vm.triggerCommand(d.activeCmd, d.needToOpenSidePanel);
		});

		function openSidePanel() {
			if (!$mdSidenav(commandContainerId).isOpen()) {
				$mdSidenav(commandContainerId).open().then(function () {
					$scope.$broadcast('teamSchedule.command.focus.default');
				});
			}

			var unbindWatchPanel = $scope.$watch(function () {
				return $mdSidenav(commandContainerId).isOpen();
			}, function (newValue, oldValue) {
				if (newValue !== oldValue && !newValue) {
					$scope.$broadcast('teamSchedule.reset.command');
					CommandCheckService.getCommandCheckStatus() && CommandCheckService.resetCommandCheckStatus();
					unbindWatchPanel();
				}
			});
		}

		function closeAllCommandSidenav() {
			$mdSidenav(commandContainerId).isOpen() && $mdSidenav(commandContainerId).close();
		}

		function closeSettingsSidenav() {
			$mdSidenav(settingsContainerId).isOpen() && $mdSidenav(settingsContainerId).close();
		}

		vm.onPageSizeSelectorChange = function () {
			teamScheduleSvc.updateAgentsPerPageSetting.post({ agents: vm.paginationOptions.pageSize }).$promise.then(function () {
				vm.resetSchedulePage();
			});
		};

		function updateShiftStatusForSelectedPerson() {
			var selectedPersonIdList = personSelectionSvc.getSelectedPersonIdList();
			if (selectedPersonIdList.length === 0) {
				return;
			}

			var currentDate = vm.scheduleDateMoment().format('YYYY-MM-DD');

			teamScheduleSvc.getSchedules(currentDate, selectedPersonIdList).then(function (result) {
				scheduleMgmtSvc.resetSchedules(result.Schedules, vm.scheduleDateMoment());
				personSelectionSvc.updatePersonInfo(scheduleMgmtSvc.groupScheduleVm.Schedules, currentDate);
			});
		}

		vm.onScheduleDateChanged = function () {
			vm.isLoading = true;
			personSelectionSvc.clearPersonInfo();
			vm.resetSchedulePage();
			updateShiftStatusForSelectedPerson();
		};

		vm.onKeyWordInSearchInputChanged = function () {
			if (vm.searchOptions.searchKeywordChanged) {
				personSelectionSvc.clearPersonInfo();
			}
			vm.resetSchedulePage();
		};

		vm.resetSchedulePage = function () {
			vm.paginationOptions.pageNumber = 1;
			vm.hasSelectedAllPeopleInEveryPage = false;
			vm.scheduleFullyLoaded = false;
			vm.loadSchedules();
		};

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
				Date: options.date || momentToYYYYMMDD(vm.scheduleDateMoment()),
				PageSize: options.pageSize || vm.paginationOptions.pageSize,
				CurrentPageIndex: options.currentPageIndex || vm.paginationOptions.pageNumber,
				IsOnlyAbsences: vm.onlyLoadScheduleWithAbsence

			};
			return params;
		}

		function afterSchedulesLoaded(result) {
			vm.paginationOptions.totalPages = result.Schedules.length > 0 ? Math.ceil(result.Total / vm.paginationOptions.pageSize) : 0;
			vm.scheduleCount = scheduleMgmtSvc.groupScheduleVm.Schedules.length;
			vm.total = result.Total;
			vm.searchOptions.searchKeywordChanged = false;
			vm.searchOptions.keyword = result.Keyword;
			vm.searchOptions.searchFields = [
				'FirstName', 'LastName', 'EmploymentNumber', 'Organization', 'Role', 'Contract', 'ContractSchedule', 'ShiftBags',
				'PartTimePercentage', 'Skill', 'BudgetGroup', 'Note'
			];
			vm.scheduleFullyLoaded = true;
		}

		function populateAvailableTimezones(schedules) {
			vm.availableTimezones = schedules.Schedules.map(function (s) {
				return s.Timezone;
			});
		}

		vm.changeTimezone = function (timezone) {
			vm.currentTimezone = timezone;
			scheduleMgmtSvc.recreateScheduleVm(vm.scheduleDateMoment(), timezone);
			personSelectionSvc.updatePersonInfo(scheduleMgmtSvc.groupScheduleVm.Schedules);
		};

		vm.changeSelectedTeams = function (teams) {
			vm.selectedTeamIds = teams;
			$stateParams.selectedTeamIds = vm.selectedTeamIds;
			vm.resetSchedulePage();
		};

		vm.loadSchedules = function () {
			vm.isLoading = true;
			var preSelectPersonIds = $stateParams.personId ? [$stateParams.personId] : [];
			if (vm.searchEnabled) {
				var params = getParamsForLoadingSchedules();

				teamScheduleSvc.searchSchedules(params).then(function (response) {
					var result = response.data;
					scheduleMgmtSvc.resetSchedules(result.Schedules, vm.scheduleDateMoment(), vm.currentTimezone);
					ScheduleNoteManagementService.resetScheduleNotes(result.Schedules, vm.scheduleDateMoment());
					afterSchedulesLoaded(result);
					personSelectionSvc.updatePersonInfo(scheduleMgmtSvc.groupScheduleVm.Schedules);
					vm.isLoading = false;
					vm.checkValidationWarningForCurrentPage();
					populateAvailableTimezones(result);
				});
			} else if (preSelectPersonIds.length > 0) {
				var date = vm.scheduleDateMoment().format('YYYY-MM-DD');

				teamScheduleSvc.getSchedules(date, preSelectPersonIds).then(function (result) {
					scheduleMgmtSvc.resetSchedules(result.Schedules, vm.scheduleDateMoment(), vm.currentTimezone);
					ScheduleNoteManagementService.resetScheduleNotes(result.Schedules, vm.scheduleDateMoment());
					afterSchedulesLoaded(result);
					personSelectionSvc.updatePersonInfo(scheduleMgmtSvc.groupScheduleVm.Schedules);
					personSelectionSvc.preSelectPeople(preSelectPersonIds, scheduleMgmtSvc.groupScheduleVm.Schedules, vm.scheduleDate);

					vm.checkValidationWarningForCurrentPage();
					populateAvailableTimezones(result);
					vm.isLoading = false;
				});
			}
		};

		vm.toggleShowAbsenceOnly = function () {
			vm.paginationOptions.pageNumber = 1;
			vm.loadSchedules();
		};

		vm.checkValidationWarningForCurrentPage = function () {
			if (vm.cmdConfigurations.validateWarningToggle) {
				var currentPagePersonIds = scheduleMgmtSvc.groupScheduleVm.Schedules.map(function (schedule) {
					return schedule.PersonId;
				});
				ValidateRulesService.getValidateRulesResultForCurrentPage(vm.scheduleDateMoment(), currentPagePersonIds);
			}
		};

		vm.checkValidationWarningForCommandTargets = function (personIds) {
			if (vm.cmdConfigurations.validateWarningToggle) {
				ValidateRulesService.updateValidateRulesResultForPeople(vm.scheduleDateMoment(), personIds);
			}
		};

		vm.updateSchedules = function (personIdList) {
			vm.isLoading = true;
			scheduleMgmtSvc.updateScheduleForPeoples(personIdList, vm.scheduleDateMoment(), vm.currentTimezone, function () {
				personSelectionSvc.clearPersonInfo();
				vm.isLoading = false;
				vm.hasSelectedAllPeopleInEveryPage = false;
			});
		};

		vm.selectAllForAllPages = function () {
			var params = getParamsForLoadingSchedules({ currentPageIndex: 1, pageSize: vm.total });
			teamScheduleSvc.searchSchedules(params).then(function (result) {

				scheduleMgmtSvc.resetSchedules(result.data.Schedules, vm.scheduleDateMoment(), vm.currentTimezone);
				personSelectionSvc.selectAllPerson(scheduleMgmtSvc.groupScheduleVm.Schedules);
				personSelectionSvc.updatePersonInfo(scheduleMgmtSvc.groupScheduleVm.Schedules);
				vm.hasSelectedAllPeopleInEveryPage = true;
			});
		};

		vm.unselectAllForAllPages = function () {
			var params = getParamsForLoadingSchedules({ currentPageIndex: 1, pageSize: vm.total });
			teamScheduleSvc.searchSchedules(params).then(function (result) {
				scheduleMgmtSvc.resetSchedules(result.data.Schedules, vm.scheduleDateMoment(), vm.currentTimezone);
				personSelectionSvc.unselectAllPerson(scheduleMgmtSvc.groupScheduleVm.Schedules);
				vm.hasSelectedAllPeopleInEveryPage = false;
			});
		};

		vm.selectAllVisible = function () {
			var selectedPersonIdList = personSelectionSvc.getSelectedPersonIdList();
			return vm.paginationOptions.totalPages > 1 && selectedPersonIdList.length < vm.total;
		};

		vm.hasSelectedAllPeople = function () {
			return vm.hasSelectedAllPeopleInEveryPage;
		};

		vm.toggleErrorDetails = function () {
			vm.showErrorDetails = !vm.showErrorDetails;
		};

		function isMessageNeedToBeHandled() {
			var personIds = scheduleMgmtSvc.groupScheduleVm.Schedules.map(function (schedule) { return schedule.PersonId; });
			var scheduleDate = vm.scheduleDateMoment();
			var viewRangeStart = scheduleDate.clone().add(-1, 'day').startOf('day');
			var viewRangeEnd = scheduleDate.clone().add(1, 'day').startOf('day');

			return function (message) {
				if (message.TrackId === vm.lastCommandTrackId) { return false; }

				var isMessageInsidePeopleList = personIds.indexOf(message.DomainReferenceId) > -1;
				var startDate = moment(message.StartDate.substring(1, message.StartDate.length));
				var endDate = moment(message.EndDate.substring(1, message.EndDate.length));
				var isScheduleDateInMessageRange = vm.toggles.ManageScheduleForDistantTimezonesEnabled
					? startDate.isBetween(viewRangeStart, viewRangeEnd, 'day', '[]') || endDate.isBetween(viewRangeStart, viewRangeEnd, 'day', '[]')
					: viewRangeStart.isSameOrBefore(endDate) && viewRangeEnd.isSameOrAfter(startDate);;

				return isMessageInsidePeopleList && isScheduleDateInMessageRange;
			}
		}

		function scheduleChangedEventHandler(messages) {
			var personIds = messages.filter(isMessageNeedToBeHandled()).map(function (message) {
				return message.DomainReferenceId;
			});

			var uniquePersonIds = [];
			personIds.forEach(function (personId) {
				if (uniquePersonIds.indexOf(personId) === -1) uniquePersonIds.push(personId);
			});

			if (uniquePersonIds.length !== 0) {
				vm.updateSchedules(uniquePersonIds);
				vm.checkValidationWarningForCommandTargets(uniquePersonIds);
			}
		}

		function monitorScheduleChanged() {
			signalRSVC.subscribeBatchMessage(
				{ DomainType: 'IScheduleChangedInDefaultScenario' }
				, scheduleChangedEventHandler
				, 300);
		}

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
			EditAndViewInternalNoteEnabled: toggleSvc.WfmTeamSchedule_EditAndDisplayInternalNotes_40671,

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
			ManageScheduleForDistantTimezonesEnabled: toggleSvc.WfmTeamSchedule_ShowShiftsForAgentsInDistantTimeZones_41305,

			MoveToBaseLicenseEnabled: toggleSvc.WfmTeamSchedule_MoveToBaseLicense_41039,
			SaveFavoriteSearchesEnabled: toggleSvc.WfmTeamSchedule_SaveFavoriteSearches_42073
		};

		vm.scheduleDate = $stateParams.selectedDate || new Date();
		vm.selectedTeamIds = $stateParams.selectedTeamIds || [];
		vm.searchOptions.keyword = $stateParams.keyword || '';

		var asyncData = {
			permissions: teamScheduleSvc.PromiseForloadedPermissions(),
			pageSetting: teamScheduleSvc.PromiseForGetAgentsPerPageSetting(),
			hierarchy: teamScheduleSvc.getAvailableHierarchy(vm.scheduleDateMoment().format('YYYY-MM-DD')),
		};

		vm.searchEnabled = $state.current.name != 'teams.for';
		if (vm.searchEnabled && vm.toggles.SaveFavoriteSearchesEnabled) {
			asyncData.defaultFavoriteSearch = vm.initFavoriteSearches.promise;
		}

		$q.all(asyncData).then(function init(data) {
			if (data.pageSetting.Agents > 0)
				vm.paginationOptions.pageSize = data.pageSetting.Agents;

			var defaultFavoriteSearch = data.defaultFavoriteSearch;
			var hierarchy = data.hierarchy.data;

			if (!$stateParams.do && defaultFavoriteSearch) {
				vm.selectedTeamIds = defaultFavoriteSearch.TeamIds;
				vm.searchOptions.keyword = defaultFavoriteSearch.SearchTerm;
			} else if (!$stateParams.do && hierarchy.LogonUserTeamId) {
				vm.selectedTeamIds = [hierarchy.LogonUserTeamId];
			}

			vm.availableGroups = {
				sites: hierarchy.Children,
			};
			vm.scheduleDate = vm.scheduleDate || new Date();
			vm.searchOptions.keyword = vm.searchOptions.keyword || '';

			vm.toggles.SeeScheduleChangesByOthers && monitorScheduleChanged();

			vm.permissions = data.permissions;
			vm.permissionsAndTogglesLoaded = true;
			vm.cmdConfigurations = {
				toggles: vm.toggles,
				permissions: vm.permissions,
				validateWarningToggle: false,
				currentCommandName: null
			};
			vm.scheduleTableSelectMode = vm.toggles.AbsenceReportingEnabled
				|| vm.toggles.AddActivityEnabled
				|| vm.toggles.RemoveActivityEnabled
				|| vm.toggles.RemoveAbsenceEnabled
				|| vm.toggles.SwapShiftEnabled
				|| vm.toggles.ModifyShiftCategoryEnabled;

			if ((!vm.toggles.DisplayScheduleOnBusinessHierachyEnabled && !vm.toggles.SaveFavoriteSearchesEnabled) || !vm.searchEnabled) {
				vm.resetSchedulePage();
			}

			showReleaseNotification();
		});

		function showReleaseNotification() {
			var template = $translate.instant('WFMReleaseNotification');
			var moduleName = $translate.instant('Teams');
			var message = template.replace('{0}', moduleName)
				.replace('{1}', '<a href="http://www.teleopti.com/wfm/customer-feedback.aspx">')
				.replace('{2}', '</a>')
				.replace('{3}', '<a href="../Anywhere#teamschedule">' + $translate.instant('TeamSchedule') + '</a>');
			NoticeService.info(message, null, true);
		}
	}
} ());
