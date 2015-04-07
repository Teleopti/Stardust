'use strict';

var outboundService = angular.module('outboundService', ['ngResource']);

outboundService.factory('OutboundService', ['$rootScope', '$resource', function ($rootScope, $resource) {

	var Campaign = $resource('../api/Outbound/Campaign');

	var outboundService = {};

	outboundService.addCampaign = function (campaign) {

		var newCampaign = new Campaign(campaign);
		newCampaign.$save();

		$rootScope.$broadcast("outbound.campaigns.updated");	
		return newCampaign;
	};

	outboundService.listCampaign = function (campaignFilter) {
		console.log(Campaign.query());
		return Campaign.query(campaignFilter);
	};

	outboundService.getCampaignById = function(id) {
		
	};

	outboundService.updateCampaign = function (campaign) {
		
	};

	outboundService.deleteCampaign = function (campaign, idx) {

	};

	return outboundService;
}]);