'use strict';

var outboundService = angular.module('outboundService', ['ngResource']);

outboundService.service('OutboundService', ['$resource', function( $resource) {

	var self = this;
	var Campaign = $resource('../api/Outbound/Campaign', { }, {
		update: { method: 'PUT' }
	});

	self.campaigns  = [];

	self.addCampaign = function (campaign, successCb, errorCb) {		
		var newCampaign = new Campaign(campaign);
		newCampaign.$save( 
			function () {				
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

	self.listCampaign = function (campaignFilter) {	
		self.campaigns = Campaign.query();
		return self.campaigns;
	};

	self.getCampaignById = function( Id) {	
		var matched = self.campaigns.filter(function(campaign) { return campaign.Id === Id });
		if (matched.length === 0) {
			var fetched = Campaign.get({ Id: Id });		
			self.campaigns.push(fetched);
			return fetched;
		} else {
			return matched[0];
		}
	};

	self.updateCampaign = function (campaign) {
		campaign.$update();
	};
	
	self.deleteCampaign = function (campaign) {	
		Campaign.remove({ Id: campaign.Id });
		self.campaigns.splice(self.campaigns.indexOf(campaign), 1);
	};
}]);
