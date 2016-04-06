(function() {
	'use strict';

	angular.module('wfm.teamSchedule')
		.service('teamScheduleNotificationService', ['NoticeService', teamScheduleNotificationService]);

	function teamScheduleNotificationService(NoticeService) {
		function notifySuccess(message) {
			NoticeService.success(message, 5000, true);
		}

		function notifyFailure(message) {
			NoticeService.error(message, 5000, true);
		}

		function notifyWarning(message) {
			NoticeService.warning(message, 5000, true);
		}

		this.notifySuccess = notifySuccess;
		this.notifyFailure = notifyFailure;
		this.notifyWarning = notifyWarning;
	}
})();
