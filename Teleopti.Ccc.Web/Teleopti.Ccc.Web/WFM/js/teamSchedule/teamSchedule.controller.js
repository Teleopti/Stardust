'use strict';

(function () {
	angular.module('wfm.teamSchedule').controller('TeamScheduleCtrl', [
			'$scope', '$q', '$locale', '$translate', 'TeamSchedule', 'GroupScheduleFactory',
			'teamScheduleNotificationService', 'PersonSelection', 'ScheduleManagement', 'SwapShifts', 'PersonAbsence',
			'Toggle', 'SignalR', '$mdComponentRegistry', '$mdSidenav', '$mdUtil', 'guidgenerator', 'ShortCuts', 'keyCodes',
			'dialogs', 'WFMDate', TeamScheduleController
		]);

	function TeamScheduleController($scope, $q, $locale, $translate, teamScheduleSvc, groupScheduleFactory,
		notificationService, personSelectionSvc, scheduleMgmtSvc, swapShiftsSvc, personAbsenceSvc, toggleSvc, signalRSvc,
		$mdComponentRegistry, $mdSidenav, $mdUtil, guidgenerator, shortCuts, keyCodes, dialogSvc, WFMDateSvc) {
		
		var vm = this;

		vm.isLoading = false;
		vm.scheduleDate = new Date();
		vm.scheduleDateMoment = function () { return moment(vm.scheduleDate); };
		vm.toggleForSelectAgentsPerPageEnabled = false;
		vm.onlyLoadScheduleWithAbsence = false;
		vm.lastCommandTrackId = "";
		vm.selectedPersonAbsences = [];
		vm.isScenarioTest = false;
		vm.searchOptions = {
			keyword: '',
			isAdvancedSearchEnabled: false,
			searchKeywordChanged: false
		};

		vm.groupScheduleVm = function() {
			return scheduleMgmtSvc.groupScheduleVm;
		}

		vm.agentsPerPageSelection = [20, 50, 100, 500];

		vm.selectorChanged = function() {
			teamScheduleSvc.updateAgentsPerPageSetting.post({ agents: vm.paginationOptions.pageSize }).$promise.then(function () {
				vm.resetSchedulePage();
			});
		};

		vm.paginationOptions = {
			pageSize: 20,
			pageNumber: 1,
			totalPages: 0
		};

		function updateShiftStatusForSelectedPerson() {
			var selectedPersonIdList = personSelectionSvc.getSelectedPersonIdList();
			if (selectedPersonIdList.length === 0) {
				return;
			}

			var currentDate = vm.scheduleDateMoment().format('YYYY-MM-DD');
			var params = {
				personIds: selectedPersonIdList,
				date: currentDate
			};

			teamScheduleSvc.getSchedules.query(params).$promise.then(function (result) {
				scheduleMgmtSvc.resetSchedules(result.Schedules, vm.scheduleDateMoment());
				personSelectionSvc.updatePersonInfo(scheduleMgmtSvc.groupScheduleVm.Schedules, currentDate);
			});
		}

		vm.onScheduleDateChanged = function () {
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
			vm.selectedPersonAbsences = [];
			vm.loadSchedules();
		};

		function getParamsForLoadingSchedules(options) {
			if (options == undefined) options = {};
			var params = {
				keyword: options.keyword != undefined ? options.keyword : vm.searchOptions.keyword,
				date: options.date != undefined ? options.date : vm.scheduleDateMoment().format("YYYY-MM-DD"),
				pageSize: options.pageSize != undefined ? options.pageSize : vm.paginationOptions.pageSize,
				currentPageIndex: options.currentPageIndex != undefined ? options.currentPageIndex : vm.paginationOptions.pageNumber,
				isOnlyAbsences: vm.onlyLoadScheduleWithAbsence
			};
			return params;
		}

		function setPersonAbsenceSelection() {
			var selectedAbsences = [];
			for (var i = 0; i < vm.selectedPersonAbsences.length; i++) {
				var personAndAbsencePair = vm.selectedPersonAbsences[i];
				for (var j = 0; j < personAndAbsencePair.SelectedPersonAbsences.length; j++) {
					selectedAbsences.push(personAndAbsencePair.SelectedPersonAbsences[j]);
				};
			}

			var schedules = vm.groupScheduleVm().Schedules;
			for (var i = 0; i < schedules.length; i++) {
				var schedule = schedules[i];
				for (var j = 0; j < schedule.Shifts.length; j++) {
					var shift = schedule.Shifts[j];
					for (var k = 0; k < shift.Projections.length; k++) {
						var projection = shift.Projections[k];
						if (projection.ParentPersonAbsence != null && selectedAbsences.indexOf(projection.ParentPersonAbsence) > -1) {
							projection.Selected = true;
						}
					}
				}
			}
		}

		function getTotalSelectedPersonAndAbsenceCount() {
			var absenceCount = 0;
			var personIds = [];

			for (var i = 0; i < vm.selectedPersonAbsences.length; i++) {
				var personAbsencePair = vm.selectedPersonAbsences[i];
				personIds.push(personAbsencePair.personId);
				absenceCount += personAbsencePair.SelectedPersonAbsences.length;
			}

			var selectedPersonInfo = personSelectionSvc.getSelectedPersonInfoList();
			for (var j = 0; j < selectedPersonInfo.length; j++) {
				var selectedPerson = selectedPersonInfo[j];
				if (personIds.indexOf(selectedPerson.personId) === -1) {
					personIds.push(selectedPerson.personId);
					absenceCount += selectedPerson.personAbsenceCount;
				}
			}

			return {
				PersonCount: personIds.length,
				AbsenceCount: absenceCount
			};
		}

		function afterSchedulesLoaded(result) {
			vm.paginationOptions.totalPages = result.Schedules.length > 0 ? Math.ceil(result.Total / vm.paginationOptions.pageSize) : 0;
			vm.scheduleCount = scheduleMgmtSvc.groupScheduleVm.Schedules.length;
			vm.total = result.Total;
			vm.searchOptions.searchKeywordChanged = false;
			vm.searchOptions.keyword = result.Keyword;
			personSelectionSvc.setScheduleDate(vm.scheduleDateMoment().format('YYYY-MM-DD'));
			personSelectionSvc.updatePersonInfo(scheduleMgmtSvc.groupScheduleVm.Schedules);
			setPersonAbsenceSelection();
		};

		vm.loadSchedules = function() {
			vm.isLoading = true;
			var params = getParamsForLoadingSchedules();
			teamScheduleSvc.searchSchedules.query(params).$promise.then(function (result) {
				scheduleMgmtSvc.resetSchedules(result.Schedules, vm.scheduleDateMoment());
				afterSchedulesLoaded(result);
				vm.isLoading = false;
			});
		};

		vm.toggleShowAbsenceOnly = function () {
			vm.paginationOptions.pageNumber = 1;
			vm.loadSchedules();
		}

		vm.updateSchedules = function (personIdList) {
			vm.isLoading = true;
			scheduleMgmtSvc.updateScheduleForPeoples(personIdList, vm.scheduleDateMoment(), function () {
				vm.isLoading = false;
			});
		};

		vm.selectAllForAllPages = function () {
			vm.loadAllResults(function(result) {
				var groupSchedule = groupScheduleFactory.Create(result.Schedules, vm.scheduleDateMoment());
				personSelectionSvc.selectAllPerson(groupSchedule.Schedules);
			});
		};

		vm.loadAllResults = function (callback) {
			var params = getParamsForLoadingSchedules({ pageSize: vm.total });
			teamScheduleSvc.searchSchedules.query(params).$promise.then(callback);
		};

		vm.selectAllVisible = function () {
			var selectedPersonIdList = personSelectionSvc.getSelectedPersonIdList();
			return vm.paginationOptions.totalPages > 1 && selectedPersonIdList.length < vm.total;
		};

		vm.setEarliestStartOfSelectedSchedule = function() {
			var selectedPersonIds = personSelectionSvc.getSelectedPersonIdList();
			vm.earliestStartTime = scheduleMgmtSvc.getEarliestStartOfSelectedSchedule(vm.scheduleDateMoment(), selectedPersonIds);
		};

		vm.defaultNewActivityStart = function() {
			var nowInUserTimeZone = WFMDateSvc.nowInUserTimeZone();
			if (vm.scheduleDateMoment().format('YYYY-MM-DD') == nowInUserTimeZone.format('YYYY-MM-DD')) {
				var minutes = Math.ceil(nowInUserTimeZone.minute() / 15) * 15;
				var start = nowInUserTimeZone.startOf('hour').minutes(minutes);
				return start.format('LT');
			}else {
				var latestStart = scheduleMgmtSvc.getLatestStartOfSelectedSchedule(vm.scheduleDateMoment(), personSelectionSvc.getSelectedPersonIdList());
				return moment(latestStart).format('LT');
			}
		};

		function addActivity() {
			vm.setCurrentCommand('addActivity');
		}

		function addAbsence() {
			if (!personSelectionSvc.isAnyAgentSelected() || !vm.canActiveAddAbsence()) return;
			vm.setEarliestStartOfSelectedSchedule();
			vm.setCurrentCommand("addAbsence");
		};

		function swapShifts() {
			if (!personSelectionSvc.canSwapShifts()) return;
			vm.setCurrentCommand('SwapShifts');

			var selectedPersonIds = personSelectionSvc.getSelectedPersonIdList();
			var personIdFrom = selectedPersonIds[0];
			var personIdTo = selectedPersonIds[1];
			swapShiftsSvc.PromiseForSwapShifts(personIdFrom, personIdTo, vm.scheduleDateMoment(), function(result) {
				vm.afterActionCallback(result, personSelectionSvc.getSelectedPersonIdList(), "FinishedSwapShifts", "FailedToSwapShifts");
			});
		}

		function canRemoveAbsence() {
			var selectedPersonAndAbsenceCount = getTotalSelectedPersonAndAbsenceCount();
			return vm.toggles.RemoveAbsenceEnabled && selectedPersonAndAbsenceCount.AbsenceCount > 0;
		}
		
		function removeAbsence(removeEntireCrossDayAbsence) {
			var selectedPersonIdList = personSelectionSvc.getSelectedPersonIdList();

			var personWithSelectedAbsences = [];
			var selectedAbsences = [];
			for (var i = 0; i < vm.selectedPersonAbsences.length; i++) {
				var personAndAbsencePair = vm.selectedPersonAbsences[i];
				personWithSelectedAbsences.push(personAndAbsencePair.PersonId);
				for (var j = 0; j < personAndAbsencePair.SelectedPersonAbsences.length; j++) {
					selectedAbsences.push(personAndAbsencePair.SelectedPersonAbsences[j]);
				};
			}

			var allPersonWithAbsenceRemoved = selectedPersonIdList.concat(personWithSelectedAbsences);
			personAbsenceSvc.PromiseForRemovePersonAbsence(vm.scheduleDateMoment(), selectedPersonIdList, selectedAbsences,
				removeEntireCrossDayAbsence, function(result) {
					vm.selectedPersonAbsences = [];
					vm.afterActionCallback(result, allPersonWithAbsenceRemoved, "FinishedRemoveAbsence", "FailedToRemoveAbsence");
				}
			);
		}

		function confirmRemoveAbsence() {
			if (!canRemoveAbsence()) return;
			vm.setCurrentCommand('RemoveAbsence');
			var selectedPersonAndAbsenceCount = getTotalSelectedPersonAndAbsenceCount();

			var message = replaceParameters($translate.instant("AreYouSureToRemoveSelectedAbsence"),
			[selectedPersonAndAbsenceCount.AbsenceCount, selectedPersonAndAbsenceCount.PersonCount]);
			dialogSvc.create("js/teamSchedule/html/removeAbsenceConfirmDialog.html", 'RemoveAbsenceConfirmDialogController',
			{
				header: $translate.instant("Warning"),
				message: message,
				removeEntireCrossDayAbsence: false
			}, { animation: !vm.isScenarioTest }).result.then(function(result) {
				removeAbsence(result);
			}, function() {
				return;
			});
		}
			
		vm.commands = [
			{
				label: "AddActivity",
				shortcut: "Alt+A",
				panelName: "add-activity", 
				action: addActivity,
				clickable: function () { return personSelectionSvc.isAnyAgentSelected(); },
				visible: function() { return vm.canActiveAddActivity(); }
			},
			{
				label: "AddAbsence",
				shortcut: "Alt+B",
				panelName: 'report-absence',
				action: addAbsence,
				clickable: function () { return personSelectionSvc.isAnyAgentSelected(); },
				visible: function () { return vm.canActiveAddAbsence(); }
			},
			{
				label: "SwapShifts",
				shortcut: "Alt+S",
				panelName: "", // Leave empty if not creating a right panel
				action: swapShifts,
				clickable: function () { return personSelectionSvc.canSwapShifts(); },
				visible: function () { return vm.canActiveSwapShifts(); }
			},
			{
				label: "RemoveAbsence",
				shortcut: "Alt+R",
				panelName: "", // Leave empty if not creating a right panel
				action: confirmRemoveAbsence,
				clickable: function() { return vm.canRemoveAbsence(); },
				visible: function() { return vm.canActiveRemoveAbsence(); }
			}
		];

		vm.canActiveAddActivity = function() {
			return vm.toggles.AddActivityEnabled && vm.permissions.HasAddingActivityPermission;
		};

		vm.canActiveAddAbsence = function () {
			return vm.toggles.AbsenceReportingEnabled
				&& (vm.permissions.IsAddFullDayAbsenceAvailable || vm.permissions.IsAddIntradayAbsenceAvailable)
				&& vm.permissions.IsModifyScheduleAvailable;
		}

		vm.canActiveRemoveAbsence = function () {
			return vm.toggles.RemoveAbsenceEnabled
				&& vm.permissions.IsRemoveAbsenceAvailable
				&& vm.permissions.IsModifyScheduleAvailable;
		}

		vm.canActiveSwapShifts = function () {
			return vm.toggles.SwapShiftEnabled
				&& vm.permissions.IsSwapShiftsAvailable
				&& vm.permissions.IsModifyScheduleAvailable;
		}

		vm.canRemoveAbsence = function () {
			return vm.toggles.RemoveAbsenceEnabled && getTotalSelectedPersonAndAbsenceCount().AbsenceCount > 0;
		};


		vm.toggleCommandState = function (menuName) {
			if (menuName !== undefined) {
				$mdSidenav(menuName).toggle();
			}
		};

		vm.getCurrentCommand = function (currentCmdName) {
			if (currentCmdName != undefined) {
				for (var i = 0; i < vm.commands.length; i++) {
					var cmd = vm.commands[i];
					if (cmd.label.toLowerCase() === currentCmdName.toLowerCase()) {
						return cmd;
					}
				};
			}
			return undefined;
		};

		vm.toggleCurrentSidenav = function () { return false; };

		vm.setCurrentCommand = function (currentCmdName) {
			var currentCmd = vm.getCurrentCommand(currentCmdName);
			if (currentCmd !== undefined) {
				vm.commands.forEach(function(cmd) {
					if (cmd.panelName.length > 0 && cmd.panelName !== currentCmd.panelName && $mdSidenav(cmd.panelName).isOpen()) {
						$mdSidenav(cmd.panelName).close();
					}
				});
			} else {
				vm.commands.forEach(function (cmd) {
					if (cmd.panelName.length > 0 && $mdSidenav(cmd.panelName).isOpen()) {
						$mdSidenav(cmd.panelName).close();
					}
				});
			}

			if (currentCmd != undefined && currentCmd.panelName != undefined && currentCmd.panelName.length > 0) {
				$mdComponentRegistry.when(currentCmd.panelName).then(function (sideNav) {
					vm.toggleCurrentSidenav = angular.bind(sideNav, sideNav.isOpen);
				});
				vm.toggleCommandState(currentCmd.panelName);
			}
		};

		vm.selectedPersonInfo = function () {
			return personSelectionSvc.personInfo;
		}

		vm.getSelectedPersonIdList = function() {
			return personSelectionSvc.getSelectedPersonIdList();
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
			var selectedPersonList = personSelectionSvc.getSelectedPersonIdList();
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

		vm.afterActionCallback = function(result, personIds, successMessageTemplate, failMessageTemplate) {
			vm.lastCommandTrackId = result.TrackId;
			handleActionResult(result.Errors, successMessageTemplate, failMessageTemplate);

			vm.updateSchedules(personIds);
			personSelectionSvc.resetPersonInfo(scheduleMgmtSvc.groupScheduleVm.Schedules);
			vm.setCurrentCommand("");
		};

		vm.afterAddActivity = function() {
			//temp test
			vm.setCurrentCommand("");
		};
		function registerShortCuts() {
			shortCuts.registerKeySequence([keyCodes.A], [keyCodes.ALT], function () {
				addActivity(); // Alt+A for add activity
			});
			shortCuts.registerKeySequence([keyCodes.B], [keyCodes.ALT], function () {
				addAbsence(); // Alt+B for add absence
			});
			shortCuts.registerKeySequence([keyCodes.S], [keyCodes.ALT], function () {
				swapShifts(); // Alt+S for swap shifts
			});
			shortCuts.registerKeySequence([keyCodes.R], [keyCodes.ALT], function () {
				confirmRemoveAbsence(); // Alt+R for remove absence
			});
			
		}

		function isMessageNeedToBeHandled() {
			var personIds = scheduleMgmtSvc.groupScheduleVm.Schedules.map(function (schedule) { return schedule.PersonId; });
			var scheduleDate = vm.scheduleDateMoment();

			return function (message) {
				if (message.TrackId === vm.lastCommandTrackId) { return false; }

				var isMessageInsidePeopleList = personIds.indexOf(message.DomainReferenceId) > -1;
				var startDate = moment(message.StartDate.substring(1, message.StartDate.length));
				var endDate = moment(message.EndDate.substring(1, message.EndDate.length));
				var isScheduleDateInMessageRange = scheduleDate.isSame(startDate, 'day') || scheduleDate.isSame(endDate, 'day')
					|| (scheduleDate.isAfter(startDate, 'day') && scheduleDate.isBefore(endDate, 'day'));

				return isMessageInsidePeopleList && isScheduleDateInMessageRange;
			}
		}

		function scheduleChangedEventHandler(messages) {
			var personIds = messages.filter(isMessageNeedToBeHandled()).map(function (message) {
				return message.DomainReferenceId;
			});

			var uniquePersonIds = [];
			personIds.forEach(function (personId) {
				if (uniquePersonIds.indexOf(personId) === -1) uniquePersonIds.push(personId);
			});
			uniquePersonIds.length !== 0 && vm.updateSchedules(uniquePersonIds);
		}

		function monitorScheduleChanged() {
			signalRSvc.subscribeBatchMessage(
				{ DomainType: 'IScheduleChangedInDefaultScenario' }
				, scheduleChangedEventHandler
				, 300);
		}

		vm.init = function () {
			vm.toggles = {
				AddActivityEnabled: toggleSvc.WfmTeamSchedule_AddActivity_37541,
				AbsenceReportingEnabled: toggleSvc.WfmTeamSchedule_AbsenceReporting_35995,
				AdvancedSearchEnabled: toggleSvc.WfmPeople_AdvancedSearch_32973,
				RemoveAbsenceEnabled: toggleSvc.WfmTeamSchedule_RemoveAbsence_36705,
				SeeScheduleChangesByOthers: toggleSvc.WfmTeamSchedule_SeeScheduleChangesByOthers_36303,
				SelectAgentsPerPageEnabled: toggleSvc.WfmTeamSchedule_SetAgentsPerPage_36230,
				SwapShiftEnabled: toggleSvc.WfmTeamSchedule_SwapShifts_36231
			};
			vm.searchOptions.isAdvancedSearchEnabled = vm.toggles.AdvancedSearchEnabled;
			vm.toggles.SeeScheduleChangesByOthers && monitorScheduleChanged();
			vm.resetSchedulePage();
			registerShortCuts();

			vm.isMenuVisible = function () {
				return vm.canActiveAddAbsence() || vm.canActiveSwapShifts() || vm.canActiveRemoveAbsence();
			};
		};

		$q.all([
			teamScheduleSvc.PromiseForloadedPermissions(function (result) {
				vm.permissions = result;
			}),
			teamScheduleSvc.PromiseForGetAgentsPerPageSetting(function (result) {
				result.Agents > 0 && (vm.paginationOptions.pageSize = result.Agents);
			})
		]).then(vm.init);
	};
}());