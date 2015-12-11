(function () {
	'use strict';
	angular.module('wfm.teamSchedule')
		.controller('TeamScheduleCtrl', ['$scope', '$q', '$locale', 'TeamSchedule', 'CurrentUserInfo',
			'GroupScheduleFactory', 'Toggle', '$mdComponentRegistry', '$mdSidenav', '$mdUtil', '$timeout', TeamScheduleController]);

	function TeamScheduleController($scope, $q, $locale, teamScheduleSvc, currentUserInfo, groupScheduleFactory,
		toggleSvc, $mdComponentRegistry, $mdSidenav, $mdUtil, $timeout) {
		var vm = this;

		vm.initialized = false;

		vm.selectedTeamId = '';
		vm.scheduleDate = new Date();
		vm.selectedAbsenceStartDate = vm.scheduleDate;
		vm.selectedAbsenceEndDate = vm.scheduleDate;
		vm.selectedAbsenceId = '';
		vm.total = 0;
		vm.isAbsenceReportingEnabled = false;
		vm.loadScheduelWithReadModel = true;
		vm.isSearchScheduleEnabled = false;

		vm.scheduleDateMoment = function () {
			return moment(vm.scheduleDate);
		};

		vm.searchOptions = {
			keyword: "",
			isAdvancedSearchEnabled: false,
			searchKeywordChanged: false
		};

		vm.dateOptions = {
			formatYear: 'yyyy',
			startingDay: 1
		};

		vm.paginationOptions = {
			pageSize: 18, pageNumber: 1, totalPages: 0
		};

		vm.datePickerStatus = {
			opened: false
		};

		vm.rightPanelOption = {
			panelState: false,
			panelTitle: "AddAbsence",
			showCloseButton: true,
			showBackdrop: false
		};

		vm.selectAllVisible = function () {
			return vm.paginationOptions.totalPages > 1 && vm.selectedPersonIdList.length < vm.total;
		};

		vm.selectedPersonIdList = [];

		vm.updateSelection = function (person) {
			if (person.IsSelected && vm.selectedPersonIdList.indexOf(person.PersonId) === -1) {
				vm.selectedPersonIdList.push(person.PersonId);
			}
			if (!person.IsSelected && vm.selectedPersonIdList.indexOf(person.PersonId) != -1) {
				vm.selectedPersonIdList.splice(vm.selectedPersonIdList.indexOf(person.PersonId), 1);
			}
			vm.toggleIsAllInCurrentPageSelected();
		};

		vm.toggleAllSelectionInCurrentPage = function () {
			if (vm.isAllInCurrentPageSelected) {
				angular.forEach(vm.groupScheduleVm.Schedules, function (personSchedule) {
					personSchedule.IsSelected = true;
					vm.updateSelection(personSchedule);
				});
			} else {
				angular.forEach(vm.groupScheduleVm.Schedules, function (personSchedule) {
					personSchedule.IsSelected = false;
					vm.updateSelection(personSchedule);
				});
			}
		};

		vm.toggleIsAllInCurrentPageSelected = function () {
			var isAnyoneUnselected = false;
			for (var i = 0; i < vm.groupScheduleVm.Schedules.length; i++) {
				if (vm.selectedPersonIdList.indexOf(vm.groupScheduleVm.Schedules[i].PersonId) === -1) {
					isAnyoneUnselected = true;
					break;
				}
			}
			vm.isAllInCurrentPageSelected = !isAnyoneUnselected;
		};

		vm.selectAllForAllPages = function () {
			vm.loadAllResults(function (result) {
				angular.forEach(result.Schedules, function (personSchedule) {
					if (vm.selectedPersonIdList.indexOf(personSchedule.PersonId) === -1) {
						vm.selectedPersonIdList.push(personSchedule.PersonId);
					}
				});
				vm.updatePersonSelectionStatus();
				vm.toggleIsAllInCurrentPageSelected();
			});
		};

		vm.updatePersonSelectionStatus = function () {
			angular.forEach(vm.groupScheduleVm.Schedules, function (personSchedule) {
				if (vm.selectedPersonIdList.indexOf(personSchedule.PersonId) === -1) {
					personSchedule.IsSelected = false;
				} else {
					personSchedule.IsSelected = true;
				}
			});
		};

		vm.getSelectedClass = function (schedule) {
			return schedule.IsSelected ? 'selected' : '';
		};

		vm.toggleCalendar = function () {
			vm.datePickerStatus.opened = !vm.datePickerStatus.opened;
		};

		var addScheduleDay = function (days) {
			vm.scheduleDate = moment(vm.scheduleDate).add(days, "day").toDate();
			vm.scheduleDateChanged();
		}

		vm.gotoPreviousDate = function () {
			addScheduleDay(-1);
		};

		vm.gotoNextDate = function () {
			addScheduleDay(1);
		};

		vm.selectedTeamIdChanged = function () {
			vm.paginationOptions.pageNumber = 1;
			vm.loadSchedules(vm.paginationOptions.pageNumber);
		};

		vm.scheduleDateChanged = function () {
			vm.selectedAbsenceStartDate = vm.scheduleDate;
			vm.selectedAbsenceEndDate = vm.scheduleDate;
			teamScheduleSvc.loadAllTeams.query({
				date: vm.scheduleDateMoment().format("YYYY-MM-DD")
			}).$promise.then(function (result) {
				vm.Teams = result;
				vm.paginationOptions.pageNumber = 1;
				vm.loadSchedules(vm.paginationOptions.pageNumber);
			});
		};

		vm.isLoading = false;

		vm.loadSchedules = function (currentPageIndex) {
			if (vm.selectedTeamId === "" && !vm.isSearchScheduleEnabled) return;
			vm.isLoading = true;
			vm.paginationOptions.pageNumber = currentPageIndex;
			if (vm.loadScheduelWithReadModel && !vm.isSearchScheduleEnabled) {
				teamScheduleSvc.loadSchedules.query({
					groupId: vm.selectedTeamId,
					date: vm.scheduleDateMoment().format("YYYY-MM-DD"),
					pageSize: vm.paginationOptions.pageSize,
					currentPageIndex: vm.paginationOptions.pageNumber
				}).$promise.then(function (result) {
					vm.isLoading = false;
					vm.paginationOptions.totalPages = result.TotalPages;
					vm.groupScheduleVm = groupScheduleFactory.Create(result.GroupSchedule, vm.scheduleDateMoment());
					vm.scheduleCount = vm.groupScheduleVm.Schedules.length;
				});
			} else if (!vm.isSearchScheduleEnabled) {
				teamScheduleSvc.loadSchedulesNoReadModel.query({
					groupId: vm.selectedTeamId,
					date: vm.scheduleDateMoment().format("YYYY-MM-DD"),
					pageSize: vm.paginationOptions.pageSize,
					currentPageIndex: vm.paginationOptions.pageNumber
				}).$promise.then(function (result) {
					vm.isLoading = false;
					vm.paginationOptions.totalPages = result.TotalPages;
					vm.groupScheduleVm = groupScheduleFactory.Create(result.GroupSchedule, vm.scheduleDateMoment());
				});

			} else if (vm.isSearchScheduleEnabled) {
				if (vm.searchOptions.searchKeywordChanged) {
					vm.selectedPersonIdList = [];
					vm.updatePersonSelectionStatus();
					vm.toggleIsAllInCurrentPageSelected();
					vm.paginationOptions.pageNumber = 1;
				}
				teamScheduleSvc.searchSchedules.query({
					keyword: vm.searchOptions.keyword,
					date: vm.scheduleDateMoment().format("YYYY-MM-DD"),
					pageSize: vm.paginationOptions.pageSize,
					currentPageIndex: vm.paginationOptions.pageNumber
				}).$promise.then(function (result) {
					vm.total = result.Total;
					vm.groupScheduleVm = groupScheduleFactory.Create(result.Schedules, vm.scheduleDateMoment());
					vm.paginationOptions.totalPages = Math.ceil(result.Total / vm.paginationOptions.pageSize);
					vm.searchOptions.searchKeywordChanged = false;
					vm.searchOptions.keyword = result.Keyword;
					vm.isLoading = false;
					vm.updatePersonSelectionStatus();
					vm.toggleIsAllInCurrentPageSelected();
				});
			}
		};

		vm.loadAllResults = function (callback) {
			teamScheduleSvc.searchSchedules.query({
				keyword: vm.searchOptions.keyword,
				date: vm.scheduleDateMoment().format("YYYY-MM-DD"),
				pageSize: vm.total,
				currentPageIndex: 1
			}).$promise.then(callback);
		};

		vm.searchSchedules = function () {
			vm.loadSchedules(vm.paginationOptions.pageNumber);
		};

		vm.showAddAbsencePanel = function () {
			vm.rightPanelOption.panelState = true;
		};

		vm.isAnyAgentSelected = function () {
			return vm.selectedPersonIdList.length != 0;
		};

		vm.menuState = 'open';

		vm.toggleMenuState = function () {
			if (vm.menuState === 'closed') {
				vm.menuState = 'open';
				if ($mdSidenav('report-absence').isOpen()) {
					$mdSidenav('report-absence').toggle();
				}
			} else {
				vm.menuState = 'closed';
			}
		};

		vm.isOpen = function () { return false; };

		$scope.$watch("vm.isOpen()", function (newValue, oldValue) {
			vm.menuState = newValue ? 'closed' : 'open';
			if (newValue) {
				suspendDocumentListeners();
			} else {
				createDocumentListeners();
			}
		}, true);

		var toggleAddAbsencePanel = function () {
			if (vm.isAnyAgentSelected()) {
				vm.toggleMenuState();
				vm.setCurrentCommand("addAbsence")();
			}
		}

		vm.commands = [
			{
				label: "AddAbsence",
				shortcut: "Alt+A",
				panelName: 'report-absence',
				action: toggleAddAbsencePanel,
				active: function () {
					return vm.isAbsenceReportingEnabled;
				}
			}
		];

		vm.currentCommand = function () {
			if (vm.commandName != undefined) {
				for (var i = 0; i < vm.commands.length; i++) {
					var cmd = vm.commands[i];
					if (cmd.label.toLowerCase() === vm.commandName.toLowerCase()) {
						return cmd;
					}
				};
			}
			return undefined;
		};

		vm.setCurrentCommand = function (cmdName) {
			var currentCmd = vm.currentCommand();
			if (currentCmd != undefined && currentCmd.panelName != undefined && currentCmd.panelName.length > 0) {
				$mdComponentRegistry.when(currentCmd.panelName).then(function (sideNav) {
					if (sideNav.isOpen()) {
						sideNav.toggle();
					}
				});
			}

			if (cmdName != undefined && cmdName.length > 0) {
				vm.commandName = cmdName;

				var cmd = vm.currentCommand();
				$mdComponentRegistry.when(cmd.panelName).then(function(sideNav) {
					vm.isOpen = angular.bind(sideNav, sideNav.isOpen);
				});
				return buildToggler(cmd.panelName);
			} else {
				return null;
			}
		};

		vm.absenceStartDatePickerOpened = false;

		vm.toggleAbsenceStartCalendar = function () {
			vm.absenceStartDatePickerOpened = !vm.absenceStartDatePickerOpened;
		};

		vm.absenceEndDatePickerOpened = false;

		vm.isFullDayAbsence = false;
		vm.permission = {};

		vm.isMenuVisible = function () {
			return vm.isAbsenceReportingEnabled && (vm.permission.IsAddFullDayAbsenceAvailable || vm.permission.IsAddIntradayAbsenceAvailable);
		};

		vm.toggleAbsenceEndCalendar = function () {
			vm.absenceEndDatePickerOpened = !vm.absenceEndDatePickerOpened;
		};

		vm.isDataChangeValid = function () {
			var absenceTimeIsValid = (!vm.isFullDayAbsence && (vm.selectedAbsenceEndDate > vm.selectedAbsenceStartDate))
				|| (vm.isFullDayAbsence && moment(vm.selectedAbsenceEndDate).startOf('day') >= moment(vm.selectedAbsenceStartDate).startOf('day'));
			return vm.selectedPersonIdList.length > 0 && vm.selectedAbsenceId !== "" && absenceTimeIsValid;
		};

		vm.hasErrorInResult = false;
		vm.showErrorDetails = false;
		vm.errorDetails = [];
		var handleActionResult = function (result) {
			vm.showActionResult = true;
			if (result.length > 0) {
				vm.hasErrorInResult = true;
				vm.errorDetails = result;
			} else {
				vm.hasErrorInResult = false;
				vm.errorDetails = [];
			}

			vm.cleanErrorPromise = $timeout(function () {
				vm.clearErrors();
			}, 5000);

			vm.loadSchedules(vm.paginationOptions.pageNumber);

			vm.setCurrentCommand("");
		}

		vm.clearErrors = function() {
			vm.showActionResult = false;
			vm.hasErrorInResult = false;
			vm.errorDetails = [];
		}

		vm.toggleErrorDetails = function() {
			if (vm.cleanErrorPromise != undefined) {
				$timeout.cancel(vm.cleanErrorPromise);
			}
			vm.showErrorDetails = !vm.showErrorDetails;
		}

		vm.applyAbsence = function () {
			if (vm.isFullDayAbsence) {
				teamScheduleSvc.applyFullDayAbsence.post({
					PersonIds: vm.selectedPersonIdList,
					AbsenceId: vm.selectedAbsenceId,
					StartDate: moment(vm.selectedAbsenceStartDate).format("YYYY-MM-DD"),
					EndDate: moment(vm.selectedAbsenceEndDate).format("YYYY-MM-DD")
				}).$promise.then(function (result) {
					handleActionResult(result);
				});
			} else {
				teamScheduleSvc.applyIntradayAbsence.post({
					PersonIds: vm.selectedPersonIdList,
					AbsenceId: vm.selectedAbsenceId,
					StartTime: moment(vm.selectedAbsenceStartDate).format("YYYY-MM-DD HH:mm"),
					EndTime: moment(vm.selectedAbsenceEndDate).format("YYYY-MM-DD HH:mm")
				}).$promise.then(function (result) {
					handleActionResult(result);
				});
			}
		};

		function buildToggler(navId) {
			var debounceFn = $mdUtil.debounce(function () {
				$mdSidenav(navId).toggle().then(function () { });
			}, 200);
			return debounceFn;
		};

		var loadTeamPromise = teamScheduleSvc.loadAllTeams.query({ date: vm.scheduleDateMoment().format("YYYY-MM-DD") }).$promise;
		loadTeamPromise.then(function (result) {
			vm.Teams = result;
		});

		var loadWithoutReadModelTogglePromise = toggleSvc.isFeatureEnabled.query({ toggle: 'WfmTeamSchedule_NoReadModel_35609' }).$promise;
		loadWithoutReadModelTogglePromise.then(function (result) {
			vm.loadScheduelWithReadModel = !result.IsEnabled;
		});

		var advancedSearchTogglePromise = toggleSvc.isFeatureEnabled.query({ toggle: 'WfmPeople_AdvancedSearch_32973' }).$promise;
		advancedSearchTogglePromise.then(function (result) {
			vm.searchOptions.isAdvancedSearchEnabled = result.IsEnabled;
		});

		var searchScheduleTogglePromise = toggleSvc.isFeatureEnabled.query({ toggle: 'WfmTeamSchedule_FindScheduleEasily_35611' }).$promise;
		searchScheduleTogglePromise.then(function (result) {
			vm.isSearchScheduleEnabled = result.IsEnabled;
		});

		var absenceReportingTogglePromise = toggleSvc.isFeatureEnabled.query({ toggle: 'WfmTeamSchedule_AbsenceReporting_35995' }).$promise;
		absenceReportingTogglePromise.then(function (result) {
			vm.isAbsenceReportingEnabled = result.IsEnabled;
		});

		var loadAbsencePromise = teamScheduleSvc.loadAbsences.query().$promise;
		loadAbsencePromise.then(function (result) {
			vm.absences = result;
		});

		var getPermissionsPromise = teamScheduleSvc.getPermissions.query().$promise;
		getPermissionsPromise.then(function (result) {
			vm.permission = result;
		});

		vm.Init = function () {
			vm.dateFormat = $locale.DATETIME_FORMATS.shortDate;
			$scope.$on('$localeChangeSuccess', function () {
				vm.dateFormat = $locale.DATETIME_FORMATS.shortDate;
			});

			$q.all([loadTeamPromise, loadWithoutReadModelTogglePromise, advancedSearchTogglePromise, searchScheduleTogglePromise,
				absenceReportingTogglePromise, loadAbsencePromise, getPermissionsPromise]).then(function () {
					if (vm.isSearchScheduleEnabled) {
						vm.searchSchedules();
					}
					vm.initialized = true;
				});

			createDocumentListeners();
		}

		$scope.$on('$destroy', function () {
			suspendDocumentListeners();
		});

		function preventDefaultEvent(event) {
			// ie <11 doesnt have e.preventDefault();
			if (event.preventDefault) event.preventDefault();
			event.returnValue = false;
		};

		function createDocumentListeners() {
			document.onkeydown = onKeyDownHandler;
		};

		function suspendDocumentListeners() {
			document.onkeydown = null;
		};

		function onKeyDownHandler(event) {
			performKeyDownAction(event);
		};

		function performKeyDownAction(event) {
			var key = window.event ? window.event.keyCode : event.keyCode;

			switch (key) {
				case 65: // Alt+A
					if (event.altKey) {
						preventDefaultEvent(event);
						$scope.$evalAsync(toggleAddAbsencePanel);
					}
					break;

				default:
					break;
			}
		};

		vm.Init();
	}
}());