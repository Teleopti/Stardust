(function () {
	'use strict';

	angular.module('wfm.teamSchedule').directive('moveActivity', moveActivityDirective);

	moveActivityCtrl.$inject = ['$attrs', '$locale', '$translate', 'ActivityService', 'PersonSelection', 'WFMDate', 'ScheduleManagement', 'teamScheduleNotificationService', 'MoveActivityValidator'];

	function moveActivityCtrl($attrs, $locale, $translate, activityService, personSelectionSvc, wFMDateSvc, scheduleManagementSvc, teamScheduleNotificationService, validator) {
		var vm = this;

		vm.label = 'MoveActivity';
		vm.showMeridian = /h:/.test($locale.DATETIME_FORMATS.shortTime);
		vm.meridians = vm.showMeridian ? $locale.DATETIME_FORMATS.AMPMS : [];
		vm.tabindex = angular.isDefined($attrs.tabindex) ? $attrs.tabindex : 0;

		vm.selectedAgents = personSelectionSvc.getSelectedPersonInfoList();

		vm.getDefaultMoveToStartTime = function () {
			var curDateMoment = moment(vm.selectedDate());
			var personIds = vm.selectedAgents.map(function (agent) { return agent.personId; });
			var selectedDateProjectionLatestStart = scheduleManagementSvc.getLatestStartTimeOfSelectedScheduleProjection(curDateMoment, personIds);
			var previousDateProjectionLatestEnd = scheduleManagementSvc.getLatestPreviousDayOvernightShiftEnd(curDateMoment, personIds);
			var time = new Date();

			time = selectedDateProjectionLatestStart != null ? selectedDateProjectionLatestStart : time;
			time = previousDateProjectionLatestEnd != null && previousDateProjectionLatestEnd > time ? previousDateProjectionLatestEnd : time;

			return moment(time).add(1, 'hour').toDate();
		}

		vm.isInputValid = function () {
			return validator.validateMoveToTime(moment(vm.getMoveToStartTimeStr()));
		}

		vm.invalidPeople = function () {
			var people = validator.getInvalidPeople().join(', ');
			return people;
		};

		vm.getMoveToStartTimeStr = function () {
			var dateStr = (vm.nextDay ? moment(vm.selectedDate()).add(1, 'days') : moment(vm.selectedDate())).format('YYYY-MM-DD');
			var timeStr = moment(vm.moveToTime).format('HH:mm');
			return dateStr + 'T' + timeStr;
		}

		vm.moveActivity = function () {
			var isMultiAcitvitiesSelectForOneAgent = false;
			var multiActivitiesSelectedAgentsList = [];
			var personProjectionsWithSelectedActivities = vm.selectedAgents.filter(function (x) {
				if (x.selectedActivities.length > 1) {
					isMultiAcitvitiesSelectForOneAgent = true;
					multiActivitiesSelectedAgentsList.push(x.name);
				}
				return (Array.isArray(x.selectedActivities) && x.selectedActivities.length == 1);
			});

			if (isMultiAcitvitiesSelectForOneAgent) {
				var errorMessage = $translate.instant('CanNotMoveMultipleActivitiesForSelectedAgents') + " " + multiActivitiesSelectedAgentsList.join(", ") + ".";
				teamScheduleNotificationService.notify('error', errorMessage);
				vm.getActionCb(vm.label) && vm.getActionCb(vm.label)(null, null);
				return;
			}

			var personIds = vm.selectedAgents.map(function (agent) { return agent.personId; });

			var requestData = {
				Date: vm.selectedDate(),
				PersonActivities: personProjectionsWithSelectedActivities.map(function (x) {
					return { PersonId: x.personId, ShiftLayerIds: x.selectedActivities };
				}),
				StartTime: vm.getMoveToStartTimeStr(),
				TrackedCommandInfo: { TrackId: vm.trackId }
			};

			activityService.moveActivity(requestData).then(function (response) {
				if (vm.getActionCb(vm.label)) {
					vm.getActionCb(vm.label)(vm.trackId, personIds);
				}
				teamScheduleNotificationService.reportActionResult({
					success: 'SuccessfulMessageForMovingActivity',
					warning: 'PartialSuccessMessageForMovingActivity'
				}, vm.selectedAgents.map(function (x) {
					return {
						PersonId: x.personId,
						Name: x.name
					}
				}), response.data);

			});
		};
	}

	function moveActivityDirective() {
		return {
			restrict: 'E',
			scope: {},
			controller: moveActivityCtrl,
			controllerAs: 'vm',
			bindToController: true,
			templateUrl: 'js/teamSchedule/html/moveActivity.tpl.html',
			require: ['^teamscheduleCommandContainer', 'moveActivity'],
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
					tElement[0].querySelectorAll('uib-timepicker'),
					tElement[0].querySelectorAll('input[type=checkbox]#mvActNextDay'),
					tElement[0].querySelectorAll('button#applyMoveActivity')
				);
				return postlink;
			},
		}

		function postlink(scope, elem, attrs, ctrls) {
			var containerCtrl = ctrls[0],
				selfCtrl = ctrls[1];

			scope.vm.selectedDate = containerCtrl.getDate;
			scope.vm.trackId = containerCtrl.getTrackId();
			scope.vm.getActionCb = containerCtrl.getActionCb;

			scope.vm.moveToTime = selfCtrl.getDefaultMoveToStartTime();
			scope.vm.nextDay = moment(selfCtrl.getDefaultMoveToStartTime()).format('YYYY-MM-DD') !== moment(scope.vm.selectedDate()).format('YYYY-MM-DD');

			scope.$on('teamSchedule.command.focus.default', function () {
				var focusTarget = elem[0].querySelector('.focus-default input');
				if (focusTarget) angular.element(focusTarget).focus();
			});

			elem.removeAttr('tabindex');
		}
	}



})();
