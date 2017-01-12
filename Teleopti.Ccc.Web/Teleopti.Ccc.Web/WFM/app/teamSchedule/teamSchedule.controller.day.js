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
		TeamScheduleController]);

	function TeamScheduleController($scope, $q, $translate, $stateParams, $state, $mdSidenav, teamScheduleSvc, groupScheduleFactory, personSelectionSvc, scheduleMgmtSvc, NoticeService, ValidateRulesService, CommandCheckService, ScheduleNoteManagementService, teamsToggles, bootstrapCommon) {
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

		
		vm.onPersonScheduleChanged = function(personIds) {
			vm.updateSchedules(personIds);
			vm.checkValidationWarningForCommandTargets(personIds);
		}
	
		vm.toggles = teamsToggles.all();	
		vm.scheduleDate = $stateParams.selectedDate || new Date();
		vm.selectedTeamIds = $stateParams.selectedTeamIds || [];
		vm.searchOptions = {
			keyword: $stateParams.keyword || '',
			searchKeywordChanged: false
		};

		vm.cmdConfigurations = {
			validateWarningToggle: false,
			currentCommandName: null
		};
	
		vm.scheduleTableSelectMode = vm.toggles.AbsenceReportingEnabled
				|| vm.toggles.AddActivityEnabled
				|| vm.toggles.RemoveActivityEnabled
				|| vm.toggles.RemoveAbsenceEnabled
				|| vm.toggles.SwapShiftEnabled
				|| vm.toggles.ModifyShiftCategoryEnabled;

		var asyncData = {			
			pageSetting: teamScheduleSvc.PromiseForGetAgentsPerPageSetting(),
			hierarchy: teamScheduleSvc.getAvailableHierarchy(vm.scheduleDateMoment().format('YYYY-MM-DD')),
		};

		vm.searchEnabled = $state.current.name !== 'teams.for';
		if (vm.searchEnabled && vm.toggles.SaveFavoriteSearchesEnabled) {
			asyncData.defaultFavoriteSearch = vm.initFavoriteSearches.promise;
		}

		vm.boostrap = bootstrapCommon.ready();

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
