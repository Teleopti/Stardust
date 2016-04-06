(function() {
	'use strict';

	angular.module('outboundServiceModule')
		.service('outboundNotificationService', ['NoticeService', '$translate', outboundNotificationService]);

	function outboundNotificationService(NoticeService, $translate) {

		this.notifySuccess = notifySuccess;
		this.notifyFailure = notifyFailure;
		this.notifyWarning = notifyWarning;

		this.notifyCampaignCreationSuccess = function(campaign) {
			notifySuccess('CampaignCreated', [
				'<strong>' + (campaign.CampaignSummary?campaign.CampaignSummary.Name:campaign.Name) + '</strong>'
			]);
		}

		this.notifyCampaignUpdateSuccess = function (campaign) {
			notifySuccess('CampaignUpdated', [
				'<strong>' + (campaign.CampaignSummary?campaign.CampaignSummary.Name:campaign.Name) + '</strong>'
			]);
		}

		this.notifyCampaignCreationFailure = function(error) {
			notifyFailure('CampaignFailedCreation', [
				(error && error.Message ? error.Message : error)
			]);
		}

		this.notifyCampaignRemoveFailure = function(error) {
			notifyFailure('CampaignFailedRemove', [
				(error && error.Message ? error.Message : error)
			]);
		}

		this.notifyCampaignUpdateFailure = function(error) {
			notifyFailure('CampaignFailedUpdate', [
				(error && error.Message ? error.Message : error)
			]);
		}

		this.notifyCampaignLoadingFailure = function(error) {
			notifyFailure('CampaignFailedLoading', [
				(error && error.Message ? error.Message : error)
			]);
		}

		function notifySuccess(message, params) {
			$translate(message).then(function(text) {
				NoticeService.success(text.replace('{0}', params[0]), 5000, false);
			});
		}

		function notifyFailure(message, params) {
			$translate(message).then(function(text) {
				NoticeService.error(text.replace('{0}', params[0]), 5000, false);
			});
		}

		function notifyWarning(message, params) {
			$translate(message).then(function(text) {
				NoticeService.warning(text.replace('{0}', params[0]), 5000, false);
			});
		}
	}
})();
