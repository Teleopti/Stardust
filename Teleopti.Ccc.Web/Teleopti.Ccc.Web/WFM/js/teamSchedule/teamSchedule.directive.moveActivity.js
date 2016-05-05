(function () {
	'use strict';
	angular.module('wfm.teamSchedule').directive('moveActivityPanel', moveActivityPanel);

	function moveActivityPanel() {
		return {
			restrict: 'E',
			scope: {
				selectedDate: '&',
				defaultStart: '&?',
				actionsAfterActivityApply: '&?'
			},
			templateUrl: 'js/teamSchedule/html/moveActivity.html',
			controller: ['PersonSelection', moveActivityPanelCtrl],
			controllerAs: 'vm',
			bindToController: true
		};
	}

	function moveActivityPanelCtrl(personSelectionSvc) {
		var vm = this;
		vm.defaultStartTime = moment(vm.defaultStart()).add('hour', 1).toDate();
		vm.isNextDay = false;
		vm.disableNextDay = false;
		vm.disableButton = false;
		vm.selectedAgents = personSelectionSvc.getCheckedPersonInfoList();
		vm.applyMoveActivity = applyMoveActivity;
		vm.isInputValid = isInputValid;

		function isInputValid() {
			var isValid = false,
			notAllowed = "";
			var formattedStartTime = moment(vm.defaultStartTime).format('YYYY-MM-DD HH:mm');
			angular.forEach(vm.selectedAgents, function (selectedAgent) {
				if (selectedAgent.scheduleEndTime == undefined) return;
				var isAllowed = isNewActivityAllowed(formattedStartTime, selectedAgent.scheduleEndTime);
				if (!isAllowed) {
					isValid = false;
					if (notAllowed.indexOf(selectedAgent.name) == -1) {
						notAllowed += selectedAgent.name + ', ';
					}
				} else isValid = true;
			});

			return isValid;
		}

		function isNewActivityAllowed(activityStart, scheduleEnd) {
			var mActivityStart = moment(activityStart);
			var mScheduleEnd = moment(scheduleEnd);
			return !vm.isNextDay || mActivityStart.isSame(mScheduleEnd, 'day') && (mScheduleEnd.isAfter(mActivityStart));
		}

		function applyMoveActivity() {
		}
	}
})();
