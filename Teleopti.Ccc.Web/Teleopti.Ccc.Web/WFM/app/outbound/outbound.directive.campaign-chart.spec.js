'use strict';
describe('Outbound campaign chart tests ', function () {
	var $rootScope,
		$filter,
		$compile,
		toggleSvc,
		target;

	beforeEach(function () {
		module('wfm.templates');
		module('wfm.outbound');

		toggleSvc = {
			togglesLoaded: {
				then: function (cb) {
					cb();
				}
			}
		};

		module(function ($provide) {
			$provide.service('Toggle', function () {
				return toggleSvc;
			});
		});

	});

	beforeEach(inject(function (_$rootScope_, _$filter_, _$compile_) {
		$rootScope = _$rootScope_;
		$filter = _$filter_;
		$compile = _$compile_;
		target = setUpTarget();
	}));

	it('should find chart extra info', function () {
		expect(target.container[0].querySelectorAll('.chart-extra-info').length).toEqual(1);
	});

	it('should get Ignore Schedule info', function () {
		var scope = target.scope;
		var container = target.container;
		var extraInfos = container.isolateScope().extraInfos;
		expect(extraInfos).toContain("I: " + scope.campaign.translations["IgnoredScheduleHint"]);
	});

	it('should get ignored schedule hint', function () {
		var container = target.container;
		var isolateScope = container.isolateScope();
		var controller = container.controller("campaignChart");
		var option = controller._setChartOption_data();

		var scope = target.scope;
		scope.campaign.ignoredDates = [1];
		scope.$apply();

		var hint = option.labels.format['Overstaffing'](1, isolateScope.campaign.id, 1);
		expect(hint).toEqual('I');
	});

	it('should get ignored schedule and mannual plan hint', function () {
		var container = target.container;
		var isolateScope = container.isolateScope();
		var controller = container.controller("campaignChart");
		var option = controller._setChartOption_data();

		isolateScope.campaign.rawManualPlan = [1];
		isolateScope.campaign.ignoredDates = [1];
		isolateScope.campaign.graphData.schedules[2] = 0;
		isolateScope.$apply();

		var hint = option.labels.format['Overstaffing'](1, isolateScope.campaign.id, 1);
		expect(hint).toContain('I');
		expect(hint).toContain('M');
	});

	it('should get ignored schedule and mannual backlog hint', function () {
		var container = target.container;
		var isolateScope = container.isolateScope();
		var controller = container.controller("campaignChart");
		var option = controller._setChartOption_data();

		isolateScope.campaign.ignoredDates = [1];
		isolateScope.campaign.isManualBacklog = [1];

		isolateScope.$apply();

		var hint = option.labels.format['Overstaffing'](1, isolateScope.campaign.id, 1);
		expect(hint).toContain('I');
		expect(hint).toContain('B');
	});

	it('should get ignored schedule,mannual backlog and mannual plan hint', function () {
		var container = target.container;
		var isolateScope = container.isolateScope();
		var controller = container.controller("campaignChart");
		var option = controller._setChartOption_data();

		isolateScope.campaign.ignoredDates = [1];
		isolateScope.campaign.isManualBacklog = [1];
		isolateScope.campaign.rawManualPlan = [1];
		isolateScope.campaign.graphData.schedules[2] = 0;

		isolateScope.$apply();

		var hint = option.labels.format['Overstaffing'](1, isolateScope.campaign.id, 1);
		expect(hint).toContain('I');
		expect(hint).toContain('B');
		expect(hint).toContain('M');
	});


	function setUpTarget() {
		var html = '<campaign-chart campaign="campaign" dictionary="campaign.translations" exclude-closed="campaign.manualPlanswitch"></campaign-chart>';

		var scope = $rootScope.$new();
		scope.campaign = {
			CampaignSummary: {
				EndDate: "2017-06-09T00:00:00",
				Id: "7cbd1b1a-2adc-4119-b49c-a78600598189",
				Name: "test1",
				StartDate: "2017-06-02T00:00:00"
			},
			Id: "7cbd1b1a-2adc-4119-b49c-a78600598189",
			IsScheduled: true,
			graphData: {
				dates: ['x', '2017-06-07', '2017-06-08', '2017-06-09'],
				rawBacklogs: ['Backlog', 80, 60, 40],
				schedules: ['Scheduled', 20, 20, 0],
				unscheduledPlans: ['Planned', 0, 0, 20],
				progress: ['Progress', 20, 20, 0],
				overStaff: ['OverStaff', 0, 0, 0]
			},
			selectedDates: ['2017-06-07', '2017-06-08'],
			selectedDatesClosed: [],
			manualPlanswitch: false,
			translations: {
				AddBacklog: "Manual Backlog",
				Backlog: "Backlog",
				ClosedDay: "Closed Day",
				EndDate: "End",
				ManualPlan: "Manual Plan",
				ManuallyPlanned: "Manually Planned",
				NeededPersonHours: "Needed Hours",
				Overstaffing: "Overstaffing",
				Planned: "Planned",
				Progress: "Progress",
				Scheduled: "Scheduled",
				Start: "Start",
				Today: "Today",
				IgnoredScheduleHint: "Ignored Schedule"
			},
			WarningInfo: [],
			closedDays: [],
			rawManualPlan: [],
			isManualBacklog: [],
			ignoredDates:[]
	};
		
		var container = $compile(html)(scope);
		scope.$apply();

		return {
			container: container,
			scope: scope
		};
	}
});