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
			togglesLoaded: {
				then: function(cb) {
					cb();
				}
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

			$provide.service('Toggle', function() {
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

		outboundService.setGanttVisualization({
			Id: 1,
			StartDate: {
				Date: '2015-09-23'
			},
			EndDate: {
				Date: '2015-09-24'
			}
		});

		expect(test.scope.ganttData).not.toBeDefined();
		test.target.init();
		expect(test.scope.ganttData[0].id).toEqual(1);
	});

	it('should update all campaign statistics in gantt chart', function() {
		var test = setUpTarget();

		outboundService.setGanttVisualization({
			Id: 1,
			StartDate: {
				Date: '2015-09-23'
			},
			EndDate: {
				Date: '2015-09-24'
			}
		});
		outboundService.setGanttVisualization({
			Id: 2,
			StartDate: {
				Date: '2015-09-23'
			},
			EndDate: {
				Date: '2015-09-24'
			}
		});

		expect(test.scope.ganttData).not.toBeDefined();

		var toBeUpdated = [
			{
				CampaignSummary: {
					Id: 1,
					StartDate: {
						Date: '2015-09-23'
					},
					EndDate: {
						Date: '2015-09-24'
					}
				},
				IsScheduled: true,
				WarningInfo: []
			},
			{
				CampaignSummary: {
					Id: 2,
					StartDate: {
						Date: '2015-09-23'
					},
					EndDate: {
						Date: '2015-09-24'
					}
				},
				IsScheduled: false,
				WarningInfo: []
			}
		];
		test.target.init();
		test.target.updateAllCampaignGanttDisplay(toBeUpdated);

		expect(test.scope.ganttData[0].tasks[0].color).toEqual('#C2E085');
		expect(test.scope.ganttData[1].tasks[0].color).toEqual('#ffc36b');
	});

	it('should extend the row when click and collapse when click again', function() {
		var test = setUpTarget();
		var campaign = {
			Id: 1,
			id: 1,
			StartDate: {
				Date: '2015-09-23'
			},
			EndDate: {
				Date: '2015-09-24'
			}
		};
		outboundService.setGanttVisualization(campaign);
		outboundService.setCampaignStatus({
			CampaignSummary: {
				Id: 1
			},
			WarningInfo: []
		});


		expect(test.scope.ganttData).not.toBeDefined();

		test.target.init();
		expect(test.scope.ganttData.length).toEqual(1);

		test.scope.isRefreshingGantt = false;

		test.scope.campaignClicked({}, campaign);
		expect(test.scope.ganttData.length).toEqual(2);
		test.scope.campaignClicked({}, campaign);
		expect(test.scope.ganttData.length).toEqual(1);
	});

	it('should draw c3 chart when click the gantt chart', function() {
		var test = setUpTarget();
		var campaign = {
			Id: 1,
			id: 1,
			StartDate: {
				Date: '2015-09-23'
			},
			EndDate: {
				Date: '2015-09-24'
			}
		};
		var campaignVisualization = {
			Dates: [new Date('2015-09-23'), new Date('2015-09-24')],
			Plans: [3, 3],
			ManualPlan: [true, false]
		};
		outboundService.setGanttVisualization(campaign);
		outboundService.setCampaignStatus({
			CampaignSummary: {
				Id: 1
			},
			WarningInfo: []
		});
		outboundChartService.setCampaignVisualization(1, campaignVisualization);
		outboundService.setCampaignsStatus({
			CampaignSummary: {
				Id: 1
			}
		});

		expect(test.scope.ganttData).not.toBeDefined();

		test.target.init();
		expect(test.scope.ganttData.length).toEqual(1);

		test.scope.isRefreshingGantt = false;

		test.scope.campaignClicked({}, campaign);
		expect(test.scope.ganttData[1].campaign.graphData).toBeDefined();
	});

	it('should get threshold value at the beginning', function() {
		var test = setUpTarget();
		outboundService.setThreshold({
			Value: 0.6,
			Type: 2
		});
		test.target.init();
		expect(test.scope.settings.threshold).toEqual(60);
	});

	it('should reset schedule data after calling getIgnoreScheduleCallback  ', function() {
		var test = setUpTarget();
		initDataForIgnore(test);
		test.scope.campaignClicked(null, {
			id: test.campaign.Id
		});
		test.campaign.selectedDates = ['2017-06-07', '2017-06-08'];

		var expectedDataRow = test.scope.ganttData[1];
		expectedDataRow.callbacks.ignoreSchedules(test.campaign.selectedDates);

		var expectedScheduleValues = [];
		test.campaign.selectedDates.forEach(function(d) {
			expectedScheduleValues.push(expectedDataRow.campaign.graphData.schedules[test.campaign.graphData.dates.indexOf(d)]);
		});

		expect(expectedScheduleValues.every(function(s) {
			return s === 0;
		})).toEqual(true);
	});

	it('should show plan data after calling getIgnoreScheduleCallback', function() {
		var test = setUpTarget();
		initDataForIgnore(test);
		test.scope.campaignClicked(null, {
			id: test.campaign.Id
		});
		test.campaign.selectedDates = ['2017-06-07', '2017-06-08'];

		var expectedDataRow = test.scope.ganttData[1];
		expectedDataRow.callbacks.ignoreSchedules(test.campaign.selectedDates);

		var expectedPlanData = [];
		test.campaign.selectedDates.forEach(function(d) {
			expectedPlanData.push(expectedDataRow.campaign.graphData.unscheduledPlans[test.campaign.graphData.dates.indexOf(d)]);
		});

		expect(expectedPlanData.every(function(s) {
			return s === 10;
		})).toEqual(true);
	});

	it('should set ignored dates after calling getIgnoreScheduleCallback', function () {
		var test = setUpTarget();
		initDataForIgnore(test);
		test.scope.campaignClicked(null, {
			id: test.campaign.Id
		});
		test.campaign.selectedDates = ['2017-06-08', '2017-06-09'];
		
		var expectedDataRow = test.scope.ganttData[1];
		expectedDataRow.callbacks.ignoreSchedules(test.campaign.selectedDates);
		 
		expect(expectedDataRow.campaign.ignoredDates[0]).toEqual(1);
		expect(expectedDataRow.campaign.ignoredDates[1]).toEqual(1);
		expect(expectedDataRow.campaign.ignoredDates[3]).toEqual(undefined);
	});

	it('should calculate and show plan data correctly when has overstaff data after hide schedule', function() {
		var test = setUpTarget();
		initDataForIgnore(test);
		test.scope.campaignClicked(null, {
			id: test.campaign.Id
		});
		test.campaign.selectedDates = ['2017-06-11'];
		var index = test.campaign.graphData.dates.indexOf(test.campaign.selectedDates[0]);
		test.campaign.graphData.rawPlans[index] = 10;
		test.campaign.graphData.overStaff[index] = 10;

		var expectedDataRow = test.scope.ganttData[1];
		expectedDataRow.callbacks.ignoreSchedules(test.campaign.selectedDates);

		expect(test.campaign.graphData.unscheduledPlans[index]).toEqual(0);
	});

	it('should restore schedule data after show schedule', function() {
		var test = setUpTarget();
		initDataForIgnore(test);
		test.scope.campaignClicked(null, {
			id: test.campaign.Id
		});
		test.campaign.selectedDates = ['2017-06-11'];
		var expectedDataRow = test.scope.ganttData[1];
		var index = test.campaign.graphData.dates.indexOf(test.campaign.selectedDates[0]);

		expectedDataRow.callbacks.ignoreSchedules(test.campaign.selectedDates);
		expect(test.campaign.graphData.unscheduledPlans[index]).toEqual(10);
		expect(test.campaign.graphData.schedules[index]).toEqual(0);

		expectedDataRow.callbacks.showAllSchedules(test.campaign.selectedDates);
		expect(test.campaign.graphData.schedules[index]).toEqual(20);
		expect(test.campaign.graphData.unscheduledPlans[index]).toEqual(0);
	});

	it('should remove ignored dates after show schedule', function () {
		var test = setUpTarget();
		initDataForIgnore(test);
		test.scope.campaignClicked(null, {
			id: test.campaign.Id
		});
		test.campaign.selectedDates = ['2017-06-11'];
		var expectedDataRow = test.scope.ganttData[1];
		
		expectedDataRow.callbacks.ignoreSchedules(test.campaign.selectedDates);
		expect(expectedDataRow.campaign.ignoredDates[3]).toEqual(1);

		expectedDataRow.callbacks.showAllSchedules(test.campaign.selectedDates);
		expect(expectedDataRow.campaign.ignoredDates[3]).toEqual(undefined);
	});

	it('should keep ignoring schedules after command callbacks', function() {
		var test = setUpTarget();
		initDataForIgnore(test);
		test.scope.campaignClicked(null, {
			id: test.campaign.Id
		});
		test.campaign.selectedDates = ['2017-06-07'];
		var expectedDataRow = test.scope.ganttData[1];
		var index = test.campaign.graphData.dates.indexOf(test.campaign.selectedDates[0]);
		var ignoredDates = test.campaign.selectedDates;
		var hasSchedulesIgnored = true;

		test.campaign.graphData.schedules[index] = 20;
		expectedDataRow.callbacks.addManualPlan(test.campaign, null, ignoredDates, hasSchedulesIgnored);
		expect(test.campaign.graphData.schedules[index]).toEqual(0);

		test.campaign.graphData.schedules[index] = 20;
		expectedDataRow.callbacks.removeManualPlan(test.campaign, null, ignoredDates, hasSchedulesIgnored);
		expect(test.campaign.graphData.schedules[index]).toEqual(0);

		test.campaign.graphData.schedules[index] = 20;
		expectedDataRow.callbacks.addManualBacklog(test.campaign, null, ignoredDates, hasSchedulesIgnored);
		expect(test.campaign.graphData.schedules[index]).toEqual(0);

		test.campaign.graphData.schedules[index] = 20;
		expectedDataRow.callbacks.removeManualBacklog(test.campaign, null, ignoredDates, hasSchedulesIgnored);
		expect(test.campaign.graphData.schedules[index]).toEqual(0);

		test.campaign.graphData.schedules[index] = 20;
		expectedDataRow.callbacks.replan(test.campaign, null, ignoredDates, hasSchedulesIgnored);
		expect(test.campaign.graphData.schedules[index]).toEqual(0);
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

		var testCampaign = {
			CampaignSummary: {
				EndDate: "2017-06-08T00:00:00",
				Id: '7cbd1b1a-2adc-4119-b49c-a78600598189',
				Name: "test1",
				StartDate: "2017-06-07T00:00:00"
			},
			Id: '7cbd1b1a-2adc-4119-b49c-a78600598189',
			IsScheduled: true,
			graphData: {
				dates: ['x', '2017-06-07', '2017-06-08', '2017-06-09', '2017-06-10', '2017-06-11'],
				rawBacklogs: ['Backlog', 80, 60, 40, 20, 0],
				schedules: ['Scheduled', 20, 20, 20, 20, 20],
				rawSchedules: ['Scheduled', 20, 20, 20, 20, 20],
				rawPlans: ['x', 10, 10, 10, 10, 10],
				unscheduledPlans: ['Planned', 0, 0, 0, 0, 0],
				overStaff: ['Overstaffing', 0, 0, 0, 0, 0]
			},
			selectedDates: [],
			selectedDatesClosed: []
		};
		var testTasks = [{
			id: testCampaign.Id,
			from: testCampaign.CampaignSummary.StartDate,
			to: testCampaign.CampaignSummary.EndDate,
			color: 'rgba(0,0,0,0.6)',
			tooltips: {
				'enabled': true
			}
		}];

		return {
			target: target,
			scope: scope,
			campaign: testCampaign,
			tasks: testTasks
		};
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
		var campaigns = [],
			campaignSummaries = [],
			PhaseStatistics,
			ganttVisualization = [],
			campaignDetail = {},
			thresholdObj = {},
			listCampaign = [],
			visualizationPeriod = {
				PeriodStart: new Date(),
				PeriodEnd: new Date()
			};

		this.checkPermission = function() {
			return {
				then: function(cb) {
					//todo: need prepare init !
					// cb();
				}
			}
		};

		this.loadCampaignSchedules = function(period, cb) {
			if (cb) cb();
		};

		this.setGanttPeriod = function(period) {
			visualizationPeriod = period;
		};

		this.getGanttPeriod = function() {
			return visualizationPeriod;
		};

		this.setGanttVisualization = function(ganttV) {
			ganttVisualization.push(ganttV);
		};

		this.getCampaigns = function(ganttPeriod, cb) {
			cb(ganttVisualization);
		};

		this.setThreshold = function(threshold) {
			thresholdObj = threshold;
		};

		this.getThreshold = function(cb) {
			cb(thresholdObj);
		};

		this.setCampaignStatus = function(campaignD) {
			campaignDetail = campaignD;
		};

		this.getCampaignStatus = function(id, cb) {
			cb(campaignDetail);
		};

		this.setCampaignsStatus = function(listCampaigns) {
			listCampaign.push(listCampaigns);
		};

		this.updateCampaignsStatus = function(cb) {
			cb(listCampaign);
		};

		this.loadWithinPeriod = function() {};

		this.getDefaultPeriod = function() {
			return ['', ''];
		};

		this.setPhaseStatistics = function(d) {
			PhaseStatistics = d;
		};

		this.getCampaign = function(campaignId, successCb, errorCb) {};

		this.addCampaign = function(campaign, successCb, errorCb) {
			campaigns.push(campaign);
			successCb(campaign);
		};

		this.calculateCampaignPersonHour = function(campaign) {
			return campaign.calculatedPersonHour;
		};

		this.load = function() {};

		this.prepareCampaignSummary = function(summary) {
			campaignSummaries.push(summary);
		};
	}

	function fakeOutboundChartService() {
		var campaignViss = {};

		this.setCampaignVisualization = function(id, vis) {
			campaignViss[id] = vis;
		};

		this.getCampaignVisualization = function(id, success) {
			success(campaignViss[id]);
		};

		this.makeGraph = function() {
			return {
				graph: 'c3'
			}
		};

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

	function initDataForIgnore(test) {
		test.target.init();
		test.scope.ganttData = [
			{
				id: test.campaign.Id,
				tasks: test.tasks
			}];
		test.scope.isRefreshingGantt = false;

		outboundService.setCampaignStatus({
			CampaignSummary: test.campaign.CampaignSummary,
			IsScheduled: true,
			WarningInfo: []
		});
		outboundService.setGanttVisualization({
			Id: test.campaign.Id,
			StartDate: {
				Date: test.campaign.CampaignSummary.StartDate
			},
			EndDate: {
				Date: test.campaign.CampaignSummary.EndDate
			}
		});
		outboundChartService.setCampaignVisualization(test.campaign.Id, test.campaign.graphData);
	}
});