(function() {
	'use strict';

	angular.module('wfm.teamSchedule')
		.service('teamScheduleNotificationService', ['growl', teamScheduleNotificationService]);

	function teamScheduleNotificationService($growl) {
		function notifySuccess(message) {
			$growl.success("<i class='mdi mdi-thumb-up'></i> " + message, {
				ttl: 5000,
				disableCountDown: true
			});
		}

		function notifyFailure(message) {
			$growl.error("<i class='mdi  mdi-alert-octagon'></i> " + message, {
				ttl: 5000,
				disableCountDown: true
			});
		}

		function notifyWarning(message) {
			$growl.warning("<i class='mdi  mdi-alert'></i> " + message, {
				ttl: 5000,
				disableCountDown: true
			});
		}

		this.notifySuccess = notifySuccess;
		this.notifyFailure = notifyFailure;
		this.notifyWarning = notifyWarning;
	}
})();