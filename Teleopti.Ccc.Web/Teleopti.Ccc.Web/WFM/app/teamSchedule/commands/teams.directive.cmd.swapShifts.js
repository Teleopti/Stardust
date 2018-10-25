(function () {

	'use strict';

	angular.module('wfm.teamSchedule').directive('swapShifts', swapShiftsDirective);

	function swapShiftsDirective() {
		return {
			restrict: 'E',
			scope: {},
			controller: swapShiftsCtrl,
			controllerAs: 'vm',
			bindToController: true,
			require: ['^teamscheduleCommandContainer', 'swapShifts'],
			link: function (scope, elem, attrs, ctrls) {
				var containerCtrl = ctrls[0],
					selfCtrl = ctrls[1];

				scope.vm.selectedDate = containerCtrl.getDate;
				scope.vm.trackId = containerCtrl.getTrackId();
				scope.vm.getActionCb = containerCtrl.getActionCb;
				scope.vm.resetActiveCmd = containerCtrl.resetActiveCmd;

				selfCtrl.init();
			}
		};
	}

	swapShiftsCtrl.$inject = ['$scope', '$translate', 'SwapShifts', 'PersonSelection', 'teamScheduleNotificationService'];

	function swapShiftsCtrl($scope, $translate, SwapShiftsSvc, PersonSelection, notification) {
		var vm = this;
		vm.label = 'SwapShifts';

		vm.selectedPersonInfo = PersonSelection.getCheckedPersonInfoList();
		vm.swapShifts = function () {

			var personIds = vm.selectedPersonInfo.map(function (x) { return x.PersonId; });

			var requestData = {
				PersonIdFrom: personIds[0],
				PersonIdTo: personIds[1],
				ScheduleDate: vm.selectedDate(),
				TrackedCommandInfo: { TrackId: vm.trackId }
			}
			$scope.$emit('teamSchedule.show.loading');
			SwapShiftsSvc.swapShifts(requestData).then(function (response) {
				$scope.$emit('teamSchedule.hide.loading');
				if (vm.getActionCb(vm.label)) {
					vm.getActionCb(vm.label)(vm.trackId, personIds);
				}

				notification.reportActionResult({
					"success": $translate.instant('FinishedSwapShifts'),
					"warning": $translate.instant('FailedToSwapShifts')
				}, vm.selectedPersonInfo.map(function (x) {
					return {
						PersonId: x.PersonId,
						Name: x.Name
					}
				}), response.data);
			});
		};

		vm.init = function () {
			vm.swapShifts();
		};
	}
})();