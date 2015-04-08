'use strict';

var outboundService = angular.module('outboundService', ['ngResource']);

outboundService.factory('OutboundService', ['$rootScope', '$resource', function($rootScope, $resource) {

	var Campaign = $resource('../api/Outbound/Campaign', {},
	{
		get: { method: 'GET', params: {}, isArray: true }
		
	});

	var outboundService = {};

	outboundService.addCampaign = function (campaign) {

		var newCampaign = new Campaign(campaign);
		newCampaign.$save();

		$rootScope.$broadcast("outbound.campaigns.updated");	
		return newCampaign;
	};

	outboundService.listCampaign = function (campaignFilter) {
		return Campaign.get(campaignFilter);
	};

	outboundService.getCampaignById = function(id) {
		
	};

	outboundService.updateCampaign = function (campaign) {
		
	};

	outboundService.deleteCampaign = function (campaign, idx) {

	};

	return outboundService;
}]);