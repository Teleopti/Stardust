'use strict';

var outboundService = angular.module('outboundService', ['ngResource']);

outboundService.service('OutboundService', ['$resource', '$filter', function( $resource, $filter) {

	var self = this;
	var Campaign = $resource('../api/Outbound/Campaign/:Id', { Id: '@Id' }, {
		update: { method: 'PUT' }
	});

	var CampaignWorkingPeriod = $resource('../api/Outbound/Campaign/:CampaignId/WorkingPeriod/:WorkingPeriodId', {
		CampaignId: '@CampaignId',
		WorkingPeriodId: '@WorkingPeriodId'
	}, {
		update: { method: 'PUT' }
	});

	var expandWorkingPeriod = function(workingPeriod) {
		var assignments = angular.isDefined(workingPeriod.WorkingPeroidAssignments)? workingPeriod.WorkingPeroidAssignments : [];
		var expandedAssignments = [];
		for (var i = 0; i < 7; i++) {
			var assigned = assignments.filter(function(x) { return x.WeekDay == i; });
			expandedAssignments.push({ WeekDay: i, Checked: assigned.length > 0 });
		}
		workingPeriod.ExpandedWorkingPeriodAssignments = expandedAssignments;
	};

	self.campaigns = [];
	self.currentCampaignId = null;

	self.getCurrentCampaign = function() {
		if (self.currentCampaignId == null) return null;
		var matched = self.campaigns.filter(function (campaign) { return campaign.Id == self.currentCampaignId; });
		if (matched.length == 0) return null;
		else return matched[0];
	};

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

	self.listCampaign = function (campaignFilter, successCb, errorCb) {	
		self.campaigns = Campaign.query({}, 
			function() {
				if (angular.isDefined(successCb))
					successCb();
			},
			function(data) {
				if (angular.isDefined(errorCb))
					errorCb(data);
			});		
		return self.campaigns;
	};

	self.getCampaignById = function (Id) {
		self.currentCampaignId = Id;
		var matched = self.campaigns.filter(function(campaign) { return campaign.Id === Id; });
		if (matched.length === 0) return null;
		var campaign = matched[0];
		if (! (angular.isDefined(campaign.IsFull) && campaign.IsFull)) {
			var fetched = Campaign.get({ Id: Id }, function() {
				campaign = angular.extend(campaign, fetched, { IsFull: true });
				angular.forEach(campaign.CampaignWorkingPeriods, function (period) {
					expandWorkingPeriod(period);
				});
			});	
		}	
		return campaign;
	};

	self.updateCampaign = function(campaign, successCb, errorCb) {
		campaign.$update(
			function() {
				if (angular.isDefined(successCb))
					successCb();
			},
			function(data) {
				if (angular.isDefined(errorCb))
					errorCb(data);
			});
	};
	
	self.deleteCampaign = function (campaign, successCb, errorCb) {
		Campaign.remove({ Id: campaign.Id },
			function() {
				if (angular.isDefined(successCb))
					successCb();
			},
			function (data) {
				if (angular.isDefined(errorCb))
					errorCb(data);
			});
		self.campaigns.splice(self.campaigns.indexOf(campaign), 1);
	}

	self.addWorkingPeriod = function (campaign, workingPeriod) {
		var newWorkingPeriod = new CampaignWorkingPeriod();
		newWorkingPeriod.CampaignId = campaign.Id;
		newWorkingPeriod.StartTime = $filter('date')(workingPeriod.StartTime, 'HH:mm');
		newWorkingPeriod.EndTime = $filter('date')(workingPeriod.EndTime, 'HH:mm');
		newWorkingPeriod.WorkingPeriod = { period: {
			_minimum: $filter('date')(workingPeriod.StartTime, 'HH:mm:ss'),
			_maximum: $filter('date')(workingPeriod.EndTime, 'HH:mm:ss')				
		}};

		newWorkingPeriod.$save(function () {		
			expandWorkingPeriod(newWorkingPeriod);
			campaign.CampaignWorkingPeriods.push(newWorkingPeriod);
		});				
	};

	self.deleteWorkingPeriod = function (campaign, workingPeriod) {
		CampaignWorkingPeriod.remove({ CampaignId: campaign.Id, WorkingPeriodId: workingPeriod.Id });
		campaign.CampaignWorkingPeriods.splice(campaign.CampaignWorkingPeriods.indexOf(workingPeriod), 1);
	}

	self.addWorkingPeriodAssignment = function (campaign, workingPeriod, weekDay) {
		CampaignWorkingPeriod.update({
			CampaignId: campaign.Id,
			WeekDay: weekDay.WeekDay,
			CampaignWorkingPeriods: [workingPeriod.Id]
		});
	};

	self.deleteWorkingPeriodAssignment = function(campaign, workingPeriod, weekDay) {
		CampaignWorkingPeriod.update({
			CampaignId: campaign.Id,
			WeekDay: weekDay.WeekDay,
			CampaignWorkingPeriods: []
		});		
	};
}]);
