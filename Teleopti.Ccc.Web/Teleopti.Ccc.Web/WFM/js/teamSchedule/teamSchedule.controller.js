'use strict';

(function () {
	angular.module('wfm.teamSchedule').controller('TeamScheduleCtrl', [
		'$scope',
		'$q',
		'$translate',
		'$mdSidenav',
		'TeamSchedule',
		'GroupScheduleFactory',
		'teamScheduleNotificationService',
		'PersonSelection',
		'ScheduleManagement',				
		'Toggle',
		'SignalR',								
		'NoticeService',
		

		'PersonAbsence',
		'SwapShifts',
		'WFMDate',
		'ActivityService',
		'CommandCommon',
		'guidgenerator',
		TeamScheduleController]);

	function TeamScheduleController($scope, $q, $translate, $mdSidenav, teamScheduleSvc, groupScheduleFactory, notificationService, personSelectionSvc,
		scheduleMgmtSvc, toggleSvc, signalRSvc, NoticeService, personAbsenceSvc, swapShiftsSvc, WFMDateSvc, ActivityService, CommandCommonSvc, guidgenerator) {

		var vm = this;

		vm.isLoading = false;
		vm.scheduleDate = new Date();

		vm.scheduleDateFunction = function () {
			return vm.scheudleDate;
		};

		vm.triggerCommand = function (label, needToOpenSidePanel) {			
			if (needToOpenSidePanel) openSidePanel();
			$scope.$broadcast('teamSchedule.init.command', {
				activeCmd: label
			});			
		}

		vm.commonCommandCallback = function (trackId, personIds) {	

			var commandContainerId = "teamschedule-command-container";
			if ($mdSidenav(commandContainerId).isOpen()) {
				$mdSidenav(commandContainerId).close();
			}
			vm.lastCommandTrackId = trackId;
			vm.updateSchedules(personIds);
		}

		function openSidePanel() {
			var commandContainerId = "teamschedule-command-container";
			if (!$mdSidenav(commandContainerId).isOpen()) {
				$mdSidenav(commandContainerId).toggle();
			}

			var unbindWatchPanel = $scope.$watch(function() {
				return $mdSidenav(commandContainerId).isOpen();
			}, function(newValue, oldValue) {
				if (newValue !== oldValue && !newValue) {
					$scope.$broadcast('teamSchedule.reset.command');
					unbindWatchPanel();
				}
			});
		}

		vm.scheduleDateMoment = function () { return moment(vm.scheduleDate); };
		vm.toggleForSelectAgentsPerPageEnabled = false;
		vm.onlyLoadScheduleWithAbsence = false;
		vm.lastCommandTrackId = "";
		vm.isScenarioTest = false;
		vm.permissionsAndTogglesLoaded = false;

		vm.searchOptions = {
			keyword: '',
			isAdvancedSearchEnabled: false,
			searchKeywordChanged: false
		};

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

			teamScheduleSvc.getSchedules(currentDate, selectedPersonIdList).then(function (result) {
				scheduleMgmtSvc.resetSchedules(result.Schedules, vm.scheduleDateMoment());
				personSelectionSvc.updatePersonInfo(scheduleMgmtSvc.groupScheduleVm.Schedules, currentDate);
			});
		}

		vm.onScheduleDateChanged = function () {
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

		function afterSchedulesLoaded(result) {
			vm.paginationOptions.totalPages = result.Schedules.length > 0 ? Math.ceil(result.Total / vm.paginationOptions.pageSize) : 0;
			vm.scheduleCount = scheduleMgmtSvc.groupScheduleVm.Schedules.length;
			vm.total = result.Total;
			vm.searchOptions.searchKeywordChanged = false;
			vm.searchOptions.keyword = result.Keyword;
		};

		vm.loadSchedules = function() {
			vm.isLoading = true;
			
			var params = getParamsForLoadingSchedules();
			teamScheduleSvc.searchSchedules.query(params).$promise.then(function (result) {
				scheduleMgmtSvc.resetSchedules(result.Schedules, vm.scheduleDateMoment());
				afterSchedulesLoaded(result);
				personSelectionSvc.updatePersonInfo(scheduleMgmtSvc.groupScheduleVm.Schedules);
				vm.isLoading = false;
			});
		};

		vm.toggleShowAbsenceOnly = function () {
			vm.paginationOptions.pageNumber = 1;
			vm.loadSchedules();
		};

		vm.updateSchedules = function (personIdList) {
			vm.isLoading = true;
			scheduleMgmtSvc.updateScheduleForPeoples(personIdList, vm.scheduleDateMoment(), function() {
				personSelectionSvc.clearPersonInfo();
				vm.isLoading = false;
			});
		};

		vm.selectAllForAllPages = function () {
			vm.loadAllResults(function(result) {
				var groupSchedule = groupScheduleFactory.Create(result.Schedules, vm.scheduleDateMoment());
				personSelectionSvc.selectAllPerson(groupSchedule.Schedules);
				personSelectionSvc.updatePersonInfo(scheduleMgmtSvc.groupScheduleVm.Schedules);
			});
		};

		vm.loadAllResults = function (callback) {
			var params = getParamsForLoadingSchedules({ currentPageIndex: 1, pageSize: vm.total });
			teamScheduleSvc.searchSchedules.query(params).$promise.then(callback);
		};

		vm.selectAllVisible = function () {
			var selectedPersonIdList = personSelectionSvc.getSelectedPersonIdList();
			return vm.paginationOptions.totalPages > 1 && selectedPersonIdList.length < vm.total;
		};

		vm.setEarliestStartOfSelectedSchedule = function() {
			var selectedPersonIds = personSelectionSvc.getSelectedPersonIdList();
			vm.earliestStartTime = scheduleMgmtSvc.getEarliestStartOfSelectedSchedule(vm.scheduleDateMoment(), selectedPersonIds);
		}

		vm.defaultNewActivityStart = function() {
			var nowInUserTimeZone = moment(WFMDateSvc.nowInUserTimeZone());

			if (vm.scheduleDateMoment().format('YYYY-MM-DD') == nowInUserTimeZone.format('YYYY-MM-DD')) {
				var minutes = Math.ceil(nowInUserTimeZone.minute() / 15) * 15;
				var start = nowInUserTimeZone.startOf('hour').minutes(minutes);
				return start.format('HH:mm');
			}else {
				var latestStart = scheduleMgmtSvc.getLatestStartOfSelectedSchedule(vm.scheduleDateMoment(), personSelectionSvc.getSelectedPersonIdList());
				return moment(latestStart).format('HH:mm');
			}
		};

		vm.defaultMoveActivityStart = function () {
			var time = scheduleMgmtSvc.getLatestStartTimeOfSelectedScheduleProjection(vm.scheduleDateMoment(), personSelectionSvc.getSelectedPersonIdList());
			return time == null ? new Date() : time;
		};

		vm.addActivity = function() {
		};

		vm.moveActivity = function () {
		};

		vm.addAbsence = function() {
			vm.setEarliestStartOfSelectedSchedule();
		};

		vm.swapShifts = CommandCommonSvc.wrapPersonWriteProtectionCheck(true, 'SwapShift', swapShifts, {
			check: function () { return personSelectionSvc.getSelectedPersonIdList().length == 2; },
			message: 'MustSelectTwoAgentsToSwap'
		}, vm.scheduleDate);


		function swapShifts() {
			var selectedPersonIds = personSelectionSvc.getCheckedPersonIds();
			var personIdFrom = selectedPersonIds[0];
			var personIdTo = selectedPersonIds[1];
			var scheduleDate = vm.scheduleDateMoment().format('YYYY-MM-DD');
			var trackId = guidgenerator.newGuid();
			var swapShiftsForm = {
				PersonIdFrom: personIdFrom,
				PersonIdTo: personIdTo,
				ScheduleDate: scheduleDate,
				TrackedCommandInfo: {
					TrackId: trackId
				}
			};
			swapShiftsSvc.SwapShifts(swapShiftsForm).then(function onSwapSuccess(result) {
				vm.afterActionCallback(trackId, personSelectionSvc.getSelectedPersonIdList());
				if (result.length === 0) {
					notificationService.notify('success', 'FinishedSwapShifts');
				} else {
					notificationService.notify('error', 'FailedToSwapShifts');
				}
			}, function onSwapError(error) {
				notificationService.notify('error', 'FailedToSwapShifts');
			});
		}

		function removeAbsence(removeEntireCrossDayAbsence) {
			var trackId = guidgenerator.newGuid();
			var selectedPersonIdList = personSelectionSvc.getSelectedPersonIdList();
			var selectedPersonProjections = personSelectionSvc.getSelectedPersonInfoList();

			var selectedPersonAbsences = [];
			angular.forEach(selectedPersonProjections, function (personProjection) {
				if (personProjection.personAbsenceCount > 0) {
					selectedPersonAbsences.push({
						PersonId: personProjection.personId,
						Name: personProjection.name,
						PersonAbsenceIds: personProjection.selectedAbsences
					});
				}
			});

			var commandInfo = {
				"success": 'FinishedRemoveAbsence',
				"warning": 'PartialSuccessMessageForRemovingAbsence'
			};

			personAbsenceSvc.removePersonAbsence(vm.scheduleDateMoment(), selectedPersonAbsences,
				removeEntireCrossDayAbsence, trackId).then(function (result) {
					vm.afterActionCallback(trackId, selectedPersonIdList);
					notificationService.reportActionResult(commandInfo, selectedPersonAbsences, result);
				});
		}

		function getRemoveAbsenceMessage() {
			return replaceParameters($translate.instant("AreYouSureToRemoveSelectedAbsence"),
			[personSelectionSvc.getTotalSelectedPersonAndProjectionCount().SelectedAbsenceInfo.AbsenceCount, personSelectionSvc.getTotalSelectedPersonAndProjectionCount().SelectedAbsenceInfo.PersonCount]);
		}


		vm.confirmRemoveAbsence = function () {
			CommandCommonSvc.wrapPersonWriteProtectionCheck(false,
			'RemoveAbsence', removeAbsence, null, vm.scheduleDate, getRemoveAbsenceMessage)();
		}
	
		vm.selectedPersonInfo = function() {
			return personSelectionSvc.personInfo;
		};

		function replaceParameters(text, params) {
			if (params) {
				params.forEach(function(element, index) {
					text = text.replace('{' + index + '}', element);
				});
			}
			return text;
		}

		vm.toggleErrorDetails = function() {
			vm.showErrorDetails = !vm.showErrorDetails;
		};
		vm.afterActionCallback = function (trackId, personIds) {
			vm.cmdConfigurations.currentCommandName = null;
			vm.lastCommandTrackId = trackId;
			vm.updateSchedules(personIds);
		};
	
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
				RemoveActivityEnabled: toggleSvc.WfmTeamSchedule_RemoveActivity_37743,
				AbsenceReportingEnabled: toggleSvc.WfmTeamSchedule_AbsenceReporting_35995,
				AdvancedSearchEnabled: toggleSvc.WfmPeople_AdvancedSearch_32973,
				RemoveAbsenceEnabled: toggleSvc.WfmTeamSchedule_RemoveAbsence_36705,
				SeeScheduleChangesByOthers: toggleSvc.WfmTeamSchedule_SeeScheduleChangesByOthers_36303,
				SelectAgentsPerPageEnabled: toggleSvc.WfmTeamSchedule_SetAgentsPerPage_36230,
				SwapShiftEnabled: toggleSvc.WfmTeamSchedule_SwapShifts_36231,
				PrepareToRelease: toggleSvc.WfmTeamSchedule_PrepareForRelease_37752,
				MoveActivityEnabled: toggleSvc.WfmTeamSchedule_MoveActivity_37744,
				AddPersonalActivityEnabled: toggleSvc.WfmTeamSchedule_AddPersonalActivity_37742
			};
			vm.searchOptions.isAdvancedSearchEnabled = vm.toggles.AdvancedSearchEnabled;
			vm.toggles.SeeScheduleChangesByOthers && monitorScheduleChanged();
			vm.resetSchedulePage();

			vm.cmdConfigurations = {
				toggles: vm.toggles,
				permissions: vm.permissions,
				currentCommandName: null
			}
			vm.permissionsAndTogglesLoaded = true;

			vm.scheduleTableSelectMode = vm.toggles.AbsenceReportingEnabled || vm.toggles.AddActivityEnabled || vm.toggles.RemoveActivityEnabled || vm.toggles.RemoveAbsenceEnabled || vm.toggles.SwapShiftEnabled;

			if (vm.toggles.PrepareToRelease) {
				var template = $translate.instant('WFMReleaseNotification');
				var moduleName = $translate.instant('MyTeam');
				var message = template.replace('{0}', moduleName)
					.replace('{1}', '<a href="http://www.teleopti.com/wfm/customer-feedback.aspx">')
					.replace('{2}', '</a>')
					.replace('{3}', '<a href="../Anywhere#teamschedule">' + moduleName + '</a>');
				NoticeService.info(message, null, true);
			}
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