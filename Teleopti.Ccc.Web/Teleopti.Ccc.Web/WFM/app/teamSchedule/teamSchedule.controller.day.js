(function () {
	'use strict';
	angular.module('wfm.teamSchedule').controller('TeamScheduleController', [
		'$scope',
		'$q',
		'$timeout',
		'$translate',
		'$state',
		'$mdSidenav',
		'$stateParams',
		'$mdComponentRegistry',
		'$document',
		'TeamSchedule',
		'PersonSelection',
		'ScheduleManagement',
		'ValidateRulesService',
		'CommandCheckService',
		'ScheduleNoteManagementService',
		'bootstrapCommon',
		'groupPageService',
		'TeamsStaffingConfigurationStorageService',
		'serviceDateFormatHelper',
		'ViewStateKeeper',
		'teamsPermissions',
		'UtilityService',
		'Toggle',
		TeamScheduleController]);

	function TeamScheduleController($scope, $q, $timeout, $translate, $state, $mdSidenav, $stateParams, $mdComponentRegistry, $document,
		teamScheduleSvc, personSelectionSvc, scheduleMgmtSvc, ValidateRulesService,
		CommandCheckService, ScheduleNoteManagementService, bootstrapCommon, groupPageService,
		StaffingConfigStorageService, serviceDateFormatHelper, ViewStateKeeper, teamsPermissions, Utility, toggleSvc) {
		var mode = {
			BusinessHierarchy: 'BusinessHierarchy',
			GroupPages: 'GroupPages'
		}

		var vm = this;
		var viewState = ViewStateKeeper.get();
		var personIdsHavingScheduleChange = {};

		vm.isLoading = false;
		vm.scheduleFullyLoaded = false;

		vm.preSelectPersonIds = $stateParams.personId ? [$stateParams.personId] : [];

		vm.currentTimezone = viewState.timezone;
		vm.scheduleDate = $stateParams.selectedDate || viewState.selectedDate || Utility.nowDateInUserTimezone();
		vm.avaliableTimezones = [];
		vm.sitesAndTeams = undefined;
		vm.staffingEnabled = viewState.staffingEnabled;
		vm.permissions = {};
		vm.isRefreshButtonVisible = !!toggleSvc.WfmTeamSchedule_DisableAutoRefreshSchedule_79826;
		vm.havingScheduleChanged = false;


		initSelectedGroups(mode.BusinessHierarchy, [], '');
		if (angular.isArray(viewState.selectedTeamIds) && viewState.selectedTeamIds.length > 0) {
			initSelectedGroups(mode.BusinessHierarchy, viewState.selectedTeamIds.slice(0), '');
		}
		else if (viewState.selectedGroupPage && viewState.selectedGroupPage.groupIds.length > 0) {
			initSelectedGroups(mode.GroupPages, viewState.selectedGroupPage.groupIds.slice(0), viewState.selectedGroupPage.pageId);
		}

		vm.lastCommandTrackId = '';
		vm.currentSettings = {};

		vm.getSearch = function () {
			return {
				TeamIds: vm.selectedGroups.mode === 'BusinessHierarchy' ? vm.selectedGroups.groupIds : [],
				SearchTerm: vm.searchOptions.keyword
			};
		};
		vm.showStaffing = function () {
			$timeout(function () {
				initTeamSize();
			});
			if (vm.staffingEnabled) {
				vm.preselectedSkills = {};
				var preference = StaffingConfigStorageService.getConfig();
				if (preference) {
					vm.preselectedSkills.skillIds = !!preference.skillId ? [preference.skillId] : undefined;
					vm.preselectedSkills.skillAreaId = preference.skillGroupId;
					vm.useShrinkage = preference.useShrinkage;
				}
			}
		}

		function getTotalTableRowHeight() {
			var rows = $document[0].querySelectorAll('.big-table-wrapper table tr');
			var sum = 0;
			angular.forEach(rows,
				function (r) {
					sum += r.offsetHeight;
				});
			return sum;
		}

		function initTeamSize() {
			var container = $document[0].querySelector('#materialcontainer');
			if (!container) return;
			var viewHeader = $document[0].querySelector('.view-header');
			var header = $document[0].querySelector('.team-schedule .teamschedule-header');
			var tHeader = $document[0].querySelector('.teamschedule-body .big-table-wrapper table thead');
			var footer = $document[0].querySelector('.teamschedule-footer');
			var tHeaderHeight = tHeader ? tHeader.offsetHeight : 0;

			var maxDefaultHeight = container.offsetHeight - viewHeader.offsetHeight - header.offsetHeight - footer.offsetHeight;
			var totalRowHeight = getTotalTableRowHeight();

			var defaultHeight = totalRowHeight > maxDefaultHeight ? maxDefaultHeight : totalRowHeight;
			var defaultTableBodyHeight = defaultHeight - tHeaderHeight;

			var storageSize = StaffingConfigStorageService.getConfig();
			var size = storageSize || {
				tableHeight: defaultHeight * 0.64,
				tableBodyHeight: defaultTableBodyHeight * 0.62,
				chartHeight: defaultHeight * 0.3 - 40
			};
			if (vm.staffingEnabled) {
				vm.scheduleTableWrapperStyle = {
					'height': size.tableHeight + 'px',
					'min-height': '0'
				};
				vm.scheduleTableBodyStyle = {
					'max-height': size.tableBodyHeight + 'px',
					'min-height': '0'
				};
				vm.chartHeight = size.chartHeight;
			}
			else {
				vm.scheduleTableWrapperStyle = { 'height': defaultHeight + 'px' };
				vm.scheduleTableBodyStyle = { 'max-height': defaultTableBodyHeight + 'px' };
			}
		};

		vm.paginationOptions = {
			pageSize: 20,
			pageNumber: 1,
			totalPages: 0
		};

		vm.hasSelectedAllPeopleInEveryPage = false;
		vm.agentsPerPageSelection = [20, 50, 100, 500];


		var commandContainerId = 'teamschedule-command-container';
		var settingsContainerId = 'teamschedule-settings-container';

		vm.triggerCommand = function (command, needToOpenSidePanel) {
			closeSettingsSidenav();
			if ($mdSidenav(commandContainerId).isOpen()) {
				$mdSidenav(commandContainerId).close().then(function () {
					initCommand(command, needToOpenSidePanel);
				});
			} else {
				initCommand(command, needToOpenSidePanel);
			}
		};

		function initCommand(command, needToOpenSidePanel) {
			vm.onCommandContainerReady = function () {
				$scope.$applyAsync(function () {
					if (needToOpenSidePanel)
						openSidePanel();
				});
				return true;
			};

			$scope.$broadcast('teamSchedule.init.command', {
				activeCmd: command
			});
		}

		$scope.$on('teamSchedule.show.loading',
			function () {
				vm.isLoading = true;
			});
		$scope.$on('teamSchedule.hide.loading',
			function () {
				vm.isLoading = false;
			});

		$scope.$on('teamSchedule.update.sortOption',
			function (e, d) {
				vm.sortOption = d.option;
				vm.loadSchedules();
			});

		$scope.$on('teamSchedule.trigger.command', function (e, d) {
			vm.triggerCommand(d.activeCmd, d.needToOpenSidePanel);
		});

		$scope.$on('teamSchedule.setting.changed', function (e, d) {
			vm.currentSettings = angular.copy(d.settings);
			switch (d.changedKey) {
				case 'validateWarningEnabled':
					vm.checkValidationWarningForCurrentPage();
					break;
				case 'onlyLoadScheduleWithAbsence':
					vm.toggleShowAbsenceOnly();
					break;
			}
		});

		$scope.$on('teamSchedule.sidenav.hide', function () {
			$scope.$evalAsync($mdSidenav(commandContainerId).close);
		});

		$scope.$on('angular-resizable.resizeEnd', function (e, d) {
			var container = $document[0].querySelector('#materialcontainer');
			var viewHeader = $document[0].querySelector('.view-header');
			var header = $document[0].querySelector('.team-schedule .teamschedule-header');
			var tHeader = $document[0].querySelector('.teamschedule-body .big-table-wrapper table thead');
			var footer = $document[0].querySelector('.teamschedule-footer');
			var tHeaderHeight = tHeader ? tHeader.offsetHeight : 0;
			var tableHeight = d.height - footer.offsetHeight;
			var tBodyHeight = tableHeight - tHeaderHeight;
			var staffingHeaderHeight = $document[0].querySelector('.staffing-header').offsetHeight;
			var containerHeight = container.offsetHeight;

			var totalHeightForScheduleTableAndChart = containerHeight
				- viewHeader.offsetHeight
				- header.offsetHeight
				- staffingHeaderHeight
				- getSkillsRowHeight()
				- 30;

			var chartHeight = totalHeightForScheduleTableAndChart - d.height;

			if (tableHeight <= 100) {
				tableHeight = 100;
				tBodyHeight = 100 - tHeaderHeight;
				chartHeight = totalHeightForScheduleTableAndChart - tableHeight - footer.offsetHeight;
			}
			if (chartHeight <= 150) {
				chartHeight = 150;
				tableHeight = totalHeightForScheduleTableAndChart - chartHeight - footer.offsetHeight;
				tBodyHeight = tableHeight - tHeaderHeight;
			}
			StaffingConfigStorageService.setSize(tableHeight, tBodyHeight, chartHeight);
			vm.scheduleTableWrapperStyle = {
				'height': tableHeight + 'px',
				'min-height': '0'
			};
			vm.scheduleTableBodyStyle = {
				'max-height': tBodyHeight + 'px',
				'min-height': '0'
			}
			vm.chartHeight = chartHeight;

		});

		function getSkillsRowHeight() {
			var skillsRow = $document[0].querySelector('.skills-row-wrapper');
			return skillsRow ? skillsRow.offsetHeight : 0;
		}

		vm.teamNameMap = viewState.teamNameMap || {};
		vm.searchOptions = {
			keyword: viewState.keyword || '',
			searchKeywordChanged: false,
			focusingSearch: false
		};
		vm.selectedFavorite = viewState.do ? viewState.selectedFavorite : null;
		vm.sortOption = viewState.selectedSortOption;

		vm.openSettingsPanel = function () {
			closeAllCommandSidenav();
			$mdSidenav(settingsContainerId).toggle();
		};

		vm.isExportScheduleEnabled = function () {
			return vm.permissions.HasExportSchedulePermission;
		}

		vm.openExportPanel = function () {
			$state.go('teams.exportSchedule');
		}

		vm.commonCommandCallback = function (trackId, personIds) {
			$mdSidenav(commandContainerId).isOpen() && $mdSidenav(commandContainerId).close();

			vm.lastCommandTrackId = trackId != null ? trackId : null;
			if (personIds) {
				vm.updateSchedules(personIds);
				resetHavingScheduleChange(personIds);
				vm.checkValidationWarningForCommandTargets(personIds);
				if (vm.staffingEnabled) {
					$scope.$broadcast('teamSchedule.command.scheduleChangedApplied');
				}
			}
		};

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
			$mdComponentRegistry.get(commandContainerId) && $mdSidenav(commandContainerId).isOpen() && $mdSidenav(commandContainerId).close();
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

			var currentDate = serviceDateFormatHelper.getDateOnly(vm.scheduleDate);

			teamScheduleSvc.getSchedules(currentDate, selectedPersonIdList).then(function (result) {
				scheduleMgmtSvc.resetSchedules(result.Schedules, currentDate, vm.currentTimezone);
				personSelectionSvc.updatePersonInfo(scheduleMgmtSvc.groupScheduleVm.Schedules, currentDate);
			});
		}

		vm.onScheduleDateChanged = function () {
			vm.isLoading = true;
			personSelectionSvc.unselectAllPerson(scheduleMgmtSvc.groupScheduleVm.Schedules);
			personSelectionSvc.clearPersonInfo();
			vm.resetSchedulePage();
			updateShiftStatusForSelectedPerson();
			vm.getGroupPagesAsync();
		};

		vm.onKeyWordInSearchInputChanged = function () {
			if (vm.searchOptions.searchKeywordChanged) {
				personSelectionSvc.unselectAllPerson(scheduleMgmtSvc.groupScheduleVm.Schedules);
			}
			personSelectionSvc.clearPersonInfo();
			vm.selectedFavorite = false;
			vm.resetSchedulePage();
		};

		vm.resetSchedulePage = function () {
			vm.paginationOptions.pageNumber = 1;
			vm.hasSelectedAllPeopleInEveryPage = false;
			vm.scheduleFullyLoaded = false;
			vm.loadSchedules();
			resetFocus();
		};

		function resetFocus() {
			$scope.$broadcast("resetFocus");
		};

		function getParamsForLoadingSchedules(options) {
			options = options || {};
			var params = {
				Keyword: options.keyword || vm.searchOptions.keyword,
				Date: options.date || serviceDateFormatHelper.getDateOnly(vm.scheduleDate),
				PageSize: options.pageSize || vm.paginationOptions.pageSize,
				CurrentPageIndex: options.currentPageIndex || vm.paginationOptions.pageNumber,
				IsOnlyAbsences: vm.currentSettings.onlyLoadScheduleWithAbsence,
				SortOption: vm.sortOption,
				SelectedGroupIds: vm.selectedGroups.groupIds,
				SelectedGroupPageId: vm.selectedGroups.groupPageId
			};
			return params;
		}

		vm.getLoadAllSchedulesParams = function () {
			return getParamsForLoadingSchedules({
				currentPageIndex: 1,
				pageSize: vm.total
			});
		};

		function afterSchedulesLoaded(result) {
			vm.paginationOptions.totalPages = result.Schedules.length > 0 ? Math.ceil(result.Total / vm.paginationOptions.pageSize) : 0;
			vm.scheduleCount = scheduleMgmtSvc.groupScheduleVm.Schedules.length;
			vm.total = result.Total;
			vm.searchOptions.searchKeywordChanged = false;
			vm.searchOptions.searchFields = [
				'FirstName', 'LastName', 'EmploymentNumber', 'Organization', 'Role', 'Contract', 'ContractSchedule', 'ShiftBag',
				'PartTimePercentage', 'Skill', 'BudgetGroup', 'Note'
			];
			vm.scheduleFullyLoaded = true;
		}

		function populateAvailableTimezones(schedules) {
			var availableTimezones = schedules.Schedules.map(function (s) {
				return s.Timezone.IanaId;
			});
			vm.avaliableTimezones = availableTimezones;
		}

		vm.changeTimezone = function () {
			scheduleMgmtSvc.recreateScheduleVm(serviceDateFormatHelper.getDateOnly(vm.scheduleDate), vm.currentTimezone);
			personSelectionSvc.updatePersonInfo(scheduleMgmtSvc.schedules());
		};

		vm.loadSchedules = function () {
			closeAllCommandSidenav();
			resetHavingScheduleChange();

			vm.isLoading = true;
			var date = serviceDateFormatHelper.getDateOnly(vm.scheduleDate);
			if (vm.searchEnabled) {
				var params = getParamsForLoadingSchedules();

				teamScheduleSvc.searchSchedules(params).then(function (response) {
					var result = response.data;
					scheduleMgmtSvc.resetSchedules(result.Schedules, date, vm.currentTimezone);
					ScheduleNoteManagementService.resetScheduleNotes(result.Schedules, date);
					afterSchedulesLoaded(result);

					if (vm.hasSelectedAllPeopleInEveryPage) {
						personSelectionSvc.selectAllPerson(scheduleMgmtSvc.groupScheduleVm.Schedules);
					}
					personSelectionSvc.updatePersonInfo(scheduleMgmtSvc.groupScheduleVm.Schedules);

					vm.checkValidationWarningForCurrentPage();
					populateAvailableTimezones(result);
					vm.isLoading = false;
					vm.searchOptions.focusingSearch = false;
				});
			} else if (vm.preSelectPersonIds.length > 0) {

				teamScheduleSvc.getSchedules(date, vm.preSelectPersonIds).then(function (result) {
					scheduleMgmtSvc.resetSchedules(result.Schedules, date, vm.currentTimezone);
					ScheduleNoteManagementService.resetScheduleNotes(result.Schedules, date);
					afterSchedulesLoaded(result);

					if (vm.hasSelectedAllPeopleInEveryPage) {
						personSelectionSvc.selectAllPerson(scheduleMgmtSvc.groupScheduleVm.Schedules);
					}
					personSelectionSvc.updatePersonInfo(scheduleMgmtSvc.groupScheduleVm.Schedules);

					personSelectionSvc.preSelectPeople(vm.preSelectPersonIds, scheduleMgmtSvc.groupScheduleVm.Schedules, vm.scheduleDate);

					vm.checkValidationWarningForCurrentPage();
					populateAvailableTimezones(result);
					vm.isLoading = false;
					vm.searchOptions.focusingSearch = false;
				});
			}
		};

		vm.toggleShowAbsenceOnly = function () {
			vm.paginationOptions.pageNumber = 1;
			vm.loadSchedules();
		};

		vm.checkValidationWarningForCurrentPage = function () {
			if (vm.currentSettings.validateWarningEnabled) {
				var currentPagePersonIds = scheduleMgmtSvc.groupScheduleVm.Schedules.map(function (schedule) {
					return schedule.PersonId;
				});
				ValidateRulesService.getValidateRulesResultForCurrentPage(serviceDateFormatHelper.getDateOnly(vm.scheduleDate), currentPagePersonIds);
			}
		};

		vm.checkValidationWarningForCommandTargets = function (personIds) {
			if (vm.currentSettings.validateWarningEnabled) {
				ValidateRulesService.updateValidateRulesResultForPeople(serviceDateFormatHelper.getDateOnly(vm.scheduleDate), personIds);
			}
		};

		vm.updateSchedules = function (personIdList) {
			vm.isLoading = true;
			scheduleMgmtSvc.updateScheduleForPeoples(personIdList, serviceDateFormatHelper.getDateOnly(vm.scheduleDate), vm.currentTimezone, function () {
				personSelectionSvc.clearPersonInfo();
				vm.isLoading = false;
				vm.hasSelectedAllPeopleInEveryPage = false;
			});
		};

		vm.selectAllForAllPages = function () {
			personSelectionSvc.selectAllPerson(scheduleMgmtSvc.groupScheduleVm.Schedules);
			personSelectionSvc.updatePersonInfo(scheduleMgmtSvc.groupScheduleVm.Schedules);
			vm.hasSelectedAllPeopleInEveryPage = true;
		};

		vm.unselectAllForAllPages = function () {
			personSelectionSvc.unselectAllPerson(scheduleMgmtSvc.groupScheduleVm.Schedules);
			vm.hasSelectedAllPeopleInEveryPage = false;
		};

		vm.commandPanelClosed = function () {
			return !$mdSidenav(commandContainerId).isOpen();
		};

		vm.selectAllVisible = function () {
			var selectedPersonIdList = personSelectionSvc.getSelectedPersonIdList();
			return (vm.paginationOptions.totalPages > 1 && selectedPersonIdList.length < vm.total)
				&& !vm.hasSelectedAllPeopleInEveryPage;
		};

		vm.hasSelectedAllPeople = function () {
			return vm.hasSelectedAllPeopleInEveryPage;
		};

		vm.toggleErrorDetails = function () {
			vm.showErrorDetails = !vm.showErrorDetails;
		};

		vm.onPersonScheduleChanged = function (personIds) {
			if (!toggleSvc.WfmTeamSchedule_DisableAutoRefreshSchedule_79826) {
				vm.updateSchedules(personIds);
				vm.checkValidationWarningForCommandTargets(personIds);
			} else {
				angular.forEach(personIds, function (personId) {
					personIdsHavingScheduleChange[personId] = personId;
				});
				vm.havingScheduleChanged = true;
			}
		};

		vm.onRefreshButtonClicked = function () {
			var personIds = Object.keys(personIdsHavingScheduleChange);
			vm.updateSchedules(personIds);
			vm.checkValidationWarningForCommandTargets(personIds);
			resetHavingScheduleChange();
			if (vm.staffingEnabled) {
				$scope.$broadcast('teamSchedule.command.scheduleChangedApplied');
			}
		}

		function resetHavingScheduleChange(personIds) {
			if (personIds) {
				personIds.forEach(function (personId) {
					if (personIdsHavingScheduleChange[personId])
						delete personIdsHavingScheduleChange[personId];
				});
			} else {
				personIdsHavingScheduleChange = {};
			}
			vm.havingScheduleChanged = !!Object.keys(personIdsHavingScheduleChange).length;
		}

		vm.searchOptions = {
			keyword: viewState.keyword || '',
			searchKeywordChanged: false,
			focusingSearch: false
		};
		vm.selectedFavorite = viewState.do ? viewState.selectedFavorite : null;

		vm.validateWarningEnabled = false;

		vm.scheduleTableSelectMode = true;
		vm.onUseShrinkageChanged = function (useShrinkage) {
			StaffingConfigStorageService.setShrinkage(useShrinkage);
		};

		vm.searchEnabled = $state.current.name !== 'teams.for';
		vm.onSelectedSkillChanged = function (skill, skillGroup) {
			StaffingConfigStorageService.setSkill((skill || {}).Id, (skillGroup || {}).Id);
		};

		vm.onSelectedTeamsChanged = function () {
			personSelectionSvc.unselectAllPerson(scheduleMgmtSvc.groupScheduleVm.Schedules);
			personSelectionSvc.clearPersonInfo();
			vm.searchOptions.focusingSearch = true;
			vm.selectedFavorite = false;
		};

		vm.applyFavorite = function (currentFavorite) {
			vm.selectedFavorite = currentFavorite;
			initSelectedGroups(mode.BusinessHierarchy, currentFavorite.TeamIds.slice(0), '');
			vm.searchOptions.keyword = currentFavorite.SearchTerm;
			vm.resetSchedulePage();
		};

		vm.hideSearchIfNoSelectedTeam = function () {
			if (angular.isArray(vm.selectedGroups.groupIds) && vm.selectedGroups.groupIds.length > 0) {
				return 'visible';
			}
			return 'hidden';
		};

		vm.orgPickerSelectedText = function () {
			switch (vm.selectedGroups.groupIds.length) {
				case 0:
					return $translate.instant('SelectOrganization');
				case 1:
					return vm.teamNameMap[vm.selectedGroups.groupIds[0]];
				default:
					return $translate.instant('SeveralTeamsSelected').replace('{0}', vm.selectedGroups.groupIds.length);
			}
		};

		vm.boostrap = bootstrapCommon.ready();
		var loggedonUsersTeamId = $q.defer();
		vm.onFavoriteSearchInitDefer = $q.defer();

		var asyncData = {
			pageSetting: teamScheduleSvc.PromiseForGetAgentsPerPageSetting(),
			loggedonUsersTeamId: loggedonUsersTeamId.promise,
			defaultFavoriteSearch: vm.onFavoriteSearchInitDefer.promise,
			bootstrapReady: bootstrapCommon.ready()
		};

		if (!vm.searchEnabled) {
			loggedonUsersTeamId.resolve(null);
			vm.onFavoriteSearchInitDefer.resolve();
		}
		vm.getGroupPagesAsync = function () {
			var dateStr = serviceDateFormatHelper.getDateOnly(vm.scheduleDate);
			groupPageService.fetchAvailableGroupPages(dateStr, dateStr).then(function (data) {
				vm.availableGroups = data;
				loggedonUsersTeamId.resolve(data.LogonUserTeamId || null);
			});
		};

		vm.isResultTooMany = function () {
			return vm.total > 500;
		}
		vm.warningMessageForTooManyResults = function () {
			return $translate.instant('TooManyResultsForSearchKeywords').replace('{0}', 500);
		}

		vm.searchPlaceholder = $translate.instant('Search');

		function initSelectedGroups(mode, groupIds, groupPageId) {
			vm.selectedGroups = {
				mode: mode,
				groupIds: groupIds,
				groupPageId: groupPageId
			};
		}

		var init = function () {
			vm.getGroupPagesAsync();

			$q.all(asyncData).then(function init(data) {
				if (data.pageSetting.Agents > 0) {
					vm.paginationOptions.pageSize = data.pageSetting.Agents;
				}

				var defaultFavoriteSearch = data.defaultFavoriteSearch;
				var loggedonUsersTeamId = data.loggedonUsersTeamId;

				if (defaultFavoriteSearch) {
					vm.selectedGroups.groupIds = defaultFavoriteSearch.TeamIds.slice(0);
					vm.searchOptions.keyword = defaultFavoriteSearch.SearchTerm;
					vm.selectedFavorite = defaultFavoriteSearch;
				} else if (loggedonUsersTeamId && vm.selectedGroups.groupIds.length === 0) {
					vm.selectedGroups.groupIds = [loggedonUsersTeamId].slice(0);
				}

				vm.resetSchedulePage();
				vm.permissions = teamsPermissions.all();
			});

			personSelectionSvc.clearPersonInfo();

			if (vm.staffingEnabled)
				vm.showStaffing();
		}

		init();
	}
}());
