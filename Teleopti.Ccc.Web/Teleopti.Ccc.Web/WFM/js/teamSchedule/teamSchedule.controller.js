'use strict';

(function () {
	angular.module('wfm.teamSchedule')
		.controller('TeamScheduleCtrl', ['$scope', '$q', '$locale', '$translate', 'TeamSchedule',
			'GroupScheduleFactory', 'teamScheduleNotificationService', 'Toggle', 'SignalR', '$mdComponentRegistry', '$mdSidenav',
			'$mdUtil', 'guidgenerator', TeamScheduleController]);

	function TeamScheduleController($scope, $q, $locale, $translate, teamScheduleSvc, groupScheduleFactory,
		notificationService, toggleSvc, signalRSvc, $mdComponentRegistry, $mdSidenav, $mdUtil, guidgenerator) {
		var vm = this;

		vm.isLoading = false;
		vm.personIdSelectionDic = {};
		vm.scheduleDate = new Date();
		vm.scheduleDateMoment = function () { return moment(vm.scheduleDate); };
		vm.toggleForSelectAgentsPerPageEnabled = false;
		vm.isSwitchAbsence = false;
		// The original schedule got from server side
		vm.rawSchedules = [];
		vm.lastCommandTrackId = "";

		vm.searchOptions = {
			keyword: '',
			isAdvancedSearchEnabled: false,
			searchKeywordChanged: false
		};

		vm.agentsPerPageSelection = [20, 50, 100, 500];

		vm.selectorChanged = function() {
			teamScheduleSvc.updateAgentsPerPageSetting.post({ agents: vm.paginationOptions.pageSize }).$promise.then(function () {
				vm.schedulePageReset();
			});
		};

		vm.paginationOptions = {
			pageSize: 20,
			pageNumber: 1,
			totalPages: 0
		};

		vm.selectAllVisible = function () {
			var selectedPersonIdList = vm.getSelectedPersonIdList();
			return vm.paginationOptions.totalPages > 1 && selectedPersonIdList.length < vm.total;
		};

		vm.canActiveAddAbsence = function() {
			return vm.toggleForAbsenceReportingEnabled && (vm.permissions.IsAddFullDayAbsenceAvailable || vm.permissions.IsAddIntradayAbsenceAvailable);
		}

		vm.canActiveSwapShifts = function() {
			return vm.toggleForSwapShiftEnabled && vm.permissions.IsSwapShiftsAvailable;
		}

		vm.isMenuVisible = function () {
			return vm.canActiveAddAbsence() || vm.canActiveSwapShifts();
		};

		function updateShiftStatusForSelectedPerson() {
			var selectedPersonIdList = vm.getSelectedPersonIdList();
			if (selectedPersonIdList.length === 0) {
				return;
			}

			var params = {
				personIds: selectedPersonIdList,
				date: vm.scheduleDateMoment().format('YYYY-MM-DD')
			};

			teamScheduleSvc.getSchedules.query(params).$promise.then(function (result) {
				vm.groupScheduleVm = groupScheduleFactory.Create(result.Schedules, vm.scheduleDateMoment());
				setupPersonIdSelectionDic(vm.groupScheduleVm.Schedules);
			});
		}

		vm.onScheduleDateChanged = function () {
			vm.schedulePageReset();
			updateShiftStatusForSelectedPerson();
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
				date: options.date != undefined ? options.date : vm.scheduleDateMoment().format("YYYY-MM-DD"),
				pageSize: options.pageSize != undefined ? options.pageSize : vm.paginationOptions.pageSize,
				currentPageIndex: options.currentPageIndex != undefined ? options.currentPageIndex : vm.paginationOptions.pageNumber,
				isOnlyAbsences: vm.isSwitchAbsence
			};
			return params;
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

		vm.loadSchedules = function() {
			vm.isLoading = true;
			var params = getParamsForLoadingSchedules();
			teamScheduleSvc.searchSchedules.query(params).$promise.then(function(result) {
				vm.rawSchedules = result.Schedules;
				afterSchedulesLoadedForSearchCondition(result);
				vm.isLoading = false;
			});
		};

		vm.updateSchedules = function (personIdList) {
			vm.isLoading = true;

			var peopleInCurrentPage = [];
			angular.forEach(vm.rawSchedules, function (schedule) {
				if (personIdList.indexOf(schedule.PersonId) > -1) {
					peopleInCurrentPage.push(schedule.PersonId);
					schedule.ContractTimeMinutes = 0;
					schedule.Projection = null;
					schedule.WorkTimeMinutes = 0;
				}
			});

			var params = { personIds: peopleInCurrentPage, date: vm.scheduleDateMoment().format('YYYY-MM-DD') };
			teamScheduleSvc.getSchedules.query(params).$promise.then(function (result) {
				vm.rawSchedules = vm.rawSchedules.concat(result.Schedules);
				vm.groupScheduleVm = groupScheduleFactory.Create(vm.rawSchedules, vm.scheduleDateMoment());
				vm.isLoading = false;
			});
		};

		function setupPersonIdSelectionDic(schedules) {
			schedules.forEach(function (personSchedule) {				
				var allowSwap = personSchedule.AllowSwap();
				var selectedPerson = vm.personIdSelectionDic[personSchedule.PersonId];
				if (selectedPerson === undefined || selectedPerson === null) {
					vm.personIdSelectionDic[personSchedule.PersonId] = {
						isSelected: false,
						allowSwap: allowSwap
					};
				} else {
					selectedPerson.allowSwap = allowSwap;
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
				enabled: canSwapShifts,
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

		function canSwapShifts() {
			var selectedPersonInfos = vm.getSelectedPersonInfoList();
			if (selectedPersonInfos.length !== 2) return false;

			return selectedPersonInfos[0].allowSwap && selectedPersonInfos[1].allowSwap;
		}

		function swapShifts() {
			if (!canSwapShifts()) return;

			var selectedPersonIds = vm.getSelectedPersonIdList();
			if (selectedPersonIds.length !== 2) return;

			var trackId = guidgenerator.newGuid();
			teamScheduleSvc.swapShifts.post({
				PersonIdFrom: selectedPersonIds[0],
				PersonIdTo: selectedPersonIds[1],
				ScheduleDate: vm.scheduleDateMoment().format("YYYY-MM-DD"),
				TrackedCommandInfo: { TrackId: trackId }
			}).$promise.then(function (result) {
				vm.afterActionCallback({
					TrackId: trackId,
					Errors: result
				}, "FinishedSwapShifts", "FailedToSwapShifts");
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

		vm.getSelectedPersonInfoList = function() {
			var result = [];
			for (var key in vm.personIdSelectionDic) {
				var schedule = vm.personIdSelectionDic[key];
				if (schedule.isSelected) {
					result.push({
						personId: key,
						allowSwap: schedule.allowSwap
					});
				}
			}
			return result;
		}

		vm.getSelectedPersonIdList = function () {
			var personIds = [];
			var list = vm.getSelectedPersonInfoList();
			angular.forEach(list, function (element) {
				personIds.push(element.personId);
			});
			return personIds;
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

		var handleActionResult = function (errors, successMessageTemplate, failMessageTemplate) {
			var selectedPersonList = vm.getSelectedPersonIdList();
			var total = selectedPersonList.length;

			vm.errorTitle = "";
			vm.errorDetails = [];
			vm.showErrorDetails = false;

			var message;
			if (errors == undefined || errors.length === 0) {
				message = replaceParameters($translate.instant(successMessageTemplate), [total]);
				notificationService.notifySuccess(message);
			} else {
				var successCount = total - errors.length;
				message = replaceParameters($translate.instant(failMessageTemplate), [total, successCount, errors.length]);
				notificationService.notifyFailure(message);
				vm.errorTitle = message;
				vm.errorDetails = errors;
				vm.showErrorDetails = true;
			}
		}

		vm.afterActionCallback = function (result, successMessageTemplate, failMessageTemplate) {
			vm.lastCommandTrackId = result.TrackId;
			handleActionResult(result.Errors, successMessageTemplate, failMessageTemplate);

			vm.updateSchedules(vm.getSelectedPersonIdList());
			vm.personIdSelectionDic = {};
			setupPersonIdSelectionDic(vm.groupScheduleVm.Schedules);

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
			vm.toggleForSelectAgentsPerPageEnabled = toggleSvc.WfmTeamSchedule_SetAgentsPerPage_36230;
			vm.toggleForAbsenceReportingEnabled = toggleSvc.WfmTeamSchedule_AbsenceReporting_35995;
			vm.searchOptions.isAdvancedSearchEnabled = toggleSvc.WfmPeople_AdvancedSearch_32973;
			vm.toggleForSwapShiftEnabled = toggleSvc.WfmTeamSchedule_SwapShifts_36231;
			vm.toggleForRemoveAbsenceEnabled = toggleSvc.WfmTeamSchedule_RemoveAbsence_36705;
			toggleSvc.WfmTeamSchedule_SeeScheduleChangesByOthers_36303 && monitorScheduleChanged();
			vm.schedulePageReset();
		};

		$q.all([
			teamScheduleSvc.PromiseForloadedPermissions(updatePermissions),
			teamScheduleSvc.PromiseForGetAgentsPerPageSetting(updateAgentPerPageSetting)
		]).then(vm.init);

		function updateAgentPerPageSetting(result) {
			if (result.Agents !== 0) {
				vm.paginationOptions.pageSize = result.Agents;
			}
		};

		function updatePermissions(result) {
			vm.permissions = result;
		};

		function monitorScheduleChanged() {
			signalRSvc.subscribeBatchMessage(
				{ DomainType: 'IScheduleChangedInDefaultScenario' }
				, scheduleChangedEventHandler
				, 300);
		}

		function scheduleChangedEventHandler(messages) {
			var personIds = messages.filter(isMessageNeedToBeHandled())
									.map(function (message) { return message.DomainReferenceId; });

			personIds = removeDuplicatePersonId(personIds);
			personIds.length !== 0 && vm.updateSchedules(personIds);
		}

		function isMessageNeedToBeHandled() {
			var personIds = vm.groupScheduleVm.Schedules.map(function(schedule) { return schedule.PersonId; });
			var scheduleDate = vm.scheduleDateMoment();

			return function (message) {
				if (message.TrackId === vm.lastCommandTrackId) {
					return false;
				}

				var isMessageInsidePeopleList = personIds.indexOf(message.DomainReferenceId) > -1;
				var startDate = moment(message.StartDate.substring(1, message.StartDate.length));
				var endDate = moment(message.EndDate.substring(1, message.EndDate.length));
				var isScheduleDateInMessageRange = scheduleDate.isSame(startDate, 'day') || scheduleDate.isSame(endDate, 'day')
					|| (scheduleDate.isAfter(startDate, 'day') && scheduleDate.isBefore(endDate, 'day'));

				return isMessageInsidePeopleList && isScheduleDateInMessageRange;
			}
		}

		function removeDuplicatePersonId(personIds) {
			var result = [];
			personIds.forEach(function(personId) {
				if (result.indexOf(personId) === -1) result.push(personId);
			});
			return result;
		}

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