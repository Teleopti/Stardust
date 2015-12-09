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
		vm.scheduleDateMoment = function () {
			return moment(vm.scheduleDate);
		}
		vm.searchOptions = {
			keyword: "",
			isAdvancedSearchEnabled: false,
			searchKeywordChanged: false
		}

		vm.dateOptions = {
			formatYear: 'yyyy',
			startingDay: 1
		};

		var pageSize = 18;
		vm.paginationOptions = { pageNumber: 1, totalPages: 0 };

		vm.datePickerStatus = {
			opened: false
		};
		vm.isAbsenceReportingEnabled = false;
		vm.loadScheduelWithReadModel = true;
		vm.isSearchScheduleEnabled = false;

		vm.rightPanelOption = {
			panelState: false,
			panelTitle: "AddAbsence",
			showCloseButton: true,
			showBackdrop: false
		};

		vm.selectedAbsenceId = '';

		vm.selectAllVisible = false;
		vm.totalSelected = [];
		vm.pageSelected = [];

		var updateSelected = function (action, id, pageNumber) {
			if (action === 'add' && vm.totalSelected.indexOf(id) === -1) {
				vm.totalSelected.push(id);

				vm.pageSelected[pageNumber].push(id);
			}
			if (action === 'remove' && vm.totalSelected.indexOf(id) !== -1) {
				vm.totalSelected.splice(vm.totalSelected.indexOf(id), 1);

				if (vm.pageSelected[pageNumber] != null) {
					vm.pageSelected[pageNumber].splice(vm.pageSelected[pageNumber].indexOf(id), 1);
				}
			}
			vm.selectAllVisible = vm.paginationOptions.totalPages > 1 && vm.totalSelected.length < vm.allAgents.length;
		};

		vm.updateSelection = function ($event, id) {
			var checkbox = $event.target;
			var action = (checkbox.checked ? 'add' : 'remove');
			updateSelected(action, id, vm.paginationOptions.pageNumber);
		};

		var updateAllStatusForOnePage = function (action, schedules, pageNumber) {
			for (var i = 0; i < schedules.length; i++) {
				updateSelected(action, schedules[i].PersonId, pageNumber);
			}
		};

		vm.toggleAll = function ($event) {
			var checkbox = $event.target;
			var action = (checkbox.checked ? 'add' : 'remove');
			updateAllStatusForOnePage(action, vm.groupScheduleVm.Schedules, vm.paginationOptions.pageNumber);
		};

		vm.selectAllForAllPages = function () {
			for (var i = 0; i < vm.paginationOptions.totalPages; ++i) {
				var scheduleVm = getScheduleForCurrentPage(vm.rawScheduleData, i + 1);
				updateAllStatusForOnePage('add', scheduleVm.Schedules, i + 1);
			}
			vm.selectAllVisible = false;
		};

		vm.getSelectedClass = function (schedule) {
			return vm.isSelected(schedule.PersonId) ? 'selected' : '';
		};

		vm.isSelected = function (id) {
			return vm.pageSelected[vm.paginationOptions.pageNumber].indexOf(id) >= 0;
		};

		vm.isSelectedAll = function () {
			if (vm.groupScheduleVm !== undefined) {
				return vm.pageSelected[vm.paginationOptions.pageNumber].length === vm.groupScheduleVm.Schedules.length;
			}
			return false;
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
		}

		vm.gotoNextDate = function () {
			addScheduleDay(1);
		}

		vm.selectedTeamIdChanged = function () {
			vm.allAgents = undefined;
			vm.paginationOptions.pageNumber = 1;
			vm.loadSchedules(vm.paginationOptions.pageNumber);
		}

		vm.scheduleDateChanged = function () {
			teamScheduleSvc.loadAllTeams.query({
				date: vm.scheduleDateMoment().format("YYYY-MM-DD")
			}).$promise.then(function (result) {
				vm.Teams = result;
				vm.allAgents = undefined;
				vm.paginationOptions.pageNumber = 1;
				vm.loadSchedules(vm.paginationOptions.pageNumber);
			});
		}

		vm.isLoading = false;

		function getScheduleForCurrentPage(rawScheduleData, currentPageIndex) {
			var start = (currentPageIndex - 1) * pageSize;
			var end = currentPageIndex * pageSize;
			var agentsForCurrentPage = vm.allAgents.slice(start, end);

			var scheduleForCurrentPage = [];
			angular.forEach(rawScheduleData, function (rawSchedule) {
				if (agentsForCurrentPage.indexOf(rawSchedule.PersonId) > -1) {
					scheduleForCurrentPage.push(rawSchedule);
				}
			});

			return groupScheduleFactory.Create(scheduleForCurrentPage, vm.scheduleDateMoment());
		}

		function setScheduleForCurrentPage(rawScheduleData, currentPageIndex) {
			var scheduleVm = getScheduleForCurrentPage(rawScheduleData, currentPageIndex);
			vm.groupScheduleVm = scheduleVm;
			vm.scheduleCount = scheduleVm.Schedules.length;
		};

		vm.loadSchedules = function (currentPageIndex) {
			if (vm.selectedTeamId === "" && !vm.isSearchScheduleEnabled) return;
			vm.isLoading = true;
			vm.paginationOptions.pageNumber = currentPageIndex;
			if (vm.loadScheduelWithReadModel && !vm.isSearchScheduleEnabled) {
				teamScheduleSvc.loadSchedules.query({
					groupId: vm.selectedTeamId,
					date: vm.scheduleDateMoment().format("YYYY-MM-DD"),
					pageSize: pageSize,
					currentPageIndex: currentPageIndex
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
					pageSize: pageSize,
					currentPageIndex: currentPageIndex
				}).$promise.then(function (result) {
					vm.isLoading = false;
					vm.paginationOptions.totalPages = result.TotalPages;
					vm.groupScheduleVm = groupScheduleFactory.Create(result.GroupSchedule, vm.scheduleDateMoment());
				});

			} else if (vm.isSearchScheduleEnabled) {
				vm.paginationOptions.pageNumber = vm.searchOptions.searchKeywordChanged ? 1 : currentPageIndex;
				if (vm.allAgents == undefined || vm.searchOptions.searchKeywordChanged) {
					teamScheduleSvc.searchSchedules.query({
						keyword: vm.searchOptions.keyword,
						date: vm.scheduleDateMoment().format("YYYY-MM-DD")
					}).$promise.then(function (result) {
						vm.rawScheduleData = result.Schedules;
						vm.total = result.Total;
						if (vm.searchOptions.keyword === "" && result.Keyword !== "") {
							vm.searchOptions.keyword = result.Keyword;
						}

						vm.allAgents = [];
						var allSchedules = groupScheduleFactory.Create(result.Schedules, vm.scheduleDateMoment()).Schedules; // keep the agents in right order
						angular.forEach(allSchedules, function (schedule) {
							vm.allAgents.push(schedule.PersonId);
						});
						vm.paginationOptions.totalPages = Math.ceil(vm.allAgents.length / pageSize);

						setScheduleForCurrentPage(vm.rawScheduleData, currentPageIndex);
						vm.searchOptions.searchKeywordChanged = false;
						vm.isLoading = false;
					});
				} else {
					setScheduleForCurrentPage(vm.rawScheduleData, currentPageIndex);
					vm.isLoading = false;
				}
			}
		}
		vm.searchSchedules = function () {
			vm.loadSchedules(vm.paginationOptions.pageNumber);
		}

		vm.showAddAbsencePanel = function () {
			vm.rightPanelOption.panelState = true;
		}

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
		}
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
			vm.toggleMenuState();
			vm.setCurrentCommand("addAbsence")();
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

		vm.selectedAbsenceStartDate = new Date();

		vm.absenceStartDatePickerOpened = false;

		vm.toggleAbsenceStartCalendar = function () {
			vm.absenceStartDatePickerOpened = !vm.absenceStartDatePickerOpened;
		};

		vm.selectedAbsenceEndDate = new Date();
		vm.absenceEndDatePickerOpened = false;

		vm.isFullDayAbsence = false;
		vm.permission = {};

		vm.isMenuVisible = function () {
			return vm.isAbsenceReportingEnabled && (vm.permission.IsAddFullDayAbsenceAvailable || vm.permission.IsAddIntradayAbsenceAvailable);
		}

		vm.toggleAbsenceEndCalendar = function () {
			vm.absenceEndDatePickerOpened = !vm.absenceEndDatePickerOpened;
		};

		vm.isDataChangeValid = function () {
			var absenceTimeIsValid = (!vm.isFullDayAbsence && (vm.selectedAbsenceEndDate > vm.selectedAbsenceStartDate))
				|| (vm.isFullDayAbsence && moment(vm.selectedAbsenceEndDate).startOf('day') >= moment(vm.selectedAbsenceStartDate).startOf('day'));
			return vm.totalSelected.length > 0 && vm.selectedAbsenceId !== "" && absenceTimeIsValid;
		}

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
					PersonIds: vm.totalSelected,
					AbsenceId: vm.selectedAbsenceId,
					StartDate: moment(vm.selectedAbsenceStartDate).format("YYYY-MM-DD"),
					EndDate: moment(vm.selectedAbsenceEndDate).format("YYYY-MM-DD")
				}).$promise.then(function (result) {
					handleActionResult(result);
				});
			} else {
				teamScheduleSvc.applyIntradayAbsence.post({
					PersonIds: vm.totalSelected,
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
		}

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

			for (var i = 0; i < 29; ++i) {//max page number is 28 = 500/18, and use page number as index
				vm.pageSelected[i] = [];
			}

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