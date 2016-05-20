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

		var commandsConfig = {};

		vm.getDate = function () { return vm.date; };
		vm.getTrackId = guidgenerator.newGuid;
		vm.getActionCb = function(label) {
			return commandsConfig[label] == null ? null : commandsConfig[label].actionCb;
		};

		vm.setActionCb = function(label, cb) {
			commandsConfig[label] = {
				actionCb: cb
			};
		};

	}
})();