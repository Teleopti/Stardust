'use strict';
describe('planningPeriodOverviewController', function () {
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
            Name: "Agent Group Test",
            Filters: []
        },
        selectedPp = {
            AgentGroupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e',
            StartDate: "2018-06-18T00:00:00",
            EndDate: "2018-07-15T00:00:00",
            HasNextPlanningPeriod: true,
            Id: 'a557210b-99cc-4128-8ae0-138d812974b6',
            State: "Scheduled",
            ValidationResult: {
                InvalidResources: []
            }
        };

    beforeEach(function () {
        module('wfm.resourceplanner');
    });

    beforeEach(inject(function (_$httpBackend_, _$controller_, _$interval_, _$rootScope_, _fakeResourcePlanningBackend_, _NoticeService_, _planningPeriodServiceNew_) {
        $httpBackend = _$httpBackend_;
        $controller = _$controller_;
        $interval = _$interval_;
        $rootScope = _$rootScope_;
        NoticeService = _NoticeService_;
        planningPeriodServiceNew = _planningPeriodServiceNew_;
        fakeBackend = _fakeResourcePlanningBackend_;

        fakeBackend.clear();

        spyOn(planningPeriodServiceNew, 'lastJobStatus').and.callThrough();
        spyOn(planningPeriodServiceNew, 'lastIntradayOptimizationJobStatus').and.callThrough();

        $httpBackend.whenGET('../api/resourceplanner/planningperiod/a557210b-99cc-4128-8ae0-138d812974b6/countagents?endDate=2018-07-15T00:00:00&startDate=2018-06-18T00:00:00').respond(function (method, url, data, headers) {
            return [200, {
                TotalAgents: 50
            }];
        });

        $httpBackend.whenPOST('../api/resourceplanner/planningperiod/a557210b-99cc-4128-8ae0-138d812974b6/schedule?runAsynchronously=true').respond(function (method, url, data, headers) {
            return [200, true];
        });

        $httpBackend.whenPOST('../api/resourceplanner/planningperiod/a557210b-99cc-4128-8ae0-138d812974b6/optimizeintraday?runAsynchronously=true').respond(function (method, url, data, headers) {
            return [200, true];
        });

        $httpBackend.whenPOST('../api/resourceplanner/planningperiod/a557210b-99cc-4128-8ae0-138d812974b6/publish').respond(function (method, url, data, headers) {
            return [200, true];
        });

        $httpBackend.whenDELETE('../api/resourceplanner/planningperiod/a557210b-99cc-4128-8ae0-138d812974b6/schedule').respond(function (method, url, data, headers) {
            return [200, true];
        });

        scope = $rootScope.$new();
        vm = $controller('planningPeriodOverviewController as ctrl', { $scope: scope, $stateParams: stateparams, selectedPp: selectedPp, planningGroupInfo: planningGroupInfo });
        scope.$destroy();
    }));

    afterEach(function () {
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
    });

    xit('should cancel the auto statue check when needed', function () {
        var result = {}
        fakeBackend.withScheduleResult(result);
        fakeBackend.withIntradayStatus(result);
        $httpBackend.flush();

        expect(planningPeriodServiceNew.lastJobStatus).toHaveBeenCalledWith({ id: 'a557210b-99cc-4128-8ae0-138d812974b6' });
        expect(planningPeriodServiceNew.lastJobStatus.calls.count()).toEqual(1);
        expect(planningPeriodServiceNew.lastIntradayOptimizationJobStatus).toHaveBeenCalledWith({ id: 'a557210b-99cc-4128-8ae0-138d812974b6' });
        expect(planningPeriodServiceNew.lastIntradayOptimizationJobStatus.calls.count()).toEqual(1);
    });

    it('should launch schedule', function () {
        spyOn(planningPeriodServiceNew, 'launchScheduling').and.callThrough();
        fakeBackend.withScheduleStatus({
            CurrentStep: 0,
            Failed: false,
            HasJob: true,
            Successful: false,
            TotalSteps: 2
        });
        $httpBackend.flush();

        vm.launchSchedule();
        $httpBackend.flush();

        expect(planningPeriodServiceNew.launchScheduling).toHaveBeenCalledWith({ id: 'a557210b-99cc-4128-8ae0-138d812974b6', runAsynchronously: true });
        expect(vm.schedulingPerformed).toEqual(true);
    });

    it('should check progress and return schedule is running on step 0', function () {
        vm.schedulingPerformed = true;
        fakeBackend.withScheduleStatus({
            CurrentStep: 0,
            Failed: false,
            HasJob: true,
            Successful: false,
            TotalSteps: 2
        });
        $httpBackend.flush();

        expect(planningPeriodServiceNew.lastJobStatus).toHaveBeenCalledWith({ id: 'a557210b-99cc-4128-8ae0-138d812974b6' });
        expect(vm.status).toEqual('PresentTenseSchedule');
        expect(vm.schedulingPerformed).toEqual(true);
    });

    it('should check progress and return schedule is running on step 1', function () {
        vm.schedulingPerformed = true;
        fakeBackend.withScheduleStatus({
            CurrentStep: 1,
            Failed: false,
            HasJob: true,
            Successful: false,
            TotalSteps: 2
        });
        $httpBackend.flush();

        expect(planningPeriodServiceNew.lastJobStatus).toHaveBeenCalledWith({ id: 'a557210b-99cc-4128-8ae0-138d812974b6' });
        expect(vm.status).toEqual('OptimizingDaysOff');
        expect(vm.schedulingPerformed).toEqual(true);
    });

    it('should check progress and return schedule is done with success (step 2)', function () {
        spyOn(NoticeService, 'success').and.callThrough();
        vm.schedulingPerformed = true;
        fakeBackend.withScheduleStatus({
            CurrentStep: 2,
            Failed: false,
            HasJob: true,
            Successful: true,
            TotalSteps: 2
        });
        vm.launchSchedule();
        $httpBackend.flush();

        expect(NoticeService.success).toHaveBeenCalledWith('SuccessfullyScheduledPlanningPeriodFromTo', null, true);
        expect(vm.schedulingPerformed).toEqual(false);
        expect(vm.status).toEqual('');
        expect(vm.schedulingPerformed).toEqual(false);
    });

    it('should check progress and return schedule is failed by step 0', function () {
        spyOn(NoticeService, 'warning').and.callThrough();
        vm.schedulingPerformed = true;
        fakeBackend.withScheduleStatus({
            CurrentStep: 0,
            Failed: true,
            HasJob: true,
            Successful: false,
            TotalSteps: 2
        });
        $httpBackend.flush();

        expect(planningPeriodServiceNew.lastJobStatus).toHaveBeenCalledWith({ id: 'a557210b-99cc-4128-8ae0-138d812974b6' });
        expect(NoticeService.warning).toHaveBeenCalledWith('FailedToScheduleForSelectedPlanningPeriodDueToTechnicalError', null, true);
        expect(vm.status).toEqual('');
        expect(vm.schedulingPerformed).toEqual(false);
    });

    it('should check progress and return schedule is failed by step 1', function () {
        spyOn(NoticeService, 'warning').and.callThrough();
        vm.schedulingPerformed = true;
        fakeBackend.withScheduleStatus({
            CurrentStep: 1,
            Failed: true,
            HasJob: true,
            Successful: false,
            TotalSteps: 2
        });
        $httpBackend.flush();

        expect(planningPeriodServiceNew.lastJobStatus).toHaveBeenCalledWith({ id: 'a557210b-99cc-4128-8ae0-138d812974b6' });
        expect(NoticeService.warning).toHaveBeenCalledWith('FailedToOptimizeDayoffForSelectedPlanningPeriodDueToTechnicalError', null, true);
        expect(vm.status).toEqual('');
        expect(vm.schedulingPerformed).toEqual(false);
    });

    it('should launch intraday optimization and return intraday optimization is running', function () {
        spyOn(planningPeriodServiceNew, 'launchIntraOptimize').and.callThrough();
        fakeBackend.withIntradayStatus({
            Failed: false,
            HasJob: true,
            Successful: false
        });
        $httpBackend.flush();

        vm.intraOptimize();
        $httpBackend.flush();

        expect(planningPeriodServiceNew.launchIntraOptimize).toHaveBeenCalledWith({ id: 'a557210b-99cc-4128-8ae0-138d812974b6', runAsynchronously: true });
        expect(vm.optimizeRunning).toEqual(true);
    });

    it('should check intraday optimization progress and return intraday optimization is done with success', function () {
        spyOn(NoticeService, 'success').and.callThrough();
        vm.optimizeRunning = true;
        fakeBackend.withIntradayStatus({
            Failed: false,
            HasJob: true,
            Successful: true
        });
        $httpBackend.flush();

        expect(planningPeriodServiceNew.lastJobStatus).toHaveBeenCalledWith({ id: 'a557210b-99cc-4128-8ae0-138d812974b6' });
        expect(NoticeService.success).toHaveBeenCalledWith('SuccessfullyIntradayOptimizationPlanningPeriodFromTo', null, true);
        expect(vm.status).toEqual('');
        expect(vm.optimizeRunning).toEqual(false);
    });

    it('should check intraday optimization progress and return intraday optimization is failed', function () {
        spyOn(NoticeService, 'warning').and.callThrough();
        vm.optimizeRunning = true;
        fakeBackend.withIntradayStatus({
            Failed: true,
            HasJob: true,
            Successful: false
        });
        $httpBackend.flush();

        expect(planningPeriodServiceNew.lastJobStatus).toHaveBeenCalledWith({ id: 'a557210b-99cc-4128-8ae0-138d812974b6' });
        expect(NoticeService.warning).toHaveBeenCalledWith('FailedToIntradayOptimizeForSelectedPlanningPeriodDueToTechnicalError', null, true);
        expect(vm.status).toEqual('');
        expect(vm.optimizeRunning).toEqual(false);
    });

    it('should clear schedule result and history data', function () {
        spyOn(NoticeService, 'success').and.callThrough();
        spyOn(planningPeriodServiceNew, 'clearSchedules').and.callThrough();
        fakeBackend.withScheduleResult({
            OptimizationResult: {
                SkillResultList: []
            },
            PlanningPeriod: {
                StartDate: "2018-06-18T00:00:00",
                EndDate: "2018-07-15T00:00:00"
            },
            ScheduleResult: {
                BusinessRulesValidationResults: {},
                ScheduledAgentsCount: 44
            }
        });
        $httpBackend.flush();

        vm.clearSchedules();
        $httpBackend.flush();

        expect(planningPeriodServiceNew.lastJobStatus).toHaveBeenCalledWith({ id: 'a557210b-99cc-4128-8ae0-138d812974b6' });
        expect(NoticeService.success).toHaveBeenCalledWith('SuccessClearPlanningPeriodData', 20000, true);
    });

    it('should launch publish for planning period return done success', function () {
        spyOn(NoticeService, 'success').and.callThrough();
        spyOn(planningPeriodServiceNew, 'publishPeriod').and.callThrough();

        vm.publishSchedule();
        $httpBackend.flush();

        expect(planningPeriodServiceNew.publishPeriod).toHaveBeenCalledWith({ id: 'a557210b-99cc-4128-8ae0-138d812974b6' });
        expect(NoticeService.success).toHaveBeenCalledWith('PublishScheduleSucessForSelectedPlanningPeriod', null, true);
        expect(vm.publishRunning).toEqual(false);
    });

    it('should block launch publish when double click', function () {
        spyOn(NoticeService, 'warning').and.callThrough();
        spyOn(planningPeriodServiceNew, 'publishPeriod').and.callThrough();
        $httpBackend.flush();

        vm.publishRunning = true;
        vm.publishSchedule();

        expect(NoticeService.warning).toHaveBeenCalledWith('PublishingScheduleSuccess', null, true);
        expect(vm.publishRunning).toEqual(true);
    });
});
