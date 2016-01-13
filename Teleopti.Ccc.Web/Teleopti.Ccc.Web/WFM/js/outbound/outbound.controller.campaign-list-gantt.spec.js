'use strict';
describe('OutboundSummaryCtrl', function() {
	var $q,
		$rootScope,
		$controller,
		$timeout,
		outboundService,
		outboundChartService,
		stateService,
		toggleSvc,
		miscService;

	beforeEach(function() {
		module('wfm.outbound');

		outboundService = new fakeOutboundService();

		outboundChartService = new fakeOutboundChartService();

		toggleSvc = {
			Wfm_Outbound_Campaign_GanttChart_Navigation_34924: true,
			togglesLoaded: {
				then: function (cb) { cb(); }
			}
		}

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

			$provide.service('Toggle', function () {				
				return toggleSvc;
			});
		});
	});

	beforeEach(inject(function(_$q_, _$rootScope_, _$controller_, _$timeout_) {
		$q = _$q_;
		$rootScope = _$rootScope_;	
		$controller = _$controller_;
		$timeout = _$timeout_;	
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
			{ CampaignSummary: { Id: 1, StartDate: { Date: '2015-09-23' }, EndDate: { Date: '2015-09-24' } }, IsScheduled: true, WarningInfo: [] },
			{ CampaignSummary: { Id: 2, StartDate: { Date: '2015-09-23' }, EndDate: { Date: '2015-09-24' } }, IsScheduled: false, WarningInfo: [] }
		];
		test.target.init();
		test.target.updateAllCampaignGanttDisplay(toBeUpdated);

		expect(test.scope.ganttData[0].tasks[0].color).toEqual('#C2E085');
		expect(test.scope.ganttData[1].tasks[0].color).toEqual('#ffc36b');
	});

	it('should extend the row when click and collapse when click again', function () {
		var test = setUpTarget();
		var campaign = { Id: 1, id: 1, StartDate: { Date: '2015-09-23' }, EndDate: { Date: '2015-09-24' }  };
		outboundService.setGanttVisualization(campaign);
		outboundService.setCampaignStatus({ CampaignSummary: { Id: 1 }, WarningInfo: [] });
		

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
		var campaign = { Id: 1, id:1, StartDate: { Date: '2015-09-23' }, EndDate: { Date: '2015-09-24' } };
		var campaignVisualization = {
			Dates: [new Date('2015-09-23'), new Date('2015-09-24')],
			Plans: [3, 3],
			ManualPlan: [true, false]
		};
		outboundService.setGanttVisualization(campaign);
		outboundService.setCampaignStatus({ CampaignSummary: { Id: 1 }, WarningInfo: [] });
		outboundChartService.setCampaignVisualization(1, campaignVisualization);
		outboundService.setCampaignsStatus({ CampaignSummary: { Id: 1 } });

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
			Toggle: toggleSvc
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

		var visualizationPeriod = {
			PeriodStart: new Date(),
			PeriodEnd: new Date()
		}

		this.checkPermission = function() {
			return {
				then: function (cb) {
					//todo: need prepare init !
					// cb();
				}
			}
		};

		this.loadCampaignSchedules=function(period, cb) {
			if (cb) cb();
		}

		this.setGanttPeriod=function(period) {
			visualizationPeriod = period;
		}

		this.getGanttPeriod=function() {
			return visualizationPeriod;
		}

		this.setGanttVisualization = function (ganttV) {
			ganttVisualization.push(ganttV);
		}

		this.getCampaigns = function (ganttPeriod, cb) {
			cb(ganttVisualization);
		}

		this.setThreshold=function(threshold) {
			thresholdObj = threshold;
		}

		this.getThreshold = function(cb) {
			cb(thresholdObj);
		};

		this.setCampaignStatus=function(campaignD) {
			campaignDetail = campaignD;
		}

		this.getCampaignStatus=function(id, cb) {
			cb(campaignDetail);
		}

		this.setCampaignsStatus = function (listCampaigns) {
			listCampaign.push(listCampaigns);
		}

		this.updateCampaignsStatus=function(cb) {
			cb(listCampaign);
		}
		

		this.loadWithinPeriod = function() {}

		this.getDefaultPeriod = function() {
			return ['', ''];
		};

		this.setPhaseStatistics = function(d) {
			PhaseStatistics = d;
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
});