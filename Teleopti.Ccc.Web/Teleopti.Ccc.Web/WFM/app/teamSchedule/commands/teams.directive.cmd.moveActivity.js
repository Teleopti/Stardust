(function () {
	'use strict';

	angular.module('wfm.teamSchedule').directive('moveActivity', moveActivityDirective);

	function moveActivityDirective() {
		return {
			restrict: 'E',
			scope: {},
			controller: moveActivityCtrl,
			controllerAs: 'vm',
			bindToController: true,
			templateUrl: 'app/teamSchedule/commands/teams.directive.cmd.moveActivity.html',
			require: ['^teamscheduleCommandContainer', 'moveActivity'],
			link: function (scope, elem, attrs, ctrls) {
				var containerCtrl = ctrls[0],
					selfCtrl = ctrls[1];

				scope.vm.containerCtrl = containerCtrl;

				scope.vm.selectedDate = containerCtrl.getDate;
				scope.vm.trackId = containerCtrl.getTrackId();
				scope.vm.convertTime = containerCtrl.convertTimeToCurrentUserTimezone;
				scope.vm.getActionCb = containerCtrl.getActionCb;
				scope.vm.getCurrentTimezone = containerCtrl.getCurrentTimezone;
				scope.vm.scheduleMgtSvc = containerCtrl.scheduleManagementSvc;

				scope.vm.moveToTime = selfCtrl.getDefaultMoveToStartTime();
				scope.vm.nextDay = moment(selfCtrl.getDefaultMoveToStartTime()).format('YYYY-MM-DD') !== moment(scope.vm.selectedDate()).format('YYYY-MM-DD');

				scope.$on('teamSchedule.command.focus.default', function () {
					var focusTarget = elem[0].querySelector('.focus-default input');
					if (focusTarget) angular.element(focusTarget).focus();
				});

				scope.$watch(function () {
					return scope.vm.getMoveToStartTimeStr();
				},
					function (newVal, oldVal) {
						scope.vm.updateInvalidAgents();
					},
					true);

				var inputs = elem[0].querySelectorAll('input[type=text]');
				angular.forEach(inputs, function (input) {
					angular.element(input).on('focus', function (event) {
						event.target.select();
					});
				});
			}
		};
	}

	moveActivityCtrl.$inject = ['$attrs', '$locale', '$translate', 'ActivityService', 'PersonSelection',  'ScheduleHelper', 'teamScheduleNotificationService', 'ActivityValidator', 'CommandCheckService'];

	function moveActivityCtrl($attrs, $locale, $translate, activityService, personSelectionSvc, scheduleHelper, teamScheduleNotificationService, validator, CommandCheckService) {
		var vm = this;

		vm.label = 'MoveActivity';
		vm.showMeridian = /h:/.test($locale.DATETIME_FORMATS.shortTime);
		vm.meridians = vm.showMeridian ? $locale.DATETIME_FORMATS.AMPMS : [];
		vm.tabindex = angular.isDefined($attrs.tabindex) ? $attrs.tabindex : 0;
		vm.checkingCommand = false;
		vm.selectedAgents = personSelectionSvc.getSelectedPersonInfoList();
		vm.invalidAgents = [];

		vm.getDefaultMoveToStartTime = function() {
			var curDateMoment = moment(vm.selectedDate());
			var personIds = vm.selectedAgents.map(function(agent) { return agent.PersonId; });
			var schedules = vm.containerCtrl.scheduleManagementSvc.schedules();

			var selectedDateProjectionLatestStart = scheduleHelper.getLatestStartTimeOfSelectedSchedulesProjections(schedules, curDateMoment, personIds);
			var previousDateProjectionLatestEnd = scheduleHelper.getLatestPreviousDayOvernightShiftEnd(schedules, curDateMoment, personIds);
			var time = new Date();

			time = selectedDateProjectionLatestStart != null ? selectedDateProjectionLatestStart : time;
			time = previousDateProjectionLatestEnd != null && previousDateProjectionLatestEnd > time ? previousDateProjectionLatestEnd : time;

			return moment(time).add(1, 'hour').toDate();
		};

		vm.anyValidAgent = function() {
			return vm.invalidAgents.length !== vm.selectedAgents.length;
		}

		vm.anyInvalidAgent = function () {
			return vm.invalidAgents.length > 0;
		};

		vm.updateInvalidAgents = function () {
			var currentTimezone = vm.getCurrentTimezone();
			validator.validateMoveToTime(vm.scheduleMgtSvc, moment(vm.getMoveToStartTimeStr()), currentTimezone);
			vm.invalidAgents = validator.getInvalidPeople();

		};

		vm.invalidPeople = function () {
			var people = validator.getInvalidPeopleNameList().join(', ');
			return people;
		};

		vm.getMoveToStartTimeStr = function () {
			var dateStr = (vm.nextDay ? moment(vm.selectedDate()).add(1, 'days') : moment(vm.selectedDate())).format('YYYY-MM-DD');
			var timeStr = moment(vm.moveToTime).format('HH:mm');
			return dateStr + 'T' + timeStr;
		};

	    function moveActivity(requestData) {
	    	if(requestData.PersonActivities.length > 0){
				activityService.moveActivity(requestData).then(function (response) {
					var personIds = requestData.PersonActivities.map(function (agent) { return agent.PersonId; });
					if (vm.getActionCb(vm.label)) {
						vm.getActionCb(vm.label)(vm.trackId, personIds);
					}
					teamScheduleNotificationService.reportActionResult({
						success: 'SuccessfulMessageForMovingActivity',
						warning: 'PartialSuccessMessageForMovingActivity'
					}, vm.selectedAgents.map(function (x) {
						return {
							PersonId: x.PersonId,
							Name: x.Name
						}
					}), response.data);

					vm.checkingCommand = false;
				});
			}else{
				if (vm.getActionCb(vm.label)) {
					vm.getActionCb(vm.label)(vm.trackId, []);
				}
				vm.checkingCommand = false;
			}
	    }

	    function getRequestData() {
		    var invalidPersonIds = vm.invalidAgents.map(function(p) { return p.PersonId });

	    	var validAgents = vm.selectedAgents.filter(function(agent) {
			    return invalidPersonIds.indexOf(agent.PersonId) < 0;
		    }).filter(function (x) {
	    		return (angular.isArray(x.SelectedActivities) && x.SelectedActivities.length === 1);
	    	});

			var personActivities = [];
			angular.forEach(validAgents, function (agent) {
				var selectedActivities = agent.SelectedActivities;
				var groupedActivitiesByDate = {};
				selectedActivities.forEach(function(a) {
					if (!groupedActivitiesByDate[a.date]) {
						groupedActivitiesByDate[a.date] = [];
					}
					if (groupedActivitiesByDate[a.date].indexOf(a.shiftLayerId) === -1) {
						groupedActivitiesByDate[a.date].push(a.shiftLayerId);
					}
				});
				for (var date in groupedActivitiesByDate) {
					personActivities.push({
						PersonId: agent.PersonId,
						Date: date,
						ShiftLayerIds: groupedActivitiesByDate[date]
					});
				}
			});

			var requestData = {
				PersonActivities: personActivities,
				StartTime: vm.convertTime(vm.getMoveToStartTimeStr()),
				TrackedCommandInfo: { TrackId: vm.trackId }
			};

		    return requestData;
	    }

	    vm.moveActivity = function () {
		    var requestData = getRequestData();
		    var multiActivitiesSelectedAgentsList = vm.selectedAgents.filter(function (x) {
			    return (angular.isArray(x.SelectedActivities) && x.SelectedActivities.length > 1);
		    });

		    if (multiActivitiesSelectedAgentsList.length > 0) {
			    var errorMessage = $translate.instant('CanNotMoveMultipleActivitiesForSelectedAgents') + ": " + multiActivitiesSelectedAgentsList.map(function (agent) { return agent.Name; }).join(", ") + ".";
			    teamScheduleNotificationService.notify('error', errorMessage);
			    vm.getActionCb(vm.label) && vm.getActionCb(vm.label)(null, null);
			    return;
		    }
		    vm.checkingCommand = true;
		    CommandCheckService.checkMoveActivityOverlapping(requestData).then(function (data) {
			    moveActivity(data);
		    });
	    }
	}
})();
