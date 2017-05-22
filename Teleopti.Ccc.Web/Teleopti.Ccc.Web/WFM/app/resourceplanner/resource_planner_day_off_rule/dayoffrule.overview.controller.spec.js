'use strict';
describe('dayoffRuleOverviewController', function () {
    var $httpBackend,
        $controller,
        $injector,
        dayOffRuleService,
        agentGroupService,
        stateparams = { groupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e' },
        agentGroupInfo = {
            Id: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e',
            Name: "Agent Group Test",
            Filters: []
        },
        dayOffRulesInfo = [{
            Id: '00e9d2f9-e35e-408a-9cef-a76cfc9f6d6c',
            AgentGroupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e',
            Name: "Default",
            Default: true,
            MinDayOffsPerWeek: 1,
            MaxDayOffsPerWeek: 3,
            MinConsecutiveWorkdays: 2,
            MaxConsecutiveWorkdays: 6,
            MinConsecutiveDayOffs: 1,
            MaxConsecutiveDayOffs: 3,
            Filters: []
        }, {
            Id: 'ec4356ba-8278-48e4-b4f8-c3102b7af684',
            AgentGroupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e',
            Name: "Day off rule 101",
            Default: false,
            MinDayOffsPerWeek: 1,
            MaxDayOffsPerWeek: 3,
            MinConsecutiveWorkdays: 2,
            MaxConsecutiveWorkdays: 6,
            MinConsecutiveDayOffs: 1,
            MaxConsecutiveDayOffs: 3,
            Filters: [{
                Id: "79c466f0-cbe8-4209-b949-9b5e015b23f7",
                FilterType: "contract",
                Name: "Full time Fixed staff"
            }, {
                Id: "cac4e7e7-5645-49ce-87c7-0bdd578d0bc6",
                FilterType: "skill",
                Name: "phone"
            }]
        }];

    beforeEach(function () {
        module('wfm.resourceplanner');
    });

    beforeEach(inject(function (_$httpBackend_, _$controller_, _dayOffRuleService_, _agentGroupService_) {
        $httpBackend = _$httpBackend_;
        $controller = _$controller_;
        dayOffRuleService = _dayOffRuleService_;
        agentGroupService = _agentGroupService_;

        $httpBackend.whenDELETE('../api/resourceplanner/dayoffrules/ec4356ba-8278-48e4-b4f8-c3102b7af684').respond(function (method, url, data, headers) {
            return [200, true];
        });

    }));

    afterEach(function () {
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
    });

    it('should get day off rules by agent group id before controller is loaded', function () {
        spyOn(dayOffRuleService, 'getDayOffRulesByAgentGroupId').and.callThrough();
        var vm = $controller('dayoffRuleOverviewController', {
            $stateParams: stateparams,
            agentGroupInfo: agentGroupInfo,
            dayOffRulesInfo: dayOffRulesInfo
        });

        expect(vm.dayOffRules.length).toEqual(2);
    });

    it('should delete selected off rule', function () {
        spyOn(dayOffRuleService, 'removeDayOffRule').and.callThrough();
         var vm = $controller('dayoffRuleOverviewController', {
            $stateParams: stateparams,
            agentGroupInfo: agentGroupInfo,
            dayOffRulesInfo: dayOffRulesInfo
        });

        vm.deleteDoRule(vm.dayOffRules[1]);
        $httpBackend.flush();

        expect(dayOffRuleService.removeDayOffRule).toHaveBeenCalledWith({ id: 'ec4356ba-8278-48e4-b4f8-c3102b7af684' });
        expect(vm.dayOffRules.length).toEqual(1);
    });

});
