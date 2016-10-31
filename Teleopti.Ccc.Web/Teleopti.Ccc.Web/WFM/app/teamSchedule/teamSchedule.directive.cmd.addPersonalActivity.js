(function () {
	'use strict';

	angular.module('wfm.teamSchedule').directive('addPersonalActivity', addPersonalActivity);

	addPersonalActivityCtrl.$inject = ['$scope', 'ActivityService', 'PersonSelection', 'UtilityService', 'ScheduleManagement', 'teamScheduleNotificationService', 'CommandCheckService', 'belongsToDateDecider'];

	function addPersonalActivityCtrl($scope, activityService, personSelectionSvc, utility, scheduleManagementSvc, teamScheduleNotificationService, CommandCheckService, belongsToDateDecider) {
		var vm = this;

		vm.label = 'AddPersonalActivity';

		vm.isNextDay = false;
		vm.disableNextDay = false;
		vm.notAllowedNameListString = '';
		vm.availableActivitiesLoaded = false;
		vm.checkingCommand = false;
		vm.selectedAgents = personSelectionSvc.getSelectedPersonInfoList();

		activityService.fetchAvailableActivities().then(function (activities) {
			vm.availableActivities = activities;
			vm.availableActivitiesLoaded = true;
		});


		function decidePersonBelongsToDates(targetTimeRange) {
			return vm.selectedAgents.map(function (selectedAgent) {
				var belongsToDate = vm.manageScheduleForDistantTimezonesEnabled
					? belongsToDateDecider.decideBelongsToDate(targetTimeRange,
						belongsToDateDecider.normalizePersonScheduleVm(scheduleManagementSvc.findPersonScheduleVmForPersonId(selectedAgent.PersonId), vm.currentTimezone()),
						moment(vm.selectedDate()).format('YYYY-MM-DD'))
					: moment(vm.selectedDate()).format('YYYY-MM-DD');

				return {
					Date: belongsToDate,
					PersonId: selectedAgent.PersonId
				};
			});
		}

		function getTimeRangeMoment() {
			return { startTime: moment(vm.timeRange.startTime), endTime: moment(vm.timeRange.endTime) };
		}

		vm.isInputValid = function () {
			if (vm.timeRange == undefined || vm.selectedActivityId == undefined || vm.timeRange.startTime == undefined) return false;

			var belongsToDates = decidePersonBelongsToDates(getTimeRangeMoment());
			var invalidAgents = [];

			if (vm.manageScheduleForDistantTimezonesEnabled) {
				for (var i = 0; i < belongsToDates.length; i++) {
					if (!belongsToDates[i].Date) invalidAgents.push(vm.selectedAgents[i]);
				}
			} else {
				vm.selectedAgents.filter(function (agent) { return !vm.isNewActivityAllowedForAgent(agent, vm.timeRange); })
					.forEach(function (agent) {
						invalidAgents.push(agent);
					});
			}

			vm.notAllowedNameListString = invalidAgents.map(function (x) { return x.Name; }).join(', ');
			return invalidAgents.length === 0;
		};

		vm.isNewActivityAllowedForAgent = function (agent, timeRange) {
			var mNewActivityStart = moment(timeRange.startTime);
			var mNewActivityEnd = moment(timeRange.endTime);
			var mScheduleStart = moment(agent.ScheduleStartTime);
			var mScheduleEnd = moment(agent.ScheduleEndTime);
			var allowShiftTotalMinutes = 36 * 60;
			var totalMinutes = (mNewActivityEnd.days() - mScheduleStart.days()) * 24 * 60 + (mNewActivityEnd.hours() - mScheduleStart.hours()) * 60 + (mNewActivityEnd.minutes() - mScheduleStart.minutes());

			var withinAllowShiftPeriod = totalMinutes <= allowShiftTotalMinutes;

			if (mNewActivityStart.isSame(moment(vm.selectedDate()), 'day')) {
				return withinAllowShiftPeriod;
			} else {
				return withinAllowShiftPeriod && (mScheduleEnd.isAfter(mNewActivityStart));
			}
		};

		function addPersonalActivity(requestData) {
			var personIds = requestData.PersonDates.map(function (personDate) {
				return personDate.PersonId;
			});

			if(requestData.PersonDates.length > 0){
				activityService.addPersonalActivity(requestData).then(function (response) {
					if (vm.getActionCb(vm.label)) {
						vm.getActionCb(vm.label)(vm.trackId, personIds);
					}
					teamScheduleNotificationService.reportActionResult({
						success: 'SuccessfulMessageForAddingActivity',
						warning: 'PartialSuccessMessageForAddingActivity'
					}, vm.selectedAgents.map(function(x) {
						return {
							PersonId: x.PersonId,
							Name: x.Name
						}
					}), response.data);
					vm.checkingCommand = false;
				});
			}else{
				if (vm.getActionCb(vm.label)) {
					vm.getActionCb(vm.label)(vm.trackId, personIds);
				}
				vm.checkingCommand = false;
			}
		};

		function getRequestData() {
			vm.selectedAgents = personSelectionSvc.getSelectedPersonInfoList();
			return {				
				PersonDates: decidePersonBelongsToDates(getTimeRangeMoment()),				
				StartTime: vm.convertTime(moment(vm.timeRange.startTime).format("YYYY-MM-DDTHH:mm")),
				EndTime: vm.convertTime(moment(vm.timeRange.endTime).format("YYYY-MM-DDTHH:mm")),
				ActivityId: vm.selectedActivityId,
				ActivityType:2,
				TrackedCommandInfo: {
					TrackId: vm.trackId
				}
			};
		}

		vm.addPersonalActivity = function () {
			var requestData = getRequestData();
			if (vm.checkCommandActivityLayerOrders){
				vm.checkingCommand = true;
				CommandCheckService.checkAddPersonalActivityOverlapping(requestData).then(function(data) {
					addPersonalActivity(data);
				});
			}
			else
				addPersonalActivity(requestData);
		};

		vm.getDefaultActvityStartTime = function() {
			var curDateMoment = moment(vm.selectedDate());
			var personIds = vm.selectedAgents.map(function(agent) { return agent.PersonId; });
			var overnightEnds = scheduleManagementSvc.getLatestPreviousDayOvernightShiftEnd(curDateMoment, personIds);
			var latestShiftStart = scheduleManagementSvc.getLatestStartOfSelectedSchedule(curDateMoment, personIds);

			// Set to 08:00 for empty schedule or day off
			var defaultStart = curDateMoment.clone().hour(8).minute(0).second(0).toDate();
			if (overnightEnds !== null) {
				defaultStart = moment(overnightEnds).add(1, 'hour').toDate();
			}

			if (moment(utility.nowInUserTimeZone()).format('YYYY-MM-DD') === moment(vm.selectedDate()).format('YYYY-MM-DD')) {
				var nextTickTime = new Date(utility.getNextTickNoEarlierThanEight());
				if (nextTickTime > defaultStart) {
					defaultStart = nextTickTime;
				}
			} else {
				if (latestShiftStart !== null) {
					var latestShiftStartPlusOneHour = moment(latestShiftStart).add(1, 'hour').toDate();
					if (latestShiftStartPlusOneHour >= defaultStart)
						defaultStart = latestShiftStartPlusOneHour;
				}
			}

			return defaultStart;
		};

		vm.getDefaultActvityEndTime = function() {
			return moment(vm.getDefaultActvityStartTime()).add(1, 'hour').toDate();
		};
	}

	function addPersonalActivity() {
		return {
			restrict: 'E',
			scope: {},
			controller: addPersonalActivityCtrl,
			controllerAs: 'vm',
			bindToController: true,
			templateUrl: 'app/teamSchedule/html/addPersonalActivity.tpl.html',
			require: ['^teamscheduleCommandContainer', 'addPersonalActivity'],
			compile: function (tElement, tAttrs) {
				var tabindex = angular.isDefined(tAttrs.tabindex) ? tAttrs.tabindex : '0';
				function addTabindexTo() {
					angular.forEach(arguments, function (arg) {
						angular.forEach(arg, function (elem) {
							elem.setAttribute('tabIndex', tabindex);
						});
					});
				}
				addTabindexTo(
					tElement[0].querySelectorAll('md-select.activity-selector'),
					tElement[0].querySelectorAll('activity-time-range-picker'),
					tElement[0].querySelectorAll('button#applyPersonalActivity')
				);
				return function postLink(scope, elem, attrs, ctrls) {
					var containerCtrl = ctrls[0],
						selfCtrl = ctrls[1];

					scope.vm.selectedDate = containerCtrl.getDate;
					scope.vm.trackId = containerCtrl.getTrackId();
					scope.vm.currentTimezone = containerCtrl.getCurrentTimezone;
					scope.vm.convertTime = containerCtrl.convertTimeToCurrentUserTimezone;
					scope.vm.getActionCb = containerCtrl.getActionCb;
					scope.vm.checkCommandActivityLayerOrders = containerCtrl.hasToggle('CheckOverlappingCertainActivitiesEnabled');
					scope.vm.manageScheduleForDistantTimezonesEnabled = containerCtrl
						.hasToggle('ManageScheduleForDistantTimezonesEnabled');


					scope.vm.timeRange = {
						startTime: selfCtrl.getDefaultActvityStartTime(),
						endTime: selfCtrl.getDefaultActvityEndTime()
					};

					scope.$on('teamSchedule.command.focus.default',
						function() {
							var focusTarget = elem[0].querySelector('.focus-default');
							if (focusTarget) angular.element(focusTarget).focus();
						});

					elem.removeAttr('tabindex');
				};
			},
		};
	}
})();
