(function () {
	'use strict';

	angular
		.module('wfm.rtaShared')
		.service('rtaPollingService', rtaPollingService);

	rtaPollingService.$inject = ['$timeout'];

	function rtaPollingService($timeout) {

		return {

			create: function (call) {

				var _destroyed = false;
				var _scheduledCall;

				return {
					start: start,
					force: force,
					destroy: destroy
				};

				function start() {
					if (_destroyed)
						return;
					callAndScheduleCall();
				}

				function force() {
					stop();
					start();
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
					call().finally(scheduleCall);
				}

				function scheduleCall() {
					if (!_destroyed)
						_scheduledCall = $timeout(callAndScheduleCall, 5000);
				}
			}

		};

	}

})();

