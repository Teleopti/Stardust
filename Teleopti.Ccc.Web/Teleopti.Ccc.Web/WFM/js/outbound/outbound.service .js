'use strict';

var outboundService = angular.module('outboundService', ['ngResource']);

outboundService.service('OutboundService', ['$resource', function( $resource) {

	var self = this;
	var Campaign = $resource('../api/Outbound/Campaign/:Id', { Id: '@Id' }, {
		update: { method: 'PUT' }
	});

	var CampaignWorkingPeriodAssignment = $resource('../api/Outbound/Capmaign/:Id/WorkingPeriod/:WorkingPeriodId/Assignment/:AssignmentId', {
		Id: '@CampaignId',
		WorkingPeriodId: '@WorkingPeriodId',
		AssignmentId: '@AssignmentId'
	});


	var expandWorkingPeriod = function (workingPeriod) {
		var assignments = workingPeriod.WorkingPeroidAssignments;
		if (!angular.isDefined(workingPeriod.ExpandedWorkingPeriodAssignments) || workingPeriod.ExpandedWorkingPeriodAssignments.length != 7) {
			var expandedAssignments = [];
			for (var i = 0; i < 7; i++) {
				var assigned = assignments.filter(function(x) { return x.WeekDay == i; });
				if (assigned.length == 0) {
					expandedAssignments.push({ WeekDay: i, Checked: false });
				} else {
					expandedAssignments.push(angular.extend(assigned[0], { Checked: true }));
				}
			}
			workingPeriod.ExpandedWorkingPeriodAssignments = expandedAssignments;
		} else {
			angular.forEach(workingPeriod.ExpandedWorkingPeriodAssignments, function(expandedAssignment) {
				var assigned = assignments.filter(function (x) { return x.WeekDay == expandedAssignment.WeekDay; });
				if (assigned.length == 0) {
					expandedAssignment.Checked = false;
					delete expandedAssignment.Id;
				} else {
					expandedAssignment.Checked = true;
					expandedAssignment.Id = assigned[0].Id;
				}
			});
		}		
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

	self.addWorkingPeriodAssignment = function (campaign, workingPeriod, weekDay) {
		var assignment = new CampaignWorkingPeriodAssignment(angular.extend(weekDay, {
			CampaignId: campaign.Id,
			WorkingPeriodId: workingPeriod.Id
		}));
		assignment.$save(function() {
			weekDay.Id = assignment.Id;
			weekDay.Checked = true;
		});		
	};

	self.deleteWorkingPeriodAssignment = function(campaign, workingPeriod, weekDay) {

		CampaignWorkingPeriodAssignment.delete({
			CampaignId: campaign.Id,
			WorkingPeriodId: workingPeriod.Id,
			AssignmentId: weekDay.Id
		}, function() {
			weekDay.Checked = false;
			delete weekDay.Id;
		});

	};
}]);
