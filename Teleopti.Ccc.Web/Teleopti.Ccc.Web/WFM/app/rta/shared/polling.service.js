(function () {
	'use strict';

	angular
		.module('wfm.rtaShared')
		.service('rtaPollingService', rtaPollingService);

	rtaPollingService.$inject = ['$timeout'];

	function rtaPollingService($timeout) {

		return {

			create: function (call, interval) {
				var _destroyed = false;
				var _scheduledCall;

				var poller = {
					start: start,
					force: force,
					forceSoon: forceSoon,
					destroy: destroy
				};

				return poller;

				function start() {
					if (_destroyed)
						return;
					callAndScheduleCall();
					return poller;
				}

				function force() {
					stop();
					start();
				}

				function forceSoon() {
					stop();
					scheduleCall(1000);
					return poller;
				}

				function stop() {
					if (_scheduledCall) {
						$timeout.cancel(_scheduledCall);
						_scheduledCall = null;
					}
				}

				function destroy() {
					_destroyed = true;
					stop();
				}

				function callAndScheduleCall() {
					_scheduledCall = null;
					call().finally(function () {
						// dont reschedule if there's already one scheduled
						// this can happen because cancelation doesnt always prevent the call from occurring
						if (_scheduledCall) 
							return;
						scheduleCall();
					});
				}

				function scheduleCall(timeout) {
					if (!_destroyed)
						_scheduledCall = $timeout(callAndScheduleCall, timeout || interval || 5000);
				}
			}
		};
	}
})();

