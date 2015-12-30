﻿'use strict';

(function () {

	angular.module('wfm.teamSchedule')
		.controller('TeamScheduleCtrl', ['$scope', '$q', '$locale', '$translate', 'TeamSchedule', 'ScheduleLoader',
			'GroupScheduleFactory', 'teamScheduleNotificationService', 'Toggle', '$mdComponentRegistry', '$mdSidenav',
			'$mdUtil', TeamScheduleController]);

	function TeamScheduleController($scope, $q, $locale, $translate, teamScheduleSvc, scheduleLoader, groupScheduleFactory,
		notificationService, toggleSvc, $mdComponentRegistry, $mdSidenav, $mdUtil) {
		var vm = this;

		vm.isLoading = false;
		vm.personIdSelectionDic = {};
		vm.scheduleDate = new Date();
		vm.scheduleDateMoment = function () { return moment(vm.scheduleDate); };
		vm.agentsPerPage = 20;
		vm.isSelectAgentsPerPageEnabled = false;

		vm.searchOptions = {
			keyword: '',
			isAdvancedSearchEnabled: false,
			searchKeywordChanged: false
		};

		vm.agentsPerPageSelection = [20, 50, 100, 500];

		vm.selectorChanged = function() {
			teamScheduleSvc.updateAgentsPerPageSetting.post({ agents: vm.agentsPerPage }).$promise.then(function () {
				vm.paginationOptions.pageSize = vm.agentsPerPage;
				vm.schedulePageReset();
			});
		};

		vm.paginationOptions = {
			pageSize: vm.agentsPerPage,
			pageNumber: 1,
			totalPages: 0
		};

		vm.selectAllVisible = function () {
			var selectedPersonIdList = vm.getSelectedPersonIdList();
			return vm.paginationOptions.totalPages > 1 && selectedPersonIdList.length < vm.total;
		};

		vm.canActiveAddAbsence = function() {
			return vm.isAbsenceReportingEnabled && (vm.permissions.IsAddFullDayAbsenceAvailable || vm.permissions.IsAddIntradayAbsenceAvailable);
		}

		vm.canActiveSwapShifts = function() {
			return vm.isSwapShiftEnabled && vm.permissions.IsSwapShiftsAvailable;
		}

		vm.isMenuVisible = function () {
			return vm.canActiveAddAbsence() || vm.canActiveSwapShifts();
		};

		vm.onSelectedTeamIdChanged = function () {
			vm.schedulePageReset();
		};

		vm.onScheduleDateChanged = function (date) {
			var queryDate = moment(date).format("YYYY-MM-DD");
			teamScheduleSvc.loadAllTeams.query({ date: queryDate }).$promise.then(updateAllTeamsForTeamPicker);
			vm.schedulePageReset();
		};

		vm.onKeyWordInSearchInputChanged = function () {
			if (vm.searchOptions.searchKeywordChanged) {
				vm.personIdSelectionDic = {};
			}
			vm.schedulePageReset();
		};

		vm.schedulePageReset = function () {
			vm.paginationOptions.pageNumber = 1;
			vm.loadSchedules();
		};

		function getParamsForLoadingSchedules(options) {
			if (options == undefined) options = {};
			var params = {
				keyword: options.keyword != undefined ? options.keyword : vm.searchOptions.keyword,
				groupId: options.groupId != undefined ? options.groupId : vm.selectedTeamId,
				date: options.date != undefined ? options.date : vm.scheduleDateMoment().format("YYYY-MM-DD"),
				pageSize: options.pageSize != undefined ? options.pageSize : vm.paginationOptions.pageSize,
				currentPageIndex: options.currentPageIndex != undefined ? options.currentPageIndex : vm.paginationOptions.pageNumber
			};
			return params;
		}

		function afterSchedulesLoadedForGroup(result) {
			vm.paginationOptions.totalPages = result.GroupSchedule.length > 0 ? result.TotalPages : 0;
			vm.groupScheduleVm = groupScheduleFactory.Create(result.GroupSchedule, vm.scheduleDateMoment());
			vm.scheduleCount = vm.groupScheduleVm.Schedules.length;
			setupPersonIdSelectionDic(vm.groupScheduleVm.Schedules);
		}

		function afterSchedulesLoadedForSearchCondition(result) {
			vm.paginationOptions.totalPages = result.Schedules.length > 0 ? Math.ceil(result.Total / vm.paginationOptions.pageSize) : 0;
			vm.groupScheduleVm = groupScheduleFactory.Create(result.Schedules, vm.scheduleDateMoment());
			vm.scheduleCount = vm.groupScheduleVm.Schedules.length;

			vm.total = result.Total;
			vm.searchOptions.searchKeywordChanged = false;
			vm.searchOptions.keyword = result.Keyword;
			setupPersonIdSelectionDic(vm.groupScheduleVm.Schedules);
		};

		vm.loadSchedules = function () {
			if (vm.selectedTeamId == undefined && !vm.isSearchScheduleEnabled) {
				return;
			}

			vm.isLoading = true;
			scheduleLoader.loadSchedules(getParamsForLoadingSchedules(), function(result) {
				if (vm.isSearchScheduleEnabled) {
					afterSchedulesLoadedForSearchCondition(result);
				} else {
					afterSchedulesLoadedForGroup(result);
				}
				vm.isLoading = false;
			});
		};

		function setupPersonIdSelectionDic(schedules) {
			schedules.forEach(function (personSchedule) {
				if (vm.personIdSelectionDic[personSchedule.PersonId] === undefined) {
					vm.personIdSelectionDic[personSchedule.PersonId] = { isSelected: false };
				}
			});
		};

		vm.selectAllForAllPages = function () {
			vm.loadAllResults(function (result) {
				setupPersonIdSelectionDic(result.Schedules);
				for (var key in vm.personIdSelectionDic) {
					vm.personIdSelectionDic[key].isSelected = true;
				}
			});
		};

		vm.loadAllResults = function (callback) {
			var params = getParamsForLoadingSchedules({ pageSize: vm.total });
			teamScheduleSvc.searchSchedules.query(params).$promise.then(callback);
		};

		vm.commands = [
			{
				label: "AddAbsence",
				shortcut: "Alt+A",
				panelName: 'report-absence',
				action: toggleAddAbsencePanel,
				enabled: function() { return vm.isAnyAgentSelected(); },
				active: function () { return vm.canActiveAddAbsence(); }
			},
			{
				label: "SwapShifts",
				shortcut: "Alt+S",
				panelName: "", // No panel needed,
				action: swapShifts,
				enabled: allowSwapShifts,
				active: function () { return vm.canActiveSwapShifts(); }
			}
		];

		function toggleAddAbsencePanel() {
			if (vm.isAnyAgentSelected() && vm.canActiveAddAbsence()) {
				vm.setEarliestStartOfSelectedSchedule();
				vm.toggleMenuState();
				vm.setCurrentCommand("addAbsence")();
			}
		};

		function allowSwap(schedule) {
			return !schedule.IsFullDayAbsence && schedule.Shifts.length <= 1;
		}

		function allowSwapShifts() {
			var selectedPersonIds = vm.getSelectedPersonIdList();
			if (selectedPersonIds.length !== 2) return false;

			var firstSchedule = undefined;
			var secondSchedule = undefined;
			angular.forEach(vm.groupScheduleVm.Schedules, function (schedule) {
				var firstPersonId = selectedPersonIds[0];
				if (schedule.PersonId === firstPersonId) {
					firstSchedule = schedule;
				}

				var secondPersonId = selectedPersonIds[1];
				if (schedule.PersonId === secondPersonId) {
					secondSchedule = schedule;
				}
			});

			if (firstSchedule == undefined || secondSchedule == undefined) {
				return false;
			}

			return allowSwap(firstSchedule) && allowSwap(secondSchedule);
		}

		function swapShifts() {
			var selectedPersonIds = vm.getSelectedPersonIdList();

			if (selectedPersonIds.length !== 2 || !vm.canActiveSwapShifts()) return;

			teamScheduleSvc.swapShifts.post({
				PersonIdFrom: selectedPersonIds[0],
				PersonIdTo: selectedPersonIds[1],
				ScheduleDate: moment(vm.selectedAbsenceStartDate).format("YYYY-MM-DD")
			}).$promise.then(function (result) {
				vm.afterActionCallback(result, "FinishedSwapShifts", "FailedToSwapShifts");
			});
		}

		vm.setEarliestStartOfSelectedSchedule = function () {
			var startUpdated = false;
			var earlistStart = new Date("2099-12-31");
			for (var i = 0; i < vm.groupScheduleVm.Schedules.length; i++) {
				var schedule = vm.groupScheduleVm.Schedules[i];
				var scheduleStart = getScheduleStartTime(schedule);
				if (vm.personIdSelectionDic[schedule.PersonId].isSelected && scheduleStart < earlistStart) {
					startUpdated = true;
					earlistStart = scheduleStart;
				}
			}

			if (!startUpdated) {
				// Set to 08:00 for empty schedule or day off
				earlistStart = moment(vm.scheduleDate).startOf('day').add(8, 'hour').toDate();
			}

			vm.earliestStartTime =  earlistStart;
		}

		function getScheduleStartTime(schedule) {
			var scheduleStart = new Date("2099-12-31");
			angular.forEach(schedule.Shifts, function (shift) {
				var firstProjection = shift.Projections[0];
				var start = moment(firstProjection.Start).toDate();
				if (!firstProjection.IsOverNight && start < scheduleStart) {
					scheduleStart = start;
				}
			});

			return scheduleStart;
		}

		vm.isAnyAgentSelected = function () {
			var selectedPersonList = vm.getSelectedPersonIdList();
			return selectedPersonList.length > 0;
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
				$mdComponentRegistry.when(cmd.panelName).then(function (sideNav) {
					vm.isOpen = angular.bind(sideNav, sideNav.isOpen);
				});
				return buildToggler(cmd.panelName);
			} else {
				return null;
			}
		};

		vm.getSelectedPersonIdList = function() {
			var result = [];
			for (var key in vm.personIdSelectionDic) {
				if (vm.personIdSelectionDic[key].isSelected) {
					result.push(key);
				}
			}
			return result;
		}

		function replaceParameters(text, params) {
			params.forEach(function (element, index) {
				text = text.replace('{' + index + '}', element);
			});
			return text;
		}

		vm.toggleErrorDetails = function() {
			vm.showErrorDetails = !vm.showErrorDetails;
		}

		var handleActionResult = function (result, successMessageTemplate, failMessageTemplate) {
			var selectedPersonList = vm.getSelectedPersonIdList();
			var total = selectedPersonList.length;

			vm.errorTitle = "";
			vm.errorDetails = [];
			vm.showErrorDetails = true;

			var message;
			if (result.length > 0) {
				var successCount = total - result.length;
				message = replaceParameters($translate.instant(failMessageTemplate), [total, successCount, result.length]);
				notificationService.notifyFailure(message);
				vm.errorTitle = message;
				vm.errorDetails = result;
				vm.showErrorDetails = true;
			}
			else {
				message = replaceParameters($translate.instant(successMessageTemplate), [total]);
				notificationService.notifySuccess(message);
			}
		}

		vm.afterActionCallback = function (result, successMessage, failMessage) {
			handleActionResult(result, successMessage, failMessage);
			vm.personIdSelectionDic = {};
			vm.loadSchedules(vm.paginationOptions.pageNumber);
			vm.setCurrentCommand("");
		}

		function buildToggler(navId) {
			var debounceFn = $mdUtil.debounce(function () {
				$mdSidenav(navId).toggle().then(function () { });
			}, 200);
			return debounceFn;
		};

		vm.init = function () {
			createDocumentListeners();
			vm.isSearchScheduleEnabled = toggleSvc.WfmTeamSchedule_FindScheduleEasily_35611;
			vm.isSelectAgentsPerPageEnabled = toggleSvc.WfmTeamSchedule_SetAgentsPerPage_36230;
			vm.isAbsenceReportingEnabled = toggleSvc.WfmTeamSchedule_AbsenceReporting_35995;
			vm.searchOptions.isAdvancedSearchEnabled = toggleSvc.WfmPeople_AdvancedSearch_32973;
			vm.isSwapShiftEnabled = toggleSvc.WfmTeamSchedule_SwapShifts_36231;
	
			if (vm.isSearchScheduleEnabled) {
				vm.schedulePageReset();
			}
			vm.initialized = true;
		};

		$q.all([
			teamScheduleSvc.PromiseForloadedAllTeamsForTeamPicker(vm.scheduleDate, updateAllTeamsForTeamPicker),
			teamScheduleSvc.PromiseForloadedPermissions(updatePermissions),
			teamScheduleSvc.PromiseForGetAgentsPerPageSetting(updateAgentPerPageSetting)
		]).then(vm.init);

		function updateAgentPerPageSetting(result) {
			if (result.Agents !== 0) {
				vm.agentsPerPage = result.Agents;
				vm.paginationOptions.pageSize = vm.agentsPerPage;
			}
		};
		
		function updateAllTeamsForTeamPicker(result) {
			vm.teams = result;
		};

		function updatePermissions(result) {
			vm.permissions = result;
		};

		function createDocumentListeners() {
			vm.originalKeydownEvent = document.onkeydown;
			document.onkeydown = onKeyDownHandler;
			$scope.$on('$destroy', suspendDocumentListeners);
		};

		function suspendDocumentListeners() {
			document.onkeydown = vm.originalKeydownEvent;
		};

		function preventDefaultEvent(event) {
			// ie <11 doesnt have e.preventDefault();
			if (event.preventDefault) event.preventDefault();
			event.returnValue = false;
		};

		function onKeyDownHandler(event) {
			var key = window.event ? window.event.keyCode : event.keyCode;

			switch (key) {
				case 65: // Alt+A
					if (event.altKey) {
						preventDefaultEvent(event);
						$scope.$evalAsync(toggleAddAbsencePanel);
					}
					break;
				case 83: // Alt+S
					if (event.altKey) {
						preventDefaultEvent(event);
						$scope.$evalAsync(swapShifts);
					}
					break;

				default:
					break;
			}
		};
	};
}());