(function () {
	'use strict';
	angular.module('wfm.teamSchedule').controller('TeamScheduleController', [
		'$scope',
		'$q',
		'$translate',
		'$stateParams',
		'$state',
		'$mdSidenav',
		'$mdComponentRegistry',
		'$document',
		'TeamSchedule',
		'PersonSelection',
		'ScheduleManagement',
		'NoticeService',
		'ValidateRulesService',
		'CommandCheckService',
		'ScheduleNoteManagementService',
		'teamsToggles',
		'Toggle',
		'bootstrapCommon',
		'groupPageService',
		'TeamsStaffingConfigurationStorageService',
		'serviceDateFormatHelper',
		TeamScheduleController]);

	function TeamScheduleController($scope, $q, $translate, $stateParams, $state, $mdSidenav, $mdComponentRegistry, $document,
		teamScheduleSvc, personSelectionSvc, scheduleMgmtSvc, NoticeService, ValidateRulesService,
		CommandCheckService, ScheduleNoteManagementService, teamsToggles, toggleSvc, bootstrapCommon, groupPageService,
		StaffingConfigStorageService, serviceDateFormatHelper) {
		var vm = this;
		vm.isLoading = false;
		vm.scheduleFullyLoaded = false;
		vm.scheduleDateMoment = function () {
			return moment(vm.scheduleDate);
		};
		vm.availableTimezones = [];
		vm.sitesAndTeams = undefined;
		vm.staffingEnabled = $stateParams.staffingEnabled;
		vm.selectedGroups = {
			mode: 'BusinessHierarchy',
			groupIds: [],
			groupPageId: ''
		};
		if (angular.isArray($stateParams.selectedTeamIds) && $stateParams.selectedTeamIds.length > 0) {
			replaceArrayValues($stateParams.selectedTeamIds, vm.selectedGroups.groupIds);
		}
		else if ($stateParams.selectedGroupPage && $stateParams.selectedGroupPage.groupIds.length > 0) {
			replaceArrayValues($stateParams.selectedGroupPage.groupIds, vm.selectedGroups.groupIds);
			vm.selectedGroups.mode = 'GroupPages';
			vm.selectedGroups.groupPageId = $stateParams.selectedGroupPage.pageId;
		}

		vm.lastCommandTrackId = '';
		vm.showDatePicker = false;
		vm.currentSettings = {};

		vm.getSearch = function () {
			return {
				TeamIds: vm.selectedGroups.mode === 'BusinessHierarchy' ? vm.selectedGroups.groupIds : [],
				SearchTerm: vm.searchOptions.keyword
			};
		};
		vm.showStaffing = function () {
			initTeamSize();
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
			} else
				initCommand(command, needToOpenSidePanel);
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
			var staffingHeader = 50;
			var chartHeight = container.offsetHeight - viewHeader.offsetHeight
				- header.offsetHeight - d.height - staffingHeader - getSkillsRowHeight() - 30;

			if (tableHeight <= 100) {
				StaffingConfigStorageService.setSize(100, 100 - tHeaderHeight, chartHeight);
				return;
			}
			if (chartHeight <= 100) {
				StaffingConfigStorageService.setSize(tableHeight, tBodyHeight, 100);
				return;
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

		vm.scheduleDate = $stateParams.selectedDate || new Date();

		vm.teamNameMap = $stateParams.teamNameMap || {};
		vm.searchOptions = {
			keyword: $stateParams.keyword || '',
			searchKeywordChanged: false,
			focusingSearch: false
		};
		vm.selectedFavorite = $stateParams.do ? $stateParams.selectedFavorite : null;
		vm.sortOption = $stateParams.selectedSortOption;

		vm.openSettingsPanel = function () {
			closeAllCommandSidenav();
			$mdSidenav(settingsContainerId).toggle();
		};

		vm.openExportPanel = function () {
			$state.go('teams.exportSchedule');
		}

		vm.commonCommandCallback = function (trackId, personIds) {
			$mdSidenav(commandContainerId).isOpen() && $mdSidenav(commandContainerId).close();

			vm.lastCommandTrackId = trackId != null ? trackId : null;
			personIds && vm.updateSchedules(personIds);
			vm.checkValidationWarningForCommandTargets(personIds);
			if (vm.staffingEnabled) {
				$scope.$broadcast('teamSchedule.command.scheduleChangedApplied');
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
				scheduleMgmtSvc.resetSchedules(result.Schedules, vm.scheduleDateMoment());
				personSelectionSvc.updatePersonInfo(scheduleMgmtSvc.groupScheduleVm.Schedules, currentDate);
			});
		}

		vm.onScheduleDateChanged = function () {
			$scope.$broadcast("teamSchedule.dateChanged");
			vm.isLoading = true;
			personSelectionSvc.unselectAllPerson(scheduleMgmtSvc.groupScheduleVm.Schedules);
			personSelectionSvc.clearPersonInfo();
			vm.resetSchedulePage();
			updateShiftStatusForSelectedPerson();
			if (vm.toggles.Wfm_HideUnusedTeamsAndSites_42690) {
				loadGroupings();
			}
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
			vm.searchOptions.keyword = result.Keyword;
			vm.searchOptions.searchFields = [
				'FirstName', 'LastName', 'EmploymentNumber', 'Organization', 'Role', 'Contract', 'ContractSchedule', 'ShiftBag',
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

		vm.loadSchedules = function () {
			closeAllCommandSidenav();
			vm.isLoading = true;
			var preSelectPersonIds = $stateParams.personId ? [$stateParams.personId] : [];
			if (vm.searchEnabled) {
				var params = getParamsForLoadingSchedules();

				teamScheduleSvc.searchSchedules(params).then(function (response) {
					var result = response.data;
					scheduleMgmtSvc.resetSchedules(result.Schedules, vm.scheduleDateMoment(), vm.currentTimezone);
					ScheduleNoteManagementService.resetScheduleNotes(result.Schedules, vm.scheduleDateMoment());
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
			} else if (preSelectPersonIds.length > 0) {
				var date = serviceDateFormatHelper.getDateOnly(vm.scheduleDate);

				teamScheduleSvc.getSchedules(date, preSelectPersonIds).then(function (result) {
					scheduleMgmtSvc.resetSchedules(result.Schedules, vm.scheduleDateMoment(), vm.currentTimezone);
					ScheduleNoteManagementService.resetScheduleNotes(result.Schedules, vm.scheduleDateMoment());
					afterSchedulesLoaded(result);

					if (vm.hasSelectedAllPeopleInEveryPage) {
						personSelectionSvc.selectAllPerson(scheduleMgmtSvc.groupScheduleVm.Schedules);
					}
					personSelectionSvc.updatePersonInfo(scheduleMgmtSvc.groupScheduleVm.Schedules);

					personSelectionSvc.preSelectPeople(preSelectPersonIds, scheduleMgmtSvc.groupScheduleVm.Schedules, vm.scheduleDate);

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
				ValidateRulesService.getValidateRulesResultForCurrentPage(vm.scheduleDateMoment(), currentPagePersonIds);
			}
		};

		vm.checkValidationWarningForCommandTargets = function (personIds) {
			if (vm.currentSettings.validateWarningEnabled) {
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
			personSelectionSvc.selectAllPerson(scheduleMgmtSvc.groupScheduleVm.Schedules);
			personSelectionSvc.updatePersonInfo(scheduleMgmtSvc.groupScheduleVm.Schedules);
			vm.hasSelectedAllPeopleInEveryPage = true;
		};

		vm.unselectAllForAllPages = function () {
			personSelectionSvc.unselectAllPerson(scheduleMgmtSvc.groupScheduleVm.Schedules);
			vm.hasSelectedAllPeopleInEveryPage = false;
		};

		vm.selectAllVisible = function () {
			var selectedPersonIdList = personSelectionSvc.getSelectedPersonIdList();
			return (vm.paginationOptions.totalPages > 1 && selectedPersonIdList.length < vm.total) && !vm.hasSelectedAllPeopleInEveryPage;
		};

		vm.hasSelectedAllPeople = function () {
			return vm.hasSelectedAllPeopleInEveryPage;
		};

		vm.toggleErrorDetails = function () {
			vm.showErrorDetails = !vm.showErrorDetails;
		};

		vm.onPersonScheduleChanged = function (personIds) {
			vm.updateSchedules(personIds);
			vm.checkValidationWarningForCommandTargets(personIds);
		};

		vm.toggles = teamsToggles.all();
		vm.toggles.WfmTeamSchedule_IncreaseLimitionTo750ForScheduleQuery_74871 = toggleSvc.WfmTeamSchedule_IncreaseLimitionTo750ForScheduleQuery_74871;

		vm.scheduleDate = $stateParams.selectedDate || new Date();

		vm.searchOptions = {
			keyword: $stateParams.keyword || '',
			searchKeywordChanged: false,
			focusingSearch: false
		};
		vm.selectedFavorite = $stateParams.do ? $stateParams.selectedFavorite : null;

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
			vm.selectedGroups = { mode: 'BusinessHierarchy', groupIds: [], groupPageId: '' };
			replaceArrayValues(currentFavorite.TeamIds, vm.selectedGroups.groupIds);
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
			var text = '';
			switch (vm.selectedGroups.groupIds.length) {
				case 0:
					text = $translate.instant('SelectOrganization');
					break;

				case 1:
					text = vm.teamNameMap[vm.selectedGroups.groupIds[0]];
					break;

				default:
					text = $translate.instant('SeveralTeamsSelected').replace('{0}', vm.selectedGroups.groupIds.length);
					break;
			}
			return text;
		};

		vm.boostrap = bootstrapCommon.ready();
		var loggedonUsersTeamId = $q.defer();
		vm.onFavoriteSearchInitDefer = $q.defer();

		var asyncData = {
			pageSetting: teamScheduleSvc.PromiseForGetAgentsPerPageSetting(),
			loggedonUsersTeamId: loggedonUsersTeamId.promise,
			defaultFavoriteSearch: vm.onFavoriteSearchInitDefer.promise
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

		if (vm.toggles.Wfm_GroupPages_45057)
			vm.getGroupPagesAsync();

		vm.getSitesAndTeamsAsync = function () {
			return $q(function (resolve, reject) {
				var date = serviceDateFormatHelper.getDateOnly(vm.scheduleDate);
				teamScheduleSvc.hierarchy(date)
					.then(function (data) {
						resolve(data);
						vm.sitesAndTeams = data.Children;

						angular.extend(vm.teamNameMap, extractTeamNames(data.Children));

						loggedonUsersTeamId.resolve(data.LogonUserTeamId || null);
					});
			});
		};
		if (!vm.toggles.Wfm_GroupPages_45057)
			vm.getSitesAndTeamsAsync();

		vm.isResultTooMany = function () {
			var toggle = vm.toggles.WfmTeamSchedule_IncreaseLimitionTo750ForScheduleQuery_74871;
			return (toggle && vm.total > 750) || (!toggle && vm.total > 500);
		}
		vm.warningMessageForTooManyResults = function () {
			var toggle = vm.toggles.WfmTeamSchedule_IncreaseLimitionTo750ForScheduleQuery_74871;
			var max = toggle ? 750 : 500;
			return $translate.instant('TooManyResultsForSearchKeywords').replace('{0}', max);
		}

		function showReleaseNotification() {
			var template = $translate.instant('WFMReleaseNotificationWithoutOldModuleLink');
			var moduleName = $translate.instant('Teams');
			var message = template.replace('{0}', moduleName)
				.replace('{1}', '<a href="http://www.teleopti.com/wfm/customer-feedback.aspx">')
				.replace('{2}', '</a>');
			NoticeService.info(message, null, true);
		}

		function extractTeamNames(sites) {
			var teamNameMap = {};
			sites.forEach(function (site) {
				site.Children.forEach(function (team) {
					teamNameMap[team.Id] = team.Name;
				});
			});
			return teamNameMap;
		}

		vm.searchPlaceholder = $translate.instant('Search');

		function loadGroupings() {
			if (vm.toggles.Wfm_GroupPages_45057)
				vm.getGroupPagesAsync();
			else
				vm.getSitesAndTeamsAsync();
		}

		var init = function () {
			$q.all(asyncData).then(function init(data) {
				if (data.pageSetting.Agents > 0) {
					vm.paginationOptions.pageSize = data.pageSetting.Agents;
				}

				var defaultFavoriteSearch = data.defaultFavoriteSearch;
				var loggedonUsersTeamId = data.loggedonUsersTeamId;

				if (!$stateParams.do) {
					if (defaultFavoriteSearch) {
						replaceArrayValues(defaultFavoriteSearch.TeamIds, vm.selectedGroups.groupIds);
						vm.searchOptions.keyword = defaultFavoriteSearch.SearchTerm;
						vm.selectedFavorite = defaultFavoriteSearch;
					} else if (loggedonUsersTeamId && vm.selectedGroups.groupIds.length === 0) {
						replaceArrayValues([loggedonUsersTeamId], vm.selectedGroups.groupIds);
					}
				}

				vm.resetSchedulePage();
			});

			showReleaseNotification();
			personSelectionSvc.clearPersonInfo();

			if (vm.staffingEnabled)
				vm.showStaffing();
		}

		init();
	}

	function replaceArrayValues(from, to) {
		to.splice(0);
		from.forEach(function (x) {
			to.push(x);
		});
	}
}());
