'use strict';

(function () {
	angular.module('wfm.teamSchedule').controller('TeamScheduleCtrl', [
		'$scope',
		'$q',
		'$translate',
		'$mdSidenav',
		'TeamSchedule',
		'GroupScheduleFactory',
		'PersonSelection',
		'ScheduleManagement',
		'Toggle',
		'signalRSVC',
		'NoticeService',
		'ValidateRulesService',
		'CommandCheckService',
		'$state',
		'$stateParams',
		TeamScheduleController]);

	function TeamScheduleController($scope, $q, $translate, $mdSidenav, teamScheduleSvc, groupScheduleFactory, personSelectionSvc,
		scheduleMgmtSvc, toggleSvc, signalRSVC, NoticeService, ValidateRulesService, CommandCheckService, $state, $stateParams) {

		var vm = this;
		var commandContainerId = 'teamschedule-command-container';

		vm.isLoading = false;
		vm.scheduleDate = $stateParams.selectedDate ? $stateParams.selectedDate : new Date();
		vm.scheduleFullyLoaded = false;
		vm.hasSelectedAllPeopleInEveryPage = false;

		vm.triggerCommand = function (label, needToOpenSidePanel) {
			$mdSidenav(commandContainerId).close().then(function () {
				if(CommandCheckService.getCommandCheckStatus())
					CommandCheckService.resetCommandCheckStatus();
				
				needToOpenSidePanel && openSidePanel();

				$scope.$broadcast('teamSchedule.init.command', {
					activeCmd: label
				});
			});
		};

		vm.commonCommandCallback = function(trackId, personIds) {
			if ($mdSidenav(commandContainerId).isOpen()) {
				$mdSidenav(commandContainerId).close();
			}
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
		vm.validateWarningToggle = false;
		vm.lastCommandTrackId = "";
		vm.permissionsAndTogglesLoaded = false;

		vm.searchOptions = {
			keyword: $stateParams.keyword ? $stateParams.keyword : '',
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
			vm.scheduleFullyLoaded = true;
		};

		vm.loadSchedules = function() {
			vm.isLoading = true;

			var params = getParamsForLoadingSchedules();
			teamScheduleSvc.searchSchedules.query(params).$promise.then(function (result) {
				scheduleMgmtSvc.resetSchedules(result.Schedules, vm.scheduleDateMoment());
				afterSchedulesLoaded(result);

				if ($stateParams.selectedPersonIds) {
					personSelectionSvc.preSelectPeople($stateParams.selectedPersonIds, scheduleMgmtSvc.groupScheduleVm.Schedules, vm.scheduleDate);
				}
				
				personSelectionSvc.updatePersonInfo(scheduleMgmtSvc.groupScheduleVm.Schedules);
				vm.isLoading = false;
				vm.checkValidationWarningForCurrentPage();
			});
		};

		vm.toggleShowAbsenceOnly = function () {
			vm.paginationOptions.pageNumber = 1;
			vm.loadSchedules();
		};

		vm.checkValidationWarningForCurrentPage = function(){
			if (vm.validateWarningToggle) {
				var currentPagePersonIds = scheduleMgmtSvc.groupScheduleVm.Schedules.map(function(schedule) {
					return schedule.PersonId;
				});
				ValidateRulesService.getValidateRulesResultForCurrentPage(vm.scheduleDateMoment(), currentPagePersonIds);
			}
		};

		vm.checkValidationWarningForCommandTargets = function (personIds) {
			if (vm.validateWarningToggle) {
				ValidateRulesService.updateValidateRulesResultForPeople(vm.scheduleDateMoment(), personIds);
			}
		};

		vm.updateSchedules = function (personIdList) {
			vm.isLoading = true;
			scheduleMgmtSvc.updateScheduleForPeoples(personIdList, vm.scheduleDateMoment(), function() {
				personSelectionSvc.clearPersonInfo();
				vm.isLoading = false;
				vm.hasSelectedAllPeopleInEveryPage = false;
			});
		};

		vm.selectAllForAllPages = function () {
			vm.loadAllResults(function(result) {
				var groupSchedule = groupScheduleFactory.Create(result.Schedules, vm.scheduleDateMoment());
				personSelectionSvc.selectAllPerson(groupSchedule.Schedules);
				personSelectionSvc.updatePersonInfo(scheduleMgmtSvc.groupScheduleVm.Schedules);
				vm.hasSelectedAllPeopleInEveryPage = true;
			});
		};

		vm.unselectAllForAllPages = function() {
			vm.loadAllResults(function (result) {
				var groupSchedules = groupScheduleFactory.Create(result.Schedules, vm.scheduleDateMoment()).Schedules;
				personSelectionSvc.uncheckAllPersonProjectionSelection(scheduleMgmtSvc.groupScheduleVm.Schedules);
				personSelectionSvc.unselectAllPerson(groupSchedules);
				vm.hasSelectedAllPeopleInEveryPage = false;
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

		vm.hasSelectedAllPeople = function () {
			return vm.hasSelectedAllPeopleInEveryPage;
		};

		vm.toggleErrorDetails = function() {
			vm.showErrorDetails = !vm.showErrorDetails;
		};

		vm.goToWeekView = function () {
			$state.go('myTeamSchedule.weekView', {
				keyword: vm.searchOptions.keyword,
				selectedDate: vm.scheduleDate
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
			
			if(uniquePersonIds.length !== 0){
				vm.updateSchedules(uniquePersonIds);
				vm.checkValidationWarningForCommandTargets(uniquePersonIds);
			}
		}

		function monitorScheduleChanged() {
			signalRSVC.subscribeBatchMessage(
				{ DomainType: 'IScheduleChangedInDefaultScenario' }
				, scheduleChangedEventHandler
				, 300);
		}

		vm.init = function() {
			vm.toggles = {
				AddActivityEnabled: toggleSvc.WfmTeamSchedule_AddActivity_37541,
				RemoveActivityEnabled: toggleSvc.WfmTeamSchedule_RemoveActivity_37743,
				AbsenceReportingEnabled: toggleSvc.WfmTeamSchedule_AbsenceReporting_35995,
				RemoveAbsenceEnabled: toggleSvc.WfmTeamSchedule_RemoveAbsence_36705,
				SeeScheduleChangesByOthers: toggleSvc.WfmTeamSchedule_SeeScheduleChangesByOthers_36303,
				SelectAgentsPerPageEnabled: toggleSvc.WfmTeamSchedule_SetAgentsPerPage_36230,
				SwapShiftEnabled: toggleSvc.WfmTeamSchedule_SwapShifts_36231,
				MoveActivityEnabled: toggleSvc.WfmTeamSchedule_MoveActivity_37744,
				AddPersonalActivityEnabled: toggleSvc.WfmTeamSchedule_AddPersonalActivity_37742,
				ShowNightlyRestWarningEnabled: toggleSvc.WfmTeamSchedule_ShowNightlyRestWarning_39619,
				ModifyShiftCategoryEnabled: toggleSvc.WfmTeamSchedule_ModifyShiftCategory_39797,
				UndoScheduleEnabled: toggleSvc.WfmTeamSchedule_RevertToPreviousSchedule_39002,
				CheckOverlappingCertainActivitiesEnabled: toggleSvc.WfmTeamSchedule_ShowWarningForOverlappingCertainActivities_39938,
				WeekViewEnabled: toggleSvc.WfmTeamSchedule_WeekView_39870
			};
			vm.toggles.SeeScheduleChangesByOthers && monitorScheduleChanged();
			vm.resetSchedulePage();

			vm.cmdConfigurations = {
				toggles: vm.toggles,
				permissions: vm.permissions,
				currentCommandName: null
			}
			vm.permissionsAndTogglesLoaded = true;

			vm.scheduleTableSelectMode = vm.toggles.AbsenceReportingEnabled
				|| vm.toggles.AddActivityEnabled
				|| vm.toggles.RemoveActivityEnabled
				|| vm.toggles.RemoveAbsenceEnabled
				|| vm.toggles.SwapShiftEnabled
				|| vm.toggles.ModifyShiftCategoryEnabled;

			var template = $translate.instant('WFMReleaseNotification');
			var moduleName = $translate.instant('MyTeam');
			var message = template.replace('{0}', moduleName)
				.replace('{1}', '<a href="http://www.teleopti.com/wfm/customer-feedback.aspx">')
				.replace('{2}', '</a>')
				.replace('{3}', '<a href="../Anywhere#teamschedule">' + moduleName + '</a>');
			NoticeService.info(message, null, true);
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