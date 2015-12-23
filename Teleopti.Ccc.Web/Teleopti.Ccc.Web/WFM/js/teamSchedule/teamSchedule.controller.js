'use strict';

(function () {

	angular.module('wfm.teamSchedule')
		.controller('TeamScheduleCtrl', ['$scope', '$q', '$locale', 'TeamSchedule', 'GroupScheduleFactory',
			'Toggle', '$mdComponentRegistry', '$mdSidenav', '$mdUtil', '$timeout', TeamScheduleController]);

	function TeamScheduleController($scope, $q, $locale, teamScheduleSvc, groupScheduleFactory,
		toggleSvc, $mdComponentRegistry, $mdSidenav, $mdUtil, $timeout) {

		var vm = this;

		vm.isLoading = false;
		vm.personIdSelectionDic = {};
		vm.scheduleDate = new Date();
		vm.selectedAbsenceEndDate = vm.scheduleDate;
		vm.selectedAbsenceStartDate = vm.scheduleDate;
		vm.scheduleDateMoment = function () { return moment(vm.scheduleDate); };
		vm.agentsPerPage = 20;
		vm.isSelectAgentsPerPageEnabled = false;

		vm.searchOptions = {
			keyword: '',
			isAdvancedSearchEnabled: false,
			searchKeywordChanged: false
		};

		vm.getAgentsPerPageSelectionId = function (agents) {
			switch (agents) {
			case 20:
				return "1";
			case 50:
				return "2";
			case 100:
				return "3";
			case 500:
				return "4";
			default:
				return "1";
			}
		};

		vm.agentsPerPageSelection = {
			availableOptions: [
			  { id: '1', value: 20 },
			  { id: '2', value: 50 },
			  { id: '3', value: 100 },
			  { id: '4', value: 500 }
			],
			selectedOption: { id: vm.getAgentsPerPageSelectionId(vm.agentsPerPage), value: vm.agentsPerPage }
		};

		$scope.$watch("vm.agentsPerPageSelection.selectedOption", function (newValue, oldValue) {
			if (newValue == undefined || oldValue == undefined) return;
			if (newValue.id != oldValue.id) {
				teamScheduleSvc.updateAgentsPerPageSetting.post({ agents: vm.agentsPerPageSelection.selectedOption.value }).$promise.then(function () {
					vm.agentsPerPage = vm.agentsPerPageSelection.selectedOption.value;
					vm.paginationOptions.pageSize = vm.agentsPerPage;
					vm.loadSchedules(1);
				});
			}
		}, true);

		vm.paginationOptions = {
			pageSize: vm.agentsPerPageSelection.selectedOption.value,
			pageNumber: 1,
			totalPages: 0
		};

		vm.selectAllVisible = function () {
			var selectedPersonIdList = vm.getSelectedPersonIdList();
			return vm.paginationOptions.totalPages > 1 && selectedPersonIdList.length < vm.total;
		};

		vm.isMenuVisible = function () {
			return vm.isAbsenceReportingEnabled && (vm.permissions.IsAddFullDayAbsenceAvailable || vm.permissions.IsAddIntradayAbsenceAvailable);
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

		vm.loadSchedules = function () {
			if (vm.selectedTeamId == undefined && !vm.isSearchScheduleEnabled)
				return;

			var params = getParamsForLoadingSchedules();
			vm.isLoading = true;

			if (vm.isSearchScheduleEnabled) {
				loadSchedulesFromSearchCondition(params);

			} else if (vm.loadScheduelWithReadModel) {
				loadSchedulesFromReadModelForGroup(params);

			} else {
				loadSchedulesNoReadModelForGroup(params);
			}
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

		function loadSchedulesFromReadModelForGroup(params) {
			teamScheduleSvc.loadSchedulesFromReadModelForGroup.query(params).$promise.then(afterSchedulesLoadedForGroup);
		}

		function loadSchedulesNoReadModelForGroup(params) {
			teamScheduleSvc.loadSchedulesNoReadModel.query(params).$promise.then(afterSchedulesLoadedForGroup);
		}

		function loadSchedulesFromSearchCondition(params) {
			teamScheduleSvc.searchSchedules.query(params).$promise.then(afterSchedulesLoadedForSearchCondition);
		}

		function afterSchedulesLoadedForGroup(result) {
			vm.isLoading = false;
			vm.paginationOptions.totalPages = result.TotalPages;
			vm.groupScheduleVm = groupScheduleFactory.Create(result.GroupSchedule, vm.scheduleDateMoment());
			vm.scheduleCount = vm.groupScheduleVm.Schedules.length;
		}

		function afterSchedulesLoadedForSearchCondition(result) {
			vm.total = result.Total;
			vm.groupScheduleVm = groupScheduleFactory.Create(result.Schedules, vm.scheduleDateMoment());
			vm.paginationOptions.totalPages = Math.ceil(result.Total / vm.paginationOptions.pageSize);
			vm.searchOptions.searchKeywordChanged = false;
			vm.searchOptions.keyword = result.Keyword;
			vm.isLoading = false;
			vm.scheduleCount = vm.groupScheduleVm.Schedules.length;
			setupPersonIdSelectionDic(vm.groupScheduleVm.Schedules);
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


		//todo: commands should be setup based on permissions
		vm.commands = [
			{
				label: "AddAbsence",
				shortcut: "Alt+A",
				panelName: 'report-absence',
				action: toggleAddAbsencePanel,
				active: function () { return vm.isAbsenceReportingEnabled; }
			}
		];

		function toggleAddAbsencePanel() {
			if (vm.isAnyAgentSelected()) {
				vm.selectedAbsenceStartDate = vm.getEarliestStartOfSelectedSchedule();

				vm.toggleMenuState();
				vm.setCurrentCommand("addAbsence")();
			}
		};

		vm.getEarliestStartOfSelectedSchedule = function () {
			var startUpdated = false;
			var earlistStart = new Date("2099-12-31");
			for (var i = 0; i < vm.groupScheduleVm.Schedules.length; i++) {
				var schedule = vm.groupScheduleVm.Schedules[i];
				var scheduleStart = getScheduleStartTime(schedule);
				if (vm.personIdSelectionDic[schedule.PersonId].isSelected > -1 && scheduleStart < earlistStart) {
					startUpdated = true;
					earlistStart = scheduleStart;
				}
			}

			if (!startUpdated) {
				// Set to 08:00 for empty schedule or day off
				earlistStart = moment(vm.scheduleDate).startOf('day').add(8, 'hour').toDate();
			}

			return earlistStart;
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

		vm.rightPanelOption = {
			panelState: false,
			panelTitle: "AddAbsence",
			showCloseButton: true,
			showBackdrop: false
		};

		vm.showAddAbsencePanel = function () {
			vm.rightPanelOption.panelState = true;
		};

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

		vm.absenceStartDatePickerOpened = false;

		vm.toggleAbsenceStartCalendar = function () {
			vm.absenceStartDatePickerOpened = !vm.absenceStartDatePickerOpened;
		};

		vm.absenceEndDatePickerOpened = false;

		vm.isFullDayAbsence = false;

		vm.toggleAbsenceEndCalendar = function () {
			vm.absenceEndDatePickerOpened = !vm.absenceEndDatePickerOpened;
		};

		vm.isDataChangeValid = function () {
			return vm.isAnyAgentSelected() && vm.selectedAbsenceId !== undefined && (vm.isAbsenceTimeValid() || vm.isAbsenceDateValid());
		};

		vm.isAbsenceTimeValid = function () {
			return !vm.isFullDayAbsence && (moment(vm.selectedAbsenceEndDate) > moment(vm.selectedAbsenceStartDate));
		};

		vm.isAbsenceDateValid = function () {
			return vm.isFullDayAbsence && moment(vm.selectedAbsenceEndDate).startOf('day') >= moment(vm.selectedAbsenceStartDate).startOf('day');
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

		vm.clearErrors = function () {
			vm.showActionResult = false;
			vm.hasErrorInResult = false;
			vm.errorDetails = [];
		}

		vm.toggleErrorDetails = function () {
			if (vm.cleanErrorPromise != undefined) {
				$timeout.cancel(vm.cleanErrorPromise);
			}
			vm.showErrorDetails = !vm.showErrorDetails;
		}

		vm.cleanUIHistoryAfterApply = function() {
			vm.selectedAbsenceId = '';
			vm.isFullDayAbsence = false;
			vm.selectedAbsenceStartDate = vm.scheduleDate;
			vm.selectedAbsenceEndDate = vm.scheduleDate;
		}

		vm.applyAbsence = function () {
			if (vm.isFullDayAbsence) {
				teamScheduleSvc.applyFullDayAbsence.post({
					PersonIds: vm.getSelectedPersonIdList(),
					AbsenceId: vm.selectedAbsenceId,
					StartDate: moment(vm.selectedAbsenceStartDate).format("YYYY-MM-DD"),
					EndDate: moment(vm.selectedAbsenceEndDate).format("YYYY-MM-DD")
				}).$promise.then(function (result) {
					handleActionResult(result);
					vm.cleanUIHistoryAfterApply();
				});
			} else {
				teamScheduleSvc.applyIntradayAbsence.post({
					PersonIds: vm.getSelectedPersonIdList(),
					AbsenceId: vm.selectedAbsenceId,
					StartTime: moment(vm.selectedAbsenceStartDate).format("YYYY-MM-DD HH:mm"),
					EndTime: moment(vm.selectedAbsenceEndDate).format("YYYY-MM-DD HH:mm")
				}).$promise.then(function (result) {
					handleActionResult(result);
					vm.cleanUIHistoryAfterApply();
				});
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

		

		function buildToggler(navId) {
			var debounceFn = $mdUtil.debounce(function () {
				$mdSidenav(navId).toggle().then(function () { });
			}, 200);
			return debounceFn;
		};

		vm.init = function () {
			updateDateAndTimeFormat();
			createDocumentListeners();
			vm.isSearchScheduleEnabled = toggleSvc['WfmTeamSchedule_FindScheduleEasily_35611'];
			vm.loadScheduelWithReadModel = !toggleSvc['WfmTeamSchedule_NoReadModel_35609'];
			vm.isSelectAgentsPerPageEnabled = toggleSvc['WfmTeamSchedule_SetAgentsPerPage_36230'];
			vm.isAbsenceReportingEnabled = toggleSvc['WfmTeamSchedule_AbsenceReporting_35995'];
			vm.searchOptions.isAdvancedSearchEnabled = toggleSvc['WfmPeople_AdvancedSearch_32973'];
	
			if (vm.isSearchScheduleEnabled) {
				vm.onKeyWordInSearchInputChanged();
				vm.schedulePageReset();
			}
			vm.initialized = true;

			var getAgentsPerPage = teamScheduleSvc.getAgentsPerPageSetting.post().
			$promise.then(function (result) {
				if (result.Agents != 0) {
					vm.agentsPerPage = result.Agents;
					vm.agentsPerPageSelection.selectedOption.id = vm.getAgentsPerPageSelectionId(vm.agentsPerPage);
					vm.agentsPerPageSelection.selectedOption.value = vm.agentsPerPage;
					vm.paginationOptions.pageSize = vm.agentsPerPage;
				}
			});
		};

		$q.all([
			teamScheduleSvc.PromiseForloadedToggle('WfmTeamSchedule_NoReadModel_35609'),
			teamScheduleSvc.PromiseForloadedToggle('WfmPeople_AdvancedSearch_32973'),
			teamScheduleSvc.PromiseForloadedToggle('WfmTeamSchedule_FindScheduleEasily_35611'),
			teamScheduleSvc.PromiseForloadedToggle('WfmTeamSchedule_AbsenceReporting_35995'),
			teamScheduleSvc.PromiseForloadedToggle('WfmTeamSchedule_SetAgentsPerPage_36230'), 
			teamScheduleSvc.PromiseForloadedAllTeamsForTeamPicker(vm.scheduleDate, updateAllTeamsForTeamPicker),
			teamScheduleSvc.PromiseForloadedPermissions(updatePermissions),
			teamScheduleSvc.PromiseForloadedAvailableAbsenceTypes(updateAvailableAbsenceTypes)
		]).then(vm.init);


		function updateAllTeamsForTeamPicker(result) {
			vm.teams = result;
		};

		function updatePermissions(result) {
			vm.permissions = result;
		};

		function updateAvailableAbsenceTypes(result) {
			vm.AvailableAbsenceTypes = result;
		};

		function updateDateAndTimeFormat() {
			var timeFormat = $locale.DATETIME_FORMATS.shortTime;
			vm.showMeridian = timeFormat.indexOf("h:") >= 0 || timeFormat.indexOf("h.") >= 0;
			$scope.$on('$localeChangeSuccess', updateDateAndTimeFormat);
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

				default:
					break;
			}
		};

	};

}());