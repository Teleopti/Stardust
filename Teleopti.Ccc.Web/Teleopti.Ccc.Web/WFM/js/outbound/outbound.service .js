﻿'use strict';

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

	self.updateCampaign = function (campaign) {
		campaign.$update();
	};
	
	self.deleteCampaign = function (campaign) {	
		Campaign.remove({ Id: campaign.Id });
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

		console.log(newWorkingPeriod);
		newWorkingPeriod.$save(function () {		
			expandWorkingPeriod(newWorkingPeriod);
			campaign.CampaignWorkingPeriods.push(newWorkingPeriod);
		});				
	};

	self.deleteWorkingPeriod = function (campaign, workingPeriod) {
		console.log({ CampaignId: campaign.Id, WorkingPeriodId: workingPeriod.Id });
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
