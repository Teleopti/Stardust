'use strict';
+describe('OutboundSummaryCtrl', function() {
	var $q,
		$rootScope,
		$controller,
		$timeout,
		outboundService,
		outboundChartService,
		stateService,
		outboundToggles,
		miscService;

	beforeEach(function() {
		module('wfm');

		outboundService = new fakeOutboundService();

		outboundChartService = new fakeOutboundChartService();

		outboundToggles = new fakeOutboundToggles();

		miscService = new fakeMiscService();

		module(function($provide) {
			$provide.service('miscService', function() {
				return miscService;
			});

			$provide.service('outboundService', function() {
				return outboundService;
			});

			$provide.service('outboundChartService', function() {
				return outboundChartService;
			});

			$provide.service('$state', function() {
				return stateService;
			});

			$provide.service('OutboundToggles', function() {
				return outboundToggles;
			});
		});
	});

	beforeEach(inject(function(_$httpBackend_, _$q_, _$rootScope_, _$controller_, _$timeout_) {

		$q = _$q_;
		$rootScope = _$rootScope_;
		var $httpBackend = _$httpBackend_;
		$controller = _$controller_;
		$timeout = _$timeout_;
		$httpBackend.expectGET("../api/Global/Language?lang=en").respond(200, 'mock');
		$httpBackend.expectGET("../api/Global/User/CurrentUser").respond(200, 'mock');

	}));

	it('should draw gantt chart at the beginning', function() {
		var test = setUpTarget();
		outboundService.setGanttVisualization({ Id: 1, StartDate: { Date: '2015-09-23' }, EndDate: { Date: '2015-09-24' } });

		expect(test.scope.ganttData).not.toBeDefined();
		test.target.init();
		expect(test.scope.ganttData[0].id).toEqual(1);
	});

	it('should update all campaign statistics in gantt chart', function () {
		var test = setUpTarget();
		outboundService.setGanttVisualization({ Id: 1, StartDate: { Date: '2015-09-23' }, EndDate: { Date: '2015-09-24' } });
		outboundService.setGanttVisualization({ Id: 2, StartDate: { Date: '2015-09-23' }, EndDate: { Date: '2015-09-24' } });

		expect(test.scope.ganttData).not.toBeDefined();

		var toBeUpdated = [
			{ Id: 1, StartDate: { Date: '2015-09-23' }, EndDate: { Date: '2015-09-24' }, IsScheduled: true, WarningInfo: [] },
			{ Id: 2, StartDate: { Date: '2015-09-23' }, EndDate: { Date: '2015-09-24' }, IsScheduled: false, WarningInfo: [] }
		];
		test.target.init();
		test.target.updateAllCampaignGanttDisplay(toBeUpdated);

		expect(test.scope.ganttData[0].color == '#C2E085');
		expect(test.scope.ganttData[1].color == '#66C2FF');
	});

	it('should extend the row when click and collapse when click again', function () {
		var test = setUpTarget();
		var campaign = { Id: 1, id: 1, StartDate: { Date: '2015-09-23' }, EndDate: { Date: '2015-09-24' } };
		outboundService.setGanttVisualization(campaign);
		

		expect(test.scope.ganttData).not.toBeDefined();

		test.target.init();
		expect(test.scope.ganttData.length).toEqual(1);

		test.scope.isRefreshingGantt = false;

		test.scope.campaignClicked({}, campaign);
		expect(test.scope.ganttData.length).toEqual(2);
		test.scope.campaignClicked({}, campaign);
		expect(test.scope.ganttData.length).toEqual(1);
	});

	it('should draw c3 chart when click the gantt chart', function () {
		var test = setUpTarget();
		var campaign = { Id: 1, id: 1, StartDate: { Date: '2015-09-23' }, EndDate: { Date: '2015-09-24' } };
		var campaignVisualization = {
			Dates: [new Date('2015-09-23'), new Date('2015-09-24')],
			Plans: [3, 3],
			ManualPlan: [true, false]
		};
		outboundService.setGanttVisualization(campaign);
		outboundService.setCampaignDetail({ Id: 1, WarningInfo: [] });
		outboundChartService.setCampaignVisualization(1, campaignVisualization);

		expect(test.scope.ganttData).not.toBeDefined();

		test.target.init();
		expect(test.scope.ganttData.length).toEqual(1);

		test.scope.isRefreshingGantt = false;

		test.scope.campaignClicked({}, campaign);
		expect(test.scope.ganttData[1].campaign.graphData).toBeDefined();
	});

	it('should get threshold value at the beginning', function() {
		var test = setUpTarget();
		outboundService.setThreshold({ Value:0.6, Type:2 });		
		test.target.init();
		expect(test.scope.settings.threshold).toEqual(60);
	});


	function setUpTarget() {
		var scope = $rootScope.$new();
		var target = $controller('CampaignListGanttCtrl', {
			$scope: scope,
			$state: stateService,
			outboundService: outboundService,
			outboundChartService: outboundChartService,
			OutboundToggles: outboundToggles

		});
		return { target: target, scope: scope };
	}

	function fakeMiscService() {
		this.getAllWeekendsInPeriod = function() {};
		this.getDateFromServer = function(date) {
			var dateToBefixed = new Date(date);
			return dateToBefixed;
		};
		this.sendDateToServer = function() {};
	}

	function fakeOutboundService() {

		var campaigns = [];
		var campaignSummaries = [];
		var PhaseStatistics;
		var ganttVisualization = [];
		var campaignDetail = {};
		var thresholdObj = {};

		var listCampaign = [];

		this.setThreshold=function(threshold) {
			thresholdObj = threshold;
		}

		this.getThreshold = function(cb) {
			cb(thresholdObj);
		};

		this.setCampaignDetail=function(campaignD) {
			campaignDetail = campaignD;
		}

		this.getCampaignDetail=function(id, cb) {
			cb(campaignDetail);
		}

		this.setListCampaignsWithinPeriod = function (listCampaigns) {
			listCampaign.push(listCampaigns);
		}

		this.listCampaignsWithinPeriod=function(cb) {
			cb(listCampaign);
		}

		this.setGanttVisualization = function (ganttV) {
			ganttVisualization.push(ganttV);
		}

		this.getGanttVisualization = function (ganttPeriod, cb) {
			cb(ganttVisualization);
		}

		this.loadWithinPeriod = function() {}

		this.getDefaultPeriod = function() {
			return ['', ''];
		};

		this.setPhaseStatistics = function(d) {
			PhaseStatistics = d;
		}

		this.getCampaignStatistics = function(d, success) {
			success(PhaseStatistics);
		}

		this.getCampaign = function(campaignId, successCb, errorCb) {
		};

		this.addCampaign = function(campaign, successCb, errorCb) {
			campaigns.push(campaign);
			successCb(campaign);
		};

		this.calculateCampaignPersonHour = function(campaign) {
			return campaign.calculatedPersonHour;
		}

		this.load = function() {};

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

		this.makeGraph = function() { return { graph: 'c3' } };

		this.updateBacklog = function(campaign, cb) {
			if (angular.isDefined(campaignViss[campaign.campaignId])) {
				cb(campaignViss[campaign.campaignId]);
			}
		};

		this.updateManualPlan = function(campaign, cb) {
			if (angular.isDefined(campaignViss[campaign.campaignId])) {
				cb(campaignViss[campaign.campaignId]);
			}
		};

		this.removeManualPlan = function(campaign, cb) {
			if (angular.isDefined(campaignViss[campaign.campaignId])) {
				cb(campaignViss[campaign.campaignId]);
			}
		};

		this.removeActualBacklog = function(campaign, cb) {
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