(function() {
	angular.module('wfm.teamSchedule').directive('teamscheduleCommandContainer', teamscheduleCommandContainer);

	teamscheduleCommandContainerCtrl.$inject = ['guidgenerator'];

	function teamscheduleCommandContainerCtrl(guidgenerator) {
		var vm = this;

		var commandsConfig = {};

		vm.getDate = function () { return vm.date; };
		vm.getTrackId = guidgenerator.newGuid;
		vm.getActionCb = function (label) {
			return commandsConfig[label] == null ? null : commandsConfig[label].actionCb;
		};

		vm.activeCmd = null;

		vm.setActiveCmd = function (label) {
			vm.activeCmd = label;
		}

		vm.resetActiveCmd = function() { vm.activeCmd = null; }

		vm.setActionCb = function (label, cb) {
			commandsConfig[label] = {
				actionCb: cb
			};
		};

	}

	function teamscheduleCommandContainer() {
		return {
			restrict: 'E',
			controller: teamscheduleCommandContainerCtrl,
			scope: {
				date: '='
			},
			controllerAs: 'vm',
			bindToController: true,
			templateUrl: 'js/teamSchedule/html/teamscheduleCommandContainer.html',
			link: postlink
		}

		function postlink(scope, elem) {
			scope.$on('teamSchedule.init.command', function(e, d) {
				scope.vm.setActiveCmd(d.activeCmd);
			});
			scope.$on('teamSchedule.reset.command', function(e, d) {
				scope.vm.resetActiveCmd();
			});

			elem.on('focus', function () {
				scope.$broadcast('teamSchedule.command.focus.default');
			});
		}
	}

	
})();