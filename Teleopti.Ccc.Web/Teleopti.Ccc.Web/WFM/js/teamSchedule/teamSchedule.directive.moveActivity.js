(function() {

	'use strict';

	angular.module('wfm.teamSchedule').directive('moveActivityPanel', moveActivityPanel);

	function moveActivityPanel() {
		return {
			restrict: 'E',
			scope: {
				selectedDate: '&',
				defaultStart:'&?',
				actionsAfterActivityApply: '&?'
			},
			templateUrl: 'js/teamSchedule/html/moveActivity.html',
			controller: moveActivityPanelCtrl,
			controlerAs: 'vm'
		};
	}

	function moveActivityPanelCtrl() {
		var vm = this;
		vm.disableNextDay = false;
		vm.isNextDay = false;

		var startTimeMoment;
		if (vm.defaultStart) {
			startTimeMoment = moment(moment(vm.selectedDate()).format("YYYY-MM-DD") + " " + vm.defaultStart());
		} else {
			startTimeMoment = moment();
		}
		var endTimeMoment = moment(startTimeMoment).add(1, 'hour');
		vm.timeRange = {
			startTime: startTimeMoment.toDate(),
			endTime: endTimeMoment.toDate()
		};
	}
})();
