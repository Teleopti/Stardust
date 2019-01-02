'use strict';
describe('planningPeriodOverviewController', function() {
	var $httpBackend,
		$controller,
		$injector,
		$interval,
		$rootScope,
		fakeBackend,
		NoticeService,
		planningPeriodServiceNew,
		scope,
		vm,
		stateparams = { groupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e', ppId: 'a557210b-99cc-4128-8ae0-138d812974b6' },
		planningGroupInfo = {
			Id: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e',
			Name: 'Plan Group Test',
			Filters: []
		},
		selectedPp = {
			PlanningGroupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e',
			StartDate: '2018-06-18T00:00:00',
			EndDate: '2018-07-15T00:00:00',
			HasNextPlanningPeriod: true,
			Id: 'a557210b-99cc-4128-8ae0-138d812974b6',
			State: 'Scheduled',
			ValidationResult: {
				InvalidResources: []
			}
		};

	beforeEach(function() {
		module('wfm.resourceplanner', 'localeLanguageSortingService');
	});

	beforeEach(inject(function(
		_$httpBackend_,
		_$controller_,
		_$interval_,
		_$rootScope_,
		_fakeResourcePlanningBackend_,
		_NoticeService_,
		_planningPeriodServiceNew_
	) {
		$httpBackend = _$httpBackend_;
		$controller = _$controller_;
		$interval = _$interval_;
		$rootScope = _$rootScope_;
		NoticeService = _NoticeService_;
		planningPeriodServiceNew = _planningPeriodServiceNew_;
		fakeBackend = _fakeResourcePlanningBackend_;

		fakeBackend.clear();

		spyOn(planningPeriodServiceNew, 'lastJobStatus').and.callThrough();

		$httpBackend.whenGET('../ToggleHandler/AllToggles').respond(function(method, url, data, headers) {
			return [200];
		});

		$httpBackend
			.whenGET(
				'../api/resourceplanner/planningperiod/a557210b-99cc-4128-8ae0-138d812974b6/countagents?endDate=2018-07-15T00:00:00&startDate=2018-06-18T00:00:00'
			)
			.respond(function(method, url, data, headers) {
				return [
					200,
					{
						TotalAgents: 50
					}
				];
			});

		$httpBackend
			.whenPOST('../api/resourceplanner/planningperiod/a557210b-99cc-4128-8ae0-138d812974b6/schedule')
			.respond(function(method, url, data, headers) {
				return [200, true];
			});

		$httpBackend
			.whenPOST('../api/resourceplanner/planningperiod/a557210b-99cc-4128-8ae0-138d812974b6/optimizeintraday')
			.respond(function(method, url, data, headers) {
				return [200, true];
			});

		$httpBackend
			.whenPOST('../api/resourceplanner/planningperiod/a557210b-99cc-4128-8ae0-138d812974b6/publish')
			.respond(function(method, url, data, headers) {
				return [200, true];
			});

		$httpBackend
			.whenDELETE('../api/resourceplanner/planningperiod/a557210b-99cc-4128-8ae0-138d812974b6/schedule')
			.respond(function(method, url, data, headers) {
				return [200, true];
			});

		scope = $rootScope.$new();
		vm = $controller('planningPeriodOverviewController as ctrl', {
			$scope: scope,
			$stateParams: stateparams,
			selectedPp: selectedPp,
			planningGroupInfo: planningGroupInfo
		});
		scope.$destroy();
	}));

	afterEach(function() {
		$httpBackend.verifyNoOutstandingExpectation();
		$httpBackend.verifyNoOutstandingRequest();
	});

	xit('should cancel the auto statue check when needed', function() {
		var result = {};
		fakeBackend.withScheduleResult(result);
		$httpBackend.flush();

		expect(planningPeriodServiceNew.lastJobStatus).toHaveBeenCalledWith({
			id: 'a557210b-99cc-4128-8ae0-138d812974b6'
		});
		expect(planningPeriodServiceNew.lastJobStatus.calls.count()).toEqual(1);
	});

	it('should launch schedule', function() {
		spyOn(planningPeriodServiceNew, 'launchScheduling').and.callThrough();
		fakeBackend.withStatus({
			SchedulingStatus: {
				Failed: false,
				HasJob: true,
				Successful: false
			}
		});
		vm.launchSchedule();
		$httpBackend.flush();

		expect(planningPeriodServiceNew.launchScheduling).toHaveBeenCalledWith({
			id: 'a557210b-99cc-4128-8ae0-138d812974b6'
		});
		expect(vm.schedulingPerformed).toEqual(true);
	});

	it('should check progress and return schedule is running', function() {
		vm.schedulingPerformed = true;
		fakeBackend.withStatus({
			SchedulingStatus: {
				Failed: false,
				HasJob: true,
				Successful: false
			}
		});
		$httpBackend.flush();

		expect(planningPeriodServiceNew.lastJobStatus).toHaveBeenCalledWith({
			id: 'a557210b-99cc-4128-8ae0-138d812974b6'
		});
		expect(vm.status).toEqual('PresentTenseSchedule');
		expect(vm.schedulingPerformed).toEqual(true);
	});

	it('should check progress and return schedule is done with success', function() {
		spyOn(NoticeService, 'success').and.callThrough();
		vm.schedulingPerformed = true;
		fakeBackend.withStatus({
			SchedulingStatus: {
				Failed: false,
				HasJob: true,
				Successful: true
			}
		});
		$httpBackend.flush();

		expect(NoticeService.success).toHaveBeenCalledWith('SuccessfullyScheduledPlanningPeriodFromTo', null, true);
		expect(vm.schedulingPerformed).toEqual(false);
		expect(vm.status).toEqual('');
		expect(vm.schedulingPerformed).toEqual(false);
	});

	it('should check progress and return schedule is failed', function() {
		spyOn(NoticeService, 'warning').and.callThrough();
		vm.schedulingPerformed = true;
		fakeBackend.withStatus({
			SchedulingStatus: {
				Failed: true,
				HasJob: true,
				Successful: false
			}
		});
		$httpBackend.flush();

		expect(planningPeriodServiceNew.lastJobStatus).toHaveBeenCalledWith({
			id: 'a557210b-99cc-4128-8ae0-138d812974b6'
		});
		expect(NoticeService.warning).toHaveBeenCalledWith(
			'FailedToScheduleForSelectedPlanningPeriodDueToTechnicalError',
			null,
			true
		);
		expect(vm.status).toEqual('');
		expect(vm.schedulingPerformed).toEqual(false);
	});

	it('should launch intraday optimization and return intraday optimization is running', function() {
		spyOn(planningPeriodServiceNew, 'launchIntraOptimize').and.callThrough();
		fakeBackend.withStatus({
			IntradayOptimizationStatus: {
				Failed: false,
				HasJob: true,
				Successful: false
			}
		});
		vm.intraOptimize();
		$httpBackend.flush();

		expect(planningPeriodServiceNew.launchIntraOptimize).toHaveBeenCalledWith({
			id: 'a557210b-99cc-4128-8ae0-138d812974b6'
		});
		expect(vm.optimizeRunning).toEqual(true);
	});

	it('should check intraday optimization progress and return intraday optimization is done with success', function() {
		spyOn(NoticeService, 'success').and.callThrough();
		vm.optimizeRunning = true;
		fakeBackend.withStatus({
			IntradayOptimizationStatus: {
				Failed: false,
				HasJob: true,
				Successful: true
			}
		});
		$httpBackend.flush();

		expect(planningPeriodServiceNew.lastJobStatus).toHaveBeenCalledWith({
			id: 'a557210b-99cc-4128-8ae0-138d812974b6'
		});
		expect(NoticeService.success).toHaveBeenCalledWith(
			'SuccessfullyIntradayOptimizationPlanningPeriodFromTo',
			null,
			true
		);
		expect(vm.status).toEqual('');
		expect(vm.optimizeRunning).toEqual(false);
	});

	it('should check intraday optimization progress and return intraday optimization is failed', function() {
		spyOn(NoticeService, 'warning').and.callThrough();
		vm.optimizeRunning = true;
		fakeBackend.withStatus({
			IntradayOptimizationStatus: {
				Failed: true,
				HasJob: true,
				Successful: false
			}
		});
		$httpBackend.flush();

		expect(planningPeriodServiceNew.lastJobStatus).toHaveBeenCalledWith({
			id: 'a557210b-99cc-4128-8ae0-138d812974b6'
		});
		expect(NoticeService.warning).toHaveBeenCalledWith(
			'FailedToIntradayOptimizeForSelectedPlanningPeriodDueToTechnicalError',
			null,
			true
		);
		expect(vm.status).toEqual('');
		expect(vm.optimizeRunning).toEqual(false);
	});

	it('should launch clear schedule and return clear schedule is running', function() {
		spyOn(planningPeriodServiceNew, 'clearSchedules').and.callThrough();

		fakeBackend.withScheduleResult({
			FullSchedulingResult: {
				BusinessRulesValidationResults: [],
				SkillResultList: [],
				ScheduledAgentsCount: 44
			},
			PlanningPeriod: {
				StartDate: '2018-06-18T00:00:00',
				EndDate: '2018-07-15T00:00:00'
			}
		});
		$httpBackend.flush();

		fakeBackend.withStatus({
			ClearScheduleStatus: {
				Failed: false,
				HasJob: true,
				Successful: false
			}
		});
		vm.clearSchedules();
		$httpBackend.flush();

		expect(planningPeriodServiceNew.clearSchedules).toHaveBeenCalledWith({
			id: 'a557210b-99cc-4128-8ae0-138d812974b6'
		});
		expect(vm.clearRunning).toEqual(true);
	});

	it('should check clear schedule progress and return clear schedule is done with success', function() {
		spyOn(NoticeService, 'success').and.callThrough();
		vm.clearRunning = true;
		fakeBackend.withStatus({
			ClearScheduleStatus: {
				Failed: false,
				HasJob: true,
				Successful: true
			}
		});
		$httpBackend.flush();

		expect(planningPeriodServiceNew.lastJobStatus).toHaveBeenCalledWith({
			id: 'a557210b-99cc-4128-8ae0-138d812974b6'
		});
		expect(NoticeService.success).toHaveBeenCalledWith('SuccessClearPlanningPeriodData', 20000, true);
		expect(vm.status).toEqual('');
		expect(vm.clearRunning).toEqual(false);
	});

	it('should check clear schedule progress and return clear schedule is failed', function() {
		spyOn(NoticeService, 'warning').and.callThrough();
		vm.clearRunning = true;
		fakeBackend.withStatus({
			ClearScheduleStatus: {
				Failed: true,
				HasJob: true,
				Successful: false
			}
		});
		$httpBackend.flush();

		expect(planningPeriodServiceNew.lastJobStatus).toHaveBeenCalledWith({
			id: 'a557210b-99cc-4128-8ae0-138d812974b6'
		});
		expect(NoticeService.warning).toHaveBeenCalledWith(
			'FailedToClearScheduleForSelectedPlanningPeriodDueToTechnicalError',
			null,
			true
		);
		expect(vm.status).toEqual('');
		expect(vm.clearRunning).toEqual(false);
	});

	it('should launch publish for planning period return done success', function() {
		spyOn(NoticeService, 'success').and.callThrough();
		spyOn(planningPeriodServiceNew, 'publishPeriod').and.callThrough();

		vm.publishSchedule();
		$httpBackend.flush();

		expect(planningPeriodServiceNew.publishPeriod).toHaveBeenCalledWith({
			id: 'a557210b-99cc-4128-8ae0-138d812974b6'
		});
		expect(NoticeService.success).toHaveBeenCalledWith(
			'PublishScheduleSucessForSelectedPlanningPeriod',
			null,
			true
		);
		expect(vm.publishRunning).toEqual(false);
	});

	it('should block launch publish when double click', function() {
		spyOn(NoticeService, 'warning').and.callThrough();
		spyOn(planningPeriodServiceNew, 'publishPeriod').and.callThrough();
		$httpBackend.flush();

		vm.publishRunning = true;
		vm.publishSchedule();

		expect(NoticeService.warning).not.toHaveBeenCalledWith('PublishingScheduleSuccess', null, true);
		expect(vm.publishRunning).toEqual(true);
	});
});
