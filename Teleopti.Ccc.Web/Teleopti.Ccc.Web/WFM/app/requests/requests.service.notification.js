(function() {
	'use strict';

	angular
		.module('wfm.requests')
		.service('requestsNotificationService', ['NoticeService', '$translate', '$q', requestsNotificationService]);

	function requestsNotificationService(NoticeService, $translate, $q) {
		var displayStandardSuccess = function(
			changedRequestsCount,
			requestsCount,
			affectedResource,
			notAffectedResource
		) {
			var changedRequestsText = $translate.instant(affectedResource);
			var notChangedRequestsText = $translate.instant(notAffectedResource);

			if (changedRequestsCount == 0) {
				NoticeService.error(
					notChangedRequestsText.replace('{0}', requestsCount - changedRequestsCount),
					5000,
					true
				);
			} else if (changedRequestsCount < requestsCount) {
				NoticeService.warning(
					changedRequestsText.replace('{0}', changedRequestsCount) +
						' ' +
						notChangedRequestsText.replace('{0}', requestsCount - changedRequestsCount),
					5000,
					true
				);
			} else {
				NoticeService.success(changedRequestsText.replace('{0}', changedRequestsCount), 5000, true);
			}
		};

		this.notifyApproveRequestsSuccess = function(changedRequestsCount, requestsCount) {
			displayStandardSuccess(
				changedRequestsCount,
				requestsCount,
				'RequestsHaveBeenApproved',
				'RequestsHaveNotBeenApproved'
			);
		};

		this.notifyDenyRequestsSuccess = function(changedRequestsCount, requestsCount) {
			displayStandardSuccess(
				changedRequestsCount,
				requestsCount,
				'RequestsHaveBeenDenied',
				'RequestsHaveNotBeenDenied'
			);
		};

		this.notifyCancelledRequestsSuccess = function(changedRequestsCount, requestsCount) {
			displayStandardSuccess(
				changedRequestsCount,
				requestsCount,
				'RequestsHaveBeenCancelled',
				'RequestsHaveNotBeenCancelled'
			);
		};

		this.notifySubmitProcessWaitlistedRequestsSuccess = function(period) {
			NoticeService.success(
				$translate.instant('SubmitProcessWaitlistedRequestsSuccess').replace('{0}', period),
				10000,
				true
			);
		};

		this.notifySaveSiteOpenHoursSuccess = function(persistedSites) {
			NoticeService.success(
				$translate.instant('SaveSiteOpenHoursSuccess').replace('{0}', persistedSites),
				10000,
				true
			);
		};

		this.notifyNothingChanged = function() {
			NoticeService.info($translate.instant('NoChangeHasBeenMade'), 10000, false);
		};

		this.notifyProcessWaitlistedRequestsFinished = function(period) {
			NoticeService.success(
				$translate.instant('ProcessWaitlistedRequestsFinished').replace('{0}', period),
				10000,
				true
			);
		};

		this.notifySubmitApproveBasedOnBusinessRulesSuccess = function() {
			NoticeService.success($translate.instant('SubmitApproveBasedOnBusinessRulesSuccess'), 10000, true);
		};

		this.notifyApproveBasedOnBusinessRulesFinished = function() {
			NoticeService.success($translate.instant('ApproveBasedOnBusinessRulesFinished'), 10000, true);
		};

		this.notifyReplySuccess = function(replySuccessCount) {
			NoticeService.success($translate.instant('ReplySuccess').replace('{0}', replySuccessCount), 10000, true);
		};

		this.notifyCommandError = function(error) {
			NoticeService.error($translate.instant('CommandFailed').replace('{0}', error), 5000, true);
		};

		this.notifyMaxSearchPersonCountExceeded = function(maxSearchPersonCount) {
			NoticeService.error(
				$translate.instant('MaxSearchPersonCountExceeded').replace('{0}', maxSearchPersonCount),
				5000,
				true
			);
		};
	}
})();
