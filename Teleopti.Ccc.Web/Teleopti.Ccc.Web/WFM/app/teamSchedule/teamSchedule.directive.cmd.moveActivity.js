﻿(function () {
	'use strict';

	angular.module('wfm.teamSchedule').directive('moveActivity', moveActivityDirective);

	moveActivityCtrl.$inject = ['$attrs', '$locale', '$translate', 'ActivityService', 'PersonSelection',  'ScheduleManagement', 'teamScheduleNotificationService', 'MoveActivityValidator', 'CommandCheckService'];

	function moveActivityCtrl($attrs, $locale, $translate, activityService, personSelectionSvc, scheduleManagementSvc, teamScheduleNotificationService, validator, CommandCheckService) {
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
			var selectedDateProjectionLatestStart = scheduleManagementSvc.getLatestStartTimeOfSelectedScheduleProjection(curDateMoment, personIds);
			var previousDateProjectionLatestEnd = scheduleManagementSvc.getLatestPreviousDayOvernightShiftEnd(curDateMoment, personIds);
			var time = new Date();

			time = selectedDateProjectionLatestStart != null ? selectedDateProjectionLatestStart : time;
			time = previousDateProjectionLatestEnd != null && previousDateProjectionLatestEnd > time ? previousDateProjectionLatestEnd : time;

			return moment(time).add(1, 'hour').toDate();
		};

		vm.anyValidAgent = function() {
			return vm.invalidAgents.length !== vm.selectedAgents.length;
		}

		vm.updateInvalidAgents = function () {
			var currentTimezone = vm.getCurrentTimezone();
			validator.validateMoveToTime(moment(vm.getMoveToStartTimeStr()), currentTimezone);
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
	    };

	    function getRequestData() {
	    	vm.selectedAgents = personSelectionSvc.getSelectedPersonInfoList();
		    var invalidPersonIds = vm.invalidAgents.map(function(p) { return p.PersonId });

	    	var personProjectionsWithSelectedActivities = vm.selectedAgents.filter(function(agent) {
			    return invalidPersonIds.indexOf(agent.PersonId) < 0;
		    }).filter(function (x) {
	    		return (Array.isArray(x.SelectedActivities) && x.SelectedActivities.length === 1);
	    	});

			var requestData = {
				Date: vm.selectedDate(),
				PersonActivities: personProjectionsWithSelectedActivities.map(function (x) {
					return { PersonId: x.PersonId, ShiftLayerIds: x.SelectedActivities };
				}),
				StartTime: vm.convertTime(vm.getMoveToStartTimeStr()),
				TrackedCommandInfo: { TrackId: vm.trackId }
			};

		    return requestData;
	    }

	    vm.moveActivity = function () {
		    var requestData = getRequestData();
	    	var multiActivitiesSelectedAgentsList = vm.selectedAgents.filter(function (x) {
	    		return (Array.isArray(x.SelectedActivities) && x.SelectedActivities.length > 1);
	    	});

	    	if (multiActivitiesSelectedAgentsList.length > 0) {
	    		var errorMessage = $translate.instant('CanNotMoveMultipleActivitiesForSelectedAgents') + ": " + multiActivitiesSelectedAgentsList.map(function(agent){return agent.Name;}).join(", ") + ".";
	    		teamScheduleNotificationService.notify('error', errorMessage);
	    		vm.getActionCb(vm.label) && vm.getActionCb(vm.label)(null, null);
	    		return;
	    	}

			if (vm.checkCommandActivityLayerOrders){
				vm.checkingCommand = true;
				CommandCheckService.checkMoveActivityOverlapping(requestData).then(function(data) {
					moveActivity(data);
				});
			}
			else
				moveActivity(requestData);
		}
	}

	function moveActivityDirective() {
		return {
			restrict: 'E',
			scope: {},
			controller: moveActivityCtrl,
			controllerAs: 'vm',
			bindToController: true,
			templateUrl: 'app/teamSchedule/html/moveActivity.tpl.html',
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
					tElement[0].querySelectorAll('[uib-timepicker]'),
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
			scope.vm.convertTime = containerCtrl.convertTimeToCurrentUserTimezone;
			scope.vm.getActionCb = containerCtrl.getActionCb;
			scope.vm.getCurrentTimezone = containerCtrl.getCurrentTimezone;

			scope.vm.moveToTime = selfCtrl.getDefaultMoveToStartTime();
			scope.vm.nextDay = moment(selfCtrl.getDefaultMoveToStartTime()).format('YYYY-MM-DD') !== moment(scope.vm.selectedDate()).format('YYYY-MM-DD');
			scope.vm.checkCommandActivityLayerOrders = containerCtrl.hasToggle('CheckOverlappingCertainActivitiesEnabled');

			scope.$on('teamSchedule.command.focus.default', function () {
				var focusTarget = elem[0].querySelector('.focus-default input');
				if (focusTarget) angular.element(focusTarget).focus();
			});

			scope.$watch(function() {
					return scope.vm.moveToTime;
				},
				function(newVal, oldVal) {
					scope.vm.updateInvalidAgents();
				},
				true);

			elem.removeAttr('tabindex');

			var inputs = elem[0].querySelectorAll('input[type=text]');
			angular.forEach(inputs, function (input) {
				angular.element(input).on('focus', function (event) {
					event.target.select();
				});
			});
		}
	}

})();
