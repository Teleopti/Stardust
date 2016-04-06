(function() {
	'use strict';

	angular.module('wfm.requests')
		.service('requestsNotificationService', ['NoticeService', '$translate', '$q',  requestsNotificationService]);

	function requestsNotificationService(NoticeService, $translate, $q) {

		this.notifyApproveRequestsSuccess = function (changedRequestsCount, requestsCount) {

			$q.all([$translate('RequestsHaveBeenApproved'), $translate('RequestsHaveNotBeenApproved')]).then(function(texts) {
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

		this.notifyDenyRequestsSuccess = function (changedRequestsCount, requestsCount) {

			$q.all([$translate('RequestsHaveBeenDenied'), $translate('RequestsHaveNotBeenDenied')]).then(function (texts) {
				var changedRequestsText = texts[0];
				var notChangedRequestsText = texts[1];

				if (changedRequestsCount == 0) {
					NoticeService.error(notChangedRequestsText.replace('{0}', requestsCount - changedRequestsCount), 5000, true);
				} else if (changedRequestsCount < requestsCount) {
					NoticeService.warning(changedRequestsText.replace('{0}', changedRequestsCount) + " " + notChangedRequestsText.replace('{0}', requestsCount - changedRequestsCount),5000, true);

				} else {
					NoticeService.success(changedRequestsText.replace('{0}', changedRequestsCount), 5000, true);
				}
			});
		}

		this.notifyCommandError = function (error) {
			$translate("CommandFailed").then(function (text) {
				NoticeService.error(text.replace('{0}', error), 5000, true);
			});
		}


	}

})();
