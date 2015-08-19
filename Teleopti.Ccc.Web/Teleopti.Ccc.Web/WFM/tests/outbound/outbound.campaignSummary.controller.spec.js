'use strict';
describe('OutboundSummaryCtrl', function () {
	var $q,
		$rootScope,
		$controller,
		$timeout,
		outboundService,
		outboundChartService,
		stateService
	;

	beforeEach(function () {
		module('wfm');

		outboundService = new fakeOutboundService();

		outboundChartService = new fakeOutboundChartService();

		module(function ($provide) {
			$provide.service('outboundService', function () {
				return outboundService;
			});

			$provide.service('outboundChartService', function () {
				return outboundChartService;
			});

			$provide.service('$state', function () {
				return stateService;
			});
		});
	});

	beforeEach(inject(function (_$httpBackend_, _$q_, _$rootScope_, _$controller_, _$timeout_) {

		$q = _$q_;
		$rootScope = _$rootScope_;
		var $httpBackend = _$httpBackend_;
		$controller = _$controller_;
		$timeout = _$timeout_;
		$httpBackend.expectGET("../api/Global/Language?lang=en").respond(200, 'mock');
		$httpBackend.expectGET("../api/Global/User/CurrentUser").respond(200, 'mock');

	}));

	it('isLoadFinished is false', function () {
		var test = setUpTarget();
		expect(test.scope.isLoadFinished).not.toBeTruthy(); 

	});


	it('should tell the campaign whether it is overstaffing', function () {
		var test = setUpTarget();
		var warnings = [{ TypeOfRule: 'OutboundOverstaffRule' }];
		expect(test.scope.isOverStaffing(warnings)==true);
		var warnings = [{ TypeOfRule: 'OutboundUnderSLARule' }];
		expect(test.scope.isOverStaffing(warnings)==false);

	});

	it('should hide warning sign, when the campaign is at phase done', function () {
		var test = setUpTarget();
		expect(test.scope.hideWhenDone(8)==true);
		expect(test.scope.hideWhenDone(1)==false);
	});	

	it('should switch to different campaign chart', function () {
		var test = setUpTarget();
		var campaign = {
			viewScheduleDiffToggle: false
		};
		test.scope.toggleChartDisplay(campaign);
		expect(campaign.viewScheduleDiffToggle == true);
		expect(campaign.chart != null);
	});

	it('add manual plan should work', function() {
		var test = setUpTarget();
		var campaign = {
			Id: 1,
			selectedDates: [new Date('2015-07-19'), new Date('2015-07-20')],
			manualPlan: {}
		};

		outboundService.prepareCampaignSummary({
			Id: 1,
			Status: 1
		});

		outboundChartService.setCampaignVisualization(1, {
			Dates: [new Date('2015-07-19'), new Date('2015-07-20')],
			Plans: [3, 3],
			ManualPlan: [true, false]
		});
		
		test.scope.addManualPlan(campaign);
		setTimeout(function () {			
			expect(campaign.graphData).toBeDefined();
			expect(campaign.Status).toEqual(1);			
		}, 100);
	});

	it('remove manual plan should work', function() {
		var test = setUpTarget(); 
		var campaign = {
			Id: 1,
			selectedDates: [new Date('2015-07-19'), new Date('2015-07-20')],
			manualPlan: {}
		};

		outboundService.prepareCampaignSummary({
			Id: 1,
			Status: 1
		});

		outboundChartService.setCampaignVisualization(1, {
			Dates: [new Date('2015-07-19'), new Date('2015-07-20')],
			Plans: [3, 3],
			ManualPlan: [true, false]
		});


		test.scope.removeManualPlan(campaign);
		setTimeout(function() {
			expect(campaign.graphData).toBeDefined();
			expect(campaign.Status).toEqual(1);		
		}, 100);

	});

	it('clear manual plan should work', function() {
		var test = setUpTarget();
		var campaign = {
			selectedDates: [new Date('2015-07-19'), new Date('2015-07-20')],
			manualPlanInput : 3
		}

		test.scope.clearManualPlan(campaign);
		expect(campaign.selectedDates.length).toEqual(0);
		expect(campaign.manualPlanInput).toBeNull();
	});
	
	function setUpTarget() {
		var scope = $rootScope.$new();
		var target = $controller('OutboundSummaryCtrl', {
			$scope: scope,
			$state: stateService,
			outboundService: outboundService,
			outboundChartService: outboundChartService

		});
		return { target: target, scope: scope };
	}



	function fakeOutboundService() {

		var campaigns = [];
		var campaignSummaries = [];

		this.getCampaignStatistics=function (){}

		this.getCampaign = function (campaignId, successCb, errorCb) {
		};

		this.addCampaign = function (campaign, successCb, errorCb) {
			campaigns.push(campaign);
			successCb(campaign);
		};

		this.calculateCampaignPersonHour = function (campaign) {
			return campaign.calculatedPersonHour;
		}

		this.load = function () { };

		this.prepareCampaignSummary = function(summary) {
			campaignSummaries.push(summary);
		};

		this.getCampaignSummary = function(id, cb) {
			var ss = campaignSummaries.filter(function(s) {
				return s.Id == id;
			});
			if (ss.length > 0) cb(ss[0]);
		}		
	}

	function fakeOutboundChartService() {

		var campaignViss = {};
	
		this.setCampaignVisualization = function(id, vis) {
			campaignViss[id] = vis;
		}

		this.getCampaignVisualization = function(d, success) {
			 success();
		};

		this.makeGraph = function () { return { graph: 'c3' } };

		this.updateManualPlan = function (campaign, cb) {			
			if (angular.isDefined(campaignViss[campaign.CampaignId])) {
				cb(campaignViss[campaign.CampaignId], campaignViss[campaign.CampaignId].ManualPlan);							
			}			
		};

		this.removeManualPlan = function (campaign, cb) {
			if (angular.isDefined(campaignViss[campaign.CampaignId])) {
				cb(campaignViss[campaign.CampaignId], campaignViss[campaign.CampaignId].ManualPlan);
			}
		};
	}

});





