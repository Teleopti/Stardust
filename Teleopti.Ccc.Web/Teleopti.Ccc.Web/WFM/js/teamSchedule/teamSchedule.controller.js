(function () {
	'use strict';
	angular.module('wfm.teamSchedule')
		.controller('TeamScheduleCtrl', ['$scope', '$q', 'TeamSchedule', 'CurrentUserInfo', 'GroupScheduleFactory', 'Toggle', '$mdComponentRegistry', '$mdSidenav', '$mdUtil', TeamScheduleController]);

	function TeamScheduleController($scope, $q, teamScheduleSvc, currentUserInfo, groupScheduleFactory, toggleSvc, $mdComponentRegistry, $mdSidenav, $mdUtil) {
		var vm = this;

		vm.permissionAddFullDayAbsence = false;

		vm.selectedTeamId = '';
		vm.scheduleDate = new Date();
		vm.scheduleDateMoment = function() {
			return moment(vm.scheduleDate);
		}
		vm.searchOptions = {
			keyword: "",
			isAdvancedSearchEnabled: false,
			searchKeywordChanged:false
		}

		vm.dateOptions = {
			formatYear: 'yyyy',
			startingDay: 1
		};

		vm.rightPanelDateOptions = {
			formatYear: 'yyyy',
			startingDay: 1
		};

		var pageSize = 18;
		vm.paginationOptions = { pageNumber: 1, totalPages: 0 };
		vm.format = 'yyyy/MM/dd';

		vm.datePickerStatus = {
			opened: false
		};
		vm.isAbsenceReportingEnabled = false;
		vm.loadScheduelWithReadModel = true;
		vm.isSearchScheduleEnabled = false;

		vm.rightPanelOption = {
			panelState: false,
			panelTitle: "Add Absence",
			showCloseButton: true,
			showBackdrop: false
		};

		vm.selectedAbsence = '';
		vm.absences = [
			{
				AbsenceId: "AC7ACFAC-C997-47DE-9729-06F424FB765D",
				AbsenceName: "Sick"
			}
		];
		vm.selectedAbsenceChanged = function(absenceId) {
			vm.selectedAbsence = absenceId;
		};

		vm.selected = [];

		var updateSelected = function (action, id) {
			if (action === 'add' && vm.selected.indexOf(id) === -1) {
				vm.selected.push(id);
			}
			if (action === 'remove' && vm.selected.indexOf(id) !== -1) {
				vm.selected.splice(vm.selected.indexOf(id), 1);
			}
		};

		vm.updateSelection = function ($event, id) {
			var checkbox = $event.target;
			var action = (checkbox.checked ? 'add' : 'remove');
			updateSelected(action, id);
		};

		vm.selectAll = function ($event) {
			var checkbox = $event.target;
			var action = (checkbox.checked ? 'add' : 'remove');
			for (var i = 0; i < vm.groupScheduleVm.Schedules.length; i++) {
				var schedule = vm.groupScheduleVm.Schedules[i];
				updateSelected(action, schedule.PersonId);
			}
		};

		vm.getSelectedClass = function (schedule) {
			return vm.isSelected(schedule.PersonId) ? 'selected' : '';
		};

		vm.isSelected = function (id) {
			return vm.selected.indexOf(id) >= 0;
		};

		vm.isSelectedAll = function () {
			if (vm.groupScheduleVm !== undefined) return vm.selected.length === vm.groupScheduleVm.Schedules.length;
			return false;
		};

		vm.toggleCalendar = function () {
			vm.datePickerStatus.opened = !vm.datePickerStatus.opened;
		};

		vm.selectedTeamIdChanged = function () {
			vm.allAgents = undefined;
			vm.paginationOptions.pageNumber = 1;
			vm.loadSchedules(vm.paginationOptions.pageNumber);
		}

		vm.scheduleDateChanged = function () {
			vm.loadTeams();
			vm.allAgents = undefined;
			vm.paginationOptions.pageNumber = 1;
			vm.loadSchedules(vm.paginationOptions.pageNumber);
		}

		vm.isLoading = false;
		vm.loadTeams = function () {
			teamScheduleSvc.loadAllTeams.query({
				date: vm.scheduleDateMoment().format("YYYY-MM-DD")
			}).$promise.then(function (result) {
				vm.Teams = result;
			});
		}

		vm.loadPermissions = function () {
			teamScheduleSvc.getPermissions.query({
			}).$promise.then(function(permissions) {
				vm.permissionAddFullDayAbsence = permissions.IsAddFullDayAbsenceAvailable;
			});
		}

		function getScheduleForCurrentPage(rawScheduleData, currentPageIndex) {
			var start = (currentPageIndex - 1) * pageSize;
			var end = currentPageIndex * pageSize;
			var agentsForCurrentPage = vm.allAgents.slice(start, end);

			var scheduleForCurrentPage = [];
			angular.forEach(rawScheduleData, function(rawSchedule) {
				if (agentsForCurrentPage.indexOf(rawSchedule.PersonId) > -1) {
					scheduleForCurrentPage.push(rawSchedule);
				}
			});

			vm.groupScheduleVm = groupScheduleFactory.Create(scheduleForCurrentPage, vm.scheduleDateMoment());
			vm.scheduleCount = vm.groupScheduleVm.Schedules.length;
		}

		vm.loadSchedules = function(currentPageIndex) {
			if (vm.selectedTeamId === "" && !vm.isSearchScheduleEnabled) return;
			vm.isLoading = true;
			vm.paginationOptions.pageNumber = currentPageIndex;
			if (vm.loadScheduelWithReadModel && !vm.isSearchScheduleEnabled) {
				teamScheduleSvc.loadSchedules.query({
					groupId: vm.selectedTeamId,
					date: vm.scheduleDateMoment().format("YYYY-MM-DD"),
					pageSize: pageSize,
					currentPageIndex: currentPageIndex
				}).$promise.then(function(result) {
					vm.isLoading = false;
					vm.paginationOptions.totalPages = result.TotalPages;
					vm.groupScheduleVm = groupScheduleFactory.Create(result.GroupSchedule, vm.scheduleDateMoment());
					vm.scheduleCount = vm.groupScheduleVm.Schedules.length;
					
				});
			} else if (!vm.isSearchScheduleEnabled) {
				if (vm.allAgents == undefined) {
					teamScheduleSvc.loadSchedulesNoReadModel.query({
						groupId: vm.selectedTeamId,
						date: vm.scheduleDateMoment().format("YYYY-MM-DD")
					}).$promise.then(function(result) {
						vm.rawScheduleData = result;

						vm.allAgents = [];
						var allSchedules = groupScheduleFactory.Create(result, vm.scheduleDateMoment()).Schedules; // keep the agents in right order
						angular.forEach(allSchedules, function(schedule) {
							vm.allAgents.push(schedule.PersonId);
						});
						vm.paginationOptions.totalPages = Math.ceil(vm.allAgents.length / pageSize);

						getScheduleForCurrentPage(vm.rawScheduleData, currentPageIndex);
						
						vm.isLoading = false;
					});
				} else {
					getScheduleForCurrentPage(vm.rawScheduleData, currentPageIndex);
					vm.isLoading = false;
				}
			} else if (vm.isSearchScheduleEnabled) {
				vm.paginationOptions.pageNumber = vm.searchOptions.searchKeywordChanged ? 1 : currentPageIndex;
				if (vm.allAgents == undefined || vm.searchOptions.searchKeywordChanged) {
					teamScheduleSvc.searchSchedules.query({
						keyword: vm.searchOptions.keyword,
						date: vm.scheduleDateMoment().format("YYYY-MM-DD")
					}).$promise.then(function(result) {
						vm.rawScheduleData = result.Schedules;
						vm.total = result.Total;
						if (vm.searchOptions.keyword == "" && result.Keyword != "") {
							vm.searchOptions.keyword = result.Keyword;
						}

						vm.allAgents = [];
						var allSchedules = groupScheduleFactory.Create(result.Schedules, vm.scheduleDateMoment()).Schedules; // keep the agents in right order
						angular.forEach(allSchedules, function(schedule) {
							vm.allAgents.push(schedule.PersonId);
						});
						vm.paginationOptions.totalPages = Math.ceil(vm.allAgents.length / pageSize);

						getScheduleForCurrentPage(vm.rawScheduleData, currentPageIndex);
						vm.searchOptions.searchKeywordChanged = false;
						vm.isLoading = false;
					});
				} else {
					getScheduleForCurrentPage(vm.rawScheduleData, currentPageIndex);
					vm.isLoading = false;
				}
			}
		}
		vm.searchSchedules = function() {
			vm.loadSchedules(vm.paginationOptions.pageNumber);
		}

		vm.showAddAbsencePanel = function() {
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
		}, true);

		vm.commands = [
			{
				label: "addAbsence",
				panelName: 'report-absence',
				action: function () {
					vm.toggleMenuState();
					vm.setCurrentCommand("addAbsence")();
				},
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

			vm.commandName = cmdName;

			var cmd = vm.currentCommand();
			$mdComponentRegistry.when(cmd.panelName).then(function (sideNav) {
				vm.isOpen = angular.bind(sideNav, sideNav.isOpen);
			});
			return buildToggler(cmd.panelName);
		}

		vm.selectedAbsenceStartDate = new Date();
		vm.absenceStartDateChanged = function(startDate) {
		//	vm.selectedAbsenceStartDate = startDate;
		}

		vm.absenceStartDatePickerOpened = false;

		vm.toggleAbsenceStartCalendar = function () {
			vm.absenceStartDatePickerOpened = !vm.absenceStartDatePickerOpened;
		};

		function buildToggler(navID) {
			var debounceFn = $mdUtil.debounce(function () {
				$mdSidenav(navID).toggle().then(function () { });
			}, 200);
			return debounceFn;
		}

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
		
		vm.Init = function () {
			vm.loadTeams();
			$q.all([loadWithoutReadModelTogglePromise, advancedSearchTogglePromise, searchScheduleTogglePromise, absenceReportingTogglePromise, loadAbsencePromise]).then(function () {
				if (vm.isSearchScheduleEnabled) {
					vm.searchSchedules();
				}
			});

			vm.loadPermissions();
		}

		vm.Init();
	}
}());