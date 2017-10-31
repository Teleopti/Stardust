﻿(function () {
	'use strict';
	angular.module('wfm.teamSchedule').controller('TeamScheduleController', [
		'$scope',
		'$q',
		'$translate',
		'$stateParams',
		'$state',
		'$mdSidenav',
		'$mdComponentRegistry',
		'TeamSchedule',
		'PersonSelection',
		'ScheduleManagement',
		'NoticeService',
		'ValidateRulesService',
		'CommandCheckService',
		'ScheduleNoteManagementService',
		'teamsToggles',
		'bootstrapCommon',
		'groupPageService',
		'SizeStorageService',
		TeamScheduleController]);

	function TeamScheduleController($scope, $q, $translate, $stateParams, $state, $mdSidenav, $mdComponentRegistry,
									teamScheduleSvc, personSelectionSvc, scheduleMgmtSvc, NoticeService, ValidateRulesService, 
									CommandCheckService, ScheduleNoteManagementService, teamsToggles, bootstrapCommon, groupPageService,
									SizeStorageService) {
		var vm = this;
		vm.isLoading = false;
		vm.scheduleFullyLoaded = false;
		vm.scheduleDateMoment = function () {
			return moment(vm.scheduleDate);
		};
		vm.availableTimezones = [];
		vm.sitesAndTeams = undefined;
		vm.staffingEnabled = false;

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
		vm.initTeamsSize = function () {
			var storageSize = SizeStorageService.getSize();

			var container = document.querySelector('#materialcontainer');
			var viewHeader = document.querySelector('.view-header');
			var header = document.querySelector('.team-schedule .teamschedule-header');
			var footer = document.querySelector('.teamschedule-footer');

			var defaultHeight = container.offsetHeight - 62 - viewHeader.offsetHeight - header.offsetHeight - footer.offsetHeight;

			var size = storageSize || {
				tableHeight: defaultHeight * 0.64,
				chartHeight: defaultHeight * 0.3
			};
			
			if (vm.staffingEnabled) {
				vm.scheduleTableWrapperStyle = {
					'height': size.tableHeight + 'px',
					'overflow-y': 'auto'
				};
				vm.chartHeight = size.chartHeight;
			}
			else {
				vm.scheduleTableWrapperStyle = {'height': defaultHeight + 'px', 'overflow-y': 'hidden'};
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

		$scope.$on('angular-resizable.resizeEnd', function (e, d) {
			var container = document.querySelector('#materialcontainer');
			var viewHeader = document.querySelector('.view-header');
			var header = document.querySelector('.team-schedule .teamschedule-header');
			var footer = document.querySelector('.teamschedule-footer');
			var tableHeight = d.height - footer.offsetHeight;
			var chartHeight = container.offsetHeight - 62 - viewHeader.offsetHeight - header.offsetHeight - d.height - 50;
			if (tableHeight <= 100 || chartHeight <= 100) {
				SizeStorageService.setSize(100, 100);
				return;
			}
			SizeStorageService.setSize(tableHeight, chartHeight);
			vm.scheduleTableWrapperStyle = {
				'height': tableHeight + 'px',
				'overflow-y': 'auto'
			};
			vm.chartHeight = chartHeight;

		});

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
			teamScheduleSvc.updateAgentsPerPageSetting.post({agents: vm.paginationOptions.pageSize}).$promise.then(function () {
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
				Date: options.date || vm.scheduleDateMoment().format('YYYY-MM-DD'),
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
				var date = vm.scheduleDateMoment().format('YYYY-MM-DD');

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

		vm.scheduleDate = $stateParams.selectedDate || new Date();

		vm.searchOptions = {
			keyword: $stateParams.keyword || '',
			searchKeywordChanged: false,
			focusingSearch: false
		};
		vm.selectedFavorite = $stateParams.do ? $stateParams.selectedFavorite : null;

		vm.validateWarningEnabled = false;

		vm.scheduleTableSelectMode = true;

		vm.searchEnabled = $state.current.name !== 'teams.for';


		vm.onSelectedTeamsChanged = function () {
			personSelectionSvc.unselectAllPerson(scheduleMgmtSvc.groupScheduleVm.Schedules);
			personSelectionSvc.clearPersonInfo();
			vm.searchOptions.focusingSearch = true;
			vm.selectedFavorite = false;
		};

		vm.applyFavorite = function (currentFavorite) {
			vm.selectedFavorite = currentFavorite;
			vm.selectedGroups = {mode: 'BusinessHierarchy', groupIds: [], groupPageId: ''};
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
			var dateStr = moment(vm.scheduleDate).format('YYYY-MM-DD');
			groupPageService.fetchAvailableGroupPages(dateStr, dateStr).then(function (data) {
				vm.availableGroups = data;
				loggedonUsersTeamId.resolve(data.LogonUserTeamId || null);
			});
		};

		if (vm.toggles.Wfm_GroupPages_45057)
			vm.getGroupPagesAsync();

		vm.getSitesAndTeamsAsync = function () {
			return $q(function (resolve, reject) {
				var date = moment(vm.scheduleDate).format('YYYY-MM-DD');
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

		personSelectionSvc.clearPersonInfo();

		function loadGroupings() {
			if (vm.toggles.Wfm_GroupPages_45057)
				vm.getGroupPagesAsync();
			else
				vm.getSitesAndTeamsAsync();
		}
	}

	function replaceArrayValues(from, to) {
		to.splice(0);
		from.forEach(function (x) {
			to.push(x);
		});
	}
}());
