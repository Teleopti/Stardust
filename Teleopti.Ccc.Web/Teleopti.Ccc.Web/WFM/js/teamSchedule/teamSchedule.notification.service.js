(function() {
	'use strict';

	angular.module('wfm.teamSchedule')
		.service('teamScheduleNotificationService', ['growl', '$translate', outboundNotificationService]);

	function outboundNotificationService($growl, $translate) {

		this.notifySuccess = notifySuccess;
		this.notifyFailure = notifyFailure;
		this.notifyWarning = notifyWarning;

		this.notifyAllAbsenceAddedSuccessed = function(count) {
			notifySuccess('AddAbsenceSuccessedResult', [count]);
		};

		this.notifAbsenceAddedFailed = function(totalCount, successCount, failCount) {
			notifySuccess('AddAbsenceTotalResult', [totalCount, successCount, failCount]);
		};

		function notifySuccess(message, params) {
			$translate(message).then(function (text) {
				params.forEach(function(element, index) {
					text = text.replace('{' + index + '}', element);
				});
				$growl.success("<i class='mdi mdi-thumb-up'></i> "
					+ text, {
						ttl: 5000,
						disableCountDown: true
					});
			});
		}

		function notifyFailure(message, params) {
			$translate(message).then(function(text) {
				$growl.error("<i class='mdi  mdi-alert-octagon'></i> "
					+ text.replace('{0}', params[0]), {
						ttl: 5000,
						disableCountDown: true
					});
			});
		}

		function notifyWarning(message, params) {
			$translate(message).then(function(text) {
				$growl.warning("<i class='mdi  mdi-alert'></i> "
					+ text.replace('{0}', params[0]), {
						ttl: 5000,
						disableCountDown: true
					});
			});
		}
	}
})();