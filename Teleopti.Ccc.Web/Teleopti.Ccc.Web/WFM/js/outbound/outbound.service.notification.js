(function() {
    'use strict';

    angular.module('outboundServiceModule')
        .service('outboundNotificationService', ['growl', '$translate', outboundNotificationService]);

    function outboundNotificationService($growl, $translate) {

        this.notifySuccess = notifySuccess;
        this.notifyFailure = notifyFailure;

        this.notifyCampaignCreationSuccess = function (campaign) {
            notifySuccess('CampaignCreated', [
		        '<strong>' + campaign.Name + '</strong>'
            ]);
        }

        this.notifyCampaignUpdateSuccess = function (campaign) {
            notifySuccess('CampaignUpdated', [
               '<strong>' + campaign.Name + '</strong>'
            ]);
        }

        this.notifyCampaignCreationFailure = function (error) {
            notifyFailure('CampaignFailedCreation', [
		        (error && error.Message ? error.Message : error)
            ]);
        }

        this.notifyCampaignRemoveFailure = function (error) {
        	notifyFailure('CampaignFailedRemove', [
			  (error && error.Message ? error.Message : error)
        	]);
        }

        this.notifyCampaignUpdateFailure = function (error) {
            notifyFailure('CampaignFailedUpdate', [
                (error && error.Message ? error.Message : error)
            ]);
        }

        this.notifyCampaignLoadingFailure = function (error) {
            notifyFailure('CampaignFailedLoading', [
		        (error && error.Message ? error.Message : error)
            ]);
        }

        function notifySuccess(message, params) {
            $translate(message).then(function (text) {
                $growl.success("<i class='mdi mdi-thumb-up'></i> "
		            + text.replace('{0}', params[0]), {
		                ttl: 5000,
		                disableCountDown: true
		            });
            });
        }

        function notifyFailure(message, params) {
            $translate(message).then(function (text) {
                $growl.error("<i class='mdi  mdi-alert-octagon'></i> "
                    + text.replace('{0}', params[0]), {
                        ttl: 5000,
                        disableCountDown: true
                    });
            });
        }

    }



})();