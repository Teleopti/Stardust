﻿(function() {
	'use strict';

	angular.module('wfm.requests')
		.service('requestsNotificationService', ['NoticeService', '$translate', '$q',  requestsNotificationService]);

	function requestsNotificationService(NoticeService, $translate, $q) {
		//ROBTODO: instant translation rather than using $q?
		var displayStandardSuccess = function(changedRequestsCount, requestsCount, affectedResource, notAffectedResource) {
			
			$q.all([$translate(affectedResource), $translate(notAffectedResource)]).then(function (texts) {
				var changedRequestsText = texts[0];
				var notChangedRequestsText = texts[1];

				if (changedRequestsCount == 0) {
					NoticeService.error(notChangedRequestsText.replace('{0}', requestsCount - changedRequestsCount), 5000, true);
				} else if (changedRequestsCount < requestsCount) {
					NoticeService.warning(changedRequestsText.replace('{0}', changedRequestsCount) + " " + notChangedRequestsText.replace('{0}', requestsCount - changedRequestsCount), 5000, true);
				} else {
					NoticeService.success(changedRequestsText.replace('{0}', changedRequestsCount), 5000, true);
				}
			});

		}

		this.notifyApproveRequestsSuccess = function (changedRequestsCount, requestsCount) {
			displayStandardSuccess(changedRequestsCount, requestsCount, "RequestsHaveBeenApproved", "RequestsHaveNotBeenApproved");
		}

		this.notifyDenyRequestsSuccess = function (changedRequestsCount, requestsCount) {
			displayStandardSuccess(changedRequestsCount, requestsCount, "RequestsHaveBeenDenied", "RequestsHaveNotBeenDenied");
		}

		this.notifyCancelledRequestsSuccess = function (changedRequestsCount, requestsCount) {
			displayStandardSuccess(changedRequestsCount, requestsCount, "RequestsHaveBeenCancelled", "RequestsHaveNotBeenCancelled");
		}

		this.notifyCommandError = function (error) {
			$translate("CommandFailed").then(function (text) {
				NoticeService.error(text.replace('{0}', error), 5000, true);
			});
		}


	}

})();
