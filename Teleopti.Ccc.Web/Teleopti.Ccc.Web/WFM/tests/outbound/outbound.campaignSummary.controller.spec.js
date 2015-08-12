'use strict';
describe('OutboundSummaryCtrl', function () {
	var $q,
		$rootScope,
		$controller,
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

	beforeEach(inject(function (_$httpBackend_, _$q_, _$rootScope_, _$controller_) {

		$q = _$q_;
		$rootScope = _$rootScope_;
		var $httpBackend = _$httpBackend_;
		$controller = _$controller_;
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

		this.getCampaign = function (campaignId, successCb, errorCb) {
		};

		this.addCampaign = function (campaign, successCb, errorCb) {
			campaigns.push(campaign);
			successCb(campaign);
		};

		this.calculateCampaignPersonHour = function (campaign) {
			return campaign.calculatedPersonHour;
		}

		this.load = function() {};
	}

	function fakeOutboundChartService() {

		this.getCampaignVisualization = function (d,success) { success();};

		this.makeGraph = function() { return {graph:'c3'}};
	}

});





