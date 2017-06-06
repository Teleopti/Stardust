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
		'NoticeService',
		'ValidateRulesService',
		'CommandCheckService',
		'ScheduleNoteManagementService',
		'teamsToggles',
		'bootstrapCommon',
		'teamsPermissions',
		TeamScheduleController]);

	function TeamScheduleController($scope, $q, $translate, $stateParams, $state, $mdSidenav, teamScheduleSvc, groupScheduleFactory, personSelectionSvc, scheduleMgmtSvc, NoticeService, ValidateRulesService, CommandCheckService, ScheduleNoteManagementService, teamsToggles, bootstrapCommon, teamsPermissions) {
		var vm = this;

		vm.isLoading = false;
		vm.scheduleFullyLoaded = false;
		vm.scheduleDateMoment = function () { return moment(vm.scheduleDate); };
		vm.availableTimezones = [];
		vm.availableGroups = [];

		vm.toggleForSelectAgentsPerPageEnabled = false;
		vm.onlyLoadScheduleWithAbsence = false;

		vm.lastCommandTrackId = '';
		vm.showDatePicker = false;

		vm.getSearch = function () {
			return {
				TeamIds: vm.selectedTeamIds,
				SearchTerm: vm.searchOptions.keyword
			};
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
			function() {
				vm.isLoading = true;
			});
		$scope.$on('teamSchedule.hide.loading',
			function () {
				vm.isLoading = false;
			});


		vm.scheduleDate = $stateParams.selectedDate || new Date();
		vm.selectedTeamIds = $stateParams.selectedTeamIds || [];
		vm.searchOptions = {
			keyword: $stateParams.keyword || '',
			searchKeywordChanged: false,
			focusingSearch: false
		};
		vm.selectedFavorite = $stateParams.do ? $stateParams.selectedFavorite : null;


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
			personSelectionSvc.unselectAllPerson(scheduleMgmtSvc.groupScheduleVm.Schedules);
			personSelectionSvc.clearPersonInfo();
			vm.resetSchedulePage();
			updateShiftStatusForSelectedPerson();
		};

		vm.onKeyWordInSearchInputChanged = function () {
			if (vm.searchOptions.searchKeywordChanged) {
				personSelectionSvc.unselectAllPerson(scheduleMgmtSvc.groupScheduleVm.Schedules);
				personSelectionSvc.clearPersonInfo();
			}
			vm.selectedFavorite = false;
			vm.resetSchedulePage();
		};

		vm.resetSchedulePage = function () {
			vm.paginationOptions.pageNumber = 1;
			vm.hasSelectedAllPeopleInEveryPage = false;
			vm.scheduleFullyLoaded = false;
			vm.loadSchedules();
		};

		function getParamsForLoadingSchedules(options) {
			options = options || {};
			var params = {
				SelectedTeamIds: vm.selectedTeamIds ? vm.selectedTeamIds : [],
				Keyword: options.keyword || vm.searchOptions.keyword,
				Date: options.date || vm.scheduleDateMoment().format('YYYY-MM-DD'),
				PageSize: options.pageSize || vm.paginationOptions.pageSize,
				CurrentPageIndex: options.currentPageIndex || vm.paginationOptions.pageNumber,
				IsOnlyAbsences: vm.onlyLoadScheduleWithAbsence

			};
			return params;
		}

		vm.getLoadAllSchedulesParams = function() {
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

					if(vm.hasSelectedAllPeopleInEveryPage){
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

					if(vm.hasSelectedAllPeopleInEveryPage){
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
			if (vm.validateWarningEnabled) {
				var currentPagePersonIds = scheduleMgmtSvc.groupScheduleVm.Schedules.map(function (schedule) {
					return schedule.PersonId;
				});
				ValidateRulesService.getValidateRulesResultForCurrentPage(vm.scheduleDateMoment(), currentPagePersonIds);
			}
		};

		vm.checkValidationWarningForCommandTargets = function (personIds) {
			if (vm.validateWarningEnabled) {
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

		vm.onPersonScheduleChanged = function(personIds) {
			vm.updateSchedules(personIds);
			vm.checkValidationWarningForCommandTargets(personIds);
		};

		vm.toggles = teamsToggles.all();

		vm.scheduleDate = $stateParams.selectedDate || new Date();

		Object.defineProperty(this, 'selectedTeamIds', { value: [] });

		if (angular.isArray($stateParams.selectedTeamIds)) {
			replaceArrayValues($stateParams.selectedTeamIds, vm.selectedTeamIds);
		}

		vm.searchOptions = {
			keyword: $stateParams.keyword || '',
			searchKeywordChanged: false,
			focusingSearch: false
		};
		vm.selectedFavorite = $stateParams.do? $stateParams.selectedFavorite: null;

		vm.validateWarningEnabled = false;

		vm.scheduleTableSelectMode = vm.toggles.AbsenceReportingEnabled
				|| vm.toggles.AddActivityEnabled
				|| vm.toggles.RemoveActivityEnabled
				|| vm.toggles.RemoveAbsenceEnabled
				|| vm.toggles.SwapShiftEnabled
				|| vm.toggles.ModifyShiftCategoryEnabled;

		vm.searchEnabled = $state.current.name !== 'teams.for';


		vm.onSelectedTeamsChanged = function () {
			personSelectionSvc.unselectAllPerson(scheduleMgmtSvc.groupScheduleVm.Schedules);
			personSelectionSvc.clearPersonInfo();
			vm.searchOptions.focusingSearch = true;
			vm.selectedFavorite = false;
		};

		vm.applyFavorite = function (currentFavorite) {
			vm.selectedFavorite = currentFavorite;
			replaceArrayValues(currentFavorite.TeamIds, vm.selectedTeamIds);
			vm.searchOptions.keyword = currentFavorite.SearchTerm;
			vm.resetSchedulePage();
		};

		vm.hideSearchIfNoSelectedTeam = function () {
			var toggle = vm.toggles.DisplayScheduleOnBusinessHierachyEnabled;
			if (!toggle || (angular.isArray(vm.selectedTeamIds) && vm.selectedTeamIds.length > 0)) {
				return 'visible';
			}
			return 'hidden';
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
		if (!(vm.toggles.SaveFavoriteSearchesEnabled)) {
			vm.onFavoriteSearchInitDefer.resolve();
		}
		if (!vm.toggles.DisplayScheduleOnBusinessHierachyEnabled) {
			loggedonUsersTeamId.resolve(null);
		}

		vm.sitesAndTeamsAsync = function () {
			vm._sitesAndTeamsPromise = vm._sitesAndTeamsPromise || $q(function (resolve, reject) {
				var date = moment(vm.scheduleDate).format('YYYY-MM-DD');
				teamScheduleSvc.hierarchy(date)
					.then(function (data) {
						resolve(data);
						loggedonUsersTeamId.resolve(data.LogonUserTeamId || null);
					});
			});
			return vm._sitesAndTeamsPromise;
		};

		$q.all(asyncData).then(function init(data) {
			if (data.pageSetting.Agents > 0) {
				vm.paginationOptions.pageSize = data.pageSetting.Agents;
			}

			var defaultFavoriteSearch = data.defaultFavoriteSearch;
			var loggedonUsersTeamId = data.loggedonUsersTeamId;

			if (!$stateParams.do) {
				if (defaultFavoriteSearch) {
					replaceArrayValues(defaultFavoriteSearch.TeamIds, vm.selectedTeamIds);
					vm.searchOptions.keyword = defaultFavoriteSearch.SearchTerm;
					vm.selectedFavorite = defaultFavoriteSearch;
				} else if (loggedonUsersTeamId && vm.selectedTeamIds.length === 0) {
					replaceArrayValues([loggedonUsersTeamId], vm.selectedTeamIds);
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

		vm.searchPlaceholder = $translate.instant('Search');
	}

	function replaceArrayValues(from, to) {
		to.splice(0);
		from.forEach(function (x) { to.push(x); });
	}
} ());
