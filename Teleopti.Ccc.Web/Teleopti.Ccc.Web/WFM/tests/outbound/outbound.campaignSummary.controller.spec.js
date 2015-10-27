'use strict';
xdescribe('OutboundSummaryCtrl', function () {
	var $q,
		$rootScope,
		$controller,
		$timeout,
		outboundService,
		outboundChartService,
		stateService,
		outboundToggles
	;

	var mockToggleService = {
		isFeatureEnabled: {
			query: function () {
				var queryDeferred = $q.defer();
				queryDeferred.resolve({
					IsEnabled: false
				});
				return { $promise: queryDeferred.promise };
			}
		},
	}

	beforeEach(function () {
		module('wfm');

		outboundService = new fakeOutboundService();

		outboundChartService = new fakeOutboundChartService();

		outboundToggles = new fakeOutboundToggles();

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

			$provide.service('OutboundToggles', function () {
				return outboundToggles;
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
		var warnings = [{ TypeOfRule: 'CampaignOverstaff' }];
		expect(test.scope.isOverStaffing(warnings)==true);
		var warnings = [{ TypeOfRule: 'CampaignUnderSLA' }];
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
		test.scope.$apply();
		var campaign = {
			Id: 1,
			selectedDates: [new Date('2015-07-19'), new Date('2015-07-20')],
			selectedDatesClosed: [],
			manualPlan: {}
		};

		outboundService.prepareCampaignSummary({
			Id: 1,
			Status: 1
		});

		outboundService.setPhaseStatistics({PlannedWarning: 2});

		outboundChartService.setCampaignVisualization(1, {
			Dates: [new Date('2015-07-19'), new Date('2015-07-20')],
			Plans: [3, 3],
			ManualPlan: [true, false]
		});

		test.scope.addManualPlan(campaign);

		expect(campaign.graphData).toBeDefined();
		expect(campaign.Status).toEqual(1);
		expect(test.scope.phaseStatistics.PlannedWarning).toEqual(2);
	});

	it('add backlog should work', function () {
		var test = setUpTarget();
		test.scope.$apply();
		var campaign = {
			Id: 1,
			selectedDates: [new Date('2015-07-19'), new Date('2015-07-20')],
			backlog: {}
		};

		outboundService.prepareCampaignSummary({
			Id: 1,
			Status: 1
		});

		outboundService.setPhaseStatistics({ PlannedWarning: 2 });

		outboundChartService.setCampaignVisualization(1, {
			Dates: [new Date('2015-07-19'), new Date('2015-07-20')],
			Plans: [3, 3],
			isManualBacklog: [true, false]
		});

		test.scope.addBacklog(campaign);

		expect(campaign.graphData).toBeDefined();
		expect(campaign.Status).toEqual(1);
		expect(test.scope.phaseStatistics.PlannedWarning).toEqual(2);
	});

	it('remove manual plan should work', function() {
		var test = setUpTarget();
		test.scope.$apply();
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

		outboundService.setPhaseStatistics({ PlannedWarning: 2 });

		test.scope.removeManualPlan(campaign);
		
		expect(campaign.graphData).toBeDefined();
		expect(campaign.Status).toEqual(1);
		expect(test.scope.phaseStatistics.PlannedWarning).toEqual(2);
	});
	
	it('remove backlog should work', function () {
		var test = setUpTarget();
		test.scope.$apply();
		var campaign = {
			Id: 1,
			selectedDates: [new Date('2015-07-19'), new Date('2015-07-20')],
			backlog: {}
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

		outboundService.setPhaseStatistics({ PlannedWarning: 2 });

		test.scope.removeBacklog(campaign);

		expect(campaign.graphData).toBeDefined();
		expect(campaign.Status).toEqual(1);
		expect(test.scope.phaseStatistics.PlannedWarning).toEqual(2);
	});
	
	function setUpTarget() {
		var scope = $rootScope.$new();
		var target = $controller('OutboundListCardsCtrl', {
			$scope: scope,
			$state: stateService,
			outboundService: outboundService,
			outboundChartService: outboundChartService,
			OutboundToggles: outboundToggles

		});
		return { target: target, scope: scope };
	}



	function fakeOutboundService() {

		var campaigns = [];
		var campaignSummaries = [];
		var PhaseStatistics;

		this.setPhaseStatistics = function (d) {
			PhaseStatistics = d;
		}

		this.getCampaignStatistics=function(d,success) {
			success(PhaseStatistics);
		}

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

		this.getCampaignVisualization = function(id, success) {
			 success(campaignViss[id]);
		};

		this.makeGraph = function () { return { graph: 'c3' } };

		this.updateBacklog = function (campaign, cb) {
			if (angular.isDefined(campaignViss[campaign.campaignId])) {
				cb(campaignViss[campaign.campaignId]);
			}
		};

		this.updateManualPlan = function (campaign, cb) {			
			if (angular.isDefined(campaignViss[campaign.campaignId])) {
				cb(campaignViss[campaign.campaignId]);							
			}			
		};

		this.removeManualPlan = function (campaign, cb) {
			if (angular.isDefined(campaignViss[campaign.campaignId])) {
				cb(campaignViss[campaign.campaignId]);
			}
		};

		this.removeActualBacklog = function (campaign, cb) {
			if (angular.isDefined(campaignViss[campaign.campaignId])) {
				cb(campaignViss[campaign.campaignId]);
			}
		};
	}

	function fakeOutboundToggles() {
		this.ready = true;
		this.isGanttEnabled = function() {
			return false;
		}
	}
});





