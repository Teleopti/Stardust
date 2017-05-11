'use strict';
describe('planningPeriodSelectController', function () {
    var $httpBackend,
        $controller,
        $injector,
        fakeBackend,
        planningPeriodServiceNew,
        vm,
        stateparams = { groupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e' };

    beforeEach(function () {
        module('wfm.resourceplanner');
    });

    beforeEach(inject(function (_$httpBackend_, _$controller_, _fakeResourcePlanningBackend_, _planningPeriodServiceNew_) {
        $httpBackend = _$httpBackend_;
        $controller = _$controller_;
        planningPeriodServiceNew = _planningPeriodServiceNew_;
        fakeBackend = _fakeResourcePlanningBackend_;

        fakeBackend.clear();

        spyOn(planningPeriodServiceNew, 'getPlanningPeriodsForAgentGroup').and.callThrough();
        spyOn(planningPeriodServiceNew, 'nextPlanningPeriod').and.callThrough();
        spyOn(planningPeriodServiceNew, 'deleteLastPlanningPeriod').and.callThrough();
        spyOn(planningPeriodServiceNew, 'changeEndDateForLastPlanningPeriod').and.callThrough();

        $httpBackend.whenGET('../api/resourceplanner/agentgroup/aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e').respond(function (method, url, data, headers) {
            return [200, {
                Id: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e',
                Name: "Agent Group Test",
                Filters: []
            }];
        });

        $httpBackend.whenPUT('../api/resourceplanner/agentgroup/aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e/lastperiod?endDate=2018-09-30&startDate=2018-09-01').respond(function (method, url, data, headers) {
            return [200, [{
                AgentGroupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e',
                StartDate: "2018-09-01T00:00:00",
                EndDate: "2018-09-30T00:00:00",
                HasNextPlanningPeriod: true,
                Id: 'a557210b-99cc-4128-8ae0-138d812974b6',
                State: "New",
                ValidationResult: null
            }]]
        });

        $httpBackend.whenPUT('../api/resourceplanner/agentgroup/aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e/lastperiod?endDate=2018-07-30').respond(function (method, url, data, headers) {
            return [200, [{
                AgentGroupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e',
                StartDate: "2018-09-01T00:00:00",
                EndDate: "2018-09-30T00:00:00",
                HasNextPlanningPeriod: true,
                Id: 'a557210b-99cc-4128-8ae0-138d812974b6',
                State: "New",
                ValidationResult: null
            }]]
        });

        $httpBackend.whenPOST('../api/resourceplanner/agentgroup/aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e/nextplanningperiod').respond(function (method, url, data, headers) {
            return [200, true];
        });

        $httpBackend.whenDELETE('../api/resourceplanner/agentgroup/aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e/lastperiod').respond(function (method, url, data, headers) {
            return [200, []];
        });

        vm = $controller('planningPeriodSelectController', { $stateParams: stateparams });

    }));

    afterEach(function () {
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
    });

    it('should get planning periods by agent group id', function () {
        fakeBackend.withPlanningPeriods({
            AgentGroupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e',
            StartDate: "2018-06-18T00:00:00",
            EndDate: "2018-07-15T00:00:00",
            HasNextPlanningPeriod: true,
            Id: 'a557210b-99cc-4128-8ae0-138d812974b6',
            State: "Scheduled",
            ValidationResult: {
                InvalidResources: []
            }
        }).withPlanningPeriods({
            AgentGroupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e',
            StartDate: "2018-07-16T00:00:00",
            EndDate: "2018-08-04T00:00:00",
            HasNextPlanningPeriod: false,
            Id: '3ccd5519-8c92-4c2b-a75c-793ec9f7da56',
            State: "New",
            ValidationResult: null
        });
        $httpBackend.flush();

        expect(planningPeriodServiceNew.getPlanningPeriodsForAgentGroup).toHaveBeenCalledWith({ agentGroupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e' });
        expect(vm.planningPeriods.length).toEqual(2);
    });

    it('should get next planning period', function () {
        vm.startNextPlanningPeriod();
        $httpBackend.flush();

        expect(planningPeriodServiceNew.nextPlanningPeriod).toHaveBeenCalledWith({ agentGroupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e' });
    });

    it('should delete last planning period', function () {
        vm.deleteLastPp();
        $httpBackend.flush();

        expect(planningPeriodServiceNew.deleteLastPlanningPeriod).toHaveBeenCalledWith({ agentGroupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e' });
    });

    it('should modify date for last planning period and only planning period', function () {
        fakeBackend.withPlanningPeriods({
            AgentGroupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e',
            StartDate: "2018-07-16T00:00:00",
            EndDate: "2018-08-04T00:00:00",
            HasNextPlanningPeriod: false,
            Id: '3ccd5519-8c92-4c2b-a75c-793ec9f7da56',
            State: "New",
            ValidationResult: null
        });
        $httpBackend.flush();

        vm.getLastPp(vm.planningPeriods[0]);
        vm.lastPp = {
            startDate: moment('2018-09-01T00:00:00').toDate(),
            endDate: moment('2018-09-30T00:00:00').toDate()
        };
        vm.changeDateForLastPp(vm.lastPp);
        $httpBackend.flush();

        expect(planningPeriodServiceNew.changeEndDateForLastPlanningPeriod).toHaveBeenCalledWith({
            agentGroupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e',
            startDate: '2018-09-01',
            endDate: '2018-09-30'
        });
    });

    it('should modify date for last planning period and it is not the only planning period', function () {
        fakeBackend.withPlanningPeriods({
            AgentGroupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e',
            StartDate: "2018-06-18T00:00:00",
            EndDate: "2018-07-15T00:00:00",
            HasNextPlanningPeriod: true,
            Id: 'a557210b-99cc-4128-8ae0-138d812974b6',
            State: "Scheduled",
            ValidationResult: {
                InvalidResources: []
            }
        }).withPlanningPeriods({
            AgentGroupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e',
            StartDate: "2018-07-16T00:00:00",
            EndDate: "2018-08-04T00:00:00",
            HasNextPlanningPeriod: false,
            Id: '3ccd5519-8c92-4c2b-a75c-793ec9f7da56',
            State: "New",
            ValidationResult: null
        });
        $httpBackend.flush();

        vm.getLastPp(vm.planningPeriods[1]);
        vm.lastPp = {
            startDate: moment('2018-07-16T00:00:00').toDate(),
            endDate: moment('2018-07-30T00:00:00').toDate()
        };
        vm.changeDateForLastPp(vm.lastPp);
        $httpBackend.flush();

        expect(planningPeriodServiceNew.changeEndDateForLastPlanningPeriod).toHaveBeenCalledWith({
            agentGroupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e',
            startDate: null, 
            endDate: '2018-07-30'
        });
    });
});
