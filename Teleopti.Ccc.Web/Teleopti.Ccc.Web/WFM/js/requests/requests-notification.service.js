(function() {
	'use strict';

	angular.module('wfm.requests')
		.service('requestsNotificationService', ['growl', '$translate', '$q',  requestsNotificationService]);

	function requestsNotificationService($growl, $translate, $q) {
		
		this.notifyApproveRequestsSuccess = function (changedRequestsCount, requestsCount) {

			$q.all([$translate('RequestsHaveBeenApproved'), $translate('RequestsHaveNotBeenApproved')]).then(function(texts) {
				var changedRequestsText = texts[0];
				var notChangedRequestsText = texts[1];

				if (changedRequestsCount == 0) {
					$growl.error(
						"<i class='mdi  mdi-alert-octagon'></i> "
						+ notChangedRequestsText.replace('{0}', requestsCount - changedRequestsCount)
						, {
							ttl: 5000,
							disableCountDown: true
						});
				} else if (changedRequestsCount < requestsCount) {
					$growl.warning(
						"<i class='mdi  mdi-alert-octagon'></i> "
						+ changedRequestsText.replace('{0}', changedRequestsCount)
						+ " "
						+ notChangedRequestsText.replace('{0}', requestsCount - changedRequestsCount), {
							ttl: 5000,
							disableCountDown: true
						});

				} else {
					$growl.success(
						"<i class='mdi mdi-thumb-up'></i> "
						+ changedRequestsText.replace('{0}', changedRequestsCount)
						, {
							ttl: 5000,
							disableCountDown: true
						});
				}
			});			
		}

		this.notifyDenyRequestsSuccess = function (changedRequestsCount, requestsCount) {

			$q.all([$translate('RequestsHaveBeenDenied'), $translate('RequestsHaveNotBeenDenied')]).then(function (texts) {
				var changedRequestsText = texts[0];
				var notChangedRequestsText = texts[1];

				if (changedRequestsCount == 0) {
					$growl.error(
						"<i class='mdi  mdi-alert-octagon'></i> "
						+ notChangedRequestsText.replace('{0}', requestsCount - changedRequestsCount)
						, {
							ttl: 5000,
							disableCountDown: true
						});
				} else if (changedRequestsCount < requestsCount) {
					$growl.warning(
						"<i class='mdi  mdi-alert-octagon'></i> "
						+ changedRequestsText.replace('{0}', changedRequestsCount)
						+ " "
						+ notChangedRequestsText.replace('{0}', requestsCount - changedRequestsCount), {
							ttl: 5000,
							disableCountDown: true
						});

				} else {
					$growl.success(
						"<i class='mdi mdi-thumb-up'></i> "
						+ changedRequestsText.replace('{0}', changedRequestsCount)
						, {
							ttl: 5000,
							disableCountDown: true
						});
				}
			});
		}

		this.notifyCommandError = function (error) {
			$translate("CommandFailed").then(function (text) {
				$growl.error("<i class='mdi  mdi-alert-octagon'></i> "
					+ text.replace('{0}', error), {
						ttl: 5000,
						disableCountDown: true
					});
			});
		}


	}

})();