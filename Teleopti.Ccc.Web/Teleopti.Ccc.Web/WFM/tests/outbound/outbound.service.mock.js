'use strict';

var outboundService =  angular.module('outboundServiceMock', []);

outboundService.factory('outboundServiceMock', function () {

	var outboundService = {};

	var campaigns = [
			{ id: 1, name: "March Sales", period: { startDate: "2015-03-01", endDate: "2015-05-01" } },
			{
				id: 2, name: "Apirl Sales",
				period: { startDate: "2015-04-01", endDate: "2015-05-01" },
				params: {
					callList: 500,
					sla: "50%",
					target: 250,
					cr: "30%",
					rpcr: "20%",
					cAHT: "30 seconds",
					rpcAHT: "120 seconds",
					unproductive: "20 seconds"
				}
			},
			{ id: 3, name: "Chocalate Sales", period: { startDate: "2015-01-01", endDate: "2015-01-07" } },
			{ id: 4, name: "Oct Debt Collection", period: { startDate: "2015-01-01", endDate: "2015-01-07" } },
			{ id: 5, name: "Nov Debt Collection", period: { startDate: "2015-01-01", endDate: "2015-01-07" } },
			{ id: 6, name: "Fruit Sales", period: { startDate: "2015-01-01", endDate: "2015-01-07" } },
			{ id: 7, name: "Game Sales", period: { startDate: "2015-01-01", endDate: "2015-01-07" } }
	];

	var maxId = 7;

	var getNextId = function () {
		maxId += 1;
		return maxId;
	};

	outboundService.addCampaign = function (campaign, successCb, errorCb) {
		var newCampaign = angular.copy(campaign);
		newCampaign.id = getNextId();
		campaigns.unshift(newCampaign);			
		return newCampaign;
	};

	outboundService.deleteCampaign = function(campaign, idx) {
		if (campaigns[idx].id == campaign.id) {
			campaigns.splice(idx, 1);		
			return true;
		} else {
			return false;
		}		
	};

	outboundService.listCampaign = function( campaignFilter) {
		return campaigns;
	};

	outboundService.getCampaignById = function(id) {
		for (var i = 0; i < campaigns.length; i ++) {
			if (campaigns[i].id == id) return campaigns[i];
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
			campaigns.splice(idx, 1, campaign);
		}		
	};

	return outboundService;
});