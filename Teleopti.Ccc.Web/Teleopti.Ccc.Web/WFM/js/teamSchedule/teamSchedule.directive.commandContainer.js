(function() {
	angular.module('wfm.teamSchedule').directive('teamscheduleCommandContainer', teamscheduleCommandContainer);

	teamscheduleCommandContainerCtrl.$inject = ['guidgenerator'];

	function teamscheduleCommandContainerCtrl(guidgenerator) {
		var vm = this;

		vm.getDate = function () { return vm.date; };
		vm.getTrackId = guidgenerator.newGuid;
		

		vm.activeCmd = null;

		vm.setActiveCmd = function(label) {
			vm.activeCmd = label;
		};

		vm.resetActiveCmd = function() { vm.activeCmd = null; };

		vm.getActionCb = function (_) {
			var returnFn = function(trackId, personIds) {
				vm.resetActiveCmd();
				if (vm.actionCallback) {
					vm.actionCallback({
						trackId: trackId,
						personIds: personIds
					});
				}
			};
			return returnFn;
		};

		vm.setActionCb = function (_, cb) {
			vm.actionCallback = cb;
		};

		vm.hasPermission = function(permission) {
			if (!vm.configurations || !vm.configurations.permissions) return false;
			return vm.configurations.permissions[permission];
		};

		vm.hasToggle = function(toggle) {
			if (!vm.configurations || !vm.configurations.toggles) return false;
			return vm.configurations.toggles[toggle];
		};

	}

	function teamscheduleCommandContainer() {
		return {
			restrict: 'E',
			controller: teamscheduleCommandContainerCtrl,
			scope: {
				date: '=',
				actionCallback: '&?',
				configurations: '=?'
			},
			controllerAs: 'vm',
			bindToController: true,
			templateUrl: 'js/teamSchedule/html/teamscheduleCommandContainer.html',
			link: postlink
		}

		function postlink(scope, elem) {
			scope.$on('teamSchedule.init.command', function (e, d) {
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