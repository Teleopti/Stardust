﻿'use strict';

describe('OutboundCreateCtrl', function() {
	var $q,
		$rootScope,
		$controller,
		outboundService,
		stateService,
		outboundNotificationService
	;

	beforeEach(function () {		
		module('wfm');

		outboundService = new fakeOutboundService();
		stateService = new fakeStateService();

		module(function($provide) {
			$provide.service('outboundService33699', function () {
				return outboundService;
			});
			
			$provide.service('$state', function () {
				return stateService;
			});			
		});	
	});

	beforeEach(inject(function (_$httpBackend_, _$q_, _$rootScope_, _$controller_, _outboundNotificationService_) {
		
		$q = _$q_;
		$rootScope = _$rootScope_;
		var $httpBackend = _$httpBackend_;
		$controller = _$controller_;
		outboundNotificationService = _outboundNotificationService_;
		$httpBackend.expectGET("../api/Global/Language?lang=en").respond(200, 'mock');
		$httpBackend.expectGET("../api/Global/User/CurrentUser").respond(200, 'mock');

	}));


	it('Name should be empty to start with', function() {	
		var test = setUpTarget();
		expect(test.scope.newCampaign.Name).not.toBeDefined();

	});

	it('Working hours should be empty to start with', function() {
		var test = setUpTarget();
		expect(test.scope.newCampaign.WorkingHours).toBeDefined();
		expect(test.scope.newCampaign.WorkingHours.length).toEqual(0);
	});

	it('Activity should be empty to start with', function() {
		var test = setUpTarget();
		expect(test.scope.newCampaign.Activity).toBeDefined();
		expect(test.scope.newCampaign.Activity.Id).not.toBeDefined();
	});

	it('Start Day and End Day should be set to start with', function() {
		var test = setUpTarget();
		expect(test.scope.newCampaign.StartDate).toBeDefined();
		expect(test.scope.newCampaign.EndDate).toBeDefined();
	});

	it('Default to invalid', function() {
		var test = setUpTarget();
		expect(test.scope.isInputValid()).not.toBeTruthy();
	});

	it('Should watch for campaign person hour', function() {
		var test = setUpTarget();

		test.scope.campaignWorkloadForm = { $valid: true };
		test.scope.newCampaign.calculatedPersonHour = 1;

		test.scope.$digest();
		expect(test.scope.estimatedWorkload).toEqual(1);
	});

	it('Valid campaign should be valid', function() {

		var test = setUpTarget();

		test.scope.campaignWorkloadForm = { $valid: true };
		test.scope.campaignGeneralForm = { $valid: true };

		completeValidCampaign(test.scope.newCampaign);
		expect(test.scope.isInputValid()).toBeTruthy();
	});

	it('Should not sumbit invalid campaign', function() {
		var test = setUpTarget();
		test.scope.addCampaign();
		var campaigns;
		outboundService.listCampaign(null, function (data) { campaigns = data; });
		expect(campaigns.length).toEqual(0);
	});

	it('Should submit valid campaign', function() {
		var test = setUpTarget();
		test.scope.campaignWorkloadForm = { $valid: true, $setPristine: function() {} };
		test.scope.campaignGeneralForm = { $valid: true, $setPristine: function () {} };
		completeValidCampaign(test.scope.newCampaign);
		test.scope.addCampaign();
		var campaigns;
		outboundService.listCampaign(null, function(data) { campaigns = data; });
		expect(campaigns.length).toEqual(1);
	});

	it('Should rest after valid submission', function() {
		var test = setUpTarget();
		test.scope.campaignWorkloadForm = { $valid: true, $setPristine: function () { } };
		test.scope.campaignGeneralForm = { $valid: true, $setPristine: function () { } };
		completeValidCampaign(test.scope.newCampaign);
		test.scope.addCampaign();
		expect(test.scope.newCampaign.WorkingHours.length).toEqual(0);
	});

	it('Invalid date range should fail', function() {

		var test = setUpTarget();
		test.scope.campaignWorkloadForm = { $valid: true, $setPristine: function() {} };
		test.scope.campaignGeneralForm = { $valid: true, $setPristine: function() {} };
		completeValidCampaign(test.scope.newCampaign);
		test.scope.newCampaign.StartDate.Date = new Date();
		test.scope.newCampaign.EndDate.Date = (new Date()).setDate((new Date()).getDate() - 1);
		expect(test.scope.isInputValid()).not.toBeTruthy();
	});

	it('At least one working hour has been selected for any weekday for the campaign to be valid', function() {
		var test = setUpTarget();
		test.scope.campaignWorkloadForm = { $valid: true, $setPristine: function () { } };
		test.scope.campaignGeneralForm = { $valid: true, $setPristine: function () { } };
		completeValidCampaign(test.scope.newCampaign);

		angular.forEach(test.scope.newCampaign.WorkingHours[0].WeekDaySelections, function(e) {
			e.Checked = false;
		});
		expect(test.scope.isInputValid()).not.toBeTruthy();
	});

	it('Should move to campaign edit upon successful submission', function() {
		var test = setUpTarget();
		test.scope.campaignWorkloadForm = { $valid: true, $setPristine: function () { } };
		test.scope.campaignGeneralForm = { $valid: true, $setPristine: function () { } };
		completeValidCampaign(test.scope.newCampaign);
		test.scope.addCampaign();
		expect(stateService.getWhere()).toEqual('outbound.edit');
	});


	function setUpTarget() {
		var scope = $rootScope.$new();
		var target = $controller('OutboundCreateCtrl', {
			$scope: scope,
			$state: stateService,
			outboundService33699: outboundService,
			outboundNotificationService: outboundNotificationService
		});
		return { target: target, scope: scope };
	}


	function fakeStateService() {

		var where;

		this.reset = function () { where = null; };
		this.getWhere = function () { return where; };

		this.go = function (gowhere) {
			where = gowhere;
		}
	}


	function fakeOutboundService() {

		var campaigns = [];

		this.listCampaign = function (filter, successCb, errorCb) {
			successCb(campaigns);
		};

		this.getCampaign = function (campaignId, successCb, errorCb) {
		};

		this.addCampaign = function (campaign, successCb, errorCb) {
			campaigns.push(campaign);
			successCb(campaign);
		};

		this.calculateCampaignPersonHour = function (campaign) {
			return campaign.calculatedPersonHour;
		}
	}


	function completeValidCampaign(campaign) {
		var weekDaySelections = [];
		for (var i = 0; i < 7; i++) {
			weekDaySelections.push({
				WeekDay: i,
				Checked: true
			});
		}

		var workingHour = {
			StartTime: new Date().setHours(12, 0, 0, 0),
			EndTime: new Date().setHours(13, 0, 0, 0),
			WeekDaySelections: weekDaySelections
		}
		campaign.WorkingHours = [workingHour];
	}
});





