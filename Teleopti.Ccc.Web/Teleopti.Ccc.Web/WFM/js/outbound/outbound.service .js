'use strict';

var outboundService = angular.module('outboundService', ['ngResource']);

outboundService.factory('OutboundService', ['$resource', function( $resource) {

	var Campaign = $resource('../api/Outbound/Campaign/:Id', { Id: '@Id'}, {
		update: { method: 'PUT' }
	});

	var outboundService = { campaigns: []};

	outboundService.addCampaign = function (campaign, successCb, errorCb) {
		var self = this;
		var newCampaign = new Campaign(campaign);

		newCampaign.$save( 
			function() {
				if (angular.isDefined(successCb))
					successCb(newCampaign);				
			}, 
			function(data) {
				if (angular.isDefined(errorCb))
					errorCb(data);
			});

		self.campaigns.push(newCampaign);
		return newCampaign;
	};

	outboundService.listCampaign = function (campaignFilter) {							
		self.campaigns = Campaign.query();			
		return self.campaigns;
	};

	outboundService.getCampaignById = function( Id) {
		var self = this;
		var matched = self.campaigns.filter(function(campaign) { return campaign.Id === Id });
		if (matched.length === 0) {
			var fetched = Campaign.get({ Id: Id });
			self.campaigns.push(fetched);
			return fetched;
		} else {
			return matched[0];
		}
	};

	outboundService.updateCampaign = function (campaign) {
		campaign.$update();
	};

	outboundService.deleteCampaign = function (campaign, idx) {

	};

	return outboundService;
}]);