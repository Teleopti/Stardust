'use strict';
describe('planningPeriodSelectController', function () {
    var $httpBackend,
        $controller,
        $injector,
        fakeBackend,
        planningPeriodServiceNew,
        vm,
        vm2,
        stateparams = { groupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e' },
        planningGroupInfo = {
            Id: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e',
            Name: "Plan Group Test",
            Filters: []
        },
        planningPeriods = [{
            PlanningGroupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e',
            StartDate: "2018-07-16T00:00:00",
            EndDate: "2018-08-05T00:00:00",
            HasNextPlanningPeriod: false,
            Id: '3ccd5519-8c92-4c2b-a75c-793ec9f7da56',
            State: "New",
            ValidationResult: null,
            Number:3,
            Type:'Week'
        }, {
            PlanningGroupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e',
            StartDate: "2018-06-18T00:00:00",
            EndDate: "2018-07-16T00:00:00",
            HasNextPlanningPeriod: true,
            Id: 'a557210b-99cc-4128-8ae0-138d812974b6',
            State: "Scheduled",
            Number:4,
            Type:'Week',
            ValidationResult: {
                InvalidResources: []
            }
        }],
        planningPeriodOnlyFirst = [{
            PlanningGroupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e',
            StartDate: "2018-09-01T00:00:00",
            EndDate: "2018-09-14T00:00:00",
            HasNextPlanningPeriod: false,
            Id: '3ccd5519-8c92-4c2b-a75c-793ec9f7da56',
            State: "New",
            ValidationResult: null,
            Number:2,
            Type:'Week'
        }];

    beforeEach(function () {
        module('wfm.resourceplanner');
        module('localeLanguageSortingService');
    });

    beforeEach(inject(function (_$httpBackend_, _$controller_, _fakeResourcePlanningBackend_, _planningPeriodServiceNew_) {
        $httpBackend = _$httpBackend_;
        $controller = _$controller_;
        planningPeriodServiceNew = _planningPeriodServiceNew_;
        fakeBackend = _fakeResourcePlanningBackend_;

        fakeBackend.clear();

        spyOn(planningPeriodServiceNew, 'nextPlanningPeriod').and.callThrough();
        spyOn(planningPeriodServiceNew, 'deleteLastPlanningPeriod').and.callThrough();
        spyOn(planningPeriodServiceNew, 'changeEndDateForLastPlanningPeriod').and.callThrough();
        spyOn(planningPeriodServiceNew, 'firstPlanningPeriod').and.callThrough();


        $httpBackend.whenPUT('../api/resourceplanner/planninggroup/aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e/lastperiod?endDate=2018-09-30&lengthOfThePeriodType=1&schedulePeriodType=Month').respond(function (method, url, data, headers) {
            return [200, [{
                PlanningGroupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e',
                StartDate: "2018-09-01T00:00:00",
                EndDate: "2018-09-30T00:00:00",
                HasNextPlanningPeriod: true,
                Id: 'a557210b-99cc-4128-8ae0-138d812974b6',
                State: "New",
                ValidationResult: null
            }]]
        });

        $httpBackend.whenGET('../ToggleHandler/AllToggles').respond(function (method, url, data, headers) {
            return [200, true];
        });

        $httpBackend.whenGET('../api/resourceplanner/planningperiod/suggestions/aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e').respond(function (method, url, data, headers) {
            return [200, [{
                Number: 4,
                PeriodType: "Week",
                StartDate: "2018-01-01T00:00:00",
                EndDate: "2018-01-28T00:00:00"
            }, {
                Number: 8,
                PeriodType: "Week",
                StartDate: "2018-01-01T00:00:00",
                EndDate: "2018-02-25T00:00:00"
            }, {
                Number: 1,
                PeriodType: "Month",
                StartDate: "2018-01-01T00:00:00",
                EndDate: "2018-01-31T00:00:00"
            }]];
        });

        $httpBackend.whenPUT('../api/resourceplanner/planninggroup/aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e/lastperiod?endDate=2018-09-30&lengthOfThePeriodType=1&schedulePeriodType=Month&startDate=2018-09-01').respond(function (method, url, data, headers) {
            return [200, [{
                PlanningGroupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e',
                StartDate: "2018-09-01T00:00:00",
                EndDate: "2018-09-30T00:00:00",
                HasNextPlanningPeriod: true,
                Id: 'a557210b-99cc-4128-8ae0-138d812974b6',
                State: "New",
                ValidationResult: null
            }]]
        });

        $httpBackend.whenPOST('../api/resourceplanner/planninggroup/aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e/firstplanningperiod?lengthOfThePeriodType=8&schedulePeriodType=Week&startDate=2018-01-01').respond(function (method, url, data, headers) {
            return [200, {
                Id: "aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e",
                StartDate: "2018-01-01T00:00:00",
                EndDate: "2018-02-25T00:00:00",
                HasNextPlanningPeriod: false,
                State: "New",
                PlanningGroupId: "a1ae7183-7f2e-4e11-84f7-a83f008efac9",
                TotalAgents: 10,
                Number: 8,
                Type: "Week"
            }];
        });

        $httpBackend.whenPOST('../api/resourceplanner/planninggroup/aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e/firstplanningperiod?lengthOfThePeriodType=1&schedulePeriodType=Month&startDate=2018-01-01').respond(function (method, url, data, headers) {
            return [200, {
                Id: "aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e",
                StartDate: "2018-01-01T00:00:00",
                EndDate: "2018-01-31T00:00:00",
                HasNextPlanningPeriod: false,
                State: "New",
                PlanningGroupId: "a1ae7183-7f2e-4e11-84f7-a83f008efac9",
                TotalAgents: 10,
                Number: 1,
                Type: "Month"
            }];
        });

        $httpBackend.whenPOST('../api/resourceplanner/planninggroup/aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e/nextplanningperiod').respond(function (method, url, data, headers) {
            return [200, true];
        });

        $httpBackend.whenDELETE('../api/resourceplanner/planninggroup/aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e/lastperiod').respond(function (method, url, data, headers) {
            return [200, []];
        });

        vm = $controller('planningPeriodSelectController', { $stateParams: stateparams, planningGroupInfo: planningGroupInfo, planningPeriods: planningPeriods });
        vm2 = $controller('planningPeriodSelectController', { $stateParams: stateparams, planningGroupInfo: planningGroupInfo, planningPeriods: [] });
    }));

    afterEach(function () {
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
    });

    it('should get planning periods by planning group id before controller is loaded', function () {
        $httpBackend.flush();
        expect(vm.planningPeriods.length).toEqual(2);
    });

    it('should have the correct period format displayed', function () {
        $httpBackend.flush();
        expect(vm.planningPeriods[0].PeriodString).toEqual('July 16, 2018 - August 5, 2018');
    });

    it('should get next planning period', function () {
        vm.startNextPlanningPeriod();
        $httpBackend.flush();

        expect(planningPeriodServiceNew.nextPlanningPeriod).toHaveBeenCalledWith({ planningGroupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e' });
    });

    it('should delete last planning period', function () {
        vm.deleteLastPp();
        $httpBackend.flush();

        expect(planningPeriodServiceNew.deleteLastPlanningPeriod).toHaveBeenCalledWith({ planningGroupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e' });
    });

    it('should fetch suggestion planning period for new created plan group', function () {
        $httpBackend.flush();

        expect(vm2.isNonePp()).toEqual(true);
        expect(vm2.suggestions.length).toEqual(3);
    });

    it('should select first suggestion as preselected planning period for creating first planning period', function () {
        $httpBackend.flush();

        expect(vm2.isNonePp()).toEqual(true);
        expect(vm2.suggestions.length).toEqual(3);
        expect(vm2.selectedSuggestion.startDate).toEqual(moment("2018-01-01T00:00:00").toDate());
        expect(vm2.selectedSuggestion.endDate).toEqual(moment("2018-01-28T00:00:00").toDate());
    });

    it('should be able to swift to other suggestion for creating first planning period', function () {
        $httpBackend.flush();
        vm2.setSelectedDate(vm2.suggestions[1]);

        expect(vm2.isNonePp()).toEqual(true);
        expect(vm2.suggestions.length).toEqual(3);
        expect(vm2.selectedSuggestion.startDate).toEqual(moment("2018-01-01T00:00:00").toDate());
        expect(vm2.selectedSuggestion.endDate).toEqual(moment("2018-02-25T00:00:00").toDate());
    });

    it('should be able to check valid custom week type interval for creating first planning period', function () {
        $httpBackend.flush();
        vm2.intervalRange = 7;
        vm2.intervalType = "Week";

        expect(vm2.isNonePp()).toEqual(true);
        expect(vm2.isValidPeriod()).toEqual(true);
    });

    it('should be able to check invalid custom week type interval for creating first planning period', function () {
        $httpBackend.flush();
        vm2.intervalRange = 9;
        vm2.intervalType = "Week";

        expect(vm2.isNonePp()).toEqual(true);
        expect(vm2.isValidPeriod()).toEqual(false);
    });

    it('should be able to check invalid custom month type interval for creating first planning period', function () {
        $httpBackend.flush();
        vm2.intervalRange = 3;
        vm2.intervalType = "Month";

        expect(vm2.isNonePp()).toEqual(true);
        expect(vm2.isValidPeriod()).toEqual(false);
    });

    it('should be able to post valid custom week type and create first planning period', function () {
        $httpBackend.flush();
        vm2.setSelectedDate(vm2.suggestions[1]);
        vm2.createFirstPp();
        $httpBackend.flush();

        expect(vm2.isNonePp()).toEqual(false);
        expect(vm2.planningPeriods.length).toEqual(1);
        expect(vm2.planningPeriods[0].StartDate).toEqual("2018-01-01T00:00:00");
        expect(vm2.planningPeriods[0].EndDate).toEqual("2018-02-25T00:00:00");
        expect(vm2.planningPeriods[0].Number).toEqual(8);
        expect(vm2.planningPeriods[0].Type).toEqual("Week");
        expect(planningPeriodServiceNew.firstPlanningPeriod).toHaveBeenCalledWith({
            planningGroupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e',
            startDate: '2018-01-01',
            schedulePeriodType: 'Week', 
            lengthOfThePeriodType: 8 
        });
    });

    it('should be able to post valid custom month type and create first planning period', function () {
        $httpBackend.flush();
        vm2.setSelectedDate(vm2.suggestions[2]);
        vm2.createFirstPp();
        $httpBackend.flush();

        expect(vm2.isNonePp()).toEqual(false);
        expect(vm2.planningPeriods.length).toEqual(1);
        expect(vm2.planningPeriods[0].StartDate).toEqual("2018-01-01T00:00:00");
        expect(vm2.planningPeriods[0].EndDate).toEqual("2018-01-31T00:00:00");
        expect(vm2.planningPeriods[0].Number).toEqual(1);
        expect(vm2.planningPeriods[0].Type).toEqual("Month");
        expect(planningPeriodServiceNew.firstPlanningPeriod).toHaveBeenCalledWith({
            planningGroupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e',
            startDate: '2018-01-01',
            schedulePeriodType: 'Month',
            lengthOfThePeriodType: 1
        });
    });

    it('should modify date for last planning period and only planning period', function () {
        vm = $controller('planningPeriodSelectController', {
            $stateParams: stateparams,
            planningGroupInfo: planningGroupInfo,
            planningPeriods: planningPeriodOnlyFirst
        });
        vm.getLastPp();
        vm.lastPp = {
            startDate: moment('2018-09-01T00:00:00').toDate(),
            endDate: moment('2018-09-30T00:00:00').toDate()
        };
        vm.intervalType = 'Month'
        vm.intervalRange = 1;
        vm.changeDateForLastPp(vm.lastPp);

        $httpBackend.flush();

        expect(vm.planningPeriods.length).toEqual(1);
        expect(planningPeriodServiceNew.changeEndDateForLastPlanningPeriod).toHaveBeenCalledWith({
            planningGroupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e', 
            startDate: '2018-09-01', 
            schedulePeriodType: 'Month', 
            lengthOfThePeriodType: 1, 
            endDate: '2018-09-30'
        });
    });

    it('should modify date for last planning period and it is not the only planning period', function () {
        vm.getLastPp(planningPeriods[1]);
        vm.lastPp = {
            startDate: moment('2018-09-01T00:00:00').toDate(),
            endDate: moment('2018-09-30T00:00:00').toDate()
        };
        vm.intervalType = 'Month'
        vm.intervalRange = 1;
        vm.changeDateForLastPp(vm.lastPp);
        $httpBackend.flush();

        expect(planningPeriodServiceNew.changeEndDateForLastPlanningPeriod).toHaveBeenCalledWith({
            planningGroupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e', 
            startDate: null, 
            schedulePeriodType: 'Month', 
            lengthOfThePeriodType: 1, 
            endDate: '2018-09-30'
        });
    });

	it('should validate null start date', function () {
		vm = $controller('planningPeriodSelectController', {
			$stateParams: stateparams,
			planningGroupInfo: planningGroupInfo,
			planningPeriods: planningPeriodOnlyFirst
		});
		vm.getLastPp();
		vm.lastPp = {
			startDate: moment('2018-09-01T00:00:00').toDate(),
			endDate: moment('2018-09-30T00:00:00').toDate()
		};
		vm.intervalType = 'Month'
		vm.intervalRange = 1;
		vm.selectedSuggestion.startDate = null;

		vm.isSelectedChanged();
		$httpBackend.flush();

		expect(vm.selectedSuggestion.startDate).toEqual(null);
		expect(vm.selectedSuggestion.endDate).toEqual(null);
	});
});
