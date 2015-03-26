'use strict';

var outboundService =  angular.module('outboundService', []);

outboundService.factory('OutboundService', ['$rootScope', function($rootScope) {

	var outboundService = {};

	var campaigns = [
			{ id: 1, name: "March Sales", period: { startDate: "2015-03-01", endDate: "2015-05-01" } },
			{ id: 2, name: "Apirl Sales", period: { startDate: "2015-04-01", endDate: "2015-05-01" } },
			{ id: 3, name: "Chocalate Sales", period: { startDate: "2015-01-01", endDate: "2015-01-07" } }
	];

	var maxId = 3;

	var getNextId = function () {
		maxId += 1;
		return maxId - 1;
	};

	outboundService.addCampaign = function (campaign) {
		var newCampaign = angular.copy(campaign);
		newCampaign.id = getNextId();
		campaigns.unshift(newCampaign);
		$rootScope.$broadcast("outbound.campaigns.updated");
	};

	outboundService.deleteCampaign = function(campaign, idx) {
		if (campaigns[idx].id == campaign.id) {		
			campaigns.splice(idx, 1);		
		}
		$rootScope.$broadcast("outbound.campaigns.updated");
	};

	outboundService.listCampaign = function( campaignFilter) {
		return angular.copy(campaigns);
	};

	outboundService.getCampaignById = function(id) {
		for (var i = 0; i < campaigns.length; i ++) {
			if (campaigns[i].id == id) return angular.copy(campaigns[i]);
		}
	};

	outboundService.updateCampaign = function (campaign) {
		var idx = -1 ;
		for (var i = 0; i < campaigns.length; i++) {
			if (campaigns[i].id == campaign.id) {
				idx = i;
				break;
			}
		}

		if (idx >= 0) {
			campaigns.splice(idx, 1, angular.copy(campaign));
		}

		$rootScope.$broadcast("outbound.campaigns.updated");
	};

	return outboundService;
}]);