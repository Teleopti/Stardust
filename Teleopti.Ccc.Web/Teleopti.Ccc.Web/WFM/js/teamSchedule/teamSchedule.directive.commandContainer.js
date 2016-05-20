(function() {
	angular.module('wfm.teamSchedule').directive('teamscheduleCommandContainer', teamscheduleCommandContainer);

	function teamscheduleCommandContainer() {
		return {
			restrict: 'E',
			controller: teamscheduleCommandContainerCtrl,
			scope: {
				date: '='
			},
			controllerAs: 'vm',
			bindToController: true,
			templateUrl: 'js/teamSchedule/html/teamscheduleCommandContainer.html'
		}
	}

	teamscheduleCommandContainerCtrl.$inject = ['guidgenerator'];

	function teamscheduleCommandContainerCtrl(guidgenerator) {
		var vm = this;
		vm.getDate = function () { return vm.date; };
		vm.getTrackId = guidgenerator.newGuid;

	}
})();