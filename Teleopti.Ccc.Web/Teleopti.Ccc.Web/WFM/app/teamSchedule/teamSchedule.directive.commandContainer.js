﻿(function () {
	angular.module('wfm.teamSchedule').directive('teamscheduleCommandContainer', teamscheduleCommandContainer);

	teamscheduleCommandContainerCtrl.$inject = ['$filter',  'guidgenerator', 'CommandCheckService'];

	function teamscheduleCommandContainerCtrl($filter, guidgenerator, CommandCheckService) {
		var vm = this;

		vm.getDate = function () {
			return moment(vm.date).format('YYYY-MM-DD');
		};

		vm.getCurrentTimezone = function () { return vm.timezone; };

		vm.convertTimeToCurrentUserTimezone = function(time) {
			return $filter('timezone')(time, null, vm.timezone);
		};

		vm.getTrackId = guidgenerator.newGuid;

		vm.activeCmd = null;

		vm.setActiveCmd = function (label) {
			vm.activeCmd = label;
		};

		vm.resetActiveCmd = function () { vm.activeCmd = null; };

		vm.getActionCb = function (_) {
			var returnFn = function (trackId, personIds) {
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

		vm.hasPermission = function (permission) {
			if (!vm.configurations || !vm.configurations.permissions) return false;
			return vm.configurations.permissions[permission];
		};

		vm.hasToggle = function (toggle) {
			if (!vm.configurations || !vm.configurations.toggles) return false;
			return vm.configurations.toggles[toggle];
		};

		vm.activeCommandCheck = function(){
			return CommandCheckService.getCommandCheckStatus();
		};
	}

	function teamscheduleCommandContainer() {
		return {
			restrict: 'E',
			controller: teamscheduleCommandContainerCtrl,
			scope: {
				date: '=',
				timezone: '=',
				actionCallback: '&?',
				configurations: '=?',
				onReady: '&'
			},
			controllerAs: 'vm',
			bindToController: true,
			templateUrl: 'app/teamSchedule/html/teamscheduleCommandContainer.html',
			link: postlink
		}

		function postlink(scope, elem) {
			scope.$on('teamSchedule.init.command', function (e, d) {
				scope.vm.setActiveCmd(d.activeCmd);
			});

			scope.$on('teamSchedule.reset.command', function (e, d) {
				scope.vm.resetActiveCmd();
			});
		}
	}
})();
