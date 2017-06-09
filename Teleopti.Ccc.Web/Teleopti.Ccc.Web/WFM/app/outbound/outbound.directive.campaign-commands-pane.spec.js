'use strict';
describe('Outbound campaign commands pane tests ', function() {
	var $rootScope,
		$compile,
		$httpBackend,
		stateService,
		toggleSvc,
		target;

	var ignoreSchedulesCallbackCalledCount = 0;

	beforeEach(function() {
		module('wfm.templates');
		module('wfm.outbound');

		toggleSvc = {
			togglesLoaded: {
				then: function(cb) {
					cb();
				}
			}
		};

		module(function($provide) {
			$provide.service('$state', function() {
				return stateService;
			});

			$provide.service('Toggle', function() {
				return toggleSvc;
			});
		});

	});

	beforeEach(inject(function(_$rootScope_, _$compile_, _$httpBackend_) {
		$rootScope = _$rootScope_;
		$compile = _$compile_;
		$httpBackend = _$httpBackend_;
		target = setUpTarget();
		ignoreSchedulesCallbackCalledCount = 0;

		$httpBackend.whenPOST('../api/Outbound/Campaign/Replan').respond(function(method, url, data) {
			return [200, {}];
		});
	}));

	it('should show manual plan button by default', function() {
		expect(target.container[0].querySelectorAll('.btn-toggle-manual-plan').length).toEqual(1);
	});

	it('should show manual backlog button by default', function() {
		expect(target.container[0].querySelectorAll('.btn-toggle-manual-backlog').length).toEqual(1);
	});

	it('should show replan button by default', function() {
		expect(target.container[0].querySelectorAll('.btn-replan').length).toEqual(1);
	});

	it('should show campaign edit button by default', function() {
		expect(target.container[0].querySelectorAll('.btn-goto-edit-campaign').length).toEqual(1);
	});

	it('should get correct ignored dates for replanning when toggle Wfm_Outbound_ReplanAfterScheduled_43752 is on', function() {
		toggleSvc.Wfm_Outbound_ReplanAfterScheduled_43752 = true;
		target.vm.replan();
		expect(target.vm.ignoredDates.length).toEqual(2);
	});

	it('should get correct ignored dates for replanning when toggle Wfm_Outbound_ReplanAfterScheduled_43752 is off', function() {
		toggleSvc.Wfm_Outbound_ReplanAfterScheduled_43752 = false;
		target.vm.replan();
		expect(target.vm.ignoredDates.length).toEqual(0);
	});

	it('should show ignore schedule button when there are schedules in campaign', function() {
		toggleSvc.Wfm_Outbound_ReplanAfterScheduled_43752 = true;
		target.scope.$apply();
		expect(target.container[0].querySelectorAll('.btn-ignore-schedules').length).toEqual(1);
	});

	it('should not show ignore schedule button when toggle off', function() {
		toggleSvc.Wfm_Outbound_ReplanAfterScheduled_43752 = false;
		target.scope.$apply();
		expect(target.container[0].querySelectorAll('.btn-ignore-schedules').length).toEqual(0);
	});

	it('ignore schedule button should be disabled when no day selected', function() {
		toggleSvc.Wfm_Outbound_ReplanAfterScheduled_43752 = true;
		target.vm.selectedDates = [];
		target.scope.$apply();
		expect(target.container[0].querySelectorAll('.btn-ignore-schedules:disabled').length).toEqual(1);
	});

	it('should disable ignore shedule button when selected dates have no schedule', function () {
		toggleSvc.Wfm_Outbound_ReplanAfterScheduled_43752 = true;
		target.vm.selectedDates = ['2017-06-09'];
		var index = target.vm.campaign.graphData.dates.indexOf(target.vm.selectedDates[0]);
		target.vm.campaign.graphData.schedules[index] = 0;
		target.scope.$apply();
		expect(target.container[0].querySelectorAll('.btn-ignore-schedules:disabled').length).toEqual(1);
	});

	it('should enable ignore shedule button when any selected dates has schedule', function () {
		toggleSvc.Wfm_Outbound_ReplanAfterScheduled_43752 = true;
		target.vm.selectedDates = ['2017-06-08', '2017-06-09'];
		var graphData = target.vm.campaign.graphData;

		graphData.schedules[graphData.dates.indexOf(target.vm.selectedDates[0])] = 20;
		graphData.schedules[graphData.dates.indexOf(target.vm.selectedDates[1])] = 0;
		target.scope.$apply();
		expect(target.container[0].querySelectorAll('.btn-ignore-schedules').length).toEqual(1);
	});

	it('ignore schedule button should be enabled when any day selected', function() {
		toggleSvc.Wfm_Outbound_ReplanAfterScheduled_43752 = true;
		target.scope.$apply();
		expect(target.container[0].querySelectorAll('.btn-ignore-schedules:disabled').length).toEqual(0);
	});

	it('should get ignored schedule days when click ignore schedule button', function() {
		toggleSvc.Wfm_Outbound_ReplanAfterScheduled_43752 = true;
		target.scope.$apply();
		angular.element(target.container[0].querySelectorAll('.btn-ignore-schedules')).triggerHandler('click');
		expect(target.vm.ignoredDates.length).toEqual(2);
	});

	it('should show plan data after clicking ignore schedule button', function() {
		toggleSvc.Wfm_Outbound_ReplanAfterScheduled_43752 = true;
		target.scope.$apply();
		angular.element(target.container[0].querySelectorAll('.btn-ignore-schedules')).triggerHandler('click');
		expect(ignoreSchedulesCallbackCalledCount).toEqual(1);
	});

	it('should reset states for other buttons', function() {
		toggleSvc.Wfm_Outbound_ReplanAfterScheduled_43752 = true;
		target.scope.$apply();
		angular.element(target.container[0].querySelectorAll('.btn-toggle-manual-plan')).triggerHandler('click');
		expect(target.vm.manualPlanSwitch).toEqual(true);
		angular.element(target.container[0].querySelectorAll('.btn-ignore-schedules')).triggerHandler('click');
		expect(target.vm.manualPlanSwitch).toEqual(false);
	});
	
	function setUpTarget() {
		var html = '<campaign-commands-pane campaign="campaign" selected-dates="campaign.selectedDates" selected-dates-closed="campaign.selectedDatesClosed" is-loading="isRefreshingData" callbacks="callbacks"></campaign-commands-pane>'

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
				unscheduledPlans: ['Planned', 0, 0, 20]
			},
			selectedDates: ['2017-06-07', '2017-06-08'],
			selectedDatesClosed: []
		};
		scope.isRefreshingData = false;
		scope.callbacks = {
			ignoreSchedules: function(ignoredDates, callback) {
				ignoreSchedulesCallbackCalledCount++;
				callback && callback();
			}
		};

		var container = $compile(html)(scope);
		scope.$apply();

		var vm = container.isolateScope().vm;
		return {
			container: container,
			vm: vm,
			scope: scope
		};
	}
});