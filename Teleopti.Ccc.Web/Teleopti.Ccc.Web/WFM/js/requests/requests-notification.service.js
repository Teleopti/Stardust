(function() {
	'use strict';

	angular.module('wfm.requests')
		.service('requestsNotificationService', ['growl', '$translate', requestsNotificationService]);

	function requestsNotificationService($growl, $translate) {
		
		this.notifyApproveRequestsSuccess = function (changedRequestsCount) {
			$translate('RequestsHaveBeenApproved').then(function (text) {				
				$growl.success("<i class='mdi mdi-thumb-up'></i> "
					+ text.replace('{0}', changedRequestsCount), {
						ttl: 5000,
						disableCountDown: true
					});
			});
		}

		this.notifyDenyRequestsSuccess = function(changedRequestsCount) {
			$translate('RequestsHaveBeenDenied').then(function (text) {
				$growl.success("<i class='mdi mdi-thumb-up'></i> "
					+ text.replace('{0}', changedRequestsCount), {
						ttl: 5000,
						disableCountDown: true
					});
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