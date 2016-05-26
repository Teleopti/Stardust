(function () {
	'use strict';

	angular.module('wfm.teamSchedule').directive('moveActivity', moveActivityDirective);

	moveActivityCtrl.$inject = ['ActivityService', 'PersonSelection', 'WFMDate', 'ScheduleManagement', 'teamScheduleNotificationService', 'MoveActivityValidator'];

	function moveActivityCtrl(activityService, personSelectionSvc, wFMDateSvc, scheduleManagementSvc, teamScheduleNotificationService, validator) {
		var vm = this;

		vm.label = 'MoveActivity';

		vm.selectedAgents = personSelectionSvc.getSelectedPersonInfoList();
	
		vm.getDefaultMoveToStartTime = function () {
			var curDateMoment = moment(vm.selectedDate());
			var personIds = vm.selectedAgents.map(function (agent) { return agent.personId; });
			var time = scheduleManagementSvc.getLatestStartTimeOfSelectedScheduleProjection(curDateMoment, personIds) || new Date();
			return moment(time).add(1, 'hour').toDate();		
		}

		vm.isInputValid = function() {
			return validator.validateMoveToTime(moment(vm.getMoveToStartTimeStr()));
		}
	
		vm.invalidPeople = function () {
			var people = validator.getInvalidPeople().join(', ');
			return people;
		};

		vm.getMoveToStartTimeStr = function() {
			var dateStr = (vm.nextDay ? moment(vm.selectedDate()).add(1, 'days') : moment(vm.selectedDate())).format('YYYY-MM-DD');
			var timeStr = moment(vm.bindingTime).format('HH:mm');
			return dateStr + 'T' + timeStr;
		}

		vm.moveActivity = function () {			
			var personProjectionsWithSelectedActivities = vm.selectedAgents.filter(function (x) {
				return (Array.isArray(x.selectedActivities) && x.selectedActivities.length > 0);
			});
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
					vm.getActionCb(vm.label)(vm.TrackId, personIds);
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
			link: postlink
		}

		function postlink(scope, elem, attrs, ctrls) {
			var containerCtrl = ctrls[0],
				selfCtrl = ctrls[1];

			scope.vm.selectedDate = containerCtrl.getDate;
			scope.vm.trackId = containerCtrl.getTrackId();
			scope.vm.getActionCb = containerCtrl.getActionCb;

			scope.vm.bindingTime = selfCtrl.getDefaultMoveToStartTime();
			scope.vm.nextDay = moment(selfCtrl.getDefaultMoveToStartTime()).format('YYYY-MM-DD') !== moment(scope.vm.selectedDate()).format('YYYY-MM-DD');


			scope.$on('teamSchedule.command.focus.default', function () {
				var focusTarget = elem[0].querySelector('.focus-default');
				if (focusTarget) angular.element(focusTarget).focus();
			});			
		}
	}



})();