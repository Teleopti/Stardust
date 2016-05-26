(function () {

	'use strict';

	angular.module('wfm.teamSchedule').directive('swapShifts', swapShiftsDirective);

	swapShiftsCtrl.$inject = ['SwapShifts', 'PersonSelection', 'teamScheduleNotificationService'];

	function swapShiftsCtrl(SwapShiftsSvc, PersonSelection, notification) {
		var vm = this;
		vm.label = 'SwapShifts';

		vm.selectedPersonInfo = PersonSelection.getCheckedPersonInfoList();
		vm.swapShifts = function () {

			var personIds = vm.selectedPersonInfo.map(function(x) { return x.personId; });

			var requestData = {
				PersonIdFrom: personIds[0],
				PersonIdTo: personIds[1],
				ScheduleDate: vm.selectedDate(),
				TrackedCommandInfo: { TrackId: vm.trackId }				
			}

			SwapShiftsSvc.swapShifts(requestData).then(function (response) {
				if (vm.getActionCb(vm.label)) {
					vm.getActionCb(vm.label)(vm.TrackId, personIds);
				}

				notification.reportActionResult({
					"success": 'FinishedSwapShifts',
					"warning": 'FailedToSwapShifts'
				}, vm.selectedPersonInfo.map(function (x) {
					return {
						PersonId: x.personId,
						Name: x.name
					}
				}), response.data);
			});
		}

		vm.init = function () {
			vm.swapShifts();
		}
	}


	function swapShiftsDirective() {
		return {
			restrict: 'E',
			scope: {},
			controller: swapShiftsCtrl,
			controllerAs: 'vm',
			bindToController: true,
			require: ['^teamscheduleCommandContainer', 'swapShifts'],
			link: postlink
		}

		function postlink(scope, elem, attrs, ctrls) {
			var containerCtrl = ctrls[0],
				selfCtrl = ctrls[1];

			scope.vm.selectedDate = containerCtrl.getDate;
			scope.vm.trackId = containerCtrl.getTrackId();
			scope.vm.getActionCb = containerCtrl.getActionCb;
			scope.vm.resetActiveCmd = containerCtrl.resetActiveCmd;

			selfCtrl.init();
		}

	}


})();